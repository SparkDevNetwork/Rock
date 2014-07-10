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

namespace RockWeb.Blocks.WorkFlow
{
    /// <summary>
    /// Activates a workflow and then redirects user to workflow entry page.
    /// </summary>
    [DisplayName( "Activate Workflow" )]
    [Category( "WorkFlow" )]
    [Description( "Activates a workflow and then redirects user to workflow entry page." )]

    [LinkedPage( "Workflow Entry Page" )]
    public partial class ActivateWorkflow : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int? workflowTypeId = PageParameter( "WorkflowTypeId" ).AsIntegerOrNull();
            if ( workflowTypeId.HasValue )
            {
                var rockContext = new RockContext();
                var workflowType = new WorkflowTypeService( rockContext ).Get( workflowTypeId.Value );
                if ( workflowType != null )
                {
                    var workflow = Workflow.Activate( workflowType, PageParameter( "WorkflowName" ) );
                    if ( workflow != null )
                    {

                        object entity = null;

                        int? personId = PageParameter( "PersonId" ).AsIntegerOrNull();
                        if ( personId.HasValue )
                        {
                            entity = new PersonService( rockContext ).Get( personId.Value );
                        }
                        else
                        {
                            int? groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
                            if ( groupId.HasValue )
                            {
                                entity = new GroupService( rockContext ).Get( groupId.Value );
                            }
                        }

                        var qryParams = new Dictionary<string, string>();
                        qryParams.Add( "WorkflowTypeId", workflowTypeId.ToString() );

                        List<string> workflowErrors;
                        if ( workflow.Process( rockContext, entity, out workflowErrors ) )
                        {
                            if ( workflow.IsPersisted || workflowType.IsPersisted )
                            {
                                var workflowService = new Rock.Model.WorkflowService( rockContext );
                                workflowService.Add( workflow );

                                rockContext.WrapTransaction( () =>
                                {
                                    rockContext.SaveChanges();
                                    workflow.SaveAttributeValues( rockContext );
                                    foreach ( var activity in workflow.Activities )
                                    {
                                        activity.SaveAttributeValues( rockContext );
                                    }
                                } );

                                qryParams.Add( "WorkflowId", workflow.Id.ToString() );
                            }

                            NavigateToLinkedPage( "WorkflowEntryPage", qryParams );

                        }
                        else
                        {
                            nbError.Title = "Workflow Processing Error(s):";
                            nbError.Text = workflowErrors.AsDelimited( "<br/>" );
                        }
                    }
                    else
                    {
                        nbError.Text = "Could not activate workflow.";
                    }
                }
                else
                {
                    nbError.Text = "Invalid Workflow Type Id.";
                }
            }
            else
            {
                nbError.Text = "A workflow could not be activated due to missing Workflow Type Id query string parameter.";
            }
        }

        #endregion
    }
}