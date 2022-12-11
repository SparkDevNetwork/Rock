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

using System;
using System.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    public partial class RegistrationTemplateFormField
    {
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
                    var tbFirstName = new FirstNameTextBox
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

                    // Enable the middle name field if it is currently disabled but required and there is no value.
                    if ( !tbMiddleName.Enabled && tbMiddleName.Required && tbMiddleName.Text.IsNullOrWhiteSpace() )
                    {
                        tbMiddleName.Enabled = true;
                    }

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

                case RegistrationPersonFieldType.Race:
                    var rpRace = new RacePicker
                    {
                        ID = "rpRace",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup
                    };

                    if ( setValue && fieldValue != null )
                    {
                        var value = fieldValue.ToString().AsInteger();
                        rpRace.SetValue( value );
                    }

                    personFieldControl = rpRace;
                    break;

                case RegistrationPersonFieldType.Ethnicity:
                    var epEthnicity = new EthnicityPicker
                    {
                        ID = "epEthnicity",
                        Required = field.IsRequired,
                        ValidationGroup = validationGroup
                    };

                    if ( setValue && fieldValue != null )
                    {
                        var value = fieldValue.ToString().AsInteger();
                        epEthnicity.SetValue( value );
                    }

                    personFieldControl = epEthnicity;
                    break;
            }

            return personFieldControl;
        }
    }
}
