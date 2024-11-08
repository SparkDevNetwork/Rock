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
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a <see cref="SignatureDocumentTemplate" />. Stored as the SignatureDocumentTemplate's Guid.
    /// </summary>
    [Rock.SystemGuid.FieldTypeGuid( "258A4AEF-F555-4AF5-8D5D-2D581A982D1C" )]
    public class SignatureDocumentTemplateFieldType : UniversalItemPickerFieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        private const string SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS = "SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS";

        /// <inheritdoc/>
        protected override List<ListItemBag> GetItemBags( IEnumerable<string> values, Dictionary<string, string> privateConfigurationValues )
        {
            return GetListItems( privateConfigurationValues )
                .Where( bag => values.Contains( bag.Value ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override List<ListItemBag> GetListItems( Dictionary<string, string> privateConfigurationValues )
        {
            bool showExternalProviders = privateConfigurationValues.GetValueOrNull( SHOW_TEMPLATES_WITH_EXTERNAL_PROVIDERS )?.AsBoolean() ?? false;

            using ( var rockContext = new RockContext() )
            {
                var templatesQuery = new SignatureDocumentTemplateService( rockContext )
                    .Queryable()
                    .Where( x => x.IsActive );

                if ( showExternalProviders )
                {
                    templatesQuery = templatesQuery.Where( a => a.ProviderEntityTypeId.HasValue );
                }
                else
                {
                    templatesQuery = templatesQuery.Where( a => !a.ProviderEntityTypeId.HasValue );
                }

                return templatesQuery.OrderBy( t => t.Name ).Select( a => new ListItemBag()
                {
                    Value = a.Guid.ToString(),
                    Text = a.Name
                } ).ToList();
            }
        }

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
