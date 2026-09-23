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

using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;

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
        "Module Instance",
        Description = "The unique identifier of the module instance this Canvas displays.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.ModuleInstance )]

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
            public const string ModuleInstance = "ModuleInstance";
        }

        #endregion Keys

        /// <inheritdoc/>
        public override string ObsidianFileUrl => null; // Static HTML content.

        #region Methods

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            var moduleInstanceGuid = GetAttributeValue( AttributeKey.ModuleInstance ).AsGuidOrNull();
            var moduleInstance = moduleInstanceGuid.HasValue
                ? new ModuleInstanceService( RockContext ).Get( moduleInstanceGuid.Value )
                : null;

            if ( moduleInstance?.ModuleType == null )
            {
                return BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
                    ? "<div class='alert alert-warning'>This Canvas has no module.</div>"
                    : string.Empty;
            }

            // The module's settings are its attribute values, which its type's Lava reads through the Attribute filter.
            moduleInstance.LoadAttributes( RockContext );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "ModuleInstance", moduleInstance );

            var moduleHtml = moduleInstance.ModuleType.WebLavaTemplate.ResolveMergeFields( mergeFields );

            return $"<div class=\"canvas-module\" data-module-type=\"{moduleInstance.ModuleType.IdKey}\">{moduleHtml}</div>";
        }

        #endregion Methods
    }
}
