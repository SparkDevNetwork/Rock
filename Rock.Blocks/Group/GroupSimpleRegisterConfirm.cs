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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Group.GroupSimpleRegisterConfirm;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Confirmation block that updates a group member's status to active.
    /// Use with the Group Simple Register block.
    /// </summary>
    [DisplayName( "Group Simple Register Confirm" )]
    [Category( "Groups" )]
    [Description( "Confirmation block that will update a group member's status to active. (Use with Group Simple Register block)." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [TextField( "Success Message",
        Key = AttributeKey.SuccessMessage,
        Description = "The text to display when a valid group member key is provided.",
        IsRequired = false,
        DefaultValue = "You have been registered.",
        Order = 0 )]

    [TextField( "Error Message",
        Key = AttributeKey.ErrorMessage,
        Description = "The text to display when a valid group member key is NOT provided.",
        IsRequired = false,
        DefaultValue = "Sorry, there was a problem confirming your registration.  Please try to register again.",
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "5D96A1C2-5785-48A2-B14C-E5A63CF6EE49" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "2865CA63-4F5A-43BF-B898-3D380DE18655" )]
    [Rock.SystemGuid.BlockTypeGuid( "B71FE9F2-0F90-497F-90FA-5A6148E8E116" )]
    public class GroupSimpleRegisterConfirm : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SuccessMessage = "SuccessMessage";
            public const string ErrorMessage = "ErrorMessage";
        }

        /*
            7/10/26 - MSE

            The query-string key "GM" is the established external-link contract used by
            Group Simple Register when building confirmation email links
            (member.UrlEncodedKey). Renaming it would break existing confirmation emails.

            Reason: Preserve the established external-link contract for this block.
        */
        private static class PageParameterKey
        {
            public const string GroupMemberKey = "GM";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return ProcessConfirmation();
        }

        /// <summary>
        /// Resolves the group member from the URL-encoded key, activates membership when found,
        /// and returns the bag used to render the result NotificationBox.
        /// </summary>
        /// <returns>The confirmation result bag.</returns>
        private GroupSimpleRegisterConfirmBag ProcessConfirmation()
        {
            try
            {
                var groupMemberKey = PageParameter( PageParameterKey.GroupMemberKey );
                if ( groupMemberKey.IsNullOrWhiteSpace() )
                {
                    return CreateErrorBag( "The confirmation link is missing the group member key (GM) query string parameter." );
                }

                var groupMember = new GroupMemberService( RockContext ).GetByUrlEncodedKey( groupMemberKey );
                if ( groupMember == null )
                {
                    return CreateErrorBag();
                }

                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                RockContext.SaveChanges();

                return new GroupSimpleRegisterConfirmBag
                {
                    Heading = "Success",
                    Message = GetAttributeValue( AttributeKey.SuccessMessage ),
                    AlertType = "success"
                };
            }
            catch ( Exception ex )
            {
                // Log for admins while still surfacing the detail in the user-facing message.
                ExceptionLogService.LogException( ex );
                return CreateErrorBag( ex.Message );
            }
        }

        /// <summary>
        /// Builds the error result bag using the configured error message.
        /// When <paramref name="errorDetail"/> is provided, it is appended in brackets
        /// (e.g. missing parameter or exception message).
        /// </summary>
        /// <param name="errorDetail">Optional detail appended after the configured error message.</param>
        /// <returns>The error result bag.</returns>
        private GroupSimpleRegisterConfirmBag CreateErrorBag( string errorDetail = null )
        {
            var errorMessage = GetAttributeValue( AttributeKey.ErrorMessage );
            if ( errorDetail.IsNotNullOrWhiteSpace() )
            {
                errorMessage = $"{errorMessage} [{errorDetail}]";
            }

            return new GroupSimpleRegisterConfirmBag
            {
                Heading = "Sorry",
                Message = errorMessage,
                AlertType = "danger"
            };
        }

        #endregion Methods
    }
}
