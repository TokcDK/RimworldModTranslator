using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class ModsConfigData
    {
        public string? Version { get; set; }
        public List<string> ActiveMods { get; set; } = [];
        public List<string> KnownExpansions { get; set; } = [];
    }
}
