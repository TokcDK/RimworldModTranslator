using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Messages
{
    internal class LoadSelectedModStringsMessage(ModData selectedMod)
    {
        public ModData SelectedMod { get; } = selectedMod;
    }
}
