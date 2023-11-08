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

using System;
using System.Collections.Generic;
using System.Linq;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;


namespace Rock.Field.Types
{
    /// <summary>
    /// Class DocumentTypeFieldType.
    /// Implements the <see cref="Rock.Field.FieldType" />
    /// </summary>
    /// <seealso cref="Rock.Field.FieldType" />
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.DOCUMENT_TYPE )]
    public class DocumentTypeFieldType : FieldType, IEntityReferenceFieldType
    {
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";
        private const string VALUES_PUBLIC_KEY = "values";

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );
            publicConfigurationValues[VALUES_PUBLIC_KEY] = DocumentTypeCache.All()
                .OrderBy( v => v.Name )
                .ToListItemBagList()
                .ToCamelCaseJson( false, true );
            return publicConfigurationValues;
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var configurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );
            configurationValues.Remove( VALUES_PUBLIC_KEY );
            return configurationValues;
        }

        #endregion Configuration

        #region Formatting

        /// <inheritdoc />
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            // This is a list of IDs, we'll want it to be document type names instead
            var selectedValues = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( int.Parse ).ToList();

            return DocumentTypeCache.All()
                .Where( v => selectedValues.Contains( v.Id ) )
                .Select( v => v.Name )
                .ToList()
                .AsDelimited( ", " );
        }

        #endregion Formatting

        #region Edit Control

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // The database stores the Document Type AttirbuteValues by their Ids.
            // However, the remote device needs the Guid to display them. So we are doing the following conversions.
            var documentTypeGuids = privateValue.Split( ',' )
                .Select( g => DocumentTypeCache.GetGuid( g.ToIntSafe() ).ToString() )
                .ToList();
            return string.Join( ",", documentTypeGuids );
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            // The publicValue which is sent by the remote device is a GUID. However, the database only stores the integer values in the database.
            // So for each DocumentTypeGuid from the remote device, we convert it to the corresponding Id
            var documentTypeIds = publicValue.Split( ',' )
                .Select( g => DocumentTypeCache.GetId( g.AsGuid() ).ToString() )
                .ToList();
            return string.Join( ",", documentTypeIds );
        }

        #endregion Edit Control

        #region Filter Control

        #endregion Filter Control

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var selectedValues = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( int.Parse ).ToList();

            if ( selectedValues.Count == 0 )
            {
                return null;
            }

            var documentTypeIds = DocumentTypeCache.All()
                .Where( v => selectedValues.Contains( v.Id ) )
                .Select( v => v.Id )
                .ToList();

            var referencedEntities = new List<ReferencedEntity>();
            foreach ( var documentTypeId in documentTypeIds )
            {
                referencedEntities.Add( new ReferencedEntity( EntityTypeCache.GetId<DocumentType>().Value, documentTypeId ) );
            }

            return referencedEntities;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<DocumentType>().Value, nameof( DocumentType.Name ) )
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
            var configKeys = new List<string>();
            configKeys.Add( ALLOW_MULTIPLE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add checkbox for deciding if the document types is rendered as a drop
            // down list or a list.
            var cbAllowMultipleValues = new RockCheckBox();
            controls.Add( cbAllowMultipleValues );
            cbAllowMultipleValues.AutoPostBack = true;
            cbAllowMultipleValues.Checked = true;
            cbAllowMultipleValues.CheckedChanged += OnQualifierUpdated;
            cbAllowMultipleValues.Label = "Allow Multiple Values";
            cbAllowMultipleValues.Text = "Yes";
            cbAllowMultipleValues.Help = "When set, allows multiple document types to be selected.";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple document types to be selected.", "True" ) );

            if ( controls != null )
            {
                var cbAllowMultipleValues = controls.Count > 0 ? controls[0] as CheckBox : null;

                if ( cbAllowMultipleValues != null )
                {
                    configurationValues[ALLOW_MULTIPLE_KEY].Value = cbAllowMultipleValues.Checked.ToString();
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
                var cbAllowMultipleValues = controls.Count > 0 ? controls[0] as CheckBox : null;

                if ( cbAllowMultipleValues != null )
                {
                    cbAllowMultipleValues.Checked = configurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBoolean( true );
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
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
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
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            ListControl editControl;

            var values = DocumentTypeCache.All().Select( v => new { v.Id, v.Name } ).OrderBy( v => v.Name ).ToList();

            var allowMultiple = true;

            if ( configurationValues != null &&
                 configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) )
            {
                allowMultiple = configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean( true );
            }

            if ( !allowMultiple )
            {
                // Select single member
                editControl = new RockDropDownList { ID = id };
            }
            else
            {
                // Select multiple members
                editControl = new RockListBox { ID = id, DisplayDropAsAbsolute = true };
            }

            var items = new List<ListItem>();
            foreach ( var value in values )
            {
                items.Add( new ListItem( value.Name, value.Id.ToString() ) );
            }

            if ( items.Count > 0 )
            {
                if ( editControl is RockListBox )
                {
                    ( editControl as RockListBox ).Items.AddRange( items.ToArray() );
                }
                else
                {
                    ( editControl as RockDropDownList ).Items.AddRange( items.ToArray() );
                }

                return editControl;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<string> values = new List<string>();

            if ( control != null && control is ListControl )
            {
                var editControl = ( ListControl ) control;
                foreach ( ListItem li in editControl.Items )
                {
                    if ( li.Selected )
                    {
                        values.Add( li.Value );
                    }
                }

                return values.AsDelimited<string>( "," );
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
            if ( value == null || control == null || !( control is ListControl ) )
            {
                return;
            }

            var values = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            var editControl = ( ListControl ) control;
            foreach ( ListItem li in editControl.Items )
            {
                li.Selected = values.Contains( li.Value );
            }
        }

#endif
        #endregion
    }
}
