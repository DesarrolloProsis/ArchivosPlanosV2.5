using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArchivosPlanosWebV2._5.Models
{
    public class ControlesExportar
    {
        public List<SelectListItem> ListDelegaciones { get; set; }


        public string DelegacionesId { get; set; }

        public List<SelectListItem> ListPlazaCobro { get; set; }

        public string PlazaCobroId { get; set; }

        public List<SelectListItem> ListTurno { get; set; }

        public string TurnoId { get; set; }

        public string EncargadoTurno { get; set; }

        public DateTime FechaFin { get; set; }

        public DateTime FechaInicio { get; set; }

        public List<HttpPostedFileBase> files { get; set; }

        public List<string> Comentario { get; set; }

        public bool Valida_Pop { get; set; }

        public List<filas> Listacomentarios { get; set; }

    }
    public class filas
    {
        public string bolsa { get; set; }
        public string red { get; set; }
        public string turno { get; set; }
    }
}
