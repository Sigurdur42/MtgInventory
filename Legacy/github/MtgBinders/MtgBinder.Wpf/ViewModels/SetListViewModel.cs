using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.Services;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinder.Wpf.ViewModels
{
    public class SetListViewModel : INotifyPropertyChanged
    {
        private readonly IMtgSetRepository _databaseService;
        private MtgSetInfo _selectedSet;

        public SetListViewModel(
                    IMtgSetRepository databaseService)
        {
            _databaseService = databaseService;
            _databaseService.SetDataUpdated += (sender, args) => UpdateSetData();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<SetListViewModelShowCardsEventArgs> RequestShowSetCards;

        public MtgSetInfo[] Sets { get; private set; }

        public MtgSetInfo SelectedSet
        {
            get => _selectedSet;
            set
            {
                if (_selectedSet != value)
                {
                    _selectedSet = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSet)));
                }
            }
        }

        public void DisplayAllCardsFromSet(string setCode)
        {
            RequestShowSetCards?.Invoke(this, new SetListViewModelShowCardsEventArgs() { SetCode = setCode });
        }

        private void UpdateSetData()
        {
            Sets = _databaseService.SetData;
            SelectedSet = null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sets)));
        }
    }
}