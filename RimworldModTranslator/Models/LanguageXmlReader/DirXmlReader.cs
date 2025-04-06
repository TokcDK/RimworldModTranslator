using RimworldModTranslator.Models;
using System.Collections.Generic;
using System.IO;

namespace RimworldModTranslator.Helpers
{
    internal partial class EditorHelper
    {
        class DirXmlReader(string languageName, string languageDirPath, EditorStringsData stringsData) : XmlReaderBase(languageName, languageDirPath, stringsData)
        {
            protected override IEnumerable<object> GetEntries()
            {
                return Directory.EnumerateFiles(languageDirPath, "*.xml", SearchOption.AllDirectories);
            }
            protected override (string, string[]) GetSubPathLines(object entry)
            {
                string file = (string)entry;
                string subPath = Path.GetRelativePath(languageDirPath, file);
                string[] lines = File.ReadAllLines(file);
                return (subPath, lines);
            }
        }
    }
}
