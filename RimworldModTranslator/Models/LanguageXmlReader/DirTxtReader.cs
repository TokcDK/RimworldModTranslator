using RimworldModTranslator.Helpers;
using System.IO;

namespace RimworldModTranslator.Models.LanguageXmlReader
{
    internal class DirTxtReader(string languageName, string languageDirPath, EditorStringsData stringsData) : DirXmlReader(languageName, languageDirPath, stringsData)
    {
        protected override string Ext
        {
            get => ".txt";
        }

        protected override int ReadStrings(string[] lines, string subPath, StringsIdsBySubPath stringIdsList, bool skipMissingIds = false)
        {
            var fileName = Path.GetFileNameWithoutExtension(subPath);
            return EditorHelper.ReadTxtStringsFile(lines, fileName, languageName, stringIdsList);
        }
    }
}
