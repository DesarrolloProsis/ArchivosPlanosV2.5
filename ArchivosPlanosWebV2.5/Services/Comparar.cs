using ArchivosPlanosWebV2._5.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;


namespace ArchivosPlanosWebV2._5.Services
{
    public class Comparar
    {
        string Carpeta = @"C:\ArchivosPlanosWeb\";
        private StreamReader A;
        public string Message = string.Empty;
        public string Error = string.Empty;
        string[] a;
        string[] b;
        string[] c;
        string[] d;
        string[] e;
        string[] f;
        string[] g;
        string[] h;
        string[] i;
        string[] j;
        string[] k;
        string[] l;

        public void ValidarComparacion()
        {
            try 
            {
                var NueveA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.AllDirectories).Where(s => s.EndsWith(".49A")).ToList();
                var DosA = Directory.EnumerateFiles(Carpeta, "*", System.IO.SearchOption.AllDirectories).Where(s => s.EndsWith(".42A")).ToList();
                DataTable dt = new DataTable();
                int columnas = 0;
                using ( TextFieldParser parser = new TextFieldParser(DosA[0].ToString()) )
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    bool primeraLinea = true;
                    bool segundaLinea = true;

                    while (!parser.EndOfData)
                    {
                        //Process row
                        string[] fields = parser.ReadFields();
                        if (primeraLinea)
                        {
                            primeraLinea = false;
                            continue;
                        }
                        if (segundaLinea)
                        {
                            segundaLinea = false;
                            char L = 'A';
                            for (int i = 0; i <= fields.Length ; i++)
                            {
                                dt.Columns.Add(L.ToString());
                                L++;
                            }
                        }
                        foreach (string field in fields)
                        {
                            columnas += 1;
                            dt.Rows.Add(field);
                        }
                    }
                }
                string res = dt.Rows[0]["A"].ToString();
                using (StreamReader A = new StreamReader(NueveA[0].ToString()))
                {
                    string columna;
                    while ((columna = A.ReadLine()) != null)
                    {

                    }
                }
            }
            catch (Exception e)
            {
                Message = "No pudo abrir el archivo";
            }
            

        }

        public void PythonExecuter()
        {
            var py = new ProcessStartInfo();
            py.FileName = @"C:\Program Files\Python310\python.exe";

            var script = @"c:\script\prueba.py";

            py.Arguments = $"\"{script}\"";

            py.UseShellExecute = false;
            py.CreateNoWindow = true;
            py.RedirectStandardOutput = true;
            py.RedirectStandardError = true;

            var errors = "";
            var result = "";

            using (Process process = Process.Start(py))
            {
                result = process.StandardOutput.ReadToEnd();
                errors = process.StandardError.ReadToEnd();
            }
            Message = result;
            Error = errors;
        }
    }
} 