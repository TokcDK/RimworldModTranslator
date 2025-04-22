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
        public string? ModsDirPath { get; internal set; }
        public string? ConfigDirPath { get; internal set; }
        public ObservableCollection<ModData> ModsList { get; set; } = [];
        public ObservableCollection<ModData> SelectedMods { get; set; } = [];
        public string? GameDirPath { get; internal set; }
        public ModsConfigData? ModsConfig { get; internal set; }
    }
}
