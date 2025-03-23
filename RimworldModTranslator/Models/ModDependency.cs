using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class ModDependency
    {
        public string? PackageId { get; set; }
        public string? DisplayName { get; set; }
        public string? SteamWorkshopUrl { get; set; }
        public string? DownloadUrl { get; set; }
    }
}
