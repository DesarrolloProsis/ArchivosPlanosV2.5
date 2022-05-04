using ArchivosPlanosWebV2._5.Models;
using ArchivosPlanosWebV2._5.Services;
using Ionic.Zip;
using Microsoft.AspNet.Identity;
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
using System.Web.Script.Serialization;

namespace ArchivosPlanosWebV2._5.Controllers
{
    [Authorize]
    public class ExportarController : Controller
    {
        private AppDbContextSQL db2 = new AppDbContextSQL();
        private ApplicationUserManager _userManager;
        private object SubirArchivo;
        public object Paht { get; private set; }        
        public object MapPath { get; private set; }
        public object SubirArchivoModelo { get; private set; }
        public object SubirArchivosMdel { get; private set; }
        public static List<filas> listaaa { get; set; }
        public static List<string> comen { get; set; }

        public static bool entra = false;

        public static string Nom1;

        public static string Nom2;

        public string ConexionDB = string.Empty;



        //GET: EXPORTAR AUTOMATICO
        [HttpGet]
        public ActionResult IndexAutomatico()
        {
            return View();
        }

        // GET: Exportar
        [HttpGet]
        public ActionResult Index()
        {                                 
            string turno = "";
            DateTime time = DateTime.Now;
            DateTime turno1;
            if (time.Day == 1)            
                if(time.Month == 1)
                    turno1 = new DateTime(time.Year, 12, time.Day, 22, 0, 0).AddMonths(1).AddDays(-1);
                else
                    turno1 = new DateTime(time.Year, time.Month - 1, time.Day, 22, 0, 0).AddMonths(1).AddDays(-1);            
            else
                turno1 = new DateTime(time.Year, time.Month, time.Day - 1, 22, 0, 0);
            DateTime turno2 = new DateTime(time.Year, time.Month, time.Day, 6, 0, 0);
            DateTime turno3 = new DateTime(time.Year, time.Month, time.Day, 14, 0, 0);

            if (time >= turno1 && time < turno3)
                turno = "1";
            else if (time >= turno2 && time < turno1.AddDays(1))
                turno = "2";
            else
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
            //Encriptar2 encriptar2 = new Encriptar2();
            //Comprimir2 comprimir2 = new Comprimir2();
            DateTime fecha_Actual = DateTime.Today;
            DateTime Hora_Actual = DateTime.Now;
            DateTime Hora_ = DateTime.Now;
            DateTime _Hora = DateTime.Now;
            string Message = string.Empty;
            var DataStrDele = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(GetDelegaciones().Data); // convert json object to string.
            model.ListDelegaciones = JsonConvert.DeserializeObject<List<SelectListItem>>(DataStrDele);

            var DataStrPlaza = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(GetPlazaCobro().Data); // convert json object to string.
            model.ListPlazaCobro = JsonConvert.DeserializeObject<List<SelectListItem>>(DataStrPlaza);

            var DataStrTurno = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(GetTurnos("full").Data); // convert json object to string.
            model.ListTurno = JsonConvert.DeserializeObject<List<SelectListItem>>(DataStrTurno);

            SelectListItem Delegacion;
            SelectListItem Plaza;
            SelectListItem Turno;
            DateTime FechaInicio;
            bool CreacionAutomatica = false;

            if (model.DelegacionesId == null && model.PlazaCobroId == null && model.TurnoId == null)
            {
                string turnovalid = "";
                DateTime time_ = DateTime.Now;
                DateTime turno1_;
                if (time_.Day == 1)
                    if (time_.Month == 1)
                        turno1_ = new DateTime(time_.Year, 12, time_.Day, 22, 0, 0).AddMonths(1).AddDays(-1);
                    else
                        turno1_ = new DateTime(time_.Year, time_.Month - 1, time_.Day, 22, 0, 0).AddMonths(1).AddDays(-1);
                else
                    turno1_ = new DateTime(time_.Year, time_.Month, time_.Day - 1, 22, 0, 0);

                DateTime turno2_ = new DateTime(time_.Year, time_.Month, time_.Day, 6, 0, 0);
                DateTime turno3_ = new DateTime(time_.Year, time_.Month, time_.Day, 14, 0, 0);

                if (time_ >= turno1_ && time_ < turno3_)
                    turnovalid = "1";
                else if (time_ >= turno2_ && time_ < turno1_.AddDays(1))
                    turnovalid = "2";
                else
                    turnovalid = "3";


                Delegacion = model.ListDelegaciones.Find(x => x.Value == model.ListDelegaciones[0].Value);
                Plaza = model.ListPlazaCobro.Find(p => p.Value == model.ListPlazaCobro[0].Value);
                Turno = model.ListTurno.Find(p => p.Value == turnovalid);
                FechaInicio = DateTime.Today;
                CreacionAutomatica = true;
            }
            else
            {
                Delegacion = model.ListDelegaciones.Find(x => x.Value == model.DelegacionesId);
                Plaza = model.ListPlazaCobro.Find(p => p.Value == model.PlazaCobroId);
                Turno = model.ListTurno.Find(p => p.Value == model.TurnoId);
                FechaInicio = model.FechaInicio;
            }

            if (Plaza == null)
            {
                ViewBag.Error = "Falta Delegaciones";
            }
            else if (Plaza.Value.Length == 2)
            {                
                var connectionPlaza = validaciones.GetStringConnection(Delegacion.Value, Plaza.Value);
                Plaza.Value = connectionPlaza.plaza;
                ConexionDB = connectionPlaza.connection;
            }

            try
            {
                bool validacionCajeroCerrado = false;//validaciones.ValidarCajeroEncargadoCerrado(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), ConexionDB).Equals("STOP");
                bool validacionCajeroAbierto = validaciones.ValidarCajeroEncargadoAbierto(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), ConexionDB).Equals("STOP");
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
                    ViewBag.Mensaje = "Aún no puedes generar este archivo<br />";
                }
                else if (validaciones.ValidarCarrilesCerrados(FechaInicio, Turno.Text, ConexionDB) == "STOP")
                {
                    ViewBag.Titulo = "Existen carriles sin cerrar:";
                    ViewBag.Mensaje = validaciones.Message;
                }
                else if (validaciones.ValidarBolsas(FechaInicio, Turno.Text, ConexionDB) == "STOP")
                {
                    ViewBag.Titulo = "Existen bolsas sin declarar:";
                    ViewBag.Mensaje = validaciones.Message;
                }
                else if (validaciones.ValidarClaseVehiculo(FechaInicio, Turno.Text, ConexionDB) == "STOP")
                {
                    ViewBag.Titulo = "Faltam Clases Detectadas:";
                    string errorFormat = string.Empty;
                    errorFormat = validaciones.errorFormatClaseDetectada + "<br/>";
                    errorFormat = errorFormat + validaciones.errorFormatClaseMarcada + "<br/>";
                    ViewBag.Mensaje = errorFormat;
                }
                else if (validaciones.ValidarComentarios(FechaInicio, Turno.Text, ConexionDB) == "STOP")
                {
                    ViewBag.Titulo = "Falta ingresar comentarios:";
                    ViewBag.Mensaje = validaciones.Message;
                }
                else if (validacionCajeroAbierto == true || validacionCajeroCerrado == true)
                {
                    ViewBag.Titulo = "Faltan Cajeros / Encargados de Turno";
                    string errorFormat = string.Empty;
                    errorFormat = validaciones.errorFormatCajeroEncargadoAbiertos + "<br/>";
                    errorFormat = errorFormat + validaciones.errorFormatCajeroEncargadoCerrado + "<br/>";
                    errorFormat = errorFormat + validaciones.errorFormatIdentOperacionAbierto;
                    ViewBag.Mensaje = errorFormat;
                }
                else
                {
                    string Carpeta = @"C:\ArchivosPlanosWeb\";
                    var NueveA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("9A")).ToList();      
                    var DosA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("2A")).ToList();  
                    var UnoA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("1A")).ToList(); 
                    var PA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("PA")).ToList();
                    var II = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("II")).ToList();

                    compara.Borrar(NueveA);
                    compara.Borrar(DosA);
                    compara.Borrar(UnoA);
                    compara.Borrar(PA);
                    compara.Borrar(II);
                    
                    string tramoNew = Delegacion.Value == "67" ? "01" : "03";//67 Acapulco se transforma a 01
                    archivo1A.Generar_Bitacora_Operacion(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), tramoNew, ConexionDB);
                    archivo2A.Preliquidaciones_de_cajero_receptor_para_transito_vehicular(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), tramoNew, ConexionDB);
                    archivo9A.eventos_detectados_y_marcados_en_el_ECT(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), tramoNew, ConexionDB);
                    archivoII.Registro_usuarios_telepeaje(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), tramoNew, ConexionDB);
                    archivoPA.eventos_detectados_y_marcados_en_el_ECT_EAP(Turno.Text, FechaInicio, Convert.ToString(Plaza.Value), Convert.ToString(Delegacion.Value), tramoNew, ConexionDB);

                    if (User.IsInRole("SuperAdmin"))
                    {

                        bool Errores = compara.Executer();
                        if (Errores)
                        {
                            ViewBag.Titulo = "Errores en los archivos planos python";
                            ViewBag.Python = true;
                            ViewBag.Mensaje = "Errores: " + compara.Message;
                            var mdlpy = new ControlesExportar
                            {
                                TurnoId = Turno.Value,
                                FechaInicio = FechaInicio,
                                DelegacionesId = Delegacion.Value
                            };
                            if (CreacionAutomatica)
                                return Json(new { mensaje = ViewBag.Mensaje, titulo = ViewBag.Titulo, errores = ViewBag.Python }, JsonRequestBehavior.AllowGet);
                            else
                                return View(mdlpy);
                        }
                    }

                    
                    encriptar.EncriptarArchivos(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), archivo1A.Archivo_1, archivo2A.Archivo_2, archivo9A.Archivo_3, archivoPA.Archivo_4, archivoII.Archivo_5, Plaza.Text);
                    comprimir.ComprimirArchivos(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), archivo1A.Archivo_1, archivo2A.Archivo_2, archivo9A.Archivo_3, archivoPA.Archivo_4, archivoII.Archivo_5, Plaza.Text);
                    
                    //Nom1 = comprimir2.Nombre1;
                    //Nom2 = comprimir2.Nombre2;
                    ViewBag.Titulo = "Resumen de creación de archivos";
                    ViewBag.Mensaje = "Archivo 1A: " + archivo1A.Message + "<br />Archivo 2A: " + archivo2A.Message + "<br />Archivo 9A: " + archivo9A.Message + "<br />Archivo LL: " + archivoII.Message + "<br />Archivo PA: " + archivoPA.Message + "<br />Encriptación: " + encriptar.Message + "<br />Compresión: " + comprimir.Message + "<br />Errores: " + compara.Message + "\n" + archivo1A.validacionesNuevas + "\n" + archivo2A.validacionesNuevas + "\n" + archivo9A.validacionesNuevas + "\n" + archivo2A.validacionNuevaEmpalmeHorario;                              
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
            DateTime turno1;
            if (time.Day == 1)
                if (time.Month == 1)
                    turno1 = new DateTime(time.Year, 12, time.Day, 22, 0, 0).AddMonths(1).AddDays(-1);
                else
                    turno1 = new DateTime(time.Year, time.Month - 1, time.Day, 22, 0, 0).AddMonths(1).AddDays(-1);
            else
                turno1 = new DateTime(time.Year, time.Month, time.Day - 1, 22, 0, 0);
            DateTime turno2 = new DateTime(time.Year, time.Month, time.Day, 6, 0, 0);
            DateTime turno3 = new DateTime(time.Year, time.Month, time.Day, 14, 0, 0);

            if (time >= turno1 && time < turno3)
                turno = "1";
            else if (time >= turno2 && time < turno1.AddDays(1))
                turno = "2";
            else
                turno = "3";

            var mdl = new ControlesExportar
            {
                TurnoId = turno,
                FechaInicio = DateTime.Now
            };
            if(CreacionAutomatica)
                return Json(new { mensaje = ViewBag.Mensaje, titulo = ViewBag.Titulo, errores = ViewBag.Error }, JsonRequestBehavior.AllowGet);            
            else
                return View(mdl);
        }

        [HttpPost]
        public ActionResult Comprimir(ControlesExportar model)
        {            
            EncriptarRepository encriptar = new EncriptarRepository();
            ComprimirRepository comprimir = new ComprimirRepository();
            Comparar compara = new Comparar();
            //Encriptar2 encriptar2 = new Encriptar2();
            //Comprimir2 comprimir2 = new Comprimir2();
            SelectListItem Plaza;
            SelectListItem Turno;
            SelectListItem Delegacion;
            DateTime FechaInicio;
            string Message = string.Empty;

            comen = model.Comentario;
            if (comen == null)
            {
                entra = false;
            }

            var DataStrDele = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(GetDelegaciones().Data); // convert json object to string.
            model.ListDelegaciones = JsonConvert.DeserializeObject<List<SelectListItem>>(DataStrDele);

            var DataStrPlaza = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(GetPlazaCobro().Data); // convert json object to string.
            model.ListPlazaCobro = JsonConvert.DeserializeObject<List<SelectListItem>>(DataStrPlaza);

            var DataStrTurno = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(GetTurnos("full").Data); // convert json object to string.
            model.ListTurno = JsonConvert.DeserializeObject<List<SelectListItem>>(DataStrTurno);

            if (model.DelegacionesId == null && model.PlazaCobroId == null && model.TurnoId == null)
            {
                string turnovalid = "";
                Delegacion = model.ListDelegaciones.Find(x => x.Value == model.ListDelegaciones[0].Value);
                DateTime time_ = DateTime.Now;
                DateTime turno1_;
                if (time_.Day == 1)
                    if (time_.Month == 1)
                        turno1_ = new DateTime(time_.Year, 12, time_.Day, 22, 0, 0).AddMonths(1).AddDays(-1);
                    else
                        turno1_ = new DateTime(time_.Year, time_.Month - 1, time_.Day, 22, 0, 0).AddMonths(1).AddDays(-1);
                else
                    turno1_ = new DateTime(time_.Year, time_.Month, time_.Day - 1, 22, 0, 0);
                DateTime turno2_ = new DateTime(time_.Year, time_.Month, time_.Day, 6, 0, 0);
                DateTime turno3_ = new DateTime(time_.Year, time_.Month, time_.Day, 14, 0, 0);

                if (time_ >= turno1_ && time_ < turno3_)
                    turnovalid = "1";
                else if (time_ >= turno2_ && time_ < turno1_.AddDays(1))
                    turnovalid = "2";
                else
                    turnovalid = "3";


                Plaza = model.ListPlazaCobro.Find(p => p.Value == model.ListPlazaCobro[0].Value);
                Turno = model.ListTurno.Find(p => p.Value == turnovalid);
                FechaInicio = DateTime.Today;                
            }
            else
            {
                Delegacion = model.ListDelegaciones.Find(x => x.Value == model.DelegacionesId);
                Plaza = model.ListPlazaCobro.Find(p => p.Value == model.PlazaCobroId);
                Turno = model.ListTurno.Find(p => p.Value == model.TurnoId);
                FechaInicio = model.FechaInicio;
            }

            if (Plaza.Value.Length == 2)
            {
                if (Delegacion.Value == "67")
                {
                    Plaza.Value = "1" + Plaza.Value;
                    if (Plaza.Value == "108") //Tlalpan
                        Plaza.Value = "008";
                }
                else
                {
                    Plaza.Value = "0" + Plaza.Value;
                }
            }
            try
            {
                if (model.ValidarEncriptar)
                {
                    bool Errores = compara.Executer();                
                    if (Errores)
                    {
                        ViewBag.Titulo = "Errores en los archivos planos";
                        ViewBag.Mensaje = "Errores: " + compara.Message;
                        ViewBag.Python = true;
                        var mdlpy = new ControlesExportar
                        {
                            DelegacionesId = model.DelegacionesId,
                            PlazaCobroId = model.PlazaCobroId,
                            TurnoId = Turno.Value,
                            FechaInicio = FechaInicio
                            
                        };
                        return Json(new { mensaje = ViewBag.Mensaje, titulo = ViewBag.Titulo, model = mdlpy, errores = true }, JsonRequestBehavior.AllowGet);
                    }
                }
                string Carpeta = @"C:\ArchivosPlanosWeb\";
                var NueveA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("9A")).ToList();
                string[] nueveA = NueveA[0].ToString().Split(new[] { "\\" }, StringSplitOptions.None);
                var DosA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("2A")).ToList();
                string[] dosA = DosA[0].ToString().Split(new[] { "\\" }, StringSplitOptions.None);
                var UnoA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("1A")).ToList();
                string[] unoA = UnoA[0].ToString().Split(new[] { "\\" }, StringSplitOptions.None);
                var PA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("PA")).ToList();
                string[] pA = PA[0].ToString().Split(new[] { "\\" }, StringSplitOptions.None);
                var II = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.TopDirectoryOnly).Where(s => s.EndsWith("II")).ToList();
                string[] ii = II[0].ToString().Split(new[] { "\\" }, StringSplitOptions.None);
                
                encriptar.EncriptarArchivos(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), unoA[2], dosA[2], nueveA[2], pA[2], ii[2], Plaza.Text);
                comprimir.ComprimirArchivos(FechaInicio, Turno.Text, Convert.ToString(Plaza.Value), unoA[2], dosA[2], nueveA[2], pA[2], ii[2], Plaza.Text);                
               

                ViewBag.Titulo = "Encriptar y Comprimir";
                ViewBag.Python = false;
                ViewBag.Mensaje = "Se encriptaron los archivos con exito";
            }
            catch (Exception ex)
            {
                Message = ex.Message + " " + ex.StackTrace;
                ViewBag.Titulo = "Error";
                ViewBag.Mensaje = Message;
            }

            string turno = "";
            DateTime time = DateTime.Now;
            DateTime turno1;
            if (time.Day == 1)
                if (time.Month == 1)
                    turno1 = new DateTime(time.Year, 12, time.Day, 22, 0, 0).AddMonths(1).AddDays(-1);
                else
                    turno1 = new DateTime(time.Year, time.Month - 1, time.Day, 22, 0, 0).AddMonths(1).AddDays(-1);
            else
                turno1 = new DateTime(time.Year, time.Month, time.Day - 1, 22, 0, 0);
            DateTime turno2 = new DateTime(time.Year, time.Month, time.Day, 6, 0, 0);
            DateTime turno3 = new DateTime(time.Year, time.Month, time.Day, 14, 0, 0);
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
            return Json(new { mensaje = ViewBag.Mensaje, titulo = ViewBag.Titulo, model = mdl, errores = false }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public JsonResult GetDelegaciones()
        {
            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
            SqlConnection Connection = new SqlConnection(ConnectionString);
            Connection.Open();                                    
            List<SelectListItem> Items = new List<SelectListItem>();     
            var propsdelega = typeof(Type_Delegacion).GetProperties();
            DataTable dataTable = new DataTable("Tabla_Deolegaciones");
            dataTable.Columns.AddRange(propsdelega.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray());
            var delegaciones = db2.Type_Delegacion.ToList();
            delegaciones.ToList().ForEach(i => dataTable.Rows.Add(propsdelega.Select(p => p.GetValue(i, null)).ToArray()));

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
            List<SelectListItem> Items = new List<SelectListItem>();
            var propsplaza = typeof(Type_Plaza).GetProperties();
            DataTable dataTable = new DataTable("Tabla_Plazas");
            dataTable.Columns.AddRange(propsplaza.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray());
            var plazas = db2.Type_Plaza.ToList();
            plazas.ToList().ForEach(i => dataTable.Rows.Add(propsplaza.Select(p => p.GetValue(i, null)).ToArray()));

            foreach (DataRow indi in dataTable.Rows)
            {
                string numpla;
                //Validaciones Irapuato
                if (indi["Num_Plaza"].ToString() == "27" || indi["Num_Plaza"].ToString() == "86" || indi["Num_Plaza"].ToString() == "83")
                    numpla = "1" + indi["Num_Plaza"].ToString();
                //Validacion Acapulco
                else if(indi["Num_Plaza"].ToString() == "81" || indi["Num_Plaza"].ToString() == "81" || indi["Num_Plaza"].ToString() == "81")
                    numpla = "8" + indi["Num_Plaza"].ToString();
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
            return Json(Items, JsonRequestBehavior.AllowGet);
        }

        private bool ValidarPlazaLocal(string numPlaza)
        {
            var listaPlazaIp = new Dictionary<string, IPAddress>()
            {
                //LOCAL
                //{ "008",  IPAddress.Parse("10.1.1.0") },
                //Tramo Irapuato
                { "004",  IPAddress.Parse("10.3.20.0") },//Tepozotlan
                { "005",  IPAddress.Parse("10.3.23.0") },//Palmillas
                { "006",  IPAddress.Parse("10.3.25.0") },//Queretaro
                { "041",  IPAddress.Parse("10.3.30.0") },//Salamanca
                { "061",  IPAddress.Parse("10.3.27.0") },//Libramiento
                { "069",  IPAddress.Parse("10.3.21.0") },//Jorobas
                { "070",  IPAddress.Parse("10.3.22.0") },//Polotitlan
                { "127",  IPAddress.Parse("10.3.24.0") },//Chichimequillas
                { "183",  IPAddress.Parse("10.3.28.0") },//Villagran
                { "186",  IPAddress.Parse("10.3.29.0") },//Cerro Gordo
                //Tramo Acapulco pendiente de buscar ip
                { "008",  IPAddress.Parse("10.4.168.0")},//Tlalpan
                { "009",  IPAddress.Parse("10.4.169.0")},//TresMarias
                { "101",  IPAddress.Parse("10.4.161.0")},//Alpuyeca
                { "102",  IPAddress.Parse("10.4.162.0")},//PasoMorelos
                { "103",  IPAddress.Parse("10.4.163.0")},//PaloBlanco
                { "104",  IPAddress.Parse("10.4.164.0")},//LaVenta
                { "105",  IPAddress.Parse("10.4.165.0")},//Xochitepec
                { "106",  IPAddress.Parse("10.4.166.0")},//Aeropuerto
                { "107",  IPAddress.Parse("10.4.167.0")},//EmilianoZapata
                { "184",  IPAddress.Parse("10.4.184.0")}//FranciscoVelasco
            };

            IPHostEntry host;            
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (KeyValuePair<string, IPAddress> kvp in listaPlazaIp)
            {
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.ToString() == "InterNetwork")
                    {
                        var ipSplite = ip.ToString().Split('.');
                        string ipCut = ipSplite[0] + "." + ipSplite[1] + "." + ipSplite[2] + '.' + '0';
                        if(kvp.Value.ToString() == ipCut && kvp.Key.ToString().Substring(1,2) == numPlaza)
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

            DateTime turno1;
            if (time.Day == 1)
                if (time.Month == 1)
                    turno1 = new DateTime(time.Year, 12, time.Day).AddMonths(1).AddDays(-1);
                else
                    turno1 = new DateTime(time.Year, time.Month - 1, time.Day).AddMonths(1).AddDays(-1);
            else
                turno1 = new DateTime(time.Year, time.Month, time.Day - 1, 22, 0, 0);
            DateTime turno2 = new DateTime(time.Year, time.Month, time.Day, 6, 0, 0);
            DateTime turno3 = new DateTime(time.Year, time.Month, time.Day, 14, 0, 0);

            if (time >= turno1 && time < turno3)
            {
                Items.Add(new SelectListItem
                {
                    Text = "22:00 - 06:00",
                    Value = "1"
                });
            }
            else if (time >= turno2 && time < turno1.AddDays(1))
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
            else
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
            try
            {
                const string Ruta = @"C:\ArchivosPlanosWeb\";
                const string RutaEncriptado = Ruta + "Download" + "\\" + "Encriptado" + "\\";
                const string RutaSinEncriptar = Ruta + "Download" + "\\" + "SinEncriptar" + "\\";

                DirectoryInfo directoryInfoEncriptado = new DirectoryInfo(RutaEncriptado);
                DirectoryInfo directoryInfoSinEncriptado = new DirectoryInfo(RutaSinEncriptar);

                FileInfo[] filesEncriptado = directoryInfoEncriptado.GetFiles();
                FileInfo[] filesSinEncriptar = directoryInfoSinEncriptado.GetFiles();

                using (ZipFile zip = new ZipFile())
                {
                    if (User.IsInRole("SuperAdmin"))
                    {
                        zip.AddFile(RutaEncriptado + filesEncriptado[0].Name, "");
                        zip.AddFile(RutaSinEncriptar + filesSinEncriptar[0].Name, "");
                    }
                    if (User.IsInRole("Capufe"))
                    {
                        zip.AddFile(RutaEncriptado + filesEncriptado[0].Name, "");                        
                    }              

                    zip.Save(Ruta  + "Download" + "\\" + "test.zip");
                }                       

                return File(Ruta + "Download" + "\\" + "test.zip", "application/Zip", filesEncriptado[0].Name);                
                
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }            
  
        }
    }
}
