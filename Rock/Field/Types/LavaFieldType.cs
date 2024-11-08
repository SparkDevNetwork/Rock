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
using System.Collections.Generic;

#if WEBFORMS
using System.Web.UI;
#endif

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Lava Field Type.  Stored as text
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.LAVA )]
    public class LavaFieldType : CodeEditorFieldType, IEntityContextFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, null, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Default method tries to HTML encode which we don't want to do.
            return GetHtmlValue( privateValue, null, privateConfigurationValues );
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueSupported( Dictionary<string, string> privateConfigurationValues )
        {
            // Lava could cause a different result with each render.
            return false;
        }

        #endregion

        #region IEntityContextFieldType

        /// <inheritdoc/>
        public string GetTextValue( string privateValue, int entityTypeId, int entityId, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !privateValue.IsLavaTemplate() )
            {
                return privateValue;
            }

            using ( var rockContext = new RockContext() )
            {
                var entity = Reflection.GetIEntityForEntityType( entityTypeId, entityId, rockContext );

                return GetTextValue( privateValue, entity, privateConfigurationValues );
            }
        }

        /// <inheritdoc/>
        public string GetTextValue( string privateValue, IEntity entity, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !privateValue.IsLavaTemplate() )
            {
                return privateValue;
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            if ( entity != null )
            {
                mergeFields.Add( "Entity", entity );
            }

            return privateValue.ResolveMergeFields( mergeFields ).Trim();
        }

        /// <inheritdoc/>
        public string GetHtmlValue( string privateValue, int entityTypeId, int entityId, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, entityTypeId, entityId, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public string GetHtmlValue( string privateValue, IEntity entity, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, entity, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public string GetCondensedTextValue( string privateValue, int entityTypeId, int entityId, Dictionary<string, string> privateConfigurationValues )
        {
            var value = GetTextValue( privateValue, entityTypeId, entityId, privateConfigurationValues );

            return value?.Truncate( CondensedTruncateLength );
        }

        /// <inheritdoc/>
        public string GetCondensedTextValue( string privateValue, IEntity entity, Dictionary<string, string> privateConfigurationValues )
        {
            var value = GetTextValue( privateValue, entity, privateConfigurationValues );

            return value?.Truncate( CondensedTruncateLength );
        }

        /// <inheritdoc/>
        public string GetCondensedHtmlValue( string privateValue, int entityTypeId, int entityId, Dictionary<string, string> privateConfigurationValues )
        {
            // Don't truncate since we might end up with an un-closed tag.
            return GetHtmlValue( privateValue, entityTypeId, entityId, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public string GetCondensedHtmlValue( string privateValue, IEntity entity, Dictionary<string, string> privateConfigurationValues )
        {
            // Don't truncate since we might end up with an un-closed tag.
            return GetHtmlValue( privateValue, entity, privateConfigurationValues );
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Formats the value.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string newValue = value;

            if ( value.IsLavaTemplate() )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                if ( entityTypeId.HasValue && entityId.HasValue )
                {
                    var entity = new EntityTypeService( new RockContext() ).GetEntity( entityTypeId.Value, entityId.Value );
                    if ( entity != null )
                    {
                        mergeFields.Add( "Entity", entity );
                    }
                }
                newValue = value.ResolveMergeFields( mergeFields ).Trim();
            }

            return base.FormatValue( parentControl, entityTypeId, entityId, newValue, configurationValues, condensed );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // implementing this as the base method encodes the HTML and we don't want to do that with the Lava control
            return FormatValue( parentControl, entityTypeId, entityId, value, configurationValues, condensed );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // implementing this as the base method encodes the HTML and we don't want to do that with the Lava control
            return FormatValue( parentControl, value, configurationValues, condensed );
        }

#endif
        #endregion
    }
}
