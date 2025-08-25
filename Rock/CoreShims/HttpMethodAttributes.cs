#if NET6_0_OR_GREATER
using System;

namespace Rock
{
    [Obsolete( "Must be made compatible with both WebForms and AspNetCore." )]
    internal class FromBodyAttribute : System.Attribute
    {
    }
}
#endif
