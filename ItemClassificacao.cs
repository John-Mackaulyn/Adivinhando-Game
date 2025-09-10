using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemClassificacao : MonoBehaviour
{
    #region Campos e Propriedades
    [Header("Referências UI")]
    [SerializeField] private TMP_Text txtPosicao;
    [SerializeField] private TMP_Text txtNome;
    [SerializeField] private TMP_Text txtPontos;
    [SerializeField] private TMP_Text txtPalavras;
    [SerializeField] private Image fundo;
    #endregion

    #region Configuração
    // Configura os elementos visuais do item de classificação
    public void Configurar(int posicao, string nome, int pontos, int palavrasAcertadas, Color corFundo)
    {
        txtPosicao.text = posicao > 0 ? posicao.ToString() : "";
        txtNome.text = nome ?? "";
        txtPontos.text = pontos > 0 ? pontos.ToString() : "";
        txtPalavras.text = palavrasAcertadas > 0 ? palavrasAcertadas.ToString() : "";

        if (fundo != null)
        {
            fundo.color = corFundo;
        }
    }
    #endregion
}