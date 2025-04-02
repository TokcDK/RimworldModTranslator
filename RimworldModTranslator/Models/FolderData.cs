using System.Collections.Generic;
using System.Data;
using System.Windows.Documents;

namespace RimworldModTranslator.Models
{
    public class FolderData
    {
        public string Name { get; set; }
        public DataTable? TranslationsTable { get; set; }

        public List<string> SupportedVersion { get; set; } = new();
    }
}