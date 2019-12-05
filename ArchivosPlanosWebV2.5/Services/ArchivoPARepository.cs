using ArchivosPlanosWebV2._5.Models;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
namespace ArchivosPlanosWebV2._5.Services
{
        public class ArchivoPARepository
    {

        private AppDbContextSQL db = new AppDbContextSQL();
        public string Archivo_4;
        string Carpeta = @" C:\ArchivosPlanosWeb\";
        //string Carpeta2 = @"C:\Users\Desarrollo3\Desktop\ArchivosPlanosWeb\ArchivosPlanosWeb\Descargas\";
        string Carpeta2 = @"C:\inetpub\wwwroot\ArchivosPlanos\Descargas\";
        string StrIdentificador = "A";
        static string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
        static SqlConnection Connection = new SqlConnection(ConnectionString);
        public string Message = string.Empty;

        /// <summary>
        /// ARCHIVO PA
        /// </summary>
        /// <param name="Str_Turno_block"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="IdPlazaCobro"></param>
        /// <param name="CabeceraTag"></param>
        /// <param name="Tramo"></param>
        /// <returns></returns>
        public void eventos_detectados_y_marcados_en_el_ECT_EAP(string Str_Turno_block, DateTime FechaInicio, string IdPlazaCobro, string CabeceraTag, string Tramo, string Conexion)
        {
            string StrQuerys;
            string Linea;
            string Cabecera;
            string Pie;
            string Numero_archivo;
            string Nombre_archivo = string.Empty;
            double Numero_registros = 0;

            int Int_turno = 0;

            string H_inicio_turno = string.Empty;
            string H_fin_turno = string.Empty;

            string No_registros = string.Empty;

            string Str_detalle;
            string str_encargado;

            double Dbl_registros;

            string StrClaseExcedente;
            string StrCodigoVhMarcado = string.Empty;
            string strCodigoVhPagoMarcado;

            string Tag_iag;
            string Tarjeta;

            string StrQuerysTag;

            string Str_pre = string.Empty;
            string Str_det = string.Empty;
            string Str_marc = string.Empty;

            string Db_pre;
            string Db_det;
            string Db_det_ejes;
            string Db_marc;

            List<string> Info = new List<string>();
            int Cont_info = 1;

            bool Bl_det_a_pos = false;
            bool Bl_sin_pre = false;

            EnumerableRowCollection<DataRow> dataRows;
            var IdCarril = string.Empty;
            var NumCarril = string.Empty;
            var NumPlaza = string.Empty;
            var NumTramo = string.Empty;
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
                    if (IdPlazaCobro == "008")
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

                Nombre_archivo = Nombre_archivo + FechaInicio.ToString("MM") + FechaInicio.ToString("dd") + "." + Int_turno + "P" + StrIdentificador;

                System.IO.StreamWriter Osw = new System.IO.StreamWriter(Carpeta + Nombre_archivo);
                System.IO.StreamWriter Osw2 = new System.IO.StreamWriter(Carpeta2 + Nombre_archivo);

                Archivo_4 = Nombre_archivo;


                Cabecera = CabeceraTag;
                if (IdPlazaCobro.Length == 3)
                {
                    if (IdPlazaCobro == "008")
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

                Cabecera = Cabecera + FechaInicio.ToString("MM") + FechaInicio.ToString("dd") + "." + Int_turno + "P" + StrIdentificador + FechaInicio.ToString("dd/MM/yyyy") + Int_turno;

                //CABECERA INICIO REGISTROS

                //CABECERA FIN

                DateTime _H_inicio_turno = DateTime.ParseExact(H_inicio_turno, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                DateTime _H_fin_turno = DateTime.ParseExact(H_fin_turno, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                /*************************************************************************************************/
                MtGlb.ConnectionOpen(ConexionDim);
                /*************************************************************************************************/

                StrQuerys = "SELECT DATE_TRANSACTION, VOIE,ID_VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " +
                            "TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " +
                            "ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, ISSUER_ID, " +
                            "TYPE_CLASSE_PRE.LIBELLE_COURT1 AS CLASE_PRE, TRANSACTION.ID_CLASSE AS ID_CLASE_PRE, CODE1 AS PRE_EJES_EX " +
                            "FROM TRANSACTION " +
                            "JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " +
                            "LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " +
                            "LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_PRE  ON TRANSACTION.ID_CLASSE = TYPE_CLASSE_PRE.ID_CLASSE " +
                            "WHERE " +
                            "(DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                            " AND  ID_PAIEMENT  <> 0 " +
                            "AND (TRANSACTION.Id_Voie = '1' " +
                            //"OR TRANSACTION.Id_Voie = '2' " +
                            "OR TRANSACTION.Id_Voie = '3' " +
                            "OR TRANSACTION.Id_Voie = '4' " +
                            "OR TRANSACTION.Id_Voie = 'X') AND ID_VOIE <> 2 " +
                            "ORDER BY DATE_TRANSACTION";


                if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                {
                    Dbl_registros = MtGlb.Ds.Tables["TRANSACTION"].Rows.Count;

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

                    /**********************************************************************************************************/

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

                    //    //Cmd.Parameters.Add(new SqlParameter("carril", MtGlb.oDataRow["Voie"].ToString().Substring(1, 2)));
                    //    try
                    //    {
                    //        SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                    //        Da.Fill(dataTableCa);
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

                    var props = typeof(Type_Carril).GetProperties();
                    DataTable dt = new DataTable("Type_Carril");
                    var idpla = IdPlazaCobro.Substring(1, 2);
                    var Carriles_Plazas = db.Type_Plaza.GroupJoin(db.Type_Carril, pla => pla.Id_Plaza, car => car.Plaza_Id, (pla, car) => new { pla, car }).Where(x => x.pla.Num_Plaza == idpla).ToArray();
                    dt.Columns.AddRange(
                            props.Select(i => new DataColumn(i.Name, i.PropertyType)).ToArray()
                        );
                    Carriles_Plazas.FirstOrDefault().car.ToList().ForEach(
                            i => dt.Rows.Add(props.Select(p => p.GetValue(i, null)).ToArray())
                        );
                    /**********************************************************************************************************/

                    foreach (DataRow item in MtGlb.Ds.Tables["TRANSACTION"].Rows)
                    {
                        if (item["ID_VOIE"].ToString() == "2")
                            continue;

                        Str_detalle = string.Empty;

                        if (!DBNull.Value.Equals(item["CLASE_DETECTADA"]))
                        {
                            //Fecha del evento 	Fecha 	dd/mm/aaaa 
                            Str_detalle = Convert.ToDateTime(item["DATE_TRANSACTION"]).ToString("dd/MM/yyyy") + ",";
                            //Número de turno	Entero 	9
                            Str_detalle = Str_detalle + Int_turno + ",";
                            //Hora de evento 	Caracter 	hhmmss 
                            Str_detalle = Str_detalle + Convert.ToDateTime(item["DATE_TRANSACTION"]).ToString("HHmmss") + ",";

                            /**********************************/
                            dataRows = from MyRow in dt.AsEnumerable()
                                           //where MyRow.Field<string>("idCarril") == item["Voie"].ToString().Substring(1, 2)
                                       where MyRow.Field<string>("Num_Gea") == item["Voie"].ToString().Substring(1, 2)
                                       select MyRow;

                            foreach (DataRow value in dataRows)
                            {
                                NumCarril = value["Num_Capufe"].ToString();
                                NumTramo = value["Num_Tramo"].ToString();
                                NumPlaza = value.Field<Type_Plaza>("Type_Plaza").Num_Plaza.ToString();
                            }
                            /**********************************/

                            if (dataRows.Count() != 0)
                            {
                                if (IdPlazaCobro.Substring(1, 2) == "02")
                                {
                                    if (Convert.ToInt32(item["ID_PAIEMENT"]) == 2 && (NumCarril == "1803" || NumCarril == "1810"))
                                        NumTramo = "261";
                                }
                                else if (IdPlazaCobro.Substring(1, 2) == "86")
                                {
                                    if (Convert.ToInt32(item["ID_PAIEMENT"]) == 2 && Convert.ToString(item["Voie"]).Substring(0, 1).Trim() == "A")
                                        NumTramo = "340";
                                }
                                Str_detalle = Str_detalle + NumTramo + ",";
                                Str_detalle = Str_detalle + NumCarril + ",";
                            }
                            else
                            {
                                Str_detalle = Str_detalle + ",,";
                            }


                            //Cuerpo	Caracter 	X(1)
                            Str_detalle = Str_detalle + item["Voie"].ToString().Substring(0, 1) + ",";
                            //Número de evento 	Entero 	>>>>>>9
                            Str_detalle = Str_detalle + item["EVENT_NUMBER"] + ",";
                            //Número de folio 	Entero 	>>>>>>9 
                            Str_detalle = Str_detalle + item["FOLIO_ECT"] + ",";
                            //Código de vehículo detectado ECT 	Caracter 	X(6)

                            Str_pre = string.Empty;
                            Str_det = string.Empty;
                            Str_marc = string.Empty;
                            Db_det = Convert.ToString(0);
                            Db_det_ejes = Convert.ToString(0);

                            if (!DBNull.Value.Equals(item["CLASE_DETECTADA"]))
                            {
                                StrClaseExcedente = string.Empty;

                                if (item["CLASE_DETECTADA"].ToString() == "T01A")
                                {
                                    Str_detalle = Str_detalle + "T01" + ",";
                                    Str_det = "T01";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T01M")
                                {
                                    Str_detalle = Str_detalle + "T01" + ",";
                                    Str_det = "T01";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T01T")
                                {
                                    Str_detalle = Str_detalle + "T09P01" + ",";
                                    Str_det = "T09P01";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T02B")
                                {
                                    Str_detalle = Str_detalle + "T02" + ",";
                                    Str_det = "T02";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T03B")
                                {
                                    Str_detalle = Str_detalle + "T03" + ",";
                                    Str_det = "T03";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T04B")
                                {
                                    Str_detalle = Str_detalle + "T04" + ",";
                                    Str_det = "T04";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T02C")
                                {
                                    Str_detalle = Str_detalle + "T02" + ",";
                                    Str_det = "T02";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T03C")
                                {
                                    Str_detalle = Str_detalle + "T03" + ",";
                                    Str_det = "T03";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T04C")
                                {
                                    Str_detalle = Str_detalle + "T04" + ",";
                                    Str_det = "T04";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T05C")
                                {
                                    Str_detalle = Str_detalle + "T05" + ",";
                                    Str_det = "T05";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T06C")
                                {
                                    Str_detalle = Str_detalle + "T06" + ",";
                                    Str_det = "T06";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T07C")
                                {
                                    Str_detalle = Str_detalle + "T07" + ",";
                                    Str_det = "T07";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T08C")
                                {
                                    Str_detalle = Str_detalle + "T08" + ",";
                                    Str_det = "T08";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T09C")
                                {
                                    Str_detalle = Str_detalle + "T09" + ",";
                                    Str_det = "T09";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "TL01A")
                                {
                                    Str_detalle = Str_detalle + "T01L01" + ",";
                                    Str_det = "T01L01";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "TL02A")
                                {
                                    Str_detalle = Str_detalle + "T01L02" + ",";
                                    Str_det = "T01L02";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "TLnnA")
                                {
                                    Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(item["ID_OBS_TT"].ToString().Length == 1, "0" + item["ID_OBS_TT"], item["ID_OBS_TT"].ToString()) + ",";
                                    Str_det = "T01L" + MtGlb.IIf(item["ID_OBS_TT"].ToString().Length == 1, "0" + item["ID_OBS_TT"], item["ID_OBS_TT"].ToString());
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "T01P")
                                {
                                    Str_detalle = Str_detalle + "T01P" + ",";
                                    Str_det = "T01P";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "TP01C")
                                {
                                    Str_detalle = Str_detalle + "T09P01" + ",";
                                    Str_det = "T09P01";
                                }
                                else if (item["CLASE_DETECTADA"].ToString() == "TPnnC")
                                {
                                    Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(item["ID_OBS_TT"].ToString().Length == 1, "0" + item["ID_OBS_TT"], item["ID_OBS_TT"].ToString()) + ",";
                                    Str_det = "T09P" + MtGlb.IIf(item["ID_OBS_TT"].ToString().Length == 1, "0" + item["ID_OBS_TT"], item["ID_OBS_TT"].ToString());
                                }
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
                                                    "AND CODE = " + item["ID_PAIEMENT"] + " " +
                                                    "ORDER BY TABLE_TARIF.CODE ";

                            if (MtGlb.QueryDataSet4(StrQuerys, "TABLE_TARIF", ConexionDim))
                            {
                                if (Convert.ToInt32(item["ACD_CLASS"]) > 0 && Convert.ToInt32(item["ACD_CLASS"]) <= 9)
                                {
                                    Str_detalle = Str_detalle + item["MONTO_DETECTADO"] + ",,";
                                    Db_det = item["MONTO_DETECTADO"].ToString();
                                    Db_det_ejes = Convert.ToString(0);
                                }
                                else if (Convert.ToInt32(item["ACD_CLASS"]) >= 12 && Convert.ToInt32(item["ACD_CLASS"]) <= 15)
                                {
                                    Str_detalle = Str_detalle + item["MONTO_DETECTADO"] + ",,";
                                    Db_det = item["MONTO_DETECTADO"].ToString();
                                    Db_det_ejes = Convert.ToString(0);
                                }
                                //EXCEDENTES
                                else if (Convert.ToInt32(item["ACD_CLASS"]) >= 10 && Convert.ToInt32(item["ACD_CLASS"]) <= 11)
                                {
                                    Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                    Db_det = MtGlb.oDataRow4["Prix_Cl01"].ToString();
                                    Db_det_ejes = (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])).ToString();
                                }
                                else if (Convert.ToInt32(item["ACD_CLASS"]) == 16)
                                {
                                    Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                    Db_det = MtGlb.oDataRow4["Prix_Cl09"].ToString();
                                    Db_det_ejes = (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])).ToString();
                                }
                                else if (Convert.ToInt32(item["ACD_CLASS"]) == 17)
                                {
                                    Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                    Db_det = MtGlb.oDataRow4["Prix_Cl01"].ToString();
                                    Db_det_ejes = (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])).ToString();
                                }
                                else if (Convert.ToInt32(item["ACD_CLASS"]) == 18)
                                {
                                    Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                    Db_det = MtGlb.oDataRow4["Prix_Cl09"].ToString();
                                    Db_det_ejes = (Convert.ToInt32(item["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])).ToString();
                                }
                                else
                                    Str_detalle = Str_detalle + ",,";
                            }
                            else
                                Str_detalle = Str_detalle + ",,";

                            //Importe eje excedente detectado ECT 	Decimal 	>9.99 
                            //Código de vehículo marcado C-R	Caracter 	X(6)

                            if (!DBNull.Value.Equals(item["CLASE_MARCADA"]))
                            {
                                StrClaseExcedente = string.Empty;
                                StrCodigoVhMarcado = string.Empty;

                                if (item["CLASE_MARCADA"].ToString() == "T01A")
                                {
                                    Str_detalle = Str_detalle + "T01" + ",A,";
                                    Str_marc = "T01";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T01M")
                                {
                                    Str_detalle = Str_detalle + "T01" + ",M,";
                                    Str_marc = "T01M";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T01T")
                                {
                                    Str_detalle = Str_detalle + "T09P01" + ",C,";
                                    Str_marc = "T09P01";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T02B")
                                {
                                    Str_detalle = Str_detalle + "T02" + ",B,";
                                    Str_marc = "T02";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T03B")
                                {
                                    Str_detalle = Str_detalle + "T03" + ",B,";
                                    Str_marc = "T03";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T04B")
                                {
                                    Str_detalle = Str_detalle + "T04" + ",B,";
                                    Str_marc = "T04";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T02C")
                                {
                                    Str_detalle = Str_detalle + "T02" + ",C,";
                                    Str_marc = "T02";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T03C")
                                {
                                    Str_detalle = Str_detalle + "T03" + ",C,";
                                    Str_marc = "T03";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T04C")
                                {
                                    Str_detalle = Str_detalle + "T04" + ",C,";
                                    Str_marc = "T04";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T05C")
                                {
                                    Str_detalle = Str_detalle + "T05" + ",C,";
                                    Str_marc = "T05";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T06C")
                                {
                                    Str_detalle = Str_detalle + "T06" + ",C,";
                                    Str_marc = "T06";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T07C")
                                {
                                    Str_detalle = Str_detalle + "T07" + ",C,";
                                    Str_marc = "T07";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T08C")
                                {
                                    Str_detalle = Str_detalle + "T08" + ",C,";
                                    Str_marc = "T08";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T09C")
                                {
                                    Str_detalle = Str_detalle + "T09" + ",C,";
                                    Str_marc = "T09";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "TL01A")
                                {
                                    Str_detalle = Str_detalle + "T01L01" + ",A,";
                                    Str_marc = "T01L01";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "TL02A")
                                {
                                    Str_detalle = Str_detalle + "T01L02" + ",A,";
                                    Str_marc = "T01L02";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "TLnnA")
                                {
                                    Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(item["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + item["CODE_GRILLE_TARIF"], item["CODE_GRILLE_TARIF"].ToString()) + ",A,";
                                    Str_marc = "T01L" + MtGlb.IIf(item["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + item["CODE_GRILLE_TARIF"], item["CODE_GRILLE_TARIF"].ToString());
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "T01P")
                                {
                                    Str_detalle = Str_detalle + "T01P" + ",A,";
                                    Str_marc = "T01P";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "TP01C")
                                {
                                    Str_detalle = Str_detalle + "T09P01" + ",C,";
                                    Str_marc = "T09P01";
                                }
                                else if (item["CLASE_MARCADA"].ToString() == "TPnnC")
                                {
                                    Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(item["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + item["CODE_GRILLE_TARIF"], item["CODE_GRILLE_TARIF"].ToString()) + ",C,";
                                    Str_marc = "T09P" + MtGlb.IIf(item["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + item["CODE_GRILLE_TARIF"], item["CODE_GRILLE_TARIF"].ToString());
                                }
                                else
                                    Str_detalle = Str_detalle + "No detectada" + ",0,";
                            }
                            else
                                Str_detalle = Str_detalle + ",0,";

                            //Tipo de vehículo marcado C-R	Caracter 	X(1)

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
                                strCodigoVhPagoMarcado = "RP2" + ",";
                            else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 73)
                                strCodigoVhPagoMarcado = "RP3" + ",";
                            else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 74)
                                strCodigoVhPagoMarcado = "RP4" + ",";
                            else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 75) // Tres Marías: RPA -> RSP
                                strCodigoVhPagoMarcado = "RSP" + ",";
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

                            //borrar
                            StrQuerys = StrQuerys + "AND TABLE_TARIF.Version_Tarif = " + item["Version_Tarif"] + " " +
                                                    "AND CODE = " + item["ID_PAIEMENT"] + " " +
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

                            Bl_sin_pre = false;

                            if (IdPlazaCobro.Substring(1, 2) == "08")
                            {
                                if (item["Voie"].ToString().Trim() == "A14" || item["Voie"].ToString().Trim() == "A01" || item["Voie"].ToString().Trim() == "B14" || item["Voie"].ToString().Trim() == "B10")
                                    Bl_sin_pre = true;
                            }

                            //determino si tengo pre

                            Bl_det_a_pos = false;

                            if (IdPlazaCobro.Substring(1, 2) == "08")
                            {
                                if (item["Voie"].ToString().Trim() == "A08" || item["Voie"].ToString().Trim() == "A09" || item["Voie"].ToString().Trim() == "B01")
                                    Bl_det_a_pos = true;
                            }

                            //determino si tengo pre
                            //tres marias

                            Bl_sin_pre = false;

                            if (IdPlazaCobro.Substring(1, 2) == "09")
                            {
                                if (item["Voie"].ToString().Trim() == "A01" || item["Voie"].ToString().Trim() == "B02")
                                    Bl_sin_pre = true;
                            }
                            //paso morelos

                            Bl_det_a_pos = false;

                            if (IdPlazaCobro.Substring(1, 2) == "02")
                            {
                                if (item["Voie"].ToString().Trim() == "A02" || item["Voie"].ToString().Trim() == "B07")
                                    Bl_det_a_pos = true;

                                if (item["Voie"].ToString().Trim() == "A09" || item["Voie"].ToString().Trim() == "B10")
                                    Bl_sin_pre = true;
                            }

                            //determino si tengo pre
                            //ÁEROPUERTO()

                            Bl_sin_pre = false;

                            if (IdPlazaCobro.Substring(1, 2) == "06")
                            {
                                if (item["Voie"].ToString().Trim() == "B01" || item["Voie"].ToString().Trim() == "B03" || item["Voie"].ToString().Trim() == "A02" || item["Voie"].ToString().Trim() == "A04")
                                    Bl_sin_pre = true;
                            }

                            //tlalpan

                            Bl_sin_pre = false;

                            if (IdPlazaCobro.Substring(1, 2) == "08")
                            {
                                if (item["Voie"].ToString().Trim() == "B01" || item["Voie"].ToString().Trim() == "B04" || item["Voie"].ToString().Trim() == "A21")
                                    Bl_sin_pre = true;
                            }

                            //xochitepec

                            Bl_sin_pre = false;

                            if (IdPlazaCobro.Substring(1, 2) == "05")
                            {
                                if (item["Voie"].ToString().Trim() == "A01" || item["Voie"].ToString().Trim() == "A02")
                                    Bl_sin_pre = true;
                            }

                            //Alpuyeca
                            Bl_sin_pre = false;

                            if (IdPlazaCobro.Substring(1, 2) == "01")
                            {
                                if (item["Voie"].ToString().Trim() == "A01" || item["Voie"].ToString().Trim() == "A02" || item["Voie"].ToString().Trim() == "B03" || item["Voie"].ToString().Trim() == "B04")
                                    Bl_sin_pre = true;
                            }

                            //PALO BLANCO
                            Bl_sin_pre = false;

                            if (IdPlazaCobro.Substring(1, 2) == "03")
                            {
                                if (item["Voie"].ToString().Trim() == "A09" || item["Voie"].ToString().Trim() == "B01")
                                    Bl_sin_pre = true;
                            }

                            //LA VENTA
                            Bl_sin_pre = false;

                            if (IdPlazaCobro.Substring(1, 2) == "04")
                            {
                                if (item["Voie"].ToString().Trim() == "A08" || item["Voie"].ToString().Trim() == "B01")
                                    Bl_sin_pre = true;
                            }

                            //emiliano zapata pre
                            Bl_sin_pre = false;

                            if (IdPlazaCobro.Substring(1, 2) == "07")
                            {
                                Bl_sin_pre = true;
                            }

                            //Cerro gordo
                            if (IdPlazaCobro.Substring(1, 2) == "86")
                            {

                                if (item["Voie"].ToString().Trim() == "B21" || item["Voie"].ToString().Trim() == "B22" || item["Voie"].ToString().Trim() == "B23" || item["Voie"].ToString().Trim() == "B24" || item["Voie"].ToString().Trim() == "B25")
                                    Bl_sin_pre = true;
                            }

                            if (Bl_det_a_pos)
                            {
                                Str_detalle = Str_detalle + Str_det + ",";
                                Str_pre = Str_det;

                                Str_detalle = Str_detalle + Db_det + "," + Db_det_ejes;
                            }
                            else
                            {
                                if (!Bl_sin_pre)
                                {
                                    //PRECLASIFICADOS
                                    if (!DBNull.Value.Equals(item["CLASE_PRE"]))
                                    {
                                        StrClaseExcedente = string.Empty;

                                        if (item["CLASE_PRE"].ToString() == "T01A")
                                        {
                                            Str_detalle = Str_detalle + "T01" + ",";
                                            Str_pre = "T01";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T01M")
                                        {
                                            Str_detalle = Str_detalle + "T01" + ",";
                                            Str_pre = "T01";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T01T")
                                        {
                                            Str_detalle = Str_detalle + "T09P01" + ",";
                                            Str_pre = "T09P01";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T02B")
                                        {
                                            Str_detalle = Str_detalle + "T02" + ",";
                                            Str_pre = "T02";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T03B")
                                        {
                                            Str_detalle = Str_detalle + "T03" + ",";
                                            Str_pre = "T03";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T04B")
                                        {
                                            Str_detalle = Str_detalle + "T04" + ",";
                                            Str_pre = "T04";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T02C")
                                        {
                                            Str_detalle = Str_detalle + "T02" + ",";
                                            Str_pre = "T02";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T03C")
                                        {
                                            Str_detalle = Str_detalle + "T03" + ",";
                                            Str_pre = "T03";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T04C")
                                        {
                                            Str_detalle = Str_detalle + "T04" + ",";
                                            Str_pre = "T04";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T05C")
                                        {
                                            Str_detalle = Str_detalle + "T05" + ",";
                                            Str_pre = "T05";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T06C")
                                        {
                                            Str_detalle = Str_detalle + "T06" + ",";
                                            Str_pre = "T06";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T07C")
                                        {
                                            Str_detalle = Str_detalle + "T07" + ",";
                                            Str_pre = "T07";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T08C")
                                        {
                                            Str_detalle = Str_detalle + "T08" + ",";
                                            Str_pre = "T08";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T09C")
                                        {
                                            Str_detalle = Str_detalle + "T09" + ",";
                                            Str_pre = "T09";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "TL01A")
                                        {
                                            Str_detalle = Str_detalle + "T01L01" + ",";
                                            Str_pre = "T01L01";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "TL02A")
                                        {
                                            Str_detalle = Str_detalle + "T01L02" + ",";
                                            Str_pre = "T01L02";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "TLnnA")
                                        {
                                            if (MtGlb.IsNumeric(item["PRE_EJES_EX"].ToString()))
                                            {
                                                if (Convert.ToInt32(item["PRE_EJES_EX"]) != 0)
                                                {
                                                    Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(item["PRE_EJES_EX"].ToString().Length == 1, "0" + item["PRE_EJES_EX"], item["PRE_EJES_EX"].ToString()) + ",";
                                                    Str_pre = "T01L" + MtGlb.IIf(item["PRE_EJES_EX"].ToString().Length == 1, "0" + item["PRE_EJES_EX"], item["PRE_EJES_EX"].ToString());
                                                }
                                                else
                                                {
                                                    Str_detalle = Str_detalle + "T01L08" + ",";
                                                    Str_pre = "T01L08";
                                                }
                                            }
                                            else
                                            {
                                                Str_detalle = Str_detalle + "T01L08" + ",";
                                                Str_pre = "T01L08";
                                            }
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "T01P")
                                        {
                                            Str_detalle = Str_detalle + "T01P" + ",";
                                            Str_pre = "T01P";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "TP01C")
                                        {
                                            Str_detalle = Str_detalle + "T09P01" + ",";
                                            Str_pre = "T09P01";
                                        }
                                        else if (item["CLASE_PRE"].ToString() == "TPnnC")
                                        {
                                            if (MtGlb.IsNumeric(item["PRE_EJES_EX"].ToString()))
                                            {
                                                Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(item["PRE_EJES_EX"].ToString().Length == 1, "0" + item["PRE_EJES_EX"], item["PRE_EJES_EX"].ToString()) + ",";
                                                Str_pre = "T09P" + MtGlb.IIf(item["PRE_EJES_EX"].ToString().Length == 1, "0" + item["PRE_EJES_EX"], item["PRE_EJES_EX"].ToString());
                                            }
                                            else
                                            {
                                                Str_detalle = Str_detalle + "T01L08" + ",";
                                                Str_pre = "T01L08";
                                            }
                                        }
                                        else
                                        {
                                            Str_detalle = Str_detalle + "T01L08" + ",";
                                            Str_pre = "T01L08";
                                        }
                                    }
                                    else
                                    {
                                        Str_detalle = Str_detalle + "T01L08" + ",";
                                        Str_pre = "T01L08";
                                    }

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
                                                            "AND CODE = " + item["ID_PAIEMENT"] + " " +
                                                            "ORDER BY TABLE_TARIF.CODE ";

                                    if (MtGlb.QueryDataSet4(StrQuerys, "TABLE_TARIF", ConexionDim))
                                    {
                                        if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 1)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 2)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl02"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 3)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl03"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 4)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl04"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 5)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl05"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 6)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl06"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 7)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl07"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 8)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl08"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 9)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 12)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl12"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 13)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl13"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 14)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl14"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 15)
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl15"] + ",";
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 10)
                                        {
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + ",";
                                            Str_detalle = Str_detalle + 1 * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl17"]);
                                        }
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 11)
                                        {
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + ",";
                                            Str_detalle = Str_detalle + 2 * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl17"]);
                                        }
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 16)
                                        {
                                            if (MtGlb.IsNumeric(item["PRE_EJES_EX"].ToString()))
                                            {
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + ",";
                                                Str_detalle = Str_detalle + Convert.ToInt32(item["PRE_EJES_EX"]) * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl16"]);
                                            }
                                            else
                                            {
                                                Str_detalle = Str_detalle + "0,";
                                                Str_detalle = Str_detalle + "0";
                                            }
                                        }
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 17)
                                        {
                                            if (MtGlb.IsNumeric(item["PRE_EJES_EX"].ToString()))
                                            {
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + ",";
                                                Str_detalle = Str_detalle + (Convert.ToInt32(item["PRE_EJES_EX"]) * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl17"]));
                                            }
                                            else
                                            {
                                                Str_detalle = Str_detalle + "0,";
                                                Str_detalle = Str_detalle + "0";
                                            }
                                        }
                                        else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 18)
                                        {
                                            Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + ",";
                                            Str_detalle = Str_detalle + 1 * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl16"]);
                                        }
                                        else
                                            Str_detalle = Str_detalle + ",,";
                                    }
                                    else
                                    {
                                        Str_detalle = Str_detalle + ",,";
                                    }
                                }
                                //FIN PRE CLASIFICADOS
                            }
                        }
                        else
                        {
                            //inicio clase detectada 
                            StrQuerys = "SELECT DATE_TRANSACTION, VOIE,  EVENT_NUMBER, FOLIO_ECT, Version_Tarif, ID_PAIEMENT, " +
                                        "TAB_ID_CLASSE, TYPE_CLASSE.LIBELLE_COURT1 AS CLASE_MARCADA,  NVL(TRANSACTION.Prix_Total,0) as MONTO_MARCADO, " +
                                        "ACD_CLASS, TYPE_CLASSE_ETC.LIBELLE_COURT1 AS CLASE_DETECTADA, NVL(TRANSACTION.transaction_CPT1 / 100, 0) as MONTO_DETECTADO, CONTENU_ISO, CODE_GRILLE_TARIF, ID_OBS_MP, ID_OBS_TT, " +
                                        "TYPE_CLASSE_PRE.LIBELLE_COURT1 AS CLASE_PRE, TRANSACTION.ID_CLASSE AS ID_CLASE_PRE, CODE1 AS PRE_EJES_EX, ISSUER_ID " +
                                        "FROM TRANSACTION " +
                                        "JOIN TYPE_CLASSE ON TAB_ID_CLASSE = TYPE_CLASSE.ID_CLASSE  " +
                                        "LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_ETC  ON ACD_CLASS = TYPE_CLASSE_ETC.ID_CLASSE " +
                                        "LEFT JOIN TYPE_CLASSE   TYPE_CLASSE_PRE  ON TRANSACTION.ID_CLASSE = TYPE_CLASSE_PRE.ID_CLASSE " +
                                        "WHERE " +
                                        "(DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                        "AND VOIE = '" + item["VOIE"] + "' " +
                                        "AND  ID_OBS_SEQUENCE = '7' " +
                                        "AND EVENT_NUMBER = " + item["EVENT_NUMBER"] + " " +
                                        "AND (TRANSACTION.Id_Voie = '1' " +
                                        "OR TRANSACTION.Id_Voie = '2' " +
                                        "OR TRANSACTION.Id_Voie = '3' " +
                                        "OR TRANSACTION.Id_Voie = '4' " +
                                        "OR TRANSACTION.Id_Voie = 'X') " +
                                        "ORDER BY DATE_TRANSACTION";

                            if (MtGlb.QueryDataSet3(StrQuerys, "TRANSACTION", ConexionDim))
                            {
                                //Fecha del evento 	Fecha 	dd/mm/aaaa 
                                Str_detalle = Convert.ToDateTime(MtGlb.oDataRow3["DATE_TRANSACTION"]).ToString("dd/MM/yyyy") + ",";
                                //Número de turno	Entero 	9
                                Str_detalle = Str_detalle + Int_turno + ",";
                                //Hora de evento 	Caracter 	hhmmss 
                                Str_detalle = Str_detalle + Convert.ToDateTime(MtGlb.oDataRow3["DATE_TRANSACTION"]).ToString("HHmmss") + ",";

                                /****************************************************/
                                //Cmd.Parameters.Add(new SqlParameter("carril", MtGlb.oDataRow3["Voie"].ToString().Substring(1, 2)));
                                dataRows = from MyRow in dt.AsEnumerable()
                                               //where MyRow.Field<string>("idCarril") == MtGlb.oDataRow3["Voie"].ToString().Substring(1, 2)
                                           where MyRow.Field<string>("Num_Gea") == MtGlb.oDataRow3["Voie"].ToString().Substring(1, 2)
                                           select MyRow;

                                foreach (DataRow value in dataRows)
                                {
                                    NumCarril = value["Num_Capufe"].ToString();
                                    NumTramo = value["Num_Tramo"].ToString();
                                    NumPlaza = value.Field<Type_Plaza>("Type_Plaza").Num_Plaza.ToString();
                                }
                                /****************************************************/

                                if (dataRows.Count() != 0)
                                {
                                    if (IdPlazaCobro.Substring(1, 2) == "02")
                                    {
                                        if (Convert.ToInt32(item["ID_PAIEMENT"]) == 2 && (NumCarril == "1803" || NumCarril == "1810"))
                                            NumTramo = "261";
                                    }
                                    else if (IdPlazaCobro.Substring(1, 2) == "86")
                                    {
                                        if (Convert.ToInt32(item["ID_PAIEMENT"]) == 2 && Convert.ToString(MtGlb.oDataRow3["Voie"]).Substring(0, 1).Trim() == "A")
                                            NumTramo = "340";
                                    }
                                    Str_detalle = Str_detalle + NumTramo + ",";
                                    Str_detalle = Str_detalle + NumCarril + ",";
                                }
                                else
                                {
                                    Str_detalle = Str_detalle + ",,";
                                }

                                //Cuerpo	Caracter 	X(1)
                                Str_detalle = Str_detalle + MtGlb.oDataRow3["Voie"].ToString().Substring(0, 1) + ",";
                                //Número de evento 	Entero 	>>>>>>9
                                Str_detalle = Str_detalle + MtGlb.oDataRow3["EVENT_NUMBER"] + ",";
                                //Número de folio 	Entero 	>>>>>>9 
                                if (Convert.ToInt32(MtGlb.oDataRow3["FOLIO_ECT"]) == 0)
                                    Str_detalle = Str_detalle + item["FOLIO_ECT"] + ",";
                                else
                                    Str_detalle = Str_detalle + MtGlb.oDataRow3["FOLIO_ECT"] + ",";

                                //Código de vehículo detectado ECT Caracter    X(6)
                                Str_pre = string.Empty;
                                Str_det = string.Empty;
                                Str_marc = string.Empty;
                                Db_det = Convert.ToString(0);
                                Db_det_ejes = Convert.ToString(0);

                                if (!DBNull.Value.Equals(MtGlb.oDataRow3["CLASE_DETECTADA"]))
                                {
                                    StrClaseExcedente = string.Empty;

                                    if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T01A")
                                    {
                                        Str_detalle = Str_detalle + "T01" + ",";
                                        Str_det = "T01";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T01M")
                                    {
                                        Str_detalle = Str_detalle + "T01" + ",";
                                        Str_det = "T01";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T01T")
                                    {
                                        Str_detalle = Str_detalle + "T09P01" + ",";
                                        Str_det = "T09P01";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T02B")
                                    {
                                        Str_detalle = Str_detalle + "T02" + ",";
                                        Str_det = "T02";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T03B")
                                    {
                                        Str_detalle = Str_detalle + "T03" + ",";
                                        Str_det = "T03";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T04B")
                                    {
                                        Str_detalle = Str_detalle + "T04" + ",";
                                        Str_det = "T04";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T02C")
                                    {
                                        Str_detalle = Str_detalle + "T02" + ",";
                                        Str_det = "T02";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T03C")
                                    {
                                        Str_detalle = Str_detalle + "T03" + ",";
                                        Str_det = "T03";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T04C")
                                    {
                                        Str_detalle = Str_detalle + "T04" + ",";
                                        Str_det = "T04";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T05C")
                                    {
                                        Str_detalle = Str_detalle + "T05" + ",";
                                        Str_det = "T05";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T06C")
                                    {
                                        Str_detalle = Str_detalle + "T06" + ",";
                                        Str_det = "T06";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T07C")
                                    {
                                        Str_detalle = Str_detalle + "T07" + ",";
                                        Str_det = "T07";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T08C")
                                    {
                                        Str_detalle = Str_detalle + "T08" + ",";
                                        Str_det = "T08";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T09C")
                                    {
                                        Str_detalle = Str_detalle + "T09" + ",";
                                        Str_det = "T09";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TL01A")
                                    {
                                        Str_detalle = Str_detalle + "T01L01" + ",";
                                        Str_det = "T01L01";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TL02A")
                                    {
                                        Str_detalle = Str_detalle + "T01L02" + ",";
                                        Str_det = "T01L02";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TLnnA")
                                    {
                                        Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(MtGlb.oDataRow3["ID_OBS_TT"].ToString().Length == 1, "0" + MtGlb.oDataRow3["ID_OBS_TT"], MtGlb.oDataRow3["ID_OBS_TT"].ToString()) + ",";
                                        Str_det = "T01L" + MtGlb.IIf(MtGlb.oDataRow3["ID_OBS_TT"].ToString().Length == 1, "0" + MtGlb.oDataRow3["ID_OBS_TT"], MtGlb.oDataRow3["ID_OBS_TT"].ToString());
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "T01P")
                                    {
                                        Str_detalle = Str_detalle + "T01P" + ",";
                                        Str_det = "T01P";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TP01C")
                                    {
                                        Str_detalle = Str_detalle + "T09P01" + ",";
                                        Str_det = "T09P01";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_DETECTADA"].ToString() == "TPnnC")
                                    {
                                        Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(MtGlb.oDataRow3["ID_OBS_TT"].ToString().Length == 1, "0" + MtGlb.oDataRow3["ID_OBS_TT"], MtGlb.oDataRow3["ID_OBS_TT"].ToString()) + ",";
                                        Str_det = "T09P" + MtGlb.IIf(MtGlb.oDataRow3["ID_OBS_TT"].ToString().Length == 1, "0" + MtGlb.oDataRow3["ID_OBS_TT"], MtGlb.oDataRow3["ID_OBS_TT"].ToString());
                                    }
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
                                                        "AND CODE = " + item["ID_PAIEMENT"] + " " +
                                                        "ORDER BY TABLE_TARIF.CODE ";

                                if (MtGlb.QueryDataSet4(StrQuerys, "TABLE_TARIF", ConexionDim))
                                {
                                    if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) > 0 && Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) <= 9)
                                    {
                                        Str_detalle = Str_detalle + MtGlb.oDataRow3["MONTO_DETECTADO"] + ",,";
                                        Db_det = MtGlb.oDataRow3["MONTO_DETECTADO"].ToString();
                                        Db_det_ejes = Convert.ToString(0);
                                    }
                                    else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) >= 12 && Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) <= 15)
                                    {
                                        Str_detalle = Str_detalle + MtGlb.oDataRow3["MONTO_DETECTADO"] + ",,";
                                        Db_det = MtGlb.oDataRow3["MONTO_DETECTADO"].ToString();
                                        Db_det_ejes = Convert.ToString(0);
                                    }
                                    //EXCEDENTES
                                    else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) >= 10 && Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) <= 11)
                                    {
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                        Db_det = MtGlb.oDataRow4["Prix_Cl01"].ToString();
                                        Db_det_ejes = (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])).ToString();
                                    }
                                    else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) == 16)
                                    {
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                        Db_det = MtGlb.oDataRow4["Prix_Cl09"].ToString();
                                        Db_det_ejes = (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])).ToString();
                                    }
                                    else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) == 17)
                                    {
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])) + ",";
                                        Db_det = MtGlb.oDataRow4["Prix_Cl01"].ToString();
                                        Db_det_ejes = (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl01"])).ToString();
                                    }
                                    else if (Convert.ToInt32(MtGlb.oDataRow3["ACD_CLASS"]) == 18)
                                    {
                                        Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + "," + (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])) + ",";
                                        Db_det = MtGlb.oDataRow4["Prix_Cl09"].ToString();
                                        Db_det_ejes = (Convert.ToInt32(MtGlb.oDataRow3["MONTO_DETECTADO"]) - Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl09"])).ToString();
                                    }
                                    else
                                        Str_detalle = Str_detalle + ",,";
                                }
                                else
                                    Str_detalle = Str_detalle + ",,";

                                //Importe eje excedente detectado ECT 	Decimal 	>9.99 
                                //Código de vehículo marcado C-R	Caracter 	X(6)

                                if (!DBNull.Value.Equals(MtGlb.oDataRow3["CLASE_MARCADA"]))
                                {
                                    StrClaseExcedente = string.Empty;
                                    StrCodigoVhMarcado = string.Empty;

                                    if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T01A")
                                    {
                                        Str_detalle = Str_detalle + "T01" + ",A,";
                                        Str_marc = "T01";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T01M")
                                    {
                                        Str_detalle = Str_detalle + "T01" + ",M,";
                                        Str_marc = "T01M";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T01T")
                                    {
                                        Str_detalle = Str_detalle + "T09P01" + ",C,";
                                        Str_marc = "T09P01";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T02B")
                                    {
                                        Str_detalle = Str_detalle + "T02" + ",B,";
                                        Str_marc = "T02";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T03B")
                                    {
                                        Str_detalle = Str_detalle + "T03" + ",B,";
                                        Str_marc = "T03";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T04B")
                                    {
                                        Str_detalle = Str_detalle + "T04" + ",B,";
                                        Str_marc = "T04";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T02C")
                                    {
                                        Str_detalle = Str_detalle + "T02" + ",C,";
                                        Str_marc = "T02";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T03C")
                                    {
                                        Str_detalle = Str_detalle + "T03" + ",C,";
                                        Str_marc = "T03";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T04C")
                                    {
                                        Str_detalle = Str_detalle + "T04" + ",C,";
                                        Str_marc = "T04";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T05C")
                                    {
                                        Str_detalle = Str_detalle + "T05" + ",C,";
                                        Str_marc = "T05";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T06C")
                                    {
                                        Str_detalle = Str_detalle + "T06" + ",C,";
                                        Str_marc = "T06";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T07C")
                                    {
                                        Str_detalle = Str_detalle + "T07" + ",C,";
                                        Str_marc = "T07";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T08C")
                                    {
                                        Str_detalle = Str_detalle + "T08" + ",C,";
                                        Str_marc = "T08";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T09C")
                                    {
                                        Str_detalle = Str_detalle + "T09" + ",C,";
                                        Str_marc = "T09";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TL01A")
                                    {
                                        Str_detalle = Str_detalle + "T01L01" + ",A,";
                                        Str_marc = "T01L01";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TL02A")
                                    {
                                        Str_detalle = Str_detalle + "T01L02" + ",A,";
                                        Str_marc = "T01L02";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TLnnA")
                                    {
                                        Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + MtGlb.oDataRow3["CODE_GRILLE_TARIF"], MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString()) + ",A,";
                                        Str_marc = "T01L" + MtGlb.IIf(MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + MtGlb.oDataRow3["CODE_GRILLE_TARIF"], MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString());
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "T01P")
                                    {
                                        Str_detalle = Str_detalle + "T01P" + ",A,";
                                        Str_marc = "T01P";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TP01C")
                                    {
                                        Str_detalle = Str_detalle + "T09P01" + ",C,";
                                        Str_marc = "T09P01";
                                    }
                                    else if (MtGlb.oDataRow3["CLASE_MARCADA"].ToString() == "TPnnC")
                                    {
                                        Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + MtGlb.oDataRow3["CODE_GRILLE_TARIF"], MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString()) + ",C,";
                                        Str_marc = "T09P" + MtGlb.IIf(MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString().Length == 1, "0" + MtGlb.oDataRow3["CODE_GRILLE_TARIF"], MtGlb.oDataRow3["CODE_GRILLE_TARIF"].ToString());
                                    }
                                    else
                                        Str_detalle = Str_detalle + "No detectada" + ",0,";
                                }
                                else
                                    Str_detalle = Str_detalle + ",0,";

                                //Tipo de vehículo marcado C-R	Caracter 	X(1)
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
                                    strCodigoVhPagoMarcado = "RP2" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 73)
                                    strCodigoVhPagoMarcado = "RP3" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 74)
                                    strCodigoVhPagoMarcado = "RP4" + ",";
                                else if (Convert.ToInt32(item["ID_PAIEMENT"]) == 75) // Tres Marías: RPA -> RSP
                                    strCodigoVhPagoMarcado = "RSP" + ",";
                                else
                                    strCodigoVhPagoMarcado = ",";

                                //Importe vehículo marcado C-R[1]	Decimal 	>>9.99
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
                                                        "AND CODE = " + item["ID_PAIEMENT"] + " " +
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

                                Bl_sin_pre = false;

                                if (IdPlazaCobro.Substring(1, 2) == "08")
                                {
                                    if (item["Voie"].ToString().Trim() == "A14" || item["Voie"].ToString().Trim() == "A01" || item["Voie"].ToString().Trim() == "B14" || item["Voie"].ToString().Trim() == "B10")
                                        Bl_sin_pre = true;
                                }

                                //determino si tengo pre

                                Bl_det_a_pos = false;

                                if (IdPlazaCobro.Substring(1, 2) == "08")
                                {
                                    if (item["Voie"].ToString().Trim() == "A08" || item["Voie"].ToString().Trim() == "A09" || item["Voie"].ToString().Trim() == "B01")
                                        Bl_det_a_pos = true;
                                }

                                //determino si tengo pre
                                //tres marias

                                Bl_sin_pre = false;

                                if (IdPlazaCobro.Substring(1, 2) == "09")
                                {
                                    if (item["Voie"].ToString().Trim() == "A01" || item["Voie"].ToString().Trim() == "B02")
                                        Bl_sin_pre = true;
                                }
                                //paso morelos

                                Bl_det_a_pos = false;

                                if (IdPlazaCobro.Substring(1, 2) == "02")
                                {
                                    if (item["Voie"].ToString().Trim() == "A02" || item["Voie"].ToString().Trim() == "B07")
                                        Bl_det_a_pos = true;

                                    if (item["Voie"].ToString().Trim() == "A09" || item["Voie"].ToString().Trim() == "B10")
                                        Bl_sin_pre = true;
                                }

                                //determino si tengo pre
                                //ÁEROPUERTO()

                                Bl_sin_pre = false;

                                if (IdPlazaCobro.Substring(1, 2) == "06")
                                {
                                    if (item["Voie"].ToString().Trim() == "B01" || item["Voie"].ToString().Trim() == "B03" || item["Voie"].ToString().Trim() == "A02" || item["Voie"].ToString().Trim() == "A04")
                                        Bl_sin_pre = true;
                                }

                                //tlalpan

                                Bl_sin_pre = false;

                                if (IdPlazaCobro.Substring(1, 2) == "08")
                                {
                                    if (item["Voie"].ToString().Trim() == "B01" || item["Voie"].ToString().Trim() == "B04" || item["Voie"].ToString().Trim() == "A21")
                                        Bl_sin_pre = true;
                                }

                                Bl_sin_pre = false;

                                //xochitepec

                                Bl_sin_pre = false;

                                if (IdPlazaCobro.Substring(1, 2) == "05")
                                {
                                    if (item["Voie"].ToString().Trim() == "A01" || item["Voie"].ToString().Trim() == "A02")
                                        Bl_sin_pre = true;
                                }

                                //Alpuyeca
                                Bl_sin_pre = false;

                                if (IdPlazaCobro.Substring(1, 2) == "01")
                                {
                                    if (item["Voie"].ToString().Trim() == "A01" || item["Voie"].ToString().Trim() == "A02" || item["Voie"].ToString().Trim() == "B03" || item["Voie"].ToString().Trim() == "B04")
                                        Bl_sin_pre = true;
                                }

                                //PALO BLANCO
                                Bl_sin_pre = false;

                                if (IdPlazaCobro.Substring(1, 2) == "03")
                                {
                                    if (item["Voie"].ToString().Trim() == "A09" || item["Voie"].ToString().Trim() == "B01")
                                        Bl_sin_pre = true;
                                }

                                //LA VENTA
                                Bl_sin_pre = false;

                                if (IdPlazaCobro.Substring(1, 2) == "04")
                                {
                                    if (item["Voie"].ToString().Trim() == "A08" || item["Voie"].ToString().Trim() == "B01")
                                        Bl_sin_pre = true;
                                }

                                //emiliano zapata pre
                                Bl_sin_pre = false;

                                if (IdPlazaCobro.Substring(1, 2) == "07")
                                {
                                    Bl_sin_pre = true;
                                }

                                //VALIDACION CERRO GORDO NO ESTA AQUI POR LA VERSION 4.0.8 DE LA APLICACION ORIGINAL 

                                //'''''''''''''''''''''''''''''''

                                if (Bl_det_a_pos)
                                {
                                    Str_detalle = Str_detalle + Str_det + ",";
                                    Str_pre = Str_det;

                                    Str_detalle = Str_detalle + Db_det + "," + Db_det_ejes;
                                }
                                else
                                {
                                    if (!Bl_sin_pre)
                                    {
                                        //PRECLASIFICADOS
                                        if (!DBNull.Value.Equals(MtGlb.oDataRow3["CLASE_PRE"]))
                                        {
                                            StrClaseExcedente = string.Empty;

                                            if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T01A")
                                            {
                                                Str_detalle = Str_detalle + "T01" + ",";
                                                Str_pre = "T01";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T01M")
                                            {
                                                Str_detalle = Str_detalle + "T01" + ",";
                                                Str_pre = "T01";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T01T")
                                            {
                                                Str_detalle = Str_detalle + "T09P01" + ",";
                                                Str_pre = "T09P01";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T02B")
                                            {
                                                Str_detalle = Str_detalle + "T02" + ",";
                                                Str_pre = "T02";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T03B")
                                            {
                                                Str_detalle = Str_detalle + "T03" + ",";
                                                Str_pre = "T03";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T04B")
                                            {
                                                Str_detalle = Str_detalle + "T04" + ",";
                                                Str_pre = "T04";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T02C")
                                            {
                                                Str_detalle = Str_detalle + "T02" + ",";
                                                Str_pre = "T02";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T03C")
                                            {
                                                Str_detalle = Str_detalle + "T03" + ",";
                                                Str_pre = "T03";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T04C")
                                            {
                                                Str_detalle = Str_detalle + "T04" + ",";
                                                Str_pre = "T04";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T05C")
                                            {
                                                Str_detalle = Str_detalle + "T05" + ",";
                                                Str_pre = "T05";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T06C")
                                            {
                                                Str_detalle = Str_detalle + "T06" + ",";
                                                Str_pre = "T06";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T07C")
                                            {
                                                Str_detalle = Str_detalle + "T07" + ",";
                                                Str_pre = "T07";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T08C")
                                            {
                                                Str_detalle = Str_detalle + "T08" + ",";
                                                Str_pre = "T08";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T09C")
                                            {
                                                Str_detalle = Str_detalle + "T09" + ",";
                                                Str_pre = "T09";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "TL01A")
                                            {
                                                Str_detalle = Str_detalle + "T01L01" + ",";
                                                Str_pre = "T01L01";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "TL02A")
                                            {
                                                Str_detalle = Str_detalle + "T01L02" + ",";
                                                Str_pre = "T01L02";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "TLnnA")
                                            {
                                                if (MtGlb.IsNumeric(MtGlb.oDataRow3["PRE_EJES_EX"].ToString()))
                                                {
                                                    if (Convert.ToInt32(MtGlb.oDataRow3["PRE_EJES_EX"]) != 0)
                                                    {
                                                        Str_detalle = Str_detalle + "T01L" + MtGlb.IIf(MtGlb.oDataRow3["PRE_EJES_EX"].ToString().Length == 1, "0" + MtGlb.oDataRow3["PRE_EJES_EX"], MtGlb.oDataRow3["PRE_EJES_EX"].ToString()) + ",";
                                                        Str_pre = "T01L" + MtGlb.IIf(MtGlb.oDataRow3["PRE_EJES_EX"].ToString().Length == 1, "0" + MtGlb.oDataRow3["PRE_EJES_EX"], MtGlb.oDataRow3["PRE_EJES_EX"].ToString());
                                                    }
                                                    else
                                                    {
                                                        Str_detalle = Str_detalle + "T01L08" + ",";
                                                        Str_pre = "T01L08";
                                                    }
                                                }
                                                else
                                                {
                                                    Str_detalle = Str_detalle + "T01L08" + ",";
                                                    Str_pre = "T01L08";
                                                }
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "T01P")
                                            {
                                                Str_detalle = Str_detalle + "T01P" + ",";
                                                Str_pre = "T01P";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "TP01C")
                                            {
                                                Str_detalle = Str_detalle + "T09P01" + ",";
                                                Str_pre = "T09P01";
                                            }
                                            else if (MtGlb.oDataRow3["CLASE_PRE"].ToString() == "TPnnC")
                                            {
                                                if (MtGlb.IsNumeric(MtGlb.oDataRow3["PRE_EJES_EX"].ToString()))
                                                {
                                                    Str_detalle = Str_detalle + "T09P" + MtGlb.IIf(MtGlb.oDataRow3["PRE_EJES_EX"].ToString().Length == 1, "0" + MtGlb.oDataRow3["PRE_EJES_EX"], MtGlb.oDataRow3["PRE_EJES_EX"].ToString()) + ",";
                                                    Str_pre = "T09P" + MtGlb.IIf(MtGlb.oDataRow3["PRE_EJES_EX"].ToString().Length == 1, "0" + MtGlb.oDataRow3["PRE_EJES_EX"], MtGlb.oDataRow3["PRE_EJES_EX"].ToString());
                                                }
                                                else
                                                {
                                                    Str_detalle = Str_detalle + "T01L08" + ",";
                                                    Str_pre = "T01L08";
                                                }
                                            }
                                            else
                                            {
                                                Str_detalle = Str_detalle + "T01L08" + ",";
                                                Str_pre = "T01L08";
                                            }
                                        }
                                        else
                                        {
                                            Str_detalle = Str_detalle + "T01L08" + ",";
                                            Str_pre = "T01L08";
                                        }

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
                                                                "AND CODE = " + item["ID_PAIEMENT"] + " " +
                                                                "ORDER BY TABLE_TARIF.CODE ";

                                        if (MtGlb.QueryDataSet4(StrQuerys, "TABLE_TARIF", ConexionDim))
                                        {
                                            if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 1)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 2)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl02"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 3)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl03"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 4)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl04"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 5)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl05"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 6)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl06"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 7)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl07"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 8)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl08"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 9)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 12)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl12"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 13)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl13"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 14)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl14"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 15)
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl15"] + ",";
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 10)
                                            {
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + ",";
                                                Str_detalle = Str_detalle + 1 * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl17"]);
                                            }
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 11)
                                            {
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + ",";
                                                Str_detalle = Str_detalle + 2 * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl17"]);
                                            }
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 16)
                                            {
                                                if (MtGlb.IsNumeric(item["PRE_EJES_EX"].ToString()))
                                                {
                                                    Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + ",";
                                                    Str_detalle = Str_detalle + Convert.ToInt32(item["PRE_EJES_EX"]) * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl16"]);
                                                }
                                                else
                                                {
                                                    Str_detalle = Str_detalle + "0,";
                                                    Str_detalle = Str_detalle + "0";
                                                }
                                            }
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 17)
                                            {
                                                if (MtGlb.IsNumeric(item["PRE_EJES_EX"].ToString()))
                                                {
                                                    Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl01"] + ",";
                                                    Str_detalle = Str_detalle + (Convert.ToInt32(item["PRE_EJES_EX"]) * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl17"]));
                                                }
                                                else
                                                {
                                                    Str_detalle = Str_detalle + "0,";
                                                    Str_detalle = Str_detalle + "0";
                                                }
                                            }
                                            else if (Convert.ToInt32(item["ID_CLASE_PRE"]) == 18)
                                            {
                                                Str_detalle = Str_detalle + MtGlb.oDataRow4["Prix_Cl09"] + ",";
                                                Str_detalle = Str_detalle + 1 * Convert.ToInt32(MtGlb.oDataRow4["Prix_Cl16"]);
                                            }
                                            else
                                                Str_detalle = Str_detalle + ",,";
                                        }
                                        else
                                        {
                                            Str_detalle = Str_detalle + ",,";
                                        }
                                    }
                                    //fin clase detectada
                                }

                            }
                        }

                        //cont

                        if (!Bl_sin_pre)
                        {
                            if (Str_pre != Str_det || Str_det != Str_marc || Str_marc != Str_pre)
                            {

                                if (Str_detalle.IndexOf("-") == -1)
                                {
                                    Info.Add(Str_detalle);
                                }
                            }
                        }
                        else
                        {
                            if (Str_det != Str_marc)

                            {
                                if (Str_detalle.IndexOf("-") == -1)
                                {
                                    Info.Add(Str_detalle);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Cabecera = Cabecera + "00000";
                    Osw.WriteLine(Cabecera);
                    Osw.Flush();
                    Osw2.WriteLine(Cabecera);
                    Osw2.Flush();
                }

                if (!DBNull.Value.Equals(Info))
                {
                    Dbl_registros = Info.Count;

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

                    //detalle
                    foreach (var item in Info)
                    {
                        Osw.WriteLine(item);
                        Osw2.WriteLine(item);
                    }

                    Osw.Flush();
                    Osw.Close();
                    Osw2.Flush();
                    Osw2.Close();

                    Message = "Todo bien";
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message + " " + ex.StackTrace;
                Message = Message.Replace(System.Environment.NewLine, "  ");
            }
        }

    }

}