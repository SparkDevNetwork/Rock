using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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