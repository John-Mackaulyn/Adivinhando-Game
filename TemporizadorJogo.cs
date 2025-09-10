using UnityEngine;
using System;

public class TemporizadorJogo : MonoBehaviour
{
    #region Campos e Propriedades
    public Action aoFinalizar;
    private float tempoRestante;
    public bool pausado;
    private bool rodando;
    #endregion

    #region Métodos do Ciclo de Vida
    // Atualiza o temporizador
    private void Update()
    {
        if (!rodando || pausado) return;

        tempoRestante -= Time.deltaTime;

        if (tempoRestante <= 0)
        {
            rodando = false;
            aoFinalizar?.Invoke();
        }
    }
    #endregion

    #region Controle do Temporizador
    // Inicia o temporizador com um tempo e callback
    public void IniciarTemporizador(float tempo, Action callback)
    {
        aoFinalizar = callback;
        tempoRestante = tempo;
        pausado = false;
        rodando = true;
    }

    // Para o temporizador
    public void PararTemporizador()
    {
        rodando = false;
        pausado = false;
        tempoRestante = 0;
    }

    // Pausa o temporizador
    public void PausarTemporizador()
    {
        pausado = true;
    }

    // Continua o temporizador
    public void ContinuarTemporizador()
    {
        if (tempoRestante <= 0) return;
        pausado = false;
    }

    // Obtém o tempo restante
    public float GetTempoRestante()
    {
        return Mathf.Max(tempoRestante, 0);
    }
    #endregion
}