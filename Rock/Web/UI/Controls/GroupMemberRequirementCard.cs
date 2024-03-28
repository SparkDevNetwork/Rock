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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a requirement control that will allow updating.
    /// </summary>
    [ToolboxData( "<{0}:GroupMemberRequirementCard runat=server></{0}:GroupMemberRequirementCard>" )]
    public class GroupMemberRequirementCard : Control, INamingContainer
    {
        private static class LabelKey
        {
            public const string RequirementMet = " Requirement Met";
            public const string RequirementNotMet = " Requirement Not Met";
            public const string RequirementMetWithWarning = "Requirement Met With Warning";
        }

        private GroupRequirementType _groupMemberRequirementType;

        private bool _canOverride;

        private bool _hasDoesNotMeetWorkflow;

        private bool _hasWarningWorkflow;

        private GroupMemberRequirement _groupMemberRequirement;

        /// <summary>
        /// Link Button control for manual "checkbox" requirements.
        /// </summary>
        private LinkButton _lbManualRequirement;

        /// <summary>
        /// Link Button control for overriding requirements.
        /// </summary>
        private LinkButton _lbMarkAsMet;

        /// <summary>
        /// Link Button control for warning workflow.
        /// </summary>
        private LinkButton _lbWarningWorkflow;

        /// <summary>
        /// Link Button control for "not met" workflow.
        /// </summary>
        private LinkButton _lbDoesNotMeetWorkflow;

        /// <summary>
        /// Modal Dialog control to permit an override.
        /// </summary>
        private ModalDialog _modalDialog;

        /// <summary>
        /// Modal Alert control to notify of workflow processing.
        /// </summary>
        private ModalAlert _modalAlert;

        /// <summary>
        /// Gets or sets the title text.
        /// </summary>
        /// <value>
        /// The title text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the card's title." )
        ]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the identifier for this group requirement.
        /// </summary>
        public int? GroupRequirementId { get; set; }

        /// <summary>
        /// Gets or sets the identifier for this group member.
        /// </summary>
        public int GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the identifier for this group member requirement.
        /// </summary>
        public int? GroupMemberRequirementId { get; set; }

        /// <summary>
        /// Gets or sets whether the summary is displayed.
        /// </summary>
        public bool IsSummaryHidden { get; set; }

        /// <summary>
        /// Gets or sets whether interaction is disabled for the child controls.
        /// </summary>
        public bool IsInteractionDisabled { get; set; }

        /// <summary>
        /// Gets or sets the calculated due date for this group member requirement.
        /// </summary>
        public DateTime? GroupMemberRequirementDueDate { get; set; }

        /// <summary>
        /// The workflow entry page Guid (as a string) for running workflows.
        /// As in <see cref="PageReference.PageReference(string, Dictionary{string, string}, System.Collections.Specialized.NameValueCollection)">PageReference(...)</see>,
        /// LinkedPageValue is in format "Page.Guid,PageRoute.Guid".
        /// </summary>
        public string WorkflowEntryLinkedPageValue { get; set; }

        /// <summary>
        /// Gets or sets the CSS Class to use for the title icon of the requirement card.
        /// </summary>
        /// <value>
        /// The card icon class.
        /// </value>
        [Bindable( true ), Category( "Appearance" ), Description( "The CSS class to add to the card div." )]
        public string TypeIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.MeetsGroupRequirement"/> for the requirement card.
        /// </summary>
        [Bindable( true ), Category( "Appearance" ), Description( "The group requirement state for the card div." )]
        public MeetsGroupRequirement MeetsGroupRequirement
        {
            get; set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberRequirementCard"/> class.
        /// </summary>
        public GroupMemberRequirementCard( GroupRequirementType groupRequirementType, bool canOverride )
        {
            this._groupMemberRequirementType = groupRequirementType;
            this._canOverride = canOverride;
        }

        /// <summary>
        /// Requirement Results for the Group Requirement
        /// </summary>
        public IEnumerable<GroupRequirementStatus> RequirementResults { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            EnsureChildControls();
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            EnsureChildControls();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            if ( !GroupRequirementId.HasValue )
            {
                return;
            }

            var currentPerson = ( ( RockPage ) Page ).CurrentPerson;
            var rockContext = new RockContext();
            GroupRequirementService groupRequirementService = new GroupRequirementService( rockContext );
            var groupRequirement = groupRequirementService.Get( GroupRequirementId.Value );

            if ( !IsInteractionDisabled )
            {
                if ( groupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual )
                {
                    var manualLabel = groupRequirement.GroupRequirementType.CheckboxLabel.IsNotNullOrWhiteSpace() ? groupRequirement.GroupRequirementType.CheckboxLabel : groupRequirement.GroupRequirementType.Name;
                    _lbManualRequirement = new LinkButton
                    {
                        CausesValidation = false,
                        ID = "lbManualRequirement" + this.ClientID,
                        Text = "<i class='fa fa-square-o fa-fw'></i> " + manualLabel,
                    };
                    _lbManualRequirement.Click += lbManualRequirement_Click;
                    Controls.Add( _lbManualRequirement );
                }

                // This _canOverride is determined by the GroupMemberRequirementContainer when it creates a GroupMemberRequirementCard.
                if ( _canOverride )
                {
                    _lbMarkAsMet = new LinkButton
                    {
                        CausesValidation = false,
                        ID = "lbMarkasMetPopup" + this.ClientID,
                        Text = "<i class='fa fa-check-circle-o fa-fw'></i> Mark as Met",
                    };
                    _lbMarkAsMet.Click += lbMarkasMetPopup_Click;
                    Controls.Add( _lbMarkAsMet );

                    _modalDialog = new ModalDialog
                    {
                        ID = "modalDialog_" + this.ClientID
                    };
                    _modalDialog.ValidationGroup = _modalDialog.ID + "_validationgroup";
                    _modalDialog.Title = "Mark as met?";

                    HtmlGenericControl headingControl = new HtmlGenericControl( "h5" );
                    headingControl.InnerText = "Are you sure you want to manually mark this requirement as met?";
                    _modalDialog.Content.Controls.Add( headingControl );

                    _modalDialog.SaveButtonText = "OK";
                    _modalDialog.SaveClick += btnMarkRequirementAsMet_Click;
                    Controls.Add( _modalDialog );
                }

                // Workflow linkbutton controls

                // Add workflow link if:
                // the Group Requirement Type has a workflow type ID,
                // the workflow is NOT auto initiated, and
                // the requirement status matches the workflow purpose (Meets with Warning or Not Met).
                _hasDoesNotMeetWorkflow = _groupMemberRequirementType.DoesNotMeetWorkflowTypeId.HasValue
                    && !_groupMemberRequirementType.ShouldAutoInitiateDoesNotMeetWorkflow
                    && MeetsGroupRequirement == MeetsGroupRequirement.NotMet;

                _hasWarningWorkflow = _groupMemberRequirementType.WarningWorkflowTypeId.HasValue
                    && !_groupMemberRequirementType.ShouldAutoInitiateWarningWorkflow
                    && MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning;

                if ( _hasDoesNotMeetWorkflow )
                {
                    var qryParms = new Dictionary<string, string>();
                    qryParms.Add( "WorkflowTypeGuid", _groupMemberRequirementType.DoesNotMeetWorkflowType.Guid.ToString() );
                    var workflowLink = new PageReference( WorkflowEntryLinkedPageValue, qryParms );
                    if ( workflowLink.PageId > 0 )
                    {
                        // If the link text has a value, use it, otherwise use the default label key value.
                        var workflowLinkText = _groupMemberRequirementType.DoesNotMeetWorkflowLinkText.IsNotNullOrWhiteSpace() ?
                            _groupMemberRequirementType.DoesNotMeetWorkflowLinkText :
                            LabelKey.RequirementNotMet;
                        _lbDoesNotMeetWorkflow = new LinkButton
                        {
                            ID = "lbDoesNotMeetWorkflow" + this.ClientID,
                            Text = "<i class='fa fa-play-circle-o fa-fw'></i>" + workflowLinkText,
                        };
                        _lbDoesNotMeetWorkflow.Click += lbDoesNotMeetWorkflow_Click;
                        Controls.Add( _lbDoesNotMeetWorkflow );

                        _modalAlert = new ModalAlert
                        {
                            ID = "modalAlert_" + this.ClientID,
                        };

                        Controls.Add( _modalAlert );
                    }
                }

                if ( _hasWarningWorkflow )
                {
                    var qryParms = new Dictionary<string, string>();
                    qryParms.Add( "WorkflowTypeGuid", _groupMemberRequirementType.WarningWorkflowType.Guid.ToString() );
                    var workflowLink = new PageReference( WorkflowEntryLinkedPageValue, qryParms );
                    if ( workflowLink.PageId > 0 )
                    {
                        // If the link text has a value, use it, otherwise use the default label key value.
                        var workflowLinkText = _groupMemberRequirementType.WarningWorkflowLinkText.IsNotNullOrWhiteSpace() ?
                            _groupMemberRequirementType.WarningWorkflowLinkText :
                            LabelKey.RequirementMetWithWarning;
                        _lbWarningWorkflow = new LinkButton
                        {
                            ID = "lblWarningWorkflow" + this.ClientID,
                            Text = "<i class='fa fa-play-circle-o fa-fw'></i>" + workflowLinkText,
                        };
                        _lbWarningWorkflow.Click += lbWarningWorkflow_Click;
                        Controls.Add( _lbWarningWorkflow );

                        _modalAlert = new ModalAlert
                        {
                            ID = "modalAlert_" + this.ClientID,
                        };

                        Controls.Add( _modalAlert );
                    }
                }
            }
        }

        /// <summary>
        /// Renders this Group Member Requirement Card control.
        /// </summary>
        /// <param name="writer"></param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            _groupMemberRequirement = new GroupMemberRequirementService( new RockContext() ).Get( this.GroupMemberRequirementId ?? 0 );
            if ( this.Title.Trim() != string.Empty )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-12 col-sm-6 col-md-4 requirement-item" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, CardStatus( MeetsGroupRequirement ) + " alert-requirement" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( !string.IsNullOrWhiteSpace( TypeIconCssClass ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-shrink-0" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Span );
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, TypeIconCssClass + " fa-fw icon" );
                    writer.RenderBeginTag( HtmlTextWriterTag.I );

                    // End the I tag.
                    writer.RenderEndTag();

                    // End the Span Col tag.
                    writer.RenderEndTag();
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-fill" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( _modalDialog != null )
                {
                    _modalDialog.RenderControl( writer );
                }

                if ( _modalAlert != null )
                {
                    _modalAlert.RenderControl( writer );
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "requirement-title h6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( this.Title.Trim() );

                // End the Span tag.
                writer.RenderEndTag();

                // If the requirement is not met and there is a due date, add the short date in a "small" tag.
                if ( MeetsGroupRequirement != MeetsGroupRequirement.Meets && GroupMemberRequirementDueDate.HasValue )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "requirement-due small" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Span );
                    writer.Write( "Due: " + GroupMemberRequirementDueDate.Value.ToShortDateString() );

                    // End the Span tag.
                    writer.RenderEndTag();
                }

                // If there is an overridden requirement, indicate it here.
                if ( _groupMemberRequirement != null && _groupMemberRequirement.WasOverridden )
                {
                    var toolTipText = string.Format(
                        "Requirement Marked Met by {0} on {1}",
                        _groupMemberRequirement.OverriddenByPersonAlias.Person.FullName,
                        _groupMemberRequirement.OverriddenDateTime.ToShortDateString() );

                    writer.AddAttribute( "class", "help" );
                    writer.AddAttribute( "href", "#" );
                    writer.AddAttribute( "tabindex", "-1" );

                    if ( !this.Visible )
                    {
                        writer.AddStyleAttribute( "display", "none" );
                    }

                    writer.AddAttribute( "data-toggle", "tooltip" );
                    writer.AddAttribute( "data-placement", "auto" );
                    writer.AddAttribute( "data-container", "body" );
                    writer.AddAttribute( "data-html", "true" );
                    writer.AddAttribute( "title", toolTipText );
                    writer.RenderBeginTag( HtmlTextWriterTag.A );
                    writer.AddAttribute( "class", "fa fa-user-check fa-fw" );
                    writer.RenderBeginTag( HtmlTextWriterTag.I );
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "requirement-message" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( CardMessage( MeetsGroupRequirement ) );

                // End the Div tag.
                writer.RenderEndTag();

                if ( this._groupMemberRequirementType.Summary.IsNotNullOrWhiteSpace() && !IsSummaryHidden )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "requirement-summary" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( this._groupMemberRequirementType.Summary );

                    // End the Small tag.
                    writer.RenderEndTag();
                }

                // If any workflows, the requirement can be overridden, or a manual requirement, create the unordered list and list items.
                if ( !IsInteractionDisabled && ( _hasDoesNotMeetWorkflow || _hasWarningWorkflow || _canOverride || this._groupMemberRequirementType.RequirementCheckType == RequirementCheckType.Manual ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "list-unstyled list-requirements" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                    if ( this._groupMemberRequirementType.RequirementCheckType == RequirementCheckType.Manual && MeetsGroupRequirement != MeetsGroupRequirement.Meets )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Li );

                        _lbManualRequirement.RenderControl( writer );

                        // End the Li tag.
                        writer.RenderEndTag();
                    }

                    if ( _hasDoesNotMeetWorkflow && _lbDoesNotMeetWorkflow != null )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Li );
                        _lbDoesNotMeetWorkflow.RenderControl( writer );

                        // End the Li tag.
                        writer.RenderEndTag();
                    }

                    if ( _hasWarningWorkflow && _lbWarningWorkflow != null )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Li );

                        _lbWarningWorkflow.RenderControl( writer );

                        // End the Li tag.
                        writer.RenderEndTag();
                    }

                    // If this requirement can be marked as met manually (overridden),
                    // and the link button exists ( the current person has authorization to override / "mark as met")
                    // and the requirement has NOT been met,
                    // and does not have a group member requirement,
                    // or it does have a group member requirement but it has not been overridden,
                    // then add the link button.
                    if ( _canOverride && _lbMarkAsMet != null && ( MeetsGroupRequirement != MeetsGroupRequirement.Meets ) && ( _groupMemberRequirement == null || ( _groupMemberRequirement != null && !_groupMemberRequirement.WasOverridden ) ) )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Li );
                        _lbMarkAsMet.RenderControl( writer );

                        // End the Li tag.
                        writer.RenderEndTag();
                    }

                    // End the Ul tag.
                    writer.RenderEndTag();
                }

                // End the Div Col tag.
                writer.RenderEndTag();

                // End the Div Row tag.
                writer.RenderEndTag();

                // End the Div Col tag.
                writer.RenderEndTag();
            }
        }

        private string CardStatus( MeetsGroupRequirement meetsGroupRequirement )
        {
            switch ( meetsGroupRequirement )
            {
                case MeetsGroupRequirement.Meets:
                    return "alert alert-success";

                case MeetsGroupRequirement.NotMet:
                    return "alert alert-danger";

                case MeetsGroupRequirement.MeetsWithWarning:
                    return "alert alert-warning";

                default:
                    return "alert alert-info";
            }
        }

        private string CardMessage( MeetsGroupRequirement meetsGroupRequirement )
        {
            switch ( meetsGroupRequirement )
            {
                case MeetsGroupRequirement.Meets:
                    return _groupMemberRequirementType.PositiveLabel.IsNotNullOrWhiteSpace() ? _groupMemberRequirementType.PositiveLabel : LabelKey.RequirementMet;

                case MeetsGroupRequirement.NotMet:
                    return _groupMemberRequirementType.NegativeLabel.IsNotNullOrWhiteSpace() ? _groupMemberRequirementType.NegativeLabel : LabelKey.RequirementNotMet;

                case MeetsGroupRequirement.MeetsWithWarning:
                    return _groupMemberRequirementType.WarningLabel.IsNotNullOrWhiteSpace() ? _groupMemberRequirementType.WarningLabel : LabelKey.RequirementMetWithWarning;

                default:
                    return "Issue With Requirement.";
            }
        }

        #region Events

        /// <summary>
        /// Handles the Click event of the _lbMarkasMetPopup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMarkasMetPopup_Click( object sender, EventArgs e )
        {
            _modalDialog.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbManualRequirement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbManualRequirement_Click( object sender, EventArgs e )
        {
            // Save the Requirement change.
            // Get the requirement ID, the group member ID, and mark it as completed.
            var rockContext = new RockContext();
            GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
            var groupMemberRequirement = groupMemberRequirementService.Get( this.GroupMemberRequirementId ?? 0 );
            if ( groupMemberRequirement == null && GroupRequirementId.HasValue )
            {
                groupMemberRequirement = new GroupMemberRequirement
                {
                    GroupRequirementId = GroupRequirementId.Value,
                    GroupMemberId = GroupMemberId
                };
                groupMemberRequirementService.Add( groupMemberRequirement );
            }

            groupMemberRequirement.WasManuallyCompleted = true;
            var currentPerson = ( ( RockPage ) Page ).CurrentPerson;
            groupMemberRequirement.ManuallyCompletedByPersonAliasId = currentPerson.PrimaryAliasId;
            groupMemberRequirement.ManuallyCompletedDateTime = RockDateTime.Now;
            groupMemberRequirement.RequirementMetDateTime = RockDateTime.Now;

            rockContext.SaveChanges();

            // Reload the page to make sure that the current status is reflected in the card styling.
            var currentPageReference = this.RockBlock().CurrentPageReference;
            Dictionary<string, string> currentPageParameters = this.RockBlock().PageParameters().ToDictionary( k => k.Key, k => k.Value.ToString() );
            var pageRef = new PageReference( currentPageReference.PageId, currentPageReference.RouteId, currentPageParameters );
            this.RockBlock().NavigateToPage( pageRef );
        }

        /// <summary>
        /// Handles the Click event of the btnMarkRequirementAsMet control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMarkRequirementAsMet_Click( object sender, EventArgs e )
        {
            // Save the Requirement change.
            // Get the requirement ID, the group member ID, and mark it as completed.
            var rockContext = new RockContext();
            GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
            var groupMemberRequirement = groupMemberRequirementService.Get( this.GroupMemberRequirementId ?? 0 );
            if ( groupMemberRequirement == null && GroupRequirementId.HasValue )
            {
                groupMemberRequirement = new GroupMemberRequirement
                {
                    GroupRequirementId = GroupRequirementId.Value,
                    GroupMemberId = GroupMemberId
                };
                groupMemberRequirementService.Add( groupMemberRequirement );
            }

            groupMemberRequirement.WasOverridden = true;
            var currentPerson = ( ( RockPage ) Page ).CurrentPerson;
            groupMemberRequirement.OverriddenByPersonAliasId = currentPerson.PrimaryAliasId;
            groupMemberRequirement.OverriddenDateTime = RockDateTime.Now;
            groupMemberRequirement.RequirementMetDateTime = RockDateTime.Now;

            rockContext.SaveChanges();
            _modalDialog.Hide();

            // Reload the page to make sure that the current status is reflected in the card styling.
            var currentPageReference = this.RockBlock().CurrentPageReference;
            Dictionary<string, string> currentPageParameters = this.RockBlock().PageParameters().ToDictionary( k => k.Key, k => k.Value.ToString() );
            var pageRef = new PageReference( currentPageReference.PageId, currentPageReference.RouteId, currentPageParameters );
            this.RockBlock().NavigateToPage( pageRef );
        }

        /// <summary>
        /// Handles the Click event of the lbDoesNotMeetWorkflow_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDoesNotMeetWorkflow_Click( object sender, EventArgs e )
        {
            if ( !_groupMemberRequirementType.DoesNotMeetWorkflowTypeId.HasValue )
            {
                return;
            }

            // Begin the workflow.
            var workflowType = WorkflowTypeCache.Get( this._groupMemberRequirementType.DoesNotMeetWorkflowTypeId.Value );

            // If a workflow type is not persisted, let the user know that it did not work.
            if ( !workflowType.IsPersisted )
            {
                _modalAlert.Show( $"The Workflow Type '{workflowType.Name}' is not configured to be automatically persisted, and could not be started.", ModalAlertType.Warning );
                return;
            }

            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var rockContext = new RockContext();
                GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
                var groupMemberRequirement = groupMemberRequirementService.Get( this.GroupMemberRequirementId ?? 0 );

                // If there is a workflow ID in the group member requirement, navigate to that workflow entry page, otherwise, activate one.
                Rock.Model.Workflow workflow;
                if ( groupMemberRequirement != null && groupMemberRequirement.DoesNotMeetWorkflowId.HasValue )
                {
                    workflow = new Rock.Model.WorkflowService( new RockContext() ).Get( groupMemberRequirement.DoesNotMeetWorkflowId.Value );
                    var qryParams = new Dictionary<string, string>
                            {
                                { "WorkflowTypeGuid", workflowType.Guid.ToString() },
                                { "WorkflowGuid", workflow.Guid.ToString() }
                            };
                    var workflowLink = new PageReference( WorkflowEntryLinkedPageValue, qryParams );

                    this.RockBlock().NavigateToPage( workflowLink );
                }
                else
                {
                    if ( groupMemberRequirement == null && GroupRequirementId.HasValue )
                    {
                        groupMemberRequirement = new GroupMemberRequirement
                        {
                            GroupRequirementId = GroupRequirementId.Value,
                            GroupMemberId = GroupMemberId
                        };
                        groupMemberRequirementService.Add( groupMemberRequirement );
                        rockContext.SaveChanges();

                        // Gets the group member requirement with the Group Member included.
                        groupMemberRequirement = groupMemberRequirementService.GetInclude( groupMemberRequirement.Guid, r => r.GroupMember );
                    }

                    workflow = Rock.Model.Workflow.Activate( workflowType, workflowType.Name );
                    workflow.SetAttributeValue( "Person", groupMemberRequirement?.GroupMember.Person.PrimaryAlias.Guid );
                    var processed = new Rock.Model.WorkflowService( new RockContext() ).Process( workflow, groupMemberRequirement, out List<string> workflowErrors );

                    if ( processed )
                    {
                        // Update the group member requirement with the workflow - could potentially overwrite an existing workflow ID, but that is expected.
                        groupMemberRequirement.DoesNotMeetWorkflowId = workflow.Id;
                        groupMemberRequirement.RequirementFailDateTime = RockDateTime.Now;
                        rockContext.SaveChanges();

                        if ( workflow.HasActiveEntryForm( ( ( RockPage ) Page ).CurrentPerson ) )
                        {
                            var message = $"A '{workflowType.Name}' workflow has been started.<br /><br />The new workflow has an active form that is ready for input.";

                            var qryParams = new Dictionary<string, string>
                            {
                                { "WorkflowTypeGuid", workflowType.Guid.ToString() },
                                { "WorkflowGuid", workflow.Guid.ToString() }
                            };

                            var workflowLink = new PageReference( WorkflowEntryLinkedPageValue, qryParams );

                            RegisterWorkflowDetailPageScript( workflowLink.BuildUrl(), message );
                        }
                        else
                        {
                            _modalAlert.Show( $"A '{workflowType.Name}' workflow was started.", ModalAlertType.Information );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbWarningWorkflow_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbWarningWorkflow_Click( object sender, EventArgs e )
        {
            if ( !_groupMemberRequirementType.WarningWorkflowTypeId.HasValue )
            {
                return;
            }

            // Begin the workflow.
            var workflowType = WorkflowTypeCache.Get( this._groupMemberRequirementType.WarningWorkflowTypeId.Value );

            // If a workflow type is not persisted, let the user know that it did not work.
            if ( !workflowType.IsPersisted )
            {
                _modalAlert.Show( $"The Workflow Type '{workflowType.Name}' is not configured to be automatically persisted, and could not be started.", ModalAlertType.Warning );
                return;
            }

            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var rockContext = new RockContext();
                GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
                var groupMemberRequirement = groupMemberRequirementService.Get( this.GroupMemberRequirementId ?? 0 );

                // If there is a workflow ID in the group member requirement, navigate to that workflow entry page, otherwise, activate one.
                Rock.Model.Workflow workflow;
                if ( groupMemberRequirement != null && groupMemberRequirement.WarningWorkflowId.HasValue )
                {
                    workflow = new Rock.Model.WorkflowService( new RockContext() ).Get( groupMemberRequirement.WarningWorkflowId.Value );
                    var qryParams = new Dictionary<string, string>
                            {
                                { "WorkflowTypeGuid", workflowType.Guid.ToString() },
                                { "WorkflowGuid", workflow.Guid.ToString() }
                            };
                    var workflowLink = new PageReference( WorkflowEntryLinkedPageValue, qryParams );

                    this.RockBlock().NavigateToPage( workflowLink );
                }
                else
                {
                    if ( groupMemberRequirement == null && GroupRequirementId.HasValue )
                    {
                        groupMemberRequirement = new GroupMemberRequirement
                        {
                            GroupRequirementId = GroupRequirementId.Value,
                            GroupMemberId = GroupMemberId
                        };
                        groupMemberRequirementService.Add( groupMemberRequirement );
                        rockContext.SaveChanges();

                        // Gets the group member requirement with the Group Member included.
                        groupMemberRequirement = groupMemberRequirementService.GetInclude( groupMemberRequirement.Guid, r => r.GroupMember );
                    }

                    workflow = Rock.Model.Workflow.Activate( workflowType, workflowType.Name );
                    workflow.SetAttributeValue( "Person", groupMemberRequirement?.GroupMember.Person.PrimaryAlias.Guid );
                    var processed = new Rock.Model.WorkflowService( new RockContext() ).Process( workflow, groupMemberRequirement, out List<string> workflowErrors );

                    if ( processed )
                    {
                        // Update the group member requirement with the workflow - could potentially overwrite an existing workflow ID, but that is expected.
                        groupMemberRequirement.WarningWorkflowId = workflow.Id;
                        groupMemberRequirement.RequirementWarningDateTime = RockDateTime.Now;
                        rockContext.SaveChanges();

                        if ( workflow.HasActiveEntryForm( ( ( RockPage ) Page ).CurrentPerson ) )
                        {
                            var message = $"A '{workflowType.Name}' workflow has been started.<br /><br />The new workflow has an active form that is ready for input.";

                            var qryParams = new Dictionary<string, string>
                            {
                                { "WorkflowTypeGuid", workflowType.Guid.ToString() },
                                { "WorkflowGuid", workflow.Guid.ToString() }
                            };

                            var workflowLink = new PageReference( WorkflowEntryLinkedPageValue, qryParams );

                            RegisterWorkflowDetailPageScript( workflowLink.BuildUrl(), message );
                        }
                        else
                        {
                            _modalAlert.Show( $"A '{workflowType.Name}' workflow was started.", ModalAlertType.Information );
                        }
                    }
                }
            }
        }

        #endregion Events

        /// <summary>
        /// Add a script to the client load event for the current page that will also open a new page for the workflow entry form.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="message"></param>
        private void RegisterWorkflowDetailPageScript( string url, string message = null )
        {
            // When the script is executed, it is also removed from the client load event to ensure that it is only run once.
            string script;

            if ( string.IsNullOrEmpty( message ) )
            {
                // Open the workflow detail page.
                script = $@"
                <script language='javascript' type='text/javascript'>
                    Sys.Application.add_load(openWorkflowEntryPage);
                    function openWorkflowEntryPage() {{
                        Sys.Application.remove_load( openWorkflowEntryPage );
                        window.open('{url}');
                    }}
                </script>";
            }
            else
            {
                // Show a modal message dialog, and open the workflow detail page when the dialog is closed.
                message = message.SanitizeHtml( false ).Replace( "'", "&#39;" );
                script = $@"
                <script language='javascript' type='text/javascript'>
                    Sys.Application.add_load(openWorkflowEntryPage);
                    function openWorkflowEntryPage() {{
                        Sys.Application.remove_load( openWorkflowEntryPage );
                        bootbox.alert({{ message:'{message}',
                            callback: function() {{ window.open('{url}'); }}
                        }});
                    }}
                </script>
                ";
            }

            ScriptManager.RegisterStartupScript( this,
                this.GetType(),
                "openWorkflowScript",
                script,
                false );
        }
    }
}
