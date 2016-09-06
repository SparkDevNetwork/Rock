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
using System.Management.Automation;
using System.Collections.ObjectModel;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;


namespace Rock.Workflow.Action
{
    /// <summary>
    /// Runs a Powershell script
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Runs Powershell commands." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Powershell Run" )]
    
    [CodeEditorField( "Powershell", "The powershell code. <span class='tip tip-lava'></span>", CodeEditorMode.Powershell, CodeEditorTheme.Rock, 100, true, "echo \"Hello World\"", "", 0 )]
    [CodeEditorField("Output Lava", "The output format. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false, @"Output:
{{ PSOutput | Join:'\n' }}

Debug:
{{ PSDebug | Map:'Message' | Join:'\n' }}

Error:
{{ PSError | Map:'Message' | Join:'\n' }}

Progress:
{{ PSProgress | ToJSON }}

Verbose:
{{ PSVerbose | Map:'Message' | Join:'\n' }}

Warning:
{{ PSWarning | Map:'Message' | Join:'\n' }}", "", 1 )]
    [WorkflowAttribute( "Result Attribute", "The attribute to set to the output of the powershell to.", false, "", "", 2 )]
    public class RunPowershell : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            String psScript = GetAttributeValue( action, "Powershell" ).ResolveMergeFields( GetMergeFields( action ) );

            using ( PowerShell PowerShellInstance = PowerShell.Create() )
            {
                PowerShellInstance.AddScript( psScript );
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                var mergeFields = GetMergeFields(action);
                mergeFields.Add( "PSOutput", PSOutput );
                mergeFields.Add( "PSDebug", PowerShellInstance.Streams.Debug );
                mergeFields.Add( "PSError", PowerShellInstance.Streams.Error );
                mergeFields.Add( "PSProgress", PowerShellInstance.Streams.Progress );
                mergeFields.Add( "PSVerbose", PowerShellInstance.Streams.Verbose );
                mergeFields.Add( "PSWarning", PowerShellInstance.Streams.Warning);

                String output = GetAttributeValue( action, "OutputLava" ).ResolveMergeFields( mergeFields );

                Guid ResultAttributeGuid = GetAttributeValue( action, "ResultAttribute" ).AsGuid();
                var ResultAttribute = AttributeCache.Read( ResultAttributeGuid, rockContext );
                if ( ResultAttribute != null )
                {
                    if ( ResultAttribute.EntityTypeId == new Rock.Model.Workflow().TypeId )
                    {
                        action.Activity.Workflow.SetAttributeValue( ResultAttribute.Key, output );
                    }
                    else if ( ResultAttribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                    {
                        action.Activity.SetAttributeValue( ResultAttribute.Key, output );
                    }
                }
            }

            return true;

        }
    }
}
