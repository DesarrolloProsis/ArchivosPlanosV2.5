using Ionic.Zip;
using System;

namespace ArchivosPlanosWebV2._5.Services
{
    public class ComprimirRepository

    {
        string Ruta = @" C:\ArchivosPlanosWeb\";
        //string Ruta2 = @"C:\Users\Desarrollo3\Desktop\ArchivosPlanosWebModel\ArchivosPlanosWeb\Descargas\";
        string StrIdentificador = "A";
        public string ArchivoZip;
        public string Message;

        /// <summary>
        /// Comprime los archivos planos.
        /// </summary>
        /// <param name="FechaInicio"></param>
        /// <param name="Str_Turno_block"></param>
        /// <param name="IdPlazaCobro"></param>
        /// <param name="Arch1"></param>
        /// <param name="Arch2"></param>
        /// <param name="Arch3"></param>
        /// <param name="Arch4"></param>
        /// <param name="Arch5"></param>
        /// 
        public void ComprimirArchivos(DateTime FechaInicio, string Str_Turno_block, string IdPlazaCobro, string Arch1, string Arch2, string Arch3, string Arch4, string Arch5, string Plaza)
        {
            try
            {
                string PathF;


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

                var ArchivoRuta = Ruta + Plaza.Substring(3) + "\\" + Año + "\\" + Mes + "\\" + FechaInicio.ToString("dd") + "\\";
                /////////////////////////////////////
                //var ArchivoRuta2 = Ruta2 + Plaza.Substring(3) + "\\" + Año + "\\" + Mes + "\\" + FechaInicio.ToString("dd") + "\\";

                string Nombre_archivo = string.Empty;
                int Int_turno = 0;

                if (Str_Turno_block.Substring(0, 2) == "06")
                    Int_turno = 5;
                else if (Str_Turno_block.Substring(0, 2) == "14")
                    Int_turno = 6;
                else if (Str_Turno_block.Substring(0, 2) == "22")
                    Int_turno = 4;

                using (ZipFile zip = new ZipFile())
                {
                    zip.AddFile(Ruta + Arch1, "");
                    zip.AddFile(Ruta + Arch2, "");
                    zip.AddFile(Ruta + Arch3, "");
                    zip.AddFile(Ruta + Arch4, "");
                    zip.AddFile(Ruta + Arch5, "");

                   
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
                            Nombre_archivo = "01" + IdPlazaCobro.Substring(1, 2);
                        else
                            Nombre_archivo = "0" + IdPlazaCobro;
                    }

                    Nombre_archivo = Nombre_archivo + FechaInicio.ToString("MM") + FechaInicio.ToString("dd") + ".Z" + Int_turno + StrIdentificador;

                    //MODIFICACION SIN ARCHIVOS DE ERRORRES: EMILIANO
                    // if(ArchivoRuta != "" && Nombre_archivo != "")
                    //{
                    zip.Save(ArchivoRuta + Nombre_archivo);                    //}

                }

                //encriptacion

                HashClass.HashClass HassText = new HashClass.HashClass();

                var TextoEncriptado = HassText.EncryptString(ArchivoRuta + Nombre_archivo);
                PathF = ArchivoRuta + "HASH.txt";

                //Creación y Escritura del Archivo validacion HASH

                using (System.IO.StreamWriter SW = System.IO.File.CreateText(PathF))
                {
                    SW.WriteLine("ValidaHASH:");
                    SW.WriteLine(TextoEncriptado);
                    SW.Flush();
                    SW.Close();
                }

                using (ZipFile Zip2 = new ZipFile())
                {
                    Zip2.AddFile(PathF,"");
                    Zip2.AddFile(ArchivoRuta + Nombre_archivo,"");

                    var NoPlaza = string.Empty;
                    if (IdPlazaCobro.Length == 3)
                    {
                        if (IdPlazaCobro == "108" || IdPlazaCobro == "008")
                            NoPlaza = "0001";
                        else if (IdPlazaCobro == "109")
                            NoPlaza = "001B";
                        else if (IdPlazaCobro == "107")
                            NoPlaza = "0107";
                        else if (IdPlazaCobro == "061")
                            NoPlaza = "061B";
                        else if (IdPlazaCobro == "086" || IdPlazaCobro == "083" || IdPlazaCobro == "027")
                            NoPlaza = NoPlaza + "01" + IdPlazaCobro.Substring(1, 2);
                        else NoPlaza = "0" + IdPlazaCobro;
                    }

                    var Archivo2 = NoPlaza + FechaInicio.ToString("MM") + FechaInicio.ToString("dd") + FechaInicio.ToString("yyyy") + ".Z" + Int_turno + StrIdentificador;
                    Zip2.Save(ArchivoRuta + Archivo2);

                }
                System.IO.File.Delete(PathF);
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.IO.File.Delete(ArchivoRuta + Nombre_archivo);

                //00010727.Z4A

                Message = "Todo bien";
                EliminarArchivos(Arch1, Arch2, Arch3, Arch4, Arch5);

            }
            catch (Exception ex)
            {
                Message = ex.Message + " " + ex.StackTrace;
                Message = Message.Replace(System.Environment.NewLine, "  ");
            }
        }

        /// <summary>
        /// Elimina los archivos planos.
        /// </summary>
        /// <param name="Arch1"></param>
        /// <param name="Arch2"></param>
        /// <param name="Arch3"></param>
        /// <param name="Arch4"></param>
        /// <param name="Arch5"></param>
        private void EliminarArchivos(string Arch1, string Arch2, string Arch3, string Arch4, string Arch5)
        {
            System.IO.File.Delete(Ruta + Arch1);
            System.IO.File.Delete(Ruta + Arch2);
            System.IO.File.Delete(Ruta + Arch3);
            System.IO.File.Delete(Ruta + Arch4);
            System.IO.File.Delete(Ruta + Arch5);
        }
    }
}