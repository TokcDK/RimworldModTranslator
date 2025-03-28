using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    internal class ModAboutData
    {
        internal string? Name;
        internal string? PackageId;
        internal string? Author;
        internal string? ModVersion;
        internal string? SupportedVersions;
        internal string? Description;
        internal string? Url;
        internal string? Preview;

        public ModData? SourceMod { get; internal set; }
    }
}
