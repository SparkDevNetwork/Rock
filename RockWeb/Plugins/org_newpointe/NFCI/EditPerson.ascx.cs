using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_newpointe.NFCI
{
    /// <summary>
    /// Block for adding new families
    /// </summary>
    [DisplayName( "NFCI Edit Person" )]
    [Category( "NewPointe NFCI" )]
    [Description( "Allows for quickly editing a Person" )]

    [LinkedPage( "Person Details Page", "The page to use to show person details.", false, "", "", 0 )]
    [LinkedPage( "Workflow Entry Page", "The Workflow Entry page.", false, "", "", 1 )]
    [WorkflowTypeField( "Request Change Workflow", "The type of workflow to launch for a change request.", false, false, "", "", 2 )]
    public partial class EditPerson : Rock.Web.UI.RockBlock
    {

        RockContext rContext = new RockContext();
        Person person;

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            person = new PersonService( rContext ).Get( PageParameter("PersonGuid").AsGuid() );

            if ( !Page.IsPostBack )
            {
                if ( person != null && person.IsAuthorized( "View", CurrentPerson ) )
                {
                    pPersonInfo.Visible = true;
                    pPersonError.Visible = false;
                    BindControls();
                }
                else
                {
                    pPersonInfo.Visible = false;
                    pPersonError.Visible = true;
                }
            }
        }

        protected void BindControls()
        {
            if(person != null)
            {
                tbNickName.Text = person.NickName;

                dvpTitle.BindToDefinedType( DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ), true );
                dvpTitle.SelectedValue = person.TitleValueId.ToString();

                tbFirstName.Text = person.FirstName;
                tbMiddleName.Text = person.MiddleName;
                tbLastName.Text = person.LastName;

                dvpSuffix.BindToDefinedType( DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ), true );
                dvpSuffix.SelectedValue = person.SuffixValueId.ToString();

                rblGender.BindToEnum<Gender>( false );
                rblGender.SelectedValue = person.Gender.ConvertToInt().ToString();

                bpBirthday.SelectedDate = person.BirthDate;
                
                if ( !person.HasGraduated ?? false )
                {
                    gpGrade.SetValue( person.GradeOffset.Value );
                }
                else
                {
                    gpGrade.SelectedIndex = 0;
                }

                person.LoadAttributes();
                rtbAllergy.Text = person.AttributeValues["Allergy"].Value;
                
                ebEmail.Text = person.Email;

                var mobilePhoneType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );

                var phoneNumbers = new List<PhoneNumber>();
                var phoneNumberTypes = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
                if ( phoneNumberTypes.DefinedValues.Any() )
                {
                    foreach ( var phoneNumberType in phoneNumberTypes.DefinedValues )
                    {
                        var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberType.Id );
                        if ( phoneNumber == null )
                        {
                            var numberType = new DefinedValue();
                            numberType.Id = phoneNumberType.Id;
                            numberType.Value = phoneNumberType.Value;

                            phoneNumber = new PhoneNumber { NumberTypeValueId = numberType.Id, NumberTypeValue = numberType };
                            phoneNumber.IsMessagingEnabled = mobilePhoneType != null && phoneNumberType.Id == mobilePhoneType.Id;
                        }
                        else
                        {
                            // Update number format, just in case it wasn't saved correctly
                            phoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number );
                        }

                        phoneNumbers.Add( phoneNumber );
                    }

                    rContactInfo.DataSource = phoneNumbers;
                    rContactInfo.DataBind();
                }
            }
        }

        protected void lbSubmit_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();

                rockContext.WrapTransaction( () =>
                {

                    if ( person != null )
                    {
                        var personService = new PersonService( rockContext );

                        var changes = new List<string>();

                        var personSave = personService.Get( person.Id );
                        personSave.LoadAttributes();


                        personSave.TitleValueId = dvpTitle.SelectedValueAsInt();
                        personSave.FirstName = tbFirstName.Text;
                        personSave.NickName = tbNickName.Text;
                        personSave.MiddleName = tbMiddleName.Text;
                        personSave.LastName = tbLastName.Text;
                        personSave.SuffixValueId = dvpSuffix.SelectedValueAsInt();
                        var birthMonth = personSave.BirthMonth;
                        var birthDay = personSave.BirthDay;
                        var birthYear = personSave.BirthYear;

                        var birthday = bpBirthday.SelectedDate;
                        if ( birthday.HasValue )
                        {
                            // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                            var today = RockDateTime.Today;
                            while ( birthday.Value.CompareTo( today ) > 0 )
                            {
                                birthday = birthday.Value.AddYears( -100 );
                            }

                            personSave.BirthMonth = birthday.Value.Month;
                            personSave.BirthDay = birthday.Value.Day;
                            if ( birthday.Value.Year != DateTime.MinValue.Year )
                            {
                                personSave.BirthYear = birthday.Value.Year;
                            }
                            else
                            {
                                personSave.BirthYear = null;
                            }
                        }
                        else
                        {
                            personSave.SetBirthDate( null );
                        }
                        
                        DateTime gradeTransitionDate = GlobalAttributesCache.Get().GetValue( "GradeTransitionDate" ).AsDateTime() ?? new DateTime( RockDateTime.Now.Year, 6, 1 );

                        // add a year if the next graduation mm/dd won't happen until next year
                        int gradeOffsetRefactor = ( RockDateTime.Now < gradeTransitionDate ) ? 0 : 1;


                        int? graduationYear = null;
                        if ( gpGrade.SelectedValue.AsIntegerOrNull() != null )
                        {
                            graduationYear = gradeTransitionDate.Year + gradeOffsetRefactor + gpGrade.SelectedValue.AsIntegerOrNull();
                        }
                        
                        personSave.GraduationYear = graduationYear;
                        
                        personSave.Gender = rblGender.SelectedValue.ConvertToEnum<Gender>();

                        personSave.SetAttributeValue( "Allergy", rtbAllergy.Text );

                        var phoneNumberTypeIds = new List<int>();

                        foreach ( RepeaterItem item in rContactInfo.Items )
                        {
                            HiddenField hfPhoneType = item.FindControl( "hfPhoneType" ) as HiddenField;
                            PhoneNumberBox pnbPhone = item.FindControl( "pnbPhone" ) as PhoneNumberBox;
                            CheckBox cbUnlisted = item.FindControl( "cbUnlisted" ) as CheckBox;

                            if ( hfPhoneType != null &&
                                pnbPhone != null &&
                                cbUnlisted != null )
                            {
                                if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
                                {
                                    int phoneNumberTypeId;
                                    if ( int.TryParse( hfPhoneType.Value, out phoneNumberTypeId ) )
                                    {
                                        var phoneNumber = personSave.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberTypeId );
                                        string oldPhoneNumber = string.Empty;
                                        if ( phoneNumber == null )
                                        {
                                            phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberTypeId };
                                            personSave.PhoneNumbers.Add( phoneNumber );
                                        }
                                        else
                                        {
                                            oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                                        }

                                        phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                                        phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );

                                        phoneNumber.IsUnlisted = cbUnlisted.Checked;
                                        phoneNumberTypeIds.Add( phoneNumberTypeId );
                                        
                                    }
                                }
                            }
                        }

                        // Remove any blank numbers
                        var phoneNumberService = new PhoneNumberService( rockContext );
                        foreach ( var phoneNumber in personSave.PhoneNumbers
                            .Where( n => n.NumberTypeValueId.HasValue && !phoneNumberTypeIds.Contains( n.NumberTypeValueId.Value ) )
                            .ToList() )
                        {
                            personSave.PhoneNumbers.Remove( phoneNumber );
                            phoneNumberService.Delete( phoneNumber );
                        }
                        
                        personSave.Email = ebEmail.Text.Trim();

                        if ( personSave.IsValid )
                        {

                            personSave.SaveAttributeValues();

                            if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "PersonDetailsPage" ) ) )
                            {
                                NavigateToLinkedPage( "PersonDetailsPage", new Dictionary<string, string>() { { "PersonId", person.Id.ToString() } } );
                            }
                            else
                            {
                                NavigateToCurrentPage( new Dictionary<string, string>() { { "PersonId", person.Id.ToString() } } );
                            }
                        }
                    }
                } );
            }
        }

        protected void lbRequestChange_Click( object sender, EventArgs e )
        {
            var pars = new Dictionary<string, string>();

            var wfType = new WorkflowTypeService( rContext ).Get( GetAttributeValue( "RequestChangeWorkflow" ).AsGuid() );
            if ( wfType != null )
            {
                pars.Add( "WorkflowTypeId", wfType.Id.ToString() );
            }

            if ( person != null )
            {
                pars.Add( "PersonId", person.Id.ToString() );
            }

            NavigateToLinkedPage( "WorkflowEntryPage", pars );
        }
    }
}