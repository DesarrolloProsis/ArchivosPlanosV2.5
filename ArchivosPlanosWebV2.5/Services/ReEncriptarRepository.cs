using ArchivosPlanosWebV2._5.Models;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ArchivosPlanosWebV2._5.Services
{
    public class ReEncriptarRepository


    {

        public string Message;
        public void SeleccionarArchivos(List<HttpPostedFileBase> lista, string ruta)
        {
            ControlesExportar model = new ControlesExportar();
            var cadena = string.Empty;
            var nom_archivo = string.Empty;
            var cadena2 = string.Empty;
            var ruta_Guardar = string.Empty;



            try
            {

                foreach (var item in lista)
                {
                    item.SaveAs(ruta + item.FileName);
                }

                //var url = ruta + lista.LastOrDefault().FileName;
                var url = ruta + lista[3].FileName;



                //Busca el año
                var objeReader = new StreamReader(url);
                var Line = objeReader.ReadLine();
                var Fecha = Line.Substring(20, 4);
                objeReader.Close();




                foreach (var indi in lista)
                {
                    //Encriptar los archivos
                    EncriptCapufe.EncriptCapufe encriptar = new EncriptCapufe.EncriptCapufe();
                    encriptar.EncriptarFile(ruta + indi.FileName);
                }


                //Agregar a Zip

                using (ZipFile zip = new ZipFile())
                {
                    foreach (var indi in lista)
                    {
                        zip.AddFile(ruta + indi.FileName + "");
                    }

                    //Creamos el nombre zip

                    ruta_Guardar = @"C:\ArchivosPlanosWeb\";
                    //string Carpeta = @"C:\Users\Desarrollo3\Desktop\ArchivosPlanosWebModel\ArchivosPlanosWeb\Descargas\";


                    string valida;
                    valida = url.Substring(url.Length - 3);
                    if (valida == "49A")
                    {
                        cadena = lista.Find(x => x.FileName.Substring(9, 3) == "4PA").FileName;
                        nom_archivo = cadena.Substring(0, 8) + "." + "Z" + cadena.Substring(9, 1) + "A";
                    }
                    else if (valida == "59A")
                    {
                        cadena = lista.Find(x => x.FileName.Substring(9, 3) == "5PA").FileName;
                        nom_archivo = cadena.Substring(0, 8) + "." + "Z" + cadena.Substring(9, 1) + "A";
                    }
                    else if (valida == "69A")
                    {
                        cadena = lista.Find(x => x.FileName.Substring(9, 3) == "6PA").FileName;
                        nom_archivo = cadena.Substring(0, 8) + "." + "Z" + cadena.Substring(9, 1) + "A";
                    }


                    //Guardamos Zip

                    if (!System.IO.File.Exists(nom_archivo))
                    {
                        zip.Save(ruta_Guardar + nom_archivo);
                    }
                }
                //Creamos el archivo HASH


                HashClass.HashClass hash = new HashClass.HashClass();
                var textoEncriptado = hash.EncryptString(ruta_Guardar + "\\" + nom_archivo);
                string PathF = ruta_Guardar + "\\HASH.txt";

                //Escribimos el archivo Hash

                using (StreamWriter sw = File.CreateText(PathF))
                {

                    sw.WriteLine("ValidaHASH");
                    sw.WriteLine(textoEncriptado);
                    sw.Close();
                }

                //Agregamos Zip

                using (ZipFile zip2 = new ZipFile())
                {

                    zip2.AddFile(ruta_Guardar + "\\" + nom_archivo, "");
                    zip2.AddFile(PathF, "");
                    string nombre_Archivo2 = nom_archivo.Substring(0, 8) + Fecha + nom_archivo.Substring(8);
                    //string ruta_Guardar3 = ruta_Guardar + nom_archivo;
                    if (!System.IO.File.Exists(nombre_Archivo2))
                    {
                        zip2.Save(ruta_Guardar + "\\" + nombre_Archivo2);

                        System.IO.File.Delete(PathF);
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        File.Delete(ruta_Guardar + nom_archivo);
                    }
                }
                foreach (var indi in Directory.GetFiles(@"C:\Users\Desarrollo3\Desktop\ArchivosPlanosWeb\ArchivosPlanosWeb\Temp"))
                {
                    File.Delete(indi);
                }
                Message = "Archvios encriptados correctamente en :  ";
            }
            catch (Exception ex)
            {
                Message = ex.Message + " " + ex.StackTrace;
                Message = Message.Replace(System.Environment.NewLine, "  ");
            }
        }
    }
}