// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// List all of the workflows that current user is currently assigned to.
    /// </summary>
    [DisplayName( "My Workflows" )]
    [Category( "Core" )]
    [Description( "List all of the workflows that current user is currently assigned to." )]

    [LinkedPage( "Workflow Entry Page", "The page used to enter workflow form data." )]
    public partial class MyWorkflows : Rock.Web.UI.RockBlock
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindData();
            }
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
            var rockContext = new RockContext();

            var activities = GetActivities( rockContext );

            rptWorkflows.DataSource = activities
                .OrderByDescending( a => a.ActivatedDateTime )
                .Select( a => new
                {
                    WorkflowTypeId = a.Workflow.WorkflowTypeId,
                    WorkflowId = a.WorkflowId,
                    WorkflowType = a.Workflow.WorkflowType.Name,
                    Workflow = a.Workflow.Name,
                    Activity = a.ActivityType.Name
                } )
                .ToList();
            rptWorkflows.DataBind();
        }


        private List<WorkflowActivity> GetActivities( RockContext rockContext )
        {
            var authorizedWorkflowTypes = GetAuthorizedWorkflowTypes( rockContext );

            var myActivities = new List<WorkflowActivity>();

            var workflows = new WorkflowService( rockContext ).Queryable( "Activities" )
                .Where( w =>
                    authorizedWorkflowTypes.Keys.Contains( w.WorkflowTypeId ) &&
                    w.ActivatedDateTime.HasValue &&
                    !w.CompletedDateTime.HasValue );

            foreach ( var workflow in workflows )
            {
                var activities = workflow.Activities
                    .Where( a =>
                        a.IsActive &&
                        authorizedWorkflowTypes[workflow.WorkflowTypeId].ContainsKey( a.ActivityTypeId ) );

                foreach ( var activity in activities.OrderBy( a => a.ActivityType.Order ) )
                {
                    if ( activity.IsAssigned( CurrentPerson, false ) )
                    {
                        if ( activity.ActiveActions.Any( a => authorizedWorkflowTypes[workflow.WorkflowTypeId][activity.ActivityTypeId].Contains( a.ActionTypeId ) ) )
                        {
                            myActivities.Add( activity );
                            break;
                        }
                    }
                }
            }

            return myActivities;
        }

        private Dictionary<int, Dictionary<int, List<int>>> GetAuthorizedWorkflowTypes( RockContext rockContext )
        {
            var service = new WorkflowTypeService( rockContext );

            var AuthorizedWorkflowTypes = Session[string.Format( "MyWorkflows_Items_{0}", CurrentPersonId )] as Dictionary<int, Dictionary<int, List<int>>>;
            if ( AuthorizedWorkflowTypes != null )
            {
                var whenCached = Session[string.Format( "MyWorkflows_Items_Cached_{0}", CurrentPersonId )] as DateTime?;
                var workflowTypesLastModified = service.Queryable().Select( w => w.ModifiedDateTime ).DefaultIfEmpty().Max() ?? DateTime.MinValue;
                if ( !whenCached.HasValue || whenCached.Value.CompareTo( workflowTypesLastModified ) < 0 )
                {
                    AuthorizedWorkflowTypes = null;
                }
            }

            if ( AuthorizedWorkflowTypes == null )
            {
                AuthorizedWorkflowTypes = new Dictionary<int, Dictionary<int, List<int>>>();

                // Iterate the workflow types
                foreach ( var workflowType in service.Queryable( "ActivityTypes" ) )
                {
                    // If user is authorized to view workflow type
                    if ( workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        // Iterate the activity types
                        foreach ( var activityType in workflowType.ActivityTypes )
                        {
                            // If user is authorized to view workflow type
                            if ( activityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                            {
                                // Iterate the actions
                                foreach ( var actionType in activityType.ActionTypes )
                                {
                                    // If action is a form
                                    if ( actionType.WorkflowFormId.HasValue )
                                    {
                                        // Save the workflow type
                                        if ( !AuthorizedWorkflowTypes.ContainsKey( workflowType.Id ) )
                                        {
                                            AuthorizedWorkflowTypes.Add( workflowType.Id, new Dictionary<int, List<int>>() );
                                        }
                                        var AuthorizedActivityTypes = AuthorizedWorkflowTypes[workflowType.Id];

                                        // Save the activity type
                                        if ( !AuthorizedActivityTypes.ContainsKey( activityType.Id ) )
                                        {
                                            AuthorizedActivityTypes.Add( activityType.Id, new List<int>() );
                                        }
                                        var AuthorizedActionTypes = AuthorizedActivityTypes[activityType.Id];

                                        // Save the action type
                                        AuthorizedActionTypes.Add( actionType.Id );


                                    }
                                }
                            }
                        }
                    }
                }

                Session[string.Format( "MyWorkflows_Items_{0}", CurrentPersonId )] = AuthorizedWorkflowTypes;
                Session[string.Format( "MyWorkflows_Items_Cached_{0}", CurrentPersonId )] = DateTime.Now;
            }

            return AuthorizedWorkflowTypes;

        }

        protected string FormatUrl( int workflowTypeId, int workflowId )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "WorkflowTypeId", workflowTypeId.ToString() );
            qryParams.Add( "WorkflowId", workflowId.ToString() );
            return LinkedPageUrl( "WorkflowEntryPage", qryParams );
        }

        #endregion
    }
}