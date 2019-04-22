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
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Updates an existing discount code on a registration template
    /// </summary>
    [ActionCategory( "Registrations" )]
    [Description( "Updates an existing discount code on a registration template" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Update Discount Code" )]

    [CustomDropdownListField( "Registration Template", "Registration template the discount code belongs to.",
        "SELECT [Id] AS [Value], [Name] AS [Text] FROM [RegistrationTemplate] ORDER BY [Name]", true, "", "", 0 )]

    [WorkflowTextOrAttribute( "Discount Code", "Discount Code Attribute", "Discount code to update.", true,
        "", "", 1, "DiscountCode", new string[] { "Rock.Field.Types.TextFieldType" } )]

    [CustomRadioListField( "Discount Type", "Type of discount to apply, percent or amount", "Percent, Amount", true, "Percent", "", 2, "DiscountType" )]

    [WorkflowTextOrAttribute( "Discount Amount", "Discount Amount Attribute", "Amount in decimal to set the discount (percent or amount)", true,
        "", "", 3, "DiscountAmount", new string[] { "Rock.Field.Types.DecimalFieldType" } )]

    [BooleanField( "Update Past Registrations", "Should registrations that have used this discount code be retroactively updated?", order: 4 )]

    public class UpdateDiscountCode : ActionComponent
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

            var registrationTemplateDiscountService = new RegistrationTemplateDiscountService( rockContext );
            var discountCode = GetAttributeValue( action, "DiscountCode", true ).ResolveMergeFields( mergeFields );
            var registrationTemplateId = GetAttributeValue( action, "RegistrationTemplate" ).ResolveMergeFields( mergeFields ).AsInteger();

            var registrationDiscountCode = registrationTemplateDiscountService
                .Queryable()
                .Where( c =>
                     c.RegistrationTemplateId == registrationTemplateId
                     && c.Code == discountCode )
                .FirstOrDefault();


            if ( registrationDiscountCode == null )
            {
                errorMessages.Add( "Could not find discount code" );
                return false;
            }

            //Set discount value
            var discountType = GetAttributeValue( action, "DiscountType" );
            decimal discountAmount = GetAttributeValue( action, "DiscountAmount", true ).ResolveMergeFields( mergeFields ).AsDecimal();
            if ( discountType == "Percent" )
            {
                registrationDiscountCode.DiscountPercentage = discountAmount / 100;
                registrationDiscountCode.DiscountAmount = 0;
            }
            if ( discountType == "Amount" )
            {
                registrationDiscountCode.DiscountAmount = discountAmount;
                registrationDiscountCode.DiscountPercentage = 0;
            }

            if ( GetAttributeValue( action, "UpdatePastRegistrations" ).AsBoolean() )
            {
                var registrationService = new RegistrationService( rockContext );
                var registrations = registrationService
                    .Queryable()
                    .Where( r => r.DiscountCode == discountCode
                    && r.RegistrationInstance.RegistrationTemplateId == registrationTemplateId );
                foreach ( var registration in registrations )
                {
                    if ( discountType == "Percent" )
                    {
                        registration.DiscountPercentage = discountAmount / 100;
                        registration.DiscountAmount = 0;
                    }
                    if ( discountType == "Amount" )
                    {
                        registration.DiscountAmount = discountAmount;
                        registration.DiscountPercentage = 0;
                    }
                }
            }

            rockContext.SaveChanges();
            return true;
        }
    }
}
