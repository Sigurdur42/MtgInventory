using System.Collections.Generic;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Services.Images
{
    public interface IMtgImageCache
    {
        string GetImageFile(MtgFullCard card);

        int DownloadMissingImages(IEnumerable<MtgFullCard> cards);
    }
}