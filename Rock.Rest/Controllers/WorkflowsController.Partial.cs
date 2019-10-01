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
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class WorkflowsController
    {
        /// <summary>
        /// Initiates a new workflow
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Workflows/WorkflowEntry/{workflowTypeId}" )]
        public Rock.Model.Workflow WorkflowEntry( int workflowTypeId )
        {
            var rockContext = new Rock.Data.RockContext();
            var workflowType = WorkflowTypeCache.Get( workflowTypeId );

            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
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
