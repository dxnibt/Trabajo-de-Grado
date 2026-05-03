namespace Infraestructura.SQLite.SQLiteGateway
{
    public class SQLiteActividadCompletadaGateway
    {
        private readonly ConexionSQLite conexion;

        public SQLiteActividadCompletadaGateway(ConexionSQLite conexion)
        {
            this.conexion = conexion;
        }

        public void Guardar(int estudianteId, int actividadId)
        {
            using var conn = conexion.CrearConexion();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT OR IGNORE INTO ActividadCompletada (EstudianteId, ActividadId, Fecha)
                VALUES ($e, $a, datetime('now'));
            ";
            cmd.Parameters.AddWithValue("$e", estudianteId);
            cmd.Parameters.AddWithValue("$a", actividadId);
            cmd.ExecuteNonQuery();
        }

        public int ContarPorEstudiante(int estudianteId)
        {
            using var conn = conexion.CrearConexion();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM ActividadCompletada WHERE EstudianteId = $e;";
            cmd.Parameters.AddWithValue("$e", estudianteId);

            return (int)(long)cmd.ExecuteScalar();
        }
    }
}
