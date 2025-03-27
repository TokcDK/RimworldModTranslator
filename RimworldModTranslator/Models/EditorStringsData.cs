using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RimworldModTranslator.ViewModels.TranslationEditorViewModel;

namespace RimworldModTranslator.Models
{
    internal class EditorStringsData
    {
        // for search by SubPath and StringId By the path
        // Dictionary<SubPath, ListStringIdValuesForEachLanguage>
        internal Dictionary<string, List<StringsBySubPath>> StringsData = [];
    }
}
