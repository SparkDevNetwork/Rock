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

        public static double Median( this IEnumerable<double> source )
        {
            var sortedList = from number in source
                             orderby number
                             select number;

            int count = sortedList.Count();
            int itemIndex = count / 2;
            if ( count % 2 == 0 ) // Even number of items. 
                return ( sortedList.ElementAt( itemIndex ) +
                        sortedList.ElementAt( itemIndex - 1 ) ) / 2;

            // Odd number of items. 
            return sortedList.ElementAt( itemIndex );
        }

        public static T? Mode<T>( this IEnumerable<T> source ) where T : struct
        {
            var sortedList = from number in source
                             orderby number
                             select number;

            int count = 0;
            int max = 0;
            T current = default( T );
            T? mode = new T?();

            foreach ( T next in sortedList )
            {
                if ( current.Equals( next ) == false )
                {
                    current = next;
                    count = 1;
                }
                else
                {
                    count++;
                }

                if ( count > max )
                {
                    max = count;
                    mode = current;
                }
            }

            if ( max > 1 )
                return mode;

            return null;
        }
    }


}
