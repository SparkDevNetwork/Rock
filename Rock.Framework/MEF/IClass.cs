using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.MEF
{
    public interface IClass : Rock.Attribute.IHasAttributes
    {
        int Order { get; }
    }

    public interface IClassData
    {
        string ClassName { get; }
    }
}
