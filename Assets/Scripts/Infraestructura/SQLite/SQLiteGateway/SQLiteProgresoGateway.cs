using System.Collections.Generic;
using Dominio.entidades;
using Dominio.gateway;
using Infraestructura.SQLite.Mappers;

namespace Infraestructura.SQLite.SQLiteGateway{

public class SQLiteProgresoGateway : ProgresoGateway
{
    private readonly ConexionSQLite conexion;

    public SQLiteProgresoGateway(ConexionSQLite conexion)
    {
        this.conexion = conexion;
    }

    public void Guardar(Progreso progreso)
    {
        var data = ProgresoMapper.ToData(progreso, progreso.EstudianteId);

        using var conn = conexion.CrearConexion();
        conn.Open();

        var cmd = conn.CreateCommand();

        cmd.CommandText = @"
        INSERT INTO Progreso (EstudianteId, ActividadId, Completada, IndiceContenidoActual)
        VALUES ($e,$a,$c,$i)
        ON CONFLICT(EstudianteId, ActividadId) DO UPDATE SET
            Completada=$c,
            IndiceContenidoActual=$i;
        ";

        cmd.Parameters.AddWithValue("$e", data.EstudianteId);
        cmd.Parameters.AddWithValue("$a", data.ActividadId);
        cmd.Parameters.AddWithValue("$c", data.Completada ? 1 : 0);
        cmd.Parameters.AddWithValue("$i", data.IndiceContenidoActual);

        cmd.ExecuteNonQuery();
    }

    public int? ObtenerIndiceGuardado(int estudianteId, int actividadId)
    {
        using var conn = conexion.CrearConexion();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT IndiceContenidoActual FROM Progreso
            WHERE EstudianteId=$e AND ActividadId=$a AND Completada=0
            LIMIT 1;
        ";
        cmd.Parameters.AddWithValue("$e", estudianteId);
        cmd.Parameters.AddWithValue("$a", actividadId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read()) return reader.GetInt32(0);
        return null;
    }

    public Progreso ObtenerPorEstudiante(int id)
    {
        using var conn = conexion.CrearConexion();
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT ActividadId, Completada, IndiceContenidoActual
        FROM Progreso WHERE EstudianteId=$id
        LIMIT 1;
        ";

        cmd.Parameters.AddWithValue("$id", id);

        using var reader = cmd.ExecuteReader();

        if (!reader.Read()) return null;

        var data = new Models.ProgresoData
        {
            EstudianteId = id,
            ActividadId = reader.GetInt32(0),
            Completada = reader.GetInt32(1) == 1,
            IndiceContenidoActual = reader.GetInt32(2)
        };

        return ProgresoMapper.ToDomain(data);
    }
}
}