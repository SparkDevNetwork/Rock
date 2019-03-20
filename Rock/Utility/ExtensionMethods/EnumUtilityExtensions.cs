using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rock.Utility;

namespace Rock
{
    /// <summary>
    /// System.Enum extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Gets the enum fields sorted by <see cref="EnumOrderAttribute"/>
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns></returns>
        public static IEnumerable<TEnum> GetOrderedValues<TEnum>( this Type enumType ) where TEnum : struct
        {

            var enumFields = enumType.GetFields( BindingFlags.Public | BindingFlags.Static );
            var enumFieldsOrder = new Dictionary<TEnum, int>();
            foreach ( var enumField in enumFields )
            {
                var enumFieldOrder = enumField.GetCustomAttribute<EnumOrderAttribute>()?.Order ?? int.MaxValue;
                enumFieldsOrder.Add( ( TEnum ) enumField.GetValue( null ), enumFieldOrder );
            }

            return enumFieldsOrder.OrderBy( f => f.Value ).ThenBy( f => f.Key ).Select( a => a.Key ).ToList();
        }
    }
}
