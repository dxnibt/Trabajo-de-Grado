using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dominio.entidades;
using System.Collections.Generic;

public class RetoPanelController : MonoBehaviour
{
    [Header("ELEMENTOS DEL PANEL")]
    public Image imagen1;
    public TMP_Text texto1;
    public Image imagen2;
    public TMP_Text texto2;

    [Header("BOTONES")]
    public Button botonSiguiente;
    public Button botonRetroceder;
    public Button botonFinalizar;

    private List<InstruccionPair> instruccionesPares;
    private int indiceActual = 0;

    void Start()
    {
        if (botonSiguiente != null)
            botonSiguiente.onClick.AddListener(MostrarSiguientePar);

        if (botonRetroceder != null)
            botonRetroceder.onClick.AddListener(MostrarAnteriorPar);
    }

    public void InicializarConReto(Reto reto)
    {
        instruccionesPares = reto.InstruccionesPares;
        indiceActual = 0;

        if (instruccionesPares == null || instruccionesPares.Count == 0)
        {
            Debug.LogWarning("El reto no tiene instrucciones configuradas");
            OcultarPanel();
            return;
        }

        Debug.Log($"Inicializando RetoPanelController con {instruccionesPares.Count} pares");
        MostrarPar(0);
        ActualizarBotones();
    }

    private void MostrarPar(int indice)
    {
        if (indice < 0 || indice >= instruccionesPares.Count)
            return;

        indiceActual = indice;
        var par = instruccionesPares[indice];

        MostrarInstruccion(1, par.Imagen1, par.Texto1);
        MostrarInstruccion(2, par.Imagen2, par.Texto2);

        ActualizarBotones();
    }

    private void MostrarInstruccion(int numero, string rutaImagen, string texto)
    {
        Image imagenTarget = numero == 1 ? imagen1 : imagen2;
        TMP_Text textoTarget = numero == 1 ? texto1 : texto2;

        bool estaVacia = string.IsNullOrEmpty(rutaImagen) && string.IsNullOrEmpty(texto);

        if (imagenTarget != null)
        {
            if (!estaVacia && !string.IsNullOrEmpty(rutaImagen))
            {
                Sprite sprite = Resources.Load<Sprite>(rutaImagen);
                if (sprite != null)
                {
                    imagenTarget.sprite = sprite;
                    imagenTarget.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"Imagen no encontrada: {rutaImagen}");
                    imagenTarget.gameObject.SetActive(false);
                }
            }
            else
            {
                imagenTarget.gameObject.SetActive(false);
            }
        }

        if (textoTarget != null)
        {
            textoTarget.text = estaVacia ? "" : texto;
            if (textoTarget.gameObject.activeSelf && estaVacia)
                textoTarget.gameObject.SetActive(false);
            else if (!textoTarget.gameObject.activeSelf && !estaVacia)
                textoTarget.gameObject.SetActive(true);
        }
    }

    private void MostrarSiguientePar()
    {
        if (indiceActual < instruccionesPares.Count - 1)
            MostrarPar(indiceActual + 1);
    }

    private void MostrarAnteriorPar()
    {
        if (indiceActual > 0)
            MostrarPar(indiceActual - 1);
    }

    private void ActualizarBotones()
    {
        bool esElPrimero = indiceActual == 0;
        bool esElUltimo = indiceActual == instruccionesPares.Count - 1;

        if (botonRetroceder != null)
            botonRetroceder.interactable = !esElPrimero;

        if (botonSiguiente != null)
            botonSiguiente.interactable = !esElUltimo;

        if (botonFinalizar != null)
            botonFinalizar.gameObject.SetActive(esElUltimo);
    }

    public void OcultarPanel()
    {
        gameObject.SetActive(false);
    }

    public bool TieneInstrucciones()
    {
        return instruccionesPares != null && instruccionesPares.Count > 0;
    }
}
