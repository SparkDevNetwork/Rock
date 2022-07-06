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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a requirement control that will allow updating.
    /// </summary>
    [ToolboxData( "<{0}:GroupMemberRequirementCard runat=server></{0}:GroupMemberRequirementCard>" )]
    public class GroupMemberRequirementCard : Control, INamingContainer
    {
        private GroupRequirementType _groupMemberRequirementType;

        private bool _canOverride;

        private GroupMemberRequirement _groupMemberRequirement;

        /// <summary>
        /// Link Button control for overriding requirements.
        /// </summary>
        private LinkButton _lbMarkAsMet;

        /// <summary>
        /// Modal Dialog control to permit an override.
        /// </summary>
        private ModalDialog _modalDialog;

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
        public string Title
        {
            get { return ViewState["Title"] as string ?? string.Empty; }
            set { ViewState["Title"] = value; }
        }

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
        /// Gets or sets the calculated due date for this group member requirement.
        /// </summary>
        public DateTime? GroupMemberRequirementDueDate { get; set; }

        /// <summary>
        /// Gets or sets the CSS Class to use for the title icon of the requirement card.
        /// </summary>
        /// <value>
        /// The card icon class.
        /// </value>
        [Bindable( true ), Category( "Appearance" ), Description( "The CSS class to add to the card div." )]
        public string TypeIconCssClass
        {
            get { return ViewState["TypeIconCssClass"] as string; }
            set { ViewState["TypeIconCssClass"] = value; }
        }

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

            if ( groupRequirement.IsAuthorized( "Override", currentPerson ) )
            {
                _lbMarkAsMet = new LinkButton
                {
                    CausesValidation = false,
                    ID = "btnMarkasMetPopup" + this.ClientID,
                    Text = "<i class='fa fa-check-circle-o fa-fw'></i>Mark as Met",
                };
                _lbMarkAsMet.Click += btnMarkasMetPopup_Click;
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
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-3 ml-3 mr-3 " + CardStatus( MeetsGroupRequirement ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                var cardContentColumnClass = "col-xs-12";

                // If there is an icon, give it a col-2 to separate it from the text column.
                if ( !string.IsNullOrWhiteSpace( TypeIconCssClass ) )
                {
                    cardContentColumnClass = "col-xs-10";
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-2" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Span );
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, TypeIconCssClass + " fa-fw fa-2x" );
                    writer.RenderBeginTag( HtmlTextWriterTag.I );

                    // End the I tag.
                    writer.RenderEndTag();

                    // End the Span Col tag.
                    writer.RenderEndTag();
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, cardContentColumnClass );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( _modalDialog != null )
                {
                    _modalDialog.RenderControl( writer );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Small );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "mr-1" );
                writer.RenderBeginTag( HtmlTextWriterTag.Strong );
                writer.Write( this.Title.Trim() );

                // End the Strong tag.
                writer.RenderEndTag();

                // If there is a due date, add the short date in a "small" tag.
                if ( GroupMemberRequirementDueDate.HasValue )
                {
                    writer.Write( "Due: " + GroupMemberRequirementDueDate.Value.ToShortDateString() );
                }

                // End the Small tag.
                writer.RenderEndTag();

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

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( CardMessage( MeetsGroupRequirement ) );

                // End the Div tag.
                writer.RenderEndTag();

                if ( this._groupMemberRequirementType.Summary.IsNotNullOrWhiteSpace() )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Small );
                    writer.Write( this._groupMemberRequirementType.Summary );

                    // End the Small tag.
                    writer.RenderEndTag();
                }

                var hasDoesNotMeetWorkflow = _groupMemberRequirementType.DoesNotMeetWorkflowTypeId.HasValue && !_groupMemberRequirementType.ShouldAutoInitiateDoesNotMeetWorkflow;
                var hasWarningWorkflow = _groupMemberRequirementType.WarningWorkflowTypeId.HasValue && !_groupMemberRequirementType.ShouldAutoInitiateWarningWorkflow;

                // If "Does Not Meet" has a workflow and it is not automatically initiated, it should be added as a control.
                if ( hasDoesNotMeetWorkflow || hasWarningWorkflow || _canOverride )
                {
                    writer.AddStyleAttribute( "list-style-type", "none" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                    if ( hasDoesNotMeetWorkflow )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Li );

                        var qryParms = new Dictionary<string, string>();
                        qryParms.Add( "WorkflowTypeId", _groupMemberRequirementType.DoesNotMeetWorkflowTypeId.ToString() );
                        var workflowLink = new PageReference( SystemGuid.Page.LAUNCHWORKFLOW, qryParms );

                        if ( workflowLink.PageId > 0 )
                        {
                            bool showLinkToEntry = _groupMemberRequirementType.DoesNotMeetWorkflowType.IsActive.HasValue ? _groupMemberRequirementType.DoesNotMeetWorkflowType.IsActive.Value : false;
                            if ( showLinkToEntry )
                            {
                                writer.AddAttribute( HtmlTextWriterAttribute.Href, workflowLink.BuildUrl() );
                                writer.AddAttribute( HtmlTextWriterAttribute.Target, "_blank" );
                                writer.RenderBeginTag( HtmlTextWriterTag.A );

                                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-play-circle-o fa-fw" );
                                writer.RenderBeginTag( HtmlTextWriterTag.I );

                                // End the I tag.
                                writer.RenderEndTag();
                                writer.Write( _groupMemberRequirementType.DoesNotMeetWorkflowLinkText );

                                // End the A tag.
                                writer.RenderEndTag();
                            }
                        }

                        // End the Li tag.
                        writer.RenderEndTag();
                    }

                    if ( hasWarningWorkflow )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Li );

                        var qryParms = new Dictionary<string, string>();
                        qryParms.Add( "WorkflowTypeId", _groupMemberRequirementType.WarningWorkflowTypeId.ToString() );
                        var workflowLink = new PageReference( SystemGuid.Page.LAUNCHWORKFLOW, qryParms );
                        if ( workflowLink.PageId > 0 )
                        {
                            bool showLinkToEntry = _groupMemberRequirementType.WarningWorkflowType.IsActive.HasValue ? _groupMemberRequirementType.WarningWorkflowType.IsActive.Value : false;
                            if ( showLinkToEntry )
                            {
                                writer.AddAttribute( HtmlTextWriterAttribute.Href, workflowLink.BuildUrl() );
                                writer.AddAttribute( HtmlTextWriterAttribute.Target, "_blank" );
                                writer.RenderBeginTag( HtmlTextWriterTag.A );

                                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-play-circle-o fa-fw" );
                                writer.RenderBeginTag( HtmlTextWriterTag.I );

                                // End the I tag.
                                writer.RenderEndTag();
                                writer.Write( _groupMemberRequirementType.WarningWorkflowLinkText );

                                // End the A tag.
                                writer.RenderEndTag();
                            }
                        }

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
                    return this._groupMemberRequirementType.PositiveLabel;

                case MeetsGroupRequirement.NotMet:
                    return this._groupMemberRequirementType.NegativeLabel;

                case MeetsGroupRequirement.MeetsWithWarning:
                    return this._groupMemberRequirementType.WarningLabel;

                default:
                    return "Issue with this message.";
            }
        }

        /// <summary>
        /// Handles the Click event of the _btnMarkasMetPopup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMarkasMetPopup_Click( object sender, EventArgs e )
        {
            _modalDialog.Show();
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
            }

            groupMemberRequirement.WasOverridden = true;
            var currentPerson = ( ( RockPage ) Page ).CurrentPerson;
            groupMemberRequirement.OverriddenByPersonAliasId = currentPerson.PrimaryAliasId;
            groupMemberRequirement.OverriddenDateTime = RockDateTime.Now;

            groupMemberRequirementService.Add( groupMemberRequirement );

            rockContext.SaveChanges();
            _modalDialog.Hide();
        }
    }
}
