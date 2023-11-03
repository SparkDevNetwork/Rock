﻿// <copyright>
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
using System.Collections.Generic;
using System.Linq;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;

#endif
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) SiteFieldType
    /// Stored as Site.Id
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.SITE )]
    public class SiteFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string SHORTENING_SITES_ONLY = "shorteningSitesOnly";
        private const string VALUES_PUBLIC_KEY = "values";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            var shorteningSitesOnly = publicConfigurationValues.GetValueOrNull( SHORTENING_SITES_ONLY ).AsBoolean();

            var sites = SiteCache.All();
            if ( shorteningSitesOnly )
            {
                sites = sites.Where( s => s.EnabledForShortening )
                    .ToList();
            }

            publicConfigurationValues[VALUES_PUBLIC_KEY] = sites
                .OrderBy( a => a.Name )
                .ToListItemBagList()
                .ToCamelCaseJson( false, true );

            return publicConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );
            privateConfigurationValues.Remove( VALUES_PUBLIC_KEY );
            return privateConfigurationValues;
        }

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // The Site Field Type stores the Site by the SiteId in the datbase. However, the remote devices display it by the Site GUID.
            // So, this method gets the site GUID based on the SiteId.
            var publicValue = SiteCache.GetGuid( privateValue.ToIntSafe() ).ToStringSafe();
            return publicValue;
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            // The Webforms version of the Site Field Type used to store the Id of the Site in the database.
            // We are replicating the same behavior in the Obsidian version for backwards compactibility.
            var privateValue = SiteCache.GetId( publicValue.AsGuid() ).ToStringSafe();
            return privateValue;
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            if ( int.TryParse( privateValue, out int id ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var site = new SiteService( rockContext ).GetNoTracking( id );
                    if ( site != null )
                    {
                        formattedValue = site.Name;
                    }
                }
            }

            return formattedValue;
        }

        #endregion

        #region Edit Control 

        #endregion

        #region Entity Methods

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
            int? id = value.AsIntegerOrNull();
            if ( id.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new SiteService( rockContext ).Get( id.Value );
            }

            return null;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var id = privateValue.AsIntegerOrNull();

            if ( !id.HasValue )
            {
                return null;
            }

            // Query the cache to make sure the id is valid.
            var siteId = SiteCache.Get( id.Value )?.Id;

            if ( !siteId.HasValue )
            {
                return null;
            }

            return new List<ReferencedEntity>
            {
                new ReferencedEntity( EntityTypeCache.GetId<Site>().Value, siteId.Value )
            };
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Site and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Site>().Value, nameof( Site.Name ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( SHORTENING_SITES_ONLY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Shortening Sites Only
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Shortening Enabled Sites Only";
            cb.Text = "Yes";
            cb.Help = "Should only sites that are enabled for shortening be displayed.";

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
            configurationValues.Add( SHORTENING_SITES_ONLY, new ConfigurationValue( "Shortening Enabled Sites Only", "Should only sites that are enabled for shortening be displayed.", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockCheckBox )
                {
                    configurationValues[SHORTENING_SITES_ONLY].Value = ( ( RockCheckBox ) controls[0] ).Checked.ToString();
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockCheckBox && configurationValues.ContainsKey( SHORTENING_SITES_ONLY ) )
                {
                    ( ( RockCheckBox ) controls[0] ).Checked = configurationValues[SHORTENING_SITES_ONLY].Value.AsBoolean();
                }
            }
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
            var editControl = new RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            SiteService siteService = new SiteService( new RockContext() );
            var siteQry = siteService.Queryable();

            bool shorteningSitesOnly = false;
            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( SHORTENING_SITES_ONLY ) )
                {
                    shorteningSitesOnly = configurationValues[SHORTENING_SITES_ONLY].Value.AsBoolean();
                }
            }

            if ( shorteningSitesOnly )
            {
                siteQry = siteQry.Where( s => s.EnabledForShortening );
            }

            var siteList = siteQry.OrderBy( a => a.Name ).ToList();

            if ( siteList.Any() )
            {
                foreach ( var site in siteList )
                {
                    editControl.Items.Add( new ListItem( site.Name, site.Id.ToString() ) );
                }

                return editControl;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as int )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<string> values = new List<string>();

            DropDownList dropDownList = control as DropDownList;

            if ( dropDownList != null )
            {
                if ( dropDownList.SelectedValue.Equals( None.IdValue ) )
                {
                    return string.Empty;
                }
                else
                {
                    return dropDownList.SelectedValue;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value. ( as int )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                DropDownList dropDownList = control as DropDownList;
                dropDownList.SetValue( value );
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return GetEditValue( control, configurationValues ).AsIntegerOrNull();
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            SetEditValue( control, configurationValues, id.ToString() );
        }

#endif
        #endregion
    }
}