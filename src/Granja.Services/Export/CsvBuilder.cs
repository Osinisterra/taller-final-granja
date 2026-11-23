using System.Globalization;
using System.Text;

namespace Granja.Services.Export;

/// <summary>
/// Construye archivos CSV (RFC 4180): separador coma, decimales con punto (cultura invariante),
/// fechas ISO y codificación UTF-8 con BOM para que Excel muestre bien las tildes.
/// </summary>
internal sealed class CsvBuilder
{
    private readonly StringBuilder _sb = new();

    public CsvBuilder Fila(params object?[] valores)
    {
        _sb.AppendJoin(',', valores.Select(Formatear)).Append("\r\n");
        return this;
    }

    public CsvBuilder FilaVacia()
    {
        _sb.Append("\r\n");
        return this;
    }

    public byte[] ToBytes() => Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(_sb.ToString())).ToArray();

    private static string Formatear(object? valor)
    {
        var texto = valor switch
        {
            null => string.Empty,
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            decimal m => m.ToString("0.###", CultureInfo.InvariantCulture),
            bool b => b ? "Sí" : "No",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => valor.ToString() ?? string.Empty
        };

        return texto.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{texto.Replace("\"", "\"\"")}\""
            : texto;
    }
}
