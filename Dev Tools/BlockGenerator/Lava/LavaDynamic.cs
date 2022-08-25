using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rock.Lava;

namespace BlockGenerator.Lava
{
    public class LavaDynamic : ILavaDataDictionary
    {
        public List<string> AvailableKeys =>
            GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public )
                .Select( p => p.Name )
                .ToList();

        public bool ContainsKey( string key )
        {
            return AvailableKeys.Contains( key );
        }

        public object GetValue( string key )
        {
            return GetType().GetProperty( key )?.GetValue( this );
        }
    }
}
