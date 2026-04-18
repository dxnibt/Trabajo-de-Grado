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

    [Header("AUDIO")]
    public AudioSource audioSource;

    [Header("CONFIGURACIÓN")]
    public string escenaMenuNiveles = "mp_nivel1";

    private SQLiteActividadGateway actividadGateway;
    private SQLiteProgresoGateway progresoGateway;

    private Actividad actividad;
    private Progreso progreso;
    private Nivel nivel;

    private bool esperandoAudio = false;
    private bool audioYaProcesado = false; // 🔥 evita bucle

    void Start()
    {
        Debug.Log("Iniciando ActivityController...");

        nivel = new Nivel(1, "Nivel 1");

        string dbPath = Path.Combine(Application.persistentDataPath, "miBase.db");
        var conexion = new ConexionSQLite(dbPath);

        // ✅ BD + SEED
        var inicializador = new InicializadorBD(conexion);
        inicializador.CrearTablas();

        var seed = new SeedActividad(conexion);
        seed.Ejecutar();

        actividadGateway = new SQLiteActividadGateway(conexion);
        progresoGateway = new SQLiteProgresoGateway(conexion);

        actividad = actividadGateway.ObtenerPorId(1, nivel);

        if (actividad == null)
        {
            Debug.LogError("❌ No se encontró la actividad");
            return;
        }

        Debug.Log("Contenidos cargados: " + actividad.Contenidos.Count);

        progreso = new Progreso();
        progreso.IniciarActividad(actividad);

        // BOTONES
        siguienteButton.onClick.AddListener(SiguienteContenido);
        volverButton.onClick.AddListener(VolverAPanelHistoria);
        volverHistoriaButton.onClick.AddListener(VolverAMenuNiveles);
        salirButton.onClick.AddListener(VolverAMenuNiveles);
        finalizarRetoButton.onClick.AddListener(SiguienteContenido);

        opcion1Button.onClick.AddListener(() => SeleccionarRespuesta(opcion1Text.text));
        opcion2Button.onClick.AddListener(() => SeleccionarRespuesta(opcion2Text.text));
        opcion3Button.onClick.AddListener(() => SeleccionarRespuesta(opcion3Text.text));
        opcion4Button.onClick.AddListener(() => SeleccionarRespuesta(opcion4Text.text));

        MostrarContenido();
    }

    void Update()
    {
        // ✅ Detectar fin de audio UNA sola vez
        if (esperandoAudio && !audioSource.isPlaying && !audioYaProcesado)
        {
            audioYaProcesado = true;
            esperandoAudio = false;

            Debug.Log("🎵 Audio terminado → avanzando");

            // 🔥 Cancelar cualquier invoke previo antes de programar uno nuevo
            CancelInvoke(nameof(SiguienteContenido));
            Invoke(nameof(SiguienteContenido), 0.2f);
        }
    }

    void MostrarContenido()
    {
        var contenido = progreso.ObtenerActual();

        if (contenido == null)
        {
            Debug.LogError("❌ No hay contenido");
            return;
        }

        // 🔥 Cancelar todos los invokes previos cuando se muestra contenido nuevo
        CancelInvoke(nameof(SiguienteContenido));

        OcultarTodo();

        // 🔥 RESET control audio cada contenido nuevo
        esperandoAudio = false;
        audioYaProcesado = false;

        Debug.Log($"📍 Mostrando contenido: {contenido.GetType().Name} en índice {progreso.IndiceContenido}");

        // 🟣 HISTORIA
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
                audioSource.loop = false; // 🔥 IMPORTANTE
                audioSource.Play();

                esperandoAudio = true;
            }
            else
            {
                Debug.LogWarning("⚠️ Audio no encontrado: " + historia.Recurso);
            }

            siguienteButton.gameObject.SetActive(false);
            volverButton.gameObject.SetActive(false);
            volverHistoriaButton.gameObject.SetActive(true);
            salirButton.gameObject.SetActive(false);
        }

        // 🔵 PREGUNTA
        else if (contenido is Pregunta pregunta)
        {
            panelPreguntas.SetActive(true);

            tituloText.text = "Pregunta";
            contenidoText.text = pregunta.Enunciado;

            MostrarOpciones(pregunta.Opciones);

            // 🔥 Botón desactivado completamente hasta responder correctamente
            siguienteButton.gameObject.SetActive(true);
            siguienteButton.interactable = false;
            volverButton.gameObject.SetActive(true);
            volverHistoriaButton.gameObject.SetActive(false);
            salirButton.gameObject.SetActive(false);

            Debug.Log($"❓ Pregunta mostrada: {pregunta.Enunciado}");
        }

        // 🟢 RETO
        else if (contenido is Reto reto)
        {
            panelReto.SetActive(true);

            tituloText.text = "Reto Práctico";
            contenidoText.text = reto.Texto;

            // 🔥 Mostrar instrucciones desde la BD
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
                Debug.LogWarning("⚠️ Imagen no encontrada: " + reto.Recurso);
            }

            siguienteButton.gameObject.SetActive(false);
            volverButton.gameObject.SetActive(false);
            volverHistoriaButton.gameObject.SetActive(false);
            salirButton.gameObject.SetActive(false);
            finalizarRetoButton.gameObject.SetActive(true);

            Debug.Log("🎯 Reto mostrado con instrucciones");
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

            Debug.Log($"🔍 Respuesta seleccionada: '{respuestaSeleccionada}'");
            Debug.Log($"✔️ Respuesta correcta: '{pregunta.RespuestaCorrecta}'");
            Debug.Log($"🎯 ¿Correcta?: {correcta}");

            if (correcta)
            {
                progreso.MarcarRespuestaCorrecta();
                siguienteButton.interactable = true;

                Debug.Log("✅ Respuesta correcta → Botón siguiente habilitado");
            }
            else
            {
                siguienteButton.interactable = false;

                Debug.Log("❌ Respuesta incorrecta → Intenta de nuevo");
            }
        }
    }

    void SiguienteContenido()
    {
        Debug.Log($"➡️ SiguienteContenido llamado. Índice actual: {progreso.IndiceContenido}, PuedeAvanzar: {progreso.PuedeAvanzar}");

        bool avanzo = progreso.Avanzar();

        if (avanzo)
        {
            Debug.Log($"✅ Avanzó a índice {progreso.IndiceContenido}");
            progresoGateway.Guardar(progreso);
            MostrarContenido();
        }
        else
        {
            Debug.Log("❌ No se pudo avanzar. ¿Requería respuesta a pregunta?");
            Debug.Log("🏁 Actividad finalizada");
            MostrarBotonSalir();
        }
    }

    void OcultarTodo()
    {
        panelHistoria.SetActive(false);
        panelPreguntas.SetActive(false);
        panelReto.SetActive(false);

        retoImage.gameObject.SetActive(false);

        opcion1Button.gameObject.SetActive(false);
        opcion2Button.gameObject.SetActive(false);
        opcion3Button.gameObject.SetActive(false);
        opcion4Button.gameObject.SetActive(false);
    }

    void VolverAPanelHistoria()
    {
        Debug.Log("⬅️ Volviendo al panel de historia");
        progreso.Retroceder();
        MostrarContenido();
    }

    void VolverAMenuNiveles()
    {
        Debug.Log("🏠 Volviendo al menú de niveles");
        UnityEngine.SceneManagement.SceneManager.LoadScene(escenaMenuNiveles);
    }

    void MostrarBotonSalir()
    {
        Debug.Log("✅ Actividad completada - Mostrando botón salir");

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
            Debug.Log("✅ Botón salir activado - gameObject activo");
        }
        else
        {
            Debug.LogError("❌ ERROR: salirButton no está asignado");
        }
    }
}