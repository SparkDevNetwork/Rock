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
using System.ComponentModel;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Web.Cache;

namespace Rock.Blocks.Mobile.Cms
{
    /// <summary>
    /// Displays content from a Lava Application and hosts its Helix
    /// interactions on the mobile shell.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Lava Application Content" )]
    [Category( "Mobile > Cms" )]
    [Description( "Displays content from a Lava Application and hosts its Helix interactions." )]
    [IconCssClass( "ti ti-bolt" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [CustomDropdownListField( "Application",
        Description = "The Lava Application this block belongs to. Descendant Helix requests can then use single-segment routes (^/endpoint-slug), and the initial template gets the application's merge fields.",
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [LavaApplication] ORDER BY [Name]",
        IsRequired = false,
        Key = AttributeKey.Application,
        Order = 0 )]

    [CodeEditorField( "Initial Template",
        Description = "The Lava template rendered on the server to produce the block's initial XAML. The 'LavaApplication' and 'ConfigurationRigging' merge fields are available when an application is selected. <span class='tip tip-lava'></span>",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        IsRequired = false,
        Key = AttributeKey.InitialTemplate,
        Order = 1 )]

    [TextField( "Initial Endpoint",
        Description = "A Helix route (for example ^/endpoint-slug) the shell fetches when the block loads. When set it is used instead of the Initial Template.",
        IsRequired = false,
        Key = AttributeKey.InitialEndpoint,
        Order = 2 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled when rendering the initial template.",
        IsRequired = false,
        Key = AttributeKey.EnabledLavaCommands,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "414D49DD-2A75-405E-BA4C-3D48CF7FE96B" )]
    [Rock.SystemGuid.BlockTypeGuid( "8E3F8E6D-D208-4556-A2C6-7202D0DEB984" )]
    public class LavaApplicationContent : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The block setting attribute keys for this block.
        /// </summary>
        private static class AttributeKey
        {
            public const string Application = "Application";
            public const string InitialTemplate = "InitialTemplate";
            public const string InitialEndpoint = "InitialEndpoint";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 20 );

        #endregion

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            var application = LavaApplicationCache.Get( GetAttributeValue( AttributeKey.Application ).AsGuid() );

            return new Rock.Common.Mobile.Blocks.Cms.LavaApplicationContent.Configuration
            {
                ApplicationSlug = application?.Slug,
                InitialEndpoint = GetAttributeValue( AttributeKey.InitialEndpoint ),
                DynamicContent = true
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the initial content for this block by rendering the initial
        /// template with the application's merge fields, mirroring the web
        /// Lava Application Content block.
        /// </summary>
        /// <returns>The initial content.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            var application = LavaApplicationCache.Get( GetAttributeValue( AttributeKey.Application ).AsGuid() );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "CurrentPage", this.PageCache );
            mergeFields.Add( "LavaApplication", application );
            mergeFields.Add( "ConfigurationRigging", application?.ConfigurationRigging );

            var content = GetAttributeValue( AttributeKey.InitialTemplate )
                .ResolveMergeFields( mergeFields, null, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );

            return new CallbackResponse
            {
                Content = content
            };
        }

        #endregion
    }
}
