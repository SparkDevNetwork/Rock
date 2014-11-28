﻿// <copyright>
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
using Rock.Security;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sends a Background Check Request.
    /// </summary>
    [Description( "Sends a Background Check Request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Background Check Request" )]

    [ComponentField( "Rock.Security.BackgroundCheckContainer, Rock", "Background Check Provider", "The Background Check provider to use", false, "", "", 0, "Provider" )]
    public class BackgroundCheckRequest : ActionComponent
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

            string providerGuid = GetAttributeValue( action, "Provider" );
            if ( !string.IsNullOrWhiteSpace( providerGuid ) )
            {
                var provider = BackgroundCheckContainer.GetComponent( providerGuid );
                if ( provider != null )
                {
                    return provider.SendRequest( rockContext, action.Activity.Workflow, out errorMessages );
                }
                else
                {
                    errorMessages.Add( "Invalid Background Check Provider!" );
                }
            }
            else
            {
                errorMessages.Add( "Invalid Background Check Provider Guid!" );
            }

            return false;
        }
    }
}