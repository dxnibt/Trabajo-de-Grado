using Dominio.entidades;
using Infraestructura.SQLite.Models;

namespace Infraestructura.SQLite.Mappers{

public static class RespuestaMapper
{
    public static RespuestaData ToData(Respuesta respuesta, int estudianteId)
    {
        return new RespuestaData
        {
            EstudianteId = estudianteId,
            PreguntaId = respuesta.Pregunta.Id,
            Valor = respuesta.Valor,
            EsCorrecta = respuesta.EsCorrecta()
        };
    }
}
}