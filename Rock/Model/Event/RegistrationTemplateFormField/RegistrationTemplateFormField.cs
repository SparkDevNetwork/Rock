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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Form Field for Registrant Fields
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "RegistrationTemplateFormField" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "A773CAA2-2211-416B-BDD7-D907085B4441")]
    public partial class RegistrationTemplateFormField : Model<RegistrationTemplateFormField>, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationTemplateForm"/> identifier.
        /// </summary>
        /// <value>
        /// The registration template form identifier.
        /// </value>
        [DataMember]
        public int RegistrationTemplateFormId { get; set; }

        /// <summary>
        /// Gets or sets the source of the field value.
        /// </summary>
        /// <value>
        /// The applies to.
        /// </value>
        [DataMember]
        public RegistrationFieldSource FieldSource { get; set; }

        /// <summary>
        /// Gets or sets the type of the person field.
        /// </summary>
        /// <value>
        /// The type of the person field.
        /// </value>
        [DataMember]
        public RegistrationPersonFieldType PersonFieldType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Attribute"/> identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        [DataMember]
        public int? AttributeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a 'shared value'. If so, the value entered will default to the value entered for first person registered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [common value]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSharedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is only for administrative, and not shown in the public form
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is internal; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsInternal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show current value].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show current value]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowCurrentValue { get; set; }

        /// <summary>
        /// Gets or sets the Pre-HTML.
        /// </summary>
        /// <value>
        /// The pre text.
        /// </value>
        [DataMember]
        public string PreText { get; set; }

        /// <summary>
        /// Gets or sets the Post-HTML.
        /// </summary>
        /// <value>
        /// The post text.
        /// </value>
        [DataMember]
        public string PostText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is grid field.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is grid field; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsGridField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field should be shown on a waitlist.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the field should be shown on a waitlist; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowOnWaitlist { get; set; }

        /// <summary>
        /// JSON Serialized <see cref="FieldVisibilityRules"/>
        /// </summary>
        /// <value>
        /// The field visibility rules json.
        /// </value>
        [DataMember]
        public string FieldVisibilityRulesJSON
        {
            get
            {
                return FieldVisibilityRules?.ToJson();
            }

            set
            {
                Field.FieldVisibilityRules rules = null;
                if ( value.IsNotNullOrWhiteSpace() )
                {
                    rules = value.FromJsonOrNull<Rock.Field.FieldVisibilityRules>();
                    if ( rules == null )
                    {
                        // if can't be deserialized as FieldVisibilityRules, it might have been serialized as an array from an earlier version
                        var rulesList = value.FromJsonOrNull<List<Field.FieldVisibilityRule>>();
                        if ( rulesList != null )
                        {
                            rules = new Field.FieldVisibilityRules();
                            rules.RuleList.AddRange( rulesList );
                        }
                    }
                }

                this.FieldVisibilityRules = rules ?? new Field.FieldVisibilityRules();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether editing the field is restricted when a value is already on the person's record.
        /// </summary>
        /// <value>
        ///   <c>true</c> if editing the field is restricted when a value is already on the person's record; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLockedIfValuesExist { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationTemplateForm"/>.
        /// </summary>
        /// <value>
        /// The registration template form.
        /// </value>
        [LavaVisible]
        public virtual RegistrationTemplateForm RegistrationTemplateForm { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Attribute"/>.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        [DataMember]
        public virtual Attribute Attribute { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationTemplateFormAttributeConfiguration : EntityTypeConfiguration<RegistrationTemplateFormField>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationTemplateFormAttributeConfiguration"/> class.
        /// </summary>
        public RegistrationTemplateFormAttributeConfiguration()
        {
            this.HasRequired( a => a.RegistrationTemplateForm ).WithMany( t => t.Fields ).HasForeignKey( i => i.RegistrationTemplateFormId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Attribute ).WithMany().HasForeignKey( a => a.AttributeId ).WillCascadeOnDelete( false );
        }
    }

#endregion
}
