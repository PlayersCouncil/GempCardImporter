using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorCSVHandler
{
    public static class StringExtensions
    {
        public static string FirstCharUpper(this string input) =>
            input switch
            {
                null => null,
                "" => "",
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
    }
}
