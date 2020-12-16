using System;
using MkmApi;
using ReactiveUI;
using ScryfallApiServices;

namespace MtgInventory.Models
{
    public class MkmApiCallStatistics : ReactiveObject, IApiCallStatistic, IScryfallApiCallStatistic
    {
        private DateTime _today = DateTime.Now.Date;
        private int _countToday;
        private int _countTotal;
        private Guid _id;
        private int _scryfallCountToday;
        private int _scryfallCountTotal;

        public MkmApiCallStatistics()
        {
        }

        public Guid Id { get => _id; set => this.RaiseAndSetIfChanged(ref _id, value); }

        public DateTime Today { get => _today; set => this.RaiseAndSetIfChanged(ref _today, value); }
        public int CountToday { get => _countToday; set => this.RaiseAndSetIfChanged(ref _countToday, value); }
        public int CountTotal { get => _countTotal; set => this.RaiseAndSetIfChanged(ref _countTotal, value); }

        public int ScryfallCountToday
        {
            get => _scryfallCountToday;
            set => this.RaiseAndSetIfChanged(ref _scryfallCountToday, value);
        }

        public int ScryfallCountTotal
        {
            get => _scryfallCountTotal;
            set => this.RaiseAndSetIfChanged(ref _scryfallCountTotal, value);
        }
    }
}