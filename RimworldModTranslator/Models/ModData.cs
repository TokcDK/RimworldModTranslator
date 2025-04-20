using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class ModData
    {
        public ModData(Game parentGame)
        {
            ParentGame = parentGame;
        }

        internal Game ParentGame { get; }

        public bool IsActive { get; set; }
        public string? ModDisplayingName => string.IsNullOrEmpty(About?.Name) ? DirectoryName : About.Name;
        public string? DirectoryName { get; set; }
        public AboutData? About { get; set; }    
    }
}
