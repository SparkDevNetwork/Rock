using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Data
{
    /// <summary>
    /// Custom attribute used to decorate model classes that don't inherit Entity&lt;T&gt; that should be displayed in the ModelMap in Rock. 
    /// Implements the <see cref="System.Attribute" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Class )]
    public class IncludeForModelMapAttribute : System.Attribute
    {
    }
}
