using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

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
    public Button opcion4Button;

    [Header("TEXTOS OPCIONES")]
    public TMP_Text opcion1Text;
    public TMP_Text opcion2Text;
    public TMP_Text opcion3Text;
    public TMP_Text opcion4Text;

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
    private bool audioYaProcesado = false; // 

    void Start()
    {
        Debug.Log("Iniciando ActivityController...");

        nivel = new Nivel(ActivityManager.NivelActualId, ActivityManager.NivelNombre);

        string dbPath = Path.Combine(Application.persistentDataPath, "miBase.db");
        var conexion = new ConexionSQLite(dbPath);

        var inicializador = new InicializadorBD(conexion);
        inicializador.CrearTablas();

        var seed = new SeedActividad(conexion);
        seed.Ejecutar();

        actividadGateway = new SQLiteActividadGateway(conexion);
        progresoGateway = new SQLiteProgresoGateway(conexion);

        actividad = actividadGateway.ObtenerPorId(ActivityManager.ActividadActualId, nivel);

        // Fallback temporal: si no existe, usar actividad 1
        if (actividad == null)
        {
            Debug.LogWarning($"Actividad {ActivityManager.ActividadActualId} no encontrada. Usando actividad 1 como fallback temporal.");
            actividad = actividadGateway.ObtenerPorId(1, nivel);
        }

        if (actividad == null)
        {
            Debug.LogError("No se encontró ni la actividad solicitada ni la actividad 1 de fallback");
            return;
        }

        Debug.Log("Contenidos cargados: " + actividad.Contenidos.Count);

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
        opcion4Button.onClick.AddListener(() => SeleccionarRespuesta(opcion4Text.text));

        // Asegurar que las imágenes de resultado estén desactivadas al inicio
        if (imagenCorrecto != null) imagenCorrecto.gameObject.SetActive(false);
        if (imagenIncorrecto != null) imagenIncorrecto.gameObject.SetActive(false);

        MostrarContenido();
    }

    void VerificarRaycastTarget(Button boton)
    {
        if (boton == null) return;
        Image img = boton.GetComponent<Image>();
        if (img != null && !img.raycastTarget)
        {
            img.raycastTarget = true;
            Debug.Log($"Raycast Target habilitado para {boton.name}");
        }
    }

    void Update()
    {
        if (esperandoAudio && !audioSource.isPlaying && !audioYaProcesado)
        {
            audioYaProcesado = true;
            esperandoAudio = false;

            Debug.Log("🎵 Audio terminado → avanzando");

            CancelInvoke(nameof(SiguienteContenido));
            Invoke(nameof(SiguienteContenido), 0.2f);
        }
    }

    void MostrarContenido()
    {
        var contenido = progreso.ObtenerActual();

        if (contenido == null)
        {
            Debug.LogError("No hay contenido");
            return;
        }

        CancelInvoke(nameof(SiguienteContenido));

        OcultarTodo();

        esperandoAudio = false;
        audioYaProcesado = false;

        Debug.Log($"Mostrando contenido: {contenido.GetType().Name} en índice {progreso.IndiceContenido}");

        if (contenido is Historia historia)
        {
            panelHistoria.SetActive(true);

            tituloText.text = "Historia";
            contenidoText.text = "Escucha la narración";

            AudioClip clip = Resources.Load<AudioClip>(historia.Recurso);

            if (clip != null)
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.loop = false; 
                audioSource.Play();

                esperandoAudio = true;
            }
            else
            {
                Debug.LogWarning("Audio no encontrado: " + historia.Recurso);
            }

            siguienteButton.gameObject.SetActive(false);
            volverButton.gameObject.SetActive(false);
            volverHistoriaButton.gameObject.SetActive(true);
            volverHistoriaButton.interactable = true;
            Debug.Log($"VolverHistoriaButton - SetActive: {volverHistoriaButton.gameObject.activeSelf}, Interactable: {volverHistoriaButton.interactable}");
            Debug.Log($"Posición: {volverHistoriaButton.transform.position}, Escala: {volverHistoriaButton.transform.localScale}");
            salirButton.gameObject.SetActive(false);
        }

        else if (contenido is Pregunta pregunta)
        {
            panelPreguntas.SetActive(true);

            tituloText.text = "Pregunta";
            contenidoText.text = pregunta.Enunciado;

            MostrarOpciones(pregunta.Opciones);

            opcion1Button.interactable = true;
            opcion2Button.interactable = true;
            opcion3Button.interactable = true;
            opcion4Button.interactable = true;

            siguienteButton.gameObject.SetActive(true);
            siguienteButton.interactable = false;
            volverButton.gameObject.SetActive(true);
            volverButton.interactable = true;
            Debug.Log($"VolverButton - SetActive: {volverButton.gameObject.activeSelf}, Interactable: {volverButton.interactable}");
            volverHistoriaButton.gameObject.SetActive(false);
            salirButton.gameObject.SetActive(false);

            Debug.Log($"Pregunta mostrada: {pregunta.Enunciado}");
        }

        else if (contenido is Reto reto)
        {
            panelReto.SetActive(true);

            tituloText.text = "Reto Práctico";
            contenidoText.text = reto.Texto;

            bool tieneInstruccionesConImagenes = reto.InstruccionesPares != null && reto.InstruccionesPares.Count > 0;

            if (retoPanelController != null && tieneInstruccionesConImagenes)
            {
                retoPanelController.gameObject.SetActive(true);
                retoPanelController.InicializarConReto(reto);
                Debug.Log($"RetoPanelController inicializado con {reto.InstruccionesPares.Count} pares de instrucciones");
            }
            else
            {
                if (retoPanelController != null)
                    retoPanelController.gameObject.SetActive(false);

                if (instruccionesText != null)
                {
                    instruccionesText.text = reto.Instrucciones;
                    instruccionesText.gameObject.SetActive(!string.IsNullOrEmpty(reto.Instrucciones));
                }

                Sprite img = Resources.Load<Sprite>(reto.Recurso);

                if (img != null)
                {
                    retoImage.sprite = img;
                    retoImage.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("Imagen no encontrada: " + reto.Recurso);
                }
            }

            siguienteButton.gameObject.SetActive(false);
            volverButton.gameObject.SetActive(false);
            volverHistoriaButton.gameObject.SetActive(false);
            salirButton.gameObject.SetActive(false);
            finalizarRetoButton.gameObject.SetActive(false);

            Debug.Log("Reto mostrado");
        }
    }

    void MostrarOpciones(List<string> opciones)
    {
        opcion1Button.gameObject.SetActive(true);
        opcion2Button.gameObject.SetActive(true);
        opcion3Button.gameObject.SetActive(true);
        opcion4Button.gameObject.SetActive(true);

        opcion1Text.text = opciones.Count > 0 ? opciones[0] : "";
        opcion2Text.text = opciones.Count > 1 ? opciones[1] : "";
        opcion3Text.text = opciones.Count > 2 ? opciones[2] : "";
        opcion4Text.text = opciones.Count > 3 ? opciones[3] : "";
    }

    void SeleccionarRespuesta(string respuestaSeleccionada)
    {
        var contenido = progreso.ObtenerActual();

        if (contenido is Pregunta pregunta)
        {
            bool correcta = pregunta.ValidarRespuesta(respuestaSeleccionada);

            Debug.Log($"Respuesta seleccionada: '{respuestaSeleccionada}'");
            Debug.Log($"Respuesta correcta: '{pregunta.RespuestaCorrecta}'");
            Debug.Log($"¿Correcta?: {correcta}");

            if (correcta)
            {
                progreso.MarcarRespuestaCorrecta();
                siguienteButton.interactable = true;
                MostrarResultado(true);

                Debug.Log("Respuesta correcta → Botón siguiente habilitado");
            }
            else
            {
                siguienteButton.interactable = false;
                MostrarResultado(false);

                Debug.Log("Respuesta incorrecta → Intenta de nuevo");
            }
        }
    }

    void MostrarResultado(bool esCorrecta)
    {
        Image imagenAMostrar = esCorrecta ? imagenCorrecto : imagenIncorrecto;

        if (imagenAMostrar == null)
        {
            Debug.LogWarning("Imagen de resultado no está asignada en el Inspector");
            return;
        }

        // Desactivar botones de opciones mientras se muestra resultado
        opcion1Button.interactable = false;
        opcion2Button.interactable = false;
        opcion3Button.interactable = false;
        opcion4Button.interactable = false;

        imagenAMostrar.gameObject.SetActive(true);

        if (esCorrecta)
        {
            Debug.Log("Mostrando imagen: Respuesta Correcta");
        }
        else
        {
            Debug.Log("Mostrando imagen: Respuesta Incorrecta");
        }

        CancelInvoke(nameof(OcultarResultado));
        Invoke(nameof(OcultarResultado), tiempoMuestraResultado);
    }

    void OcultarResultado()
    {
        if (imagenCorrecto != null) imagenCorrecto.gameObject.SetActive(false);
        if (imagenIncorrecto != null) imagenIncorrecto.gameObject.SetActive(false);

        // Solo reactivar botones si la respuesta fue incorrecta (para reintentar)
        if (!progreso.PuedeAvanzar)
        {
            opcion1Button.interactable = true;
            opcion2Button.interactable = true;
            opcion3Button.interactable = true;
            opcion4Button.interactable = true;
        }

        Debug.Log("Imágenes de resultado ocultadas");
    }

    void SiguienteContenido()
    {
        Debug.Log($"SiguienteContenido llamado. Índice actual: {progreso.IndiceContenido}, PuedeAvanzar: {progreso.PuedeAvanzar}");

        bool avanzo = progreso.Avanzar();

        if (avanzo)
        {
            Debug.Log($"Avanzó a índice {progreso.IndiceContenido}");
            progresoGateway.Guardar(progreso);
            MostrarContenido();
        }
        else
        {
            Debug.Log("No se pudo avanzar. ¿Requería respuesta a pregunta?");
            Debug.Log("Actividad finalizada");
            MostrarBotonSalir();
        }
    }

    void OcultarTodo()
    {
        panelHistoria.SetActive(false);
        panelPreguntas.SetActive(false);
        panelReto.SetActive(false);

        retoImage.gameObject.SetActive(false);

        if (imagenCorrecto != null) imagenCorrecto.gameObject.SetActive(false);
        if (imagenIncorrecto != null) imagenIncorrecto.gameObject.SetActive(false);

        opcion1Button.gameObject.SetActive(false);
        opcion2Button.gameObject.SetActive(false);
        opcion3Button.gameObject.SetActive(false);
        opcion4Button.gameObject.SetActive(false);
    }

    void VolverAPanelHistoria()
    {
        Debug.Log("CLICKEADO: Volviendo al panel de historia");

        // Retroceder hasta encontrar una Historia
        while (progreso.IndiceContenido > 0)
        {
            progreso.Retroceder();
            var contenido = progreso.ObtenerActual();

            if (contenido is Historia)
            {
                Debug.Log("Historia encontrada en índice: " + progreso.IndiceContenido);
                break;
            }
        }

        MostrarContenido();
    }

    void VolverAMenuNiveles()
    {
        Debug.Log("CLICKEADO: Volviendo al menú de niveles");

        string escenaDestino = ActivityManager.EscenaMenuNivel;

        // Fallback si EscenaMenuNivel está vacío (ej: abrir escena directamente desde editor)
        if (string.IsNullOrEmpty(escenaDestino))
        {
            Debug.LogWarning("EscenaMenuNivel vacío. Usando fallback basado en NivelActualId");

            switch (ActivityManager.NivelActualId)
            {
                case 1:
                    escenaDestino = "mp_nivel1";
                    break;
                case 2:
                    escenaDestino = "mp_nivel2";
                    break;
                case 3:
                    escenaDestino = "mp_ttj";
                    break;
                default:
                    escenaDestino = "mp_estudiante";
                    break;
            }
        }

        SceneManager.LoadScene(escenaDestino);
    }

    void MostrarBotonSalir()
    {
        Debug.Log("Actividad completada - Mostrando botón salir");

        // Ocultar paneles
        panelHistoria.SetActive(false);
        panelPreguntas.SetActive(false);
        panelReto.SetActive(false);

        retoImage.gameObject.SetActive(false);

        // Ocultar botones de opciones
        opcion1Button.gameObject.SetActive(false);
        opcion2Button.gameObject.SetActive(false);
        opcion3Button.gameObject.SetActive(false);
        opcion4Button.gameObject.SetActive(false);

        // Ocultar otros botones
        siguienteButton.gameObject.SetActive(false);
        volverButton.gameObject.SetActive(false);
        volverHistoriaButton.gameObject.SetActive(false);
        finalizarRetoButton.gameObject.SetActive(false);

        // Mostrar mensaje de finalización
        tituloText.gameObject.SetActive(true);
        contenidoText.gameObject.SetActive(true);
        tituloText.text = "¡ACTIVIDAD COMPLETADA!";
        contenidoText.text = "¡Excelente trabajo!\n\nPresiona SALIR para volver al menú de actividades";

        // Mostrar botón de salir
        if (salirButton != null)
        {
            salirButton.gameObject.SetActive(true);
            Debug.Log("Botón salir activado - gameObject activo");
        }
        else
        {
            Debug.LogError("ERROR: salirButton no está asignado");
        }
    }
}