using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using Infraestructura.SQLite;
using Infraestructura.SQLite.SQLiteGateway;

public class DocenteController : MonoBehaviour
{
    private const string USUARIO_DOCENTE = "docente";
    private const string CONTRASENA_DOCENTE = "1234";
    private const int TOTAL_ACTIVIDADES = 20;

    [Header("Panel: Login")]
    public GameObject panelLogin;
    public TMP_InputField campoUsuario;
    public TMP_InputField campoContrasena;
    public Button botonIngresar;
    public TMP_Text textoErrorLogin;

    [Header("Panel: Dashboard")]
    public GameObject panelDashboard;
    public Transform contenedorLista;
    public GameObject prefabEntradaEstudiante;
    public Button botonCerrarSesion;
    public Button botonVolver;

    private SQLiteEstudianteGateway estudianteGateway;
    private SQLiteActividadCompletadaGateway completadaGateway;

    void Start()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "miBase.db");
        var conexion = new ConexionSQLite(dbPath);
        new InicializadorBD(conexion).CrearTablas();

        estudianteGateway = new SQLiteEstudianteGateway(conexion);
        completadaGateway = new SQLiteActividadCompletadaGateway(conexion);

        panelDashboard.SetActive(false);
        panelLogin.SetActive(true);
        if (textoErrorLogin != null) textoErrorLogin.gameObject.SetActive(false);

        botonIngresar.onClick.AddListener(Ingresar);
        botonCerrarSesion?.onClick.AddListener(CerrarSesion);
        botonVolver?.onClick.AddListener(() => SceneManager.LoadScene("mp"));
    }

    void Ingresar()
    {
        string usuario = campoUsuario != null ? campoUsuario.text.Trim() : "";
        string contrasena = campoContrasena != null ? campoContrasena.text : "";

        if (usuario == USUARIO_DOCENTE && contrasena == CONTRASENA_DOCENTE)
        {
            panelLogin.SetActive(false);
            panelDashboard.SetActive(true);
            CargarDashboard();
        }
        else
        {
            if (textoErrorLogin != null)
            {
                textoErrorLogin.text = "Usuario o contraseña incorrectos.";
                textoErrorLogin.gameObject.SetActive(true);
            }
        }
    }

    void CargarDashboard()
    {
        foreach (Transform hijo in contenedorLista)
            Destroy(hijo.gameObject);

        var estudiantes = estudianteGateway.ObtenerTodos();

        foreach (var est in estudiantes)
        {
            int completadas = completadaGateway.ContarPorEstudiante(est.Id);
            float porcentaje = TOTAL_ACTIVIDADES > 0 ? (float)completadas / TOTAL_ACTIVIDADES : 0f;

            var entrada = Instantiate(prefabEntradaEstudiante, contenedorLista);
            var ui = entrada.GetComponent<EntradaEstudianteUI>();
            if (ui != null)
                ui.Configurar(est.Nombre, est.EsGrupo, completadas, TOTAL_ACTIVIDADES, porcentaje);
        }
    }

    void CerrarSesion()
    {
        panelDashboard.SetActive(false);
        panelLogin.SetActive(true);
        if (campoUsuario != null) campoUsuario.text = "";
        if (campoContrasena != null) campoContrasena.text = "";
        if (textoErrorLogin != null) textoErrorLogin.gameObject.SetActive(false);
    }
}
