using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class Game
    {
        public string? GamePath { get; set; }
        public string? ConfigPath { get; set; }
        public ObservableCollection<ModData> ModsList { get; set; } = [];
        public ObservableCollection<ModData> SelectedMods { get; set; } = [];
    }
}
