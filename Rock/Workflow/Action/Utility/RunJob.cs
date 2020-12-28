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
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Tasks;

namespace Rock.Workflow.Action
{
    [ActionCategory( "Utility" )]
    [Description( "Triggers a job to run" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Job Run" )]

    [CustomDropdownListField( "Job", "The job to run.", "SELECT j.[Guid] AS [Value], j.[Name] AS [Text] From [ServiceJob] j ORDER BY j.[Name]", true, "", "", 0)]
    class RunJob : ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            
            var JobGuid = GetAttributeValue( action, "Job" ).AsGuid();

            if ( !JobGuid.IsEmpty() )
            {
                ServiceJob Job = new ServiceJobService( rockContext ).Get( JobGuid );
                if ( Job != null )
                {
                    new ProcessRunJobNow.Message { JobId = Job.Id }.Send();
                    action.AddLogEntry( string.Format( "The '{0}' job has been started.", Job.Name ) );

                    return true;
                }

            }

            errorMessages.Add("The specified Job could not be found");

            return false;
        }
    }
}
