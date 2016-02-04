using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTester
{
    public static class MoreExtensions
    {
        public static string AsDelimited<T>( this List<T> items, string delimiter, string finalDelimiter = null )
        {
            List<string> strings = new List<string>();
            foreach ( T item in items )
            {
                strings.Add( item.ToString() );
            }

            if ( finalDelimiter != null && strings.Count > 1 )
            {
                return String.Join( delimiter, strings.Take( strings.Count - 1 ).ToArray() ) + string.Format( "{0}{1}", finalDelimiter, strings.Last() );
            }
            else
            {
                return String.Join( delimiter, strings.ToArray() );
            }
        }
    }
}
