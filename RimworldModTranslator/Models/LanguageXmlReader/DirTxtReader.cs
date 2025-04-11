using RimworldModTranslator.Models;
using System.Collections.Generic;
using System.IO;

namespace RimworldModTranslator.Helpers
{
    internal partial class EditorHelper
    {
        class DirTxtReader(string languageName, string languageDirPath, EditorStringsData stringsData) : DirXmlReader(languageName, languageDirPath, stringsData)
        {
            protected override string Ext
            {
                get => ".txt";
            }

            protected override void ReadStrings(string[] lines, string subPath, StringsIdsBySubPath stringIdsList, bool skipMissingIds = false)
            {
                var fileName = Path.GetFileNameWithoutExtension(subPath);
                EditorHelper.ReadTxtStringsFile(lines, fileName, languageName, stringIdsList);
            }
        }
    }
}
