using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.ValueObjects
{
    public enum MtgCondition
    {
        Unknown = 0,
        Mint,
        NearMint,
        Excellent,
        Good,
        Played,
        Bad
    }
}