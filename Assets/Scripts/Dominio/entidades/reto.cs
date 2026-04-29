using System;
using System.Collections.Generic;

namespace Dominio.entidades
{
    [Serializable]
    public class Reto : ContenidoActividad
    {
        public string Texto { get; set; }
        public string Instrucciones { get; set; }
        public string Recurso { get; set; }
        public List<InstruccionPar> InstruccionesPares { get; set; }

        public Reto()
        {
            InstruccionesPares = new List<InstruccionPar>();
        }

        public Reto(int orden, string texto, string recurso, string instrucciones)
        {
            Orden = orden;
            Texto = texto ?? "";
            Recurso = recurso ?? "";
            Instrucciones = instrucciones ?? "";
            InstruccionesPares = new List<InstruccionPar>();
        }

        public void AgregarParInstruccion(string img1, string txt1, string img2, string txt2)
        {
            InstruccionesPares.Add(new InstruccionPar(
                img1 ?? "",
                txt1 ?? "",
                img2 ?? "",
                txt2 ?? ""
            ));
        }
    }
}