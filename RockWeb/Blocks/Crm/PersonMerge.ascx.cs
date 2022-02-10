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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility.Enums;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Merges two or more person records into one.
    /// </summary>
    [DisplayName( "Person Merge" )]
    [Category( "CRM" )]
    [Description( "Merges two or more person records into one." )]

    [SecurityAction( SecurityActionKey.ViewAllAttributes, "Grants permission to view all person attribute values." )]

    #region Block Attributes

    [BooleanField(
        "Reset Login Confirmation",
        Description = RESET_LOGIN_CONFIRMATION_DESCRIPTION,
        DefaultBooleanValue = true,
        Order = 0,
        Key = AttributeKey.ResetLoginConfirmation )]

    [LinkedPage(
        "Person Detail Page",
        Description = "The page to navigate to after the merge is completed.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.PersonDetailPage )]

    #endregion Block Attributes

    public partial class PersonMerge : Rock.Web.UI.RockBlock
    {
        #region Security Actions

        /// <summary>
        /// Keys to use for Security Actions.
        /// </summary>
        public static class SecurityActionKey
        {
            public const string ViewAllAttributes = "ViewAllAttributes";
        }

        #endregion

        #region Constants

        private const string FAMILY_VALUES = "FamilyValues";
        private const string FAMILY_NAME = "FamilyName";
        private const string BUSINESS_ATTRIBUTES = "BusinessAttributes";
        private const string CAMPUS = "Campus";
        private const string RESET_LOGIN_CONFIRMATION_DESCRIPTION = "When merging people with different emails, should their logins be updated to require reconfirmation of their email before allowing log in? This is typically enabled to prevent someone from maliciously obtaining login credentials by creating an account with same name but different login.";
        private const string BUSINESS_INFORMATION = "BusinessInformation";

        #endregion Constants

        #region Attribute Keys

        private class AttributeKey
        {
            public const string ResetLoginConfirmation = "ResetLoginConfirmation";
            public const string PersonDetailPage = "PersonDetailPage";
        }

        #endregion Attribute Keys

        #region Fields

        private readonly List<string> headingKeys = new List<string>
        {
            "PhoneNumbers",
            "Addresses",
            "PersonAttributes",
            "BusinessAttributes",
            "FamilyAttributes",
            FAMILY_VALUES,
            BUSINESS_INFORMATION,
            BUSINESS_ATTRIBUTES
    };

        #endregion

        #region Properties

        private MergeData MergeData { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            MergeData = ViewState["MergeData"] as MergeData;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gValues.DataKeyNames = new string[] { "PropertyKey" };
            gValues.AllowPaging = false;
            gValues.ShowActionRow = false;
            gValues.RowDataBound += gValues_RowDataBound;

            var resetConfirmation = string.Empty;
            if ( GetAttributeValue( AttributeKey.ResetLoginConfirmation ).AsBoolean() )
            {
                resetConfirmation = @"<br>Additionally, this person will be prompted to reconfirm before they can log in using the email address you select.";
            }

            nbSecurityNotice.Text = string.Format(
                @"There are two different emails associated with this merge, and at least one of the records has a login.  It is possible that the new record was created in an attempt to gain access to the account through the merge process. Since all email addresses are saved and used when searching for this person,<b> remove any invalid email address before you perform this merge </b>. {0}",
                resetConfirmation );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbPeople.Visible = false;
            nbError.Visible = false;

            bool canEdit = this.IsUserAuthorized( Authorization.EDIT );

            pnlEdit.Visible = canEdit;
            pnlView.Visible = !canEdit;

            if ( canEdit )
            {
                LoadEditDetails();
            }
            else
            {
                LoadViewDetails();
            }
        }

        /// <summary>
        /// Loads the view details.
        /// </summary>
        private void LoadViewDetails()
        {
            if ( Page.IsPostBack )
            {
                nbMergeRequestSuccess.Visible = false;
                nbMergeRequestAlreadySubmitted.Visible = false;
            }
            else
            {
                nbNotAuthorized.Visible = true;

                int? setId = PageParameter( "Set" ).AsIntegerOrNull();
                if ( setId.HasValue )
                {
                    // if the user only has View auth to the page, mark the EntitySet as a Person Merge Request and let them edit the EntitySet note
                    var rockContext = new RockContext();
                    var entitySetService = new EntitySetService( rockContext );
                    var entitySet = entitySetService.Get( setId.Value );
                    if ( entitySet != null )
                    {
                        tbEntitySetNote.Text = entitySet.Note;
                        var definedValuePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.ENTITY_SET_PURPOSE_PERSON_MERGE_REQUEST.AsGuid() );
                        if ( definedValuePurpose != null )
                        {
                            nbNotAuthorized.Visible = false;
                            tbEntitySetNote.Visible = true;
                            btnSaveRequestNote.Visible = true;

                            if ( entitySet.EntitySetPurposeValueId != definedValuePurpose.Id && entitySet.ExpireDateTime != null )
                            {
                                nbMergeRequestSuccess.Visible = true;
                                entitySet.EntitySetPurposeValueId = definedValuePurpose.Id;
                                entitySet.ExpireDateTime = null;
                                rockContext.SaveChanges();
                            }
                            else
                            {
                                nbMergeRequestAlreadySubmitted.Visible = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the edit details.
        /// </summary>
        private void LoadEditDetails()
        {
            if ( !Page.IsPostBack )
            {
                List<int> selectedPersonIds = null;

                // Process Query String parameter "Set", specifying a set of people to merge.
                int? setId = PageParameter( "Set" ).AsIntegerOrNull();

                if ( setId.HasValue )
                {
                    selectedPersonIds = new EntitySetItemService( new RockContext() )
                        .GetByEntitySetId( setId.Value, true )
                        .Select( i => i.EntityId )
                        .Distinct()
                        .ToList();
                }

                // Process Query String parameter "PersonId", specifying a delimited list of people to merge.
                var personIdList = PageParameter( "PersonId" );

                if ( personIdList.IsNotNullOrWhiteSpace() )
                {
                    selectedPersonIds = personIdList.SplitDelimitedValues().AsIntegerList();
                }

                // Load the set of people specified by query string parameters.
                if ( selectedPersonIds != null )
                {
                    foreach ( var personId in selectedPersonIds )
                    {
                        PersonService.UpdateAccountProtectionProfileForPerson( personId, new RockContext() );
                    }

                    if ( selectedPersonIds.Count == 0 )
                    {
                        ScriptManager.RegisterStartupScript( this, this.GetType(), "goBack", "history.go(-1);", true );
                    }

                    // Get the selected people.
                    var people = new PersonService( new RockContext() )
                        .Queryable( new PersonService.PersonQueryOptions
                        {
                            IncludeDeceased = true,
                            IncludeNameless = true,
                        } )
                        .Include( a => a.CreatedByPersonAlias.Person )
                        .Include( a => a.Users )
                        .Where( p => selectedPersonIds.Contains( p.Id ) )
                        .ToList();

                    ppAdd.Visible = !people.All( a => a.IsBusiness() );

                    // Create the data structure used to build the grid.
                    MergeData = new MergeData( people, headingKeys, CurrentPerson, IsUserAuthorized( PersonMerge.SecurityActionKey.ViewAllAttributes ) );

                    if ( setId != null )
                    {
                        MergeData.EntitySetId = setId.Value;
                    }

                    // If a Person Id list has been specified as a query parameter, select the first person in the list as the merge target.
                    if ( personIdList.IsNotNullOrWhiteSpace() )
                    {
                        MergeData.PrimaryPersonId = selectedPersonIds.FirstOrDefault();
                    }

                    BuildColumns();
                    BindGrid();
                }
            }
            else
            {
                var selectedPrimaryPersonId = hfSelectedColumnPersonId.Value.AsIntegerOrNull();

                // Save the primary header radio button's selection
                foreach ( var col in gValues.Columns.OfType<MergePersonField>() )
                {
                    col.OnDelete += personCol_OnDelete;
                    if ( selectedPrimaryPersonId.HasValue && selectedPrimaryPersonId.Value == col.PersonId )
                    {
                        MergeData.PrimaryPersonId = col.PersonId;
                    }
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["MergeData"] = MergeData ?? new MergeData();
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectPerson event of the ppAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAdd_SelectPerson( object sender, EventArgs e )
        {
            int? personId = ppAdd.PersonId;
            if ( personId.HasValue && ( MergeData == null || !MergeData.People.Any( p => p.Id == personId.Value ) ) )
            {
                var selectedPersonIds = MergeData != null ? MergeData.People.Select( p => p.Id ).ToList() : new List<int>();
                selectedPersonIds.Add( personId.Value );

                PersonService.UpdateAccountProtectionProfileForPerson( personId.Value, new RockContext() );

                // Get the people selected
                var people = new PersonService( new RockContext() ).Queryable( "CreatedByPersonAlias.Person,Users" )
                    .Where( p => selectedPersonIds.Contains( p.Id ) )
                    .ToList();

                // Rebuild mergdata, columns, and grid
                MergeData = new MergeData( people, headingKeys, CurrentPerson, IsUserAuthorized( PersonMerge.SecurityActionKey.ViewAllAttributes ) );
                BuildColumns();
                BindGrid();
            }

            ppAdd.SetValue( null );
        }

        /// <summary>
        /// Handles the OnDelete event of the personCol control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void personCol_OnDelete( object sender, EventArgs e )
        {
            var personMergeField = sender as MergePersonField;
            if ( personMergeField != null )
            {
                var selectedPersonIds = MergeData.People
                    .Where( p => p.Id != personMergeField.PersonId )
                    .Select( p => p.Id ).ToList();

                // Get the people selected
                var people = new PersonService( new RockContext() ).Queryable( "CreatedByPersonAlias.Person,Users" )
                    .Where( p => selectedPersonIds.Contains( p.Id ) )
                    .ToList();

                // Rebuild mergedata, columns, and grid
                MergeData = new MergeData( people, headingKeys, CurrentPerson, IsUserAuthorized( PersonMerge.SecurityActionKey.ViewAllAttributes ) );
                BuildColumns();
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the DataBound event of the personCol control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="personMergeFieldRowEventArgs">The <see cref="MergePersonField.MergePersonFieldRowEventArgs"/> instance containing the event data.</param>
        private void personCol_DataBound( object sender, MergePersonField.MergePersonFieldRowEventArgs personMergeFieldRowEventArgs )
        {
            int personId = personMergeFieldRowEventArgs.MergePersonField.PersonId;
            ValuesRow rowValue = personMergeFieldRowEventArgs.Row.DataItem as ValuesRow;
            ValuesRowPersonPersonProperty valuesRowPersonPersonProperty = rowValue.PersonPersonPropertyList.FirstOrDefault( a => a.Person.Id == personId );
            if ( rowValue.IsSectionHeading )
            {
                personMergeFieldRowEventArgs.SelectionControlType = MergePersonField.SelectionControlType.None;
                personMergeFieldRowEventArgs.ContentHTML = rowValue.PropertyLabel;
                return;
            }

            if ( valuesRowPersonPersonProperty == null )
            {
                return;
            }

            if ( rowValue.PersonProperty.Attribute != null && rowValue.PersonProperty.Attribute.FieldType.Field is Rock.Field.Types.MatrixFieldType )
            {
                personMergeFieldRowEventArgs.SelectionControlType = MergePersonField.SelectionControlType.Checkbox;
                personMergeFieldRowEventArgs.ContentDisplayType = MergePersonField.ContentDisplayType.ContentWrapper;
            }
            else
            {
                personMergeFieldRowEventArgs.SelectionControlType = MergePersonField.SelectionControlType.RadioButton;
                personMergeFieldRowEventArgs.ContentDisplayType = MergePersonField.ContentDisplayType.SelectionLabel;
            }

            personMergeFieldRowEventArgs.ContentHTML = valuesRowPersonPersonProperty.PersonPropertyValue.FormattedValue;
            personMergeFieldRowEventArgs.Selected = valuesRowPersonPersonProperty.PersonPropertyValue.Selected;
        }

        /// <summary>
        /// Handles the RowDataBound event of the gValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gValues_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                if ( headingKeys.Contains( gValues.DataKeys[e.Row.RowIndex].Value.ToString() ) )
                {
                    e.Row.AddCssClass( "grid-section-header" );
                }
                else
                {
                    e.Row.Cells[1].AddCssClass( "grid-row-header" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbMerge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMerge_Click( object sender, EventArgs e )
        {
            if ( !CanMerge().IsAllowedToMerge )
            {
                nbError.Heading = "Merge Error";
                nbError.Text = string.Format( "<p>You do not have the necessary permissions to merge this user.</p>" );
                nbError.Visible = true;
                return;
            }

            /*
            01/02/2020 - SK
            Similar code is used in ExpungePerson in PersonService class
            https://github.com/SparkDevNetwork/Rock/blob/develop/Rock/Model/CodeGenerated/PersonService.cs
            and might also to consider for any future changes made to current method.
            */

            if ( MergeData.People.Count < 2 )
            {
                nbPeople.Visible = true;
                return;
            }

            if ( MergeDataIncludesAnonymousGiver() )
            {
                nbError.Heading = "Merge Error";
                nbError.Text = string.Format( "<p>You can't merge the Anonymous Giver unless it is the primary selected person.</p>" );
                nbError.Visible = true;
                return;
            }

            bool reconfirmRequired =
                GetAttributeValue( AttributeKey.ResetLoginConfirmation ).AsBoolean() &&
                MergeData.People.Select( p => p.Email ).Distinct().Count() > 1 &&
                MergeData.People.Where( p => p.HasLogins ).Any();

            bool isBusiness = MergeData.People.Any( a => a.IsBusiness );

            GetValuesSelection();

            int? primaryPersonId = null;

            var oldPhotos = new List<int>();

            var rockContext = new RockContext();

            try
            {
                rockContext.WrapTransaction( () =>
                {
                    var personService = new PersonService( rockContext );
                    var userLoginService = new UserLoginService( rockContext );
                    var groupService = new GroupService( rockContext );
                    var groupMemberService = new GroupMemberService( rockContext );
                    var binaryFileService = new BinaryFileService( rockContext );
                    var phoneNumberService = new PhoneNumberService( rockContext );
                    var taggedItemService = new TaggedItemService( rockContext );
                    var personSearchKeyService = new PersonSearchKeyService( rockContext );

                    Person primaryPerson = personService.Get( MergeData.PrimaryPersonId ?? 0 );
                    if ( primaryPerson != null )
                    {
                        primaryPersonId = primaryPerson.Id;

                        // Write a history record about the merge
                        var changes = new History.HistoryChangeList();
                        foreach ( var p in MergeData.People.Where( p => p.Id != primaryPerson.Id ) )
                        {
                            changes.AddChange( History.HistoryVerb.Merge, History.HistoryChangeType.Record, string.Format( "{0} [ID: {1}]", p.FullName, p.Id ) );
                        }

                        HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), primaryPerson.Id, changes );

                        // Photo Id
                        primaryPerson.PhotoId = MergeData.GetSelectedValue( MergeData.GetProperty( "Photo" ) ).Value.AsIntegerOrNull();
                        primaryPerson.TitleValueId = GetNewIntValue( "Title" );
                        if ( !isBusiness )
                        {
                            primaryPerson.FirstName = GetNewStringValue( "FirstName" );
                            primaryPerson.NickName = GetNewStringValue( "NickName" );
                            primaryPerson.MiddleName = GetNewStringValue( "MiddleName" );
                            primaryPerson.SuffixValueId = GetNewIntValue( "Suffix" );
                        }

                        primaryPerson.LastName = GetNewStringValue( "LastName" );
                        primaryPerson.RecordTypeValueId = GetNewIntValue( "RecordType" );
                        primaryPerson.RecordStatusValueId = GetNewIntValue( "RecordStatus" );
                        primaryPerson.RecordStatusReasonValueId = GetNewIntValue( "RecordStatusReason" );
                        primaryPerson.ConnectionStatusValueId = GetNewIntValue( "ConnectionStatus" );
                        primaryPerson.IsDeceased = GetNewBoolValue( "Deceased" ) ?? false;
                        primaryPerson.Gender = ( Gender ) GetNewEnumValue( "Gender", typeof( Gender ) );
                        primaryPerson.MaritalStatusValueId = GetNewIntValue( "MaritalStatus" );
                        primaryPerson.SetBirthDate( GetNewDateTimeValue( "BirthDate" ) );
                        primaryPerson.AnniversaryDate = GetNewDateTimeValue( "AnniversaryDate" );
                        primaryPerson.GraduationYear = GetNewIntValue( "GraduationYear" );
                        primaryPerson.Email = GetNewStringValue( "Email" );
                        primaryPerson.IsEmailActive = GetNewBoolValue( "EmailActive" ) ?? true;
                        primaryPerson.EmailNote = GetNewStringValue( "EmailNote" );
                        primaryPerson.EmailPreference = ( EmailPreference ) GetNewEnumValue( "EmailPreference", typeof( EmailPreference ) );
                        primaryPerson.InactiveReasonNote = GetNewStringValue( "InactiveReasonNote" );
                        primaryPerson.SystemNote = GetNewStringValue( "SystemNote" );
                        primaryPerson.ContributionFinancialAccountId = GetNewIntValue( "ContributionFinancialAccountId" );

                        primaryPerson.CreatedDateTime = MergeData.People
                                                        .Min( a => a.CreatedDateTime );

                        // Update phone numbers
                        var phoneTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).DefinedValues;
                        foreach ( var phoneType in phoneTypes )
                        {
                            var phoneNumber = primaryPerson.PhoneNumbers.Where( p => p.NumberTypeValueId == phoneType.Id ).FirstOrDefault();
                            string oldValue = phoneNumber != null ? phoneNumber.Number : string.Empty;

                            string key = "phone_" + phoneType.Id.ToString();
                            string newValue = GetNewStringValue( key );
                            bool phoneNumberDeleted = false;

                            if ( !oldValue.Equals( newValue, StringComparison.OrdinalIgnoreCase ) )
                            {
                                // New phone doesn't match old
                                if ( !string.IsNullOrWhiteSpace( newValue ) )
                                {
                                    // New value exists
                                    if ( phoneNumber == null )
                                    {
                                        // Old value didn't exist... create new phone record
                                        phoneNumber = new PhoneNumber { NumberTypeValueId = phoneType.Id };
                                        primaryPerson.PhoneNumbers.Add( phoneNumber );
                                    }

                                    // Update phone number
                                    phoneNumber.Number = newValue;
                                }
                                else
                                {
                                    // New value doesn't exist
                                    if ( phoneNumber != null )
                                    {
                                        // old value existed.. delete it
                                        primaryPerson.PhoneNumbers.Remove( phoneNumber );
                                        phoneNumberService.Delete( phoneNumber );
                                        phoneNumberDeleted = true;
                                    }
                                }
                            }

                            // check to see if IsMessagingEnabled is true for any of the merged people for this number/numbertype
                            if ( phoneNumber != null && !phoneNumberDeleted && !phoneNumber.IsMessagingEnabled )
                            {
                                var personIds = MergeData.People.Select( a => a.Id ).ToList();
                                var isMessagingEnabled = phoneNumberService.Queryable().Where( a => personIds.Contains( a.PersonId ) && a.Number == phoneNumber.Number && a.NumberTypeValueId == phoneNumber.NumberTypeValueId ).Any( a => a.IsMessagingEnabled );
                                if ( isMessagingEnabled )
                                {
                                    phoneNumber.IsMessagingEnabled = true;
                                }
                            }
                        }

                        // Save the new record
                        rockContext.SaveChanges();

                        // Update the attributes
                        primaryPerson.LoadAttributes( rockContext );
                        foreach ( var property in MergeData.Properties.Where( p => p.Key.StartsWith( "attr_" ) ) )
                        {
                            var attribute = AttributeCache.Get( property.AttributeId.Value );
                            if ( attribute.FieldType.Field is Rock.Field.Types.MatrixFieldType )
                            {
                                MergeAttributeMatrixAttributeValues( rockContext, primaryPerson, property, attribute );
                            }
                            else
                            {
                                string oldValue = primaryPerson.GetAttributeValue( attribute.Key ) ?? string.Empty;
                                string newValue = GetNewStringValue( property.Key ) ?? string.Empty;

                                if ( !oldValue.Equals( newValue ) )
                                {
                                    Rock.Attribute.Helper.SaveAttributeValue( primaryPerson, attribute, newValue, rockContext );
                                }
                            }
                        }

                        // Update the Primary Family.
                        var primaryFamily = primaryPerson.GetFamily( rockContext );

                        if ( primaryFamily != null )
                        {
                            // Update the family attributes.
                            primaryFamily.Name = GetNewStringValue( FAMILY_NAME );
                            primaryFamily.CampusId = GetNewIntValue( CAMPUS );

                            primaryFamily.LoadAttributes( rockContext );
                            foreach ( var property in MergeData.Properties.Where( p => p.Key.StartsWith( "groupattr_" ) ) )
                            {
                                string attributeKey = AttributeCache.Get( property.AttributeId.Value ).Key;
                                string oldValue = primaryFamily.GetAttributeValue( attributeKey ) ?? string.Empty;
                                string newValue = GetNewStringValue( property.Key ) ?? string.Empty;

                                if ( !oldValue.Equals( newValue ) )
                                {
                                    var attribute = primaryFamily.Attributes[attributeKey];
                                    Rock.Attribute.Helper.SaveAttributeValue( primaryFamily, attribute, newValue, rockContext );
                                }
                            }

                            // Update Addresses.
                            MergeAddresses( rockContext, primaryFamily );
                        }

                        // Delete the unselected photos
                        string photoKeeper = primaryPerson.PhotoId.HasValue ? primaryPerson.PhotoId.Value.ToString() : string.Empty;
                        foreach ( var photoValue in MergeData.Properties
                            .Where( p => p.Key == "Photo" )
                            .SelectMany( p => p.Values )
                            .Where( v => v.Value != string.Empty && v.Value != photoKeeper )
                            .Select( v => v.Value ) )
                        {
                            int photoId = 0;
                            if ( int.TryParse( photoValue, out photoId ) )
                            {
                                var photo = binaryFileService.Get( photoId );
                                if ( photo != null )
                                {
                                    string errorMessages;
                                    if ( binaryFileService.CanDelete( photo, out errorMessages ) )
                                    {
                                        binaryFileService.Delete( photo );
                                    }
                                }
                            }
                        }

                        rockContext.SaveChanges();

                        // If there was more than one email address and user has logins, then set any of the local
                        // logins ( database & AD ) to require a reconfirmation
                        if ( reconfirmRequired )
                        {
                            var personIds = MergeData.People.Select( a => a.Id ).ToList();
                            foreach ( var login in userLoginService.Queryable()
                                .Where( l =>
                                    l.PersonId.HasValue &&
                                    personIds.Contains( l.PersonId.Value ) ) )
                            {
                                var component = Rock.Security.AuthenticationContainer.GetComponent( login.EntityType.Name );
                                if ( component != null && !component.RequiresRemoteAuthentication )
                                {
                                    login.IsConfirmed = false;
                                }
                            }
                        }

                        rockContext.SaveChanges();

                        // Merge search keys on merge
                        var searchTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL.AsGuid() );
                        var personSearchKeys = primaryPerson.GetPersonSearchKeys( rockContext ).Where( a => a.SearchTypeValueId == searchTypeValue.Id ).ToList();
                        foreach ( var p in MergeData.People.Where( p => p.Id != primaryPersonId.Value ) )
                        {
                            if ( !string.IsNullOrEmpty( p.Email ) && p.Email != GetNewStringValue( "Email" ) && !personSearchKeys.Any( a => a.SearchValue.Equals( p.Email, StringComparison.OrdinalIgnoreCase ) ) )
                            {
                                PersonSearchKey personSearchKey = new PersonSearchKey()
                                {
                                    PersonAliasId = primaryPerson.PrimaryAliasId.Value,
                                    SearchTypeValueId = searchTypeValue.Id,
                                    SearchValue = p.Email
                                };
                                personSearchKeyService.Add( personSearchKey );
                                rockContext.SaveChanges();
                            }

                            var mergeSearchKeys = personService.GetPersonSearchKeys( p.Id ).Where( a => a.SearchTypeValueId == searchTypeValue.Id ).ToList();
                            var duplicateKeys = mergeSearchKeys.Where( a => personSearchKeys.Any( b => b.SearchValue.Equals( a.SearchValue, StringComparison.OrdinalIgnoreCase ) ) );

                            if ( duplicateKeys.Any() )
                            {
                                personSearchKeyService.DeleteRange( duplicateKeys );
                                rockContext.SaveChanges();
                            }
                        }

                        // Delete merged person's family records and any families that would be empty after merge
                        foreach ( var p in MergeData.People.Where( p => p.Id != primaryPersonId.Value ) )
                        {
                            // Delete the merged person's phone numbers (we've already updated the primary persons values)
                            foreach ( var phoneNumber in phoneNumberService.GetByPersonId( p.Id ) )
                            {
                                phoneNumberService.Delete( phoneNumber );
                            }

                            rockContext.SaveChanges();

                            // Delete the merged person's other family member records and the family if they were the only one in the family
                            Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                            foreach ( var familyMember in groupMemberService.Queryable().Where( m => m.PersonId == p.Id && m.Group.GroupType.Guid == familyGuid ) )
                            {
                                groupMemberService.Delete( familyMember );

                                rockContext.SaveChanges();

                                // Get the family
                                var family = groupService.Queryable( "Members" ).Where( f => f.Id == familyMember.GroupId ).FirstOrDefault();
                                if ( !family.Members.Any() )
                                {
                                    // If there are not any other family members, delete the family record.

                                    // If theres any people that have this group as a giving group, set it to null (the person being merged should be the only one)
                                    foreach ( Person gp in personService.Queryable().Where( g => g.GivingGroupId == family.Id ) )
                                    {
                                        gp.GivingGroupId = null;
                                    }

                                    // save to the database prior to doing groupService.Delete since .Delete quietly might not delete if thinks the Family is used for a GivingGroupId
                                    rockContext.SaveChanges();

                                    // Delete the family
                                    string errorMessage;
                                    if ( groupService.CanDelete( family, out errorMessage ) )
                                    {
                                        groupService.Delete( family );
                                        rockContext.SaveChanges();
                                    }
                                }
                            }
                        }

                        // Flush any security roles that the merged person's other records were a part of
                        foreach ( var p in MergeData.People.Where( p => p.Id != primaryPersonId.Value ) )
                        {
                            foreach ( var groupMember in groupMemberService.Queryable().Where( m => m.PersonId == p.Id ) )
                            {
                                Group group = new GroupService( rockContext ).Get( groupMember.GroupId );
                                if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                                {
                                    RoleCache.Remove( group.Id );
                                    Rock.Security.Authorization.Clear();
                                }
                            }
                        }

                        // If merging records into the Anonymous Giver record, remove any UserLogins.
                        bool mergingWithAnonymousGiver = primaryPerson.Guid == Rock.SystemGuid.Person.GIVER_ANONYMOUS.AsGuid();
                        if ( mergingWithAnonymousGiver )
                        {
                            RemoveAnonymousGiverUserLogins( userLoginService, rockContext );
                        }

                        // now that the Merge is complete, the EntitySet can be marked to be deleted by the RockCleanup job
                        var entitySetService = new EntitySetService( rockContext );
                        var entitySet = entitySetService.Get( MergeData.EntitySetId );
                        if ( entitySet != null )
                        {
                            entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( -1 );
                            entitySet.EntitySetPurposeValueId = null;
                            rockContext.SaveChanges();
                        }
                    }
                } );

                foreach ( var p in MergeData.People.Where( p => p.Id != primaryPersonId.Value ) )
                {
                    // Run merge proc to merge all associated data
                    var parms = new Dictionary<string, object>();
                    parms.Add( "OldId", p.Id );
                    parms.Add( "NewId", primaryPersonId.Value );
                    DbService.ExecuteCommand( "spCrm_PersonMerge", CommandType.StoredProcedure, parms );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );

                nbError.Heading = "Merge Error";
                nbError.Text = string.Format( "<p>The following error occurred when attempting the merge: {0}</p>", ex.Message );
                nbError.Visible = true;
                return;
            }

            NavigateToLinkedPage( AttributeKey.PersonDetailPage, isBusiness ? "BusinessId" : "PersonId", primaryPersonId.Value );
        }

        /// <summary>
        /// Merge the selected Addresses into the merge target.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="primaryPerson"></param>
        private void MergeAddresses( RockContext rockContext, Group primaryFamily )
        {
            if ( primaryFamily == null )
            {
                return;
            }

            // Process the Address entry for each Address Type.
            var addressTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() ).DefinedValues;

            var locationService = new LocationService( rockContext );
            var groupLocationService = new GroupLocationService( rockContext );

            foreach ( var addressType in addressTypes )
            {
                GroupLocation mergeSourceFamilyLocation = null;

                var key = "address_" + addressType.Id.ToString();
                var keyPrefix = key + "_";

                // Get all of the property keys that correspond to addresses of this type.
                var addressKeys = MergeData.Properties.Where( p => p.Key == key || p.Key.StartsWith( keyPrefix, StringComparison.OrdinalIgnoreCase ) ).Select( x => x.Key ).ToList();

                foreach ( var addressKey in addressKeys )
                {
                    /*
                     * 12/12/2019 BJW
                     *
                     * There was a bug around address merge if the primary person (person to keep) has a later creation date than the other person.
                     * In that case, the address properties that are not displayed in the UI (because neither person has an address of that type)
                     * have the older person record's address property "selected". This caused the code to try to merge the address, but it
                     * didn't exist. The solution was to not call groupLocationService.Delete( currentTargetFamilyLocation ) if the
                     * currentTargetFamilyLocation is null.  Furthermore, there is no need to do anything if all of the address property values
                     * are empty (for example: no one has a work address).
                     *
                     * Task: https://app.asana.com/0/1120115219297347/1153049097899625/f
                     */

                    // Get the current value for the merge target.
                    var property = MergeData.GetProperty( addressKey );

                    // If there are no values for this address type then no action is required
                    if ( property.Values.All( v => v.Value.IsNullOrWhiteSpace() ) )
                    {
                        continue;
                    }

                    var primaryPersonGroupLocationValue = property.Values.Where( v => v.PersonId == MergeData.PrimaryPersonId ).FirstOrDefault();

                    // If the merge target address is selected, there is no need to process this entry.
                    if ( primaryPersonGroupLocationValue.Selected )
                    {
                        continue;
                    }

                    // Get the updated value for this merge address.
                    var newValueId = GetNewIntValue( addressKey );

                    if ( newValueId != null )
                    {
                        mergeSourceFamilyLocation = groupLocationService.Get( newValueId.Value );
                    }

                    GroupLocation currentTargetFamilyLocation = null;

                    if ( primaryPersonGroupLocationValue.Value.IsNotNullOrWhiteSpace() )
                    {
                        currentTargetFamilyLocation = primaryFamily.GroupLocations.FirstOrDefault( p => p.Id == primaryPersonGroupLocationValue.Value.AsInteger() );
                    }

                    // Compare the address components to determine if an update is required.
                    var isUpdated = true;

                    if ( currentTargetFamilyLocation != null )
                    {
                        var targetLocation = currentTargetFamilyLocation.Location;

                        Location sourceLocation = null;

                        if ( mergeSourceFamilyLocation != null )
                        {
                            sourceLocation = mergeSourceFamilyLocation.Location;

                            if ( targetLocation.Id == sourceLocation.Id )
                            {
                                isUpdated = false;
                            }
                            else if ( sourceLocation.Street1.ToStringSafe() == targetLocation.Street1.ToStringSafe()
                                 && sourceLocation.Street2.ToStringSafe() == targetLocation.Street2.ToStringSafe()
                                 && sourceLocation.City.ToStringSafe() == targetLocation.City.ToStringSafe()
                                 && sourceLocation.County.ToStringSafe() == targetLocation.County.ToStringSafe()
                                 && sourceLocation.State.ToStringSafe() == targetLocation.State.ToStringSafe()
                                 && sourceLocation.PostalCode.ToStringSafe() == targetLocation.PostalCode.ToStringSafe()
                                 && sourceLocation.Country.ToStringSafe() == targetLocation.Country.ToStringSafe() )
                            {
                                isUpdated = false;
                            }
                        }
                    }

                    if ( isUpdated )
                    {
                        if ( mergeSourceFamilyLocation == null )
                        {
                            // Remove the address if it exists.
                            if ( currentTargetFamilyLocation != null )
                            {
                                primaryFamily.GroupLocations.Remove( currentTargetFamilyLocation );
                                groupLocationService.Delete( currentTargetFamilyLocation );
                            }
                        }
                        else
                        {
                            // Update the existing address.
                            var prevLocType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );

                            GroupLocation newTargetFamilyLocation = new GroupLocation();

                            var newGroupLocationId = 0;
                            var newGroupLocationGuid = Guid.NewGuid();

                            if ( currentTargetFamilyLocation != null )
                            {
                                // A Family Address of this Type already exists in the target.
                                if ( prevLocType != null )
                                {
                                    // Change the existing address to a previous address.
                                    currentTargetFamilyLocation.GroupLocationTypeValue = null;
                                    currentTargetFamilyLocation.GroupLocationTypeValueId = prevLocType.Id;

                                    newTargetFamilyLocation = new GroupLocation();
                                }
                                else
                                {
                                    // No Previous Address Type is available, so just update the current address.
                                    newTargetFamilyLocation = currentTargetFamilyLocation;

                                    newGroupLocationId = currentTargetFamilyLocation.Id;
                                    newGroupLocationGuid = currentTargetFamilyLocation.Guid;
                                }
                            }
                            else
                            {
                                newTargetFamilyLocation = new GroupLocation();
                            }

                            newTargetFamilyLocation.CopyPropertiesFrom( mergeSourceFamilyLocation );

                            // Set the appropriate identifiers for this record.
                            newTargetFamilyLocation.Id = newGroupLocationId;
                            newTargetFamilyLocation.Guid = newGroupLocationGuid;

                            // If this is a new location, associate it with the Family.
                            if ( newTargetFamilyLocation.Id == 0 )
                            {
                                primaryFamily.GroupLocations.Add( newTargetFamilyLocation );
                            }
                        }
                    }
                }
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Merges the attribute matrix attribute values' Items into one AttributeMatrixValue
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="primaryPerson">The primary person.</param>
        /// <param name="property">The property.</param>
        /// <param name="attribute">The attribute.</param>
        private void MergeAttributeMatrixAttributeValues( RockContext rockContext, Person primaryPerson, PersonProperty property, AttributeCache attribute )
        {
            var attributeMatrixService = new AttributeMatrixService( rockContext );
            AttributeMatrix primaryPersonAttributeMatrix = null;
            var primaryPersonAttributeMatrixGuid = primaryPerson.GetAttributeValue( attribute.Key ).AsGuidOrNull();
            if ( primaryPersonAttributeMatrixGuid.HasValue )
            {
                primaryPersonAttributeMatrix = attributeMatrixService.Get( primaryPersonAttributeMatrixGuid.Value );
            }

            var selectedPersonAttributeMatrixList = new List<AttributeMatrix>();

            Guid? newPersonAttributeMatrixGuid = null;

            // Get the set of Attribute Matrix references selected for the merge and ensure they are valid.
            var selectedAttributeMatrixGuidList = GetSelectedValues( property.Key ).Select( a => a.Value ).AsGuidList();

            var selectedAttributeMatrixList = attributeMatrixService.GetByGuids( selectedAttributeMatrixGuidList ).ToList();

            if ( selectedAttributeMatrixList.Count > 1 )
            {
                int attributeMatrixTemplateId;
                if ( primaryPersonAttributeMatrix != null )
                {
                    attributeMatrixTemplateId = primaryPersonAttributeMatrix.AttributeMatrixTemplateId;
                }
                else
                {
                    attributeMatrixTemplateId = selectedAttributeMatrixList.Select( a => a.AttributeMatrixTemplateId ).FirstOrDefault();
                }

                // If a valid Attribute Matrix exists, merge all of the values into it.
                if ( attributeMatrixTemplateId > 0 )
                {
                    // Create a new Attribute Matrix instance and assign a Guid so the Attribute Values can be linked to it.
                    // We can't use SaveChanges() to get a server-generated Guid here, because it will cause a deadlock in the merge transaction.
                    newPersonAttributeMatrixGuid = Guid.NewGuid();

                    var newPersonAttributeMatrix = new AttributeMatrix()
                    {
                        AttributeMatrixTemplateId = attributeMatrixTemplateId,
                        Guid = newPersonAttributeMatrixGuid.Value
                    };

                    var combinedMatrixItems = selectedAttributeMatrixList.SelectMany( a => a.AttributeMatrixItems ).ToList();

                    newPersonAttributeMatrix.AttributeMatrixItems = combinedMatrixItems;

                    attributeMatrixService.Add( newPersonAttributeMatrix );
                }
            }
            else if ( selectedAttributeMatrixList.Count == 1 )
            {
                newPersonAttributeMatrixGuid = selectedAttributeMatrixList.First().Guid;
            }

            if ( primaryPersonAttributeMatrixGuid != newPersonAttributeMatrixGuid )
            {
                Rock.Attribute.Helper.SaveAttributeValue( primaryPerson, attribute, newPersonAttributeMatrixGuid.ToString(), rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveRequestNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveRequestNote_Click( object sender, EventArgs e )
        {
            int? setId = PageParameter( "Set" ).AsIntegerOrNull();
            if ( setId.HasValue )
            {
                var rockContext = new RockContext();
                var entitySet = new EntitySetService( rockContext ).Get( setId.Value );
                entitySet.Note = tbEntitySetNote.Text;
                rockContext.SaveChanges();

                nbNoteSavedSuccess.Visible = true;
                tbEntitySetNote.Visible = false;
                btnSaveRequestNote.Visible = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the values columns.
        /// </summary>
        private void BuildColumns()
        {
            gValues.Columns.Clear();

            if ( MergeData != null && MergeData.People != null && MergeData.People.Any() )
            {
                var keyCol = new BoundField();
                keyCol.DataField = "PropertyKey";
                keyCol.Visible = false;
                gValues.Columns.Add( keyCol );

                var labelCol = new BoundField();
                labelCol.DataField = "PropertyLabel";
                ////labelCol.HeaderStyle.CssClass = "grid-section-header";
                gValues.Columns.Add( labelCol );

                foreach ( var person in MergeData.People )
                {
                    var personCol = new MergePersonField();
                    personCol.DataBound += personCol_DataBound;
                    personCol.PersonId = person.Id;
                    personCol.PersonName = person.FullName;
                    personCol.ID = "person_" + person.Id;
                    personCol.HeaderContent = GetValuesColumnHeader( person.Id, person.IsBusiness, person.IsNameless );
                    personCol.ModifiedDateTime = person.ModifiedDateTime;
                    personCol.ModifiedBy = person.ModifiedBy;
                    personCol.OnDelete += personCol_OnDelete;
                    gValues.Columns.Add( personCol );
                }
            }
        }

        /// <summary>
        /// Gets the values column header.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="isBusiness">Whether it should be business?</param>
        /// <returns></returns>
        private string GetValuesColumnHeader( int personId, bool isBusiness, bool isNameless )
        {
            Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

            var groupMemberService = new GroupMemberService( new RockContext() );
            var families = groupMemberService.Queryable()
                .Where( m => m.PersonId == personId && m.Group.GroupType.Guid == familyGuid )
                .Select( m => m.Group )
                .Distinct()
                .ToList();

            StringBuilder sbHeaderData = new StringBuilder();

            if ( families.Count > 1 )
            {
                sbHeaderData.Append( "<div class='js-person-header js-person-has-multiple-families'>" );
            }
            else
            {
                sbHeaderData.Append( "<div class='js-person-header'>" );
            }

            if ( isNameless )
            {
                sbHeaderData.Append( "<div class='merge-heading-family'>Nameless Person</div>" );
            }

            foreach ( var family in families )
            {
                sbHeaderData.Append( "<div class='merge-heading-family'>" );

                List<string> nickNames = new List<string>();
                if ( !isBusiness )
                {
                    nickNames = groupMemberService.Queryable( "Person" )
                    .Where( m => m.GroupId == family.Id )
                    .OrderBy( m => m.GroupRole.Order )
                    .ThenBy( m => m.Person.BirthDate ?? DateTime.MinValue )
                    .ThenByDescending( m => m.Person.Gender )
                    .Select( m => m.Person.NickName )
                    .ToList();
                }

                if ( nickNames.Any() )
                {
                    sbHeaderData.AppendFormat( "{0} ({1})", family.Name, nickNames.AsDelimited( ", " ) );
                }
                else
                {
                    sbHeaderData.Append( family.Name );
                }

                bool showType = family.GroupLocations.Count() > 1;
                foreach ( var loc in family.GroupLocations )
                {
                    sbHeaderData.AppendFormat(
                        " <br><span>{0}{1}</span>",
                        loc.Location.ToStringSafe(),
                        showType ? " (" + loc.GroupLocationTypeValue.Value + ")" : string.Empty );
                }

                sbHeaderData.Append( "</div>" );
            }

            sbHeaderData.Append( "<div>" );

            return sbHeaderData.ToString();
        }

        private class CanMergeResult
        {
            public bool IsAllowedToMerge { get; set; }

            public AccountProtectionProfile MaxAccountProtectionProfile { get; set; }

            public RoleCache RequiredSecurityRole { get; set; }

            public ElevatedSecurityLevel MaxElevatedSecurityLevel { get; internal set; }
        }

        private CanMergeResult CanMerge()
        {
            var maxAccountProtectionProfile = MergeData.People.Max( p => p.AccountProtectionProfile );
            var securitySettingService = new SecuritySettingsService();
            RoleCache requiredSecurityRole = null;
            securitySettingService.SecuritySettings.AccountProtectionProfileSecurityGroup.TryGetValue( maxAccountProtectionProfile, out requiredSecurityRole );

            var personIds = MergeData.People.Select( a => a.Id ).ToArray();

            var maxElevatedSecurityLevel = new GroupMemberService( new RockContext() )
                .Queryable()
                .IsInSecurityRoleGroupOrSecurityRoleGroupType()
                .Where( a => a.Group.IsActive && personIds.Contains( a.PersonId ) && a.GroupMemberStatus == GroupMemberStatus.Active )
                .Max( a => ( ElevatedSecurityLevel? ) a.Group.ElevatedSecurityLevel ) ?? ElevatedSecurityLevel.None;

            var canMergeResult = new CanMergeResult
            {
                MaxAccountProtectionProfile = maxAccountProtectionProfile,
                MaxElevatedSecurityLevel = maxElevatedSecurityLevel,
                RequiredSecurityRole = requiredSecurityRole,
                IsAllowedToMerge = true,
            };

            if ( requiredSecurityRole != null )
            {
                if ( !requiredSecurityRole.IsPersonInRole( RockPage.CurrentPerson.Guid ) )
                {
                    canMergeResult.IsAllowedToMerge = false;
                }
            }

            return canMergeResult;
        }

        /// <summary>
        /// Binds the values.
        /// </summary>
        private void BindGrid()
        {
            if ( MergeData != null && MergeData.People != null && MergeData.People.Any() )
            {
                var canMergeResult = CanMerge();
                lbMerge.Visible = canMergeResult.IsAllowedToMerge;

                if ( !canMergeResult.IsAllowedToMerge )
                {
                    nbAccountProtectProfile.Text = $"A record on this merge request has an Account Protection Profile of '{canMergeResult.MaxAccountProtectionProfile}'. This will require an individual in the '{canMergeResult.RequiredSecurityRole}' role to perform the merge.";
                    nbAccountProtectProfile.Visible = true;
                }

                // If the people have different email addresses and any logins, display security alert box
                ShowMessages( canMergeResult );

                // If the values of any hidden Attributes differ, display warning message.
                SetAttributesSecurityNoticeState();

                foreach ( var col in gValues.Columns.OfType<MergePersonField>() )
                {
                    col.IsPrimaryPerson = col.PersonId == MergeData.PrimaryPersonId;
                    if ( col.IsPrimaryPerson )
                    {
                        hfSelectedColumnPersonId.Value = col.PersonId.ToString();
                    }
                }

                List<ValuesRow> valuesRowList = MergeData.GetValuesRowList( headingKeys );
                gValues.DataSource = valuesRowList;
                gValues.DataBind();
            }
        }

        /// <summary>
        /// Shows the messages.
        /// </summary>
        /// <param name="canMergeResult">The can merge result.</param>
        private void ShowMessages( CanMergeResult canMergeResult )
        {
            if ( !canMergeResult.IsAllowedToMerge )
            {
                return;
            }

            if ( canMergeResult.MaxAccountProtectionProfile == AccountProtectionProfile.Low )
            {
                // no security messages, regardless of existence of Login, email address differences or phone differences
                nbSecurityNotice.Visible = false;
                return;
            }

            var hasLogins = MergeData.People.Where( p => p.HasLogins ).Any();

            var hasDifferentEmailAddresses = MergeData
                .People
                .Select( p => p.Email )
                .Where( e => e != null && e != string.Empty )
                .Distinct( StringComparer.CurrentCultureIgnoreCase )
                .Count() > 1;

            var hasDifferentMobilePhoneNumbers = MergeData
                .People
                .Select( p => p.MobilePhoneNumber )
                .Where( e => e != null && e != string.Empty )
                .Distinct( StringComparer.CurrentCultureIgnoreCase )
                .Count() > 1;

            var hasMobilePhoneNumbers = MergeData.People.Any( a => a.MobilePhoneNumber.IsNotNullOrWhiteSpace() );
            var hasEmailsAddresses = MergeData.People.Any( a => a.Email.IsNotNullOrWhiteSpace() );

            var maxAccountProtectionProfile = canMergeResult.MaxAccountProtectionProfile;
            var maxElevatedSecurityLevel = canMergeResult.MaxElevatedSecurityLevel;

            if ( maxAccountProtectionProfile == AccountProtectionProfile.Medium && !hasDifferentEmailAddresses && !hasDifferentEmailAddresses )
            {
                // Medium AccountProtectionProfile, but not email or phone issues, so just a friendly warning
                nbSecurityNotice.Heading = null;
                nbSecurityNotice.Visible = true;
                nbSecurityNotice.NotificationBoxType = NotificationBoxType.Warning;

                if ( hasLogins )
                {
                    nbSecurityNotice.Text = "This merge is considered of moderate risk as one of the records has a login. Please be sure to carefully consider the changed information to ensure this is not an attempt to hijack the account.";
                    return;
                }
                else
                {
                    nbSecurityNotice.Text = "This merge is considered of moderate risk. Please be sure to carefully consider the changed information.";
                    return;
                }
            }

            /* Set message wording based on
               - MaxAccountProtectionProfile
                    - Determines if a message will be displayed.
                    - Note: message will always be displayed unless the AccountProtectionLevel is Low
               - Has Logins
               - Has different emails
               - Has different mobile phone numbers
             **/

            var warningMessageBuilder = new StringBuilder();

            var warningMessageIssues = new List<string>();
            var cautionMessageIssues = new List<string>();

            if ( hasDifferentEmailAddresses )
            {
                warningMessageIssues.Add( "emails" );
                cautionMessageIssues.Add( "email" );
            }

            if ( hasDifferentMobilePhoneNumbers )
            {
                warningMessageIssues.Add( "mobile phone numbers" );
                cautionMessageIssues.Add( "mobile phone" );
            }

            string warningMessage = string.Empty;

            if ( hasLogins )
            {
                if ( warningMessageIssues.Any() )
                {
                    warningMessage = "One or more of the records has a login and different " + warningMessageIssues.AsDelimited( " and " ) + " associated with this merge.";
                }
                else
                {
                    warningMessage = "One or more of the records has a login.";
                }

                warningMessage += " This could be an attempt to hijack the account.";
            }
            else if ( warningMessageIssues.Any() )
            {
                warningMessage = "Different " + warningMessageIssues.AsDelimited( " and " ) + " are associated with this merge.";
                if ( hasMobilePhoneNumbers || hasEmailsAddresses )
                {

                    // Even nobody person has a login, if any person has a phone number or email address, it might be possible to hijack the account;
                    warningMessage += " This could be an attempt to hijack the account.";
                }
            }

            if ( cautionMessageIssues.Any() )
            {
                warningMessage += " You should apply considerable caution in selecting the appropriate " + cautionMessageIssues.AsDelimited( " and " ) + " for this merge.";
            }

            if ( hasLogins )
            {
                warningMessage += " Additionally, this person will be prompted to reconfirm before they can log in.";
            }

            nbSecurityNotice.Heading = null;
            nbSecurityNotice.NotificationBoxType = NotificationBoxType.Danger;
            nbSecurityNotice.Visible = true;

            string securityAlertHeading;

            if ( maxAccountProtectionProfile == AccountProtectionProfile.Extreme )
            {
                securityAlertHeading = "Critical Security Alert";
            }
            else if ( maxAccountProtectionProfile == AccountProtectionProfile.High )
            {
                securityAlertHeading = "Important Security Alert";
            }
            else
            {
                securityAlertHeading = "Security Alert";
            }

            string memberOfSecurityRoleMessage;

            if ( maxElevatedSecurityLevel > ElevatedSecurityLevel.None )
            {
                if ( warningMessage.IsNotNullOrWhiteSpace() )
                {
                    memberOfSecurityRoleMessage = "Additionally, one or more of these records is a member of a security role with elevated privileges.";
                }
                else
                {
                    memberOfSecurityRoleMessage = "One or more of these records is a member of a security role with elevated privileges.";
                }

                if ( maxAccountProtectionProfile == AccountProtectionProfile.Extreme )
                {
                    memberOfSecurityRoleMessage = $"<b class='text-uppercase'>{memberOfSecurityRoleMessage}</b>";
                }
                else
                {
                    memberOfSecurityRoleMessage = $"<b>{memberOfSecurityRoleMessage}</b>";
                }
            }
            else
            {
                memberOfSecurityRoleMessage = null;
            }

            if ( memberOfSecurityRoleMessage.IsNullOrWhiteSpace() && warningMessage.IsNullOrWhiteSpace() )
            {
                // if there is no message to show since there are only some other issues (like Financial Data), show a generic message;
                warningMessage = "This merge is considered a high risk due to financial or other sensitive data. Please be sure to carefully consider the changed information.";
            }

            if ( warningMessage.IsNotNullOrWhiteSpace() )
            {
                nbSecurityNotice.Text = $"<b class='text-uppercase'>{securityAlertHeading}:</b> {warningMessage}";
            }
            else
            {
                nbSecurityNotice.Text = $"<b class='text-uppercase'>{securityAlertHeading}</b>";
            }

            if ( memberOfSecurityRoleMessage.IsNotNullOrWhiteSpace() )
            {
                nbSecurityNotice.Text += $"<br><br>{memberOfSecurityRoleMessage}";
            }
        }

        /// <summary>
        /// Show or hide the Attributes security warning.
        /// </summary>
        private void SetAttributesSecurityNoticeState()
        {
            // Show a warning if there are any properties with differing values that the current user does not have permission to view.
            var conflictingHiddenProperties = MergeData.Properties.Where( p => !p.HasViewPermission
                                                        && ( p.Values.Select( v => v.Value ).Distinct().Count() > 1 ) )
                                              .ToList();

            var showWarning = conflictingHiddenProperties.Any();

            nbPermissionNotice.Visible = showWarning;
        }

        /// <summary>
        /// Gets the values selection.
        /// </summary>
        private void GetValuesSelection()
        {
            var mergeDataPropertiesLookup = MergeData.Properties.ToDictionary( k => k.Key, v => v.Values.ToDictionary( vk => vk.PersonId, vv => vv ) );

            foreach ( var column in gValues.Columns.OfType<MergePersonField>() )
            {
                int personId = column.PersonId;

                foreach ( GridViewRow row in gValues.Rows )
                {
                    var propertySelection = MergePersonField.GetPropertySelection( row, personId );
                    if ( !propertySelection.IsSectionHeader )
                    {
                        PersonPropertyValue personPropertyValue = mergeDataPropertiesLookup[propertySelection.PropertyKey][personId];
                        personPropertyValue.Selected = propertySelection.Selected;
                    }
                }
            }
        }

        private string GetNewStringValue( string key )
        {
            var ppValue = GetNewValue( key );
            return ppValue != null ? ppValue.Value : string.Empty;
        }

        private int? GetNewIntValue( string key )
        {
            var ppValue = GetNewValue( key );
            if ( ppValue != null )
            {
                int newValue = int.MinValue;
                if ( int.TryParse( ppValue.Value, out newValue ) )
                {
                    return newValue;
                }
            }

            return null;
        }

        private bool? GetNewBoolValue( string key )
        {
            var ppValue = GetNewValue( key );
            if ( ppValue != null )
            {
                bool newValue = false;
                if ( bool.TryParse( ppValue.Value, out newValue ) )
                {
                    return newValue;
                }
            }

            return null;
        }

        private DateTime? GetNewDateTimeValue( string key )
        {
            var ppValue = GetNewValue( key );
            if ( ppValue != null )
            {
                DateTime newValue = DateTime.MinValue;
                if ( DateTime.TryParse( ppValue.Value, out newValue ) )
                {
                    return newValue;
                }
            }

            return null;
        }

        private Enum GetNewEnumValue( string key, Type enumType )
        {
            var ppValue = GetNewValue( key );
            if ( ppValue != null )
            {
                return ( Enum ) Enum.Parse( enumType, ppValue.Value );
            }

            return null;
        }

        private PersonPropertyValue GetNewValue( string key )
        {
            var property = MergeData.GetProperty( key );
            var primaryPersonValue = property.Values.Where( v => v.PersonId == MergeData.PrimaryPersonId ).FirstOrDefault();
            var selectedPersonValue = property.Values.Where( v => v.Selected ).FirstOrDefault();

            return selectedPersonValue;
        }

        private PersonPropertyValue[] GetSelectedValues( string key )
        {
            var property = MergeData.GetProperty( key );
            var primaryPersonValue = property.Values.Where( v => v.PersonId == MergeData.PrimaryPersonId ).FirstOrDefault();
            var selectedPersonValues = property.Values.Where( v => v.Selected );

            return selectedPersonValues.ToArray();
        }

        /// <summary>
        /// Removes any UserLogin records associated with the Anonymous Giver.
        /// </summary>
        /// <param name="userLoginService">The <see cref="UserLoginService"/>.</param>
        private void RemoveAnonymousGiverUserLogins( UserLoginService userLoginService, RockContext rockContext )
        {
            var personIds = MergeData.People.Select( a => a.Id ).ToList();

            var logins = userLoginService.Queryable()
                .Where( l => l.PersonId.HasValue && personIds.Contains( l.PersonId.Value ) );

            userLoginService.DeleteRange( logins );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Checks to see if one of the records being merged (other than the primary record) is the Anonymous Giver account.
        /// </summary>
        private bool MergeDataIncludesAnonymousGiver()
        {
            var mergePersonGuids = MergeData.People.Where( p => p.Id != MergeData.PrimaryPersonId ).Select( p => p.Guid );
            return mergePersonGuids.Contains( Rock.SystemGuid.Person.GIVER_ANONYMOUS.AsGuid() );
        }

        #endregion
    }

    #region MergeData Class

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    internal class MergeData
    {
        #region Developer Settings

        /*
            [01-Dec-2019 - DL]
            This switch determines if conflicting merge properties for which the user does not have view permission should be excluded from the merge data
            or included as a set of masked values.
            Per the product owner, this option should only be enabled for development and diagnostic purposes.
        */
        private const bool _ShowSecuredProperties = false;

        #endregion

        #region Constants

        private const string FAMILY_VALUES = "FamilyValues";
        private const string BUSINESS_INFORMATION = "BusinessInformation";
        private const string BUSINESS_ATTRIBUTES = "BusinessAttributes";
        private const string FAMILY_NAME = "FamilyName";
        private const string CAMPUS = "Campus";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the entity set identifier.
        /// </summary>
        /// <value>
        /// The entity set identifier.
        /// </value>
        public int EntitySetId { get; set; }

        /// <summary>
        /// Gets or sets the people.
        /// </summary>
        /// <value>
        /// The people.
        /// </value>
        public List<MergePerson> People { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public List<PersonProperty> Properties { get; set; }

        /// <summary>
        /// Gets or sets the primary person identifier.
        /// </summary>
        /// <value>
        /// The primary person identifier.
        /// </value>
        public int? PrimaryPersonId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeData"/> class.
        /// </summary>
        public MergeData()
        {
            People = new List<MergePerson>();
            Properties = new List<PersonProperty>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeData"/> class.
        /// </summary>
        /// <param name="people">The people.</param>
        /// <param name="headingKeys">The key values of the merge categories to display.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="grantPermissionForAllAttributes">Should the current user be granted permission to view all secured Attributes?</param>
        public MergeData( List<Person> people, List<string> headingKeys, Person currentPerson, bool grantPermissionForAllAttributes )
        {
            People = new List<MergePerson>();
            Properties = new List<PersonProperty>();

            bool isBusiness = people.All( a => a.IsBusiness() );

            foreach ( var person in people )
            {
                AddPerson( person, isBusiness );
            }

            // Add Phone Numbers
            var phoneTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).DefinedValues;
            foreach ( var person in people )
            {
                AddProperty( "PhoneNumbers", "Phone Numbers", 0, string.Empty );

                foreach ( var phoneType in phoneTypes )
                {
                    string key = "phone_" + phoneType.Id.ToString();
                    var phoneNumber = person.PhoneNumbers.Where( p => p.NumberTypeValueId == phoneType.Id ).FirstOrDefault();
                    if ( phoneNumber != null )
                    {
                        string iconHtml = string.Empty;

                        if ( phoneNumber.IsUnlisted )
                        {
                            iconHtml += " <span class='label label-info' title='Unlisted' data-toggle='tooltip' data-placement='top'><i class='fa fa-phone-slash'></i></span>";
                        }

                        if ( phoneNumber.IsMessagingEnabled )
                        {
                            iconHtml += " <span class='label label-success' title='SMS Enabled' data-toggle='tooltip' data-placement='top'><i class='fa fa-sms'></i></span>";
                        }

                        AddProperty( key, phoneType.Value, person.Id, phoneNumber.Number, phoneNumber.NumberFormatted + iconHtml );
                    }
                    else
                    {
                        AddProperty( key, phoneType.Value, person.Id, string.Empty, string.Empty );
                    }
                }
            }

            // Add Addresses, grouped by Address Type.
            var addressTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() ).DefinedValues;

            foreach ( var addressType in addressTypes )
            {
                string key = "address_" + addressType.Id.ToString();

                foreach ( var person in people )
                {
                    AddProperty( "Addresses", "Addresses", 0, string.Empty );

                    var family = person.PrimaryFamily;

                    if ( family == null )
                    {
                        continue;
                    }

                    var addresses = family.GroupLocations;

                    var addressesOfType = addresses.Where( p => p.GroupLocationTypeValueId == addressType.Id ).ToList();

                    var addressTypeCount = addressesOfType.Count;

                    if ( addressTypeCount > 0 )
                    {
                        foreach ( var address in addressesOfType )
                        {
                            string iconHtml = string.Empty;

                            if ( address.IsMailingLocation )
                            {
                                iconHtml += " <span class='label label-info' title='Mailing' data-toggle='tooltip' data-placement='top'><i class='fa fa-envelope'></i></span>";
                            }

                            if ( address.IsMappedLocation )
                            {
                                iconHtml += " <span class='label label-success' title='Mapped' data-toggle='tooltip' data-placement='top'><i class='fa fa-map-marker'></i></span>";
                            }

                            var addressKey = key;

                            if ( addressTypeCount > 1 )
                            {
                                addressKey = addressKey + "_" + address.Id;
                            }

                            AddProperty( addressKey, addressType.Value, person.Id, address.Id.ToString(), address.Location.GetFullStreetAddress() + iconHtml );
                        }
                    }
                    else
                    {
                        AddProperty( key, addressType.Value, person.Id, string.Empty, string.Empty );
                    }
                }
            }

            foreach ( var person in people )
            {
                if ( isBusiness )
                {
                    AddProperty( BUSINESS_ATTRIBUTES, "Business Attributes", 0, string.Empty );
                }
                else
                {
                    AddProperty( "PersonAttributes", "Person Attributes", 0, string.Empty );
                }

                person.LoadAttributes();
                foreach ( var attribute in person.Attributes.OrderBy( a => a.Value.Order ) )
                {
                    string value = person.GetAttributeValue( attribute.Key );
                    bool condensed = attribute.Value.FieldType.Class == typeof( Rock.Field.Types.ImageFieldType ).FullName;
                    string formattedValue = attribute.Value.FieldType.Field.FormatValue( null, attribute.Value.EntityTypeId, person.Id, value, attribute.Value.QualifierValues, condensed );

                    var hasViewPermission = attribute.Value.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson )
                                            || grantPermissionForAllAttributes;

                    AddProperty( "attr_" + attribute.Key, attribute.Value.Name, person.Id, value, formattedValue, hasViewPermission, selected: false, attribute: attribute.Value );
                }
            }

            foreach ( var person in people )
            {
                if ( isBusiness )
                {
                    AddProperty( BUSINESS_INFORMATION, "Business Information", 0, string.Empty );
                }
                else
                {
                    AddProperty( FAMILY_VALUES, FAMILY_VALUES.SplitCase(), 0, string.Empty );
                }

                var family = person.GetFamily();
                if ( family != null )
                {
                    AddProperty( FAMILY_NAME, FAMILY_NAME.SplitCase(), person.Id, family.Name );
                    AddProperty( CAMPUS, CAMPUS, person.Id, family.CampusId.HasValue ? family.CampusId.ToString() : string.Empty, family.CampusId.HasValue ? family.Campus.Name : string.Empty );
                }

                if ( isBusiness )
                {
                    AddProperty( "BusinessAttributes", "Business Attributes", 0, string.Empty );
                }
                else
                {
                    AddProperty( "FamilyAttributes", "Family Attributes", 0, string.Empty );
                }

                if ( family != null )
                {
                    family.LoadAttributes();
                    foreach ( var attribute in family.Attributes.OrderBy( a => a.Value.Order ) )
                    {
                        string value = family.GetAttributeValue( attribute.Key );
                        bool condensed = attribute.Value.FieldType.Class == typeof( Rock.Field.Types.ImageFieldType ).FullName;
                        string formattedValue = attribute.Value.FieldType.Field.FormatValue( null, attribute.Value.EntityTypeId, person.Id, value, attribute.Value.QualifierValues, condensed );

                        var hasViewPermission = attribute.Value.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson )
                                                || grantPermissionForAllAttributes;

                        AddProperty( "groupattr_" + attribute.Key, attribute.Value.Name, person.Id, value, formattedValue, hasViewPermission, selected: false, attribute: attribute.Value );
                    }
                }
            }

            // Add missing values
            foreach ( var property in Properties.Where( p => !headingKeys.Contains( p.Key ) ) )
            {
                foreach ( var person in People.Where( p => !property.Values.Any( v => v.PersonId == p.Id ) ) )
                {
                    property.Values.Add( new PersonPropertyValue() { PersonId = person.Id } );
                }
            }

            var primaryPerson = people.OrderBy( p => p.CreatedDateTime ).FirstOrDefault();
            if ( primaryPerson != null )
            {
                SetPrimary( primaryPerson.Id, primaryPerson.Guid );
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Sets the primary.
        /// </summary>
        /// <param name="primaryPersonId">The primary person identifier.</param>
        public void SetPrimary( int primaryPersonId, Guid primaryPersonGuid )
        {
            PrimaryPersonId = primaryPersonId;

            foreach ( var personProperty in Properties )
            {
                PersonPropertyValue value = null;

                // If the Primary Person Guid is the anonymous giver, always set that record value as default.
                if ( primaryPersonGuid == Rock.SystemGuid.Person.GIVER_ANONYMOUS.AsGuid() )
                {
                    value = personProperty.Values.Where( v => v.PersonId == primaryPersonId ).FirstOrDefault();
                }
                else if ( personProperty.Values.Any( v => v.Value != null && v.Value != string.Empty ) )
                {
                    // Find primary person's non-blank value
                    value = personProperty.Values.Where( v => v.PersonId == primaryPersonId && v.Value != null && v.Value != string.Empty ).FirstOrDefault();
                    if ( value == null )
                    {
                        // Find any other selected value
                        value = personProperty.Values.Where( v => v.Selected ).FirstOrDefault();
                        if ( value == null )
                        {
                            // Find first non-blank value
                            value = personProperty.Values.Where( v => v.Value != string.Empty ).FirstOrDefault();
                            if ( value == null )
                            {
                                value = personProperty.Values.FirstOrDefault();
                            }
                        }
                    }
                }
                else
                {
                    value = personProperty.Values.Where( v => v.PersonId == primaryPersonId ).FirstOrDefault();
                }

                // Unselect all the values
                personProperty.Values.ForEach( v => v.Selected = false );

                if ( personProperty.AttributeId.HasValue )
                {
                    var attribute = AttributeCache.Get( personProperty.AttributeId.Value );
                    if ( attribute.FieldType.Field is Rock.Field.Types.MatrixFieldType )
                    {
                        personProperty.Values.ForEach( v => v.Selected = true );
                    }
                }

                if ( value != null )
                {
                    value.Selected = true;
                }
            }
        }

        /// <summary>
        /// Gets the selected value.
        /// </summary>
        /// <param name="personProperty">The person property.</param>
        /// <returns></returns>
        public PersonPropertyValue GetSelectedValue( PersonProperty personProperty )
        {
            if ( personProperty != null )
            {
                return personProperty.Values.Where( v => v.Selected ).FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Gets the data table.
        /// </summary>
        /// <param name="headingKeys">The heading keys.</param>
        /// <returns></returns>
        public List<ValuesRow> GetValuesRowList( List<string> headingKeys )
        {
            var valuesRowList = new List<ValuesRow>();

            ValuesRow headingRow = null;

            // Only show properties that match the selected headingKeys, and have more than one distinct value.
            var visibleProperties = Properties.Where( p => ( p.HasViewPermission || _ShowSecuredProperties )
                                                           && ( headingKeys.Contains( p.Key ) || p.Values.Select( v => v.Value ?? string.Empty ).Distinct().Count() > 1 ) )
                                              .ToList();

            foreach ( var personProperty in visibleProperties )
            {
                var valuesRow = new ValuesRow();
                valuesRow.PersonProperty = personProperty;
                valuesRow.PersonPersonPropertyList = new List<ValuesRowPersonPersonProperty>();
                foreach ( var person in People )
                {
                    ValuesRowPersonPersonProperty valuesRowPersonPersonProperty = new ValuesRowPersonPersonProperty();
                    valuesRowPersonPersonProperty.Person = person;
                    valuesRowPersonPersonProperty.PersonProperty = personProperty;

                    bool addValuesRow;

                    if ( personProperty.HasViewPermission )
                    {
                        valuesRowPersonPersonProperty.PersonPropertyValue = personProperty.Values.Where( v => v.PersonId == person.Id ).FirstOrDefault();
                        addValuesRow = true;
                    }
                    else
                    {
                        // The current user does not have permission to view the property, so mask the value.
                        valuesRowPersonPersonProperty.PersonPropertyValue = new PersonPropertyValue() { PersonId = person.Id, Selected = false, Value = null, FormattedValue = "<span class='label label-danger'>secured</span>" };
                        addValuesRow = _ShowSecuredProperties;
                    }

                    if ( addValuesRow )
                    {
                        valuesRow.PersonPersonPropertyList.Add( valuesRowPersonPersonProperty );
                    }
                }

                if ( headingKeys.Contains( personProperty.Key ) )
                {
                    headingRow = valuesRow;
                    headingRow.IsSectionHeading = true;
                }
                else
                {
                    if ( headingRow != null )
                    {
                        valuesRowList.Add( headingRow );
                        headingRow = null;
                    }

                    valuesRowList.Add( valuesRow );
                }
            }

            return valuesRowList;
        }

        #endregion

        #region Private Methods

        private void AddPerson( Person person, bool isBusiness )
        {
            string personPhotoTag = string.Format( "<img src='{0}' style='max-width:65px;max-height:65px'>", Person.GetPersonPhotoUrl( person ) + "&width=65" );

            People.Add( new MergePerson( person ) );
            AddProperty( "Photo", "Photo", person.Id, person.PhotoId.ToString(), personPhotoTag );
            AddProperty( "Title", person.Id, person.TitleValue );
            if ( !isBusiness )
            {
                AddProperty( "FirstName", person.Id, person.FirstName );
                AddProperty( "NickName", person.Id, person.NickName );
                AddProperty( "MiddleName", person.Id, person.MiddleName );
                AddProperty( "LastName", person.Id, person.LastName );
                AddProperty( "Suffix", person.Id, person.SuffixValue );
            }
            else
            {
                AddProperty( "LastName", "Business Name", person.Id, person.LastName );
            }

            AddProperty( "RecordType", person.Id, person.RecordTypeValue );
            AddProperty( "RecordStatus", person.Id, person.RecordStatusValue );
            AddProperty( "RecordStatusReason", person.Id, person.RecordStatusReasonValue );
            AddProperty( "ConnectionStatus", person.Id, person.ConnectionStatusValue );
            AddProperty( "Deceased", person.Id, person.IsDeceased );
            AddProperty( "Gender", person.Id, person.Gender );
            AddProperty( "MaritalStatus", person.Id, person.MaritalStatusValue );
            AddProperty( "BirthDate", person.Id, person.BirthDate );
            AddProperty( "AnniversaryDate", person.Id, person.AnniversaryDate );
            AddProperty( "GraduationYear", person.Id, person.GraduationYear.HasValue ? person.GraduationYear.ToString() : string.Empty );
            AddProperty( "Email", person.Id, person.Email );
            AddProperty( "EmailActive", person.Id, person.IsEmailActive );
            AddProperty( "EmailNote", person.Id, person.EmailNote );
            AddProperty( "EmailPreference", person.Id, person.EmailPreference );
            AddProperty( "InactiveReasonNote", person.Id, person.InactiveReasonNote );
            AddProperty( "SystemNote", person.Id, person.SystemNote );
            AddProperty(
                "ContributionFinancialAccountId",
                "Contribution Financial Account",
                person.Id,
                person.ContributionFinancialAccountId.ToStringSafe(),
                person.ContributionFinancialAccount != null ? person.ContributionFinancialAccount.PublicName : string.Empty );
        }

        private void AddProperty( string key, int personId, string value, bool hasViewPermission = true, bool selected = false )
        {
            AddProperty( key, key.SplitCase(), personId, value, value, hasViewPermission, selected: selected );
        }

        private void AddProperty( string key, string label, int personId, string value, bool hasViewPermission = true, bool selected = false )
        {
            AddProperty( key, label, personId, value, value, hasViewPermission, selected: selected );
        }

        /// <summary>
        /// Adds a merge property value for the specified person.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="label"></param>
        /// <param name="personId"></param>
        /// <param name="value"></param>
        /// <param name="formattedValue"></param>
        /// <param name="selected"></param>
        /// <param name="attribute"></param>
        /// <param name=""></param>
        private void AddProperty( string key, string label, int personId, string value, string formattedValue, bool hasViewPermission = true, bool selected = false, AttributeCache attribute = null )
        {
            var property = GetProperty( key, true, label );
            if ( attribute != null )
            {
                property.AttributeId = attribute.Id;
            }

            property.HasViewPermission = hasViewPermission;

            var propertyValue = property.Values.Where( v => v.PersonId == personId ).FirstOrDefault();
            if ( propertyValue == null )
            {
                propertyValue = new PersonPropertyValue { PersonId = personId };
                property.Values.Add( propertyValue );
            }

            propertyValue.Value = value ?? string.Empty;
            propertyValue.FormattedValue = formattedValue ?? string.Empty;
            propertyValue.Selected = selected;
        }

        private void AddProperty( string key, int personId, DefinedValue value, bool hasViewPermission = true, bool selected = false )
        {
            AddProperty( key, key.SplitCase(), personId, value, hasViewPermission, selected );
        }

        private void AddProperty( string key, string label, int personId, DefinedValue value, bool hasViewPermission = true, bool selected = false )
        {
            var property = GetProperty( key, true, label );

            property.HasViewPermission = hasViewPermission;

            var propertyValue = property.Values.Where( v => v.PersonId == personId ).FirstOrDefault();
            if ( propertyValue == null )
            {
                propertyValue = new PersonPropertyValue { PersonId = personId };
                property.Values.Add( propertyValue );
            }

            propertyValue.Value = value != null ? value.Id.ToString() : string.Empty;
            propertyValue.FormattedValue = value != null ? value.Value : string.Empty;
            propertyValue.Selected = selected;
        }

        private void AddProperty( string key, int personId, bool? value, bool hasViewPermission = true, bool selected = false )
        {
            AddProperty( key, personId, ( value ?? false ).ToString(), hasViewPermission, selected );
        }

        private void AddProperty( string key, int personId, DateTime? value, bool hasViewPermission = true, bool selected = false )
        {
            AddProperty( key, personId, value.HasValue ? value.Value.ToShortDateString() : string.Empty, hasViewPermission, selected );
        }

        private void AddProperty( string key, int personId, Enum value, bool hasViewPermission = true, bool selected = false )
        {
            AddProperty( key, key.SplitCase(), personId, value.ConvertToString( false ), value.ConvertToString(), hasViewPermission, selected: selected );
        }

        public PersonProperty GetProperty( string key, bool createIfNotFound = false, string label = "" )
        {
            var property = Properties.Where( p => p.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
            if ( property == null && createIfNotFound )
            {
                if ( label == string.Empty )
                {
                    label = key.SplitCase();
                }

                property = new PersonProperty( key, label );
                Properties.Add( property );
            }

            return property;
        }

        #endregion

        #endregion
    }

    #endregion

    #region MergePerson Class

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class MergePerson
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public string ModifiedBy { get; set; }

        public string Email { get; set; }

        public bool HasLogins { get; set; }

        public bool IsBusiness { get; set; }

        public bool IsNameless { get; set; }

        public Guid Guid { get; set; }

        public AccountProtectionProfile AccountProtectionProfile { get; set; }

        public string MobilePhoneNumber { get; set; }

        public MergePerson( Person person )
        {
            Id = person.Id;
            FullName = person.FullName;
            ModifiedDateTime = person.ModifiedDateTime;
            CreatedDateTime = person.CreatedDateTime;
            Email = person.Email;
            HasLogins = person.Users.Any();
            Guid = person.Guid;
            IsBusiness = person.IsBusiness();
            IsNameless = person.IsNameless();
            if ( person.ModifiedByPersonAlias != null && person.ModifiedByPersonAlias.Person != null )
            {
                ModifiedBy = person.ModifiedByPersonAlias.Person.FullName;
            }

            AccountProtectionProfile = person.AccountProtectionProfile;
            MobilePhoneNumber = person.PhoneNumbers.Where( p => p.IsMessagingEnabled ).FirstOrDefault()?.FullNumber;
        }
    }

    #endregion

    #region PersonProperty Class

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class PersonProperty
    {
        public string Key { get; set; }

        public string Label { get; set; }

        public int? AttributeId { get; set; }

        public List<PersonPropertyValue> Values { get; set; }

        /// <summary>
        /// Does the current user have view permission for this property?
        /// </summary>
        public bool HasViewPermission { get; set; }

        public PersonProperty()
        {
            Values = new List<PersonPropertyValue>();
        }

        public PersonProperty( string key )
            : this()
        {
            Key = key;
            Label = key.SplitCase();
        }

        public PersonProperty( string key, string label )
            : this()
        {
            Key = key;
            Label = label;
        }

        public AttributeCache Attribute
        {
            get
            {
                if ( AttributeId.HasValue )
                {
                    return AttributeCache.Get( AttributeId.Value );
                }

                return null;
            }
        }
    }

    #endregion

    #region PersonPropertyValue class

    [Serializable]
    public class PersonPropertyValue
    {
        public int PersonId { get; set; }

        public bool Selected { get; set; }

        public string Value { get; set; }

        public string FormattedValue { get; set; }
    }

    #endregion

    /// <summary>
    /// Holds a gridview data item for the Person Merge block.
    /// </summary>
    public class ValuesRow
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is section heading.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is section heading; otherwise, <c>false</c>.
        /// </value>
        public bool IsSectionHeading { get; set; }

        /// <summary>
        /// Gets the property key.
        /// </summary>
        /// <value>
        /// The property key.
        /// </value>
        public string PropertyKey
        {
            get
            {
                return PersonProperty.Key;
            }
        }

        /// <summary>
        /// Gets the property label.
        /// </summary>
        /// <value>
        /// The property label.
        /// </value>
        public string PropertyLabel
        {
            get
            {
                return PersonProperty.Label;
            }
        }

        /// <summary>
        /// Gets or sets the person person property list.
        /// </summary>
        /// <value>
        /// The person person property list.
        /// </value>
        public List<ValuesRowPersonPersonProperty> PersonPersonPropertyList { get; set; }

        /// <summary>
        /// Gets the person property.
        /// </summary>
        /// <value>
        /// The person property.
        /// </value>
        public PersonProperty PersonProperty { get; internal set; }
    }

    /// <summary>
    /// Holds values for the person merge info on the Person Merge block.
    /// </summary>
    public class ValuesRowPersonPersonProperty
    {
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public MergePerson Person { get; set; }

        /// <summary>
        /// Gets or sets the person property.
        /// </summary>
        /// <value>
        /// The person property.
        /// </value>
        public PersonProperty PersonProperty { get; set; }

        /// <summary>
        /// Gets or sets the person property value.
        /// </summary>
        /// <value>
        /// The person property value.
        /// </value>
        public PersonPropertyValue PersonPropertyValue { get; set; }
    }
}