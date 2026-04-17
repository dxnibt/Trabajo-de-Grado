using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dominio.entidades;
using Infraestructura.SQLite;
using Infraestructura.SQLite.SQLiteGateway;

public class ActivityController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text tituloText;
    public TMP_Text contenidoText;
    public TMP_InputField respuestaInput;
    public Button responderButton;
    public Button siguienteButton;
    public Image retoImage;

    [Header("Audio")]
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

        responderButton.onClick.AddListener(ValidarRespuesta);
        siguienteButton.onClick.AddListener(SiguienteContenido);

        siguienteButton.interactable = false;

        MostrarContenido();
    }

    // ----------------------------
    // MOSTRAR CONTENIDO
    // ----------------------------
    void MostrarContenido()
    {
        var contenido = progreso.ObtenerActual();

        contenidoText.text = "";
        respuestaInput.text = "";
        retoImage.gameObject.SetActive(false);

        respuestaInput.gameObject.SetActive(false);

        if (contenido is Historia historia)
        {
            tituloText.text = "Historia";

            AudioClip clip = Resources.Load<AudioClip>(historia.Recurso);

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();

            // ✔️ puede avanzar sin responder
            siguienteButton.interactable = true;
        }
        else if (contenido is Pregunta pregunta)
        {
            tituloText.text = "Pregunta";

            contenidoText.text = pregunta.Enunciado;

            respuestaInput.gameObject.SetActive(true);

            siguienteButton.interactable = false;
        }
        else if (contenido is Reto reto)
        {
            tituloText.text = "Reto";

            contenidoText.text = reto.Texto;

            Sprite img = Resources.Load<Sprite>(reto.Recurso);
            retoImage.sprite = img;
            retoImage.gameObject.SetActive(true);

            // ✔️ puede avanzar
            siguienteButton.interactable = true;
        }
    }

    void ValidarRespuesta()
    {
        var contenido = progreso.ObtenerActual();

        if (contenido is Pregunta pregunta)
        {
            bool correcta = pregunta.ValidarRespuesta(respuestaInput.text);

            if (correcta)
            {
                progreso.MarcarRespuestaCorrecta();
                siguienteButton.interactable = true;

                Debug.Log("✅ Respuesta correcta");
            }
            else
            {
                siguienteButton.interactable = false;

                Debug.Log("❌ Respuesta incorrecta");
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
            Debug.Log("No puedes avanzar aún");
        }
    }
}