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

using Rock.Mobile;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

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
    public sealed class MobileNavigationActionFieldType : FieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var navigationAction = value.FromJsonOrNull<MobileNavigationAction>();

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

        /// <inheritdoc/>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new MobileNavigationActionEditor { ID = id };
        }

        /// <inheritdoc/>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is MobileNavigationActionEditor actionEditor )
            {
                return actionEditor.NavigationAction.ToJson();
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control is MobileNavigationActionEditor actionEditor )
            {
                actionEditor.NavigationAction = value.FromJsonOrNull<MobileNavigationAction>();
            }
        }

        #endregion

        #region Filter Control

        /// <inheritdoc/>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            return null;
        }

        /// <inheritdoc/>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion
    }
}
