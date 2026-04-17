namespace Dominio.entidades
{
    public class Reto : ContenidoActividad
    {
        public int Id { get; set; }

        public string Texto { get; set; }

        public string Recurso { get; set; }

        public string Instrucciones { get; set; }

        public Reto(int orden, string texto, string recurso, string instrucciones = "")
        {
            Id = orden;
            Orden = orden;
            Texto = texto;
            Recurso = recurso;
            Instrucciones = instrucciones;
        }
    }
}