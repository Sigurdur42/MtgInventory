namespace MtgJson
{
    public class MtgJsonPriceFilter
    {
        public bool HideCardMarket { get; set; }
        public bool HideTcgPlayer { get; set; } = false;
        public bool HideCardKingdom { get; set; } = true;

        public bool HideBuyList { get; set; } = true;
        public bool HideCardHoarder { get; set; } = true;

        public int HistoryDays { get; set; } = 1;
    }
}
