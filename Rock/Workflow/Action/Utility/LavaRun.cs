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
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Runs Lava and sets an attribute's value to the result.
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Runs Lava and sets an attribute's value to the result." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Lava Run" )]

    [CodeEditorField( "Lava", "The <span class='tip tip-lava'></span> to run.", Web.UI.Controls.CodeEditorMode.Lava, Web.UI.Controls.CodeEditorTheme.Rock, 300, true, "", "", 0, "Value" )]
    [WorkflowAttribute( "Attribute", "The attribute to store the result in.", false, "", "", 1 )]
    public class RunLava : ActionComponent
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

            var attribute = AttributeCache.Read( GetAttributeValue( action, "Attribute" ).AsGuid(), rockContext );
            if ( attribute != null )
            {
                string value = GetAttributeValue( action, "Value" ).ResolveMergeFields( GetMergeFields( action ) );
                SetWorkflowAttributeValue( action, attribute.Guid, value );
                action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, value ) );
            }

            return true;
        }

    }
}