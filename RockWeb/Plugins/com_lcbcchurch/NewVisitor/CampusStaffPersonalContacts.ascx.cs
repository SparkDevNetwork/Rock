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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Web.UI.WebControls;
using Rock.Web.Cache;
using Rock.Reporting;
using Newtonsoft.Json;
using com.lcbcchurch.NewVisitor;

namespace RockWeb.Plugins.com_lcbcchurch.NewVisitor
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Campus Staff Personal Contacts" )]
    [Category( "LCBC > New Visitor" )]
    [Description( " List of staff of the assigned given campus with Connections metrics." )]

    #region Block Attributes
    [IntegerField( "Success Minimum",
        Description = "The minimum percentage to display the success label.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKeys.SuccessMinimum )]
    [IntegerField( "Warning Minimum",
        Description = "The minimum percentage to display the warning label.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKeys.WarningMinimum )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON,
        name: "Campus Attribute",
        description: "The person attribute used to determine which campus a staff person is assigned to.",
        required: true,
        allowMultiple: false,
        order: 2,
        Key = AttributeKeys.CampusAttribute )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON,
        name: "Staff Connection Expected Attribute",
        description: "The attribute that holds whether or not the staff member is currently part of the connection process.",
        required: true,
        allowMultiple: false,
        order: 3,
        Key = AttributeKeys.StaffConnectionExpectedAttribute )]
    [LinkedPage( "Staff Followups Page",
        Key = AttributeKeys.StaffFollowups,
        Description = "The page that handles the selected row clicks.",
        IsRequired = false )]
    # endregion Block Attributes
    public partial class CampusStaffPersonalContacts : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int _campusEntityId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;

        private List<Guid> CONNECTION_TYPES = new List<Guid>()
        {
            new Guid( "381303b2-5ac9-4d25-a6f5-acd35fff2fcf" ),
            new Guid( "f5c4ba16-6f9b-44b0-a357-3d935abc40ab" ),
            new Guid( "73f59c35-4dbc-4b0d-928f-d747668518e3" ),
            new Guid( "5cc3fcc6-86da-4b88-8060-bcd31a183c61" ),
            new Guid( "a301da6e-742a-4755-b50d-9611c0e134fd" ),
            new Guid( "ec8e4ed9-246b-4559-89cc-b9cd97c92c53" )

        };

        private Guid FACE_TO_FACE_CONNECTION_TYPE = new Guid( "ec8e4ed9-246b-4559-89cc-b9cd97c92c53" );

        #endregion

        #region Attribute Keys

        protected static class AttributeKeys
        {
            public const string SuccessMinimum = "SuccessMinimum";
            public const string WarningMinimum = "WarningMinimum";
            public const string CampusAttribute = "CampusAttribute";
            public const string StaffConnectionExpectedAttribute = "StaffConnectionExpectedAttribute";
            public const string StaffFollowups = "StaffFollowups";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var personEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

            gPersons.DataKeyNames = new string[] { "Id" };
            gPersons.PersonIdField = "Id";
            gPersons.GridRebind += gPersons_GridRebind;
            gPersons.EntityTypeId = personEntityTypeId;
            gPersons.RowSelected += gPersons_Selected;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                bool isValid = IsBlockSettingValid();
                if ( isValid )
                {
                    int? campusId = PageParameter( "campusId" ).AsIntegerOrNull();
                    if ( campusId.HasValue )
                    {
                        hfCampusId.SetValue( campusId.Value );
                        BindGrid();
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Selected event of the gPersons control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersons_Selected( object sender, RowEventArgs e )
        {
            int personId = e.RowKeyId;
            NavigateToLinkedPage( AttributeKeys.StaffFollowups, "PersonId", personId );
        }

        /// <summary>
        /// Gets the percentage HTML include bootstrap label
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        /// <returns></returns>
        public string GetPercentageColumnHtml( int percentage )
        {
            string css;

            var warningMinimum = GetAttributeValue( AttributeKeys.WarningMinimum ).AsInteger();
            var successMinimum = GetAttributeValue( AttributeKeys.SuccessMinimum ).AsInteger();
            if ( percentage >= successMinimum )
            {
                css = "label-success";
            }
            else if ( percentage >= warningMinimum )
            {
                css = "label-warning";
            }
            else
            {
                css = "label-danger";
            }
            return string.Format( "<span class='label {0}'>{1}%</span>", css, percentage );
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gPersons_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;
            int campusId = hfCampusId.ValueAsInt();
            CampusCache campus = CampusCache.Get( campusId );
            AttributeCache campusAttribute = AttributeCache.Get( GetAttributeValue( AttributeKeys.CampusAttribute ).AsGuid() );
            AttributeCache staffConnectionExpectedAttribute = AttributeCache.Get( GetAttributeValue( AttributeKeys.StaffConnectionExpectedAttribute ).AsGuid() );

            if ( campus == null )
            {
                return;
            }
            lCampus.Text = campus.Name;

            var campusGuid = campus.Guid.ToString();
            var campusPersonIds = new AttributeValueService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                     ( a.AttributeId == campusAttribute.Id &&
                    a.Value == campusGuid ) &&
                    a.Attribute.EntityTypeId == personEntityTypeId &&
                    a.EntityId.HasValue )
                   .Select( a => a.EntityId.Value )
                   .ToList();

            var personIds = new AttributeValueService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.AttributeId == staffConnectionExpectedAttribute.Id &&
                    a.ValueAsBoolean == true &&
                    a.Attribute.EntityTypeId == personEntityTypeId &&
                    a.EntityId.HasValue
                    && campusPersonIds.Contains( a.EntityId.Value ) )
                   .Select( a => a.EntityId.Value )
                   .ToList();

            var connectionTypes = new List<int>();
            foreach ( var item in CONNECTION_TYPES )
            {
                var noteType = NoteTypeCache.Get( item );

                if ( noteType != null )
                {
                    connectionTypes.Add( noteType.Id );
                }
            }

            var startDate = NewVisitorHelper.StartOfLastWeek( RockDateTime.Now );
            var previous4WeekStartDate = startDate.AddDays( -28 );
            var endDate = startDate.AddDays( 7 );
            var personConnections = new NoteService( rockContext )
                     .Queryable()
                     .Where( a =>
                     a.CreatedDateTime > startDate && a.CreatedDateTime < endDate
                      && connectionTypes.Contains( a.NoteTypeId )
                      && a.NoteType.EntityTypeId == personEntityTypeId
                      && a.EntityId.HasValue
                      && personIds.Contains( a.CreatedByPersonAlias.PersonId ) )
                     .Select( a => new { a.NoteTypeId, PersonId = a.CreatedByPersonAlias.PersonId } )
                     .ToList();


            var person4WeekConnections = new NoteService( rockContext )
                     .Queryable()
                     .Where( a =>
                     a.CreatedDateTime > previous4WeekStartDate && a.CreatedDateTime < startDate
                      && connectionTypes.Contains( a.NoteTypeId )
                      && a.NoteType.EntityTypeId == personEntityTypeId
                      && a.EntityId.HasValue
                      && personIds.Contains( a.CreatedByPersonAlias.PersonId ) )
                     .Select( a => new PersonConnection { PersonId = a.CreatedByPersonAlias.PersonId, Date = a.CreatedDateTime.Value } )
                     .ToList();

            var persons = new PersonService( rockContext ).GetByIds( personIds );
            List<PersonDataRow> personDataRows = new List<PersonDataRow>();
            var faceToFaceConnections = NoteTypeCache.Get( FACE_TO_FACE_CONNECTION_TYPE );
            foreach ( var person in persons )
            {
                PersonDataRow personDataRow = new PersonDataRow()
                {
                    Id = person.Id,
                    Person = person,
                    FullName = person.FullName,
                };

                personDataRow.LastWeek = personConnections.Any( a => a.PersonId == person.Id ) ? 100 : 0;
                personDataRow.FaceToFaceConnections = personConnections.Where( a => a.PersonId == person.Id && a.NoteTypeId == faceToFaceConnections.Id ).Count();
                personDataRow.OtherConnections = personConnections.Where( a => a.PersonId == person.Id && a.NoteTypeId != faceToFaceConnections.Id ).Count();
                personDataRow.Previous4Weeks = person4WeekConnections.Where( a => a.PersonId == person.Id ).DistinctBy( a => a.SundayDate ).Count() * 25;
                personDataRows.Add( personDataRow );
            }

            SortProperty sortProperty = gPersons.SortProperty;
            if ( sortProperty != null )
            {
                gPersons.DataSource = personDataRows.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gPersons.DataSource = personDataRows
                    .OrderBy( r => r.Person.LastName )
                    .ThenBy( r => r.Person.NickName ).ToList();
            }
            gPersons.DataBind();
        }

        private bool IsBlockSettingValid()
        {
            bool isValid = false;


            var attributeGuid = GetAttributeValue( AttributeKeys.CampusAttribute ).AsGuid();
            var attribute = AttributeCache.Get( attributeGuid );
            if ( !( attribute != null && attribute.FieldTypeId == FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.CAMPUS ) ).Id ) )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>Campus Attribute block setting is not correctly configured.</p>";
                nbWarningMessage.Visible = true;
                return isValid;
            }

            var staffConnectionExpectedGuid = GetAttributeValue( AttributeKeys.StaffConnectionExpectedAttribute ).AsGuid();
            var staffConnectionExpectedAttribute = AttributeCache.Get( staffConnectionExpectedGuid );
            if ( !( staffConnectionExpectedAttribute != null && staffConnectionExpectedAttribute.FieldTypeId == FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.BOOLEAN ) ).Id ) )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>Staff Connection Expected Attribute block setting is not correctly configured.</p>";
                nbWarningMessage.Visible = true;
                return isValid;
            }

            return true;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="DotLiquid.Drop" />
        public class PersonDataRow : DotLiquid.Drop
        {
            public int Id { get; set; }

            public Person Person { get; set; }

            public string FullName { get; set; }

            public int LastWeek { get; set; }

            public int FaceToFaceConnections { get; set; }

            public int OtherConnections { get; set; }

            public int Previous4Weeks { get; set; }

        }

        public class PersonConnection
        {
            public DateTime SundayDate { get { return Date.EndOfWeek( DayOfWeek.Saturday ); } }
            public DateTime Date { get; set; }
            public int PersonId { get; set; }
        }

        #endregion

    }
}