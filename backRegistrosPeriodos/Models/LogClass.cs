using System;

namespace backRegistrosPeriodos.Models
{
    public class LogClass
    {
        public int registro { set; get; }
        public int idsap { set; get; }
        public string log { set; get; }
        public Nullable<System.DateTime> fecha_creacion { set; get; }
        public int idsap_creacion { get; set; }

    }
}
