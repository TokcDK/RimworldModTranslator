using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class TranslationRow
    {
        public string? Type { get; set; }
        public string? Key { get; set; }
        public Dictionary<string, string> Translations { get; set; } = [];
    }
}
