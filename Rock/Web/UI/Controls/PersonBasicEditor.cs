using System;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A general purpose editor for editing basic person information 
    /// </summary>
    public class PersonBasicEditor : CompositeControl, IHasValidationGroup
    {
        #region Controls

        private DefinedValuePicker _dvpPersonTitle;
        private RockTextBox _tbPersonFirstName;
        private RockTextBox _tbPersonLastName;
        private DefinedValuePicker _dvpPersonSuffix;
        private DefinedValuePicker _dvpPersonConnectionStatus;
        private RockRadioButtonList _rblPersonRole;
        private RockRadioButtonList _rblPersonGender;
        private DatePicker _dpPersonBirthDate;
        private GradePicker _ddlGradePicker;
        private DefinedValuePicker _dvpPersonMaritalStatus;

        #endregion Controls

        #region Properties

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
                return _ddlGradePicker.SelectedValue.AsIntegerOrNull();
            }

            set
            {
                EnsureChildControls();
                _ddlGradePicker.SetValue( value );
            }
        }

        /// <summary>
        /// Gets or sets the person group role identifier.
        /// </summary>
        /// <value>
        /// The person group role identifier.
        /// </value>
        public int PersonGroupRoleId
        {
            get
            {
                EnsureChildControls();
                return _rblPersonRole.SelectedValue.AsInteger();
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
        public Gender PersonGender
        {
            get
            {
                EnsureChildControls();
                return _rblPersonGender.SelectedValueAsEnum<Gender>();
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
                return _dpPersonBirthDate.SelectedDate;
            }

            set
            {
                EnsureChildControls();
                _dpPersonBirthDate.SelectedDate = value;
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
                return _dpPersonBirthDate.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup { get; set; }

        #endregion Properties

        #region Events

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Panel pnlRow = new Panel
            {
                ID = "pnlRow",
                CssClass = "row"
            };

            this.Controls.Add( pnlRow );

            Panel pnlCol1 = new Panel
            {
                ID = "pnlCol1",
                CssClass = "col-sm-4"
            };

            pnlRow.Controls.Add( pnlCol1 );
            _dvpPersonTitle = new DefinedValuePicker
            {
                ID = "_dvpPersonTitle",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ),
                Label = "Title",
                CssClass = "input-width-md",
                ValidationGroup = ValidationGroup
            };

            pnlCol1.Controls.Add( _dvpPersonTitle );

            _tbPersonFirstName = new RockTextBox
            {
                ID = "tbPersonFirstName",
                Label = "First Name",
                Required = true,
                AutoCompleteType = AutoCompleteType.None,
                ValidationGroup = ValidationGroup
            };

            pnlCol1.Controls.Add( _tbPersonFirstName );

            _tbPersonLastName = new RockTextBox
            {
                ID = "tbPersonLastName",
                Label = "Last Name",
                Required = true,
                AutoCompleteType = AutoCompleteType.None,
                ValidationGroup = ValidationGroup
            };

            pnlCol1.Controls.Add( _tbPersonLastName );

            _dvpPersonSuffix = new DefinedValuePicker
            {
                ID = "dvpPersonSuffix",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ),
                Label = "Suffix",
                CssClass = "input-width-md",
                ValidationGroup = ValidationGroup
            };

            pnlCol1.Controls.Add( _dvpPersonSuffix );

            Panel pnlCol2 = new Panel
            {
                ID = "pnlCol2",
                CssClass = "col-sm-4"
            };

            pnlRow.Controls.Add( pnlCol2 );

            _dvpPersonConnectionStatus = new DefinedValuePicker
            {
                ID = "dvpPersonConnectionStatus",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ),
                Label = "Connection Status",
                Required = true,
                ValidationGroup = ValidationGroup
            };

            pnlCol2.Controls.Add( _dvpPersonConnectionStatus );

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

            pnlCol2.Controls.Add( _rblPersonRole );

            _rblPersonGender = new RockRadioButtonList
            {
                ID = "rblPersonGender",
                Label = "Gender",
                RepeatDirection = RepeatDirection.Horizontal,
                Required = true,
                ValidationGroup = ValidationGroup
            };

            pnlCol2.Controls.Add( _rblPersonGender );

            Panel pnlCol3 = new Panel { ID = "pnlCol3", CssClass = "col-sm-4" };
            pnlRow.Controls.Add( pnlCol3 );

            _dpPersonBirthDate = new DatePicker
            {
                ID = "dpPersonBirthDate",
                Label = "Birthdate",
                AllowFutureDateSelection = false,
                ForceParse = false,
                ValidationGroup = ValidationGroup
            };

            pnlCol3.Controls.Add( _dpPersonBirthDate );

            _ddlGradePicker = new GradePicker
            {
                ID = "ddlGradePicker",
                Label = "Grade",
                UseAbbreviation = true,
                UseGradeOffsetAsValue = true,
                ValidationGroup = ValidationGroup
            };

            pnlCol3.Controls.Add( _ddlGradePicker );

            _dvpPersonMaritalStatus = new DefinedValuePicker
            {
                ID = "_vpPersonMaritalStatus",
                Label = "Marital Status",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ),
                ValidationGroup = ValidationGroup
            };

            pnlCol3.Controls.Add( _dvpPersonMaritalStatus );

            var groupType = GroupTypeCache.GetFamilyGroupType();
            _rblPersonRole.DataSource = groupType.Roles.OrderBy( r => r.Order ).ToList();
            _rblPersonRole.DataBind();

            _rblPersonGender.Items.Clear();
            _rblPersonGender.Items.Add( new ListItem( Gender.Male.ConvertToString(), Gender.Male.ConvertToInt().ToString() ) );
            _rblPersonGender.Items.Add( new ListItem( Gender.Female.ConvertToString(), Gender.Female.ConvertToInt().ToString() ) );
            _rblPersonGender.Items.Add( new ListItem( Gender.Unknown.ConvertToString(), Gender.Unknown.ConvertToInt().ToString() ) );
        }

        #endregion Events
    }
}
