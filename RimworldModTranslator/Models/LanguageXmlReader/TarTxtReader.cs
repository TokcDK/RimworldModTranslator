using RimworldModTranslator.Helpers;
using System.IO;

namespace RimworldModTranslator.Models.LanguageXmlReader
{
    internal class TarTxtReader(string languageName, string languageDirPath, EditorStringsData stringsData) : TarXmlReader(languageName, languageDirPath, stringsData)
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
