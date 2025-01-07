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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Transactions;
using System.Web;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.IO;
using Rock.Tasks;
using Rock.Utility;
using Rock.RealTime.Topics;
using Rock.RealTime;
using Rock.Logging;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Bulk Update" )]
    [Category( "CRM" )]
    [Description( "Used for updating information about several individuals at once." )]

    [SecurityAction( SecurityActionKey.EditConnectionStatus, "The roles and/or users that can edit the connection status for the selected persons." )]
    [SecurityAction( SecurityActionKey.EditRecordStatus, "The roles and/or users that can edit the record status for the selected persons." )]

    #region Block Attributes

    [AttributeCategoryField(
        "Attribute Categories",
        Key = AttributeKey.AttributeCategories,
        Description = "The person attribute categories to display and allow bulk updating",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Order = 0 )]

    [IntegerField(
        "Display Count",
        Key = AttributeKey.DisplayCount,
        Description = "The initial number of individuals to display prior to expanding list",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 1 )]

    [WorkflowTypeField(
        "Workflow Types",
        Key = AttributeKey.WorkflowTypes,
        Description = "The workflows to make available for bulk updating.",
        AllowMultiple = true,
        IsRequired = false,
        Order = 2 )]

    [IntegerField(
        "Task Count",
        Key = AttributeKey.TaskCount,
        Description = "The number of concurrent tasks to use when performing updates. If left blank then it will be determined automatically.",
        DefaultIntegerValue = 0,
        IsRequired = false,
        Order = 3 )]

    [IntegerField(
        "Batch Size",
        Key = AttributeKey.BatchSize,
        Description = "The maximum number of items in each processing batch. If not specified, this value will be automatically determined.",
        DefaultIntegerValue = 0,
        IsRequired = false,
        Order = 4 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.BULK_UPDATE )]
    public partial class BulkUpdate : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys for block attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string AttributeCategories = "AttributeCategories";
            public const string DisplayCount = "DisplayCount";
            public const string WorkflowTypes = "WorkflowTypes";
            public const string TaskCount = "TaskCount";
            public const string BatchSize = "BatchSize";
        }

        #endregion Attribute Keys

        #region Security Actions

        /// <summary>
        /// Keys to use for Security Actions
        /// </summary>
        private static class SecurityActionKey
        {
            public const string EditConnectionStatus = "EditConnectionStatus";
            public const string EditRecordStatus = "EditRecordStatus";
        }

        #endregion Security Actions

        #region Page Parameter Keys

        // <summary>
        // Keys for the page parameters
        // </summary>
        private static class ParameterKey
        {
            public const string Set = "Set";
        }

        #endregion Page Parameter Keys

        #region Fields

        DateTime _gradeTransitionDate = new DateTime( RockDateTime.Today.Year, 6, 1 );
        bool _canEditConnectionStatus = false;
        bool _canEditRecordStatus = true;

        #endregion

        #region Properties

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

            var personEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

            dvpTitle.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ).Id;
            dvpConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ).Id;
            dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ).Id;
            dvpSuffix.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ).Id;
            dvpRecordStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ).Id;
            dvpInactiveReason.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ).Id;
            dvpReviewReason.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_REVIEW_REASON ) ).Id;

            _canEditConnectionStatus = UserCanAdministrate || IsUserAuthorized( SecurityActionKey.EditConnectionStatus );
            dvpConnectionStatus.Visible = _canEditConnectionStatus;

            _canEditRecordStatus = UserCanAdministrate || IsUserAuthorized( SecurityActionKey.EditRecordStatus );
            dvpRecordStatus.Visible = _canEditRecordStatus;

            rlbWorkFlowType.Items.Clear();
            var guidList = GetAttributeValue( AttributeKey.WorkflowTypes ).SplitDelimitedValues().AsGuidList();
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

            new TagService( new RockContext() ).Queryable()
                                            .Where( t =>
                                                        t.EntityTypeId == personEntityTypeId
                                                        && ( t.OwnerPersonAliasId == null || currentPersonAliasIds.Contains( t.OwnerPersonAliasId.Value ) ) )
                                            .OrderByDescending( t => t.OwnerPersonAliasId.HasValue )
                                            .ThenBy( t => t.Name )
                                            .ToList()
                                            .ForEach( t =>
                                            {
                                                if ( t.IsAuthorized( Authorization.TAG, CurrentPerson ) )
                                                {
                                                    ListItem item = new ListItem( t.Name, t.Id.ToString() );
                                                    item.Attributes["OptionGroup"] = t.OwnerPersonAliasId == null ? "Organization Tags" : "Personal Tags";
                                                    ddlTagList.Items.Add( item );
                                                }
                                            } );
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
    $('a.remove-all-individuals').on('click', function( e ){
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
    $( 'label.control-label' ).has( 'span.js-select-item').on('click', function() {{

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

        // Radiobuttons needs special handling
        var radioButtons = formGroup.find(':radio');
        if ( radioButtons.length ) {{
            $(radioButtons).each(function() {{
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

            // Enhanced lists need special handling
            var enhancedList = $(this).parent().find('.chosen-select');
            if (enhancedList.length) {{
                $(enhancedList).trigger('chosen:updated');
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
        if($(this).closest('.form-group.attribute-matrix-editor').length){{
        __doPostBack('{3}', null);
        }}
    }});
", hfSelectedItems.ClientID, ddlGradePicker.ClientID, ypGraduation.ClientID, pnlEntry.ClientID );
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
            var rockContext = new RockContext();

            if ( !Page.IsPostBack )
            {
                AttributeCategories = GetAttributeValue( AttributeKey.AttributeCategories ).SplitDelimitedValues().AsGuidList();
                cpCampus.Campuses = CampusCache.All();
                Individuals = new List<Individual>();
                SelectedFields = new List<string>();

                int? setId = PageParameter( ParameterKey.Set ).AsIntegerOrNull();
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

            base.OnLoad( e );
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
            if ( !Page.IsValid )
            {
                return;
            }

            var processor = this.GetProcessorForCurrentConfiguration( HttpContext.Current.Request );

            var changes = processor.GetPendingActionSummary();

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
            ProcessBulkUpdate();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            AttributeCategories = GetAttributeValue( AttributeKey.AttributeCategories ).SplitDelimitedValues().AsGuidList();
            phAttributesCol1.Controls.Clear();
            phAttributesCol2.Controls.Clear();
            BuildAttributes( new RockContext(), true );
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
        /// Process the bulk update.
        /// </summary>
        private void ProcessBulkUpdate()
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var processor = this.GetProcessorForCurrentConfiguration( HttpContext.Current.Request );
            TaskActivityProgress progress = null;
            
            if ( tapReporter.ConnectionId.IsNotNullOrWhiteSpace() )
            {
                progress = new TaskActivityProgress( RealTimeHelper.GetTopicContext<ITaskActivityProgress>().Clients.Client( tapReporter.ConnectionId ) );
                tapReporter.TaskId = progress.TaskId;
            }

            // Define a background task for the bulk update process, because it may take considerable time.
            var task = new Task( () =>
            {
                // Handle status notifications from the bulk processor.
                processor.StatusUpdated += ( s, args ) =>
                {
                    if ( progress == null )
                    {
                        return;
                    }

                    if ( args.UpdateType == PersonBulkUpdateProcessor.ProcessorStatusUpdateTypeSpecifier.Progress )
                    {
                        // Progress Update
                        progress.ReportProgressUpdate( args.ProcessedCount, args.TotalCount, $"{args.ProcessedCount}/{args.TotalCount}" );
                    }
                    else if ( args.UpdateType == PersonBulkUpdateProcessor.ProcessorStatusUpdateTypeSpecifier.Error )
                    {
                        // Error Message
                        progress.StopTask( args.StatusMessage, new string[] { "1 or more errors occurred." } );
                    }
                    else if ( args.UpdateType == PersonBulkUpdateProcessor.ProcessorStatusUpdateTypeSpecifier.Warning )
                    {
                        // Warning Message
                        progress.StopTask( args.StatusMessage, new string[] { "1 or more warnings occurred." } );
                    }
                    else
                    {
                        // Status Update
                        progress.StopTask( args.StatusMessage );
                    }
                };

                // Wait for the browser to finish loading.
                Task.Delay( 1000 ).Wait();

                processor.Process();
            } );

            pnlConfirm.Visible = false;
            tapReporter.Visible = progress != null;
            nbTapReportFailed.Visible = progress == null;

            // Start the background processing task and complete this request.
            // The task will continue to run until complete, delivering client
            // status notifications via the RealTime topic.
            task.Start();
        }

        /// <summary>
        /// Creates a new instance of the bulk update processor configured with the current page settings.
        /// </summary>
        /// <returns></returns>
        private PersonBulkUpdateProcessor GetProcessorForCurrentConfiguration( HttpRequest request )
        {
            // Get an identifier for the current client request.
            string clientId = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if ( string.IsNullOrWhiteSpace( clientId ) )
            {
                clientId = request.ServerVariables["REMOTE_ADDR"];
            }

            var processor = new PersonBulkUpdateProcessor();

            processor.InstanceId = clientId;

            processor.TaskCount = GetAttributeValue( AttributeKey.TaskCount ).AsInteger();
            processor.BatchSize = GetAttributeValue( AttributeKey.BatchSize ).AsInteger();

            // Set the list of individuals to process.
            processor.PersonIdList = this.Individuals.Select( x => x.PersonId ).ToList();

            // Get Individual Settings
            processor.UpdateTitleValueId = dvpTitle.SelectedValueAsInt();
            processor.UpdateSuffixValueId = dvpSuffix.SelectedValueAsInt();

            if ( _canEditConnectionStatus )
            {
                processor.UpdateConnectionStatusValueId = dvpConnectionStatus.SelectedValueAsInt();
            }

            if ( _canEditRecordStatus )
            {
                processor.UpdateRecordStatusValueId = dvpRecordStatus.SelectedValueAsInt();
                processor.UpdateInactiveReasonId = dvpInactiveReason.SelectedValueAsInt();
                processor.UpdateInactiveReasonNote = tbInactiveReasonNote.Text;
            }

            processor.UpdateGender = ddlGender.SelectedValue.ConvertToEnum<Gender>();
            processor.UpdateMaritalStatusValueId = dvpMaritalStatus.SelectedValueAsInt();

            if ( ypGraduation.SelectedYear.HasValue )
            {
                processor.UpdateGraduationYear = ypGraduation.SelectedYear.Value;
            }

            processor.UpdateCampusId = cpCampus.SelectedCampusId;

            if ( !string.IsNullOrWhiteSpace( ddlIsEmailActive.SelectedValue ) )
            {
                processor.UpdateEmailActive = ( ddlIsEmailActive.SelectedValue == "Active" );
            }

            processor.UpdateCommunicationPreference = ddlCommunicationPreference.SelectedValueAsEnumOrNull<CommunicationType>();
            processor.UpdateEmailPreference = ddlEmailPreference.SelectedValue.ConvertToEnumOrNull<EmailPreference>();

            processor.UpdateEmailNote = tbEmailNote.Text;

            processor.UpdateReviewReasonValueId = dvpReviewReason.SelectedValueAsInt();
            processor.UpdateSystemNote = tbSystemNote.Text;
            processor.UpdateReviewReasonNote = tbReviewReasonNote.Text;

            // Get Person Attributes
            var rockContext = new RockContext();

            processor.PersonAttributeCategories = this.AttributeCategories;
            processor.UpdatePersonAttributeValues = this.GetPersonAttributeValueSettings( rockContext );

            // Notes
            if ( ddlNoteType.SelectedItem != null
                 && !string.IsNullOrWhiteSpace( tbNote.Text )
                 && CurrentPerson != null )
            {
                processor.UpdateNoteAction = PersonBulkUpdateProcessor.NoteChangeActionSpecifier.Add;

                processor.UpdateNoteText = tbNote.Text;
                processor.UpdateNoteIsAlert = cbIsAlert.Checked;

                processor.UpdateNoteIsPrivate = cbIsPrivate.Checked;
                processor.UpdateNoteTypeId = ddlNoteType.SelectedValueAsId();
            }

            // Group Membership
            int groupId = gpGroup.SelectedValue.AsInteger();

            processor.UpdateGroupId = groupId;
            processor.UpdateGroupRoleId = ddlGroupRole.SelectedValueAsInt();
            processor.UpdateGroupStatus = ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>();

            var groupAction = ddlGroupAction.SelectedValue;

            processor.UpdateGroupAttributeValues = GetGroupMemberAttributeValues( rockContext, groupId, groupAction );

            if ( groupId > 0 )
            {
                if ( groupAction == "Add" )
                {
                    processor.UpdateGroupAction = PersonBulkUpdateProcessor.GroupChangeActionSpecifier.Add;
                }
                else if ( groupAction == "Remove" )
                {
                    processor.UpdateGroupAction = PersonBulkUpdateProcessor.GroupChangeActionSpecifier.Remove;
                }
                else if ( groupAction == "Update" )
                {
                    processor.UpdateGroupAction = PersonBulkUpdateProcessor.GroupChangeActionSpecifier.Modify;
                }
            }

            // New Following
            if ( SelectedFields.Contains( ddlFollow.ClientID ) )
            {
                var followingAction = ddlFollow.SelectedValue;

                if ( followingAction == "Add" )
                {
                    processor.UpdateFollowingAction = PersonBulkUpdateProcessor.FollowingChangeActionSpecifier.Add;
                }
                else if ( followingAction == "Remove" )
                {
                    processor.UpdateFollowingAction = PersonBulkUpdateProcessor.FollowingChangeActionSpecifier.Remove;
                }

                processor.UpdateFollowingPersonId = this.CurrentPersonId;
            }

            // Add Tag
            if ( !string.IsNullOrWhiteSpace( ddlTagList.SelectedValue ) )
            {
                var tagAction = ddlTagAction.SelectedValue;

                if ( tagAction == "Add" )
                {
                    processor.UpdateTagAction = PersonBulkUpdateProcessor.TagChangeActionSpecifier.Add;
                }
                else if ( tagAction == "Remove" )
                {
                    processor.UpdateTagAction = PersonBulkUpdateProcessor.TagChangeActionSpecifier.Remove;
                }
            }

            processor.UpdateTagId = ddlTagList.SelectedValue.AsInteger();

            // Set post-processing Workflows
            var selectedWorkflows = rlbWorkFlowType.Items
                                           .Cast<ListItem>()
                                           .Where( x => x.Selected )
                                           .Select( x => x.Value )
                                           .ToList();

            processor.PostUpdateWorkflowIdList = selectedWorkflows;

            // Field Selections
            processor.SelectedFields = new List<string>();

            foreach ( var fieldControlId in this.SelectedFields )
            {
                var ctl = FindControlByClientId( this.Page, fieldControlId );

                if ( ctl != null )
                {
                    processor.SelectedFields.Add( ctl.ID );
                }
            }

            processor.CurrentPerson = this.CurrentPerson;

            return processor;
        }

        private Dictionary<string, string> GetPersonAttributeValueSettings( RockContext rockContext )
        {
            var newPersonAttributeValues = new Dictionary<string, string>();

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

                                newPersonAttributeValues.Add( attributeCache.Key, newValue );
                            }
                        }
                    }
                }
            }

            return newPersonAttributeValues;
        }

        private Dictionary<string, string> GetGroupMemberAttributeValues( RockContext rockContext, int? groupId, string action )
        {
            var selectedGroupAttributeValues = new Dictionary<string, string>();

            if ( groupId.HasValue )
            {
                var group = new GroupService( rockContext ).Get( groupId.Value );

                if ( group != null )
                {
                    // Get the attribute values updated
                    var gm = new GroupMember();
                    gm.Group = group;
                    gm.GroupId = group.Id;
                    gm.LoadAttributes( rockContext );
                    var selectedGroupAttributes = new List<AttributeCache>();

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
                }
            }

            return selectedGroupAttributeValues;
        }

        /// <summary>
        /// Find a control instance recursively by the unique ClientId.
        /// </summary>
        /// <param name="rootControl"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        private Control FindControlByClientId( Control rootControl, string clientId )
        {
            if ( rootControl.ClientID == clientId )
            {
                return rootControl;
            }

            foreach ( Control controlToSearch in rootControl.Controls )
            {
                var controlToReturn = FindControlByClientId( controlToSearch, clientId );

                if ( controlToReturn != null )
                {
                    return controlToReturn;
                }
            }

            return null;
        }

        /// <summary>
        /// Binds the individuals.
        /// </summary>
        private void BindIndividuals()
        {
            int individualCount = Individuals.Count();
            lNumIndividuals.Text = individualCount.ToString( "N0" ) +
                ( individualCount == 1 ? " Person" : " People" );

            // Reset the PersonPicker control selection.
            ppAddPerson.SetValue( null );
            ppAddPerson.PersonName = "Add Person";

            int displayCount = int.MaxValue;

            if ( !ShowAllIndividuals )
            {
                int.TryParse( GetAttributeValue( AttributeKey.DisplayCount ), out displayCount );
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
            if ( canEdit )
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
            nbGroupMessage.Visible = false;
            var rockContext = new RockContext();
            Group group = null;

            int? groupId = gpGroup.SelectedValueAsId();
            if ( groupId.HasValue )
            {
                group = new GroupService( rockContext ).Get( groupId.Value );
            }

            string action = ddlGroupAction.SelectedValue;

            // If the person is not authorized to update/edit the group members...
            if ( group != null && !( group.IsAuthorized( Authorization.EDIT, CurrentPerson ) || group.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson ) ) )
            {
                nbGroupMessage.Visible = true;
                nbGroupMessage.Text = $"You are not authorized to {action.ToLowerInvariant()} members for {group.Name}";
                gpGroup.SetValue( null );
                ddlGroupMemberStatus.Visible = false;
                ddlGroupRole.Visible = false;
                return;
            }

            if ( action == "Remove" )
            {
                ddlGroupMemberStatus.Visible = false;
                ddlGroupRole.Visible = false;
            }
            else
            {
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
                    if ( action == "Update" && !( control is RockCheckBox ) && !( control is PersonPicker ) && !( control is ItemPicker ) )
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

        #region Bulk Update Processor

        /// <summary>
        /// Processes bulk update actions for a set of Person records.
        /// </summary>
        /// <remarks>
        /// Last Updated: [2020-05-01] DL
        /// </remarks>
        private class PersonBulkUpdateProcessor
        {
            #region Enumerations and Constants

            public static class FieldNames
            {
                public const string Title = "dvpTitle";
                public const string Suffix = "dvpSuffix";
                public const string ConnectionStatus = "dvpConnectionStatus";
                public const string RecordStatus = "dvpRecordStatus";
                public const string Gender = "ddlGender";
                public const string GroupRole = "ddlGroupRole";
                public const string MaritalStatus = "dvpMaritalStatus";
                public const string GraduationYear = "ddlGradePicker";
                public const string EmailIsActive = "ddlIsEmailActive";
                public const string CommunicationPreference = "ddlCommunicationPreference";
                public const string EmailPreference = "ddlEmailPreference";
                public const string GroupMemberStatus = "ddlGroupMemberStatus";
                public const string EmailNote = "tbEmailNote";
                public const string SystemNote = "tbSystemNote";
                public const string ReviewReason = "dvpReviewReason";
                public const string ReviewReasonNote = "tbReviewReasonNote";
                public const string Campus = "cpCampus";
            }

            public enum ProcessorStatusUpdateTypeSpecifier
            {
                Progress = 0,
                Info = 1,
                Warning = 2,
                Error = 3,
                Summary = 4
            }

            public enum NoteChangeActionSpecifier
            {
                None = 0,
                Add = 1
            }

            public enum TagChangeActionSpecifier
            {
                None = 0,
                Add = 1,
                Remove = 2
            }

            public enum FollowingChangeActionSpecifier
            {
                None = 0,
                Add = 1,
                Remove = 2
            }

            public enum GroupChangeActionSpecifier
            {
                None = 0,
                Add = 1,
                Remove = 2,
                Modify = 3
            }

            #endregion

            #region Events

            public class ProcessorStatusUpdateEventArgs : EventArgs
            {
                public ProcessorStatusUpdateEventArgs()
                {
                    StatusDateTime = RockDateTime.Now;
                }

                public DateTime StatusDateTime;
                public long ProcessedCount;
                public long TotalCount;
                public long ErrorCount;

                public string StatusMessage;
                public string StatusDetail;

                public ProcessorStatusUpdateTypeSpecifier UpdateType = ProcessorStatusUpdateTypeSpecifier.Progress;
            }

            public event EventHandler<ProcessorStatusUpdateEventArgs> StatusUpdated;

            #endregion

            #region Constructors

            public PersonBulkUpdateProcessor()
            {
                this.SelectedFields = new List<string>();
                this.PersonIdList = new List<int>();
                this.PersonAttributeCategories = new List<Guid>();

                UpdatePersonAttributeValues = new Dictionary<string, string>();
                UpdateGroupAttributeValues = new Dictionary<string, string>();

                UpdateGroupAction = GroupChangeActionSpecifier.None;
                UpdateFollowingAction = FollowingChangeActionSpecifier.None;
                UpdateTagAction = TagChangeActionSpecifier.None;
                UpdateNoteAction = NoteChangeActionSpecifier.None;
            }

            #endregion

            #region Fields and Properties

            private readonly static TraceSource _tracer = new TraceSource( "Rock.Crm.BulkUpdate" );

            private int _currentPersonAliasId;
            private Person _currentPerson = null;

            private long _errorCount;
            private int _totalCount = 0;
            private int _processedCount = 0;
            private DateTime? _lastNotified = null;
            private decimal? _lastCompletionPercentage;

            // The number of CPU threads that can be used to process the updates.
            public int TaskCount { get; set; }

            /// <summary>
            /// The maximum size of a processing batch size for maximum number work items assigned to each task.
            /// </summary>
            public int BatchSize { get; set; }


            /// <summary>
            /// The list of unique identifiers for the people who will be targeted by the bulk update operation.
            /// </summary>
            public List<int> PersonIdList { get; set; }

            public List<string> SelectedFields { get; set; }
            public List<Guid> PersonAttributeCategories { get; set; }

            public Person CurrentPerson
            {
                get
                {
                    return _currentPerson;
                }

                set
                {
                    _currentPerson = value;

                    _currentPersonAliasId = ( _currentPerson == null ? 0 : _currentPerson.PrimaryAliasId ?? 0 );
                }
            }

            /// <summary>
            /// Gets a unique identifier for this instance of the processor that can be used for trace and diagnostic purposes.
            /// </summary>
            public string InstanceId { get; set; }

            /// <summary>
            /// The minimum time (measured in ms) that must elapse between subsequent progress notifications.
            /// </summary>
            public int NotificationPeriod { get; set; }

            #region Bulk Update Values

            public int? UpdateTitleValueId { get; set; }
            public int? UpdateSuffixValueId { get; set; }
            public int? UpdateConnectionStatusValueId { get; set; }
            public int? UpdateRecordStatusValueId { get; set; }
            public int? UpdateInactiveReasonId { get; set; }
            public string UpdateInactiveReasonNote { get; set; }
            public Gender UpdateGender { get; set; }
            public int? UpdateMaritalStatusValueId { get; set; }
            public int? UpdateGraduationYear { get; set; }
            public int? UpdateCampusId { get; set; }
            public bool UpdateEmailActive { get; set; }
            public CommunicationType? UpdateCommunicationPreference { get; set; }
            public EmailPreference? UpdateEmailPreference { get; set; }
            public string UpdateEmailNote { get; set; }
            public int? UpdateReviewReasonValueId { get; set; }
            public string UpdateSystemNote { get; set; }
            public string UpdateReviewReasonNote { get; set; }
            public Dictionary<string, string> UpdatePersonAttributeValues { get; set; }
            public NoteChangeActionSpecifier UpdateNoteAction { get; set; }
            public string UpdateNoteText { get; set; }
            public bool UpdateNoteIsAlert { get; set; }
            public bool UpdateNoteIsPrivate { get; set; }
            public int? UpdateNoteTypeId { get; set; }
            public GroupChangeActionSpecifier UpdateGroupAction { get; set; }
            public int? UpdateGroupId { get; set; }
            public int? UpdateGroupRoleId { get; set; }
            public GroupMemberStatus UpdateGroupStatus { get; set; }
            public Dictionary<string, string> UpdateGroupAttributeValues { get; set; }

            public FollowingChangeActionSpecifier UpdateFollowingAction { get; set; }

            /// <summary>
            /// The identifier of the Person who will be added as the Following target.
            /// </summary>
            public int? UpdateFollowingPersonId { get; set; }

            public TagChangeActionSpecifier UpdateTagAction { get; set; }
            public int UpdateTagId { get; set; }

            #endregion

            /// <summary>
            /// A list of identifiers of workflows that should be executed for each person after the update.
            /// </summary>
            public List<string> PostUpdateWorkflowIdList;

            /// <summary>
            /// Set an update value for the specified field.
            /// </summary>
            /// <param name="fieldName"></param>
            /// <param name="newValue"></param>
            public void SetNewFieldValue( string fieldName, object newValue )
            {
                switch ( fieldName )
                {
                    case FieldNames.Campus:
                        this.UpdateCampusId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.CommunicationPreference:
                        this.UpdateCommunicationPreference = newValue.ToStringSafe().ConvertToEnum<CommunicationType>( null );
                        break;
                    case FieldNames.ConnectionStatus:
                        this.UpdateConnectionStatusValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.EmailPreference:
                        this.UpdateEmailPreference = newValue.ToStringSafe().ConvertToEnum<EmailPreference>( null );
                        break;
                    case FieldNames.EmailIsActive:
                        this.UpdateEmailActive = newValue.ToStringSafe().AsBoolean();
                        break;
                    case FieldNames.EmailNote:
                        if ( newValue != null )
                        {
                            newValue = newValue.ToString().Trim();
                        }
                        this.UpdateEmailNote = newValue as string;
                        break;
                    case FieldNames.Gender:
                        this.UpdateGender = newValue.ToStringSafe().ConvertToEnum<Gender>( null );
                        break;
                    case FieldNames.GraduationYear:
                        this.UpdateGraduationYear = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.MaritalStatus:
                        this.UpdateMaritalStatusValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.RecordStatus:
                        this.UpdateRecordStatusValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.ReviewReason:
                        this.UpdateReviewReasonValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.ReviewReasonNote:
                        if ( newValue != null )
                        {
                            newValue = newValue.ToString().Trim();
                        }
                        this.UpdateReviewReasonNote = newValue as string;
                        break;
                    case FieldNames.Suffix:
                        this.UpdateSuffixValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;
                    case FieldNames.SystemNote:
                        if ( newValue != null )
                        {
                            newValue = newValue.ToString().Trim();
                        }
                        this.UpdateSystemNote = newValue as string;
                        break;

                    case FieldNames.Title:
                        this.UpdateTitleValueId = newValue.ToStringSafe().AsIntegerOrNull();
                        break;

                    case FieldNames.GroupRole:
                    case FieldNames.GroupMemberStatus:

                    default:
                        throw new Exception( string.Format( "Invalid field name. The field \"{0}\" cannot be resolved.", fieldName ) );
                }

                if ( !this.SelectedFields.Contains( fieldName ) )
                {
                    this.SelectedFields.Add( fieldName );
                }
            }

            #endregion

            #region Trace Output

            private void TraceInformation( string message, params object[] args )
            {
                TraceEvent( TraceEventType.Information, message, args );
            }

            private void TraceVerbose( string message, params object[] args )
            {
                TraceEvent( TraceEventType.Verbose, message, args );
            }

            private void TraceEvent( TraceEventType eventType, string message, params object[] args )
            {
                string prefix;

                if ( this.TaskCount == 1 )
                {
                    prefix = string.Format( "InstanceId={0}", this.InstanceId );
                }
                else
                {
                    prefix = string.Format( "InstanceId={0}, ThreadId={1}", this.InstanceId, Thread.CurrentThread.ManagedThreadId );
                }

                message = string.Format( "{0} || {1}", prefix, string.Format( message, args ) );

                _tracer.TraceEvent( eventType, 0, message );
            }

            #endregion

            #region Status Reporting

            private void SetTaskProgress( int completedCount, int totalCount, string statusMessage = null, string statusDetail = null )
            {
                lock ( _processingQueueLocker )
                {
                    _processedCount = completedCount;
                    _totalCount = totalCount;
                }

                var currentCompletionPercentage = decimal.Divide( _processedCount, _totalCount ) * 100;

                // Only send a progress notification if this is the first update, or work has been completed
                // and the elapsed time is sufficient to warrant an update.
                if ( _lastNotified.HasValue )
                {
                    var timeDiff = RockDateTime.Now - _lastNotified.Value;

                    if ( NotificationPeriod > 0
                         && timeDiff.TotalMilliseconds < NotificationPeriod )
                    {
                        return;
                    }

                    if ( currentCompletionPercentage == _lastCompletionPercentage )
                    {
                        return;
                    }
                }

                TraceVerbose( "Status Update Notification. [Processed={0} of {1}, Errors={2}, Message={3}]", _processedCount, _totalCount, _errorCount, statusMessage );

                _lastNotified = RockDateTime.Now;
                _lastCompletionPercentage = currentCompletionPercentage;

                if ( StatusUpdated != null )
                {
                    var args = new ProcessorStatusUpdateEventArgs
                    {
                        ProcessedCount = _processedCount,
                        TotalCount = _totalCount,
                        ErrorCount = _errorCount,
                        StatusMessage = statusMessage,
                        StatusDetail = statusDetail,
                        UpdateType = ProcessorStatusUpdateTypeSpecifier.Progress
                    };

                    StatusUpdated.Invoke( this, args );
                }
            }

            private void UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier updateType, string statusMessage = null, string statusDetail = null )
            {
                if ( StatusUpdated != null )
                {
                    var args = new ProcessorStatusUpdateEventArgs
                    {
                        ProcessedCount = _processedCount,
                        TotalCount = _totalCount,
                        ErrorCount = _errorCount,
                        StatusMessage = statusMessage,
                        StatusDetail = statusDetail,
                        UpdateType = updateType
                    };

                    StatusUpdated.Invoke( this, args );
                }
            }

            #endregion

            #region Bulk Update Processing

            private static object _processingQueueLocker = new object();

            /// <summary>
            /// Returns a data context that is associated with the current user for audit purposes.
            /// As a side effect, ensures that the HttpContext contains required information about the current user.
            /// </summary>
            /// <returns></returns>
            private RockContext GetDataContextForCurrentUser()
            {
                /* [2020-05-01] DL
                 * The RockContext determines the current user by retrieving the value of HttpContext.Current.Items["CurrentPerson"].
                 * In this component, the RockContext operates in a background thread that does not have access to the HttpContext of the original request.
                 *
                 * The workaround implemented here ensures that a copy of the original HttpRequest is available to the current thread.
                 * A more robust solution would be to add a RockContext.CurrentPerson property that can be set as an override for instances where data access occurs in absence of a HttpRequest.
                 */

                // Set a fake HttpContext for the current thread if it does not have one, and inject the CurrentPerson.
                if ( HttpContext.Current == null )
                {
                    var request = new HttpRequest( "", "http://localhost", "" );
                    var response = new HttpResponse( new StringWriter() );
                    var testHttpContext = new HttpContext( request, response );

                    testHttpContext.Items["CurrentPerson"] = this.CurrentPerson;

                    HttpContext.Current = testHttpContext;
                }

                return new RockContext();
            }

            /// <summary>
            /// Processes the bulk update.
            /// </summary>
            /// <param name="httpContext"></param>
            /// <param name="httpContextItems"></param>
            /// <param name="instanceId"></param>
            public void Process()
            {
                if ( string.IsNullOrWhiteSpace( this.InstanceId ) )
                {
                    this.InstanceId = Guid.NewGuid().ToString();
                }

                TraceInformation( "Process started." );

                var actionSummary = this.GetPendingActionSummary();

                foreach ( var action in this.GetPendingActionSummary() )
                {
                    TraceInformation( "Pending Action: {0}", action );
                }

                var startTime = RockDateTime.Now;

                ValidateCanProcess();

                var individuals = PersonIdList.ToList();

                _totalCount = individuals.Count;
                _processedCount = 0;
                _errorCount = 0;

                _lastNotified = null;
                _lastCompletionPercentage = null;

                // Determine the number of tasks to use.
                int taskCount = this.TaskCount;

                if ( taskCount > 64 )
                {
                    // Prevent the user from doing too much damage.
                    taskCount = 64;
                }
                else if ( taskCount < 1 )
                {
                    taskCount = Environment.ProcessorCount;
                }

                TraceInformation( "Processing initialized. [ItemCount={0}, MaximumThreadCount={1}, BatchSize={2}]", _totalCount, taskCount, this.BatchSize );

                try
                {
                    string finalStatus = null;
                    bool hasErrors = false;

                    SetTaskProgress( 0, _totalCount );

                    if ( individuals.Any() )
                    {
                        var options = new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = taskCount
                        };

                        // Partition the collection into batches.
                        var batchSize = this.BatchSize;

                        OrderablePartitioner<Tuple<int, int>> partitioner;

                        if ( batchSize == 0 )
                        {
                            // Let the scheduler determine the batch size.
                            partitioner = Partitioner.Create( 0, individuals.Count );
                        }
                        else
                        {
                            partitioner = Partitioner.Create( 0, individuals.Count, batchSize );
                        }

                        var updateExceptions = new ConcurrentQueue<Exception>();

                        Action<Tuple<int, int>, ParallelLoopState> processItemBatchDelegate = ( Tuple<int, int> range, ParallelLoopState loopState ) =>
                        {
                            try
                            {
                                TraceVerbose( "Worker task started." );

                                var firstIndex = range.Item1;
                                var lastIndex = range.Item2 - 1;
                                var itemCount = lastIndex - firstIndex + 1;

                                TraceVerbose( "Processing work items... [From={0}, To={1}, Count={2}]", firstIndex + 1, lastIndex + 1, itemCount );

                                var personIdList = individuals.Skip( firstIndex ).Take( itemCount ).ToList();

                                ProcessIndividuals( personIdList );

                                SetTaskProgress( _processedCount + itemCount, _totalCount );

                                TraceVerbose( "Worker task completed." );
                            }
                            catch ( Exception ex )
                            {
                                updateExceptions.Enqueue( ex );
                            }
                        };

                        var result = Parallel.ForEach( partitioner, options, processItemBatchDelegate );

                        if ( !result.IsCompleted )
                        {
                            finalStatus = string.Join( "<br>", updateExceptions.Select( w => w.Message.EncodeHtml() ) );

                            hasErrors = true;
                        }
                    }

                    SetTaskProgress( _totalCount, _totalCount );

                    var elapsedTime = RockDateTime.Now.Subtract( startTime );

                    if ( hasErrors )
                    {
                        UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier.Error, finalStatus, "alert-danger" );
                    }
                    else
                    {
                        if ( _errorCount == 0 )
                        {
                            finalStatus = string.Format( "{0} {1} successfully updated. ({2:0.0}s)",
                                PersonIdList.Count().ToString( "N0" ), ( PersonIdList.Count() > 1 ? "people were" : "person was" ),
                                elapsedTime.TotalSeconds );

                            UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier.Summary, finalStatus.EncodeHtml(), "alert-success" );
                        }
                        else
                        {
                            finalStatus = string.Format( "{0} {1} updated with {2} error(s). ({3:0.0}s)",
                                PersonIdList.Count().ToString( "N0" ), ( PersonIdList.Count() > 1 ? "people were" : "person was" ),
                                _errorCount,
                                elapsedTime.TotalSeconds );

                            UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier.Warning, finalStatus.EncodeHtml(), "alert-warning" );
                        }
                    }

                    TraceInformation( "Result: {0}", finalStatus );
                }
                catch ( Exception ex )
                {
                    string status = ex.Message;

                    UpdateTaskStatus( ProcessorStatusUpdateTypeSpecifier.Error, status.EncodeHtml(), "error" );
                }

                TraceInformation( "Process completed." );

            }

            private void ValidateCanProcess()
            {
                if ( PersonIdList == null || !PersonIdList.Any() )
                {
                    throw new Exception( "PersonIdList must contain one or more items." );
                }

                bool hasUpdateActions = false;

                hasUpdateActions = hasUpdateActions || ( this.SelectedFields != null && this.SelectedFields.Any() );
                hasUpdateActions = hasUpdateActions || ( this.UpdateGroupAction != GroupChangeActionSpecifier.None );
                hasUpdateActions = hasUpdateActions || ( this.UpdateFollowingAction != FollowingChangeActionSpecifier.None );
                hasUpdateActions = hasUpdateActions || ( this.UpdateTagAction != TagChangeActionSpecifier.None );
                hasUpdateActions = hasUpdateActions || ( this.UpdateNoteAction != NoteChangeActionSpecifier.None );
                hasUpdateActions = hasUpdateActions || ( this.UpdatePersonAttributeValues != null && this.UpdatePersonAttributeValues.Any() );
                hasUpdateActions = hasUpdateActions || ( this.UpdateGroupAttributeValues != null && this.UpdateGroupAttributeValues.Any() );
                hasUpdateActions = hasUpdateActions || ( this.PostUpdateWorkflowIdList != null && this.PostUpdateWorkflowIdList.Any() );

                if ( !hasUpdateActions )
                {
                    throw new Exception( "SelectedFields must contain one or more items." );
                }

                if ( this.UpdatePersonAttributeValues != null && this.UpdatePersonAttributeValues.Any() )
                {
                    if ( this.PersonAttributeCategories == null || !this.PersonAttributeCategories.Any() )
                    {
                        // This requirement is arbitrary and should be removed.
                        throw new Exception( "PersonAttributeValues filter requires PersonAttributeCategories to be populated with corresponding categories." );
                    }
                }
            }

            /// <summary>
            /// Process the given individuals. This is used to be able to run smaller batches. This provides
            /// a huge boost to performance when dealing with large numbers of people.
            /// </summary>
            /// <param name="personIdList">The list of individuals to process in this batch.</param>
            private void ProcessIndividuals( List<int> personIdList )
            {
                var rockContext = this.GetDataContextForCurrentUser();

                var personService = new PersonService( rockContext );

                var ids = personIdList.ToList();

                #region Individual Details Updates

                int inactiveStatusId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;

                var people = personService.Queryable( true ).Where( p => ids.Contains( p.Id ) ).ToList();

                foreach ( var person in people )
                {
                    if ( SelectedFields.Contains( FieldNames.Title ) )
                    {
                        person.TitleValueId = UpdateTitleValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.Suffix ) )
                    {
                        person.SuffixValueId = UpdateSuffixValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.ConnectionStatus ) )
                    {
                        person.ConnectionStatusValueId = UpdateConnectionStatusValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.RecordStatus ) )
                    {
                        person.RecordStatusValueId = UpdateRecordStatusValueId;

                        if ( UpdateRecordStatusValueId.HasValue && UpdateRecordStatusValueId.Value == inactiveStatusId )
                        {
                            person.RecordStatusReasonValueId = UpdateInactiveReasonId;

                            if ( !string.IsNullOrWhiteSpace( UpdateInactiveReasonNote ) )
                            {
                                person.InactiveReasonNote = UpdateInactiveReasonNote;
                            }
                        }
                    }

                    if ( SelectedFields.Contains( FieldNames.Gender ) )
                    {
                        person.Gender = UpdateGender;
                    }

                    if ( SelectedFields.Contains( FieldNames.MaritalStatus ) )
                    {
                        person.MaritalStatusValueId = UpdateMaritalStatusValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.GraduationYear ) )
                    {
                        person.GraduationYear = UpdateGraduationYear;
                    }

                    if ( SelectedFields.Contains( FieldNames.EmailIsActive ) )
                    {
                        person.IsEmailActive = UpdateEmailActive;
                    }

                    if ( SelectedFields.Contains( FieldNames.CommunicationPreference ) )
                    {
                        person.CommunicationPreference = UpdateCommunicationPreference.Value;
                    }

                    if ( SelectedFields.Contains( FieldNames.EmailPreference ) )
                    {
                        person.EmailPreference = UpdateEmailPreference.Value;
                    }

                    if ( SelectedFields.Contains( FieldNames.EmailNote ) )
                    {
                        person.EmailNote = UpdateEmailNote;
                    }

                    if ( SelectedFields.Contains( FieldNames.SystemNote ) )
                    {
                        person.SystemNote = UpdateSystemNote;
                    }

                    if ( SelectedFields.Contains( FieldNames.ReviewReason ) )
                    {
                        person.ReviewReasonValueId = UpdateReviewReasonValueId;
                    }

                    if ( SelectedFields.Contains( FieldNames.ReviewReasonNote ) )
                    {
                        person.ReviewReasonNote = UpdateReviewReasonNote;
                    }
                }

                if ( SelectedFields.Contains( FieldNames.Campus ) && UpdateCampusId.HasValue )
                {
                    int campusId = UpdateCampusId.Value;

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
                if ( this.UpdateFollowingAction != FollowingChangeActionSpecifier.None )
                {
                    var personAliasEntityType = EntityTypeCache.Get( "Rock.Model.PersonAlias" );
                    if ( personAliasEntityType != null )
                    {
                        int personAliasEntityTypeId = personAliasEntityType.Id;

                        var personAliasService = new PersonAliasService( rockContext );
                        var followingService = new FollowingService( rockContext );

                        var followedPerson = personService.Get( this.UpdateFollowingPersonId.GetValueOrDefault() );

                        if ( followedPerson == null )
                        {
                            throw new Exception( "A Following Person must be specified." );
                        }

                        var followedPersonAliasId = followedPerson.PrimaryAliasId.GetValueOrDefault();

                        if ( UpdateFollowingAction == FollowingChangeActionSpecifier.Add )
                        {
                            var paQry = personAliasService.Queryable();

                            var alreadyFollowingIds = followingService.Queryable()
                                .Where( f =>
                                    f.EntityTypeId == personAliasEntityTypeId &&
                                    string.IsNullOrEmpty( f.PurposeKey ) &&
                                    f.PersonAlias.Id == followedPersonAliasId )
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
                                        PersonAliasId = followedPersonAliasId
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
                                    ( f.PurposeKey == null || f.PurposeKey == string.Empty ) &&
                                    paQry.Contains( f.EntityId ) &&
                                    f.PersonAlias.Id == _currentPersonAliasId ) )
                            {
                                followingService.Delete( following );
                            }
                        }
                    }
                }

                rockContext.SaveChanges();

                #endregion

                #region Person Attributes

                if ( this.UpdatePersonAttributeValues != null && this.UpdatePersonAttributeValues.Any() )
                {
                    var selectedCategories = new List<CategoryCache>();

                    foreach ( Guid categoryGuid in PersonAttributeCategories )
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
                        categoryIndex++;

                        var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id, false )
                            .OrderBy( a => a.Order ).ThenBy( a => a.Name );

                        foreach ( var attribute in orderedAttributeList )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, _currentPerson ) )
                            {
                                var attributeCache = AttributeCache.Get( attribute.Id );

                                if ( this.UpdatePersonAttributeValues.ContainsKey( attribute.Key ) )
                                {
                                    attributes.Add( attributeCache );
                                    attributeValues.Add( attributeCache.Id, this.UpdatePersonAttributeValues[attribute.Key] );
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
                                    }
                                }
                            }
                        }
                    }
                }

                rockContext.SaveChanges();

                #endregion

                #region Add Note

                if ( this.UpdateNoteAction != NoteChangeActionSpecifier.None
                     && !string.IsNullOrWhiteSpace( UpdateNoteText )
                     && _currentPerson != null )
                {
                    bool isPrivate = this.UpdateNoteIsPrivate;

                    var noteType = NoteTypeCache.Get( UpdateNoteTypeId.GetValueOrDefault( 0 ) );
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
                            note.Text = UpdateNoteText;
                            note.IsAlert = UpdateNoteIsAlert;
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

                if ( this.UpdateGroupAction != GroupChangeActionSpecifier.None )
                {
                    var group = new GroupService( rockContext ).Get( UpdateGroupId.Value );
                    if ( group != null && ( group.IsAuthorized( Authorization.EDIT, CurrentPerson ) || group.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson ) ) )
                    {
                        var groupMemberService = new GroupMemberService( rockContext );

                        var existingMembersQuery = groupMemberService.Queryable( true ).Include( a => a.Group )
                                                                     .Where( m => m.GroupId == group.Id
                                                                                  && ids.Contains( m.PersonId ) );

                        if ( this.UpdateGroupAction == GroupChangeActionSpecifier.Remove )
                        {
                            var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );

                            var existingIds = existingMembersQuery.Select( gm => gm.Id ).Distinct().ToList();

                            Action<RockContext, List<int>> deleteAction = ( context, items ) =>
                            {
                                // Load the batch of GroupMember items into the context and delete them.
                                groupMemberService = new GroupMemberService( context );

                                var batchGroupMembers = groupMemberService.Queryable( true ).Where( x => items.Contains( x.Id ) ).ToList();

                                GroupMemberHistoricalService groupMemberHistoricalService = new GroupMemberHistoricalService( context );

                                foreach ( GroupMember groupMember in batchGroupMembers )
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
                                            groupMemberService.Archive( groupMember, this._currentPersonAliasId, true );
                                        }
                                        else
                                        {
                                            groupMemberService.Delete( groupMember, true );
                                        }

                                        context.SaveChanges();
                                    }
                                    catch ( Exception ex )
                                    {
                                        ExceptionLogService.LogException( new Exception( string.Format( "Error removing person {0} from group {1}", groupMember.Person.FullName, group.Name ), ex ), null );
                                        Interlocked.Increment( ref _errorCount );
                                    }
                                }
                            };

                            ProcessBatchUpdate( existingIds, this.BatchSize, deleteAction );
                        }
                        else
                        {
                            // Get the attribute values updated
                            var gm = new GroupMember();
                            gm.Group = group;
                            gm.GroupId = group.Id;
                            gm.LoadAttributes( rockContext );
                            var selectedGroupAttributes = new List<AttributeCache>();

                            if ( UpdateGroupAttributeValues != null
                                 && UpdateGroupAttributeValues.Count > 0 )
                            {
                                foreach ( var attributeCache in gm.Attributes.Select( a => a.Value ) )
                                {
                                    if ( UpdateGroupAttributeValues.ContainsKey( attributeCache.Key ) )
                                    {
                                        selectedGroupAttributes.Add( attributeCache );
                                    }
                                }
                            }

                            if ( UpdateGroupAction == GroupChangeActionSpecifier.Add )
                            {
                                if ( UpdateGroupRoleId.HasValue )
                                {
                                    var newGroupMembers = new List<GroupMember>();

                                    var existingMembers = existingMembersQuery
                                        .Select( m => new
                                        {
                                            m.PersonId,
                                            m.GroupRoleId
                                        } ).ToList();

                                    var personKeys = ids.Where( id => !existingMembers.Any( m => m.PersonId == id && m.GroupRoleId == UpdateGroupRoleId.Value ) ).ToList();

                                    Action<RockContext, List<int>> addAction = ( context, items ) =>
                                    {
                                        groupMemberService = new GroupMemberService( context );

                                        foreach ( int id in items )
                                        {
                                            var groupMember = new GroupMember();
                                            groupMember.GroupId = group.Id;
                                            groupMember.GroupRoleId = UpdateGroupRoleId.Value;
                                            groupMember.GroupMemberStatus = UpdateGroupStatus;
                                            groupMember.PersonId = id;

                                            if ( groupMember.IsValidGroupMember( context ) )
                                            {

                                                groupMemberService.Add( groupMember );

                                                newGroupMembers.Add( groupMember );
                                            }
                                            else
                                            {
                                                // Validation errors will get added to the ValidationResults collection.
                                                // Add those results to the log and then move on to the next person.
                                                var validationMessage = string.Join( ",", groupMember.ValidationResults.Select( r => r.ErrorMessage ).ToArray() );
                                                Interlocked.Increment( ref _errorCount );
                                                RockLogger.Log.Information( RockLogDomains.Group, validationMessage );
                                            }
                                        }

                                        context.SaveChanges();
                                    };

                                    ProcessBatchUpdate( personKeys, this.BatchSize, addAction );

                                    if ( selectedGroupAttributes.Any() )
                                    {
                                        foreach ( var groupMember in newGroupMembers )
                                        {
                                            foreach ( var attribute in selectedGroupAttributes )
                                            {
                                                Rock.Attribute.Helper.SaveAttributeValue( groupMember, attribute, UpdateGroupAttributeValues[attribute.Key], rockContext );
                                            }
                                        }
                                    }
                                }
                            }
                            else // Update
                            {
                                if ( SelectedFields.Contains( FieldNames.GroupRole ) && UpdateGroupRoleId.HasValue )
                                {
                                    foreach ( var member in existingMembersQuery.Where( m => m.GroupRoleId != UpdateGroupRoleId.Value ) )
                                    {
                                        if ( !existingMembersQuery.Any( m => m.PersonId == member.PersonId && m.GroupRoleId == UpdateGroupRoleId.Value ) )
                                        {
                                            member.GroupRoleId = UpdateGroupRoleId.Value;
                                        }
                                    }
                                }

                                if ( SelectedFields.Contains( FieldNames.GroupMemberStatus ) )
                                {
                                    foreach ( var member in existingMembersQuery )
                                    {
                                        member.GroupMemberStatus = UpdateGroupStatus;
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
                                                Rock.Attribute.Helper.SaveAttributeValue( groupMember, attribute, UpdateGroupAttributeValues[attribute.Key], context );
                                            }
                                        }

                                        context.SaveChanges();
                                    };

                                    // Process the Attribute updates in batches.
                                    var existingMembers = existingMembersQuery.ToList();

                                    ProcessBatchUpdate( existingMembers, this.BatchSize, updateAction );
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Tag
                var personEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

                if ( this.UpdateTagAction != TagChangeActionSpecifier.None )
                {
                    var tag = new TagService( rockContext ).Get( UpdateTagId );
                    if ( tag != null && tag.IsAuthorized( Rock.Security.Authorization.TAG, _currentPerson ) )
                    {
                        var taggedItemService = new TaggedItemService( rockContext );

                        // get guids of selected individuals
                        var personGuids = new PersonService( rockContext ).Queryable( true )
                                            .Where( p =>
                                                ids.Contains( p.Id ) )
                                            .Select( p => p.Guid )
                                            .ToList();

                        if ( this.UpdateTagAction == TagChangeActionSpecifier.Add )
                        {
                            foreach ( var personGuid in personGuids )
                            {
                                if ( !taggedItemService.Queryable().Where( t => t.TagId == UpdateTagId && t.EntityGuid == personGuid ).Any() )
                                {
                                    TaggedItem taggedItem = new TaggedItem();
                                    taggedItem.TagId = UpdateTagId;
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
                                var taggedPerson = taggedItemService.Queryable().Where( t => t.TagId == UpdateTagId && t.EntityGuid == personGuid ).FirstOrDefault();
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

                if ( PostUpdateWorkflowIdList != null )
                {
                    foreach ( string value in PostUpdateWorkflowIdList )
                    {
                        int? intValue = value.AsIntegerOrNull();
                        if ( intValue.HasValue )
                        {
                            // Queue a transaction to launch workflow
                            var workflowDetails = people.Select( p => new LaunchWorkflowDetails( p ) ).ToList();
                            var launchWorkflowsTxn = new Rock.Transactions.LaunchWorkflowsTransaction( intValue.Value, workflowDetails );
                            launchWorkflowsTxn.InitiatorPersonAliasId = _currentPersonAliasId;
                            launchWorkflowsTxn.Enqueue();
                        }
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

                if ( batchSize <= 0 )
                {
                    batchSize = 50;
                }

                while ( remainingCount > 0 )
                {
                    var batchItems = itemsToProcess.Skip( batchesProcessed * batchSize ).Take( batchSize ).ToList();

                    using ( var batchContext = this.GetDataContextForCurrentUser() )
                    {
                        processingAction.Invoke( batchContext, batchItems );
                    }

                    batchesProcessed++;

                    remainingCount -= batchItems.Count();
                }
            }

            #endregion

            #region Action Summary

            /// <summary>
            /// Gets a summary of the changes that will be made when the processor is executed according to the current settings.
            /// </summary>
            public List<string> GetPendingActionSummary()
            {
                #region Individual Details Updates

                int inactiveStatusId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;

                var changes = new List<string>();

                if ( SelectedFields.Contains( FieldNames.Title ) )
                {
                    EvaluateChange( changes, "Title", DefinedValueCache.GetName( this.UpdateTitleValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.Suffix ) )
                {
                    EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( this.UpdateSuffixValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.ConnectionStatus ) )
                {
                    EvaluateChange( changes, "Connection Status", DefinedValueCache.GetName( this.UpdateConnectionStatusValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.RecordStatus ) )
                {
                    EvaluateChange( changes, "Record Status", DefinedValueCache.GetName( this.UpdateRecordStatusValueId ) );

                    if ( this.UpdateRecordStatusValueId.HasValue && this.UpdateRecordStatusValueId.Value == inactiveStatusId )
                    {
                        EvaluateChange( changes, "Inactive Reason", DefinedValueCache.GetName( this.UpdateInactiveReasonId ) );

                        if ( !string.IsNullOrWhiteSpace( this.UpdateInactiveReasonNote ) )
                        {
                            EvaluateChange( changes, "Inactive Reason Note", this.UpdateInactiveReasonNote );
                        }
                    }
                }

                if ( SelectedFields.Contains( FieldNames.Gender ) )
                {
                    EvaluateChange( changes, "Gender", this.UpdateGender );
                }

                if ( SelectedFields.Contains( FieldNames.MaritalStatus ) )
                {
                    EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( this.UpdateMaritalStatusValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.GraduationYear ) )
                {
                    EvaluateChange( changes, "Graduation Year", this.UpdateGraduationYear );
                }

                if ( SelectedFields.Contains( FieldNames.EmailIsActive ) )
                {
                    EvaluateChange( changes, "Email Is Active", this.UpdateEmailActive );
                }

                if ( SelectedFields.Contains( FieldNames.CommunicationPreference ) )
                {
                    EvaluateChange( changes, "Communication Preference", this.UpdateCommunicationPreference );
                }

                if ( SelectedFields.Contains( FieldNames.EmailPreference ) )
                {
                    EvaluateChange( changes, "Email Preference", this.UpdateEmailPreference );
                }

                if ( SelectedFields.Contains( FieldNames.EmailNote ) )
                {
                    EvaluateChange( changes, "Email Note", this.UpdateEmailNote );
                }

                if ( SelectedFields.Contains( FieldNames.SystemNote ) )
                {
                    EvaluateChange( changes, "System Note", this.UpdateSystemNote );
                }

                if ( SelectedFields.Contains( FieldNames.ReviewReason ) )
                {
                    EvaluateChange( changes, "Review Reason", DefinedValueCache.GetName( this.UpdateReviewReasonValueId ) );
                }

                if ( SelectedFields.Contains( FieldNames.ReviewReasonNote ) )
                {
                    EvaluateChange( changes, "Review Reason Note", this.UpdateReviewReasonNote );
                }

                if ( SelectedFields.Contains( FieldNames.Campus ) )
                {
                    if ( this.UpdateCampusId.HasValue )
                    {
                        var campus = CampusCache.Get( this.UpdateCampusId.Value );
                        if ( campus != null )
                        {
                            EvaluateChange( changes, "Campus (for all family members)", campus.Name );
                        }
                    }
                }

                // following
                if ( this.UpdateFollowingAction == FollowingChangeActionSpecifier.Add )
                {
                    changes.Add( "Add to your Following list." );
                }
                else if ( this.UpdateFollowingAction == FollowingChangeActionSpecifier.Remove )
                {
                    changes.Add( "Remove from your Following list." );
                }

                #endregion

                #region Attributes

                var rockContext = this.GetDataContextForCurrentUser();

                if ( this.UpdatePersonAttributeValues != null
                     && this.UpdatePersonAttributeValues.Any() )
                {
                    var selectedCategories = new List<CategoryCache>();
                    foreach ( Guid categoryGuid in this.PersonAttributeCategories )
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
                        categoryIndex++;

                        var orderedAttributeList = new AttributeService( rockContext ).GetByCategoryId( category.Id, false )
                            .OrderBy( a => a.Order ).ThenBy( a => a.Name );

                        foreach ( var attribute in orderedAttributeList )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                var attributeCache = AttributeCache.Get( attribute.Id );

                                if ( attributeCache != null
                                     && this.UpdatePersonAttributeValues.ContainsKey( attributeCache.Key ) )
                                {
                                    var newValue = this.UpdatePersonAttributeValues[attributeCache.Key];

                                    EvaluateChange( changes, attributeCache.Name, attributeCache.FieldType.Field.FormatValue( null, newValue, attributeCache.QualifierValues, false ) );
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Note

                if ( this.UpdateNoteAction == NoteChangeActionSpecifier.Add )
                {
                    if ( !string.IsNullOrWhiteSpace( this.UpdateNoteText ) && this.UpdateNoteTypeId.HasValue && CurrentPerson != null )
                    {
                        var noteType = NoteTypeCache.Get( this.UpdateNoteTypeId.Value );

                        if ( noteType != null )
                        {
                            changes.Add( string.Format( "Add a <span class='field-name'>{0}{1}{2}</span> of <p><span class='field-value'>{3}</span></p>.",
                                ( this.UpdateNoteIsPrivate ? "Private " : "" ), noteType.Name, ( this.UpdateNoteIsAlert ? " (Alert)" : "" ), this.UpdateNoteText.ConvertCrLfToHtmlBr() ) );
                        }
                    }
                }

                #endregion

                #region Group

                if ( this.UpdateGroupAction != GroupChangeActionSpecifier.None )
                {
                    var group = new GroupService( rockContext ).Get( this.UpdateGroupId.Value );
                    if ( group != null )
                    {
                        if ( this.UpdateGroupAction == GroupChangeActionSpecifier.Remove )
                        {
                            changes.Add( string.Format( "Remove from <span class='field-name'>{0}</span> group.", group.Name ) );
                        }
                        else if ( this.UpdateGroupAction == GroupChangeActionSpecifier.Add )
                        {
                            changes.Add( string.Format( "Add to <span class='field-name'>{0}</span> group.", group.Name ) );
                        }
                        else // Update
                        {
                            if ( SelectedFields.Contains( FieldNames.GroupRole ) )
                            {
                                if ( this.UpdateGroupRoleId.HasValue )
                                {
                                    var roleId = this.UpdateGroupRoleId.Value;
                                    var groupType = GroupTypeCache.Get( group.GroupTypeId );
                                    var role = groupType.Roles.Where( r => r.Id == roleId ).FirstOrDefault();
                                    if ( role != null )
                                    {
                                        string field = string.Format( "{0} Role", group.Name );
                                        EvaluateChange( changes, field, role.Name );
                                    }
                                }
                            }

                            if ( SelectedFields.Contains( FieldNames.GroupMemberStatus ) )
                            {
                                string field = string.Format( "{0} Member Status", group.Name );
                                EvaluateChange( changes, field, this.UpdateGroupStatus.ToString() );
                            }

                            var groupMember = new GroupMember();
                            groupMember.Group = group;
                            groupMember.GroupId = group.Id;
                            groupMember.LoadAttributes( rockContext );

                            foreach ( var attributeCache in groupMember.Attributes.Select( a => a.Value ) )
                            {
                                if ( this.UpdateGroupAttributeValues.ContainsKey( attributeCache.Key ) )
                                {
                                    var newValue = this.UpdateGroupAttributeValues[attributeCache.Key];

                                    string field = string.Format( "{0}: {1}", group.Name, attributeCache.Name );
                                    EvaluateChange( changes, field, attributeCache.FieldType.Field.FormatValue( null, newValue, attributeCache.QualifierValues, false ) );
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Tag

                if ( this.UpdateTagAction != TagChangeActionSpecifier.None )
                {
                    var tagService = new TagService( rockContext );

                    var tag = tagService.Queryable().FirstOrDefault( x => x.Id == this.UpdateTagId );

                    if ( tag != null )
                    {
                        changes.Add( string.Format( "{0} {1} <span class='field-name'>{2}</span> tag.",
                                this.UpdateTagAction.ToString(),
                                this.UpdateTagAction == TagChangeActionSpecifier.Add ? "to" : "from",
                                tag.Name ) );
                    }
                }

                #endregion

                #region workflow

                if ( this.PostUpdateWorkflowIdList != null
                     && this.PostUpdateWorkflowIdList.Any() )
                {
                    var workflowTypes = new List<string>();

                    foreach ( var workflowId in this.PostUpdateWorkflowIdList )
                    {
                        var workflowType = WorkflowTypeCache.Get( workflowId );

                        if ( workflowType != null )
                        {
                            workflowTypes.Add( workflowType.Name );
                        }
                    }

                    if ( workflowTypes.Any() )
                    {
                        changes.Add( string.Format( "Activate the <span class='field-name'>{0}</span> {1}.",
                             workflowTypes.AsDelimited( ", ", " and " ),
                             "workflow".PluralizeIf( workflowTypes.Count > 1 ) ) );
                    }
                }

                #endregion

                return changes;
            }

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
        }

        #endregion
    }
}