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

    [Header("Panel: Login")]
    public GameObject panelLogin;
    public TMP_InputField campoUsuario;
    public TMP_InputField campoContrasena;
    public Button botonIngresar;
    public TMP_Text textoErrorLogin;

    [Header("Panel: Principal (post-login)")]
    public GameObject panelPrincipal;
    public Button botonVerProgreso;
    public Button botonVerRecursos;
    public Button botonCerrarSesion;

    [Header("Panel: Progreso estudiantes")]
    public GameObject panelProgreso;
    public Transform contenedorLista;
    public GameObject prefabEntradaEstudiante;
    public Button botonVolverDesdeProgreso;

    [Header("Panel: Recursos (seleccion de nivel)")]
    public GameObject panelRecursos;
    public Button botonNivel1Recursos;
    public Button botonNivel2Recursos;
    public Button botonTTJRecursos;
    public Button botonVolverDesdeRecursos;

    private SQLiteEstudianteGateway estudianteGateway;
    private SQLiteRespuestaGateway respuestaGateway;

    void Start()
    {
        ActivityManager.ModoDocente = false; // Limpiar flag al entrar a la escena docente

        string dbPath = Path.Combine(Application.persistentDataPath, "miBase.db");
        var conexion = new ConexionSQLite(dbPath);
        new InicializadorBD(conexion).CrearTablas();

        estudianteGateway = new SQLiteEstudianteGateway(conexion);
        respuestaGateway = new SQLiteRespuestaGateway(conexion);

        // Verificar si hay sesion previa de docente
        bool sesionDocente = PlayerPrefs.GetInt("SesionDocente", 0) == 1;
        if (sesionDocente)
        {
            MostrarSoloPanel(panelPrincipal);
        }
        else
        {
            MostrarSoloPanel(panelLogin);
        }

        if (textoErrorLogin != null) textoErrorLogin.gameObject.SetActive(false);

        botonIngresar.onClick.AddListener(Ingresar);
        botonCerrarSesion?.onClick.AddListener(CerrarSesion);

        botonVerProgreso?.onClick.AddListener(AbrirProgreso);
        botonVerRecursos?.onClick.AddListener(AbrirRecursos);
        botonVolverDesdeProgreso?.onClick.AddListener(() => MostrarSoloPanel(panelPrincipal));
        botonVolverDesdeRecursos?.onClick.AddListener(() => MostrarSoloPanel(panelPrincipal));

        botonNivel1Recursos?.onClick.AddListener(() => IrANivelRecursos("mp_nivel1"));
        botonNivel2Recursos?.onClick.AddListener(() => IrANivelRecursos("mp_nivel2"));
        botonTTJRecursos?.onClick.AddListener(() => IrANivelRecursos("mp_ttj"));
    }

    void Ingresar()
    {
        string usuario = campoUsuario != null ? campoUsuario.text.Trim() : "";
        string contrasena = campoContrasena != null ? campoContrasena.text : "";

        if (usuario == USUARIO_DOCENTE && contrasena == CONTRASENA_DOCENTE)
        {
            PlayerPrefs.SetInt("SesionDocente", 1);
            PlayerPrefs.Save();
            MostrarSoloPanel(panelPrincipal);
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

    void CerrarSesion()
    {
        PlayerPrefs.SetInt("SesionDocente", 0);
        PlayerPrefs.Save();
        if (campoUsuario != null) campoUsuario.text = "";
        if (campoContrasena != null) campoContrasena.text = "";
        if (textoErrorLogin != null) textoErrorLogin.gameObject.SetActive(false);
        MostrarSoloPanel(panelLogin);
    }

    void AbrirProgreso()
    {
        MostrarSoloPanel(panelProgreso);
        CargarListaEstudiantes();
    }

    void CargarListaEstudiantes()
    {
        if (contenedorLista == null)
        {
            Debug.LogError("[Docente] contenedorLista NO está asignado en el Inspector.");
            return;
        }
        if (prefabEntradaEstudiante == null)
        {
            Debug.LogError("[Docente] prefabEntradaEstudiante NO está asignado en el Inspector.");
            return;
        }

        foreach (Transform hijo in contenedorLista)
            Destroy(hijo.gameObject);

        var estudiantes = estudianteGateway.ObtenerTodos();
        Debug.Log($"[Docente] Estudiantes encontrados en BD: {estudiantes.Count}");

        foreach (var est in estudiantes)
        {
            try
            {
                var resumen = respuestaGateway.ObtenerResumenPorEstudiante(est.Id);
                Debug.Log($"[Docente] Estudiante '{est.Nombre}' — total:{resumen.Total} incorrectos:{resumen.Incorrectos}");

                var entrada = Instantiate(prefabEntradaEstudiante, contenedorLista);
                var ui = entrada.GetComponent<EntradaEstudianteUI>();
                if (ui != null)
                    ui.Configurar(est.Nombre, est.EsGrupo, resumen.Total, resumen.Incorrectos);
                else
                    Debug.LogError($"[Docente] El prefab '{prefabEntradaEstudiante.name}' no tiene el componente EntradaEstudianteUI.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Docente] Error procesando estudiante '{est.Nombre}': {ex.Message}");
            }
        }

        // Forzar recálculo del layout para que los items sean visibles en el ScrollView
        Canvas.ForceUpdateCanvases();
        var rt = contenedorLista.GetComponent<RectTransform>();
        if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    void AbrirRecursos()
    {
        MostrarSoloPanel(panelRecursos);
    }

    void IrANivelRecursos(string escenaMenu)
    {
        ActivityManager.ModoDocente = true;
        SceneManager.LoadScene(escenaMenu);
    }

    void MostrarSoloPanel(GameObject panelActivo)
    {
        if (panelLogin != null) panelLogin.SetActive(panelActivo == panelLogin);
        if (panelPrincipal != null) panelPrincipal.SetActive(panelActivo == panelPrincipal);
        if (panelProgreso != null) panelProgreso.SetActive(panelActivo == panelProgreso);
        if (panelRecursos != null) panelRecursos.SetActive(panelActivo == panelRecursos);
    }
}
