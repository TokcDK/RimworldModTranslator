using RimworldModTranslator.Helpers;
using RimworldModTranslator.Properties;
using System;
using System.Collections.Generic;

namespace RimworldModTranslator.Models.LanguageXmlReader
{
    internal abstract class XmlReaderBase(string languageName, string languageDirPath, EditorStringsData stringsData)
    {
        protected bool LoadOnlyExtracted = Settings.Default.LoadOnlyStringsForExtractedIds;

        internal void ProcessFiles()
        {
            bool isXml = Ext == ".xml";
            foreach (var entry in GetEntries())
            {
                var (subPath, lines) = GetSubPathLines(entry);

                bool isDefInjected = isXml && subPath.StartsWith("DefInjected");
                if (isXml && !isDefInjected && !subPath.StartsWith("Keyed"))
                {
                    // was About.xml in language dir
                    continue;
                }
                if (!stringsData.SubPathStringIdsList.TryGetValue(subPath, out StringsIdsBySubPath? stringIdsList))
                {
                    stringIdsList = new();
                    stringsData.SubPathStringIdsList[subPath] = stringIdsList;
                }

                try
                {
                    stringsData.loadedStringsCount += ReadStrings(lines, subPath, stringIdsList, skipMissingIds: LoadOnlyExtracted && isDefInjected);
                }
                catch (Exception ex) { }
            }
        }

        protected virtual int ReadStrings(string[] lines, string subPath, StringsIdsBySubPath stringIdsList, bool skipMissingIds = false)
        {
            return EditorHelper.ReadFromTheStringsArray(lines, languageName, stringIdsList, skipMissingIds: skipMissingIds);
        }
        protected abstract (string, string[]) GetSubPathLines(object entry);

        protected abstract IEnumerable<object> GetEntries();

        protected virtual string Ext { get => ".xml"; }

    }
}
