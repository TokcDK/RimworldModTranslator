using RimworldModTranslator.Models;
using System.Collections.Generic;

namespace RimworldModTranslator.ViewModels
{



    public partial class TranslationEditorViewModel
    {
        public class StringsIdsBySubPath
        {
            // Dictionary<StringId, ListLanguageValuePair>
            public Dictionary<string, LanguageValuePairsData> StringIdLanguageValuePairsList { get; set; } = [];
        }
    }
}