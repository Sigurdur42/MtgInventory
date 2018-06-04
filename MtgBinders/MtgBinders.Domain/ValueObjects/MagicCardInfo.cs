using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.ValueObjects
{
    public class MagicCardInfo
    {
        public string Name { get; set; }
        public string NumberInSet { get; set; }
        public MagicSetInfo Set { get; set; }
    }
}