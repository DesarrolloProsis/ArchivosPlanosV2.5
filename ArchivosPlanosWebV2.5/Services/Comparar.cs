using ArchivosPlanosWebV2._5.Models;
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

        public void PythonExecuter()
        {
            var py = new ProcessStartInfo();
            py.FileName = @"C:\Program Files\Python310\python.exe";

            var script = @"c:\script\prueba.py";
            //var script = "c:/script/prueba.py";
            py.Arguments = script;

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