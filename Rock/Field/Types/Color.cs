﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of System.Drawing.Color options
    /// </summary>
    [Serializable]
    public class Color : FieldType
    {
        /// <summary>
        /// Renders the controls neccessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
        {
            DropDownList ddl = new DropDownList();

            Type colors = typeof( System.Drawing.Color );
            PropertyInfo[] colorInfo = colors.GetProperties( BindingFlags.Public | BindingFlags.Static );
            foreach ( PropertyInfo info in colorInfo )
                ddl.Items.Add( new ListItem( info.Name, info.Name ) );

            return ddl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is DropDownList )
                return ( ( DropDownList )control ).SelectedValue;
            return null;
        }

        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is DropDownList )
                ( ( DropDownList )control ).SelectedValue = value;
        }
    }
}