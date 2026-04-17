using Dominio.entidades;
using Dominio.gateway;

namespace Dominio.casos_de_uso
{

public class PreguntaUseCase
{
    private readonly RespuestaGateway gateway;

    public PreguntaUseCase(RespuestaGateway gateway)
    {
        this.gateway = gateway;
    }

    public bool Ejecutar(Estudiante estudiante, Pregunta pregunta, string respuesta)
    {
        bool correcta = pregunta.ValidarRespuesta(respuesta);

        var respuestaObj = new Respuesta(estudiante.Id, pregunta, respuesta);

        gateway.Guardar(respuestaObj);

        if (correcta)
            estudiante.Progreso.MarcarRespuestaCorrecta();

        return correcta;
    }
}
}