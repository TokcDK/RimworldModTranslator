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
    internal class T
    {
        static Catalog GetGlobal()
        {
            var localesDir = Path.Combine(Application.Current.StartupUri.LocalPath, "Locale");
            return new Catalog("rmt", localesDir);
        }

        private static readonly Catalog _catalog = GetGlobal();

        public static string _(string text) => _catalog.GetString(text);

        public static string _(string text, params object[] args) => _catalog.GetString(text, args);

        public static string _n(string text, string pluralText, long n) => _catalog.GetPluralString(text, pluralText, n);

        public static string _n(string text, string pluralText, long n, params object[] args) => _catalog.GetPluralString(text, pluralText, n, args);

        public static string _p(string context, string text) => _catalog.GetParticularString(context, text);

        public static string _p(string context, string text, params object[] args) => _catalog.GetParticularString(context, text, args);

        public static string _pn(string context, string text, string pluralText, long n) => _catalog.GetParticularPluralString(context, text, pluralText, n);

        public static string _pn(string context, string text, string pluralText, long n, params object[] args) => _catalog.GetParticularPluralString(context, text, pluralText, n, args);
    }
}
