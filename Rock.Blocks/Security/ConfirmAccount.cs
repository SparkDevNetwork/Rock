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
using Rock.Data;
using Rock.Enums.Blocks.Security.ConfirmAccount;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.ViewModels.Blocks.Security.ConfirmAccount;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Block for user to confirm a newly created login account.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Confirm Account" )]
    [Category( "Security" )]
    [Description( "Block for user to confirm a newly created login account, usually from an email that was sent to them." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Confirmed Caption",
        Key = AttributeKey.ConfirmedCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "{0}, your account has been confirmed. Thank you for creating the account.",
        Category = "Captions",
        Order = 0 )]
    
    [CodeEditorField( "Reset Password Caption",
        Key = AttributeKey.ResetPasswordCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "{0}, enter a new password for your '{1}' account.",
        Category = "Captions",
        Order = 1 )]

    [CodeEditorField( "Password Reset Caption",
        Key = AttributeKey.PasswordResetCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "{0}, the password for your '{1}' account has been changed.",
        Category = "Captions",
        Order = 2 )]

    [CodeEditorField( "Delete Caption",
        Key = AttributeKey.DeleteCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "Are you sure you want to delete the '{0}' account?",
        Category = "Captions",
        Order = 3 )]

    [CodeEditorField( "Deleted Caption",
        Key = AttributeKey.DeletedCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "The account has been deleted.",
        Category = "Captions",
        Order = 4 )]

    [CodeEditorField( "Invalid Caption",
        Key = AttributeKey.InvalidCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "The confirmation code you've entered is not valid.  Please enter a valid confirmation code or <a href='{0}'>create a new account</a>.",
        Category = "Captions",
        Order = 5 )]

    [CodeEditorField( "Password Reset Unavailable Caption",
        Key = AttributeKey.PasswordResetUnavailableCaption,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "This type of account does not allow passwords to be changed.  Please contact your system administrator for assistance changing your password.",
        Category = "Captions",
        Order = 6 )]

    [LinkedPage( "New Account Page",
        Key = AttributeKey.NewAccountPage,
        Description = "Page to navigate to when user selects 'Create New Account' option (if blank will use 'NewAccount' page route)",
        Order = 7 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "49098480-A041-4404-964C-10EFF41B7DCA" )]
    [Rock.SystemGuid.BlockTypeGuid( "F9FD6BE8-8073-40E9-83D9-CA3F947D2E2A" )]
    public class ConfirmAccount : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ConfirmedCaption = "ConfirmedCaption";
            public const string ResetPasswordCaption = "ResetPasswordCaption";
            public const string PasswordResetCaption = "PasswordResetCaption";
            public const string DeleteCaption = "DeleteCaption";
            public const string DeletedCaption = "DeletedCaption";
            public const string InvalidCaption = "InvalidCaption";
            public const string PasswordResetUnavailableCaption = "PasswordResetUnavailableCaption";
            public const string NewAccountPage = "NewAccountPage";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class PageParameterKey
        {
            public const string Action = "action";
            public const string CodeConfirmation = "cc";
        }

        private static class ConfirmAccountAction
        {
            public const string Confirm = "confirm";
            public const string Delete = "delete";
            public const string ChangePassword = "reset";
        }

        #endregion

        #region Properties

        private string ActionPageParameter => this.PageParameter( PageParameterKey.Action );

        private string CodeConfirmationPageParameter => this.PageParameter( PageParameterKey.CodeConfirmation );

        /// <summary>
        /// Gets the confirmed caption template where instances of <c>{0}</c> will be replaced with the affected account's First Name.
        /// </summary>
        private string AccountConfirmedCaptionTemplate => GetAttributeValue( AttributeKey.ConfirmedCaption );

        /// <summary>
        /// Gets the password reset caption template where instances of <c>{0}</c> and <c>{1}</c> will be replaced with the affected account's First Name and Username, respectively.
        /// </summary>
        private string PasswordChangedCaptionTemplate => GetAttributeValue( AttributeKey.PasswordResetCaption );

        /// <summary>
        /// Gets the reset password caption template where instances of <c>{0}</c> and <c>{1}</c> will be replaced with the affected account's First Name and Username, respectively.
        /// </summary>
        private string ChangePasswordCaptionTemplate => GetAttributeValue( AttributeKey.ResetPasswordCaption );

        /// <summary>
        /// Gets the delete caption template where instances of <c>{0}</c> will be replaced with the affected account's Username.
        /// </summary>
        private string DeleteConfirmationCaptionTemplate => GetAttributeValue( AttributeKey.DeleteCaption );

        private string AccountDeletedCaption => GetAttributeValue( AttributeKey.DeletedCaption );

        /// <summary>
        /// Gets the invalid caption template where instances of <c>{0}</c> will be replaced with the configured NewAccountPage URL or "/NewAccount" by default.
        /// </summary>
        private string InvalidConfirmationCodeCaptionTemplate => GetAttributeValue( AttributeKey.InvalidCaption );

        private string ChangePasswordUnavailableCaption => GetAttributeValue( AttributeKey.PasswordResetUnavailableCaption );

        private string NewAccountPageUrl => this.GetLinkedPageUrl( AttributeKey.NewAccountPage );

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ConfirmAccountInitializationBox
            {
                ActionNames = GetActionNames(),
                ErrorMessage = null,
                NavigationUrls = GetBoxNavigationUrls(),
                SecurityGrantToken = null,
                View = ProcessActionView()
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Shows the change password view.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction]
        public BlockActionResult ShowChangePasswordView( ConfirmAccountShowChangePasswordViewRequestBag bag )
        {
            return ActionOk( ShowChangePasswordView( bag.Code ) );
        }

        /// <summary>
        /// Shows the delete confirmation view.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction]
        public BlockActionResult ShowDeleteConfirmationView( ConfirmAccountShowDeleteConfirmationRequestBag bag )
        {
            return ActionOk( ShowDeleteConfirmationView( bag.Code ) );
        }

        /// <summary>
        /// Changes the account password.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction]
        public BlockActionResult ChangePassword( ConfirmAccountChangePasswordRequestBag bag )
        {
            return ActionOk( ChangePassword( bag.Code, bag.Password ) );
        }

        /// <summary>
        /// Confirms the account.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction]
        public BlockActionResult Confirm( ConfirmAccountConfirmRequestBag bag )
        {
            return ActionOk( Confirm( bag.Code ) );
        }

        /// <summary>
        /// Deletes the account.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction]
        public BlockActionResult Delete( ConfirmAccountDeleteRequestBag bag )
        {
            return ActionOk( DeleteAccount( bag.Code ) );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the change password view if the confirmation code is valid.
        /// </summary>
        /// <param name="code">The confirmation code.</param>
        private ConfirmAccountViewBox ShowChangePasswordView( string code )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );

                if ( !IsCodeValid( userLoginService, code, out var user ) )
                {
                    return ShowAccountConfirmationView( code );
                }

                var component = AuthenticationContainer.GetComponent( user.EntityType.Name );

                if ( !component.SupportsChangePassword )
                {
                    // Intentionally show the error message-only view when change password is not supported.
                    return ShowAlertView( this.ChangePasswordUnavailableCaption, ConfirmAccountAlertType.Danger );
                }

                return ShowChangePasswordView( code, user );
            }
        }

        /// <summary>
        /// Shows the delete confirmation view if the confirmation code is valid.
        /// </summary>
        /// <param name="code">The confirmation code.</param>
        private ConfirmAccountViewBox ShowDeleteConfirmationView( string code )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );

                if ( !IsCodeValid( userLoginService, code, out var user ) )
                {
                    return ShowAccountConfirmationView( code );
                }

                return ShowDeleteConfirmationView( code, user );
            }
        }

        /// <summary>
        /// Resets the account password if the <paramref name="code"/>, <paramref name="password"/>, and <paramref name="passwordConfirm"/> are valid.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="password">The password.</param>
        /// <returns>The next view to show after attempting to reset the account password.</returns>
        private ConfirmAccountViewBox ChangePassword( string code, string password )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );

                if ( !IsCodeValid( userLoginService, code, out var user ) )
                {
                    return ShowAccountConfirmationView( code );
                }

                var component = AuthenticationContainer.GetComponent( user.EntityType.Name );

                if ( !component.SupportsChangePassword )
                {
                    // Intentionally show the error message-only view when change password is not supported.
                    return ShowAlertView( this.ChangePasswordUnavailableCaption, ConfirmAccountAlertType.Danger );
                }

                if ( UserLoginService.IsPasswordValid( password ) )
                {
                    userLoginService.SetPassword( user, password );
                    user.IsConfirmed = true;
                    user.IsLockedOut = false; // unlock the user account if they reset their password.
                    rockContext.SaveChanges();

                    return ShowAlertView( GetPasswordChangedCaption( user ), ConfirmAccountAlertType.Success );
                }
                else
                {
                    return ShowChangePasswordView( code, user, UserLoginService.FriendlyPasswordRules() );
                }
            }
        }

        /// <summary>
        /// Confirms the account if the <paramref name="code"/> is valid.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>The next view to show after attempting to confirm the account.</returns>
        private ConfirmAccountViewBox Confirm( string code )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );

                if ( !IsCodeValid( userLoginService, code, out var user ) )
                {
                    return ShowAccountConfirmationView( code );
                }

                user.IsConfirmed = true;
                rockContext.SaveChanges();
                
                /*
                    10/20/2023 - JMH

                    Do not automatically authenticate the individual after confirming their account.
                    Instead, they should have to authenticate using the Login (or other authentication) block,
                    where 2FA and other authentication logic is handled.

                    Reason: Two-Factor Authentication
                 */
                // Authorization.SetAuthCookie( user.UserName, isPersisted: false, isImpersonated: false );

                return ShowAlertView( GetAccountConfirmedCaption( user ), ConfirmAccountAlertType.Success );
            }
        }

        /// <summary>
        /// Deletes the account if the <paramref name="code"/> is valid.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>The next view to show after attempting to delete the account.</returns>
        private ConfirmAccountViewBox DeleteAccount( string code )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );

                if ( !IsCodeValid( userLoginService, code, out var user ) )
                {
                    return ShowAccountConfirmationView( code );
                }

                if ( this.RequestContext.CurrentUser != null && this.RequestContext.CurrentUser.UserName == user.UserName )
                {
                    // It seems silly to update user activity when deleting and this may be removed in the future, but this is for compatibility with WebForms.
                    var updateUserLastActivityMsg = new UpdateUserLastActivity.Message
                    {
                        UserId = this.RequestContext.CurrentUser.Id,
                        LastActivityDate = RockDateTime.Now,
                        IsOnline = false
                    };

                    updateUserLastActivityMsg.Send();

                    Authorization.SignOut();
                }

                userLoginService.Delete( user );
                rockContext.SaveChanges();

                return ShowContentView( this.AccountDeletedCaption );
            }
        }

        /// <summary>
        /// Gets the account confirmed caption for the <paramref name="user"/>, using the <see cref="AccountConfirmedCaptionTemplate"/>.
        /// </summary>
        /// <param name="user">The user for whom to generate the caption.</param>
        /// <returns>The account confirmed caption for the user.</returns>
        private string GetAccountConfirmedCaption( UserLogin user )
        {
            var caption = this.AccountConfirmedCaptionTemplate;

            if ( caption.Contains( "{0}" ) )
            {
                caption = string.Format( caption, user.Person.FirstName );
            }

            return caption;
        }

        /// <summary>
        /// Gets the action names bag.
        /// </summary>
        private ConfirmAccountActionNamesBag GetActionNames()
        {
            return new ConfirmAccountActionNamesBag
            {
                ChangePassword = nameof( ConfirmAccount.ChangePassword ),
                ConfirmAccount = nameof( ConfirmAccount.Confirm ),
                DeleteAccount = nameof( ConfirmAccount.Delete ),
                ShowChangePasswordView = nameof( ConfirmAccount.ShowChangePasswordView ),
                ShowDeleteConfirmationView = nameof( ConfirmAccount.ShowDeleteConfirmationView )
            };
        }

        /// <summary>
        /// Gets the view based on the <see cref="ActionPageParameter"/>.
        /// <para>This should only be invoked during page load block initialization (not for each block action request), since it reacts to confirm account, delete account, and change password actions.</para>
        /// </summary>
        /// <returns>The action-based view.</returns>
        private ConfirmAccountViewBox ProcessActionView()
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );

                var action = this.ActionPageParameter;
                var code = this.CodeConfirmationPageParameter;
                UserLogin user;

                switch ( action?.ToLowerInvariant() )
                {
                    // Don't actually delete the account, but instead show a delete confirmation view to the individual.
                    case ConfirmAccountAction.Delete:
                        if ( !IsCodeValid( userLoginService, code, out user ) )
                        {
                            return ShowAccountConfirmationView( code );
                        }

                        return ShowDeleteConfirmationView( code, user );

                    // Don't actually change the password, but instead show a change password view to the individual.
                    case ConfirmAccountAction.ChangePassword:
                        if ( !IsCodeValid( userLoginService, code, out user ) )
                        {
                            return ShowAccountConfirmationView( code );
                        }

                        var component = AuthenticationContainer.GetComponent( user.EntityType.Name );

                        if ( !component.SupportsChangePassword )
                        {
                            return ShowAlertView( this.ChangePasswordUnavailableCaption, ConfirmAccountAlertType.Danger );
                        }

                        return ShowChangePasswordView( code, user );

                    // By default, we will try to automatically confirm the account.
                    default:
                        return Confirm( code );
                }
            }
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Gets the change password caption for the <paramref name="user"/>, using the <see cref="ChangePasswordCaptionTemplate"/>.
        /// </summary>
        /// <param name="user">The user for whom to generating the caption.</param>
        /// <returns>The change password caption for the user.</returns>
        private string GetChangePasswordCaption( UserLogin user )
        {
            var caption = this.ChangePasswordCaptionTemplate;
            if ( caption.Contains( "{1}" ) )
            {
                caption = string.Format( caption, user.Person.FirstName, user.UserName );
            }
            else if ( caption.Contains( "{0}" ) )
            {
                caption = string.Format( caption, user.Person.FirstName );
            }
            return caption;
        }

        /// <summary>
        /// Gets the delete confirmation caption for the <paramref name="user"/>, using the <see cref="DeleteConfirmationCaptionTemplate"/>.
        /// </summary>
        /// <param name="user">The user for whom to generate the caption.</param>
        /// <returns>The delete confirmation caption for the user.</returns>
        private string GetDeleteConfirmationCaption( UserLogin user )
        {
            var caption = this.DeleteConfirmationCaptionTemplate;

            if ( caption.Contains( "{0}" ) )
            {
                caption = string.Format( caption, user.UserName );
            }

            return caption;
        }

        /// <summary>
        /// Gets the invalid confirmation code caption using the <see cref="InvalidConfirmationCodeCaptionTemplate"/>.
        /// </summary>
        /// <returns>The invalid confirmation code caption.</returns>
        private string GetInvalidConfirmationCodeCaption()
        {
            var invalidCaption = this.InvalidConfirmationCodeCaptionTemplate;

            if ( invalidCaption.Contains( "{0}" ) )
            {
                var url = this.NewAccountPageUrl;
                if ( string.IsNullOrWhiteSpace( url ) )
                {
                    url = "/NewAccount";
                }

                invalidCaption = string.Format( invalidCaption, url );
            }

            return invalidCaption;
        }

        /// <summary>
        /// Gets the password changed caption for the <paramref name="user"/>, using the <see cref="PasswordChangedCaptionTemplate"/>.
        /// </summary>
        /// <param name="user">The user for whom to generating the caption.</param>
        /// <returns>The password changed caption for the user.</returns>
        private string GetPasswordChangedCaption( UserLogin user )
        {
            var caption = this.PasswordChangedCaptionTemplate;

            if ( caption.Contains( "{1}" ) )
            {
                caption = string.Format( caption, user.Person.FirstName, user.UserName );
            }
            else if ( caption.Contains( "{0}" ) )
            {
                caption = string.Format( caption, user.Person.FirstName );
            }

            return caption;
        }

        /// <summary>
        /// Determines whether the code is valid.
        /// </summary>
        /// <param name="userLoginService">The user login service.</param>
        /// <param name="code">The code.</param>
        /// <param name="user">The user associated with the <paramref name="code"/>, if valid; otherwise, set to <c>null</c>.</param>
        /// <returns>
        ///   <c>true</c> if the code valid; otherwise; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCodeValid( UserLoginService userLoginService, string code, out UserLogin user )
        {
            user = userLoginService.GetByConfirmationCode( code );
            return user != null;
        }

        /// <summary>
        /// Gets the required information to show the confirmation code view.
        /// </summary>
        /// <param name="invalidCode">The invalid code.</param>
        /// <returns>The required information to show the confirmation code view.</returns>
        private ConfirmAccountViewBox ShowAccountConfirmationView( string invalidCode )
        {
            return new ConfirmAccountViewBox
            {
                ViewType = ConfirmAccountViewType.AccountConfirmation,
                AccountConfirmationViewOptions = new ConfirmAccountAccountConfirmationViewOptionsBag
                {
                    ErrorCaption = invalidCode.IsNotNullOrWhiteSpace() ? GetInvalidConfirmationCodeCaption() : null
                }
            };
        }

        /// <summary>
        /// Gets the required information to show the alert view.
        /// </summary>
        /// <param name="alertContent">The alert content.</param>
        /// <param name="alertType">The alert type (<see cref="ConfirmAccountAlertType"/>).</param>
        /// <returns>The required information to show the alert view.</returns>
        private ConfirmAccountViewBox ShowAlertView( string alertContent, string alertType )
        {
            return new ConfirmAccountViewBox
            {
                ViewType = ConfirmAccountViewType.Alert,
                AlertViewOptions = new ConfirmAccountAlertViewOptionsBag
                {
                    Alert = new ConfirmAccountAlertControlBag
                    {
                        Content = alertContent,
                        IsHtml = true,
                        Type = alertType
                    }
                }
            };
        }

        private ConfirmAccountViewBox ShowChangePasswordView( string code, UserLogin user, string errorCaption = null )
        {
            return new ConfirmAccountViewBox
            {
                ViewType = ConfirmAccountViewType.ChangePassword,
                ChangePasswordViewOptions = new ConfirmAccountChangePasswordViewOptionsBag
                {
                    Code = code,
                    ErrorCaption = errorCaption,
                    ViewCaption = GetChangePasswordCaption( user )
                }
            };
        }

        private ConfirmAccountViewBox ShowContentView( string content )
        {
            return new ConfirmAccountViewBox
            {
                ViewType = ConfirmAccountViewType.Content,
                ContentViewOptions = new ConfirmAccountContentViewOptionsBag
                {
                    Content = content
                }
            };
        }

        private ConfirmAccountViewBox ShowDeleteConfirmationView( string code, UserLogin user )
        {
            return new ConfirmAccountViewBox
            {
                ViewType = ConfirmAccountViewType.DeleteConfirmation,
                DeleteConfirmationViewOptions = new ConfirmAccountDeleteConfirmationViewOptionsBag
                {
                    Code = code,
                    ViewCaption = GetDeleteConfirmationCaption( user )
                }
            };
        }

        #endregion

        #region Helper Classes

        private static class ConfirmAccountAlertType
        {
            public static readonly string Default = "default";
            public static readonly string Success = "success";
            public static readonly string Info = "info";
            public static readonly string Danger = "danger";
            public static readonly string Warning = "warning";
            public static readonly string Primary = "primary";
            public static readonly string Validation = "validation";
        }

        #endregion
    }
}