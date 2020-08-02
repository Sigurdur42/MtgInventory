using System.Linq;
using MtgBinder.Domain.Database;
using PropertyChanged;

namespace MtgBinder.Database
{
    [AddINotifyPropertyChangedInterface]
    public class CardDatabaseViewModel
    {
        public CardDatabaseViewModel(
            ICardDatabase cardDatabase)
        {
            CardDatabase = cardDatabase;
            CardDatabase.DatabaseInitialized += (sender, args) => UpdateSetStatistics();
            CardDatabase.CardsLoaded += (sender, args) => UpdateSetStatistics();
            CardDatabase.SetsLoaded += (sender, args) => UpdateSetStatistics();
        }

        public ICardDatabase CardDatabase { get; }
        public SetStaticData[] SetStatistics { get; private set; }

        public void UpdateSetStatistics()
        {
            SetStatistics = CardDatabase.Sets
                .FindAll()
                .Select(g =>
                {
                    var setData = new SetStaticData()
                    {
                        SetCode = g.Code,
                        SetName = g.Name,
                        CardsPerSet = g.CardCount,
                        DownloadedCards = CardDatabase.Cards.Count(c=>c.SetCode == g.Code)
                    };
                    return setData;
                })
                .ToArray();

        }
    }
}