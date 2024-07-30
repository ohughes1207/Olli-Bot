using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OlliBot.Utilities
{
    public class Helpers
    {
        public static bool HasURL(string? input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            var regex = new Regex(@"https?://[^\s/$.?#].[^\s]*");
            return regex.IsMatch(input);
        }
    }
}
