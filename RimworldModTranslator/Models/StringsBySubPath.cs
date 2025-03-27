using RimworldModTranslator.Models;
using System.Collections.Generic;

namespace RimworldModTranslator.ViewModels
{



    public partial class TranslationEditorViewModel
    {
        public class StringsBySubPath
        {
            // Dictionary<StringId, ListLanguageValuePair>
            public Dictionary<string, List<LanguageValueData>> Strings { get; set; } = [];
        }
        #endregion
    }
}