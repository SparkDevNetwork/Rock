using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Rock.Models.Cms
{
    public partial class Page
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public enum DisplayInNavWhen
    {
        WhenAllowed = 0,
        Always = 1,
        Never = 2
    }
}
