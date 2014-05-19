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
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets the name of the workflow
    /// </summary>
    [Description( "Sets the name of the workflow" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Name" )]

    [TextField( "Text Value", "A text value to use for the workflow name.", false, "", "", 0 )]
    [WorkflowAttribute( "Attribute", "An attribute value to use for the workflow name (will only be used if 'Text Value' is blank).", false, "", "", 1 )]
    public class SetName : ActionComponent
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

            string name = GetAttributeValue( action, "TextValue" );
            if (string.IsNullOrWhiteSpace(name))
            {
                Guid guid = GetAttributeValue( action, "Attribute" ).AsGuid();
                if ( !guid.IsEmpty() )
                {
                    name = GetWorklowAttributeValue( action, guid );
                }
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                action.Activity.Workflow.Name = name;
                action.AddLogEntry( string.Format( "Set Workflow Name to '{0}'", name ) );
            }

            return true;
        }
    }
}