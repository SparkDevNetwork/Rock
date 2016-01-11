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
    /// Sets a workflow status
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Logs an error message. This can be used to stop processing. If used in a workflow initiated by a PreSave or PreDelete trigger, this will result in save or delete being aborted. This action always returns an unsuccessful result which causes processing of the activity to stop." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Log Error" )]
    [TextField( "Message", "The error message to return. <span class='tip tip-lava'></span>" )]
    public class LogError : ActionComponent
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
            string message = GetAttributeValue( action, "Message" ).ResolveMergeFields( GetMergeFields( action ) );
            errorMessages.Add( message );
            return false;
        }
    }
}