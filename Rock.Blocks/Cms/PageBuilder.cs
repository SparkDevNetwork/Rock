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
using Rock.ViewModels.Blocks.Cms.PageBuilder;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Composes a page by dragging modules into its builder-enabled zones.
    /// </summary>

    [DisplayName( "Page Builder" )]
    [Category( "CMS" )]
    [Description( "Composes a page by dragging modules into its builder-enabled zones." )]
    [IconCssClass( "ti ti-layout" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Target Page",
        Description = "The page to compose. Only zones that opt in to the Page Builder accept modules.",
        IsRequired = true,
        DefaultValue = "83FC4394-ED09-461D-B865-68C1F5AFB34E", // Page Builder Sample
        Order = 0,
        Key = AttributeKey.TargetPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "5968C4FC-2508-4457-B1D6-E3AC6711C9F9" )]
    [Rock.SystemGuid.BlockTypeGuid( "89B56E5A-5A7D-44E6-85A0-49F101F814BE" )]
    public class PageBuilder : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string TargetPage = "TargetPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var targetPageUrl = this.GetLinkedPageUrl( AttributeKey.TargetPage );

            if ( targetPageUrl.IsNullOrWhiteSpace() )
            {
                return new PageBuilderInitializationBox
                {
                    ErrorMessage = "No target page is configured."
                };
            }

            return new PageBuilderInitializationBox
            {
                TargetPageUrl = targetPageUrl
            };
        }

        #endregion Methods
    }
}
