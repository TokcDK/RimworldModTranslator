using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NGettext;

namespace RimworldModTranslator.Services
{
    internal static class T
    {
        static Catalog GetGlobal()
        {
            var localesDir = Path.Combine(Path.GetFullPath(".\\"), "RES", "Locale");
            return new Catalog("rmt", localesDir);
        }

        private static readonly Catalog _catalog = GetGlobal();

        internal static string _(string text) => _catalog.GetString(text);

        internal static string _(string text, params object[] args) => _catalog.GetString(text, args);

        internal static string _n(string text, string pluralText, long n) => _catalog.GetPluralString(text, pluralText, n);

        internal static string _n(string text, string pluralText, long n, params object[] args) => _catalog.GetPluralString(text, pluralText, n, args);

        internal static string _p(string context, string text) => _catalog.GetParticularString(context, text);

        internal static string _p(string context, string text, params object[] args) => _catalog.GetParticularString(context, text, args);

        internal static string _pn(string context, string text, string pluralText, long n) => _catalog.GetParticularPluralString(context, text, pluralText, n);

        internal static string _pn(string context, string text, string pluralText, long n, params object[] args) => _catalog.GetParticularPluralString(context, text, pluralText, n, args);
    }
}
