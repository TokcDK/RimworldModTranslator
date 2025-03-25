using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class TranslationRow(string? subPath)
    {
        public string? SubPath { get; } = subPath;

        private string? _key;
        public string? Key { get => _key; }
        public void SetKey(string key)
        {
            _key = key;
        }

        public List<LanguageValueData> Translations { get; } = [];
    }
}
