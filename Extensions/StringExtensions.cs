using System.Linq;

namespace Bus.RedeliveryCountError.Sample.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string str)
    {
        str = str.Replace(" ", "_");

        var stringSnake = string.Concat(
            str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString().ToLower() : x.ToString().ToLower())
        );

        return stringSnake;
    }
}