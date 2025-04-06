using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;

namespace RimworldModTranslator.Helpers
{
    internal partial class EditorHelper
    {
        internal abstract class XmlReaderBase(string languageName, string languageDirPath, EditorStringsData stringsData)
        {
            internal void ProcessXmlFiles()
            {
                foreach (var entry in GetEntries())
                {
                    var (xmlSubPath, lines) = GetSubPathLines(entry);
                    if (!stringsData.SubPathStringIdsList.TryGetValue(xmlSubPath, out StringsIdsBySubPath? stringIdsList))
                    {
                        stringIdsList = new();
                        stringsData.SubPathStringIdsList[xmlSubPath] = stringIdsList;
                    }

                    try
                    {
                        EditorHelper.ReadFromTheStringsArray(lines, languageName, stringIdsList);
                    }
                    catch (Exception ex) { }
                }
            }

            protected abstract (string, string[]) GetSubPathLines(object entry);

            protected abstract IEnumerable<object> GetEntries();
        }
    }
}
