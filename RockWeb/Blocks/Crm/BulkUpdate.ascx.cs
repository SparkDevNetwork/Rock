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
    [TextField( "Note Type", "The note type name (If it doesn't exist it will be created).", false, "Timeline", "", 2 )]
    public partial class BulkUpdate : RockBlock
    {
        #region Fields

        DateTime _gradeTransitionDate = new DateTime( RockDateTime.Today.Year, 6, 1 );

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the individuals.
        /// </summary>
        /// <value>
        /// The individual ids.
        /// </value>
        protected List<Individual> Individuals
        {
            get 
            { 
                var individuals = ViewState["Individuals"] as List<Individual>;
                if ( individuals == null )
                {
                    individuals = new List<Individual>();
                    ViewState["Individuals"] = individuals;
                }
                return individuals;
            }

            set { ViewState["Individuals"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show all individuals].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show all individuals]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowAllIndividuals
        {
            get { return ViewState["ShowAllIndividuals"] as bool? ?? false; }
            set { ViewState["ShowAllIndividuals"] = value; }
        }
            
        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ddlTitle.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ), true );
            ddlStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ), true );
            ddlMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ), true );
            ddlSuffix.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ), true );
            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ), true );
            ddlInactiveReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ), true );
            ddlReviewReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_REVIEW_REASON ) ), true );

            DateTime? gradeTransitionDate = GlobalAttributesCache.Read().GetValue( "GradeTransitionDate" ).AsDateTime();
            if ( gradeTransitionDate.HasValue )
            {
                _gradeTransitionDate = gradeTransitionDate.Value;
            }

            ddlGrade.Items.Clear();
            ddlGrade.Items.Add( new ListItem( "", "" ) );
            ddlGrade.Items.Add( new ListItem( "K", "0" ) );
            ddlGrade.Items.Add( new ListItem( "1st", "1" ) );
            ddlGrade.Items.Add( new ListItem( "2nd", "2" ) );
            ddlGrade.Items.Add( new ListItem( "3rd", "3" ) );
            ddlGrade.Items.Add( new ListItem( "4th", "4" ) );
            ddlGrade.Items.Add( new ListItem( "5th", "5" ) );
            ddlGrade.Items.Add( new ListItem( "6th", "6" ) );
            ddlGrade.Items.Add( new ListItem( "7th", "7" ) );
            ddlGrade.Items.Add( new ListItem( "8th", "8" ) );
            ddlGrade.Items.Add( new ListItem( "9th", "9" ) );
            ddlGrade.Items.Add( new ListItem( "10th", "10" ) );
            ddlGrade.Items.Add( new ListItem( "11th", "11" ) );
            ddlGrade.Items.Add( new ListItem( "12th", "12" ) );

            int gradeFactorReactor = ( RockDateTime.Now < _gradeTransitionDate ) ? 12 : 13;

            string script = string.Format( @"
    $('#{0}').change(function(){{
        if ($(this).val() == '') {{
            $('#{1}').val('');
        }} else {{
            $('#{1}').val( {2} + ( {3} - parseInt( $(this).val() ) ) );
        }} 
    }});

    $('#{1}').change(function(){{
        if ($(this).val() == '') {{
            $('#{0}').val('');
        }} else {{
            var grade = {3} - ( parseInt( $(this).val() ) - {4} );
            if (grade >= 0 && grade <= 12) {{
                $('#{0}').val(grade.toString());
            }} else {{
                $('#{0}').val('');
            }}
        }}
    }});

", ddlGrade.ClientID, ypGraduation.ClientID, _gradeTransitionDate.Year, gradeFactorReactor, RockDateTime.Now.Year );
            ScriptManager.RegisterStartupScript( ddlGrade, ddlGrade.GetType(), "grade-selection-" + BlockId.ToString(), script, true );

            script = @"
    $('a.remove-all-individuals').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to remove all of the individuals from this update?', function (result) {
            if (result) {
                eval(e.target.href);
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( lbRemoveAllIndividuals, lbRemoveAllIndividuals.GetType(), "confirm-remove-all-" + BlockId.ToString(), script, true );

            bddlGroupAction.SelectedValue = "Add";
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
                var campusi = new CampusService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToList();
                cpCampus.Campuses = campusi;
                cpCampus.Required = false;

                ShowAllIndividuals = false;
                ShowDetail();
            }
            else
            {
                BuildAttributes( false );
                BuildGroupControls( false );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            BindIndividuals();
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
        /// Handles the Click event of the btnComplete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnComplete_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var ids = Individuals.Select( i => i.PersonId ).ToList();

                var personEntityType = EntityTypeCache.Read( "Rock.Model.Person" );
                if ( personEntityType != null )
                {
                    int personEntityTypeId = personEntityType.Id;

                    var people = personService.Queryable().Where( p => ids.Contains( p.Id ) ).ToList();

                    #region Individual Details Updates

                    int? newTitleId = ddlTitle.SelectedValueAsInt();
                    int? newSuffixId = ddlSuffix.SelectedValueAsInt();
                    int? newConnectionStatusId = ddlStatus.SelectedValueAsInt();
                    int? newRecordStatusId = ddlRecordStatus.SelectedValueAsInt();
                    int? newInactiveReasonId = ddlInactiveReason.SelectedValueAsInt();
                    string newInactiveReasonNote = tbInactiveReasonNote.Text;
                    Gender? newGender = ddlGender.SelectedValue.ConvertToEnumOrNull<Gender>();

                    int? newMaritalStatusId = ddlMaritalStatus.SelectedValueAsInt();

                    DateTime? newGraduationDate = null;
                    if ( ypGraduation.SelectedYear.HasValue )
                    {
                        newGraduationDate = new DateTime( ypGraduation.SelectedYear.Value, _gradeTransitionDate.Month, _gradeTransitionDate.Day );
                    }

                    int? newCampusId = cpCampus.SelectedCampusId;
                    bool? newEmailActive = null;
                    if ( !string.IsNullOrWhiteSpace( ddlIsEmailActive.SelectedValue ) )
                    {
                        newEmailActive = ddlIsEmailActive.SelectedValue == "Active";
                    }
                    EmailPreference? newEmailPreference = ddlEmailPreference.SelectedValue.ConvertToEnumOrNull<EmailPreference>();
                    string newEmailNote = tbEmailNote.Text;
                    bool? newFollow = null;
                    if ( !string.IsNullOrWhiteSpace( ddlFollow.SelectedValue ) )
                    {
                        newFollow = ddlFollow.SelectedValue == "Add";
                    }
                    int? newReviewReason = ddlReviewReason.SelectedValueAsInt();
                    string newSystemNote = tbSystemNote.Text;
                    string newReviewReasonNote = tbReviewReasonNote.Text;

                    var allChanges = new Dictionary<int, List<string>>();

                    foreach ( var person in people )
                    {
                        var changes = new List<string>();
                        allChanges.Add( person.Id, changes );

                        if ( newTitleId.HasValue )
                        {
                            History.EvaluateChange( changes, "Title", DefinedValueCache.GetName( person.TitleValueId ), DefinedValueCache.GetName( newTitleId ) );
                            person.TitleValueId = newTitleId;
                        }

                        if ( newSuffixId.HasValue )
                        {
                            History.EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( person.SuffixValueId ), DefinedValueCache.GetName( newSuffixId ) );
                            person.SuffixValueId = newSuffixId;
                        }

                        if ( newConnectionStatusId.HasValue )
                        {
                            History.EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( person.ConnectionStatusValueId ), DefinedValueCache.GetName( newConnectionStatusId ) );
                            person.ConnectionStatusValueId = newConnectionStatusId;
                        }

                        if ( newRecordStatusId.HasValue )
                        {
                            History.EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( person.RecordStatusValueId ), DefinedValueCache.GetName( newRecordStatusId ) );
                            person.RecordStatusValueId = newRecordStatusId;
                        }

                        if ( newInactiveReasonId.HasValue )
                        {
                            History.EvaluateChange( changes, "Inactive Reason", DefinedValueCache.GetName( person.RecordStatusReasonValueId ), DefinedValueCache.GetName( newInactiveReasonId ) );
                            person.RecordStatusReasonValueId = newInactiveReasonId;
                        }

                        if ( !string.IsNullOrWhiteSpace( newInactiveReasonNote ) )
                        {
                            History.EvaluateChange( changes, "Inactive Reason Note", person.InactiveReasonNote, newInactiveReasonNote );
                            person.InactiveReasonNote = newInactiveReasonNote;
                        }

                        if ( newGender.HasValue )
                        {
                            History.EvaluateChange( changes, "Gender", person.Gender, newGender.Value );
                            person.Gender = newGender.Value;
                        }

                        if ( newMaritalStatusId.HasValue )
                        {
                            History.EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( newMaritalStatusId ) );
                            person.MaritalStatusValueId = newMaritalStatusId;
                        }

                        if ( newGraduationDate.HasValue )
                        {
                            History.EvaluateChange( changes, "Graduation Date", person.GraduationDate, newGraduationDate );
                            person.GraduationDate = newGraduationDate;
                        }

                        if ( newEmailActive.HasValue )
                        {
                            History.EvaluateChange( changes, "Email Is Active", person.IsEmailActive ?? true, newEmailActive.Value );
                            person.IsEmailActive = newEmailActive;
                        }

                        if ( newEmailPreference.HasValue )
                        {
                            History.EvaluateChange( changes, "Email Preference", person.EmailPreference, newEmailPreference );
                            person.EmailPreference = newEmailPreference.Value;
                        }

                        if ( !string.IsNullOrWhiteSpace( newEmailNote ) )
                        {
                            History.EvaluateChange( changes, "Email Note", person.EmailNote, newEmailNote );
                            person.EmailNote = newEmailNote;
                        }

                        if ( !string.IsNullOrWhiteSpace( newSystemNote ) )
                        {
                            History.EvaluateChange( changes, "System Note", person.SystemNote, newSystemNote );
                            person.SystemNote = newSystemNote;
                        }

                        if ( newReviewReason.HasValue )
                        {
                            History.EvaluateChange( changes, "Review Reason", DefinedValueCache.GetName( person.ReviewReasonValueId ), DefinedValueCache.GetName( newReviewReason ) );
                            person.ReviewReasonValueId = newReviewReason;
                        }

                        if ( !string.IsNullOrWhiteSpace( newReviewReasonNote ) )
                        {
                            History.EvaluateChange( changes, "Review Reason Note", person.ReviewReasonNote, newReviewReasonNote );
                            person.ReviewReasonNote = newReviewReasonNote;
                        }
                    }

                    // Update following
                    if ( newFollow.HasValue && CurrentPersonAlias != null )
                    {
                        var followingService = new FollowingService( rockContext );
                        if ( newFollow.Value )
                        {
                            var alreadyFollingIds = followingService.Queryable()
                                .Where( f =>
                                    f.EntityTypeId == personEntityTypeId &&
                                    f.PersonAlias.Id == CurrentPersonAlias.Id )
                                .Select( f => f.EntityId )
                                .Distinct()
                                .ToList();
                            foreach ( int id in ids.Where( id => !alreadyFollingIds.Contains( id ) ) )
                            {
                                var following = new Following
                                {
                                    EntityTypeId = personEntityTypeId,
                                    EntityId = id,
                                    PersonAliasId = CurrentPersonAlias.Id
                                };
                                followingService.Add( following );
                            }
                        }
                        else
                        {
                            foreach ( var following in followingService.Queryable()
                                .Where( f =>
                                    f.EntityTypeId == personEntityTypeId &&
                                    ids.Contains( f.EntityId ) &&
                                    f.PersonAlias.Id == CurrentPersonAlias.Id ) )
                            {
                                followingService.Delete( following );
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
                            pw = phAttributesCol2.FindControl( controlId) as PanelWidget;
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
                                    if ( attributeControl != null )
                                    {
                                        string newValue = attributeCache.FieldType.Field.GetEditValue( attributeControl, attributeCache.QualifierValues );
                                        if (!string.IsNullOrWhiteSpace(newValue))
                                        {
                                            attributes.Add( attributeCache);
                                            attributeValues.Add( attributeCache.Id, newValue );
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (attributes.Any())
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

                        var noteTypeService = new NoteTypeService( rockContext );
                        var noteService = new NoteService( rockContext );

                        string noteTypeName = GetAttributeValue( "NoteType" );
                        var noteType = noteTypeService.Get( personEntityTypeId, noteTypeName );

                        if ( noteType != null )
                        {
                            var notes = new List<Note>();

                            foreach ( int id in ids )
                            {
                                var note = new Note();
                                note.IsSystem = false;
                                note.EntityId = id;
                                note.Caption = isPrivate ? "You - Personal Note" : string.Empty;
                                note.Text = tbNote.Text;
                                note.IsAlert = cbIsAlert.Checked;
                                note.NoteType = noteType;

                                notes.Add( note );
                                noteService.Add( note );
                            }

                            rockContext.WrapTransaction( () =>
                            {
                                rockContext.SaveChanges();
                                foreach( var note in notes)
                                {
                                    note.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                                    if ( isPrivate)
                                    {
                                        note.MakePrivate( Authorization.VIEW, CurrentPerson, rockContext );
                                    }
                                }
                            } );

                        }
                    }

                    #endregion


                }

                string message = string.Format("{0} {1} succesfully updated!",
                    ids.Count().ToString("N0"), (ids.Count() > 1 ? "people were" : "person was") );
                ShowResult( message );
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
        /// Handles the SelectionChanged event of the bddlGroupAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlGroupAction_SelectionChanged( object sender, EventArgs e )
        {
            BuildGroupControls( true );
        }

        /// <summary>
        /// Handles the SelectItem event of the gpGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpGroup_SelectItem( object sender, EventArgs e )
        {
            BuildGroupControls( true );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void ShowDetail ()
        {
            Individuals.Clear();
            BuildAttributes( true );
        }

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

        private void BuildAttributes( bool setValues )
        {
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

            int categoryIndex = 0;
            foreach( var category in selectedCategories.OrderBy( c => c.Name ) )
            {
                var pw = new PanelWidget();
                pw.ID = "pwAttributes_" + category.Id.ToString();
                if ( categoryIndex % 2 == 0)
                {
                    phAttributesCol1.Controls.Add( pw );
                }
                else
                {
                    phAttributesCol2.Controls.Add( pw );
                }
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
                        attributeCache.AddControl( pw.Controls, string.Empty, string.Empty, setValues, true, false );
                    }
                }
            }
        }

        private void BuildGroupControls( bool setValues )
        {
            ddlGroupRole.Items.Clear();
            ddlGroupMemberStatus.Items.Clear();
            phAttributes.Controls.Clear();

            if ( bddlGroupAction.SelectedValue == "Remove" )
            {
                ddlGroupMemberStatus.Visible = false;
                ddlGroupRole.Visible = false;
            }
            else
            {
                ddlGroupRole.Visible = true;
                ddlGroupMemberStatus.Visible = true;

                var rockContext = new RockContext();
                Group group = null;

                int? groupId = gpGroup.SelectedValueAsId();
                if ( groupId.HasValue )
                {
                    group = new GroupService( rockContext ).Get( groupId.Value );
                }

                if ( group != null )
                {
                    var groupType = GroupTypeCache.Read( group.GroupTypeId );
                    ddlGroupRole.DataSource = groupType.Roles.OrderBy( r => r.Order ).ToList();
                    ddlGroupRole.DataBind();

                    ddlGroupMemberStatus.Items.Add( new ListItem( "Active", "1" ) );
                    ddlGroupMemberStatus.Items.Add( new ListItem( "Pending", "2" ) );
                    ddlGroupMemberStatus.Items.Add( new ListItem( "Inactive", "0" ) );

                    var groupMember = new GroupMember();
                    groupMember.Group = group;
                    groupMember.GroupId = group.Id;
                    groupMember.LoadAttributes( rockContext );
                    Rock.Attribute.Helper.AddEditControls( groupMember, phAttributes, setValues, "", true );
                }
                else
                {
                    ddlGroupRole.Items.Add( new ListItem( string.Empty, string.Empty ) );
                    ddlGroupMemberStatus.Items.Add( new ListItem( string.Empty, string.Empty ) );
                }
            }
        }

        /// <summary>
        /// Shows the result.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowResult( string message )
        {
            pnlEntry.Visible = false;

            nbResult.Text = message;

            pnlResult.Visible = true;
        }

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

        }

        #endregion

    }
}
