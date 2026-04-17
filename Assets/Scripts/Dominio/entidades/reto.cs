namespace Dominio.entidades
{
    public class Reto : ContenidoActividad
    {
        public int Id { get; set; }

        public string Texto { get; set; }

        public string Recurso { get; set; }

        public Reto(int orden, string texto, string recurso)
        {
            Id = orden;
            Orden = orden;
            Texto = texto;
            Recurso = recurso;
        }
    }
}