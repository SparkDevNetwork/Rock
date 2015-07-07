// <copyright>
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "RegistrationTemplateFormField" )]
    [DataContract]
    public partial class RegistrationTemplateFormField : Model<RegistrationTemplateFormField>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the registration template form identifier.
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
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        [DataMember]
        public int? AttributeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a 'shared value'. If so, the value entered will be auto set for each person on the registration.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [common value]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSharedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show current value].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show current value]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowCurrentValue { get; set; }

        /// <summary>
        /// Gets or sets the pre text.
        /// </summary>
        /// <value>
        /// The pre text.
        /// </value>
        [DataMember]
        public string PreText { get; set; }

        /// <summary>
        /// Gets or sets the post text.
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration template form.
        /// </summary>
        /// <value>
        /// The registration template form.
        /// </value>
        public virtual RegistrationTemplateForm RegistrationTemplateForm { get; set; }

        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        [DataMember]
        public virtual Attribute Attribute { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( FieldSource == RegistrationFieldSource.PersonField )
            {
                return PersonFieldType.ConvertToString();
            }

            if ( Attribute != null )
            {
                return Attribute.Name;
            }

            return base.ToString();
        }

        #endregion

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

    #region Enumerations

    /// <summary>
    /// The entity that attribute applies to
    /// </summary>
    public enum RegistrationFieldSource
    {
        /// <summary>
        /// Person attribute
        /// </summary>
        PersonField = 0,

        /// <summary>
        /// Person attribute
        /// </summary>
        PersonAttribute = 1,

        /// <summary>
        /// Group Member attribute
        /// </summary>
        GroupMemberAttribute = 2,

        /// <summary>
        /// Registration attribute
        /// </summary>
        RegistrationAttribute = 4
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RegistrationPersonFieldType
    {
        /// <summary>
        /// The first name
        /// </summary>
        FirstName = 0,

        /// <summary>
        /// The last name
        /// </summary>
        LastName = 1,

        /// <summary>
        /// The person's campus
        /// </summary>
        Campus = 2,

        /// <summary>
        /// The Address
        /// </summary>
        Address = 3,

        /// <summary>
        /// The email
        /// </summary>
        Email = 4,

        /// <summary>
        /// The birthdate
        /// </summary>
        Birthdate = 5,

        /// <summary>
        /// The gender
        /// </summary>
        Gender = 6,

        /// <summary>
        /// The marital status
        /// </summary>
        MaritalStatus = 7,

        /// <summary>
        /// The mobile phone
        /// </summary>
        MobilePhone = 8,

        /// <summary>
        /// The home phone
        /// </summary>
        HomePhone = 9,

        /// <summary>
        /// The work phone
        /// </summary>
        WorkPhone = 10,
    }

    #endregion

}
