using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotLiquid;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace Geode.RockUtils.GroupRequirements.Blocks
{
    /// <summary>
    /// Displays details about a person's pending groups.
    /// </summary>
    [DisplayName( "My Pending Groups Lava" )]
    [Category( "Groups" )]
    [Description( "Displays details about a person's pending groups." )]

    #region Block Attributes

    [CodeEditorField(
        "Content",
        Description = "The content to render.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 600,
        IsRequired = true,
        DefaultValue = @"{% for item in PendingGroups %}
    <h2>{{ item.Group.Name }}</h2>
    <h4>Requirements:</h4>
    <ul>
        {% for status in item.Requirements %}
            <li>{{ status.GroupRequirement.GroupRequirementType.Name }} - {{ status.MeetsGroupRequirement }}</li>
        {% endfor %}
    </ul>
{% endfor %}",
        Order = 0,
        Key = AttributeKey.Content )]
    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this HTML block.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.EnabledLavaCommands )]

    #endregion Block Attributes
    public partial class MyPendingGroupsLava : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Content = "Content";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        #endregion Attribute Keys

        #region Controls

        protected Literal lLavaOutput;
        protected UpdatePanel upnlContent;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                ShowDetail();
            }
        }

        #endregion

        #region Methods
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();
        }

        private void ShowDetail()
        {
            if ( CurrentPerson != null )
            {
                RockContext rockContext = new RockContext();

                // Pending groups come from 2 sources:
                // 1. Active connection requests
                // 2. Pending group memberships
                List<PendingGroup> pendingGroups = new List<PendingGroup>();

                ConnectionRequestService connectionRequestService = new ConnectionRequestService( rockContext );

                var connectionRequests = connectionRequestService
                    .Queryable()
                    .AsNoTracking()
                    .Include( x => x.AssignedGroup )
                    .Where( connectionRequest =>
                        connectionRequest.PersonAlias.PersonId == CurrentPerson.Id // Current Person
                        && connectionRequest.ConnectionState == ConnectionState.Active // Active
                        && connectionRequest.AssignedGroupId.HasValue // Has Group Assigned
                    );

                foreach ( var connectionRequest in connectionRequests )
                {
                    pendingGroups.Add( new PendingGroup
                    {
                        ConnectionRequest = connectionRequest,
                        GroupMember = null,
                        Group = connectionRequest.AssignedGroup,
                        Requirements = connectionRequest.AssignedGroup.PersonMeetsGroupRequirements( rockContext, CurrentPerson.Id, connectionRequest.AssignedGroupMemberRoleId )
                    } );
                }

                GroupMemberService groupMemberService = new GroupMemberService( rockContext );

                var groupMemberships = groupMemberService
                    .Queryable()
                    .AsNoTracking()
                    .Include( gm => gm.Group )
                    .Where( groupMembership =>
                        !groupMembership.IsArchived // Not Archived
                        && groupMembership.Group.IsActive // Group Active
                        && !groupMembership.Group.IsArchived // Group Not Archived
                        && groupMembership.PersonId == CurrentPerson.Id // Current Person
                        && groupMembership.GroupMemberStatus == GroupMemberStatus.Pending // Pending Status
                    );

                foreach ( var groupMembership in groupMemberships )
                {
                    pendingGroups.Add( new PendingGroup
                    {
                        ConnectionRequest = null,
                        GroupMember = groupMembership,
                        Group = groupMembership.Group,
                        Requirements = groupMembership.Group.PersonMeetsGroupRequirements( rockContext, CurrentPerson.Id, groupMembership.GroupRoleId )
                    } );
                }

                // Get the block settings
                string lavaTemplate = GetAttributeValue( AttributeKey.Content );
                string enabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

                // Build the merge fields
                Dictionary<string, object> mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "PendingGroups", pendingGroups );

                // Render the output
                lLavaOutput.Text = lavaTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands );
            }
        }

        #endregion

        public class PendingGroup : Drop
        {
            public ConnectionRequest ConnectionRequest { get; set; }
            public GroupMember GroupMember { get; set; }
            public Group Group { get; set; }
            public IEnumerable<PersonGroupRequirementStatus> Requirements { get; set; }
        }
    }
}