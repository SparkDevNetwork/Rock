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
    /// Runs a SQL query
    /// </summary>
    [ActionCategory( "Finance" )]
    [Description( "Sets a benevolence request's attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Benevolence Request Set Attribute" )]

    [WorkflowAttribute( "Benevolence Request", "Workflow attribute to set the returned benevolence request to.", true, "", "", 0, "BenevolenceRequest",
        new string[] { "Rock.Field.Types.BenevolenceRequestFieldType" } )]
    [AttributeField( SystemGuid.EntityType.BENEVOLENCE_REQUEST, "Benevolence Request Attribute", "The benevolence request attribute that should be updated with the provided value.", true, false, "", "", 1 )]
    [WorkflowTextOrAttribute( "Value", "Attribute Value", "The value or attribute value to set the benevolence request attribute to. <span class='tip tip-lava'></span>", true, "", "", 2, "Value" )]
    [BooleanField("Use Blank Value", "If the provided value is an empty string should it be used to blank out the attribute value or should it be ignored? If true the empty value will be used to blank out the attribute.", true, order: 3)]
    public class BenevolenceRequestSetAttribute : ActionComponent
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

            var mergeFields = GetMergeFields( action );

            var benevolenceRequest = new BenevolenceRequestService( rockContext ).Get( GetAttributeValue( action, "BenevolenceRequest", true ).AsGuid() );

            if ( benevolenceRequest == null )
            {
                var errorMessage = "Benevolence request could not be found.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            var attribute = AttributeCache.Get( GetAttributeValue( action, "BenevolenceRequestAttribute" ).AsGuid() );

            if ( attribute == null )
            {
                var errorMessage = "Could not find a benevolence attribute matching the one provided.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            var attributeValue = GetAttributeValue( action, "Value", true ).ResolveMergeFields( mergeFields );
            bool useBlankValues = GetActionAttributeValue( action, "UseBlankValue" ).AsBoolean();

            if ( !string.IsNullOrWhiteSpace( attributeValue ) ||  useBlankValues)
            {
                benevolenceRequest.LoadAttributes();

                Rock.Attribute.Helper.SaveAttributeValue( benevolenceRequest, attribute, attributeValue, rockContext );
                action.AddLogEntry( $"Updated benevolence attribute '{attribute.Name}' to '{attributeValue}'." );
            }

            return true;
        }
    }
}