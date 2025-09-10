using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ClassificacaoControle : MonoBehaviour
{
    #region Campos e Propriedades
    [Header("Referências de UI")]
    public TMP_Text txtTitulo;
    public Button btnRodada;
    public Button btnGeral;
    public Transform contentPanel;
    public GameObject itemClassificacaoPrefab;
    public Button btnMenuPrincipal;
    public Button btnJogarNovamente;
    public TMP_Text txtEstatisticas;

    [Header("Cores")]
    public Color corTabSelecionada = Color.blue;
    public Color corTabNormal = Color.gray;
    public Color corPrimeiroLugar = new Color(1f, 0.84f, 0f); // Ouro
    public Color corSegundoLugar = Color.gray;
    public Color corTerceiroLugar = new Color(0.8f, 0.5f, 0.2f); // Bronze

    private List<GameObject> itensClassificacao = new List<GameObject>();
    #endregion

    #region Métodos do Ciclo de Vida
    private void Start()
    {
        if (GerenciadorJogo.Instance == null)
        {
            Debug.LogError("GerenciadorJogo não encontrado!");
            return;
        }

        ConfigurarBotoesUI();
        MostrarClassificacaoRodada();
        AtualizarEstatisticas();
    }
    #endregion

    #region Configuração da UI
    private void ConfigurarBotoesUI()
    {
        btnRodada.onClick.RemoveAllListeners();
        btnGeral.onClick.RemoveAllListeners();
        btnMenuPrincipal.onClick.RemoveAllListeners();
        btnJogarNovamente.onClick.RemoveAllListeners();

        btnRodada.onClick.AddListener(MostrarClassificacaoRodada);
        btnGeral.onClick.AddListener(MostrarClassificacaoGeral);

        btnMenuPrincipal.onClick.AddListener(() => GerenciadorJogo.Instance.IrParaCena("MenuPrincipal"));
        btnJogarNovamente.onClick.AddListener(() => GerenciadorJogo.Instance.IrParaCena("Categorias"));
    }
    #endregion

    #region Lógica de Classificação
    private void AtualizarEstatisticas()
    {
        if (txtEstatisticas == null) return;

        int totalJogadores = GerenciadorJogo.Instance.jogadores.Count;
        int totalPontos = GerenciadorJogo.Instance.jogadores.Sum(j => j.pontos);
        int totalPartidas = 1; // Simplificado - ajuste conforme necessário

        txtEstatisticas.text = $"Jogadores: {totalJogadores}\nTotal de Pontos: {totalPontos}\nPartidas: {totalPartidas}";
    }

    private void LimparClassificacao()
    {
        foreach (var item in itensClassificacao)
        {
            Destroy(item);
        }
        itensClassificacao.Clear();
    }

    private void MostrarClassificacaoRodada()
    {
        LimparClassificacao();

        var ranking = GerenciadorJogo.Instance.GetRankingFinal();

        if (ranking == null || ranking.Count == 0)
        {
            CriarItemMensagem("Sem dados!");
            return;
        }

        for (int i = 0; i < ranking.Count; i++)
        {
            var jogador = ranking[i];
            Color corFundo = GetCorPosicao(i);
            CriarItemClassificacao(i + 1, jogador.nome, jogador.pontos, jogador.palavrasAcertadas, corFundo);
        }
    }

    private void MostrarClassificacaoGeral()
    {
        LimparClassificacao();

        if (GerenciadorJogo.Instance.classificacaoTotal == null || GerenciadorJogo.Instance.classificacaoTotal.Count == 0)
        {
            CriarItemMensagem("Sem dados!");
            return;
        }

        var ranking = GerenciadorJogo.Instance.classificacaoTotal
            .OrderByDescending(kvp => kvp.Value)
            .ToList();

        for (int i = 0; i < ranking.Count; i++)
        {
            var item = ranking[i];
            Color corFundo = GetCorPosicao(i);
            CriarItemClassificacao(i + 1, item.Key, item.Value, 0, corFundo);
        }
    }
    #endregion

    #region Utilitários
    private Color GetCorPosicao(int posicao)
    {
        return posicao switch
        {
            0 => corPrimeiroLugar,
            1 => corSegundoLugar,
            2 => corTerceiroLugar,
            _ => Color.white
        };
    }

    private void CriarItemClassificacao(int posicao, string nome, int pontos, int palavrasAcertadas, Color corFundo)
    {
        if (itemClassificacaoPrefab == null || contentPanel == null)
        {
            Debug.LogError("Prefab ou contentPanel não atribuídos!");
            return;
        }

        GameObject novoItem = Instantiate(itemClassificacaoPrefab, contentPanel);
        ItemClassificacao item = novoItem.GetComponent<ItemClassificacao>();

        if (item != null)
        {
            item.Configurar(posicao, nome, pontos, palavrasAcertadas, corFundo);
        }
        else
        {
            Debug.LogError("Componente ItemClassificacao não encontrado no prefab!");
        }

        itensClassificacao.Add(novoItem);
    }

    private void CriarItemMensagem(string mensagem)
    {
        if (itemClassificacaoPrefab == null || contentPanel == null) return;

        GameObject novoItem = Instantiate(itemClassificacaoPrefab, contentPanel);
        ItemClassificacao item = novoItem.GetComponent<ItemClassificacao>();

        if (item != null)
        {
            item.Configurar(0, mensagem, 0, 0, Color.white);
        }

        itensClassificacao.Add(novoItem);
    }
    #endregion
}