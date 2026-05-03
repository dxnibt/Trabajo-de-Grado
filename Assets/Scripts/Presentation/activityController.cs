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
    public Slider barraProgreso;
    public Image tituloImage;
    public Image preguntaImage;
    public Image retoImage;
    public TMP_Text contenidoText;
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
    public RetoPanelController retoPanelController;

    [Header("RETROALIMENTACIÓN DE RESPUESTAS")]
    public Image imagenCorrecto;
    public Image imagenIncorrecto;
    public float tiempoMuestraResultado = 1.5f;

    [Header("AUDIO")]
    public AudioSource audioSource;

    private SQLiteActividadGateway actividadGateway;
    private SQLiteProgresoGateway progresoGateway;
    private SQLiteActividadCompletadaGateway completadaGateway;

    private Actividad actividad;
    private Progreso progreso;
    private Nivel nivel;

    private bool esperandoAudio = false;
    private bool audioYaProcesado = false;
    private float tiempoAudioInicio = 0f;
    private float duracionAudioActual = 0f;
    private bool ultimaRespuestaCorrecta = false;
    private bool mostrandoResultado = false;

    void Start()
    {
        if (ActivityManager.ActividadActualId == 0)
            InferirContextoDesdEscena(SceneManager.GetActiveScene().name);

        nivel = new Nivel(ActivityManager.NivelActualId, ActivityManager.NivelNombre);

        string dbPath = Path.Combine(Application.persistentDataPath, "miBase.db");
        var conexion = new ConexionSQLite(dbPath);

        var inicializador = new InicializadorBD(conexion);
        inicializador.CrearTablas();

        using (var connCheck = conexion.CrearConexion())
        {
            connCheck.Open();
            if (BaseDeDatosVacia(connCheck))
            {
                Debug.Log("[ActivityController] BD sin datos completos → ejecutando seeds");
                new SeedActividad(conexion).Ejecutar();
                new SeedPregunta(conexion).Ejecutar();
            }
        }

        actividadGateway = new SQLiteActividadGateway(conexion);
        progresoGateway = new SQLiteProgresoGateway(conexion);
        completadaGateway = new SQLiteActividadCompletadaGateway(conexion);

        actividad = actividadGateway.ObtenerPorId(ActivityManager.ActividadActualId, nivel);

        if (actividad == null)
        {
            Debug.LogWarning($"[ActivityController] Actividad {ActivityManager.ActividadActualId} no encontrada. Usando actividad 1.");
            actividad = actividadGateway.ObtenerPorId(1, nivel);
        }

        if (actividad == null)
        {
            Debug.LogError("[ActivityController] No se encontró la actividad");
            return;
        }

        OcultarTodo();
        siguienteButton?.gameObject.SetActive(false);
        volverButton?.gameObject.SetActive(false);
        volverHistoriaButton?.gameObject.SetActive(false);
        salirButton?.gameObject.SetActive(false);
        finalizarRetoButton?.gameObject.SetActive(false);

        progreso = new Progreso();
        progreso.EstudianteId = ActivityManager.EstudianteId;
        progreso.IniciarActividad(actividad);

        if (barraProgreso != null)
        {
            barraProgreso.minValue = 0f;
            barraProgreso.maxValue = 1f;
            barraProgreso.value = 0f;
            barraProgreso.interactable = false;
        }

        VerificarRaycastTarget(volverButton);
        VerificarRaycastTarget(volverHistoriaButton);
        VerificarRaycastTarget(siguienteButton);
        VerificarRaycastTarget(salirButton);
        VerificarRaycastTarget(finalizarRetoButton);

        siguienteButton?.onClick.AddListener(SiguienteContenido);
        volverButton?.onClick.AddListener(VolverAPanelHistoria);
        volverHistoriaButton?.onClick.AddListener(VolverAMenuNiveles);
        salirButton?.onClick.AddListener(VolverAMenuNiveles);

        opcion1Button?.onClick.AddListener(() => SeleccionarRespuesta(opcion1Text.text));
        opcion2Button?.onClick.AddListener(() => SeleccionarRespuesta(opcion2Text.text));
        opcion3Button?.onClick.AddListener(() => SeleccionarRespuesta(opcion3Text.text));

        if (imagenCorrecto != null) imagenCorrecto.gameObject.SetActive(false);
        if (imagenIncorrecto != null) imagenIncorrecto.gameObject.SetActive(false);

        MostrarContenido();
    }

    // Re-siembra si no hay registros de Historia (BD vacía o sembrada con versión anterior)
    bool BaseDeDatosVacia(SqliteConnection conn)
    {
        try
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM ContenidoActividad WHERE Tipo = 'Historia'";
            long count = (long)cmd.ExecuteScalar();
            return count == 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ActivityController] Error al verificar BD: {e.Message}");
            return true;
        }
    }

    void VerificarRaycastTarget(Button boton)
    {
        if (boton == null) return;
        Image img = boton.GetComponent<Image>();
        if (img != null && !img.raycastTarget)
            img.raycastTarget = true;
    }

    void Update()
    {
        if (!esperandoAudio || audioYaProcesado) return;

        if (!audioSource.isPlaying)
        {
            ProcesarFinAudio();
            return;
        }

        float tiempoTranscurrido = Time.time - tiempoAudioInicio;
        if (tiempoTranscurrido >= duracionAudioActual && duracionAudioActual > 0)
            ProcesarFinAudio();
    }

    private void ProcesarFinAudio()
    {
        audioYaProcesado = true;
        esperandoAudio = false;
        CancelInvoke(nameof(SiguienteContenido));
        Invoke(nameof(SiguienteContenido), 0.2f);
    }

    void ActualizarBarraProgreso()
    {
        if (barraProgreso == null || actividad == null) return;
        int total = actividad.TotalContenidos;
        barraProgreso.value = total > 1 ? (float)progreso.IndiceContenido / (total - 1) : 1f;
    }

    void MostrarContenido()
    {
        ActualizarBarraProgreso();
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

            if (tituloImage != null)
            {
                Sprite spriteHistoria = Resources.Load<Sprite>("Titulos/historia_titulo");
                if (spriteHistoria != null)
                    tituloImage.sprite = spriteHistoria;
                else
                    tituloImage.sprite = null;
            }

            if (contenidoText != null) contenidoText.text = "Escucha la narración";

            AudioClip clip = Resources.Load<AudioClip>(historia.Recurso);

            if (clip == null && audioSource.clip != null)
                clip = audioSource.clip;

            if (clip != null)
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();

                duracionAudioActual = clip.length;
                tiempoAudioInicio = Time.time;
                esperandoAudio = true;
                audioYaProcesado = false;

                Debug.Log($"[ActivityController] Audio iniciado: {clip.name} ({duracionAudioActual:F2}s)");
                siguienteButton?.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[ActivityController] No hay AudioClip disponible para esta actividad");
                siguienteButton?.gameObject.SetActive(true);
                if (siguienteButton != null) siguienteButton.interactable = true;
            }

            volverHistoriaButton.gameObject.SetActive(true);
        }
        else if (contenido is Pregunta pregunta)
        {
            panelPreguntas.SetActive(true);
            preguntaImage?.gameObject.SetActive(true);

            if (tituloImage != null)
            {
                Sprite spritePregunta = Resources.Load<Sprite>("Titulos/pregunta_titulo");
                if (spritePregunta != null)
                    tituloImage.sprite = spritePregunta;
                else
                    tituloImage.sprite = null;
            }

            if (contenidoText != null) contenidoText.text = pregunta.Enunciado;

            MostrarOpciones(pregunta.Opciones);
            HabilitarBotonesOpciones(true);

            volverButton?.gameObject.SetActive(true);
            mostrandoResultado = false;
        }
        else if (contenido is Reto reto)
        {
            panelReto.SetActive(true);
            retoImage?.gameObject.SetActive(true);

            if (tituloImage != null)
            {
                Sprite spriteReto = Resources.Load<Sprite>("Titulos/reto_titulo");
                if (spriteReto != null)
                    tituloImage.sprite = spriteReto;
                else
                    tituloImage.sprite = null;
            }

            if (contenidoText != null) contenidoText.text = reto.Texto;

            finalizarRetoButton?.gameObject.SetActive(false);
            volverHistoriaButton?.gameObject.SetActive(true);

            if (retoPanelController != null)
            {
                retoPanelController.InicializarConReto(reto);
                retoPanelController.OnRetoFinalizado += ManejarRetoFinalizado;
            }
        }
    }

    void MostrarOpciones(List<string> opciones)
    {
        opcion1Text.text = opciones.Count > 0 ? opciones[0] : "";
        opcion2Text.text = opciones.Count > 1 ? opciones[1] : "";
        opcion3Text.text = opciones.Count > 2 ? opciones[2] : "";
    }

    void HabilitarBotonesOpciones(bool habilitar)
    {
        if (opcion1Button != null) opcion1Button.interactable = habilitar;
        if (opcion2Button != null) opcion2Button.interactable = habilitar;
        if (opcion3Button != null) opcion3Button.interactable = habilitar;
    }

    void SeleccionarRespuesta(string respuesta)
    {
        if (mostrandoResultado) return;

        var contenido = progreso.ObtenerActual();

        if (contenido is Pregunta pregunta)
        {
            bool correcta = pregunta.ValidarRespuesta(respuesta);
            ultimaRespuestaCorrecta = correcta;
            mostrandoResultado = true;

            HabilitarBotonesOpciones(false);

            if (correcta)
            {
                progreso.MarcarRespuestaCorrecta();
            }

            MostrarResultado(correcta);
        }
    }

    void MostrarResultado(bool correcto)
    {
        Image img = correcto ? imagenCorrecto : imagenIncorrecto;
        if (img == null) return;
        img.gameObject.SetActive(true);
        Invoke(nameof(OcultarResultado), 1f);
    }

    void OcultarResultado()
    {
        if (imagenCorrecto != null) imagenCorrecto.gameObject.SetActive(false);
        if (imagenIncorrecto != null) imagenIncorrecto.gameObject.SetActive(false);

        mostrandoResultado = false;

        if (ultimaRespuestaCorrecta)
        {
            SiguienteContenido();
        }
        else
        {
            HabilitarBotonesOpciones(true);
        }
    }

    void SiguienteContenido()
    {
        if (progreso.Avanzar())
        {
            progresoGateway.Guardar(progreso);
            MostrarContenido();
        }
        else
        {
            MostrarBotonSalir();
        }
    }

    void OcultarTodo()
    {
        panelHistoria.SetActive(false);
        panelPreguntas.SetActive(false);
        panelReto.SetActive(false);

        preguntaImage?.gameObject.SetActive(false);
        retoImage?.gameObject.SetActive(false);

        siguienteButton?.gameObject.SetActive(false);
        volverButton?.gameObject.SetActive(false);
        volverHistoriaButton?.gameObject.SetActive(false);
        salirButton?.gameObject.SetActive(false);
        finalizarRetoButton?.gameObject.SetActive(false);
    }

    void ManejarRetoFinalizado()
    {
        if (retoPanelController != null)
        {
            retoPanelController.OnRetoFinalizado -= ManejarRetoFinalizado;
            retoPanelController.OcultarPanel();
        }

        if (barraProgreso != null) barraProgreso.value = 1f;

        if (ActivityManager.EstudianteId > 0)
            completadaGateway.Guardar(ActivityManager.EstudianteId, ActivityManager.ActividadActualId);

        VolverAMenuNiveles();
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
        contenidoText.text = "Presiona SALIR";
        salirButton?.gameObject.SetActive(true);
    }

    private void InferirContextoDesdEscena(string nombreEscena)
    {
        try
        {
            int sep = nombreEscena.IndexOf('_');
            int nivelNum = int.Parse(nombreEscena.Substring(1, sep - 1));
            int act = int.Parse(nombreEscena.Substring(sep + 2));

            ActivityManager.NivelActualId = nivelNum;
            ActivityManager.ActividadActualId = act + (nivelNum - 1) * 10;

            // Establecer la escena del menú según el nivel
            if (nivelNum == 1)
                ActivityManager.EscenaMenuNivel = "mp_nivel1";
            else if (nivelNum == 2)
                ActivityManager.EscenaMenuNivel = "mp_nivel2";
            else if (nivelNum == 3)
                ActivityManager.EscenaMenuNivel = "mp_ttj";
            else
                ActivityManager.EscenaMenuNivel = $"mp_nivel{nivelNum}";
        }
        catch { }
    }
}
