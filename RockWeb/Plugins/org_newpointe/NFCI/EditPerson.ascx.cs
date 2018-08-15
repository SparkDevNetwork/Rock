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

                dvpTitle.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ), true );
                dvpTitle.SelectedValue = person.TitleValueId.ToString();

                tbFirstName.Text = person.FirstName;
                tbMiddleName.Text = person.MiddleName;
                tbLastName.Text = person.LastName;

                dvpSuffix.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ), true );
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

                var mobilePhoneType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );

                var phoneNumbers = new List<PhoneNumber>();
                var phoneNumberTypes = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
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


                        int? newTitleId = dvpTitle.SelectedValueAsInt();
                        History.EvaluateChange( changes, "Title", DefinedValueCache.GetName( personSave.TitleValueId ), DefinedValueCache.GetName( newTitleId ) );
                        personSave.TitleValueId = newTitleId;

                        History.EvaluateChange( changes, "First Name", personSave.FirstName, tbFirstName.Text );
                        personSave.FirstName = tbFirstName.Text;

                        string nickName = string.IsNullOrWhiteSpace( tbNickName.Text ) ? tbFirstName.Text : tbNickName.Text;
                        History.EvaluateChange( changes, "Nick Name", personSave.NickName, nickName );
                        personSave.NickName = tbNickName.Text;

                        History.EvaluateChange( changes, "Middle Name", personSave.MiddleName, tbMiddleName.Text );
                        personSave.MiddleName = tbMiddleName.Text;

                        History.EvaluateChange( changes, "Last Name", personSave.LastName, tbLastName.Text );
                        personSave.LastName = tbLastName.Text;

                        int? newSuffixId = dvpSuffix.SelectedValueAsInt();
                        History.EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( personSave.SuffixValueId ), DefinedValueCache.GetName( newSuffixId ) );
                        personSave.SuffixValueId = newSuffixId;

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

                        History.EvaluateChange( changes, "Birth Month", birthMonth, personSave.BirthMonth );
                        History.EvaluateChange( changes, "Birth Day", birthDay, personSave.BirthDay );
                        History.EvaluateChange( changes, "Birth Year", birthYear, personSave.BirthYear );


                        DateTime gradeTransitionDate = GlobalAttributesCache.Read().GetValue( "GradeTransitionDate" ).AsDateTime() ?? new DateTime( RockDateTime.Now.Year, 6, 1 );

                        // add a year if the next graduation mm/dd won't happen until next year
                        int gradeOffsetRefactor = ( RockDateTime.Now < gradeTransitionDate ) ? 0 : 1;


                        int? graduationYear = null;
                        if ( gpGrade.SelectedValue.AsIntegerOrNull() != null )
                        {
                            graduationYear = gradeTransitionDate.Year + gradeOffsetRefactor + gpGrade.SelectedValue.AsIntegerOrNull();
                        }

                        History.EvaluateChange( changes, "Graduation Year", person.GraduationYear, graduationYear );
                        personSave.GraduationYear = graduationYear;

                        var newGender = rblGender.SelectedValue.ConvertToEnum<Gender>();
                        History.EvaluateChange( changes, "Gender", personSave.Gender, newGender );
                        personSave.Gender = newGender;

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

                                        History.EvaluateChange(
                                            changes,
                                            string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumberTypeId ) ),
                                            oldPhoneNumber,
                                            phoneNumber.NumberFormattedWithCountryCode );
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
                            History.EvaluateChange(
                                changes,
                                string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumber.NumberTypeValueId ) ),
                                phoneNumber.ToString(),
                                string.Empty );

                            personSave.PhoneNumbers.Remove( phoneNumber );
                            phoneNumberService.Delete( phoneNumber );
                        }

                        History.EvaluateChange( changes, "Email", personSave.Email, ebEmail.Text );
                        personSave.Email = ebEmail.Text.Trim();

                        if ( personSave.IsValid )
                        {

                            personSave.SaveAttributeValues();

                            if ( rockContext.SaveChanges() > 0 )
                            {
                                if ( changes.Any() )
                                {
                                    HistoryService.SaveChanges(
                                        rockContext,
                                        typeof( Person ),
                                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                        person.Id,
                                        changes );
                                }

                            }

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