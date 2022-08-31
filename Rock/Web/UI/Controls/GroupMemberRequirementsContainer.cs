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
//
using System;
using System.Web;
using System.Web.UI;

using System.Linq;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections.Generic;
using Rock.Model;
using Rock.Data;
using System.Web.UI.HtmlControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a container for the requirement cards that will allow updating.
    /// </summary>
    [ToolboxData( "<{0}:GroupMemberRequirementsContainer runat=server></{0}:GroupMemberRequirementsContainer>" )]
    public class GroupMemberRequirementsContainer : CompositeControl, INamingContainer
    {
        #region Properties

        /// <summary>
        /// The Group Member ID for the container.
        /// </summary>
        public int GroupMemberId
        {
            get
            {
                return _groupMemberId;
            }

            set
            {
                _groupMemberId = value;
                ViewState[ViewStateKey.GroupMemberId] = _groupMemberId;
            }
        }

        /// <summary>
        /// The enumerated collection of Group Member Requirements for the container.
        /// </summary>
        public IEnumerable<GroupMemberRequirement> Requirements { get; set; }

        /// <summary>
        /// The workflow entry page Guid (as a string) for running workflows.
        /// As in <see cref="PageReference.PageReference(string, Dictionary{string, string}, System.Collections.Specialized.NameValueCollection)">PageReference(...)</see>,
        /// LinkedPageValue is in format "Page.Guid,PageRoute.Guid".
        /// </summary>
        public string WorkflowEntryLinkedPageValue { get; set; }

        /// <summary>
        /// A boolean to set whether the requirement summary is displayed on cards.
        /// </summary>
        public bool IsSummaryHidden { get; set; }

        /// <summary>
        /// If true, recalculate the requirements of this group member, otherwise load existing requirements. 
        /// </summary>
        public bool ForceRefreshRequirements { get; set; }

        #endregion Properties

        #region ViewStateKeys
        private static class ViewStateKey
        {
            public const string GroupMemberId = "GroupMemberId";
        }
        #endregion ViewStateKeys

        private int _groupMemberId { get; set; }

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
        /// DataBind for the Requirements Container.
        /// </summary>
        public override void DataBind()
        {
            base.DataBind();
            RecreateChildControls();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            if ( !this.Visible )
            {
                return;
            }

            base.CreateChildControls();
            Controls.Clear();

            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( GroupMemberId );

            // If there is not a groupMember record, hide the container, and return.
            if ( groupMember == null )
            {
                this.Visible = false;
                return;
            }

            var attributeValueService = new AttributeValueService( rockContext );
            IEnumerable<GroupRequirementStatus> groupRequirementStatuses;
            if ( ForceRefreshRequirements )
            {
                groupMember.CalculateRequirements( rockContext, true );
            }

            groupRequirementStatuses = groupMember.GetGroupRequirementsStatuses( rockContext );

            // This collects the statuses by their requirement type category with empty / no category requirement types first, then it is by category name.
            var requirementCategories = groupRequirementStatuses
            .Select( s => new
            {
                CategoryId = s.GroupRequirement.GroupRequirementType.CategoryId,
                Name = s.GroupRequirement.GroupRequirementType.CategoryId.HasValue ? s.GroupRequirement.GroupRequirementType.Category.Name : string.Empty,
                RequirementResults = groupRequirementStatuses.Where( gr => gr.GroupRequirement.GroupRequirementType.CategoryId == s.GroupRequirement.GroupRequirementType.CategoryId )
            } ).OrderBy( a => a.CategoryId.HasValue ).ThenBy( a => a.Name ).DistinctBy( a => a.CategoryId );

            var requirementsWithErrors = groupRequirementStatuses.Where( a => a.MeetsGroupRequirement == MeetsGroupRequirement.Error ).ToList();

            if ( requirementsWithErrors.Any() )
            {
                var nbRequirementErrors = new NotificationBox
                {
                    NotificationBoxType = NotificationBoxType.Danger,
                    Visible = true,
                    Text = string.Format( "An error occurred in one or more of the requirement calculations" ),
                    Details = requirementsWithErrors.Select( a => string.Format( "{0}: {1}", a.GroupRequirement.GroupRequirementType.Name, a.CalculationException.Message ) ).ToList().AsDelimited( Environment.NewLine )
                };
                this.Controls.Add( nbRequirementErrors );
            }

            foreach ( var requirementCategory in requirementCategories )
            {
                HtmlGenericControl categoryControl = new HtmlGenericControl( "div" );
                categoryControl.AddCssClass( "row" );
                var categoryName = requirementCategory.Name;

                HtmlGenericControl columnControl = new HtmlGenericControl( "div" );
                columnControl.AddCssClass( "col-xs-12" );

                if ( Visible && categoryName.IsNotNullOrWhiteSpace() )
                {
                    HtmlGenericControl headerControl = new HtmlGenericControl( "h5" );
                    headerControl.InnerText = categoryName;
                    columnControl.Controls.Add( headerControl );
                }

                var currentPerson = this.RockBlock().CurrentPerson;

                // Add the Group Member Requirement Cards here.
                foreach ( var requirementStatus in requirementCategory.RequirementResults.OrderBy( r => r.GroupRequirement.GroupRequirementType.Name ) )
                {
                    // Set up Security or Override access.
                    var currentPersonIsLeaderOfCurrentGroup = groupMember.Group.Members.Where( m => m.GroupRole.IsLeader ).Select( m => m.PersonId ).Contains( currentPerson.Id );
                    bool leaderCanOverride = requirementStatus.GroupRequirement.AllowLeadersToOverride && currentPersonIsLeaderOfCurrentGroup;

                    var hasPermissionToOverride = requirementStatus.GroupRequirement.GroupRequirementType.IsAuthorized( Rock.Security.Authorization.OVERRIDE, currentPerson );

                    var card = new GroupMemberRequirementCard( requirementStatus.GroupRequirement.GroupRequirementType, leaderCanOverride || hasPermissionToOverride )
                    {
                        Title = requirementStatus.GroupRequirement.GroupRequirementType.Name,
                        TypeIconCssClass = requirementStatus.GroupRequirement.GroupRequirementType.IconCssClass,
                        MeetsGroupRequirement = requirementStatus.MeetsGroupRequirement,
                        GroupMemberRequirementId = requirementStatus.GroupMemberRequirementId,
                        GroupRequirementId = requirementStatus.GroupRequirement.Id,
                        GroupMemberId = GroupMemberId,
                        GroupMemberRequirementDueDate = requirementStatus.RequirementDueDate,
                        WorkflowEntryLinkedPageValue = WorkflowEntryLinkedPageValue,
                        IsSummaryHidden = IsSummaryHidden
                    };

                    columnControl.Controls.Add( card );
                }

                categoryControl.Controls.Add( columnControl );
                this.Controls.Add( categoryControl );
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
            ViewState[ViewStateKey.GroupMemberId] = this._groupMemberId;
            return base.SaveViewState();
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            this._groupMemberId = Convert.ToInt32( ViewState[ViewStateKey.GroupMemberId] );
        }
    }
}
