using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class ModData
    {
        public string? DirectoryName { get; set; }
        public AboutData? About { get; set; }
        public string? ModDisplayingName => string.IsNullOrEmpty(About?.Name) ? DirectoryName : About.Name;
        public bool IsEnabled { get; set; }
    }
}
