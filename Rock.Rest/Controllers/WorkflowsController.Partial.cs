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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;
using System.Net;
using System;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class WorkflowsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="hidePageIds">List of pages that should not be included in results</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Workflows/WorkflowEntry/{workflowTypeId}" )]
        public Rock.Model.Workflow WorkflowEntry( int workflowTypeId )
        {
            var rockContext = new Rock.Data.RockContext();
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( workflowTypeId );

            if ( workflowType != null )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, "Workflow From REST" );

                // set workflow attributes from querystring
                foreach(var parm in Request.GetQueryStrings()){
                    workflow.SetAttributeValue( parm.Key, parm.Value );
                }

                // save -> run workflow
                List<string> workflowErrors;
                new Rock.Model.WorkflowService( rockContext ).Process( workflow, out workflowErrors );

                var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
                return workflow;
            }
            else 
            {
                var response = ControllerContext.Request.CreateResponse( HttpStatusCode.NotFound );
            }

            return null;

        }
    }
}
