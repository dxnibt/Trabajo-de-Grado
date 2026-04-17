using Infraestructura.SQLite;
using Mono.Data.Sqlite;

public class SeedActividad
{
    private readonly ConexionSQLite conexion;

    public SeedActividad(ConexionSQLite conexion)
    {
        this.conexion = conexion;
    }

    public void Ejecutar()
    {
        using var conn = conexion.CrearConexion();
        conn.Open();

        var cmd = conn.CreateCommand();

        cmd.CommandText = @"
        INSERT INTO Actividad (Id, Titulo, NivelId)
        VALUES (1, 'El Sonido', 1);
        ";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
        INSERT INTO ContenidoActividad (ActividadId, Tipo, Orden, Texto, Recurso, RespuestaCorrecta)
        VALUES 
        (1, 'Historia', 0, '', 'Audios/historia1', NULL),
        (1, 'Pregunta', 1, 'Cual fue la causa principal de la tristeza en Pueblo Sonoro al inicio de la historia?', NULL, 'Los instrumentos musicales desaparecieron por completo'),
        (1, 'Pregunta', 2, 'Segun la profesora Tono-fonia, que es lo que ocurre cuando algo choca o se mueve rapidamente?', NULL, 'Se produce una vibracion'),
        (1, 'Pregunta', 3, 'Como viaja el sonido por el aire para llegar a nuestros oidos?', NULL, 'Como una ola invisible'),
        (1, 'Reto', 4, 'Construye una maraca', 'Imagenes/reto1', NULL);
        ";

        cmd.ExecuteNonQuery();
    }
}