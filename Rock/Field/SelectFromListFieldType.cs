// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SelectFromListFieldType : FieldType
    {

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var valueGuidList = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();
            return this.ListSource.Where( a => valueGuidList.Contains( a.Key.AsGuid() ) ).Select( s => s.Value ).ToList().AsDelimited( "," );
        }

        #endregion

        #region Edit Control 

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal abstract Dictionary<string, string> ListSource { get; }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            RockCheckBoxList editControl = new RockCheckBoxList { ID = id };
            editControl.RepeatDirection = RepeatDirection.Horizontal;

            if ( ListSource.Any() )
            {
                foreach ( var item in ListSource )
                {
                    ListItem listItem = new ListItem( item.Value, item.Key );
                    editControl.Items.Add( listItem );
                }

                return editControl;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<string> values = new List<string>();

            if ( control != null && control is RockCheckBoxList )
            {
                RockCheckBoxList cbl = (RockCheckBoxList)control;
                foreach ( ListItem li in cbl.Items )
                    if ( li.Selected )
                        values.Add( li.Value );
                return values.AsDelimited<string>( "," );
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                List<string> values = new List<string>();
                values.AddRange( value.Split( ',' ) );

                if ( control != null && control is RockCheckBoxList )
                {
                    RockCheckBoxList cbl = (RockCheckBoxList)control;
                    foreach ( ListItem li in cbl.Items )
                        li.Selected = values.Contains( li.Value, StringComparer.OrdinalIgnoreCase );
                }
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.ContainsFilterComparisonTypes;
            }
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required"></param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            var ddlList = new RockDropDownList();
            ddlList.ID = string.Format( "{0}_ddlList", id );
            ddlList.AddCssClass( "js-filter-control" );

            if ( !required )
            {
                ddlList.Items.Add( new ListItem() );
            }

            if ( ListSource.Any() )
            {
                foreach ( var item in ListSource )
                {
                    ListItem listItem = new ListItem( item.Value, item.Key );
                    ddlList.Items.Add( listItem );
                }

                return ddlList;
            }

            return null;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is RockDropDownList )
            {
                return ( (RockDropDownList)control ).SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is RockDropDownList )
            {
                ( (RockDropDownList)control ).SetValue( value );
            }
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var values = new List<string>();
            foreach ( string key in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                if ( ListSource.ContainsKey(key))
                {
                    values.Add( ListSource[key] );
                }
            }

            return values.Select( v => "'" + v + "'" ).ToList().AsDelimited( " or " );
        }

        #endregion

    }
}