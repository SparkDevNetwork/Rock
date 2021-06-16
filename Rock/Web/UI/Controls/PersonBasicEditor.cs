﻿// <copyright>
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A general purpose editor for editing basic person information 
    /// </summary>
    public class PersonBasicEditor : CompositeControl, IHasValidationGroup
    {
        #region ViewStateKey

        private static class ViewStateKey
        {
            public const string PersonLabelPrefix = "PersonLabelPrefix";
            public const string ShowInColumns = "ShowInColumns";
            public const string RequireGender = "RequireGender";
        }

        #endregion

        #region Controls

        private DynamicPlaceholder _phControls;
        private DynamicControlsPanel _pnlRow;
        private DynamicControlsPanel _pnlCol1;
        private DynamicControlsPanel _pnlCol2;
        private DynamicControlsPanel _pnlCol3;

        private HiddenField _hfPersonId;
        private DefinedValuePicker _dvpPersonTitle;
        private RockTextBox _tbPersonFirstName;
        private RockTextBox _tbPersonLastName;
        private DefinedValuePicker _dvpPersonSuffix;
        private DefinedValuePicker _dvpPersonConnectionStatus;
        private RockRadioButtonList _rblPersonRole;
        private RockRadioButtonList _rblPersonGender;
        private BirthdayPicker _bdpPersonBirthDate;
        private GradePicker _ddlGradePicker;
        private DefinedValuePicker _dvpPersonMaritalStatus;
        private EmailBox _ebPersonEmail;
        private PhoneNumberBox _pnbMobilePhoneNumber;

        #endregion Controls

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [show title].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show title]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowTitle
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonTitle.Visible;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonTitle.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show suffix].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show suffix]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSuffix
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonSuffix.Visible;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonSuffix.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show grade].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show grade]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowGrade
        {
            get
            {
                EnsureChildControls();
                return _ddlGradePicker.Visible;
            }

            set
            {
                EnsureChildControls();
                _ddlGradePicker.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show birthdate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show birthdate]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowBirthdate
        {
            get
            {
                EnsureChildControls();
                return _bdpPersonBirthDate.Visible;
            }

            set
            {
                EnsureChildControls();
                _bdpPersonBirthDate.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require birthdate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require birthdate]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireBirthdate
        {
            get
            {
                EnsureChildControls();
                return _bdpPersonBirthDate.Required;
            }

            set
            {
                EnsureChildControls();
                _bdpPersonBirthDate.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show email].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show email]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowEmail
        {
            get
            {
                EnsureChildControls();
                return _ebPersonEmail.Visible;
            }

            set
            {
                EnsureChildControls();
                _ebPersonEmail.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require email].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require email]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireEmail
        {
            get
            {
                EnsureChildControls();
                return _ebPersonEmail.Required;
            }

            set
            {
                EnsureChildControls();
                _ebPersonEmail.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show mobile phone].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show mobile phone]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMobilePhone
        {
            get
            {
                EnsureChildControls();
                return _pnbMobilePhoneNumber.Visible;
            }

            set
            {
                EnsureChildControls();
                _pnbMobilePhoneNumber.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require mobile phone].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require mobile phone]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireMobilePhone
        {
            get
            {
                EnsureChildControls();
                return _pnbMobilePhoneNumber.Required;
            }

            set
            {
                EnsureChildControls();
                _pnbMobilePhoneNumber.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show person role].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show person role]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPersonRole
        {
            get
            {
                EnsureChildControls();
                return _rblPersonRole.Visible;
            }

            set
            {
                EnsureChildControls();
                _rblPersonRole.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show connection status].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show connection status]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowConnectionStatus
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonConnectionStatus.Visible;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonConnectionStatus.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show marital status].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show marital status]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMaritalStatus
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonMaritalStatus.Visible;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonMaritalStatus.Visible = value;
            }
        }

        /// <summary>
        /// If Required, the "Unknown" option won't be displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require gender]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGender
        {
            get
            {
                return ViewState[ViewStateKey.RequireGender] as bool? ?? false;
            }

            set
            {
                ViewState[ViewStateKey.RequireGender] = value;

                var listItemUnknown = _rblPersonGender.Items.OfType<ListItem>().FirstOrDefault( x => x.Value == Gender.Unknown.ConvertToInt().ToString() );

                if ( this.RequireGender )
                {
                    if ( listItemUnknown != null )
                    {
                        _rblPersonGender.Items.Remove( listItemUnknown );
                    }
                }
                else
                {
                    if ( listItemUnknown == null )
                    {
                        _rblPersonGender.Items.Add( new ListItem( Gender.Unknown.ConvertToString(), Gender.Unknown.ConvertToInt().ToString() ) );
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show gender].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show gender]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowGender
        {
            get
            {
                EnsureChildControls();
                return _rblPersonGender.Visible;
            }

            set
            {
                EnsureChildControls();
                _rblPersonGender.Visible = value;
            }
        }

        /// <summary>
        /// Gets the Person Id of the <see cref="Person"/> record that was passed to <see cref="SetFromPerson"/>. This will be null if this is a new person
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId
        {
            get
            {
                EnsureChildControls();
                var selectedPersonId = _hfPersonId.Value.AsInteger();
                if ( selectedPersonId > 0 )
                {
                    return selectedPersonId;
                }
                else
                {
                    return null;
                }
            }

            private set
            {
                EnsureChildControls();
                _hfPersonId.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName
        {
            get
            {
                EnsureChildControls();
                return _tbPersonFirstName.Text;
            }

            set
            {
                EnsureChildControls();
                _tbPersonFirstName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName
        {
            get
            {
                EnsureChildControls();
                return _tbPersonLastName.Text;
            }

            set
            {
                EnsureChildControls();
                _tbPersonLastName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the person title value identifier.
        /// </summary>
        /// <value>
        /// The person title value identifier.
        /// </value>
        public int? PersonTitleValueId
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonTitle.SelectedDefinedValueId;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonTitle.SelectedDefinedValueId = value;
            }
        }

        /// <summary>
        /// Gets or sets the person suffix value identifier.
        /// </summary>
        /// <value>
        /// The person suffix value identifier.
        /// </value>
        public int? PersonSuffixValueId
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonSuffix.SelectedDefinedValueId;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonSuffix.SelectedDefinedValueId = value;
            }
        }

        /// <summary>
        /// Gets or sets the person marital status value identifier.
        /// </summary>
        /// <value>
        /// The person marital status value identifier.
        /// </value>
        public int? PersonMaritalStatusValueId
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonMaritalStatus.SelectedDefinedValueId;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonMaritalStatus.SelectedDefinedValueId = value;
            }
        }

        /// <summary>
        /// Gets or sets the person grade offset.
        /// </summary>
        /// <value>
        /// The person grade offset.
        /// </value>
        public int? PersonGradeOffset
        {
            get
            {
                EnsureChildControls();
                return _ddlGradePicker.SelectedGradeOffset;
            }

            set
            {
                EnsureChildControls();
                _ddlGradePicker.SelectedGradeOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the person group role identifier. (Adult or Child)
        /// </summary>
        /// <value>
        /// The person group role identifier.
        /// </value>
        public int PersonGroupRoleId
        {
            get
            {
                EnsureChildControls();
                var selectedRoleId = _rblPersonRole.SelectedValue.AsInteger();
                if ( selectedRoleId == 0 )
                {
                    var adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
                    return adultRoleId;
                }
                else
                {
                    return selectedRoleId;
                }
            }

            set
            {
                EnsureChildControls();
                _rblPersonRole.SetValue( value );
            }
        }

        /// <summary>
        /// Gets or sets the person connection status value identifier.
        /// </summary>
        /// <value>
        /// The person connection status value identifier.
        /// </value>
        public int? PersonConnectionStatusValueId
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonConnectionStatus.SelectedDefinedValueId;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonConnectionStatus.SelectedDefinedValueId = value;
            }
        }

        /// <summary>
        /// Gets or sets the person gender.
        /// </summary>
        /// <value>
        /// The person gender.
        /// </value>
        public Gender? PersonGender
        {
            get
            {
                EnsureChildControls();
                return _rblPersonGender.SelectedValueAsEnumOrNull<Gender>();
            }

            set
            {
                EnsureChildControls();
                _rblPersonGender.SetValue( ( int ) value );
            }
        }

        /// <summary>
        /// Gets or sets the person birth date.
        /// </summary>
        /// <value>
        /// The person birth date.
        /// </value>
        public DateTime? PersonBirthDate
        {
            get
            {
                EnsureChildControls();
                return _bdpPersonBirthDate.SelectedDate;
            }

            set
            {
                EnsureChildControls();
                _bdpPersonBirthDate.SelectedDate = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [person birth date is valid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person birth date is valid]; otherwise, <c>false</c>.
        /// </value>
        public bool PersonBirthDateIsValid
        {
            get
            {
                EnsureChildControls();
                return _bdpPersonBirthDate.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email
        {
            get
            {
                EnsureChildControls();
                return _ebPersonEmail.Text;
            }

            set
            {
                EnsureChildControls();
                _ebPersonEmail.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the mobile phone number.
        /// </summary>
        /// <value>
        /// The mobile phone number.
        /// </value>
        public string MobilePhoneNumber
        {
            get
            {
                EnsureChildControls();
                return _pnbMobilePhoneNumber.Number;
            }

            set
            {
                EnsureChildControls();
                _pnbMobilePhoneNumber.Number = value;
            }
        }

        private string MobilePhoneCountryCode
        {
            get
            {
                EnsureChildControls();
                return _pnbMobilePhoneNumber.CountryCode;
            }

            set
            {
                EnsureChildControls();
                _pnbMobilePhoneNumber.CountryCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup { get; set; }

        /// <summary>
        /// Gets or sets the label prefix. For example "Spouse", would label things as "Spouse First Name", "Spouse Last Name", etc
        /// </summary>
        /// <value>
        /// The label prefix.
        /// </value>
        public string PersonLabelPrefix
        {
            get
            {
                return ViewState[ViewStateKey.PersonLabelPrefix] as string;
            }

            set
            {
                ViewState[ViewStateKey.PersonLabelPrefix] = value;
                if ( ChildControlsCreated )
                {
                    UpdatePersonControlLabels();
                }
            }
        }

        /// <summary>
        /// Updates the person control labels.
        /// </summary>
        private void UpdatePersonControlLabels()
        {
            _dvpPersonTitle.Label = AddLabelPrefix( "Title" );
            _tbPersonFirstName.Label = AddLabelPrefix( "First Name" );
            _tbPersonLastName.Label = AddLabelPrefix( "Last Name" );
            _dvpPersonSuffix.Label = AddLabelPrefix( "Suffix" );
            _dvpPersonConnectionStatus.Label = AddLabelPrefix( "Connection Status" );
            _rblPersonRole.Label = AddLabelPrefix( "Role" );
            _rblPersonGender.Label = AddLabelPrefix( "Gender" );
            _bdpPersonBirthDate.Label = AddLabelPrefix( "Birthdate" );
            _ddlGradePicker.Label = AddLabelPrefix( "Grade" );
            _dvpPersonMaritalStatus.Label = AddLabelPrefix( "Marital Status" );
            _ebPersonEmail.Label = AddLabelPrefix( "Email" );
            _pnbMobilePhoneNumber.Label = AddLabelPrefix( "Mobile Phone" );
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show in columns] (defaults to true)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in columns]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInColumns
        {
            get
            {
                return ViewState[ViewStateKey.ShowInColumns] as bool? ?? true;
            }

            set
            {
                if ( value != ShowInColumns )
                {
                    ViewState[ViewStateKey.ShowInColumns] = value;
                    if ( ChildControlsCreated )
                    {
                        // if child controls were already created, we'll have to move re-arrange the person controls
                        ArrangePersonControls( value );
                    }
                }
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _phControls = new DynamicPlaceholder { ID = "phControls" };
            this.Controls.Add( _phControls );

            _pnlRow = new DynamicControlsPanel { ID = "pnlRow", CssClass = "row" };
            _pnlCol1 = new DynamicControlsPanel { ID = "pnlCol1", CssClass = "col-sm-4" };
            _pnlCol2 = new DynamicControlsPanel { ID = "pnlCol2", CssClass = "col-sm-4" };
            _pnlCol3 = new DynamicControlsPanel { ID = "pnlCol3", CssClass = "col-sm-4" };
            _phControls.Controls.Add( _pnlRow );
            _pnlRow.Controls.Add( _pnlCol1 );
            _pnlRow.Controls.Add( _pnlCol2 );
            _pnlRow.Controls.Add( _pnlCol3 );


            _hfPersonId = new HiddenField
            {
                ID = "_hfPersonId"
            };

            _phControls.Controls.Add( _hfPersonId );

            _dvpPersonTitle = new DefinedValuePicker
            {
                ID = "_dvpPersonTitle",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ),
                Label = "Title",
                CssClass = "input-width-md",
                ValidationGroup = ValidationGroup
            };

            _tbPersonFirstName = new RockTextBox
            {
                ID = "tbPersonFirstName",
                Label = "First Name",
                Required = true,
                AutoCompleteType = AutoCompleteType.None,
                ValidationGroup = ValidationGroup
            };

            _tbPersonLastName = new RockTextBox
            {
                ID = "tbPersonLastName",
                Label = "Last Name",
                Required = true,
                AutoCompleteType = AutoCompleteType.None,
                ValidationGroup = ValidationGroup
            };

            _dvpPersonSuffix = new DefinedValuePicker
            {
                ID = "dvpPersonSuffix",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ),
                Label = "Suffix",
                CssClass = "input-width-md",
                ValidationGroup = ValidationGroup
            };

            // Have Email and PhoneNumber hidden by default
            _ebPersonEmail = new EmailBox
            {
                ID = "ebPersonEmail",
                Label = "Email",
                ValidationGroup = ValidationGroup,
                Visible = false
            };

            _pnbMobilePhoneNumber = new PhoneNumberBox
            {
                Label = "Mobile Phone",
                ID = "pnbMobilePhoneNumber",
                Visible = false
            };

            _dvpPersonConnectionStatus = new DefinedValuePicker
            {
                ID = "dvpPersonConnectionStatus",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ),
                Label = "Connection Status",
                Required = true,
                ValidationGroup = ValidationGroup
            };

            _rblPersonRole = new RockRadioButtonList
            {
                ID = "rblPersonRole",
                Label = "Role",
                RepeatDirection = RepeatDirection.Horizontal,
                DataTextField = "Name",
                DataValueField = "Id",
                Required = true,
                ValidationGroup = ValidationGroup
            };

            _rblPersonGender = new RockRadioButtonList
            {
                ID = "rblPersonGender",
                Label = "Gender",
                RepeatDirection = RepeatDirection.Horizontal,
                Required = true,
                ValidationGroup = ValidationGroup
            };

            _bdpPersonBirthDate = new BirthdayPicker
            {
                ID = "bdpPersonBirthDate",
                Label = "Birthdate",
                ValidationGroup = ValidationGroup
            };

            _ddlGradePicker = new GradePicker
            {
                ID = "ddlGradePicker",
                Label = "Grade",
                UseAbbreviation = true,
                UseGradeOffsetAsValue = true,
                ValidationGroup = ValidationGroup
            };

            _dvpPersonMaritalStatus = new DefinedValuePicker
            {
                ID = "dvpPersonMaritalStatus",
                Label = "Marital Status",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ),
                ValidationGroup = ValidationGroup
            };

            var groupType = GroupTypeCache.GetFamilyGroupType();
            _rblPersonRole.DataSource = groupType.Roles.OrderBy( r => r.Order ).ToList();
            _rblPersonRole.DataBind();

            _rblPersonGender.Items.Clear();
            _rblPersonGender.Items.Add( new ListItem( Gender.Male.ConvertToString(), Gender.Male.ConvertToInt().ToString() ) );
            _rblPersonGender.Items.Add( new ListItem( Gender.Female.ConvertToString(), Gender.Female.ConvertToInt().ToString() ) );
            _rblPersonGender.Items.Add( new ListItem( Gender.Unknown.ConvertToString(), Gender.Unknown.ConvertToInt().ToString() ) );

            ArrangePersonControls( this.ShowInColumns );
            UpdatePersonControlLabels();
        }

        /// <summary>
        /// Arranges the person controls based on the ShowInColumns setting
        /// </summary>
        /// <param name="showInColumns">if set to <c>true</c> [show in columns].</param>
        private void ArrangePersonControls( bool showInColumns )
        {
            _dvpPersonTitle.Parent?.Controls.Remove( _dvpPersonTitle );
            _tbPersonFirstName.Parent?.Controls.Remove( _tbPersonFirstName );
            _tbPersonLastName.Parent?.Controls.Remove( _tbPersonLastName );
            _dvpPersonSuffix.Parent?.Controls.Remove( _dvpPersonSuffix );
            _ebPersonEmail.Parent?.Controls.Remove( _ebPersonEmail );
            _pnbMobilePhoneNumber.Parent?.Controls.Remove( _pnbMobilePhoneNumber );
            _bdpPersonBirthDate.Parent?.Controls.Remove( _bdpPersonBirthDate );
            _rblPersonGender.Parent?.Controls.Remove( _rblPersonGender );
            _rblPersonRole.Parent?.Controls.Remove( _rblPersonRole );
            _ddlGradePicker.Parent?.Controls.Remove( _ddlGradePicker );
            _dvpPersonMaritalStatus.Parent?.Controls.Remove( _dvpPersonMaritalStatus );

            if ( showInColumns )
            {
                _pnlCol1.Controls.Add( _dvpPersonTitle );
                _pnlCol1.Controls.Add( _tbPersonFirstName );
                _pnlCol1.Controls.Add( _tbPersonLastName );
                _pnlCol1.Controls.Add( _dvpPersonSuffix );
                _pnlCol1.Controls.Add( _ebPersonEmail );
                _pnlCol1.Controls.Add( _pnbMobilePhoneNumber );

                _pnlCol2.Controls.Add( _dvpPersonConnectionStatus );
                _pnlCol2.Controls.Add( _rblPersonRole );
                _pnlCol2.Controls.Add( _rblPersonGender );

                _pnlCol3.Controls.Add( _bdpPersonBirthDate );
                _pnlCol3.Controls.Add( _ddlGradePicker );
                _pnlCol3.Controls.Add( _dvpPersonMaritalStatus );
            }
            else
            {
                _phControls.Controls.Add( _dvpPersonTitle );
                _phControls.Controls.Add( _tbPersonFirstName );
                _phControls.Controls.Add( _tbPersonLastName );
                _phControls.Controls.Add( _dvpPersonSuffix );
                _phControls.Controls.Add( _ebPersonEmail );
                _phControls.Controls.Add( _pnbMobilePhoneNumber );
                _phControls.Controls.Add( _bdpPersonBirthDate );
                _phControls.Controls.Add( _rblPersonGender );
                _phControls.Controls.Add( _dvpPersonConnectionStatus );
                _phControls.Controls.Add( _rblPersonRole );
                _phControls.Controls.Add( _ddlGradePicker );
                _phControls.Controls.Add( _dvpPersonMaritalStatus );
            }
        }

        /// <summary>
        /// Adds the label prefix.
        /// </summary>
        /// <param name="labelText">The label text.</param>
        /// <returns></returns>
        private string AddLabelPrefix( string labelText )
        {
            if ( PersonLabelPrefix.IsNullOrWhiteSpace() )
            {
                return labelText;
            }

            return $"{PersonLabelPrefix} {labelText}";
        }

        /// <summary>
        /// Updates the person fields based on what the values in the PersonBasicEditor are
        /// (Changes are not saved to the database.)
        /// </summary>
        /// <param name="person">The person.</param>
        [Obsolete( "Use UpdatePerson(Person,RockContext) instead" )]
        [RockObsolete( "1.12" )]
        public void UpdatePerson( Person person )
        {
            UpdatePerson( person, new RockContext() );
        }


        /// <summary>
        /// 
        /// </summary>
        public class PersonEditorException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonEditorException"/> class.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public PersonEditorException( string message )
                : base( message )
            {
            }
        }

        /// <summary>
        /// Updates the person fields based on what the values in the PersonBasicEditor are.
        /// (Changes are not saved to the database.)
        /// </summary>
        /// <param name="person">The new person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public void UpdatePerson( Person person, RockContext rockContext )
        {
            if ( this.PersonId > 0 && this.PersonId != person.Id )
            {
                throw new PersonEditorException( "UpdatePerson must use the same person that was used in SetFromPerson." );
            }

            if ( ShowTitle )
            {
                person.TitleValueId = this.PersonTitleValueId;
            }

            person.FirstName = this.FirstName;
            person.NickName = this.FirstName;
            person.LastName = this.LastName;

            if ( ShowSuffix )
            {
                person.SuffixValueId = this.PersonSuffixValueId;
            }

            if ( this.PersonGender.HasValue )
            {
                person.Gender = this.PersonGender.Value;
            }

            if ( ShowMaritalStatus )
            {
                person.MaritalStatusValueId = this.PersonMaritalStatusValueId;
            }

            if ( ShowBirthdate )
            {
                person.SetBirthDate( this.PersonBirthDate );
            }

            if ( ShowGrade )
            {
                person.GradeOffset = this.PersonGradeOffset;
            }

            if ( ShowConnectionStatus )
            {
                person.ConnectionStatusValueId = this.PersonConnectionStatusValueId;
            }

            if ( ShowEmail )
            {
                person.Email = this.Email;
            }

            if ( ShowMobilePhone )
            {
                var existingMobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

                var numberTypeMobile = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                var messagingEnabled = existingMobilePhone?.IsMessagingEnabled ?? true;
                var isUnlisted = existingMobilePhone?.IsUnlisted ?? false;
                person.UpdatePhoneNumber( numberTypeMobile.Id, MobilePhoneCountryCode, MobilePhoneNumber, messagingEnabled, isUnlisted, rockContext );
            }
        }

        /// <summary>
        /// Updates the PersonEditor values based on the specified person
        /// </summary>
        /// <param name="person">The person.</param>
        public void SetFromPerson( Person person )
        {
            // if a null person is specified, use whatever the defaults are for a new Person object
            person = person ?? new Person();
            this.PersonTitleValueId = person.TitleValueId;
            this.PersonId = person.Id;
            this.FirstName = person.FirstName;
            this.FirstName = person.NickName;
            this.LastName = person.LastName;
            this.PersonSuffixValueId = person.SuffixValueId;
            this.PersonGender = person.Gender;
            this.PersonMaritalStatusValueId = person.MaritalStatusValueId;
            this.PersonBirthDate = person.BirthDate;
            this.PersonGradeOffset = person.GradeOffset;

            if ( person.AgeClassification == AgeClassification.Child )
            {
                var childRoleId = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;
                this.PersonGroupRoleId = childRoleId;
            }
            else
            {
                var adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
                this.PersonGroupRoleId = adultRoleId;
            }

            this.PersonConnectionStatusValueId = person.ConnectionStatusValueId;
            this.Email = person.Email;

            var existingMobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            this.MobilePhoneNumber = existingMobilePhone?.NumberFormatted;
            this.MobilePhoneCountryCode = existingMobilePhone?.CountryCode;
        }

        /// <summary>
        /// Gets the validation results.
        /// </summary>
        /// <value>
        /// The validation results.
        /// </value>
        public ValidationResult[] ValidationResults { get; private set; } = new ValidationResult[0];

        /// <summary>
        /// Returns true if the edited values are valid, otherwise returns false and populates <see cref="ValidationResults"/>
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            var validationResults = new List<ValidationResult>();
            bool isValid = true;

            DateTime? birthdate = this.PersonBirthDate;
            if ( !this.PersonBirthDateIsValid )
            {
                validationResults.Add( new ValidationResult( "Birth date is not valid." ) );
                isValid = false;
            }

            this.ValidationResults = validationResults.ToArray();

            return isValid;
        }

        #endregion Events
    }
}
