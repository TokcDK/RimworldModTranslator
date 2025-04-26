using System.Collections.Generic;
using System.Data;
using System.Windows.Documents;

namespace RimworldModTranslator.Models
{
    public class FolderData
    {
        public string? Name { get; internal set; }
        public DataTable? TranslationsTable { get; internal set; }

        public List<string> SupportedVersions { get; internal set; } = new();
        public EditorStringsData? StringsData { get; internal set; }
    }
}