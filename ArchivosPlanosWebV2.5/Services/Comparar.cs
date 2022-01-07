using ArchivosPlanosWebV2._5.Models;

using System.Diagnostics;
using System.IO;
using System.Linq;


namespace ArchivosPlanosWebV2._5.Services
{
    public class Comparar
    {
        string Carpeta = @"C:\ArchivosPlanosWeb\";
        private StreamReader A;
        public string Message = string.Empty;
        public string Error = string.Empty;

        public bool PythonExecuter()
        {
            var py = new ProcessStartInfo();
            py.FileName = @"C:\Python38-32\python.exe";

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

            if (Message.Contains(" Todo bien"))
                return false;
            else
                return true;
        }
    }
} 