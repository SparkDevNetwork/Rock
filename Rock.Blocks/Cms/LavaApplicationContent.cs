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
using Rock.Utility.ExtensionMethods;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular lava application.
    /// </summary>

    [DisplayName( "Lava Application Content" )]
    [Description( "Displays the details of a particular lava application." )]
    [IconCssClass( "fa fa-question" )]
    [Category( "CMS" )]
    [SupportedSiteTypes( SiteType.Web )]
    [ConfigurationChangedReload( Rock.Enums.Cms.BlockReloadMode.Page )]
    #region Block Attributes
    [CustomDropdownListField( "Application",
        Description = "The Lava application to target. This is optional, but if provided you'll have access to the application in your Lava.",
        Key = AttributeKey.Application,
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [LavaApplication] ORDER BY [Name]",
        Order = 0 )]

    [CodeEditorField("Lava Template",
        Description = "Your Lava template for the page. You can access your application's configuration rigging using the 'ConfigurationRigging' merge field.",
        Key = AttributeKey.LavaTemplate,
        EditorHeight = 400,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        IsRequired = true,
        Order = 1 )]
    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this HTML block.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.EnabledLavaCommands )]
    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "ACB4674B-22EC-FE88-48D0-2EEDB6536B85" )]
    [Rock.SystemGuid.BlockTypeGuid( "9D863719-3E92-8681-4DDC-DE63ACEFEDF1" )]
    public class LavaApplicationContent : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LavaTemplate = "LavaTemplate";
            public const string Application = "Application";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            RequestContext.Response.AddScriptLinkToHead( "/Scripts/Rock/helix-script.js", true );
            RequestContext.Response.AddCssLink( "/Styles/Blocks/Cms/helix.css", true );

            return GetContent();
        }

        #endregion

        #region Private Methods

        private string GetContent()
        {
            // Get attribute information
            var lavaApplication = LavaApplicationCache.Get( GetAttributeValue( AttributeKey.Application ).AsGuid() );
            var lavaTemplate = GetAttributeValue( AttributeKey.LavaTemplate );
            var enabledLavaComments = GetAttributeValue( AttributeKey.EnabledLavaCommands );

            // Get merge fields
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.Add( "LavaApplication", lavaApplication );
            mergeFields.Add( "ConfigurationRigging", lavaApplication?.ConfigurationRigging );

            // Render Lava
            return lavaTemplate.ResolveMergeFields( mergeFields, enabledLavaComments );
        }

        #endregion
    }
}
