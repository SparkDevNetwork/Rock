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
using System.Web.UI;

using System.Linq;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Rock.Model;
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
        /// Gets or sets the selected integer for the Group Member's Group Role Id.
        /// </summary>
        public int? SelectedGroupRoleId { get; set; }

        /// <summary>
        /// The enumerated collection of Group Requirement Statuses for the container.
        /// </summary>
        public IEnumerable<GroupRequirementStatus> RequirementStatuses
        {
            get
            {
                return _requirementStatuses;
            }

            set
            {
                _requirementStatuses = value;
                RecreateChildControls();
            }
        }

        private IEnumerable<GroupRequirementStatus> _requirementStatuses = new List<GroupRequirementStatus>();

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
            public const string RequirementStatuses = "RequirementStatuses";
        }
        #endregion ViewStateKeys

        /// <summary>
        /// Have this control render as a div instead of a span
        /// </summary>
        /// <value>The tag key.</value>
        protected override HtmlTextWriterTag TagKey => HtmlTextWriterTag.Div;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        /// <summary>
        /// DataBind for the Requirements Container.
        /// </summary>
        public override void DataBind()
        {
            base.DataBind();
        }

        /// <summary>
        /// Create the Requirement Statuses to be created as Group Member Requirement Cards.
        /// </summary>
        /// <param name="groupMemberId"></param>
        /// <param name="currentPersonIsLeaderOfCurrentGroup"></param>
        /// <param name="isInteractionDisabled"></param>
        public void CreateRequirementStatusControls( int groupMemberId, bool currentPersonIsLeaderOfCurrentGroup, bool isInteractionDisabled )
        {
            if ( !this.Visible )
            {
                return;
            }
            // set class of the parent control
            this.AddCssClass( "group-member-requirements-container" );

            Controls.Clear();

            IEnumerable<GroupRequirementStatus> groupRequirementStatuses = new List<GroupRequirementStatus>();

            // If the container has a collection of requirement statuses, use them.
            if ( RequirementStatuses != null )
            {
                groupRequirementStatuses = RequirementStatuses;
            }

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
                var categoryName = requirementCategory.Name;
                var categoryId = requirementCategory.CategoryId;
                HtmlGenericControl categoryControl = new HtmlGenericControl( "div" );
                categoryControl.AddCssClass( "row d-flex flex-wrap requirement-category requirement-category-" + categoryId.ToString() );


                if ( Visible && categoryName.IsNotNullOrWhiteSpace() )
                {
                    HtmlGenericControl columnControl = new HtmlGenericControl( "div" );
                    columnControl.AddCssClass( "col-xs-12" );

                    HtmlGenericControl headerControl = new HtmlGenericControl( "h5" );
                    headerControl.InnerText = categoryName;
                    columnControl.Controls.Add( headerControl );
                    categoryControl.Controls.Add( columnControl );
                }

                var currentPerson = this.RockBlock().CurrentPerson;

                // Set up Security or Override access.

                // Add the Group Member Requirement Cards here.
                foreach ( var requirementStatus in requirementCategory.RequirementResults.OrderBy( r => r.GroupRequirement.GroupRequirementType.Name ) )
                {
                    bool leaderCanOverride = requirementStatus.GroupRequirement.AllowLeadersToOverride && currentPersonIsLeaderOfCurrentGroup;

                    var hasPermissionToOverride = requirementStatus.GroupRequirement.GroupRequirementType.IsAuthorized( Rock.Security.Authorization.OVERRIDE, currentPerson );

                    var isAuthorized = requirementStatus.GroupRequirement.GroupRequirementType.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson );

                    // Do not render cards where the current person is not authorized, or the status is "Not Applicable" or "Error".
                    if ( isAuthorized && requirementStatus.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable && requirementStatus.MeetsGroupRequirement != MeetsGroupRequirement.Error )
                    {
                        var card = new GroupMemberRequirementCard( requirementStatus.GroupRequirement.GroupRequirementType, leaderCanOverride || hasPermissionToOverride )
                        {
                            Title = requirementStatus.GroupRequirement.GroupRequirementType.Name,
                            TypeIconCssClass = requirementStatus.GroupRequirement.GroupRequirementType.IconCssClass,
                            MeetsGroupRequirement = requirementStatus.MeetsGroupRequirement,
                            GroupMemberRequirementId = requirementStatus.GroupMemberRequirementId,
                            GroupRequirementId = requirementStatus.GroupRequirement.Id,
                            GroupMemberRequirementDueDate = requirementStatus.RequirementDueDate,
                            WorkflowEntryLinkedPageValue = WorkflowEntryLinkedPageValue,
                            IsSummaryHidden = IsSummaryHidden,
                            GroupMemberId = groupMemberId,
                            IsInteractionDisabled = isInteractionDisabled
                        };

                        categoryControl.Controls.Add( card );
                    }
                }


                this.Controls.Add( categoryControl );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {

        }
    }
}
