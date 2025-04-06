using RimworldModTranslator.Models;
using System.Collections.Generic;

namespace RimworldModTranslator.Models
{
    public class StringsIdsBySubPath
    {
        // Dictionary<StringId, ListLanguageValuePair>
        public Dictionary<string, LanguageValuePairsData> StringIdLanguageValuePairsList { get; set; } = [];
    }
}