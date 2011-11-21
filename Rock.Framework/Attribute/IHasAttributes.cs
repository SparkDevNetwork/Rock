using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Attribute
{
    public interface IHasAttributes
    {
        int Id { get; }
        List<Rock.Cms.Cached.Attribute> Attributes { get; set; }
        Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }
    }
}
