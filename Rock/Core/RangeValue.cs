using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Core
{
    public class RangeValue<T>
    {
        public RangeValue(T from, T to)
        {
            this.From = from;
            this.To = to;
        }

        public T From { get; set; }
        public T To { get; set; }
    }
}