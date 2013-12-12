//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile blockthe main information about a peron 
    /// </summary>
    public partial class EditPerson : Rock.Web.UI.PersonBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ddlTitle.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ), true );
            ddlSuffix.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ), true );
            rblMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ) );
            rblStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ) );
            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ) );
            ddlReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ), true );

            ddlGivingGroup.Items.Clear();
            ddlGivingGroup.Items.Add(new ListItem(None.Text, None.IdValue));
            if ( Person != null )
            {
                var personService = new PersonService();
                foreach ( var family in personService.GetFamilies( Person ) )
                {
                    ddlGivingGroup.Items.Add( new ListItem( family.Name, family.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && Person != null )
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRecordStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlRecordStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlReason.Visible = ( ddlRecordStatus.SelectedValueAsInt() == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var personService = new PersonService();
            var person = personService.Get( Person.Id );

            int? orphanedPhotoId = null;
            if ( person.PhotoId != imgPhoto.BinaryFileId )
            {
                orphanedPhotoId = person.PhotoId;
                person.PhotoId = imgPhoto.BinaryFileId;
            }
            
            person.TitleValueId = ddlTitle.SelectedValueAsInt();
            person.GivenName = tbGivenName.Text;
            person.NickName = tbNickName.Text;
            person.MiddleName = tbMiddleName.Text;
            person.LastName = tbLastName.Text;
            person.SuffixValueId = ddlSuffix.SelectedValueAsInt();

            var birthday = bpBirthDay.SelectedDate;
            if ( birthday.HasValue )
            {
                person.BirthMonth = birthday.Value.Month;
                person.BirthDay = birthday.Value.Day;
                if ( birthday.Value.Year != DateTime.MinValue.Year )
                {
                    person.BirthYear = birthday.Value.Year;
                }
                else
                {
                    person.BirthYear = null;
                }
            }
            else
            {
                person.BirthDate = null;
            }

            person.AnniversaryDate = dpAnniversaryDate.SelectedDate;
            person.Gender = rblGender.SelectedValue.ConvertToEnum<Gender>();
            person.MaritalStatusValueId = rblMaritalStatus.SelectedValueAsInt();
            person.ConnectionStatusValueId = rblStatus.SelectedValueAsInt();

            var phoneNumberTypeIds = new List<int>();

            foreach ( RepeaterItem item in rContactInfo.Items )
            {
                HiddenField hfPhoneType = item.FindControl( "hfPhoneType" ) as HiddenField;
                TextBox tbPhone = item.FindControl( "tbPhone" ) as TextBox;
                CheckBox cbUnlisted = item.FindControl( "cbUnlisted" ) as CheckBox;
                CheckBox cbSms = item.FindControl( "cbSms" ) as CheckBox;

                if ( hfPhoneType != null &&
                    tbPhone != null &&
                    cbSms != null &&
                    cbUnlisted != null )
                {
                    if ( !string.IsNullOrWhiteSpace( tbPhone.Text ) )
                    {
                        int phoneNumberTypeId;
                        if ( int.TryParse( hfPhoneType.Value, out phoneNumberTypeId ) )
                        {
                            var phoneNumber = person.PhoneNumbers.FirstOrDefault(n => n.NumberTypeValueId == phoneNumberTypeId);
                            if ( phoneNumber == null )
                            {
                                phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberTypeId };
                                person.PhoneNumbers.Add( phoneNumber );
                            }

                            phoneNumber.Number = PhoneNumber.CleanNumber( tbPhone.Text );
                            phoneNumber.IsMessagingEnabled = cbSms.Checked;
                            phoneNumber.IsUnlisted = cbUnlisted.Checked;

                            phoneNumberTypeIds.Add( phoneNumberTypeId );
                        }
                    }
                }
            }

            // Remove any blank numbers
            person.PhoneNumbers
                .Where( n => n.NumberTypeValueId.HasValue && !phoneNumberTypeIds.Contains( n.NumberTypeValueId.Value ) )
                .ToList()
                .ForEach( n => person.PhoneNumbers.Remove( n ) );

            person.Email = tbEmail.Text.Trim();

            person.GivingGroupId = ddlGivingGroup.SelectedValueAsId();

            person.RecordStatusValueId = ddlRecordStatus.SelectedValueAsInt();
            person.RecordStatusReasonValueId = ddlReason.SelectedValueAsInt();

            if ( !person.IsValid )
            {
                return;
            }
            personService.Save( person, CurrentPersonId );

            if ( orphanedPhotoId.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService(personService.RockContext);
                var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                if ( binaryFile != null )
                {
                    // marked the old images as IsTemporary so they will get cleaned up later
                    binaryFile.IsTemporary = true;
                    binaryFileService.Save( binaryFile, CurrentPersonId );
                }
            }

            Response.Redirect( string.Format( "~/Person/{0}", Person.Id ), false );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            Response.Redirect( string.Format( "~/Person/{0}", Person.Id ), false );
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            imgPhoto.BinaryFileId = Person.PhotoId;
            ddlTitle.SelectedValue = Person.TitleValueId.HasValue ? Person.TitleValueId.Value.ToString() : string.Empty;
            tbGivenName.Text = Person.GivenName;
            tbNickName.Text = Person.NickName;
            tbMiddleName.Text = Person.MiddleName;
            tbLastName.Text = Person.LastName;
            ddlSuffix.SelectedValue = Person.SuffixValueId.HasValue ? Person.SuffixValueId.Value.ToString() : string.Empty;
            bpBirthDay.SelectedDate = Person.BirthDate;
            dpAnniversaryDate.SelectedDate = Person.AnniversaryDate;
            rblGender.SelectedValue = Person.Gender.ConvertToString();
            rblMaritalStatus.SelectedValue = Person.MaritalStatusValueId.HasValue ? Person.MaritalStatusValueId.Value.ToString() : string.Empty;
            rblStatus.SelectedValue = Person.ConnectionStatusValueId.HasValue ? Person.ConnectionStatusValueId.Value.ToString() : string.Empty;
            tbEmail.Text = Person.Email;

            ddlRecordStatus.SelectedValue = Person.RecordStatusValueId.HasValue ? Person.RecordStatusValueId.Value.ToString() : string.Empty;
            ddlReason.SelectedValue = Person.RecordStatusReasonValueId.HasValue ? Person.RecordStatusReasonValueId.Value.ToString() : string.Empty;

            var mobilePhoneType = DefinedValueCache.Read(new Guid(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE));

            var phoneNumbers = new List<PhoneNumber>();
            var phoneNumberTypes = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
            if ( phoneNumberTypes.DefinedValues.Any() )
            {
                foreach ( var phoneNumberType in phoneNumberTypes.DefinedValues )
                {
                    var phoneNumber = Person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberType.Id );
                    if ( phoneNumber == null )
                    {
                        var numberType = new DefinedValue();
                        numberType.Id = phoneNumberType.Id;
                        numberType.Name = phoneNumberType.Name;

                        phoneNumber = new PhoneNumber { NumberTypeValueId = numberType.Id, NumberTypeValue = numberType };
                        phoneNumber.IsMessagingEnabled = mobilePhoneType != null && phoneNumberType.Id == mobilePhoneType.Id;
                    }

                    phoneNumbers.Add( phoneNumber );
                }

                rContactInfo.DataSource = phoneNumbers;
                rContactInfo.DataBind();
            }

            ddlGivingGroup.SetValue( Person.GivingGroupId );

        }
    }
}