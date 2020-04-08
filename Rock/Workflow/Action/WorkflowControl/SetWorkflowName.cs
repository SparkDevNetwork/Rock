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
using System.ComponentModel.DataAnnotations;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets the name of the workflow
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Sets the name of the workflow" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Workflow Set Name" )]

    [WorkflowTextOrAttribute( "Text Value", "Attribute Value", "The value to use for the workflow's name. <span class='tip tip-lava'></span>", false, "", "", 1, "NameValue" )]
    public class SetWorkflowName : ActionComponent
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

            string nameValue = GetAttributeValue( action, "NameValue" );
            Guid guid = nameValue.AsGuid();
            if (guid.IsEmpty())
            {
                nameValue = nameValue.ResolveMergeFields( GetMergeFields( action ) );
            }
            else
            {
                nameValue = action.GetWorklowAttributeValue( guid, true, true );
                
                // HtmlDecode the name since we are storing it in the database and it might be formatted to be shown in HTML
                nameValue = System.Web.HttpUtility.HtmlDecode( nameValue );
            }

            if ( !string.IsNullOrWhiteSpace( nameValue ) )
            {
                var nameMaxLength = action.Activity.Workflow.GetAttributeFrom<MaxLengthAttribute>( "Name" ).Length;

                action.Activity.Workflow.Name = nameValue.Truncate( nameMaxLength );
                action.AddLogEntry( string.Format( "Set Workflow Name to '{0}'", nameValue ) );
            }

            return true;
        }
    }
}