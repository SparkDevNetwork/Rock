// <copyright>
// Copyright by BEMA Information Technologies
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
//
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web;

namespace RockWeb.com_bemaservices.CheckIn
{
    [DisplayName ( "Age Promotion List" )]
    [Category ( "BEMA Services > Check-In" )]
    [Description ( "Lists groups with a Promotes To attribute to set up promotion of check-in groups." )]

    [LinkedPage ( "Group Detail Page", "", true, "", "", 0 )]
    [BooleanField ( "Remove People from Current Group", "Should members be removed from the current group (otherwise they will be inactivated).", true, "", 8 )]
    [BooleanField ( "Promote Inactive Members", "Should inactive members be promoted?", false, "", 8 )]
    [GroupField ( "Backup Root Group", "Parent group under which backup groups are created", false,"", "",9 )]
    [BooleanField ( "Match Similar Named Groups", "Should group names be matched on first X characters",false,  "", 10 )]
    [IntegerField ("Number of Characters to Match", "This is a special use case where first characters identify the service for the group.", false, 5, "", 11)]
    [GroupTypeField ( "Group Type for Backup Groups", "When creating backup groups, what group type is used? (must be allowed child group type of Backup Root Group)", false, "", "", 9 )]
    [GroupTypesField ( "Include Group Types", "The group types to display in the list.  If none are selected, all group types will be included.", false, "", "", 1 )]
    [GroupTypesField ( "Exclude Group Types", "The group types to exclude from the list (only valid if including all groups).", false, "", "", 3 )]
    [BooleanField ( "Display Group Path", "Should the Group path be displayed?", false, "", 4 )]
    [BooleanField ( "Display Group Type Column", "Should the Group Type column be displayed?", true, "", 5 )]
    [BooleanField ( "Display Description Column", "Should the Description column be displayed?", true, "", 6 )]
    [BooleanField ( "Display Active Status Column", "Should the Active Status column be displayed?", false, "", 7 )]
    [BooleanField ( "Display Member Count Column", "Should the Member Count column be displayed? Does not affect lists with a person context.", true, "", 8 )]
    [BooleanField ( "Display Filter", "Should filter be displayed to allow filtering by group type?", false, "", 11 )]
    [CustomDropdownListField ( "Limit to Active Status", "Select which groups to show, based on active status. Select [All] to let the user filter by active status.", "all^[All], active^Active, inactive^Inactive", false, "all", Order = 12 )]
    [TextField ( "Set Panel Title", "The title to display in the panel header. Leave empty to have the title be set automatically based on the group type or block name.", required: false, order: 13 )]
    [TextField ( "Set Panel Icon", "The icon to display in the panel header. Leave empty to have the icon be set automatically based on the group type or default icon.", required: false, order: 14 )]
    [ContextAware]
    public partial class AgePromotionList : RockBlock, ICustomGridColumns
    {
        private int _groupTypesCount = 0;
        private bool _showGroupPath = false;

        private Guid _startAttributeGuid = new Guid( "904C0CE0-D97A-406F-9E44-D10F155F60F9" );
        private string _startKey = "StartDOB";
        private string _startName = "Start DOB";
        private Guid _endAttributeGuid = new Guid ( "785DB41A-1DE5-4761-B837-931B03DA48FF" );
        private string _endKey = "EndDOB";
        private string _endName = "End DOB";

        private int _dateFieldTypeId = 11;
        private Guid _memberAttributeGuid = new Guid ( "A7F9514C-F6D6-44FC-8CCC-935BC921D64A" );
        private string _memberKey = "MemberPromotesTo-BEMA";
        private string _memberName = "Promotes To (BEMA Internal Use)";
        private Guid _undoAttributeGuid = new Guid ( "6D8E26F4-CC4F-4F1C-999C-4A917ACB8348" );
        private string _undoKey = "MemberPromotedFrom-BEMA";
        private string _undoName = "Promoted From (BEMA Internal Use)";
        private Guid _checkInGuid = Guid.Parse ( "6E7AD783-7614-4721-ABC1-35842113EF59" );
        private int _groupFieldTypeId = 23;
        private Guid _groupAttributeGuid = new Guid ( "79E659F7-76B9-4A25-ABF9-F1464473D539" );
        private string _groupKey = "PromotesTo";
        private string _groupName = "Promotes To";


        private HashSet<int> _groupsWithGroupHistory = null;

        public enum GridListGridMode
        {
            // Block doesn't have a context of person, so it is just a normal list of groups
            GroupList = 1
        }

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub> ();

        /// <summary>
        /// Gets the signal r notification key.
        /// </summary>
        /// <value>
        /// The signal r notification key.
        /// </value>
        public string SignalRNotificationKey
        {
            get
            {
                return string.Format ( "GroupPromotion_BlockId:{0}_SessionId:{1}", this.BlockId, Session.SessionID );
            }
        }


        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit ( e );

            ApplyBlockSettings ();

            this.BlockUpdated += GroupList_BlockUpdated;
            this.AddConfigurationUpdateTrigger ( upnlGroupList );
            RockPage.AddScriptLink ( "~/Scripts/jquery.signalR-2.2.0.min.js", fingerprint: false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {

            if ( !Page.IsPostBack )
            {
                // Check if BEMA Plug-In attributes exist - if not, add
                var rockContext = new RockContext ();
                var attributeService = new AttributeService ( rockContext );
                var qryAttributes = attributeService.Queryable ().Where ( a => a.Key == _startKey ).Select ( a => a.Id ).ToList ();
                if ( qryAttributes.Count () == 0 )
                {
                    // Find Check-In Group Type
                    var groupTypeId = new GroupTypeService ( rockContext ).Get ( _checkInGuid ).Id;
                    // Create attribute
                    var attrQualifier = new AttributeQualifier ();
                    attrQualifier.Key = "displayCurrentOption";
                    attrQualifier.Value = "True";

                    var newAttribute = new Rock.Model.Attribute ();
                    newAttribute.Name = _startName;
                    newAttribute.Key = _startKey;
                    newAttribute.Guid = _startAttributeGuid;
                    newAttribute.EntityTypeId = EntityTypeCache.Get ( Rock.SystemGuid.EntityType.GROUP ).Id;
                    newAttribute.EntityTypeQualifierColumn = "GroupTypeId";
                    newAttribute.EntityTypeQualifierValue = groupTypeId.ToString ();
                    newAttribute.FieldTypeId = _dateFieldTypeId;
                    newAttribute.AttributeQualifiers.Add ( attrQualifier );

                    attributeService.Add ( newAttribute );
                    rockContext.SaveChanges ();
                    AttributeCache.UpdateCachedEntity ( EntityTypeCache.Get ( Rock.SystemGuid.EntityType.GROUP ).Id , EntityState.Modified );
                }

                qryAttributes = attributeService.Queryable ().Where ( a => a.Key == _endKey ).Select ( a => a.Id ).ToList ();
                if ( qryAttributes.Count () == 0 )
                {
                    // Find Check-In Group Type
                    var groupTypeId = new GroupTypeService ( rockContext ).Get ( _checkInGuid ).Id;
                    // Create attribute
                    var attrQualifier = new AttributeQualifier ();
                    attrQualifier.Key = "displayCurrentOption";
                    attrQualifier.Value = "True";

                    var newAttribute = new Rock.Model.Attribute ();
                    newAttribute.Name = _endName;
                    newAttribute.Key = _endKey;
                    newAttribute.Guid = _endAttributeGuid;
                    newAttribute.EntityTypeId = EntityTypeCache.Get ( Rock.SystemGuid.EntityType.GROUP ).Id;
                    newAttribute.EntityTypeQualifierColumn = "GroupTypeId";
                    newAttribute.EntityTypeQualifierValue = groupTypeId.ToString ();
                    newAttribute.FieldTypeId = _dateFieldTypeId;
                    newAttribute.AttributeQualifiers.Add ( attrQualifier );

                    attributeService.Add ( newAttribute );
                    rockContext.SaveChanges ();
                }

                qryAttributes = attributeService.Queryable ().Where ( a => a.Key == _memberKey ).Select ( a => a.Id ).ToList ();
                if ( qryAttributes.Count () == 0 )
                {
                    // Find Check-In Group Type
                    var groupTypeId = new GroupTypeService ( rockContext ).Get ( _checkInGuid ).Id;
                    // Create attributes
                    var newAttribute = new Rock.Model.Attribute (); newAttribute.Name = _memberName;
                    newAttribute.Key = _memberKey;
                    newAttribute.EntityTypeId = EntityTypeCache.Get ( Rock.SystemGuid.EntityType.GROUP_MEMBER ).Id;
                    newAttribute.Guid = _memberAttributeGuid;
                    newAttribute.EntityTypeQualifierColumn = "GroupTypeId";
                    newAttribute.EntityTypeQualifierValue = groupTypeId.ToString ();
                    newAttribute.FieldTypeId = _groupFieldTypeId;

                    attributeService.Add ( newAttribute );
                    rockContext.SaveChanges ();
                }

                qryAttributes = attributeService.Queryable ().Where ( a => a.Key == _undoKey ).Select ( a => a.Id ).ToList ();
                if ( qryAttributes.Count () == 0 )
                {
                    // Find Check-In Group Type
                    var groupTypeId = new GroupTypeService ( rockContext ).Get ( _checkInGuid ).Id;
                    // Create attributes
                    var newAttribute = new Rock.Model.Attribute (); newAttribute.Name = _undoName;
                    newAttribute.Key = _undoKey;
                    newAttribute.Guid = _undoAttributeGuid;
                    newAttribute.EntityTypeId = EntityTypeCache.Get ( Rock.SystemGuid.EntityType.GROUP_MEMBER ).Id;
                    newAttribute.EntityTypeQualifierColumn = "GroupTypeId";
                    newAttribute.EntityTypeQualifierValue = groupTypeId.ToString ();
                    newAttribute.FieldTypeId = _groupFieldTypeId;

                    attributeService.Add ( newAttribute );
                    rockContext.SaveChanges ();
                }

                BindFilter ();
                BindGrid ();
            }

            base.OnLoad ( e );
        }

        /// <summary>
        /// Applies the block settings.
        /// </summary>
        private void ApplyBlockSettings()
        {
            gfSettings.Visible = GetAttributeValue ( "DisplayFilter" ).AsBooleanOrNull () ?? false;
            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;

            // only show the user active filter if the block setting doesn't already restrict it
            ddlActiveFilter.Visible = GetAttributeValue ( "LimittoActiveStatus" ) == "all";

            gGroups.DataKeyNames = new string[] { "Id" };
            gGroups.Actions.AddClick += gGroups_Add;
            gGroups.GridRebind += gGroups_GridRebind;
            gGroups.ExportSource = ExcelExportSource.DataSource;
            gGroups.ShowConfirmDeleteDialog = false;

            // set up Grid based on Block Settings and Context
            bool showDescriptionColumn = GetAttributeValue ( "DisplayDescriptionColumn" ).AsBoolean ();
            bool showActiveStatusColumn = GetAttributeValue ( "DisplayActiveStatusColumn" ).AsBoolean ();
            bool showSystemColumn = GetAttributeValue ( "DisplaySystemColumn" ).AsBoolean ();
            bool showSecurityColumn = GetAttributeValue ( "DisplaySecurityColumn" ).AsBoolean ();

            if ( !showDescriptionColumn )
            {
                gGroups.TooltipField = "Description";
            }

            _showGroupPath = GetAttributeValue ( "DisplayGroupPath" ).AsBoolean ();

            Dictionary<string, BoundField> boundFields = gGroups.Columns.OfType<BoundField> ().ToDictionary ( a => a.DataField );
            boundFields["Name"].Visible = !_showGroupPath;

            // The GroupPathName field is the RockTemplateField that has a headertext of "Name"
            var groupPathNameField = gGroups.ColumnsOfType<RockTemplateField> ().FirstOrDefault ( a => a.HeaderText == "Name" );
            groupPathNameField.Visible = _showGroupPath;

            boundFields["GroupTypeName"].Visible = GetAttributeValue ( "DisplayGroupTypeColumn" ).AsBoolean ();
            boundFields["Description"].Visible = showDescriptionColumn;

            Dictionary<string, BoolField> boolFields = gGroups.Columns.OfType<BoolField> ().ToDictionary ( a => a.DataField );
            boolFields["IsActive"].Visible = showActiveStatusColumn;

            int personEntityTypeId = EntityTypeCache.Get ( "Rock.Model.Person" ).Id;

            // Grid is in normal 'Group List' mode
            bool canEdit = IsUserAuthorized ( Authorization.EDIT );
            gGroups.Actions.ShowAdd = false;
            gGroups.IsDeleteEnabled = false;
            gGroups.DataKeyNames = new string[] { "Id" };

            boundFields["GroupRole"].Visible = false;
            boundFields["DateAdded"].Visible = false;
            boundFields["ActiveMemberCount"].Visible = GetAttributeValue ( "DisplayMemberCountColumn" ).AsBoolean ();
            boundFields["InactiveMemberCount"].Visible = GetAttributeValue ( "DisplayMemberCountColumn" ).AsBoolean ();
            var backupGroup = GetAttributeValue ( "BackupRootGroup" ).AsGuidOrNull ();
            var backupGroupType = GetAttributeValue ( "GroupTypeforBackupGroups" ).AsGuidOrNull ();
            if ( backupGroup.HasValue && backupGroupType.HasValue)
            {
                btnBackup.Visible = true;
                // If there are groups to be deleted
                var groupService = new GroupService ( new RockContext () );
                var parentGroup = groupService.Get ( backupGroup.Value );
                var groupsSelected = groupService.Queryable ().Where ( g => g.ParentGroupId == parentGroup.Id ).Select ( g => g.Id ).ToList ();

                if ( groupsSelected.Any () )
                {
                    btnPurgeBackup.Visible = true;
                    btnPurgeBackup.Text = String.Format ( "Delete {0} Backup Group(s) in '{1}'", groupsSelected.Count (), parentGroup.Name );
                    btnPurgeBackup.Enabled = true;
                }
                else
                {
                    btnPurgeBackup.Visible = true;
                    btnPurgeBackup.Text = "Delete Backup Groups";
                    btnPurgeBackup.Enabled = false;
                }
            }
            else
            {
                btnBackup.Visible = false;
                btnPurgeBackup.Visible = false;
            }

            SetPanelTitleAndIcon ();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gGroups_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var groupInfo = (GroupListRowInfo) e.Row.DataItem;

                // Show inactive entries in a lighter font.
                if ( !groupInfo.IsActive || groupInfo.IsArchived )
                {
                    e.Row.AddCssClass ( "is-inactive" );
                }

                var deleteOrArchiveField = gGroups.ColumnsOfType<DeleteField> ().FirstOrDefault ();
                if ( deleteOrArchiveField != null && deleteOrArchiveField.Visible )
                {
                    var deleteFieldColumnIndex = gGroups.GetColumnIndex ( deleteOrArchiveField );
                    var deleteButton = e.Row.Cells[deleteFieldColumnIndex].ControlsOfTypeRecursive<LinkButton> ().FirstOrDefault ();
                    if ( deleteButton != null )
                    {
                        var buttonIcon = deleteButton.ControlsOfTypeRecursive<HtmlGenericControl> ().FirstOrDefault ();

                        if ( groupInfo.IsSynced )
                        {
                            deleteButton.Enabled = false;
                            buttonIcon.Attributes["class"] = "fa fa-exchange";

                            deleteButton.ToolTip = string.Format ( "Managed by group sync for role \"{0}\".", groupInfo.GroupRole );
                        }
                        else if ( groupInfo.GroupType.EnableGroupHistory && _groupsWithGroupHistory.Contains ( groupInfo.Id ) )
                        {
                            buttonIcon.Attributes["class"] = "fa fa-archive";
                            deleteButton.AddCssClass ( "btn-danger" );
                            deleteButton.ToolTip = "Archive";
                            e.Row.AddCssClass ( "js-has-grouphistory" );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the GroupList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void GroupList_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference ();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnClear control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnPromote_Click( object sender, EventArgs e )
        {
            pnlProgress.Visible = true;
            pnlActions.Visible = false;
            gfSettings.Visible = false;
            gGroups.Visible = false;

            var importTask = new Task ( () =>
            {
                // wait a little so the browser can render and start listening to events
                System.Threading.Thread.Sleep ( 1000 );
                _hubContext.Clients.All.showButtons ( this.SignalRNotificationKey, false );


                var groupsSelected = new List<int> ();

                gGroups.SelectedKeys.ToList ().ForEach ( b => groupsSelected.Add ( b.ToString ().AsInteger () ) );

                if ( !groupsSelected.Any () )
                {
                    _hubContext.Clients.All.showButtons ( this.SignalRNotificationKey, true );
                    WriteProgressMessage ( "Error", "There were no groups selected." );
                    return;
                }
                WriteProgressMessage ( "Starting Promotion...", "Cleaning up old data..." );

                // Clear Old Member Attributes (if they exist)
                var rockContext = new RockContext ();
                var attributeService = new AttributeService ( rockContext );
                var attributeValueService = new AttributeValueService ( rockContext );

                var memberAttributeId = AttributeCache.Get ( _memberAttributeGuid ).Id;
                var startingAttributeId = AttributeCache.Get ( _startAttributeGuid ).Id;
                var endingAttributeId = AttributeCache.Get ( _endAttributeGuid ).Id;

                var qryPromoteAttributes = attributeService.Queryable ().Where ( a => a.Key == _memberKey ).Select ( a => a.Id ).ToList ();
                var idList = string.Join ( ",", qryPromoteAttributes.Select ( n => n.ToString () ).ToArray () );
                var qry = string.Format ( "Delete From AttributeValue Where AttributeId in ({0})", idList );
                rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@idList", idList ) );

                var qryUndoAttributes = attributeService.Queryable ().Where ( a => a.Key == _undoKey ).Select ( a => a.Id ).ToList ();
                idList = string.Join ( ",", qryUndoAttributes.Select ( n => n.ToString () ).ToArray () );
                qry = string.Format ( "Delete From AttributeValue Where AttributeId in ({0})", idList );
                rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@idList", idList ) );

                WriteProgressMessage ( "Step 1 of 5 Complete", "Clean up of old data complete..." );

                groupsSelected = new List<int> ();

                gGroups.SelectedKeys.ToList ().ForEach ( b => groupsSelected.Add ( b.ToString ().AsInteger () ) );

                if ( groupsSelected.Any () )
                {
                    // Add New Member Attributes for Selected Groups
                    var groupService = new GroupService ( rockContext );
                    var memberService = new GroupMemberService ( rockContext );
                    var groupsToUpdate = groupService.Queryable ()
                        .Where ( g =>
                             groupsSelected.Contains ( g.Id ) )
                        .ToList ();

                    rockContext.Database.CommandTimeout = 180; // 3 minutes (usually takes 25 seconds to do 500 people in one group)

                    var shouldMatch = GetAttributeValue ( "MatchSimilarNamedGroups" ).AsBoolean ();
                    var matchSize = GetAttributeValue ( "NumberofCharacterstoMatch" ).AsIntegerOrNull ();
                    var listOfGroupIds = String.Join ( ",", groupsToUpdate.Select ( a => a.Id ).ToList ());

                    foreach ( var group in groupsToUpdate )
                    {
                        WriteProgressMessage ( "Step 2 of 5 in progress...", group.Name );
                        if (shouldMatch && matchSize.Value > 0)
                        {
                            if ( GetAttributeValue ( "PromoteInactiveMembers" ).AsBoolean () )
                            {
                                qry = string.Format ( "Insert Into AttributeValue (IsSystem, EntityId, AttributeId, Value, Guid, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId) " +
                                    "Select 0, gm.Id, {0}, B.Guid, NewId(), CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, {2}, {2} From GroupMember gm" +
                                    " Inner Join Person p on p.Id = gm.PersonId" +
                                    " Inner Join[Group] g on gm.GroupId = g.Id" + 
                                    " Outer Apply (" +
                                        " Select Id, Name, DefaultGroupRoleId, Guid From (" +
                                        " Select PG.Id, PG.Name, gt.DefaultGroupRoleId,  PG.Guid," +
                                        "   CASE When ( Left ( starting.Value, 7 ) = 'CURRENT' ) THEN CURRENT_TIMESTAMP + CAST ( RIGHT ( starting.Value, LEN ( starting.Value ) - 8 ) As Int ) ELSE starting.ValueAsDateTime End As StartDate," +
                                        "   CASE When ( Left ( ending.Value, 7 ) = 'CURRENT' ) THEN CURRENT_TIMESTAMP + CAST ( RIGHT ( ending.Value, LEN ( ending.Value ) - 8 ) As Int ) ELSE ending.ValueAsDateTime End As EndDate " +
                                        " FROM [Group] PG " +
                                        " Inner Join AttributeValue starting on starting.EntityId = PG.Id And starting.AttributeId = {3} " + 
                                        " Inner Join AttributeValue ending on ending.EntityId = PG.Id And ending.AttributeId = {4} " +                                         " Inner Join GroupType gt on PG.GroupTypeId = gt.Id " +
                                        " Inner Join GroupType gt on PG.GroupTypeId = gt.Id " +
                                        " WHERE PG.Id in ({6}) and PG.[Name] like '{5}%' and PG.IsActive = 1 " +
                                        " ) As A Where StartDate != '' And StartDate <= p.BirthDate " +
                                        " And EndDate != '' And p.BirthDate <= EndDate ) B " + "  Where g.IsActive = 1 And gm.GroupId = {1}" + 
                                        " And g.Name <> B.Name",
                                    memberAttributeId, group.Id, this.CurrentPersonAliasId, startingAttributeId, endingAttributeId, group.Name.Left(matchSize.Value),
                                    listOfGroupIds );

                                rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@groupId", group.Id ) );

                            }
                            else
                            {
                                qry = string.Format ( "Insert Into AttributeValue (IsSystem, EntityId, AttributeId, Value, Guid, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId) " +
                                    "Select 0, gm.Id, {0}, B.Guid, NewId(), CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, {2}, {2} From GroupMember gm" +
                                    " Inner Join Person p on p.Id = gm.PersonId" +
                                    " Inner Join[Group] g on gm.GroupId = g.Id" +
                                    " Outer Apply (" +
                                        " Select Id, Name, DefaultGroupRoleId, Guid From (" +
                                        " Select PG.Id, PG.Name, gt.DefaultGroupRoleId,  PG.Guid," +
                                        "   CASE When ( Left ( starting.Value, 7 ) = 'CURRENT' ) THEN CURRENT_TIMESTAMP + CAST ( RIGHT ( starting.Value, LEN ( starting.Value ) - 8 ) As Int ) ELSE starting.ValueAsDateTime End As StartDate," +
                                        "   CASE When ( Left ( ending.Value, 7 ) = 'CURRENT' ) THEN CURRENT_TIMESTAMP + CAST ( RIGHT ( ending.Value, LEN ( ending.Value ) - 8 ) As Int ) ELSE ending.ValueAsDateTime End As EndDate " +
                                        " FROM [Group] PG " +
                                        " Inner Join AttributeValue starting on starting.EntityId = PG.Id And starting.AttributeId = {3} " +
                                        " Inner Join AttributeValue ending on ending.EntityId = PG.Id And ending.AttributeId = {4} " +
                                        " Inner Join GroupType gt on PG.GroupTypeId = gt.Id " +
                                        " WHERE PG.Id in ({6}) and PG.[Name] like '{5}%' and PG.IsActive = 1 " +
                                        " ) As A Where StartDate != '' And StartDate <= p.BirthDate " +
                                        " And EndDate != '' And p.BirthDate <= EndDate ) B " +
                                        "  Where g.IsActive = 1 And gm.GroupId = {1} And gm.GroupMemberStatus = 1" +
                                        " And g.Name <> B.Name",
                                    memberAttributeId, group.Id, this.CurrentPersonAliasId, startingAttributeId, endingAttributeId, group.Name.Left ( matchSize.Value ),
                                    listOfGroupIds );

                                rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@groupId", group.Id ) );

                            }
                        }
                        else
                        {
                            if ( GetAttributeValue ( "PromoteInactiveMembers" ).AsBoolean () )
                            {
                                qry = string.Format ( "Insert Into AttributeValue (IsSystem, EntityId, AttributeId, Value, Guid, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId) " +
                                    "Select 0, gm.Id, {0}, B.Guid, NewId(), CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, {2}, {2} From GroupMember gm" +
                                    " Inner Join Person p on p.Id = gm.PersonId" +
                                    " Inner Join[Group] g on gm.GroupId = g.Id" +
                                    " Outer Apply (" +
                                        " Select Id, Name, DefaultGroupRoleId, Guid From (" +
                                        " Select PG.Id, PG.Name, gt.DefaultGroupRoleId,  PG.Guid," +
                                        "   CASE When ( Left ( starting.Value, 7 ) = 'CURRENT' ) THEN CURRENT_TIMESTAMP + CAST ( RIGHT ( starting.Value, LEN ( starting.Value ) - 8 ) As Int ) ELSE starting.ValueAsDateTime End As StartDate," +
                                        "   CASE When ( Left ( ending.Value, 7 ) = 'CURRENT' ) THEN CURRENT_TIMESTAMP + CAST ( RIGHT ( ending.Value, LEN ( ending.Value ) - 8 ) As Int ) ELSE ending.ValueAsDateTime End As EndDate " +
                                        " FROM [Group] PG " +
                                        " Inner Join AttributeValue starting on starting.EntityId = PG.Id And starting.AttributeId = {3} " +
                                        " Inner Join AttributeValue ending on ending.EntityId = PG.Id And ending.AttributeId = {4} " +
                                        " Inner Join GroupType gt on PG.GroupTypeId = gt.Id " +
                                        " WHERE PG.Id in ({5}) and PG.IsActive = 1 " +
                                        " ) As A Where StartDate <= p.BirthDate And p.BirthDate <= EndDate ) B " +
                                        "  Where g.IsActive = 1 And gm.GroupId = {1}" +
                                        " And g.Name <> B.Name",
                                    memberAttributeId, group.Id, this.CurrentPersonAliasId, startingAttributeId, endingAttributeId, 
                                    listOfGroupIds );

                                rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@groupId", group.Id ) );

                            }
                            else
                            {
                                qry = string.Format ( "Insert Into AttributeValue (IsSystem, EntityId, AttributeId, Value, Guid, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId) " +
                                    "Select 0, gm.Id, {0}, B.Guid, NewId(), CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, {2}, {2} From GroupMember gm" +
                                    " Inner Join Person p on p.Id = gm.PersonId" +
                                    " Inner Join[Group] g on gm.GroupId = g.Id" +
                                    " Outer Apply (" +
                                        " Select Id, Name, DefaultGroupRoleId, Guid From (" +
                                        " Select PG.Id, PG.Name, gt.DefaultGroupRoleId,  PG.Guid," +
                                        "   CASE When ( Left ( starting.Value, 7 ) = 'CURRENT' ) THEN CURRENT_TIMESTAMP + CAST ( RIGHT ( starting.Value, LEN ( starting.Value ) - 8 ) As Int ) ELSE starting.ValueAsDateTime End As StartDate," +
                                        "   CASE When ( Left ( ending.Value, 7 ) = 'CURRENT' ) THEN CURRENT_TIMESTAMP + CAST ( RIGHT ( ending.Value, LEN ( ending.Value ) - 8 ) As Int ) ELSE ending.ValueAsDateTime End As EndDate " +
                                        " FROM [Group] PG " +
                                        " Inner Join AttributeValue starting on starting.EntityId = PG.Id And starting.AttributeId = {3} " +
                                        " Inner Join AttributeValue ending on ending.EntityId = PG.Id And ending.AttributeId = {4} " +
                                        " Inner Join GroupType gt on PG.GroupTypeId = gt.Id " +
                                        " WHERE PG.Id in ({5})  and PG.IsActive = 1 " +
                                        " ) As A Where StartDate <= p.BirthDate And p.BirthDate <= EndDate ) B " +
                                        "  Where g.IsActive = 1 And gm.GroupId = {1} And gm.GroupMemberStatus = 1" +
                                        " And g.Name <> B.Name",
                                    memberAttributeId, group.Id, this.CurrentPersonAliasId, startingAttributeId, endingAttributeId, 
                                    listOfGroupIds );

                                rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@groupId", group.Id ) );
                            }

                        }
                    }
                    WriteProgressMessage ( "Step 2 of 5 Complete", "Added promotion attributes..." );


                    rockContext.SaveChanges ();

                    // Add members to New Groups

                    // Update if already a member
                    qry = string.Format ( "Update gm2 Set GroupMemberStatus = gm.GroupMemberStatus, ModifiedDateTime = CURRENT_TIMESTAMP, ModifiedByPersonAliasId = 10 " +
                        ", ForeignKey = 'BEMAPromotion', ForeignId = gm.Id " +
                        " From GroupMember gm " +
                        " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId = {0} " +
                        " Inner Join [Group] g on cast (g.Guid as nvarchar(40)) like av.Value" +
                        " Inner Join GroupMember gm2 on gm2.GroupId = g.Id And gm2.PersonId = gm.PersonId" +
                        " Inner Join GroupType gt on g.GroupTypeId = gt.Id",
                        memberAttributeId );

                    rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", memberAttributeId ) );

                        WriteProgressMessage ( "Step 3 of 5 in progress...", "Inserting people into groups... This takes a while..." );

                        var count = 0;
                        var result = 1;

                        do
                        {
                            // Insert if not already a member
                            qry = string.Format ( "Insert Into GroupMember (IsSystem, GroupId, PersonId, GroupRoleId, GroupMemberStatus, Guid, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId, ForeignKey, ForeignId) " +
                                "Select Top 50 0, g.Id, gm.PersonId, gt.DefaultGroupRoleId, gm.GroupMemberStatus, NewId(), CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, {1}, {1}, 'BEMAPromotion', gm.Id From GroupMember gm " +
                                " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId = {0} " +
                                " Inner Join [Group] g on cast (g.Guid as nvarchar(40)) like av.Value" +
                                " Inner Join GroupType gt on g.GroupTypeId = gt.Id" +
                                " Where gm.Id Not in (Select gm.Id From GroupMember gm Inner Join AttributeValue av on av.EntityId = gm.Id And AttributeId = {0} Inner Join [Group] g on cast ( g.Guid as nvarchar( 40 ) ) like av.Value" +
                                " Left Outer Join GroupMember gm2 on gm2.PersonId = gm.PersonId  And g.Id = gm2.GroupId Where gm2.Id Is Not Null )",
                                memberAttributeId, this.CurrentPersonAliasId );

                            result = rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", memberAttributeId ) );

                            count += result;
                            WriteProgressMessage ( "Step 3 of 5 in progress...", String.Format ( "{0} people are added to new groups...", count ) );

                        } while ( result == 50 );

                        WriteProgressMessage ( "Step 3 of 5 Complete", String.Format ( "People are added to new groups... ({0} people)", count ) );

                        // Add Undo attributes for each group
                        qry = string.Format ( "Insert Into AttributeValue (IsSystem, EntityId, AttributeId, Value, Guid, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId) " +
                            "Select 0, gm2.Id, a.Id, g2.Guid, NewId(), CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, {2}, {2} " +
                            "From GroupMember gm " +
                            " Inner Join AttributeValue av on av.EntityId = gm.Id and AttributeId = {0}" +
                            " Inner Join [Group] g on cast (g.Guid as nvarchar(40)) like av.Value" +
                            " Inner Join GroupType gt on g.GroupTypeId = gt.Id" +
                            " Inner Join GroupMember gm2 on gm2.PersonId = gm.PersonId and gm2.GroupId = g.Id and gm2.ForeignId = gm.Id and gm2.ForeignKey = 'BEMAPromotion'" +
                            " Inner Join Attribute a on a.[Key] = '{1}' and a.EntityTypeId = 90 and (a.EntityTypeQualifierValue = gt.Id or a.EntityTypeQualifierValue = gt.InheritedGroupTypeId) " +
                            " Inner Join [Group] g2 on gm.GroupId = g2.Id",
                            memberAttributeId, _undoKey, this.CurrentPersonAliasId );
                        rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", memberAttributeId ) );

                    WriteProgressMessage ( "Step 4 of 5 Complete", "Added undo attributes...." );

                    foreach ( var group in groupsToUpdate )
                    {

                        if ( GetAttributeValue ( "RemovePeoplefromCurrentGroup" ).AsBoolean () == true )
                        {
                            if ( GetAttributeValue ( "PromoteInactiveMembers" ).AsBoolean () )
                            {
                                if ( group.GroupType.EnableGroupHistory == true )
                                {
                                    // Can't delete.  Mark as Archived
                                    qry = string.Format ( "Update gm Set IsArchived = 1, ModifiedDateTime = CURRENT_TIMESTAMP, ModifiedByUserAliasId = 10) " +
                                        " From GroupMember gm " +
                                        " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId = {0} " +
                                        " Where gm.GroupId = {1}",
                                        memberAttributeId, group.Id );
                                    rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", memberAttributeId ) );
                                }
                                else
                                {
                                    // Delete
                                    qry = string.Format ( "Delete gm From GroupMember gm " +
                                        " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId = {0} " +
                                        " Where gm.GroupId = {1}",
                                        memberAttributeId, group.Id );
                                    rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", memberAttributeId ) );
                                }
                            }
                            else
                            {
                                if ( group.GroupType.EnableGroupHistory == true )
                                {
                                    // Can't delete.  Mark as Archived
                                    qry = string.Format ( "Update gm Set IsArchived = 1, ModifiedDateTime = CURRENT_TIMESTAMP, ModifiedByPersonAliasId = 10 " +
                                        " From GroupMember gm " +
                                        " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId = {0} " +
                                        " Where gm.GroupId = {1} And GroupMemberStatus = 1",
                                        memberAttributeId, group.Id );
                                    rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", memberAttributeId ) );
                                }
                                else
                                {
                                    // Delete
                                    qry = string.Format ( "Delete gm From GroupMember gm " +
                                        " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId = {0} " +
                                        " Where gm.GroupId = {1} And GroupMemberStatus = 1",
                                        memberAttributeId, group.Id );
                                    rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", memberAttributeId ) );
                                }
                            }
                            WriteProgressMessage ( "Step 5 of 5 Complete", "Remove from old groups..." );

                        }
                        else
                        {
                            // Mark Inactive
                            qry = string.Format ( "Update gm Set GroupMemberStatus = 0, ModifiedDateTime = CURRENT_TIMESTAMP, ModifiedByPersonAliasId = 10 " +
                                " From GroupMember gm " +
                                " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId = {0} " +
                                " Where gm.GroupId = {1}",
                                memberAttributeId, group.Id );
                            rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", memberAttributeId ) );
                            WriteProgressMessage ( "Step 5 of 5 Complete", "Inactivated people in old groups..." );
                        }
                    }
                    rockContext.SaveChanges ();

                    WriteProgressMessage ( "Success.", string.Format ( "{0} group(s) were updated.", groupsSelected.Count () ) );
                }
                else
                {
                    WriteProgressMessage ( "Error.", "There were no groups selected." );
                }

                _hubContext.Clients.All.showButtons ( this.SignalRNotificationKey, true );
            } );

            importTask.Start ();
        }

        /// <summary>
        /// Handles the Click event of the btnSet control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnBackup_Click( object sender, EventArgs e )
        {
            pnlProgress.Visible = true;
            pnlActions.Visible = false;
            gfSettings.Visible = false;
            gGroups.Visible = false;

            var importTask = new Task ( () =>
            {
                // wait a little so the browser can render and start listening to events
                System.Threading.Thread.Sleep ( 1000 );
                _hubContext.Clients.All.showButtons ( this.SignalRNotificationKey, false );

                WriteProgressMessage ( "Starting Backup...", "" );

                var rockContext = new RockContext ();

                var groupsSelected = new List<int> ();

                gGroups.SelectedKeys.ToList ().ForEach ( b => groupsSelected.Add ( b.ToString ().AsInteger () ) );

                var backupGroup = GetAttributeValue ( "BackupRootGroup" ).AsGuidOrNull ();
                var backupGroupType = GetAttributeValue ( "GroupTypeforBackupGroups" ).AsGuidOrNull ();
                var groupService = new GroupService ( rockContext );
                var parentGroup = groupService.Get ( backupGroup.Value );

                if ( groupsSelected.Any () && backupGroup.HasValue && backupGroupType.HasValue )
                {

                    var memberService = new GroupMemberService ( rockContext );
                    var newGroupType = new GroupTypeService( rockContext).Get ( backupGroupType.Value );


                    var groupsToBackup = groupService.Queryable ()
                        .Where ( g =>
                             groupsSelected.Contains ( g.Id ) )
                        .ToList ();

                    foreach ( var group in groupsToBackup )
                    {
                        WriteProgressMessage ( "Backing up...", group.Name );
                        // Create backup group
                        var newGroup = new Group ();
                        newGroup.Name = group.Name;
                        newGroup.GroupType = newGroupType;
                        newGroup.GroupTypeId = newGroupType.Id;
                        newGroup.ParentGroupId = parentGroup.Id;
                        newGroup.Guid = Guid.NewGuid();
                        newGroup.IsActive = true;
                        groupService.Add ( newGroup );

                        rockContext.SaveChanges ();
                        newGroup = groupService.Get ( newGroup.Guid );

                        var qry = string.Format ( "Insert Into GroupMember (IsSystem, GroupId, PersonId, GroupRoleId, GroupMemberStatus, Guid, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId) " +
                            "Select 0, {0}, PersonId, {1}, GroupMemberStatus, NewId(), CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, {2}, {3} From GroupMember Where GroupId = {4}",
                            newGroup.Id, newGroup.GroupType.DefaultGroupRoleId, this.CurrentPersonAliasId, this.CurrentPersonAliasId, group.Id);
                        rockContext.Database.ExecuteSqlCommand(qry, new SqlParameter("@groupId", group.Id));

                    }

                    rockContext.SaveChanges ();

                    WriteProgressMessage ( "Success.", string.Format ( "{0} group(s) were backed up.", groupsSelected.Count () ) );

                }
                else
                {
                    WriteProgressMessage ( "Error.", "There were no groups selected." );
                }


                btnBackup.Visible = true;
                // If there are groups to be deleted
                var purgeGroupsSelected = groupService.Queryable ().Where ( g => g.ParentGroupId == parentGroup.Id ).Select ( g => g.Id ).ToList ();

                if ( purgeGroupsSelected.Any () )
                {
                    btnPurgeBackup.Visible = true;
                    btnPurgeBackup.Text = String.Format ( "Delete {0} Backup Group(s) in '{1}'", purgeGroupsSelected.Count (), parentGroup.Name );
                    btnPurgeBackup.Enabled = true;
                }
                else
                {
                    btnPurgeBackup.Visible = true;
                    btnPurgeBackup.Text = "Delete Backup Groups";
                    btnPurgeBackup.Enabled = false;
                }

                _hubContext.Clients.All.showButtons ( this.SignalRNotificationKey, true );
            } );

            importTask.Start ();

        }

        /// <summary>
        /// Handles the Click event of the btnSet control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnPurge_Click( object sender, EventArgs e )
        {
            pnlProgress.Visible = true;
            pnlActions.Visible = false;
            gfSettings.Visible = false;
            gGroups.Visible = false;

            var importTask = new Task ( () =>
            {
                // wait a little so the browser can render and start listening to events
                System.Threading.Thread.Sleep ( 1000 );
                _hubContext.Clients.All.showButtons ( this.SignalRNotificationKey, false );

                WriteProgressMessage ( "Starting Removal of Old Backup Groups...", "" );

                var rockContext = new RockContext ();

                var backupGroup = GetAttributeValue ( "BackupRootGroup" ).AsGuidOrNull ();
                var backupGroupType = GetAttributeValue ( "GroupTypeforBackupGroups" ).AsGuidOrNull ();
                if (! backupGroup.HasValue)
                {
                    maWarning.Show ( "No backup root group specified.", Rock.Web.UI.Controls.ModalAlertType.None );
                    return;
                }

                var groupService = new GroupService ( rockContext );

                var parentGroup = groupService.Get ( backupGroup.Value );
                var groupsSelected = groupService.Queryable().Where(g => g.ParentGroupId == parentGroup.Id ).Select(g => g.Id ).ToList();

                if ( groupsSelected.Any ()  )
                {

                    foreach ( var groupId in groupsSelected )
                    {
                        // Delete Group
                        var group = groupService.Get ( groupId );
                        if (group.IsValid)
                        {
                            WriteProgressMessage ( "Deleting...", group.Name );
                            groupService.Delete ( group );
                        }
                        groupService.Delete ( group );
                    }

                    rockContext.SaveChanges ();

                    WriteProgressMessage ( "Success.", string.Format ( "{0} group(s) were deleted.", groupsSelected.Count () ) );

                }
                else
                {
                    WriteProgressMessage ( "Error.", "There were no groups selected." );
                }

                btnBackup.Visible = true;
                // If there are groups to be deleted
                var purgeGroupsSelected = groupService.Queryable ().Where ( g => g.ParentGroupId == parentGroup.Id ).Select ( g => g.Id ).ToList ();

                if ( purgeGroupsSelected.Any () )
                {
                    btnPurgeBackup.Visible = true;
                    btnPurgeBackup.Text = String.Format ( "Delete {0} Backup Group(s) in '{1}'", purgeGroupsSelected.Count (), parentGroup.Name );
                    btnPurgeBackup.Enabled = true;
                }
                else
                {
                    btnPurgeBackup.Visible = true;
                    btnPurgeBackup.Text = "Delete Backup Groups";
                    btnPurgeBackup.Enabled = false;
                }

                _hubContext.Clients.All.showButtons ( this.SignalRNotificationKey, true );
            } );

            importTask.Start();

        }

        /// <summary>
        /// Handles the Click event of the btnRemove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnUndo_Click( object sender, EventArgs e )
        {
            pnlProgress.Visible = true;
            pnlActions.Visible = false;
            gfSettings.Visible = false;
            gGroups.Visible = false;

            var importTask = new Task ( () =>
            {
                // wait a little so the browser can render and start listening to events
                System.Threading.Thread.Sleep ( 1000 );
                _hubContext.Clients.All.showButtons ( this.SignalRNotificationKey, false );

                WriteProgressMessage ( "Starting Undo...", "" );

                // Find members with Promoted From attribute
                var rockContext = new RockContext ();

                var attributeValueService = new AttributeValueService ( rockContext );
                var attributeService = new AttributeService ( rockContext );
                var qryAttributes = attributeService.Queryable().Where ( a => a.Key == _undoKey ).Select(a => a.Id).ToList();
                var idList = string.Join ( ",", qryAttributes.Select ( n => n.ToString () ).ToArray () );


                var qry = string.Format ( "SELECT EntityId From AttributeValue Where AttributeId in ({0}) And Value != '' And Value Is Not Null", idList );
                var qryMemberAVTask = rockContext.Database.SqlQuery ( typeof ( int ), qry, new SqlParameter ( "@attrId", idList ) ).ToListAsync ();
                qryMemberAVTask.Wait ();
                var qryMemberValues = qryMemberAVTask.Result.Select ( i => (int) i ).ToList ();


                var groupService = new GroupService ( rockContext );
                var memberService = new GroupMemberService ( rockContext );
                var memberList = memberService.Queryable ()
                    .Where ( m => qryMemberValues.Contains ( m.Id ) )
                    .ToList ();

                if (qryMemberValues.Any())
                {
                    rockContext.Database.CommandTimeout = 180; // 3 minutes (usually takes 25 seconds to do 500 people in one group)

                    // Add members to New Groups
                    WriteProgressMessage ( "Re-Adding to Old Groups...", "" );

                    // Update if already a member
                    qry = string.Format ( "Update gm2 Set GroupMemberStatus = gm.GroupMemberStatus, ModifiedDateTime = CURRENT_TIMESTAMP, ModifiedByPersonAliasId = 10 " +
                        " From GroupMember gm " +
                        " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId in ({0}) " +
                        " Inner Join [Group] g on cast (g.Guid as nvarchar(40)) like av.Value" +
                        " Inner Join GroupMember gm2 on gm2.GroupId = g.Id And gm2.PersonId = gm.PersonId" +
                        " Inner Join GroupType gt on g.GroupTypeId = gt.Id",
                        idList );

                    rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", idList ) );

                    // Insert if not already a member
                    WriteProgressMessage ( "Re-Adding to Old Groups...", "Inserting members in old groups." );

                    var count = 0;
                    var result = 1;

                    do
                    {

                        qry = string.Format ( "Insert Into GroupMember (IsSystem, GroupId, PersonId, GroupRoleId, GroupMemberStatus, Guid, CreatedDateTime, ModifiedDateTime, CreatedByPersonAliasId, ModifiedByPersonAliasId) " +
                        "Select Top 50 0, g.Id, gm.PersonId, gt.DefaultGroupRoleId, gm.GroupMemberStatus, NewId(), CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, {1}, {1} From GroupMember gm " +
                        " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId in ({0}) " +
                        " Inner Join [Group] g on cast (g.Guid as nvarchar(40)) like av.Value" +
                        " Inner Join GroupType gt on g.GroupTypeId = gt.Id" +
                        " Where gm.Id Not in (Select gm.Id From GroupMember gm Inner Join AttributeValue av on av.EntityId = gm.Id And AttributeId in ({0}) Inner Join [Group] g on cast ( g.Guid as nvarchar( 40 ) ) like av.Value" +
                        " Left Outer Join GroupMember gm2 on gm2.PersonId = gm.PersonId  And g.Id = gm2.GroupId Where gm2.Id Is Not Null )",
                        idList, this.CurrentPersonAliasId );

                        result = rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", idList ) );

                        count += result;
                        WriteProgressMessage ( "Re-Adding to Old Groups...", String.Format ( "{0} people are added to old groups...", count ) );

                    } while ( result == 50 );
                    WriteProgressMessage ( "Re-Adding to Old Groups...", String.Format ( "{0} people are added to old groups...", count ) );

                    // Remove members from promotion groups
                    WriteProgressMessage ( "Removing from Promotion Groups...", "" );

                    // Archive Groups that don't allow deletion - set to IsArchived 
                    qry = string.Format ( "Update gm Set IsArchived = 1, ModifiedDateTime = CURRENT_TIMESTAMP, ModifiedByPersonAliasId = 10 " +
                        " From GroupMember gm " +
                        " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId in ({0}) " +
                        " Inner Join [Group] g on g.Id = gm.GroupId" +
                        " Inner Join GroupType gt on g.GroupTypeId = gt.Id" +
                        " Where gt.EnableGroupHistory = 1",
                        idList);
                    rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", idList ) );

                    // Delete Members that can be deleted
                    qry = string.Format ( "Delete gm From GroupMember gm " +
                        " Inner Join AttributeValue av on av.EntityId = gm.Id And av.AttributeId in ({0}) " +
                        " Inner Join [Group] g on g.Id = gm.GroupId" +
                        " Inner Join GroupType gt on g.GroupTypeId = gt.Id" +
                        " Where gt.EnableGroupHistory = 0",
                        idList );
                    rockContext.Database.ExecuteSqlCommand ( qry, new SqlParameter ( "@attrId", idList ) );

                    rockContext.SaveChanges ();

                    WriteProgressMessage ( "Success.", string.Format ( "{0} group member(s) were updated.", qryMemberValues.Count () ) );
                }
                else
                {
                    WriteProgressMessage ( "Error.", "No promotions to Undo." );
                }
                _hubContext.Clients.All.showButtons ( this.SignalRNotificationKey, true );
            } );

            importTask.Start ();

        }

        /// <summary>
        /// Handles the Click event of the btnRemove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDone_Click( object sender, EventArgs e )
        {

            pnlProgress.Visible = false;
            pnlActions.Visible = true;
            gfSettings.Visible = true;
            gGroups.Visible = true;

            ApplyBlockSettings ();
            BindGrid (); 
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference ( "Group Type", gtpGroupType.SelectedValue );

            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfSettings.SaveUserPreference ( "Active Status", string.Empty );
            }
            else
            {
                gfSettings.SaveUserPreference ( "Active Status", ddlActiveFilter.SelectedValue );
            }

            gfSettings.SaveUserPreference ( "Group Type Purpose", ddlGroupTypePurpose.SelectedValue );

            BindGrid ();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Group Type":

                    int id = e.Value.AsInteger ();

                    var groupType = GroupTypeCache.Get ( id );
                    if ( groupType != null )
                    {
                        e.Value = groupType.Name;
                    }

                    break;

                case "Active Status":

                    // if the ActiveFilter control is hidden (because there is a block setting that overrides it), don't filter by Active Status
                    if ( !ddlActiveFilter.Visible )
                    {
                        e.Value = string.Empty;
                    }

                    break;

                case "Group Type Purpose":
                    var groupTypePurposeTypeValueId = e.Value.AsIntegerOrNull ();
                    if ( groupTypePurposeTypeValueId.HasValue )
                    {
                        var groupTypePurpose = DefinedValueCache.Get ( groupTypePurposeTypeValueId.Value );
                        e.Value = groupTypePurpose != null ? groupTypePurpose.ToString () : string.Empty;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the Add event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroups_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage ( "GroupDetailPage", "GroupId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_Edit( object sender, RowEventArgs e )
        {
            int groupId;
            if ( gGroups.DataKeyNames[0] == "GroupMemberId" )
            {
                int groupMemberId = e.RowKeyId;
                groupId = new GroupMemberService ( new RockContext () ).GetSelect ( groupMemberId, a => a.GroupId );
            }
            else
            {
                groupId = e.RowKeyId;
            }

            NavigateToLinkedPage ( "GroupDetailPage", "GroupId", groupId );
        }

        /// <summary>
        /// Handles the Click event of the delete/archive button in the grid
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_DeleteOrArchive( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext ();
            GroupService groupService = new GroupService ( rockContext );
            GroupMemberService groupMemberService = new GroupMemberService ( rockContext );
            AuthService authService = new AuthService ( rockContext );
            Group group = null;
            GroupMember groupMember = null;

            // the DataKey Id of the grid is GroupId
            group = groupService.Get ( e.RowKeyId );

            if ( group != null )
            {
                bool isSecurityRoleGroup = group.IsSecurityRole || group.GroupType.Guid.Equals ( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid () );

                // Grid is in 'Group List' mode
                bool archive = false;
                var groupMemberHistoricalService = new GroupHistoricalService ( rockContext );
                if ( group.GroupType.EnableGroupHistory == true && groupMemberHistoricalService.Queryable ().Any ( a => a.GroupId == group.Id ) )
                {
                    // if the group has GroupHistory enabled and has history snapshots, and they were prompted to Archive
                    archive = true;
                }

                if ( archive )
                {
                    if ( !group.IsAuthorized ( Authorization.EDIT, this.CurrentPerson ) )
                    {
                        mdGridWarning.Show ( "You are not authorized to archive this group", ModalAlertType.Information );
                        return;
                    }

                    // NOTE: groupService.Delete will automatically Archive instead Delete if this Group has GroupHistory enabled, but since this block has UI logic for Archive vs Delete, we can do a direct Archive
                    groupService.Archive ( group, this.CurrentPersonAliasId, true );
                }
                else
                {
                    if ( !group.IsAuthorized ( Authorization.EDIT, this.CurrentPerson ) )
                    {
                        mdGridWarning.Show ( "You are not authorized to delete this group", ModalAlertType.Information );
                        return;
                    }

                    string errorMessage;
                    if ( !groupService.CanDelete ( group, out errorMessage ) )
                    {
                        mdGridWarning.Show ( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    groupService.Delete ( group, true );
                }


                rockContext.SaveChanges ();

                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Authorization.Clear ();
                }
            }

            BindGrid ();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gGroups_GridRebind( object sender, EventArgs e )
        {
            BindGrid ();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var groupTypeIds = GetAvailableGroupTypes ();

            if ( groupTypeIds.Count () == 1 )
            {
                // if this block only permits one GroupType, there is no reason to show the GroupType filter.  So hide it
                gtpGroupType.Visible = false;
                gtpGroupType.SelectedValue = null;
            }
            else
            {
                gtpGroupType.Visible = true;
                gtpGroupType.GroupTypes = new GroupTypeService ( new RockContext () ).Queryable ()
                    .Where ( g => groupTypeIds.Contains ( g.Id ) ).ToList ();

                gtpGroupType.SelectedValue = gfSettings.GetUserPreference ( "Group Type" );
            }

            ddlGroupTypePurpose.BindToDefinedType ( DefinedTypeCache.Get ( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid () ), true );
            ddlGroupTypePurpose.SetValue ( gfSettings.GetUserPreference ( "Group Type Purpose" ) );

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue ( gfSettings.GetUserPreference ( "Active Status" ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // Find all the Group Types
            var groupTypeIds = GetAvailableGroupTypes ();

            if ( GetAttributeValue ( "DisplayFilter" ).AsBooleanOrNull () ?? false )
            {
                int? groupTypeFilter = gfSettings.GetUserPreference ( "Group Type" ).AsIntegerOrNull ();
                if ( groupTypeFilter.HasValue )
                {
                    groupTypeIds = groupTypeIds.Where ( g => g == groupTypeFilter.Value ).ToList ();
                }
            }

            // filter to a specific group type if provided in the query string
            if ( !string.IsNullOrWhiteSpace ( RockPage.PageParameter ( "GroupTypeId" ) ) )
            {
                int? groupTypeId = RockPage.PageParameter ( "GroupTypeId" ).AsIntegerOrNull ();

                if ( groupTypeId.HasValue )
                {
                    groupTypeIds.Clear ();
                    groupTypeIds.Add ( groupTypeId.Value );
                }
            }

            var rockContext = new RockContext ();
            var groupService = new GroupService ( rockContext );

            SortProperty sortProperty = gGroups.SortProperty;
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty ( new GridViewSortEventArgs ( "StartDate", SortDirection.Descending ) );
            }

            var attributeValueService = new AttributeValueService ( rockContext );
            var startDateId = AttributeCache.Get ( _startAttributeGuid ).Id;
            var endDateId = AttributeCache.Get ( _endAttributeGuid ).Id;

            var qryGroupMemberAttributes = new AttributeService ( rockContext ).Queryable ().Where ( a => a.Key == _memberKey ).Select ( a => a.Id ).ToList ();
            var qryMemberValues = attributeValueService.Queryable ()
                .Where ( av => qryGroupMemberAttributes.Contains ( av.AttributeId ) && av.Value != "" )
                .Select ( av => av.EntityId )
                .ToList ();

            var qryValues = attributeValueService.Queryable ()
                .Where ( av => av.AttributeId == startDateId && av.Value != "" )
                .Select ( av => av.EntityId )
                .ToList ();

            var qryGroups = groupService.Queryable ()
               .Where ( g => groupTypeIds.Contains ( g.GroupTypeId ) && ( !g.IsSecurityRole ) );
               //.Where ( g => qryValues.Contains ( g.Id ) );

            string limitToActiveStatus = GetAttributeValue ( "LimittoActiveStatus" );

            bool showActive = true;
            bool showInactive = true;

            var attributeService = new AttributeService ( rockContext );
            var qryAttributes = attributeService.Queryable ().Where ( a => a.Key == _undoKey ).Select ( a => a.Id ).ToList ();

            var qryUndoValues = attributeValueService.Queryable ()
                .Where ( av => qryAttributes.Contains ( av.AttributeId ) && av.Value != "" )
                .Select ( av => av.EntityId )
                .ToList ();

            var qryUndoMembers = new GroupMemberService ( rockContext ).Queryable ().Where ( g => qryUndoValues.Contains ( g.Id ) ).Select ( g => g.Id ).ToList ();

            if ( qryUndoMembers.Count() == 0 )
            {
                btnUndo.Visible = true;
                btnUndo.AddCssClass ( "disabled" );
            }
            else
            {
                btnUndo.Visible = true;
                btnUndo.RemoveCssClass ( "disabled" );
            }

            if ( limitToActiveStatus == "all" && gfSettings.Visible )
            {
                // Filter by active/inactive unless the block settings restrict it
                if ( ddlActiveFilter.SelectedIndex > -1 )
                {
                    switch ( ddlActiveFilter.SelectedValue )
                    {
                        case "active":
                            showInactive = false;
                            break;
                        case "inactive":
                            showActive = false;
                            break;
                    }
                }
            }
            else if ( limitToActiveStatus != "all" )
            {
                // filter by the block setting for Active Status
                if ( limitToActiveStatus == "active" )
                {
                    showInactive = false;
                }
            }

            var groupTypePurposeValue = gfSettings.GetUserPreference ( "Group Type Purpose" ).AsIntegerOrNull ();

            var groupList = new List<GroupListRowInfo> ();

            // Grid is in normal 'Group List' mode
            var roleGroupType = GroupTypeCache.Get ( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid () );
            int roleGroupTypeId = roleGroupType != null ? roleGroupType.Id : 0;

            if ( !showInactive )
            {
                qryGroups = qryGroups.Where ( x => x.IsActive );
            }
            else if ( !showActive )
            {
                qryGroups = qryGroups.Where ( x => !x.IsActive );
            }

            if ( groupTypePurposeValue.HasValue && gfSettings.Visible )
            {
                qryGroups = qryGroups.Where ( t => t.GroupType.GroupTypePurposeValueId == groupTypePurposeValue );
            }

            // load with groups that have Group History
            _groupsWithGroupHistory = new HashSet<int> ( new GroupHistoricalService ( rockContext ).Queryable ().Where ( a => qryGroups.Any ( g => g.Id == a.GroupId ) ).Select ( a => a.GroupId ).ToList () );

            groupList = qryGroups
                .AsEnumerable ()
                .Where ( g => g.IsAuthorized ( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                .Select ( g => new GroupListRowInfo
                {
                    Id = g.Id,
                    Path = string.Empty,
                    Name = g.Name,
                    GroupType = GroupTypeCache.Get ( g.GroupTypeId ),
                    GroupOrder = g.Order,
                    Description = g.Description,
                    IsActive = g.IsActive,
                    IsArchived = g.IsArchived,
                    IsActiveOrder = g.IsActive ? 1 : 2,
                    GroupRole = string.Empty,
                    DateAdded = DateTime.MinValue,
                    IsSynced = g.GroupSyncs.Any (),
                    ActiveMemberCount = g.Members.AsQueryable ().Where ( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Count (),
                    InactiveMemberCount = g.Members.AsQueryable ().Where ( m => m.GroupMemberStatus == GroupMemberStatus.Inactive ).Count (),
                    StartDate = new Func<String> ( () =>
                    {
                        Group gr = groupService.Get ( g.Id );
                        if ( gr != null )
                        {
                            gr.LoadAttributes ();

                            var sd = gr.GetAttributeValue ( AttributeCache.Get ( _startAttributeGuid ).Key );
                            if (sd != null && sd != "")
                            {
                                return sd.Replace("T00:00:00.0000000","");
                            }
                            else
                            {
                                return "NO START DATE SET";
                            }
                        }
                        return String.Empty;
                    } ) (),
                    EndDate = new Func<String> ( () =>
                    {
                        Group gr = groupService.Get ( g.Id );
                        if ( gr != null )
                        {
                            gr.LoadAttributes ();

                            var ed = gr.GetAttributeValue ( AttributeCache.Get ( _endAttributeGuid ).Key );
                            if ( ed != null && ed != "" )
                            {
                                return ed.Replace ( "T00:00:00.0000000", "" );
                            }
                            else
                            {
                                return "NO END DATE SET.";
                            }
                        }
                        return String.Empty;
                    } ) ()
                } )
                .AsQueryable ()
                .Sort ( sortProperty )
                .ToList ();

            if ( _showGroupPath )
            {
                foreach ( var groupRow in groupList )
                {
                    groupRow.Path = groupService.GroupAncestorPathName ( groupRow.Id );
                }
            }

            gGroups.DataSource = groupList;
            gGroups.EntityTypeId = EntityTypeCache.Get<Group> ().Id;
            gGroups.DataBind ();

            // hide the group type column if there's only one type; must come after DataBind()
            if ( _groupTypesCount == 1 )
            {
                var groupTypeColumn = this.gGroups.ColumnsOfType<RockBoundField> ().FirstOrDefault ( a => a.DataField == "GroupTypeName" );
                groupTypeColumn.Visible = false;
            }
        }

        /// <summary>
        /// Gets the available group types.
        /// </summary>
        /// <returns></returns>
        private List<int> GetAvailableGroupTypes()
        {
            var groupTypeIds = new List<int> ();
            var checkinTypeIds = new List<int> ();
            var rockContext = new RockContext ();

            var groupTypeService = new GroupTypeService ( rockContext );
            foreach ( var inheritIds in groupTypeService.GetAllCheckinGroupTypePaths () )
            {
                checkinTypeIds.Add ( inheritIds.GroupTypeId );
            }

            var qry = groupTypeService.Queryable ().Where ( t => checkinTypeIds.Contains ( t.Id ) );

            List<Guid> includeGroupTypeGuids = GetAttributeValue ( "IncludeGroupTypes" ).SplitDelimitedValues ().Select ( a => Guid.Parse ( a ) ).ToList ();
            if ( includeGroupTypeGuids.Count > 0 )
            {
                _groupTypesCount = includeGroupTypeGuids.Count;
                qry = qry.Where ( t => includeGroupTypeGuids.Contains ( t.Guid ) );
            }

            List<Guid> excludeGroupTypeGuids = GetAttributeValue ( "ExcludeGroupTypes" ).SplitDelimitedValues ().Select ( a => Guid.Parse ( a ) ).ToList ();
            if ( excludeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where ( t => !excludeGroupTypeGuids.Contains ( t.Guid ) );
            }

            foreach ( int groupTypeId in qry.Select ( t => t.Id ) )
            {
                var groupType = GroupTypeCache.Get ( groupTypeId );
                if ( groupType != null && groupType.IsAuthorized ( Authorization.VIEW, CurrentPerson ) )
                {
                    groupTypeIds.Add ( groupTypeId );
                }
            }

            groupTypeIds = qry.Select ( t => t.Id ).ToList ();

            return groupTypeIds;
        }

        /// <summary>
        /// Sets the panel title and icon.
        /// </summary>
        private void SetPanelTitleAndIcon()
        {
            List<int> groupTypeIds = GetAvailableGroupTypes ();

            // automatically set the panel title and icon based on group type
            // If there's only one group type, use it's 'group term' in the panel title.
            if ( groupTypeIds.Count == 1 )
            {
                var singleGroupType = GroupTypeCache.Get ( groupTypeIds.FirstOrDefault () );
                lTitle.Text = string.Format ( "{0}", singleGroupType.GroupTerm.Pluralize () );
                iIcon.AddCssClass ( singleGroupType.IconCssClass );
            }
            else
            {
                lTitle.Text = BlockName;
                iIcon.AddCssClass ( "fa fa-users" );
            }

            // if a SetPanelTitle is specified in block settings, use that instead
            string customSetPanelTitle = this.GetAttributeValue ( "SetPanelTitle" );
            if ( !string.IsNullOrEmpty ( customSetPanelTitle ) )
            {
                lTitle.Text = customSetPanelTitle;
            }

            // if a SetPanelIcon is specified in block settings, use that instead
            string customSetPanelIcon = this.GetAttributeValue ( "SetPanelIcon" );
            if ( !string.IsNullOrEmpty ( customSetPanelIcon ) )
            {
                iIcon.Attributes["class"] = customSetPanelIcon;
            }
        }

        /// <summary>
        /// Writes the progress message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteProgressMessage( string message, string results )
        {
            _hubContext.Clients.All.receiveNotification ( this.SignalRNotificationKey, message, results.ConvertCrLfToHtmlBr () );
        }

        #endregion

        private class GroupListRowInfo : DotLiquid.Drop
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the group member identifier.
            /// </summary>
            /// <value>
            /// The group member identifier.
            /// </value>
            public int? GroupMemberId { get; set; }

            /// <summary>
            /// Gets or sets the path.
            /// </summary>
            /// <value>
            /// The path.
            /// </value>
            public string Path { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the type of the group.
            /// </summary>
            /// <value>
            /// The type of the group.
            /// </value>
            public GroupTypeCache GroupType { get; set; }

            /// <summary>
            /// Gets the name of the group type.
            /// </summary>
            /// <value>
            /// The name of the group type.
            /// </value>
            public string GroupTypeName
            {
                get
                {
                    return GroupType.Name;
                }
            }

            /// <summary>
            /// Gets the group type order.
            /// </summary>
            /// <value>
            /// The group type order.
            /// </value>
            public int GroupTypeOrder
            {
                get
                {
                    return GroupType.Order;
                }
            }

            /// <summary>
            /// Gets or sets the group order.
            /// </summary>
            /// <value>
            /// The group order.
            /// </value>
            public int GroupOrder { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the group role.
            /// </summary>
            /// <value>
            /// The group role.
            /// </value>
            public string GroupRole { get; set; }

            /// <summary>
            /// Gets or sets the date added.
            /// </summary>
            /// <value>
            /// The date added.
            /// </value>
            public DateTime? DateAdded { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is active.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
            /// </value>
            public bool IsActive { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is archived.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
            /// </value>
            public bool IsArchived { get; set; }

            /// <summary>
            /// Gets or sets the is active order.
            /// </summary>
            /// <value>
            /// The is active order.
            /// </value>
            public int IsActiveOrder { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is synced.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is synced; otherwise, <c>false</c>.
            /// </value>
            public bool IsSynced { get; set; }

            /// <summary>
            /// Gets or sets the member count.
            /// </summary>
            /// <value>
            /// The member count.
            /// </value>
            public int ActiveMemberCount { get; set; }

            public int InactiveMemberCount { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string StartDate { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string EndDate { get; set; }
        }

    }
}