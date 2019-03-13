using ArchivosPlanosWebV2._5.Models;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
namespace ArchivosPlanosWebV2._5.Services
{
    public class Archivo9ARepository

    {
        private AppDbContextSQL db = new AppDbContextSQL();
        public string Archivo_3;
        string Carpeta = @" C:\ArchivosPlanosWeb\";
        //string Carpeta2 = @"C:\Users\Desarrollo3\Desktop\ArchivosPlanosWeb\ArchivosPlanosWeb\Descargas\";
        string Carpeta2 = @"C:\inetpub\wwwroot\ArchivosPlanos\Descargas\";
        string StrIdentificador = "A";
        static string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
        static SqlConnection Connection = new SqlConnection(ConnectionString);
        public string Message = string.Empty;


        /// <summary>
        /// ARCHIVO 9A
        /// </summary>
        /// <param name="Str_Turno_block"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="IdPlazaCobro"></param>
        /// <param name="CabeceraTag"></param>
        /// <param name="Tramo"></param>
        /// <returns></returns>
        public void eventos_detectados_y_marcados_en_el_ECT(string Str_Turno_block, DateTime FechaInicio, string IdPlazaCobro, string CabeceraTag, string Tramo, string Conexion)
        {
            string StrQuerys;
            string Linea = string.Empty;
            string Cabecera;
            string Numero_archivo = string.Empty;
            string Nombre_archivo = string.Empty;
            int Int_turno = 0;
            int cont = 0;

            string H_inicio_turno = string.Empty;
            string H_fin_turno = string.Empty;

            string No_registros = string.Empty;

            string Str_detalle = string.Empty;

            double Dbl_registros;

            string StrClaseExcedente;
            string StrCodigoVhMarcado = string.Empty;
            string strCodigoVhPagoMarcado;

            string Tag_iag;
            string Tarjeta;


            List<string> Val = new List<string>();
            //DataTable dataTableCa = new DataTable();
            EnumerableRowCollection<DataRow> dataRows;
            var NumCarril = string.Empty;
            var NumTramo = string.Empty;
            var NumPlaza = string.Empty;
            var IdCarril = string.Empty;
            OracleConnection ConexionDim = new OracleConnection(Conexion);
            MetodosGlbRepository MtGlb = new MetodosGlbRepository();

            try
            {
                if (Str_Turno_block.Substring(0, 2) == "06")
                {

                    Int_turno = 5;
                    H_inicio_turno = FechaInicio.ToString("MM/dd/yyyy") + " 06:00:00";
                    H_fin_turno = FechaInicio.ToString("MM/dd/yyyy") + " 13:59:59";
                }
                else if (Str_Turno_block.Substring(0, 2) == "14")
                {
                    Int_turno = 6;
                    H_inicio_turno = FechaInicio.ToString("MM/dd/yyyy") + " 14:00:00";
                    H_fin_turno = FechaInicio.ToString("MM/dd/yyyy") + " 21:59:59";
                }
                else if (Str_Turno_block.Substring(0, 2) == "22")
                {
                    Int_turno = 4;
                    H_inicio_turno = FechaInicio.AddDays(-1).ToString("MM/dd/yyyy") + " 22:00:00";
                    H_fin_turno = FechaInicio.ToString("MM/dd/yyyy") + " 05:59:59";
                }

                if (IdPlazaCobro.Length == 3)
                {
                    if (IdPlazaCobro == "108")
                        Nombre_archivo = "0001";
                    else if (IdPlazaCobro == "109")
                        Nombre_archivo = "001B";
                    else if (IdPlazaCobro == "107")
                        Nombre_archivo = "0107";
                    else if (IdPlazaCobro == "061")
                        Nombre_archivo = "061B";
                    else if (IdPlazaCobro == "086" || IdPlazaCobro == "083" || IdPlazaCobro == "027")
                        Nombre_archivo = "01" + IdPlazaCobro.Substring(1, 2);
                    else Nombre_archivo = "0" + IdPlazaCobro;
                }

                Nombre_archivo = Nombre_archivo + FechaInicio.ToString("MM") + FechaInicio.ToString("dd") + "." + Int_turno + "9" + StrIdentificador;

                StreamWriter Osw = new StreamWriter(Carpeta + Nombre_archivo);
                StreamWriter Osw2 = new StreamWriter(Carpeta2 + Nombre_archivo);

                Archivo_3 = Nombre_archivo;

                Cabecera = CabeceraTag;
                if (IdPlazaCobro.Length == 3)
                {
                    if (IdPlazaCobro == "108")
                        Cabecera = Cabecera + "0001";
                    else if (IdPlazaCobro == "109")
                        Cabecera = Cabecera + "001B";
                    else if (IdPlazaCobro == "107")
                        Cabecera = Cabecera + "0107";
                    else if (IdPlazaCobro == "061")
                        Cabecera = Cabecera + "061B";
                    else if (IdPlazaCobro == "086" || IdPlazaCobro == "083" || IdPlazaCobro == "027")
                        Cabecera = Cabecera + "01" + IdPlazaCobro.Substring(1, 2);
                    else Cabecera = Cabecera + "0" + IdPlazaCobro;
                }

                Cabecera = Cabecera + FechaInicio.ToString("MM") + FechaInicio.ToString("dd") + "." + Int_turno + "9" + StrIdentificador + FechaInicio.ToString("dd/MM/yyyy") + Int_turno;


                DateTime _H_inicio_turno = DateTime.ParseExact(H_inicio_turno, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                DateTime _H_fin_turno = DateTime.ParseExact(H_fin_turno, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                /************************************************************************/
                MtGlb.ConnectionOpen(ConexionDim);
                /************************************************************************/

                //DATE_DEBUT_POSTE
                StrQuerys = "SELECT DATE_TRANSACTION, VOIE,  ID_GARE, EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, INDICE_SUITE," +
                            "TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " +
                            "ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, ISSUER_ID " +
                            "FROM TRANSACTION " +
                            "JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " +
                            "LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " +
                            "WHERE " +
                            "(DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                            " AND  ID_PAIEMENT  <> 0 " +
                            "AND (TRANSACTION.Id_Voie = '1' " +
                            "OR TRANSACTION.Id_Voie = '2' " +
                            "OR TRANSACTION.Id_Voie = '3' " +
                            "OR TRANSACTION.Id_Voie = '4' " +
                            "OR TRANSACTION.Id_Voie = 'X') " +
                            "ORDER BY DATE_TRANSACTION";

                if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                {

                    Dbl_registros = 0;

                    /*************************************************************/

                    //EN UN DATATABLE ALACENAMOS TODOS LOS CARRILES PARA LA BUSQUEDA RAPIDA 

                    //Connection.Open();
                    //string Query = @"SELECT t.idTramo, t.nomTramo, p.idPlaza, p.nomPlaza, c.idCarril, c.numCarril, c.numTramo 
                    //                  FROM TYPE_PLAZA p 
                    //                  INNER JOIN TYPE_TRAMO t ON t.idenTramo = p.idTramo
                    //                  INNER JOIN TYPE_CARRIL c ON c.idPlaza = p.idenPlaza
                    //                  WHERE t.idTramo = @tramo and p.idPlaza = @plaza";


                    //string Query = @"SELECT d.ID_Delegacion, d.Nom_Delegacion, p.ID_Plaza, p.Nom_Plaza, c.Num_Gea, c.num_Capufe, c.Num_Tramo " +
                    //              "FROM TYPE_PLAZA p " +
                    //              "INNER JOIN TYPE_TRAMO d on d.ID_Delegacion = d.ID_Delegacion " +
                    //              "INNER JOIN TYPE_CARRIL c on c.ID_Plaza = p.ID_Plaza " +
                    //              "WHERE d.ID_Delegacion = @tramo and p.ID_Plaza = @plaza";


                    //using (SqlCommand Cmd = new SqlCommand(Query, Connection))
                    //{
                    //    Cmd.Parameters.Add(new SqlParameter("tramo", Tramo));
                    //    Cmd.Parameters.Add(new SqlParameter("plaza", IdPlazaCobro));
                    //    //Cmd.Parameters.Add(new SqlParameter("plaza", IdPlazaCobro.Substring(1, 2)));
                    //    try
                    //    {
                    //        SqlDataAdapter da = new SqlDataAdapter(Cmd);
                    //        da.Fill(dataTableCa);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Message = ex.Message + " " + ex.StackTrace;
                    //    }
                    //    finally
                    //    {
                    //        Cmd.Dispose();
                    //        Connection.Close();
                    //    }
                    //}
                    var idpla = IdPlazaCobro.Substring(1, 2);
                    var Carriles_Plazas = db.Type_Plaza.GroupJoin(db.Type_Carril, pla => pla.Id_Plaza, car => car.Plaza_Id, (pla, car) => new { pla, car }).Where(x => x.pla.Num_Plaza == idpla);

                    var props = typeof(Type_Carril).GetProperties();
                    DataTable dt = new DataTable("Tacla_Carriles");
                    dt.Columns.AddRange(
                            props.Select(x => new DataColumn(x.Name, x.PropertyType)).ToArray()
                        );
                    Carriles_Plazas.FirstOrDefault().car.ToList().ForEach(
                            i => dt.Rows.Add(props.Select(p => p.GetValue(i, null)).ToArray())
                        );

                    foreach (DataRow item in MtGlb.Ds.Tables["TRANSACTION"].Rows)
                    {

                        if (!DBNull.Value.Equals(item["CLASE_DETECTADA"]))
                        {
                            Str_detalle = string.Empty;

                            if (!DBNull.Value.Equals(item))
                            {
                                //Fecha del evento 	Fecha 	dd/mm/aaaa 
                                Str_detalle = Convert.ToDateTime(item["DATE_TRANSACTION"]).ToString("dd/MM/yyyy") + ",";
                                //Número de turno	Entero 	9
                                Str_detalle = Str_detalle + Int_turno + ",";
                                //Hora de evento 	Caracter 	hhmmss 
                                Str_detalle = Str_detalle + Convert.ToDateTime(item["DATE_TRANSACTION"]).ToString("HHmmss") + ",";

                                /*******************************/

                                dataRows = from myRow in dt.AsEnumerable()
                                               //where myRow.Field<string>("idCarril") == item["Voie"].ToString().Substring(1, 2)
                                           where myRow.Field<string>("Num_Gea") == item["Voie"].ToString().Substring(1, 2)
                                           select myRow;

                                foreach (DataRow value in dataRows)
                                {
                                    //NumCarril = value["numCarril"].ToString();
                                    //NumTramo = value["numTramo"].ToString();
                                    //NumPlaza = value["idPlaza"].ToString();
                                    NumCarril = value["Num_Capufe"].ToString();
                                    NumTramo = value["Num_Tramo"].ToString();
                                    NumPlaza = value.Field<Type_Plaza>("Type_Plaza").Num_Plaza.ToString();
                                    break;
                                }
                                /*******************************/
                                if (dataRows.Count() != 0)
                                {
                                    Str_detalle = Str_detalle + NumTramo + ",";
                                    Str_detalle = Str_detalle + NumCarril + ",";
                                }
                                else
                                {
                                    Str_detalle = Str_detalle + ",,";
                                }

                                //Cuerpo Caracter    X(1)
                                Str_detalle = Str_detalle + item["Voie"].ToString().Substring(0, 1) + ",";
                                //Número de evento 	Entero 	>>>>>>9
                                Str_detalle = Str_detalle + item["EVENT_NUMBER"] + ",";
                                //Número de folio 	Entero 	>>>>>>9 
                                Str_detalle = Str_detalle + item["FOLIO_ECT"] + ",";
                                //Código de vehículo detectado ECT 	Caracter 	X(6)

                                if (!DBNull.Value.Equals(item["CLASE_DETECTADA"]))
                                {
                                    StrClaseExcedente = string.Empty;

                                    if (item["CLASE_DETECTADA"].ToString() == "T01A")
                                        Str_detalle = Str_detalle + "T01" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T01M")
                                        Str_detalle = Str_detalle + "T01" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T01T")
                                        Str_detalle = Str_detalle + "T09P01" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T02B")
                                        Str_detalle = Str_detalle + "T02" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T03B")
                                        Str_detalle = Str_detalle + "T03" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T04B")
                                        Str_detalle = Str_detalle + "T04" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T02C")
                                        Str_detalle = Str_detalle + "T02" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T03C")
                                        Str_detalle = Str_detalle + "T03" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T04C")
                                        Str_detalle = Str_detalle + "T04" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T05C")
                                        Str_detalle = Str_detalle + "T05" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T06C")
                                        Str_detalle = Str_detalle + "T06" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T07C")
                                        Str_detalle = Str_detalle + "T07" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T08C")
                                        Str_detalle = Str_detalle + "T08" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T09C")
                                        Str_detalle = Str_detalle + "T09" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "TL01A")
                                        Str_detalle = Str_detalle + "T01L01" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "TL02A")
                                        Str_detalle = Str_detalle + "T01L02" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "TLnnA")
                                        Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(item["ID_OBS_TT"].ToString().Length == 1, "0" + item["ID_OBS_TT"], item["ID_OBS_TT"].ToString()) + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "T01P")
                                        Str_detalle = Str_detalle + "T01P" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "TP01C")
                                        Str_detalle = Str_detalle + "T09P01" + ",";
                                    else if (item["CLASE_DETECTADA"].ToString() == "TPnnC")
                                        Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(item["ID_OBS_TT"].ToString().Length == 1, "0" + item["ID_OBS_TT"], item["ID_OBS_TT"].ToString()) + ",";
                                    else
                                        Str_detalle = Str_detalle + "No detectada" + ",0,";
                                }
                                else
                                {
                                    Str_detalle = Str_detalle + "0,";
                                }

                                //Importe vehículo detectado ECT 	Decimal 	>>9.99 
                                StrQuerys = "SELECT " +
                                            "TYPE_PAIEMENT.libelle_paiement_L2 " +
                                            ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " +
                                            ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " +
                                            ",Prix_Cl19, Prix_Cl20 " +
                                            ",TYPE_PAIEMENT.libelle_paiement " +
                                            ",TABLE_TARIF.CODE " +
                                            "FROM TABLE_TARIF, " +
                                            "TYPE_PAIEMENT " +
                                            "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) ";

                                //borrar
                                StrQuerys = StrQuerys + "AND TABLE_TARIF.Version_Tarif = " + item["Version_Tarif"] + " " +
                                                        "AND CODE = " + item["ID_PAIEMENT"] +  " " + " AND ID_GARE =  " + item["ID_GARE"] + " " +
                                                        "ORDER BY TABLE_TARIF.CODE ";

                                if (MtGlb.QueryDataSet4(StrQuerys, "TABLE_TARIF", ConexionDim))
                                {
                                    if (Convert.ToInt32(item["ACD_CLASS"]) > 0 && Convert.ToInt32(item["ACD_CLASS"]) <= 9)
                                        Str_detalle = Str_detalle + item["MONTO_DETECTADO"] + ",,";
                                    else if (Convert.ToInt32(item["ACD_CLASS"]) >= 12 && Convert.ToInt32(item["ACD_CLASS"]) <= 15)
                                        Str_detalle = Str_detalle + item["MONTO_DETECTADO"] + ",,";
                                    //EXCEDENTES
                                    else if (Convert.ToInt32(item["ACD_CLASS"]) >= 10 && Convert.ToInt32(item["ACD_CLASS"]) <= 11)
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                    else if (Convert.ToInt32(item["ACD_CLASS"]) == 16)
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                    else if (Convert.ToInt32(item["ACD_CLASS"]) == 17)
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                    else if (Convert.ToInt32(item["ACD_CLASS"]) == 18)
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                    else
                                        Str_detalle = Str_detalle + ",,";
                                }
                                else
                                    Str_detalle = Str_detalle + ",,";

                                //Importe eje excedente detectado ECT Decimal     > 9.99
                                //Código de vehículo marcado C-R	Caracter 	X(6)

                                if (!DBNull.Value.Equals(item["CLASE_MARCADA"]))
                                {
                                    StrClaseExcedente = string.Empty;
                                    StrCodigoVhMarcado = string.Empty;

                                    if (item["CLASE_MARCADA"].ToString() == "T01A")
                                        Str_detalle = Str_detalle + "T01" + ",A,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T01M")
                                        Str_detalle = Str_detalle + "T01" + ",M,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T01T")
                                        Str_detalle = Str_detalle + "T09P01" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T02B")
                                        Str_detalle = Str_detalle + "T02" + ",B,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T03B")
                                        Str_detalle = Str_detalle + "T03" + ",B,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T04B")
                                        Str_detalle = Str_detalle + "T04" + ",B,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T02C")
                                        Str_detalle = Str_detalle + "T02" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T03C")
                                        Str_detalle = Str_detalle + "T03" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T04C")
                                        Str_detalle = Str_detalle + "T04" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T05C")
                                        Str_detalle = Str_detalle + "T05" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T06C")
                                        Str_detalle = Str_detalle + "T06" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T07C")
                                        Str_detalle = Str_detalle + "T07" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T08C")
                                        Str_detalle = Str_detalle + "T08" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T09C")
                                        Str_detalle = Str_detalle + "T09" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "TL01A")
                                        Str_detalle = Str_detalle + "T01L01" + ",A,";
                                    else if (item["CLASE_MARCADA"].ToString() == "TL02A")
                                        Str_detalle = Str_detalle + "T01L02" + ",A,";
                                    else if (item["CLASE_MARCADA"].ToString() == "TLnnA")
                                        Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(item["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + item["CODE_GRILLE_TARIF"], item["CODE_GRILLE_TARIF"].ToString()) + ",A,";
                                    else if (item["CLASE_MARCADA"].ToString() == "T01P")
                                        Str_detalle = Str_detalle + "T01P" + ",A,";
                                    else if (item["CLASE_MARCADA"].ToString() == "TP01C")
                                        Str_detalle = Str_detalle + "T09P01" + ",C,";
                                    else if (item["CLASE_MARCADA"].ToString() == "TPnnC")
                                        Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(item["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + item["CODE_GRILLE_TARIF"], item["CODE_GRILLE_TARIF"].ToString()) + ",C,";
                                    else
                                        Str_detalle = Str_detalle + "No detectada" + ",0,";
                                }
                                else
                                    Str_detalle = Str_detalle + ",0,";

                                //Tipo de vehículo marcado C - R    Caracter X(1)
                                //Código de usuario pago marcado C-R	Caracter 	X(3)
                                if (Convert.ToInt32(item["ID_PAIEMENT"]) == 1)
                                    strCodigoVhPagoMarcado = "NOR" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 2)
                                    strCodigoVhPagoMarcado = "NOR" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 27)
                                    strCodigoVhPagoMarcado = "VSC" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 9)
                                    strCodigoVhPagoMarcado = "FCUR" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 10)
                                    strCodigoVhPagoMarcado = "RPI" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 12)
                                    strCodigoVhPagoMarcado = "TDC" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 14)
                                    strCodigoVhPagoMarcado = "TDD" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 15)
                                    strCodigoVhPagoMarcado = "IAV" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 13)
                                    strCodigoVhPagoMarcado = "ELU" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 71)
                                    strCodigoVhPagoMarcado = "RPI" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 72)
                                    strCodigoVhPagoMarcado = "RPI" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 73)
                                    strCodigoVhPagoMarcado = "RPI" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 74)
                                    strCodigoVhPagoMarcado = "RPI" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 75)
                                    strCodigoVhPagoMarcado = "RPA" + ",";
                                else
                                    strCodigoVhPagoMarcado = ",";

                                //Importe vehículo marcado C-R[1] Decimal >> 9.99
                                StrQuerys = "SELECT " +
                                            "TYPE_PAIEMENT.libelle_paiement_L2 " +
                                            ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " +
                                            ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " +
                                            ",Prix_Cl19, Prix_Cl20 " +
                                            ",TYPE_PAIEMENT.libelle_paiement " +
                                            ",TABLE_TARIF.CODE " +
                                            "FROM TABLE_TARIF, " +
                                            "TYPE_PAIEMENT " +
                                            "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) ";

                                StrQuerys = StrQuerys + "AND TABLE_TARIF.Version_Tarif = " + item["Version_Tarif"] + " " +
                                                        "AND CODE = " + item["ID_PAIEMENT"] + " " + " AND ID_GARE =  " + item["ID_GARE"] + " " +
                                                        "ORDER BY TABLE_TARIF.CODE ";

                                if (MtGlb.QueryDataSet4(StrQuerys, "TABLE_TARIF", ConexionDim))
                                {
                                    if (Convert.ToInt32(item["TAB_ID_CLASSE"]) > 0 && Convert.ToInt32(item["TAB_ID_CLASSE"]) <= 9)
                                    {
                                        Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                        Str_detalle = Str_detalle + item["MONTO_MARCADO"] + ",,";
                                    }
                                    else if (Convert.ToInt32(item["TAB_ID_CLASSE"]) >= 12 && Convert.ToInt32(item["TAB_ID_CLASSE"]) <= 15)
                                    {
                                        Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                        Str_detalle = Str_detalle + item["MONTO_MARCADO"] + ",,";
                                        //EXCEDENTES
                                    }
                                    else if (Convert.ToInt32(item["TAB_ID_CLASSE"]) >= 10 && Convert.ToInt32(item["TAB_ID_CLASSE"]) <= 11)
                                    {
                                        Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(item["MONTO_MARCADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                    }
                                    else if (Convert.ToInt32(item["TAB_ID_CLASSE"]) == 16)
                                    {
                                        Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(item["MONTO_MARCADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                    }
                                    else if (Convert.ToInt32(item["TAB_ID_CLASSE"]) == 17)
                                    {
                                        Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(item["MONTO_MARCADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                    }
                                    else if (Convert.ToInt32(item["TAB_ID_CLASSE"]) == 18)
                                    {
                                        Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(item["MONTO_MARCADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                    }
                                    else
                                    {
                                        Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                        Str_detalle = Str_detalle + ",,";
                                    }
                                }
                                else
                                {
                                    Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                    Str_detalle = Str_detalle + ",,";
                                }

                                //Importe eje excedente marcado C - R   Decimal > 9.99
                                //Número de tarjeta Pagos Electrónicos[2]	Caracter 	X(20)

                                Tag_iag = string.Empty;
                                Tarjeta = string.Empty;

                                if (Convert.ToInt32(item["ID_PAIEMENT"]) == 15)
                                {
                                    Tag_iag = MtGlb.IIf(item["CONTENU_ISO"].ToString() == "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", item["CONTENU_ISO"].ToString().Trim());

                                    //IMDM22879778            ISOB0000       
                                    Tag_iag = Tag_iag.Substring(0, 16).Trim();

                                    if (Tag_iag.Length == 13 && Tag_iag.Substring(0, 3) == "009")
                                        Tag_iag = Tag_iag.Substring(0, 3) + Tag_iag.Substring(5, 8);

                                    Str_detalle = Str_detalle + Tag_iag + ",";

                                    Str_detalle = Str_detalle + "V" + ",";
                                    Str_detalle = Str_detalle + ",";
                                    Str_detalle = Str_detalle + ",";
                                }
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 12 || Convert.ToInt32(item["ID_PAIEMENT"]) == 14)
                                {
                                    Str_detalle = Str_detalle + item["ISSUER_ID"] + ",";
                                    Str_detalle = Str_detalle + "V" + ",";

                                    if (MtGlb.IsNumeric(item["CONTENU_ISO"].ToString().Substring(0, 6)))
                                    {
                                        if (item["CONTENU_ISO"].ToString().Substring(0, 6).IndexOf("E") != 0)
                                            Str_detalle = Str_detalle + item["CONTENU_ISO"].ToString().Substring(0, 6) + ",";

                                        else
                                            Str_detalle = Str_detalle + "0,";
                                    }
                                    else
                                        Str_detalle = Str_detalle + "0,";

                                    Str_detalle = Str_detalle + Convert.ToDateTime(item["DATE_TRANSACTION"]).ToString("dd/MM/yyyy") + ",";
                                }
                                else
                                {
                                    Str_detalle = Str_detalle + MtGlb.IIf(item["CONTENU_ISO"].ToString() == "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") + ",";
                                    Str_detalle = Str_detalle + ",";

                                    Str_detalle = Str_detalle + ",";
                                    Str_detalle = Str_detalle + ",";
                                }
                            }
                        }
                        /***************************************************************************************************/
                        else
                        {
                            StrQuerys = "SELECT DATE_TRANSACTION, VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " +
                                        "TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " +
                                        "ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, ISSUER_ID " +
                                        "FROM TRANSACTION " +
                                        "JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " +
                                        "LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " +
                                        "WHERE " +
                                        "(DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                        "AND VOIE = '" + item["VOIE"] + "' " +
                                        "AND  ID_OBS_SEQUENCE <> '7777' " +
                                        "AND EVENT_NUMBER = " + item["EVENT_NUMBER"] + " " +
                                        "AND (TRANSACTION.Id_Voie = '1' " +
                                        "OR TRANSACTION.Id_Voie = '2' " +
                                        "OR TRANSACTION.Id_Voie = '3' " +
                                        "OR TRANSACTION.Id_Voie = '4' " +
                                        "OR TRANSACTION.Id_Voie = 'X') " +
                                        "ORDER BY DATE_TRANSACTION desc";

                            if (MtGlb.QueryDataSet3(StrQuerys, "TRANSACTION", ConexionDim))
                            {
                                Str_detalle = string.Empty;

                                if (!DBNull.Value.Equals(item))
                                {
                                    //Fecha del evento 	Fecha 	dd/mm/aaaa 
                                    Str_detalle = Convert.ToDateTime(MtGlb.oDataRow3["DATE_TRANSACTION"]).ToString("dd/MM/yyyy") + ",";
                                    //Número de turno	Entero 	9
                                    Str_detalle = Str_detalle + Int_turno + ",";
                                    //Hora de evento 	Caracter 	hhmmss 
                                    Str_detalle = Str_detalle + Convert.ToDateTime(MtGlb.oDataRow3["DATE_TRANSACTION"]).ToString("HHmmss") + ",";

                                    /*******************************/

                                    dataRows = from myRow in dt.AsEnumerable()
                                                   //where myRow.Field<string>("idCarril") == MtGlb.oDataRow3["Voie"].ToString().Substring(1, 2)
                                               where myRow.Field<string>("Num_Gea") == MtGlb.oDataRow3["Voie"].ToString().Substring(1, 2)
                                               select myRow;

                                    foreach (DataRow value in dataRows)
                                    {
                                        //NumCarril = value["numCarril"].ToString();
                                        //NumTramo = value["numTramo"].ToString();
                                        //NumPlaza = value["idPlaza"].ToString();
                                        NumCarril = value["Num_Capufe"].ToString();
                                        NumTramo = value["Num_Tramo"].ToString();
                                        NumPlaza = value.Field<Type_Plaza>("Type_plaza").Num_Plaza.ToString();
                                    }

                                    /*******************************/

                                    if (dataRows.Count() != 0)
                                    {
                                        Str_detalle = Str_detalle + NumTramo + ",";
                                        Str_detalle = Str_detalle + NumCarril + ",";
                                    }
                                    else
                                    {
                                        Str_detalle = Str_detalle + ",,";
                                    }

                                    //Cuerpo Caracter    X(1)
                                    Str_detalle = Str_detalle + MtGlb.oDataRow3["Voie"].ToString().Substring(0, 1) + ",";
                                    //Número de evento 	Entero 	>>>>>>9
                                    Str_detalle = Str_detalle + MtGlb.oDataRow3["EVENT_NUMBER"] + ",";
                                    //Número de folio 	Entero 	>>>>>>9 
                                    if (Convert.ToString(MtGlb.oDataRow3["FOLIO_ECT"]) == "0")
                                        Str_detalle = Str_detalle + item["FOLIO_ECT"] + ",";
                                    else
                                        Str_detalle = Str_detalle + MtGlb.oDataRow3["FOLIO_ECT"] + ",";
                                    //Código de vehículo detectado ECT 	Caracter 	X(6)

                                    if (!DBNull.Value.Equals(MtGlb.oDataRow3["CLASE_DETECTADA"]))
                                    {
                                        StrClaseExcedente = string.Empty;

                                        if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T01A")
                                            Str_detalle = Str_detalle + "T01" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T01M")
                                            Str_detalle = Str_detalle + "T01" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T01T")
                                            Str_detalle = Str_detalle + "T09P01" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T02B")
                                            Str_detalle = Str_detalle + "T02" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T03B")
                                            Str_detalle = Str_detalle + "T03" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T04B")
                                            Str_detalle = Str_detalle + "T04" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T02C")
                                            Str_detalle = Str_detalle + "T02" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T03C")
                                            Str_detalle = Str_detalle + "T03" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T04C")
                                            Str_detalle = Str_detalle + "T04" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T05C")
                                            Str_detalle = Str_detalle + "T05" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T06C")
                                            Str_detalle = Str_detalle + "T06" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T07C")
                                            Str_detalle = Str_detalle + "T07" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T08C")
                                            Str_detalle = Str_detalle + "T08" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T09C")
                                            Str_detalle = Str_detalle + "T09" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TL01A")
                                            Str_detalle = Str_detalle + "T01L01" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TL02A")
                                            Str_detalle = Str_detalle + "T01L02" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TLnnA")
                                            Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(MtGlb.oDataRow3["ID_OBS_TT"].ToString().Length == 1, "0" + MtGlb.oDataRow3["ID_OBS_TT"], MtGlb.oDataRow3["ID_OBS_TT"].ToString()) + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T01P")
                                            Str_detalle = Str_detalle + "T01P" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TP01C")
                                            Str_detalle = Str_detalle + "T09P01" + ",";
                                        else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TPnnC")
                                            Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(MtGlb.oDataRow3["ID_OBS_TT"].ToString().Length == 1, "0" + MtGlb.oDataRow3["ID_OBS_TT"], MtGlb.oDataRow3["ID_OBS_TT"].ToString()) + ",";
                                        else
                                            Str_detalle = Str_detalle + "No detectada" + ",0,";
                                    }
                                    else
                                    {
                                        Str_detalle = Str_detalle + "0,";
                                    }

                                    //Importe vehículo detectado ECT 	Decimal 	>>9.99 
                                    StrQuerys = "SELECT " +
                                                "TYPE_PAIEMENT.libelle_paiement_L2 " +
                                                ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " +
                                                ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " +
                                                ",Prix_Cl19, Prix_Cl20 " +
                                                ",TYPE_PAIEMENT.libelle_paiement " +
                                                ",TABLE_TARIF.CODE " +
                                                "FROM TABLE_TARIF, " +
                                                "TYPE_PAIEMENT " +
                                                "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) ";

                                    //borrar
                                    StrQuerys = StrQuerys + "AND TABLE_TARIF.Version_Tarif = " + MtGlb.oDataRow3["Version_Tarif"] + " " +
                                                            "AND CODE = " + item["ID_PAIEMENT"] + " " + " AND ID_GARE =  " + item["ID_GARE"] + " " +
                                                            "ORDER BY TABLE_TARIF.CODE ";

                                    if (MtGlb.QueryDataSet4(StrQuerys, "TABLE_TARIF", ConexionDim))
                                    {
                                        if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) > 0 && Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) <= 9)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow3["MONTO_DETECTADO"] + ",,";
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) >= 12 && Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) <= 15)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow3["MONTO_DETECTADO"] + ",,";
                                        //EXCEDENTES
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) >= 10 && Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) <= 11)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) == 16)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) == 17)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) == 18)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                        else
                                            Str_detalle = Str_detalle + ",,";
                                    }
                                    else
                                        Str_detalle = Str_detalle + ",,";

                                    //Importe eje excedente detectado ECT Decimal     > 9.99
                                    //Código de vehículo marcado C-R	Caracter 	X(6)

                                    if (!DBNull.Value.Equals(MtGlb.oDataRow3["CLASE_MARCADA"]))
                                    {
                                        StrClaseExcedente = string.Empty;
                                        StrCodigoVhMarcado = string.Empty;

                                        if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T01A")
                                            Str_detalle = Str_detalle + "T01" + ",A,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T01M")
                                            Str_detalle = Str_detalle + "T01" + ",M,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T01T")
                                            Str_detalle = Str_detalle + "T09P01" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T02B")
                                            Str_detalle = Str_detalle + "T02" + ",B,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T03B")
                                            Str_detalle = Str_detalle + "T03" + ",B,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T04B")
                                            Str_detalle = Str_detalle + "T04" + ",B,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T02C")
                                            Str_detalle = Str_detalle + "T02" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T03C")
                                            Str_detalle = Str_detalle + "T03" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T04C")
                                            Str_detalle = Str_detalle + "T04" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T05C")
                                            Str_detalle = Str_detalle + "T05" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T06C")
                                            Str_detalle = Str_detalle + "T06" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T07C")
                                            Str_detalle = Str_detalle + "T07" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T08C")
                                            Str_detalle = Str_detalle + "T08" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T09C")
                                            Str_detalle = Str_detalle + "T09" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TL01A")
                                            Str_detalle = Str_detalle + "T01L01" + ",A,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TL02A")
                                            Str_detalle = Str_detalle + "T01L02" + ",A,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TLnnA")
                                            Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + MtGlb.oDataRow3["CODE_GRILLE_TARIF"], MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString()) + ",A,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T01P")
                                            Str_detalle = Str_detalle + "T01P" + ",A,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TP01C")
                                            Str_detalle = Str_detalle + "T09P01" + ",C,";
                                        else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TPnnC")
                                            Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + MtGlb.oDataRow3["CODE_GRILLE_TARIF"], MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString()) + ",C,";
                                        else
                                            Str_detalle = Str_detalle + "No detectada" + ",0,";
                                    }
                                    else
                                        Str_detalle = Str_detalle + ",0,";

                                    //Tipo de vehículo marcado C - R    Caracter X(1)
                                    //Código de usuario pago marcado C-R	Caracter 	X(3)
                                    if (Convert.ToInt32(item["ID_PAIEMENT"]) == 1)
                                        strCodigoVhPagoMarcado = "NOR" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 2)
                                        strCodigoVhPagoMarcado = "NOR" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 27)
                                        strCodigoVhPagoMarcado = "VSC" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 9)
                                        strCodigoVhPagoMarcado = "FCUR" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 10)
                                        strCodigoVhPagoMarcado = "RPI" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 12)
                                        strCodigoVhPagoMarcado = "TDC" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 14)
                                        strCodigoVhPagoMarcado = "TDD" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 15)
                                        strCodigoVhPagoMarcado = "IAV" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 13)
                                        strCodigoVhPagoMarcado = "ELU" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 71)
                                        strCodigoVhPagoMarcado = "RPI" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 72)
                                        strCodigoVhPagoMarcado = "RPI" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 73)
                                        strCodigoVhPagoMarcado = "RPI" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 74)
                                        strCodigoVhPagoMarcado = "RPI" + ",";
                                    else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 75)
                                        strCodigoVhPagoMarcado = "RPA" + ",";
                                    else
                                        strCodigoVhPagoMarcado = ",";

                                    //Importe vehículo marcado C-R[1] Decimal >> 9.99
                                    StrQuerys = "SELECT " +
                                                "TYPE_PAIEMENT.libelle_paiement_L2 " +
                                                ",Prix_Cl01 ,Prix_Cl02 ,Prix_Cl03 ,Prix_Cl04 ,Prix_Cl05 ,Prix_Cl06 ,Prix_Cl07 ,Prix_Cl08 ,Prix_Cl09 " +
                                                ",Prix_Cl10 ,Prix_Cl11 ,Prix_Cl12 ,Prix_Cl13 ,Prix_Cl14 ,Prix_Cl15 ,Prix_Cl16 ,Prix_Cl17 ,Prix_Cl18 " +
                                                ",Prix_Cl19, Prix_Cl20 " +
                                                ",TYPE_PAIEMENT.libelle_paiement " +
                                                ",TABLE_TARIF.CODE " +
                                                "FROM TABLE_TARIF, " +
                                                "TYPE_PAIEMENT " +
                                                "WHERE   TABLE_TARIF.CODE =	TYPE_PAIEMENT.Id_Paiement(+) ";

                                    StrQuerys = StrQuerys + "AND TABLE_TARIF.Version_Tarif = " + MtGlb.oDataRow3["Version_Tarif"] + " " +
                                                            "AND CODE = " + item["ID_PAIEMENT"] + " " + " AND ID_GARE =  " + item["ID_GARE"] + " " +
                                                            "ORDER BY TABLE_TARIF.CODE ";

                                    if (MtGlb.QueryDataSet4(StrQuerys, "TABLE_TARIF", ConexionDim))
                                    {
                                        if (Convert.ToInt32(MtGlb.oDataRow3["TAB_ID_CLASSE"]) > 0 && Convert.ToInt32(MtGlb.oDataRow3["TAB_ID_CLASSE"]) <= 9)
                                        {
                                            Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                            Str_detalle = Str_detalle + MtGlb.oDataRow3["MONTO_MARCADO"] + ",,";
                                        }
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["TAB_ID_CLASSE"]) >= 12 && Convert.ToInt32(MtGlb.oDataRow3["TAB_ID_CLASSE"]) <= 15)
                                        {
                                            Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                            Str_detalle = Str_detalle + MtGlb.oDataRow3["MONTO_MARCADO"] + ",,";
                                            //EXCEDENTES
                                        }
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["TAB_ID_CLASSE"]) >= 10 && Convert.ToInt32(MtGlb.oDataRow3["TAB_ID_CLASSE"]) <= 11)
                                        {
                                            Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_MARCADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                        }
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["TAB_ID_CLASSE"]) == 16)
                                        {
                                            Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_MARCADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                        }
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["TAB_ID_CLASSE"]) == 17)
                                        {
                                            Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_MARCADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                        }
                                        else if (Convert.ToInt32(MtGlb.oDataRow3["TAB_ID_CLASSE"]) == 18)
                                        {
                                            Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_MARCADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                        }
                                        else
                                        {
                                            Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                            Str_detalle = Str_detalle + ",,";
                                        }
                                    }
                                    else
                                    {
                                        Str_detalle = Str_detalle + StrCodigoVhMarcado + strCodigoVhPagoMarcado;
                                        Str_detalle = Str_detalle + ",,";
                                    }

                                    //Importe eje excedente marcado C - R   Decimal > 9.99
                                    //Número de tarjeta Pagos Electrónicos[2]	Caracter 	X(20)

                                    Tag_iag = string.Empty;
                                    Tarjeta = string.Empty;

                                    if (Convert.ToInt32(item["ID_PAIEMENT"]) == 15)
                                    {
                                        Tag_iag = MtGlb.IIf(MtGlb.oDataRow3["CONTENU_ISO"].ToString() == "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", MtGlb.oDataRow3["CONTENU_ISO"].ToString().TrimStart());
                                        //Salto de proceso por variable null 
                                        if (Tag_iag != "")
                                        {
                                            Tag_iag = Tag_iag.Substring(0, 14).Trim();

                                            if (Tag_iag.Length == 13 && Tag_iag.Substring(0, 3) == "009")
                                                Tag_iag = Tag_iag.Substring(0, 3) + Tag_iag.Substring(5, 8);
                                        }

                                        Str_detalle = Str_detalle + Tag_iag + ",";

                                        Str_detalle = Str_detalle + "V" + ",";
                                        Str_detalle = Str_detalle + ",";
                                        Str_detalle = Str_detalle + ",";
                                    }
                                    else if (Convert.ToInt32(MtGlb.oDataRow3["ID_PAIEMENT"]) == 12 || Convert.ToInt32(MtGlb.oDataRow3["ID_PAIEMENT"]) == 14)
                                    {
                                        Str_detalle = Str_detalle + MtGlb.oDataRow3["ISSUER_ID"] + ",";
                                        Str_detalle = Str_detalle + "V" + ",";

                                        if (MtGlb.IsNumeric(MtGlb.oDataRow3["CONTENU_ISO"].ToString().Substring(0, 6)))
                                        {
                                            if (MtGlb.oDataRow3["CONTENU_ISO"].ToString().Substring(0, 6).IndexOf("E") != 0)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow3["CONTENU_ISO"].ToString().Substring(0, 6) + ",";

                                            else
                                                Str_detalle = Str_detalle + "0,";
                                        }
                                        else
                                            Str_detalle = Str_detalle + "0,";

                                        Str_detalle = Str_detalle + Convert.ToDateTime(MtGlb.oDataRow3["DATE_TRANSACTION"]).ToString("dd/MM/yyyy") + ",";
                                    }
                                    else
                                    {
                                        Str_detalle = Str_detalle + MtGlb.IIf(MtGlb.oDataRow3["CONTENU_ISO"].ToString() == "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "", "") + ",";
                                        Str_detalle = Str_detalle + ",";

                                        Str_detalle = Str_detalle + ",";
                                        Str_detalle = Str_detalle + ",";
                                    }
                                }
                            }

                            /***************************************************************************************************/

                        }


                        cont++;
                        //24/09/2018,4,033442,364,2681,B,87690,86913,T01L02,67,-51,T01L02,A,NOR,67,-51,,,,,
                        //24/09/2018,4,033442,364,2681,B,87690,86913,T01L02,8,8,T01L02,A,NOR,8,8,,,,,
                        if (Str_detalle.IndexOf("-") == -1)
                        {
                            Dbl_registros = Dbl_registros + 1;
                            Val.Add(Str_detalle);

                            //cont += 1;
                        }
                        else
                        {
                            Str_detalle = Str_detalle + "";
                        }

                        if (cont == 1761)
                        {
                            string J = string.Empty;
                        }



                    }

                    /************************************************************************/

                    if (Convert.ToString(Dbl_registros).Length == 1)
                        No_registros = "0000" + Dbl_registros;
                    else if (Convert.ToString(Dbl_registros).Length == 2)
                        No_registros = "000" + Dbl_registros;
                    else if (Convert.ToString(Dbl_registros).Length == 3)
                        No_registros = "00" + Dbl_registros;
                    else if (Convert.ToString(Dbl_registros).Length == 4)
                        No_registros = "0" + Dbl_registros;
                    else if (Convert.ToString(Dbl_registros).Length == 5)
                        No_registros = Dbl_registros.ToString();

                    Cabecera = Cabecera + No_registros;

                    Osw.WriteLine(Cabecera);
                    Osw2.WriteLine(Cabecera);
                    // CABECERA FIN

                }
                else
                {
                    Cabecera = Cabecera + "00000";

                    Osw.WriteLine(Cabecera);
                    Osw2.WriteLine(Cabecera);
                    //FIN DETALLE
                }

                //CERRAR CONEXION
                MtGlb.ConnectionClose(ConexionDim);
                int t = 0;
                foreach (var item in Val)
                {

                    Osw.WriteLine(item);
                    Osw2.WriteLine(item);
                    t++;
                    if (t == 1755)
                    {
                        string Aqui = string.Empty;
                    }

                }

                Osw.Flush();
                Osw.Close();
                Osw2.Flush();
                Osw2.Close();

                Message = "Todo bien";
            }
            catch (Exception ex)
            {

                Message = ex.Message + ex.StackTrace;
                Message = Message.Replace(System.Environment.NewLine, "  ");
            }
        }

    }
}