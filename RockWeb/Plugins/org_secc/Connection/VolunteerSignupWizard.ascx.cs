// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace org.secc.Connection
{
    /// <summary>
    /// Block that can load sample data into your Rock database.
    /// Dev note: You can set the XML Document Url setting to your local
    /// file when you're testing new data.  Something like C:\Misc\Rock\Documentation\sampledata.xml
    /// </summary>
    [DisplayName( "Volunteer Signup Wizard" )]
    [Category( "SECC > Connection" )]
    [Description( "Block for configuring and outputing a configurable page for setting numbers of volunteers needed." )]

    [CodeEditorField( "Settings", mode: CodeEditorMode.JavaScript, category: "Custom Setting" )]
    [KeyValueListField( "Counts", category: "Custom Setting" )]
    [KeyValueListField( "Groups", category: "Custom Setting" )]
    [CodeEditorField( "Lava", mode: CodeEditorMode.JavaScript, category: "Custom Setting", defaultValue:
@"<link rel=""stylesheet"" type=""text/css"" href=""/Plugins/org_secc/Connection/VolunteerSignupWizard.css"" />
{%- comment -%}
    Select from one of the following templates that come prebuilt for you with the Signup Wizard and set the
    output variable below to the appropriate value

    Genius     - This will output as a structured table similar to other signup systems out there
    CardPage   - This will output as a single page with panels containing cards.  This is probably best
                 for outputting 1-2 partitions
    CardWizard - This is good for fairly complex signups with 2-4 partitions (Campus, DefinedType, Role, Schedule).
                 It will output and behave in a left-to-right animated set of cards.  The CardWizardMode can be set
				 to ""Single"" or ""Multiple"" which configures the output style to allow for either a single signup or
				 multiple roles/attributes at once.

                 With this layout, you can also add and set the lava variable ""partitionDescriptionShow"" to ""True"" and it will show the description of each partition at the top.

    This is setup to encourage you to copy existing lava templates if you make modifications rather than modifying the
    default ones which come with the Signup Wizard plugin.
{%- endcomment -%}

{%- assign output = ""Genius"" -%}
{%- assign partitionDescriptionShow = ""False"" -%}

{% if output == ""Genius"" %}
{% include '~/Plugins/org_secc/Connection/VolunteerGenius.lava' %}
{% elseif output == ""CardPage"" %}
{% include '~/Plugins/org_secc/Connection/CardPage.lava' %}
{% elseif output == ""CardWizard"" %}
{% assign CardWizardMode = ""Single"" %}
{% include '~/Plugins/org_secc/Connection/CardWizard.lava' %}
{% endif %}
" )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false )]

    [ViewStateModeById]
    public partial class VolunteerSignupWizard : Rock.Web.UI.RockBlockCustomSettings
    {

        private Dictionary<string, string> attributes = new Dictionary<string, string>();

        private ICollection<ConnectionRequest> connectionRequests = null;


        protected SignupSettings Settings
        {
            get
            {
                var settings = GetSetting<SignupSettings>( "Settings" );
                foreach ( var p in settings.Partitions )
                {
                    p.SignupSettings = settings;
                }
                return settings;
            }
            set
            {
                ViewState["Settings"] = value;
                SaveViewState();
            }
        }

        protected Dictionary<string, string> Counts
        {
            get
            {
                return GetSetting<Dictionary<string, string>>( "Counts" );
            }
            set
            {
                ViewState["Counts"] = value;
                SaveViewState();
            }
        }

        protected Dictionary<string, string> Groups
        {
            get
            {
                return GetSetting<Dictionary<string, string>>( "Groups" );
            }
            set
            {
                ViewState["Groups"] = value;
                SaveViewState();
            }
        }

        private T GetSetting<T>( string key ) where T : new()
        {

            if ( ViewState[key] != null )
            {
                try
                {
                    return ( T ) ViewState[key];
                }
                catch ( Exception )
                {
                    // Just swallow this exception
                }
            }
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( key ) ) )
            {
                try
                {
                    if ( BlockCache.Attributes[key].FieldType.Guid == Rock.SystemGuid.FieldType.KEY_VALUE_LIST.AsGuid() )
                    {
                        ViewState[key] = GetAttributeValue( key ).Split( '|' ).Select( x => x.Split( '^' ) ).ToDictionary( x => x[0], x => x[1] );
                    }
                    else
                    {
                        ViewState[key] = GetAttributeValue( key ).FromJsonOrNull<T>();
                    }
                }
                catch ( Exception )
                {
                    // Just swallow this exception
                }
            }
            if ( ViewState[key] == null )
            {
                ViewState[key] = new T();
            }
            SaveViewState();
            return ( T ) ViewState[key];
        }

        #region Base Control Methods

        ////  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            LoadSettings();
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {

            var slDebugTimings = new StringBuilder();
            var stopwatchInitEvents = Stopwatch.StartNew();
            bool showDebugTimings = this.PageParameter( "ShowDebugTimings" ).AsBoolean();

            if ( showDebugTimings )
            {
                TimeSpan tsDuration = RockDateTime.Now.Subtract( ( DateTime ) Context.Items["Request_Start_Time"] );
                slDebugTimings.AppendFormat( "Signup Wizard OnInit [{0}ms] @ {1} \n", stopwatchInitEvents.Elapsed.TotalMilliseconds, tsDuration.TotalMilliseconds );
                stopwatchInitEvents.Restart();
            }
            base.OnLoad( e );


            if ( Settings.Entity() == null )
            {
                nbConfigurationWarning.Visible = true;
                nbConfigurationWarning.Text = "The connection opportunities, partitions, and display settings need to be configured in block settings.";
            }

            if ( !IsPostBack )
            {
                ceLava.Text = GetAttributeValue( "Lava" );
            }

            if ( Settings.Partitions.Count > 0 )
            {
                ConnectionOpportunity connection = ( ConnectionOpportunity ) Settings.Entity();
                if ( connection != null && connectionRequests == null )
                {
                    connectionRequests = connection.ConnectionRequests;
                }

                if ( showDebugTimings )
                {
                    slDebugTimings.AppendFormat( "Loading Settings [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                    stopwatchInitEvents.Restart();
                }

                PreloadPartitionPointers();

                if ( showDebugTimings )
                {
                    slDebugTimings.AppendFormat( "Preloading Partition Pointers [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                    stopwatchInitEvents.Restart();
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "Settings", Rock.Lava.RockFilters.FromJSON( GetAttributeValue( "Settings" ) ) );
                string url = "";
                if ( Settings.SignupPage() != null )
                {
                    url = new Rock.Web.PageReference( Settings.SignupPage().Id ).BuildUrl();
                    if ( Settings.EntityTypeGuid == Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY.AsGuid() )
                    {
                        url += "?OpportunityId=" + Settings.Entity().Id;
                    }
                }


                if ( showDebugTimings )
                {
                    slDebugTimings.AppendFormat( "Loading Merge Fields [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                    stopwatchInitEvents.Restart();
                }
                mergeFields.Add( "Tree", GetTree( Settings.Partitions.FirstOrDefault(), connectionRequests, parentUrl: url ) );


                if ( showDebugTimings )
                {
                    slDebugTimings.AppendFormat( "Loading Tree [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                    stopwatchInitEvents.Restart();
                }
                //mergeFields.Add( "ConnectionRequests", connectionRequests );
                lBody.Text = GetAttributeValue( "Lava" ).ResolveMergeFields( mergeFields );


                if ( showDebugTimings )
                {
                    slDebugTimings.AppendFormat( "Rendering Lava [{0}ms]\n", stopwatchInitEvents.Elapsed.TotalMilliseconds );
                    stopwatchInitEvents.Restart();
                }
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
            if ( showDebugTimings && IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                phTimings.Controls.Add( new Label
                {
                    ID = "lblShowDebugTimingsSignupWizard",
                    Text = string.Format( "<pre>{0}</pre>", slDebugTimings.ToString() )
                } );
            }
        }

        /// <summary>
        /// Set the next partition on each partition. Preloading like this is O(n).
        /// </summary>
        private void PreloadPartitionPointers()
        {
            for ( int i = 0; i < Settings.Partitions.Count - 1; i++ )
            {
                var partition = Settings.Partitions[i];
                partition.NextPartition = Settings.Partitions[i + 1];
            }
        }

        protected override void OnPreRender( EventArgs e )
        {
            bddlAddPartition.SelectedValue = "";
            HideRows();
        }

        #endregion


        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            if ( Settings.Partitions.Count > 0 )
            {
                deactivateTabs();
                liCounts.AddCssClass( "active" );
                pnlCounts.Visible = true;
            }

            var connectionOpportunityService = new ConnectionOpportunityService( new RockContext() );
            var connections = connectionOpportunityService.Queryable().Where( co => co.IsActive == true || co.Guid == Settings.EntityGuid ).OrderBy( co => co.ConnectionType.Name ).ThenBy( co => co.Name ).ToList()
                                                                    .Select( co => new ListItem( co.ConnectionType.Name + ": " + co.Name, co.Guid.ToString() ) ).ToList();
            connections.Insert( 0, new ListItem( "Select One . . ." ) );
            rddlConnection.DataSource = connections;
            rddlConnection.DataTextField = "Text";
            rddlConnection.DataValueField = "Value";
            rddlConnection.DataBind();

            if ( Settings.EntityGuid != Guid.Empty )
            {
                rddlConnection.SelectedValue = Settings.EntityGuid.ToString();
            }

            if ( Settings.EntityTypeGuid == Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY.AsGuid() )
            {
                rddlType.SelectedValue = "Connection";
                rddlConnection.Visible = true;
            }
            else if ( Settings.EntityTypeGuid == Rock.SystemGuid.EntityType.GROUP.AsGuid() )
            {
                rddlType.SelectedValue = "Group";
                gpGroup.Visible = true;
            }


            mdEdit.Show();
        }

        private void UpdateCounts()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            Dictionary<string, string> groups = new Dictionary<string, string>();
            foreach ( GridViewRow gridRow in gCounts.Rows )
            {
                string rowId = gCounts.DataKeys[gridRow.RowIndex].Value.ToString();

                RockTextBox textBox = ( RockTextBox ) gridRow.FindControl( "tb" + rowId );
                if ( textBox != null )
                {
                    // If a user sets an existing count to 0 - remove it!
                    if ( textBox.Text.Trim() == "0" )
                    {
                        values.Remove( rowId );
                    }

                    if ( values.ContainsKey( rowId ) )
                    {
                        values[rowId] = textBox.Text;
                    }
                    else
                    {
                        values.Add( rowId, textBox.Text );
                    }
                }

                RockDropDownList groupPicker = ( RockDropDownList ) gridRow.FindControl( rowId + "_group" );
                if ( groupPicker != null )
                {
                    if ( string.IsNullOrWhiteSpace( groupPicker.SelectedValue ) )
                    {
                        groups.Remove( rowId );
                    }
                    else if ( groups.ContainsKey( rowId ) )
                    {
                        groups[rowId] = groupPicker.SelectedValue;
                    }
                    else
                    {
                        groups.Add( rowId, groupPicker.SelectedValue );
                    }

                }
            }

            Counts = values.Where( a => a.Value != "0" ).ToDictionary( a => a.Key, a => a.Value );
            Groups = groups.Where( a => a.Value != "0" ).ToDictionary( a => a.Key, a => a.Value );
            // Clear out the legacy group map
            Settings.Partitions.ForEach( p => p.GroupMap = null );
            SaveViewState();
        }

        private void LoadSettings()
        {
            using ( var context = new RockContext() )
            {
                GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( context );
                ScheduleService scheduleService = new ScheduleService( context );

                if ( Settings.SignupPage() != null )
                {
                    ppSignupPage.SetValue( Settings.SignupPage().Id );
                }

                // Load all the partition settings
                if ( Settings.EntityGuid != Guid.Empty )
                {
                    pnlPartitions.Visible = true;
                }

                rptPartions.DataSource = Settings.Partitions;
                rptPartions.DataBind();

                // Remove all existing dynamic columns
                while ( gCounts.Columns.Count > 2 )
                {
                    gCounts.Columns.RemoveAt( 0 );
                }
                DataTable dt = new DataTable();
                dt.Columns.Add( "RowId" );
                foreach ( var partition in Settings.Partitions )
                {
                    DataTable dtTmp = dt.Copy();
                    dt.Clear();
                    String column = partition.PartitionType;
                    if ( partition.PartitionType == "DefinedType" )
                    {
                        var definedType = Rock.Web.Cache.DefinedTypeCache.Get( partition.PartitionValue.AsGuid() );
                        if ( definedType == null )
                        {
                            break;
                        }
                        column = definedType.Name;
                    }
                    var boundField = new BoundField() { HeaderText = column, DataField = column + partition.Guid };
                    gCounts.Columns.Insert( gCounts.Columns.Count - 2, boundField );
                    dt.Columns.Add( column + partition.Guid );

                    switch ( partition.PartitionType )
                    {
                        case "DefinedType":

                            var definedType = Rock.Web.Cache.DefinedTypeCache.Get( partition.PartitionValue.AsGuid() );
                            var definedValues = definedType.DefinedValues;

                            var filterControl = AddColumnFilter( partition, definedType.DefinedValues.Select( dv => new ListItem( dv.Value, dv.Guid.ToString() ) ).ToList(), definedType.Name );

                            // Apply the filtering (if applicable)
                            /*if ( filterControl.SelectedValues.Count() > 0 )
                            {
                                definedValues = definedValues.Where( dv => filterControl.SelectedValues.Contains( dv.Guid.ToString() ) ).ToList();
                            }*/

                            foreach ( var value in definedValues )
                            {
                                AddRowColumnPartition( dtTmp, dt, column + partition.Guid, value.Guid, value.Value );
                            }

                            break;
                        case "Campus":
                            if ( partition.PartitionValue != null )
                            {
                                var selectedCampuses = partition.PartitionValue.Split( ',' );
                                foreach ( string campusGuid in selectedCampuses )
                                {
                                    var campus = CampusCache.Get( campusGuid.AsGuid() );
                                    if ( campus != null )
                                    {
                                        AddRowColumnPartition( dtTmp, dt, column + partition.Guid, campus.Guid, campus.Name );
                                    }
                                }
                                AddColumnFilter( partition, CampusCache.All().Where( c => selectedCampuses.Contains( c.Guid.ToString() ) ).Select( c => new ListItem( c.Name, c.Guid.ToString() ) ).ToList(), "Campus"  );
                            }
                            break;
                        case "Schedule":
                            if ( partition.PartitionValue != null )
                            {
                                var selectedSchedules = partition.PartitionValue.Split( ',' );
                                var scheduleList = scheduleService.Queryable().Where( s => selectedSchedules.Contains( s.Guid.ToString() ) ).ToList();
                                foreach ( var schedule in scheduleList )
                                {
                                    AddRowColumnPartition( dtTmp, dt, column + partition.Guid, schedule.Guid, schedule.Name );
                                }

                                AddColumnFilter( partition, scheduleList.Select( c => new ListItem( c.Name, c.Guid.ToString() ) ).ToList(), "Schedule" );
                            }
                            break;
                        case "Role":
                            if ( partition.PartitionValue != null )
                            {
                                var selectedRoles = partition.PartitionValue.Split( ',' );
                                List<GroupTypeRole> roles = new List<GroupTypeRole>();
                                foreach ( string roleGuid in selectedRoles )
                                {
                                    GroupTypeRole role = groupTypeRoleService.Get( roleGuid.AsGuid() );
                                    if ( role != null )
                                    {
                                        roles.Add( role );
                                    }
                                }
                                roles.OrderBy( r => r.GroupTypeId ).ThenBy( r => r.Order );

                                foreach ( GroupTypeRole role in roles )
                                {
                                    AddRowColumnPartition( dtTmp, dt, column + partition.Guid, role.Guid, role.Name );
                                }

                                AddColumnFilter( partition, roles.Select( r => new ListItem( r.Name, r.Guid.ToString() ) ).ToList(), "Role" );
                            }
                            break;

                    }
                }
                if ( Settings.Partitions.Count > 0 && dt.Rows.Count > 0 )
                {
                    var dv = dt.AsEnumerable();
                    var dvOrdered = dv.OrderBy( r => r.Field<String>( dt.Columns.Cast<DataColumn>().Select( c => c.ColumnName ).Skip( 1 ).FirstOrDefault() ) );
                    foreach ( var column in dt.Columns.Cast<DataColumn>().Select( c => c.ColumnName ).Skip( 2 ) )
                    {
                        dvOrdered = dvOrdered.ThenBy( r => r.Field<String>( column ) );
                        break;
                    }
                    dt = dvOrdered.CopyToDataTable();
                    gCounts.DataSource = dt;
                    gCounts.DataBind();

                }
            }
        }

        private RockListBox AddColumnFilter( PartitionSettings partition, List<ListItem> listItems, string label )
        {
            var filterSelections = GetBlockUserPreference( partition.Guid + "_filter" ).Split( ',' );
            listItems.ForEach(li => li.Selected = filterSelections.Contains( li.Value) );
            var filterListBox = new RockListBox();
            filterListBox.Items.AddRange( listItems.ToArray() );
            filterListBox.ID = partition.Guid + "_filter";
            filterListBox.Label = label;
            gFilter.Controls.Add( filterListBox );
            return filterListBox;
        }

        private void AddRowColumnPartition( DataTable source, DataTable target, String columnKey, Guid guid, String value )
        {
            if ( source.Rows.Count == 0 )
            {
                var newRow = target.NewRow();
                newRow["RowId"] = guid.ToString();
                target.Rows.Add( newRow );
                newRow[columnKey] = value;
            }
            foreach ( DataRow rowTmp in source.Rows )
            {
                var newRow = target.NewRow();
                foreach ( DataColumn columnTmp in source.Columns )
                {
                    newRow[columnTmp.ColumnName] = rowTmp[columnTmp.ColumnName];
                }
                newRow["RowId"] = rowTmp["RowId"] + "," + guid.ToString();
                newRow[columnKey] = value;
                target.Rows.Add( newRow );
            }
        }


        [Serializable]
        public class PartitionSettings
        {
            public string AttributeKey { get; set; }
            public string PartitionType { get; set; }
            public string PartitionValue { get; set; }
            /// <summary>
            /// Only used for multi-step selections e.g. A Defined Type must be selected be defined values can be chosen. The Defined Type would be the Partition Value, the sub values are the selected defined values
            /// </summary>
            /// <value>
            /// The partition group.
            /// </value>
            public string PartitionSubValues { get; set; }
            public Guid Guid { get; set; }
            public Dictionary<string, string> GroupMap { get; set; }

            [JsonIgnore]
            public SignupSettings SignupSettings { get; set; }

            [JsonIgnore]
            public PartitionSettings NextPartition
            {
                get; set;
            }
        }

        [Serializable]
        public class SignupSettings
        {
            public Guid SignupPageGuid { get; set; }

            private List<PartitionSettings> _partitions = new List<PartitionSettings>();

            public List<PartitionSettings> Partitions { get { return _partitions; } }

            public Guid EntityTypeGuid { get; set; }

            public Guid EntityGuid { get; set; }

            /// <summary>
            /// Get the entity (group or connection) for this signup
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public virtual IModel Entity( RockContext context = null )
            {
                if ( context == null )
                {
                    context = new RockContext();
                }
                if ( EntityTypeGuid == Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY.AsGuid() )
                {
                    ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( context );
                    return connectionOpportunityService.Get( EntityGuid );
                }

                if ( EntityTypeGuid == Rock.SystemGuid.EntityType.GROUP.AsGuid() )
                {
                    GroupService groupService = new GroupService( context );
                    return groupService.Get( EntityGuid );
                }
                return null;
            }

            /// <summary>
            /// Get the signup page
            /// </summary>
            /// <returns></returns>
            public virtual PageCache SignupPage()
            {
                return PageCache.Get( SignupPageGuid );
            }
        }

        #region Events

        protected void lbSettings_Click( object sender, EventArgs e )
        {
            UpdateCounts();
            deactivateTabs();
            liSettings.AddCssClass( "active" );
            pnlSettings.Visible = true;
        }

        protected void lbCounts_Click( object sender, EventArgs e )
        {
            UpdateCounts();
            deactivateTabs();
            liCounts.AddCssClass( "active" );
            pnlCounts.Visible = true;

        }

        protected void lbLava_Click( object sender, EventArgs e )
        {
            UpdateCounts();
            deactivateTabs();
            liLava.AddCssClass( "active" );
            pnlLava.Visible = true;
        }

        private void deactivateTabs()
        {
            liSettings.RemoveCssClass( "active" );
            liCounts.RemoveCssClass( "active" );
            liLava.RemoveCssClass( "active" );
            pnlSettings.Visible = false;
            pnlCounts.Visible = false;
            pnlLava.Visible = false;

        }

        /// <summary>
        /// Private list of groups applicable for the current connection opportunity
        /// </summary>
        private List<Group> _groups = null;

        /// <summary>
        /// Get the list of groups for this connection opportunity
        /// </summary>
        /// <returns>List of groups</returns>
        protected List<Group> GetGroups()
        {
            if ( _groups != null )
            {
                return _groups;
            }

            ConnectionOpportunity opportunity = ( ( ConnectionOpportunity ) Settings.Entity() );
            int[] campusIds = opportunity.ConnectionOpportunityCampuses.Select( o => o.CampusId ).ToArray();
            // Build list of groups
            var groups = new List<Group>();

            // First add any groups specifically configured for the opportunity
            var opportunityGroupIds = opportunity.ConnectionOpportunityGroups.Select( o => o.Id ).ToList();
            if ( opportunityGroupIds.Any() )
            {
                groups = opportunity.ConnectionOpportunityGroups
                    .Where( g =>
                        g.Group != null &&
                        g.Group.IsActive &&
                        ( !g.Group.CampusId.HasValue || campusIds.Contains( g.Group.CampusId.Value ) ) )
                    .Select( g => g.Group )
                    .ToList();
            }

            // Then get any groups that are configured with 'all groups of type'
            foreach ( var groupConfig in opportunity.ConnectionOpportunityGroupConfigs )
            {
                if ( groupConfig.UseAllGroupsOfType )
                {
                    var existingGroupIds = groups.Select( g => g.Id ).ToList();

                    groups.AddRange( new GroupService( new RockContext() )
                        .Queryable().AsNoTracking()
                        .Where( g =>
                            !existingGroupIds.Contains( g.Id ) &&
                            g.IsActive &&
                            g.GroupTypeId == groupConfig.GroupTypeId &&
                            ( !g.CampusId.HasValue || campusIds.Contains( g.CampusId.Value ) ) )
                        .ToList() );
                }
            }
            _groups = groups;
            return groups;
        }

        protected void rptPartions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {

                /*var ddlAttribute = ( DropDownList ) e.Item.FindControl( "ddlAttribute" );
                ddlAttribute.DataSource = attributes;
                ddlAttribute.DataTextField = "Value";
                ddlAttribute.DataValueField = "Key";
                ddlAttribute.DataBind();
                */

                var partition = ( ( PartitionSettings ) e.Item.DataItem );
                var phPartitionControl = ( PlaceHolder ) e.Item.FindControl( "phPartitionControl" );
                RockTextBox tbAttributeKey = ( ( RockTextBox ) e.Item.FindControl( "tbAttributeKey" ) );
                switch ( partition.PartitionType )
                {
                    case "Campus":

                        foreach ( var campus in CampusCache.All() )
                        {
                            var campusCbl = new CheckBox();
                            campusCbl.ID = campus.Guid.ToString() + "_" + partition.Guid + "_checkbox";
                            if ( partition.PartitionValue != null )
                            {
                                campusCbl.Checked = partition.PartitionValue.Contains( campus.Guid.ToString() );
                            }
                            campusCbl.Text = campus.Name;
                            campusCbl.CheckedChanged += CampusCbl_CheckedChanged;
                            campusCbl.AutoPostBack = true;
                            if ( partition.PartitionValue != null )
                            {
                                campusCbl.Checked = partition.PartitionValue.Contains( campus.Guid.ToString() );
                            }
                            phPartitionControl.Controls.Add( campusCbl );
                        }
                        tbAttributeKey.ReadOnly = true;

                        break;
                    case "Schedule":
                        var schedule = new SchedulePicker() { AllowMultiSelect = true, ID = partition.Guid.ToString() };
                        schedule.SelectItem += Schedule_SelectItem;
                        if ( !string.IsNullOrWhiteSpace( partition.PartitionValue ) )
                        {
                            ScheduleService scheduleService = new ScheduleService( new RockContext() );
                            List<Guid> scheduleGuids = partition.PartitionValue.Split( ',' ).Select( pv => pv.AsGuid() ).ToList();

                            schedule.SetValues( scheduleService.GetByGuids( scheduleGuids ).ToList().Select( s => s.Id ).ToList() );
                        }
                        phPartitionControl.Controls.Add( schedule );
                        break;
                    case "DefinedType":
                        e.Item.ID = "DefinedType" + partition.Guid.ToString();

                        var definedTypeRddl = new RockDropDownList() { ID = partition.Guid.ToString() };
                        DefinedTypeService definedTypeService = new DefinedTypeService( new RockContext() );
                        var listItems = definedTypeService.Queryable().Select( dt => new { Name = ( dt.Category != null ? dt.Category.Name + ": " : "" ) + dt.Name, Guid = dt.Guid } ).ToList();
                        listItems.Insert( 0, new { Name = "Select One . . .", Guid = Guid.Empty } );
                        definedTypeRddl.DataSource = listItems;
                        definedTypeRddl.DataTextField = "Name";
                        definedTypeRddl.DataValueField = "Guid";
                        definedTypeRddl.DataBind();
                        definedTypeRddl.AutoPostBack = true;
                        definedTypeRddl.SelectedIndexChanged += DefinedTypeRddl_SelectedIndexChanged;
                        phPartitionControl.Controls.Add( definedTypeRddl );
                        var additionalControls = new DynamicPlaceholder() { ID = "phAdditionalControls" };
                        phPartitionControl.Controls.Add( additionalControls );

                        if ( !string.IsNullOrWhiteSpace( partition.PartitionValue ) )
                        {
                            definedTypeRddl.SelectedValue = partition.PartitionValue;
                            SetUpDefinedTypeDynamicControls( additionalControls, partition.PartitionValue.AsGuid(), partition );
                        }
                        break;
                    case "Role":
                        if ( Settings.EntityTypeGuid == Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY.AsGuid() )
                        {
                            ConnectionOpportunity connection = ( ConnectionOpportunity ) Settings.Entity();
                            var roles = connection.ConnectionOpportunityGroupConfigs.Where( cogc => cogc.GroupMemberRole != null ).OrderBy( r => r.GroupTypeId ).ThenBy( r => r.GroupMemberRole.Order ).Select( r => new { Name = r.GroupType.Name + ": " + r.GroupMemberRole.Name, Guid = r.GroupMemberRole.Guid } ).ToList();


                            foreach ( var role in roles )
                            {

                                var roleCbl = new CheckBox();
                                roleCbl.ID = role.Guid.ToString() + "_" + partition.Guid + "_checkbox";
                                if ( !string.IsNullOrWhiteSpace( partition.PartitionValue ) )
                                {
                                    roleCbl.Checked = partition.PartitionValue.Contains( role.Guid.ToString() );
                                }
                                roleCbl.Text = role.Name;
                                roleCbl.CheckedChanged += CampusCbl_CheckedChanged;
                                roleCbl.AutoPostBack = true;
                                phPartitionControl.Controls.Add( roleCbl );
                            }
                            tbAttributeKey.ReadOnly = true;
                        }

                        break;
                }

                tbAttributeKey.ID = "attribute_key_" + partition.Guid.ToString();
                tbAttributeKey.Text = partition.AttributeKey;
                ( ( LinkButton ) e.Item.FindControl( "bbPartitionDelete" ) ).CommandArgument = partition.Guid.ToString();
            }
        }

        private void CampusCbl_CheckedChanged( object sender, EventArgs e )
        {
            var partition = Settings.Partitions.Where( p => ( ( Control ) sender ).ID.Contains( p.Guid.ToString() ) ).FirstOrDefault();
            if ( partition != null )
            {
                string valueGuid = ( ( Control ) sender ).ID.Replace( partition.Guid.ToString() + "_", "" ).Replace( "_checkbox", "" );

                // Defined Types don't store defined values in PartitionValue
                if ( partition.PartitionType == "DefinedType" )
                {
                    List<string> selectedValues = new List<string>();
                    if ( partition.PartitionSubValues != null )
                    {
                        selectedValues = partition.PartitionSubValues.Trim( ',' ).Split( ',' ).ToList();
                    }
                    selectedValues.Remove( valueGuid );
                    if ( ( ( CheckBox ) sender ).Checked )
                    {
                        selectedValues.Add( valueGuid );
                    }
                    partition.PartitionSubValues = String.Join( ",", selectedValues );
                }
                else
                {
                    List<string> selectedValues = new List<string>();
                    if ( partition.PartitionValue != null )
                    {
                        selectedValues = partition.PartitionValue.Trim( ',' ).Split( ',' ).ToList();
                    }
                    selectedValues.Remove( valueGuid );
                    if ( ( ( CheckBox ) sender ).Checked )
                    {
                        selectedValues.Add( valueGuid );
                    }
                    partition.PartitionValue = String.Join( ",", selectedValues );
                }
            }
            SaveViewState();
        }

        protected void tbAttributeKey_TextChanged( object sender, EventArgs e )
        {
            var partition = Settings.Partitions.Where( p => p.Guid == ( ( Control ) sender ).ID.Replace( "attribute_key_", "" ).AsGuid() ).FirstOrDefault();
            if ( partition != null )
            {
                partition.AttributeKey = ( ( TextBox ) sender ).Text;
            }
            SaveViewState();
        }

        protected void BddlAddPartition_SelectionChanged( object sender, EventArgs e )
        {
            if ( Counts.Count > 0 )
            {
                hdnPartitionType.Value = ( ( DropDownList ) sender ).SelectedValue;
                ScriptManager.RegisterStartupScript( upEditControls, upEditControls.GetType(), "PartitionWarning", "Rock.dialogs.confirm('Making changes to partition settings can affect existing counts!  Are you sure you want to proceed?', function(result) {if(result) {$(\"#" + btnAddPartition.ClientID + "\")[0].click();}});", true );
                return;
            }
            var partition = new PartitionSettings() { PartitionType = ( ( DropDownList ) sender ).SelectedValue, Guid = Guid.NewGuid(), SignupSettings = Settings };
            if ( partition.PartitionType == "Role" )
            {
                partition.AttributeKey = "GroupTypeRole";
            }
            else if ( partition.PartitionType == "Campus" )
            {
                partition.AttributeKey = "Campus";
            }
            Settings.Partitions.Add( partition );
            SaveViewState();
            rptPartions.DataSource = Settings.Partitions;
            rptPartions.DataBind();
        }

        protected void btnAddPartition_Click( object sender, EventArgs e )
        {
            var partition = new PartitionSettings() { PartitionType = hdnPartitionType.Value, Guid = Guid.NewGuid(), SignupSettings = Settings };
            if ( partition.PartitionType == "Role" )
            {
                partition.AttributeKey = "GroupTypeRole";
            }
            else if ( partition.PartitionType == "Campus" )
            {
                partition.AttributeKey = "Campus";
            }
            Settings.Partitions.Add( partition );
            SaveViewState();
            rptPartions.DataSource = Settings.Partitions;
            rptPartions.DataBind();
        }

        protected void bbPartitionDelete_Click( object sender, EventArgs e )
        {
            Settings.Partitions.Remove( Settings.Partitions.Where( p => p.Guid == ( ( LinkButton ) sender ).CommandArgument.AsGuid() ).FirstOrDefault() );
            SaveViewState();
            rptPartions.DataSource = Settings.Partitions;
            rptPartions.DataBind();
        }

        protected void GPicker_SelectItem( object sender, EventArgs e )
        {
            int groupId = ( ( GroupPicker ) ( ( HtmlAnchor ) sender ).Parent ).SelectedValue.AsInteger();
            GroupService groupService = new GroupService( new RockContext() );
            Settings.EntityGuid = groupService.Get( groupId ).Guid;
        }

        protected void ConnectionRddl_SelectedIndexChanged( object sender, EventArgs e )
        {
            Settings.EntityGuid = ( ( RockDropDownList ) sender ).SelectedValue.AsGuid();
            SaveViewState();
            pnlPartitions.Visible = true;
        }

        protected void RddlType_SelectedIndexChanged( object sender, EventArgs e )
        {
            gpGroup.Visible = false;
            rddlConnection.Visible = false;
            if ( ( ( RockDropDownList ) sender ).SelectedValue == "Group" )
            {
                gpGroup.Visible = true;
                Settings.EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP.AsGuid();
            }
            else if ( ( ( RockDropDownList ) sender ).SelectedValue == "Connection" )
            {
                rddlConnection.Visible = true;
                Settings.EntityTypeGuid = Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY.AsGuid();
            }
            SaveViewState();
        }


        protected void mdEdit_SaveClick( object sender, EventArgs e )
        {
            UpdateCounts();
            SetAttributeValue( "Settings", Settings.ToJson() );
            SetAttributeValue( "Counts", string.Join( "|", Counts.Where( a => a.Value != "0" ).Select( a => a.Key + "^" + a.Value ).ToList() ) );
            SetAttributeValue( "Groups", string.Join( "|", Groups.Where( a => a.Value != "0" ).Select( a => a.Key + "^" + a.Value ).ToList() ) );
            SetAttributeValue( "Lava", ceLava.Text );
            SaveAttributeValues();
            mdEdit.Hide();
            Response.Redirect( Request.RawUrl );
        }

        private void Schedule_SelectItem( object sender, EventArgs e )
        {
            var partition = Settings.Partitions.Where( p => p.Guid == ( ( Control ) sender ).Parent.ID.AsGuid() ).FirstOrDefault();
            if ( partition != null )
            {
                List<int> scheduleIds = ( ( SchedulePicker ) ( ( Control ) sender ).Parent ).SelectedValues.Select( i => i.AsInteger() ).ToList();
                ScheduleService scheduleService = new ScheduleService( new RockContext() );
                partition.PartitionValue = String.Join( ",", scheduleService.GetByIds( scheduleIds ).ToList().OrderBy( s => s.GetNextStartDateTime( RockDateTime.Now ) ).Select( s => s.Guid.ToString() ) );
            }
            SaveViewState();
        }

        private void SetUpDefinedTypeDynamicControls( PlaceHolder placeHolder, Guid selectedDefinedType, PartitionSettings partition )
        {
            var definedType = DefinedTypeCache.Get( selectedDefinedType );
            var definedValues = definedType.DefinedValues.OrderBy( r => r.Order ).Select( r => new { Value = r.Value, Guid = r.Guid } ).ToList();

            foreach ( var definedValue in definedValues )
            {

                var definedValueCbl = new CheckBox();
                definedValueCbl.ID = definedValue.Guid.ToString() + "_" + partition.Guid + "_checkbox";
                if ( !string.IsNullOrWhiteSpace( partition.PartitionSubValues ) )
                {
                    definedValueCbl.Checked = partition.PartitionSubValues.Contains( definedValue.Guid.ToString() );
                }
                definedValueCbl.Text = definedValue.Value;
                definedValueCbl.CheckedChanged += CampusCbl_CheckedChanged;
                definedValueCbl.AutoPostBack = true;
                placeHolder.Controls.Add( definedValueCbl );
            }
            SaveViewState();
        }

        private void DefinedTypeRddl_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddl = ( ( Control ) sender );
            var repeater = ddl.NamingContainer.NamingContainer;

            if ( repeater == null )
            {
                throw new Exception( "Could not find repeater" );
            }

            var partition = Settings.Partitions.Where( p => p.Guid == ( ddl.ID.AsGuid() ) ).FirstOrDefault();
            var selectedValue = ( ( RockDropDownList ) sender ).SelectedValue;
            if ( partition != null )
            {
                partition.PartitionValue = selectedValue;
            }

            var repeaterItem = ( RepeaterItem ) repeater.FindControl( "DefinedType" + ddl.ID );
            var placeHolder = ( DynamicPlaceholder ) repeaterItem.FindControl( "phAdditionalControls" );
            if ( repeaterItem == null || placeHolder == null )
            {
                throw new Exception( "Could not find defined type control or its child placeholder" );
            }

            SetUpDefinedTypeDynamicControls( placeHolder, selectedValue.AsGuid(), partition );
        }

        protected void gCounts_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {

                string rowId = gCounts.DataKeys[e.Row.RowIndex].Value.ToString();
                RockTextBox textBox = new RockTextBox();
                textBox.Width = 60;
                textBox.Text = "";
                textBox.ID = "tb" + rowId;
                if ( Counts.ContainsKey( rowId ) )
                {
                    textBox.Text = Counts[rowId];
                }
                else
                {
                    textBox.Text = "0";
                }
                e.Row.Cells[gCounts.Columns.Count - 1].Controls.Add( textBox );

                // Create and set the value for a group picker for each partition option
                var ddlPlacementGroup = new RockDropDownList();
                ddlPlacementGroup.ID =  rowId + "_group";

                List<ListItem> groupList = GetGroups().Select( g => new ListItem( String.Format( "{0} ({1})", g.Name, g.Campus != null ? g.Campus.Name : "No Campus" ), g.Id.ToString() ) ).ToList();
                groupList.Insert( 0, new ListItem( "Not Mapped", null ) );
                ddlPlacementGroup.Items.AddRange( groupList.ToArray() );

                if ( Groups.ContainsKey( rowId ) )
                {
                    ddlPlacementGroup.SetValue( Groups[rowId].AsIntegerOrNull() );
                } else
                {
                    var keys = rowId.Split( ',' );
                    foreach ( var partition in Settings.Partitions )
                    {
                        foreach( var key in keys )
                        {
                            if ( partition.GroupMap != null && partition.GroupMap.ContainsKey( key ) )
                            {
                                ddlPlacementGroup.SetValue( partition.GroupMap[key].AsIntegerOrNull() );
                            }
                        }
                    }
                    
                }
                e.Row.Cells[gCounts.Columns.Count - 2].Controls.Add( ddlPlacementGroup );
            }
        }

        protected List<Schedule> Schedules { get ; set; }
        protected List<GroupTypeRole> GroupTypeRoles { get; set; }

        /// <summary>
        /// Recursively builds a tree like structure of dictionaries which represent the nodes of the tree. Each node is an option which belongs to a partition.
        /// </summary>
        /// <param name="partition">The partition to get the nodes of</param>
        /// <param name="connectionRequests">A subset of the connection requests that belong to signup oppportunity. Each recursive call filters the requests further - the requests are used to calculate the amount of filled spots each node has.</param>
        /// <param name="concatGuid">The unique identifier for each node that is built using a comma separated concat of guids for the options taken so far in this recursive walk of the tree. There is a guid for each layer of partition so far.</param>
        /// <param name="parentIdentifier">The parent identifier.</param>
        /// <param name="parentUrl">The parent URL.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="roleId">The role identifier.</param>
        /// <returns></returns>
        protected List<Dictionary<string, object>> GetTree( PartitionSettings partition, ICollection<ConnectionRequest> connectionRequests = null, String concatGuid = null, string parentIdentifier = "signup", string parentUrl = "", string groupId = "", string roleId = "" )
        {
            if (Schedules == null)
            {
                Schedules = new List<Schedule>();
                if ( Settings.Partitions.Any( p => p.PartitionType == "Schedule" ) )
                {
                    var scheduleService = new ScheduleService( new RockContext() );
                    var scheduleGuidList = Settings.Partitions.Where( p => p.PartitionType == "Schedule" && p.PartitionValue != null).SelectMany( p => p.PartitionValue.Trim( ',' ).Split( ',' ).AsGuidList() ).ToArray();
                    Schedules = scheduleService.Queryable().Where(s => scheduleGuidList.Contains( s.Guid ) ).ToList().OrderBy( s => s.GetNextStartDateTime( DateTime.Now)??s.GetFirstStartDateTime() ).ToList();
                }
            }
            if ( GroupTypeRoles == null )
            {
                var groupTypeRoleService = new GroupTypeRoleService( new RockContext() );
                GroupTypeRoles = groupTypeRoleService.Queryable().ToList();
            }

            if ( partition.PartitionValue == null )
            {
                return null;
            }

            var values = partition.PartitionValue.Trim( ',' ).Split( ',' );

            // Defined Types don't store the defined values in PartitionValue, that's where the defined type guid is stored.
            if ( partition.PartitionType == "DefinedType" )
            {
                if ( !string.IsNullOrWhiteSpace( partition.PartitionSubValues ) )
                {
                    values = partition.PartitionSubValues.SplitDelimitedValues().ToArray();
                }
                else
                {
                    // Use every Defined Value
                    values = DefinedTypeCache.Get( partition.PartitionValue.AsGuid() ).DefinedValues.Select( dv => dv.Guid.ToString() ).ToArray();
                }
            }

            if ( partition.PartitionType == "Schedule" )
            {
                values = Schedules.Where(s => values.AsGuidList().Contains(s.Guid) ).Select( s => s.Guid.ToString() ).ToArray();
            }

            var partitionList = new List<Dictionary<string, object>>();

            // For each inner node in this partition, build a dictionary that represents it
            foreach ( var value in values )
            {
                // Concat id is the concat of chosen options' guids which is why we use contains here
                var newConcatGuid = concatGuid == null ? value : concatGuid + "," + value;
                int? limit = GetLimitForTreeNode( newConcatGuid );


                // null here is unlimited, (i.e. no limit) - not an error!
                if ( limit != null && limit == 0 )
                {
                    continue;
                }


                if ( partition.PartitionType == "Role" )
                {
                    roleId = value;
                }
                int? newGroupId = Groups.Where( kvp => kvp.Key.Contains( newConcatGuid ) && kvp.Value != "Not Mapped" ).Select( v => v.Value.AsIntegerOrNull() ).FirstOrDefault();

                if ( partition.GroupMap != null && partition.GroupMap.ContainsKey( value ) && partition.GroupMap[value] != "Not Mapped" )
                {
                    groupId = partition.GroupMap[value];
                }

                string url = parentUrl + ( parentUrl.Contains( "?" ) ? "&" : "?" ) + partition.AttributeKey + "=" + value;
                Dictionary<string, object> inner = new Dictionary<string, object>();
                inner.Add( "ParentIdentifier", parentIdentifier );
                inner.Add( "PartitionType", partition.PartitionType );
                inner.Add( "Url", url );
                inner.Add( "ParameterName", partition.AttributeKey );
                inner.Add( "Value", value );
                inner.Add( "RoleGuid", roleId );
                inner.Add( "GroupId", newGroupId.HasValue?newGroupId.Value.ToString():groupId );
                inner.Add( "Limit", limit );

                // Filter the requests recursively depending on the type of partition
                ICollection<ConnectionRequest> subRequests = null;
                switch ( partition.PartitionType )
                {
                    case "DefinedType":
                        inner["Entity"] = DefinedValueCache.Get( value.AsGuid() );
                        if ( connectionRequests != null )
                        {
                            subRequests = connectionRequests.Where( cr => cr.AssignedGroupMemberAttributeValues != null && cr.AssignedGroupMemberAttributeValues.Contains( value ) ).ToList();
                        }
                        break;
                    case "Schedule":
                        inner["Entity"] = Schedules.Where(s => s.Guid == value.AsGuid() ).FirstOrDefault();
                        if ( connectionRequests != null )
                        {
                            subRequests = connectionRequests.Where( cr => cr.AssignedGroupMemberAttributeValues != null && cr.AssignedGroupMemberAttributeValues.Contains( value ) ).ToList();
                        }
                        break;
                    case "Campus":
                        var campus = CampusCache.Get( value.AsGuid() );
                        inner["Entity"] = campus;
                        if ( connectionRequests != null && campus != null )
                        {
                            subRequests = connectionRequests.Where( cr => cr.CampusId == campus.Id ).ToList();
                        }
                        break;
                    case "Role":
                        var role = GroupTypeRoles.Where(gtr => gtr.Guid == value.AsGuid() ).FirstOrDefault();
                        inner["Entity"] = role;
                        if ( connectionRequests != null && role != null )
                        {
                            subRequests = connectionRequests.Where( cr => cr.AssignedGroupMemberRoleId == role.Id ).ToList();
                        }
                        break;
                }

                string childIdentifier = parentIdentifier + "_" + value;

                if ( partition.NextPartition != null )
                {
                    inner.Add( "Partitions", GetTree( partition.NextPartition, subRequests, newConcatGuid, childIdentifier, url, groupId, roleId ) );
                    if ( inner["Partitions"] != null )
                    {
                        // The amount filled for this inner node in the partition is the sum of the amount filled of the nodes beneath it
                        IEnumerable<Dictionary<string, object>> childNodes = ( ( List<Dictionary<string, object>> ) inner["Partitions"] ).Where( i => ( ( string ) i["ParentIdentifier"] ).Equals( childIdentifier ) );
                        // If the limit is not null, set the filled amount to the lesser of the sum of spots filled or the sum of the limits
                        inner.Add( "Filled", childNodes.Sum( i => i["Limit"] == null ? ( int ) i["Filled"] : Math.Min( ( int ) i["Filled"], ( int ) i["Limit"] ) ) );
                        //inner.Add( "Filled", childNodes.Sum( i => ( int ) i["Filled"] ) );
                    }
                    else
                    {
                        inner.Add( "Filled", 0 );
                    }
                }
                else
                {
                    // Base case for recursion
                    inner.Add( "Filled", subRequests != null ? subRequests.Where( sr => sr.ConnectionState != ConnectionState.Inactive ).Count() : 0 );
                }
                partitionList.Add( inner );
            }
            // Try to sort by order than by string value
            if ( partition.PartitionType != "Schedule" )
            {
                partitionList = partitionList.OrderBy( a => a["Entity"].GetType().GetProperty( "Order" ) != null ? a["Entity"].GetType().GetProperty( "Order" ).GetValue( a["Entity"], null ) : 0 ).ThenBy( a => a["Entity"].ToString() ).ToList();
            }
            return partitionList;
        }

        /// <summary>
        /// Gets the limit (i.e. maximum number of potential spaces this node can hold) for a tree node.
        /// </summary>
        /// <param name="nodeConcatGuidId">The new concat unique identifier.</param>
        /// <returns>The number of spaces a tree node can hold</returns>
        private int? GetLimitForTreeNode( string nodeConcatGuidId )
        {
            // Get the counts for nodes that made be relevant. If this is a leaf node then this will be a single value but for any other node this will include child nodes
            var potentialCounts = Counts.Where( kvp => kvp.Key.Contains( nodeConcatGuidId ) );

            // Assume the limit is 0 to prevent cases where a large number of 0s must be saved as attribute values, ! or blank should be treated as an unlimited value
            int? limit = 0;
            if ( potentialCounts.Count() > 0 )
            {
                limit = potentialCounts.Any( kvp => String.IsNullOrWhiteSpace( kvp.Value ) || kvp.Value.Equals( "!" ) ) ? null : ( int? ) potentialCounts.Select( kvp => kvp.Value.AsInteger() ).Sum();
            }

            return limit;
        }

        #endregion

        protected void ppSignupPage_SelectItem( object sender, EventArgs e )
        {
            if ( ppSignupPage.SelectedValueAsInt().HasValue )
            {
                Settings.SignupPageGuid = PageCache.Get( ppSignupPage.SelectedValueAsInt().Value ).Guid;
            }
            SaveViewState();
        }

        protected void gFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            foreach( var control in gFilter.Controls )
            {
                if ( control is RockListBox )
                {
                    var rockListBox = ( RockListBox ) control;
                    SetBlockUserPreference( rockListBox.ID, rockListBox.SelectedValues.JoinStrings( "," ) );
                }
            }
        }

        private void HideRows()
        {
            foreach( GridViewRow row in gCounts.Rows)
            {
                string rowId = gCounts.DataKeys[row.RowIndex].Value.ToString();
                foreach ( var control in gFilter.Controls )
                {
                    if ( control is RockListBox )
                    {
                        var rockListBox = ( RockListBox ) control;
                        // Apply the filter if we have one.
                        if ( rockListBox.SelectedValues.Count() > 0 )
                        {
                            bool visible = false;
                            foreach ( var filter in rockListBox.SelectedValues )
                            {

                                if ( rowId.Contains( filter ) )
                                {
                                    row.Visible = row.Visible && true;
                                    visible = true;
                                    break;
                                }
                            }
                            if (visible == false)
                            {
                                row.Visible = false;
                            }
                        }
                    }
                }

            }
        }

        protected void gFilter_ClearFilterClick( object sender, EventArgs e )
        {

            foreach ( var control in gFilter.Controls )
            {
                if ( control is RockListBox )
                {
                    var rockListBox = ( RockListBox ) control;
                    foreach ( ListItem item in rockListBox.Items ) {
                        item.Selected = false;
                    }
                    DeleteBlockUserPreference( rockListBox.ID );
                }
            }
        }
    }
}
