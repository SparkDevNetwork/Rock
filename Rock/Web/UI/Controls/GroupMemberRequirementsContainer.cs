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



        /// <summary>
        /// Renders the tag to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
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
                var categoryName = requirementCategory.Name;

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-12" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                if ( Visible && categoryName.IsNotNullOrWhiteSpace() )
                {

                    writer.RenderBeginTag( HtmlTextWriterTag.H5 );
                    writer.Write( categoryName );

                    // End heading tag.
                    writer.RenderEndTag();


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
                        GroupMemberRequirementDueDate = CalculateGroupMemberRequirementDueDate(
                            requirementStatus.GroupRequirement.GroupRequirementType.DueDateType,
                            requirementStatus.GroupRequirement.GroupRequirementType.DueDateOffsetInDays,
                            requirementStatus.GroupRequirement.DueDateStaticDate,
                            requirementStatus.GroupRequirement.DueDateAttributeId.HasValue ? attributeValueService.GetByAttributeIdAndEntityId( requirementStatus.GroupRequirement.DueDateAttributeId.Value, groupMember.GroupId ).Value.AsDateTime() : null,
                groupMember.DateTimeAdded )
                    };
                    this.Controls.Add( card );

                    //card.RenderControl( writer );
                    index++;
                }

                // End Div Col tag.
                writer.RenderEndTag();
                base.RenderControl( writer );
                // End of row div tag.
                writer.RenderEndTag();
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
