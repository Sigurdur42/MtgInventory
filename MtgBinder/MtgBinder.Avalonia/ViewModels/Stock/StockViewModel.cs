using System.Linq;
using System.Reactive;
using MtgBinder.Domain.Mkm;
using ReactiveUI;

namespace MtgBinder.Avalonia.ViewModels.Stock
{
    public class StockViewModel : ReactiveObject
    {
        private MkmStockItem[] _stockItems;
        private string _stockSummary;

        public StockViewModel()
        {
            LoadStock = ReactiveCommand.Create(RunLoadStock);
        }

        public MkmStockItem[] StockItems
        {
            get => _stockItems;
            set => this.RaiseAndSetIfChanged(ref _stockItems, value);
        }

        public string StockSummary
        {
            get => _stockSummary;
            set => this.RaiseAndSetIfChanged(ref _stockSummary, value);
        }

        public ReactiveCommand<Unit, Unit> LoadStock { get; }

        public void RunLoadStock()
        {
            var target = new MkmRequest();
            StockItems = target.GetStockAsCsv(new MkmAuthentication()).ToArray();

            StockSummary = $"{StockItems?.Length} rows ({StockItems.GroupBy(c=>c.IdProduct).Count()} distinct items)" ;
        }
    }
}