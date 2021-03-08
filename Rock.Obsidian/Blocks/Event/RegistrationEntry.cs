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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.Blocks;
using Rock.Web.Cache;

namespace Rock.Obsidian.Blocks.Event
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

    [DisplayName( "Registration Entry" )]
    [Category( "Obsidian > Event" )]
    [Description( "Block used to register for a registration instance." )]
    [IconCssClass( "fa fa-clipboard-list" )]

    public class RegistrationEntry : ObsidianBlockType
    {
        /// <summary>
        /// Page Parameter
        /// </summary>
        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianConfigurationValues()
        {
            var currentPerson = GetCurrentPerson();
            var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsInteger();

            using ( var rockContext = new RockContext() )
            {
                var now = RockDateTime.Now;

                var registrationInstance = new RegistrationInstanceService( rockContext )
                    .Queryable( "RegistrationTemplate.Forms.Fields, RegistrationTemplate.Fees.FeeItems" )
                    .AsNoTracking()
                    .Where( r =>
                        r.Id == registrationInstanceId &&
                        r.IsActive &&
                        r.RegistrationTemplate != null &&
                        r.RegistrationTemplate.IsActive &&
                        ( !r.StartDateTime.HasValue || r.StartDateTime <= now ) &&
                        ( !r.EndDateTime.HasValue || r.EndDateTime > now ) )
                    .FirstOrDefault();

                var registrationTemplate = registrationInstance?.RegistrationTemplate;

                // Get the instructions
                var instructions = registrationInstance?.RegistrationInstructions;

                if ( instructions.IsNullOrWhiteSpace() )
                {
                    instructions = registrationTemplate?.RegistrationInstructions ?? string.Empty;
                }

                // Get the fee term
                var feeTerm = registrationTemplate?.FeeTerm;

                if ( feeTerm.IsNullOrWhiteSpace() )
                {
                    feeTerm = "Fee";
                }

                feeTerm = feeTerm.ToLower();
                var pluralFeeTerm = feeTerm.Pluralize();

                // Get the registrant term
                var registrantTerm = registrationTemplate?.RegistrantTerm;

                if (registrantTerm.IsNullOrWhiteSpace())
                {
                    registrantTerm = "Person";
                }

                registrantTerm = registrantTerm.ToLower();
                var pluralRegistrantTerm = registrantTerm.Pluralize();

                // Get the fees
                var feeModels = registrationTemplate?.Fees?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateFee>();
                var fees = new List<RegistrationEntryBlockFeeViewModel>();

                foreach ( var feeModel in feeModels )
                {
                    var feeViewModel = new RegistrationEntryBlockFeeViewModel
                    {
                        Guid = feeModel.Guid,
                        Name = feeModel.Name,
                        Items = new List<RegistrationEntryBlockFeeItemViewModel>()
                    };

                    foreach ( var feeItemModel in feeModel.FeeItems )
                    {
                        feeViewModel.Items.Add( new RegistrationEntryBlockFeeItemViewModel
                        {
                            Cost = feeItemModel.Cost,
                            Name = feeItemModel.Name,
                            Guid = feeItemModel.Guid
                        } );
                    }

                    fees.Add( feeViewModel );
                }

                // Get forms with fields
                var formModels = registrationTemplate?.Forms?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateForm>();
                var forms = new List<RegistrationEntryBlockFormViewModel>();

                foreach ( var formModel in formModels )
                {
                    var form = new RegistrationEntryBlockFormViewModel();
                    var fieldModels = formModel.Fields.OrderBy( f => f.Order );
                    var fields = new List<RegistrationEntryBlockFormFieldViewModel>();

                    foreach ( var fieldModel in fieldModels )
                    {
                        var field = new RegistrationEntryBlockFormFieldViewModel();
                        var attribute = fieldModel.AttributeId.HasValue ? AttributeCache.Get( fieldModel.AttributeId.Value ) : null;

                        field.Guid = fieldModel.Guid;
                        field.Attribute = attribute?.ToViewModel();
                        field.FieldSource = ( int ) fieldModel.FieldSource;
                        field.PersonFieldType = ( int ) fieldModel.PersonFieldType;
                        field.IsRequired = fieldModel.IsRequired;
                        field.VisibilityRuleType = ( int ) fieldModel.FieldVisibilityRules.FilterExpressionType;

                        field.VisibilityRules = fieldModel.FieldVisibilityRules
                            .RuleList
                            .Where( vr => vr.ComparedToRegistrationTemplateFormFieldGuid.HasValue )
                            .Select( vr => new RegistrationEntryBlockVisibilityViewModel
                            {
                                ComparedToRegistrationTemplateFormFieldGuid = vr.ComparedToRegistrationTemplateFormFieldGuid.Value,
                                ComparedToValue = vr.ComparedToValue,
                                ComparisonType = ( int ) vr.ComparisonType
                            } );

                        fields.Add( field );
                    }

                    form.Fields = fields;
                    forms.Add( form );
                }

                return new RegistrationEntryBlockViewModel
                {
                    InstructionsHtml = instructions,
                    RegistrantTerm = registrantTerm,
                    PluralRegistrantTerm = pluralRegistrantTerm,
                    PluralFeeTerm = pluralFeeTerm,
                    RegistrantForms = forms,
                    Fees = fees
                };
            }
        }
    }
}
