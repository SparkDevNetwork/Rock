using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Lava;
using System.Runtime.Caching;
using church.ccv.Datamart.Model;

namespace Plugins.church_ccv.Groups
{
    [DisplayName( "CCV Group List Personalized Lava" )]
    [Category( "CCV > Groups" )]
    [Description( "Lists all group that the person is a member of using a Lava template. CCV Customized to include features for coach toolboxes" )]

    [GroupField( "Parent Group", "If a group is chosen, only the groups under this group will be displayed.", false, "", "", 2 )]
    [IntegerField( "Cache Duration", "Length of time in seconds to cache which groups are descendants of the parent group.", false, 3600, "", 3 )]
    [GroupTypesField( "Include Group Types", "The group types to display in the list.  If none are selected, all group types will be included.", false, "", "", 4 )]
    [GroupTypesField( "Exclude Group Types", "The group types to exclude from the list (only valid if including all groups).", false, "", "", 5 )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupListSidebar.lava' %}", "", 6 )]
    [CodeEditorField( "Add Group Member Panel Pre HTML", "The pre lava template to use to wrap the add group member panel.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, "", "", 7 )]
    [CodeEditorField( "Add Group Member Panel Post HTML", "The pst lava template to use to wrap the add group member panel.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, "", "", 8 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 9 )]
    public partial class CCVGroupListPersonalizedLava : RockBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( Page.IsPostBack )
            {
                // DOM is reset to default during postback
                // ReApply panel css classes to show correct panel
                pnlGroupView.CssClass = hfGroupViewCSS.Value;
                pnlAddGroupMember.CssClass = hfAddGroupMemberCSS.Value;
            }

            if ( !Page.IsPostBack )
            {
                // populate the member statuses, but remove InActive
                rblStatus.BindToEnum<GroupMemberStatus>();
                var inactiveItem = rblStatus.Items.FindByValue( ( ( int ) GroupMemberStatus.Inactive ).ToString() );
                if ( inactiveItem != null )
                {
                    rblStatus.Items.Remove( inactiveItem );
                }

                // set default values
                ppGroupMemberPerson.SetValue( null );
                rblStatus.SetValue( ( int ) GroupMemberStatus.Active );

                ListGroups();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ListGroups();
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Handles the Click event of the btnSaveGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveGroupMember_Click( object sender, EventArgs e )
        {
            // Check for selected person and member status
            if ( Page.IsValid && ppGroupMemberPerson.SelectedValue.HasValue && rblStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>().HasValue )
            {
                // Check that groupId and defaultGroupId hidden fields have values
                if ( hfGroupId.ValueAsInt() != 0 && hfDefaultGroupRoleId.ValueAsInt() != 0)
                {
                    // Try to add member to the group
                    bool result = AddGroupMember( 
                                    ppGroupMemberPerson.SelectedValue.Value,
                                    hfGroupId.ValueAsInt(),
                                    hfDefaultGroupRoleId.ValueAsInt(),
                                    rblStatus.SelectedValueAsEnum<GroupMemberStatus>() );

                    if ( result )
                    {
                        // success - hide add panel, clear any error messages, and refresh groups list
                        pnlGroupView.CssClass = "";
                        hfGroupViewCSS.Value = "";
                        pnlAddGroupMember.CssClass = "hidden";
                        hfAddGroupMemberCSS.Value = "hidden";

                        nbGroupMemberErrorMessage.Text = "";

                        ListGroups();
                    }
                    else
                    {
                        // failed - don't add and show generic error message
                        var person = new PersonService( new RockContext() ).Get( ( int ) ppGroupMemberPerson.PersonId );

                        nbGroupMemberErrorMessage.Text = string.Format(
                            "{0} already belongs to this group and cannot be added again.", person.FullName );
                    }
                }
            }
        }

        #endregion

        #region Internal Methods

        private void ListGroups()
        {
            RockContext rockContext = new RockContext();

            var qry = new GroupMemberService( rockContext )
                        .Queryable( "Group" );

            var parentGroupGuid = GetAttributeValue( "ParentGroup" ).AsGuidOrNull();
            if ( parentGroupGuid != null )
            {
                var availableGroupIds = ( List<int> ) GetCacheItem( "GroupListPersonalizedLava:" + parentGroupGuid.ToString() );

                if ( availableGroupIds == null )
                {
                    var parentGroup = new GroupService( rockContext ).Get( parentGroupGuid ?? new Guid() );
                    if ( parentGroup != null )
                    {
                        availableGroupIds = GetChildGroups( parentGroup ).Select( g => g.Id ).ToList();
                    }
                    else
                    {
                        availableGroupIds = new List<int>();
                    }
                    var cacheLength = GetAttributeValue( "CacheDuration" ).AsInteger();
                    AddCacheItem( "GroupListPersonalizedLava:" + parentGroupGuid.ToString(), availableGroupIds, cacheLength );
                }
                qry = qry.Where( m => availableGroupIds.Contains( m.GroupId ) );
            }

            qry = qry.Where( m => m.PersonId == CurrentPersonId
                        && m.GroupMemberStatus == GroupMemberStatus.Active
                        && m.Group.IsActive == true );

            List<Guid> includeGroupTypeGuids = GetAttributeValue( "IncludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( includeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( t => includeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
            }

            List<Guid> excludeGroupTypeGuids = GetAttributeValue( "ExcludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( excludeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( t => !excludeGroupTypeGuids.Contains( t.Group.GroupType.Guid ) );
            }

            var groups = new List<GroupInvolvementSummary>();

            foreach ( var groupMember in qry.ToList() )
            {
                groups.Add( new GroupInvolvementSummary
                {
                    Group = groupMember.Group,
                    Role = groupMember.GroupRole.Name,
                    IsLeader = groupMember.GroupRole.IsLeader,
                    CanView = groupMember.GroupRole.CanView,
                    CanEdit = groupMember.GroupRole.CanEdit,
                    GroupType = groupMember.Group.GroupType.Name
                } );
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Groups", groups );

            // Get CurrentPerson's Associate Pastor and Neighborhood info for Lava 
            if (CurrentPerson != null)
            {
                // object for merge
                DatamartSummary datamartSummary = new DatamartSummary();

                // Services needed
                var datamartPersonService = new Service<DatamartPerson>( rockContext );
                var personService = new Service<Person>( rockContext );

                // get current persons datamart data (verify that there IS a datamartPerson. If they are added to the system AND a group before the datamart runs, this could be null)
                var qryDatamartPerson = datamartPersonService.Queryable().Where( a => a.PersonId == CurrentPersonId ).FirstOrDefault();
                if( qryDatamartPerson != null )
                {
                    // Add associate pastor object if exists in datamart
                    int neighborhoodPastorId = qryDatamartPerson.NeighborhoodPastorId ?? 0;
                    if ( neighborhoodPastorId > 0 )
                    {
                        datamartSummary.AssociatePastor = personService.Get( neighborhoodPastorId );
                    }

                    // Add region info if exists in datamart
                    //(purposesly didnt add as a group object for performance, can always look up group with lava)
                    datamartSummary.RegionName = qryDatamartPerson.NeighborhoodName;
                    datamartSummary.RegionId = qryDatamartPerson.NeighborhoodId ?? 0;

                    mergeFields.Add( "DatamartSummary", datamartSummary );
                }
            }

            string template = GetAttributeValue( "LavaTemplate" );

            // show debug info
            bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();
            if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }

            lContent.Text = template.ResolveMergeFields( mergeFields );

            // Apply Pre / Post HTML to add group member panel
            ApplyAddGroupMemberPreHTML( mergeFields );
            ApplyAddGroupMemberPostHTML( mergeFields );
        }

        /// <summary>
        /// Recursively loads all descendants of the group
        /// </summary>
        /// <param name="group">Group to load from</param>
        /// <returns></returns>
        private List<Group> GetChildGroups( Group group )
        {
            List<Group> childGroups = group.Groups.ToList();
            List<Group> grandChildGroups = new List<Group>();
            foreach ( var childGroup in childGroups )
            {
                grandChildGroups.AddRange( GetChildGroups( childGroup ) );
            }
            childGroups.AddRange( grandChildGroups );
            return childGroups;
        }

        /// <summary>
        /// Apply add group member pre HTML lava template
        /// </summary>
        private void ApplyAddGroupMemberPreHTML( Dictionary<string, object> mergeFields )
        {
            string template = GetAttributeValue( "AddGroupMemberPanelPreHTML" );

            lAddGroupMemberPreHTML.Text = template.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Apply add group member post HTML lava template
        /// </summary>
        private void ApplyAddGroupMemberPostHTML( Dictionary<string, object> mergeFields )
        {
            string template = GetAttributeValue( "AddGroupMemberPanelPostHTML" );

            lAddGroupMemberPostHTML.Text = template.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Add person to group
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="groupId"></param>
        /// <param name="groupRoleId"></param>
        /// <param name="memberStatus"></param>
        /// <returns></returns>
        protected bool AddGroupMember( int personId, int groupId, int groupRoleId, GroupMemberStatus memberStatus )
        {
            var rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            // check to see if the person is already a member of this group
            var existingGroupMember = groupMemberService.GetByGroupIdAndPersonId( groupId, personId ).SingleOrDefault();
            if ( existingGroupMember == null )
            {
                GroupMember groupMember = new GroupMember { Id = 0 };
                groupMember.GroupId = groupId;
                groupMember.PersonId = personId;

                // set their role
                GroupTypeRole role = new GroupTypeRoleService( rockContext ).Get( groupRoleId );
                groupMember.GroupRoleId = role.Id;

                // set their status.
                groupMember.GroupMemberStatus = memberStatus;

                // using WrapTransaction because there are two Saves
                rockContext.WrapTransaction( () =>
                {
                    groupMemberService.Add( groupMember );

                    rockContext.SaveChanges();
                    groupMember.SaveAttributeValues( rockContext );
                } );

                Group group = new GroupService( rockContext ).Get( groupMember.GroupId );
                if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                {
                    Rock.Security.Role.Flush( group.Id );
                }

                return true;
            }

            return false;
        }

        [DotLiquid.LiquidType( "Group", "Role", "IsLeader", "GroupType", "CanView", "CanEdit" )]
        public class GroupInvolvementSummary
        {
            public Group Group { get; set; }
            public string Role { get; set; }
            public bool IsLeader { get; set; }
            public string GroupType { get; set; }
            public bool CanView { get; set; }
            public bool CanEdit { get; set; }
        }

        [DotLiquid.LiquidType( "AssociatePastor", "RegionName", "RegionId" )]
        public class DatamartSummary
        {
            public Person AssociatePastor { get; set; }
            public string RegionName { get; set; }
            public int? RegionId { get; set; }
        }

        #endregion
    }
}