using ArchivosPlanosWebV2._5.Models;
using ArchivosPlanosWebV2._5.Services;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ArchivosPlanosWebV2._5.Controllers
{
    public class ExportarController : Controller
    {
        private AppDbContextSQL db2 = new AppDbContextSQL();

        private object SubirArchivo;

        public object Paht { get; private set; }
        //public object Files { get; private set; }
        public object MapPath { get; private set; }
        public object SubirArchivoModelo { get; private set; }
        public object SubirArchivosMdel { get; private set; }
        public static List<filas> listaaa { get; set; }
        public static List<string> comen { get; set; }

        public static bool entra = false;

        public static string Nom1;
        public static string Nom2;

        public string ConexionDB = string.Empty;
        
        // GET: Exportar
        [HttpGet]
        public ActionResult Index()
        {
            string turno = "";
            DateTime time = DateTime.Now;
            DateTime turno1 = new DateTime(time.Year, time.Month, time.Day -1, 22, 0, 0);
            DateTime turno2 = new DateTime(time.Year, time.Month, time.Day, 6, 0, 0);
            DateTime turno3 = new DateTime(time.Year, time.Month, time.Day, 14, 0, 0);
            //DateTime turno3_help = new DateTime(time.Year, time.Month, time.Day, 22, 0, 0);
            if (time >= turno2)
                turno = "1";
            else if (time >= turno3)
                turno = "2";
            else if(time >= turno1.AddDays(1))
                turno = "3";

            var model = new ControlesExportar
            {
                TurnoId = turno,
                FechaInicio = DateTime.Now
            };            
            return View(model);
        }

        // POST : Exportar
        [HttpPost]
        public ActionResult Index(ControlesExportar model)
        {
            //entra = false;
            comen = model.Comentario;
            if (comen == null)
            {
                entra = false;
            }


            ValidacionesRepository validaciones = new ValidacionesRepository();
            Archivo2ARepository archivo2A = new Archivo2ARepository();
            Archivo1ARepository archivo1A = new Archivo1ARepository();
            Archivo9ARepository archivo9A = new Archivo9ARepository();
            ArchivoIIRepository archivoII = new ArchivoIIRepository();
            ArchivoPARepository archivoPA = new ArchivoPARepository();
            EncriptarRepository encriptar = new EncriptarRepository();
            ComprimirRepository comprimir = new ComprimirRepository();
            Comparar compara = new Comparar();
            Encriptar2 encriptar2 = new Encriptar2();
            Comprimir2 comprimir2 = new Comprimir2();
            DateTime fecha_Actual = DateTime.Today;
            DateTime Hora_Actual = DateTime.Now;
            DateTime Hora_ = DateTime.Now;
            DateTime _Hora = DateTime.Now;
            int Int_turno;
            string Message = string.Empty;

            //if (Nom1 != null && Nom2 != null)
            //{
            //    comprimir2.EliminarZip(Nom1, Nom2);
            //}

            var DataStrDele = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(GetDelegaciones().Data); // convert json object to string.
            model.ListDelegaciones = JsonConvert.DeserializeObject<List<SelectListItem>>(DataStrDele);

            var DataStrPlaza = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(GetPlazaCobro().Data); // convert json object to string.
            model.ListPlazaCobro = JsonConvert.DeserializeObject<List<SelectListItem>>(DataStrPlaza);

            var DataStrTurno = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(GetTurnos("full").Data); // convert json object to string.
            model.ListTurno = JsonConvert.DeserializeObject<List<SelectListItem>>(DataStrTurno);

            var Delegacion = model.ListDelegaciones.Find(x => x.Value == model.DelegacionesId);
            var Plaza = model.ListPlazaCobro.Find(p => p.Value == model.PlazaCobroId);
            var Turno = model.ListTurno.Find(p => p.Value == model.TurnoId);
            if (Plaza == null)
            {
                ViewBag.Error = "Falta Delegaciones";
            }
            else if (Plaza.Value.Length == 2)
            {
                Plaza.Value = "0" + Plaza.Value;
                if (Plaza.Value == "004") //Tepotzotlan
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "070") //Polotitlan
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "005") //Palmillas
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "027") //Chichimequillas
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "006") //Querétaro
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "061") //Libramiento
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "083") //Villagrán
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "086") //Cerro Gordo
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "041") //Salamanca
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "069") //Jorobas
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";

                else if (Plaza.Value == "041") //Salamanca
                    ConexionDB = "User Id = GEADBA; Password = fgeuorjvne; Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.1.1.148)(PORT = 1521)))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = GEAPROD)))";
                else
                    Response.Write("<script>alert('" + "Plaza en progreso" + "');</script>");
            }

            DateTime FechaInicio = model.FechaInicio;
            try
            {
                //if (entra == true && comen.ToString() != null)
                //{

                //    entra = false;
                //     if (validaciones.Isertar_Comentarios(listaaa, comen) == "OK")
                //     {
                //        Response.Write("<script>alert('" + "error" + "');</script>");
                //        return View();
                //     }
                //}

                if (Delegacion == null)
                {
                    ViewBag.Titulo = "Formulario incompleto";
                    ViewBag.Mensaje = "Falta Delegación<br />";
                }
                else if (Plaza == null)
                {
                    ViewBag.Titulo = "Formulario incompleto";
                    ViewBag.Mensaje = "Falta Plaza <br />";
                }
                else if (Turno == null)
                {
                    ViewBag.Titulo = "Formulario incompleto";
                    ViewBag.Mensaje = "Falta Turno<br />";
                }
                else if (FechaInicio.ToString() == "01/01/0001 12:00:00 a. m.")
                {
                    ViewBag.Titulo = "Formulario incompleto";
                    ViewBag.Mensaje = "Falta Fecha<br />";
                }
                else if (FechaInicio > fecha_Actual)
                {
                    ViewBag.Titulo = "Formulario llenado incorrectamente";
                    ViewBag.Mensaje = "La fecha debe ser menor a la actual<br />";
                }
                else if (validaciones.Valida_Turno(Convert.ToInt32(Turno.Value), FechaInicio) == "STOP")

                {
                    ViewBag.Titulo = "Formulario llenado incorrectamente";
                    ViewBag.Mensaje = "Aún puedes generar este archivo<br />";
                }

                //else
                //listaaa = validaciones.ValidarComentarios(FechaInicio.AddDays(-1), FechaInicio, Turno.Text);

                //if(listaaa.Count == 1)
                //{

                //    ViewBag.List = new List<filas>();
                //    ViewBag.List = listaaa;
                //    entra = true;
                //    Response.Write("<script>alert('" + validaciones.Message + "');</script>");
                //    return View(ViewBag.List, model);

                //}
                else if (validaciones.ValidarCarrilesCerrados( FechaInicio, Turno.Text, ConexionDB) == "STOP")
                {
                    ViewBag.Titulo = "Existen carriles sin cerrar:";
                    ViewBag.Mensaje = validaciones.Message;
                }
                else if (validaciones.ValidarBolsas(FechaInicio, Turno.Text, ConexionDB) == "STOP")
                {
                    ViewBag.Titulo = "Existen bolsas sin declarar:";
                    ViewBag.Mensaje = validaciones.Message;
                }
                else if (validaciones.ValidarComentarios(FechaInicio, Turno.Text, ConexionDB) == "STOP")
                {
                    ViewBag.Titulo = "Falta ingresar comentarios:";
                    ViewBag.Mensaje = validaciones.Message;
                }

                else

                {

                    //"01" SE DEBE ALMACENAR DE ACUERDO AL INICION DE SESIÓN 
                    archivo1A.Generar_Bitacora_Operacion(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), "03", ConexionDB);
                    archivo2A.Preliquidaciones_de_cajero_receptor_para_transito_vehicular(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), "03", ConexionDB);
                    archivo9A.eventos_detectados_y_marcados_en_el_ECT(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), "03", ConexionDB);
                    archivoII.Registro_usuarios_telepeaje(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), "03", ConexionDB);
                    archivoPA.eventos_detectados_y_marcados_en_el_ECT_EAP(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), "03", ConexionDB);
                    compara.PythonExecuter();
                    encriptar2.EncriptarArchivos(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), archivo1A.Archivo_1, archivo2A.Archivo_2, archivo9A.Archivo_3, archivoPA.Archivo_4, archivoII.Archivo_5, Plaza.Text);
                    encriptar.EncriptarArchivos(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), archivo1A.Archivo_1, archivo2A.Archivo_2, archivo9A.Archivo_3, archivoPA.Archivo_4, archivoII.Archivo_5, Plaza.Text);
                    comprimir.ComprimirArchivos(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), archivo1A.Archivo_1, archivo2A.Archivo_2, archivo9A.Archivo_3, archivoPA.Archivo_4, archivoII.Archivo_5, Plaza.Text);
                    comprimir2.ComprimirArchivos(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), archivo1A.Archivo_1, archivo2A.Archivo_2, archivo9A.Archivo_3, archivoPA.Archivo_4, archivoII.Archivo_5, Plaza.Text);
                    Nom1 = comprimir2.Nombre1;
                    Nom2 = comprimir2.Nombre2;
                    //Response.Write("<script>alert('Archivo 1A: " + archivo1A.Message + "\\nArchivo 2A: " + archivo2A.Message + "\\nArchivo 9A: " + archivo9A.Message + "\\nArchivo LL: " + archivoII.Message + "\\nArchivo PA:" + archivoPA.Message + "\\nEncriptación: " + encriptar.Message + "\\nCompresión: " + comprimir.Message + "');</script>");

                    ViewBag.Titulo = "Resumen de creación de archivos";
                    ViewBag.Mensaje = "Archivo 1A: " + archivo1A.Message + "<br />Archivo 2A: " + archivo2A.Message + "<br />Archivo 9A: " + archivo9A.Message + "<br />Archivo LL: " + archivoII.Message + "<br />Archivo PA: " + archivoPA.Message + "<br />Encriptación: " + encriptar.Message + "<br />Compresión: "+ comprimir.Message + "<br />Errores: " + compara.Message;

                }

            }
            catch (Exception ex)
            {
                Message = ex.Message + " " + ex.StackTrace;
                ViewBag.Titulo = "Error";
                ViewBag.Mensaje = Message;
            }
            string turno = "";
            DateTime time = DateTime.Now;
            DateTime turno1 = new DateTime(time.Year, time.Month, time.Day - 1, 22, 0, 0);
            DateTime turno2 = new DateTime(time.Year, time.Month, time.Day, 6, 0, 0);
            DateTime turno3 = new DateTime(time.Year, time.Month, time.Day, 14, 0, 0);
            //DateTime turno3_help = new DateTime(time.Year, time.Month, time.Day, 22, 0, 0);
            if (time >= turno2)
                turno = "1";
            else if (time >= turno3)
                turno = "2";
            else if (time >= turno1.AddDays(1))
                turno = "3";

            var mdl = new ControlesExportar
            {
                TurnoId = turno,
                FechaInicio = DateTime.Now
            };
            return View(mdl);
        }


        //JSON RESULT PARA LLENAR CON AJAX LAS DELEGACIONES
        [HttpGet]
        public JsonResult GetDelegaciones()
        {

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
            SqlConnection Connection = new SqlConnection(ConnectionString);
            Connection.Open();

            //string query = string.Empty;
            //query = @"SELECT idTramo,nomTramo FROM TYPE_TRAMO";
            //query = @"SELECT ID_Delegacion, Nom_Delegacion FROM TYPE_TRAMO";
            List<SelectListItem> Items = new List<SelectListItem>();
            //SqlCommand cmd = new SqlCommand(query, Connection);
            //SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
            //DataTable dataTable = new DataTable();
            //sqlDataAdapter.Fill(dataTable);
            //SelectListItem listItem = new SelectListItem();
            //DataSet dataSet = new DataSet();
            //dataSet.Tables.Add(dataTable);

            var propsdelega = typeof(Type_Delegacion).GetProperties();
            DataTable dataTable = new DataTable("Tabla_Deolegaciones");
            dataTable.Columns.AddRange(
                propsdelega.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray()
                );
            var delegaciones = db2.Type_Delegacion.ToList();
            delegaciones.ToList().ForEach(
                    i => dataTable.Rows.Add(propsdelega.Select(p => p.GetValue(i, null)).ToArray())
                    );


            foreach (DataRow indi in dataTable.Rows)
            {
                Items.Add(new SelectListItem
                {
                    Text = indi["Nom_Delegacion"].ToString(),
                    Value = indi["Num_Delegacion"].ToString()
                });
            }

            return Json(Items, JsonRequestBehavior.AllowGet);

        }

        //JSON RESULT PARA LLENAR CON AJAX LAS PLAZAS DE COBRO
        [HttpGet]
        public JsonResult GetPlazaCobro()
        {
            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
            SqlConnection Connection = new SqlConnection(ConnectionString);
            Connection.Open();
            //string query = string.Empty;
            //query = @"SELECT idPlaza,nomPlaza FROM TYPE_PLAZA";
            //query = @"SELECT ID_Plaza, Nom_Plaza FROM TYPE_PLAZA";
            List<SelectListItem> Items = new List<SelectListItem>();
            var propsplaza = typeof(Type_Plaza).GetProperties();
            DataTable dataTable = new DataTable("Tabla_Plazas");
            dataTable.Columns.AddRange(propsplaza.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray()
                );
            var plazas = db2.Type_Plaza.ToList();

            plazas.ToList().ForEach(
                i => dataTable.Rows.Add(propsplaza.Select(p => p.GetValue(i, null)).ToArray())
                );
            //SqlCommand cmd = new SqlCommand(query, Connection);
            //SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
            //DataTable dataTable = new DataTable();
            //sqlDataAdapter.Fill(dataTable);
            //SelectListItem listItem = new SelectListItem();
            //DataSet dataSet = new DataSet();
            //dataSet.Tables.Add(dataTable);

            foreach (DataRow indi in dataTable.Rows)
            {
                string numpla;
                if (indi["Num_Plaza"].ToString() == "27" || indi["Num_Plaza"].ToString() == "86" || indi["Num_Plaza"].ToString() == "83")
                    numpla = "1" + indi["Num_Plaza"].ToString();
                else
                    numpla = indi["Num_Plaza"].ToString();

                if (ValidarPlazaLocal(indi["Num_Plaza"].ToString()))
                {
                    Items.Add(new SelectListItem
                    {
                        Text = numpla + " " + indi["Nom_Plaza"].ToString(),
                        Value = indi["Num_Plaza"].ToString()
                    });
                }
            }

            //Items.Add(new SelectListItem
            //{
            //    Value = "08",
            //    Text = "Tlalpan"
            //});

            return Json(Items, JsonRequestBehavior.AllowGet);


        }

        private bool ValidarPlazaLocal(string numPlaza)
        {
            var listaPlazaIp = new Dictionary<string, IPAddress>()
            {                
                { "004",  IPAddress.Parse("10.1.1.55") },//LocalDesarrollo se debe cambiar por la ip de su maquina 
                { "005",  IPAddress.Parse("10.3.23.111") },
                { "006",  IPAddress.Parse("10.3.25.111") },
                { "041",  IPAddress.Parse("10.3.30.111") },
                { "061",  IPAddress.Parse("10.3.27.111") },
                { "069",  IPAddress.Parse("10.3.21.111") },
                { "070",  IPAddress.Parse("10.3.22.111") },
                { "127",  IPAddress.Parse("10.3.24.111") },
                { "183",  IPAddress.Parse("10.3.28.111") },
                { "186",  IPAddress.Parse("10.3.29.111") }
            };

            IPHostEntry host;            
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (KeyValuePair<string, IPAddress> kvp in listaPlazaIp)
            {
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.ToString() == "InterNetwork")
                    {
                        if(kvp.Value.ToString() == ip.ToString() && kvp.Key.ToString().Substring(1,2) == numPlaza)
                        {
                            return true;
                        }
                        
                    }
                }
            }
            return false;
        }

        //JSON RESULT PARA LLENAR CON AJAX LOS TURNO
        [HttpGet]
        public JsonResult GetTurnos(string fecha)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            DateTime time = new DateTime();                                    
            if (fecha != "null")
            {
                Items.Add(new SelectListItem
                {
                    Text = "22:00 - 06:00",
                    Value = "1"
                });

                Items.Add(new SelectListItem
                {
                    Text = "06:00 - 14:00",
                    Value = "2"
                });

                Items.Add(new SelectListItem
                {
                    Text = "14:00 - 22:00",
                    Value = "3",
                });
                return Json(Items, JsonRequestBehavior.AllowGet);
            }
            else if (fecha == "null")
            {
                time = DateTime.Now;
            }




                            
            DateTime turno1 = new DateTime(time.Year, time.Month, time.Day - 1, 22, 0, 0);
            DateTime turno2 = new DateTime(time.Year, time.Month, time.Day, 6, 0, 0);
            DateTime turno3 = new DateTime(time.Year, time.Month, time.Day, 14, 0, 0);

            if (time >= turno2)
            {
                Items.Add(new SelectListItem
                {
                    Text = "22:00 - 06:00",
                    Value = "1"
                });
            }
            else if (time >= turno3)
            {
                Items.Add(new SelectListItem
                {
                    Text = "22:00 - 06:00",
                    Value = "1"
                });

                Items.Add(new SelectListItem
                {
                    Text = "06:00 - 14:00",
                    Value = "2"
                });
            }
            else if (time >= turno1.AddDays(1))
            {

                Items.Add(new SelectListItem
                {
                    Text = "22:00 - 06:00",
                    Value = "1"
                });

                Items.Add(new SelectListItem
                {
                    Text = "06:00 - 14:00",
                    Value = "2"
                });

                Items.Add(new SelectListItem
                {
                    Text = "14:00 - 22:00",
                    Value = "3",
                });
            }
            return Json(Items, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Encriptar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Encriptar(List<HttpPostedFileBase> file)
        {
            List<HttpPostedFileBase> lista = new List<HttpPostedFileBase>();
            if (file != null)
            {

                foreach (var indi in file)
                {
                    lista.Add(indi);
                }

            }
            //string ruta = Server.MapPath("~/Temp/");
            string ruta = "C:\\inetpub\\wwwroot\\ArchivosPlanos\\Temp";

            if (lista.Count == 5)
            {

                ReEncriptarRepository ce = new ReEncriptarRepository();
                ce.SeleccionarArchivos(lista, ruta);
                Response.Write("<script>alert('" + ce.Message + "C:ARCHIVOSPLANOS2\" " + "');</script>");


            }
            else

            {
                Response.Write("<script>alert('Faltan Archivos compruebe que sean 5');</script>");
            }




            return View();
        }

        public ActionResult Descargar()
        {
            Comprimir2 comprimir = new Comprimir2();
            if (Nom1 == null && Nom2 == null)
            {
                Response.Write("<script>alert('" + "Tienes que generar los archivos primero" + "');</script>");
                return View("Index");
            }


            using (ZipFile zip = new ZipFile())
            {

                //C:\Users\Desarrollo3\Desktop\ArchivosPlanosWebModel\ArchivosPlanosWeb\Descargas\Tlalpan\2017\junio\22
                //000106222017.Z4A

                //C:\Users\Desarrollo3\Desktop\ArchivosPlanosWebModel\ArchivosPlanosWeb\Descargas\Tlalpan\2017\junio\22\SinEncriptar
                //00010622.Z4A

                //var archivo1 = Server.MapPath("~/Descargas/" + "\\" + "SinEncriptar\\" + nombre1);
                //var archivo2 = Server.MapPath("~/Descargas/" + nombre2);

                var archivo1 = "C:\\inetpub\\wwwroot\\ArchivosPlanos\\Descargas" + "\\" + "SinEncriptar\\" + Nom1;
                var archivo2 = "C:\\inetpub\\wwwroot\\ArchivosPlanos\\Descargas" + "\\" + Nom2;

                var archivo1_nombre = Path.GetFileName(archivo1);
                var archivo1_arreglo = System.IO.File.ReadAllBytes(archivo1);

                var archivo2_nombre = Path.GetFileName(archivo2);
                var archivo2_arreglo = System.IO.File.ReadAllBytes(archivo2);

                zip.AddEntry(archivo1_nombre, archivo1_arreglo);
                zip.AddEntry(archivo2_nombre, archivo2_arreglo);

                var nombredelZIp = "MIZIP.zip";


                using (MemoryStream output = new MemoryStream())
                {
                    zip.Save(output);
                    comprimir.EliminarZip(Nom1, Nom2);
                    Nom1 = null;
                    Nom2 = null;
                    return File(output.ToArray(), "application/ZIP", nombredelZIp);
                }
            }
        }
    }
}
