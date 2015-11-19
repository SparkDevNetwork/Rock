using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace com.reallifeministries
{
    [DisplayName( "SubGroupInfo (RLM Custom)" )]
    [Category( "Groups" )]
    [Description( "Lists all the sub groups of the given group & member counts" )]

    [GroupField( "Group", "Either pick a specific group or choose <none> to have group be determined by the groupId page parameter" )]
    [GroupTypesField("Include Group Types","Only count members and groups from of group types",false)]
    [TextField("Sub Group Term","What do you call a 'sub-group' (heading for sub-groups count)?",true,"Group")]
    [LinkedPage( "Detail Page","A detail page to go to when a group is clicked. Default same page", false )]
    public partial class SubGroupInfo : Rock.Web.UI.RockBlock, ISecondaryBlock
    {
        #region Private Variables

        private Group _group = null;
        private bool _canView = false;
        private string _groupTerm = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // if this block has a specific GroupId set, use that, otherwise, determine it from the PageParameters
            Guid groupGuid = GetAttributeValue( "Group" ).AsGuid();
            int groupId = 0;

            if (groupGuid == Guid.Empty)
            {
                groupId = PageParameter( "GroupId" ).AsInteger();
            }

            if (!(groupId == 0 && groupGuid == Guid.Empty))
            {
                string key = string.Format( "Group:{0}", groupId );
                _group = RockPage.GetSharedItem( key ) as Group;
                if (_group == null)
                {
                    _group = new GroupService( new RockContext() ).Queryable( "GroupType.Roles" )
                        .Where( g => g.Id == groupId || g.Guid == groupGuid )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, _group );
                }

                if (_group != null && _group.IsAuthorized( Authorization.VIEW, CurrentPerson ))
                {
                    _canView = true;
                }
                pnlContent.Visible = _canView;
            }
        } 

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                
                if ( _canView )
                {
                    BindSubGroupsGrid();
                }
            }
        }

        #endregion

        #region SubGroupsGrid

         /// <summary>
        /// Handles the GridRebind event of the gSubGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gSubGroups_GridRebind( object sender, EventArgs e )
        {
            BindSubGroupsGrid();
        }

        #endregion

        #region Internal Methods

        protected void BindSubGroupsGrid()
        {
            if (_group != null)
            {
                var rockContext = new RockContext();
                

                var subGroups = rockContext.Groups.Where( g => g.ParentGroupId == _group.Id ).ToList();
                if (subGroups.Count > 0)
                {
                    List<Guid> groupTypeIncludeGuids = GetAttributeValue( "IncludeGroupTypes" ).SplitDelimitedValues().AsGuidList();
                    var groupService = new GroupService( rockContext );

                    gSubGroups.Columns[1].HeaderText = GetAttributeValue( "SubGroupTerm" ).Pluralize(); 
                    ;

                    if (groupTypeIncludeGuids.Any())
                    {
                        var subGroupsAndSubMembers = (
                            from g in subGroups
                            let groupIds = (groupService.GetAllDescendents( g.Id ).Select( a => a.Id ).ToList().Concat( new List<int>() { g.Id } ))
                            select new
                            {
                                Group = g,
                                Members = rockContext.GroupMembers.Where( m => groupIds.Contains( m.GroupId ) )
                                                .Where(m=>groupTypeIncludeGuids.Contains(m.Group.GroupType.Guid)).ToList()
                            });

                        var subGroupMemberCounts = subGroupsAndSubMembers.OrderBy( a => a.Group.Name ).Select( g => new
                        {
                            GroupId = g.Group.Id,
                            Group = g.Group,
                            PendingMembers = g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Pending ).Count(),
                            ActiveMembers = g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Count()
                        } ).ToList();

                        gSubGroups.DataSource = subGroupMemberCounts;
                        gSubGroups.DataBind();
                    }
                    else
                    {
                        var subGroupsAndSubMembers = (
                            from g in subGroups
                            let groupIds = (groupService.GetAllDescendents( g.Id ).Select( a => a.Id ).ToList().Concat( new List<int>() { g.Id } ))
                            select new
                            {
                                Group = g,
                                Members = rockContext.GroupMembers.Where( m => groupIds.Contains( m.GroupId ) ).ToList()
                            });

                        var subGroupMemberCounts = subGroupsAndSubMembers.OrderBy( a => a.Group.Name ).Select( g => new
                        {
                            GroupId = g.Group.Id,
                            Group = g.Group,
                            PendingMembers = g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Pending ).Count(),
                            ActiveMembers = g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Count()
                        } ).ToList();

                        gSubGroups.DataSource = subGroupMemberCounts;
                        gSubGroups.DataBind();
                    }
                    
                }  
                else
                {
                    pnlContent.Visible = false;
                }                  
            }
            else
            {
                pnlContent.Visible = false; ;
            }
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion

        protected void gSubGroups_RowSelected(object sender, RowEventArgs e)
        {
            var detailPage = GetAttributeValue( "DetailPage" );
                if (! String.IsNullOrEmpty(detailPage) )
                {
                    NavigateToLinkedPage( "DetailPage", "GroupId", e.RowKeyId );

                }
                else
                {
                    var pf = RockPage.PageReference;
                    var qs = pf.Parameters;
                    qs.AddOrReplace("GroupId",e.RowKeyId.ToString());
                    NavigateToPage( RockPage.Guid,qs );
                }
                
        }
    }
    
}
