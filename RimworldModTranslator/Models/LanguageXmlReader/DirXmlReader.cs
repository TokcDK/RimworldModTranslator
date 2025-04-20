using System.Collections.Generic;
using System.IO;

namespace RimworldModTranslator.Models.LanguageXmlReader
{
    internal class DirXmlReader(string languageName, string languageDirPath, EditorStringsData stringsData) : XmlReaderBase(languageName, languageDirPath, stringsData)
    {
        protected override IEnumerable<object> GetEntries()
        {
            return Directory.EnumerateFiles(languageDirPath, $"*{Ext}", SearchOption.AllDirectories);
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
