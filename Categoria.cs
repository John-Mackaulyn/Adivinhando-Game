[System.Serializable]
public class Categoria
{
    public string nome;
    public bool desbloqueada;

    public Categoria(string nome, bool desbloqueada)
    {
        this.nome = nome;
        this.desbloqueada = desbloqueada;
    }
}