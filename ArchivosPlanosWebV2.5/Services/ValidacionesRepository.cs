using ArchivosPlanosWebV2._5.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace ArchivosPlanosWebV2._5.Services
{
    public class ValidacionesRepository

    {
        bool BanValidaciones = true;
        bool BanValidaciones2 = false;
        bool BanValidaciones3 = false;
        public List<filas> listass = new List<filas>();
        public List<string> erresCajeroEncargadoAbierto = new List<string>();
        public List<string> erresCajeroEncargadoCerrado = new List<string>();
        public List<string> erresIdentOperacionAbierto = new List<string>();
        public string Message = string.Empty;
        bool Null = false;


        public string ValidarCajeroEncargadoCerrado(DateTime FechaSelect, string Str_Turno_block, string IdPlazaCobro, string Conexion)
        {

            int Int_turno;
            string H_inicio_turno = string.Empty;
            string H_fin_turno = string.Empty;
            if (Str_Turno_block.Substring(0, 2) == "06")
            {
                Int_turno = 5;
                H_inicio_turno = FechaSelect.ToString("MM/dd/yyyy") + " 06:00:00";
                H_fin_turno = FechaSelect.ToString("MM/dd/yyyy") + " 13:59:59";
            }
            else if (Str_Turno_block.Substring(0, 2) == "14")
            {
                Int_turno = 6;
                H_inicio_turno = FechaSelect.ToString("MM/dd/yyyy") + " 14:00:00";
                H_fin_turno = FechaSelect.ToString("MM/dd/yyyy") + " 21:59:59";
            }
            else if (Str_Turno_block.Substring(0, 2) == "22")
            {
                Int_turno = 4;
                H_inicio_turno = FechaSelect.AddDays(-1).ToString("MM/dd/yyyy") + " 22:00:00";
                H_fin_turno = FechaSelect.ToString("MM/dd/yyyy") + " 05:59:59";
            }
            DateTime _H_inicio_turno = DateTime.ParseExact(H_inicio_turno, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime _H_fin_turno = DateTime.ParseExact(H_fin_turno, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);


            string StrQuerys = "SELECT ID_NETWORK, ID_PLAZA,ID_LANE, LANE, BEGIN_DHM, END_DHM, BAG_NUMBER, REPORT_FLAG, GENERATION_DHM " +
                        "FROM CLOSED_LANE_REPORT, SITE_GARE " +
                        "where " +
                        "CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " +
                        "AND	SITE_GARE.id_Site		=	'" + IdPlazaCobro.Substring(1, 2) + "' " +
                        "AND ((BEGIN_DHM >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                        "AND (BEGIN_DHM <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                        "order by BEGIN_DHM";

            OracleConnection ConexionDim = new OracleConnection(Conexion);
            MetodosGlbRepository MtGlb = new MetodosGlbRepository();
            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
            SqlConnection Connection = new SqlConnection(ConnectionString);
            AppDbContextSQL db = new AppDbContextSQL();
            DataSet dataSet = new DataSet();
            EnumerableRowCollection<DataRow> dataRows;
            DataTable dt = new DataTable();
            string StrEncargadoTurno = string.Empty;
            //CARRILES CERRADOS UNO
            if (MtGlb.QueryDataSet(StrQuerys, "CLOSED_LANE_REPORT", ConexionDim))
            {
                foreach (DataRow item in MtGlb.Ds.Tables["CLOSED_LANE_REPORT"].Rows)
                {

                    StrQuerys = "SELECT	LANE_ASSIGN.Id_plaza,LANE_ASSIGN.Id_lane,TO_CHAR(LANE_ASSIGN.MSG_DHM,'MM/DD/YY HH24:MI:SS'),LANE_ASSIGN.SHIFT_NUMBER,LANE_ASSIGN.OPERATION_ID, " +
                                      "LANE_ASSIGN.DELEGATION, TO_CHAR(LANE_ASSIGN.ASSIGN_DHM,'MM/DD/YY'),LTRIM(TO_CHAR(LANE_ASSIGN.JOB_NUMBER,'09')),	LANE_ASSIGN.STAFF_NUMBER,LANE_ASSIGN.IN_CHARGE_SHIFT_NUMBER " +
                                      "FROM 	LANE_ASSIGN, SITE_GARE " +
                                      "WHERE	LANE_ASSIGN.id_NETWORK = SITE_GARE.id_Reseau " +
                                      "AND LANE_ASSIGN.id_plaza = SITE_GARE.id_Gare " +
                                      "AND SITE_GARE.id_reseau = '01' " +
                                      "AND	SITE_GARE.id_Site = '" + IdPlazaCobro.Substring(1, 2) + "' " +
                                      "AND LANE_ASSIGN.Id_lane = '" + item["LANE"] + "' " +
                                      "AND ((MSG_DHM >= TO_DATE('" + Convert.ToDateTime(item["BEGIN_DHM"]).ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) AND (MSG_DHM <= TO_DATE('" + Convert.ToDateTime(item["BEGIN_DHM"]).ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                                      "ORDER BY LANE_ASSIGN.Id_PLAZA, LANE_ASSIGN.Id_LANE, LANE_ASSIGN.MSG_DHM";

                    string Query;
                    string Cajero = string.Empty;
                    string EncargadoTurno = string.Empty;

                    //SI NO ENCUENTRA NADA, SE ASIGNA PENDIENTE A ENCARGADO
                    if (MtGlb.QueryDataSet2(StrQuerys, "Asig_Carril", ConexionDim))
                    {
                        StrEncargadoTurno = MtGlb.oDataRow2["IN_CHARGE_SHIFT_NUMBER"].ToString();

                        //VERIFICAR SI EL ENCARGADO TURNO EXISTEN
                        //Query = @"SELECT numCapufe FROM TYPE_OPERADORES WHERE numGea = @numGea";
                        Query = @"SELECT Num_Capufe FROM TYPE_OPERADORES WHERE Num_Gea = @numGea";

                        using (SqlCommand Cmd = new SqlCommand(Query, Connection))
                        {
                            Cmd.Parameters.Add(new SqlParameter("numGea", StrEncargadoTurno));
                            try
                            {
                                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(Cmd);
                                sqlDataAdapter.Fill(dataSet, "STRENCARGADO_TURNO");
                                if (dataSet.Tables["STRENCARGADO_TURNO"].Rows.Count != 0)
                                {
                                    foreach (DataRow item1 in dataSet.Tables["STRENCARGADO_TURNO"].Rows)
                                    {
                                        EncargadoTurno = item1[0].ToString();
                                    }
                                }                            
                            }
                            catch (Exception ex)
                            {
                                Message = ex.Message + " " + ex.StackTrace;
                            }
                            finally
                            {
                                dataSet.Clear();
                                Cmd.Dispose();
                            }
                        }
                    }
                    //VERIFICAMOS EL CAJERO Y ENCARGADO GUARDAMOS EL EVENTO AL QUE LE FALTAN DATOS
                    var id_pla = IdPlazaCobro.Substring(1, 2);
                    var Carriles_Plazas = db.Type_Plaza.GroupJoin(db.Type_Carril, pla => pla.Id_Plaza, car => car.Plaza_Id, (pla, car) => new { pla, car }).Where(x => x.pla.Num_Plaza == id_pla).ToList();
                    var props = typeof(Type_Carril).GetProperties();
                    dt = new DataTable("Tabla_Carriles");
                    dt.Columns.AddRange(
                        props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray()
                    );
                    Carriles_Plazas.FirstOrDefault().car.ToList().ForEach(
                       i => dt.Rows.Add(props.Select(p => p.GetValue(i, null)).ToArray())
                   );
                    string NumCarril = string.Empty;
                    dataRows = from myRow in dt.AsEnumerable()
                               where myRow.Field<string>("Num_Gea") == Convert.ToString(item["Voie"]).Substring(1, 2)
                               select myRow;


                    foreach (DataRow value in dataRows)
                    {
                        NumCarril = value["Num_Capufe"].ToString();
                    }

                    if (EncargadoTurno == string.Empty)
                    {
                        erresCajeroEncargadoCerrado.Add(NumCarril);
                    }
                }

            }
            //CARRILES CERRADOS DOS
            StrQuerys = "SELECT VOIE, NUM_SEQUENCE FROM SEQ_VOIE_TOD ";

            if (IdPlazaCobro == "106")
                StrQuerys = StrQuerys + "where VOIE <> 'B04' and VOIE <> 'A03' ";

            if (MtGlb.QueryDataSet1(StrQuerys, "SEQ_VOIE_TOD", ConexionDim))
            {
                foreach (DataRow item1 in MtGlb.Ds1.Tables["SEQ_VOIE_TOD"].Rows)
                {
                    StrQuerys = "SELECT	* FROM 	FIN_POSTE " +
                                  "WHERE	VOIE = '" + item1["VOIE"] + "' " +
                                  "AND ((DATE_DEBUT_POSTE >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                  "AND (DATE_DEBUT_POSTE <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) ";


                    string Query;                    
                    string EncargadoTurno = string.Empty;

                    if (MtGlb.QueryDataSet(StrQuerys, "FIN_POSTE", ConexionDim) == false)
                    {
                        StrQuerys = "SELECT * " +
                                    "FROM CLOSED_LANE_REPORT, SITE_GARE " +
                                    "where " +
                                    "CLOSED_LANE_REPORT.ID_PLAZA	=	SITE_GARE.id_Gare " +
                                    "AND	SITE_GARE.id_Site		=	'" + IdPlazaCobro.Substring(1, 2) + "' " +
                                    "AND	LANE		=	'" + item1["VOIE"] + "' " +
                                    "AND ((BEGIN_DHM >= TO_DATE('" + _H_inicio_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) " +
                                    "AND (BEGIN_DHM <= TO_DATE('" + _H_fin_turno.ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                                    "order by BEGIN_DHM";

                        if (MtGlb.QueryDataSet(StrQuerys, "CLOSED_LANE_REPORT", ConexionDim) == false)
                        {

                            if (StrEncargadoTurno.Trim() == "")
                                StrEncargadoTurno = "encargado_plaza";

                            //VERIFICAR EL ENCARGADO EL TURNO; SI NO ESTA, SERÁ EL ENCARGADO DE PLAZA                             
                            Query = @"SELECT Num_Capufe FROM TYPE_OPERADORES WHERE Num_Gea = @numGEa";

                            using (SqlCommand Cmd = new SqlCommand(Query, Connection))
                            {
                                Cmd.Parameters.Add(new SqlParameter("numGea", StrEncargadoTurno));
                                try
                                {
                                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(Cmd);
                                    sqlDataAdapter.Fill(dataSet, "STRENCARGADO_TURNO");
                                    if (dataSet.Tables["STRENCARGADO_TURNO"].Rows.Count != 0)
                                    {
                                        foreach (DataRow item in dataSet.Tables["STRENCARGADO_TURNO"].Rows)
                                        {
                                            EncargadoTurno = item[0].ToString();
                                        }
                                    }                                    
                                }
                                catch (Exception ex)
                                {
                                    Message = ex.Message + " " + ex.StackTrace;
                                }
                                finally
                                {
                                    dataSet.Clear();
                                    Cmd.Dispose();

                                }
                            }

                        }
                    }

                    //VERIFICAMOS EL CAJERO Y ENCARGADO GUARDAMOS EL EVENTO AL QUE LE FALTAN DATOS
                    var id_pla = IdPlazaCobro.Substring(1, 2);
                    var Carriles_Plazas = db.Type_Plaza.GroupJoin(db.Type_Carril, pla => pla.Id_Plaza, car => car.Plaza_Id, (pla, car) => new { pla, car }).Where(x => x.pla.Num_Plaza == id_pla).ToList();
                    var props = typeof(Type_Carril).GetProperties();
                    dt = new DataTable("Tabla_Carriles");
                    dt.Columns.AddRange(
                        props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray()
                    );
                    Carriles_Plazas.FirstOrDefault().car.ToList().ForEach(
                       i => dt.Rows.Add(props.Select(p => p.GetValue(i, null)).ToArray())
                   );
                    string NumCarril = string.Empty;
                    dataRows = from myRow in dt.AsEnumerable()
                               where myRow.Field<string>("Num_Gea") == Convert.ToString(item1["Voie"]).Substring(1, 2)
                               select myRow;


                    foreach (DataRow value in dataRows)
                    {
                        NumCarril = value["Num_Capufe"].ToString();
                    }

                    if (EncargadoTurno == string.Empty)
                    {
                        erresCajeroEncargadoCerrado.Add(NumCarril);
                    }
                }

            }

            
            if (erresCajeroEncargadoCerrado.Count == 0)
                return "OK";
            else
            {
                string errorFormat = "FALTA CAJERO O ENCARGADO DE TURNO EN LOS CARRILES <br/>";
                foreach(string carril in erresCajeroEncargadoCerrado)
                {
                    errorFormat = errorFormat + carril;
                }
                return "STOP";
            }
                
            
        }
        public string ValidarCajeroEncargadoAbierto(DateTime FechaSelect, string Str_Turno_block, string IdPlazaCobro, string Conexion)
        {
            int Int_turno;
            string H_inicio_turno = string.Empty;
            string H_fin_turno = string.Empty;
            if (Str_Turno_block.Substring(0, 2) == "06")
            {
                Int_turno = 5;
                H_inicio_turno = FechaSelect.ToString("MM/dd/yyyy") + " 06:00:00";
                H_fin_turno = FechaSelect.ToString("MM/dd/yyyy") + " 13:59:59";
            }
            else if (Str_Turno_block.Substring(0, 2) == "14")
            {
                Int_turno = 6;
                H_inicio_turno = FechaSelect.ToString("MM/dd/yyyy") + " 14:00:00";
                H_fin_turno = FechaSelect.ToString("MM/dd/yyyy") + " 21:59:59";
            }
            else if (Str_Turno_block.Substring(0, 2) == "22")
            {
                Int_turno = 4;
                H_inicio_turno = FechaSelect.AddDays(-1).ToString("MM/dd/yyyy") + " 22:00:00";
                H_fin_turno = FechaSelect.ToString("MM/dd/yyyy") + " 05:59:59";
            }
            DateTime _H_inicio_turno = DateTime.ParseExact(H_inicio_turno, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime _H_fin_turno = DateTime.ParseExact(H_fin_turno, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            //CARRILES ABIERTOS 
            string StrQuerys = "SELECT	FIN_POSTE.Id_Gare, " +
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

            OracleConnection ConexionDim = new OracleConnection(Conexion);
            MetodosGlbRepository MtGlb = new MetodosGlbRepository();
            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
            SqlConnection Connection = new SqlConnection(ConnectionString);
            AppDbContextSQL db = new AppDbContextSQL();
            DataSet dataSet = new DataSet();
            EnumerableRowCollection<DataRow> dataRows;
            DataTable dt = new DataTable();

            if (MtGlb.QueryDataSet(StrQuerys, "FIN_POSTE", ConexionDim))
            {
                foreach (DataRow item in MtGlb.Ds.Tables["FIN_POSTE"].Rows)
                {
                    //CHECAR ENCARGADO Y IDENT OPERACION                    
                    StrQuerys = "SELECT	LANE_ASSIGN.Id_plaza,LANE_ASSIGN.Id_lane,TO_CHAR(LANE_ASSIGN.MSG_DHM,'MM/DD/YY HH24:MI:SS'),LANE_ASSIGN.SHIFT_NUMBER,LANE_ASSIGN.OPERATION_ID, " +
                                "LANE_ASSIGN.DELEGATION, TO_CHAR(LANE_ASSIGN.ASSIGN_DHM,'MM/DD/YY'),LTRIM(TO_CHAR(LANE_ASSIGN.JOB_NUMBER,'09')),	LANE_ASSIGN.STAFF_NUMBER,LANE_ASSIGN.IN_CHARGE_SHIFT_NUMBER " +
                                "FROM 	LANE_ASSIGN, SITE_GARE " +
                                "WHERE	LANE_ASSIGN.id_NETWORK = SITE_GARE.id_Reseau " +
                                "AND LANE_ASSIGN.id_plaza = SITE_GARE.id_Gare " +
                                "AND SITE_GARE.id_reseau = '01' " +
                                "AND	SITE_GARE.id_Site ='" + IdPlazaCobro.Substring(1, 2) + "' " +
                                "AND LANE_ASSIGN.Id_lane = '" + item["Voie"].ToString().Trim() + "' " +
                                "AND ((MSG_DHM >= TO_DATE('" + Convert.ToDateTime(item["DATE_DEBUT_POSTE"]).ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS')) AND (MSG_DHM <= TO_DATE('" + Convert.ToDateTime(item["DATE_DEBUT_POSTE"]).ToString("yyyyMMddHHmmss") + "','YYYYMMDDHH24MISS'))) " +
                                "ORDER BY LANE_ASSIGN.Id_PLAZA, LANE_ASSIGN.Id_LANE, LANE_ASSIGN.MSG_DHM";


                    string Query;
                    string Cajero = string.Empty;
                    string EncargadoTurno = string.Empty;
                    string IdentOperacion = string.Empty;

                    if (MtGlb.QueryDataSet2(StrQuerys, "Asig_Carril", ConexionDim))
                    {
                        IdentOperacion = MtGlb.oDataRow2["OPERATION_ID"].ToString();
                        string Str_encargado = MtGlb.oDataRow2["STAFF_NUMBER"].ToString();
                        string StrEncargadoTurno = MtGlb.oDataRow2["IN_CHARGE_SHIFT_NUMBER"].ToString();                        
                        Query = @"SELECT Num_Capufe FROM TYPE_OPERADORES WHERE Num_Gea = @numGea";

                        using (SqlCommand Cmd = new SqlCommand(Query, Connection))
                        {
                            Cmd.Parameters.Add(new SqlParameter("numGea", Str_encargado));
                            try
                            {
                                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(Cmd);
                                sqlDataAdapter.Fill(dataSet, "STR_ENCARGADO");
                                if (dataSet.Tables["STR_ENCARGADO"].Rows.Count != 0)
                                {
                                    foreach (DataRow item1 in dataSet.Tables["STR_ENCARGADO"].Rows)
                                    {
                                        Cajero = item1[0].ToString();
                                    }
                                }                                

                            }
                            catch (Exception ex)
                            {
                                Message = ex.Message + " " + ex.StackTrace;
                            }
                            finally
                            {
                                dataSet.Clear();
                                Cmd.Dispose();
                            }               
                        }
                        //VERFICAR EL ENCARGADO DE TURNO                            
                        Query = @"SELECT Num_Capufe FROM TYPE_OPERADORES WHERE Num_Gea = @numGea";

                        using (SqlCommand Cmd = new SqlCommand(Query, Connection))
                        {
                            Cmd.Parameters.Add(new SqlParameter("numGea", StrEncargadoTurno));
                            try
                            {
                                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(Cmd);
                                sqlDataAdapter.Fill(dataSet, "STRENCARGADO_TURNO");
                                if (dataSet.Tables["STRENCARGADO_TURNO"].Rows.Count != 0)
                                {
                                    foreach (DataRow item1 in dataSet.Tables["STRENCARGADO_TURNO"].Rows)
                                    {
                                        EncargadoTurno = item1[0].ToString();
                                    }
                                }                                    
                            }
                            catch (Exception ex)
                            {
                                Message = ex.Message + " " + ex.StackTrace;
                            }
                            finally
                            {
                                dataSet.Clear();
                                Cmd.Dispose();
                            }
                        }

                    }
                    else
                    {
                        Query = @"SELECT Num_Capufe FROM TYPE_OPERADORES WHERE Num_Gea = @numGEa";

                        using (SqlCommand Cmd = new SqlCommand(Query, Connection))
                        {
                            Cmd.Parameters.Add(new SqlParameter("numGEa", item["Matricule"].ToString()));
                            try
                            {
                                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(Cmd);
                                sqlDataAdapter.Fill(dataSet, "MATRICULE");
                                if (dataSet.Tables["MATRICULE"].Rows.Count != 0)
                                {
                                    foreach (DataRow item1 in dataSet.Tables["MATRICULE"].Rows)
                                    {
                                        Cajero = item1[0].ToString();
                                    }
                                }                                                                
                            }
                            catch (Exception ex)
                            {
                                Message = ex.Message + " " + ex.StackTrace;
                            }
                            finally
                            {
                                dataSet.Clear();
                                Cmd.Dispose();
                            }
                        }
                    }
                    //VERIFICAMOS EL CAJERO Y ENCARGADO GUARDAMOS EL EVENTO AL QUE LE FALTAN DATOS
                    var id_pla = IdPlazaCobro.Substring(1, 2);                    
                    var Carriles_Plazas = db.Type_Plaza.GroupJoin(db.Type_Carril, pla => pla.Id_Plaza, car => car.Plaza_Id, (pla, car) => new { pla, car }).Where(x => x.pla.Num_Plaza == id_pla).ToList();
                    var props = typeof(Type_Carril).GetProperties();
                    dt = new DataTable("Tabla_Carriles");
                    dt.Columns.AddRange(
                        props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray()
                    );
                    Carriles_Plazas.FirstOrDefault().car.ToList().ForEach(
                       i => dt.Rows.Add(props.Select(p => p.GetValue(i, null)).ToArray())
                   );
                    string NumCarril = string.Empty;
                    dataRows = from myRow in dt.AsEnumerable()
                               where myRow.Field<string>("Num_Gea") == Convert.ToString(item["Voie"]).Substring(1, 2)
                               select myRow;

                    foreach (DataRow value in dataRows)
                    {                     
                        NumCarril = value["Num_Capufe"].ToString();                                          
                    }
                    if (Cajero == string.Empty &&  EncargadoTurno == string.Empty)
                    {
                        erresCajeroEncargadoAbierto.Add(NumCarril);
                    }
                    else if(Cajero == string.Empty)
                    {
                        erresCajeroEncargadoAbierto.Add(NumCarril);
                    }
                    else if(EncargadoTurno == string.Empty)
                    {
                        erresCajeroEncargadoAbierto.Add(NumCarril);
                    }
                    else if(IdentOperacion == string.Empty)
                    {
                        erresIdentOperacionAbierto.Add(NumCarril);
                    }
                }
                if (erresCajeroEncargadoAbierto.Count == 0 && erresIdentOperacionAbierto.Count == 0)
                    return "OK";
                else
                    return "STOP";                  
            }
            return "OK";
        }

        /// <summary>
        /// Validar carriles cerrados
        /// </summary>
        /// <param name="FechaInicioD"></param>
        /// <param name="FechaSelect"></param>
        /// <param name="TempTurno"></param>
        /// <returns></returns>
        public string ValidarCarrilesCerrados(DateTime FechaSelect, string TempTurno, string Conexion)
        {
            Carril Carril = new Carril();
            OracleCommand Cmd = new OracleCommand();
            OracleConnection Connection = new OracleConnection(Conexion);
            List<Carril> Carriles = new List<Carril>();
            List<Carril> CarrilesCerrados = new List<Carril>();
            //Connection.ConnectionString = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;

            string rpt = string.Empty;
            string TurnoP = string.Empty;
            string FechaInicio = string.Empty;
            string FechaFinal = string.Empty;

            switch (TempTurno)
            {
                case "22:00 - 06:00":
                    TurnoP = "1";
                    FechaInicio = FechaSelect.AddDays(-1).ToString("MM/dd/yyyy") + " 22:00:00";
                    FechaFinal = FechaSelect.ToString("MM/dd/yyyy") + " 05:59:59";
                    break;
                case "06:00 - 14:00":
                    TurnoP = "2";
                    FechaInicio = FechaSelect.ToString("MM/dd/yyyy") + " 06:00:00";
                    FechaFinal = FechaSelect.ToString("MM/dd/yyyy") + " 13:59:59";
                    break;
                case "14:00 - 22:00":
                    TurnoP = "3";
                    FechaInicio = FechaSelect.ToString("MM/dd/yyyy") + " 14:00:00";
                    FechaFinal = FechaSelect.ToString("MM/dd/yyyy") + " 21:59:59";
                    break;
            }

            string Query = @"SELECT	LANE_ASSIGN.Id_plaza,
 		                    LANE_ASSIGN.Id_lane,
		                    TO_CHAR(LANE_ASSIGN.MSG_DHM,'MM/DD/YY HH24:MI:SS') AS FECHA_INICIO,
 		                    LANE_ASSIGN.SHIFT_NUMBER,
 		                    LANE_ASSIGN.OPERATION_ID,
		                    TO_CHAR(LANE_ASSIGN.ASSIGN_DHM,'MM/DD/YY') AS FECHA,
		                    LTRIM(TO_CHAR(LANE_ASSIGN.JOB_NUMBER,'09')) AS EMPLEADO,
		                    LANE_ASSIGN.STAFF_NUMBER,
		                    LANE_ASSIGN.IN_CHARGE_SHIFT_NUMBER
                            FROM 	LANE_ASSIGN
                            WHERE	 SHIFT_NUMBER = " + TurnoP + "" +
                            "AND LANE_ASSIGN.OPERATION_ID = 'NA'" +
                            "AND((MSG_DHM >= TO_DATE('" + FechaInicio + "', 'MM-DD-YYYY HH24:MI:SS')) AND(MSG_DHM <= TO_DATE('" + FechaFinal + "', 'MM-DD-YYYY HH24:MI:SS')))" +
                            "ORDER BY LANE_ASSIGN.Id_PLAZA," +
                            "LANE_ASSIGN.Id_LANE," +
                            "LANE_ASSIGN.MSG_DHM ";

            // Se llaman a todos los carriles con NA
            Connection.Open();
            Cmd.CommandText = Query.ToString();
            Cmd.Connection = Connection;
            OracleDataReader DataReader = Cmd.ExecuteReader();
            while (DataReader.Read())
            {
                Carril = new Carril();
                Carril.LANE = DataReader["ID_LANE"].ToString();
                Carril.FECHA = DataReader["FECHA_INICIO"].ToString();
                Carril.MATRICULE = DataReader["STAFF_NUMBER"].ToString();
                Carriles.Add(Carril);
            }
            Connection.Close();

            // Se verifican que los carriles se encuentren cerrados en la tabla FIN_POSTE
            foreach (Carril item in Carriles)
            {
                string QueryFin_Poste = @"SELECT COUNT(*) FROM FIN_POSTE WHERE DATE_DEBUT_POSTE = TO_DATE('" + item.FECHA + "', 'MM/DD/YY HH24:MI:SS') AND VOIE = '" + item.LANE + "' AND MATRICULE = '" + item.MATRICULE + "'";
                Connection.Open();
                Cmd.CommandText = Query;
                Cmd.Connection = Connection;
                if (Convert.ToInt32(Cmd.ExecuteScalar()) < 1)
                {
                    Carril = new Carril();
                    Carril.LANE = item.LANE;
                    Carril.FECHA = item.FECHA;
                    Carril.MATRICULE = item.MATRICULE;
                    CarrilesCerrados.Add(Carril);
                    BanValidaciones = false;
                }
                Connection.Close();

                foreach (Carril value in CarrilesCerrados)
                {
                    Message += Message + value.LANE + ", ";
                }
            }

            rpt = BanValidaciones == true ? "OK" : "STOP";

            return rpt;
        }

        /// <summary>
        /// Validar bolsas
        /// </summary>
        /// <param name="FechaInicioD"></param>
        /// <param name="FechaSelect"></param>
        /// <param name="TempTurno"></param>
        /// <returns></returns>
        public string ValidarBolsas(DateTime FechaSelect, string TempTurno, string Conexion)
        {
            OracleCommand Cmd = new OracleCommand();
            OracleConnection Connection = new OracleConnection(Conexion);
            //Connection.ConnectionString = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;

            string rpt = string.Empty;
            string FechaInicio = string.Empty;
            string FechaFinal = string.Empty;
            string TurnoP = string.Empty;

            switch (TempTurno)
            {
                case "22:00 - 06:00":
                    TurnoP = "1";
                    FechaInicio = FechaSelect.AddDays(-1).ToString("MM/dd/yyyy") + " 22:00:00";
                    FechaFinal = FechaSelect.ToString("MM/dd/yyyy") + " 05:59:59";
                    break;
                case "06:00 - 14:00":
                    TurnoP = "2";
                    FechaInicio = FechaSelect.ToString("MM/dd/yyyy") + " 06:00:00";
                    FechaFinal = FechaSelect.ToString("MM/dd/yyyy") + " 13:59:59";
                    break;
                case "14:00 - 22:00":
                    TurnoP = "3";
                    FechaInicio = FechaSelect.ToString("MM/dd/yyyy") + " 14:00:00";
                    FechaFinal = FechaSelect.ToString("MM/dd/yyyy") + " 21:59:59";
                    break;
            }

            // Verifica que todos los carriles cerrados tengan bolsa
            string Query = @"SELECT TO_CHAR(C.DATE_FIN_POSTE,'yyyy-mm-dd') AS FECHA, " +
                            "C.MATRICULE AS cajero, " +
                            "C.VOIE AS Carril, " +
                            "C.NUMERO_POSTE AS Corte, " +
                            "TO_CHAR(C.DATE_DEBUT_POSTE,'HH24:mi:SS') AS Inicio_Turno, " +
                            "TO_CHAR(C.DATE_FIN_POSTE,'HH24:mi:SS') AS Fin_Turno, " +
                            "'Entrega no realizada de bolsa '||C.VOIE||' Inicio '||TO_CHAR(C.DATE_DEBUT_POSTE,'HH24:mi:SS')||',Fin '||TO_CHAR(C.DATE_FIN_POSTE,'HH24:mi:SS')||' '||A.MATRICULE||'/'|| A.NOM AS Aviso " +
                            "FROM FIN_POSTE C " +
                            "LEFT JOIN TABLE_PERSONNEL  A ON C.Matricule = A.Matricule " +
                            "WHERE C.DATE_DEBUT_POSTE " +
                            "BETWEEN to_date('" + FechaInicio + "' ,'mm-dd-yyyy HH24:mi:SS') " +
                            "AND to_date('" + FechaFinal + "' ,'mm-dd-yyyy HH24:mi:SS') " +
                            "AND SAC IS NULL AND FIN_POSTE_CPT22 = " + TurnoP + "AND C.ID_MODE_VOIE in (1,7)";
            Connection.Open();
            Cmd.CommandText = Query;
            Cmd.Connection = Connection;
            OracleDataReader DataReader = Cmd.ExecuteReader();
            while (DataReader.Read())
            {
                BanValidaciones = false;
                Message += DataReader["Aviso"].ToString();
            }
            Connection.Close();

            rpt = BanValidaciones == true ? "OK" : "STOP";

            return rpt;
        }

        /// <summary>
        /// Validar Comentarios
        /// </summary>
        /// <param name="FechaInicioD"></param>
        /// <param name="FechaSelect"></param>
        /// <param name="TempTurno"></param>
        /// <returns></returns>
        //public List<filas> ValidarComentarios(DateTime FechaInicioD, DateTime FechaSelect, string TempTurno)
        public string ValidarComentarios(DateTime FechaSelect, string TempTurno, string Conexion)
        {
            OracleCommand Cmd = new OracleCommand();
            OracleConnection Connection = new OracleConnection(Conexion);
            BanValidaciones = true;
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            //Connection.ConnectionString = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;

            string rpt = string.Empty;
            string FechaInicio = string.Empty;
            string FechaFinal = string.Empty;
            string TurnoP = string.Empty;


            switch (TempTurno)
            {
                case "22:00 - 06:00":
                    TurnoP = "1";
                    FechaInicio = FechaSelect.AddDays(-1).ToString("MM/dd/yyyy") + " 22:00:00";
                    FechaFinal = FechaSelect.ToString("MM/dd/yyyy") + " 05:59:59";
                    break;
                case "06:00 - 14:00":
                    TurnoP = "2";
                    FechaInicio = FechaSelect.ToString("MM/dd/yyyy") + " 06:00:00";
                    FechaFinal = FechaSelect.ToString("MM/dd/yyyy") + " 13:59:59";
                    break;
                case "14:00 - 22:00":
                    TurnoP = "3";
                    FechaInicio = FechaSelect.ToString("MM/dd/yyyy") + " 14:00:00";
                    FechaFinal = FechaSelect.ToString("MM/dd/yyyy") + " 21:59:59";
                    break;
            }

            // Valida que se hayan capturado los  comentarios en la entrega de Bolsa
            // SE MODIFICIO DATE_FIN_POSTE POR C.DATE_DEBUT_POSTE [RODRIGO]




            string Query = @"SELECT " +
                            "C.COMMENTAIRE AS COMENTARIOS, " +
                            "C.SAC AS BOLSA, " +
                            "C.OPERATING_SHIFT AS TURNO, " +
                            "C.DATE_REDDITION AS FECHA, " +
                            "C.RED_TXT1, " +
                            "''||C.RED_TXT1||' bolsa '||TO_CHAR(C.SAC)||' '||A.MATRICULE||'/'|| A.NOM ||'                          ' AS Aviso " +
                            "FROM REDDITION  C " +
                            "LEFT JOIN TABLE_PERSONNEL  A ON C.Matricule = A.Matricule " +
                            "WHERE DATE_REDDITION " +
                            "BETWEEN to_date('" + FechaInicio + "' ,'mm-dd-yyyy HH24:mi:SS') " +
                            "AND to_date('" + FechaFinal + "' ,'mm-dd-yyyy HH24:mi:SS') " +
                            " AND COMMENTAIRE IS NULL AND C.OPERATING_SHIFT  = " + TurnoP;



            Connection.Open();
            Cmd.CommandText = Query;
            Cmd.Connection = Connection;
            OracleDataAdapter myAdapter = new OracleDataAdapter(Cmd);
            OracleDataReader DataReader = Cmd.ExecuteReader();

            while (DataReader.Read())
            {
                BanValidaciones = false;
                myAdapter.Fill(dt);
                Message += DataReader["Aviso"].ToString();
                // break;


            }


            //List<filas> filass = new List<filas>();
            foreach (DataRow indi in dt.Rows)
            {
                filas datos = new filas();
                datos.bolsa = indi["BOLSA"].ToString();
                datos.red = indi["RED_TXT1"].ToString();
                datos.turno = indi["TURNO"].ToString();
                listass.Add(datos);
                break;
            }


            ControlesExportar model = new ControlesExportar();
            model.Listacomentarios = listass;
            Connection.Close();

            rpt = BanValidaciones == true ? "OK" : "STOP";
            return rpt;

            //return listass;
        }


        //public string Insertar_Comentarios(DateTime FechaInicioD, DateTime FechaSelect, string TempTurno, string Comentario)
        public string Isertar_Comentarios(List<filas> listass, List<string> comentario, string Conexion)
        {

            string rpt = string.Empty;

            for (int i = 0; i < comentario.Count; i++)
            {



                string Query = "UPDATE REDDITION SET COMMENTAIRE = '" + comentario[i] +
                                "' WHERE SAC = '" + listass[i].bolsa +
                                "' AND OPERATING_SHIFT = '" + listass[i].turno +
                                "' ";



                //string ConnectionString = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;

                try
                {
                    using (OracleConnection Connection = new OracleConnection(Conexion))

                    {
                        OracleCommand command = new OracleCommand(Query, Connection);
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {

                }

            }

            BanValidaciones = true;
            Message = "MENSAJE INSERTADO";
            rpt = BanValidaciones == true ? "OK" : "STOP";

            return rpt;
        }

        public string Valida_Turno(int valida, DateTime dia)
        {
            string rpt = string.Empty;

            DateTime Dia_Actual = DateTime.Today;
            DateTime Hora_Actual = DateTime.Now;
            DateTime _hora = Convert.ToDateTime("22:00:00").AddDays(-1);
            DateTime hora_ = Convert.ToDateTime("05:59:59");
            DateTime _hora1 = Convert.ToDateTime("06:00:00");
            DateTime hora1_ = Convert.ToDateTime("13:59:59");
            DateTime _hora2 = Convert.ToDateTime("14:00:00");
            DateTime hora2_ = Convert.ToDateTime("21:59:59");




            if (dia < Dia_Actual)
            {
                rpt = "Ok";
                return (rpt);
            }
            else
            {

                switch (valida)
                {
                    case 1:
                        {

                            if (Valida1(Hora_Actual, _hora, hora_) == true)
                            {
                                rpt = "Ok";


                            }
                            else
                            {
                                rpt = "STOP";
                            }

                            return (rpt);

                        }
                    case 2:
                        {

                            if (Valida2(Hora_Actual, _hora1, hora1_) == true)
                            {
                                rpt = "Ok";
                            }
                            else
                            {
                                rpt = "STOP";
                            }

                            return (rpt);

                        }
                    case 3:
                        {


                            if (Valida3(Hora_Actual, _hora2, hora2_) == true)
                            {
                                rpt = "Ok";
                            }
                            else
                            {
                                rpt = "STOP";
                            }

                            return (rpt);

                        }

                }

            }
            return ("OK");
        }


        public bool Valida1(DateTime hora_actual, DateTime hora1, DateTime hora2)
        {

            if (hora_actual > hora1 && hora_actual > hora2)
            {
                BanValidaciones = true;
            }
            else
            {
                BanValidaciones = false;
            }
            return (BanValidaciones);
        }
        public bool Valida2(DateTime hora_actual, DateTime hora1, DateTime hora2)
        {

            if (hora_actual > hora1 && hora_actual > hora2)
            {
                BanValidaciones2 = true;
            }
            else
            {
                BanValidaciones2 = false;
            }
            return (BanValidaciones2);
        }
        public bool Valida3(DateTime hora_actual, DateTime hora1, DateTime hora2)
        {

            if (hora_actual > hora1 && hora_actual > hora2)
            {
                BanValidaciones3 = true;
            }
            else
            {
                BanValidaciones3 = false;
            }
            return (BanValidaciones3);
        }

    }
}