// <copyright>
// Copyright by Central Christian Church
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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Plugins.com_centralaz.Utility
{
    /// <summary>
    /// Allows you to pick a person, group, workflow instance, or registration entity and test your lava.
    /// </summary>
    [DisplayName( "Lava Tester" )]
    [Category( "com_centralaz > Utility" )]
    [Description( "Allows you to pick a person, group, workflow instance, or registration entity and test your lava." )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled.", false, order: 0 )]
    public partial class LavaTester : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        Dictionary<string, object> mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
        private readonly string _USER_PREF_KEY = "MyLavaTestText";
        private readonly string _USER_PREF_PERSON = "MyLavaTest:Person";
        private readonly string _USER_PREF_GROUP = "MyLavaTest:Group";
        private readonly string _USER_PREF_REGISTRATION_INSTANCE = "MyLavaTest:RegistrationInstance";
        private readonly string _USER_PREF_REGISTRATION = "MyLavaTest:Registration";
        private readonly string _USER_PREF_WORKFLOWTYPE = "MyLavaTest:WorkflowTYPE";
        private readonly string _USER_PREF_WORKFLOW = "MyLavaTest:Workflow";
        private readonly string _USER_PREF_WORKFLOW_ACTIVITY = "MyLavaTest:WorkflowActivity";
        private readonly string _USER_PREF_EDITORHEIGHT = "MyLavaTestEditorHeight";
        private readonly string _EMPTY_SAVED_SLOT = "empty saved slot";
        private readonly string _TEXT_MUTED = "text-muted";

        private readonly int MAX_SAVE_SLOTS = 25;
        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            
            if ( string.IsNullOrEmpty( ceLava.Text ) )
            {
                var text = GetUserPreference( _USER_PREF_KEY );
                if ( ! string.IsNullOrEmpty( GetUserPreference( _USER_PREF_EDITORHEIGHT ) ) )
                {
                    ceLava.EditorHeight = GetUserPreference( _USER_PREF_EDITORHEIGHT );
                    nbHeight.Text = ceLava.EditorHeight;
                }
                ceLava.Text = text;

                // Set up the Merge Fields
                ceLava.MergeFields.Clear();

                ceLava.MergeFields.Add( "Person^Rock.Model.Person|Selected \"Person\"" );
                ceLava.MergeFields.Add( "Group^Rock.Model.Group|Selected \"Group\"" );
                ceLava.MergeFields.Add( "Workflow^Rock.Model.Workflow|Selected \"Workflow\"" );
                ceLava.MergeFields.Add( "Activity^Rock.Model.WorkflowActivity|Selected \"Activity\"" );
                ceLava.MergeFields.Add( "Registration^Rock.Model.Registration|Selected \"Registration\"" );

                ceLava.MergeFields.Add( "GlobalAttribute" );
                ceLava.MergeFields.Add( "CurrentPerson^Rock.Model.Person|Current Person" );
                ceLava.MergeFields.Add( "Campuses" );
                ceLava.MergeFields.Add( "RockVersion" );
                ceLava.MergeFields.Add( "PageParameter" );
                ceLava.MergeFields.Add( "Date" );
                ceLava.MergeFields.Add( "Time" );
                ceLava.MergeFields.Add( "DayOfWeek" );

                // Only show instructions the first time.
                if ( !string.IsNullOrEmpty( text ) )
                {
                    nbInstructions.Visible = false;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            litDebug.Text = "";

            if ( ! Page.IsPostBack )
            {
                // Load the storage slots
                BindData();

                if ( cbEnableDebug.Checked )
                {
                    litDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Saves the user's lava into a user preference storage slot
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var item = ddlSaveSlot.SelectedItem;
            if ( string.IsNullOrWhiteSpace( ceLava.Text ) )
            {
                item.Text = _EMPTY_SAVED_SLOT;
                AddCssClass( item, _TEXT_MUTED );
                SetUserPreference( string.Format( "{0}:{1}", _USER_PREF_KEY, item.Value ), string.Empty );
            }
            else
            {
                string title = ceLava.Text.TrimStart();

                SetSavedSlotItemName( item, title );

                RemoveCssClass( item, _TEXT_MUTED );
                SetUserPreference( string.Format( "{0}:{1}", _USER_PREF_KEY, item.Value ), ceLava.Text );
            }
        }

        private static void SetSavedSlotItemName( ListItem item, string title )
        {
            string commentPattern = @"<!--(.*)-->";
            Regex r = new Regex( commentPattern, RegexOptions.IgnoreCase );
            Match m = r.Match( title );
            if ( m.Success )
            {
                item.Text = m.Groups[1].Value;
            }
            else
            {
                item.Text = title.Truncate( 150 );
            }
        }

        /// <summary>
        ///  Loads the user's lava from the selected storage slot
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLoadLava_Click( object sender, EventArgs e )
        {
            var item = ddlSaveSlot.SelectedItem;
            ceLava.Text = GetUserPreference( string.Format( "{0}:{1}", _USER_PREF_KEY, item.Value ) );
            litOutput.Text = string.Empty;
        }

        /// <summary>
        /// Handles the TextChanged event of the nbHeight control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void nbHeight_TextChanged( object sender, EventArgs e )
        {
            if ( nbHeight.Text.AsInteger() < 150 )
            {
                nbHeight.Text = "150";
            }
            ceLava.EditorHeight = nbHeight.Text;
            SetUserPreference( _USER_PREF_EDITORHEIGHT, nbHeight.Text, true );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the storage slots from the users saved data.
        /// </summary>
        private void BindData()
        {
            var savedLava = string.Empty;
            for ( int i = 0; i < MAX_SAVE_SLOTS; i++ )
            {
                savedLava = GetUserPreference( string.Format( "{0}:{1}", _USER_PREF_KEY, i ) );

                if ( string.IsNullOrWhiteSpace( savedLava ) )
                {
                    AddCssClass( ddlSaveSlot.Items[i], _TEXT_MUTED );
                    savedLava = string.Format( "{0} {1}", _EMPTY_SAVED_SLOT, i+1 );
                    ddlSaveSlot.Items[i].Text = savedLava;
                }
                else
                {
                    SetSavedSlotItemName( ddlSaveSlot.Items[i], savedLava.TrimStart() );
                }

            }

            using ( var rockContext = new RockContext() )
            {
                ppPerson.PersonId = GetUserPreference( _USER_PREF_PERSON ).AsIntegerOrNull();
                if ( ppPerson.PersonId != null )
                {
                    var person = new PersonService( rockContext ).Get( ppPerson.PersonId ?? -1 );
                    ppPerson.SetValue( person );
                }

                var groupId = GetUserPreference( _USER_PREF_GROUP ).AsIntegerOrNull();
                if ( groupId != null )
                {
                    gpGroups.SetValue( groupId );
                }

                var workflowTypeId = GetUserPreference( _USER_PREF_WORKFLOWTYPE ).AsIntegerOrNull();
                if ( workflowTypeId != null )
                {
                    wfpWorkflowType.SetValue( workflowTypeId );
                    BindWorkflowsUsingWorkflowType( workflowTypeId, setUserPreference: false );
                }

                var workflowId = GetUserPreference( _USER_PREF_WORKFLOW ).AsIntegerOrNull();
                if ( workflowId != null )
                {
                    ddlWorkflows.SetValue( workflowId );
                    BindWorkflowActivitiesUsingWorkflowInstance( workflowId, setUserPreference: false );
                }

                var registrationInstanceId = GetUserPreference( _USER_PREF_REGISTRATION_INSTANCE ).AsIntegerOrNull();
                if ( registrationInstanceId != null )
                {
                    BindRegistrationInstances( rockContext, registrationInstanceId );
                }
                else
                {
                    BindRegistrationInstances( rockContext, -1 );
                }
            }
        }

        /// <summary>
        /// Adds a CSS class name to an ListItem.
        /// </summary>
        /// <param name="htmlControl">The html control.</param>
        /// <param name="className">Name of the class.</param>
        public static void AddCssClass( ListItem item, string className )
        {
            string match = @"\b" + className + "\b";
            string css = item.Attributes["class"] ?? string.Empty;

            if ( !Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
                item.Attributes["class"] = Regex.Replace( css + " " + className, @"^\s+", "", RegexOptions.IgnoreCase );
        }

        /// <summary>
        /// Removes a CSS class name from a ListItem.
        /// </summary>
        /// <param name="htmlControl">The html control.</param>
        /// <param name="className">Name of the class.</param>
        public static void RemoveCssClass( ListItem item, string className )
        {
            string match = @"\s*\b" + className + @"\b";
            string css = item.Attributes["class"] ?? string.Empty;

            if ( Regex.IsMatch( css, match, RegexOptions.IgnoreCase ) )
                item.Attributes["class"] = Regex.Replace( css, match, "", RegexOptions.IgnoreCase );
        }

        #endregion

        protected void bbTest_Click( object sender, EventArgs e )
        {
            nbInstructions.Visible = false;

            try
            {
                // Save lava test string for future use.
                SetUserPreference( _USER_PREF_KEY, ceLava.Text );

                using ( var rockContext = new RockContext() )
                {
                    PersonService personService = new PersonService( rockContext );
                    Person person;
                    if ( ppPerson.PersonId.HasValue )
                    {
                        person = personService.Get( ppPerson.PersonId ?? -1, false );
                    }
                    else
                    {
                        person = CurrentPerson;
                    }

                    // Get Lava
                    mergeFields.Add( "Person", person );

                    if ( gpGroups != null && gpGroups.SelectedValueAsInt().HasValue )
                    {
                        GroupService groupService = new GroupService( rockContext );
                        mergeFields.Add( "Group", groupService.Get( gpGroups.SelectedValueAsInt() ?? -1 ) );
                    }

                    if ( ddlWorkflows != null && ddlWorkflows.Items.Count > 0 && ddlWorkflows.SelectedValueAsInt().HasValue )
                    {
                        WorkflowService workflowService = new WorkflowService( rockContext );
                        if ( mergeFields.ContainsKey( "Workflow" ) )
                        {
                            mergeFields.Remove( "Workflow" );
                        }

                        if ( ddlWorkflows.SelectedValueAsInt() != null )
                        {
                            mergeFields.Add( "Workflow", workflowService.Get( ddlWorkflows.SelectedValueAsInt() ?? -1 ) );
                        }
                    }

                    if ( ddlWorkflowActivities != null && ddlWorkflowActivities.Items.Count > 0 && ddlWorkflowActivities.SelectedValueAsInt().HasValue )
                    {
                        var workflowActivityService = new WorkflowActivityService( rockContext );
                        if ( mergeFields.ContainsKey( "Activity" ) )
                        {
                            mergeFields.Remove( "Activity" );
                        }

                        if ( ddlWorkflowActivities.SelectedValueAsInt() != null )
                        {
                            mergeFields.Add( "Activity", workflowActivityService.Get( ddlWorkflowActivities.SelectedValueAsInt() ?? -1 ) );
                        }
                    }

                    if ( ddlRegistrations != null && ddlRegistrations.Items.Count > 0 && ddlRegistrations.SelectedValueAsInt().HasValue )
                    {
                        if ( mergeFields.ContainsKey( "RegistrationInstance" ) )
                        {
                            mergeFields.Remove( "RegistrationInstance" );
                        }

                        if ( mergeFields.ContainsKey( "Registration" ) )
                        {
                            mergeFields.Remove( "RegistrationInstance" );
                        }

                        RegistrationService registrationService = new RegistrationService( rockContext );

                        if ( ddlRegistrations.SelectedValueAsInt() != null )
                        {
                            var registration = registrationService.Get( ddlRegistrations.SelectedValueAsInt() ?? -1 );
                            if ( registration != null )
                            {
                                mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                                mergeFields.Add( "Registration", registration );
                            }
                        }
                    }

                    ResolveLava();
                }
            }
            catch( Exception ex )
            {
                //LogException( ex );
                litDebug.Text = "<pre>" + ex.StackTrace + "</pre>";
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the wfpWorkflowType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void wfpWorkflowType_SelectItem( object sender, EventArgs e )
        {
            int? workflowTypeId = wfpWorkflowType.SelectedValueAsInt();
            if ( workflowTypeId.HasValue )
            {
                SetUserPreference( _USER_PREF_WORKFLOWTYPE, workflowTypeId.Value.ToStringSafe() );
                BindWorkflowsUsingWorkflowType( workflowTypeId.Value, setUserPreference: true );
                BindWorkflowActivitiesUsingWorkflowInstance( ddlWorkflows.SelectedValueAsInt() ?? -1, setUserPreference: true );
            }
            else
            {
                ddlWorkflows.SetValue( "-1" );
                ddlWorkflows.SelectedIndex = -1;
                ddlWorkflows.Items.Clear();
                ddlWorkflows.Visible = false;

                ddlWorkflowActivities.SetValue( "-1" );
                ddlWorkflowActivities.SelectedIndex = -1;
                ddlWorkflowActivities.Items.Clear();
                ddlWorkflowActivities.Visible = false;

                SetUserPreference( _USER_PREF_WORKFLOWTYPE, string.Empty );
                SetUserPreference( _USER_PREF_WORKFLOW, string.Empty );
                SetUserPreference( _USER_PREF_WORKFLOW_ACTIVITY, string.Empty );
            }

            litOutput.Text = string.Empty;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlWorkflows_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? workflowId = ddlWorkflows.SelectedValueAsInt();
            if ( workflowId.HasValue )
            {
                RockContext rockContext = new RockContext();
                WorkflowService workflowService = new WorkflowService( rockContext );
                mergeFields.Add( "Workflow", workflowService.Get( ddlWorkflows.SelectedValueAsInt() ?? -1 ) );

                SetUserPreference( _USER_PREF_WORKFLOW, ddlWorkflows.SelectedValue );
                BindWorkflowActivitiesUsingWorkflowInstance( workflowId, setUserPreference: true );
            }
            else
            {
                ddlWorkflowActivities.SetValue( "-1" );
                ddlWorkflowActivities.SelectedIndex = -1;
                ddlWorkflowActivities.Visible = false;
                ddlWorkflowActivities.Items.Clear();
                SetUserPreference( _USER_PREF_WORKFLOWTYPE, string.Empty );
                SetUserPreference( _USER_PREF_WORKFLOW, string.Empty );
                SetUserPreference( _USER_PREF_WORKFLOW_ACTIVITY, string.Empty );
            }

            litOutput.Text = string.Empty;
        }
        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlWorkflowActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlWorkflowActivities_SelectedIndexChanged( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            WorkflowActivityService workflowActivityService = new WorkflowActivityService( rockContext );
            mergeFields.Remove( "Activity" );
            mergeFields.Add( "Activity", workflowActivityService.Get( ddlWorkflowActivities.SelectedValueAsInt() ?? -1 ) );
            SetUserPreference( _USER_PREF_WORKFLOW_ACTIVITY, ddlWorkflowActivities.SelectedValue );

            litOutput.Text = string.Empty;
        }

        /// <summary>
        /// Binds the type of the workflows using workflow type.
        /// </summary>
        private void BindWorkflowsUsingWorkflowType(int? workflowTypeId, bool setUserPreference )
        {
            using ( var rockContext = new RockContext() )
            {
                WorkflowService workflowService = new WorkflowService( rockContext );

                var workflows = workflowService.Queryable().AsNoTracking()
                    .Where( w => w.WorkflowTypeId == workflowTypeId.Value && ( w.CompletedDateTime == null || cbIncludeInactive.Checked ) ).ToList();

                var list = workflows.Select( a => new
                {
                    Id = a.Id,
                    Name = string.Format( "{0} {1}", a.Name, a.CompletedDateTime != null ? a.CompletedDateTime.Value.ToString( "(MM/dd/yy hh:mm tt)" ) : "" ),
                    CompletedDateTime = a.CompletedDateTime,
                    IsActive = a.IsActive
                } );
                ddlWorkflows.DataSource = list;
                ddlWorkflows.DataBind();

                // Style each inactive/complete workflow instance using text-muted
                foreach ( var item in ddlWorkflows.Items )
                {
                    var listItem = ( ListItem ) item;
                    var x = listItem.Value.AsInteger();
                    if ( list.Any( a => x == a.Id && !a.IsActive ) )
                    {
                        AddCssClass( listItem, _TEXT_MUTED );
                    }
                }

                ddlWorkflows.Visible = true;
                ddlWorkflowActivities.Visible = true;

                if ( workflows.Count > 0 )
                {
                    if ( setUserPreference )
                    {
                        SetUserPreference( _USER_PREF_WORKFLOW, workflows[0].Id.ToStringSafe() );
                        mergeFields.Add( "Workflow", workflows[0] );
                    }
                    else
                    {
                        var workflowId = GetUserPreference( _USER_PREF_WORKFLOW ).AsIntegerOrNull();
                        if ( workflowId != null )
                        {
                            ddlWorkflows.SetValue( workflowId );
                            mergeFields.Add( "Workflow", workflows.Where( w =>w.Id == workflowId ).FirstOrDefault() );
                        }
                    }

                    litOutput.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Binds the WF Activities of the workflows using workflow (instance).
        /// </summary>
        private void BindWorkflowActivitiesUsingWorkflowInstance( int? workflowId, bool setUserPreference )
        {
            using ( var rockContext = new RockContext() )
            {
                var workflowActivityService = new WorkflowActivityService( rockContext );

                var list = workflowActivityService.Queryable().AsNoTracking().Where( a => a.WorkflowId == workflowId.Value ).ToList();

                ddlWorkflowActivities.DataSource = list.Select( a => new
                {
                    Id = a.Id,
                    ActivityTypeName = a.ActivityType.Name,
                    Name = string.Format( "{0} {1}", a.ActivityType.Name, a.CompletedDateTime != null ? a.CompletedDateTime.Value.ToString("(MM/dd/yy hh:mm tt)") : "" ),
                    ActivityTypeId = a.ActivityTypeId,
                    CompletedDateTime = a.CompletedDateTime,
                    IsActive = a.IsActive
                } );

                ddlWorkflowActivities.DataBind();

                Regex re = new Regex(@"\(\d\d/\d\d/\d\d \d\d:\d\d");
                foreach ( ListItem item in ddlWorkflowActivities.Items )
                {
                    if ( re.IsMatch( item.Text ) )
                    {
                        item.Attributes.CssStyle.Add( "color", "#aaa" );
                    }
                }

                ddlWorkflowActivities.Visible = true;

                if ( list.Count > 0 )
                {
                    if ( setUserPreference )
                    {
                        SetUserPreference( _USER_PREF_WORKFLOW_ACTIVITY, list[0].Id.ToStringSafe() );
                        mergeFields.Remove( "Activity" );
                        mergeFields.Add( "Activity", list[0] );
                    }
                    else
                    {
                        var activityId = GetUserPreference( _USER_PREF_WORKFLOW_ACTIVITY ).AsIntegerOrNull();
                        if ( activityId != null )
                        {
                            ddlWorkflowActivities.SetValue( activityId );
                            mergeFields.Remove( "Activity" );
                            mergeFields.Add( "Activity", list.Where( a => a.Id == activityId ).FirstOrDefault() );
                        }
                    }

                    litOutput.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRegistrationInstances control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRegistrationInstances_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? registrationInstanceId = ddlRegistrationInstances.SelectedValueAsInt();
            if ( registrationInstanceId.HasValue && registrationInstanceId.Value != -1 )
            {
                SetUserPreference( _USER_PREF_REGISTRATION_INSTANCE, registrationInstanceId.Value.ToStringSafe() );
                BindRegistrationsUsingRegistrationInstances( registrationInstanceId.Value );
            }
            else
            {
                ddlRegistrations.SetValue( "-1" );
                ddlRegistrations.SelectedIndex = -1;
                ddlRegistrations.Visible = false;
                ddlRegistrations.Items.Clear();
                SetUserPreference( _USER_PREF_REGISTRATION_INSTANCE, string.Empty );
                SetUserPreference( _USER_PREF_REGISTRATION, string.Empty );
                mergeFields.Remove( "RegistrationInstance" );
                mergeFields.Remove( "Registration" );
            }

            litOutput.Text = string.Empty;
        }

        /// <summary>
        /// Binds the Registrations (people who registered) using the RegistrationInstances.
        /// </summary>
        /// <param name="rockContext">The RockContext.</param>
        /// <param name="registrationInstanceId">The id of a RegistrationInstance.</param>
        /// <param name="setUserPreference">Set to true if you want to remember the item in the users preferences</param>
        private void BindRegistrationInstances( RockContext rockContext, int? registrationInstanceId )
        {
            RegistrationInstanceService registrationInstanceService = new RegistrationInstanceService( rockContext );

            var registrationInstances = registrationInstanceService.Queryable().AsNoTracking().Where( r => r.IsActive == true ).ToList();
            ddlRegistrationInstances.DataSource = registrationInstances;
            RegistrationInstance emptyRegistrationInstance = new RegistrationInstance { Id = -1, Name = "" };
            registrationInstances.Insert( 0, emptyRegistrationInstance );
            ddlRegistrationInstances.DataBind();

            // Select the given registrationInstanceId if it still exists in the list
            if ( ddlRegistrationInstances.Items.FindByValue( registrationInstanceId.ToStringSafe() ) != null )
            {
                ddlRegistrationInstances.SelectedValue = registrationInstanceId.ToStringSafe();
            }

            if ( registrationInstances.Count >= 1 )
            {
                BindRegistrationsUsingRegistrationInstances( registrationInstanceId );
            }
        }

        /// <summary>
        /// Binds the Registrations (people who registered) using the RegistrationInstances.
        /// </summary>
        private void BindRegistrationsUsingRegistrationInstances( int? registrationInstanceId )
        {
            if ( registrationInstanceId == -1 )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                RegistrationService registrationService = new RegistrationService( rockContext );

                var registrations = registrationService.Queryable().AsNoTracking()
                    .Where( r => r.RegistrationInstanceId == registrationInstanceId.Value )
                    .ToList();

                // convert to something we can modify to show FirstName as first and last
                var registrationsList = registrations.OrderBy( r => r.FirstName ).ThenBy( r => r.LastName )
                    .Select( r => new { Id = r.Id, FirstName = string.Format( "{0} {1}", r.FirstName, r.LastName ) } )
                    .ToList();
                registrationsList.Insert( 0, new { Id = -1, FirstName = "" } );
                
                ddlRegistrations.DataSource = registrationsList;
                ddlRegistrations.Visible = true;
                ddlRegistrations.DataBind();

                if ( registrations.Count > 0 )
                {
                    var registrationId = GetUserPreference( _USER_PREF_REGISTRATION ).AsIntegerOrNull();
                    if ( registrationId != null )
                    {
                        ddlRegistrations.SetValue( registrationId );
                        var registration = registrations.Where( r => r.Id == registrationId ).FirstOrDefault();
                        if ( registration != null )
                        {
                            mergeFields.AddOrReplace( "RegistrationInstance", registration.RegistrationInstance );
                            mergeFields.AddOrReplace( "Registration", registration );
                        }
                    }

                    //ResolveLava();
                    litOutput.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRegistrations_SelectedIndexChanged( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            RegistrationService registrationService = new RegistrationService( rockContext );
            var registration = registrationService.Get( ddlRegistrations.SelectedValueAsInt() ?? -1 );
            if ( registration != null )
            {
                mergeFields.AddOrReplace( "RegistrationInstance", registration.RegistrationInstance );
                mergeFields.Add( "Registration", registration );
            }
            else
            {
                mergeFields.Remove( "Registration" );
            }

            SetUserPreference( _USER_PREF_REGISTRATION, ddlRegistrations.SelectedValue );

            //ResolveLava();
            litOutput.Text = string.Empty;
        }

        /// <summary>
        /// Resolves the lava.
        /// </summary>
        protected void ResolveLava()
        {
            string lava = ceLava.Text;
            litOutput.Text = lava.ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
            if ( cbEnableDebug.Checked )
            {
                litDebug.Text = mergeFields.lavaDebugInfo();
                h3DebugTitle.Visible = true;
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppPerson control and saves the user preference.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            SetUserPreference( _USER_PREF_PERSON, ppPerson.PersonId.ToStringSafe() );
            litOutput.Text = string.Empty;
        }

        /// <summary>
        /// Handles the SelectItem event of the gpGroups control and saves the user preference.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpGroups_SelectItem( object sender, EventArgs e )
        {
            SetUserPreference( _USER_PREF_GROUP, gpGroups.SelectedValue );
            litOutput.Text = string.Empty;
        }


        /// <summary>
        /// Handles the CheckedChanged event of the cbIncludeInactive control and rebinds the Workflow instances
        /// as if the Workflow Type just changed in order to include/exclude inactive workflow instances.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbIncludeInactive_CheckedChanged( object sender, EventArgs e )
        {
            wfpWorkflowType_SelectItem( sender, e );
        }
    }
}