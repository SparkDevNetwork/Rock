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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.WorkFlow
{
    /// <summary>
    /// Block to display active workflow activities assigned to the current user that have a form entry action.  The display format is controlled by a lava template.
    /// </summary>
    [DisplayName( "My Workflows Lava" )]
    [Category( "WorkFlow" )]
    [Description( "Block to display active workflow activities assigned to the current user that have a form entry action.  The display format is controlled by a lava template." )]

    [CustomRadioListField("Role", "Display the active workflows that the current user Initiated, or is currently Assigned To.", "0^Assigned To,1^Initiated", true, "0", "", 0 )]
    [CategoryField( "Categories", "Optional categories to limit display to.", true, "Rock.Model.WorkflowType", "", "", false, "", "", 1 )]
    [BooleanField( "Include Child Categories", "Should descendent categories of the selected Categories be included?", true, "", 2 )]
    [CodeEditorField( "Contents", @"The Lava template to use for displaying activities assigned to current user.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, @"{% include '/Assets/Lava/MyWorkflowsSortable.lava' %}", "", 3 )]
    [TextField( "Set Panel Title", "The title to display in the panel header. Leave empty to have the block name.", required: false, order: 4 )]
    [TextField( "Set Panel Icon", "The icon to display in the panel header.", required: false, order: 5 )]
    [Rock.SystemGuid.BlockTypeGuid( "4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1" )]
    public partial class MyWorkflowsLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindData();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindData();
        }

        #endregion

        #region Methods

        private void BindData()
        {
            try
            {
                string role = GetAttributeValue( "Role" );
                if ( string.IsNullOrWhiteSpace( role ) )
                {
                    role = "0";
                }

                string contents = GetAttributeValue( "Contents" );
                string panelTitle = GetAttributeValue( "SetPanelTitle" );
                string panelIcon = GetAttributeValue( "SetPanelIcon" );

                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );
                contents = contents.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                using ( var rockContext = new RockContext() )
                {
                    List<WorkflowAction> actions = null;
                    if ( role == "1" )
                    {
                        actions = GetWorkflows( rockContext );
                    }
                    else
                    {
                        actions = GetActions( rockContext );
                    }

                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Role", role );
                    mergeFields.Add( "Actions", actions.OrderByDescending( a => a.CreatedDateTime ) );
                    mergeFields.Add( "PanelTitle", panelTitle );
                    mergeFields.Add( "PanelIcon", panelIcon );

                    lContents.Text = contents.ResolveMergeFields( mergeFields );
                }

            }
            catch ( Exception ex )
            {
                LogException( ex );
                lContents.Text = "error getting workflows";
            }
        }

        private List<WorkflowAction> GetWorkflows( RockContext rockContext )
        {
            var actions = new List<WorkflowAction>();

            if ( CurrentPerson != null )
            {

                var categoryIds = GetCategories( rockContext );

                var qry = new WorkflowService( rockContext ).Queryable( "WorkflowType" )
                    .Where( w =>
                        w.ActivatedDateTime.HasValue &&
                        !w.CompletedDateTime.HasValue &&
                        w.InitiatorPersonAlias.PersonId == CurrentPerson.Id );

                if ( categoryIds.Any() )
                {
                    qry = qry
                        .Where( w =>
                            w.WorkflowType.CategoryId.HasValue &&
                            categoryIds.Contains( w.WorkflowType.CategoryId.Value ) );
                }

                foreach ( var workflow in qry.OrderBy( w => w.ActivatedDateTime ) )
                {
                    var activity = new WorkflowActivity();
                    activity.Workflow = workflow;

                    var action = new WorkflowAction();
                    action.Activity = activity;

                    actions.Add( action );
                }
            }

            return actions;
        }

        private List<WorkflowAction> GetActions( RockContext rockContext )
        {
            var formActions = new List<WorkflowAction>();

            if ( CurrentPerson != null )
            {

                // Get all of the active form actions that user is assigned to and authorized to view
                formActions = GetActiveForms( rockContext );

                // If a category filter was specified, filter list by selected categories
                var categoryIds = GetCategories( rockContext );
                if ( categoryIds.Any() )
                {
                    formActions = formActions
                        .Where( a =>
                            a.ActionType.ActivityType.WorkflowType.CategoryId.HasValue &&
                            categoryIds.Contains( a.ActionType.ActivityType.WorkflowType.CategoryId.Value ) )
                        .ToList();
                }
            }

            return formActions;
        }

        private List<WorkflowAction> GetActiveForms( RockContext rockContext )
        {
            var formActions = RockPage.GetSharedItem( "ActiveForms" ) as List<WorkflowAction>;
            if ( formActions == null )
            {
                formActions = new WorkflowActionService( rockContext ).GetActiveForms( CurrentPerson );
                RockPage.SaveSharedItem( "ActiveForms", formActions );
            }

            // find first form for each activity
            var firstForms = new List<WorkflowAction>();
            foreach( var activityId in formActions.Select( a => a.ActivityId ).Distinct().ToList() )
            {
                firstForms.Add( formActions.First( a => a.ActivityId == activityId ) );
            }

            return firstForms;
        }

        private List<int> GetCategories( RockContext rockContext )
        {
            int entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.WorkflowType ) ).Id;

            var selectedCategories = new List<Guid>();
            GetAttributeValue( "Categories" ).SplitDelimitedValues().ToList().ForEach( c => selectedCategories.Add( c.AsGuid() ) );

            bool includeChildCategories = GetAttributeValue( "IncludeChildCategories" ).AsBoolean();

            return GetCategoryIds( new List<int>(), new CategoryService( rockContext ).GetNavigationItems( entityTypeId, selectedCategories, includeChildCategories, CurrentPerson ) );
        }

        private List<int> GetCategoryIds( List<int> ids, List<CategoryNavigationItem> categories )
        {
            foreach ( var categoryNavItem in categories )
            {
                ids.Add( categoryNavItem.Category.Id );
                GetCategoryIds( ids, categoryNavItem.ChildCategories );
            }

            return ids;
        }

        #endregion
    }
}