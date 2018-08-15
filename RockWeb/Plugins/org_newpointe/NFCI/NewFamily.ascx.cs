using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [DisplayName( "NFCI New Family" )]
    [Category( "NewPointe NFCI" )]
    [Description( "Allows for quickly adding a new Family" )]

    [LinkedPage( "Person Details Page", "The page to use to show person details.", false, "", "", 0 )]
    public partial class NewFamily : Rock.Web.UI.RockBlock
    {
        List<KidData> kidsList = new List<KidData>();

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            //pError.Visible = false;

        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                //kidsList.Add( new KidData { FirstName = "", LastName = "", Gender = Gender.Unknown } );
                BindControls();
            }
            else
            {
                loadKids();
            }
        }

        protected void BindControls()
        {

            rKids.DataSource = kidsList;
            rKids.DataBind();

        }

        private class KidData
        {
            public String FirstName { get; set; }
            public String LastName { get; set; }
            public Gender Gender { get; set; }
            public DateTime? Birthdate { get; set; }
            public DefinedValueCache Grade { get; set; }
            public String Allergy { get; set; }
        }

        protected void loadKids()
        {
            kidsList.Clear();
            foreach ( RepeaterItem Item in rKids.Items )
            {
                RockTextBox firstName = Item.FindControl( "rtpKidFirstName" ) as RockTextBox;
                RockTextBox lastName = Item.FindControl( "rtpKidLastName" ) as RockTextBox;
                RockRadioButtonList gender = Item.FindControl( "rblGender" ) as RockRadioButtonList;
                DatePicker birthdate = Item.FindControl( "dpBirthdate" ) as DatePicker;
                GradePicker grade = Item.FindControl( "gpGrade" ) as GradePicker;
                RockTextBox allergy = Item.FindControl( "rtbAllergy" ) as RockTextBox;

                var kidData = new KidData
                {
                    FirstName = firstName.Text,
                    LastName = lastName.Text,
                    Gender = gender.SelectedValueAsEnum<Gender>( Gender.Unknown ),
                    Birthdate = birthdate.SelectedDate,
                    Grade = grade.SelectedGradeValue,
                    Allergy = allergy.Text
                };

                kidsList.Add( kidData );
            }
        }

        protected void rKids_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            RepeaterItem Item = e.Item;
            if ( Item.ItemType == ListItemType.Item || Item.ItemType == ListItemType.AlternatingItem )
            {
                var kid = e.Item.DataItem as KidData;

                RockTextBox firstName = Item.FindControl( "rtpKidFirstName" ) as RockTextBox;
                RockTextBox lastName = Item.FindControl( "rtpKidLastName" ) as RockTextBox;
                RockRadioButtonList gender = Item.FindControl( "rblGender" ) as RockRadioButtonList;
                DatePicker birthdate = Item.FindControl( "dpBirthdate" ) as DatePicker;
                GradePicker grade = Item.FindControl( "gpGrade" ) as GradePicker;
                RockTextBox allergy = Item.FindControl( "rtbAllergy" ) as RockTextBox;

                firstName.Text = kid.FirstName;
                lastName.Text = kid.LastName;
                gender.SelectedValue = kid.Gender.ConvertToInt().ToString();
                birthdate.SelectedDate = kid.Birthdate;
                grade.SelectedGradeValue = kid.Grade;
                allergy.Text = kid.Allergy;
            }
        }

        protected void lbAddKid_Click( object sender, EventArgs e )
        {
            kidsList.Add( new KidData { FirstName = "", LastName = rtbParentLastName.Text, Gender = Gender.Unknown } );
            BindControls();
        }

        protected void rKids_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Delete" )
            {
                kidsList.RemoveAt( e.Item.ItemIndex );
                BindControls();
            }
        }

        protected void lbSubmit_Click( object sender, EventArgs e )
        {

            if ( Page.IsValid )
            {
                RockContext rockContext = new RockContext();

                var adultRole = new GroupTypeRoleService( rockContext ).Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );
                var childRole = new GroupTypeRoleService( rockContext ).Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() );

                var personRecordType = new DefinedValueService( rockContext ).Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );
                var personRecordStatus = new DefinedValueService( rockContext ).Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                var personConnectionStatus = new DefinedValueService( rockContext ).Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );

                Person parent = new Person();
                parent.RecordTypeValue = personRecordType;
                parent.RecordStatusValue = personRecordStatus;
                parent.ConnectionStatusValue = personConnectionStatus;

                parent.FirstName = rtbParentFirstName.Text;
                parent.LastName = rtbParentLastName.Text;

                parent.LoadAttributes( rockContext );

                PhoneNumber parentPhone = new PhoneNumber();
                parentPhone.Number = pnbParentPhoneNumber.Number;

                parent.PhoneNumbers.Add( parentPhone );

                List<GroupMember> familyMembers = new List<GroupMember>();

                GroupMember parentGM = new GroupMember();
                parentGM.Person = parent;
                parentGM.GroupRoleId = adultRole.Id;

                familyMembers.Add( parentGM );

                foreach ( KidData kidData in kidsList )
                {
                    Person kid = new Person();
                    kid.RecordTypeValue = personRecordType;
                    kid.RecordStatusValue = personRecordStatus;
                    kid.ConnectionStatusValue = personConnectionStatus;
                    kid.FirstName = kidData.FirstName;
                    kid.LastName = kidData.LastName;
                    kid.Gender = kidData.Gender;
                    kid.SetBirthDate( kidData.Birthdate );
                    if ( kidData.Grade != null )
                    {
                        kid.GradeOffset = kidData.Grade.Value.AsIntegerOrNull();
                    }

                    kid.LoadAttributes( rockContext );
                    kid.SetAttributeValue( "Allergy", kidData.Allergy );

                    GroupMember kidGM = new GroupMember();
                    kidGM.Person = kid;
                    kidGM.GroupRoleId = childRole.Id;

                    familyMembers.Add( kidGM );
                }

                Group family = GroupService.SaveNewFamily( rockContext, familyMembers, null, true );

                if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "PersonDetailsPage" ) ) )
                {
                    NavigateToLinkedPage( "PersonDetailsPage", new Dictionary<string, string>() { { "PersonId", parent.Id.ToString() } } );
                }
                else
                {
                    NavigateToCurrentPage( new Dictionary<string, string>() { { "PersonId", parent.Id.ToString() } } );
                }

            }

        }
    }
}