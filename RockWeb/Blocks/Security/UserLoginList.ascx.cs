// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class UserLoginList : Rock.Web.UI.RockBlock
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

                // Hide the person name column
                gUserLogins.Columns[1].Visible = false;
            }

            _canEdit = IsUserAuthorized( Authorization.EDIT );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gUserLogins.DataKeyNames = new string[] { "Id" };
            gUserLogins.Actions.ShowAdd = _personId.HasValue && _canEdit;
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
                    SetPasswordState();
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
                        var entityType = EntityTypeCache.Read( compProviderFilter.SelectedValue.AsGuid() );
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
            gfSettings.SaveUserPreference( "Username", tbUserNameFilter.Text );
            gfSettings.SaveUserPreference( "Authentication Provider", compProviderFilter.SelectedValue );
            gfSettings.SaveUserPreference( "Created", drpCreated.DelimitedValues );
            gfSettings.SaveUserPreference( "Last Login", drpLastLogin.DelimitedValues );
            gfSettings.SaveUserPreference( "Is Confirmed", ddlIsConfirmedFilter.SelectedValue );
            gfSettings.SaveUserPreference( "Is Locked Out", ddlLockedOutFilter.SelectedValue );

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

                int userLoginId = int.Parse( hfIdValue.Value );

                if ( userLoginId != 0 )
                {
                    userLogin = service.Get( userLoginId );
                }

                if ( userLogin == null )
                {
                    userLogin = new UserLogin();

                    if ( _personId.HasValue )
                    {
                        userLogin.PersonId = _personId;
                    }
                    else
                    {
                        nbErrorMessage.Title = "Invalid Situation";
                        nbErrorMessage.Text = "The person you are editing has no person Id.";
                        nbErrorMessage.Visible = true;
                        return;
                    }

                    if ( service.GetByUserName( tbUserName.Text.Trim() ) != null )
                    {
                        nbErrorMessage.Title = "Invalid User Name";
                        nbErrorMessage.Text = "The User Name you selected already exists.  Please select a different User Name.";
                        nbErrorMessage.Visible = true;
                        return;
                    }

                    service.Add( userLogin );
                }

                userLogin.UserName = tbUserName.Text.Trim();
                userLogin.IsConfirmed = cbIsConfirmed.Checked;
                userLogin.IsLockedOut = cbIsLockedOut.Checked;

                var entityType = EntityTypeCache.Read( compProvider.SelectedValue.AsGuid() );
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
                                    nbErrorMessage.Title = "Invalid Password";
                                    nbErrorMessage.Text = UserLoginService.FriendlyPasswordRules();
                                    nbErrorMessage.Visible = true;
                                    return;
                                }
                            }
                            else
                            {
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

                HideDialog();
                BindGrid();
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
            tbUserNameFilter.Text = gfSettings.GetUserPreference( "Username" );
            compProviderFilter.SetValue( gfSettings.GetUserPreference( "Authentication Provider" ) );
            drpCreated.DelimitedValues = gfSettings.GetUserPreference( "Created" );
            drpLastLogin.DelimitedValues = gfSettings.GetUserPreference( "Last Login" );
            ddlIsConfirmedFilter.SetValue( gfSettings.GetUserPreference( "Is Confirmed" ) );
            ddlLockedOutFilter.SetValue( gfSettings.GetUserPreference( "Is Locked Out" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) && ContextEntity<Person>() == null )
            {
                return;
            }

            var qry = new UserLoginService( new RockContext() ).Queryable()
                .Where( l => !_personId.HasValue || l.PersonId == _personId.Value );

            // username filter
            string usernameFilter = gfSettings.GetUserPreference( "Username" );
            if ( !string.IsNullOrWhiteSpace( usernameFilter ) )
            {
                qry = qry.Where( l => l.UserName.StartsWith( usernameFilter ) );
            }

            // provider filter
            Guid guid = Guid.Empty;
            if ( Guid.TryParse( gfSettings.GetUserPreference( "Authentication Provider" ), out guid ) )
            {
                qry = qry.Where( l => l.EntityType.Guid.Equals( guid ) );
            }

            // created filter
            var drp = new DateRangePicker();
            drp.DelimitedValues = gfSettings.GetUserPreference( "Created" );
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
            drp2.DelimitedValues = gfSettings.GetUserPreference( "Last Login" );
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
            if ( bool.TryParse( gfSettings.GetUserPreference( "Is Confirmed" ), out isConfirmed ) )
            {
                qry = qry.Where( l => l.IsConfirmed == isConfirmed || ( !isConfirmed && l.IsConfirmed == null ) );
            }

            // is locked out filter
            bool isLockedOut = false;
            if ( bool.TryParse( gfSettings.GetUserPreference( "Is Locked Out" ), out isLockedOut ) )
            {
                qry = qry.Where( l => l.IsLockedOut == isLockedOut || ( !isLockedOut && l.IsLockedOut == null ) );
            }

            // Sort
            SortProperty sortProperty = gUserLogins.SortProperty;
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty( new GridViewSortEventArgs( "UserName", SortDirection.Ascending ) );
            }

            gUserLogins.DataSource = qry.Sort( sortProperty )
                .Select( l => new
                    {
                        Id = l.Id,
                        UserName = l.UserName,
                        PersonId = l.PersonId,
                        PersonName = l.Person.LastName + ", " + l.Person.NickName,
                        ProviderName = l.EntityType.FriendlyName,
                        CreatedDateTime = l.CreatedDateTime,
                        LastLoginDateTime = l.LastLoginDateTime,
                        IsConfirmed = l.IsConfirmed,
                        IsLockedOut = l.IsLockedOut
                    } ).ToList();
            gUserLogins.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int userLoginId )
        {
            UserLogin userLogin = null;
            if ( userLoginId > 0 )
            {
                userLogin = new UserLoginService( new RockContext() ).Get( userLoginId );
            }

            if ( userLogin == null )
            {
                userLogin = new UserLogin { Id = 0, IsConfirmed = true };
            }

            tbUserName.Text = userLogin.UserName;
            cbIsConfirmed.Checked = userLogin.IsConfirmed ?? false;
            cbIsLockedOut.Checked = userLogin.IsLockedOut ?? false;
            if ( userLogin.EntityType != null )
            {
                compProvider.SetValue( userLogin.EntityType.Guid.ToString().ToUpper() );
            }

            hfIdValue.Value = userLogin.Id.ToString();

            SetPasswordState();

            ShowDialog( "EDITLOGIN", true );
        }

        /// <summary>
        /// Sets the state of the password.
        /// </summary>
        private void SetPasswordState()
        {
            tbPassword.Enabled = false;
            tbPasswordConfirm.Enabled = false;
            tbPassword.Required = false;
            tbPasswordConfirm.Required = false;

            var entityType = EntityTypeCache.Read( compProvider.SelectedValue.AsGuid() );
            if ( entityType != null )
            {
                var component = AuthenticationContainer.GetComponent( entityType.Name );
                if ( component != null )
                {
                    if ( component.ServiceType == AuthenticationServiceType.Internal )
                    {
                        tbPassword.Enabled = true;
                        tbPasswordConfirm.Enabled = true;

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