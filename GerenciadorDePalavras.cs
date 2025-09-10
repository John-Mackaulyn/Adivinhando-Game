using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GerenciadorDePalavras : MonoBehaviour
{
    #region Campos e Propriedades
    public static GerenciadorDePalavras Instance { get; private set; }

    [Header("Configurações")]
    [SerializeField] private TextAsset palavrasJSON;
    [SerializeField] private bool usarPalavrasPadrao;

    private Dictionary<string, List<string>> palavrasPorCategoria = new Dictionary<string, List<string>>();
    private bool inicializado;
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
    private void Inicializar()
    {
        if (usarPalavrasPadrao || palavrasJSON == null)
        {
            CarregarPalavrasPadrao();
        }
        else
        {
            CarregarDeJSON();
        }
        inicializado = true;
    }

    private void CarregarDeJSON()
    {
        try
        {
            BancoPalavrasWrapper wrapper = JsonUtility.FromJson<BancoPalavrasWrapper>(palavrasJSON.text);
            palavrasPorCategoria.Clear();

            if (wrapper?.categorias == null) return;

            foreach (CategoriaJSON categoria in wrapper.categorias)
            {
                if (ValidarCategoria(categoria))
                {
                    string nomeFormatado = FormatadorTexto.FormatarNomeCategoria(categoria.nome);
                    palavrasPorCategoria[nomeFormatado] = categoria.palavras
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p.Trim())
                        .Distinct()
                        .ToList();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao carregar JSON: {e.Message}");
            CarregarPalavrasPadrao();
        }
    }

    private bool ValidarCategoria(CategoriaJSON categoria)
    {
        return categoria != null &&
               !string.IsNullOrWhiteSpace(categoria.nome) &&
               categoria.palavras != null &&
               categoria.palavras.Count > 0;
    }

    private void CarregarPalavrasPadrao()
    {
        palavrasPorCategoria = new Dictionary<string, List<string>>
        {
            { "Animais", new List<string> { "Cachorro", "Gato", "Elefante", "Leão", "Tigre" } },
            { "Frutas", new List<string> { "Maçã", "Banana", "Laranja", "Manga", "Abacaxi" } },
            { "Filmes", new List<string> { "Titanic", "Avatar", "Star Wars", "Jurassic Park", "Matrix" } },
            { "Países", new List<string> { "Brasil", "Argentina", "França", "Japão", "Canadá" } },
            { "Esportes", new List<string> { "Futebol", "Basquete", "Tênis", "Natação", "Vôlei" } }
        };
    }
    #endregion

    #region Métodos Públicos
    public List<string> ObterPalavrasCategoria(string categoria)
    {
        if (!inicializado || string.IsNullOrWhiteSpace(categoria))
        {
            return new List<string>();
        }

        string nomeFormatado = FormatadorTexto.FormatarNomeCategoria(categoria);
        if (palavrasPorCategoria.TryGetValue(nomeFormatado, out List<string> palavras))
        {
            List<string> listaEmbaralhada = new List<string>(palavras);
            listaEmbaralhada.Shuffle();
            return listaEmbaralhada;
        }
        return new List<string>();
    }

    public List<string> GetCategoriasDisponiveis()
    {
        return inicializado ? palavrasPorCategoria.Keys.OrderBy(k => k).ToList() : new List<string>();
    }

    public bool EstaInicializado() => inicializado;
    #endregion

    #region Classes Internas
    [System.Serializable]
    private class BancoPalavrasWrapper
    {
        public List<CategoriaJSON> categorias;
    }

    [System.Serializable]
    private class CategoriaJSON
    {
        public string nome;
        public List<string> palavras;
    }
    #endregion
}
public static class ListExtensions
{
    #region Métodos de Extensão
    private static readonly System.Random rng = new System.Random();


    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null) return;

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    #endregion
}