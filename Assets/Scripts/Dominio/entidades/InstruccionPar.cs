using System;

namespace Dominio.entidades
{
    [Serializable]
    public class InstruccionPar
    {
        public string Imagen1 { get; set; }
        public string Texto1 { get; set; }
        public string Imagen2 { get; set; }
        public string Texto2 { get; set; }

        public InstruccionPar()
        {
            Imagen1 = "";
            Texto1 = "";
            Imagen2 = "";
            Texto2 = "";
        }

        public InstruccionPar(string imagen1, string texto1, string imagen2, string texto2)
        {
            Imagen1 = imagen1 ?? "";
            Texto1 = texto1 ?? "";
            Imagen2 = imagen2 ?? "";
            Texto2 = texto2 ?? "";
        }
    }
}