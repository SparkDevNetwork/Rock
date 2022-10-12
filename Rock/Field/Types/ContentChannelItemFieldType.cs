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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) content channel item filtered by a selected content channel
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CONTENT_CHANNEL_ITEM )]
    public class ContentChannelItemFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
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
            contentChannels.ForEach( g => ddl.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) ) );

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
            configurationValues.Add( CONTENT_CHANNEL_KEY, new ConfigurationValue( "Content Channel", "Content Channel to select items from, if left blank any content channel's item can be selected.", string.Empty ) );

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

        /// <inheritdoc />
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();
            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var contentChannelItem = new ContentChannelItemService( rockContext ).GetNoTracking( guid.Value );
                    if ( contentChannelItem != null )
                    {
                        return contentChannelItem.Title;
                    }
                }
            }

            return string.Empty;
        }

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
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
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
                if ( int.TryParse( configurationValues[CONTENT_CHANNEL_KEY].Value, out contentChannelId ) && contentChannelId > 0 )
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
                int? configurationChannelId = configurationValues.ContainsKey( CONTENT_CHANNEL_KEY ) ? configurationValues[CONTENT_CHANNEL_KEY].Value.AsIntegerOrNull() : null;
                Guid? itemGuid = null;
                if ( itemId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        // If the configuration Content Channel has a selected value, include that value in the clause.
                        if ( configurationChannelId.HasValue )
                        {
                            itemGuid = new ContentChannelItemService( rockContext ).Queryable().AsNoTracking()
                                .Where( a => a.Id == itemId.Value
                                && a.ContentChannelId == configurationChannelId.Value )
                                .Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
                        }
                        else
                        {
                            itemGuid = new ContentChannelItemService( rockContext ).Queryable().AsNoTracking()
                                .Where( a => a.Id == itemId.Value )
                                .Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
                        }
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

        #region IEntityFieldType
        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new ContentChannelItemService( new RockContext() ).Get( guid );
            return item != null ? item.Id : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new ContentChannelItemService( new RockContext() ).Get( id ?? 0 );
            var guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            var guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new ContentChannelItemService( rockContext ).Get( guid.Value );
            }

            return null;
        }
        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemId = new ContentChannelItemService( rockContext ).GetId( guid.Value );

                if ( !contentChannelItemId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>()
                {
                    new ReferencedEntity( EntityTypeCache.GetId<ContentChannelItem>().Value, contentChannelItemId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<ContentChannelItem>().Value, nameof( ContentChannelItem.Title ) ),
            };
        }

        #endregion
    }
}