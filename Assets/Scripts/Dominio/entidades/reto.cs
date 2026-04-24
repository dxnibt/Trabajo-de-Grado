using System.Collections.Generic;

namespace Dominio.entidades
{
    public class Reto : ContenidoActividad
    {
        public int Id { get; set; }

        public string Texto { get; set; }

        public string Recurso { get; set; }

        public string Instrucciones { get; set; }

        public List<InstruccionPair> InstruccionesPares { get; set; }

        public Reto(int orden, string texto, string recurso, string instrucciones = "")
        {
            Id = orden;
            Orden = orden;
            Texto = texto;
            Recurso = recurso;
            Instrucciones = instrucciones;
            InstruccionesPares = new List<InstruccionPair>();
        }

        public void AgregarInstrucciones(string imagen1, string texto1, string imagen2, string texto2)
        {
            InstruccionesPares.Add(new InstruccionPair
            {
                Imagen1 = imagen1,
                Texto1 = texto1,
                Imagen2 = imagen2,
                Texto2 = texto2
            });
        }
    }

    public class InstruccionPair
    {
        public string Imagen1 { get; set; }
        public string Texto1 { get; set; }
        public string Imagen2 { get; set; }
        public string Texto2 { get; set; }
    }
}