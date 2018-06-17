using MtgBinders.Domain.Services;
using MtgBinders.Domain.ValueObjects;
using System;
using System.ComponentModel;
using System.Linq;

namespace MtgBinder.Wpf.ViewModels
{
    internal class MainCardSearchViewModel : INotifyPropertyChanged
    {
        private readonly IMtgDatabaseService _mtgDatabase;

        private string _searchPattern;

        public MainCardSearchViewModel(
               IMtgDatabaseService mtgDatabase)
        {
            _mtgDatabase = mtgDatabase;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string SearchPattern
        {
            get
            {
                return _searchPattern;
            }

            set
            {
                if (_searchPattern != value)
                {
                    _searchPattern = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchPattern)));
                }
            }
        }

        public MtgFullCard[] FoundCards { get; private set; }

        public void StartCardSearch()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_searchPattern))
                {
                    FoundCards = new MtgFullCard[0];
                    return;
                }

                // TODO: Encapsulate search in class
                FoundCards = _mtgDatabase.CardData
                    .Where(c => c.Name.IndexOf(_searchPattern, 0, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    .OrderBy(c => c.Name)
                    .ToArray();
            }
            finally
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FoundCards)));
            }
        }
    }
}