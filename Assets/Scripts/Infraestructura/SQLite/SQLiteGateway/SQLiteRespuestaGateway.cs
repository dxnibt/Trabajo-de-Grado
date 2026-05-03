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
public ResumenIntentos ObtenerResumenPorEstudiante(int estudianteId)
{
    using var conn = conexion.CrearConexion();
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        SELECT
            COUNT(*) as total,
            SUM(CASE WHEN EsCorrecta = 0 THEN 1 ELSE 0 END) as incorrectos
        FROM Respuestas WHERE EstudianteId = $e;
    ";
    cmd.Parameters.AddWithValue("$e", estudianteId);

    using var reader = cmd.ExecuteReader();
    if (reader.Read())
        return new ResumenIntentos(reader.GetInt32(0), reader.GetInt32(1));
    return new ResumenIntentos(0, 0);
}

public List<Respuesta> ObtenerPorActividad(int actividadId)
{
    return new List<Respuesta>();
}
}

public class ResumenIntentos
{
    public int Total { get; }
    public int Incorrectos { get; }
    public ResumenIntentos(int total, int incorrectos) { Total = total; Incorrectos = incorrectos; }
}
}