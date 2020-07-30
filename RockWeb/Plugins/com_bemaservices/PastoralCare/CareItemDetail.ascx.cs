// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;

using Newtonsoft.Json;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;
using com.bemaservices.PastoralCare.Model;
using System.Web.UI.HtmlControls;

namespace RockWeb.Plugins.com_bemaservices.PastoralCare
{
    [DisplayName( "Care Item Detail" )]
    [Category( "BEMA Services > Pastoral Care" )]
    [Description( "Displays the details of the given care item for editing." )]

    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, order: 0 )]
    [BadgesField( "Badges", "The person badges to display in this block.", false, "", "", 0 )]
    public partial class CareItemDetail : PersonBlock, IDetailBlock, ICustomGridColumns
    {
        #region Properties
        public int? _careTypeId = null;
        public List<CareTypeItem> ItemsState { get; set; }
        public List<AttributeCache> AvailableAttributes { get; set; }

        private DeleteField _deleteField = null;
        private int? _deleteFieldColumnIndex = null;

        private EditField _editField = null;
        private int? _editFieldColumnIndex = null;
        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["ItemsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ItemsState = new List<CareTypeItem>();
            }
            else
            {
                ItemsState = JsonConvert.DeserializeObject<List<CareTypeItem>>( json );
            }

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;
            AddDynamicControls();

            //  BuildAttributes( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gCareContacts.DataKeyNames = new string[] { "Id" };
            gCareContacts.Actions.ShowAdd = true;
            gCareContacts.Actions.AddClick += gCareContacts_Add;
            gCareContacts.GridRebind += gCareContacts_GridRebind;
            gCareContacts.RowDataBound += gCareContacts_RowDataBound;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upDetail );

            // Get the careType id of the careType that user navigated from 
            _careTypeId = PageParameter( "CareTypeId" ).AsIntegerOrNull();

            // Load the other careTypes user is authorized to view 
            cblCareTypes.Items.Clear();
            using ( var rockContext = new RockContext() )
            {
                foreach ( var careType in new CareTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( c => c.Name ) )
                {
                    if ( !_careTypeId.HasValue && careType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        _careTypeId = careType.Id;
                    }

                    if ( UserCanEdit || careType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        cblCareTypes.Items.Add( new ListItem( careType.Name, careType.Id.ToString() ) );
                    }
                }
            }
            cblCareTypes.SelectedIndexChanged += cblCareTypes_SelectedIndexChanged;

            string badgeList = GetAttributeValue( "Badges" );
            if ( !string.IsNullOrWhiteSpace( badgeList ) )
            {
                pnlBadges.Visible = true;
                foreach ( string badgeGuid in badgeList.SplitDelimitedValues() )
                {
                    Guid guid = badgeGuid.AsGuid();
                    if ( guid != Guid.Empty )
                    {
                        var personBadge = BadgeCache.Get( guid );
                        if ( personBadge != null )
                        {
                            blStatus.BadgeTypes.Add( personBadge );
                        }
                    }
                }
            }
            else
            {
                pnlBadges.Visible = false;
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var careItemId = PageParameter( "CareItemId" ).AsInteger();
            nbErrorMessage.Visible = false;
            nbNoParameterMessage.Visible = false;

            if ( careItemId == 0 && PageParameter( "CareTypeId" ).AsIntegerOrNull() == null )
            {
                nbNoParameterMessage.Visible = true;
                pnlContents.Visible = false;
                wpCareContacts.Visible = false;
                return;
            }

            if ( !Page.IsPostBack )
            {
                ShowDetail( careItemId );
            }
            else
            {

                if ( careItemId > 0 )
                {
                    CareItem careItem = new CareItemService( new RockContext() ).Get( careItemId );
                    if ( careItem != null )
                    {
                        // Set the person
                        Person = careItem.PersonAlias.Person;
                    }
                }

                if ( pnlEditDetails.Visible )
                {
                    ShowItemAttributes();
                }

                if ( dlgCareContacts.Visible )
                {
                    ShowContactAttributes();
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["ItemsState"] = JsonConvert.SerializeObject( ItemsState, Formatting.None, jsonSetting );
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var rockContext = new RockContext();
            var breadCrumbs = new List<BreadCrumb>();

            CareItem careItem = null;

            int? requestId = PageParameter( "CareItemId" ).AsIntegerOrNull();
            if ( requestId.HasValue && requestId.Value > 0 )
            {
                careItem = new CareItemService( rockContext ).Get( requestId.Value );
            }

            if ( careItem != null )
            {
                breadCrumbs.Add( new BreadCrumb( careItem.PersonAlias.Person.FullName, pageReference ) );
            }
            else
            {
                var careType = new CareTypeService( rockContext ).Get( PageParameter( "CareTypeId" ).AsInteger() );
                if ( careType != null )
                {
                    breadCrumbs.Add( new BreadCrumb( String.Format( "New {0} Care Item", careType.Name ), pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Care Item", pageReference ) );
                }
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        #region View/Edit Panel Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ShowEditDetails( new CareItemService( rockContext ).Get( hfCareItemId.ValueAsInt() ), rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            int careItemId = hfCareItemId.ValueAsInt();
            if ( careItemId > 0 )
            {
                ShowReadonlyDetails( new CareItemService( new RockContext() ).Get( careItemId ) );
                pnlReadDetails.Visible = true;
                pnlEditDetails.Visible = false;
                wpCareContacts.Visible = true;
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCareTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCareTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowItemAttributes();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !ppPerson.PersonAliasId.HasValue )
            {
                ShowErrorMessage( "Incomplete", "You must select a person to save a care item." );
                return;
            }

            if ( Page.IsValid )
            {
                using ( var rockContext = new RockContext() )
                {
                    CareTypeItemService careTypeItemService = new CareTypeItemService( rockContext );
                    CareItemService careItemService = new CareItemService( rockContext );
                    CareItem careItem = null;

                    int careItemId = hfCareItemId.ValueAsInt();

                    // if adding a new connection request
                    if ( careItemId.Equals( 0 ) )
                    {
                        careItem = new CareItem();
                        careItemService.Add( careItem );
                    }
                    else
                    {
                        // load existing connection request
                        careItem = careItemService.Get( careItemId );
                    }

                    var personAliasService = new PersonAliasService( rockContext );

                    int? oldContactorPersonAliasId = careItem.ContactorPersonAliasId;
                    int? newContactorPersonId = ppContactorEdit.PersonId;
                    int? newContactorPersonAliasId = newContactorPersonId.HasValue ? personAliasService.GetPrimaryAliasId( newContactorPersonId.Value ) : ( int? ) null;

                    careItem.ContactorPersonAliasId = newContactorPersonAliasId;
                    careItem.PersonAlias = personAliasService.Get( ppPerson.PersonAliasId.Value );
                    careItem.PersonAliasId = ppPerson.PersonAliasId.Value;
                    careItem.IsActive = cbIsActive.Checked;

                    careItem.Description = tbDescription.Text.SanitizeHtml();
                    careItem.ContactDateTime = dtpContactDate.SelectedDateTime.Value;

                    // remove any care type items that removed in the UI
                    var careTypeIds = new List<int>();
                    careTypeIds.AddRange( cblCareTypes.SelectedValuesAsInt );
                    var uiCareTypeGuids = ItemsState.Where( i => careTypeIds.Contains( i.CareTypeId ) ).Select( a => a.Guid );
                    foreach ( var careTypeItem in careItem.CareTypeItems.Where( a => !uiCareTypeGuids.Contains( a.Guid ) ).ToList() )
                    {
                        // Make sure user is authorized to remove careType (they may not have seen every careType due to security)
                        if ( UserCanEdit || careTypeItem.CareType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            careItem.CareTypeItems.Remove( careTypeItem );
                            careTypeItemService.Delete( careTypeItem );
                        }
                    }

                    // Add or Update care type items from the UI
                    foreach ( var careType in ItemsState.Where( i => careTypeIds.Contains( i.CareTypeId ) ) )
                    {
                        var careTypeItem = careItem.CareTypeItems.Where( a => a.Guid == careType.Guid ).FirstOrDefault();
                        if ( careTypeItem == null )
                        {
                            careTypeItem = new CareTypeItem();
                            careItem.CareTypeItems.Add( careTypeItem );
                        }
                        careTypeItem.CopyPropertiesFrom( careType );
                    }

                    var validationMessages = new List<string>();

                    if ( !careItem.CareTypeItems.Any() )
                    {
                        validationMessages.Add( "At least one calendar is required." );
                    }

                    if ( validationMessages.Any() )
                    {
                        nbValidation.Text = "Please correct the following:<ul><li>" + validationMessages.AsDelimited( "</li><li>" ) + "</li></ul>";
                        nbValidation.Visible = true;
                        return;
                    }

                    careItem.LoadAttributes();
                    Rock.Attribute.Helper.GetEditValues( phAttributes, careItem );

                    if ( !Page.IsValid )
                    {
                        return;
                    }

                    // using WrapTransaction because there are three Saves
                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();

                        foreach ( CareTypeItem careTypeItem in careItem.CareTypeItems )
                        {
                            careTypeItem.LoadAttributes();
                            Rock.Attribute.Helper.GetEditValues( phAttributes, careTypeItem );
                            careTypeItem.SaveAttributeValues();
                        }

                        if ( careItem != null && careItem.Attributes != null && careItem.AttributeValues != null && careItem.Attributes.Any() && careItem.AttributeValues.Any() )
                        {
                            var attributeValueService = new AttributeValueService( rockContext );

                            var attributeIds = careItem.Attributes.Select( y => y.Value.Id ).ToList();
                            var valueQuery = attributeValueService.Queryable().Where( x => attributeIds.Contains( x.AttributeId ) && x.EntityId == careItem.Id );
                            bool changesMade = false;

                            var attributeValues = valueQuery.ToDictionary( x => x.AttributeKey );
                            foreach ( var attribute in careItem.Attributes.Values.Where( a => a.EntityTypeQualifierColumn.IsNullOrWhiteSpace() ) )
                            {
                                if ( careItem.AttributeValues.ContainsKey( attribute.Key ) )
                                {
                                    if ( attributeValues.ContainsKey( attribute.Key ) )
                                    {
                                        if ( attributeValues[attribute.Key].Value != careItem.AttributeValues[attribute.Key].Value )
                                        {
                                            attributeValues[attribute.Key].Value = careItem.AttributeValues[attribute.Key].Value;
                                            changesMade = true;
                                        }
                                    }
                                    else
                                    {
                                        // only save a new AttributeValue if the value has a nonempty value
                                        var value = careItem.AttributeValues[attribute.Key].Value;
                                        if ( value.IsNotNullOrWhiteSpace() )
                                        {
                                            var attributeValue = new AttributeValue();
                                            attributeValue.AttributeId = attribute.Id;
                                            attributeValue.EntityId = careItem.Id;
                                            attributeValue.Value = value;
                                            attributeValueService.Add( attributeValue );
                                            changesMade = true;
                                        }
                                    }
                                }
                            }

                            if ( changesMade )
                            {
                                rockContext.SaveChanges();
                            }
                        }
                    } );

                    var qryParams = new Dictionary<string, string>();
                    qryParams["CareItemId"] = careItem.Id.ToString();
                    qryParams["CareTypeId"] = _careTypeId.ToString();

                    NavigateToPage( RockPage.Guid, qryParams );
                }
            }
        }

        #endregion

        #region CareContact Events

        /// <summary>
        /// Handles the Click event of the btnAddCareContact control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddCareContact_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var careItemService = new CareItemService( rockContext );
                var careContactService = new CareContactService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                var careItem = careItemService.Get( hfCareItemId.ValueAsInt() );
                if ( careItem != null )
                {
                    int? personAliasId = ppContactor.PersonAliasId;
                    if ( personAliasId.HasValue )
                    {

                        CareContact careContact = null;
                        int? id = hfAddCareContactId.Value.AsIntegerOrNull();
                        if ( id.HasValue )
                        {
                            careContact = careContactService.Get( id.Value );
                        }

                        if ( careContact == null )
                        {
                            careContact = new CareContact();
                            careContact.CareItemId = careItem.Id;
                            careContactService.Add( careContact );
                        }

                        careContact.ContactDateTime = dtpVisitDate.SelectedDateTime ?? RockDateTime.Now;
                        careContact.ContactorPersonAliasId = personAliasId.Value;
                        careContact.Description = tbNote.Text;

                        careContact.LoadAttributes();
                        Rock.Attribute.Helper.GetEditValues( phContactAttributes, careContact );

                        rockContext.SaveChanges();
                        careContact.SaveAttributeValues();

                        BindCareContactsGrid( careItem, rockContext );
                        HideDialog();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gCareContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCareContacts_GridRebind( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var careItemService = new CareItemService( rockContext );
                var careItem = careItemService.Get( hfCareItemId.ValueAsInt() );
                if ( careItem != null )
                {
                    BindCareContactsGrid( careItem, rockContext );
                }
            }
        }

        /// <summary>
        /// Handles the Add event of the gCareContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCareContacts_Add( object sender, EventArgs e )
        {
            ShowActivityDialog( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gCareContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCareContacts_Edit( object sender, RowEventArgs e )
        {
            // only allow editing if current user created the activity, and not a system activity
            var contactId = e.RowKeyValue.ToString().AsInteger();
            using ( var rockContext = new RockContext() )
            {
                var careItemService = new CareItemService( rockContext );
                var careItem = careItemService.Get( hfCareItemId.ValueAsInt() );
                bool editAllowed = UserCanEdit || careItem.IsAuthorized( Authorization.EDIT, CurrentPerson );

                var contactService = new CareContactService( rockContext );
                var contact = contactService.Get( contactId );
                if ( contact != null &&
                    ( editAllowed || contact.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) || contact.ContactorPersonAliasId.Equals( CurrentPersonAliasId ) ) )
                {
                    ShowActivityDialog( contactId );
                }
            }

        }

        /// <summary>
        /// Handles the RowDataBound event of the gCareContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gCareContacts_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                bool canEdit = e.Row.DataItem.GetPropertyValue( "CanEdit" ) as bool? ?? false;
                if ( !canEdit )
                {
                    if ( _editField != null && _editField.Visible )
                    {
                        LinkButton editButton = null;
                        HtmlGenericControl buttonIcon = null;

                        if ( !_editFieldColumnIndex.HasValue )
                        {
                            _editFieldColumnIndex = gCareContacts.GetColumnIndex( gCareContacts.Columns.OfType<EditField>().First() );
                        }

                        if ( _editFieldColumnIndex.HasValue && _editFieldColumnIndex > -1 )
                        {
                            editButton = e.Row.Cells[_editFieldColumnIndex.Value].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                        }

                        if ( editButton != null )
                        {
                            buttonIcon = editButton.ControlsOfTypeRecursive<HtmlGenericControl>().FirstOrDefault();
                        }

                        editButton.Visible = false;
                    }

                    if ( _deleteField != null && _deleteField.Visible )
                    {
                        LinkButton deleteButton = null;
                        HtmlGenericControl buttonIcon = null;

                        if ( !_deleteFieldColumnIndex.HasValue )
                        {
                            _deleteFieldColumnIndex = gCareContacts.GetColumnIndex( gCareContacts.Columns.OfType<DeleteField>().First() );
                        }

                        if ( _deleteFieldColumnIndex.HasValue && _deleteFieldColumnIndex > -1 )
                        {
                            deleteButton = e.Row.Cells[_deleteFieldColumnIndex.Value].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                        }

                        if ( deleteButton != null )
                        {
                            buttonIcon = deleteButton.ControlsOfTypeRecursive<HtmlGenericControl>().FirstOrDefault();
                        }

                        deleteButton.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gCareContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCareContacts_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var careItemService = new CareItemService( rockContext );
                var careItem = careItemService.Get( hfCareItemId.ValueAsInt() );
                bool editAllowed = UserCanEdit || careItem.IsAuthorized( Authorization.EDIT, CurrentPerson );

                // only allow deleting if current user created the activity, and not a system activity
                var activityId = e.RowKeyValue.ToString().AsInteger();
                var careContactService = new CareContactService( rockContext );
                var activity = careContactService.Get( activityId );
                if ( activity != null &&
                    ( editAllowed || activity.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) || activity.ContactorPersonAliasId.Equals( CurrentPersonAliasId ) ) )
                {
                    careContactService.Delete( activity );
                    rockContext.SaveChanges();
                }

                BindCareContactsGrid( careItem, rockContext );
            }
        }

        /// <summary>
        /// Binds the connection request activities grid.
        /// </summary>
        private void BindCareContactsGrid( CareItem careItem, RockContext rockContext )
        {
            if ( careItem != null && careItem.PersonAlias != null )
            {
                BindAttributes();
                AddDynamicControls();

                bool editAllowed = UserCanEdit || careItem.IsAuthorized( Authorization.EDIT, CurrentPerson );

                var careContactService = new CareContactService( rockContext );
                var qry = careContactService
                    .Queryable( "ContactorPersonAlias.Person" )
                    .AsNoTracking()
                    .Where( a =>
                        a.CareItem != null &&
                        a.CareItem.PersonAlias != null &&
                        a.CareItem.PersonAlias.PersonId == careItem.PersonAlias.PersonId );


                qry = qry.Where( a => a.CareItemId == careItem.Id );

                gCareContacts.ObjectList = new Dictionary<string, object>();
                qry.ToList().ForEach( m => gCareContacts.ObjectList.Add( m.Id.ToString(), m ) );
                gCareContacts.EntityTypeId = new CareContact().TypeId;

                gCareContacts.DataSource = qry.ToList()
                    .Select( a => new
                    {
                        a.Id,
                        a.Guid,
                        VisitDate = a.ContactDateTime,
                        Date = a.ContactDateTime.ToShortDateString(),
                        Contactor = a.ContactorPersonAlias != null && a.ContactorPersonAlias.Person != null ? a.ContactorPersonAlias.Person.FullName : "",
                        Description = a.Description,
                        CanEdit = editAllowed || ( a.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) || a.ContactorPersonAliasId.Equals( CurrentPersonAliasId ) )
                    } )
                    .OrderByDescending( a => a.VisitDate )
                    .ToList();
                gCareContacts.DataBind();


            }
        }

        #endregion

        #endregion

        #region Internal Methods     

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="careItemId">The connection request identifier.</param>
        /// <param name="careTypeId">The careType id.</param>
        public void ShowDetail( int careItemId )
        {
            bool editAllowed = UserCanEdit;

            // autoexpand the person picker if this is an add
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "StartupScript", @"Sys.Application.add_load(function () {

                // if the person picker is empty then open it for quick entry
                var personPicker = $('.js-authorizedperson');
                var currentPerson = personPicker.find('.picker-selectedperson').html();
                if (currentPerson != null && currentPerson.length == 0) {
                    $(personPicker).find('a.picker-label').trigger('click');
                }

            });", true );

            var rockContext = new RockContext();
            var careTypeService = new CareTypeService( rockContext );
            var careItemService = new CareItemService( rockContext );

            CareType careType = null;
            CareItem careItem = null;
            careType = careTypeService.Get( _careTypeId ?? 0 );

            if ( careItemId > 0 )
            {
                careItem = new CareItemService( rockContext ).Get( careItemId );
            }

            if ( careItem == null )
            {
                careItem = new CareItem();
                careItem.IsActive = true;
                careItem.CareTypeItems = new List<CareTypeItem>();
                var careTypeItem = new CareTypeItem { CareTypeId = ( _careTypeId ?? 0 ) };
                careItem.CareTypeItems.Add( careTypeItem );
            }
            else
            {
                // Set the person
                Person = careItem.PersonAlias.Person;
            }

            if ( careItem != null )
            {
                hfCareItemId.Value = careItem.Id.ToString();

                pnlReadDetails.Visible = true;

                if ( careItem.PersonAlias != null && careItem.PersonAlias.Person != null )
                {
                    lTitle.Text = careItem.PersonAlias.Person.FullName.FormatAsHtmlTitle() + " - " + careType.Name;
                }
                else
                {
                    lTitle.Text = String.Format( "New {0}Care Item", careType.Name + " " );
                }

                // Only users that have Edit rights to block, or edit rights to the opportunity
                if ( !editAllowed )
                {
                    editAllowed = careItem.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }

                lbEdit.Visible = editAllowed;
                gCareContacts.IsDeleteEnabled = editAllowed;
                gCareContacts.Actions.ShowAdd = editAllowed;

                if ( !editAllowed )
                {
                    // User is not authorized
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( CareItem.FriendlyTypeName );
                    ShowReadonlyDetails( careItem );
                }
                else
                {
                    nbEditModeMessage.Text = string.Empty;
                    if ( careItem.Id > 0 )
                    {
                        ShowReadonlyDetails( careItem );
                    }
                    else
                    {
                        ShowEditDetails( careItem, rockContext );
                    }
                }
            }
        }

        private void BuildAttributeControls( CareItem careItem, bool readOnly )
        {
            careItem.LoadAttributes();

            phAttributes.Controls.Clear();
            phAttributes.Visible = false;

            phAttributesReadOnly.Controls.Clear();
            phAttributesReadOnly.Visible = false;

            var editableAttributes = !readOnly ? careItem.Attributes.Where( a => a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList() : new List<string>();
            var viewableAttributes = careItem.Attributes.Where( a => !editableAttributes.Contains( a.Key ) && a.Value.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Key ).ToList();

            if ( editableAttributes.Any() )
            {
                phAttributes.Visible = true;
                var excludeKeys = careItem.Attributes.Where( a => !editableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
                Rock.Attribute.Helper.AddEditControls( careItem, phAttributes, true, string.Empty, excludeKeys );
            }

            if ( viewableAttributes.Any() )
            {
                phAttributesReadOnly.Visible = true;
                var excludeKeys = careItem.Attributes.Where( a => !viewableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
                Rock.Attribute.Helper.AddDisplayControls( careItem, phAttributesReadOnly, excludeKeys, false, false );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="careItem">The connection request.</param>
        private void ShowReadonlyDetails( CareItem careItem )
        {
            pdAuditDetails.SetEntity( careItem, ResolveRockUrl( "~" ) );
            lContactInfo.Text = string.Empty;
            hlInactive.Visible = careItem.IsActive == false;

            Person person = null;
            if ( careItem != null && careItem.PersonAlias != null )
            {
                person = careItem.PersonAlias.Person;
            }

            if ( person != null && ( person.PhoneNumbers.Any() || !String.IsNullOrWhiteSpace( person.Email ) ) )
            {
                List<String> contactList = new List<string>();

                foreach ( PhoneNumber phoneNumber in person.PhoneNumbers )
                {
                    contactList.Add( String.Format( "{0} <font color='#808080'>{1}</font>", phoneNumber.NumberFormatted, phoneNumber.NumberTypeValue ) );
                }

                string emailTag = person.GetEmailTag( ResolveRockUrl( "/" ) );
                if ( !string.IsNullOrWhiteSpace( emailTag ) )
                {
                    contactList.Add( emailTag );
                }

                lContactInfo.Text = contactList.AsDelimited( "</br>" );
            }
            else
            {
                lContactInfo.Text = "No contact Info";
            }

            if ( person != null && !string.IsNullOrWhiteSpace( GetAttributeValue( "PersonProfilePage" ) ) )
            {
                lbProfilePage.Visible = true;

                Dictionary<string, string> queryParms = new Dictionary<string, string>();
                queryParms.Add( "PersonId", person.Id.ToString() );
                lbProfilePage.PostBackUrl = LinkedPageUrl( "PersonProfilePage", queryParms );
            }
            else
            {
                lbProfilePage.Visible = false;
            }

            if ( person != null )
            {
                string imgTag = Rock.Model.Person.GetPersonPhotoImageTag( person, 200, 200, className: "img-thumbnail" );
                if ( person.PhotoId.HasValue )
                {
                    lPortrait.Text = string.Format( "<a href='{0}'>{1}</a>", person.PhotoUrl, imgTag );
                }
                else
                {
                    lPortrait.Text = imgTag;
                }
            }
            else
            {
                lPortrait.Text = string.Empty;
            }

            lDescription.Text = careItem != null && careItem.Description != null ? careItem.Description.ConvertMarkdownToHtml() : string.Empty;
            lContactDate.Text = careItem != null ? careItem.ContactDateTime.ToShortDateString() : string.Empty;

            if ( careItem != null &&
                careItem.ContactorPersonAlias != null &&
                careItem.ContactorPersonAlias.Person != null )
            {
                lContactor.Text = careItem.ContactorPersonAlias.Person.FullName;
            }
            else
            {
                lContactor.Text = "No contactor assigned";
            }

            var careTypes = careItem.CareTypeItems
                .Select( c => c.CareType.Name ).ToList();
            if ( careTypes.Any() )
            {
                lCareType.Visible = true;
                lCareType.Text = careTypes.AsDelimited( ", " );
            }
            else
            {
                lCareType.Visible = false;
            }

            phAttributesReadOnly.Controls.Clear();

            var areUniversalAttributesLoaded = false;
            foreach ( var careTypeItem in careItem.CareTypeItems )
            {
                careTypeItem.LoadAttributes();
                if ( careTypeItem.Attributes.Count > 0 )
                {
                    if ( areUniversalAttributesLoaded == false )
                    {
                        if ( careTypeItem.Attributes.Where( a => a.Value.EntityTypeQualifierColumn.IsNullOrWhiteSpace() ).Count() > 0 )
                        {
                            phAttributesReadOnly.Controls.Add( new LiteralControl( "<h4>Care Attributes</h4>" ) );

                            foreach ( var attr in careTypeItem.Attributes.Where( a => a.Value.EntityTypeQualifierColumn.IsNullOrWhiteSpace() ) )
                            {
                                if ( attr.Value.EntityTypeQualifierColumn.IsNullOrWhiteSpace() )
                                {
                                    string value = careTypeItem.GetAttributeValue( attr.Key );
                                    if ( !string.IsNullOrWhiteSpace( value ) )
                                    {
                                        var rl = new RockLiteral();
                                        rl.ID = String.Format( "attr_{0}_{1}", attr.Key, attr.Value.Id );
                                        rl.Label = attr.Value.Name;
                                        rl.Text = attr.Value.FieldType.Field.FormatValueAsHtml( null, attr.Value.EntityTypeId, careTypeItem.Id, value, attr.Value.QualifierValues, false );
                                        phAttributesReadOnly.Controls.Add( rl );
                                    }
                                }
                            }
                            areUniversalAttributesLoaded = true;
                        }
                    }

                    if ( careTypeItem.Attributes.Where( a => a.Value.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() ).Count() > 0 )
                    {
                        phAttributesReadOnly.Controls.Add( new LiteralControl( String.Format( "<h4>{0}</h4>", careTypeItem.CareType.Name ) ) );
                        foreach ( var attr in careTypeItem.Attributes.Where( a => a.Value.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() ) )
                        {
                            string value = careTypeItem.GetAttributeValue( attr.Key );
                            if ( !string.IsNullOrWhiteSpace( value ) )
                            {
                                var rl = new RockLiteral();
                                rl.ID = String.Format( "attr_{0}_{1}", attr.Key, attr.Value.Id );
                                rl.Label = attr.Value.Name;
                                rl.Text = attr.Value.FieldType.Field.FormatValueAsHtml( null, attr.Value.EntityTypeId, careTypeItem.Id, value, attr.Value.QualifierValues, false );
                                phAttributesReadOnly.Controls.Add( rl );
                            }
                        }
                    }

                }
            }

            BindCareContactsGrid( careItem, new RockContext() );
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="_careItem">The _connection request.</param>
        private void ShowEditDetails( CareItem careItem, RockContext rockContext )
        {
            pnlReadDetails.Visible = false;
            pnlEditDetails.Visible = true;
            wpCareContacts.Visible = false;

            // Person
            if ( careItem.PersonAlias != null )
            {
                ppPerson.SetValue( careItem.PersonAlias.Person );
                ppPerson.Enabled = false;
            }
            else
            {
                ppPerson.Enabled = true;
            }

            // Contactor
            if ( careItem.ContactorPersonAlias != null )
            {
                ppContactorEdit.SetValue( careItem.ContactorPersonAlias.Person );
            }

            // Contact Date

            dtpContactDate.Visible = true;
            if ( careItem.ContactDateTime != null )
            {
                dtpContactDate.SelectedDateTime = careItem.ContactDateTime;
            }
            else
            {
                dtpContactDate.Visible = false;
            }

            // Description
            tbDescription.Text = careItem.Description;

            if ( careItem.CareTypeItems != null )
            {
                cblCareTypes.SetValues( careItem.CareTypeItems.Select( c => c.CareTypeId ).ToList() );
            }

            cbIsActive.Checked = careItem.IsActive;

            ItemsState = careItem.CareTypeItems.ToList();
            ShowItemAttributes();
            //BuildAttributeControls( careItem, false );
        }


        /// <summary>
        /// Shows the activity dialog.
        /// </summary>
        /// <param name="contactGuid">The activity unique identifier.</param>
        private void ShowActivityDialog( int contactId )
        {
            CareContact contact = null;

            using ( var rockContext = new RockContext() )
            {
                if ( contactId != 0 )
                {
                    contact = new CareContactService( rockContext ).Get( contactId );
                }
                else
                {
                    contact = new CareContact();
                }

                if ( contact != null && contact.ContactorPersonAlias != null && contact.ContactorPersonAlias.Person != null )
                {
                    ppContactor.SetValue( contact.ContactorPersonAlias.Person );
                }

                dtpVisitDate.SelectedDateTime = contact != null ? contact.ContactDateTime : RockDateTime.Now;

                tbNote.Text = contact != null ? contact.Description : string.Empty;

                contact.LoadAttributes();

                phContactAttributes.Controls.Clear();
                phContactAttributes.Visible = false;

                var editableAttributes = contact.Attributes.Where( a => a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList();

                if ( editableAttributes.Any() )
                {
                    phContactAttributes.Visible = true;
                    var excludeKeys = contact.Attributes.Where( a => !editableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
                    Rock.Attribute.Helper.AddEditControls( contact, phContactAttributes, true, string.Empty, excludeKeys );
                }

                hfAddCareContactId.Value = contactId.ToString();
                if ( contactId == 0 )
                {
                    dlgCareContacts.Title = "Add Contact";
                    dlgCareContacts.SaveButtonText = "Add";
                }
                else
                {
                    dlgCareContacts.Title = "Edit Contact";
                    dlgCareContacts.SaveButtonText = "Save";
                }

                ShowDialog( "CareContacts", true );
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
                case "CARECONTACTS":
                    dlgCareContacts.Show();
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
                case "CARECONTACTS":
                    dlgCareContacts.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowErrorMessage( string title, string message )
        {
            nbErrorMessage.Title = title;
            nbErrorMessage.Text = string.Format( "<p>{0}</p>", message );
            nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbErrorMessage.Visible = true;
        }

        /// <summary>
        /// Shows the item attributes.
        /// </summary>
        private void ShowItemAttributes()
        {
            var careItemId = hfCareItemId.ValueAsInt();
            var areUniversalAttributesLoaded = false;

            var careTypeList = new List<int>();
            careTypeList.AddRange( cblCareTypes.SelectedValuesAsInt );

            phAttributes.Controls.Clear();
            phContactAttributes.Controls.Clear();

            using ( var rockContext = new RockContext() )
            {
                var careTypeService = new CareTypeService( rockContext );

                foreach ( int careTypeId in careTypeList.Distinct() )
                {
                    CareTypeItem careTypeItem = ItemsState.FirstOrDefault( i => i.CareTypeId == careTypeId );
                    if ( careTypeItem == null )
                    {
                        careTypeItem = new CareTypeItem();
                        careTypeItem.CareTypeId = careTypeId;
                        ItemsState.Add( careTypeItem );
                    }

                    careTypeItem.LoadAttributes();

                    if ( careTypeItem.Attributes.Count > 0 )
                    {
                        if ( areUniversalAttributesLoaded == false )
                        {
                            if ( careTypeItem.Attributes.Where( a => a.Value.EntityTypeQualifierColumn.IsNullOrWhiteSpace() ).Count() > 0 )
                            {
                                phAttributes.Controls.Add( new LiteralControl( "<h3>Care Attributes</h3>" ) );
                                PlaceHolder phcalAttributes = new PlaceHolder();
                                // Rock.Attribute.Helper.AddEditControls( careItem, phAttributes, true, BlockValidationGroup );
                                if ( careTypeItem != null && careTypeItem.Attributes != null )
                                {
                                    foreach ( var attributeCategory in Rock.Attribute.Helper.GetAttributeCategories( careTypeItem, false, false, false ) )
                                    {
                                        if ( attributeCategory.Attributes.Where( a => a.IsActive ).Where( a => a.EntityTypeQualifierColumn.IsNullOrWhiteSpace() ).Select( a => a.Key ).Count() > 0 )
                                        {
                                            Rock.Attribute.Helper.AddEditControls(
                                                attributeCategory.Category != null ? attributeCategory.Category.Name : string.Empty,
                                                attributeCategory.Attributes.Where( a => a.IsActive && a.EntityTypeQualifierColumn.IsNullOrWhiteSpace() ).Select( a => a.Key ).ToList(),
                                                careTypeItem, phAttributes, BlockValidationGroup, true, new List<string>(), null );
                                            areUniversalAttributesLoaded = true;
                                        }
                                    }
                                }
                            }
                        }

                        if ( careTypeItem.Attributes.Where( a => a.Value.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() ).Count() > 0 )
                        {
                            phAttributes.Controls.Add( new LiteralControl( String.Format( "<h3>{0}</h3>", careTypeService.Get( careTypeId ).Name ) ) );
                            PlaceHolder phcalAttributes = new PlaceHolder();
                            // Rock.Attribute.Helper.AddEditControls( careItem, phAttributes, true, BlockValidationGroup );
                            if ( careTypeItem != null && careTypeItem.Attributes != null )
                            {
                                foreach ( var attributeCategory in Rock.Attribute.Helper.GetAttributeCategories( careTypeItem, false, false, false ) )
                                {
                                    if ( attributeCategory.Attributes.Where( a => a.IsActive ).Where( a => a.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() ).Select( a => a.Key ).Count() > 0 )
                                    {
                                        Rock.Attribute.Helper.AddEditControls(
                                            attributeCategory.Category != null ? attributeCategory.Category.Name : string.Empty,
                                            attributeCategory.Attributes.Where( a => a.IsActive && a.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() ).Select( a => a.Key ).ToList(),
                                            careTypeItem, phAttributes, BlockValidationGroup, true, new List<string>(), null );
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        private void ShowContactAttributes()
        {
            var contact = new CareContact();
            contact.LoadAttributes();

            phContactAttributes.Controls.Clear();
            phContactAttributes.Visible = false;

            var editableAttributes = contact.Attributes.Where( a => a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList();

            if ( editableAttributes.Any() )
            {
                phContactAttributes.Visible = true;
                var excludeKeys = contact.Attributes.Where( a => !editableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
                Rock.Attribute.Helper.AddEditControls( contact, phContactAttributes, true, string.Empty, excludeKeys );
            }
        }


        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear dynamic controls so we can re-add them
            RemoveAttributeColumns();

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    bool columnExists = gCareContacts.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;
                        boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;

                        gCareContacts.Columns.Add( boundField );
                    }
                }
            }


            // Add delete column
            _editField = new EditField();
            _editField.Click += gCareContacts_Edit;
            gCareContacts.Columns.Add( _editField );

            // Add delete column
            _deleteField = new DeleteField();
            _deleteField.Click += gCareContacts_Delete;
            gCareContacts.Columns.Add( _deleteField );
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            AvailableAttributes = new List<AttributeCache>();
            var rockContext = new RockContext();

            // Parse the attribute filters 
            int entityTypeId = new CareContact().TypeId;

            foreach ( var attribute in new AttributeService( rockContext ).GetByEntityTypeQualifier( entityTypeId, "", "", true )
            .Where( a => a.IsGridColumn )
            .OrderByDescending( a => a.EntityTypeQualifierColumn )
            .ThenBy( a => a.Order )
            .ThenBy( a => a.Name ).ToAttributeCacheList() )
            {
                if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    AvailableAttributes.Add( attribute );
                }
            }
        }

        private void RemoveAttributeColumns()
        {
            // Remove added button columns
            DataControlField deleteButtonColumn = gCareContacts.Columns.OfType<DeleteField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( deleteButtonColumn != null )
            {
                gCareContacts.Columns.Remove( deleteButtonColumn );
            }

            DataControlField editButtonColumn = gCareContacts.Columns.OfType<EditField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( editButtonColumn != null )
            {
                gCareContacts.Columns.Remove( editButtonColumn );
            }

            // Remove attribute columns
            foreach ( var column in gCareContacts.Columns.OfType<AttributeField>().ToList() )
            {
                gCareContacts.Columns.Remove( column );
            }
        }

        #endregion

    }
}
