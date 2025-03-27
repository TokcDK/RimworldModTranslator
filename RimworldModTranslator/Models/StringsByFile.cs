using RimworldModTranslator.Models;
using System.Collections.Generic;

namespace RimworldModTranslator.ViewModels
{



    public partial class TranslationEditorViewModel
    {
        public class StringsByFile
        {
            // Dictionary<StringId, ListLanguageValuePair>
            public Dictionary<string, List<LanguageValueData>> Strings { get; set; } = [];
        }
        #endregion
    }
}