using System;
using MkmApi;
using ReactiveUI;

namespace MtgInventory.Models
{
    public class MkmApiCallStatistics : ReactiveObject, IApiCallStatistic
    {
        private DateTime _today = DateTime.Now.Date;
        private int _countToday;
        private int _countTotal;
        private Guid _id;

        public MkmApiCallStatistics()
        {
        }

        public Guid Id { get => _id; set => this.RaiseAndSetIfChanged(ref _id, value); }

        public DateTime Today { get => _today; set => this.RaiseAndSetIfChanged(ref _today, value); }
        public int CountToday { get => _countToday; set => this.RaiseAndSetIfChanged(ref _countToday, value); }
        public int CountTotal { get => _countTotal; set => this.RaiseAndSetIfChanged(ref _countTotal, value); }
    }
}