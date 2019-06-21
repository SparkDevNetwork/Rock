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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a RegistrationTemplateFormField that is cached by Rock. 
    /// </summary>
    [Serializable]
    [DataContract]
    public class RegistrationTemplateFormFieldCache : ModelCache<RegistrationTemplateFormFieldCache, RegistrationTemplateFormField>
    {
        #region Matched Entity Properties
               
        /// <summary>
        /// Gets or sets the registration template form identifier.
        /// </summary>
        /// <value>
        /// The registration template form identifier.
        /// </value>
        [DataMember]
        public int RegistrationTemplateFormId { get; private set; }

        /// <summary>
        /// Gets or sets the source of the field value.
        /// </summary>
        /// <value>
        /// The applies to.
        /// </value>
        [DataMember]
        public RegistrationFieldSource FieldSource { get; private set; }

        /// <summary>
        /// Gets or sets the type of the person field.
        /// </summary>
        /// <value>
        /// The type of the person field.
        /// </value>
        [DataMember]
        public RegistrationPersonFieldType PersonFieldType { get; private set; }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        [DataMember]
        public int? AttributeId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a 'shared value'. If so, the value entered will default to the value entered for first person registered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [common value]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSharedValue { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is only for administrative, and not shown in the public form
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is internal; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsInternal { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show current value].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show current value]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowCurrentValue { get; private set; }

        /// <summary>
        /// Gets or sets the Pre-HTML.
        /// </summary>
        /// <value>
        /// The pre text.
        /// </value>
        [DataMember]
        public string PreText { get; private set; }

        /// <summary>
        /// Gets or sets the Post-HTML.
        /// </summary>
        /// <value>
        /// The post text.
        /// </value>
        [DataMember]
        public string PostText { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is grid field.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grid field; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsGridField { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field should be shown on a waitlist.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the field should be shown on a waitlist; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowOnWaitlist { get; private set; }

        /// <summary>
        /// JSON Serialized <see cref="Rock.Model.RegistrationTemplateFormField.FieldVisibilityRules" />
        /// </summary>
        /// <value>
        /// The field visibility rules json. 
        /// </value>
        [DataMember]
        public string FieldVisibilityRulesJSON { get; private set; }

        #endregion Matched Entity Properties

        #region Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var registrationTemplateFormField = entity as RegistrationTemplateFormField;
            if ( registrationTemplateFormField == null )
            {
                return;
            };

            RegistrationTemplateFormId = registrationTemplateFormField.RegistrationTemplateFormId;
            FieldSource = registrationTemplateFormField.FieldSource;
            PersonFieldType = registrationTemplateFormField.PersonFieldType;
            AttributeId = registrationTemplateFormField.AttributeId;
            IsSharedValue = registrationTemplateFormField.IsSharedValue;
            IsInternal = registrationTemplateFormField.IsInternal;
            ShowCurrentValue = registrationTemplateFormField.ShowCurrentValue;
            PreText = registrationTemplateFormField.PreText;
            PostText = registrationTemplateFormField.PostText;
            IsGridField = registrationTemplateFormField.IsGridField;
            IsRequired = registrationTemplateFormField.IsRequired;
            Order = registrationTemplateFormField.Order;
            ShowOnWaitlist = registrationTemplateFormField.ShowOnWaitlist;
            FieldVisibilityRulesJSON = registrationTemplateFormField.FieldVisibilityRulesJSON;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( FieldSource == RegistrationFieldSource.PersonField )
            {
                return PersonFieldType.ConvertToString();
            }

            return base.ToString();
        }

        #endregion

    }
}