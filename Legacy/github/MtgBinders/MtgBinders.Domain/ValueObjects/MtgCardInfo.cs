using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.ValueObjects
{
    public class MtgCardInfo
    {
        public string Name { get; set; }
        public string NumberInSet { get; set; }
        public MtgSetInfo Set { get; set; }
    }
}