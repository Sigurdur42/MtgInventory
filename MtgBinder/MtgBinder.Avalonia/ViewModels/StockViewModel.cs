using System.Linq;
using System.Reactive;
using MtgBinder.Domain.Mkm;
using ReactiveUI;

namespace MtgBinder.Avalonia.ViewModels
{
    public class StockViewModel : ReactiveObject
    {
        private MkmStockItem[] _stockItems;

        public StockViewModel()
        {
            LoadStock = ReactiveCommand.Create(RunLoadStock);
        }

        public MkmStockItem[] StockItems
        {
            get => _stockItems;
            set => this.RaiseAndSetIfChanged(ref _stockItems, value);
        }

        public ReactiveCommand<Unit, Unit> LoadStock { get; }

        // CurrentStock

        public void RunLoadStock()
        {
            var target = new MkmRequest();
            StockItems = target.GetStockAsCsv(new MkmAuthentication()).ToArray();
        }
    }
}