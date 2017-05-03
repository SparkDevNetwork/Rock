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
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Xsl;

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

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// User control for creating a new communication.  This block should be used on same page as the CommunicationDetail block and only visible when editing a new or transient communication
    /// </summary>
    [DisplayName( "Bulk Update" )]
    [Category( "CRM" )]
    [Description( "Used for updating information about several individuals at once." )]

    [AttributeCategoryField( "Attribute Categories", "The person attribute categories to display and allow bulk updating", true, "Rock.Model.Person", false, "", "", 0 )]
    [IntegerField( "Display Count", "The initial number of individuals to display prior to expanding list", false, 0, "", 1  )]
    public partial class BulkUpdate : RockBlock
    {
        #region Fields

        DateTime _gradeTransitionDate = new DateTime( RockDateTime.Today.Year, 6, 1 );

        #endregion

        #region Properties

        private List<Individual> Individuals { get; set; }
        private bool ShowAllIndividuals { get; set; }
        private int? GroupId { get; set; }
        private List<string> SelectedFields { get; set; }

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var personEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

            ddlTitle.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ), true );
            ddlStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ) );
            ddlMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ) );
            ddlSuffix.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ), true );
            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ) );
            ddlInactiveReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ) );
            ddlReviewReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_REVIEW_REASON ) ), true );

            ddlTagList.Items.Clear();
            ddlTagList.DataTextField = "Name";
            ddlTagList.DataValueField = "Id";
            var currentPersonAliasIds = CurrentPerson.Aliases.Select( a => a.Id ).ToList();

            var tagList = new TagService( new RockContext() ).Queryable()
                                            .Where( t =>
                                                        t.EntityTypeId == personEntityTypeId
                                                        && (t.OwnerPersonAliasId == null || currentPersonAliasIds.Contains( t.OwnerPersonAliasId.Value )) )
                                            .Select( t => new   {
                                                                    Id = t.Id,
                                                                    Type = t.OwnerPersonAliasId == null ? "Personal Tags" : "Organization Tags",
                                                                    Name = t.Name
                                                                } )
                                            .OrderByDescending(t => t.Type)
                                            .ThenBy( t => t.Name)
                                            .ToList();
            foreach (var tag in tagList )
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
                cpCampus.Campuses = CampusCache.All();

                Individuals = new List<Individual>();
                SelectedFields = new List<string>();

                int? setId = PageParameter( "Set" ).AsIntegerOrNull();
                if ( setId.HasValue )
                {
                    var selectedPersonIds = new EntitySetItemService( rockContext )
                        .GetByEntitySetId( setId.Value )
                        .Select( i => i.EntityId )
                        .Distinct()
                        .ToList();

                    // Get the people selected
                    foreach ( var person in new PersonService( rockContext ).Queryable( true )
                        .Where( p => selectedPersonIds.Contains( p.Id ) )
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
            args.IsValid = SelectedFields.Any() || !string.IsNullOrWhiteSpace( tbNote.Text ) || ( groupId.HasValue && groupId > 0 ) || tagId.HasValue; 
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

                int inactiveStatusId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;

                var changes = new List<string>();

                if ( SelectedFields.Contains( ddlTitle.ClientID ) )
                {
                    int? newTitleId = ddlTitle.SelectedValueAsInt();
                    EvaluateChange( changes, "Title", DefinedValueCache.GetName( newTitleId ) );
                }

                if ( SelectedFields.Contains( ddlSuffix.ClientID ) )
                {
                    int? newSuffixId = ddlSuffix.SelectedValueAsInt();
                    EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( newSuffixId ) );
                }

                if ( SelectedFields.Contains( ddlStatus.ClientID ) )
                {
                    int? newConnectionStatusId = ddlStatus.SelectedValueAsInt();
                    EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( newConnectionStatusId ) );
                }

                if ( SelectedFields.Contains( ddlRecordStatus.ClientID ) )
                {
                    int? newRecordStatusId = ddlRecordStatus.SelectedValueAsInt();
                    EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( newRecordStatusId ) );

                    if ( newRecordStatusId.HasValue && newRecordStatusId.Value == inactiveStatusId )
                    {
                        int? newInactiveReasonId = ddlInactiveReason.SelectedValueAsInt();
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

                if ( SelectedFields.Contains( ddlMaritalStatus.ClientID ) )
                {
                    int? newMaritalStatusId = ddlMaritalStatus.SelectedValueAsInt();
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

                if ( SelectedFields.Contains( ddlReviewReason.ClientID ) )
                {
                    int? newReviewReason = ddlReviewReason.SelectedValueAsInt();
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
                        var campus = CampusCache.Read(newCampusId.Value);
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
                foreach ( string categoryGuid in GetAttributeValue( "AttributeCategories" ).SplitDelimitedValues() )
                {
                    var category = CategoryCache.Read( categoryGuid.AsGuid(), rockContext );
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
                        var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id )
                            .OrderBy( a => a.Order ).ThenBy( a => a.Name );
                        foreach ( var attribute in orderedAttributeList )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                var attributeCache = AttributeCache.Read( attribute.Id );

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
                        else if ( action == "Add")
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
                                    var groupType = GroupTypeCache.Read( group.GroupTypeId );
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

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat( "<p>You are about to make the following updates to {0} individuals:</p>", Individuals.Count().ToString( "N0" ) );
                sb.AppendLine();

                sb.AppendLine( "<ul>" );
                changes.ForEach( c => sb.AppendFormat("<li>{0}</li>\n", c));
                sb.AppendLine( "</ul>" );

                sb.AppendLine( "<p>Please confirm that you want to make these updates.</p>");

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

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var ids = Individuals.Select( i => i.PersonId ).ToList();

                #region Individual Details Updates

                int? newTitleId = ddlTitle.SelectedValueAsInt();
                int? newSuffixId = ddlSuffix.SelectedValueAsInt();
                int? newConnectionStatusId = ddlStatus.SelectedValueAsInt();
                int? newRecordStatusId = ddlRecordStatus.SelectedValueAsInt();
                int? newInactiveReasonId = ddlInactiveReason.SelectedValueAsInt();
                string newInactiveReasonNote = tbInactiveReasonNote.Text;
                Gender newGender = ddlGender.SelectedValue.ConvertToEnum<Gender>();
                int? newMaritalStatusId = ddlMaritalStatus.SelectedValueAsInt();

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

                EmailPreference? newEmailPreference = ddlEmailPreference.SelectedValue.ConvertToEnumOrNull<EmailPreference>();

                string newEmailNote = tbEmailNote.Text;


                int? newReviewReason = ddlReviewReason.SelectedValueAsInt();
                string newSystemNote = tbSystemNote.Text;
                string newReviewReasonNote = tbReviewReasonNote.Text;

                int inactiveStatusId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;

                var allChanges = new Dictionary<int, List<string>>();

                var people = personService.Queryable().Where( p => ids.Contains( p.Id ) ).ToList();
                foreach ( var person in people )
                {
                    var changes = new List<string>();
                    allChanges.Add( person.Id, changes );

                    if ( SelectedFields.Contains( ddlTitle.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Title", DefinedValueCache.GetName( person.TitleValueId ), DefinedValueCache.GetName( newTitleId ) );
                        person.TitleValueId = newTitleId;
                    }

                    if ( SelectedFields.Contains( ddlSuffix.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( person.SuffixValueId ), DefinedValueCache.GetName( newSuffixId ) );
                        person.SuffixValueId = newSuffixId;
                    }

                    if ( SelectedFields.Contains( ddlStatus.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( person.ConnectionStatusValueId ), DefinedValueCache.GetName( newConnectionStatusId ) );
                        person.ConnectionStatusValueId = newConnectionStatusId;
                    }

                    if ( SelectedFields.Contains( ddlRecordStatus.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( person.RecordStatusValueId ), DefinedValueCache.GetName( newRecordStatusId ) );
                        person.RecordStatusValueId = newRecordStatusId;

                        if ( newRecordStatusId.HasValue && newRecordStatusId.Value == inactiveStatusId )
                        {
                            History.EvaluateChange( changes, "Inactive Reason", DefinedValueCache.GetName( person.RecordStatusReasonValueId ), DefinedValueCache.GetName( newInactiveReasonId ) );
                            person.RecordStatusReasonValueId = newInactiveReasonId;

                            if ( !string.IsNullOrWhiteSpace( newInactiveReasonNote ) )
                            {
                                History.EvaluateChange( changes, "Inactive Reason Note", person.InactiveReasonNote, newInactiveReasonNote );
                                person.InactiveReasonNote = newInactiveReasonNote;
                            }
                        }
                    }

                    if ( SelectedFields.Contains( ddlGender.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Gender", person.Gender, newGender );
                        person.Gender = newGender;
                    }

                    if ( SelectedFields.Contains( ddlMaritalStatus.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( newMaritalStatusId ) );
                        person.MaritalStatusValueId = newMaritalStatusId;
                    }

                    if ( SelectedFields.Contains( ddlGradePicker.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Graduation Year", person.GraduationYear, newGraduationYear );
                        person.GraduationYear = newGraduationYear;
                    }

                    if ( SelectedFields.Contains( ddlIsEmailActive.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Email Is Active", person.IsEmailActive, newEmailActive );
                        person.IsEmailActive = newEmailActive;
                    }

                    if ( SelectedFields.Contains( ddlEmailPreference.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Email Preference", person.EmailPreference, newEmailPreference );
                        person.EmailPreference = newEmailPreference.Value;
                    }

                    if ( SelectedFields.Contains( tbEmailNote.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Email Note", person.EmailNote, newEmailNote );
                        person.EmailNote = newEmailNote;
                    }

                    if ( SelectedFields.Contains( tbSystemNote.ClientID ) )
                    {
                        History.EvaluateChange( changes, "System Note", person.SystemNote, newSystemNote );
                        person.SystemNote = newSystemNote;
                    }

                    if ( SelectedFields.Contains( ddlReviewReason.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Review Reason", DefinedValueCache.GetName( person.ReviewReasonValueId ), DefinedValueCache.GetName( newReviewReason ) );
                        person.ReviewReasonValueId = newReviewReason;
                    }

                    if ( SelectedFields.Contains( tbReviewReasonNote.ClientID ) )
                    {
                        History.EvaluateChange( changes, "Review Reason Note", person.ReviewReasonNote, newReviewReasonNote );
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
                    var personAliasEntityType = EntityTypeCache.Read( "Rock.Model.PersonAlias" );
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
                foreach ( string categoryGuid in GetAttributeValue( "AttributeCategories" ).SplitDelimitedValues() )
                {
                    var category = CategoryCache.Read( categoryGuid.AsGuid(), rockContext );
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
                        var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id )
                            .OrderBy( a => a.Order ).ThenBy( a => a.Name );
                        foreach ( var attribute in orderedAttributeList )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                var attributeCache = AttributeCache.Read( attribute.Id );

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

                                History.EvaluateChange( allChanges[person.Id], attribute.Name, formattedOriginalValue, formattedNewValue );
                            }
                        }
                    }
                }

                // Create the history records
                foreach ( var changes in allChanges )
                {
                    if ( changes.Value.Any() )
                    {
                        HistoryService.AddChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                            changes.Key, changes.Value );
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

                    var noteType = NoteTypeCache.Read( ddlNoteType.SelectedValueAsId() ?? 0 );
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

                        var existingMembersQuery = groupMemberService.Queryable("Group")
                                                                     .Where(m => m.GroupId == group.Id
                                                                                 && ids.Contains(m.PersonId));

                        string action = ddlGroupAction.SelectedValue;
                        if ( action == "Remove" )
                        {
                            var existingIds = existingMembersQuery.Select( gm => gm.Id ).Distinct().ToList();

                            Action<RockContext, List<int>> deleteAction = (context, items) =>
                                                                                  {
                                                                                      // Load the batch of GroupMember items into the context and delete them.
                                                                                      groupMemberService = new GroupMemberService(context);

                                                                                      var batchGroupMembers = groupMemberService.Queryable().Where(x => items.Contains(x.Id)).ToList();

                                                                                      // also unregister them from any registration groups
                                                                                      RegistrationRegistrantService registrantService = new RegistrationRegistrantService( context );
                                                                                      foreach ( var registrant in registrantService.Queryable().Where( r => r.GroupMemberId.HasValue && items.Contains( r.GroupMemberId.Value ) ) )
                                                                                      {
                                                                                         registrant.GroupMemberId = null;
                                                                                      }
                                                                                      
                                                                                      groupMemberService.DeleteRange( batchGroupMembers );
                                                                                      
                                                                                      context.SaveChanges();
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
                                    
                                    var personKeys = ids.Where(id => !existingIds.Contains(id)).ToList();

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
                                            groupMemberService.Add(groupMember);
                                            
                                            newGroupMembers.Add(groupMember);
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
                                            foreach (var attribute in selectedGroupAttributes)
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
                var personEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

                if ( !string.IsNullOrWhiteSpace( ddlTagList.SelectedValue ) )
                {
                    int tagId = ddlTagList.SelectedValue.AsInteger();
                    var taggedItemService = new TaggedItemService( rockContext );

                    // get guids of selected individuals
                    var personGuids = new PersonService( rockContext ).Queryable()
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
                                taggedItem.EntityGuid = personGuid;

                                taggedItemService.Add( taggedItem );
                                rockContext.SaveChanges();
                            }
                        }
                    }
                    else // remove
                    {
                        foreach(var personGuid in personGuids )
                        {
                            var taggedPerson = taggedItemService.Queryable().Where( t => t.TagId == tagId && t.EntityGuid == personGuid ).FirstOrDefault();
                            if (taggedPerson != null )
                            {
                                taggedItemService.Delete( taggedPerson );
                            }
                        }
                        rockContext.SaveChanges();
                    }
                }
                #endregion

                    pnlEntry.Visible = false;
                pnlConfirm.Visible = false;

                nbResult.Text = string.Format( "{0} {1} successfully updated.",
                    ids.Count().ToString( "N0" ), ( ids.Count() > 1 ? "people were" : "person was" ) ); ;
                pnlResult.Visible = true;
            }
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

            while (remainingCount > 0)
            {
                var batchItems = itemsToProcess.Skip(batchesProcessed * batchSize).Take( batchSize ).ToList();
                                
                using (var batchContext = new RockContext())
                {
                    processingAction.Invoke(batchContext, batchItems);
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
            ddlInactiveReason.Visible = ( ddlRecordStatus.SelectedValueAsInt() == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id );
            tbInactiveReasonNote.Visible = ddlInactiveReason.Visible;
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
                (individualCount == 1 ? " Person" : " People");

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
            SetControlSelection( ddlTitle, "Title" );
            SetControlSelection( ddlStatus, "Connection Status" );
            SetControlSelection( ddlGender, "Gender" );
            SetControlSelection( ddlMaritalStatus, "Marital Status" );
            SetControlSelection( ddlGradePicker, GlobalAttributesCache.Read().GetValue( "core.GradeLabel" ) );
            ypGraduation.Enabled = ddlGradePicker.Enabled;

            SetControlSelection( cpCampus, "Campus" );
            SetControlSelection( ddlSuffix, "Suffix" );
            SetControlSelection( ddlRecordStatus, "Record Status" );
            SetControlSelection( ddlIsEmailActive, "Email Status" );
            SetControlSelection( ddlEmailPreference, "Email Preference" );
            SetControlSelection( tbEmailNote, "Email Note" );
            SetControlSelection( ddlFollow, "Follow" );
            SetControlSelection( tbSystemNote, "System Note" );
            SetControlSelection( ddlReviewReason, "Review Reason" );
            SetControlSelection( tbReviewReasonNote, "Review Reason Note" );
        }

        private void SetControlSelection( IRockControl control, string label )
        {
            bool controlEnabled = SelectedFields.Contains( control.ClientID, StringComparer.OrdinalIgnoreCase );
            string iconCss = controlEnabled ? "fa-check-circle-o" : "fa-circle-o";
            control.Label = string.Format( "<span class='js-select-item'><i class='fa {0}'></i></span> {1}", iconCss, label );
            var webControl = control as WebControl;
            if (webControl != null)
            {
                webControl.Enabled = controlEnabled;
            }
        }

        private void BuildAttributes( RockContext rockContext, bool setValues = false )
        {
            var selectedCategories = new List<CategoryCache>();
            foreach ( string categoryGuid in GetAttributeValue( "AttributeCategories" ).SplitDelimitedValues() )
            {
                var category = CategoryCache.Read( categoryGuid.AsGuid(), rockContext );
                if ( category != null )
                {
                    selectedCategories.Add( category );
                }
            }

            int categoryIndex = 0;
            foreach( var category in selectedCategories.OrderBy( c => c.Name ) )
            {
                var pw = new PanelWidget();
                if ( categoryIndex % 2 == 0)
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

                var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id )
                    .OrderBy( a => a.Order ).ThenBy( a => a.Name );
                foreach ( var attribute in orderedAttributeList )
                {
                    if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        var attributeCache = AttributeCache.Read( attribute.Id );

                        string clientId = string.Format( "{0}_attribute_field_{1}", pw.ClientID, attribute.Id );
                        bool controlEnabled = SelectedFields.Contains( clientId, StringComparer.OrdinalIgnoreCase );
                        string iconCss = controlEnabled ? "fa-check-circle-o" : "fa-circle-o";

                        string labelText = string.Format( "<span class='js-select-item'><i class='fa {0}'></i></span> {1}", iconCss, attributeCache.Name );
                        Control control = attributeCache.AddControl( pw.Controls, string.Empty, string.Empty, setValues, true, false, labelText );

                        if ( !( control is RockCheckBox ) && !( control is PersonPicker ) && !(control is ItemPicker) )
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
                        pnlGroupMemberStatus.RemoveCssClass("fade-inactive");
                        pnlGroupMemberAttributes.RemoveCssClass("fade-inactive");

                        ddlGroupRole.Label = "Role";
                        ddlGroupRole.Enabled = true;

                        ddlGroupMemberStatus.Label = "Member Status";
                        ddlGroupMemberStatus.Enabled = true;
                    }
                    else
                    {
                        pnlGroupMemberStatus.AddCssClass("fade-inactive");
                        pnlGroupMemberAttributes.AddCssClass("fade-inactive");
                        SetControlSelection( ddlGroupRole, "Role" );
                        SetControlSelection( ddlGroupMemberStatus, "Member Status" );
                    }
                    
                    var groupType = GroupTypeCache.Read( group.GroupTypeId );
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

        private void BuildGroupAttributes(RockContext rockContext)
        {
            if ( GroupId.HasValue )
            {
                var group = new GroupService( rockContext ).Get( GroupId.Value );
                BuildGroupAttributes( group, rockContext, false );
            }
        }

        private void BuildGroupAttributes( Group group, RockContext rockContext, bool setValues )
        {
            if (group != null)
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
            public Individual( int id, string name)
            {
                PersonId = id;
                PersonName = name;
            }

        }

        #endregion


}
}
