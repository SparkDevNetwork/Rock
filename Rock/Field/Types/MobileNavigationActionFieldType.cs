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
using System.Linq;
using Rock.Attribute;
using Rock.Mobile;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif

namespace Rock.Field.Types
{
    /// <summary>
    /// Allows configuration of a mobile navigation action. This allows one
    /// block to support multiple actions depending on how the administrator
    /// is using the block.
    /// </summary>
    /// <remarks>
    /// This is not meant to be used directly in user attributes, only block
    /// settings.
    /// </remarks>
    /// <seealso cref="Rock.Field.FieldType" />
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8" )]
    public sealed class MobileNavigationActionFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var navigationAction = privateValue.FromJsonOrNull<MobileNavigationAction>();

            if ( navigationAction == null )
            {
                return string.Empty;
            }

            var popCount = navigationAction.PopCount ?? 1;
            var pageCache = navigationAction.PageGuid.HasValue
                ? PageCache.Get( navigationAction.PageGuid.Value )
                : null;

            switch ( navigationAction.Type )
            {
                case MobileNavigationActionType.None:
                    return "None";

                case MobileNavigationActionType.PopPage:
                    return $"Pop {popCount} page{( popCount != 1 ? "s" : string.Empty )}";

                case MobileNavigationActionType.DismissCoverSheet:
                    return $"Dismiss Cover Sheet";

                case MobileNavigationActionType.PushPage:
                    return $"Push '{pageCache?.InternalName}'";

                case MobileNavigationActionType.ReplacePage:
                    return $"Replace page with '{pageCache?.InternalName}'";

                case MobileNavigationActionType.ResetToPage:
                    return $"Reset to '{pageCache?.InternalName}'";

                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Edit Control

        #endregion

        #region Filter Control

        /// <inheritdoc/>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var navigationAction = privateValue.FromJsonOrNull<MobileNavigationAction>();

            if ( navigationAction == null || !navigationAction.PageGuid.HasValue )
            {
                return null;
            }

            var pageId = PageCache.GetId( navigationAction.PageGuid.Value );

            if ( !pageId.HasValue )
            {
                return null;
            }

            return new List<ReferencedEntity>
            {
                new ReferencedEntity( EntityTypeCache.GetId<Rock.Model.Page>().Value, pageId.Value )
            };
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the InternalName property of a Page and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Rock.Model.Page>().Value, nameof( Rock.Model.Page.InternalName ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <inheritdoc/>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <inheritdoc/>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new MobileNavigationActionEditor { ID = id };
        }

        /// <inheritdoc/>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is MobileNavigationActionEditor actionEditor )
            {
                return actionEditor.NavigationAction.ToJson();
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control is MobileNavigationActionEditor actionEditor )
            {
                actionEditor.NavigationAction = value.FromJsonOrNull<MobileNavigationAction>();
            }
        }

        /// <inheritdoc/>
        public override Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            return null;
        }

#endif
        #endregion

    }
}
