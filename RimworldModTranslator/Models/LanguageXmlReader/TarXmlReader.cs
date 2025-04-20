using RimworldModTranslator.Models;
using RimworldModTranslator.Translations;
using SharpCompress.Archives.Tar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using static RimworldModTranslator.Helpers.EditorHelper;
using static System.Net.Mime.MediaTypeNames;

namespace RimworldModTranslator.Helpers
{
    class TarXmlReader(string languageName, string languageDirPath, EditorStringsData stringsData) : XmlReaderBase(languageName, languageDirPath, stringsData), IDisposable
    {
        TarArchive? _tarArchive;

        public void Dispose()
        {
            _tarArchive?.Dispose();
        }

        protected override IEnumerable<object> GetEntries()
        {
            _tarArchive = TarArchive.Open(languageDirPath + ".tar");

            return _tarArchive.Entries
                .Where(e => !e.IsDirectory && e.Key != null && e.Key.EndsWith(Ext));
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
