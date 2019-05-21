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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Web.UI;

using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    /// <summary>
    /// Form Field for Registrant Fields
    /// </summary>
    [RockDomain( "Event" )]
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the field visibility rules.
        /// </summary>
        /// <value>
        /// The field visibility rules.
        /// </value>
        [NotMapped]
        public virtual Rock.Field.FieldVisibilityRules FieldVisibilityRules { get; set; } = new Rock.Field.FieldVisibilityRules();

        /// <summary>
        /// Gets or sets the registration template form.
        /// </summary>
        /// <value>
        /// The registration template form.
        /// </value>
        [LavaInclude]
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

        /// <summary>
        /// Gets the person control for this field's <see cref="PersonFieldType"/>
        /// </summary>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="familyMemberSelected">if set to <c>true</c> [family member selected].</param>
        /// <param name="validationGroup">The validation group.</param>
        /// <returns></returns>
        public Control GetPersonControl( bool setValue, object fieldValue, bool familyMemberSelected, string validationGroup )
        {
            RegistrationTemplateFormField field = this;
            Control personFieldControl = null;

            switch ( field.PersonFieldType )
            {
                case RegistrationPersonFieldType.FirstName:
                    var tbFirstName = new RockTextBox
                    {
                        ID = "tbFirstName",
                        Label = "First Name",
                        Required = field.IsRequired,
                        CssClass = "js-first-name",
                        ValidationGroup = validationGroup,
                        Enabled = !familyMemberSelected,
                        Text = setValue && fieldValue != null ? fieldValue.ToString() : string.Empty
                    };

                    personFieldControl = tbFirstName;
                    break;

                case RegistrationPersonFieldType.LastName:
                    var tbLastName = new RockTextBox
                    {
                        ID = "tbLastName",
                        Label = "Last Name",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        Enabled = !familyMemberSelected,
                        Text = setValue && fieldValue != null ? fieldValue.ToString() : string.Empty
                    };

                    personFieldControl = tbLastName;
                    break;

                case RegistrationPersonFieldType.MiddleName:
                    var tbMiddleName = new RockTextBox
                    {
                        ID = "tbMiddleName",
                        Label = "Middle Name",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        Enabled = !familyMemberSelected,
                        Text = setValue && fieldValue != null ? fieldValue.ToString() : string.Empty
                    };

                    personFieldControl = tbMiddleName;
                    break;

                case RegistrationPersonFieldType.Campus:
                    var cpHomeCampus = new CampusPicker
                    {
                        ID = "cpHomeCampus",
                        Label = "Campus",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        Campuses = CampusCache.All( false ),
                        SelectedCampusId = setValue && fieldValue != null ? fieldValue.ToString().AsIntegerOrNull() : null
                    };

                    personFieldControl = cpHomeCampus;
                    break;

                case RegistrationPersonFieldType.Address:
                    var acAddress = new AddressControl
                    {
                        ID = "acAddress",
                        Label = "Address",
                        UseStateAbbreviation = true,
                        UseCountryAbbreviation = false,
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup
                    };

                    if ( setValue && fieldValue != null )
                    {
                        acAddress.SetValues( fieldValue as Location );
                    }

                    personFieldControl = acAddress;
                    break;

                case RegistrationPersonFieldType.Email:
                    var tbEmail = new EmailBox
                    {
                        ID = "tbEmail",
                        Label = "Email",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        Text = setValue && fieldValue != null ? fieldValue.ToString() : string.Empty
                    };

                    personFieldControl = tbEmail;
                    break;

                case RegistrationPersonFieldType.Birthdate:
                    var bpBirthday = new BirthdayPicker
                    {
                        ID = "bpBirthday",
                        Label = "Birthday",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        SelectedDate = setValue && fieldValue != null ? fieldValue as DateTime? : null
                    };

                    personFieldControl = bpBirthday;
                    break;

                case RegistrationPersonFieldType.Grade:
                    var gpGrade = new GradePicker
                    {
                        ID = "gpGrade",
                        Label = "Grade",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        UseAbbreviation = true,
                        UseGradeOffsetAsValue = true,
                        CssClass = "input-width-md"
                    };

                    personFieldControl = gpGrade;

                    if ( setValue && fieldValue != null )
                    {
                        var value = fieldValue.ToString().AsIntegerOrNull();
                        gpGrade.SetValue( Person.GradeOffsetFromGraduationYear( value ) );
                    }

                    break;

                case RegistrationPersonFieldType.Gender:
                    var ddlGender = new RockDropDownList
                    {
                        ID = "ddlGender",
                        Label = "Gender",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                    };

                    ddlGender.BindToEnum<Gender>( true, new Gender[1] { Gender.Unknown } );

                    personFieldControl = ddlGender;

                    if ( setValue && fieldValue != null )
                    {
                        var value = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                        ddlGender.SetValue( value.ConvertToInt() );
                    }

                    break;

                case RegistrationPersonFieldType.MaritalStatus:
                    var dvpMaritalStatus = new DefinedValuePicker
                    {
                        ID = "dvpMaritalStatus",
                        Label = "Marital Status",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup
                    };

                    dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
                    personFieldControl = dvpMaritalStatus;

                    if ( setValue && fieldValue != null )
                    {
                        var value = fieldValue.ToString().AsInteger();
                        dvpMaritalStatus.SetValue( value );
                    }

                    break;

                case RegistrationPersonFieldType.AnniversaryDate:
                    var dppAnniversaryDate = new DatePartsPicker
                    {
                        ID = "dppAnniversaryDate",
                        Label = "Anniversary Date",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        SelectedDate = setValue && fieldValue != null ? fieldValue as DateTime? : null
                    };

                    personFieldControl = dppAnniversaryDate;
                    break;

                case RegistrationPersonFieldType.MobilePhone:
                    var dvMobilePhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                    if ( dvMobilePhone == null )
                    {
                        break;
                    }

                    var ppMobile = new PhoneNumberBox
                    {
                        ID = "ppMobile",
                        Label = dvMobilePhone.Value,
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        CountryCode = PhoneNumber.DefaultCountryCode()
                    };

                    var mobilePhoneNumber = setValue && fieldValue != null ? fieldValue as PhoneNumber : null;
                    ppMobile.CountryCode = mobilePhoneNumber != null ? mobilePhoneNumber.CountryCode : string.Empty;
                    ppMobile.Number = mobilePhoneNumber != null ? mobilePhoneNumber.ToString() : string.Empty;

                    personFieldControl = ppMobile;
                    break;

                case RegistrationPersonFieldType.HomePhone:
                    var dvHomePhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                    if ( dvHomePhone == null )
                    {
                        break;
                    }

                    var ppHome = new PhoneNumberBox
                    {
                        ID = "ppHome",
                        Label = dvHomePhone.Value,
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        CountryCode = PhoneNumber.DefaultCountryCode()
                    };

                    var homePhoneNumber = setValue && fieldValue != null ? fieldValue as PhoneNumber : null;
                    ppHome.CountryCode = homePhoneNumber != null ? homePhoneNumber.CountryCode : string.Empty;
                    ppHome.Number = homePhoneNumber != null ? homePhoneNumber.ToString() : string.Empty;

                    personFieldControl = ppHome;
                    break;

                case RegistrationPersonFieldType.WorkPhone:
                    var dvWorkPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
                    if ( dvWorkPhone == null )
                    {
                        break;
                    }

                    var ppWork = new PhoneNumberBox
                    {
                        ID = "ppWork",
                        Label = dvWorkPhone.Value,
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup,
                        CountryCode = PhoneNumber.DefaultCountryCode()
                    };

                    var workPhoneNumber = setValue && fieldValue != null ? fieldValue as PhoneNumber : null;
                    ppWork.CountryCode = workPhoneNumber != null ? workPhoneNumber.CountryCode : string.Empty;
                    ppWork.Number = workPhoneNumber != null ? workPhoneNumber.ToString() : string.Empty;

                    personFieldControl = ppWork;
                    break;

                case RegistrationPersonFieldType.ConnectionStatus:
                    var dvpConnectionStatus = new DefinedValuePicker
                    {
                        ID = "dvpConnectionStatus",
                        Label = "Connection Status",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup
                    };

                    dvpConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ).Id;

                    if ( setValue && fieldValue != null )
                    {
                        var value = fieldValue.ToString().AsInteger();
                        dvpConnectionStatus.SetValue( value );
                    }

                    personFieldControl = dvpConnectionStatus;
                    break;
            }

            return personFieldControl;
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
        /// Registrant attribute
        /// </summary>
        RegistrantAttribute = 4,

        /// <summary>
        /// Registration attribute
        /// NOTE: Put obsolete Enums AFTER the one that replaces it so that enum.ConvertToString() returns the non-obsolete name
        /// </summary>
        [Obsolete( "Use RegistrantAttribute instead" )]
        [RockObsolete( "1.9" )]
        RegistrationAttribute = RegistrantAttribute,
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

        /// <summary>
        /// The grade
        /// </summary>
        Grade = 11,

        /// <summary>
        /// The connection status
        /// </summary>
        ConnectionStatus = 12,

        /// <summary>
        /// The middle name
        /// </summary>
        MiddleName = 13,

        /// <summary>
        /// The anniversary date
        /// </summary>
        AnniversaryDate = 14,
    }

    #endregion

}
