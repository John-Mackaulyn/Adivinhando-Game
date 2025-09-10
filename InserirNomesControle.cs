using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InserirNomesControle : MonoBehaviour
{
    #region Campos e Propriedades
    [Header("Configurações")]
    [SerializeField] private int maxJogadores = 8;
    [SerializeField] private float tempoFeedback = 1.5f;

    [Header("Referências UI")]
    [SerializeField] private TMP_InputField inputNome;
    [SerializeField] private Button btnAdicionar;
    [SerializeField] private Button btnComecar;
    [SerializeField] private Button btnVoltar;
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject itemJogadorPrefab;
    [SerializeField] private TMP_Text txtAviso;
    [SerializeField] private GameObject painelAviso;

    private List<GameObject> itensJogadores = new List<GameObject>();
    #endregion

    #region Métodos do Ciclo de Vida
    private void Start()
    {
        ConfigurarBotoesUI();
        CarregarJogadoresSalvos();
        inputNome.onSubmit.AddListener(_ => AdicionarJogador());
        inputNome.ActivateInputField();
    }

    private void OnDestroy()
    {
        btnAdicionar.onClick.RemoveAllListeners();
        btnComecar.onClick.RemoveAllListeners();
        btnVoltar.onClick.RemoveAllListeners();
        inputNome.onSubmit.RemoveAllListeners();
    }
    #endregion

    #region Configuração da UI
    // Configura os listeners dos botões e estado inicial
    private void ConfigurarBotoesUI()
    {
        btnAdicionar.onClick.AddListener(AdicionarJogador);
        btnComecar.onClick.AddListener(ComecarJogo);
        btnVoltar.onClick.AddListener(() => GerenciadorJogo.Instance.IrParaCena("Categorias"));
        AtualizarEstadoComecar();
        painelAviso.SetActive(false);
    }

    // Carrega jogadores salvos no gerenciador
    private void CarregarJogadoresSalvos()
    {
        foreach (GerenciadorJogo.Jogador jogador in GerenciadorJogo.Instance.jogadores)
        {
            CriarItemJogador(jogador.nome);
        }
        AtualizarEstadoComecar();
    }
    #endregion

    #region Gerenciamento de Jogadores
    // Adiciona um novo jogador à lista
    private void AdicionarJogador()
    {
        string nome = inputNome.text?.Trim();
        if (string.IsNullOrEmpty(nome))
        {
            MostrarAviso("Digite um nome válido!");
            return;
        }

        if (GerenciadorJogo.Instance.jogadores.Count >= maxJogadores)
        {
            MostrarAviso($"Máximo de {maxJogadores} jogadores!");
            return;
        }

        if (GerenciadorJogo.Instance.jogadores.Exists(j => j.nome.Equals(nome, System.StringComparison.OrdinalIgnoreCase)))
        {
            MostrarAviso("Jogador já existe!");
            return;
        }

        GerenciadorJogo.Instance.AdicionarJogador(nome);
        CriarItemJogador(nome);
        inputNome.text = "";
        inputNome.ActivateInputField();
        AtualizarEstadoComecar();
    }

    // Remove um jogador da lista
    private void RemoverJogador(string nome, GameObject item)
    {
        GerenciadorJogo.Instance.RemoverJogador(nome);
        itensJogadores.Remove(item);
        Destroy(item);
        AtualizarEstadoComecar();
    }

    // Cria um item visual para o jogador
    private void CriarItemJogador(string nome)
    {
        GameObject novoItem = Instantiate(itemJogadorPrefab, contentPanel);
        TMP_Text texto = novoItem.GetComponentInChildren<TMP_Text>();
        Button btnRemover = novoItem.GetComponentInChildren<Button>();

        texto.text = nome;
        btnRemover.onClick.AddListener(() => RemoverJogador(nome, novoItem));

        itensJogadores.Add(novoItem);
    }
    #endregion

    #region Ações da UI
    // Inicia o jogo se houver jogadores suficientes
    private void ComecarJogo()
    {
        if (GerenciadorJogo.Instance.jogadores.Count >= 2)
        {
            GerenciadorJogo.Instance.IniciarNovaRodada();
            GerenciadorJogo.Instance.IrParaCena("GamePlay");
        }
    }

    // Atualiza o estado do botão de começar
    private void AtualizarEstadoComecar()
    {
        btnComecar.interactable = GerenciadorJogo.Instance.jogadores.Count >= 2;
    }

    // Exibe uma mensagem de aviso temporária
    private void MostrarAviso(string mensagem)
    {
        txtAviso.text = mensagem;
        painelAviso.SetActive(true);
        Invoke(nameof(EsconderAviso), tempoFeedback);
    }

    // Esconde o painel de aviso
    private void EsconderAviso()
    {
        painelAviso.SetActive(false);
    }
    #endregion
}