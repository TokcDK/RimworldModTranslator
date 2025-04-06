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
        class TarXmlReader(string languageName, string languageDirPath, EditorStringsData stringsData) : XmlReaderBase(languageName, languageDirPath, stringsData) , IDisposable
        {
            TarArchive? tarArchive;

            public void Dispose()
            {
                tarArchive?.Dispose();
            }

            protected override IEnumerable<object> GetEntries()
            {
                tarArchive = TarArchive.Open(languageDirPath + ".tar");

                return tarArchive.Entries
                    .Where(e => !e.IsDirectory && e.Key != null && e.Key.EndsWith(".xml"));
            }
            protected override (string, string[]) GetSubPathLines(object entry)
            {
                TarArchiveEntry tarArchiveEntry = (TarArchiveEntry)entry;

                using var entryStream = tarArchiveEntry.OpenEntryStream();
                using var reader = new StreamReader(entryStream);
                string subPath = tarArchiveEntry.Key!.Replace('/', '\\');
                string[] lines = reader.ReadToEnd().Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

                return (subPath, lines);
            }
        }
    }
}
