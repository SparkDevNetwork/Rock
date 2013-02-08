//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( 0, "Treeview Title", "", "", false, "Group Tree View" )]
    [GroupTypesField( 1, "Group Types", false, "", "", "", "Select group types to show in this block.  Leave all unchecked to show all group types." )]
    [GroupField( 2, "Group", false, "", "", "", "Select the root group to show in this block." )]
    [BooleanField( 3, "Limit to Security Role Groups", false )]
    [DetailPage]
    public partial class GroupTreeView : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            hfLimitToSecurityRoleGroups.Value = GetAttributeValue( "LimittoSecurityRoleGroups" );
            hfRootGroupId.Value = GetAttributeValue( "Group" );
            string groupTypes = GetAttributeValue( "GroupTypes" );
            groupTypes = string.IsNullOrWhiteSpace(groupTypes) ? "0" : groupTypes;
            hfGroupTypes.Value = groupTypes;
            string groupId = PageParameter( "groupId" );
            if ( !string.IsNullOrWhiteSpace( groupId ) )
            {
                hfInitialGroupId.Value = groupId;
                hfSelectedGroupId.Value = groupId.ToString();
                Group group = ( new GroupService() ).Get( int.Parse( groupId ) );
                List<string> parentIdList = new List<string>();
                while ( group != null )
                {
                    group = group.ParentGroup;
                    if ( group != null )
                    {
                        parentIdList.Insert( 0, group.Id.ToString() );
                    }
                }

                hfInitialGroupParentIds.Value = parentIdList.AsDelimited( "," );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddGroup_Click( object sender, EventArgs e )
        {
            int groupId = hfSelectedGroupId.ValueAsInt();
            NavigateToDetailPage( "groupId", 0, "parentGroupId", groupId );
        }
}
}