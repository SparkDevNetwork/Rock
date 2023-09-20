using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_lakepointe.Crm
{
    public partial class AnnualSurveyPersonInformation : System.Web.UI.UserControl
    {
        #region Fields
        RockContext _context; 
        #endregion

        #region Properties
        public Person CurrentPerson { get; set; }
        public GroupTypeRoleCache FamilyMemberRole { get; set; }
        public bool IsMemberOfFamily { get; set; }
        public bool IsActivelyAttending { get; set; }

        public bool FamilyMemberRoleIsEditable { get; set; }
        public List<int> PhoneNumberTypeLUIDs { get; set; }
        public string ValidationGroup { get; set; }

        public event EventHandler RemoveFamilyMember;
        private RockContext DBContext
        {
            get
            {
                if ( _context == null )
                {
                    _context = new RockContext();
                }
                return _context;
            }
            set
            {
                _context = value;
            }
        }

        #endregion

        protected void Page_Init( object sender, EventArgs e )
        {
            rblGender.BindToEnum<Gender>( false, new Gender[] { Gender.Unknown } );
            dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid(), DBContext ).Id;
            ddlEmailPreference.BindToEnum<EmailPreference>();

            if ( CurrentPerson.Id > 0 )
            {
                lName.Text = CurrentPerson.FullName;
                btnRemove.Visible = false;
            }
            else
            {
                lName.Text = "New Family Member";
                btnRemove.Visible = true;
            }
            if ( FamilyMemberRole != null )
            {
                bool isChild = FamilyMemberRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                lFamilyRole.Text = FamilyMemberRole.Name;
                pnlGrade.Visible = isChild;
                ddlGradePicker.Visible = isChild;
            }
            LoadGroupRoles();
            LoadValidators();

            //LoadPhoneNumbers( false );
        }

        protected void Page_Load( object sender, EventArgs e )
        {

        }

        protected void btnRemove_Click( object sender, EventArgs e )
        {
            OnRemoveFamilyMember(e);
        }
        #region Public Methods
        public void LoadPhoneNumbers( bool setvalues )
        {
            var phoneNumberTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).DefinedValues
                .Where( dv => PhoneNumberTypeLUIDs.Contains( dv.Id ) )
                .OrderBy( dv => dv.Order );

            phPhoneNumber.Controls.Clear();

            if ( phoneNumberTypes == null || phoneNumberTypes.Count() == 0 )
            {
                return;
            }

            foreach ( var phonetype in phoneNumberTypes )
            {
                PhoneNumber pn = CurrentPerson.GetPhoneNumber( phonetype.Guid );

                var div1 = new HtmlGenericControl( "div" );
                div1.AddCssClass( "row" );

                phPhoneNumber.Controls.Add( div1 );
                var divPhone = new HtmlGenericContainer( "div" );
                divPhone.AddCssClass( "col-md-4" );
                div1.Controls.Add( divPhone );

                var tbPhone = new PhoneNumberBox();
                tbPhone.ID = string.Format( "tbPhone_{0}", phonetype.Id );
                tbPhone.Label = String.Format( "{0} Phone", phonetype.Value );
                tbPhone.Required = false;
                divPhone.Controls.Add( tbPhone );

                if ( pn != null && setvalues )
                {
                    tbPhone.Number = pn.NumberFormatted;
                }

                if ( phonetype.Guid.Equals( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ) )
                {
                    var divAllowMessaging = new HtmlGenericControl( "div" );
                    divAllowMessaging.AddCssClass( "col-md-4" );
                    div1.Controls.Add( divAllowMessaging );
                    var cbAllowMessaging = new RockCheckBox();
                    cbAllowMessaging.ID = "cbAllowMessaging";
                    cbAllowMessaging.Label = "Allow Text Messages ";
                    divAllowMessaging.Controls.Add( cbAllowMessaging );

                    if ( pn != null && setvalues )
                    {
                        cbAllowMessaging.Checked = pn.IsMessagingEnabled;
                    }
                }
            }
        }
        public void LoadPerson( )
        {
            if ( CurrentPerson == null )
            {
                throw new Exception( "Person is required." );

            }

            IsActivelyAttending = true;
            IsMemberOfFamily = true;

            ddlGroupRole.Visible = true;
            LoadGroupRoles();
            ddlGroupRole.SelectedValue = FamilyMemberRole.Id.ToString();
            if ( FamilyMemberRoleIsEditable )
            {
                ddlGroupRole.Visible = true;
                LoadGroupRoles();
                if ( FamilyMemberRole != null )
                {
                    ddlGroupRole.SelectedValue = FamilyMemberRole.Id.ToString();
                }
                ddlGroupRole.Enabled = true;
            }
            else
            {
                ddlGroupRole.Enabled = false;
            }
            dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid(), DBContext ).Id;
            bool isChild = FamilyMemberRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

            rblGender.BindToEnum<Gender>( false, new Gender[] { Gender.Unknown }, true );
            ddlEmailPreference.BindToEnum<EmailPreference>();
            dpBirthDate.AllowFutureDateSelection = false;
            dpBirthDate.AllowPastDateSelection = true;

            pnlGrade.Visible = isChild;
            ddlGradePicker.Visible = isChild;



            lFamilyRole.Text = FamilyMemberRole.Name;

            hfPersonId.Value = CurrentPerson.Id.ToString();

            if ( CurrentPerson.Id > 0 )
            {
                lName.Text = CurrentPerson.FullName;
            }
            else
            {
                lName.Text = "New Family Member";
            }

            if ( CurrentPerson.MaritalStatusValueId.HasValue )
            {
                dvpMaritalStatus.SelectedDefinedValueId = CurrentPerson.MaritalStatusValueId;
            }


            imgPhoto.BinaryFileId = CurrentPerson.PhotoId;


            tbFirstName.Text = CurrentPerson.FirstName;
            tbNickName.Text = CurrentPerson.NickName;
            tbLastName.Text = CurrentPerson.LastName;

            if ( CurrentPerson.Gender != Gender.Unknown )
            {
                rblGender.SelectedValue = CurrentPerson.Gender.ConvertToInt().ToString();
            }

            dpBirthDate.SelectedDate = CurrentPerson.BirthDate;

            if (isChild )
            {
                ddlGradePicker.SelectedValue = CurrentPerson.GradeOffset.HasValue ? CurrentPerson.GradeOffset.ToString() : String.Empty;
            }

            tbEmail.Text = CurrentPerson.Email;
            ddlEmailPreference.SelectedValue = CurrentPerson.EmailPreference.ConvertToInt().ToString();

            //LoadPhoneNumbers( true );

        }

        private void LoadValidators()
        {
            
            var validationMessageFormat = "{0} {1} is required.";
            var personName = "";

            if ( CurrentPerson.Id > 0 )
            {
                personName = Rock.Lava.RockFilters.Possessive( CurrentPerson.NickName );
            }
            else
            {
                personName = "All Family Members";
            }

            tbFirstName.Required = true;
            tbFirstName.ValidationGroup = ValidationGroup;
            tbFirstName.RequiredErrorMessage = string.Format( validationMessageFormat, personName, tbFirstName.Label );

            tbLastName.Required = true;
            tbLastName.ValidationGroup = ValidationGroup;
            tbLastName.RequiredErrorMessage = string.Format( validationMessageFormat, personName, tbLastName.Label );

            dpBirthDate.Required = true;
            dpBirthDate.ValidationGroup = ValidationGroup;
            dpBirthDate.RequiredErrorMessage = string.Format( validationMessageFormat, personName, dpBirthDate.Label );

            ddlActive.Required = true;
            ddlActive.ValidationGroup = ValidationGroup;
            ddlActive.RequiredErrorMessage = string.Format( "Please indicate if {0} {1} actively attending Lake Pointe.", personName, CurrentPerson.Id > 0 ? "is" : "are" );

            ddlFamilyMember.Required = true;
            ddlFamilyMember.ValidationGroup = ValidationGroup;
            ddlFamilyMember.RequiredErrorMessage = string.Format( "Please indicate if {0} {1} of your immediate family.", personName, CurrentPerson.Id > 0 ? "is a member" : "are members" );

			dvpMaritalStatus.Required = true;
            dvpMaritalStatus.ValidationGroup = ValidationGroup;
            dvpMaritalStatus.RequiredErrorMessage = string.Format( validationMessageFormat, personName, dvpMaritalStatus.Label );

        }

        private void OnRemoveFamilyMember(EventArgs e)
        {
            EventHandler remove = RemoveFamilyMember;
            if ( remove != null )
            {
                remove( this, e );
            }
        }

        public void UpdatePerson()
        {
            var childRole = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Roles
                .Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() )
                .FirstOrDefault();

            FamilyMemberRole = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), DBContext ).Roles
                .FirstOrDefault( r => r.Id == ddlGroupRole.SelectedValueAsInt() );

            CurrentPerson.PhotoId = imgPhoto.BinaryFileId;
            CurrentPerson.FirstName = tbFirstName.Text.Trim();

            if ( !String.IsNullOrWhiteSpace( tbNickName.Text ) )
            {
                CurrentPerson.NickName = tbNickName.Text.Trim();
            }
            else
            {
                CurrentPerson.NickName = tbFirstName.Text.Trim();
            }

            CurrentPerson.LastName = tbLastName.Text.Trim();
            CurrentPerson.Gender = rblGender.SelectedValueAsEnum<Gender>( Gender.Unknown );
            CurrentPerson.SetBirthDate( dpBirthDate.SelectedDate );
            CurrentPerson.MaritalStatusValueId = dvpMaritalStatus.SelectedDefinedValueId;


            if ( FamilyMemberRole.Guid == childRole.Guid )
            {
                CurrentPerson.GradeOffset = ddlGradePicker.SelectedValueAsId();
            }
            else
            {
                CurrentPerson.GradeOffset = null;
            }

            CurrentPerson.Email = tbEmail.Text.Trim();
            if ( !String.IsNullOrWhiteSpace( tbEmail.Text ) )
            {
                CurrentPerson.IsEmailActive = true;
                CurrentPerson.EmailPreference = ddlEmailPreference.SelectedValueAsEnum<EmailPreference>();
            }
            else
            {
                CurrentPerson.IsEmailActive = false;
                CurrentPerson.EmailPreference = EmailPreference.DoNotEmail;
            }
            IsActivelyAttending = ddlActive.SelectedValue.AsBoolean( true );
            IsMemberOfFamily = ddlFamilyMember.SelectedValue.AsBoolean( true );

            UpdatePersonPhoneNumbers();

        }

        public void UpdatePersonPhoneNumbers()
        {

            var phoneNumberTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).DefinedValues
                .Where( dv => PhoneNumberTypeLUIDs.Contains( dv.Id ) )
                .OrderBy( dv => dv.Order );
                
            if ( phoneNumberTypes == null || phoneNumberTypes.Count() == 0  )
            {
                return;
            }


            foreach ( Control rowDiv in phPhoneNumber.Controls )
            {
                
                var sms = false;

                var phoneDiv = rowDiv.Controls[0];
                if ( phoneDiv == null || phoneDiv.Controls.Count == 0 )
                {
                    continue;
                }
                var tbNumber = (PhoneNumberBox)phoneDiv.Controls[0];
                var phoneTypeId = tbNumber.ID.Replace( "tbPhone_", "" ).AsInteger();
                var numberValue = PhoneNumber.CleanNumber( tbNumber.Text.Trim() );

                if ( rowDiv.Controls.Count > 1 )
                {
                    var cbSMS = (RockCheckBox)rowDiv.Controls[1].FindControl( "cbAllowMessaging" );
                    if ( cbSMS != null )
                    {
                        sms = cbSMS.Checked;
                    }
                }

                var phoneNumber = CurrentPerson.PhoneNumbers.Where( pn => pn.NumberTypeValueId == phoneTypeId ).FirstOrDefault();

                if ( phoneNumber == null && !String.IsNullOrWhiteSpace( numberValue ) )
                {
                    phoneNumber = new PhoneNumber()
                    {
                        Number = numberValue,
                        NumberFormatted = PhoneNumber.FormattedNumber( "1", numberValue ),
                        NumberTypeValueId = phoneTypeId,
                        IsMessagingEnabled = sms
                    };
                    CurrentPerson.PhoneNumbers.Add( phoneNumber );
                }
                else if ( phoneNumber != null && !String.IsNullOrWhiteSpace( numberValue ) )
                {
                    phoneNumber.Number = numberValue;
                    phoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( "1", numberValue );
                    phoneNumber.IsMessagingEnabled = sms;
                }
                else if ( phoneNumber != null && String.IsNullOrWhiteSpace( numberValue ) )
                {
                    CurrentPerson.PhoneNumbers.Remove( phoneNumber );
                }

            }

        }

        #endregion

        #region Private Methods

        private void LoadGroupRoles()
        {
            var roles = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), DBContext ).Roles
                .Select( r => new
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Order = r.Order
                    } )
                .OrderBy( o => o.Order )
                .ThenBy( o => o.Name )
                .ToList();

            ddlGroupRole.DataSource = roles;
            ddlGroupRole.DataValueField = "Id";
            ddlGroupRole.DataTextField = "Name";
            ddlGroupRole.DataBind();
        }


        #endregion



    }

}