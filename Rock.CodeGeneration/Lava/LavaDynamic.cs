using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rock.Lava;

namespace Rock.CodeGeneration.Lava
{
    /// <summary>
    /// Dynamic lava object that exposes properties to Lava.
    /// </summary>
    public class LavaDynamic : ILavaDataDictionary
    {
        #region Properties

        /// <summary>
        /// Gets the available keys that can be accessed from Lava.
        /// </summary>
        /// <value>The available keys that can be accessed from Lava.</value>
        public List<string> AvailableKeys =>
            GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public )
                .Select( p => p.Name )
                .ToList();

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key contains key; otherwise, <c>false</c>.</returns>
        public bool ContainsKey( string key )
        {
            return AvailableKeys.Contains( key );
        }

        /// <summary>
        /// Gets the value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value that is referenced by the key.</returns>
        public object GetValue( string key )
        {
            return GetType().GetProperty( key )?.GetValue( this );
        }

        #endregion
    }
}
