using System;

namespace backRegistrosPeriodos.Models
{
    public class RegistroPeridoClass
    {
        public int registro { get; set; }
        public int registro_padre { get; set; }
        public int idsap { get; set; }
        public int dias { get; set; }
        public int disponibles { get; set; }
        public int id_tipo_solicitud { get; set; }
        public Nullable<System.DateTime> fecha_creacion { get; set; }
        public Nullable<System.DateTime> caducidad { get; set; }
        public string periodo { get; set; }
    }
}
