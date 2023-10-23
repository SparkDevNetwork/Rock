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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for displaying logins.  By default displays all logins, but can be configured to use person context to display logins for a specific person.
    /// </summary>
    [DisplayName( "User Login List" )]
    [Category( "Security" )]
    [Description( "Block for displaying logins.  By default displays all logins, but can be configured to use person context to display logins for a specific person." )]
    [ContextAware]
    [Rock.SystemGuid.BlockTypeGuid( "CE06640D-C1BA-4ACE-AF03-8D733FD3247C" )]
    public partial class UserLoginList : RockBlock, ICustomGridColumns
    {
        #region Fields

        private int? _personId = null;
        private bool _canEdit = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            base.OnInit( e );

            var person = ContextEntity<Person>();
            if ( person != null )
            {
                _personId = person.Id;

                var personNameField = gUserLogins.ColumnsOfType<PersonField>().FirstOrDefault( a => a.HeaderText == "Person" );
                if ( personNameField != null )
                {
                    // Hide the person name column
                    personNameField.Visible = false;
                }
                ppPerson.Visible = false;
            }

            _canEdit = IsUserAuthorized( Authorization.EDIT );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gUserLogins.DataKeyNames = new string[] { "Id" };
            gUserLogins.Actions.ShowAdd = _canEdit;
            gUserLogins.Actions.AddClick += gUserLogins_Add;
            gUserLogins.IsDeleteEnabled = _canEdit;
            gUserLogins.GridRebind += gUserLogins_GridRebind;

            if ( _canEdit )
            {
                gUserLogins.RowSelected += gUserLogins_Edit;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
            else
            {
                if ( _canEdit && !string.IsNullOrWhiteSpace( hfIdValue.Value ) )
                {
                    nbErrorMessage.Visible = false;
                    SetUIControls();
                }

                ShowDialog();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Gfs the settings_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfSettings_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Authentication Provider":
                    {
                        var entityType = EntityTypeCache.Get( compProviderFilter.SelectedValue.AsGuid() );
                        if ( entityType != null )
                        {
                            e.Value = entityType.FriendlyName;
                        }

                        break;
                    }

                case "Created":
                case "Last Login":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                case "Is Confirmed":
                case "Is Locked Out":
                    {
                        bool value = false;
                        if ( bool.TryParse( e.Value, out value ) )
                        {
                            e.Value = value ? "Yes" : "No";
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SetFilterPreference( "Username", tbUserNameFilter.Text );
            gfSettings.SetFilterPreference( "Authentication Provider", compProviderFilter.SelectedValue );
            gfSettings.SetFilterPreference( "Created", drpCreated.DelimitedValues );
            gfSettings.SetFilterPreference( "Last Login", drpLastLogin.DelimitedValues );
            gfSettings.SetFilterPreference( "Is Confirmed", ddlIsConfirmedFilter.SelectedValue );
            gfSettings.SetFilterPreference( "Is Locked Out", ddlLockedOutFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gUserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gUserLogins_Add( object sender, EventArgs e )
        {
            if ( _canEdit )
            {
                ShowEdit( 0 );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gUserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gUserLogins_Edit( object sender, RowEventArgs e )
        {
            if ( _canEdit )
            {
                ShowEdit( e.RowKeyId );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gUserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gUserLogins_Delete( object sender, RowEventArgs e )
        {
            if ( _canEdit )
            {
                var rockContext = new RockContext();
                var service = new UserLoginService( rockContext );
                var userLogin = service.Get( e.RowKeyId );

                if ( userLogin != null )
                {
                    string errorMessage;
                    if ( !service.CanDelete( userLogin, out errorMessage ) )
                    {
                        maGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    service.Delete( userLogin );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gUserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gUserLogins_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgDetails_SaveClick( object sender, EventArgs e )
        {
            if ( _canEdit )
            {
                var rockContext = new RockContext();
                UserLogin userLogin = null;
                var service = new UserLoginService( rockContext );
                string newUserName = tbUserNameEdit.Text.Trim();

                int userLoginId = int.Parse( hfIdValue.Value );

                if ( userLoginId != 0 )
                {
                    userLogin = service.Get( userLoginId );
                }

                // Check to see if there is a change to the username, and if so check that the new username does not exist.
                if ( userLogin == null || ( userLogin.UserName != newUserName ) )
                {
                    if ( service.GetByUserName( newUserName ) != null )
                    {
                        // keep looking until we find the next available one 
                        int numericSuffix = 1;
                        string nextAvailableUserName = newUserName + numericSuffix.ToString();
                        while ( service.GetByUserName( nextAvailableUserName ) != null )
                        {
                            numericSuffix++;
                            nextAvailableUserName = newUserName + numericSuffix.ToString();
                        }

                        nbErrorMessage.NotificationBoxType = NotificationBoxType.Warning;
                        nbErrorMessage.Title = "Invalid User Name";
                        nbErrorMessage.Text = "The User Name you selected already exists. Next available username: " + nextAvailableUserName;
                        nbErrorMessage.Visible = true;
                        return;
                    }
                }

                if ( userLogin == null )
                {
                    userLogin = new UserLogin();

                    if ( _personId.HasValue )
                    {
                        userLogin.PersonId = _personId;
                    }
                    else if ( ppPerson.PersonId.HasValue )
                    {
                        userLogin.PersonId = ppPerson.PersonId.Value;
                    }
                    else
                    {
                        nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
                        nbErrorMessage.Title = "Invalid Situation";
                        nbErrorMessage.Text = "No person selected, or the person you are editing has no person Id.";
                        nbErrorMessage.Visible = true;
                        return;
                    }

                    service.Add( userLogin );
                }

                userLogin.UserName = newUserName;
                userLogin.IsConfirmed = cbIsConfirmed.Checked;
                userLogin.IsLockedOut = cbIsLockedOut.Checked;
                userLogin.IsPasswordChangeRequired = cbIsRequirePasswordChange.Checked;

                var entityType = EntityTypeCache.Get( compProvider.SelectedValue.AsGuid() );
                if ( entityType != null )
                {
                    userLogin.EntityTypeId = entityType.Id;

                    if ( !string.IsNullOrWhiteSpace( tbPassword.Text ) )
                    {
                        var component = AuthenticationContainer.GetComponent( entityType.Name );
                        if ( component != null && component.ServiceType == AuthenticationServiceType.Internal )
                        {
                            if ( tbPassword.Text == tbPasswordConfirm.Text )
                            {
                                if ( UserLoginService.IsPasswordValid( tbPassword.Text ) )
                                {
                                    userLogin.Password = component.EncodePassword( userLogin, tbPassword.Text );
                                    userLogin.LastPasswordChangedDateTime = RockDateTime.Now;
                                }
                                else
                                {
                                    nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
                                    nbErrorMessage.Title = "Invalid Password";
                                    nbErrorMessage.Text = UserLoginService.FriendlyPasswordRules();
                                    nbErrorMessage.Visible = true;
                                    return;
                                }
                            }
                            else
                            {
                                nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
                                nbErrorMessage.Title = "Invalid Password";
                                nbErrorMessage.Text = "Password and Confirmation do not match.";
                                nbErrorMessage.Visible = true;
                                return;
                            }
                        }
                    }
                }

                if ( !userLogin.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                rockContext.SaveChanges();

                // Reload the page so that other blocks will know about any data that changed as a result of a new login.
                // On the PersonProfile page, this will help update the UserProtectionProfile label just in case it was changed.
                this.NavigateToCurrentPageReference();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the UserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbUserNameFilter.Text = gfSettings.GetFilterPreference( "Username" );
            compProviderFilter.SetValue( gfSettings.GetFilterPreference( "Authentication Provider" ) );
            drpCreated.DelimitedValues = gfSettings.GetFilterPreference( "Created" );
            drpLastLogin.DelimitedValues = gfSettings.GetFilterPreference( "Last Login" );
            ddlIsConfirmedFilter.SetValue( gfSettings.GetFilterPreference( "Is Confirmed" ) );
            ddlLockedOutFilter.SetValue( gfSettings.GetFilterPreference( "Is Locked Out" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            int personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) && ContextEntity<Person>() == null )
            {
                return;
            }

            var qry = new UserLoginService( new RockContext() ).Queryable()
                .Where( l => !_personId.HasValue || l.PersonId == _personId.Value );

            // username filter
            string usernameFilter = gfSettings.GetFilterPreference( "Username" );
            if ( !string.IsNullOrWhiteSpace( usernameFilter ) )
            {
                qry = qry.Where( l => l.UserName.StartsWith( usernameFilter ) );
            }

            // provider filter
            Guid? authProviderGuid = gfSettings.GetFilterPreference( "Authentication Provider" ).AsGuidOrNull();
            if ( authProviderGuid.HasValue )
            {
                qry = qry.Where( l => l.EntityType.Guid.Equals( authProviderGuid.Value ) );
            }

            // created filter
            var drp = new DateRangePicker();
            drp.DelimitedValues = gfSettings.GetFilterPreference( "Created" );
            if ( drp.LowerValue.HasValue )
            {
                qry = qry.Where( l => l.CreatedDateTime.HasValue && l.CreatedDateTime.Value >= drp.LowerValue.Value );
            }

            if ( drp.UpperValue.HasValue )
            {
                DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                qry = qry.Where( l => l.CreatedDateTime.HasValue && l.CreatedDateTime < upperDate );
            }

            // last login filter
            var drp2 = new DateRangePicker();
            drp2.DelimitedValues = gfSettings.GetFilterPreference( "Last Login" );
            if ( drp2.LowerValue.HasValue )
            {
                qry = qry.Where( l => l.LastLoginDateTime >= drp2.LowerValue.Value );
            }

            if ( drp2.UpperValue.HasValue )
            {
                DateTime upperDate = drp2.UpperValue.Value.Date.AddDays( 1 );
                qry = qry.Where( l => l.LastLoginDateTime < upperDate );
            }

            // Is Confirmed filter
            bool isConfirmed = false;
            if ( bool.TryParse( gfSettings.GetFilterPreference( "Is Confirmed" ), out isConfirmed ) )
            {
                qry = qry.Where( l => l.IsConfirmed == isConfirmed || ( !isConfirmed && l.IsConfirmed == null ) );
            }

            // is locked out filter
            bool isLockedOut = false;
            if ( bool.TryParse( gfSettings.GetFilterPreference( "Is Locked Out" ), out isLockedOut ) )
            {
                qry = qry.Where( l => l.IsLockedOut == isLockedOut || ( !isLockedOut && l.IsLockedOut == null ) );
            }

            // Sort
            SortProperty sortProperty = gUserLogins.SortProperty;
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty( new GridViewSortEventArgs( "UserName", SortDirection.Ascending ) );
            }

            gUserLogins.EntityTypeId = EntityTypeCache.Get<UserLogin>().Id;
            gUserLogins.DataSource = qry.Sort( sortProperty ).ToList();
            gUserLogins.DataBind();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gUserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gUserLogins_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            UserLogin userLogin = e.Row.DataItem as UserLogin;
            Literal lUserNameOrRemoteProvider = e.Row.FindControl( "lUserNameOrRemoteProvider" ) as Literal;
            Literal lProviderName = e.Row.FindControl( "lProviderName" ) as Literal;
            if ( userLogin != null )
            {
                lUserNameOrRemoteProvider.Text = userLogin.UserName;
                if ( userLogin.EntityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( userLogin.EntityTypeId.Value );
                    if ( entityType != null )
                    {
                        var component = AuthenticationContainer.GetComponent( entityType.Name );
                        lProviderName.Text = entityType.FriendlyName;
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int userLoginId )
        {
            tbPassword.ClearPassword();
            tbPasswordConfirm.ClearPassword();
            UserLogin userLogin = null;
            ppPerson.Visible = false;

            if ( userLoginId > 0 )
            {
                userLogin = new UserLoginService( new RockContext() ).Get( userLoginId );
                ppPerson.PersonId = userLogin.PersonId;
            }
            else
            {
                // Shows the person picker when adding a new login outside of a person context
                ppPerson.SetValue( null );
                if ( !_personId.HasValue )
                {
                    ppPerson.Visible = true;
                }
            }

            if ( userLogin == null )
            {
                userLogin = new UserLogin { Id = 0, IsConfirmed = true };
            }

            tbUserNameEdit.Text = userLogin.UserName;
            cbIsConfirmed.Checked = userLogin.IsConfirmed ?? false;
            cbIsLockedOut.Checked = userLogin.IsLockedOut ?? false;
            cbIsRequirePasswordChange.Checked = userLogin.IsPasswordChangeRequired ?? false;

            if ( userLogin.EntityType != null )
            {
                compProvider.SetValue( userLogin.EntityType.Guid.ToString().ToUpper() );
            }
            else
            {
                compProvider.SetValue( string.Empty );
            }

            hfIdValue.Value = userLogin.Id.ToString();

            SetUIControls();

            ShowDialog( "EDITLOGIN", true );
        }

        /// <summary>
        /// Set up the controls based on the AuthenticationServiceType
        /// </summary>
        private void SetUIControls()
        {
            tbUserNameEdit.Visible = true;
            rcwPassword.Visible = false;
            tbPassword.Required = false;
            tbPasswordConfirm.Required = false;

            var entityType = EntityTypeCache.Get( compProvider.SelectedValue.AsGuid() );
            if ( entityType != null )
            {
                var component = AuthenticationContainer.GetComponent( entityType.Name );
                if ( component != null )
                {
                    cbIsRequirePasswordChange.Visible = component.SupportsChangePassword;

                    if ( !component.SupportsChangePassword )
                    {
                        cbIsRequirePasswordChange.Checked = false;
                    }

                    rcwPassword.Visible = component.PromptForPassword;
                    tbUserNameEdit.Visible = true;

                    if ( component.PromptForPassword )
                    {
                        tbPassword.Enabled = true;
                        tbPasswordConfirm.Enabled = true;
                        tbPassword.Focus();

                        if ( hfIdValue.Value == "0" )
                        {
                            tbPassword.Required = true;
                            tbPasswordConfirm.Required = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "EDITLOGIN":
                    dlgDetails.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "EDITLOGIN":
                    dlgDetails.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
    }
}