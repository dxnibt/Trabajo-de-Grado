using System.Collections.Generic;
namespace Dominio.entidades{

public class Pregunta : ContenidoActividad
{
    public int Id { get; set; }
    public string Enunciado { get; set; }
    public List <string> Opciones {get; set;}
    public string RespuestaCorrecta { get; set; }

    public Pregunta(int id, int orden, string enunciado, List<string> opciones, string respuestaCorrecta)
    {
        Id = id;
        Orden = orden;
        Enunciado = enunciado;
        Opciones = opciones;
        RespuestaCorrecta = respuestaCorrecta;
    }

    public bool ValidarRespuesta(string respuesta)
    {
        return RespuestaCorrecta.Trim().ToLower() ==
               respuesta.Trim().ToLower();
    }
}
}