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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Plugins.cc_newspring.Blocks.WorkflowAlert
{
    /// <summary>
    /// Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user.
    /// </summary>
    [DisplayName( "My Workflow Alerts" )]
    [Category( "NewSpring" )]
    [Description( "Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user." )]
    [LinkedPage( "Listing Page", "Page used to view all workflows assigned to the current user.", false, "F3FA9EBE-A540-4106-90E5-2DFB2D72BBF0" )]

    [IntegerField( "Cache Duration", "Number of seconds to cache the content per person.", false, 0, "", 2 )]
    public partial class WorkflowAlert : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            activeWorkflowCount();
        }

        protected void activeWorkflowCount()
        {
            // Check for current person
            if ( CurrentPersonAliasId.HasValue )
            {
                int cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger();
                string cacheKey = "WorkflowAlertCount:PersonAliasId:" + CurrentPersonAliasId.ToString();
                int? activeIncompleteWorkflows = null;
                if ( cacheDuration > 0 )
                {
                    activeIncompleteWorkflows = this.GetCacheItem( cacheKey ) as int?;
                }

                if ( !activeIncompleteWorkflows.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        // Return the count of active workflows assigned to the current user (or user's group)
                        activeIncompleteWorkflows = GetActions( rockContext ).Count();
                        if ( cacheDuration > 0 )
                        {
                            this.AddCacheItem( cacheKey, activeIncompleteWorkflows, cacheDuration );
                        }
                    }
                }

                // set the default display
                var spanLiteral = "";
                if ( activeIncompleteWorkflows > 0 )
                {
                    // add the count of how many workflows need to be assigned/completed
                    spanLiteral = string.Format( "<span class='badge badge-danger'>{0}</span>", activeIncompleteWorkflows );
                }

                lbListingPage.Controls.Add( new LiteralControl( spanLiteral ) );
            }
        }

        /// <summary>
        /// Navigates to the Listing page.
        /// </summary>
        protected void lbListingPage_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "RoleFilter", "false" );
            queryParams.Add( "StatusFilter", "true" );

            NavigateToLinkedPage( "ListingPage", queryParams );
        }

        /// <summary>
        /// Returns a list of the actions assigned to the current person
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets a list of all the active workflow actions for the current person
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a list of all workflowtype category ids
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private List<int> GetCategories( RockContext rockContext )
        {
            int entityTypeId = EntityTypeCache.Read( typeof( WorkflowType ) ).Id;
            return GetCategoryIds( new List<int>(), new CategoryService( rockContext ).GetNavigationItems( entityTypeId, new List<Guid>(), true, CurrentPerson ) );
        }

        /// <summary>
        /// Recursively gets all category ids in the tree of categories passed
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="categories"></param>
        /// <returns></returns>
        private List<int> GetCategoryIds( List<int> ids, List<CategoryNavigationItem> categories )
        {
            foreach ( var categoryNavItem in categories )
            {
                ids.Add( categoryNavItem.Category.Id );
                GetCategoryIds( ids, categoryNavItem.ChildCategories );
            }

            return ids;
        }
    }
}