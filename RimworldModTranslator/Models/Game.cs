using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class Game
    {
        public string? Path { get; set; }
        public List<ModData> ModsList { get; set; } = [];
        public List<ModData> SelectedMods { get; set; } = [];
    }
}
