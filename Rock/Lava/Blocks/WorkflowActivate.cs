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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DotLiquid;
using Rock.Data;
using Rock.Model;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Tag which allows a snippet of Lava code to fire off a new Workflow or a new
    /// Activity in an existing Workflow. The Lava inside the tags is provided
    /// <c>Workflow</c> and <c>Activity</c> that contain the workflow and the
    /// activity (if one was activated). Also provides an <c>Error</c> variable if any
    /// error occurred.
    /// </summary>
    /// 
    /// <example>
    /// This example shows how to initiate a new Workflow instance. The <c>WorkflowType</c> can
    /// be either an integer (42), string integer ('42') or string GUID
    /// ('19bb963c-d8c5-415c-ad36-202757b082bb'). You can optionally provide a <c>WorkflowName</c>
    /// parameter to set the name of the workflow.
    /// <code>
    /// {% workflowactivate WorkflowType:42 WorkflowName:'My Test Workflow' %}
    ///   Activated new workflow {{ Workflow.Name }} with Id #{{ Workflow.Id }}.
    /// {% endworkflowactivate %}
    /// </code>
    /// </example>
    /// 
    /// <example>
    /// This example shows how to activate an Activity in an existing Workflow instance. The
    /// <c>WorkflowId</c> can be either an integer (122), string integer ('122') or string GUID
    /// ('e567d8a4-2c85-4f8f-9f59-12218ffa2213'). You must also provide a <c>ActivityType</c>
    /// parameter that is either an integer (79), string integer ('79') or string GUID
    /// ('dfbe3fc9-53e2-4253-bc3d-6f5b8312ee6f').
    /// <code>
    /// {% workflowactivate WorkflowId:122 ActivityType:'dfbe3fc9-53e2-4253-bc3d-6f5b8312ee6f' %}
    ///   Activated new activity {{ Activity.Name }} with Id #{{ Activity.Id }} in workflow {{ Workflow.Name }}.
    /// {% endworkflowactivate %}
    /// </code>
    /// </example>
    /// 
    /// <example>
    /// Example of passing attribute values to a workflow. The same can be done with an Activity.
    /// Any unrecognized parameters are treated as potential attributes in the Workflow or Activity.
    /// <code>
    /// {% workflowactivate WorkflowId:42 Person:CurrentPerson.Guid ProcessValue:7283 %}
    /// {% endworkflowactivate %}
    /// </code>
    /// </example>
    public class WorkflowActivate : RockLavaBlockBase
    {
        private string _markup;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterTag<WorkflowActivate>( "workflowactivate" );
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            var attributes = new Dictionary<string, string>();
            string parmWorkflowType = null;
            string parmWorkflowName = null;
            string parmWorkflowId = null;
            string parmActivityType = null;

            bool doReturn = false;
            if ( doReturn )
                return;

            /* Parse the markup text to pull out configuration parameters. */
            var parms = ParseMarkup( _markup, context );
            foreach ( var p in parms )
            {
                if ( p.Key.ToLower() == "workflowtype" )
                {
                    parmWorkflowType = p.Value;
                }
                else if ( p.Key.ToLower() == "workflowname" )
                {
                    parmWorkflowName = p.Value;
                }
                else if ( p.Key.ToLower() == "workflowid" )
                {
                    parmWorkflowId = p.Value;
                }
                else if ( p.Key.ToLower() == "activitytype" )
                {
                    parmActivityType = p.Value;
                }
                else
                {
                    attributes.AddOrReplace( p.Key, p.Value );
                }
            }

            /* Process inside a new stack level so our own created variables do not
             * persist throughout the rest of the workflow. */
            context.Stack( () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    WorkflowTypeService workflowTypeService = new WorkflowTypeService( rockContext );
                    WorkflowActivityTypeService workflowActivityTypeService = new WorkflowActivityTypeService( rockContext );
                    WorkflowService workflowService = new WorkflowService( rockContext );
                    Rock.Model.Workflow workflow = null;
                    WorkflowActivity activity = null;

                    /* They provided a WorkflowType, so we need to kick off a new workflow. */
                    if ( parmWorkflowType != null )
                    {
                        string type = parmWorkflowType;
                        string name = parmWorkflowName ?? string.Empty;
                        WorkflowType workflowType = null;

                        /* Get the type of workflow */
                        if ( type.AsGuidOrNull() != null )
                        {
                            workflowType = workflowTypeService.Get( type.AsGuid() );
                        }
                        else if ( type.AsIntegerOrNull() != null )
                        {
                            workflowType = workflowTypeService.Get( type.AsInteger() );
                        }

                        /* Try to activate the workflow */
                        if ( workflowType != null )
                        {
                            workflow = Rock.Model.Workflow.Activate( workflowType, parmWorkflowName );

                            /* Set any workflow attributes that were specified. */
                            foreach ( var attr in attributes )
                            {
                                if ( workflow.Attributes.ContainsKey( attr.Key ) )
                                {
                                    workflow.SetAttributeValue( attr.Key, attr.Value.ToString() );
                                }
                            }

                            if ( workflow != null )
                            {
                                List<string> errorMessages;

                                workflowService.Process( workflow, out errorMessages );

                                if ( errorMessages.Any() )
                                {
                                    context["Error"] = string.Join( "; ", errorMessages.ToArray() );
                                }

                                context["Workflow"] = workflow;
                            }
                            else
                            {
                                context["Error"] = "Could not activate workflow.";
                            }
                        }
                        else
                        {
                            context["Error"] = "Workflow type not found.";
                        }
                    }

                    /* They instead provided a WorkflowId and ActivityType, so we need to activate
                     * an activity in an existing workflow. */
                    else if ( parmWorkflowId != null && parmActivityType != null )
                    {
                        string id = parmWorkflowId.ToString();
                        string type = parmActivityType.ToString();
                        WorkflowActivityType activityType = null;

                        /* Get the workflow */
                        if ( id.AsGuidOrNull() != null )
                        {
                            workflow = workflowService.Get( id.AsGuid() );
                        }
                        else if ( id.AsIntegerOrNull() != null )
                        {
                            workflow = workflowService.Get( id.AsInteger() );
                        }

                        if ( workflow != null )
                        {
                            if ( workflow.CompletedDateTime == null )
                            {
                                /* Get the type of activity */
                                if ( type.AsGuidOrNull() != null )
                                {
                                    activityType = workflowActivityTypeService.Get( type.AsGuid() );
                                }
                                else if ( type.AsIntegerOrNull() != null )
                                {
                                    activityType = workflowActivityTypeService.Get( type.AsInteger() );
                                }

                                if ( activityType != null )
                                {
                                    activity = WorkflowActivity.Activate( activityType, workflow );

                                    /* Set any workflow attributes that were specified. */
                                    foreach ( var attr in attributes )
                                    {
                                        if ( activity.Attributes.ContainsKey( attr.Key ) )
                                        {
                                            activity.SetAttributeValue( attr.Key, attr.Value.ToString() );
                                        }
                                    }

                                    /* Stack overflow recursion results in calling Process on a workflow that
                                     * is already being processed. */
                                    if ( !workflow.IsProcessing )
                                    {
                                        List<string> errorMessages;
                                        workflowService.Process( workflow, out errorMessages );

                                        if ( errorMessages.Any() )
                                        {
                                            context["Error"] = string.Join( "; ", errorMessages.ToArray() );
                                        }

                                        context["Workflow"] = workflow;
                                        context["Activity"] = activity;
                                    }
                                    else
                                    {
                                        context["Error"] = "Cannot activate activity on workflow that is currently being processed.";
                                    }
                                }
                                else
                                {
                                    context["Error"] = "Activity type was not found.";
                                }
                            }
                            else
                            {
                                context["Error"] = "Workflow has already been completed.";
                            }
                        }
                        else
                        {
                            context["Error"] = "Workflow not found.";
                        }
                    }
                    else
                    {
                        context["Error"] = "Must specify one of WorkflowType or WorkflowId.";
                    }

                    RenderAll( NodeList, context, result );
                }
            } );
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No parameters were found in your command. The syntax for a parameter is parmName:'' (note that you must use single quotes).</exception>
        private Dictionary<string, string> ParseMarkup( string markup, Context context )
        {
            var parms = new Dictionary<string, string>();

            var markupItems = Regex.Matches( markup, "([\\w]+):(\"[^\"]*\"|'[^']*'|(?-mix:\\(?[\\w\\-\\.\\[\\]]\\)?)+)" );

            foreach ( Match item in markupItems )
            {
                var key = item.Groups[1].Value;
                var value = context[item.Groups[2].Value].ToStringSafe();

                parms.AddOrReplace( key, value );
            }

            return parms;
        }
    }
}
