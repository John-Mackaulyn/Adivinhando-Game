using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class JogoControle : MonoBehaviour
{
    #region Campos e Propriedades
    [Header("Referências UI")]
    [SerializeField] private TMP_Text txtTempo;
    [SerializeField] private TMP_Text txtJogador;
    [SerializeField] private TMP_Text txtPontos;
    [SerializeField] private TMP_Text txtPalavra;
    [SerializeField] private TMP_Text txtCategoria;
    [SerializeField] private Button btnAcertar;
    [SerializeField] private Button btnPular;
    [SerializeField] private Button btnVoltar;
    [SerializeField] private GameObject painelInicioTurno;
    [SerializeField] private TMP_Text txtInicioTurno;
    [SerializeField] private GameObject painelPalavraAcertada;
    [SerializeField] private GameObject painelPalavraPulada;
    [SerializeField] private GameObject painelFimTurno;

    [Header("Configurações")]
    [SerializeField] private float tempoTurno = 60f;
    [SerializeField] private float tempoPreTurno = 5f;
    [SerializeField] private float tempoFeedbackPalavra = 1f;
    [SerializeField] private float tempoFimTurno = 3f;

    private TemporizadorJogo temporizador;
    private bool turnoAtivo;
    #endregion

    #region Métodos do Ciclo de Vida
    private void Start()
    {
        temporizador = FindFirstObjectByType<TemporizadorJogo>();
        if (temporizador == null)
        {
            Debug.LogError("TemporizadorJogo não encontrado!");
            return;
        }

        ConfigurarBotoesUI();
        txtCategoria.text = GerenciadorJogo.Instance.categoriaSelecionada;
        IniciarNovoTurno();
    }

    private void Update()
    {
        if (turnoAtivo && !temporizador.pausado)
        {
            AtualizarTempoUI();
        }
    }
    #endregion

    #region Configuração da UI
    // Configura os listeners dos botões
    private void ConfigurarBotoesUI()
    {
        btnVoltar.onClick.AddListener(VoltarParaCategorias);
        btnAcertar.onClick.AddListener(ProcessarAcerto);
        btnPular.onClick.AddListener(ProcessarPulo);
    }
    #endregion

    #region Gerenciamento do Turno
    // Inicia um novo turno
    private void IniciarNovoTurno()
    {
        StartCoroutine(PrepararTurno());
    }

    // Prepara a contagem regressiva para o turno
    private IEnumerator PrepararTurno()
    {
        turnoAtivo = false;
        btnAcertar.interactable = false;
        btnPular.interactable = false;
        painelInicioTurno.SetActive(true);

        GerenciadorJogo.Jogador jogadorAtual = GerenciadorJogo.Instance.jogadores[GerenciadorJogo.Instance.jogadorAtualIndex];
        txtInicioTurno.text = $"{jogadorAtual.nome}, iniciando em: ";

        float tempo = tempoPreTurno;
        while (tempo > 0)
        {
            txtInicioTurno.text = $"{jogadorAtual.nome}, iniciando em: {Mathf.CeilToInt(tempo)}...";
            tempo -= Time.deltaTime;
            yield return null;
        }

        painelInicioTurno.SetActive(false);
        IniciarTurnoPrincipal();
    }

    // Inicia o turno principal
    private void IniciarTurnoPrincipal()
    {
        turnoAtivo = true;
        btnAcertar.interactable = true;
        btnPular.interactable = true;

        temporizador.IniciarTemporizador(tempoTurno, FinalizarTurnoPorTempo);
        AtualizarUI();
    }

    // Finaliza o turno por tempo esgotado
    private void FinalizarTurnoPorTempo()
    {
        if (turnoAtivo)
        {
            StartCoroutine(FinalizarTurno());
        }
    }

    // Finaliza o turno e passa para o próximo jogador
    private IEnumerator FinalizarTurno()
    {
        turnoAtivo = false;
        temporizador.PararTemporizador();

        painelFimTurno.SetActive(true);
        yield return new WaitForSeconds(tempoFimTurno);
        painelFimTurno.SetActive(false);

        GerenciadorJogo.Instance.IrParaProximoJogador();

        if (GerenciadorJogo.Instance.jogoAtivo)
        {
            IniciarNovoTurno();
        }
        else
        {
            TelaDeClassificacao();
        }
    }
    #endregion

    #region Ações do Jogo
    // Processa um acerto de palavra
    private void ProcessarAcerto()
    {
        if (!turnoAtivo) return;

        temporizador.PausarTemporizador();
        GerenciadorJogo.Instance.Acertou();

        painelPalavraAcertada.SetActive(true);
        Invoke(nameof(ContinuarAposFeedback), tempoFeedbackPalavra);
    }

    // Processa um pulo de palavra
    private void ProcessarPulo()
    {
        if (!turnoAtivo) return;

        temporizador.PausarTemporizador();
        GerenciadorJogo.Instance.Pular();

        painelPalavraPulada.SetActive(true);
        Invoke(nameof(ContinuarAposFeedback), tempoFeedbackPalavra);
    }

    // Continua após feedback de acerto ou pulo
    private void ContinuarAposFeedback()
    {
        painelPalavraAcertada.SetActive(false);
        painelPalavraPulada.SetActive(false);

        if (turnoAtivo)
        {
            ProximaPalavra();
            temporizador.ContinuarTemporizador();
        }
    }

    // Avança para a próxima palavra
    private void ProximaPalavra()
    {
        GerenciadorJogo.Instance.ProximaPalavra();
        AtualizarUI();
    }
    #endregion

    #region Atualização da UI
    // Atualiza o tempo restante na UI
    private void AtualizarTempoUI()
    {
        float tempo = temporizador.GetTempoRestante();
        int minutos = Mathf.FloorToInt(tempo / 60f);
        int segundos = Mathf.FloorToInt(tempo % 60f);
        txtTempo.text = $"{minutos:00}:{segundos:00}";
    }

    // Atualiza os elementos visuais da UI
    private void AtualizarUI()
    {
        GerenciadorJogo.Jogador jogador = GerenciadorJogo.Instance.jogadores[GerenciadorJogo.Instance.jogadorAtualIndex];
        txtJogador.text = jogador.nome;
        txtPontos.text = jogador.pontos.ToString();
        txtPalavra.text = GerenciadorJogo.Instance.GetPalavraAtual();
    }
    #endregion

    #region Navegação
    // Volta para a tela de categorias
    private void VoltarParaCategorias()
    {
        temporizador.PararTemporizador();
        GerenciadorJogo.Instance.IrParaCena("Categorias");
    }

    // Vai para a tela de classificação
    private void TelaDeClassificacao()
    {
        temporizador.PararTemporizador();
        GerenciadorJogo.Instance.IrParaCena("Classificacao");
    }
    #endregion
}