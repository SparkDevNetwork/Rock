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
using Rock.Security.SecurityGrantRules;
using Rock.ViewModels.Blocks.Example.ControlGallery;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Allows the user to try out various controls.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Control Gallery" )]
    [Category( "Obsidian > Example" )]
    [Description( "Allows the user to try out various controls." )]
    [IconCssClass( "fa fa-flask" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [BooleanField( "Show Reflection",
        Description = "When enabled, a Show Reflection option will be enabled that will add a second control to demonstrate two-way databinding.  This is typically only useful to developers when they are developing a new control.",
        DefaultValue = "false",
        Order = 0,
        Key = AttributeKey.ShowReflection )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.OBSIDIAN_EXAMPLE_CONTROL_GALLERY )]
    [Rock.SystemGuid.BlockTypeGuid( "6FAB07FF-D4C6-412B-B13F-7B881ECBFAD0" )]
    public class ControlGallery : RockBlockType
    {
        public static class AttributeKey
        {
            public const string ShowReflection = "ShowReflection";
        }

        /// <inheritdoc/>
        public override string ObsidianFileUrl => base.ObsidianFileUrl.ReplaceIfEndsWith( ".obs", string.Empty );

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ControlGalleryInitializationBox();

            box.ShowReflection = GetAttributeValue( AttributeKey.ShowReflection ).AsBoolean();
            box.SecurityGrantToken = GetSecurityGrantToken();

            return box;
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            return GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            // The asset manager endpoints trust this token alone. Anyone who
            // can view the block may browse and select existing files within
            // the block's root folder. Only block editors may upload, rename,
            // move or delete files and folders.
            if ( IsAssetManagerViewAuthorized() )
            {
                securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Rock.Security.Authorization.VIEW ) );
            }

            if ( IsAssetManagerEditAuthorized() )
            {
                securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Rock.Security.Authorization.EDIT ) );
                securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Rock.Security.Authorization.DELETE ) );
            }

            securityGrant.AddRule( new EmailEditorSecurityGrantRule() );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Determines whether the current person may browse and select
        /// existing files with the asset manager. Requires VIEW, EDIT or
        /// ADMINISTRATE on the block.
        /// </summary>
        /// <returns><c>true</c> if the current person may browse the asset manager; otherwise <c>false</c>.</returns>
        private bool IsAssetManagerViewAuthorized()
        {
            var currentPerson = GetCurrentPerson();

            return currentPerson != null
                && ( BlockCache.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson )
                    || IsAssetManagerEditAuthorized() );
        }

        /// <summary>
        /// Determines whether the current person may upload, rename, move
        /// and delete files and folders with the asset manager. Requires EDIT
        /// or ADMINISTRATE on the block.
        /// </summary>
        /// <returns><c>true</c> if the current person may manage files in the asset manager; otherwise <c>false</c>.</returns>
        private bool IsAssetManagerEditAuthorized()
        {
            var currentPerson = GetCurrentPerson();

            return currentPerson != null
                && ( BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson )
                    || BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, currentPerson ) );
        }
    }
}
