﻿// <copyright>
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

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Writes a message to Workflow log.
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Writes a message to Workflow log." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Write To Log" )]
    [MemoField( "Message", "The message to write to the log. <span class='tip tip-lava'></span>", true, "" )]
    public class WriteToLog : ActionComponent
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

            var message = GetAttributeValue( action, "Message" ).ResolveMergeFields( GetMergeFields( action ) );

            if (!string.IsNullOrWhiteSpace(message))
            {
                action.Activity.Workflow.AddLogEntry(message, true);
            }
            

            return true;
        }
    }
}