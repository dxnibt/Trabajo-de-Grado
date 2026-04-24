using UnityEngine;
using Infraestructura.SQLite;
using System.IO;

public class InicializadorManager : MonoBehaviour
{
    void Start()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "miBase.db");

        var conexion = new ConexionSQLite(dbPath);

        var inicializador = new InicializadorBD(conexion);
        inicializador.CrearTablas();

        var seed = new SeedActividad(conexion);
        seed.Ejecutar();

        Debug.Log("Base de datos inicializada con datos");
    }
}