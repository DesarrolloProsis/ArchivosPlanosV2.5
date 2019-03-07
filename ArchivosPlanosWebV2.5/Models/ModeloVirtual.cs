using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArchivosPlanosWebV2._5.Models
{
    public class ModeloVirtual
    {
        public List<SQL_Delegacion> Delegaciones { get; set; }
        public List<SQL_Plaza> Plazas { get; set; }
        public List<SQL_Carril> Carriles { get; set; }
        public List<SQL_Operadores> Operadores { get; set; }
    }
    public class SQL_Delegacion
    {
        public int Id_Delegacion { get; set; }
        public string Num_Delegacion { get; set; }
        public string Nom_Delegacion { get; set; }
    }
    public class SQL_Plaza
    {
        public int Id_Plaza { get; set; }
        public string Num_Plaza { get; set; }
        public string Nom_Plaza { get; set; }
        public int Delegacion_Id { get; set; }
    }
    public class SQL_Carril
    {
        public int Id_Carril { get; set; }
        public string Num_Gea { get; set; }
        public string Num_Capufe { get; set; }
        public string Num_Tramo { get; set; }
        public int Plaza_Id { get; set; }
    }
    public class SQL_Operadores
    {
        public int Id_Operador { get; set; }
        public string Num_Capufe { get; set; }
        public string Num_Gea { get; set; }
        public string Nom_Operador { get; set; }
        public int Plaza_Id { get; set; }
    }
}