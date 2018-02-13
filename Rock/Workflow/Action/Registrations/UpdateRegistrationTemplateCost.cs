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
using System.Linq;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Generates a new discount code on a registration template
    /// </summary>
    [ActionCategory( "Registrations" )]
    [Description( "Updates the cost of a registration template." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Update Registration Template Cost" )]

    [CustomDropdownListField( "Registration Template", "Registration template to add the discount code to.",
        "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [RegistrationTemplate] ORDER BY [Name]", true, "", "", 0 )]

    [WorkflowTextOrAttribute( "New Cost", "New Cost Attribute", "The new cost of the workflow.", true,
        "", "", 0, "NewCost", new string[] { "Rock.Field.Types.DecimalFieldType" } )]

    [WorkflowTextOrAttribute( "New Minimum Cost", "New Minimum Cost Attribute", "The new minimum cost of the workflow.", true,
        "", "", 0, "NewMinimumCost", new string[] { "Rock.Field.Types.DecimalFieldType" } )]

    public class UpdateRegistrationCost : ActionComponent
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

            var registrationTemplateService = new RegistrationTemplateService( rockContext );
            var registrationTemplate = registrationTemplateService.Get( GetAttributeValue( action, "RegistrationTemplate" ).ResolveMergeFields( mergeFields ).AsGuid() );
            if ( registrationTemplate == null )
            {
                errorMessages.Add( "Could not find selected registration template" );
                return false;
            }

            var newCost = GetAttributeValue( action, "NewCost", true )
                .ResolveMergeFields( mergeFields )
                .AsDecimal();

            var newMinimumCost = GetAttributeValue( action, "NewMinimumCost", true )
                .ResolveMergeFields( mergeFields )
                .AsDecimal();

            registrationTemplate.Cost = newCost;
            registrationTemplate.MinimumInitialPayment = newMinimumCost;
            registrationTemplate.SetCostOnInstance = false;
            rockContext.SaveChanges();
            return true;
        }
    }
}
