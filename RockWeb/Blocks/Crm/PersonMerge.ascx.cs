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

    public partial class PersonMerge : Rock.Web.UI.RockBlock
    {

        #region Fields

        private List<string> headingKeys = new List<string> {
            "PhoneNumbers", 
            "PersonAttributes"
        };

        #endregion

        #region Properties

        private List<int> SelectedPersonIds { get; set; }
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
            SelectedPersonIds = ViewState["SelectedPersonIds"] as List<int>;
            MergeData = ViewState["MergeData"] as MergeData;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPeople.DataKeyNames = new string[] { "Id" };
            gPeople.AllowPaging = false;
            gPeople.ShowActionRow = false;
            gPeople.ShowConfirmDeleteDialog = false;

            gValues.DataKeyNames = new string[] { "Key" };
            gValues.AllowPaging = false;
            gValues.ShowActionRow = false;
            gValues.RowDataBound += gValues_RowDataBound;

            if ( !Page.IsPostBack )
            {
                SelectedPersonIds = PageParameter( "People" ).SplitDelimitedValues().Select( p => p.AsInteger().Value ).ToList();
            }

            string script = @"
    $(""input[name$='PrimaryPerson']"").change( function (event) {
        var rbId = $(this).attr('id');
        var colId = rbId.substring(rbId.lastIndexOf('_')+1);
        $(this).closest('table').find(""input[id$='cbSelect_"" + colId + ""']"").each(function(index) {
            var textNode = this.nextSibling;
            if ( !$(this).is(':last-child') || (textNode && textNode.nodeType == 3)) {
                $(this).prop('checked', true);
            }
        });
    });
";
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
                BindPeople();
            }
            else
            {
                if ( pnlSelectValues.Visible )
                {
                    // Save the primary header radio button's selection
                    foreach ( var col in gValues.Columns.OfType<PersonMergeField>() )
                    {
                        var colIndex = gValues.Columns.IndexOf( col ).ToString();
                        var rb = gValues.HeaderRow.FindControl( "rbSelectPrimary_" + colIndex ) as RadioButton;
                        if ( rb != null )
                        {
                            string value = Page.Request.Form[rb.UniqueID.Replace( rb.ID, rb.GroupName )];
                            if ( value == rb.ClientID )
                            {
                                MergeData.PrimaryPersonId = col.PersonId;
                            }
                        }
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
            ViewState["SelectedPersonIds"] = SelectedPersonIds ?? new List<int>();
            ViewState["MergeData"] = MergeData ?? new MergeData();
            return base.SaveViewState();
        }

        #endregion

        #region Events

        #region Select People Events

        /// <summary>
        /// Handles the SelectPerson event of the ppAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAdd_SelectPerson( object sender, EventArgs e )
        {
            int? personId = ppAdd.PersonId;
            if ( personId.HasValue && !SelectedPersonIds.Contains( personId.Value ) )
            {
                SelectedPersonIds.Add( personId.Value );
            }

            ppAdd.SetValue( null );

            BindPeople();
        }

        /// <summary>
        /// Handles the Delete event of the gPeople control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPeople_Delete( object sender, RowEventArgs e )
        {
            SelectedPersonIds.Remove( (int)e.RowKeyValue );

            BindPeople();
        }

        /// <summary>
        /// Handles the Click event of the lbSelectPeople control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSelectPeople_Click( object sender, EventArgs e )
        {
            if ( SelectedPersonIds.Count > 1 )
            {
                // Get the people selected
                var people = new PersonService().Queryable( "CreatedByPersonAlias.Person" )
                    .Where( p => SelectedPersonIds.Contains( p.Id ) )
                    .ToList();

                // Create the data structure used to build grid
                MergeData = new MergeData( people, headingKeys );

                BuildValuesColumns();

                BindValues();

                pnlSelectPeople.Visible = false;
                pnlSelectValues.Visible = true;
                pnlConfirm.Visible = false;
            }
            else
            {
                nbPeople.Visible = true;
            }
        }

        #endregion

        #region Select Values Events

        /// <summary>
        /// Handles the RowDataBound event of the gValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gValues_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                if ( headingKeys.Contains( gValues.DataKeys[e.Row.RowIndex].Value.ToString() ) )
                {
                    e.Row.AddCssClass( "grid-section-header" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSelectValuesBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSelectValuesBack_Click( object sender, EventArgs e )
        {
            pnlSelectPeople.Visible = true;
            pnlSelectValues.Visible = false;
            pnlConfirm.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbSelectValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSelectValues_Click( object sender, EventArgs e )
        {
            GetValuesSelection();

            var tbl = new DataTable();

            tbl.Columns.Add( "Property" );
            tbl.Columns.Add( "CurrentValue" );
            tbl.Columns.Add( "NewValue" );
            tbl.Columns.Add( "ValueUpdated", typeof( bool ) );

            foreach ( var personProperty in MergeData.Properties )
            {
                var primaryValue = personProperty.Values
                    .Where( p => p.PersonId == MergeData.PrimaryPersonId )
                    .FirstOrDefault();

                var selectedValue = personProperty.Values
                    .Where( p => p.Selected)
                    .FirstOrDefault();

                string oldValue = primaryValue != null ? primaryValue.FormattedValue : string.Empty;
                string newValue = selectedValue != null ? selectedValue.FormattedValue : string.Empty;

                if ( !string.IsNullOrWhiteSpace( oldValue ) || !string.IsNullOrWhiteSpace( newValue ) )
                {
                    var rowValues = new List<object>();
                    rowValues.Add( personProperty.Label );
                    rowValues.Add( oldValue );
                    rowValues.Add( newValue );
                    rowValues.Add( ( primaryValue != null ? primaryValue.Value : string.Empty ) !=
                        ( selectedValue != null ? selectedValue.FormattedValue : string.Empty ) );
                    tbl.Rows.Add( rowValues.ToArray() );
                }
            }

            gConfirm.DataSource = tbl;
            gConfirm.DataBind();

            pnlSelectPeople.Visible = false;
            pnlSelectValues.Visible = false;
            pnlConfirm.Visible = true;
        }

        #endregion

        #region Confirm Events

        /// <summary>
        /// Handles the Click event of the lbConfirmBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConfirmBack_Click( object sender, EventArgs e )
        {
            BindValues();

            pnlSelectPeople.Visible = false;
            pnlSelectValues.Visible = true;
            pnlConfirm.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConfirm_Click( object sender, EventArgs e )
        {
            var oldPhotos = new List<int>();

            RockTransactionScope.WrapTransaction( () =>
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    var personService = new PersonService();
                    var binaryFileService = new BinaryFileService();

                    Person primaryPerson = personService.Get( MergeData.PrimaryPersonId ?? 0 );
                    if ( primaryPerson != null )
                    {
                        var changes = new List<string>();
                        foreach ( var p in MergeData.People.Where( p => p.Id != primaryPerson.Id ) )
                        {
                            changes.Add( string.Format( "Merged <span class='field-value'>{0} [ID: {1}]</span> with this record.", p.FullName, p.Id ) );
                        }

                        // Photo Id
                        int? newPhotoId = MergeData.GetSelectedValue( "Photo" ).AsInteger( false );
                        if ( !primaryPerson.PhotoId.Equals( newPhotoId ) )
                        {
                            changes.Add( "Modified the photo." );
                            primaryPerson.PhotoId = newPhotoId;
                        }

                        primaryPerson.TitleValueId = GetSelectedDefinedValue( "Title", primaryPerson.TitleValueId, changes );
                        primaryPerson.FirstName = GetSelectedValue( "FirstName", primaryPerson.FirstName, changes );
                        primaryPerson.NickName = GetSelectedValue( "NickName", primaryPerson.NickName, changes );
                        primaryPerson.MiddleName = GetSelectedValue( "MiddleName", primaryPerson.MiddleName, changes );
                        primaryPerson.LastName = GetSelectedValue( "LastName", primaryPerson.LastName, changes );
                        primaryPerson.SuffixValueId = GetSelectedDefinedValue( "Suffix", primaryPerson.SuffixValueId, changes );
                        primaryPerson.RecordTypeValueId = GetSelectedDefinedValue( "RecordType", primaryPerson.RecordTypeValueId, changes );
                        primaryPerson.RecordStatusValueId = GetSelectedDefinedValue( "RecordStatus", primaryPerson.RecordStatusValueId, changes );
                        primaryPerson.RecordStatusReasonValueId = GetSelectedDefinedValue( "RecordStatusReason", primaryPerson.RecordStatusReasonValueId, changes );
                        primaryPerson.ConnectionStatusValueId = GetSelectedDefinedValue( "ConnectionStatus", primaryPerson.ConnectionStatusValueId, changes );
                        primaryPerson.IsDeceased = GetSelectedValue( "Deceased", primaryPerson.IsDeceased, changes );
                        primaryPerson.Gender = (Gender)GetSelectedValue( "Gender", primaryPerson.Gender, changes );
                        primaryPerson.MaritalStatusValueId = GetSelectedDefinedValue( "MaritalStatus", primaryPerson.MaritalStatusValueId, changes );
                        primaryPerson.BirthDate = GetSelectedValue( "BirthDate", primaryPerson.BirthDate, changes );
                        primaryPerson.AnniversaryDate = GetSelectedValue( "AnniversaryDate", primaryPerson.AnniversaryDate, changes );
                        primaryPerson.GraduationDate = GetSelectedValue( "GraduationDate", primaryPerson.GraduationDate, changes );
                        primaryPerson.Email = GetSelectedValue( "Email", primaryPerson.Email, changes );
                        primaryPerson.IsEmailActive = GetSelectedValue( "EmailActive", primaryPerson.IsEmailActive, changes );
                        primaryPerson.EmailNote = GetSelectedValue( "EmailNote", primaryPerson.EmailNote, changes );
                        primaryPerson.DoNotEmail = GetSelectedValue( "DoNotEmail", primaryPerson.DoNotEmail, changes );
                        primaryPerson.SystemNote = GetSelectedValue( "SystemNote", primaryPerson.SystemNote, changes );

                        // Save the new record
                        personService.Save( primaryPerson, CurrentPersonAlias );
                        // TODO Get phone number

                        // TODO Get attribute values

                        new HistoryService().SaveChanges( typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                            primaryPerson.Id, changes, CurrentPersonAlias );

                        foreach ( var p in MergeData.People.Where( p => p.Id != primaryPerson.Id ) )
                        {
                            // Call merge proc to update all related data and delete record
                        }

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
                                    //binaryFileService.Delete( photo, CurrentPersonAlias );
                                    binaryFileService.Save( photo, CurrentPersonAlias );
                                }
                            }
                        }

                    }
                }

            } );

        }

        #endregion

        #endregion

        #region Methods

        #region People Grid Methods

        private void BindPeople()
        {
            var people = new PersonService()
                .Queryable( "CreatedByPersonAlias.Person,ModifiedByPersonAlias.Person" )
                .Where( p => SelectedPersonIds.Contains( p.Id ) )
                .ToList();

            gPeople.DataSource = people;
            gPeople.DataBind();
        }

        #endregion

        #region Values Panel Methods

        /// <summary>
        /// Builds the values columns.
        /// </summary>
        private void BuildValuesColumns()
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
                labelCol.HeaderText = "Value";
                gValues.Columns.Add( labelCol );

                var personService = new PersonService();
                foreach ( var person in MergeData.People )
                {
                    var personCol = new PersonMergeField();
                    personCol.SelectionMode = SelectionMode.Single;
                    personCol.PersonId = person.Id;
                    personCol.PersonName = person.FullName;
                    personCol.HeaderContent = GetValuesColumnHeader( person.Id );
                    personCol.ModifiedDateTime = person.ModifiedDateTime;
                    personCol.ModifiedBy = person.ModifiedBy;
                    personCol.DataTextField = string.Format( "property_{0}", person.Id );
                    personCol.DataSelectedField = string.Format( "property_{0}_selected", person.Id );
                    personCol.DataVisibleField = string.Format( "property_{0}_visible", person.Id );
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

            var groupMemberService = new GroupMemberService();
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
                    sbHeaderData.AppendFormat( " <span class='merge-heading-location'>{0}{1}</span>",
                        loc.Location.ToStringSafe(),
                        ( showType ? " (" + loc.GroupLocationTypeValue.Name + ")" : "" ) );
                }

                sbHeaderData.Append( "</div>" );
            }

            return sbHeaderData.ToString();

        }

        /// <summary>
        /// Binds the values.
        /// </summary>
        private void BindValues()
        {
            foreach ( var col in gValues.Columns.OfType<PersonMergeField>() )
            {
                col.IsPrimaryPerson = col.PersonId == MergeData.PrimaryPersonId;
            }

            DataTable dt = MergeData.GetDataTable( headingKeys );
            gValues.DataSource = dt;
            gValues.DataBind();
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
                MergeData.Properties
                    .Where( p => column.SelectedKeys.Contains( p.Key ))
                    .SelectMany( v => v.Values )
                    .Where( v => v.PersonId == personId )
                    .ToList()
                    .ForEach( v => v.Selected = true );
            }
        }

        #endregion

        #region Confirmation Panel Methods

        private string GetSelectedValue( string key, string value, List<string> changes )
        {
            return GetSelectedValue( key, key.SplitCase(), value, changes );
        }

        private string GetSelectedValue( string key, string label, string value, List<string> changes )
        {
            string newValue = MergeData.GetSelectedValue( key );
            if ( !value.Equals( newValue ) )
            {
                History.EvaluateChange( changes, label, value, newValue );
            }

            return newValue;
        }

        private Enum GetSelectedValue( string key, Enum value, List<string> changes )
        {
            return GetSelectedValue( key, key.SplitCase(), value, changes );
        }

        private Enum GetSelectedValue( string key, string label, Enum value, List<string> changes )
        {
            Enum newValue = (Enum)Enum.Parse( value.GetType(), MergeData.GetSelectedValue( key ) );
            if ( !value.Equals( newValue ) )
            {
                History.EvaluateChange( changes, label, value, newValue );
            }

            return newValue;
        }

        private bool GetSelectedValue( string key, bool value, List<string> changes )
        {
            return GetSelectedValue( key, key.SplitCase(), value, changes );
        }

        private bool GetSelectedValue( string key, string label, bool value, List<string> changes )
        {
            bool newValue = false;
            if ( !bool.TryParse( MergeData.GetSelectedValue( key ), out newValue ) )
            {
                newValue = false;
            }

            if ( !value.Equals( newValue ) )
            {
                History.EvaluateChange( changes, label, value.ToString(), newValue.ToString() );
            }

            return newValue;
        }

        private bool? GetSelectedValue( string key, bool? value, List<string> changes )
        {
            return GetSelectedValue( key, key.SplitCase(), value, changes );
        }

        private bool? GetSelectedValue( string key, string label, bool? value, List<string> changes )
        {
            bool? newValue = null;
            bool b = false;
            if ( bool.TryParse( MergeData.GetSelectedValue( key ), out b ) )
            {
                newValue = b;
            }

            if ( !value.Equals( newValue ) )
            {
                History.EvaluateChange( changes, label, ( value ?? false ).ToString(), ( newValue ?? false ).ToString() );
            }

            return newValue;
        }

        private DateTime? GetSelectedValue( string key, DateTime? value, List<string> changes )
        {
            return GetSelectedValue( key, key.SplitCase(), value, changes );
        }

        private DateTime? GetSelectedValue( string key, string label, DateTime? value, List<string> changes )
        {
            DateTime? newValue = null;
            DateTime d = DateTime.MinValue;
            if ( DateTime.TryParse( MergeData.GetSelectedValue( key ), out d ) )
            {
                newValue = d;
            }

            if ( !value.Equals( newValue ) )
            {
                History.EvaluateChange( changes, label, value, newValue );
            }

            return newValue;
        }

        private int? GetSelectedDefinedValue( string key, int? valueId, List<string> changes )
        {
            return GetSelectedDefinedValue( key, key.SplitCase(), valueId, changes );
        }

        private int? GetSelectedDefinedValue( string key, string label, int? valueId, List<string> changes )
        {
            int? newValueId = null;

            int i = int.MinValue;
            if ( int.TryParse( MergeData.GetSelectedValue( key ), out i ) )
            {
                newValueId = i;
            }

            if ( !valueId.Equals( newValueId ) )
            {
                History.EvaluateChange( changes, label,
                   valueId.HasValue ? DefinedValueCache.GetName( valueId ) : string.Empty,
                   newValueId.HasValue ? DefinedValueCache.GetName( newValueId ) : string.Empty );
            }

            return newValueId;
        }

        #endregion

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

            foreach ( var person in people )
            {
                AddProperty( "PhoneNumbers", "Phone Numbers", 0, string.Empty );

                foreach ( var phoneType in person.PhoneNumbers )
                {
                    string keyRoot = "phone_" + phoneType.NumberTypeValueId.ToString();

                    int i = 1;
                    string key = keyRoot;
                    while ( Properties.Where( p => p.Key == key && p.Values.Any( v => v.PersonId == person.Id ) ).Any() )
                    {
                        key = string.Format( "{0}_{1}", keyRoot, i++ );
                    }

                    AddProperty( key, phoneType.NumberTypeValue.Name, person.Id, phoneType.Number, phoneType.NumberFormatted );
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
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetSelectedValue( string key )
        {
            var personProperty = Properties.Where( p => p.Key == key ).FirstOrDefault();
            if ( personProperty != null )
            {
                return personProperty.Values.Where( v => v.Selected ).Select( v => v.Value ).FirstOrDefault();
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
                Person.GetPhotoImageTag( person, 100, 100 ) );
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
            AddProperty( "GraduationDate", person.Id, person.GraduationDate );
            AddProperty( "Email", person.Id, person.Email );
            AddProperty( "EmailActive", person.Id, person.IsEmailActive );
            AddProperty( "EmailNote", person.Id, person.EmailNote );
            AddProperty( "DoNotEmail", person.Id, person.DoNotEmail );
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
            var property = GetProperty( key, label );
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
            AddProperty( key, personId, value != null ? value.Name : "", selected );
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
            AddProperty( key, personId, value.ConvertToString(), selected );
        }

        private PersonProperty GetProperty( string key, string label )
        {
            var property = Properties.Where( p => p.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
            if ( property == null )
            {
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

        public MergePerson( Person person )
        {
            Id = person.Id;
            FullName = person.FullName;
            ModifiedDateTime = person.ModifiedDateTime;
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