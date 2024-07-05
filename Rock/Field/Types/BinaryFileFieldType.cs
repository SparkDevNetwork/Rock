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
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Utility;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of binary files of a specific type
    /// Stored as BinaryFile's Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.BINARY_FILE )]
    public class BinaryFileFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string BINARY_FILE_TYPE = "binaryFileType";

        private const string BINARY_FILE_TYPES_PROPERTY_KEY = "binaryFileTypes";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicEditConfigurationProperties( Dictionary<string, string> privateConfigurationValues )
        {
            var configurationProperties = new Dictionary<string, string>();

            using ( var rockContext = new RockContext() )
            {
                var binaryFileTypes = new BinaryFileTypeService( rockContext )
                    .Queryable()
                    .OrderBy( t => t.Name )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.Name
                    } )
                    .ToList();

                configurationProperties[BINARY_FILE_TYPES_PROPERTY_KEY] = binaryFileTypes.ToCamelCaseJson( false, true );
            }

            return configurationProperties;
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Get the formatted value as either a plain text string or an HTML formatted string.
        /// </summary>
        /// <param name="privateValue">The value to be formatted.</param>
        /// <param name="formatAsHtml"><c>true</c> if the output should be formatted as HTML; otherwise <c>false</c>.</param>
        /// <returns>A string that represents the value.</returns>
        private string GetFormattedValue( string privateValue, bool formatAsHtml )
        {
            string formattedValue = string.Empty;
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue || guid.Value.IsEmpty() )
            {
                return "";
            }

            using ( var rockContext = new RockContext() )
            {
                var binaryFileInfo = new BinaryFileService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( f => f.Guid == guid.Value )
                    .Select( f => new
                    {
                        f.FileName,
                        f.Guid
                    } )
                    .FirstOrDefault();

                if ( binaryFileInfo == null )
                {
                    return "";
                }

                if ( !formatAsHtml )
                {
                    return binaryFileInfo.FileName;
                }
                else
                {
                    var filePath = FileUrlHelper.GetFileUrl( binaryFileInfo.Guid );
                    return string.Format( "<a href='{0}' title='{1}' class='btn btn-xs btn-default'>View</a>", filePath, System.Web.HttpUtility.HtmlEncode( binaryFileInfo.FileName ) );
                }
            }
        }

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetFormattedValue( privateValue, false );
        }

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetFormattedValue( privateValue, true );
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue || guid.Value.IsEmpty() )
            {
                return "";
            }

            using ( var rockContext = new RockContext() )
            {
                var binaryFileInfo = new BinaryFileService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( f => f.Guid == guid.Value )
                    .Select( f => new
                    {
                        f.FileName,
                        f.Guid
                    } )
                    .FirstOrDefault();

                if ( binaryFileInfo == null )
                {
                    return "";
                }

                // A binary file needs more than just the Guid to properly display
                // in most cases, so include the guid and the filename.
                return new ListItemBag
                {
                    Value = binaryFileInfo.Guid.ToString(),
                    Text = binaryFileInfo.FileName
                }.ToCamelCaseJson( false, true );
            }
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Extract the raw value.
            return publicValue.FromJsonOrNull<ListItemBag>()?.Value ?? string.Empty;
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
        }

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
                // This field type only supports IsBlank and IsNotBlank since the content is stored as binarydata
                return ComparisonType.IsBlank | ComparisonType.IsNotBlank;
            }
        }

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
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new BinaryFileService( rockContext ).Get( guid.Value );
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
                var binaryFileId = new BinaryFileService( rockContext ).GetId( guid.Value );

                if ( !binaryFileId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<BinaryFile>().Value, binaryFileId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the FileName property of a BinaryFile and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<BinaryFile>().Value, nameof( BinaryFile.FileName ) )
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
            configKeys.Add( BINARY_FILE_TYPE );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Items.Clear();
            ddl.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var ft in new BinaryFileTypeService( new RockContext() )
                .Queryable()
                .OrderBy( f => f.Name )
                .Select( f => new { f.Guid, f.Name } ) )
            {
                ddl.Items.Add( new ListItem( ft.Name, ft.Guid.ToString().ToLower() ) );
            }
            ddl.Label = "File Type";
            ddl.Help = "File type to use to store and retrieve the file. New file types can be configured under 'Admin Tools > General Settings > File Types'";

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
            configurationValues.Add( BINARY_FILE_TYPE, new ConfigurationValue( "File Type", "The type of files to list", string.Empty ) );

            if ( controls != null && controls.Count > 0 &&
                controls[0] != null && controls[0] is DropDownList )
            {
                configurationValues[BINARY_FILE_TYPE].Value = ( ( DropDownList ) controls[0] ).SelectedValue;
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
            if ( controls != null && controls.Count > 0 && configurationValues != null &&
                controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( BINARY_FILE_TYPE ) )
            {
                ( ( DropDownList ) controls[0] ).SetValue( configurationValues[BINARY_FILE_TYPE].Value?.ToLower() ?? string.Empty );
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
                ? GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
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
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var control = new BinaryFilePicker { ID = id };

            if ( configurationValues != null && configurationValues.ContainsKey( BINARY_FILE_TYPE ) )
            {
                control.BinaryFileTypeGuid = configurationValues[BINARY_FILE_TYPE].Value.AsGuidOrNull();
            }

            return control;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// returns BinaryFile.Guid
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as BinaryFilePicker;
            if ( picker != null )
            {
                int? itemId = picker.SelectedValue.AsIntegerOrNull();
                Guid? itemGuid = null;
                if ( itemId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        itemGuid = new BinaryFileService( rockContext ).Queryable().AsNoTracking().Where( a => a.Id == itemId.Value ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
                    }
                }

                return itemGuid?.ToString() ?? string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as BinaryFilePicker;

            if ( picker != null )
            {
                BinaryFile binaryFile = null;
                Guid? guid = value.AsGuidOrNull();

                // get the item (or null) and set it
                if ( guid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        binaryFile = new BinaryFileService( rockContext ).Get( guid.Value );
                    }
                }

                picker.SetValue( binaryFile );
            }
        }

        /// <summary>
        /// Gets the filter value control with the specified FilterMode
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            // only supports IsBlank and IsNotBlank, so don't show any value control
            var filterValueControl = base.FilterValueControl( configurationValues, id, required, filterMode );
            if ( filterValueControl != null )
            {
                filterValueControl.Visible = false;
            }

            return filterValueControl;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // return a dummy value since this only supports IsBlank and IsNotBlank
            return "0";
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            int? itemId = new BinaryFileService( new RockContext() ).Queryable().Where( a => a.Guid == guid ).Select( a => a.Id ).FirstOrDefault();
            return itemId != null ? itemId : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            Guid? itemGuid = null;
            if ( id.HasValue )
            {
                itemGuid = new BinaryFileService( new RockContext() ).Queryable().Where( a => a.Id == id.Value ).Select( a => a.Guid ).FirstOrDefault();
            }

            string guidValue = itemGuid.HasValue ? itemGuid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}