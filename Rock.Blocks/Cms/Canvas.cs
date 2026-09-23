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
using System.ComponentModel;

using Rock.Attribute;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a module placed on the page by the Page Builder.
    /// </summary>

    [DisplayName( "Canvas" )]
    [Category( "CMS" )]
    [Description( "Displays a module placed on the page by the Page Builder." )]
    [IconCssClass( "ti ti-layout-board" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [TextField(
        "Module Type",
        Description = "The key of the module type this Canvas displays.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.ModuleType )]

    [CodeEditorField(
        "Module Settings",
        Description = "The module's settings as JSON.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.JavaScript,
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.ModuleSettings )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "55D00245-837C-4B7D-B63F-1FDDF27F11E9" )]
    [Rock.SystemGuid.BlockTypeGuid( "7B5D110D-849E-437D-B9E0-AD3D0CA4C253" )]
    public class Canvas : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The keys of the Canvas block's attributes, shared with the Page Builder that sets them.
        /// </summary>
        internal static class AttributeKey
        {
            public const string ModuleType = "ModuleType";
            public const string ModuleSettings = "ModuleSettings";
        }

        #endregion Keys

        /// <inheritdoc/>
        public override string ObsidianFileUrl => null; // Static HTML content.

        #region Methods

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            var moduleType = PageBuilderModuleTypes.Get( GetAttributeValue( AttributeKey.ModuleType ) );

            if ( moduleType == null )
            {
                return BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
                    ? "<div class='alert alert-warning'>This Canvas has no module type.</div>"
                    : string.Empty;
            }

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Settings", GetModuleSettings( moduleType ) );

            var moduleHtml = moduleType.WebLavaTemplate.ResolveMergeFields( mergeFields );

            return $"<div class=\"canvas-module\" data-module-type=\"{moduleType.Key.EncodeHtml()}\">{moduleHtml}</div>";
        }

        /// <summary>
        /// Gets the module's saved settings, using the module type's default for any setting that has not been saved.
        /// </summary>
        /// <param name="moduleType">The module type this Canvas displays.</param>
        /// <returns>The module's settings, keyed by setting key.</returns>
        private Dictionary<string, object> GetModuleSettings( PageBuilderModuleType moduleType )
        {
            var savedSettings = GetAttributeValue( AttributeKey.ModuleSettings ).FromJsonOrNull<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();
            var settings = new Dictionary<string, object>();

            foreach ( var setting in moduleType.Settings )
            {
                settings[setting.Key] = savedSettings.TryGetValue( setting.Key, out var value ) ? value : setting.DefaultValue;
            }

            return settings;
        }

        #endregion Methods
    }
}
