using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RimworldModTranslator.ViewModels
{
    public partial class SearchOptionsData : ObservableObject
    {
        [ObservableProperty]
        private string _searchWhat;

        [ObservableProperty]
        private string _replaceWith;

        [ObservableProperty]
        private bool _isRegexSearch;

        [ObservableProperty]
        private bool _isCaseSensitive;

        [ObservableProperty]
        private string _selectedColumn;
    }
}
