//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace RockWeb
{
    /// <summary>
    /// Summary description for ExtensionMethods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Binds to enum.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="enumType">Type of the enum.</param>
        public static void BindToEnum( this ListControl listControl, Type enumType )
        {
            var dictionary = new Dictionary<int, string>();
            foreach ( int value in Enum.GetValues( enumType ) )
            {
                dictionary.Add( value, Enum.GetName( enumType, value ) );
            }

            listControl.DataSource = dictionary;
            listControl.DataTextField = "Value";
            listControl.DataValueField = "Key";
            listControl.DataBind();
        }
    }
}