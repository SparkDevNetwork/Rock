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

namespace RockWeb.Blocks.WorkFlow
{
    /// <summary>
    /// Block to display active workflow activities assigned to the current user that have a form entry action.  The display format is controlled by a liquid template.
    /// </summary>
    [DisplayName( "My Workflows Liquid" )]
    [Category( "WorkFlow" )]
    [Description( "Block to display active workflow activities assigned to the current user that have a form entry action.  The display format is controlled by a liquid template." )]

    [CustomRadioListField("Role", "Display the active workflows that the current user Initiated, or is currently Assigned To.", "0^Assigned To,1^Initiated", true, "0", "", 0 )]
    [CategoryField( "Categories", "Optional categories to limit display to.", true, "Rock.Model.WorkflowType", "", "", false, "", "", 1 )]
    [BooleanField( "Include Child Categories", "Should descendent categories of the selected Categories be included?", true, "", 2 )]
    [CodeEditorField( "Contents", @"The Liquid template to use for displaying activities assigned to current user. 
The following object model is available to the liquid template (Note: and workflow or activity attributes will also be available 
as fields on the workflow or activity)...
<pre>
{
  ""Role"": 0,
  ""Actions"": [
    {
      ""ActivityId"": 0,
      ""ActionTypeId"": 0,
      ""LastProcessedDateTime"": null,
      ""CompletedDateTime"": null,
      ""FormAction"": null,
      ""IsActive"": true,
      ""CreatedDateTime"": null,
      ""ModifiedDateTime"": null,
      ""CreatedByPersonAliasId"": null,
      ""ModifiedByPersonAliasId"": null,
      ""Id"": 0,
      ""Guid"": ""68673d26-fbeb-464c-882d-992c5bd60e19"",
      ""ForeignId"": null,
      ""UrlEncodedKey"": ""EAAAABul!2fMqcPFoFJEAauTEsj0aH1yCR7B3RI6v9wDzBsy4OPcDXF28gnUw90KVGKZCtX2dea449c9jYMO!2b71NLpdBk!3d"",
      ""Activity"": {
        ""WorkflowId"": 0,
        ""ActivityTypeId"": 0,
        ""AssignedPersonAliasId"": null,
        ""AssignedGroupId"": null,
        ""ActivatedDateTime"": null,
        ""LastProcessedDateTime"": null,
        ""CompletedDateTime"": null,
        ""IsActive"": false,
        ""Actions"": [],
        ""CreatedDateTime"": null,
        ""ModifiedDateTime"": null,
        ""CreatedByPersonAliasId"": null,
        ""ModifiedByPersonAliasId"": null,
        ""Id"": 0,
        ""Guid"": ""75bb018e-cdd4-467f-bfe8-dfeea4588d7b"",
        ""ForeignId"": null,
        ""UrlEncodedKey"": ""EAAAAPhlc7JODOrteeyeyY0j8P4gMZJVp2krvh2233xa0Nk1GZL4QvXIxSWGzugZVgafMjSgmua5xkaUugJzMMuHlcU!3d"",
        ""Workflow"": {
          ""WorkflowTypeId"": 0,
          ""Name"": null,
          ""Description"": null,
          ""Status"": null,
          ""IsProcessing"": false,
          ""ActivatedDateTime"": null,
          ""LastProcessedDateTime"": null,
          ""CompletedDateTime"": null,
          ""IsActive"": false,
          ""Activities"": [],
          ""IsPersisted"": false,
          ""CreatedDateTime"": null,
          ""ModifiedDateTime"": null,
          ""CreatedByPersonAliasId"": null,
          ""ModifiedByPersonAliasId"": null,
          ""Id"": 0,
          ""Guid"": ""bff03650-ac70-4172-9915-ec3dae54a84c"",
          ""ForeignId"": null,
          ""UrlEncodedKey"": ""EAAAAFe1QzuSp!2bDm5xz7DXcbsTZVbgtGLqL59qBA2bC9NQLBfyHm3NgsLjGJ93CGvA!2fXIec6B5CXAnS70oyVuLXPTwk!3d"",
          ""WorkflowType"": {
            ""IsSystem"": false,
            ""IsActive"": null,
            ""Name"": null,
            ""Description"": null,
            ""CategoryId"": null,
            ""Order"": 0,
            ""WorkTerm"": null,
            ""ProcessingIntervalSeconds"": null,
            ""IsPersisted"": false,
            ""LoggingLevel"": 0,
            ""IconCssClass"": null,
            ""Category"": null,
            ""ActivityTypes"": [],
            ""CreatedDateTime"": null,
            ""ModifiedDateTime"": null,
            ""CreatedByPersonAliasId"": null,
            ""ModifiedByPersonAliasId"": null,
            ""Id"": 0,
            ""Guid"": ""c5436b8c-3318-42ce-955e-8c92941e31e7"",
            ""ForeignId"": null,
            ""UrlEncodedKey"": ""EAAAAGSb5DpmqLSq!2bDDN0k1JVmEfc6UTBMebv9YISx0q4PzxZ33Mu8!2boX!2bGWkGPq8z3KBBOLEhLGY8vvnw7sN3kLFM8!3d""
          }
        },
        ""ActivityType"": {
          ""IsActive"": null,
          ""WorkflowTypeId"": 0,
          ""Name"": null,
          ""Description"": null,
          ""IsActivatedWithWorkflow"": false,
          ""Order"": 0,
          ""ActionTypes"": [],
          ""CreatedDateTime"": null,
          ""ModifiedDateTime"": null,
          ""CreatedByPersonAliasId"": null,
          ""ModifiedByPersonAliasId"": null,
          ""Id"": 0,
          ""Guid"": ""dba263b3-1d8c-492b-8e3b-f22b0ec8de74"",
          ""ForeignId"": null,
          ""UrlEncodedKey"": ""EAAAAAdvo6mI5UkO9Nl2hFwQORghGVY431blosTie11MmydvePVfZty5vE!2fPQddx40e3bAP50K6oqpPhlxkfeerZ278!3d""
        }
      },
      ""ActionType"": {
        ""ActivityTypeId"": 0,
        ""Name"": null,
        ""Order"": 0,
        ""EntityTypeId"": 0,
        ""IsActionCompletedOnSuccess"": false,
        ""IsActivityCompletedOnSuccess"": false,
        ""WorkflowFormId"": null,
        ""CriteriaAttributeGuid"": null,
        ""CriteriaComparisonType"": 0,
        ""CriteriaValue"": null,
        ""EntityType"": null,
        ""WorkflowForm"": null,
        ""CreatedDateTime"": null,
        ""ModifiedDateTime"": null,
        ""CreatedByPersonAliasId"": null,
        ""ModifiedByPersonAliasId"": null,
        ""Id"": 0,
        ""Guid"": ""b2da5aea-0a29-468b-b793-629712a80d5d"",
        ""ForeignId"": null,
        ""UrlEncodedKey"": ""EAAAANd2YTTVJlcnjRwqg0FLMcyUD9NZ8CF6T!2bAkdZqMiQv!2fc6GU8A2vbLPR187daK8B!2bgBZeKPEz4UUvV9K1Xyey70!3d""
      }
    }
  ]
}
</pre>
", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, false, @"
{% if Actions.size > 0 %}
    <div class='panel panel-info'> 
        <div class='panel-heading'>
            <h4 class='panel-title'>My {% if Role == '0' %}Tasks{% else %}Requests{% endif %}</h4>
        </div>
        <div class='panel-body'>
            <ul class='fa-ul'>
                {% for action in Actions %}
                    <li>
                        <i class='fa-li {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>
                        <a href='~/{% if Role == '0' %}WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}{% else %}Workflow{% endif %}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }}{% if role == '0' %} ({{ action.Activity.ActivityType.Name }}){% endif %}</a>
                    </li>
                {% endfor %}
            </ul>
        </div>
    </div>
{% endif %}
", "", 3 )]
    public partial class MyWorkflowsLiquid : Rock.Web.UI.RockBlock
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

            // helper code to get the liquid help text
            //var workflowAction = new WorkflowAction();
            //workflowAction.ActionType = new WorkflowActionType();
            //workflowAction.Activity = new WorkflowActivity();
            //workflowAction.Activity.ActivityType = new WorkflowActivityType();
            //workflowAction.Activity.Workflow = new Workflow();
            //workflowAction.Activity.Workflow.WorkflowType = new WorkflowType();
            //var actions = new List<object>();
            //actions.Add( workflowAction.ToLiquid( true ) );
            //var mergeFields = new Dictionary<string, object>();
            //mergeFields.Add( "Actions", actions );
            //string help = mergeFields.ToJson();
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
            string role = GetAttributeValue("Role");
            if (string.IsNullOrWhiteSpace(role))
            {
                role = "0";
            }

            string contents = GetAttributeValue( "Contents" );

            string appRoot = ResolveRockUrl( "~/" );
            string themeRoot = ResolveRockUrl( "~~/" );
            contents = contents.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            List<WorkflowAction> actions = null;
            if (role == "1")
            {
                actions = GetWorkflows();
            }
            else
            {
                actions = GetActions();
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Role", role );
            mergeFields.Add( "Actions", actions );

            lContents.Text = contents.ResolveMergeFields( mergeFields );
        }

        private List<WorkflowAction> GetWorkflows()
        {
            var actions = new List<WorkflowAction>();

            if ( CurrentPerson != null )
            {
                var rockContext = new RockContext();

                var categoryIds = GetCategories( rockContext );
                
                var qry = new WorkflowService( rockContext ).Queryable()
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

        private List<WorkflowAction> GetActions()
        {
            var formActions = new List<WorkflowAction>();
            
            if ( CurrentPerson != null )
            {
                var rockContext = new RockContext();

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
            if (formActions == null)
            {
                formActions = new WorkflowActionService( rockContext ).GetActiveForms( CurrentPerson );
                RockPage.SaveSharedItem( "ActiveForms", formActions );
            }

            return formActions;
        }

        private List<int> GetCategories( RockContext rockContext )
        {
            int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.WorkflowType ) ).Id;

            var selectedCategories = new List<Guid>();
            GetAttributeValue( "Categories" ).SplitDelimitedValues().ToList().ForEach( c => selectedCategories.Add( c.AsGuid() ) );

            bool includeChildCategories = GetAttributeValue( "IncludeChildCategories" ).AsBoolean();

            return GetCategoryIds( new List<int>(), new CategoryService( rockContext ).GetNavigationItems( entityTypeId, selectedCategories, includeChildCategories, CurrentPerson ) );
        }

        private List<int> GetCategoryIds( List<int> ids, List<CategoryNavigationItem> categories )
        {
            foreach( var categoryNavItem in categories)
            {
                ids.Add( categoryNavItem.Category.Id );
                GetCategoryIds( ids, categoryNavItem.ChildCategories );
            }

            return ids;
        }

        #endregion
    }
}