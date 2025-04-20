using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    internal class EditorStringsDBCache
    {
        public Dictionary<string, LanguageValuePairsData>? IdCache { get; internal set; }
        public Dictionary<string, LanguageValuePairsData>? ValueCache { get; internal set; }
    }
}
