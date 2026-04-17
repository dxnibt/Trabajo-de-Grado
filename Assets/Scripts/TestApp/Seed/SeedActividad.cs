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

        // 🔥 Limpiar datos viejos primero
        cmd.CommandText = @"
        DELETE FROM ContenidoActividad WHERE ActividadId = 1;
        DELETE FROM Actividad WHERE Id = 1;
        ";
        cmd.ExecuteNonQuery();

        // ACTIVIDAD
        cmd.CommandText = @"
        INSERT INTO Actividad (Id, Titulo, NivelId)
        VALUES (1, 'El Sonido', 1);
        ";
        cmd.ExecuteNonQuery();

        // CONTENIDO
        cmd.CommandText = @"
        INSERT INTO ContenidoActividad
        (ActividadId, Tipo, Orden, Texto, Recurso, Opciones, RespuestaCorrecta, Instrucciones)
        VALUES 

        -- HISTORIA
        (1, 'Historia', 0, '', 'Audios/historia1', NULL, NULL, NULL),

        -- PREGUNTA 1
        (1, 'Pregunta', 1,
        'Cual fue la causa principal de la tristeza en Pueblo Sonoro al inicio de la historia?',
        NULL,
        'Se prohibió el gran festival de música reciclada|Los pájaros dejaron de silbar sus melodías|Los instrumentos musicales desaparecieron por completo|Doña vibración decidió que no habrían más fiestas',
        'Los instrumentos musicales desaparecieron por completo', NULL),

        -- PREGUNTA 2
        (1, 'Pregunta', 2,
        'Segun la profesora Tono-fonia, que es lo que ocurre cuando algo choca o se mueve rapidamente?',
        NULL,
        'Se produce una vibración|El objeto cambia de color|El sonido desaparece instantáneamente|Se crea un vacío en el aire',
        'Se produce una vibración', NULL),

        -- PREGUNTA 3
        (1, 'Pregunta', 3,
        'Como viaja el sonido por el aire para llegar a nuestros oidos?',
        NULL,
        'A través de hilos musicales ocultos|Únicamente a través de tubos de cartón|Como una línea recta de luz|Como una ola invisible',
        'Como una ola invisible', NULL),

        -- RETO FINAL
        (1, 'Reto', 4,
        'Construye una maraca',
        'Imagenes/reto1',
        NULL,
        NULL,
        'Paso 1: Consigue una botella de plástico vacía y dos cucharas de palo
Paso 2: Llena la botella a mitad con arroz o granos de frijol
Paso 3: Asegura las cucharas a los lados de la botella con cinta adhesiva
Paso 4: ¡Ahora tienes tu propia maraca! Pruébala haciendo ruido');
        ";

        cmd.ExecuteNonQuery();
    }
}