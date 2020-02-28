using ArchivosPlanosWebV2._5.Models;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace ArchivosPlanosWebV2._5.Services
{
    public class Archivo2ARepository

    {
        private MetodosGlbRepository MtGlb = new MetodosGlbRepository();
        private AppDbContextSQL db = new AppDbContextSQL();
        static string ConnectString = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
        static SqlConnection Connection = new SqlConnection(ConnectString);

        public string Archivo_2;
        string Carpeta = @" C:\ArchivosPlanosWeb\";
        //string Carpeta2 = @"C:\Users\Desarrollo3\Desktop\ArchivosPlanosWeb\ArchivosPlanosWeb\Descargas\";
        string Carpeta2 = @"C:\inetpub\wwwroot\ArchivosPlanos\Descargas\";
        string StrIdentificador = "A";
        public string Message = string.Empty;
        string StrLinea;
        string Str_Cajero_Receptor;

        /// <summary>
        /// ARCHIVO 2A
        /// </summary>
        /// <param name="Str_Turno_block"></param>
        /// <param name="FechaInicio"></param>
        /// <param name="IdPlazaCobro"></param>
        /// <param name="CabeceraTag"></param>
        /// <param name="Tramo"></param>
        /// <returns></returns>
        public void Preliquidaciones_de_cajero_receptor_para_transito_vehicular(string Str_Turno_block, DateTime FechaInicio, string IdPlazaCobro, string CabeceraTag, string Tramo, string Conexion)
        {
            string StrQuerys = string.Empty;
            string Cabecera = string.Empty;
            string Numero_archivo = string.Empty;
            string Nombre_archivo = string.Empty;
            int Int_turno = 0;
            string H_inicio_turno = string.Empty;
            string H_fin_turno = string.Empty;
            string No_registros = string.Empty;
            string Str_detalle = string.Empty;
            string Str_detalle_tc = string.Empty;
            string Str_encargado = string.Empty;
            double Dbl_registros = 0;
            string StrEncargadoTurno = string.Empty;
            int Cont_cerrado_todo_turno = 0;

            List<EventoCarril> ErroresHorario = new List<EventoCarril>();
            List<EventoCarril> ErroresCorregidos = new List<EventoCarril>();
            DataTable dt = new DataTable("Tabla_Carriles");
            DataSet dataSet = new DataSet();
            //DataTable dataTableCa = new DataTable();
            EnumerableRowCollection<DataRow> dataRows;
            OracleConnection ConexionDim = new OracleConnection(Conexion);
            var NumCarril = string.Empty;
            var NumPlaza = string.Empty;
            var NumTramo = string.Empty;
            bool dia_Restado = false;

            try
            {

                if (Str_Turno_block.Substring(0, 2) == "06")
                {
                    Int_turno = 5;
                    H_inicio_turno = FechaInicio.ToString("MM/dd/yyyy") + " 06:00:00";
                    H_fin_turno = FechaInicio.ToString("MM/dd/yyyy") + " 13:59:59";
                    dia_Restado = false;
                }
                else if (Str_Turno_block.Substring(0, 2) == "14")
                {
                    Int_turno = 6;
                    H_inicio_turno = FechaInicio.ToString("MM/dd/yyyy") + " 14:00:00";
                    H_fin_turno = FechaInicio.ToString("MM/dd/yyyy") + " 21:59:59";
                    dia_Restado = false;
                }
                else if (Str_Turno_block.Substring(0, 2) == "22")
                {
                    Int_turno = 4;
                    H_inicio_turno = FechaInicio.AddDays(-1).ToString("MM/dd/yyyy") + " 22:00:00";
                    H_fin_turno = FechaInicio.ToString("MM/dd/yyyy") + " 05:59:59";
                    dia_Restado = true;
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

                Nombre_archivo = Nombre_archivo + FechaInicio.ToString("MM") + FechaInicio.ToString("dd") + "." + Int_turno + "2" + StrIdentificador;

                StreamWriter Osw = new StreamWriter(Carpeta + Nombre_archivo);
                StreamWriter Osw2 = new StreamWriter(Carpeta2 + Nombre_archivo);


                Archivo_2 = Nombre_archivo;


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

                Cabecera = Cabecera + FechaInicio.ToString("MM") + FechaInicio.ToString("dd") + "." + Int_turno + "2" + StrIdentificador + FechaInicio.ToString("dd/MM/yyyy") + Int_turno;

                // CABECERA INICIO REGISTROS
                DateTime _H_inicio_turno = DateTime.ParseExact(H_inicio_turno, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime _H_fin_turno = DateTime.ParseExact(H_fin_turno, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                /*******************************************************************************/
                MtGlb.ConnectionOpen(ConexionDim);
                /*******************************************************************************/

                StrQuerys = "SELECT	FIN_POSTE.Id_Gare, " +
                                           "TYPE_VOIE.libelle_court_voie_L2, " +
                                           "Voie, " +
                                           "'zzz', " +
                                           "TO_CHAR(Numero_Poste,'FM09'), " +
                                           "TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " +
                                           "TO_CHAR(Date_Fin_Poste,'HH24:MI'), " +
                                           "Matricule, " +
                                           "Sac, " +
                                           "FIN_POSTE.Id_Voie, " +
                                           "DATE_DEBUT_POSTE,Date_Fin_Poste, " +
                                           "TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " +
                                           "TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " +
                                           ",TYPE_VOIE.libelle_court_voie " +
                                           ",FIN_POSTE_CPT22, " +
                                           "ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " +
                                           "FROM 	TYPE_VOIE, " +
                                           "FIN_POSTE, " +
                                           "SITE_GARE " +
                                           "WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " +
                                           "AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " +
                                           "AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " +
                                           "AND	SITE_GARE.id_reseau		= 	'01' " +
                                           "AND	SITE_GARE.id_Site		=	'" + IdPlazaCobro.Substring(1, 2) + "' " +
                                           "AND (Id_Mode_Voie IN (1,7,9)) " +
                                           "AND ((DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                           "AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                                           "AND (FIN_POSTE.Id_Voie = '1' " +
                                           "OR FIN_POSTE.Id_Voie = '2' " +
                                           "OR FIN_POSTE.Id_Voie = '3' " +
                                           "OR FIN_POSTE.Id_Voie = '4' " +
                                           "OR FIN_POSTE.Id_Voie = 'X' " +
                                           ") " +
                                           "ORDER BY Id_Gare, " +
                                           "Id_Voie, " +
                                           "Voie, " +
                                           "Date_Debut_Poste," +
                                           "Date_Fin_Poste, " +
                                           "Numero_Poste, " +
                                           "Matricule " +
                                           ",Sac";

                if (MtGlb.QueryDataSet(StrQuerys, "FIN_POSTE", ConexionDim))
                    Dbl_registros = MtGlb.Ds.Tables["FIN_POSTE"].Rows.Count;
                else
                    Dbl_registros = 0;

                if (IdPlazaCobro.Substring(1, 2) == "84")
                {
                    StrQuerys = "SELECT	FIN_POSTE.Id_Gare, " +
                                "TYPE_VOIE.libelle_court_voie_L2, " +
                                "Voie, " +
                                "'zzz', " +
                                "TO_CHAR(Numero_Poste,'FM09'), " +
                                "TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " +
                                "TO_CHAR(Date_Fin_Poste,'HH24:MI'), " +
                                "Matricule, " +
                                "Sac, " +
                                "FIN_POSTE.Id_Voie, " +
                                "DATE_DEBUT_POSTE,Date_Fin_Poste, " +
                                "TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " +
                                "TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " +
                                ",TYPE_VOIE.libelle_court_voie " +
                                ",FIN_POSTE_CPT22, " +
                                "ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " +
                                "FROM 	TYPE_VOIE, " +
                                "FIN_POSTE, " +
                                "SITE_GARE " +
                                "WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " +
                                "AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " +
                                "AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " +
                                "AND	SITE_GARE.id_reseau		= 	'01' " +
                                "AND	SITE_GARE.id_Site		=	'" + IdPlazaCobro.Substring(1, 2) + "' " +
                                "AND (Id_Mode_Voie IN (1,7,9)) " +
                                "AND ((DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                "AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                                "AND (FIN_POSTE.Id_Voie = '1' " +
                                "OR FIN_POSTE.Id_Voie = '2' " +
                                "OR FIN_POSTE.Id_Voie = '3' " +
                                "OR FIN_POSTE.Id_Voie = '4' " +
                                "OR FIN_POSTE.Id_Voie = 'X' " +
                                ")  and SUBSTR(Voie,1,1) = 'A'  " +
                                "ORDER BY Id_Gare, " +
                                "Id_Voie, " +
                                "Voie, " +
                                "Date_Debut_Poste," +
                                "Date_Fin_Poste, " +
                                "Numero_Poste, " +
                                "Matricule " +
                                ",Sac";

                    if (MtGlb.QueryDataSet(StrQuerys, "FIN_POSTE", ConexionDim))
                        Dbl_registros = Dbl_registros + MtGlb.Ds.Tables["FIN_POSTE"].Rows.Count;
                    else
                        Dbl_registros = Dbl_registros + 0;
                }
                else if (IdPlazaCobro.Substring(1, 2) == "02")
                {
                    //tramo corto
                    StrQuerys = "SELECT	FIN_POSTE.Id_Gare, " +
                                "TYPE_VOIE.libelle_court_voie_L2, " +
                                "Voie, " +
                                "'zzz', " +
                                "TO_CHAR(Numero_Poste,'FM09'), " +
                                "TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " +
                                "TO_CHAR(Date_Fin_Poste,'HH24:MI'), " +
                                "Matricule, " +
                                "Sac, " +
                                "FIN_POSTE.Id_Voie, " +
                                "DATE_DEBUT_POSTE,Date_Fin_Poste, " +
                                "TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " +
                                "TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " +
                                ",TYPE_VOIE.libelle_court_voie " +
                                ",FIN_POSTE_CPT22, " +
                                "ROUND((DATE_FIN_POSTE - DATE_DEBUT_POSTE) * (60 * 24), 2) AS time_in_minutes " +
                                "FROM 	TYPE_VOIE, " +
                                "FIN_POSTE, " +
                                "SITE_GARE " +
                                "WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " +
                                "AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " +
                                "AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " +
                                "AND	SITE_GARE.id_reseau		= 	'01' " +
                                "AND	SITE_GARE.id_Site		=	'" + IdPlazaCobro.Substring(1, 2) + "' " +
                                "AND (Id_Mode_Voie IN (1,7,9)) " +
                                "AND ((DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                "AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                                "AND (FIN_POSTE.Id_Voie = '1' " +
                                "OR FIN_POSTE.Id_Voie = '2' " +
                                "OR FIN_POSTE.Id_Voie = '3' " +
                                "OR FIN_POSTE.Id_Voie = '4' " +
                                "OR FIN_POSTE.Id_Voie = 'X' " +
                                ")  and (Voie = 'A01' OR Voie = 'B08')  " +
                                "ORDER BY Id_Gare, " +
                                "Id_Voie, " +
                                "Voie, " +
                                "Date_Debut_Poste," +
                                "Date_Fin_Poste, " +
                                "Numero_Poste, " +
                                "Matricule " +
                                ",Sac";

                    if (MtGlb.QueryDataSet(StrQuerys, "FIN_POSTE", ConexionDim))
                        Dbl_registros = Dbl_registros + MtGlb.Ds.Tables["FIN_POSTE"].Rows.Count;
                    else
                        Dbl_registros = Dbl_registros + 0;
                }

                StrQuerys = "SELECT ID_NETWORK, ID_PLAZA,ID_LANE, LANE, BEGIN_DHM, END_DHM, BAG_NUMBER, REPORT_FLAG, GENERATION_DHM " +
                            "FROM CLOSED_LANE_REPORT, SITE_GARE " +
                            "where " +
                            "CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " +
                            "AND	SITE_GARE.id_Site		=	'" + IdPlazaCobro.Substring(1, 2) + "' " +
                            "AND ((BEGIN_DHM >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                            "AND (BEGIN_DHM <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                            "order by BEGIN_DHM";

                if (MtGlb.QueryDataSet(StrQuerys, "CLOSED_LANE_REPORT", ConexionDim))
                    Dbl_registros = Dbl_registros + MtGlb.Ds.Tables["CLOSED_LANE_REPORT"].Rows.Count;
                else
                    Dbl_registros = Dbl_registros + 0;

                // carriles siempre cerrados
                // cont_cerrado_todo_turno

                StrQuerys = "SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD ";

                if (IdPlazaCobro == "106")
                {
                    StrQuerys = StrQuerys + "where VOIE <> 'B04' and VOIE <> 'A03' ";
                }

                if (MtGlb.QueryDataSet1(StrQuerys, "SEQ_VOIE_TOD", ConexionDim))
                {
                    for (int i = 0; i < MtGlb.Ds1.Tables["SEQ_VOIE_TOD"].Rows.Count; i++)
                    {
                        MtGlb.oDataRow1 = MtGlb.Ds1.Tables["SEQ_VOIE_TOD"].Rows[i];
                        MtGlb.oDataRow1.AcceptChanges();
                        StrQuerys = "SELECT	* FROM 	FIN_POSTE " +
                                     "WHERE	 VOIE = '" + MtGlb.oDataRow1["VOIE"] + "' " +
                                     "AND ((DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                     "AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) ";

                        if (MtGlb.QueryDataSet(StrQuerys, "FIN_POSTE", ConexionDim) == false)
                        {
                            StrQuerys = "SELECT * " +
                                        "FROM CLOSED_LANE_REPORT, SITE_GARE " +
                                        "where " +
                                        "CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " +
                                        "AND	SITE_GARE.id_Site		=	'" + IdPlazaCobro.Substring(1, 2) + "' " +
                                        "AND	LANE		=	'" + MtGlb.oDataRow1["VOIE"] + "' " +
                                        "AND ((BEGIN_DHM >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                        "AND (BEGIN_DHM <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                                        "order by BEGIN_DHM";

                            if (MtGlb.QueryDataSet(StrQuerys, "CLOSED_LANE_REPORT", ConexionDim) == false)
                                Cont_cerrado_todo_turno = Cont_cerrado_todo_turno + 1;
                        }
                    }
                }

                Dbl_registros = Dbl_registros + Cont_cerrado_todo_turno;

                // fin carriles siempre cerrados

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

                //INICIO DETALLE
                //**************************************
                //**************************************
                //**************************************

                string StrQuerys_varios_cortes;

                DateTime Hr_fecha_ini_varios_turnos = new DateTime();
                DateTime Hr_fecha_fin_varios_turnos = new DateTime();
                bool Bl_varios_turnos = false;

                DataSet odataSetReporte = new DataSet();
                DataTable oDataTableReporte = new DataTable("tabla_reporte");

                StrQuerys = StrLinea;

                string Str_id_RESEAU = string.Empty;
                int Int_id_gare = 0;
                string Str_id_voie = string.Empty;
                string Int_id_voie = string.Empty;
                int Int_numero_poste;
                string Dt_ini_poste = string.Empty;
                string Dt_fin_poste = string.Empty;
                string Str_MATRICULE = string.Empty;

                string Str_cajero = Str_Cajero_Receptor;
                string Id_plaza_cobro_local = IdPlazaCobro;

                //VARIABLES DE CAMPO
                string VAR01 = string.Empty;
                string VAR_01 = string.Empty;
                string VAR_08 = string.Empty;
                string VAR_02 = string.Empty;

                double VAR_11 = 0;
                double VAR_12 = 0;
                double VAR_13 = 0;
                double VAR_14 = 0;
                double VAR_15 = 0;
                double VAR_16 = 0;
                double VAR_17 = 0;
                //tramo corto
                double VAR_17_tc = 0;
                double VAR_18 = 0;
                double VAR_19 = 0;
                //tramo corto
                double VAR_19_tc = 0;

                double VAR_20 = 0;
                double VAR_21 = 0;
                double VAR_22 = 0;
                double VAR_23 = 0;

                double VAR_24 = 0;
                double VAR_25 = 0;
                double VAR_26 = 0;
                double VAR_27 = 0;

                double VAR_28 = 0;
                double VAR_29 = 0;
                double VAR_30 = 0;
                double VAR_31 = 0;
                double VAR_32 = 0;
                double VAR_33 = 0;
                double VAR_34 = 0;
                double VAR_35 = 0;
                double VAR_36 = 0;
                double VAR_37 = 0;
                double VAR_38 = 0;
                double VAR_39 = 0;

                double VAR_40 = 0;
                double VAR_41 = 0;
                double VAR_42 = 0;
                double VAR_43 = 0;
                double VAR_44 = 0;
                double VAR_45 = 0;
                double VAR_46 = 0;
                double VAR_47 = 0;
                double VAR_48 = 0;
                double VAR_49 = 0;

                double VAR_50 = 0;
                double VAR_51 = 0;
                double VAR_52 = 0;
                double VAR_53 = 0;
                double VAR_54 = 0;
                double VAR_55 = 0;
                double VAR_56 = 0;
                double VAR_57 = 0;
                double VAR_58 = 0;
                double VAR_59 = 0;

                double VAR_60 = 0;
                double VAR_61 = 0;
                double VAR_62 = 0;
                double VAR_63 = 0;
                double VAR_64 = 0;
                double VAR_65 = 0;
                double VAR_66 = 0;
                double VAR_67 = 0;
                double VAR_68 = 0;
                double VAR_69 = 0;

                double VAR_70 = 0;
                double VAR_71 = 0;
                double VAR_72 = 0;
                double VAR_73 = 0;
                double VAR_74 = 0;
                double VAR_75 = 0;
                double VAR_76 = 0;
                double VAR_77 = 0;
                double VAR_78 = 0;
                double VAR_79 = 0;

                double VAR_80 = 0;
                double VAR_81 = 0;
                double VAR_82 = 0;
                double VAR_83 = 0;
                double VAR_84 = 0;
                double VAR_85 = 0;
                double VAR_86 = 0;
                double VAR_87 = 0;
                double VAR_88 = 0;
                double VAR_89 = 0;

                double VAR_90 = 0;
                double VAR_91 = 0;
                double VAR_92 = 0;
                double VAR_93 = 0;
                double VAR_94 = 0;
                double VAR_95 = 0;
                double VAR_96 = 0;
                double VAR_97 = 0;
                double VAR_98 = 0;
                double VAR_99 = 0;

                double VAR_103 = 0;

                string No_Turno;

                double FOLIO_NUMBER_OPEN;
                double FOLIO_NUMBER_CLOSE;

                double FOLIO2_NUMBER_OPEN;
                double FOLIO2_NUMBER_CLOSE;

                double FOLIO3_NUMBER_OPEN;
                double FOLIO3_NUMBER_CLOSE;

                bool Bl_carril_automatico;

                //reporte cajero receptor / inicio turno

                //verifico cuantos cortes tiene en ese turno
                //Id_plaza_cobro_local = Id_plaza_cobro_local.Substring(1, 2);

                StrQuerys = @"SELECT GEADBA.FIN_POSTE.ID_RESEAU, GEADBA.FIN_POSTE.ID_GARE, GEADBA.TYPE_VOIE.LIBELLE_COURT_VOIE_L2, GEADBA.FIN_POSTE.VOIE, TO_CHAR(GEADBA.FIN_POSTE.NUMERO_POSTE, 'FM09') AS Expr1, " +
                            "TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'MM/DD/YY') AS Expr2, TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'HH24:MI') AS Expr3, " +
                            "GEADBA.FIN_POSTE.MATRICULE, GEADBA.FIN_POSTE.SAC AS Expr4, GEADBA.FIN_POSTE.ID_VOIE, " +
                            "TO_CHAR(GEADBA.FIN_POSTE.DATE_DEBUT_POSTE, 'YYYYMMDDHH24MISS') AS Expr5, TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'YYYYMMDDHH24MISS') AS Expr6, GEADBA.TYPE_VOIE.LIBELLE_COURT_VOIE, 0 AS Expr7, " +
                            "FOLIO_NUMBER_CLOSE, INITIAL_EVENT_NUMBER, FINAL_EVENT_NUMBER " +
                            "FROM GEADBA.TYPE_VOIE, GEADBA.FIN_POSTE, GEADBA.SITE_GARE " +
                            "WHERE GEADBA.TYPE_VOIE.ID_VOIE = GEADBA.FIN_POSTE.ID_VOIE AND GEADBA.FIN_POSTE.ID_RESEAU = GEADBA.SITE_GARE.ID_RESEAU AND " +
                            "GEADBA.FIN_POSTE.ID_GARE = GEADBA.SITE_GARE.ID_GARE AND (GEADBA.SITE_GARE.ID_RESEAU = '01') AND (GEADBA.SITE_GARE.ID_SITE = '" + IdPlazaCobro.Substring(1, 2) + "') AND " +
                            "(GEADBA.FIN_POSTE.ID_MODE_VOIE IN (1, 6, 7)) ";

                if (Str_Turno_block.Substring(0, 2) == "06")
                    No_Turno = "5 MATUTINO";
                else if (Str_Turno_block.Substring(0, 2) == "14")
                    No_Turno = "6 VESPERTINO";
                else if (Str_Turno_block.Substring(0, 2) == "22")
                    No_Turno = "4 NOCTURNO";

                StrQuerys = StrQuerys + "AND (GEADBA.FIN_POSTE.DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) AND " +
                                        "(GEADBA.FIN_POSTE.DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) ";

                StrQuerys = StrQuerys + "AND (GEADBA.FIN_POSTE.ID_VOIE = '1' OR " +
                                        "GEADBA.FIN_POSTE.ID_VOIE = '2' OR " +
                                        "GEADBA.FIN_POSTE.ID_VOIE = '3' OR " +
                                        "GEADBA.FIN_POSTE.ID_VOIE = '4' OR " +
                                        " GEADBA.FIN_POSTE.ID_VOIE = 'X') " +
                                        "AND GEADBA.FIN_POSTE.SAC is not null " +
                                        "ORDER BY GEADBA.FIN_POSTE.ID_GARE, GEADBA.TYPE_VOIE.ID_VOIE, GEADBA.FIN_POSTE.VOIE, GEADBA.FIN_POSTE.DATE_FIN_POSTE, " +
                                        "GEADBA.FIN_POSTE.NUMERO_POSTE, GEADBA.FIN_POSTE.MATRICULE, Expr4";

                if (MtGlb.QueryDataSet3(StrQuerys, "TYPE_VOIE", ConexionDim))
                {
                    /******************************************************************/

                    //BUSCAMOS EN LA BD SQLSERVER TODOS LOS CARRILES POR PLAZA
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
                    //    Cmd.Parameters.Add(new SqlParameter("plaza", Id_plaza_cobro_local));
                    //    //Cmd.Parameters.Add(new SqlParameter("carril", VAR_08.Substring(1, 2)));

                    //    try
                    //    {
                    //        SqlDataAdapter Da = new SqlDataAdapter(Cmd);
                    //        Da.Fill(dataTableCa);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //         Message = ex.Message + " " + ex.StackTrace;
                    //    }
                    //    finally
                    //    {
                    //        Cmd.Dispose();
                    //        Connection.Close();
                    //    }
                    //}

                    /******************************************************************/
                    var id_pla = IdPlazaCobro.Substring(1, 2);
                    var Carriles_Plazas = db.Type_Plaza.GroupJoin(db.Type_Carril, pla => pla.Id_Plaza, car => car.Plaza_Id, (pla, car) => new { pla, car }).Where(x => x.pla.Num_Plaza == id_pla).ToList();

                    var props = typeof(Type_Carril).GetProperties();

                    dt.Columns.AddRange(
                            props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray()
                        );
                    Carriles_Plazas.FirstOrDefault().car.ToList().ForEach(
                            i => dt.Rows.Add(props.Select(p => p.GetValue(i, null)).ToArray())
                        );
                    foreach (DataRow item3 in MtGlb.Ds3.Tables["TYPE_VOIE"].Rows)
                    {
                        Hr_fecha_ini_varios_turnos = Convert.ToDateTime(MtGlb.Fecha(item3["Expr5"].ToString()));
                        Hr_fecha_fin_varios_turnos = Convert.ToDateTime(MtGlb.Fecha(item3["Expr6"].ToString()));
                        Str_cajero = item3["MATRICULE"].ToString();

                        //fin de verifico cuantos cortes tiene en ese turno

                        //Inicio
                        //DATOS GENERALES

                        StrQuerys = "SELECT GEADBA.FIN_POSTE.ID_RESEAU, GEADBA.FIN_POSTE.ID_GARE, GEADBA.TYPE_VOIE.LIBELLE_COURT_VOIE_L2, GEADBA.FIN_POSTE.VOIE, TO_CHAR(GEADBA.FIN_POSTE.NUMERO_POSTE, 'FM09') AS Expr1, " +
                                    "TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'MM/DD/YY') AS Expr2, TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'HH24:MI') AS Expr3, " +
                                    "GEADBA.FIN_POSTE.MATRICULE, GEADBA.FIN_POSTE.SAC AS Expr4, GEADBA.FIN_POSTE.ID_VOIE, " +
                                    "TO_CHAR(GEADBA.FIN_POSTE.DATE_DEBUT_POSTE, 'YYYYMMDDHH24MISS') AS Expr5, TO_CHAR(GEADBA.FIN_POSTE.DATE_FIN_POSTE, 'YYYYMMDDHH24MISS') AS Expr6, GEADBA.TYPE_VOIE.LIBELLE_COURT_VOIE, 0 AS Expr7, " +
                                    "FOLIO_NUMBER_OPEN, FOLIO_NUMBER_CLOSE, INITIAL_EVENT_NUMBER, FINAL_EVENT_NUMBER, FOLIO_ECT_NUMBER_INITIAL, FOLIO_ECT_NUMBER_FINAL, " +
                                    "FOLIO2_NUMBER_INITIAL,FOLIO2_NUMBER_FINAL,FOLIO3_NUMBER_INITIAL,FOLIO3_NUMBER_FINAL " +
                                    "FROM GEADBA.TYPE_VOIE, GEADBA.FIN_POSTE, GEADBA.SITE_GARE " +
                                    "WHERE GEADBA.TYPE_VOIE.ID_VOIE = GEADBA.FIN_POSTE.ID_VOIE AND GEADBA.FIN_POSTE.ID_RESEAU = GEADBA.SITE_GARE.ID_RESEAU AND " +
                                    "GEADBA.FIN_POSTE.ID_GARE = GEADBA.SITE_GARE.ID_GARE AND (GEADBA.SITE_GARE.ID_RESEAU = '01') AND (GEADBA.SITE_GARE.ID_SITE = '" + IdPlazaCobro.Substring(1, 2) + "') AND " +
                                    "(GEADBA.FIN_POSTE.ID_MODE_VOIE IN (1, 6, 7)) ";

                        StrQuerys = StrQuerys + "AND (GEADBA.FIN_POSTE.DATE_DEBUT_POSTE = TO_DATE('" + Hr_fecha_ini_varios_turnos.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS'))  ";

                        StrQuerys = StrQuerys + "AND (GEADBA.FIN_POSTE.ID_VOIE = '1' OR " +
                                                "GEADBA.FIN_POSTE.ID_VOIE = '2' OR " +
                                                "GEADBA.FIN_POSTE.ID_VOIE = '3' OR " +
                                                "GEADBA.FIN_POSTE.ID_VOIE = '4' OR " +
                                                "GEADBA.FIN_POSTE.ID_VOIE = 'X') " +
                                                "AND (GEADBA.FIN_POSTE.MATRICULE = '" + Str_cajero + "') " +
                                                "AND FIN_POSTE.SAC = '" + item3["Expr4"] + "' " +
                                                "ORDER BY GEADBA.FIN_POSTE.ID_GARE, GEADBA.TYPE_VOIE.ID_VOIE, GEADBA.FIN_POSTE.VOIE, GEADBA.FIN_POSTE.DATE_FIN_POSTE, " +
                                                "GEADBA.FIN_POSTE.NUMERO_POSTE, GEADBA.FIN_POSTE.MATRICULE, Expr4";

                        if (MtGlb.QueryDataSet(StrQuerys, "TYPE_VOIE", ConexionDim))
                        {
                            VAR_08 = MtGlb.oDataRow["VOIE"].ToString();

                            Str_id_voie = MtGlb.oDataRow["VOIE"].ToString();
                            Int_id_voie = MtGlb.oDataRow["ID_VOIE"].ToString();
                            Int_numero_poste = Convert.ToInt32(MtGlb.oDataRow["Expr1"]);


                            Dt_ini_poste = Convert.ToDateTime(MtGlb.Fecha(MtGlb.oDataRow["Expr5"].ToString())).ToString("MM/dd/yyyy HH:mm:ss");

                            VAR_01 = _H_inicio_turno.ToString("dd/MM/yyyy");

                            Dt_fin_poste = Convert.ToDateTime(MtGlb.Fecha(MtGlb.oDataRow["Expr6"].ToString())).ToString("MM/dd/yyyy HH:mm:ss");


                            if (!Convert.ToBoolean(Convert.ToString(MtGlb.oDataRow["FOLIO_NUMBER_OPEN"]).Length == 10))
                                FOLIO_NUMBER_OPEN = Convert.ToInt64(MtGlb.oDataRow["FOLIO_NUMBER_OPEN"].ToString());
                            else
                                FOLIO_NUMBER_OPEN = Convert.ToInt64(MtGlb.oDataRow["FOLIO_NUMBER_OPEN"].ToString().Substring(0, 9));

                            if (!Convert.ToBoolean(Convert.ToString(MtGlb.oDataRow["FOLIO_NUMBER_CLOSE"]).Length == 10))
                                FOLIO_NUMBER_CLOSE = Convert.ToInt64(MtGlb.oDataRow["FOLIO_NUMBER_CLOSE"]);
                            else
                                FOLIO_NUMBER_CLOSE = Convert.ToInt64(MtGlb.oDataRow["FOLIO_NUMBER_CLOSE"].ToString().Substring(0, 9));


                            if (!Convert.ToBoolean(FOLIO_NUMBER_OPEN > FOLIO_NUMBER_CLOSE))
                                VAR_77 = FOLIO_NUMBER_OPEN;
                            else
                                VAR_77 = FOLIO_NUMBER_CLOSE;

                            VAR_78 = FOLIO_NUMBER_CLOSE;

                            if (!Convert.ToBoolean(Convert.ToString(MtGlb.oDataRow["FOLIO2_NUMBER_INITIAL"]).Length == 10))
                                FOLIO2_NUMBER_OPEN = Convert.ToInt64(MtGlb.oDataRow["FOLIO2_NUMBER_INITIAL"]);
                            else
                                FOLIO2_NUMBER_OPEN = Convert.ToInt64(MtGlb.oDataRow["FOLIO2_NUMBER_INITIAL"].ToString().Substring(0, 9));

                            if (!Convert.ToBoolean(Convert.ToString(MtGlb.oDataRow["FOLIO2_NUMBER_FINAL"]).Length == 10))
                                FOLIO2_NUMBER_CLOSE = Convert.ToInt64(MtGlb.oDataRow["FOLIO2_NUMBER_FINAL"]);
                            else
                                FOLIO2_NUMBER_CLOSE = Convert.ToInt64(MtGlb.oDataRow["FOLIO2_NUMBER_FINAL"].ToString().Substring(0, 9));

                            VAR_82 = FOLIO2_NUMBER_OPEN;
                            VAR_83 = FOLIO2_NUMBER_CLOSE;

                            if (!Convert.ToBoolean(Convert.ToString(MtGlb.oDataRow["FOLIO3_NUMBER_INITIAL"]).Length == 10))
                                FOLIO3_NUMBER_OPEN = Convert.ToInt64(MtGlb.oDataRow["FOLIO3_NUMBER_INITIAL"]);
                            else
                                FOLIO3_NUMBER_OPEN = Convert.ToInt64(MtGlb.oDataRow["FOLIO3_NUMBER_INITIAL"].ToString().Substring(0, 9));

                            if (!Convert.ToBoolean(Convert.ToString(MtGlb.oDataRow["FOLIO3_NUMBER_FINAL"]).Length == 10))
                                FOLIO3_NUMBER_CLOSE = Convert.ToInt64(MtGlb.oDataRow["FOLIO3_NUMBER_FINAL"]);
                            else
                                FOLIO3_NUMBER_CLOSE = Convert.ToInt64(MtGlb.oDataRow["FOLIO3_NUMBER_FINAL"].ToString().Substring(0, 9));

                            VAR_87 = FOLIO3_NUMBER_OPEN;
                            VAR_88 = FOLIO3_NUMBER_CLOSE;

                            Bl_carril_automatico = false;

                            if (Id_plaza_cobro_local == "08")
                            {
                                if (Str_id_voie == "A08" || Str_id_voie == "A09" || Str_id_voie == "B01")
                                    Bl_carril_automatico = true;
                            }

                            if (Convert.ToInt32(MtGlb.oDataRow["FOLIO_ECT_NUMBER_INITIAL"].ToString()) < Convert.ToInt32(MtGlb.oDataRow["FOLIO_ECT_NUMBER_FINAL"].ToString()))
                            {
                                if (Bl_carril_automatico == false)
                                {
                                    VAR_92 = Convert.ToInt64(MtGlb.oDataRow["FOLIO_ECT_NUMBER_INITIAL"].ToString());
                                    VAR_93 = Convert.ToInt64(MtGlb.oDataRow["FOLIO_ECT_NUMBER_FINAL"].ToString());
                                }
                                else
                                {
                                    StrQuerys = "select  MIN(FOLIO_ECT) AS FOLIO_ECT_NUMBER_INITIAL_C, MAX(FOLIO_ECT) AS FOLIO_ECT_NUMBER_FINAL_C FROM TRANSACTION WHERE ID_GARE = '" + MtGlb.oDataRow["ID_GARE"] + "' AND ID_VOIE  = '" + MtGlb.oDataRow["ID_VOIE"] + "'  AND VOIE  = '" + MtGlb.oDataRow["VOIE"] + "' " +
                                                "AND  DATE_DEBUT_POSTE = TO_DATE('" + MtGlb.oDataRow["Expr5"] + "', 'YYYYMMDDHH24MISS') and FOLIO_ECT <> 0   ";

                                    if (MtGlb.QueryDataSet2(StrQuerys, "TRANSACTION", ConexionDim))
                                    {
                                        if (!DBNull.Value.Equals(MtGlb.oDataRow2["FOLIO_ECT_NUMBER_INITIAL_C"]))
                                        {
                                            VAR_92 = Convert.ToInt64(MtGlb.oDataRow2["FOLIO_ECT_NUMBER_INITIAL_C"].ToString());
                                            VAR_93 = Convert.ToInt64(MtGlb.oDataRow2["FOLIO_ECT_NUMBER_FINAL_C"].ToString());
                                        }
                                        else
                                        {
                                            VAR_92 = 0;
                                            VAR_93 = 0;
                                        }
                                    }
                                    else
                                    {
                                        VAR_92 = 0;
                                        VAR_93 = 0;
                                    }
                                }
                            }
                            else
                            {
                                StrQuerys = "select  MIN(FOLIO_ECT) AS FOLIO_ECT_NUMBER_INITIAL_C, MAX(FOLIO_ECT) AS FOLIO_ECT_NUMBER_FINAL_C FROM TRANSACTION WHERE ID_GARE = '" + MtGlb.oDataRow["ID_GARE"] + "' AND ID_VOIE  = '" + MtGlb.oDataRow["ID_VOIE"] + "'  AND VOIE  = '" + MtGlb.oDataRow["VOIE"] + "' " +
                                            "AND  DATE_DEBUT_POSTE = TO_DATE('" + MtGlb.oDataRow["Expr5"] + "', 'YYYYMMDDHH24MISS') and FOLIO_ECT <> 0   ";

                                if (MtGlb.QueryDataSet2(StrQuerys, "TRANSACTION", ConexionDim))
                                {
                                    if (!DBNull.Value.Equals(MtGlb.oDataRow2["FOLIO_ECT_NUMBER_INITIAL_C"]))
                                    {
                                        VAR_92 = Convert.ToInt64(MtGlb.oDataRow2["FOLIO_ECT_NUMBER_INITIAL_C"].ToString());
                                        VAR_93 = Convert.ToInt64(MtGlb.oDataRow2["FOLIO_ECT_NUMBER_FINAL_C"].ToString());
                                    }
                                    else
                                    {
                                        VAR_92 = 0;
                                        VAR_93 = 0;
                                    }
                                }
                                else
                                {
                                    VAR_92 = 0;
                                    VAR_93 = 0;
                                }
                            }

                            if (Convert.ToInt32(MtGlb.oDataRow["INITIAL_EVENT_NUMBER"].ToString()) < Convert.ToInt32(MtGlb.oDataRow["FINAL_EVENT_NUMBER"].ToString()))
                            {
                                if (Bl_carril_automatico == false)
                                {
                                    StrQuerys = "select  MIN(EVENT_NUMBER) AS INITIAL_EVENT_NUMBER_C, MAX(EVENT_NUMBER) AS FINAL_EVENT_NUMBER_C FROM TRANSACTION WHERE ID_GARE = '" + MtGlb.oDataRow["ID_GARE"] + "' AND ID_VOIE  = '" + MtGlb.oDataRow["ID_VOIE"] + "'  AND VOIE  = '" + MtGlb.oDataRow["VOIE"] + "' " +
                                                "AND  DATE_DEBUT_POSTE = TO_DATE('" + MtGlb.oDataRow["Expr5"] + "', 'YYYYMMDDHH24MISS')  AND EVENT_NUMBER <> 0 AND ID_OBS_PASSAGE <> '1' ";

                                    if (MtGlb.QueryDataSet2(StrQuerys, "TRANSACTION", ConexionDim))
                                    {
                                        if (!DBNull.Value.Equals(MtGlb.oDataRow2["INITIAL_EVENT_NUMBER_C"]))
                                        {
                                            VAR_97 = Convert.ToInt64(MtGlb.oDataRow2["INITIAL_EVENT_NUMBER_C"].ToString());
                                            VAR_98 = Convert.ToInt64(MtGlb.oDataRow2["FINAL_EVENT_NUMBER_C"].ToString());
                                        }
                                        else
                                        {
                                            VAR_97 = 0;
                                            VAR_98 = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    StrQuerys = "select  MIN(EVENT_NUMBER) AS INITIAL_EVENT_NUMBER_C, MAX(EVENT_NUMBER) AS FINAL_EVENT_NUMBER_C FROM TRANSACTION WHERE ID_GARE = '" + MtGlb.oDataRow["ID_GARE"] + "' AND ID_VOIE  = '" + MtGlb.oDataRow["ID_VOIE"] + "'  AND VOIE  = '" + MtGlb.oDataRow["VOIE"] + "' " +
                                                "AND  DATE_DEBUT_POSTE = TO_DATE('" + MtGlb.oDataRow["Expr5"] + "', 'YYYYMMDDHH24MISS')  AND EVENT_NUMBER <> 0 AND ID_OBS_PASSAGE <> '1' ";

                                    if (MtGlb.QueryDataSet2(StrQuerys, "TRANSACTION", ConexionDim))
                                    {
                                        if (!DBNull.Value.Equals(MtGlb.oDataRow2["INITIAL_EVENT_NUMBER_C"]))
                                        {
                                            VAR_97 = Convert.ToInt64(MtGlb.oDataRow2["INITIAL_EVENT_NUMBER_C"].ToString());
                                            VAR_98 = Convert.ToInt64(MtGlb.oDataRow2["FINAL_EVENT_NUMBER_C"].ToString());
                                        }
                                        else
                                        {
                                            VAR_97 = 0;
                                            VAR_98 = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                StrQuerys = "select  MIN(EVENT_NUMBER) AS INITIAL_EVENT_NUMBER_C, MAX(EVENT_NUMBER) AS FINAL_EVENT_NUMBER_C FROM TRANSACTION WHERE ID_GARE = '" + MtGlb.oDataRow["ID_GARE"] + "' AND ID_VOIE  = '" + MtGlb.oDataRow["ID_VOIE"] + "'  AND VOIE  = '" + MtGlb.oDataRow["VOIE"] + "' " +
                                            "AND  DATE_DEBUT_POSTE = TO_DATE('" + MtGlb.oDataRow["Expr5"] + "', 'YYYYMMDDHH24MISS')  AND EVENT_NUMBER <> 0 AND ID_OBS_PASSAGE <> '1' ";

                                if (MtGlb.QueryDataSet2(StrQuerys, "TRANSACTION", ConexionDim))
                                {
                                    if (!DBNull.Value.Equals(MtGlb.oDataRow2["INITIAL_EVENT_NUMBER_C"]))
                                    {
                                        VAR_97 = Convert.ToInt64(MtGlb.oDataRow2["INITIAL_EVENT_NUMBER_C"]);
                                        VAR_98 = Convert.ToInt64(MtGlb.oDataRow2["FINAL_EVENT_NUMBER_C"]);
                                    }
                                    else
                                    {
                                        VAR_97 = 0;
                                        VAR_98 = 0;
                                    }
                                }
                            }
                        }

                        //OPERADOR
                        StrQuerys = "SELECT MATRICULE , rtrim(NOM)||' '||rtrim(PRENOM) as nombre " +
                                    "FROM TABLE_PERSONNEL " +
                                    "WHERE Matricule = '" + MtGlb.oDataRow["MATRICULE"] + "'";

                        if (MtGlb.QueryDataSet4(StrQuerys, "TABLE_PERSONNEL", ConexionDim))
                            Str_MATRICULE = MtGlb.oDataRow4["MATRICULE"].ToString().Trim();

                        StrQuerys = "SELECT  ID_RESEAU, ID_GARE, NOM_GARE, NOM_GARE_L2 " +
                                    "FROM GEADBA.TYPE_GARE " +
                                    "WHERE ID_GARE = " + MtGlb.oDataRow["ID_GARE"] + "";

                        if (MtGlb.QueryDataSet4(StrQuerys, "TYPE_GARE", ConexionDim))
                        {
                            Int_id_gare = Convert.ToInt32(MtGlb.oDataRow4["ID_GARE"]);
                            Str_id_RESEAU = MtGlb.oDataRow4["ID_RESEAU"].ToString();
                        }


                        //pantalla: RESUMEN DE INGRESO CAJERO
                        StrQuerys = "SELECT MATRICULE,SAC AS Expr1, NVL(NB_POSTE, 0) + NVL(NB_POSTE_POS, 0) AS Expr2, NVL(RED_RCT_MONNAIE1, 0) AS Expr3, " +
                                    "NVL(RED_RCT_CHQ, 0) AS Expr4, NVL(RED_NB_CHQ, 0) AS Expr5, NVL(RED_RCT_DEVISE, 0) AS Expr6, NVL(RED_RCT_MONNAIE3, 0) AS Expr7, " +
                                    "NVL(RED_RCT_MONNAIE4, 0) AS Expr8, NVL(RED_CPT1, 0) AS Expr9, NVL(RED_RCT_MONNAIE1, 0) + NVL(RED_RCT_MONNAIE2, 0) + NVL(RED_RCT_MONNAIE3, 0) " +
                                    "+ NVL(RED_RCT_MONNAIE4, 0) + NVL(RED_RCT_CHQ, 0) + NVL(RED_RCT_DEVISE, 0) AS Expr10, NVL(POSTE_RCT_MONNAIE1, 0) + NVL(POSTE_RCT_MONNAIE2, 0) " +
                                    "+ NVL(POSTE_RCT_MONNAIE3, 0) + NVL(POSTE_RCT_MONNAIE4, 0) + NVL(POSTE_RCT_DEVISE, 0) + NVL(POSTE_RCT_CHQ, 0) + NVL(POSTE_POS_RCT_MONNAIE1, " +
                                    "0) + NVL(POSTE_POS_RMB_MONNAIE1, 0) + NVL(POSTE_POS_RCT_MONNAIE2, 0) + NVL(POSTE_POS_RCT_MONNAIE3, 0) + NVL(POSTE_POS_RCT_MONNAIE4, 0) " +
                                    "+ NVL(POSTE_POS_RCT_DEVISE, 0) + NVL(POSTE_POS_RCT_CHQ, 0) AS Expr11, NVL(RED_RCT_MONNAIE1, 0) + NVL(RED_RCT_MONNAIE2, 0) " +
                                    "+ NVL(RED_RCT_MONNAIE3, 0) + NVL(RED_RCT_MONNAIE4, 0) + NVL(RED_RCT_DEVISE, 0) + NVL(RED_RCT_CHQ, 0) - NVL(POSTE_RCT_MONNAIE1, 0) " +
                                    "- NVL(POSTE_RCT_MONNAIE2, 0) - NVL(POSTE_RCT_MONNAIE3, 0) - NVL(POSTE_RCT_MONNAIE4, 0) - NVL(POSTE_RCT_DEVISE, 0) - NVL(POSTE_RCT_CHQ, 0) " +
                                    "- NVL(POSTE_POS_RCT_MONNAIE1, 0) - NVL(POSTE_POS_RMB_MONNAIE1, 0) - NVL(POSTE_POS_RCT_MONNAIE2, 0) - NVL(POSTE_POS_RCT_MONNAIE3, 0) " +
                                    "- NVL(POSTE_POS_RCT_MONNAIE4, 0) - NVL(POSTE_POS_RCT_DEVISE, 0) - NVL(POSTE_POS_RCT_CHQ, 0) AS Expr12, NVL(RED_RCT_MONNAIE1, 0) " +
                                    "- NVL(POSTE_RCT_MONNAIE1, 0) - NVL(POSTE_POS_RMB_MONNAIE1, 0) - NVL(POSTE_POS_RCT_MONNAIE1, 0) AS Expr13, NVL(RED_RCT_CHQ, 0) " +
                                    "- NVL(POSTE_RCT_CHQ, 0) - NVL(POSTE_POS_RCT_CHQ, 0) AS Expr14, NVL(RED_RCT_DEVISE, 0) - NVL(POSTE_RCT_DEVISE, 0) - NVL(POSTE_POS_RCT_DEVISE, 0) " +
                                    "AS Expr15, NVL(RED_CPT24, 0) AS Expr16, NVL(RED_CPT25, 0) AS Expr17, NVL(RED_JETON, 0) - NVL(POSTE_JETON, 0) AS Expr18, NVL(RED_RDD, 0) " +
                                    "- NVL(POSTE_RDD, 0) AS Expr19, NVL(RED_GRATUIT, 0) + NVL(RED_CPT2, 0) - NVL(POSTE_GRATUIT, 0) AS Expr20, MATRICULE_COMMENTAIRE, COMMENTAIRE, " +
                                    "0 AS Expr21, TO_CHAR(DATE_REDDITION, 'YYYYMMDDHH24MISS') AS Expr22, ID_SITE, RED_CPT21, NB_POSTE, ETAT_REDDITION, " +
                                    "NVL(Red_Rct_Monnaie1,0)	+ NVL(Red_Rct_Devise,0)	+ NVL(Red_Rct_Chq,0) + NVL(Red_cpt21,0)	- NVL(Poste_Rct_Monnaie1,0) - NVL(Poste_Rct_Devise,0) - NVL(Poste_Rct_Chq,0) AS Expr23, RED_CPT22 " +
                                    "FROM REDDITION " +
                                    "WHERE  (ID_SITE = '" + IdPlazaCobro.Substring(1, 2) + "') AND (MATRICULE = '" + MtGlb.oDataRow["MATRICULE"] + "') AND (SAC = '" + MtGlb.oDataRow["Expr4"] + "')";

                        if (MtGlb.QueryDataSet2(StrQuerys, "REDDITION", ConexionDim))
                        {
                            VAR_11 = Convert.ToInt64(MtGlb.oDataRow2["Expr3"].ToString());
                            VAR_58 = Convert.ToInt64(MtGlb.oDataRow2["Expr6"].ToString());

                            if (!DBNull.Value.Equals(MtGlb.oDataRow2["RED_CPT21"]))
                                VAR_71 = Convert.ToInt64(MtGlb.oDataRow2["RED_CPT21"]);
                            else
                                VAR_71 = 0;

                            if (Convert.ToInt32(MtGlb.oDataRow2["Expr23"]) > 0)
                                VAR_73 = 0;
                            else if (Convert.ToInt32(MtGlb.oDataRow2["Expr23"]) == 0)
                                VAR_73 = 0;
                            else if (Convert.ToInt32(MtGlb.oDataRow2["Expr23"]) < 0)
                                VAR_73 = Math.Abs(Convert.ToInt32(MtGlb.oDataRow2["Expr23"]));

                            VAR_02 = MtGlb.oDataRow2["Expr1"].ToString();

                            VAR_02 = VAR_02.Replace("A", "");
                            VAR_02 = VAR_02.Replace("B", "");

                            //BOLETOS GENERADOS POR ERROR
                            VAR_13 = 0;
                            VAR_12 = 0;
                        }

                        //---------------------------------------

                        //VEHICULOS RESIDENTES PAGO INMEDIATO
                        DateTime _Dt_ini_poste = DateTime.ParseExact(Dt_ini_poste, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime _Dt_fin_poste = DateTime.ParseExact(Dt_fin_poste, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1, SUM(PRIX_TOTAL) AS Monto_2, COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND (ID_PAIEMENT = 71 " +
                                                "OR ID_PAIEMENT = 72 " +
                                                "OR ID_PAIEMENT = 73 " +
                                                "OR ID_PAIEMENT = 74  or ID_PAIEMENT = 10) AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') ";

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            VAR_14 = Convert.ToInt64(MtGlb.IIf(DBNull.Value.Equals(MtGlb.oDataRow["Monto_2"]), "0", MtGlb.oDataRow["Monto_2"].ToString()));
                            VAR_15 = Convert.ToInt64(MtGlb.IIf(DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]), "0", MtGlb.oDataRow["Cruces"].ToString()));
                        }

                        //SUBTOTAL MARCADO COMO PAGADO
                        VAR_16 = VAR_11 + VAR_12;

                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND (ID_PAIEMENT = 1 " +
                                                "OR ID_PAIEMENT = 2 " +
                                                "OR ID_PAIEMENT = 71 " +
                                                "OR ID_PAIEMENT = 72 " +
                                                "OR ID_PAIEMENT = 73 " +
                                                "OR ID_PAIEMENT = 74  or ID_PAIEMENT = 10) " +
                                                "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') ";

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            VAR_17 = Convert.ToInt64(MtGlb.IIf(DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]), Convert.ToString(0 + VAR_13), Convert.ToString(Convert.ToInt64(MtGlb.oDataRow["Cruces"]) + VAR_13)));
                        }

                        //tramo corto 17

                        if (IdPlazaCobro.Substring(1, 2) == "84" || IdPlazaCobro.Substring(1, 2) == "02")
                        {
                            StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                        "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                        "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                        "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                            StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                    "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                    "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                    "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                    "AND VOIE = '" + Str_id_voie + "' ";

                            StrQuerys = StrQuerys + "AND " +
                                                    "ID_PAIEMENT = 2 " +
                                                    "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') ";

                            if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                                VAR_17_tc = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }
                        //fin tramo corto 17

                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND (ID_PAIEMENT = 1 " +
                                                "OR ID_PAIEMENT = 2 " +
                                                "OR ID_PAIEMENT = 71 " +
                                                "OR ID_PAIEMENT = 72 " +
                                                "OR ID_PAIEMENT = 73 " +
                                                "OR ID_PAIEMENT = 74  or ID_PAIEMENT = 10) " +
                                                "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') ";

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            VAR_18 = Convert.ToInt64(MtGlb.IIf(DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]), "0", MtGlb.oDataRow["Monto_1"].ToString()));
                            VAR_19 = Convert.ToInt64(MtGlb.IIf(DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]), "0", MtGlb.oDataRow["Cruces"].ToString()));
                        }

                        //trmao cotro var 19
                        if (IdPlazaCobro.Substring(1, 2) == "84" || IdPlazaCobro.Substring(1, 2) == "02")
                        {
                            StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                        "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                        "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                        "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                        "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                            StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                    "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                    "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                    "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                    "AND VOIE = '" + Str_id_voie + "' ";

                            StrQuerys = StrQuerys + "AND  " +
                                                    " ID_PAIEMENT = 2 " +
                                                    "AND (ID_OBS_SEQUENCE = '7' or ID_OBS_SEQUENCE = 'F') ";

                            if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                                VAR_19_tc = Convert.ToInt64(MtGlb.IIf(DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]), "0", MtGlb.oDataRow["Cruces"].ToString()));
                        }

                        //fin tramo corto var 19

                        VAR_20 = VAR_16 - VAR_18;
                        VAR_21 = VAR_17 - VAR_19;

                        //SISTEMAS ELECTRONICOS
                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND ID_PAIEMENT = 15 ";

                        VAR_28 = 0;
                        VAR_29 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_28 = 0;
                            else
                                VAR_28 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_29 = 0;
                            else
                                VAR_29 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }

                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                        "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                        "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                        "AND ID_VOIE = '" + Int_id_voie + "' " +
                        "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND ID_PAIEMENT = 15 ";

                        VAR_30 = 0;
                        VAR_31 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_30 = 0;
                            else
                                VAR_30 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_31 = 0;
                            else
                                VAR_31 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }

                        //VSC

                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND (ID_PAIEMENT = 27 and (ID_OBS_SEQUENCE = '0' or ID_OBS_SEQUENCE = 'F')) ";

                        VAR_40 = 0;
                        VAR_41 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_40 = 0;
                            else
                                VAR_40 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_41 = 0;
                            else
                                VAR_41 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }

                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND (ID_PAIEMENT = 27 and (ID_OBS_SEQUENCE = '0' or ID_OBS_SEQUENCE = 'F')) ";

                        VAR_42 = 0;
                        VAR_43 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_42 = 0;
                            else
                                VAR_42 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_43 = 0;
                            else
                                VAR_43 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }

                        //VEHICULOS ELUDIDOS

                        /*
                         * CHECAR EN CODIGO ORIGINAL, 2 CONSULTAS SE REALIZAN PERO NO SE UTILIZAN
                         */

                        //RECLASIFICADOS
                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                    "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                    "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                    "AND ID_VOIE = '" + Int_id_voie + "' " +
                                    "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND ID_OBS_SEQUENCE = 'F'";

                        VAR_67 = 0;
                        VAR_68 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_67 = 0;
                            else
                                VAR_67 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_68 = 0;
                            else
                                VAR_68 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }

                        //DETECCIONES ERRONEAS
                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND ID_OBS_PASSAGE = '6' ";

                        VAR_69 = 0;
                        VAR_70 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_69 = 0;
                            else
                                VAR_69 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_70 = 0;
                            else
                                VAR_70 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }

                        //tarjera de credito
                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND ID_PAIEMENT = 12 ";

                        VAR_32 = 0;
                        VAR_33 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_32 = 0;
                            else
                                VAR_32 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_33 = 0;
                            else
                                VAR_33 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }

                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND ID_PAIEMENT = 12 ";

                        VAR_34 = 0;
                        VAR_35 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_34 = 0;
                            else
                                VAR_34 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_35 = 0;
                            else
                                VAR_35 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }

                        //tarjeta de debito
                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND ID_PAIEMENT = 14 ";

                        VAR_36 = 0;
                        VAR_37 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_36 = 0;
                            else
                                VAR_36 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_37 = 0;
                            else
                                VAR_37 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }

                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + _Dt_ini_poste.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _Dt_fin_poste.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                                "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                                "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                                "AND ID_VOIE = '" + Int_id_voie + "' " +
                                                "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND ID_PAIEMENT = 14 ";

                        VAR_38 = 0;
                        VAR_39 = 0;

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"]))
                                VAR_38 = 0;
                            else
                                VAR_38 = Convert.ToInt64(MtGlb.oDataRow["Monto_1"]);

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"]))
                                VAR_39 = 0;
                            else
                                VAR_39 = Convert.ToInt64(MtGlb.oDataRow["Cruces"]);
                        }
                        //Validacion para corregir error en los dias
                        if (dia_Restado == true)
                        {
                            VAR01 = Convert.ToDateTime(VAR_01).AddDays(+1).ToString("dd/MM/yyyy");
                            Str_detalle = VAR01 + "," + Int_turno + "," + _Dt_ini_poste.ToString("HHmmss") + "," + _Dt_fin_poste.ToString("HHmmss") + ",";
                            Str_detalle_tc = VAR01 + "," + Int_turno + "," + _Dt_ini_poste.ToString("HHmmss") + "," + _Dt_fin_poste.ToString("HHmmss") + ",";
                        }
                        else
                        {

                            Str_detalle = Convert.ToDateTime(VAR_01).ToString("dd/MM/yyyy") + "," + Int_turno + "," + _Dt_ini_poste.ToString("HHmmss") + "," + _Dt_fin_poste.ToString("HHmmss") + ",";
                            Str_detalle_tc = Convert.ToDateTime(VAR_01).ToString("dd/MM/yyyy") + "," + Int_turno + "," + _Dt_ini_poste.ToString("HHmmss") + "," + _Dt_fin_poste.ToString("HHmmss") + ",";
                        }
                        /*************************************************************************/
                        dataRows = from MyRow in dt.AsEnumerable()
                                       //where MyRow.Field<string>("idCarril") == VAR_08.Substring(1, 2)
                                   where MyRow.Field<string>("Num_Gea") == VAR_08.Substring(1, 2)
                                   select MyRow;

                        foreach (DataRow value in dataRows)
                        {
                            //NumCarril = value["numCarril"].ToString();
                            //NumTramo = value["numTramo"].ToString();
                            //NumPlaza = value["idPlaza"].ToString();
                            NumCarril = value["Num_Capufe"].ToString();
                            NumTramo = value["Num_Tramo"].ToString();
                            NumPlaza = value.Field<Type_Plaza>("Type_plaza").Num_Plaza.ToString();
                        }
                        /*************************************************************************/

                        if (dataRows.Count() != 0)
                        {
                            Str_detalle = Str_detalle + NumTramo + ",";
                            Str_detalle = Str_detalle + NumCarril + ",";

                        }
                        else
                        {
                            Str_detalle = Str_detalle + ",,";
                        }

                        //cuerpo
                        Str_detalle = Str_detalle + VAR_08.Substring(0, 1) + ",";
                        Str_detalle_tc = Str_detalle_tc + VAR_08.Substring(0, 1) + ",";

                        Str_detalle = Str_detalle + VAR_02 + ",";
                        Str_detalle_tc = Str_detalle_tc + VAR_02 + ",";

                        //97
                        Str_detalle = Str_detalle + VAR_97 + ",";
                        Str_detalle_tc = Str_detalle_tc + "0,";
                        //98
                        Str_detalle = Str_detalle + VAR_98 + ",";
                        Str_detalle_tc = Str_detalle_tc + "0,";
                        //100 (no se maneja)
                        Str_detalle = Str_detalle + ",";
                        Str_detalle_tc = Str_detalle_tc + "0,";
                        //92
                        Str_detalle = Str_detalle + VAR_92 + ",";
                        Str_detalle_tc = Str_detalle_tc + "0,";
                        //93
                        Str_detalle = Str_detalle + VAR_93 + ",";
                        Str_detalle_tc = Str_detalle_tc + "0,";
                        //95
                        Str_detalle = Str_detalle + ",";
                        Str_detalle_tc = Str_detalle_tc + ",";
                        //11
                        Str_detalle = Str_detalle + VAR_11 + ",";
                        Str_detalle_tc = Str_detalle_tc + "0,";
                        //18
                        Str_detalle = Str_detalle + VAR_18 + ",";
                        Str_detalle_tc = Str_detalle_tc + "0,";
                        //16
                        Str_detalle = Str_detalle + VAR_16 + ",";
                        Str_detalle_tc = Str_detalle_tc + "0,";
                        //17
                        Str_detalle = Str_detalle + VAR_17 + ",";
                        //19
                        Str_detalle = Str_detalle + VAR_19 + ",";
                        //58
                        Str_detalle = Str_detalle + VAR_58 + ",";
                        //+
                        Str_detalle = Str_detalle + VAR_11 + ",";
                        //71
                        Str_detalle = Str_detalle + VAR_71 + ",";
                        //73
                        Str_detalle = Str_detalle + VAR_73 + ",";
                        //13
                        Str_detalle = Str_detalle + VAR_13 + ",";
                        //12
                        Str_detalle = Str_detalle + VAR_12 + ","; // HEY
                                                                  //VEHICULOS RESIDENTES S.P.
                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE ";

                        StrQuerys = StrQuerys + "AND (DATE_TRANSACTION >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_TRANSACTION <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + "AND (MATRICULE = '" + Str_MATRICULE + "') " +
                                    "AND TRANSACTION.ID_RESEAU = '" + Str_id_RESEAU + "' " +
                                    "AND TRANSACTION.ID_GARE = " + Int_id_gare + " " +
                                    "AND ID_VOIE = '" + Int_id_voie + "' " +
                                    "AND VOIE = '" + Str_id_voie + "' ";

                        StrQuerys = StrQuerys + "AND (ID_PAIEMENT = 75 and ID_OBS_SEQUENCE = '0') ";

                        if (MtGlb.QueryDataSet(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (DBNull.Value.Equals(MtGlb.oDataRow["Monto_1"])) ;

                            else
                            {
                                VAR_23 = Convert.ToDouble(MtGlb.oDataRow["Monto_1"]);
                            }

                            if (DBNull.Value.Equals(MtGlb.oDataRow["Cruces"])) ;

                            else
                            {
                                VAR_22 = Convert.ToDouble(MtGlb.oDataRow["Cruces"]);
                            }
                        }


                        //23
                        Str_detalle = Str_detalle + VAR_23 + ",";
                        //22
                        Str_detalle = Str_detalle + VAR_22 + ",";
                        //25
                        Str_detalle = Str_detalle + VAR_23 + ",";
                        //24
                        Str_detalle = Str_detalle + VAR_22 + ",";

                        //---------------------------------------------------------------
                        //29
                        Str_detalle = Str_detalle + VAR_29 + ",";
                        //28
                        Str_detalle = Str_detalle + VAR_28 + ",";
                        //31
                        Str_detalle = Str_detalle + VAR_31 + ",";
                        //30
                        Str_detalle = Str_detalle + VAR_30 + ",";
                        //41
                        Str_detalle = Str_detalle + VAR_41 + ",";
                        //40
                        Str_detalle = Str_detalle + VAR_40 + ",";
                        //43
                        Str_detalle = Str_detalle + VAR_43 + ",";
                        //42
                        Str_detalle = Str_detalle + VAR_42 + ",";
                        //45
                        Str_detalle = Str_detalle + VAR_45 + ",";
                        //44
                        Str_detalle = Str_detalle + VAR_44 + ",";
                        //47
                        Str_detalle = Str_detalle + VAR_47 + ",";
                        //46
                        Str_detalle = Str_detalle + VAR_46 + ",";
                        //49
                        Str_detalle = Str_detalle + VAR_49 + ",";
                        //48
                        Str_detalle = Str_detalle + VAR_48 + ",";
                        //51
                        Str_detalle = Str_detalle + VAR_51 + ",";
                        //50
                        Str_detalle = Str_detalle + VAR_50 + ",";
                        //68
                        Str_detalle = Str_detalle + VAR_68 + ",";
                        //67
                        Str_detalle = Str_detalle + VAR_67 + ",";
                        //70
                        Str_detalle = Str_detalle + VAR_70 + ",";
                        //69
                        Str_detalle = Str_detalle + VAR_69 + ",";
                        //102
                        //str_detalle = str_detalle & VAR_94 & ","
                        //Str_detalle = Str_detalle + Convert.ToDecimal(VAR_11 + VAR_71).ToString("########0.00") + ",";
                        string suma = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", (VAR_11 + VAR_71));
                        Str_detalle = Str_detalle + suma + ",";
                        //103
                        Str_detalle = Str_detalle + VAR_103 + ",";
                        //+
                        Str_detalle = Str_detalle + ",";

                        if ((VAR_78 - VAR_77) >= 9999)
                        {
                            double h78 = 0;
                            h78 = VAR_98 - VAR_97;
                            VAR_78 = VAR_77 + h78;
                            //77
                            Str_detalle = Str_detalle + VAR_77 + ",";
                            //78
                            Str_detalle = Str_detalle + VAR_78 + ",";
                        }
                        else
                        {
                            //77
                            Str_detalle = Str_detalle + VAR_77 + ",";
                            //78
                            Str_detalle = Str_detalle + VAR_78 + ",";
                        }
                        //80
                        Str_detalle = Str_detalle + VAR_80 + ",";
                        //82
                        Str_detalle = Str_detalle + VAR_82 + ",";
                        //83
                        Str_detalle = Str_detalle + VAR_83 + ",";
                        //85
                        Str_detalle = Str_detalle + VAR_85 + ",";
                        //87
                        Str_detalle = Str_detalle + VAR_87 + ",";
                        //88
                        Str_detalle = Str_detalle + VAR_88 + ",";
                        //90
                        Str_detalle = Str_detalle + VAR_90 + ",";
                        //60
                        Str_detalle = Str_detalle + VAR_60 + ",";
                        //59
                        Str_detalle = Str_detalle + VAR_59 + ",";
                        //62
                        Str_detalle = Str_detalle + VAR_62 + ",";
                        //15
                        Str_detalle = Str_detalle + VAR_15 + ",";
                        //14
                        Str_detalle = Str_detalle + VAR_14 + ",";
                        //15
                        Str_detalle = Str_detalle + VAR_15 + ",";
                        //14
                        Str_detalle = Str_detalle + VAR_14 + ",";
                        //33
                        Str_detalle = Str_detalle + VAR_33 + ",";
                        //32
                        Str_detalle = Str_detalle + VAR_32 + ",";
                        //35
                        Str_detalle = Str_detalle + VAR_35 + ",";
                        //34
                        Str_detalle = Str_detalle + VAR_34 + ",";
                        //37
                        Str_detalle = Str_detalle + VAR_37 + ",";
                        //36
                        Str_detalle = Str_detalle + VAR_36 + ",";
                        //39
                        Str_detalle = Str_detalle + VAR_39 + ",";
                        //38
                        Str_detalle = Str_detalle + VAR_38 + ",";

                        Osw.WriteLine(Str_detalle);
                        Osw2.WriteLine(Str_detalle);

                        //tramos cortos
                        if (NumPlaza == "84")
                        {
                            Str_detalle_tc = Str_detalle_tc + VAR_17_tc + "," + VAR_19_tc + "," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0,";

                            if (VAR_08.Substring(0, 1) == "A")
                            {
                                Osw.WriteLine(Str_detalle_tc);
                                Osw2.WriteLine(Str_detalle_tc);
                            }
                        }
                        else if (NumPlaza == "02")
                        {
                            Str_detalle_tc = Str_detalle_tc + VAR_17_tc + "," + VAR_19_tc + "," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0," + "0,";

                            if (VAR_08 == "A01" || VAR_08 == "B08")
                            {
                                Osw.WriteLine(Str_detalle_tc);
                                Osw2.WriteLine(Str_detalle_tc);
                            }
                        }

                        if (dia_Restado == true)
                        {
                            Str_detalle = Convert.ToDateTime(VAR_01).ToString("dd/MM/yyyy") + "," + Int_turno + "," + _Dt_ini_poste.ToString("HHmmss") + "," + _Dt_fin_poste.ToString("HHmmss") + ",";
                            Str_detalle_tc = Convert.ToDateTime(VAR_01).ToString("dd/MM/yyyy") + "," + Int_turno + "," + _Dt_ini_poste.ToString("HHmmss") + "," + _Dt_fin_poste.ToString("HHmmss") + ",";
                        }
                        //fin tramos cortos

                        //************************************
                        //************************************
                        //***********************************
                        //fin detalle
                    }
                }

                /************************************************************************************/


                //cerrados inicio

                StrQuerys = "SELECT LANE, BEGIN_DHM, END_DHM FROM CLOSED_LANE_REPORT " +
                            "WHERE BEGIN_DHM IN( " +
                                "SELECT BEGIN_DHM " +
                                "FROM CLOSED_LANE_REPORT, SITE_GARE " +
                                "WHERE " +
                                "CLOSED_LANE_REPORT.ID_PLAZA = SITE_GARE.id_Gare " +
                                "GROUP BY BEGIN_DHM, LANE " +
                                "HAVING " +
                                "COUNT(BEGIN_DHM) > 1 AND COUNT(LANE) > 1)" +
                            "AND((BEGIN_DHM >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                            "AND(BEGIN_DHM <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS'))) " +
                            "ORDER BY LANE, BEGIN_DHM, END_DHM";


                if (MtGlb.QueryDataSet(StrQuerys, "CLOSED_LANE_REPORT", ConexionDim))
                {
                    foreach (DataRow item in MtGlb.Ds.Tables["CLOSED_LANE_REPORT"].Rows)
                    {
                        ErroresHorario.Add(new EventoCarril
                        {
                            Carril = Convert.ToString(item["LANE"]),
                            Hora_Inicio = Convert.ToDateTime(item["BEGIN_DHM"]),
                            Hora_Fin = Convert.ToDateTime(item["END_DHM"])
                        });
                    }
                }

                ErroresCorregidos = BarridoCarriles(ErroresHorario, _H_inicio_turno, _H_fin_turno, ConexionDim);


                StrQuerys = "SELECT ID_NETWORK, ID_PLAZA,ID_LANE, LANE, BEGIN_DHM, END_DHM, BAG_NUMBER, REPORT_FLAG, GENERATION_DHM " +
                            "FROM CLOSED_LANE_REPORT, SITE_GARE " +
                            "where " +
                            "CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " +
                            "AND	SITE_GARE.id_Site		=	'" + IdPlazaCobro.Substring(1, 2) + "' " +
                            "AND ((BEGIN_DHM >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                            "AND (BEGIN_DHM <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                            "order by BEGIN_DHM";

                if (MtGlb.QueryDataSet(StrQuerys, "CLOSED_LANE_REPORT", ConexionDim))
                {
                    foreach (DataRow item in MtGlb.Ds.Tables["CLOSED_LANE_REPORT"].Rows)
                    {
                        if (ErroresHorario.Count != 0)
                        {
                            DateTime Hora_Inicio_Evento;

                            Hora_Inicio_Evento = Convert.ToDateTime(item["BEGIN_DHM"]);

                            foreach (var Error in ErroresHorario)
                            {
                                if (Error.Hora_Inicio == Hora_Inicio_Evento && Error.Carril == item["LANE"].ToString())
                                {
                                    foreach (var Correcion in ErroresCorregidos)
                                    {
                                        if (Correcion.Hora_Fin == Convert.ToDateTime(item["END_DHM"]))
                                        {
                                            item["BEGIN_DHM"] = Correcion.Hora_Inicio;
                                        }
                                    }
                                }
                            }
                        }

                        Str_detalle = string.Empty;

                        //Fecha base de operación 	Fecha 	dd/mm/aaaa
                        //str_detalle = Format(oDataRow("BEGIN_DHM"), "dd/MM/yyyy") & ","
                        //Format(dt_Fecha_Inicio, "MM/dd/yyyy")
                        Str_detalle = FechaInicio.ToString("dd/MM/yyyy") + ",";


                        //Número de turno	Entero 	9	Valores posibles: Tabla 12 - Ejemplo del Catálogo de Turnos por Plaza de Cobro.
                        Str_detalle = Str_detalle + Int_turno + ",";
                        //Hora inicial de operación 	Caracter 	hhmmss 	
                        Str_detalle = Str_detalle + Convert.ToDateTime(item["BEGIN_DHM"]).ToString("HHmmss") + ",";
                        //Hora final de operación 	Caracter 	hhmmss 	

                        if (Convert.ToDateTime(item["END_DHM"]) > _H_fin_turno)
                            Str_detalle = Str_detalle + _H_fin_turno.ToString("HHmmss") + ",";
                        else
                            Str_detalle = Str_detalle + Convert.ToDateTime(item["END_DHM"]).ToString("HHmmss") + ",";


                        /************************************************/
                        dataRows = from MyRow in dt.AsEnumerable()
                                       //where MyRow.Field<string>("idCarril") == item["LANE"].ToString().Substring(1, 2)
                                   where MyRow.Field<string>("Num_Gea") == item["LANE"].ToString().Substring(1, 2)
                                   select MyRow;

                        foreach (DataRow value in dataRows)
                        {
                            //NumCarril = value["numCarril"].ToString();
                            //NumTramo = value["numTramo"].ToString();
                            //NumPlaza = value["idPlaza"].ToString();
                            NumCarril = value["Num_Capufe"].ToString();
                            NumTramo = value["Num_Tramo"].ToString();
                            NumPlaza = value.Field<Type_Plaza>("Type_plaza").Num_Plaza.ToString();
                        }
                        /************************************************/

                        if (dataRows.Count() != 0)
                        {
                            Str_detalle = Str_detalle + NumTramo + ",";
                            Str_detalle = Str_detalle + NumCarril + ",";

                        }
                        else
                        {
                            Str_detalle = Str_detalle + ",,";
                        }

                        //Cuerpo 	Caracter 	X(1)	Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                        Str_detalle = Str_detalle + item["LANE"].ToString().Substring(0, 1) + ",";
                        //mmss
                        Str_detalle = Str_detalle + Convert.ToDateTime(item["END_DHM"]).ToString("mm") + Convert.ToDateTime(item["END_DHM"]).ToString("ss") + ",";

                        StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces, MIN(EVENT_NUMBER) AS minimo, MAX(EVENT_NUMBER) as maximo " +
                                    "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                    "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                    "AND (DATE_TRANSACTION >= TO_DATE('" + Convert.ToDateTime(item["BEGIN_DHM"]).ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) AND ID_OBS_PASSAGE <> '1' ";

                        if (Convert.ToDateTime(item["END_DHM"]) > _H_fin_turno)
                            StrQuerys = StrQuerys + "AND (DATE_TRANSACTION <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";
                        else
                            StrQuerys = StrQuerys + "AND (DATE_TRANSACTION <= TO_DATE('" + Convert.ToDateTime(item["END_DHM"]).ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                        StrQuerys = StrQuerys + " " + "AND VOIE = '" + item["LANE"] + "'  and EVENT_NUMBER <> 0  ";

                        if (MtGlb.QueryDataSet3(StrQuerys, "TRANSACTION", ConexionDim))
                        {
                            if (Convert.ToInt32(MtGlb.oDataRow3["Cruces"]) > 0)
                                Str_detalle = Str_detalle + MtGlb.oDataRow3["minimo"] + ",";
                            else
                                Str_detalle = Str_detalle + "0,";
                            if (Convert.ToInt32(MtGlb.oDataRow3["Cruces"]) > 0)
                                Str_detalle = Str_detalle + MtGlb.oDataRow3["maximo"] + ",";
                            else
                                Str_detalle = Str_detalle + "0,";
                        }


                        Str_detalle = Str_detalle + ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";


                        Osw.WriteLine(Str_detalle);
                        Osw2.WriteLine(Str_detalle);

                        //cerrados fin

                        //CARRILES CERRADOS DOS
                        //SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD;

                        //************************************************
                        //************************************************
                    }
                }

                /************************************************************************************/

                //CARRILES CERRADO DOS
                StrQuerys = "SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD ";

                if (IdPlazaCobro == "106")
                    StrQuerys = StrQuerys + "where VOIE <> 'B04' and VOIE <> 'A03' ";

                if (MtGlb.QueryDataSet4(StrQuerys, "SEQ_VOIE_TOD", ConexionDim))
                {
                    foreach (DataRow item4 in MtGlb.Ds4.Tables["SEQ_VOIE_TOD"].Rows)
                    {
                        StrQuerys = "SELECT	* FROM 	FIN_POSTE " +
                                    "WHERE VOIE = '" + item4["VOIE"] + "' " +
                                    "AND ((DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                    "AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) ";

                        if (MtGlb.QueryDataSet(StrQuerys, "FIN_POSTE", ConexionDim) == false)
                        {
                            StrQuerys = "SELECT * " +
                                        "FROM CLOSED_LANE_REPORT, SITE_GARE " +
                                        "where " +
                                        "CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " +
                                        "AND	SITE_GARE.id_Site		=	'" + IdPlazaCobro.Substring(1, 2) + "' " +
                                        "AND	LANE		=	'" + item4["VOIE"] + "' " +
                                        "AND ((BEGIN_DHM >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                        "AND (BEGIN_DHM <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                                        "order by BEGIN_DHM";

                            if (MtGlb.QueryDataSet(StrQuerys, "CLOSED_LANE_REPORT", ConexionDim) == false)
                            {
                                Str_detalle = string.Empty;
                                //Fecha base de operación 	Fecha 	dd/mm/aaaa
                                Str_detalle = FechaInicio.ToString("dd/MM/yyyy") + ",";
                                //Número de turno	Entero 	9	Valores posibles: Tabla 12 - Ejemplo del Catálogo de Turnos por Plaza de Cobro.
                                Str_detalle = Str_detalle + Int_turno + ",";
                                //Hora inicial de operación 	Caracter 	hhmmss 	
                                Str_detalle = Str_detalle + _H_inicio_turno.ToString("HHmmss") + ",";
                                //Hora final de operación 	Caracter 	hhmmss 	
                                //str_detalle = str_detalle & Format(h_fin_turno, "HHmmss") & ","
                                Str_detalle = Str_detalle + _H_fin_turno.AddSeconds(1).ToString("HHmmss") + ",";
                                //Número de carril Entero  >> 9 Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.


                                /*******************************************/

                                dataRows = from MyRows in dt.AsEnumerable()
                                               //where MyRows.Field<string>("idCarril") == item4["Voie"].ToString().Substring(1, 2)
                                           where MyRows.Field<string>("Num_Gea") == item4["Voie"].ToString().Substring(1, 2)
                                           select MyRows;

                                foreach (DataRow value in dataRows)
                                {
                                    //NumCarril = value["numCarril"].ToString();
                                    //NumTramo = value["numTramo"].ToString();
                                    //NumPlaza = value["idPlaza"].ToString();
                                    NumCarril = value["Num_Capufe"].ToString();
                                    NumTramo = value["Num_Tramo"].ToString();
                                    NumPlaza = value.Field<Type_Plaza>("Type_plaza").Num_Plaza.ToString();
                                }
                                /*******************************************/
                                if (dataRows.Count() != 0)
                                {
                                    Str_detalle = Str_detalle + NumTramo + ",";
                                    Str_detalle = Str_detalle + NumCarril + ",";

                                }
                                else
                                {
                                    Str_detalle = Str_detalle + ",,";
                                }
                                //Cuerpo Caracter    X(1)    Valores posibles: Tabla 13 - Ejemplo del Catálogo de Carriles y Tramos por Plaza de Cobro.
                                Str_detalle = Str_detalle + item4["VOIE"].ToString().Substring(0, 1) + ",";

                                //*****************************************
                                Str_detalle = Str_detalle + _H_fin_turno.ToString("mm") + _H_fin_turno.ToString("ss") + ",";

                                StrQuerys = "SELECT SUM(PRIX_TOTAL) AS Monto_1,  COUNT(*) AS Cruces, MIN(EVENT_NUMBER) AS minimo, MAX(EVENT_NUMBER) as maximo " +
                                            "FROM GEADBA.TRANSACTION,SITE_GARE " +
                                            "WHERE TRANSACTION.ID_GARE = SITE_GARE.ID_GARE " +
                                            "AND (DATE_TRANSACTION >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                            "AND (DATE_TRANSACTION <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                            "AND (DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                                            "AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) ";

                                StrQuerys = StrQuerys + " " + "AND VOIE = '" + item4["VOIE"] + "' and EVENT_NUMBER <> 0 AND ID_OBS_PASSAGE <> '1' ";

                                if (MtGlb.QueryDataSet3(StrQuerys, "TRANSACTION", ConexionDim))
                                {
                                    if (Convert.ToInt32(MtGlb.oDataRow3["Cruces"]) > 0)
                                        Str_detalle = Str_detalle + MtGlb.oDataRow3["minimo"] + ",";
                                    else
                                        Str_detalle = Str_detalle + "0,";
                                    if (Convert.ToInt32(MtGlb.oDataRow3["Cruces"]) > 0)
                                        Str_detalle = Str_detalle + MtGlb.oDataRow3["maximo"] + ",";
                                    else
                                        Str_detalle = Str_detalle + "0,";
                                }

                                Str_detalle = Str_detalle + ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

                                //*****************************************

                                Osw.WriteLine(Str_detalle);
                                Osw2.WriteLine(Str_detalle);
                            }
                        }
                    }
                }

                //************************************************
                //************************************************

                //Cerramos la conexion
                MtGlb.ConnectionClose(ConexionDim);

                Osw.Flush();
                Osw.Close();
                Osw2.Flush();
                Osw2.Close();



                Message = "Todo bien";
            }
            catch (Exception ex)
            {
                Message = ex.Message + " " + ex.StackTrace;
                Message = Message.Replace(System.Environment.NewLine, "  ");
            }

        }
        public List<EventoCarril> BarridoCarriles(List<EventoCarril> Errores, DateTime _H_inicio_turno, DateTime _H_fin_turno, OracleConnection ConexionDim)
        {
            List<EventoCarril> ErroresCorregidos = new List<EventoCarril>();

            string StrQuerys = "SELECT COUNT(BEGIN_DHM), BEGIN_DHM, LANE " +
                               "FROM CLOSED_LANE_REPORT, SITE_GARE " +
                               "where " +
                               "CLOSED_LANE_REPORT.ID_PLAZA = SITE_GARE.id_Gare " +
                               "AND((BEGIN_DHM >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS')) " +
                               "AND(BEGIN_DHM <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "', 'YYYYMMDDHH24MISS'))) " +
                               "GROUP by BEGIN_DHM, LANE " +
                               "HAVING COUNT(BEGIN_DHM) > 1 AND COUNT(LANE) > 1 ";

            MtGlb.QueryDataSet4(StrQuerys, "CLOSED_LANE_REPORT", ConexionDim);

            foreach (DataRow CasosRepetidos in MtGlb.Ds4.Tables["CLOSED_LANE_REPORT"].Rows)
            {
                StrQuerys = "SELECT	FIN_POSTE.Id_Gare, " +
                              "Voie, " +
                              "TO_CHAR(Date_Fin_Poste,'MM/DD/YY'), " +
                              "TO_CHAR(Date_Fin_Poste,'HH24:MI'), " +
                              "Matricule, " +
                              "FIN_POSTE.Id_Voie, " +
                              "DATE_DEBUT_POSTE,Date_Fin_Poste, " +
                              "TO_CHAR(Date_Debut_Poste,'YYYYMMDDHH24MISS'), " +
                              "TO_CHAR(Date_Fin_Poste,'YYYYMMDDHH24MISS') " +
                              "FROM 	TYPE_VOIE, " +
                              "FIN_POSTE, " +
                              "SITE_GARE " +
                              "WHERE	FIN_POSTE.Id_Voie	=	TYPE_VOIE.Id_Voie " +
                              "AND Voie	= '" + CasosRepetidos["LANE"] + "' " +
                              "AND FIN_POSTE.id_reseau	= 	SITE_GARE.id_Reseau " +
                              "AND	FIN_POSTE.id_Gare	=	SITE_GARE.id_Gare " +
                              "AND	SITE_GARE.id_reseau		= 	'01' " +
                              "AND (Id_Mode_Voie IN (1,7,9)) " +
                              "AND ((DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                              "AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                              "AND (FIN_POSTE.Id_Voie = '1' " +
                              "OR FIN_POSTE.Id_Voie = '2' " +
                              "OR FIN_POSTE.Id_Voie = '3' " +
                              "OR FIN_POSTE.Id_Voie = '4' " +
                              "OR FIN_POSTE.Id_Voie = 'X' " +
                              ") " +
                              "ORDER BY Id_Gare, " +
                              "Id_Voie, " +
                              "Voie, " +
                              "Date_Debut_Poste," +
                              "Date_Fin_Poste, " +
                              "Numero_Poste, " +
                              "Matricule " +
                              ",Sac";

                MtGlb.QueryDataSet(StrQuerys, "FIN_POSTE", ConexionDim);

                List<EventoCarril> EventosDeCarril = new List<EventoCarril>();

                //Carriles Abiertos del Carril
                foreach (DataRow CarrilesAbiertos in MtGlb.Ds.Tables["FIN_POSTE"].Rows)
                {
                    EventosDeCarril.Add(new EventoCarril
                    {
                        Hora_Inicio = Convert.ToDateTime(CarrilesAbiertos["DATE_DEBUT_POSTE"]),
                        Hora_Fin = Convert.ToDateTime(CarrilesAbiertos["Date_Fin_Poste"]),
                        Carril = CarrilesAbiertos["Voie"].ToString().Substring(1, 2)
                    });
                }

                StrQuerys = "SELECT ID_NETWORK, ID_PLAZA,ID_LANE, LANE, BEGIN_DHM, END_DHM " +
                            "FROM CLOSED_LANE_REPORT, SITE_GARE " +
                            "where " +
                            "CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " +
                            "AND ((BEGIN_DHM >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                            "AND (BEGIN_DHM <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                            "AND LANE = '" + CasosRepetidos["LANE"] + "'" +
                            "order by BEGIN_DHM";


                MtGlb.QueryDataSet(StrQuerys, "CLOSED_LANE_REPORT", ConexionDim);

                foreach (DataRow CarrilesCerrados in MtGlb.Ds.Tables["CLOSED_LANE_REPORT"].Rows)
                {
                    EventosDeCarril.Add(new EventoCarril
                    {
                        Hora_Inicio = Convert.ToDateTime(CarrilesCerrados["BEGIN_DHM"]),
                        Hora_Fin = Convert.ToDateTime(CarrilesCerrados["END_DHM"]) > _H_fin_turno ? _H_fin_turno : Convert.ToDateTime(CarrilesCerrados["END_DHM"]),
                        Carril = CarrilesCerrados["LANE"].ToString().Substring(1, 2)
                    });
                }

                foreach (var Error in Errores)
                {
                    ErroresCorregidos.Add(BarridoEventos(EventosDeCarril, Error, _H_fin_turno, Error.Hora_Fin));
                    if (ErroresCorregidos.LastOrDefault().Carril == "N/A")
                    {
                        ErroresCorregidos.Remove(ErroresCorregidos.LastOrDefault());
                    }
                }
            }
            return ErroresCorregidos;
        }
        public EventoCarril BarridoEventos(List<EventoCarril> EventosDeCarril, EventoCarril Error, DateTime _H_fin_turno, DateTime Hora_Fin)
        {
            DateTime NuevaHoraInicio = DateTime.MinValue;
            foreach (var item in EventosDeCarril)
            {
                if (Convert.ToDateTime(Hora_Fin) >= _H_fin_turno)
                {
                    Error.Carril = "N/A";
                    return Error;
                }
                else if (item.Hora_Inicio == Hora_Fin)
                {
                    return BarridoEventos(EventosDeCarril, Error, _H_fin_turno, item.Hora_Fin);
                }
            }

            var ListadeErrores = EventosDeCarril.Where(x => x.Hora_Inicio == Error.Hora_Inicio).ToList();

            int diferencia = 0;

            foreach (var item in ListadeErrores)
            {
                if (item.Hora_Fin > Hora_Fin && item.Hora_Fin <= _H_fin_turno)
                {
                    if (diferencia == 0)
                    {
                        diferencia = Convert.ToInt32(item.Hora_Fin.ToString("HHmmss")) - Convert.ToInt32(Hora_Fin.ToString("HHmmss"));
                        item.Hora_Inicio = Hora_Fin;
                    }
                    else if (diferencia > Convert.ToInt32(item.Hora_Fin.ToString("HHmmss")) - Convert.ToInt32(Hora_Fin.ToString("HHmmss")))
                    {
                        diferencia = Convert.ToInt32(item.Hora_Fin.ToString("HHmmss")) - Convert.ToInt32(Hora_Fin.ToString("HHmmss"));
                        item.Hora_Inicio = Hora_Fin;
                    }
                    Error = item;
                }
            }
            return Error;
        }
    }
}