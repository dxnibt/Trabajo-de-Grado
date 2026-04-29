using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Dominio.entidades;

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

    private List<InstruccionPar> instruccionesPares;
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
            Debug.LogWarning($"[RetoPanelController] El reto no tiene instrucciones. RAW: '{reto.Instrucciones}'");
            OcultarInstrucciones();
            if (botonFinalizar != null)
                botonFinalizar.gameObject.SetActive(true);
            return;
        }

        Debug.Log($"[RetoPanelController] Reto iniciado con {instruccionesPares.Count} pares de instrucciones");
        MostrarPar(0);
    }

    private void MostrarPar(int indice)
    {
        if (indice < 0 || indice >= instruccionesPares.Count) return;

        indiceActual = indice;
        var par = instruccionesPares[indice];

        MostrarSlot(imagen1, texto1, par.Imagen1, par.Texto1);
        MostrarSlot(imagen2, texto2, par.Imagen2, par.Texto2);

        ActualizarBotones();
    }

    private void MostrarSlot(Image img, TMP_Text txt, string rutaImagen, string texto)
    {
        if (img != null) img.gameObject.SetActive(false);
        if (txt != null) txt.gameObject.SetActive(false);

        if (img != null && !string.IsNullOrEmpty(rutaImagen))
        {
            Sprite sprite = Resources.Load<Sprite>(rutaImagen);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[RetoPanelController] Imagen no encontrada: {rutaImagen}");
            }
        }

        if (txt != null && !string.IsNullOrEmpty(texto))
        {
            txt.text = texto;
            txt.gameObject.SetActive(true);
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
        if (instruccionesPares == null || instruccionesPares.Count == 0) return;

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

    private void OcultarInstrucciones()
    {
        if (imagen1 != null) imagen1.gameObject.SetActive(false);
        if (texto1 != null) texto1.gameObject.SetActive(false);
        if (imagen2 != null) imagen2.gameObject.SetActive(false);
        if (texto2 != null) texto2.gameObject.SetActive(false);
        if (botonSiguiente != null) botonSiguiente.gameObject.SetActive(false);
        if (botonRetroceder != null) botonRetroceder.gameObject.SetActive(false);
    }
}
