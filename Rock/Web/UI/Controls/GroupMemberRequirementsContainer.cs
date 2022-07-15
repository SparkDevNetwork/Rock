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
    public class GroupMemberRequirementsContainer : Control, INamingContainer
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string RequirementsCategories { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int GroupMemberId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<GroupMemberRequirement> Requirements { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string WorkflowEntryPage { get; set; }

        /// <summary>
        /// If true, recalculate the requirements of this group member, otherwise load existing requirements. 
        /// </summary>
        public bool ForceRefreshRequirements { get; set; }

        #endregion Properties

        ///// <summary>
        ///// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        ///// </summary>
        ///// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        //protected override void OnInit( System.EventArgs e )
        //{
        //    EnsureChildControls();
        //    base.OnInit( e );
        //}

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
            if ( !this.Visible )
            {
                return;
            }

            base.CreateChildControls();
            Controls.Clear();

            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( GroupMemberId );
            var attributeValueService = new AttributeValueService( rockContext );
            IEnumerable<GroupRequirementStatus> groupRequirementStatuses;
            if ( ForceRefreshRequirements )
            {
                groupRequirementStatuses = groupMember.Group.PersonMeetsGroupRequirements( rockContext, groupMember.PersonId, groupMember.GroupRoleId );
            }
            else
            {
                groupRequirementStatuses = groupMember.GetGroupRequirementsStatuses( rockContext );
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

            int index = 1;
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

                //TO-DO set up security access here
                var hasPermissionToOverride = false;

                // Add the Group Member Requirement Cards here.
                foreach ( var requirementStatus in requirementCategory.RequirementResults.OrderBy( rr => rr.MeetsGroupRequirement ).ThenBy( r => r.GroupRequirement.GroupRequirementType.Name ) )
                {
                    var card = new GroupMemberRequirementCard( requirementStatus.GroupRequirement.GroupRequirementType, requirementStatus.GroupRequirement.AllowLeadersToOverride || hasPermissionToOverride )
                    {
                        Title = requirementStatus.GroupRequirement.GroupRequirementType.Name,
                        TypeIconCssClass = requirementStatus.GroupRequirement.GroupRequirementType.IconCssClass,
                        MeetsGroupRequirement = requirementStatus.MeetsGroupRequirement,
                        GroupMemberRequirementId = requirementStatus.GroupMemberRequirementId,
                        GroupRequirementId = requirementStatus.GroupRequirement.Id,
                        GroupMemberId = GroupMemberId,
                        GroupMemberRequirementDueDate = requirementStatus.RequirementDueDate,
                        WorkflowEntryPage = WorkflowEntryPage
                    };
                    columnControl.Controls.Add( card );

                    //card.RenderControl( writer );
                    index++;
                }

                categoryControl.Controls.Add( columnControl );
                this.Controls.Add( categoryControl );
            }
        }
    }
}
