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
using Rock.Security;
using Rock.Tasks;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.Logout;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Logs the current person out of Rock.
    /// </summary>
    [DisplayName( "Log Out" )]
    [Category( "Security" )]
    [Description( "This block logs the current person out." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Redirect Page",
        Key = AttributeKey.RedirectPage,
        Description = "The page to redirect the user to after logging out.",
        IsRequired = false,
        Order = 0 )]

    [CodeEditorField( "Message",
        Key = AttributeKey.Message,
        Description = "The Lava template message to display if no redirect page was provided. Supports {{ CurrentPerson }} merge field.",
        EditorMode = CodeEditorMode.Lava,
        DefaultValue = @"<div class=""alert alert-success"">You have been logged out.</div>",
        IsRequired = false,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "83598C59-EF7D-4B6F-88AE-9A5B93C16386" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "B1FCA754-D278-4C6E-9BF5-585ECCBA1B7D" )]
    [Rock.SystemGuid.BlockTypeGuid( "CCB87054-8AA3-4F44-AA48-19BD028C4190" )]
    public class Logout : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string RedirectPage = "RedirectPage";
            public const string Message = "Message";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var isAdminUser = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            var box = new CustomBlockBox<LogoutBag, LogoutOptionsBag>
            {
                Options = new LogoutOptionsBag
                {
                    IsAdminUser = isAdminUser
                }
            };

            // If the user has edit access, show a warning instead of logging them out.
            if ( !isAdminUser )
            {
                box.Bag = PerformLogout();
            }

            return box;
        }

        /// <summary>
        /// Performs the logout operation for the current user and returns the
        /// appropriate bag with either a redirect URL or a rendered message.
        /// </summary>
        /// <returns>A <see cref="LogoutBag"/> containing the result of the logout operation.</returns>
        private LogoutBag PerformLogout()
        {
            var currentPerson = GetCurrentPerson();
            var bag = new LogoutBag();

            if ( currentPerson != null )
            {
                if ( RequestContext.CurrentUser != null )
                {
                    var updateUserLastActivityMsg = new UpdateUserLastActivity.Message
                    {
                        UserId = RequestContext.CurrentUser.Id,
                        LastActivityDate = RockDateTime.Now,
                        IsOnline = false
                    };
                    updateUserLastActivityMsg.Send();
                }

                // Resolve the Lava message before signing out so CurrentPerson is available.
                var redirectPageUrl = this.GetLinkedPageUrl( AttributeKey.RedirectPage );

                if ( redirectPageUrl.IsNullOrWhiteSpace() )
                {
                    var message = GetAttributeValue( AttributeKey.Message );
                    var mergeFields = new Dictionary<string, object>
                    {
                        { "CurrentPerson", currentPerson }
                    };

                    bag.Message = message.ResolveMergeFields( mergeFields );
                }
                else
                {
                    bag.RedirectUrl = redirectPageUrl;
                }

                Authorization.SignOut();
            }

            return bag;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Logs out an admin user who clicked the Logout button.
        /// </summary>
        /// <returns>A bag containing the redirect URL or rendered message.</returns>
        [BlockAction]
        public BlockActionResult LogoutUser()
        {
            return ActionOk( PerformLogout() );
        }

        #endregion Block Actions
    }
}
