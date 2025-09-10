using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// Gerencia a interface de seleção de categorias no jogo
public class CategoriasControle : MonoBehaviour
{
    #region Campos e Propriedades
    [Header("Referências de UI")]
    [SerializeField] private Button btnMenuPrincipal;
    [SerializeField] private Button btnComecar;
    [SerializeField] private Button btnMenuOpcoes;
    [SerializeField] private Button btnClassificacao;
    [SerializeField] private Button btnSom;
    [SerializeField] private Button btnCompartilhar;
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject botaoCategoriaPrefab;
    [SerializeField] private TMP_Text txtCategoriaSelecionada;
    [SerializeField] private Animator menuOpcoesAnim;
    private bool menuAtivo;

    [Header("Configurações")]
    [SerializeField] private int categoriasIniciaisDesbloqueadas = 3;
    [SerializeField] private float tempoCarregamentoMinimo = 1f;
    [SerializeField] private GameObject loadingIndicator;

    [Header("Cores")]
    [SerializeField] private Color corSelecionado = Color.green;
    [SerializeField] private Color corNormal = Color.white;
    [SerializeField] private Color corBloqueado = Color.gray;
    [SerializeField] private Color corTextoBloqueado = Color.gray;

    [Header("Ícones de Categorias")]
    [SerializeField] private List<Sprite> iconesCategorias;

    private string categoriaSelecionada;
    private List<Button> botoesCategorias = new List<Button>();
    private Dictionary<string, Sprite> mapaIconesCategorias = new Dictionary<string, Sprite>();
    #endregion

    #region Métodos do Ciclo de Vida
    private IEnumerator Start()
    {
        loadingIndicator.SetActive(true);
        btnComecar.interactable = false;
        txtCategoriaSelecionada.text = "Carregando...";

        float tempoInicio = Time.time;
        yield return new WaitUntil(() => 
            GerenciadorJogo.Instance != null &&
            GerenciadorDePalavras.Instance != null &&
            GerenciadorDePalavras.Instance.EstaInicializado());

        float tempoRestante = tempoCarregamentoMinimo - (Time.time - tempoInicio);
        if (tempoRestante > 0)
            yield return new WaitForSeconds(tempoRestante);

        InicializarMapaIcones();
        ConfigurarBotoesUI();
        CarregarCategorias();
        loadingIndicator.SetActive(false);
    }

    private void OnDestroy()
    {
        btnMenuPrincipal.onClick.RemoveAllListeners();
        btnComecar.onClick.RemoveAllListeners();
        btnMenuOpcoes.onClick.RemoveAllListeners();
        btnClassificacao.onClick.RemoveAllListeners();

        foreach (Button btn in botoesCategorias)
        {
            btn.onClick.RemoveAllListeners();
        }
    }
    #endregion

    #region Configuração da UI
    private void ConfigurarBotoesUI()
    {
        btnMenuPrincipal.onClick.AddListener(() => GerenciadorJogo.Instance.IrParaCena("MenuPrincipal"));
        btnComecar.onClick.AddListener(IniciarJogo);
        btnMenuOpcoes.onClick.AddListener(ControleMenu);
        btnClassificacao.onClick.AddListener(IrParaClassificacao);
    }

    private void CarregarCategorias()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        botoesCategorias.Clear();

        List<string> categorias = GerenciadorDePalavras.Instance.GetCategoriasDisponiveis();
        bool primeiraSelecionada = false;

        for (int i = 0; i < categorias.Count; i++)
        {
            string categoria = categorias[i];
            bool desbloqueada = VerificarSeCategoriaEstaDesbloqueada(categoria, i);

            GameObject novoBotao = Instantiate(botaoCategoriaPrefab, contentPanel);
            Button btn = novoBotao.GetComponent<Button>();
            TMP_Text texto = novoBotao.GetComponentInChildren<TMP_Text>();
            Image icone = novoBotao.transform.GetChild(0).GetComponent<Image>();

            texto.text = categoria;
            texto.color = desbloqueada ? Color.white : corTextoBloqueado;
            btn.interactable = desbloqueada;

            if (icone != null && mapaIconesCategorias.TryGetValue(categoria, out Sprite sprite))
            {
                icone.sprite = sprite;
            }
            else if (icone != null)
            {
                Debug.LogWarning($"Ícone não encontrado para a categoria: {categoria}");
            }

            ConfigurarCoresBotao(btn, desbloqueada);
            ConfigurarEventoBotao(btn, categoria, desbloqueada, ref primeiraSelecionada);
        }

        btnComecar.interactable = primeiraSelecionada;
    }
    #endregion

    #region Lógica de Categorias
    private bool VerificarSeCategoriaEstaDesbloqueada(string categoria, int indice)
    {
        return indice < categoriasIniciaisDesbloqueadas ||
               GerenciadorJogo.Instance.compraRealizada ||
               GerenciadorJogo.Instance.usuarioLogado;
    }

    private void ConfigurarCoresBotao(Button btn, bool desbloqueada)
    {
        ColorBlock cores = btn.colors;
        cores.normalColor = desbloqueada ? corNormal : corBloqueado;
        cores.highlightedColor = desbloqueada ? corNormal * 1.1f : corBloqueado * 1.1f;
        cores.pressedColor = desbloqueada ? corSelecionado * 0.9f : corBloqueado * 0.8f;
        cores.selectedColor = desbloqueada ? corSelecionado : corBloqueado;
        btn.colors = cores;
    }

    private void ConfigurarEventoBotao(Button btn, string categoria, bool desbloqueada, ref bool primeiraSelecionada)
    {
        btn.onClick.AddListener(() =>
        {
            if (desbloqueada)
                SelecionarCategoria(categoria, btn);
        });

        botoesCategorias.Add(btn);

        if (desbloqueada && !primeiraSelecionada)
        {
            SelecionarCategoria(categoria, btn);
            primeiraSelecionada = true;
        }
    }

    private void SelecionarCategoria(string categoria, Button botao)
    {
        foreach (Button btn in botoesCategorias)
        {
            if (btn != botao)
            {
                ColorBlock cores = btn.colors;
                cores.normalColor = btn.interactable ? corNormal : corBloqueado;
                btn.colors = cores;
            }
        }

        ColorBlock coresSelecionado = botao.colors;
        coresSelecionado.normalColor = corSelecionado;
        botao.colors = coresSelecionado;

        categoriaSelecionada = categoria;
        txtCategoriaSelecionada.text = $"Categoria: {categoria}";
        GerenciadorJogo.Instance.SelecionarCategoria(categoria);
        btnComecar.interactable = true;
    }
    #endregion

    #region Ações dos Botões
    private void IniciarJogo()
    {
        if (!string.IsNullOrEmpty(categoriaSelecionada))
        {
            GerenciadorJogo.Instance.IrParaCena("Jogadores");
        }
    }

    private void ControleMenu()
    {
        menuOpcoesAnim.Play(menuAtivo ? "MenuCategoriasRetorna" : "MenuCategorias");
        menuAtivo = !menuAtivo;
    }

    private void IrParaClassificacao()
    {
        SceneManager.LoadScene("Classificacao");
    }
    #endregion

    #region Utilitários
    private void InicializarMapaIcones()
    {
        List<string> categorias = GerenciadorDePalavras.Instance.GetCategoriasDisponiveis();
        for (int i = 0; i < categorias.Count && i < iconesCategorias.Count; i++)
        {
            mapaIconesCategorias[categorias[i]] = iconesCategorias[i];
        }
    }
    #endregion
}