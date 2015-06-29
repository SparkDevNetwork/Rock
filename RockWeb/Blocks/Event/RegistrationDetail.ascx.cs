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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Event
{
    [DisplayName( "Registration Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a given registration." )]

    [LinkedPage( "Group Detail Page", "The page for viewing details about a group", true, "", "", 0 )]
    [LinkedPage( "Group Member Page", "The page for viewing details about a group member", true, "", "", 1 )]
    public partial class RegistrationDetail : RockBlock, IDetailBlock
    {

        #region Properties

        private RegistrationTemplate RegistrationTemplate { get; set; }
        private int? RegistrationTemplateId { get; set; }
        private int? RegistrationInstanceId { get; set; }
        private int? RegistrationId { get; set; }
        private int? RegistrantId { get; set; }
        private Dictionary<Guid, bool> RegistrantGuids { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            RegistrationInstanceId = ViewState["RegistrationInstanceId"] as int?;
            RegistrationId = ViewState["RegistrationId"] as int?;
            RegistrantGuids = ViewState["RegistrantGuids"] as Dictionary<Guid, bool>;

            BuildRegistrantControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Registration.FriendlyTypeName );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlRegistrationDetail );

            RegistrationInstanceId = PageParameter( "RegistrationInstanceId" ).AsIntegerOrNull();
            RegistrationId = PageParameter( "RegistrationId" ).AsIntegerOrNull();
            RegistrantId = PageParameter( "RegistrantId" ).AsIntegerOrNull();

            // This block may be used with a RegistrantId, RegistrationId, or RegistrationInstanceId passed in. 
            // Based on this, query for the remaining items.

            using ( var rockContext = new RockContext() )
            {
                if ( RegistrantId.HasValue )
                {
                    var registrant = new RegistrationRegistrantService( rockContext )
                        .Queryable( "Registration.RegistrationInstance.RegistrationTemplate.Forms.Fields" ).AsNoTracking()
                        .Where( r => r.Id == RegistrantId.Value )
                        .FirstOrDefault();
                    if ( registrant != null )
                    {
                        RegistrationId = registrant.RegistrationId;
                        RegistrationInstanceId = registrant.Registration.RegistrationInstanceId;
                        RegistrationTemplateId = registrant.Registration.RegistrationInstance.RegistrationTemplateId;
                        RegistrationTemplate = registrant.Registration.RegistrationInstance.RegistrationTemplate;
                    }
                }

                if ( RegistrationId.HasValue && !RegistrationInstanceId.HasValue )
                {
                    var registration = new RegistrationService( rockContext )
                        .Queryable( "RegistrationInstance.RegistrationTemplate.Forms.Fields" ).AsNoTracking()
                        .Where( r => r.Id == RegistrationId.Value )
                        .FirstOrDefault();
                    if ( registration != null )
                    {
                        RegistrationInstanceId = registration.RegistrationInstanceId;
                        RegistrationTemplateId = registration.RegistrationInstance.RegistrationTemplateId;
                        RegistrationTemplate = registration.RegistrationInstance.RegistrationTemplate;
                    }
                }

                if ( RegistrationInstanceId.HasValue && !RegistrationTemplateId.HasValue )
                {
                    var registrationInstance = new RegistrationInstanceService( rockContext )
                        .Queryable( "RegistrationTemplate.Forms.Fields" ).AsNoTracking()
                        .Where( r => r.Id == RegistrationInstanceId.Value )
                        .FirstOrDefault();
                    if ( registrationInstance != null )
                    {
                        RegistrationTemplateId = registrationInstance.RegistrationTemplateId;
                        RegistrationTemplate = registrationInstance.RegistrationTemplate;
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( RegistrationInstanceId.HasValue )
                {
                    ShowDetail( RegistrationId.Value, RegistrationInstanceId );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? registrationId = PageParameter( pageReference, "RegistrationId" ).AsIntegerOrNull();
            if ( registrationId.HasValue )
            {
                Registration registration = GetRegistration( registrationId.Value );
                if ( registration != null )
                {
                    breadCrumbs.Add( new BreadCrumb( registration.ToString(), pageReference ) );
                    return breadCrumbs;
                }
            }

            breadCrumbs.Add( new BreadCrumb( "New Registration", pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["RegistrationId"] = RegistrationId;
            ViewState["RegistrationInstanceId"] = RegistrationInstanceId;
            ViewState["RegistrantGuids"] = RegistrantGuids;

            return base.SaveViewState();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetRegistration( hfRegistrationId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();

            var registrationService = new RegistrationService( rockContext );
            Registration registration = registrationService.Get( int.Parse( hfRegistrationId.Value ) );

            if ( registration != null )
            {
                if ( !registration.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this registration.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !registrationService.CanDelete( registration, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                registrationService.Delete( registration );

                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Registration registration = null;
            RockContext rockContext = new RockContext();

            var registrationService = new RegistrationService( rockContext );

            int registrationId = int.Parse( hfRegistrationId.Value );

            if ( registrationId != 0 )
            {
                registration = registrationService.Queryable().Where( g => g.Id == registrationId ).FirstOrDefault();
            }

            if ( registration == null )
            {
                registration = new Registration { RegistrationInstanceId = RegistrationInstanceId ?? 0 };
                registrationService.Add( registration );
            }

            if ( registration != null && RegistrationInstanceId > 0 )
            {
                registration.PersonAliasId = ppPerson.PersonAliasId.Value;
                registration.FirstName = tbFirstName.Text;
                registration.LastName = tbLastName.Text;
                registration.ConfirmationEmail = ebConfirmationEmail.Text;

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !registration.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                } );

                // Reload registration
                ShowReadonlyDetails( GetRegistration( registration.Id ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfRegistrationId.Value.Equals( "0" ) )
            {
                var pageParams = new Dictionary<string, string>();
                pageParams.Add( "RegistrationInstanceId", RegistrationInstanceId.ToString() );
                NavigateToParentPage( pageParams );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ShowReadonlyDetails( GetRegistration( hfRegistrationId.Value.AsInteger() ) );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var currentRegistration = GetRegistration( hfRegistrationId.Value.AsInteger() );
            if ( currentRegistration != null )
            {
                ShowReadonlyDetails( currentRegistration );
            }
            else
            {
                string registrationId = PageParameter( "RegistrationId" );
                if ( !string.IsNullOrWhiteSpace( registrationId ) )
                {
                    ShowDetail( registrationId.AsInteger(), PageParameter( "registrationInstanceId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Registration Detail Events

        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppPerson.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var person = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Id == ppPerson.PersonId.Value )
                        .Select( p => new
                        {
                            Email = p.Email,
                            NickName = p.NickName,
                            LastName = p.LastName
                        } )
                        .FirstOrDefault();

                    if ( person != null )
                    {
                        if ( string.IsNullOrWhiteSpace( ebConfirmationEmail.Text ) )
                        {
                            ebConfirmationEmail.Text = person.Email;
                        }
                        if ( string.IsNullOrWhiteSpace( tbFirstName.Text ) )
                        {
                            tbFirstName.Text = person.NickName;
                        }
                        if ( string.IsNullOrWhiteSpace( tbLastName.Text ) )
                        {
                            tbLastName.Text = person.LastName;
                        }
                    }
                }
            }
        }

        #endregion

        #region Registrant Events

        void registrantEditor_SelectPersonClick( object sender, EventArgs e )
        {
            var editor = sender as RegistrantEditor;
            if ( editor != null )
            {
                if ( editor.PersonId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var person = new PersonService( rockContext ).Get( editor.PersonId.Value );
                        if ( person != null )
                        {
                            editor.Title = person.ToString();
                        }
                        else
                        {
                            editor.Title = "New Registrant";
                        }
                    }
                }
                else
                {
                    editor.Title = "New Registrant";
                }
            }
        }

        void registrantEditor_EditRegistrantClick( object sender, EventArgs e )
        {
            var editor = sender as RegistrantEditor;
            if ( editor != null )
            {
                editor.ShowEditMode();
            }
        }

        void registrantEditor_DeleteRegistrantClick( object sender, EventArgs e )
        {
            var editor = sender as RegistrantEditor;
            if ( editor != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = new RegistrationRegistrantService( rockContext );
                    var registrant = service.Get( editor.RegistrantGuid );
                    if ( registrant != null )
                    {
                        service.Delete( registrant );
                        rockContext.SaveChanges();
                    }
                }

                if ( RegistrantGuids.ContainsKey( editor.RegistrantGuid ) )
                {
                    phRegistrants.Controls.Remove( editor );
                }
                RegistrantGuids.Remove( editor.RegistrantGuid );
            }
        }

        void registrantEditor_SaveRegistrantClick( object sender, EventArgs e )
        {
            var editor = sender as RegistrantEditor;
            if ( editor != null && RegistrantGuids.ContainsKey( editor.RegistrantGuid ) )
            {
                var uiRegistrant = new RegistrationRegistrant();
                editor.SetRegistrantFromControl( uiRegistrant );

                using ( var rockContext = new RockContext() )
                {
                    var service = new RegistrationRegistrantService( rockContext );
                    RegistrationRegistrant registrant = null;

                    if ( !RegistrantGuids[editor.RegistrantGuid] )
                    {
                        registrant = service.Get( editor.RegistrantGuid );
                    }

                    if ( registrant == null )
                    {
                        registrant = new RegistrationRegistrant();
                        service.Add( registrant );
                    }
                    else
                    {
                        uiRegistrant.Id = registrant.Id;
                        uiRegistrant.Guid = registrant.Guid;
                    }

                    registrant.CopyPropertiesFrom( uiRegistrant );
                    registrant.RegistrationId = hfRegistrationId.ValueAsInt();
                    
                    rockContext.SaveChanges();

                    editor.SetControlFromRegistrant( registrant );

                    var registration = new RegistrationService( rockContext ).Get( hfRegistrationId.ValueAsInt() );
                    SetCostLabels( registration );

                    RegistrantGuids[editor.RegistrantGuid] = false;
                }

                editor.ShowViewMode();
            }
        }

        void registrantEditor_CancelRegistrantClick( object sender, EventArgs e )
        {
            var editor = sender as RegistrantEditor;
            if ( editor != null )
            {
                if ( RegistrantGuids.ContainsKey( editor.RegistrantGuid ) && RegistrantGuids[editor.RegistrantGuid] )
                {
                    phRegistrants.Controls.Remove( editor );
                    RegistrantGuids.Remove( editor.RegistrantGuid );
                }
                else
                {
                    editor.ShowViewMode();
                }
            }
        }

        protected void lbAddRegistrant_Click( object sender, EventArgs e )
        {
            Guid guid = Guid.NewGuid();
            var registrant = new RegistrationRegistrant { Guid = guid, RegistrationId = hfRegistrationId.ValueAsInt() };
            RegistrantGuids.Add( guid, true );

            var editor = AddRegistrantControl( guid, registrant );
            editor.Expanded = true;
            editor.ShowEditMode();
        }

        #endregion

        #region Methods

        #region Display Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        public void ShowDetail( int registrationId )
        {
            ShowDetail( registrationId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        public void ShowDetail( int registrationId, int? registrationInstanceId )
        {
            Registration registration = null;

            bool viewAllowed = false;
            bool editAllowed = IsUserAuthorized( Authorization.EDIT );
            RockContext rockContext = new RockContext();

            if ( !registrationId.Equals( 0 ) )
            {
                registration = GetRegistration( registrationId, rockContext );
            }

            if ( registration == null && registrationInstanceId.HasValue )
            {
                registration = new Registration { Id = 0, RegistrationInstanceId = registrationInstanceId ?? 0 };
            }

            if ( registration != null )
            {
                lReadOnlyTitle.Text = "Registration".FormatAsHtmlTitle();

                RegistrationInstanceId = registration.RegistrationInstanceId;

                viewAllowed = editAllowed || registration.IsAuthorized( Authorization.VIEW, CurrentPerson );
                editAllowed = IsUserAuthorized( Authorization.EDIT ) || registration.IsAuthorized( Authorization.EDIT, CurrentPerson );

                pnlDetails.Visible = viewAllowed;

                hfRegistrationId.Value = registration.Id.ToString();

                // render UI based on Authorized and IsSystem
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                if ( !editAllowed )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    ShowReadonlyDetails( registration );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;
                    if ( registration.Id > 0 )
                    {
                        ShowReadonlyDetails( registration );
                    }
                    else
                    {
                        ShowEditDetails( registration );
                    }
                }

                BuildRegistrantControls( registration.Registrants );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="registration">The group.</param>
        private void ShowEditDetails( Registration registration )
        {
            SetCostLabels( registration );

            SetEditMode( true );

            if ( registration.PersonAlias != null )
            {
                ppPerson.SetValue( registration.PersonAlias.Person );
            }
            else
            {
                ppPerson.SetValue( null );
            }

            tbFirstName.Text = registration.FirstName;
            tbLastName.Text = registration.LastName;
            ebConfirmationEmail.Text = registration.ConfirmationEmail;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="registration">The group.</param>
        private void ShowReadonlyDetails( Registration registration )
        {
            SetEditMode( false );

            var rockContext = new RockContext();

            hfRegistrationId.SetValue( registration.Id );

            SetCostLabels( registration );

            if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
            {
                lName.Text = registration.PersonAlias.Person.FullName;
            }
            else
            {
                lName.Text = string.Format( "{0} {1}", registration.FirstName, registration.LastName );
            }

            lConfirmationEmail.Text = registration.ConfirmationEmail;
            lConfirmationEmail.Visible = !string.IsNullOrWhiteSpace( registration.ConfirmationEmail );

            if ( registration.Group != null )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "GroupId", registration.Group.Id.ToString() );
                string groupUrl = LinkedPageUrl( "GroupDetailPage", qryParams );

                lGroup.Text = string.Format( "<a href='{0}'>{1}</a>", groupUrl, registration.Group.Name );
                lGroup.Visible = true;
            }
            else
            {
                lGroup.Visible = false;
            }
        }

        /// <summary>
        /// Sets the cost labels.
        /// </summary>
        /// <param name="registration">The registration.</param>
        private void SetCostLabels( Registration registration )
        {
            if ( registration != null && registration.TotalCost > 0.0M )
            {
                hlCost.Visible = true;
                hlCost.Text = registration.TotalCost.ToString( "C2" );

                hlBalance.Visible = true;
                hlBalance.Text = registration.BalanceDue.ToString( "C2" );
                hlBalance.LabelType = registration.BalanceDue > 0 ? LabelType.Danger : LabelType.Success;
            }
            else
            {
                hlCost.Visible = false;
                hlBalance.Visible = false;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <param name="registrationId">The group identifier.</param>
        /// <returns></returns>
        private Registration GetRegistration( int registrationId, RockContext rockContext = null )
        {
            string key = string.Format( "Registration:{0}", registrationId );
            Registration registration = RockPage.GetSharedItem( key ) as Registration;
            if ( registration == null )
            {
                rockContext = rockContext ?? new RockContext();
                registration = new RegistrationService( rockContext ).Queryable( "PersonAlias.Person,Group,Registrants.Fees" )
                    .Where( g => g.Id == registrationId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, registration );
            }

            return registration;
        }

        #endregion

        #region Registrant Control Methods

        /// <summary>
        /// Adds the registrant controls.
        /// </summary>
        /// <param name="registrants">The registrants.</param>
        private void BuildRegistrantControls( IEnumerable<RegistrationRegistrant> registrants )
        {
            phRegistrants.Controls.Clear();
            RegistrantGuids = new Dictionary<Guid, bool>();

            foreach( var registrant in registrants )
            {
                RegistrantGuids.Add( registrant.Guid, false );
                AddRegistrantControl( registrant.Guid, registrant );
            }
        }

        /// <summary>
        /// Adds the registrant controls.
        /// </summary>
        private void BuildRegistrantControls()
        {
            phRegistrants.Controls.Clear();
            if ( RegistrantGuids != null )
            {
                RegistrantGuids.Keys.ToList().ForEach( r =>
                    AddRegistrantControl( r, null ) );
            }
        }

        /// <summary>
        /// Adds the registrant control.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="registrant">The registrant.</param>
        private RegistrantEditor AddRegistrantControl( Guid guid, RegistrationRegistrant registrant = null )
        {
            var registrantEditor = new RegistrantEditor();
            registrantEditor.SetForms( RegistrationTemplate );
            phRegistrants.Controls.Add( registrantEditor );
            registrantEditor.ID = "re" + guid;
            registrantEditor.SelectPersonClick += registrantEditor_SelectPersonClick;
            registrantEditor.EditRegistrantClick += registrantEditor_EditRegistrantClick;
            registrantEditor.DeleteRegistrantClick += registrantEditor_DeleteRegistrantClick;
            registrantEditor.SaveRegistrantClick += registrantEditor_SaveRegistrantClick;
            registrantEditor.CancelRegistrantClick += registrantEditor_CancelRegistrantClick;

            if ( registrant != null )
            {
                registrantEditor.SetControlFromRegistrant( registrant );
                if ( !Page.IsPostBack && RegistrantId.HasValue && RegistrantId.Value == registrant.Id )
                {
                    pwRegistrationDetails.Expanded = false;
                    registrantEditor.Expanded = true;
                }
            }

            return registrantEditor;
        }

        #endregion

        #region Dialog Methods

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
                default:
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
                default:
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        #endregion


    }
}