using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dominio.entidades;
using Infraestructura.SQLite;
using Infraestructura.SQLite.SQLiteGateway;

public class ActivityController : MonoBehaviour
{
    [Header("UI GENERAL")]
    public TMP_Text tituloText;
    public TMP_Text contenidoText;
    public Button siguienteButton;

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

    private SQLiteActividadGateway actividadGateway;
    private SQLiteProgresoGateway progresoGateway;

    private Actividad actividad;
    private Progreso progreso;
    private Nivel nivel;

    void Start()
    {
        nivel = new Nivel(1, "Nivel 1");

        var conexion = new ConexionSQLite("mi_basedatos.db");

        actividadGateway = new SQLiteActividadGateway(conexion);
        progresoGateway = new SQLiteProgresoGateway(conexion);

        actividad = actividadGateway.ObtenerPorId(1, nivel);

        progreso = new Progreso();
        progreso.IniciarActividad(actividad);

        siguienteButton.onClick.AddListener(SiguienteContenido);

        opcion1Button.onClick.AddListener(() => SeleccionarRespuesta(opcion1Text.text));
        opcion2Button.onClick.AddListener(() => SeleccionarRespuesta(opcion2Text.text));
        opcion3Button.onClick.AddListener(() => SeleccionarRespuesta(opcion3Text.text));
        opcion4Button.onClick.AddListener(() => SeleccionarRespuesta(opcion4Text.text));

        siguienteButton.interactable = false;

        MostrarContenido();
    }

    void MostrarContenido()
    {
        var contenido = progreso.ObtenerActual();

        OcultarTodo();

        if (contenido is Historia historia)
        {
            tituloText.text = "Historia";
            contenidoText.text = "Escucha la narración";

            AudioClip clip = Resources.Load<AudioClip>(historia.Recurso);

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();

            siguienteButton.interactable = true;
        }
        else if (contenido is Pregunta pregunta)
        {
            tituloText.text = "Pregunta";
            contenidoText.text = pregunta.Enunciado;

            MostrarOpciones(pregunta.Opciones);

            siguienteButton.interactable = false;
        }
        else if (contenido is Reto reto)
        {
            tituloText.text = "Reto";
            contenidoText.text = reto.Texto;

            Sprite img = Resources.Load<Sprite>(reto.Recurso);
            retoImage.sprite = img;
            retoImage.gameObject.SetActive(true);

            siguienteButton.interactable = true;
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

            if (correcta)
            {
                progreso.MarcarRespuestaCorrecta();
                siguienteButton.interactable = true;

                Debug.Log("Respuesta correcta");
            }
            else
            {
                siguienteButton.interactable = false;

                Debug.Log("Respuesta incorrecta");
            }
        }
    }

    void SiguienteContenido()
    {
        bool avanzo = progreso.Avanzar();

        if (avanzo)
        {
            progresoGateway.Guardar(progreso);
            MostrarContenido();
        }
        else
        {
            Debug.Log("Actividad finalizada o no puedes avanzar");
        }
    }

    void OcultarTodo()
    {
        retoImage.gameObject.SetActive(false);

        opcion1Button.gameObject.SetActive(false);
        opcion2Button.gameObject.SetActive(false);
        opcion3Button.gameObject.SetActive(false);
        opcion4Button.gameObject.SetActive(false);
    }
}