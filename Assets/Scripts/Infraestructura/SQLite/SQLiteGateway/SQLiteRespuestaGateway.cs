using System.Collections.Generic;
using Dominio.entidades;
using Dominio.gateway;
using Infraestructura.SQLite.Mappers;
using Mono.Data.Sqlite;

namespace Infraestructura.SQLite{

public class SQLiteRespuestaGateway : RespuestaGateway
{
    private readonly ConexionSQLite conexion;

    public SQLiteRespuestaGateway(ConexionSQLite conexion)
    {
        this.conexion = conexion;
    }

public void Guardar(Respuesta respuesta)
{
    var data = RespuestaMapper.ToData(respuesta, respuesta.EstudianteId);

    using var conn = conexion.CrearConexion();
    conn.Open();

    var cmd = conn.CreateCommand();

    cmd.CommandText = @"
    INSERT INTO Respuestas (EstudianteId, PreguntaId, RespuestaSeleccionada, EsCorrecta)
    VALUES ($estudianteId, $preguntaId, $valor, $correcta);
    ";

    cmd.Parameters.AddWithValue("$estudianteId", data.EstudianteId);
    cmd.Parameters.AddWithValue("$preguntaId", data.PreguntaId);
    cmd.Parameters.AddWithValue("$valor", data.Valor);
    cmd.Parameters.AddWithValue("$correcta", data.EsCorrecta ? 1 : 0);

    cmd.ExecuteNonQuery();
}
public List<Respuesta> ObtenerPorActividad(int actividadId)
{
    return new List<Respuesta>();
}
}
}