using Mono.Data.Sqlite;

namespace Infraestructura.SQLite{

public class ConexionSQLite
{
    private readonly string connectionString;

    public ConexionSQLite(string dbPath)
    {
        connectionString = $"Data Source={dbPath};UTF8Encoding=True";
    }

    public SqliteConnection CrearConexion()
    {
        return new SqliteConnection(connectionString);
    }
}
}