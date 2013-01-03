//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [TextField(0, "Treeview Title", "", "", false, "Group Tree View")]
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
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            GroupService groupService = new GroupService();
            var qry = groupService.Queryable();

            List<int> groupTypeIds = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();
            if ( groupTypeIds.Count > 0 )
            {
                qry = qry.Where( a => groupTypeIds.Contains( a.GroupTypeId ) );
            }

            if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).FromTrueFalse() )
            {
                qry = qry.Where( a => a.IsSecurityRole );
            }

            ltlTreeViewTitle.Text = GetAttributeValue( "TreeviewTitle" );

            List<Group> allGroupItems = qry.ToList();

            string rootGroupTypeId = GetAttributeValue( "GroupType" );

            WebControl treeviewList = new WebControl( HtmlTextWriterTag.Ul ) { ID = "treeviewGroups" };

            if ( !string.IsNullOrWhiteSpace( rootGroupTypeId ) )
            {
                Group parentGroup = groupService.Get( int.Parse( rootGroupTypeId ) );
                WebControl node = AddNode( treeviewList, parentGroup );
                AddChildNodes( node, parentGroup, allGroupItems );
            }
            else
            {
                foreach ( var parentGroup in groupService.Queryable().Where( a => a.ParentGroupId == null ).OrderBy( a => a.Name ) )
                {
                    WebControl node = AddNode( treeviewList, parentGroup );
                    AddChildNodes( node, parentGroup, allGroupItems );
                }
            }

            phGroupTree.Controls.Add( treeviewList );

            string script = @"
$(document).ready(function () {                
  
    $('.groupTreeview').kendoTreeView();
    var treeview = $('.groupTreeview').data('kendoTreeView');
    treeview.expand('*');

});
            ";

            ScriptManager.RegisterStartupScript( treeviewList, treeviewList.GetType(), "kendotreeviewscript", script, true );
        }

        /// <summary>
        /// Adds the node.
        /// </summary>
        /// <param name="treeviewList">The treeview list.</param>
        /// <param name="group">The group.</param>
        private WebControl AddNode( WebControl treeviewList, Group group )
        {
            WebControl node = new WebControl( HtmlTextWriterTag.Li );
            treeviewList.Controls.Add( node );

            LinkButton groupItem = new LinkButton();
            groupItem.Attributes["groupId"] = group.Id.ToString();
            groupItem.Click += groupItem_Click;
            groupItem.Text = group.Name;
            groupItem.ID = string.Format( "groupItem_{0}", group.Guid.ToString() );
            node.Controls.Add( groupItem );

            return node;
        }

        /// <summary>
        /// Handles the Click event of the groupItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void groupItem_Click( object sender, EventArgs e )
        {
            WebControl groupItem = sender as WebControl;
            if ( groupItem != null )
            {
                var groupId = groupItem.Attributes["groupId"];
                NavigateToDetailPage( "groupId", int.Parse( groupId ) );
            }
        }

        /// <summary>
        /// Adds the child nodes.
        /// </summary>
        /// <param name="nodeHtml">The node HTML.</param>
        /// <param name="parentGroup">The parent group.</param>
        /// <param name="groupList">The group list.</param>
        protected void AddChildNodes( WebControl parentNode, Rock.Model.Group parentGroup, List<Rock.Model.Group> groupList )
        {
            var childGroups = groupList.Where( a => a.ParentGroupId.Equals( parentGroup.Id ) );
            if ( childGroups.Count() > 0 )
            {
                WebControl childTree = new WebControl( HtmlTextWriterTag.Ul );
                parentNode.Controls.Add( childTree );

                foreach ( var childGroup in childGroups.OrderBy( a => a.Name ) )
                {
                    WebControl childNode = AddNode( childTree, childGroup );
                    AddChildNodes( childNode, childGroup, groupList );
                }
            }
        }
    }
}