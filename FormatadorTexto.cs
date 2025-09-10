using System.Globalization;

public static class FormatadorTexto
{
    public static string FormatarNomeCategoria(string nome)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nome.ToLower().Trim());
    }
}