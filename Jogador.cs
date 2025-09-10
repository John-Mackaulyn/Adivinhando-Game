public class Jogador
{
    #region Campos e Propriedades
    public string nome;
    public int pontos;
    #endregion

    #region Construtor
    // Inicializa um jogador com nome e pontos zerados
    public Jogador(string nome)
    {
        this.nome = nome?.Trim() ?? "";
        pontos = 0;
    }
    #endregion
}