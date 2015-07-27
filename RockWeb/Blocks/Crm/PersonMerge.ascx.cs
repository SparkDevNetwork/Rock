// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
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

    [LinkedPage( "Person Detail Page" )]
    public partial class PersonMerge : Rock.Web.UI.RockBlock
    {

        #region Fields

        private List<string> headingKeys = new List<string> {
            "PhoneNumbers", 
            "PersonAttributes"
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

            gValues.DataKeyNames = new string[] { "Key" };
            gValues.AllowPaging = false;
            gValues.ShowActionRow = false;
            gValues.RowDataBound += gValues_RowDataBound;

            string script = string.Format( @"
    $('div.merge-header-summary').click( function (event) {{
        var $i = $(this).children('i');
        if ($i.hasClass('fa-square-o')) {{
            $i.removeClass('fa-square-o').addClass('fa-check-square-o');
            $('div.merge-header-summary > i').not($i).removeClass('fa-check-square-o').addClass('fa-square-o');
            var colId = $(this).attr('data-col');
            $('#{0}').val(colId);
            $(this).closest('table').find(""input[id$='cbSelect_"" + colId + ""']"").each(function(index) {{
                var textNode = this.nextSibling;
                if ( !$(this).is(':last-child') || (textNode && textNode.nodeType == 3)) {{
                    $(this).prop('checked', true);
                }}
            }});
        }}
    }});
", hfSelectedColumn.ClientID );
            ScriptManager.RegisterStartupScript( gValues, gValues.GetType(), "primary-person-click", script, true );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbPeople.Visible = false;

            if ( !Page.IsPostBack )
            {
                int? setId = PageParameter( "Set" ).AsIntegerOrNull();
                if (setId.HasValue)
                {
                    var selectedPersonIds = new EntitySetItemService( new RockContext() )
                        .GetByEntitySetId( setId.Value )
                        .Select( i => i.EntityId )
                        .Distinct()
                        .ToList();

                    // Get the people selected
                    var people = new PersonService( new RockContext() ).Queryable( "CreatedByPersonAlias.Person,Users", true )
                        .Where( p => selectedPersonIds.Contains( p.Id ) )
                        .ToList();

                    // Create the data structure used to build grid
                    MergeData = new MergeData( people, headingKeys );
                    BuildColumns();
                    BindGrid();
                }
            }
            else
            {
                var primaryColIndex = hfSelectedColumn.Value.AsIntegerOrNull();
                
                // Save the primary header radio button's selection
                foreach ( var col in gValues.Columns.OfType<PersonMergeField>() )
                {
                    col.OnDelete += personCol_OnDelete;
                    if (primaryColIndex.HasValue && primaryColIndex.Value == col.ColumnIndex)
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

                // Get the people selected
                var people = new PersonService( new RockContext() ).Queryable( "CreatedByPersonAlias.Person,Users" )
                    .Where( p => selectedPersonIds.Contains( p.Id ) )
                    .ToList();

                // Rebuild mergdata, columns, and grid
                MergeData = new MergeData( people, headingKeys );
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
            var personMergeField = sender as PersonMergeField;
            if ( personMergeField != null )
            {
                var selectedPersonIds = MergeData.People
                    .Where( p => p.Id != personMergeField.PersonId )
                    .Select( p => p.Id ).ToList();

                // Get the people selected
                var people = new PersonService( new RockContext() ).Queryable( "CreatedByPersonAlias.Person,Users" )
                    .Where( p => selectedPersonIds.Contains( p.Id ) )
                    .ToList();

                // Rebuild mergdata, columns, and grid
                MergeData = new MergeData( people, headingKeys );
                BuildColumns();
                BindGrid();
            }
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
            }
            else if ( e.Row.RowType == DataControlRowType.Header )
            {
                e.Row.AddCssClass( "grid-header-bold" );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbMerge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMerge_Click( object sender, EventArgs e )
        {
            if ( MergeData.People.Count < 2 )
            {
                nbPeople.Visible = true;
                return;
            }

            bool reconfirmRequired = ( MergeData.People.Select( p => p.Email ).Distinct().Count() > 1 && MergeData.People.Where( p => p.HasLogins ).Any() ); 

            GetValuesSelection();

            int? primaryPersonId = null;

            var oldPhotos = new List<int>();

            var rockContext = new RockContext();

            rockContext.WrapTransaction( () =>
            {
                var personService = new PersonService( rockContext );
                var userLoginService = new UserLoginService( rockContext );
                var familyService = new GroupService( rockContext );
                var familyMemberService = new GroupMemberService( rockContext );
                var binaryFileService = new BinaryFileService( rockContext );
                var phoneNumberService = new PhoneNumberService( rockContext );

                Person primaryPerson = personService.Get( MergeData.PrimaryPersonId ?? 0 );
                if ( primaryPerson != null )
                {
                    primaryPersonId = primaryPerson.Id;

                    var changes = new List<string>();

                    foreach ( var p in MergeData.People.Where( p => p.Id != primaryPerson.Id ) )
                    {
                        changes.Add( string.Format( "Merged <span class='field-value'>{0} [ID: {1}]</span> with this record.", p.FullName, p.Id ) );
                    }

                    // Photo Id
                    int? newPhotoId = MergeData.GetSelectedValue( MergeData.GetProperty( "Photo" ) ).Value.AsIntegerOrNull();
                    if ( !primaryPerson.PhotoId.Equals( newPhotoId ) )
                    {
                        changes.Add( "Modified the photo." );
                        primaryPerson.PhotoId = newPhotoId;
                    }

                    primaryPerson.TitleValueId = GetNewIntValue( "Title", changes );
                    primaryPerson.FirstName = GetNewStringValue( "FirstName", changes );
                    primaryPerson.NickName = GetNewStringValue( "NickName", changes );
                    primaryPerson.MiddleName = GetNewStringValue( "MiddleName", changes );
                    primaryPerson.LastName = GetNewStringValue( "LastName", changes );
                    primaryPerson.SuffixValueId = GetNewIntValue( "Suffix", changes );
                    primaryPerson.RecordTypeValueId = GetNewIntValue( "RecordType", changes );
                    primaryPerson.RecordStatusValueId = GetNewIntValue( "RecordStatus", changes );
                    primaryPerson.RecordStatusReasonValueId = GetNewIntValue( "RecordStatusReason", changes );
                    primaryPerson.ConnectionStatusValueId = GetNewIntValue( "ConnectionStatus", changes );
                    primaryPerson.IsDeceased = GetNewBoolValue( "Deceased", changes ) ?? false;
                    primaryPerson.Gender = (Gender)GetNewEnumValue( "Gender", typeof( Gender ), changes );
                    primaryPerson.MaritalStatusValueId = GetNewIntValue( "MaritalStatus", changes );
                    primaryPerson.SetBirthDate( GetNewDateTimeValue( "BirthDate", changes ) );
                    primaryPerson.AnniversaryDate = GetNewDateTimeValue( "AnniversaryDate", changes );
                    primaryPerson.GraduationYear = GetNewIntValue( "GraduationYear", changes );
                    primaryPerson.Email = GetNewStringValue( "Email", changes );
                    primaryPerson.IsEmailActive = GetNewBoolValue( "EmailActive", changes );
                    primaryPerson.EmailNote = GetNewStringValue( "EmailNote", changes );
                    primaryPerson.EmailPreference = (EmailPreference)GetNewEnumValue( "EmailPreference", typeof( EmailPreference ), changes );
                    primaryPerson.SystemNote = GetNewStringValue( "InactiveReasonNote", changes );
                    primaryPerson.SystemNote = GetNewStringValue( "SystemNote", changes );

                    // Update phone numbers
                    var phoneTypes = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).DefinedValues;
                    foreach ( var phoneType in phoneTypes )
                    {
                        var phoneNumber = primaryPerson.PhoneNumbers.Where( p => p.NumberTypeValueId == phoneType.Id ).FirstOrDefault();
                        string oldValue = phoneNumber != null ? phoneNumber.Number : string.Empty;

                        string key = "phone_" + phoneType.Id.ToString();
                        string newValue = GetNewStringValue( key, changes );

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
                                }
                            }
                        }
                    }

                    // Save the new record
                    rockContext.SaveChanges();

                    // Update the attributes
                    primaryPerson.LoadAttributes( rockContext );
                    foreach ( var property in MergeData.Properties.Where( p => p.Key.StartsWith( "attr_" ) ) )
                    {
                        string attributeKey = property.Key.Substring( 5 );
                        string oldValue = primaryPerson.GetAttributeValue( attributeKey ) ?? string.Empty;
                        string newValue = GetNewStringValue( property.Key, changes ) ?? string.Empty;

                        if ( !oldValue.Equals( newValue ) )
                        {
                            var attribute = primaryPerson.Attributes[attributeKey];
                            Rock.Attribute.Helper.SaveAttributeValue( primaryPerson, attribute, newValue, rockContext );
                        }
                    }

                    HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                        primaryPerson.Id, changes );

                    // Delete the unselected photos
                    string photoKeeper = primaryPerson.PhotoId.HasValue ? primaryPerson.PhotoId.Value.ToString() : string.Empty;
                    foreach ( var photoValue in MergeData.Properties
                        .Where( p => p.Key == "Photo" )
                        .SelectMany( p => p.Values )
                        .Where( v => v.Value != "" && v.Value != photoKeeper )
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

                    // Delete merged person's family records and any families that would be empty after merge
                    foreach ( var p in MergeData.People.Where( p => p.Id != primaryPersonId.Value ) )
                    {
                        // Delete the merged person's phone numbers (we've already updated the primary persons values)
                        foreach ( var phoneNumber in phoneNumberService.GetByPersonId( p.Id ) )
                        {
                            phoneNumberService.Delete( phoneNumber );
                        }

                        // If there was more than one email address and user has logins, then set any of the local 
                        // logins ( database & AD ) to require a reconfirmation
                        if ( reconfirmRequired )
                        {
                            foreach ( var login in userLoginService.GetByPersonId( p.Id ) )
                            {
                                var component = Rock.Security.AuthenticationContainer.GetComponent( login.EntityType.Name );
                                if ( !component.RequiresRemoteAuthentication )
                                {
                                    login.IsConfirmed = false;
                                }
                            }
                        }

                        rockContext.SaveChanges();

                        // Delete the merged person's other family member records and the family if they were the only one in the family
                        Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                        foreach ( var familyMember in familyMemberService.Queryable().Where( m => m.PersonId == p.Id && m.Group.GroupType.Guid == familyGuid ) )
                        {
                            familyMemberService.Delete( familyMember );

                            rockContext.SaveChanges();

                            // Get the family
                            var family = familyService.Queryable( "Members" ).Where( f => f.Id == familyMember.GroupId ).FirstOrDefault();
                            if ( !family.Members.Any() )
                            {
                                // If there are not any other family members, delete the family record.

                                var oldFamilyChanges = new List<string>();
                                History.EvaluateChange( oldFamilyChanges, "Family", family.Name, string.Empty );
                                HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                    primaryPersonId.Value, oldFamilyChanges, family.Name, typeof( Group ), family.Id );

                                // If theres any people that have this group as a giving group, set it to null (the person being merged should be the only one)
                                foreach ( Person gp in personService.Queryable().Where( g => g.GivingGroupId == family.Id ) )
                                {
                                    gp.GivingGroupId = null;
                                }

                                // Delete the family
                                familyService.Delete( family );

                                rockContext.SaveChanges();

                            }
                        }
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

            NavigateToLinkedPage( "PersonDetailPage", "PersonId", primaryPersonId.Value );
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
                keyCol.DataField = "Key";
                keyCol.Visible = false;
                gValues.Columns.Add( keyCol );

                var labelCol = new BoundField();
                labelCol.DataField = "Label";
                //labelCol.HeaderStyle.CssClass = "grid-section-header";
                gValues.Columns.Add( labelCol );

                var personService = new PersonService( new RockContext() );
                foreach ( var person in MergeData.People )
                {
                    var personCol = new PersonMergeField();
                    personCol.SelectionMode = SelectionMode.Single;
                    personCol.PersonId = person.Id;
                    personCol.PersonName = person.FullName;
                    personCol.HeaderContent = GetValuesColumnHeader( person.Id );
                    personCol.ModifiedDateTime = person.ModifiedDateTime;
                    personCol.ModifiedBy = person.ModifiedBy;
                    //personCol.HeaderStyle.CssClass = "grid-section-header";
                    personCol.DataTextField = string.Format( "property_{0}", person.Id );
                    personCol.DataSelectedField = string.Format( "property_{0}_selected", person.Id );
                    personCol.DataVisibleField = string.Format( "property_{0}_visible", person.Id );
                    personCol.OnDelete += personCol_OnDelete;
                    gValues.Columns.Add( personCol );
                }
            }
        }

        /// <summary>
        /// Gets the values column header.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        private string GetValuesColumnHeader( int personId )
        {
            Guid familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

            var groupMemberService = new GroupMemberService( new RockContext() );
            var families = groupMemberService.Queryable()
                .Where( m => m.PersonId == personId && m.Group.GroupType.Guid == familyGuid )
                .Select( m => m.Group )
                .Distinct();

            StringBuilder sbHeaderData = new StringBuilder();

            foreach ( var family in families )
            {
                sbHeaderData.Append( "<div class='merge-heading-family'>" );

                var nickNames = groupMemberService.Queryable( "Person" )
                    .Where( m => m.GroupId == family.Id )
                    .OrderBy( m => m.GroupRole.Order )
                    .ThenBy( m => m.Person.BirthDate ?? DateTime.MinValue )
                    .ThenByDescending( m => m.Person.Gender )
                    .Select( m => m.Person.NickName )
                    .ToList();
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
                    sbHeaderData.AppendFormat( " <br><span>{0}{1}</span>",
                        loc.Location.ToStringSafe(),
                        ( showType ? " (" + loc.GroupLocationTypeValue.Value + ")" : "" ) );
                }

                sbHeaderData.Append( "</div>" );
            }

            return sbHeaderData.ToString();

        }

        /// <summary>
        /// Binds the values.
        /// </summary>
        private void BindGrid()
        {
            if ( MergeData != null && MergeData.People != null && MergeData.People.Any() )
            {
                // If the people have different email addresses and any logins, display security alert box
                nbSecurityNotice.Visible = 
                    ( MergeData.People.Select( p => p.Email ).Where( e => e != null && e != "").Distinct().Count() > 1 && 
                    MergeData.People.Where( p => p.HasLogins ).Any() ) ;

                foreach ( var col in gValues.Columns.OfType<PersonMergeField>() )
                {
                    col.IsPrimaryPerson = col.PersonId == MergeData.PrimaryPersonId;
                    if (col.IsPrimaryPerson)
                    {
                        hfSelectedColumn.Value = col.ColumnIndex.ToString();
                    }
                }

                DataTable dt = MergeData.GetDataTable( headingKeys );
                gValues.DataSource = dt;
                gValues.DataBind();
            }
        }

        /// <summary>
        /// Gets the values selection.
        /// </summary>
        private void GetValuesSelection()
        {
            foreach ( var column in gValues.Columns.OfType<PersonMergeField>() )
            {
                // Get person id from the datafield that has format 'property_{0}' with {0} being the person id
                int personId = int.Parse( column.DataTextField.Substring( 9 ) );

                // Set the correct person's value as selected
                foreach( var property in MergeData.Properties.Where( p => column.SelectedKeys.Contains( p.Key ) ))
                {
                    foreach( var personValue in property.Values)
                    {
                        personValue.Selected = personValue.PersonId == personId;
                    }
                }
            }
        }

        private string GetNewStringValue( string key, List<string> changes )
        {
            var ppValue = GetNewValue( key, changes );
            return ppValue != null ? ppValue.Value : string.Empty;
        }

        private int? GetNewIntValue( string key, List<string> changes )
        {
            var ppValue = GetNewValue( key, changes );
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

        private bool? GetNewBoolValue( string key, List<string> changes )
        {
            var ppValue = GetNewValue( key, changes );
            if (ppValue != null)
            {
                bool newValue = false;
                if (bool.TryParse(ppValue.Value, out newValue))
                {
                    return newValue;
                }
            }
            
            return null;
        }

        private DateTime? GetNewDateTimeValue( string key, List<string> changes )
        {
            var ppValue = GetNewValue( key, changes );
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

        private Enum GetNewEnumValue( string key, Type enumType, List<string> changes )
        {
            var ppValue = GetNewValue(key, changes);
            if (ppValue != null)
            {
                return (Enum)Enum.Parse( enumType, ppValue.Value );
            }

            return null;
        }

        private PersonPropertyValue GetNewValue(string key, List<string> changes)
        {
            var property = MergeData.GetProperty( key );
            var primaryPersonValue = property.Values.Where( v => v.PersonId == MergeData.PrimaryPersonId ).FirstOrDefault();
            var selectedPersonValue = property.Values.Where( v => v.Selected ).FirstOrDefault();

            string oldValue = primaryPersonValue != null ? primaryPersonValue.Value : string.Empty;
            string newValue = selectedPersonValue != null ? selectedPersonValue.Value : string.Empty;

            if (oldValue != newValue)
            {
                string oldFormattedValue = primaryPersonValue != null ? primaryPersonValue.FormattedValue : string.Empty;
                string newFormattedValue = selectedPersonValue != null ? selectedPersonValue.FormattedValue : string.Empty;
                History.EvaluateChange( changes, property.Label, oldFormattedValue, newFormattedValue );
            }

            return selectedPersonValue;
        }

        #endregion

    }

    #region MergeData Class

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    class MergeData
    {

        #region Properties

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
        public MergeData( List<Person> people, List<string> headingKeys )
        {
            People = new List<MergePerson>();
            Properties = new List<PersonProperty>();

            foreach ( var person in people )
            {
                AddPerson( person );
            }

            var phoneTypes = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).DefinedValues;
            foreach ( var person in people )
            {
                AddProperty( "PhoneNumbers", "Phone Numbers", 0, string.Empty );

                foreach ( var phoneType in phoneTypes )
                {
                    string key = "phone_" + phoneType.Id.ToString();
                    var phoneNumber = person.PhoneNumbers.Where( p => p.NumberTypeValueId == phoneType.Id ).FirstOrDefault();
                    if ( phoneNumber != null )
                    {
                        AddProperty( key, phoneType.Value, person.Id, phoneNumber.Number, phoneNumber.ToString() );
                    }
                    else
                    {
                        AddProperty( key, phoneType.Value, person.Id, string.Empty, string.Empty );
                    }
                }
            }

            foreach ( var person in people )
            {
                AddProperty( "PersonAttributes", "Person Attributes", 0, string.Empty );
                person.LoadAttributes();
                foreach ( var attribute in person.Attributes.OrderBy( a => a.Value.Order ) )
                {
                    string value = person.GetAttributeValue( attribute.Key );
                    string formattedValue = attribute.Value.FieldType.Field.FormatValue( null, value, attribute.Value.QualifierValues, false );
                    AddProperty( "attr_" + attribute.Key, attribute.Value.Name, person.Id, value, formattedValue );
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

            SetPrimary( people.OrderBy( p => p.CreatedDateTime ).Select( p => p.Id ).FirstOrDefault() );

        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Sets the primary.
        /// </summary>
        /// <param name="primaryPersonId">The primary person identifier.</param>
        public void SetPrimary( int primaryPersonId )
        {
            PrimaryPersonId = primaryPersonId;

            foreach ( var personProperty in Properties )
            {
                PersonPropertyValue value = null;

                if ( personProperty.Values.Any( v => v.Value != null && v.Value != "" ) )
                {
                    // Find primary person's non-blank value
                    value = personProperty.Values.Where( v => v.PersonId == primaryPersonId && v.Value != null && v.Value != "" ).FirstOrDefault();
                    if ( value == null )
                    {
                        // Find any other selected value
                        value = personProperty.Values.Where( v => v.Selected ).FirstOrDefault();
                        if ( value == null )
                        {
                            // Find first non-blank value
                            value = personProperty.Values.Where( v => v.Value != "" ).FirstOrDefault();
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
        public PersonPropertyValue GetSelectedValue(PersonProperty personProperty)
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
        public DataTable GetDataTable( List<string> headingKeys )
        {
            var tbl = new DataTable();

            tbl.Columns.Add( "Key" );
            tbl.Columns.Add( "Label" );

            foreach ( var person in People )
            {
                tbl.Columns.Add( string.Format( "property_{0}", person.Id ) );
                tbl.Columns.Add( string.Format( "property_{0}_selected", person.Id ), typeof( bool ) );
                tbl.Columns.Add( string.Format( "property_{0}_visible", person.Id ), typeof( bool ) );
            }

            List<object> heading = null;
            foreach ( var personProperty in Properties )
            {
                // If this is a heading property, or this is a property with more than one disctict value build the row data
                if ( headingKeys.Contains( personProperty.Key ) ||
                    personProperty.Values.Select( v => v.Value ).Distinct().Count() > 1 )
                {
                    var rowValues = new List<object>();
                    rowValues.Add( personProperty.Key );
                    rowValues.Add( personProperty.Label );

                    foreach ( var person in People )
                    {
                        var value = personProperty.Values.Where( v => v.PersonId == person.Id ).FirstOrDefault();
                        string formattedValue = value != null ? value.FormattedValue : string.Empty;
                        rowValues.Add( formattedValue );
                        rowValues.Add( value != null ? value.Selected : false );
                        if ( headingKeys.Contains( personProperty.Key ) )
                        {
                            rowValues.Add( false );
                        }
                        else
                        {
                            rowValues.Add( true );
                        }
                    }

                    if ( headingKeys.Contains( personProperty.Key ) )
                    {
                        heading = rowValues;
                    }
                    else
                    {
                        if ( heading != null )
                        {
                            tbl.Rows.Add( heading.ToArray() );
                            heading = null;
                        }

                        tbl.Rows.Add( rowValues.ToArray() );
                    }
                }
            }

            return tbl;
        }

        #endregion

        #region Private Methods

        private void AddPerson( Person person )
        {
            People.Add( new MergePerson( person ) );

            AddProperty( "Photo", "Photo", person.Id,
                person.PhotoId.HasValue ? person.PhotoId.ToString() : string.Empty,
                Person.GetPhotoImageTag( person, 65, 65, "merge-photo" ) );
            AddProperty( "Title", person.Id, person.TitleValue );
            AddProperty( "FirstName", person.Id, person.FirstName );
            AddProperty( "NickName", person.Id, person.NickName );
            AddProperty( "MiddleName", person.Id, person.MiddleName );
            AddProperty( "LastName", person.Id, person.LastName );
            AddProperty( "Suffix", person.Id, person.SuffixValue );
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
        }

        private void AddProperty( string key, int personId, string value, bool selected = false )
        {
            AddProperty( key, key.SplitCase(), personId, value, value, selected );
        }

        private void AddProperty( string key, string label, int personId, string value, bool selected = false )
        {
            AddProperty( key, label, personId, value, value, selected );
        }

        private void AddProperty( string key, string label, int personId, string value, string formattedValue, bool selected = false )
        {
            var property = GetProperty( key, true, label );
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

        private void AddProperty( string key, int personId, DefinedValue value, bool selected = false )
        {
            AddProperty( key, key.SplitCase(), personId, value, selected );
        }

        private void AddProperty( string key, string label, int personId, DefinedValue value, bool selected = false )
        {
            var property = GetProperty( key, true, label );
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

        private void AddProperty( string key, int personId, bool? value, bool selected = false )
        {
            AddProperty( key, personId, ( value ?? false ).ToString(), selected );
        }

        private void AddProperty( string key, int personId, DateTime? value, bool selected = false )
        {
            AddProperty( key, personId, value.HasValue ? value.Value.ToShortDateString() : string.Empty, selected );
        }

        private void AddProperty( string key, int personId, Enum value, bool selected = false )
        {
            AddProperty( key, key.SplitCase(), personId, value.ConvertToString(false), value.ConvertToString(), selected );
        }

        public PersonProperty GetProperty( string key, bool createIfNotFound = false, string label = "")
        {
            var property = Properties.Where( p => p.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
            if ( property == null && createIfNotFound )
            {
                if (label == string.Empty)
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
    class MergePerson
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public string ModifiedBy { get; set; }
        public string Email { get; set; }
        public bool HasLogins { get; set; }

        public MergePerson( Person person )
        {
            Id = person.Id;
            FullName = person.FullName;
            ModifiedDateTime = person.ModifiedDateTime;
            Email = person.Email;
            HasLogins = person.Users.Any();

            if ( person.ModifiedByPersonAlias != null &&
                person.ModifiedByPersonAlias.Person != null )
            {
                ModifiedBy = person.ModifiedByPersonAlias.Person.FullName;
            }
        }
    }

    #endregion

    #region PersonProperty Class

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    class PersonProperty
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public List<PersonPropertyValue> Values { get; set; }

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
    }

    #endregion

    #region PersonPropertyValue class

    [Serializable]
    class PersonPropertyValue
    {
        public int PersonId { get; set; }
        public bool Selected { get; set; }
        public string Value { get; set; }
        public string FormattedValue { get; set; }
    }

    #endregion

}