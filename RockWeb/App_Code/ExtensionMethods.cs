//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;

namespace RockWeb
{
    /// <summary>
    /// Summary description for ExtensionMethods
    /// </summary>
    public static class ExtensionMethods
    {
        public static void BindToEnum( this System.Web.UI.WebControls.ListControl listControl, Type enumType )
        {
            var dictionary = new Dictionary<int, string>();
            foreach ( int value in Enum.GetValues( enumType ) )
                dictionary.Add( value, Enum.GetName( enumType, value ) );

            listControl.DataSource = dictionary;
            listControl.DataTextField = "Value";
            listControl.DataValueField = "Key";
            listControl.DataBind();
        }
    }
}