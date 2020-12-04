using MtgDatabase.Models;

namespace MtgDatabase
{
    public class FoundMagicCard
    {
        public QueryableMagicCard Card { get; set; } = new QueryableMagicCard();
        public ReprintInfo PrintInfo { get; set; } = new ReprintInfo();
    }
}