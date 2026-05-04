using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Infraestructura.SQLite;
using Infraestructura.SQLite.SQLiteGateway;

public class EstudianteMenuController : MonoBehaviour
{
    [Header("Panel: Selección de modo")]
    public GameObject panelSeleccion;
    public Button botonIndividual;
    public Button botonGrupo;

    [Header("Panel: Ingreso de nombre")]
    public GameObject panelNombre;
    public TMP_Text labelNombre;
    public TMP_InputField campoNombre;
    public Button botonContinuar;
    public TMP_Text textoError;

    private bool esGrupo;
    private SQLiteEstudianteGateway estudianteGateway;

    void Start()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "miBase.db");
        var conexion = new ConexionSQLite(dbPath);
        new InicializadorBD(conexion).CrearTablas();
        estudianteGateway = new SQLiteEstudianteGateway(conexion);

        // Si hay un ActivityController en la escena, él gestiona la sesión completa
        if (FindObjectOfType<ActivityController>() != null)
        {
            if (panelSeleccion != null) panelSeleccion.SetActive(false);
            if (panelNombre != null) panelNombre.SetActive(false);
            return;
        }

        // Si ya hay sesión activa en esta ejecución, ocultar overlay directamente
        if (ActivityManager.EstudianteId > 0)
        {
            panelSeleccion.SetActive(false);
            panelNombre.SetActive(false);
            return;
        }

        panelSeleccion.SetActive(true);
        panelNombre.SetActive(false);

        botonIndividual.onClick.AddListener(() => AbrirPanelNombre(false));
        botonGrupo.onClick.AddListener(() => AbrirPanelNombre(true));
        botonContinuar.onClick.AddListener(Continuar);
    }

    void AbrirPanelNombre(bool grupo)
    {
        esGrupo = grupo;
        panelSeleccion.SetActive(false);
        panelNombre.SetActive(true);
        campoNombre.text = "";
        if (textoError != null) textoError.gameObject.SetActive(false);
        if (labelNombre != null)
            labelNombre.text = grupo ? "Nombre del grupo:" : "Tu nombre:";
    }

    void Continuar()
    {
        string nombre = campoNombre.text.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            MostrarError("Por favor ingresa un nombre.");
            return;
        }

        var estudiante = estudianteGateway.ObtenerOCrearPorNombre(nombre, esGrupo);
        ActivityManager.EstudianteId = estudiante.Id;
        ActivityManager.EstudianteNombre = estudiante.Nombre;
        ActivityManager.EsGrupo = esGrupo;
        ActivityManager.TipoConfirmado = true;

        PlayerPrefs.SetString("UltimoEstudiante", nombre);
        PlayerPrefs.SetInt("UltimoEsGrupo", esGrupo ? 1 : 0);
        PlayerPrefs.SetInt("TipoConfirmado", 1);
        PlayerPrefs.Save();

        panelNombre.SetActive(false);
    }

    void MostrarError(string mensaje)
    {
        if (textoError == null) return;
        textoError.text = mensaje;
        textoError.gameObject.SetActive(true);
    }
}
