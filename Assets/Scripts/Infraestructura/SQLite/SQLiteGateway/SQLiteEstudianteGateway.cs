using System.Collections.Generic;
using Dominio.entidades;

namespace Infraestructura.SQLite.SQLiteGateway
{
    public class SQLiteEstudianteGateway
    {
        private readonly ConexionSQLite conexion;

        public SQLiteEstudianteGateway(ConexionSQLite conexion)
        {
            this.conexion = conexion;
        }

        public Estudiante ObtenerOCrearPorNombre(string nombre, bool esGrupo)
        {
            using var conn = conexion.CrearConexion();
            conn.Open();

            var cmdSelect = conn.CreateCommand();
            cmdSelect.CommandText = "SELECT Id FROM Estudiante WHERE Nombre = $n LIMIT 1;";
            cmdSelect.Parameters.AddWithValue("$n", nombre);

            using (var reader = cmdSelect.ExecuteReader())
            {
                if (reader.Read())
                    return new Estudiante(reader.GetInt32(0), nombre, null);
            }

            var cmdInsert = conn.CreateCommand();
            cmdInsert.CommandText = @"
                INSERT INTO Estudiante (Nombre, EsGrupo) VALUES ($n, $g);
                SELECT last_insert_rowid();
            ";
            cmdInsert.Parameters.AddWithValue("$n", nombre);
            cmdInsert.Parameters.AddWithValue("$g", esGrupo ? 1 : 0);

            long newId = (long)cmdInsert.ExecuteScalar();
            return new Estudiante((int)newId, nombre, null);
        }

        public List<EstudianteResumen> ObtenerTodos()
        {
            var lista = new List<EstudianteResumen>();

            using var conn = conexion.CrearConexion();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Nombre, EsGrupo FROM Estudiante ORDER BY Nombre;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(new EstudianteResumen(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2) == 1));

            return lista;
        }
    }

    public class EstudianteResumen
    {
        public int Id { get; }
        public string Nombre { get; }
        public bool EsGrupo { get; }

        public EstudianteResumen(int id, string nombre, bool esGrupo)
        {
            Id = id;
            Nombre = nombre;
            EsGrupo = esGrupo;
        }
    }
}
