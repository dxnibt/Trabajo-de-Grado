using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using Mono.Data.Sqlite;

using Dominio.entidades;
using Infraestructura.SQLite;
using Infraestructura.SQLite.SQLiteGateway;

public class ActivityController : MonoBehaviour
{
    [Header("UI GENERAL")]
    public TMP_Text tituloText;
    public TMP_Text contenidoText;
    public TMP_Text instruccionesText;
    public Button siguienteButton;
    public Button volverButton;
    public Button volverHistoriaButton;
    public Button salirButton;
    public Button finalizarRetoButton;

    [Header("PANELES")]
    public GameObject panelHistoria;
    public GameObject panelPreguntas;
    public GameObject panelReto;

    [Header("BOTONES OPCIONES")]
    public Button opcion1Button;
    public Button opcion2Button;
    public Button opcion3Button;

    [Header("TEXTOS OPCIONES")]
    public TMP_Text opcion1Text;
    public TMP_Text opcion2Text;
    public TMP_Text opcion3Text;

    [Header("RETO")]
    public Image retoImage;
    public RetoPanelController retoPanelController;

    [Header("RETROALIMENTACIÓN DE RESPUESTAS")]
    public Image imagenCorrecto;
    public Image imagenIncorrecto;
    public float tiempoMuestraResultado = 1.5f;

    [Header("AUDIO")]
    public AudioSource audioSource;

    private SQLiteActividadGateway actividadGateway;
    private SQLiteProgresoGateway progresoGateway;

    private Actividad actividad;
    private Progreso progreso;
    private Nivel nivel;

    private bool esperandoAudio = false;
    private bool audioYaProcesado = false;
    private float tiempoAudioInicio = 0f;
    private float duracionAudioActual = 0f;

    void Start()
    {
        Debug.Log("Iniciando ActivityController...");

        if (ActivityManager.ActividadActualId == 0)
            InferirContextoDesdEscena(SceneManager.GetActiveScene().name);

        nivel = new Nivel(ActivityManager.NivelActualId, ActivityManager.NivelNombre);

        string dbPath = Path.Combine(Application.persistentDataPath, "miBase.db");
        var conexion = new ConexionSQLite(dbPath);

        var inicializador = new InicializadorBD(conexion);
        inicializador.CrearTablas();

        // ✅ Verificar si la BD ya tiene datos ANTES de ejecutar seeds
        bool necesitaSembrar = false;
        using (var connCheck = conexion.CrearConexion())
        {
            connCheck.Open();
            necesitaSembrar = BaseDeDatosVacia(connCheck);
        }

        if (necesitaSembrar)
        {
            Debug.Log("========== BD VACÍA → EJECUTANDO SEEDS ==========");
            Debug.Log("[ActivityController] Iniciando SeedActividad...");
            var seed = new SeedActividad(conexion);
            seed.Ejecutar();
            Debug.Log("[ActivityController] SeedActividad completado ✓");

            Debug.Log("[ActivityController] Iniciando SeedPregunta...");
            var seedPregunta = new SeedPregunta(conexion);
            seedPregunta.Ejecutar();
            Debug.Log("[ActivityController] SeedPregunta completado ✓");
            Debug.Log("========== SEEDS EJECUTADOS EXITOSAMENTE ==========");
        }
        else
        {
            Debug.Log("========== BD YA TIENE DATOS → NO SE EJECUTAN SEEDS ==========");
        }

        actividadGateway = new SQLiteActividadGateway(conexion);
        progresoGateway = new SQLiteProgresoGateway(conexion);

        actividad = actividadGateway.ObtenerPorId(ActivityManager.ActividadActualId, nivel);

        // Fallback
        if (actividad == null)
        {
            Debug.LogWarning($"Actividad {ActivityManager.ActividadActualId} no encontrada. Usando actividad 1.");
            actividad = actividadGateway.ObtenerPorId(1, nivel);
        }

        OcultarTodo();
        siguienteButton?.gameObject.SetActive(false);
        volverButton?.gameObject.SetActive(false);
        volverHistoriaButton?.gameObject.SetActive(false);
        salirButton?.gameObject.SetActive(false);
        finalizarRetoButton?.gameObject.SetActive(false);

        if (actividad == null)
        {
            Debug.LogError("No se encontró la actividad");
            return;
        }

        Debug.Log("Contenidos cargados: " + actividad.Contenidos.Count);
        for (int i = 0; i < actividad.Contenidos.Count; i++)
        {
            var c = actividad.Contenidos[i];
            Debug.Log($"  [{i}] {c.GetType().Name} (Orden: {c.Orden})");
        }

        progreso = new Progreso();
        progreso.IniciarActividad(actividad);

        VerificarRaycastTarget(volverButton);
        VerificarRaycastTarget(volverHistoriaButton);
        VerificarRaycastTarget(siguienteButton);
        VerificarRaycastTarget(salirButton);
        VerificarRaycastTarget(finalizarRetoButton);

        siguienteButton.onClick.AddListener(SiguienteContenido);
        volverButton.onClick.AddListener(VolverAPanelHistoria);
        volverHistoriaButton.onClick.AddListener(VolverAMenuNiveles);
        salirButton.onClick.AddListener(VolverAMenuNiveles);
        finalizarRetoButton.onClick.AddListener(SiguienteContenido);

        opcion1Button.onClick.AddListener(() => SeleccionarRespuesta(opcion1Text.text));
        opcion2Button.onClick.AddListener(() => SeleccionarRespuesta(opcion2Text.text));
        opcion3Button.onClick.AddListener(() => SeleccionarRespuesta(opcion3Text.text));

        if (imagenCorrecto != null) imagenCorrecto.gameObject.SetActive(false);
        if (imagenIncorrecto != null) imagenIncorrecto.gameObject.SetActive(false);

        MostrarContenido();
    }

    // 🔥 Método mejorado: verifica si la BD está vacía
    bool BaseDeDatosVacia(SqliteConnection conn)
    {
        try
        {
            // Verificar si hay actividades
            var cmdAct = conn.CreateCommand();
            cmdAct.CommandText = "SELECT COUNT(*) FROM Actividad";
            long countAct = (long)cmdAct.ExecuteScalar();
            
            // Verificar si hay contenido
            var cmdCont = conn.CreateCommand();
            cmdCont.CommandText = "SELECT COUNT(*) FROM ContenidoActividad";
            long countCont = (long)cmdCont.ExecuteScalar();
            
            Debug.Log($"BD actual: Actividad={countAct}, ContenidoActividad={countCont}");
            
            // Si no hay actividades O no hay contenido, consideramos vacía
            return countAct == 0 || countCont == 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al verificar BD: {e.Message}");
            return true; // Si hay error, ejecutar seeds por seguridad
        }
    }

    void VerificarRaycastTarget(Button boton)
    {
        if (boton == null) return;
        Image img = boton.GetComponent<Image>();
        if (img != null && !img.raycastTarget)
        {
            img.raycastTarget = true;
        }
    }

    void Update()
    {
        if (!esperandoAudio || audioYaProcesado)
            return;

        if (!audioSource.isPlaying)
        {
            Debug.Log("[ActivityController] ✓ Audio terminó (isPlaying=false), avanzando");
            ProcesarFinAudio();
            return;
        }

        float tiempoTranscurrido = Time.time - tiempoAudioInicio;
        if (tiempoTranscurrido >= duracionAudioActual && duracionAudioActual > 0)
        {
            Debug.Log($"[ActivityController] ✓ Audio terminó (tiempo: {tiempoTranscurrido:F2}s >= {duracionAudioActual:F2}s), avanzando");
            ProcesarFinAudio();
        }
    }

    private void ProcesarFinAudio()
    {
        audioYaProcesado = true;
        esperandoAudio = false;

        CancelInvoke(nameof(SiguienteContenido));
        Invoke(nameof(SiguienteContenido), 0.2f);
    }

    void MostrarContenido()
    {
        var contenido = progreso.ObtenerActual();
        if (contenido == null)
        {
            Debug.LogWarning("[ActivityController] No hay más contenidos");
            return;
        }

        Debug.Log($"[ActivityController] Mostrando: {contenido.GetType().Name} (Orden: {contenido.Orden})");

        CancelInvoke(nameof(SiguienteContenido));
        OcultarTodo();

        esperandoAudio = false;
        audioYaProcesado = false;

        if (contenido is Historia historia)
        {
            panelHistoria.SetActive(true);

            tituloText.text = "Historia";
            contenidoText.text = "Escucha la narración";

            AudioClip clip = Resources.Load<AudioClip>(historia.Recurso);

            if (clip == null && audioSource.clip != null)
            {
                clip = audioSource.clip;
                Debug.Log("[ActivityController] ⚠ AudioClip no encontrado en Resources, usando el asignado en Inspector");
            }

            if (clip != null)
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();

                duracionAudioActual = clip.length;
                tiempoAudioInicio = Time.time;
                esperandoAudio = true;
                audioYaProcesado = false;

                Debug.Log($"[ActivityController] ▶ Audio iniciado: {clip.name} ({duracionAudioActual:F2}s)");
                siguienteButton.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("[ActivityController] ✗ No hay AudioClip disponible");
                siguienteButton.gameObject.SetActive(true);
                siguienteButton.interactable = true;
            }

            volverHistoriaButton.gameObject.SetActive(true);
        }
        else if (contenido is Pregunta pregunta)
        {
            panelPreguntas.SetActive(true);

            tituloText.text = "Pregunta";
            contenidoText.text = pregunta.Enunciado;

            MostrarOpciones(pregunta.Opciones);

            siguienteButton.gameObject.SetActive(true);
            siguienteButton.interactable = false;
            volverButton.gameObject.SetActive(true);
        }
        else if (contenido is Reto reto)
        {
            panelReto.SetActive(true);

            tituloText.text = "Reto";
            contenidoText.text = reto.Texto;

            finalizarRetoButton.gameObject.SetActive(true);
            finalizarRetoButton.interactable = true;

            if (retoPanelController != null)
            {
                retoPanelController.InicializarConReto(reto);
            }
        }
    }

    void MostrarOpciones(List<string> opciones)
    {
        opcion1Text.text = opciones.Count > 0 ? opciones[0] : "";
        opcion2Text.text = opciones.Count > 1 ? opciones[1] : "";
        opcion3Text.text = opciones.Count > 2 ? opciones[2] : "";
    }

    void SeleccionarRespuesta(string respuesta)
    {
        Debug.Log($"[ActivityController] Respuesta seleccionada: {respuesta}");
        var contenido = progreso.ObtenerActual();

        if (contenido is Pregunta pregunta)
        {
            bool correcta = pregunta.ValidarRespuesta(respuesta);
            Debug.Log($"[ActivityController] ¿Correcta? {(correcta ? "✓ SÍ" : "✗ NO")}");
            Debug.Log($"[ActivityController] Respuesta esperada: {pregunta.RespuestaCorrecta}");

            if (correcta)
            {
                progreso.MarcarRespuestaCorrecta();
                siguienteButton.interactable = true;
                Debug.Log("[ActivityController] ✓ Botón Siguiente activado");
                MostrarResultado(true);
            }
            else
            {
                Debug.Log("[ActivityController] ✗ Respuesta incorrecta");
                MostrarResultado(false);
            }
        }
    }

    void MostrarResultado(bool correcto)
    {
        Image img = correcto ? imagenCorrecto : imagenIncorrecto;
        if (img == null) return;

        img.gameObject.SetActive(true);
        Invoke(nameof(OcultarResultado), tiempoMuestraResultado);
    }

    void OcultarResultado()
    {
        if (imagenCorrecto != null) imagenCorrecto.gameObject.SetActive(false);
        if (imagenIncorrecto != null) imagenIncorrecto.gameObject.SetActive(false);
    }

    void SiguienteContenido()
    {
        Debug.Log("[ActivityController] Botón Siguiente presionado");
        Debug.Log($"[ActivityController] Índice actual: {progreso.IndiceContenido}");
        Debug.Log($"[ActivityController] Total contenidos: {progreso.ObtenerTotalActividades()}");

        if (progreso.Avanzar())
        {
            Debug.Log("[ActivityController] ✓ Avanzando a siguiente contenido");
            progresoGateway.Guardar(progreso);
            MostrarContenido();
        }
        else
        {
            Debug.LogWarning("[ActivityController] ✗ No se pudo avanzar - mostrando pantalla de fin");
            MostrarBotonSalir();
        }
    }

    void OcultarTodo()
    {
        panelHistoria.SetActive(false);
        panelPreguntas.SetActive(false);
        panelReto.SetActive(false);
    }

    void VolverAPanelHistoria()
    {
        while (progreso.IndiceContenido > 0)
        {
            progreso.Retroceder();
            if (progreso.ObtenerActual() is Historia) break;
        }
        MostrarContenido();
    }

    void VolverAMenuNiveles()
    {
        SceneManager.LoadScene(ActivityManager.EscenaMenuNivel);
    }

    void MostrarBotonSalir()
    {
        tituloText.text = "¡ACTIVIDAD COMPLETADA!";
        contenidoText.text = "Presiona SALIR";
        salirButton.gameObject.SetActive(true);
    }

    private void InferirContextoDesdEscena(string nombreEscena)
    {
        try
        {
            int sep = nombreEscena.IndexOf('_');
            int nivel = int.Parse(nombreEscena.Substring(1, sep - 1));
            int act = int.Parse(nombreEscena.Substring(sep + 2));

            ActivityManager.NivelActualId = nivel;
            ActivityManager.ActividadActualId = act + (nivel - 1) * 10;
        }
        catch { }
    }
}