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
using System.Linq;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a <see cref="SignatureDocumentTemplate" />. Stored as the SignatureDocumentTemplate's Guid.
    /// </summary>
    [Rock.SystemGuid.FieldTypeGuid( "258A4AEF-F555-4AF5-8D5D-2D581A982D1C" )]
    public class SignatureDocumentTemplateFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        private const string SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS = "SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS";

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = privateValue;

            System.Guid? guid = privateValue.AsGuidOrNull();
            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var name = new SignatureDocumentTemplateService( rockContext ).GetSelect( guid.Value, a => a.Name );
                    if ( name != null )
                    {
                        formattedValue = name;
                    }
                }
            }

            return formattedValue;
        }

        #endregion Formatting

        #region EditControl

        #endregion EditControl

        #region IEntityFieldType

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
                return new SignatureDocumentTemplateService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var signatureDocumentTemplateId = new SignatureDocumentTemplateService( rockContext ).GetId( guid.Value );

                if ( !signatureDocumentTemplateId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<SignatureDocumentTemplate>().Value, signatureDocumentTemplateId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Signature Document Template and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<SignatureDocumentTemplate>().Value, nameof( SignatureDocumentTemplate.Name ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns>System.String.</returns>
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
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editControl = new RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            bool showExternalProviders = configurationValues.GetValueOrNull( SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS )?.AsBoolean() ?? false;

            var templatesQuery = new SignatureDocumentTemplateService( new RockContext() ).Queryable().Where( x => x.IsActive );
            if ( showExternalProviders )
            {
                templatesQuery = templatesQuery.Where( a => a.ProviderEntityTypeId.HasValue );
            }
            else
            {
                templatesQuery = templatesQuery.Where( a => !a.ProviderEntityTypeId.HasValue );
            }

            var templates = templatesQuery.OrderBy( t => t.Name ).Select( a => new
            {
                a.Guid,
                a.Name
            } );

            foreach ( var template in templates )
            {
                editControl.Items.Add( new ListItem( template.Name, template.Guid.ToString() ) );
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var editControl = control as ListControl;
            if ( editControl != null )
            {
                return editControl.SelectedValue;
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
            var editControl = control as ListControl;
            if ( editControl != null )
            {
                var guid = value.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    var selectedValue = editControl.Items.FindByValue( value );

                    // If the value is not part of the control's ListItems then it's most likely the template was
                    // marked inactive after it was selected so add it to the control's list.
                    if ( selectedValue == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var name = new SignatureDocumentTemplateService( rockContext ).GetSelect( guid.Value, a => a.Name );
                            editControl.Items.Add( new ListItem( name, value ) );
                        }
                    }
                }
                editControl.SetValue( value );
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new SignatureDocumentTemplateService( new RockContext() ).Get( guid );
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
            var item = new SignatureDocumentTemplateService( new RockContext() ).Get( id ?? 0 );
            var guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}
