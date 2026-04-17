using Mono.Data.Sqlite;

namespace Infraestructura.SQLite{

public class ConexionSQLite
{
    private readonly string connectionString;

    public ConexionSQLite(string dbPath)
    {
        connectionString = $"Data Source={dbPath}";
    }

    public SqliteConnection CrearConexion()
    {
        return new SqliteConnection(connectionString);
    }
}
}