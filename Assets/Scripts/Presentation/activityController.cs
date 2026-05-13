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
    [Header("SESIÃ“N")]
    public GameObject panelSesionSeleccion;
    public Button botonIndividualSesion;
    public Button botonGrupoSesion;
    public GameObject panelNombreSesion;
    public Image labelIndividualSesion;
    public Image labelGrupoSesion;
    public TMP_InputField campoNombreSesion;
    public Button botonContinuarSesion;
    public TMP_Text textoErrorSesion;

    [Header("UI GENERAL")]
    public Slider barraProgreso;
    public Image tituloImage;
    public Image preguntaImage;
    public Image retoImage;
    public TMP_Text contenidoText;
    public Button siguienteButton;
    public Button volverButton;
    public Button volverHistoriaButton;
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

    [Header("RETROALIMENTACIÃ“N DE RESPUESTAS")]
    public Image imagenCorrecto;
    public Image imagenIncorrecto;
    public float tiempoMuestraResultado = 1.5f;

    [Header("AUDIO")]
    public AudioSource audioSource;

    private SQLiteActividadGateway actividadGateway;
    private SQLiteProgresoGateway progresoGateway;
    private SQLiteActividadCompletadaGateway completadaGateway;
    private SQLiteRespuestaGateway respuestaGateway;
    private SQLiteEstudianteGateway estudianteGateway;

    private Actividad actividad;
    private Progreso progreso;
    private Nivel nivel;

    private bool esGrupoSesion = false;
    private bool esperandoAudio = false;
    private bool audioYaProcesado = false;
    private float tiempoAudioInicio = 0f;
    private float duracionAudioActual = 0f;
    private bool ultimaRespuestaCorrecta = false;
    private bool mostrandoResultado = false;

    void Start()
    {
        Debug.Log($"[ActivityController] === INICIO ===");
        Debug.Log($"[ActivityController] ActivityManager - EstudianteId: {ActivityManager.EstudianteId}, TipoConfirmado: {ActivityManager.TipoConfirmado}");
        Debug.Log($"[ActivityController] PlayerPrefs - UltimoEstudiante: '{PlayerPrefs.GetString("UltimoEstudiante", "")}', TipoConfirmado: {PlayerPrefs.GetInt("TipoConfirmado", 0)}");

        if (ActivityManager.ActividadActualId == 0)
            InferirContextoDesdEscena(SceneManager.GetActiveScene().name);

        nivel = new Nivel(ActivityManager.NivelActualId, ActivityManager.NivelNombre);

        string dbPath = Path.Combine(Application.persistentDataPath, "miBase.db");
        var conexion = new ConexionSQLite(dbPath);
        new InicializadorBD(conexion).CrearTablas();

        using (var connCheck = conexion.CrearConexion())
        {
            connCheck.Open();
            if (BaseDeDatosVacia(connCheck))
            {
                Debug.Log("[ActivityController] BD sin datos â†’ seeds");
                new SeedActividad(conexion).Ejecutar();
                new SeedPregunta(conexion).Ejecutar();
            }
        }

        actividadGateway = new SQLiteActividadGateway(conexion);
        progresoGateway = new SQLiteProgresoGateway(conexion);
        completadaGateway = new SQLiteActividadCompletadaGateway(conexion);
        respuestaGateway = new SQLiteRespuestaGateway(conexion);
        estudianteGateway = new SQLiteEstudianteGateway(conexion);

        actividad = actividadGateway.ObtenerPorId(ActivityManager.ActividadActualId, nivel);
        if (actividad == null)
        {
            Debug.LogWarning($"[ActivityController] Actividad {ActivityManager.ActividadActualId} no encontrada. Usando 1.");
            actividad = actividadGateway.ObtenerPorId(1, nivel);
        }
        if (actividad == null)
        {
            Debug.LogError("[ActivityController] No se encontrÃ³ la actividad");
            return;
        }

        // Modo docente: escena cargada accidentalmente por listener secundario — volver al menú
        if (ActivityManager.ModoDocente)
        {
            if (audioSource != null) { audioSource.playOnAwake = false; audioSource.Stop(); }
            ActivityManager.ModoDocente = false;
            SceneManager.LoadScene("mp_docente");
            return;
        }

        OcultarTodo();
        ConfigurarListeners();

        progreso = new Progreso();

        // ================================================
        // LÃ“GICA DE RESTAURACIÃ“N DE SESIÃ“N
        // ================================================
        
        // Si no hay estudiante en memoria, intentar restaurar desde PlayerPrefs
        // Solo restaura si la sesiÃ³n guardada corresponde exactamente a esta misma actividad
        if (ActivityManager.EstudianteId == 0)
        {
            string nombreGuardado = PlayerPrefs.GetString("UltimoEstudiante", "");
            bool tieneSesionGuardada = !string.IsNullOrEmpty(nombreGuardado);
            bool tipoConfirmadoGuardado = PlayerPrefs.GetInt("TipoConfirmado", 0) == 1;
            int actividadConfirmada = PlayerPrefs.GetInt("UltimaActividadConfirmada", 0);
            bool mismaActividad = actividadConfirmada == ActivityManager.ActividadActualId;

            if (tieneSesionGuardada && tipoConfirmadoGuardado && mismaActividad)
            {
                Debug.Log($"[ActivityController] Restaurando sesiÃ³n guardada: {nombreGuardado}");
                bool esGrupoGuardado = PlayerPrefs.GetInt("UltimoEsGrupo", 0) == 1;
                var est = estudianteGateway.ObtenerOCrearPorNombre(nombreGuardado, esGrupoGuardado);

                ActivityManager.EstudianteId = est.Id;
                ActivityManager.EstudianteNombre = est.Nombre;
                ActivityManager.EsGrupo = esGrupoGuardado;
                ActivityManager.TipoConfirmado = true;
            }
        }
        
        // Verificar si tenemos sesiÃ³n activa
        if (ActivityManager.EstudianteId > 0 && ActivityManager.TipoConfirmado)
        {
            Debug.Log($"[ActivityController] SesiÃ³n activa encontrada: {ActivityManager.EstudianteNombre} (ID: {ActivityManager.EstudianteId})");
            IniciarContenidoActividad();
            return;
        }
        
        // No hay sesiÃ³n, mostrar panel por primera vez
        Debug.Log("[ActivityController] No hay sesiÃ³n activa - mostrando panel de selecciÃ³n");
        MostrarPanelSesion();
    }

    // â”€â”€ SesiÃ³n â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    void MostrarPanelSesion()
    {
        Debug.Log("[ActivityController] Mostrando panel de selecciÃ³n de sesiÃ³n");
        
        if (panelSesionSeleccion != null) panelSesionSeleccion.SetActive(true);
        if (panelNombreSesion != null) panelNombreSesion.SetActive(false);
        if (textoErrorSesion != null) textoErrorSesion.gameObject.SetActive(false);

        botonIndividualSesion?.onClick.RemoveAllListeners();
        botonGrupoSesion?.onClick.RemoveAllListeners();
        botonContinuarSesion?.onClick.RemoveAllListeners();

        botonIndividualSesion?.onClick.AddListener(() => AbrirNombreInput(false));
        botonGrupoSesion?.onClick.AddListener(() => AbrirNombreInput(true));
        botonContinuarSesion?.onClick.AddListener(ConfirmarSesion);
    }

    void AbrirNombreInput(bool grupo)
    {
        esGrupoSesion = grupo;

        if (panelSesionSeleccion != null) panelSesionSeleccion.SetActive(false);
        if (panelNombreSesion != null) panelNombreSesion.SetActive(true);
        if (campoNombreSesion != null)
        {
            campoNombreSesion.text = "";
            campoNombreSesion.characterLimit = 30;
        }
        if (textoErrorSesion != null) textoErrorSesion.gameObject.SetActive(false);
        if (labelIndividualSesion != null) labelIndividualSesion.gameObject.SetActive(!grupo);
        if (labelGrupoSesion != null) labelGrupoSesion.gameObject.SetActive(grupo);
    }

    void ConfirmarSesion()
    {
        string nombre = campoNombreSesion != null ? campoNombreSesion.text.Trim() : "";
        if (string.IsNullOrEmpty(nombre))
        {
            if (textoErrorSesion != null)
            {
                textoErrorSesion.text = "Por favor ingresa un nombre.";
                textoErrorSesion.gameObject.SetActive(true);
            }
            return;
        }

        if (ContienNumeros(nombre))
        {
            if (textoErrorSesion != null)
            {
                textoErrorSesion.text = "El nombre no puede contener números.";
                textoErrorSesion.gameObject.SetActive(true);
            }
            return;
        }

        Debug.Log($"[ActivityController] Confirmando sesiÃ³n - Nombre: {nombre}, EsGrupo: {esGrupoSesion}");

        var est = estudianteGateway.ObtenerOCrearPorNombre(nombre, esGrupoSesion);
        ActivityManager.EstudianteId = est.Id;
        ActivityManager.EstudianteNombre = est.Nombre;
        ActivityManager.EsGrupo = esGrupoSesion;
        ActivityManager.TipoConfirmado = true;

        PlayerPrefs.SetString("UltimoEstudiante", nombre);
        PlayerPrefs.SetInt("UltimoEsGrupo", esGrupoSesion ? 1 : 0);
        PlayerPrefs.SetInt("TipoConfirmado", 1);
        PlayerPrefs.SetInt("UltimaActividadConfirmada", ActivityManager.ActividadActualId);
        PlayerPrefs.Save();

        if (panelSesionSeleccion != null) panelSesionSeleccion.SetActive(false);
        if (panelNombreSesion != null) panelNombreSesion.SetActive(false);

        IniciarContenidoActividad();
    }

    // â”€â”€ Inicio de actividad con restauraciÃ³n de progreso â”€â”€â”€â”€

    void IniciarContenidoActividad()
    {
        Debug.Log($"[ActivityController] Iniciando actividad para estudiante ID: {ActivityManager.EstudianteId}");
        
        progreso.EstudianteId = ActivityManager.EstudianteId;
        progreso.IniciarActividad(actividad);

        if (ActivityManager.EstudianteId > 0)
        {
            int? indice = progresoGateway.ObtenerIndiceGuardado(
                ActivityManager.EstudianteId, ActivityManager.ActividadActualId);
            if (indice.HasValue && indice.Value > 0)
            {
                Debug.Log($"[ActivityController] Restaurando progreso en Ã­ndice: {indice.Value}");
                progreso.RestaurarIndice(indice.Value);
            }
        }

        if (barraProgreso != null)
        {
            barraProgreso.minValue = 0f;
            barraProgreso.maxValue = 1f;
            barraProgreso.value = 0f;
            barraProgreso.interactable = false;
        }

        ActualizarBarraProgreso();
        MostrarContenido();
    }

    // â”€â”€ Setup â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    void ConfigurarListeners()
    {
        VerificarRaycastTarget(volverButton);
        VerificarRaycastTarget(volverHistoriaButton);
        VerificarRaycastTarget(siguienteButton);
        VerificarRaycastTarget(finalizarRetoButton);

        siguienteButton?.onClick.AddListener(SiguienteContenido);
        volverButton?.onClick.AddListener(VolverAPanelHistoria);
        volverHistoriaButton?.onClick.AddListener(VolverAMenuNiveles);

        opcion1Button?.onClick.AddListener(() => SeleccionarRespuesta(opcion1Text.text));
        opcion2Button?.onClick.AddListener(() => SeleccionarRespuesta(opcion2Text.text));
        opcion3Button?.onClick.AddListener(() => SeleccionarRespuesta(opcion3Text.text));

        if (imagenCorrecto != null) imagenCorrecto.gameObject.SetActive(false);
        if (imagenIncorrecto != null) imagenIncorrecto.gameObject.SetActive(false);
    }

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

    void ActualizarBarraProgreso()
    {
        if (barraProgreso == null || actividad == null) return;
        int total = actividad.TotalContenidos;
        barraProgreso.value = total > 1 ? (float)progreso.IndiceContenido / (total - 1) : 1f;
    }

    // â”€â”€ Loop de audio â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    // â”€â”€ Mostrar contenido â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    void MostrarContenido()
    {
        ActualizarBarraProgreso();

        var contenido = progreso.ObtenerActual();
        if (contenido == null)
        {
            Debug.LogWarning("[ActivityController] No hay mÃ¡s contenidos");
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
                Sprite s = Resources.Load<Sprite>("Titulos/historia_titulo");
                tituloImage.sprite = s != null ? s : null;
            }

            if (contenidoText != null) contenidoText.text = "Escucha la narraciÃ³n";

            AudioClip clip = Resources.Load<AudioClip>(historia.Recurso);
            if (clip == null && audioSource.clip != null) clip = audioSource.clip;

            if (clip != null)
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
                duracionAudioActual = clip.length;
                tiempoAudioInicio = Time.time;
                esperandoAudio = true;
                audioYaProcesado = false;
                Debug.Log($"[ActivityController] Audio: {clip.name} ({clip.length:F2}s)");
                siguienteButton?.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[ActivityController] Sin AudioClip para esta actividad");
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
                Sprite s = Resources.Load<Sprite>("Titulos/pregunta_titulo");
                tituloImage.sprite = s != null ? s : null;
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
                Sprite s = Resources.Load<Sprite>("Titulos/reto_titulo");
                tituloImage.sprite = s != null ? s : null;
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

    // â”€â”€ Opciones de respuesta â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

            // Guardar intento en BD
            if (ActivityManager.EstudianteId > 0 && respuestaGateway != null)
            {
                var resp = new Respuesta(ActivityManager.EstudianteId, pregunta, respuesta);
                respuestaGateway.Guardar(resp);
            }

            if (correcta) progreso.MarcarRespuestaCorrecta();
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
            SiguienteContenido();
        else
            HabilitarBotonesOpciones(true);
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
            // Cuando se completa la actividad
            if (retoPanelController != null && retoPanelController.gameObject.activeSelf)
            {
                // El reto ya estÃ¡ mostrÃ¡ndose, no hacer nada
            }
            else
            {
                // Si no hay reto, completar directamente
                CompletarActividad();
            }
        }
    }

    // â”€â”€ Completar actividad â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    void CompletarActividad()
    {
        Debug.Log($"[ActivityController] Actividad completada para estudiante ID: {ActivityManager.EstudianteId}");
        
        // Guardar progreso completado
        if (ActivityManager.EstudianteId > 0)
            completadaGateway.Guardar(ActivityManager.EstudianteId, ActivityManager.ActividadActualId);

        ActivityManager.TipoConfirmado = false;
        PlayerPrefs.DeleteKey("TipoConfirmado");
        PlayerPrefs.DeleteKey("UltimoEsGrupo");
        PlayerPrefs.DeleteKey("UltimaActividadConfirmada");
        PlayerPrefs.Save();

        VolverAMenuNiveles();
    }

    // â”€â”€ Eventos del Reto â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    void ManejarRetoFinalizado()
    {
        if (retoPanelController != null)
        {
            retoPanelController.OnRetoFinalizado -= ManejarRetoFinalizado;
            retoPanelController.OcultarPanel();
        }

        if (barraProgreso != null) barraProgreso.value = 1f;

        CompletarActividad();
    }

    // â”€â”€ NavegaciÃ³n â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    void VolverAPanelHistoria()
    {
        while (progreso.IndiceContenido > 0)
        {
            progreso.Retroceder();
            if (progreso.ObtenerActual() is Historia) break;
        }
        MostrarContenido();
    }

        void VolverAModoDocente()
    {
        SceneManager.LoadScene("mp_docente");
    }

    void VolverAMenuNiveles()
    {
        SceneManager.LoadScene(ActivityManager.EscenaMenuNivel);
    }

    // â”€â”€ Utilidades UI â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    void OcultarTodo()
    {
        if (panelSesionSeleccion != null) panelSesionSeleccion.SetActive(false);
        if (panelNombreSesion != null) panelNombreSesion.SetActive(false);

        panelHistoria.SetActive(false);
        panelPreguntas.SetActive(false);
        panelReto.SetActive(false);

        preguntaImage?.gameObject.SetActive(false);
        retoImage?.gameObject.SetActive(false);

        siguienteButton?.gameObject.SetActive(false);
        volverButton?.gameObject.SetActive(false);
        volverHistoriaButton?.gameObject.SetActive(false);
        finalizarRetoButton?.gameObject.SetActive(false);
    }

    public void VolverAlPanelSesion()
    {
        OcultarTodo();
        MostrarPanelSesion();
    }

    // â”€â”€ MÃ©todo pÃºblico para cerrar sesiÃ³n â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public void CerrarSesion()
    {
        Debug.Log("[ActivityController] Cerrando sesiÃ³n manualmente");
        
        ActivityManager.EstudianteId = 0;
        ActivityManager.EstudianteNombre = "";
        ActivityManager.EsGrupo = false;
        ActivityManager.TipoConfirmado = false;
        
        PlayerPrefs.DeleteKey("UltimoEstudiante");
        PlayerPrefs.DeleteKey("UltimoEsGrupo");
        PlayerPrefs.DeleteKey("TipoConfirmado");
        PlayerPrefs.Save();
        
        // Recargar la escena para mostrar el panel de selecciÃ³n
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // â”€â”€ Inferir contexto desde nombre de escena â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void InferirContextoDesdEscena(string nombreEscena)
    {
        try
        {
            int sep = nombreEscena.IndexOf('_');
            int nivelNum = int.Parse(nombreEscena.Substring(1, sep - 1));
            int act = int.Parse(nombreEscena.Substring(sep + 2));

            ActivityManager.NivelActualId = nivelNum;
            ActivityManager.ActividadActualId = act + (nivelNum - 1) * 10;

            if (nivelNum == 1) ActivityManager.EscenaMenuNivel = "mp_nivel1";
            else if (nivelNum == 2) ActivityManager.EscenaMenuNivel = "mp_nivel2";
            else if (nivelNum == 3) ActivityManager.EscenaMenuNivel = "mp_ttj";
            else ActivityManager.EscenaMenuNivel = $"mp_nivel{nivelNum}";
        }
        catch { }
    }

    bool ContienNumeros(string texto)
    {
        foreach (char c in texto)
        {
            if (char.IsDigit(c))
                return true;
        }
        return false;
    }
}




