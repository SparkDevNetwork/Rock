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

namespace Rock.ViewModel.Blocks
{
    /// <summary>
    /// RegistrationEntryBlockViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockViewModel : IViewModel
    {
        /// <summary>
        /// Gets or sets the instructions HTML.
        /// </summary>
        /// <value>
        /// The instructions HTML.
        /// </value>
        public string InstructionsHtml { get; set; }

        /// <summary>
        /// Gets or sets the registrant term.
        /// </summary>
        /// <value>
        /// The registrant term.
        /// </value>
        public string RegistrantTerm { get; set; }

        /// <summary>
        /// Gets or sets the registrant plural term.
        /// </summary>
        /// <value>
        /// The registrant plural term.
        /// </value>
        public string PluralRegistrantTerm { get; set; }

        /// <summary>
        /// Gets or sets the plural fee term.
        /// </summary>
        /// <value>
        /// The plural fee term.
        /// </value>
        public string PluralFeeTerm { get; set; }

        /// <summary>
        /// Gets or sets the registrant forms.
        /// </summary>
        /// <value>
        /// The registrant forms.
        /// </value>
        public IEnumerable<RegistrationEntryBlockFormViewModel> RegistrantForms { get; set; }

        /// <summary>
        /// Gets or sets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        public IEnumerable<RegistrationEntryBlockFeeViewModel> Fees { get; set; }

        /// <summary>
        /// Gets or sets the family members.
        /// </summary>
        /// <value>
        /// The family members.
        /// </value>
        public IEnumerable<RegistrationEntryBlockFamilyMemberViewModel> FamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets the registration attribute title end.
        /// </summary>
        /// <value>
        /// The registration attribute title end.
        /// </value>
        public string RegistrationAttributeTitleEnd { get; set; }

        /// <summary>
        /// Gets or sets the registration attribute title start.
        /// </summary>
        /// <value>
        /// The registration attribute title start.
        /// </value>
        public string RegistrationAttributeTitleStart { get; set; }

        /// <summary>
        /// Gets or sets the registration attributes start.
        /// </summary>
        /// <value>
        /// The registration attributes start.
        /// </value>
        public IEnumerable<AttributeViewModel> RegistrationAttributesStart { get; set; }

        /// <summary>
        /// Gets or sets the registration attributes end.
        /// </summary>
        /// <value>
        /// The registration attributes end.
        /// </value>
        public IEnumerable<AttributeViewModel> RegistrationAttributesEnd { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFamilyMemberViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFamilyMemberViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        /// <value>
        /// The family unique identifier.
        /// </value>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the field values.
        /// </summary>
        /// <value>
        /// The field values.
        /// </value>
        public IDictionary<Guid, object> FieldValues { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFeeViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFeeViewModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public IEnumerable<RegistrationEntryBlockFeeItemViewModel> Items { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFeeItemViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFeeItemViewModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the count remaining.
        /// </summary>
        /// <value>
        /// The count remaining.
        /// </value>
        public int? CountRemaining { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFormViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFormViewModel
    {
        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public IEnumerable<RegistrationEntryBlockFormFieldViewModel> Fields { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFormFieldViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFormFieldViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the field source.
        /// </summary>
        /// <value>
        /// The field source.
        /// </value>
        public int FieldSource { get; set; }

        /// <summary>
        /// Gets or sets the type of the person field.
        /// </summary>
        /// <value>
        /// The type of the person field.
        /// </value>
        public int PersonFieldType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public AttributeViewModel Attribute { get; set; }

        /// <summary>
        /// Gets or sets the type of the visibility rule.
        /// </summary>
        /// <value>
        /// The type of the visibility rule.
        /// </value>
        public int VisibilityRuleType { get; set; }

        /// <summary>
        /// Gets or sets the visibility rules.
        /// </summary>
        /// <value>
        /// The visibility rules.
        /// </value>
        public IEnumerable<RegistrationEntryBlockVisibilityViewModel> VisibilityRules { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockVisibilityViewModel
    /// </summary>
    public class RegistrationEntryBlockVisibilityViewModel
    {
        /// <summary>
        /// Gets or sets the compared to registration template form field unique identifier.
        /// </summary>
        /// <value>
        /// The compared to registration template form field unique identifier.
        /// </value>
        public Guid ComparedToRegistrationTemplateFormFieldGuid { get; set; }

        /// <summary>
        /// Gets or sets the type of the comparison.
        /// </summary>
        /// <value>
        /// The type of the comparison.
        /// </value>
        public int ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the compared to value.
        /// </summary>
        /// <value>
        /// The compared to value.
        /// </value>
        public string ComparedToValue { get; set; }
    }
}
