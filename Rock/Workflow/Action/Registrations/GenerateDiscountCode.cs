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
    [Description( "Generates a new discount code on a registration template" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Generate Discount Code" )]

    [WorkflowTextOrAttribute( "Discount Code Length", "Discount Code Length Attribute", "Length to set the discount code", true,
        "", "", 0, "DiscountCodeLength", new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    [CustomRadioListField( "Discount Type", "Type of discount to apply, percent or Amount", "Percent, Amount", true, "Percent", "", 1, "DiscountType" )]

    [WorkflowTextOrAttribute( "Discount Amount", "Discount Amount Attribute", "Ammount in decimal to set the discount (percent or Amount)", true,
        "", "", 2, "DiscountAmount", new string[] { "Rock.Field.Types.DecimalFieldType" } )]

    [WorkflowTextOrAttribute( "Maximum Usage", "Maximum Usage Attribute", "The maximum number of times (registrations) that the discount code can be used.", true,
        "", "", 3, "MaximumUsage", new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    [WorkflowTextOrAttribute( "Maximum Registrants", "Maximum Registrants Attribute", "The maximum number of registrants (per registration) that the discount code should apply to. ", true,
        "", "", 4, "MaximumRegistrants", new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    [WorkflowTextOrAttribute( "Minimum Registrants", "Minimum Registrants Attribute", "The minimum number of registrants (per registration) that are required in order to use this discount code.", true,
        "", "", 5, "MinimumRegistrants", new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    [WorkflowAttribute( "Effective Dates Attribute", "The date range in which the discount code is valid.", true,
        "", "", 6, "EffectiveDates", new string[] { "Rock.Field.Types.DateRangeFieldType" } )]

    [CustomDropdownListField( "Registration Template", "Registration template to add the discount code to.",
        "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [RegistrationTemplate] ORDER BY [Name]", true, "", "", 7 )]

    [WorkflowAttribute( "Discount Code Attribute", "Attribute to save the discount code into.", true, "", "", 8, null,
        new string[] { "Rock.Field.Types.TextFieldType" } )]

    public class GenerateDiscountCode : ActionComponent
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

            var length = GetAttributeValue( action, "DiscountCodeLength", true ).ResolveMergeFields( mergeFields ).AsInteger();
            if ( length < 3 )
            {
                length = 6;
            }
            var code = Guid.NewGuid().ToString().Replace( "-", "" ).ToUpper().Substring( 0, length );
            var discountCode = new RegistrationTemplateDiscount();
            discountCode.Code = code;

            //Set discount value
            var discountType = GetAttributeValue( action, "DiscountType" );
            decimal discountAmount = GetAttributeValue( action, "DiscountAmount", true ).ResolveMergeFields( mergeFields ).AsDecimal();
            if ( discountType == "Percent" )
            {
                discountCode.DiscountPercentage = discountAmount / 100;
            }
            if ( discountType == "Amount" )
            {
                discountCode.DiscountAmount = discountAmount;
            }

            var maximumUsage = GetAttributeValue( action, "MaximumUsage", true ).ResolveMergeFields( mergeFields ).AsInteger();
            if ( maximumUsage > 0 )
            {
                discountCode.MaxUsage = maximumUsage;
            }

            var maximumRegistrants = GetAttributeValue( action, "MaximumRegistrants", true ).ResolveMergeFields( mergeFields ).AsInteger();
            if ( maximumRegistrants > 0 )
            {
                discountCode.MaxRegistrants = maximumRegistrants;
            }

            var minimumRegistrants = GetAttributeValue( action, "MinimumRegistrants", true ).ResolveMergeFields( mergeFields ).AsInteger();
            if ( minimumRegistrants < 0 )
            {
                discountCode.MinRegistrants = minimumRegistrants;
            }

            var effectiveDates = GetAttributeValue( action, "EffectiveDates", true );
            if ( !string.IsNullOrWhiteSpace( effectiveDates ) )
            {
                var dates = effectiveDates.Split( ',' );
                if ( dates.Length > 1
                    && !string.IsNullOrWhiteSpace( dates[0] )
                    && !string.IsNullOrWhiteSpace( dates[1] ) )
                {
                    discountCode.StartDate = dates[0].AsDateTime();
                    discountCode.EndDate = dates[1].AsDateTime();
                }
            }

            registrationTemplate.Discounts.Add( discountCode );
            rockContext.SaveChanges();
            SetWorkflowAttributeValue( action, "DiscountCodeAttribute", code );
            return true;
        }
    }
}