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
            var groupRequirementStatuses = groupMember.GetGroupRequirementsStatuses( rockContext );
            //var groupMemberRequirement = groupMember.GroupMemberRequirements;
            var requirementCategories = groupRequirementStatuses
            .Select( rr => new
            {
                CategoryId = rr.GroupRequirement.GroupRequirementType.CategoryId,
                Name = rr.GroupRequirement.GroupRequirementType.CategoryId.HasValue ? rr.GroupRequirement.GroupRequirementType.Category.Name : string.Empty,
                RequirementResults = groupRequirementStatuses.Where( gr => gr.GroupRequirement.GroupRequirementType.CategoryId == rr.GroupRequirement.GroupRequirementType.CategoryId )
            } ).DistinctBy( c => c.CategoryId );

            int index = 1;
            foreach ( var requirementCategory in requirementCategories.OrderByDescending( a => a.Name ) )
            {
                HtmlGenericControl categoryControl = new HtmlGenericControl( "div" );
                categoryControl.AddCssClass( "row" );
                var categoryName = requirementCategory.Name;

                //this.Controls.Add( categoryControl );

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

                // Add the custom Cards here.

                foreach ( var requirementStatus in requirementCategory.RequirementResults.OrderBy( rr => rr.MeetsGroupRequirement ) )
                {

                    var card = new GroupMemberRequirementCard( requirementStatus.GroupRequirement.GroupRequirementType, requirementStatus.GroupRequirement.AllowLeadersToOverride || hasPermissionToOverride )
                    {
                        Title = requirementStatus.GroupRequirement.GroupRequirementType.Name,
                        TypeIconCssClass = requirementStatus.GroupRequirement.GroupRequirementType.IconCssClass,
                        MeetsGroupRequirement = requirementStatus.MeetsGroupRequirement,
                        GroupMemberRequirementId = requirementStatus.GroupMemberRequirementId,
                        GroupRequirementId = requirementStatus.GroupRequirement.Id,
                        GroupMemberId = GroupMemberId,
                        GroupMemberRequirementDueDate = CalculateGroupMemberRequirementDueDate(
                            requirementStatus.GroupRequirement.GroupRequirementType.DueDateType,
                            requirementStatus.GroupRequirement.GroupRequirementType.DueDateOffsetInDays,
                            requirementStatus.GroupRequirement.DueDateStaticDate,
                            requirementStatus.GroupRequirement.DueDateAttributeId.HasValue ? attributeValueService.GetByAttributeIdAndEntityId( requirementStatus.GroupRequirement.DueDateAttributeId.Value, groupMember.GroupId ).Value.AsDateTime() : null,
                groupMember.DateTimeAdded )
                    };
                    columnControl.Controls.Add( card );

                    //card.RenderControl( writer );
                    index++;
                }

                categoryControl.Controls.Add( columnControl );
                this.Controls.Add( categoryControl );
            }
        }

        private DateTime? CalculateGroupMemberRequirementDueDate( DueDateType dueDateType, int? dueDateOffsetInDays, DateTime? dueDateStaticDate, DateTime? dueDateFromGroupAttribute, DateTime? dueDateGroupMemberAdded )
        {
            switch ( dueDateType )
            {
                case DueDateType.ConfiguredDate:
                    if ( dueDateStaticDate.HasValue )
                    {
                        return dueDateStaticDate.Value;
                    }

                    return null;
                case DueDateType.GroupAttribute:
                    if ( dueDateFromGroupAttribute.HasValue )
                    {
                        return dueDateFromGroupAttribute.Value.AddDays( dueDateOffsetInDays.HasValue ? dueDateOffsetInDays.Value : 0 );
                    }

                    return null;
                case DueDateType.DaysAfterJoining:
                    if ( dueDateGroupMemberAdded.HasValue )
                    {
                        return dueDateGroupMemberAdded.Value.AddDays( dueDateOffsetInDays.HasValue ? dueDateOffsetInDays.Value : 0 );
                    }

                    return null;

                case DueDateType.Immediate:
                default:
                    return null;
            }
        }
    }
}
