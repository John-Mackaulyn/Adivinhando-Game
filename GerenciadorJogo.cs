using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GerenciadorJogo : MonoBehaviour
{
    #region Campos e Propriedades
    public static GerenciadorJogo Instance { get; private set; }

    [Header("Configurações")]
    [SerializeField] private string versao = "1.0.0";
    [SerializeField] private int categoriasIniciaisDesbloqueadas = 3;
    [SerializeField] private bool debugMode = true;

    [Header("Dados do Jogo")]
    public string categoriaSelecionada;
    public Dictionary<string, bool> categoriasDesbloqueadas = new Dictionary<string, bool>();
    public List<string> palavrasCategoriaAtual = new List<string>();
    public int palavraAtualIndex = -1;
    public List<Jogador> jogadores = new List<Jogador>();
    public int jogadorAtualIndex;
    public bool jogoAtivo;
    public bool usuarioLogado;
    public bool compraRealizada;
    public Dictionary<string, int> classificacaoTotal = new Dictionary<string, int>();
    private HashSet<int> jogadoresQueJogaram = new HashSet<int>();
    private bool dadosCarregados;
    #endregion

    #region Classes Internas
    [System.Serializable]
    public class Jogador
    {
        public string nome;
        public int pontos;
        public int palavrasAcertadas;

        public Jogador(string nome)
        {
            this.nome = nome;
            pontos = 0;
            palavrasAcertadas = 0;
        }
    }

    [System.Serializable]
    private class SaveData
    {
        public List<CategoriaSave> categorias;
        public List<JogadorSave> jogadores;
        public List<ClassificacaoSave> classificacao;
        public bool usuarioLogado;
        public bool compraRealizada;
    }

    [System.Serializable]
    private class CategoriaSave
    {
        public string nome;
        public bool desbloqueada;
    }

    [System.Serializable]
    private class JogadorSave
    {
        public string nome;
        public int pontos;
        public int palavrasAcertadas;
    }

    [System.Serializable]
    private class ClassificacaoSave
    {
        public string nome;
        public int pontos;
    }
    #endregion

    #region Métodos do Ciclo de Vida
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Inicializar();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Inicialização
    // Inicializa os dados do jogo
    private void Inicializar()
    {
        CarregarTodosDados();
        if (categoriasDesbloqueadas.Count == 0)
        {
            InicializarCategorias();
        }
        dadosCarregados = true;
    }

    // Inicializa as categorias disponíveis
    private void InicializarCategorias()
    {
        if (GerenciadorDePalavras.Instance == null)
        {
            Debug.LogError("GerenciadorDePalavras não inicializado!");
            return;
        }

        List<string> todasCategorias = GerenciadorDePalavras.Instance.GetCategoriasDisponiveis();
        foreach (string categoria in todasCategorias)
        {
            categoriasDesbloqueadas[categoria] = todasCategorias.IndexOf(categoria) < categoriasIniciaisDesbloqueadas;
        }

        if (debugMode) Debug.Log($"Categorias inicializadas: {string.Join(", ", categoriasDesbloqueadas.Keys)}");
    }
    #endregion

    #region Salvamento e Carregamento
    // Salva todos os dados do jogo
    public void SalvarTodosDados()
    {
        SaveData dados = new SaveData
        {
            categorias = categoriasDesbloqueadas.Select(kvp => new CategoriaSave
            {
                nome = kvp.Key,
                desbloqueada = kvp.Value
            }).ToList(),
            jogadores = jogadores.Select(j => new JogadorSave
            {
                nome = j.nome,
                pontos = j.pontos,
                palavrasAcertadas = j.palavrasAcertadas
            }).ToList(),
            classificacao = classificacaoTotal.Select(kvp => new ClassificacaoSave
            {
                nome = kvp.Key,
                pontos = kvp.Value
            }).ToList(),
            usuarioLogado = usuarioLogado,
            compraRealizada = compraRealizada
        };

        string json = JsonUtility.ToJson(dados);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();

        if (debugMode) Debug.Log("Dados salvos com sucesso!");
    }

    // Carrega todos os dados salvos
    private void CarregarTodosDados()
    {
        if (!PlayerPrefs.HasKey("SaveData"))
        {
            if (debugMode) Debug.Log("Nenhum save encontrado, iniciando novo jogo");
            InicializarCategorias();
            return;
        }

        string json = PlayerPrefs.GetString("SaveData");
        SaveData dados = JsonUtility.FromJson<SaveData>(json);

        categoriasDesbloqueadas = dados.categorias?.ToDictionary(c => c.nome, c => c.desbloqueada) ?? new Dictionary<string, bool>();
        jogadores = dados.jogadores?.Select(j => new Jogador(j.nome) { pontos = j.pontos, palavrasAcertadas = j.palavrasAcertadas }).ToList() ?? new List<Jogador>();
        classificacaoTotal = dados.classificacao?.ToDictionary(c => c.nome, c => c.pontos) ?? new Dictionary<string, int>();
        usuarioLogado = dados.usuarioLogado;
        compraRealizada = dados.compraRealizada;

        if (debugMode) Debug.Log("Dados carregados com sucesso!");
    }
    #endregion

    #region Gerenciamento de Jogo
    // Verifica se uma categoria está desbloqueada
    public bool VerificarCategoriaDesbloqueada(string categoria)
    {
        if (usuarioLogado && compraRealizada) return true;
        return categoriasDesbloqueadas.TryGetValue(categoria, out bool desbloqueada) && desbloqueada;
    }

    // Seleciona uma categoria e carrega suas palavras
    public void SelecionarCategoria(string categoria)
    {
        if (GerenciadorDePalavras.Instance == null || !GerenciadorDePalavras.Instance.EstaInicializado())
        {
            Debug.LogError("GerenciadorDePalavras não está inicializado!");
            return;
        }

        categoriaSelecionada = categoria;
        palavrasCategoriaAtual = GerenciadorDePalavras.Instance.ObterPalavrasCategoria(categoria);
        palavraAtualIndex = -1;

        if (palavrasCategoriaAtual == null || palavrasCategoriaAtual.Count == 0)
        {
            Debug.LogError($"Nenhuma palavra encontrada para a categoria: {categoria}");
            palavrasCategoriaAtual = new List<string>();
            return;
        }

        if (debugMode) Debug.Log($"Categoria selecionada: {categoria}, Palavras carregadas: {palavrasCategoriaAtual.Count}");
        ProximaPalavra();
    }

    // Inicia uma nova rodada
    public void IniciarNovaRodada()
    {
        foreach (Jogador jogador in jogadores)
        {
            jogador.pontos = 0;
            jogador.palavrasAcertadas = 0;
        }

        palavraAtualIndex = -1;
        jogoAtivo = true;
        jogadorAtualIndex = Random.Range(0, jogadores.Count);
        jogadoresQueJogaram.Clear();
        jogadoresQueJogaram.Add(jogadorAtualIndex);

        if (palavrasCategoriaAtual.Count > 0)
        {
            ProximaPalavra();
        }
        else
        {
            Debug.LogError("Nenhuma palavra disponível para iniciar a rodada!");
        }
    }

    // Adiciona um novo jogador
    public void AdicionarJogador(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || jogadores.Any(j => j.nome.Equals(nome, System.StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        jogadores.Add(new Jogador(nome));
        classificacaoTotal.TryAdd(nome, 0);
        SalvarTodosDados();
    }

    // Remove um jogador
    public void RemoverJogador(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome)) return;

        jogadores.RemoveAll(j => j.nome.Equals(nome, System.StringComparison.OrdinalIgnoreCase));
        SalvarTodosDados();
    }

    // Registra um acerto
    public void Acertou()
    {
        if (jogadorAtualIndex >= 0 && jogadorAtualIndex < jogadores.Count)
        {
            Jogador jogador = jogadores[jogadorAtualIndex];
            jogador.pontos += 10;
            jogador.palavrasAcertadas++;
            classificacaoTotal[jogador.nome] = jogador.pontos;
        }
    }

    // Registra um pulo
    public void Pular()
    {
        if (jogadorAtualIndex >= 0 && jogadorAtualIndex < jogadores.Count)
        {
            Jogador jogador = jogadores[jogadorAtualIndex];
            jogador.pontos = Mathf.Max(0, jogador.pontos - 5);
            classificacaoTotal[jogador.nome] = jogador.pontos;
        }
    }

    // Avança para a próxima palavra
    public void ProximaPalavra()
    {
        if (palavrasCategoriaAtual == null || palavrasCategoriaAtual.Count == 0)
        {
            Debug.LogWarning("Nenhuma palavra disponível para avançar!");
            palavraAtualIndex = -1;
            return;
        }

        palavraAtualIndex = (palavraAtualIndex + 1) % palavrasCategoriaAtual.Count;
        if (debugMode) Debug.Log($"Próxima palavra: {GetPalavraAtual()}, Índice: {palavraAtualIndex}");
    }

    // Obtém a palavra atual
    public string GetPalavraAtual()
    {
        if (palavrasCategoriaAtual == null || palavrasCategoriaAtual.Count == 0)
        {
            if (debugMode) Debug.LogWarning("Lista de palavras vazia ou não inicializada!");
            return "Sem Palavra";
        }

        if (palavraAtualIndex >= 0 && palavraAtualIndex < palavrasCategoriaAtual.Count)
        {
            return palavrasCategoriaAtual[palavraAtualIndex];
        }

        if (debugMode) Debug.LogWarning($"Índice inválido: {palavraAtualIndex}, Tamanho da lista: {palavrasCategoriaAtual.Count}");
        return "Sem Palavra";
    }

    // Avança para o próximo jogador
    public void IrParaProximoJogador()
    {
        if (jogadoresQueJogaram.Count >= jogadores.Count)
        {
            jogoAtivo = false;
            SalvarTodosDados();
            IrParaCena("Classificacao");
            return;
        }

        List<int> disponiveis = Enumerable.Range(0, jogadores.Count)
            .Where(i => !jogadoresQueJogaram.Contains(i))
            .ToList();

        if (disponiveis.Count > 0)
        {
            jogadorAtualIndex = disponiveis[Random.Range(0, disponiveis.Count)];
            jogadoresQueJogaram.Add(jogadorAtualIndex);
            if (palavrasCategoriaAtual.Count > 0)
            {
                ProximaPalavra();
            }
            else
            {
                Debug.LogError("Nenhuma palavra disponível para o próximo jogador!");
            }
        }
    }
    #endregion

    #region Navegação e Controle
    // Navega para a cena especificada
    public void IrParaCena(string cena)
    {
        if (string.IsNullOrWhiteSpace(cena)) return;

        SalvarTodosDados();
        SceneManager.LoadScene(cena);
    }

    // Obtém o ranking final
    public List<Jogador> GetRankingFinal()
    {
        return jogadores.OrderByDescending(j => j.pontos).ToList();
    }

    // Sai do jogo
    public void SairDoJogo()
    {
        SalvarTodosDados();
        Application.Quit();
    }

    // Alterna o estado do som
    public void MutarSom()
    {
        AudioListener.volume = AudioListener.volume > 0 ? 0 : 1;
        PlayerPrefs.SetInt("SomMutado", (int)AudioListener.volume);
    }

    // Registra o status de login
    public void RealizarLogin(bool sucesso)
    {
        usuarioLogado = sucesso;
        SalvarTodosDados();
    }

    // Registra uma compra
    public void RealizarCompra(bool sucesso)
    {
        compraRealizada = sucesso;
        if (sucesso)
        {
            foreach (string key in categoriasDesbloqueadas.Keys.ToList())
            {
                categoriasDesbloqueadas[key] = true;
            }
            SalvarTodosDados();
        }
    }

    // Verifica se o gerenciador está inicializado
    public bool EstaInicializado()
    {
        return dadosCarregados &&
               GerenciadorDePalavras.Instance != null &&
               GerenciadorDePalavras.Instance.EstaInicializado();
    }
    #endregion
}