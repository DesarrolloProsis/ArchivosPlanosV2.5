using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ArchivosPlanosWebV2._5.Services
{
    public class Encriptar2

    {

        //string Ruta = @" C:\ArchivosPlanosWeb\";
        //string Ruta = @"C:\Users\Desarrollo3\Desktop\ArchivosPlanosWebModel\ArchivosPlanosWeb\Descargas\";
        string Ruta = @"C:\inetpub\wwwroot\ArchivosPlanos\Descargas\";
        string StrIdentificador = "A";
        public string ArchivoZip;
        public string Message;


        /// <summary>
        /// Encripta los archivos planos.
        /// </summary>
        /// <param name="FechaInicio"></param>
        /// <param name="Str_Turno_block"></param>
        /// <param name="IdPlazaCobro"></param>
        /// <param name="Arch1"></param>
        /// <param name="Arch2"></param>
        /// <param name="Arch3"></param>
        /// <param name="Arch4"></param>
        /// <param name="Arch5"></param>


        public void EncriptarArchivos(DateTime FechaInicio, string Str_Turno_block, string IdPlazaCobro, string Arch1, string Arch2, string Arch3, string Arch4, string Arch5, string Plaza)
        {
            try
            {
                var Mes = FechaInicio.ToString("MM");
                var Año = FechaInicio.ToString("yyyy");

                switch (Mes)
                {
                    case "01":
                        Mes = "enero";
                        break;
                    case "02":
                        Mes = "febrero";
                        break;
                    case "03":
                        Mes = "marzo";
                        break;
                    case "04":
                        Mes = "abril";
                        break;
                    case "05":
                        Mes = "mayo";
                        break;
                    case "06":
                        Mes = "junio";
                        break;
                    case "07":
                        Mes = "julio";
                        break;
                    case "08":
                        Mes = "agosto";
                        break;
                    case "09":
                        Mes = "septiembre";
                        break;
                    case "10":
                        Mes = "octubre";
                        break;
                    case "11":
                        Mes = "noviembre";
                        break;
                    case "12":
                        Mes = "diciembre";
                        break;
                }

                //Cambio para Mostrar la carpeta de la plaza correspondiente 
                //string Dir_archivo_sinEncriptar2 = Ruta + Plaza.Substring(3) + "\\" + Año + "\\" + Mes + "\\" + FechaInicio.ToString("dd") + "\\" + "SinEncriptar\\";
                //string Dir_archivo_sinEncriptar = Ruta + Plaza.Substring(3) + "\\" + Año + "\\" + Mes + "\\" + FechaInicio.ToString("dd") + "\\" + "SinEncriptar\\";
                string Dir_archivo_sinEncriptar2 = Ruta + "\\" + "SinEncriptar\\";
                string Dir_archivo_sinEncriptar = Ruta + "\\" + "SinEncriptar\\";

                //string Nombre_archivo_de_errores = Format(dt_Fecha_Inicio, "dd") & "_errores.txt"

                string Nombre_archivo = string.Empty;

                int Int_turno = 0;

                if (Str_Turno_block.Substring(0, 2) == "06")
                    Int_turno = 5;
                else if (Str_Turno_block.Substring(0, 2) == "14")
                    Int_turno = 6;
                else if (Str_Turno_block.Substring(0, 2) == "22")
                    Int_turno = 4;




                using (ZipFile zipOriginales = new ZipFile())
                {
                    zipOriginales.Password = "ald3s4";
                    zipOriginales.Encryption = EncryptionAlgorithm.WinZipAes256;

                    zipOriginales.AddFile(Ruta + Arch1, "");
                    zipOriginales.AddFile(Ruta + Arch2, "");
                    zipOriginales.AddFile(Ruta + Arch3, "");
                    zipOriginales.AddFile(Ruta + Arch4, "");
                    zipOriginales.AddFile(Ruta + Arch5, "");


                    if (IdPlazaCobro.Length == 3)
                    {
                        if (IdPlazaCobro == "108" || IdPlazaCobro == "008")
                            Nombre_archivo = "0001";
                        else if (IdPlazaCobro == "109")
                            Nombre_archivo = "001B";
                        else if (IdPlazaCobro == "107")
                            Nombre_archivo = "0107";
                        else if (IdPlazaCobro == "061")
                            Nombre_archivo = "061B";
                        else if (IdPlazaCobro == "086" || IdPlazaCobro == "083" || IdPlazaCobro == "027")
                            Nombre_archivo = Nombre_archivo + "01" + IdPlazaCobro.Substring(1, 2);
                        else Nombre_archivo = "0" + IdPlazaCobro;
                    }

                    Nombre_archivo = Nombre_archivo + FechaInicio.ToString("MM") + FechaInicio.ToString("dd") + ".Z" + Int_turno + StrIdentificador;

                    if (!Directory.Exists(Dir_archivo_sinEncriptar))
                    {
                        Directory.CreateDirectory(Dir_archivo_sinEncriptar);
                    }


                    zipOriginales.Save(Dir_archivo_sinEncriptar + Nombre_archivo);
                    //ArchivoZip = Dir_archivo_sinEncriptar + Nombre_archivo;

                }


                EncriptCapufe.EncriptCapufe encripta = new EncriptCapufe.EncriptCapufe();
                encripta.EncriptarFile(Ruta + Arch1);
                encripta.EncriptarFile(Ruta + Arch2);
                encripta.EncriptarFile(Ruta + Arch3);
                encripta.EncriptarFile(Ruta + Arch4);
                encripta.EncriptarFile(Ruta + Arch5);
                //Message = "TODO BIEN";



            }
            catch (Exception ex)
            {
                Message = ex.Message + " " + ex.StackTrace;
                Message = Message.Replace(System.Environment.NewLine, "  ");
            }
        }
    }
}