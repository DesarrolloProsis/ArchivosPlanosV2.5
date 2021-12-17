using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace ArchivosPlanosWebV2._5.Services
{
    public class MetodosGlbRepository
    {
        //private static string ConnectString = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;
        //private static OracleConnection Conexion = new OracleConnection(ConnectString);

        public DataSet Ds = new DataSet();
        public DataSet Ds1 = new DataSet();
        public DataSet Ds2 = new DataSet();
        public DataSet Ds3 = new DataSet();
        public DataSet Ds4 = new DataSet();
        public DataRow oDataRow;
        public DataRow oDataRow1;
        public DataRow oDataRow2;
        public DataRow oDataRow3;
        public DataRow oDataRow4;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        /// <summary>
        /// Abre la conexión Oracle.
        /// </summary>
        /// <returns></returns>
        public OracleConnection ConnectionOpen(OracleConnection Conexion)
        {
            if (Conexion.State == ConnectionState.Closed)
                Conexion.Open();

            return Conexion;
        }

        /// <summary>
        /// Cierra la conexión Oracle.
        /// </summary>
        /// <returns></returns>
        public OracleConnection ConnectionClose(OracleConnection Conexion)
        {
            if (Conexion.State == ConnectionState.Open)
                Conexion.Close();

            return Conexion;
        }
        /// <summary>
        /// Lee archivo .ini.
        /// </summary>
        /// <param name="Ruta"></param>
        /// <param name="Seccion"></param>
        /// <param name="Variable"></param>
        /// <returns></returns>
        public string LeeINI(string Ruta, string Seccion, string Variable)
        {
            string Res;
            try
            {

                StringBuilder Resultado;
                Resultado = new StringBuilder((char)0, 255);

                uint Caracteres;
                Caracteres = GetPrivateProfileString(Seccion, Variable, "", Resultado, 255, Ruta);

                Res = Left(Convert.ToString(Resultado), Convert.ToInt32(Caracteres));
            }
            catch (Exception ex)
            {
                Res = string.Empty;
            }
            return Res;
        }

        /// <summary>
        /// Devuelve una cadena que contiene un número especificado de caracteres a partir del lado izquierdo de una cadena.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(string param, int length)
        {
            string result = param.Substring(0, length);

            return result;
        }

        /// <summary>
        /// Funcion creada con base a VB para los if comprimidos. 
        /// </summary>
        /// <param name="Expression"></param>
        /// <param name="TruePart"></param>
        /// <param name="FalsePart"></param>
        /// <returns></returns>
        public string IIf(bool Expression, string TruePart, string FalsePart)
        {
            string ReturnValue = Expression == true ? TruePart : FalsePart;

            return ReturnValue;
        }

        /// <summary>
        /// Función creada con base a VB para retornar un booleano si la expresión se puede evaluar en númerico.
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public Boolean IsNumeric(string valor)
        {
            return int.TryParse(valor, out int result);
        }

        /// <summary>
        /// Convierte una cadena a un formato dd/MM/yyyy. 
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public string Fecha(string fecha)
        {
            string _fecha = fecha.Substring(6, 2) + "/" + fecha.Substring(4, 2) + "/" + fecha.Substring(0, 4) + " " + fecha.Substring(8, 2) + ":" + fecha.Substring(10, 2) + ":" + fecha.Substring(12, 2);

            return _fecha;
        }

        /// <summary>
        /// Método 0 para ejecutrar un query y adaptarlo a un DataSet especificando la columna.
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public bool QueryDataSet(string Query, string Column, OracleConnection Conexion)
        {
            bool _return = false;

            if (Ds.Tables.Count != 0)
                Ds.Clear();

            using (OracleCommand Cmd = new OracleCommand(Query, Conexion))
            {
                using (OracleDataAdapter Da = new OracleDataAdapter(Cmd))
                {
                    Da.Fill(Ds, Column);
                    try
                    {
                        if (Ds.Tables[Column].Rows.Count > 0)
                        {
                            _return = true;
                            oDataRow = Ds.Tables[Column].Rows[0];
                        }
                        else
                            _return = false;
                    }
                    catch (Exception ex)
                    {
                        _return = false;
                    }

                    finally
                    {
                        Cmd.Dispose();
                    }
                }
            }

            return _return;
        }

        /// <summary>
        /// Método 1 para ejecutrar un query y adaptarlo a un DataSet especificando la columna.
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public bool QueryDataSet1(string Query, string Column, OracleConnection Conexion)
        {
            bool _return = false;

            if (Ds1.Tables.Count != 0)
                Ds1.Clear();

            using (OracleCommand Cmd = new OracleCommand(Query, Conexion))
            {
                using (OracleDataAdapter Da = new OracleDataAdapter(Cmd))
                {
                    Da.Fill(Ds1, Column);
                    try
                    {
                        if (Ds1.Tables[Column].Rows.Count > 0)
                        {
                            _return = true;
                            oDataRow1 = Ds1.Tables[Column].Rows[0];
                        }
                        else
                            _return = false;
                    }
                    catch (Exception ex)
                    {
                        _return = false;
                    }

                    finally
                    {
                        Cmd.Dispose();
                    }
                }
            }

            return _return;
        }

        /// <summary>
        /// Método 2 para ejecutrar un query y adaptarlo a un DataSet especificando la columna.
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public bool QueryDataSet2(string Query, string Column, OracleConnection Conexion)
        {
            bool _return = false;

            if (Ds2.Tables.Count != 0)
                Ds2.Clear();

            using (OracleCommand Cmd = new OracleCommand(Query, Conexion))
            {
                using (OracleDataAdapter Da = new OracleDataAdapter(Cmd))
                {
                    Da.Fill(Ds2, Column);
                    try
                    {
                        if (Ds2.Tables[Column].Rows.Count > 0)
                        {
                            _return = true;
                            oDataRow2 = Ds2.Tables[Column].Rows[0];
                        }
                        else
                            _return = false;
                    }
                    catch (Exception ex)
                    {
                        _return = false;
                    }
                    finally
                    {
                        Cmd.Dispose();
                    }
                }
            }

            return _return;
        }

        /// <summary>
        /// Método 3 para ejecutrar un query y adaptarlo a un DataSet especificando la columna.
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public bool QueryDataSet3(string Query, string Column, OracleConnection Conexion)
        {
            bool _return = false;

            if (Ds3.Tables.Count != 0)
                Ds3.Clear();

            using (OracleCommand Cmd = new OracleCommand(Query, Conexion))
            {
                using (OracleDataAdapter Da = new OracleDataAdapter(Cmd))
                {
                    Da.Fill(Ds3, Column);
                    try
                    {
                        if (Ds3.Tables[Column].Rows.Count > 0)
                        {
                            _return = true;
                            oDataRow3 = Ds3.Tables[Column].Rows[0];
                        }
                        else
                            _return = false;
                    }
                    catch (Exception ex)
                    {
                        _return = false;
                    }
                    finally
                    {
                        Cmd.Dispose();
                    }
                }
            }

            return _return;
        }

        /// <summary>
        /// Método 4 para ejecutrar un query y adaptarlo a un DataSet especificando la columna.
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="Column"></param>
        /// <returns></returns>

        public bool QueryDataSet4(string Query, string Column, OracleConnection Conexion)
        {
            bool _return = false;

            if (Ds4.Tables.Count != 0)
                Ds4.Clear();

            using (OracleCommand Cmd = new OracleCommand(Query, Conexion))
            {
                using (OracleDataAdapter Da = new OracleDataAdapter(Cmd))
                {
                    Da.Fill(Ds4, Column);
                    try
                    {
                        if (Ds4.Tables[Column].Rows.Count > 0)
                        {
                            _return = true;
                            oDataRow4 = Ds4.Tables[Column].Rows[0];
                        }
                        else
                            _return = false;
                    }
                    catch (Exception ex)
                    {
                        _return = false;
                    }
                    finally
                    {
                        Cmd.Dispose();
                    }
                }
            }

            return _return;
        }
    }

}