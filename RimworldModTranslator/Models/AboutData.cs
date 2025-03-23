using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class AboutData
    {
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Url { get; set; }
        public List<string> SupportedVersions { get; set; } = [];
        public string? PackageId { get; set; }
        public string? Description { get; set; }
        public List<string> LoadAfter { get; set; } = [];
        public List<ModDependency> ModDependencies { get; set; } = [];
    }
}
