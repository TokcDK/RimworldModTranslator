using RimworldModTranslator.Models;
using SharpCompress.Archives.Tar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace RimworldModTranslator.Helpers
{
    internal partial class EditorHelper
    {
        class TarTxtReader(string languageName, string languageDirPath, EditorStringsData stringsData) : TarXmlReader(languageName, languageDirPath, stringsData)
        {
            protected override string Ext
            {
                get => ".txt";
            }

            protected override void ReadStrings(string[] lines, string subPath, StringsIdsBySubPath stringIdsList)
            {
                var fileName = Path.GetFileNameWithoutExtension(subPath);
                EditorHelper.ReadTxtStringsFile(lines, fileName, languageName, stringIdsList);
            }
        }
    }
}
