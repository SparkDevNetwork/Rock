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
    [DisplayName( "Edit Person" )]
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

            person = new PersonService( rContext ).Get( PageParameter( "PersonGuid" ).AsGuid() );

            if ( !Page.IsPostBack )
            {
                if ( person != null && person.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson ) )
                {
                    ShowEdit( person );
                }
                else
                {
                    ShowError( "Error", "This person does not exist or you do not have permission to edit them.", true );
                }
            }
        }

        protected void ShowError( string title, string message, bool hideSecondary = false )
        {
            rlErrorTitle.Text = title;
            rlErrorMessage.Text = message;
            pPersonError.Visible = true;
            pPersonEdit.Visible = !hideSecondary;
        }

        protected void ShowEdit( Person person )
        {
            SetupControls();
            LoadPersonDetails( person );
            pPersonEdit.Visible = true;
            pPersonError.Visible = false;
        }

        protected void SetupControls()
        {
            DefinedTypeCache dtcPersonTitle = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() );
            DefinedTypeCache dtcPersonSuffix = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() );

            dvpTitle.DefinedTypeId = dtcPersonTitle.Id;
            dvpSuffix.DefinedTypeId = dtcPersonSuffix.Id;
            rblGender.BindToEnum<Gender>( false );
        }

        protected void LoadPersonDetails( Person person )
        {
            DefinedTypeCache dtcPersonPhoneTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() );

            tbNickName.Text = person.NickName;

            dvpTitle.SelectedValue = person.TitleValueId.ToString();
            tbFirstName.Text = person.FirstName;
            tbMiddleName.Text = person.MiddleName;
            tbLastName.Text = person.LastName;
            dvpSuffix.SelectedValue = person.SuffixValueId.ToString();

            rblGender.SelectedValue = person.Gender.ConvertToInt().ToString();
            bpBirthday.SelectedDate = person.BirthDate;
            gpGrade.SelectedValue = person.GradeOffset.ToString();

            person.LoadAttributes();
            rtbAllergy.Text = person.GetAttributeValue( "Allergy" );

            ebEmail.Text = person.Email;

            var phoneNumbers = dtcPersonPhoneTypes.DefinedValues.Select( dv =>
                person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == dv.Id )
                ?? new PhoneNumber { NumberTypeValueId = dv.Id, NumberTypeValue = new DefinedValue { Id = dv.Id, Value = dv.Value } }
            ).ToList();
            rContactInfo.DataSource = phoneNumbers;
            rContactInfo.DataBind();
        }

        protected void SavePersonDetails( Person person, RockContext rockContext )
        {
            person.NickName = tbNickName.Text;

            person.TitleValueId = dvpTitle.SelectedValue.AsIntegerOrNull();
            person.FirstName = tbFirstName.Text;
            person.MiddleName = tbMiddleName.Text;
            person.LastName = tbLastName.Text;
            person.SuffixValueId = dvpSuffix.SelectedValue.AsIntegerOrNull();

            person.Gender = rblGender.SelectedValueAsEnum<Gender>();
            person.SetBirthDate( bpBirthday.SelectedDate );
            person.GradeOffset = gpGrade.SelectedValue.AsIntegerOrNull();

            person.LoadAttributes();
            person.SetAttributeValue( "Allergy", rtbAllergy.Text );

            person.Email = ebEmail.Text;


            var newPhoneNumbers = new List<PhoneNumber>();
            foreach ( RepeaterItem item in rContactInfo.Items )
            {
                HiddenField hfPhoneType = item.FindControl( "hfPhoneType" ) as HiddenField;
                PhoneNumberBox pnbPhone = item.FindControl( "pnbPhone" ) as PhoneNumberBox;
                CheckBox cbUnlisted = item.FindControl( "cbUnlisted" ) as CheckBox;

                if ( hfPhoneType != null && pnbPhone != null && cbUnlisted != null )
                {
                    var countryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                    var phoneNumber = PhoneNumber.CleanNumber( pnbPhone.Number );
                    if ( !string.IsNullOrWhiteSpace( countryCode ) && !string.IsNullOrWhiteSpace( phoneNumber ) )
                    {
                        newPhoneNumbers.Add( new PhoneNumber
                        {
                            NumberTypeValueId = hfPhoneType.ValueAsInt(),
                            Number = phoneNumber,
                            CountryCode = countryCode,
                            IsMessagingEnabled = false,
                            IsUnlisted = cbUnlisted.Checked
                        } );
                    }
                }
            }

            foreach ( PhoneNumber oldPhoneNumber in person.PhoneNumbers.ToList() )
            {
                var matchingPhoneNumber = newPhoneNumbers.FirstOrDefault( pn => pn.NumberTypeValueId == oldPhoneNumber.NumberTypeValueId );
                if ( matchingPhoneNumber != null )
                {
                    oldPhoneNumber.CountryCode = matchingPhoneNumber.CountryCode;
                    oldPhoneNumber.Number = matchingPhoneNumber.Number;
                    oldPhoneNumber.IsUnlisted = matchingPhoneNumber.IsUnlisted;
                    newPhoneNumbers.Remove( matchingPhoneNumber );
                }
                else
                {
                    person.PhoneNumbers.Remove( oldPhoneNumber );
                    new PhoneNumberService( rockContext ).Delete( oldPhoneNumber );
                }
            }

            foreach ( PhoneNumber newPhoneNumber in newPhoneNumbers )
            {
                person.PhoneNumbers.Add( newPhoneNumber );
            }

        }

        protected void lbSubmit_Click( object sender, EventArgs evt )
        {
            if ( person == null || !person.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson ) )
            {
                ShowError( "Error", "This person does not exist or you do not have permission to edit them.", true );
                return;
            }

            if ( !Page.IsValid )
            {
                return;
            }

            SavePersonDetails( person, rContext );

            if ( !person.IsValid )
            {
                ShowError( "Error", "<ul><li>" + String.Join( "</li><li>", person.ValidationResults.Select( vr => vr.ErrorMessage ) ) + "</li></ul>", true );
                return;
            }

            try
            {
                rContext.WrapTransaction( () =>
                {
                    person.SaveAttributeValues( rContext );
                    rContext.SaveChanges();
                } );
            }
            catch ( Exception err )
            {
                ShowError( "Error", "There was an error saving this person: " + err.Message );
                return;
            }
            
            if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "PersonDetailsPage" ) ) )
            {
                NavigateToLinkedPage( "PersonDetailsPage", new Dictionary<string, string>() { { "PersonGuid", person.Guid.ToString() } } );
            }
            else
            {
                NavigateToCurrentPage( new Dictionary<string, string>() { { "PersonGuid", person.Guid.ToString() } } );
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