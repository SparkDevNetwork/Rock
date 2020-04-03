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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_hfbc.Legacy685
{
    /// <summary>
    /// The main Person Profile block the main information about a peron 
    /// </summary>
    [DisplayName( "Family Members" )]
    [Category( "org_hfbc > Legacy 685" )]
    [Description( "Block used to view members of a Legacy 685 family" )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE, "Phone Numbers", "The types of phone numbers to display / edit.", true, true, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Person Attributes (adults)", "The person attributes that should be displayed / edited for adults.", false, true )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Person Attributes (children)", "The person attributes that should be displayed / edited for children.", false, true )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Legacy 685 Record Status Attribute", "", false, false, key: "LegacyStatus" )]
    [DefinedValueField(Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status For New Members", "The Connection Status that will be used for all new Family Members")]
    [ConnectionStatusField("Connection Status For New Members", "This connection status will be used for all new Family Members", true)]
    [BooleanField( "UseCurrentPerson", "Whether the block should use the current person to generate family members" )]
    public partial class FamilyMembers : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            ScriptManager.RegisterStartupScript( ddlGradePicker, ddlGradePicker.GetType(), "grade-selection-" + BlockId.ToString(), ddlGradePicker.GetJavascriptForYearPicker( ypGraduation ), true );

            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/imagesloaded.min.js" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.fluidbox.min.js" ) );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( CurrentPerson != null )
            {
                if ( !Page.IsPostBack )
                {
                    ShowDetail();
                }
                else
                {
                    var rockContext = new RockContext();
                    var personService = new PersonService( rockContext );
                    var group = new GroupService( rockContext ).Get( GetGroupId( rockContext ).Value );
                    var person = personService.Get( hfPersonId.ValueAsInt() );
                    if ( person != null && group != null )
                    {
                        // Person Attributes
                        var displayedAttributeGuids = GetPersonAttributeGuids( person.Id );
                        if ( !displayedAttributeGuids.Any() || person.Id == 0 )
                        {
                            pnlPersonAttributes.Visible = false;
                        }
                        else
                        {
                            pnlPersonAttributes.Visible = true;
                            DisplayEditAttributes( person, displayedAttributeGuids, phPersonAttributes, pnlPersonAttributes, false );
                        }
                    }
                }
            }
            else
            {
                pnlView.Visible = false;
                pnlEdit.Visible = false;
            }
        }

        #endregion

        #region Events

        #region View Events

        /// <summary>
        /// Handles the Click event of the lbEditPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditPerson_Click( object sender, EventArgs e )
        {
            ShowEditPersonDetails( CurrentPerson.Id );
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptGroupMembers control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptGroupMembers_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int personId = e.CommandArgument.ToString().AsInteger();
            ShowEditPersonDetails( personId );
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddGroupMember_Click( object sender, EventArgs e )
        {
            if ( GetGroupId().HasValue )
            {
                ShowEditPersonDetails( 0 );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGroupMembers_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var attributeValueService = new AttributeValueService( rockContext );

            var groupMember = e.Item.DataItem as GroupMember;
            var person = groupMember.Person;
            var lGroupMemberImage = e.Item.FindControl( "lGroupMemberImage" ) as Literal;
            var lGroupMemberName = e.Item.FindControl( "lGroupMemberName" ) as Literal;
            var lGroupMemberEmail = e.Item.FindControl( "lGroupMemberEmail" ) as Literal;
            var lAge = e.Item.FindControl( "lAge" ) as Literal;
            var lGender = e.Item.FindControl( "lGender" ) as Literal;
            var lMaritalStatus = e.Item.FindControl( "lMaritalStatus" ) as Literal;
            var lGrade = e.Item.FindControl( "lGrade" ) as Literal;
            var rptGroupMemberPhones = e.Item.FindControl( "rptGroupMemberPhones" ) as Repeater;
            var rptGroupMemberAttributes = e.Item.FindControl( "rptGroupMemberAttributes" ) as Repeater;
            var lbEditGroupMember = e.Item.FindControl( "lbEditGroupMember" ) as LinkButton;

            // Setup Image
            string imgTag = Rock.Model.Person.GetPersonPhotoImageTag( person, 200, 200 );
            if ( person.PhotoId.HasValue )
            {
                lGroupMemberImage.Text = string.Format( "<a href='{0}'>{1}</a>", person.PhotoUrl, imgTag );
            }
            else
            {
                lGroupMemberImage.Text = imgTag;
            }

            // Person Info
            lGroupMemberName.Text = person.FullName;
            lGroupMemberEmail.Text = person.Email;
            if ( person.BirthDate.HasValue )
            {
                lAge.Text = string.Format( "{0}<small>({1})</small><br/>", person.FormatAge(), person.BirthYear != DateTime.MinValue.Year ? person.BirthDate.Value.ToShortDateString() : person.BirthDate.Value.ToMonthDayString() );
            }

            lGender.Text = person.Gender != Gender.Unknown ? person.Gender.ToString() : string.Empty;
            lGrade.Text = person.GradeFormatted;
            lMaritalStatus.Text = person.MaritalStatusValueId.DefinedValue();
            if ( person.AnniversaryDate.HasValue )
            {
                lMaritalStatus.Text += string.Format( " {0} yrs <small>({1})</small>", person.AnniversaryDate.Value.Age(), person.AnniversaryDate.Value.ToMonthDayString() );
            }

            // Contact Info
            if ( person.PhoneNumbers != null )
            {
                var selectedPhoneTypeGuids = GetAttributeValue( "PhoneNumbers" ).Split( ',' ).AsGuidList();
                rptGroupMemberPhones.DataSource = person.PhoneNumbers.Where( pn => selectedPhoneTypeGuids.Contains( pn.NumberTypeValue.Guid ) ).ToList();
                rptGroupMemberPhones.DataBind();
            }

            // Person Attributes
            List<Guid> attributeGuidList = new List<Guid>();
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

            if ( groupMember.GroupRole.Guid == adultGuid )
            {
                attributeGuidList = GetAttributeValue( "PersonAttributes(adults)" ).SplitDelimitedValues().AsGuidList();
            }
            else
            {
                attributeGuidList = GetAttributeValue( "PersonAttributes(children)" ).SplitDelimitedValues().AsGuidList();
            }

            person.LoadAttributes();
            rptGroupMemberAttributes.DataSource = person.Attributes.Where( a =>
             attributeGuidList.Contains( a.Value.Guid ) )
            .Select( a => new
            {
                Name = a.Value.Name,
                Value = a.Value.FieldType.Field.FormatValue( null, person.GetAttributeValue( a.Key ), a.Value.QualifierValues, a.Value.FieldType.Class == typeof( Rock.Field.Types.ImageFieldType ).FullName )
            } )
            .OrderBy( av => av.Name )
            .ToList()
            .Where( av => !String.IsNullOrWhiteSpace( av.Value ) );
            rptGroupMemberAttributes.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            if ( GetGroupId().HasValue )
            {
                var group = new GroupService( rockContext ).Get( GetGroupId().Value );
                if ( group != null )
                {
                    rockContext.WrapTransaction( () =>
                    {
                        var personService = new PersonService( rockContext );

                        var changes = new List<string>();

                        var personId = hfPersonId.Value.AsInteger();
                        if ( personId == 0 )
                        {
                            changes.Add( "Created" );

                            var groupMemberService = new GroupMemberService( rockContext );
                            var groupMember = new GroupMember() { Person = new Person(), Group = group, GroupId = group.Id };
                            groupMember.Person.TitleValueId = ddlTitle.SelectedValueAsId();
                            groupMember.Person.FirstName = tbFirstName.Text;
                            groupMember.Person.NickName = tbNickName.Text;
                            groupMember.Person.LastName = tbLastName.Text;
                            groupMember.Person.SuffixValueId = ddlSuffix.SelectedValueAsId();
                            groupMember.Person.Gender = rblGender.SelectedValueAsEnum<Gender>();
                            DateTime? birthdate = bpBirthDay.SelectedDate;
                            if ( birthdate.HasValue )
                            {
                                // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                                var today = RockDateTime.Today;
                                while ( birthdate.Value.CompareTo( today ) > 0 )
                                {
                                    birthdate = birthdate.Value.AddYears( -100 );
                                }
                            }

                            groupMember.Person.SetBirthDate( birthdate );
                            if ( ddlGradePicker.Visible )
                            {
                                groupMember.Person.GradeOffset = ddlGradePicker.SelectedValueAsInt();
                            }

                            var role = group.GroupType.Roles.Where( r => r.Id == ( rblRole.SelectedValueAsInt() ?? 0 ) ).FirstOrDefault();
                            if ( role != null )
                            {
                                groupMember.GroupRole = role;
                                groupMember.GroupRoleId = role.Id;
                            }

                            var connectionStatusGuid = GetAttributeValue( "ConnectionStatusForNewMembers" ).AsGuidOrNull();
                            if ( connectionStatusGuid != null )
                            {
                                DefinedValueCache dvConnectionStatus = DefinedValueCache.Read( connectionStatusGuid.Value );
                                if( dvConnectionStatus != null )
                                {
                                    groupMember.Person.ConnectionStatusValueId = dvConnectionStatus.Id;
                                }

                                groupMember.Person.RecordStatusValueId = 3; // Active
                            }
                            else
                            {
                                var headOfHousehold = GroupServiceExtensions.HeadOfHousehold( group.Members.AsQueryable() );
                                if ( headOfHousehold != null )
                                {
                                    DefinedValueCache dvcConnectionStatus = DefinedValueCache.Read( headOfHousehold.ConnectionStatusValueId ?? 0 );
                                    DefinedValueCache dvcRecordStatus = DefinedValueCache.Read( headOfHousehold.ConnectionStatusValueId ?? 0 );
                                    if ( dvcConnectionStatus != null )
                                    {
                                        groupMember.Person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                                    }

                                    if ( dvcRecordStatus != null )
                                    {
                                        groupMember.Person.RecordStatusValueId = dvcRecordStatus.Id;
                                    }
                                }
                            }

                            if ( groupMember.GroupRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() )
                            {
                                groupMember.Person.GivingGroupId = group.Id;
                            }

                            groupMember.Person.IsEmailActive = true;
                            groupMember.Person.EmailPreference = EmailPreference.EmailAllowed;
                            groupMember.Person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                            groupMemberService.Add( groupMember );
                            rockContext.SaveChanges();
                            personId = groupMember.PersonId;
                        }

                        var person = personService.Get( personId );
                        if ( person != null )
                        {
                            int? orphanedPhotoId = null;
                            if ( person.PhotoId != imgPhoto.BinaryFileId )
                            {
                                orphanedPhotoId = person.PhotoId;
                                person.PhotoId = imgPhoto.BinaryFileId;

                                if ( orphanedPhotoId.HasValue )
                                {
                                    if ( person.PhotoId.HasValue )
                                    {
                                        changes.Add( "Modified the photo." );
                                    }
                                    else
                                    {
                                        changes.Add( "Deleted the photo." );
                                    }
                                }
                                else if ( person.PhotoId.HasValue )
                                {
                                    changes.Add( "Added a photo." );
                                }
                            }

                            int? newTitleId = ddlTitle.SelectedValueAsInt();
                            History.EvaluateChange( changes, "Title", DefinedValueCache.GetName( person.TitleValueId ), DefinedValueCache.GetName( newTitleId ) );
                            person.TitleValueId = newTitleId;

                            History.EvaluateChange( changes, "First Name", person.FirstName, tbFirstName.Text );
                            person.FirstName = tbFirstName.Text;

                            History.EvaluateChange( changes, "Nick Name", person.NickName, tbNickName.Text );
                            person.NickName = tbNickName.Text;

                            History.EvaluateChange( changes, "Last Name", person.LastName, tbLastName.Text );
                            person.LastName = tbLastName.Text;

                            int? newSuffixId = ddlSuffix.SelectedValueAsInt();
                            History.EvaluateChange( changes, "Suffix", DefinedValueCache.GetName( person.SuffixValueId ), DefinedValueCache.GetName( newSuffixId ) );
                            person.SuffixValueId = newSuffixId;

                            var birthMonth = person.BirthMonth;
                            var birthDay = person.BirthDay;
                            var birthYear = person.BirthYear;

                            var birthday = bpBirthDay.SelectedDate;
                            if ( birthday.HasValue )
                            {
                                // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                                var today = RockDateTime.Today;
                                while ( birthday.Value.CompareTo( today ) > 0 )
                                {
                                    birthday = birthday.Value.AddYears( -100 );
                                }

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
                                person.SetBirthDate( null );
                            }

                            History.EvaluateChange( changes, "Birth Month", birthMonth, person.BirthMonth );
                            History.EvaluateChange( changes, "Birth Day", birthDay, person.BirthDay );
                            History.EvaluateChange( changes, "Birth Year", birthYear, person.BirthYear );

                            int? graduationYear = null;
                            if ( ypGraduation.SelectedYear.HasValue )
                            {
                                graduationYear = ypGraduation.SelectedYear.Value;
                            }

                            History.EvaluateChange( changes, "Graduation Year", person.GraduationYear, graduationYear );
                            person.GraduationYear = graduationYear;

                            var newGender = rblGender.SelectedValue.ConvertToEnum<Gender>();
                            History.EvaluateChange( changes, "Gender", person.Gender, newGender );
                            person.Gender = newGender;

                            var phoneNumberTypeIds = new List<int>();

                            bool smsSelected = false;

                            foreach ( RepeaterItem item in rContactInfo.Items )
                            {
                                HiddenField hfPhoneType = item.FindControl( "hfPhoneType" ) as HiddenField;
                                PhoneNumberBox pnbPhone = item.FindControl( "pnbPhone" ) as PhoneNumberBox;
                                CheckBox cbUnlisted = item.FindControl( "cbUnlisted" ) as CheckBox;
                                CheckBox cbSms = item.FindControl( "cbSms" ) as CheckBox;

                                if ( hfPhoneType != null &&
                                    pnbPhone != null &&
                                    cbSms != null &&
                                    cbUnlisted != null )
                                {
                                    if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
                                    {
                                        int phoneNumberTypeId;
                                        if ( int.TryParse( hfPhoneType.Value, out phoneNumberTypeId ) )
                                        {
                                            var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberTypeId );
                                            string oldPhoneNumber = string.Empty;
                                            if ( phoneNumber == null )
                                            {
                                                phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberTypeId };
                                                person.PhoneNumbers.Add( phoneNumber );
                                            }
                                            else
                                            {
                                                oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                                            }

                                            phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                                            phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );

                                            // Only allow one number to have SMS selected
                                            if ( smsSelected )
                                            {
                                                phoneNumber.IsMessagingEnabled = false;
                                            }
                                            else
                                            {
                                                phoneNumber.IsMessagingEnabled = cbSms.Checked;
                                                smsSelected = cbSms.Checked;
                                            }

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
                            foreach ( var phoneNumber in person.PhoneNumbers
                                .Where( n => n.NumberTypeValueId.HasValue && !phoneNumberTypeIds.Contains( n.NumberTypeValueId.Value ) )
                                .ToList() )
                            {
                                History.EvaluateChange(
                                    changes,
                                    string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumber.NumberTypeValueId ) ),
                                    phoneNumber.ToString(),
                                    string.Empty );

                                person.PhoneNumbers.Remove( phoneNumber );
                                phoneNumberService.Delete( phoneNumber );
                            }

                            History.EvaluateChange( changes, "Email", person.Email, tbEmail.Text );
                            person.Email = tbEmail.Text.Trim();

                            var newEmailPreference = rblEmailPreference.SelectedValue.ConvertToEnum<EmailPreference>();
                            History.EvaluateChange( changes, "Email Preference", person.EmailPreference, newEmailPreference );
                            person.EmailPreference = newEmailPreference;

                            person.LoadAttributes();
                            Rock.Attribute.Helper.GetEditValues( phPersonAttributes, person );

                            if ( person.IsValid )
                            {
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

                                    if ( orphanedPhotoId.HasValue )
                                    {
                                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                                        var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                                        if ( binaryFile != null )
                                        {
                                            // marked the old images as IsTemporary so they will get cleaned up later
                                            binaryFile.IsTemporary = true;
                                            rockContext.SaveChanges();
                                        }
                                    }

                                    // if they used the ImageEditor, and cropped it, the uncropped file is still in BinaryFile. So clean it up
                                    if ( imgPhoto.CropBinaryFileId.HasValue )
                                    {
                                        if ( imgPhoto.CropBinaryFileId != person.PhotoId )
                                        {
                                            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                                            var binaryFile = binaryFileService.Get( imgPhoto.CropBinaryFileId.Value );
                                            if ( binaryFile != null && binaryFile.IsTemporary )
                                            {
                                                string errorMessage;
                                                if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                                                {
                                                    binaryFileService.Delete( binaryFile );
                                                    rockContext.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                }
                                person.SaveAttributeValues();
                            }
                        }
                    } );

                    var queryString = new Dictionary<string, string>();
                    if ( PageParameter( "GroupId" ).AsIntegerOrNull().HasValue )
                    {
                        queryString.Add( "GroupId", PageParameter( "GroupId" ) );
                    }

                    if ( PageParameter( "PersonId" ).AsIntegerOrNull().HasValue )
                    {
                        queryString.Add( "PersonId", PageParameter( "PersonId" ) );
                    }
                    NavigateToCurrentPage( queryString );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ShowDetail();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( new RockContext() );
            List<Guid> attributeGuidList = new List<Guid>();
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var groupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            var selectedId = rblRole.SelectedValueAsId();
            if ( selectedId.HasValue )
            {
                if ( groupTypeRoleService.Queryable().Where( gr =>
                               gr.GroupType.Guid == groupTypeGuid &&
                               gr.Guid == adultGuid &&
                               gr.Id == selectedId ).Any() )
                {
                    attributeGuidList = GetAttributeValue( "PersonAttributes(adults)" ).SplitDelimitedValues().AsGuidList();
                    ddlGradePicker.Visible = false;
                }
                else
                {
                    attributeGuidList = GetAttributeValue( "PersonAttributes(children)" ).SplitDelimitedValues().AsGuidList();
                    ddlGradePicker.Visible = true;
                }

                if ( attributeGuidList.Any() )
                {
                    pnlPersonAttributes.Visible = true;
                    DisplayEditAttributes( new Person(), attributeGuidList, phPersonAttributes, pnlPersonAttributes, true );
                }
                else
                {
                    pnlPersonAttributes.Visible = false;
                }
            }
        }

        #endregion

        #region Methods

        private int? GetGroupId( RockContext rockContext = null )
        {
            int? groupId = null;

            groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
            if ( !groupId.HasValue )
            {
                if ( GetAttributeValue( "UseCurrentPerson" ).AsBoolean() )
                {
                    if ( CurrentPersonId.HasValue )
                    {
                        if ( rockContext == null )
                        {
                            rockContext = new RockContext();
                        }

                        var person = new PersonService( rockContext ).Get( CurrentPersonId.Value );
                        if ( person != null )
                        {
                            groupId = person.GetFamily().Id;
                        }
                    }

                }
                else
                {
                    var personId = PageParameter( "PersonId" ).AsIntegerOrNull();

                    if ( personId != null )
                    {
                        if ( rockContext == null )
                        {
                            rockContext = new RockContext();
                        }

                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            groupId = person.GetFamily().Id;
                        }
                    }
                }
            }

            return groupId;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var attributeValueService = new AttributeValueService( rockContext );
            var recordAttributeGuid = GetAttributeValue( "LegacyStatus" ).AsGuid();
            if ( GetGroupId().HasValue )
            {
                var group = new GroupService( rockContext ).Get( GetGroupId().Value );
                if ( group != null )
                {
                    var groupMembers = group.Members.Where( gm =>
                        gm.Person.IsDeceased == false );

                    var personIds = groupMembers.Select( gm => gm.PersonId ).ToList();

                    var inactivePeopleIds = attributeValueService.Queryable().AsNoTracking().Where( av =>
                         av.Attribute.Guid == recordAttributeGuid &&
                         av.Value == "Inactive" &&
                         av.EntityId != null &&
                         personIds.Contains( av.EntityId.Value ) )
                         .Select( av => av.EntityId.Value )
                         .ToList();

                    rptGroupMembers.DataSource = groupMembers
                        .Where( gm => !inactivePeopleIds.Contains( gm.PersonId ) )
                        .OrderBy( m => m.GroupRole.Order )
                        .ToList();
                    rptGroupMembers.DataBind();

                    if ( groupMembers
                        .Where( gm => inactivePeopleIds.Contains( gm.PersonId ) )
                        .Any() )
                    {
                        pnlInactiveFamily.Visible = true;

                        rptInactiveGroupMembers.DataSource = groupMembers
                            .Where( gm => inactivePeopleIds.Contains( gm.PersonId ) )
                            .OrderBy( m => m.GroupRole.Order )
                            .ToList();
                        rptInactiveGroupMembers.DataBind();
                    }
                }
            }

            hfPersonId.Value = string.Empty;
            pnlEdit.Visible = false;
            pnlView.Visible = true;
        }

        /// <summary>
        /// Shows the edit person details.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        private void ShowEditPersonDetails( int personId )
        {
            var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

            RockContext rockContext = new RockContext();
            if ( GetGroupId().HasValue )
            {
                var group = new GroupService( rockContext ).Get( GetGroupId().Value );
                if ( group != null )
                {
                    hfPersonId.Value = personId.ToString();
                    var person = new Person();
                    if ( personId == 0 )
                    {
                        rblRole.DataSource = group.GroupType.Roles.OrderBy( r => r.Order ).ToList();
                        rblRole.DataBind();
                        rblRole.Visible = true;
                        rblRole.Required = true;
                    }
                    else
                    {
                        person = new PersonService( rockContext ).Get( personId );
                    }

                    if ( person != null )
                    {
                        imgPhoto.BinaryFileId = person.PhotoId;
                        imgPhoto.NoPictureUrl = Person.GetPersonPhotoUrl( person, 200, 200 );
                        ddlTitle.SelectedValue = person.TitleValueId.HasValue ? person.TitleValueId.Value.ToString() : string.Empty;
                        tbFirstName.Text = person.FirstName;
                        tbNickName.Text = person.NickName;
                        tbLastName.Text = person.LastName;
                        ddlSuffix.SelectedValue = person.SuffixValueId.HasValue ? person.SuffixValueId.Value.ToString() : string.Empty;
                        bpBirthDay.SelectedDate = person.BirthDate;
                        rblGender.SelectedValue = person.Gender.ConvertToString();
                        if ( group.Members.Where( gm => gm.PersonId == person.Id && gm.GroupRole.Guid == childGuid ).Any() )
                        {
                            if ( person.GraduationYear.HasValue )
                            {
                                ypGraduation.SelectedYear = person.GraduationYear.Value;
                            }
                            else
                            {
                                ypGraduation.SelectedYear = null;
                            }

                            ddlGradePicker.Visible = true;
                            if ( !person.HasGraduated ?? false )
                            {
                                int gradeOffset = person.GradeOffset.Value;
                                var maxGradeOffset = ddlGradePicker.MaxGradeOffset;

                                // keep trying until we find a Grade that has a gradeOffset that that includes the Person's gradeOffset (for example, there might be combined grades)
                                while ( !ddlGradePicker.Items.OfType<ListItem>().Any( a => a.Value.AsInteger() == gradeOffset ) && gradeOffset <= maxGradeOffset )
                                {
                                    gradeOffset++;
                                }

                                ddlGradePicker.SetValue( gradeOffset );
                            }
                            else
                            {
                                ddlGradePicker.SelectedIndex = 0;
                            }
                        }

                        tbEmail.Text = person.Email;
                        rblEmailPreference.SelectedValue = person.EmailPreference.ConvertToString( false );

                        // Person Attributes
                        var displayedAttributeGuids = GetPersonAttributeGuids( person.Id );
                        if ( !displayedAttributeGuids.Any() || personId == 0 )
                        {
                            pnlPersonAttributes.Visible = false;
                        }
                        else
                        {
                            pnlPersonAttributes.Visible = true;
                            DisplayEditAttributes( person, displayedAttributeGuids, phPersonAttributes, pnlPersonAttributes, true );
                        }

                        var mobilePhoneType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );

                        var phoneNumbers = new List<PhoneNumber>();
                        var phoneNumberTypes = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
                        var selectedPhoneTypeGuids = GetAttributeValue( "PhoneNumbers" ).Split( ',' ).AsGuidList();
                        if ( phoneNumberTypes.DefinedValues.Where( pnt => selectedPhoneTypeGuids.Contains( pnt.Guid ) ).Any() )
                        {
                            foreach ( var phoneNumberType in phoneNumberTypes.DefinedValues.Where( pnt => selectedPhoneTypeGuids.Contains( pnt.Guid ) ) )
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
            }

            pnlView.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Gets the person attribute guids.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        private List<Guid> GetPersonAttributeGuids( int personId )
        {
            GroupMemberService groupMemberService = new GroupMemberService( new RockContext() );
            List<Guid> attributeGuidList = new List<Guid>();
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
            var groupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            if ( groupMemberService.Queryable().Where( gm =>
               gm.PersonId == personId &&
               gm.Group.GroupType.Guid == groupTypeGuid &&
               gm.GroupRole.Guid == adultGuid ).Any() )
            {
                attributeGuidList = GetAttributeValue( "PersonAttributes(adults)" ).SplitDelimitedValues().AsGuidList();
            }
            else
            {
                attributeGuidList = GetAttributeValue( "PersonAttributes(children)" ).SplitDelimitedValues().AsGuidList();
            }

            return attributeGuidList;
        }

        /// <summary>
        /// Displays the edit attributes.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="displayedAttributeGuids">The displayed attribute guids.</param>
        /// <param name="phAttributes">The ph attributes.</param>
        /// <param name="pnlAttributes">The PNL attributes.</param>
        private void DisplayEditAttributes( IHasAttributes item, List<Guid> displayedAttributeGuids, PlaceHolder phAttributes, Panel pnlAttributes, bool setValue )
        {
            phAttributes.Controls.Clear();
            item.LoadAttributes();
            var excludedAttributeList = item.Attributes.Where( a => !displayedAttributeGuids.Contains( a.Value.Guid ) ).Select( a => a.Value.Key ).ToList();
            if ( item.Attributes != null && item.Attributes.Any() && displayedAttributeGuids.Any() )
            {
                pnlAttributes.Visible = true;
                Rock.Attribute.Helper.AddEditControls( item, phAttributes, setValue, BlockValidationGroup, excludedAttributeList, false, 2 );
            }
            else
            {
                pnlAttributes.Visible = false;
            }
        }

        /// <summary>
        /// Formats the phone number.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        protected string FormatPhoneNumber( object countryCode, object number )
        {
            string cc = countryCode as string ?? string.Empty;
            string n = number as string ?? string.Empty;
            return PhoneNumber.FormattedNumber( cc, n );
        }

        #endregion                          
    }
}