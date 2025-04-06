using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace RimworldModTranslator.Helpers
{
    internal partial class EditorHelper
    {
        internal abstract class XmlReaderBase(string languageName, string languageDirPath, EditorStringsData stringsData)
        {
            internal void ProcessFiles()
            {
                foreach (var entry in GetEntries())
                {
                    var (subPath, lines) = GetSubPathLines(entry);
                    if (!stringsData.SubPathStringIdsList.TryGetValue(subPath, out StringsIdsBySubPath? stringIdsList))
                    {
                        stringIdsList = new();
                        stringsData.SubPathStringIdsList[subPath] = stringIdsList;
                    }

                    try
                    {
                        ReadStrings(lines, subPath, stringIdsList);
                    }
                    catch (Exception ex) { }
                }
            }

            protected virtual void ReadStrings(string[] lines, string subPath, StringsIdsBySubPath stringIdsList)
            {
                EditorHelper.ReadFromTheStringsArray(lines, languageName, stringIdsList);
            }
            protected abstract (string, string[]) GetSubPathLines(object entry);

            protected abstract IEnumerable<object> GetEntries();

            protected virtual string Ext { get => ".xml"; }

        }
    }
}
