// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) content channel item filtered by a selected content channel
    /// </summary>
    public class ContentChannelItemFieldType : FieldType
    {

        #region Configuration

        /// <summary>
        /// Configuration Key for Content Channel
        /// </summary>
        public static readonly string CONTENT_CHANNEL_KEY = "contentchannel";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( CONTENT_CHANNEL_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of defined types (the one that gets selected is
            // used to build a list of defined values) 
            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.EnhanceForLongLists = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Content Channel";
            ddl.Help = "Content Channel to select items from, if left blank any content channel's item can be selected.";

            ddl.Items.Add( new ListItem() );
            
            var contentChannels = ContentChannelCache.All().OrderBy( a => a.Name ).ToList();
            contentChannels.ForEach( g =>
                ddl.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) )
            );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( CONTENT_CHANNEL_KEY, new ConfigurationValue( "Content Channel", "Content Channel to select items from, if left blank any content channel's item can be selected.", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is DropDownList )
                {
                    configurationValues[CONTENT_CHANNEL_KEY].Value = ( ( DropDownList ) controls[0] ).SelectedValue;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( CONTENT_CHANNEL_KEY ) )
                {
                    ( ( DropDownList ) controls[0] ).SelectedValue = configurationValues[CONTENT_CHANNEL_KEY].Value;
                }
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            Guid guid = Guid.Empty;
            if ( Guid.TryParse( value, out guid ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var contentChannelItem = new ContentChannelItemService( rockContext ).GetNoTracking( guid );
                    if ( contentChannelItem != null )
                    {
                        formattedValue = contentChannelItem.Title;
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion

        #region Edit Control

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
            ContentChannelItemPicker editControl = new ContentChannelItemPicker { ID = id };

            if ( configurationValues != null && configurationValues.ContainsKey( CONTENT_CHANNEL_KEY ) )
            {
                int contentChannelId = 0;
                if ( Int32.TryParse( configurationValues[CONTENT_CHANNEL_KEY].Value, out contentChannelId ) && contentChannelId > 0 )
                {
                    editControl.ContentChannelId = contentChannelId;
                }
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as ContentChannelItemPicker;
            if ( picker != null )
            {
                int? itemId = picker.ContentChannelItemId;
                Guid? itemGuid = null;
                if ( itemId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        itemGuid = new ContentChannelItemService( rockContext ).Queryable().AsNoTracking().Where( a => a.Id == itemId.Value ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
                    }
                }

                return itemGuid?.ToString() ?? string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as ContentChannelItemPicker;
            if ( picker != null )
            {
                int? itemId = null;
                Guid? itemGuid = value.AsGuidOrNull();
                if ( itemGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        itemId = new ContentChannelItemService( rockContext ).Queryable().Where( a => a.Guid == itemGuid.Value ).Select( a => ( int? ) a.Id ).FirstOrDefault();
                    }
                }

                picker.ContentChannelItemId = itemId;
            }
        }

        #endregion
    }
}