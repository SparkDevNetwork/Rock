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
using System.Collections.Generic;
using System.Threading;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.SignalR;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls.Communication;
using Rock.Web.UI.Controls;
using Rock.Transactions;
using System.Web;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// User control for creating a new communication.  This block should be used on same page as the CommunicationDetail block and only visible when editing a new or transient communication
    /// </summary>
    [DisplayName( "Bulk Update" )]
    [Category( "CRM" )]
    [Description( "Used for updating information about several individuals at once." )]

    [SecurityAction( "EditConnectionStatus", "The roles and/or users that can edit the connection status for the selected persons." )]
    [SecurityAction( "EditRecordStatus", "The roles and/or users that can edit the record status for the selected persons." )]

    [AttributeCategoryField( "Attribute Categories", "The person attribute categories to display and allow bulk updating", true, "Rock.Model.Person", false, "", "", 0 )]
    [IntegerField( "Display Count", "The initial number of individuals to display prior to expanding list", false, 0, "", 1 )]
    [WorkflowTypeField( "Workflow Types", "The workflows to make available for bulk updating.", true, false, "", "", 2 )]
    [IntegerField( "Task Count", "The number of concurrent tasks to use when performing updates. If left blank then it will be determined automatically.", false, 0, "", 3 )]
    public partial class BulkUpdate : RockBlock
    {
        #region Fields

        DateTime _gradeTransitionDate = new DateTime( RockDateTime.Today.Year, 6, 1 );
        bool _canEditConnectionStatus = false;
        bool _canEditRecordStatus = true;

        #endregion

        #region Properties

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        private List<Individual> Individuals { get; set; }
        private bool ShowAllIndividuals { get; set; }
        private int? GroupId { get; set; }
        private List<string> SelectedFields { get; set; }
        private List<Guid> AttributeCategories { get; set; }

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );

            var personEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

            dvpTitle.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ).Id;
            dvpConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ).Id;
            dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ).Id;
            dvpSuffix.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ).Id;
            dvpRecordStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ).Id;
            dvpInactiveReason.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ).Id;
            dvpReviewReason.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_REVIEW_REASON ) ).Id;

            _canEditConnectionStatus = UserCanAdministrate || IsUserAuthorized( "EditConnectionStatus" );
            dvpConnectionStatus.Visible = _canEditConnectionStatus;

            _canEditRecordStatus = UserCanAdministrate || IsUserAuthorized( "EditRecordStatus" );
            dvpRecordStatus.Visible = _canEditRecordStatus;

            rlbWorkFlowType.Items.Clear();
            var guidList = GetAttributeValue( "WorkflowTypes" ).SplitDelimitedValues().AsGuidList();
            using ( var rockContext = new RockContext() )
            {
                var workflowTypeService = new WorkflowTypeService( rockContext );
                foreach ( var workflowType in new WorkflowTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( t => guidList.Contains( t.Guid ) && t.IsActive == true )
                    .ToList() )
                {
                    if ( workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ListItem item = new ListItem( workflowType.Name, workflowType.Id.ToString() );
                        rlbWorkFlowType.Items.Add( item );
                    }
                }
            }

            if ( rlbWorkFlowType.Items.Count <= 0 )
            {
                pwWorkFlows.Visible = false;
            }

            ddlTagList.Items.Clear();
            ddlTagList.DataTextField = "Name";
            ddlTagList.DataValueField = "Id";
            var currentPersonAliasIds = CurrentPerson.Aliases.Select( a => a.Id ).ToList();

            var tagList = new TagService( new RockContext() ).Queryable()
                                            .Where( t =>
                                                        t.EntityTypeId == personEntityTypeId
                                                        && ( t.OwnerPersonAliasId == null || currentPersonAliasIds.Contains( t.OwnerPersonAliasId.Value ) ) )
                                            .Select( t => new
                                            {
                                                Id = t.Id,
                                                Type = t.OwnerPersonAliasId == null ? "Organization Tags" : "Personal Tags",
                                                Name = t.Name
                                            } )
                                            .OrderByDescending( t => t.Type )
                                            .ThenBy( t => t.Name )
                                            .ToList();
            foreach ( var tag in tagList )
            {
                ListItem item = new ListItem( tag.Name, tag.Id.ToString() );
                item.Attributes["OptionGroup"] = tag.Type;
                ddlTagList.Items.Add( item );
            }
            ddlTagList.Items.Insert( 0, "" );

            ScriptManager.RegisterStartupScript( ddlGradePicker, ddlGradePicker.GetType(), "grade-selection-" + BlockId.ToString(), ddlGradePicker.GetJavascriptForYearPicker( ypGraduation ), true );

            ddlNoteType.Items.Clear();
            var noteTypes = NoteTypeCache.GetByEntity( personEntityTypeId, string.Empty, string.Empty, true );
            foreach ( var noteType in noteTypes )
            {
                if ( noteType.UserSelectable && noteType.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson ) )
                {
                    ddlNoteType.Items.Add( new ListItem( noteType.Name, noteType.Id.ToString() ) );
                }
            }
            pwNote.Visible = ddlNoteType.Items.Count > 0;

            string script = @"
    $('a.remove-all-individuals').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to remove all of the individuals from this update?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( lbRemoveAllIndividuals, lbRemoveAllIndividuals.GetType(), "confirm-remove-all-" + BlockId.ToString(), script, true );

            // This will cause this script to be injected upon each partial-postback because the script is
            // needed due to the fact that the controls are dynamically changing (added/removed) during each
            // partial postback.  Don't try to 'fix' this unless you're going to re-engineer this section. :)
            script = string.Format( @"

    // Add the 'bulk-item-selected' class to form-group of any item selected after postback
    $( 'label.control-label' ).has( 'span.js-select-item > i.fa-check-circle-o').each( function() {{
        $(this).closest('.form-group').addClass('bulk-item-selected');
    }});

    // Handle the click event for any label that contains a 'js-select-span' span
    $( 'label.control-label' ).has( 'span.js-select-item').click( function() {{

        var formGroup = $(this).closest('.form-group');
        var selectIcon = formGroup.find('span.js-select-item').children('i');

        // Toggle the selection of the form group        
        formGroup.toggleClass('bulk-item-selected');
        var enabled = formGroup.hasClass('bulk-item-selected');
            
        // Set the selection icon to show selected
        selectIcon.toggleClass('fa-check-circle-o', enabled);
        selectIcon.toggleClass('fa-circle-o', !enabled);

        // Checkboxes needs special handling
        var checkboxes = formGroup.find(':checkbox');
        if ( checkboxes.length ) {{
            $(checkboxes).each(function() {{
                if (this.nodeName === 'INPUT' ) {{
                    $(this).toggleClass('aspNetDisabled', !enabled);
                    $(this).prop('disabled', !enabled);
                    $(this).closest('label').toggleClass('text-muted', !enabled);
                    $(this).closest('.form-group').toggleClass('bulk-item-selected', enabled);
                }}
            }});
        }}

        // Enable/Disable the controls
        formGroup.find('.form-control').each( function() {{

            $(this).toggleClass('aspNetDisabled', !enabled);
            $(this).prop('disabled', !enabled);

            // Grade/Graduation needs special handling
            if ( $(this).prop('id') == '{1}' ) {{
                $('#{2}').toggleClass('aspNetDisabled', !enabled);
                $('#{2}').prop('disabled', !enabled);
                $('#{2}').closest('.form-group').toggleClass('bulk-item-selected', enabled)
            }}

        }});
        
        // Update the hidden field with the client id of each selected control, (if client id ends with '_hf' as in the case of multi-select attributes, strip the ending '_hf').
        var newValue = '';
        $('div.bulk-item-selected').each(function( index ) {{
            $(this).find('[id]').each(function() {{
                var re = /_hf$/;
                var ctrlId = $(this).prop('id').replace(re, '');
                newValue += ctrlId + '|';
            }});
        }});
        $('#{0}').val(newValue);            

    }});
", hfSelectedItems.ClientID, ddlGradePicker.ClientID, ypGraduation.ClientID );
            ScriptManager.RegisterStartupScript( hfSelectedItems, hfSelectedItems.GetType(), "select-items-" + BlockId.ToString(), script, true );

            ddlGroupAction.SelectedValue = "Add";
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>();

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPanel );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            Individuals = ViewState["Individuals"] as List<Individual>;
            if ( Individuals == null )
            {
                Individuals = new List<Individual>();
            }

            ShowAllIndividuals = ViewState["ShowAllIndividuals"] as bool? ?? false;
            GroupId = ViewState["GroupId"] as int?;

            string selectedItemsValue = Request.Form[hfSelectedItems.UniqueID];
            if ( !string.IsNullOrWhiteSpace( selectedItemsValue ) )
            {
                SelectedFields = selectedItemsValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            }
            else
            {
                SelectedFields = new List<string>();
            }

            AttributeCategories = ViewState["AttributeCategories"] as List<Guid>;
            if ( AttributeCategories == null )
            {
                AttributeCategories = new List<Guid>();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var rockContext = new RockContext();

            if ( !Page.IsPostBack )
            {
                AttributeCategories = GetAttributeValue( "AttributeCategories" ).SplitDelimitedValues().AsGuidList();
                cpCampus.Campuses = CampusCache.All();
                Individuals = new List<Individual>();
                SelectedFields = new List<string>();

                int? setId = PageParameter( "Set" ).AsIntegerOrNull();
                if ( setId.HasValue )
                {
                    var selectedPersonsQry = new EntitySetService( rockContext ).GetEntityQuery<Person>( setId.Value );

                    // Get the people selected
                    foreach ( var person in selectedPersonsQry
                        .Select( p => new
                        {
                            p.Id,
                            FullName = p.NickName + " " + p.LastName
                        } ) )
                    {
                        Individuals.Add( new Individual( person.Id, person.FullName ) );
                    }
                }

                SetControlSelection();
                BuildAttributes( rockContext, true );
            }
            else
            {
                SetControlSelection();
                BuildAttributes( rockContext );

                if ( ddlGroupAction.SelectedValue == "Update" )
                {
                    SetControlSelection( ddlGroupRole, "Role" );
                    SetControlSelection( ddlGroupMemberStatus, "Member Status" );
                }

                BuildGroupAttributes( rockContext );
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
            ViewState["Individuals"] = Individuals;
            ViewState["ShowAllIndividuals"] = ShowAllIndividuals;
            ViewState["GroupId"] = GroupId;
            ViewState["AttributeCategories"] = AttributeCategories;
            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( pnlEntry.Visible )
            {
                BindIndividuals();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppAddPerson.PersonId.HasValue )
            {
                if ( !Individuals.Any( r => r.PersonId == ppAddPerson.PersonId.Value ) )
                {
                    var Person = new PersonService( new RockContext() ).Get( ppAddPerson.PersonId.Value );
                    if ( Person != null )
                    {
                        Individuals.Add( new Individual( Person ) );
                        ShowAllIndividuals = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptIndividuals control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs" /> instance containing the event data.</param>
        protected void rptIndividuals_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int personId = int.MinValue;
            if ( int.TryParse( e.CommandArgument.ToString(), out personId ) )
            {
                Individuals = Individuals.Where( r => r.PersonId != personId ).ToList();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbShowAllIndividuals control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbShowAllIndividuals_Click( object sender, EventArgs e )
        {
            ShowAllIndividuals = true;
        }

        /// <summary>
        /// Handles the Click event of the lbRemoveAllIndividuals control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbRemoveAllIndividuals_Click( object sender, EventArgs e )
        {
            Individuals = new List<Individual>();
        }

        /// <summary>
        /// Handles the ServerValidate event of the valIndividuals control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs" /> instance containing the event data.</param>
        protected void valIndividuals_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = Individuals.Any();
        }

        /// <summary>
        /// Handles the ServerValidate event of the cvSelection control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvSelection_ServerValidate( object source, ServerValidateEventArgs args )
        {
            int? groupId = gpGroup.SelectedValue.AsIntegerOrNull();
            int? tagId = ddlTagList.SelectedValue.AsIntegerOrNull();
            int? workFlowTypeId = rlbWorkFlowType.SelectedValue.AsIntegerOrNull();
            args.IsValid = SelectedFields.Any() || !string.IsNullOrWhiteSpace( tbNote.Text ) || ( groupId.HasValue && groupId > 0 ) || tagId.HasValue || workFlowTypeId.HasValue;
        }

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnComplete_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                #region Individual Details Updates

                int inactiveStatusId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;

                var changes = new List<string>();

                if ( SelectedFields.Contains( dvpTitle.ClientID ) )
                {
                    int? newTitleId = dvpTitle.SelectedValueAsInt();
                    EvaluateChange( changes, "Title", DefinedValueCache.GetName( newTitleId ) );
                }

                if ( SelectedFields.Contains( dvpSuffix.ClientID ) )
                {
                    int? newSuffixId = dvpSuffix.SelectedValueAsInt();
                    EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( newSuffixId ) );
                }

                if ( SelectedFields.Contains( dvpConnectionStatus.ClientID ) && _canEditConnectionStatus )
                {
                    int? newConnectionStatusId = dvpConnectionStatus.SelectedValueAsInt();
                    EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( newConnectionStatusId ) );
                }

                if ( SelectedFields.Contains( dvpRecordStatus.ClientID ) && _canEditRecordStatus )
                {
                    int? newRecordStatusId = dvpRecordStatus.SelectedValueAsInt();
                    EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( newRecordStatusId ) );

                    if ( newRecordStatusId.HasValue && newRecordStatusId.Value == inactiveStatusId )
                    {
                        int? newInactiveReasonId = dvpInactiveReason.SelectedValueAsInt();
                        EvaluateChange( changes, "Inactive Reason", DefinedValueCache.GetName( newInactiveReasonId ) );

                        string newInactiveReasonNote = tbInactiveReasonNote.Text;
                        if ( !string.IsNullOrWhiteSpace( newInactiveReasonNote ) )
                        {
                            EvaluateChange( changes, "Inactive Reason Note", newInactiveReasonNote );
                        }
                    }
                }

                if ( SelectedFields.Contains( ddlGender.ClientID ) )
                {
                    Gender newGender = ddlGender.SelectedValue.ConvertToEnum<Gender>();
                    EvaluateChange( changes, "Gender", newGender );
                }

                if ( SelectedFields.Contains( dvpMaritalStatus.ClientID ) )
                {
                    int? newMaritalStatusId = dvpMaritalStatus.SelectedValueAsInt();
                    EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( newMaritalStatusId ) );
                }

                if ( SelectedFields.Contains( ddlGradePicker.ClientID ) )
                {
                    int? newGraduationYear = null;
                    if ( ypGraduation.SelectedYear.HasValue )
                    {
                        newGraduationYear = ypGraduation.SelectedYear.Value;
                    }
                    EvaluateChange( changes, "Graduation Year", newGraduationYear );
                }

                if ( SelectedFields.Contains( ddlIsEmailActive.ClientID ) )
                {
                    bool? newEmailActive = null;
                    if ( !string.IsNullOrWhiteSpace( ddlIsEmailActive.SelectedValue ) )
                    {
                        newEmailActive = ddlIsEmailActive.SelectedValue == "Active";
                    }
                    EvaluateChange( changes, "Email Is Active", newEmailActive );
                }

                if ( SelectedFields.Contains( ddlCommunicationPreference.ClientID ) )
                {
                    var newCommunicationPreference = ddlCommunicationPreference.SelectedValueAsEnum<CommunicationType>();
                    EvaluateChange( changes, "Communication Preference", newCommunicationPreference );
                }

                if ( SelectedFields.Contains( ddlEmailPreference.ClientID ) )
                {
                    EmailPreference? newEmailPreference = ddlEmailPreference.SelectedValue.ConvertToEnumOrNull<EmailPreference>();
                    EvaluateChange( changes, "Email Preference", newEmailPreference );
                }

                if ( SelectedFields.Contains( tbEmailNote.ClientID ) )
                {
                    string newEmailNote = tbEmailNote.Text;
                    EvaluateChange( changes, "Email Note", newEmailNote );
                }

                if ( SelectedFields.Contains( tbSystemNote.ClientID ) )
                {
                    string newSystemNote = tbSystemNote.Text;
                    EvaluateChange( changes, "System Note", newSystemNote );
                }

                if ( SelectedFields.Contains( dvpReviewReason.ClientID ) )
                {
                    int? newReviewReason = dvpReviewReason.SelectedValueAsInt();
                    EvaluateChange( changes, "Review Reason", DefinedValueCache.GetName( newReviewReason ) );
                }

                if ( SelectedFields.Contains( tbReviewReasonNote.ClientID ) )
                {
                    string newReviewReasonNote = tbReviewReasonNote.Text;
                    EvaluateChange( changes, "Review Reason Note", newReviewReasonNote );
                }

                if ( SelectedFields.Contains( cpCampus.ClientID ) )
                {
                    int? newCampusId = cpCampus.SelectedCampusId;
                    if ( newCampusId.HasValue )
                    {
                        var campus = CampusCache.Get( newCampusId.Value );
                        if ( campus != null )
                        {
                            EvaluateChange( changes, "Campus (for all family members)", campus.Name );
                        }
                    }
                }

                // following
                if ( SelectedFields.Contains( ddlFollow.ClientID ) )
                {
                    bool follow = true;
                    if ( !string.IsNullOrWhiteSpace( ddlFollow.SelectedValue ) )
                    {
                        follow = ddlFollow.SelectedValue == "Add";
                    }

                    if ( follow )
                    {
                        changes.Add( "Add to your Following list." );
                    }
                    else
                    {
                        changes.Add( "Remove from your Following list." );
                    }
                }

                #endregion

                #region Attributes

                var rockContext = new RockContext();

                var selectedCategories = new List<CategoryCache>();
                foreach ( Guid categoryGuid in AttributeCategories )
                {
                    var category = CategoryCache.Get( categoryGuid, rockContext );
                    if ( category != null )
                    {
                        selectedCategories.Add( category );
                    }
                }

                var attributes = new List<AttributeCache>();
                var attributeValues = new Dictionary<int, string>();

                int categoryIndex = 0;
                foreach ( var category in selectedCategories.OrderBy( c => c.Name ) )
                {
                    PanelWidget pw = null;
                    string controlId = "pwAttributes_" + category.Id.ToString();
                    if ( categoryIndex % 2 == 0 )
                    {
                        pw = phAttributesCol1.FindControl( controlId ) as PanelWidget;
                    }
                    else
                    {
                        pw = phAttributesCol2.FindControl( controlId ) as PanelWidget;
                    }
                    categoryIndex++;

                    if ( pw != null )
                    {
                        var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id, false )
                            .OrderBy( a => a.Order ).ThenBy( a => a.Name );
                        foreach ( var attribute in orderedAttributeList )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                var attributeCache = AttributeCache.Get( attribute.Id );

                                Control attributeControl = pw.FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );

                                if ( attributeControl != null && SelectedFields.Contains( attributeControl.ClientID ) )
                                {
                                    string newValue = attributeCache.FieldType.Field.GetEditValue( attributeControl, attributeCache.QualifierValues );
                                    EvaluateChange( changes, attributeCache.Name, attributeCache.FieldType.Field.FormatValue( null, newValue, attributeCache.QualifierValues, false ) );
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Note

                if ( !string.IsNullOrWhiteSpace( tbNote.Text ) && CurrentPerson != null && ddlNoteType.SelectedItem != null )
                {
                    string noteTypeName = ddlNoteType.SelectedItem.Text;
                    changes.Add( string.Format( "Add a <span class='field-name'>{0}{1}{2}</span> of <p><span class='field-value'>{3}</span></p>.",
                        ( cbIsPrivate.Checked ? "Private " : "" ), noteTypeName, ( cbIsAlert.Checked ? " (Alert)" : "" ), tbNote.Text.ConvertCrLfToHtmlBr() ) );
                }

                #endregion

                #region Group

                int? groupId = gpGroup.SelectedValue.AsIntegerOrNull();
                if ( groupId.HasValue && groupId > 0 )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    if ( group != null )
                    {
                        string action = ddlGroupAction.SelectedValue;
                        if ( action == "Remove" )
                        {
                            changes.Add( string.Format( "Remove from <span class='field-name'>{0}</span> group.", group.Name ) );
                        }
                        else if ( action == "Add" )
                        {
                            changes.Add( string.Format( "Add to <span class='field-name'>{0}</span> group.", group.Name ) );
                        }
                        else // Update
                        {
                            if ( SelectedFields.Contains( ddlGroupRole.ClientID ) )
                            {
                                var roleId = ddlGroupRole.SelectedValueAsInt();
                                if ( roleId.HasValue )
                                {
                                    var groupType = GroupTypeCache.Get( group.GroupTypeId );
                                    var role = groupType.Roles.Where( r => r.Id == roleId.Value ).FirstOrDefault();
                                    if ( role != null )
                                    {
                                        string field = string.Format( "{0} Role", group.Name );
                                        EvaluateChange( changes, field, role.Name );
                                    }
                                }
                            }

                            if ( SelectedFields.Contains( ddlGroupMemberStatus.ClientID ) )
                            {
                                string field = string.Format( "{0} Member Status", group.Name );
                                EvaluateChange( changes, field, ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>().ToString() );
                            }

                            var groupMember = new GroupMember();
                            groupMember.Group = group;
                            groupMember.GroupId = group.Id;
                            groupMember.LoadAttributes( rockContext );

                            foreach ( var attributeCache in groupMember.Attributes.Select( a => a.Value ) )
                            {
                                Control attributeControl = phAttributes.FindControl( string.Format( "attribute_field_{0}", attributeCache.Id ) );
                                if ( attributeControl != null && SelectedFields.Contains( attributeControl.ClientID ) )
                                {
                                    string field = string.Format( "{0}: {1}", group.Name, attributeCache.Name );
                                    string newValue = attributeCache.FieldType.Field.GetEditValue( attributeControl, attributeCache.QualifierValues );
                                    EvaluateChange( changes, field, attributeCache.FieldType.Field.FormatValue( null, newValue, attributeCache.QualifierValues, false ) );
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Tag
                if ( !string.IsNullOrWhiteSpace( ddlTagList.SelectedValue ) )
                {
                    changes.Add( string.Format( "{0} {1} <span class='field-name'>{2}</span> tag.",
                        ddlTagAction.SelectedValue,
                        ddlTagAction.SelectedValue == "Add" ? "to" : "from",
                        ddlTagList.SelectedItem.Text ) );
                }
                #endregion

                #region workflow

                if ( !string.IsNullOrWhiteSpace( rlbWorkFlowType.SelectedValue ) )
                {
                    var workFlowTypes = new List<string>();
                    foreach ( ListItem item in this.rlbWorkFlowType.Items )
                    {
                        if ( item.Selected )
                        {
                            workFlowTypes.Add( item.Text );
                        }
                    }
                    changes.Add( string.Format( "Activate the <span class='field-name'>{0}</span> {1}.",
                         workFlowTypes.AsDelimited( ", ", " and " ),
                         "workflow".PluralizeIf( workFlowTypes.Count > 1 ) ) );
                }
                #endregion


                StringBuilder sb = new StringBuilder();
                sb.AppendFormat( "<p>You are about to make the following updates to {0} individuals:</p>", Individuals.Count().ToString( "N0" ) );
                sb.AppendLine();

                sb.AppendLine( "<ul>" );
                changes.ForEach( c => sb.AppendFormat( "<li>{0}</li>\n", c ) );
                sb.AppendLine( "</ul>" );

                sb.AppendLine( "<p>Please confirm that you want to make these updates.</p>" );

                phConfirmation.Controls.Add( new LiteralControl( sb.ToString() ) );

                pnlEntry.Visible = false;
                pnlConfirm.Visible = true;

                ScriptManager.RegisterStartupScript( Page, this.GetType(), "ScrollPage", "ResetScrollPosition();", true );


            }
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {
            pnlEntry.Visible = true;
            pnlConfirm.Visible = false;
        }

        private long _errorCount;

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                _errorCount = 0;
                var individuals = Individuals.ToList();

                var httpContext = HttpContext.Current;
                Dictionary<object, object> httpContextItems = new Dictionary<object, object>();
                foreach (var key in HttpContext.Current.Items.Keys )
                {
                    httpContextItems[key] = HttpContext.Current.Items[key];
                }

                var task = new Task( () =>
                {
                    int taskCount = GetAttributeValue( "TaskCount" ).AsInteger();
                    int totalCount = individuals.Count;
                    int processedCount = 0;
                    var workers = new List<Task>();
                    DateTime lastNotified = RockDateTime.Now;

                    //
                    // Validate task count.
                    //
                    if ( taskCount > 64 )
                    {
                        // Prevent the user from doing too much damage.
                        taskCount = 64;
                    }
                    else if ( taskCount < 1 )
                    {
                        taskCount = Environment.ProcessorCount;
                    }

                    //
                    // Wait for the browser to finish loading.
                    //
                    Task.Delay( 1000 ).Wait();
                    HubContext.Clients.Client( hfConnectionId.Value ).bulkUpdateProgress( "0", "0" );

                    if ( individuals.Any() )
                    {
                        //
                        // Spin up some workers to process the updates.
                        //
                        for ( int i = 0; i < taskCount; i++ )
                        {
                            var worker = Task.Factory.StartNew( () => WorkerTask( individuals, httpContext, httpContextItems ) );
                            workers.Add( worker );
                        }

                        //
                        // Wait for the workers to finish processing.
                        //
                        while ( workers.Any( t => !t.IsCompleted ) )
                        {
                            var timeDiff = RockDateTime.Now - lastNotified;
                            if ( timeDiff.TotalSeconds >= 2.5 )
                            {
                                lock ( individuals )
                                {
                                    processedCount = totalCount - individuals.Count;
                                }

                                HubContext.Clients.Client( hfConnectionId.Value ).bulkUpdateProgress(
                                    processedCount.ToString( "n0" ),
                                    totalCount.ToString( "n0" ) );

                                lastNotified = RockDateTime.Now;
                            }

                            Task.Delay( 250 ).Wait();
                        }
                    }

                    //
                    // Give any jQuery transitions a moment to settle.
                    //
                    Task.Delay( 600 ).Wait();

                    if ( workers.Any( w => w.IsFaulted ) )
                    {
                        string status = string.Join( "<br>", workers.Where( w => w.IsFaulted ).Select( w => w.Exception.InnerException.Message.EncodeHtml() ) );

                        HubContext.Clients.Client( hfConnectionId.Value ).bulkUpdateStatus( status, false );
                    }
                    else
                    {
                        string status;
                        if ( _errorCount == 0 )
                        {
                            status = string.Format( "{0} {1} successfully updated.",
                                Individuals.Count().ToString( "N0" ), ( Individuals.Count() > 1 ? "people were" : "person was" ) );
                        }
                        else
                        {
                            status = string.Format( "{0} {1} updated with {2} error(s). Please look in the exception log for more details.",
                                Individuals.Count().ToString( "N0" ), ( Individuals.Count() > 1 ? "people were" : "person was" ), _errorCount );
                        }

                        HubContext.Clients.Client( hfConnectionId.Value ).bulkUpdateStatus( status.EncodeHtml(), true );
                    }
                } );

                task.ContinueWith( ( t ) =>
                {
                    if ( t.IsFaulted )
                    {
                        string status = t.Exception.InnerException.Message;
                        HubContext.Clients.Client( hfConnectionId.Value ).exportStatus( status.EncodeHtml(), false );
                    }
                } );

                pnlConfirm.Visible = false;
                pnlProcessing.Visible = true;

                task.Start();
            }
        }

        /// <summary>
        /// Worker task for processing individuals in batches.
        /// </summary>
        /// <param name="individualsList">The individuals list.</param>
        /// <param name="httpContext">The HTTP context. This is needed because is not automatically passed between threads. This is needed to determine the current user.</param>
        /// <param name="httpContextItems">The HTTP context items. This is needed because the Items are cleared when main thread finish.</param>
        protected void WorkerTask( object individualsList, HttpContext httpContext, Dictionary<object, object> httpContextItems )
        {
            HttpContext.Current = httpContext;
            foreach ( var item in httpContextItems )
            {
                HttpContext.Current.Items[item.Key] = item.Value;
            }

            const int batchSize = 50;

            var individuals = ( List<Individual> ) individualsList;

            while ( true )
            {
                List<Individual> batch = null;

                //
                // Get the next chunk.
                //
                lock ( individuals )
                {
                    batch = individuals.Take( batchSize ).ToList();
                    if ( batch.Count > 0 )
                    {
                        individuals.RemoveRange( 0, batch.Count );
                    }
                }

                //
                // Check if we are all done.
                //
                if ( batch.Count == 0 )
                {
                    break;
                }

                ProcessIndividuals( batch );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            AttributeCategories = GetAttributeValue( "AttributeCategories" ).SplitDelimitedValues().AsGuidList();
            phAttributesCol1.Controls.Clear();
            phAttributesCol2.Controls.Clear();
            BuildAttributes( new RockContext(), true );
        }

        /// <summary>
        /// Process the given individuals. This is used to be able to run smaller batches. This provides
        /// a huge boost to performance when dealing with large numbers of people.
        /// </summary>
        /// <param name="individuals">The list of individuals to process in this batch.</param>
        private void ProcessIndividuals( List<Individual> individuals )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var ids = individuals.Select( i => i.PersonId ).ToList();
            //int errorCount = 0;

            #region Individual Details Updates

            int? newTitleId = dvpTitle.SelectedValueAsInt();
            int? newSuffixId = dvpSuffix.SelectedValueAsInt();
            int? newConnectionStatusId = dvpConnectionStatus.SelectedValueAsInt();
            int? newRecordStatusId = dvpRecordStatus.SelectedValueAsInt();
            int? newInactiveReasonId = dvpInactiveReason.SelectedValueAsInt();
            string newInactiveReasonNote = tbInactiveReasonNote.Text;
            Gender newGender = ddlGender.SelectedValue.ConvertToEnum<Gender>();
            int? newMaritalStatusId = dvpMaritalStatus.SelectedValueAsInt();

            int? newGraduationYear = null;
            if ( ypGraduation.SelectedYear.HasValue )
            {
                newGraduationYear = ypGraduation.SelectedYear.Value;
            }

            int? newCampusId = cpCampus.SelectedCampusId;

            bool newEmailActive = true;
            if ( !string.IsNullOrWhiteSpace( ddlIsEmailActive.SelectedValue ) )
            {
                newEmailActive = ddlIsEmailActive.SelectedValue == "Active";
            }

            var newCommunicationPreference = ddlCommunicationPreference.SelectedValueAsEnumOrNull<CommunicationType>();
            EmailPreference? newEmailPreference = ddlEmailPreference.SelectedValue.ConvertToEnumOrNull<EmailPreference>();

            string newEmailNote = tbEmailNote.Text;

            int? newReviewReason = dvpReviewReason.SelectedValueAsInt();
            string newSystemNote = tbSystemNote.Text;
            string newReviewReasonNote = tbReviewReasonNote.Text;

            int inactiveStatusId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;

            var people = personService.Queryable( true ).Where( p => ids.Contains( p.Id ) ).ToList();
            foreach ( var person in people )
            {
                if ( SelectedFields.Contains( dvpTitle.ClientID ) )
                {
                    person.TitleValueId = newTitleId;
                }

                if ( SelectedFields.Contains( dvpSuffix.ClientID ) )
                {
                    person.SuffixValueId = newSuffixId;
                }

                if ( SelectedFields.Contains( dvpConnectionStatus.ClientID ) && _canEditConnectionStatus )
                {
                    person.ConnectionStatusValueId = newConnectionStatusId;
                }

                if ( SelectedFields.Contains( dvpRecordStatus.ClientID )  && _canEditRecordStatus )
                {
                    person.RecordStatusValueId = newRecordStatusId;

                    if ( newRecordStatusId.HasValue && newRecordStatusId.Value == inactiveStatusId )
                    {
                        person.RecordStatusReasonValueId = newInactiveReasonId;

                        if ( !string.IsNullOrWhiteSpace( newInactiveReasonNote ) )
                        {
                            person.InactiveReasonNote = newInactiveReasonNote;
                        }
                    }
                }

                if ( SelectedFields.Contains( ddlGender.ClientID ) )
                {
                    person.Gender = newGender;
                }

                if ( SelectedFields.Contains( dvpMaritalStatus.ClientID ) )
                {
                    person.MaritalStatusValueId = newMaritalStatusId;
                }

                if ( SelectedFields.Contains( ddlGradePicker.ClientID ) )
                {
                    person.GraduationYear = newGraduationYear;
                }

                if ( SelectedFields.Contains( ddlIsEmailActive.ClientID ) )
                {
                    person.IsEmailActive = newEmailActive;
                }

                if ( SelectedFields.Contains( ddlCommunicationPreference.ClientID ) )
                {
                    person.CommunicationPreference = newCommunicationPreference.Value;
                }

                if ( SelectedFields.Contains( ddlEmailPreference.ClientID ) )
                {
                    person.EmailPreference = newEmailPreference.Value;
                }

                if ( SelectedFields.Contains( ddlEmailPreference.ClientID ) )
                {
                    person.EmailPreference = newEmailPreference.Value;
                }

                if ( SelectedFields.Contains( tbEmailNote.ClientID ) )
                {
                    person.EmailNote = newEmailNote;
                }

                if ( SelectedFields.Contains( tbSystemNote.ClientID ) )
                {
                    person.SystemNote = newSystemNote;
                }

                if ( SelectedFields.Contains( dvpReviewReason.ClientID ) )
                {
                    person.ReviewReasonValueId = newReviewReason;
                }

                if ( SelectedFields.Contains( tbReviewReasonNote.ClientID ) )
                {
                    person.ReviewReasonNote = newReviewReasonNote;
                }
            }

            if ( SelectedFields.Contains( cpCampus.ClientID ) && cpCampus.SelectedCampusId.HasValue )
            {
                int campusId = cpCampus.SelectedCampusId.Value;

                Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

                var familyMembers = new GroupMemberService( rockContext ).Queryable()
                    .Where( m => ids.Contains( m.PersonId ) && m.Group.GroupType.Guid == familyGuid )
                    .Select( m => new { m.PersonId, m.GroupId } )
                    .Distinct()
                    .ToList();

                var families = new GroupMemberService( rockContext ).Queryable()
                    .Where( m => ids.Contains( m.PersonId ) && m.Group.GroupType.Guid == familyGuid )
                    .Select( m => m.Group )
                    .Distinct()
                    .ToList();

                foreach ( int personId in ids )
                {
                    var familyIds = familyMembers.Where( m => m.PersonId == personId ).Select( m => m.GroupId ).ToList();
                    if ( familyIds.Count == 1 )
                    {
                        int familyId = familyIds.FirstOrDefault();
                        var family = families.Where( g => g.Id == familyId ).FirstOrDefault();
                        {
                            if ( family != null )
                            {
                                family.CampusId = campusId;
                            }
                            familyMembers.RemoveAll( m => m.GroupId == familyId );
                        }
                    }
                }

                rockContext.SaveChanges();
            }

            // Update following
            if ( SelectedFields.Contains( ddlFollow.ClientID ) )
            {
                var personAliasEntityType = EntityTypeCache.Get( "Rock.Model.PersonAlias" );
                if ( personAliasEntityType != null )
                {
                    int personAliasEntityTypeId = personAliasEntityType.Id;


                    bool follow = true;
                    if ( !string.IsNullOrWhiteSpace( ddlFollow.SelectedValue ) )
                    {
                        follow = ddlFollow.SelectedValue == "Add";
                    }

                    var personAliasService = new PersonAliasService( rockContext );
                    var followingService = new FollowingService( rockContext );
                    if ( follow )
                    {
                        var paQry = personAliasService.Queryable();

                        var alreadyFollowingIds = followingService.Queryable()
                            .Where( f =>
                                f.EntityTypeId == personAliasEntityTypeId &&
                                f.PersonAlias.Id == CurrentPersonAlias.Id )
                            .Join( paQry, f => f.EntityId, p => p.Id, ( f, p ) => new { PersonAlias = p } )
                            .Select( p => p.PersonAlias.PersonId )
                            .Distinct()
                            .ToList();

                        foreach ( int id in ids.Where( id => !alreadyFollowingIds.Contains( id ) ) )
                        {
                            var person = people.FirstOrDefault( p => p.Id == id );
                            if ( person != null && person.PrimaryAliasId.HasValue )
                            {
                                var following = new Following
                                {
                                    EntityTypeId = personAliasEntityTypeId,
                                    EntityId = person.PrimaryAliasId.Value,
                                    PersonAliasId = CurrentPersonAlias.Id
                                };
                                followingService.Add( following );
                            }
                        }
                    }
                    else
                    {
                        var paQry = personAliasService.Queryable()
                            .Where( p => ids.Contains( p.PersonId ) )
                            .Select( p => p.Id );

                        foreach ( var following in followingService.Queryable()
                            .Where( f =>
                                f.EntityTypeId == personAliasEntityTypeId &&
                                paQry.Contains( f.EntityId ) &&
                                f.PersonAlias.Id == CurrentPersonAlias.Id ) )
                        {
                            followingService.Delete( following );
                        }
                    }
                }
            }

            rockContext.SaveChanges();

            #endregion

            #region Attributes

            var selectedCategories = new List<CategoryCache>();
            foreach ( Guid categoryGuid in AttributeCategories )
            {
                var category = CategoryCache.Get( categoryGuid, rockContext );
                if ( category != null )
                {
                    selectedCategories.Add( category );
                }
            }

            var attributes = new List<AttributeCache>();
            var attributeValues = new Dictionary<int, string>();

            int categoryIndex = 0;
            foreach ( var category in selectedCategories.OrderBy( c => c.Name ) )
            {
                PanelWidget pw = null;
                string controlId = "pwAttributes_" + category.Id.ToString();
                if ( categoryIndex % 2 == 0 )
                {
                    pw = phAttributesCol1.FindControl( controlId ) as PanelWidget;
                }
                else
                {
                    pw = phAttributesCol2.FindControl( controlId ) as PanelWidget;
                }
                categoryIndex++;

                if ( pw != null )
                {
                    var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id, false )
                        .OrderBy( a => a.Order ).ThenBy( a => a.Name );
                    foreach ( var attribute in orderedAttributeList )
                    {
                        if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            var attributeCache = AttributeCache.Get( attribute.Id );

                            Control attributeControl = pw.FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );

                            if ( attributeControl != null && SelectedFields.Contains( attributeControl.ClientID ) )
                            {
                                string newValue = attributeCache.FieldType.Field.GetEditValue( attributeControl, attributeCache.QualifierValues );
                                attributes.Add( attributeCache );
                                attributeValues.Add( attributeCache.Id, newValue );
                            }
                        }
                    }
                }
            }

            if ( attributes.Any() )
            {
                foreach ( var person in people )
                {
                    person.LoadAttributes();
                    foreach ( var attribute in attributes )
                    {
                        string originalValue = person.GetAttributeValue( attribute.Key );
                        string newValue = attributeValues[attribute.Id];
                        if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                        {
                            Rock.Attribute.Helper.SaveAttributeValue( person, attribute, newValue, rockContext );

                            string formattedOriginalValue = string.Empty;
                            if ( !string.IsNullOrWhiteSpace( originalValue ) )
                            {
                                formattedOriginalValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                            }

                            string formattedNewValue = string.Empty;
                            if ( !string.IsNullOrWhiteSpace( newValue ) )
                            {
                                formattedNewValue = attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false );
                            }
                        }
                    }
                }
            }

            rockContext.SaveChanges();

            #endregion

            #region Add Note

            if ( !string.IsNullOrWhiteSpace( tbNote.Text ) && CurrentPerson != null )
            {
                string text = tbNote.Text;
                bool isAlert = cbIsAlert.Checked;
                bool isPrivate = cbIsPrivate.Checked;

                var noteType = NoteTypeCache.Get( ddlNoteType.SelectedValueAsId() ?? 0 );
                if ( noteType != null )
                {
                    var notes = new List<Note>();
                    var noteService = new NoteService( rockContext );

                    foreach ( int id in ids )
                    {
                        var note = new Note();
                        note.IsSystem = false;
                        note.EntityId = id;
                        note.Caption = isPrivate ? "You - Personal Note" : string.Empty;
                        note.Text = tbNote.Text;
                        note.IsAlert = cbIsAlert.Checked;
                        note.IsPrivateNote = isPrivate;
                        note.NoteTypeId = noteType.Id;
                        notes.Add( note );
                        noteService.Add( note );
                    }

                    rockContext.SaveChanges();
                }
            }

            #endregion

            #region Group

            int? groupId = gpGroup.SelectedValue.AsIntegerOrNull();
            if ( groupId.HasValue )
            {
                var group = new GroupService( rockContext ).Get( groupId.Value );
                if ( group != null )
                {
                    var groupMemberService = new GroupMemberService( rockContext );

                    var existingMembersQuery = groupMemberService.Queryable( true ).Include( a => a.Group )
                                                                 .Where( m => m.GroupId == group.Id
                                                                              && ids.Contains( m.PersonId ) );

                    string action = ddlGroupAction.SelectedValue;
                    if ( action == "Remove" )
                    {
                        var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );

                        var existingIds = existingMembersQuery.Select( gm => gm.Id ).Distinct().ToList();

                        Action<RockContext, List<int>> deleteAction = ( context, items ) =>
                        {
                            // Load the batch of GroupMember items into the context and delete them.
                            groupMemberService = new GroupMemberService( context );

                            var batchGroupMembers = groupMemberService.Queryable( true ).Where( x => items.Contains( x.Id ) ).ToList();

                            GroupMemberHistoricalService groupMemberHistoricalService = new GroupMemberHistoricalService( context );

                            foreach( GroupMember groupMember in batchGroupMembers )
                            {
                                try
                                {
                                    bool archive = false;
                                    if ( groupTypeCache.EnableGroupHistory == true && groupMemberHistoricalService.Queryable().Any( a => a.GroupMemberId == groupMember.Id ) )
                                    {
                                        // if the group has GroupHistory enabled, and this group member has group member history snapshots, they were prompted to Archive
                                        archive = true;
                                    }
                                    else
                                    {
                                        string errorMessage;
                                        if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                                        {
                                            ExceptionLogService.LogException( new Exception( string.Format( "Error removing person {0} from group {1}: ", groupMember.Person.FullName, group.Name ) + errorMessage ), null );
                                            Interlocked.Increment( ref _errorCount );
                                            continue;
                                        }
                                    }

                                    if ( archive )
                                    {
                                        // NOTE: Delete will AutoArchive, but since we know that we need to archive, we can call .Archive directly
                                        groupMemberService.Archive( groupMember, this.CurrentPersonAliasId, true );
                                    }
                                    else
                                    {
                                        groupMemberService.Delete( groupMember, true );
                                    }

                                    context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    ExceptionLogService.LogException( new Exception( string.Format("Error removing person {0} from group {1}", groupMember.Person.FullName, group.Name), ex ), null );
                                    Interlocked.Increment( ref _errorCount );
                                }
                            }
                        };

                        ProcessBatchUpdate( existingIds, 50, deleteAction );
                    }
                    else
                    {
                        var roleId = ddlGroupRole.SelectedValueAsInt();
                        var status = ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>();

                        // Get the attribute values updated
                        var gm = new GroupMember();
                        gm.Group = group;
                        gm.GroupId = group.Id;
                        gm.LoadAttributes( rockContext );
                        var selectedGroupAttributes = new List<AttributeCache>();
                        var selectedGroupAttributeValues = new Dictionary<string, string>();
                        foreach ( var attributeCache in gm.Attributes.Select( a => a.Value ) )
                        {
                            Control attributeControl = phAttributes.FindControl( string.Format( "attribute_field_{0}", attributeCache.Id ) );
                            if ( attributeControl != null && ( action == "Add" || SelectedFields.Contains( attributeControl.ClientID ) ) )
                            {
                                string newValue = attributeCache.FieldType.Field.GetEditValue( attributeControl, attributeCache.QualifierValues );
                                selectedGroupAttributes.Add( attributeCache );
                                selectedGroupAttributeValues.Add( attributeCache.Key, newValue );
                            }
                        }

                        if ( action == "Add" )
                        {
                            if ( roleId.HasValue )
                            {
                                var newGroupMembers = new List<GroupMember>();

                                var existingIds = existingMembersQuery.Select( m => m.PersonId ).Distinct().ToList();

                                var personKeys = ids.Where( id => !existingIds.Contains( id ) ).ToList();

                                Action<RockContext, List<int>> addAction = ( context, items ) =>
                                {
                                    groupMemberService = new GroupMemberService( context );

                                    foreach ( int id in items )
                                    {
                                        var groupMember = new GroupMember();
                                        groupMember.GroupId = group.Id;
                                        groupMember.GroupRoleId = roleId.Value;
                                        groupMember.GroupMemberStatus = status;
                                        groupMember.PersonId = id;
                                        groupMemberService.Add( groupMember );

                                        newGroupMembers.Add( groupMember );
                                    }

                                    context.SaveChanges();
                                };

                                ProcessBatchUpdate( personKeys, 50, addAction );

                                if ( selectedGroupAttributes.Any() )
                                {
                                    foreach ( var groupMember in newGroupMembers )
                                    {
                                        foreach ( var attribute in selectedGroupAttributes )
                                        {
                                            Rock.Attribute.Helper.SaveAttributeValue( groupMember, attribute, selectedGroupAttributeValues[attribute.Key], rockContext );
                                        }
                                    }
                                }
                            }
                        }
                        else // Update
                        {
                            if ( SelectedFields.Contains( ddlGroupRole.ClientID ) && roleId.HasValue )
                            {
                                foreach ( var member in existingMembersQuery.Where( m => m.GroupRoleId != roleId.Value ) )
                                {
                                    if ( !existingMembersQuery.Any( m => m.PersonId == member.PersonId && m.GroupRoleId == roleId.Value ) )
                                    {
                                        member.GroupRoleId = roleId.Value;
                                    }
                                }
                            }

                            if ( SelectedFields.Contains( ddlGroupMemberStatus.ClientID ) )
                            {
                                foreach ( var member in existingMembersQuery )
                                {
                                    member.GroupMemberStatus = status;
                                }
                            }

                            rockContext.SaveChanges();

                            if ( selectedGroupAttributes.Any() )
                            {
                                Action<RockContext, List<GroupMember>> updateAction = ( context, items ) =>
                                {
                                    foreach ( var groupMember in items )
                                    {
                                        foreach ( var attribute in selectedGroupAttributes )
                                        {
                                            Rock.Attribute.Helper.SaveAttributeValue( groupMember, attribute, selectedGroupAttributeValues[attribute.Key], context );
                                        }
                                    }

                                    context.SaveChanges();
                                };

                                // Process the Attribute updates in batches.
                                var existingMembers = existingMembersQuery.ToList();

                                ProcessBatchUpdate( existingMembers, 50, updateAction );
                            }
                        }
                    }
                }
            }

            #endregion

            #region Tag
            var personEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

            if ( !string.IsNullOrWhiteSpace( ddlTagList.SelectedValue ) )
            {
                int tagId = ddlTagList.SelectedValue.AsInteger();

                var tag = new TagService( rockContext ).Get( tagId );
                if ( tag != null && tag.IsAuthorized( Rock.Security.Authorization.TAG, CurrentPerson ) )
                {
                    var taggedItemService = new TaggedItemService( rockContext );

                    // get guids of selected individuals
                    var personGuids = new PersonService( rockContext ).Queryable( true )
                                        .Where( p =>
                                            ids.Contains( p.Id ) )
                                        .Select( p => p.Guid )
                                        .ToList();

                    if ( ddlTagAction.SelectedValue == "Add" )
                    {
                        foreach ( var personGuid in personGuids )
                        {
                            if ( !taggedItemService.Queryable().Where( t => t.TagId == tagId && t.EntityGuid == personGuid ).Any() )
                            {
                                TaggedItem taggedItem = new TaggedItem();
                                taggedItem.TagId = tagId;
                                taggedItem.EntityTypeId = personEntityTypeId;
                                taggedItem.EntityGuid = personGuid;

                                taggedItemService.Add( taggedItem );
                                rockContext.SaveChanges();
                            }
                        }
                    }
                    else // remove
                    {
                        foreach ( var personGuid in personGuids )
                        {
                            var taggedPerson = taggedItemService.Queryable().Where( t => t.TagId == tagId && t.EntityGuid == personGuid ).FirstOrDefault();
                            if ( taggedPerson != null )
                            {
                                taggedItemService.Delete( taggedPerson );
                            }
                        }
                        rockContext.SaveChanges();
                    }
                }
            }
            #endregion

            #region workflow

            IEnumerable<string> selectedWorkflows = from ListItem li in rlbWorkFlowType.Items
                                                    where li.Selected == true
                                                    select li.Value;
            foreach ( string value in selectedWorkflows )
            {
                int? intValue = value.AsIntegerOrNull();
                if ( intValue.HasValue )
                {

                    var workflowDetails = people.Select( p => new LaunchWorkflowDetails( p ) ).ToList();
                    var launchWorkflowsTxn = new Rock.Transactions.LaunchWorkflowsTransaction( intValue.Value, workflowDetails );
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( launchWorkflowsTxn );
                }
            }
            #endregion
        }

        /// <summary>
        /// Process database updates for the supplied list of items in batches to improve performance for large datasets.
        /// </summary>
        /// <param name="itemsToProcess"></param>
        /// <param name="batchSize"></param>
        /// <param name="processingAction"></param>
        private void ProcessBatchUpdate<TListItem>( List<TListItem> itemsToProcess, int batchSize, Action<RockContext, List<TListItem>> processingAction )
        {
            int remainingCount = itemsToProcess.Count();

            int batchesProcessed = 0;

            while ( remainingCount > 0 )
            {
                var batchItems = itemsToProcess.Skip( batchesProcessed * batchSize ).Take( batchSize ).ToList();

                using ( var batchContext = new RockContext() )
                {
                    processingAction.Invoke( batchContext, batchItems );
                }

                batchesProcessed++;

                remainingCount -= batchItems.Count();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRecordStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRecordStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            dvpInactiveReason.Visible = ( dvpRecordStatus.SelectedValueAsInt() == DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id );
            tbInactiveReasonNote.Visible = dvpInactiveReason.Visible;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupAction_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetGroupControls();
        }

        /// <summary>
        /// Handles the SelectItem event of the gpGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpGroup_SelectItem( object sender, EventArgs e )
        {
            SetGroupControls();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Binds the individuals.
        /// </summary>
        private void BindIndividuals()
        {
            int individualCount = Individuals.Count();
            lNumIndividuals.Text = individualCount.ToString( "N0" ) +
                ( individualCount == 1 ? " Person" : " People" );

            ppAddPerson.PersonId = Rock.Constants.None.Id;
            ppAddPerson.PersonName = "Add Person";

            int displayCount = int.MaxValue;

            if ( !ShowAllIndividuals )
            {
                int.TryParse( GetAttributeValue( "DisplayCount" ), out displayCount );
            }

            if ( displayCount > 0 && displayCount < Individuals.Count )
            {
                rptIndividuals.DataSource = Individuals.Take( displayCount ).ToList();
                lbShowAllIndividuals.Visible = true;
            }
            else
            {
                rptIndividuals.DataSource = Individuals.ToList();
                lbShowAllIndividuals.Visible = false;
            }

            rptIndividuals.DataBind();
        }

        private void SetControlSelection()
        {
            SetControlSelection( dvpTitle, "Title" );
            SetControlSelection( dvpConnectionStatus, "Connection Status", _canEditConnectionStatus );
            SetControlSelection( ddlGender, "Gender" );
            SetControlSelection( dvpMaritalStatus, "Marital Status" );
            SetControlSelection( ddlGradePicker, GlobalAttributesCache.Get().GetValue( "core.GradeLabel" ) );
            ypGraduation.Enabled = ddlGradePicker.Enabled;

            SetControlSelection( cpCampus, "Campus" );
            SetControlSelection( ddlCommunicationPreference, "Communication Preference" );
            SetControlSelection( dvpSuffix, "Suffix" );
            SetControlSelection( dvpRecordStatus, "Record Status", _canEditRecordStatus );
            SetControlSelection( ddlIsEmailActive, "Email Status" );
            SetControlSelection( ddlEmailPreference, "Email Preference" );
            SetControlSelection( tbEmailNote, "Email Note" );
            SetControlSelection( ddlFollow, "Follow" );
            SetControlSelection( tbSystemNote, "System Note" );
            SetControlSelection( dvpReviewReason, "Review Reason" );
            SetControlSelection( tbReviewReasonNote, "Review Reason Note" );
        }

        private void SetControlSelection( IRockControl control, string label )
        {
            bool controlEnabled = SelectedFields.Contains( control.ClientID, StringComparer.OrdinalIgnoreCase );
            string iconCss = controlEnabled ? "fa-check-circle-o" : "fa-circle-o";
            control.Label = string.Format( "<span class='js-select-item'><i class='fa {0}'></i></span> {1}", iconCss, label );
            var webControl = control as WebControl;
            if ( webControl != null )
            {
                webControl.Enabled = controlEnabled;
            }
        }
        private void SetControlSelection( IRockControl control, string label, bool canEdit )
        {
            if (canEdit)
            {
                SetControlSelection( control, label );
            }
        }

        private void BuildAttributes( RockContext rockContext, bool setValues = false )
        {
            var selectedCategories = new List<CategoryCache>();
            foreach ( Guid categoryGuid in AttributeCategories )
            {
                var category = CategoryCache.Get( categoryGuid, rockContext );
                if ( category != null )
                {
                    selectedCategories.Add( category );
                }
            }

            int categoryIndex = 0;
            foreach ( var category in selectedCategories.OrderBy( c => c.Name ) )
            {
                var pw = new PanelWidget();
                if ( categoryIndex % 2 == 0 )
                {
                    phAttributesCol1.Controls.Add( pw );
                }
                else
                {
                    phAttributesCol2.Controls.Add( pw );
                }
                pw.ID = "pwAttributes_" + category.Id.ToString();
                categoryIndex++;


                if ( !string.IsNullOrWhiteSpace( category.IconCssClass ) )
                {
                    pw.TitleIconCssClass = category.IconCssClass;
                }
                pw.Title = category.Name;

                var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id, false )
                    .OrderBy( a => a.Order ).ThenBy( a => a.Name );
                foreach ( var attribute in orderedAttributeList )
                {
                    if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        var attributeCache = AttributeCache.Get( attribute.Id );

                        string clientId = string.Format( "{0}_attribute_field_{1}", pw.ClientID, attribute.Id );
                        bool controlEnabled = SelectedFields.Contains( clientId, StringComparer.OrdinalIgnoreCase );
                        string iconCss = controlEnabled ? "fa-check-circle-o" : "fa-circle-o";

                        string labelText = string.Format( "<span class='js-select-item'><i class='fa {0}'></i></span> {1}", iconCss, attributeCache.Name );
                        Control control = attributeCache.AddControl( pw.Controls, string.Empty, string.Empty, setValues, true, false, labelText );

                        if ( !( control is RockCheckBox ) && !( control is PersonPicker ) && !( control is ItemPicker ) )
                        {
                            var webControl = control as WebControl;
                            if ( webControl != null )
                            {
                                webControl.Enabled = controlEnabled;
                            }
                        }
                    }
                }
            }
        }

        private void SetGroupControls()
        {
            string action = ddlGroupAction.SelectedValue;
            if ( action == "Remove" )
            {
                ddlGroupMemberStatus.Visible = false;
                ddlGroupRole.Visible = false;
            }
            else
            {
                var rockContext = new RockContext();
                Group group = null;

                int? groupId = gpGroup.SelectedValueAsId();
                if ( groupId.HasValue )
                {
                    group = new GroupService( rockContext ).Get( groupId.Value );
                }

                if ( group != null )
                {
                    GroupId = group.Id;

                    ddlGroupRole.Visible = true;
                    ddlGroupMemberStatus.Visible = true;

                    if ( action == "Add" )
                    {
                        pnlGroupMemberStatus.RemoveCssClass( "fade-inactive" );
                        pnlGroupMemberAttributes.RemoveCssClass( "fade-inactive" );

                        ddlGroupRole.Label = "Role";
                        ddlGroupRole.Enabled = true;

                        ddlGroupMemberStatus.Label = "Member Status";
                        ddlGroupMemberStatus.Enabled = true;
                    }
                    else
                    {
                        pnlGroupMemberStatus.AddCssClass( "fade-inactive" );
                        pnlGroupMemberAttributes.AddCssClass( "fade-inactive" );
                        SetControlSelection( ddlGroupRole, "Role" );
                        SetControlSelection( ddlGroupMemberStatus, "Member Status" );
                    }

                    var groupType = GroupTypeCache.Get( group.GroupTypeId );
                    ddlGroupRole.Items.Clear();
                    ddlGroupRole.DataSource = groupType.Roles.OrderBy( r => r.Order ).ToList();
                    ddlGroupRole.DataBind();
                    ddlGroupRole.SelectedValue = groupType.DefaultGroupRoleId.ToString();

                    ddlGroupMemberStatus.SelectedValue = "1";

                    phAttributes.Controls.Clear();
                    BuildGroupAttributes( group, rockContext, true );
                }
                else
                {
                    ddlGroupRole.Visible = false;
                    ddlGroupMemberStatus.Visible = false;

                    ddlGroupRole.Items.Add( new ListItem( string.Empty, string.Empty ) );
                    ddlGroupMemberStatus.Items.Add( new ListItem( string.Empty, string.Empty ) );
                }
            }
        }

        private void BuildGroupAttributes( RockContext rockContext )
        {
            if ( GroupId.HasValue )
            {
                var group = new GroupService( rockContext ).Get( GroupId.Value );
                BuildGroupAttributes( group, rockContext, false );
            }
        }

        private void BuildGroupAttributes( Group group, RockContext rockContext, bool setValues )
        {
            if ( group != null )
            {
                string action = ddlGroupAction.SelectedValue;

                var groupMember = new GroupMember();
                groupMember.Group = group;
                groupMember.GroupId = group.Id;
                groupMember.LoadAttributes( rockContext );

                foreach ( var attributeCache in groupMember.Attributes.Select( a => a.Value ) )
                {
                    string labelText = attributeCache.Name;
                    bool controlEnabled = true;
                    if ( action == "Update" )
                    {
                        string clientId = string.Format( "{0}_attribute_field_{1}", phAttributes.NamingContainer.ClientID, attributeCache.Id );
                        controlEnabled = SelectedFields.Contains( clientId, StringComparer.OrdinalIgnoreCase );

                        string iconCss = controlEnabled ? "fa-check-circle-o" : "fa-circle-o";
                        labelText = string.Format( "<span class='js-select-item'><i class='fa {0}'></i></span> {1}", iconCss, attributeCache.Name );
                    }

                    Control control = attributeCache.AddControl( phAttributes.Controls, attributeCache.DefaultValue, string.Empty, setValues, true, attributeCache.IsRequired, labelText );

                    // Q: Why don't we enable if the control is a RockCheckBox?
                    if ( action == "Update" && !( control is RockCheckBox ) )
                    {
                        var webControl = control as WebControl;
                        if ( webControl != null )
                        {
                            webControl.Enabled = controlEnabled;
                        }
                    }
                }
            }
        }

        #region Evaluate Change Methods

        /// <summary>
        /// Evaluates the change, and adds a summary string of what if anything changed
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void EvaluateChange( List<string> historyMessages, string propertyName, string newValue )
        {
            if ( !string.IsNullOrWhiteSpace( newValue ) )
            {
                historyMessages.Add( string.Format( "Update <span class='field-name'>{0}</span> to value of <span class='field-value'>{1}</span>.", propertyName, newValue ) );
            }
            else
            {
                historyMessages.Add( string.Format( "Clear <span class='field-name'>{0}</span> value.", propertyName ) );
            }
        }

        /// <summary>
        /// Evaluates the change, and adds a summary string of what if anything changed
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void EvaluateChange( List<string> historyMessages, string propertyName, int? newValue )
        {
            EvaluateChange( historyMessages, propertyName,
                newValue.HasValue ? newValue.Value.ToString() : string.Empty );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="includeTime">if set to <c>true</c> [include time].</param>
        private void EvaluateChange( List<string> historyMessages, string propertyName, DateTime? newValue, bool includeTime = false )
        {
            string newStringValue = string.Empty;
            if ( newValue.HasValue )
            {
                newStringValue = includeTime ? newValue.Value.ToString() : newValue.Value.ToShortDateString();
            }

            EvaluateChange( historyMessages, propertyName, newStringValue );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">if set to <c>true</c> [old value].</param>
        /// <param name="newValue">if set to <c>true</c> [new value].</param>
        private void EvaluateChange( List<string> historyMessages, string propertyName, bool? newValue )
        {
            EvaluateChange( historyMessages, propertyName,
                newValue.HasValue ? newValue.Value.ToString() : string.Empty );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void EvaluateChange( List<string> historyMessages, string propertyName, Enum newValue )
        {
            string newStringValue = newValue != null ? newValue.ConvertToString() : string.Empty;
            EvaluateChange( historyMessages, propertyName, newStringValue );
        }

        #endregion

        #endregion

        #region Helper Classes

        /// <summary>
        /// Helper class used to maintain state of individuals
        /// </summary>
        [Serializable]
        protected class Individual
        {
            /// <summary>
            /// Gets or sets the person id.
            /// </summary>
            /// <value>
            /// The person id.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the name of the person.
            /// </summary>
            /// <value>
            /// The name of the person.
            /// </value>
            public string PersonName { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Individual" /> class.
            /// </summary>
            /// <param name="personId">The person id.</param>
            /// <param name="personName">Name of the person.</param>
            /// <param name="status">The status.</param>
            public Individual( Person person )
            {
                PersonId = person.Id;
                PersonName = person.FullName;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Individual"/> class.
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <param name="name">The name.</param>
            public Individual( int id, string name )
            {
                PersonId = id;
                PersonName = name;
            }

        }

        #endregion


    }
}
