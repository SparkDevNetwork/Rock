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

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Person Duplicate Detail" )]
    [Category( "CRM" )]
    [Description( "Shows records that are possible duplicates of the selected person" )]
    [DecimalField( "Confidence Score High", "The minimum confidence score required to be considered a likely match", true, 80.00 )]
    [DecimalField( "Confidence Score Low", "The maximum confidence score required to be considered an unlikely match. Values lower than this will not be shown in the grid.", true, 40.00 )]
    public partial class PersonDuplicateDetail : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gList.Actions.ShowAdd = false;
            gList.DataKeyNames = new string[] { "PersonId" };
            gList.GridRebind += gList_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the Confidence Score HTML include bootstrap label
        /// </summary>
        /// <param name="confidenceScore">The confidence score.</param>
        /// <returns></returns>
        public string GetConfidenceScoreColumnHtml( double? confidenceScore )
        {
            string css;

            if ( confidenceScore >= this.GetAttributeValue( "ConfidenceScoreHigh" ).AsDoubleOrNull() )
            {
                css = "label label-success";
            }
            else if ( confidenceScore <= this.GetAttributeValue( "ConfidenceScoreLow" ).AsDoubleOrNull() )
            {
                css = "label label-default";
            }
            else
            {
                css = "label label-warning";
            }

            if ( confidenceScore.HasValue )
            {
                return string.Format( "<span class='{0}'>{1}</span>", css, ( confidenceScore.Value / 100 ).ToString( "P" ) );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the person view onclick.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public string GetPersonViewOnClick( int personId )
        {
            var url = "/person/" + personId.ToString();

            // force the link to open a new scrollable,resizable browser window (and make it work in FF, Chrome and IE) http://stackoverflow.com/a/2315916/1755417
            return string.Format( "javascript: window.open('{0}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;", url );
        }

        /// <summary>
        /// Gets the campus.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public List<Campus> GetCampuses( Person person )
        {
            return person.GetFamilies().Select( a => a.Campus ).ToList();
        }

        /// <summary>
        /// Gets the addresses.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public List<GroupLocation> GetGroupLocations( Person person )
        {
            return person.GetFamilies().Select( a => a.GroupLocations ).SelectMany( a => a ).OrderByDescending( a => a.IsMappedLocation ).ThenBy( a => a.Id ).ToList();
        }

        /// <summary>
        /// Gets the phone numbers.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public List<PhoneNumber> GetPhoneNumbers( Person person )
        {
            return person.PhoneNumbers.ToList();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            var personDuplicateService = new PersonDuplicateService( rockContext );
            var personService = new PersonService( rockContext );
            int personId = this.PageParameter( "PersonId" ).AsInteger();
            int recordStatusInactiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

            //// select person duplicate records
            //// list duplicates that aren't confirmed as NotDuplicate and aren't IgnoreUntilScoreChanges. Also, don't include records where both the Person and Duplicate are inactive
            var qry = personDuplicateService.Queryable()
                .Where( a => a.PersonAlias.PersonId == personId && !a.IsConfirmedAsNotDuplicate && !a.IgnoreUntilScoreChanges )
                .Where( a => a.PersonAlias.Person.RecordStatusValueId != recordStatusInactiveId && a.DuplicatePersonAlias.Person.RecordStatusValueId != recordStatusInactiveId )
                .Select( s => new
                {
                    PersonId = s.DuplicatePersonAlias.Person.Id, // PersonId has to be the key field in the grid for the Merge button to work
                    PersonDuplicateId = s.Id,
                    DuplicatePerson = s.DuplicatePersonAlias.Person,
                    s.ConfidenceScore,
                    IsComparePerson = true
                } );

            double? confidenceScoreLow = GetAttributeValue( "ConfidenceScoreLow" ).AsDoubleOrNull();
            if ( confidenceScoreLow.HasValue )
            {
                qry = qry.Where( a => a.ConfidenceScore >= confidenceScoreLow );
            }

            qry = qry.OrderByDescending( a => a.ConfidenceScore ).ThenBy( a => a.DuplicatePerson.LastName ).ThenBy( a => a.DuplicatePerson.FirstName );

            var gridList = qry.ToList();

            // put the person we are comparing the duplicates to at the top of the list
            var person = personService.Get( personId );
            gridList.Insert(
                0,
                new
                {
                    PersonId = person.Id, // PersonId has to be the key field in the grid for the Merge button to work
                    PersonDuplicateId = 0,
                    DuplicatePerson = person,
                    ConfidenceScore = (double?)null,
                    IsComparePerson = false
                } );

            nbNoDuplicatesMessage.Visible = gridList.Count == 1;

            gList.DataSource = gridList;
            gList.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the RowDataBound event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var person = e.Row.DataItem.GetPropertyValue( "DuplicatePerson" );
                bool isComparePerson = (bool)e.Row.DataItem.GetPropertyValue( "IsComparePerson" );

                // If this is the main person for the compare, select them, but then hide the checkbox. 
                var row = e.Row;
                var cell = row.Cells[gList.Columns.OfType<SelectField>().First().ColumnIndex];
                var selectBox = cell.Controls[0] as CheckBox;
                selectBox.Visible = isComparePerson;
                selectBox.Checked = !isComparePerson;

                if ( !isComparePerson )
                {
                    row.AddCssClass( "duplicate-source" );
                }

                // If this is the main person for the compare, hide the "not duplicate" button
                LinkButton btnNotDuplicate = e.Row.ControlsOfTypeRecursive<LinkButton>().FirstOrDefault( a => a.ID == "btnNotDuplicate" );
                btnNotDuplicate.Visible = isComparePerson;

                // If this is the main person for the compare, hide the "ignore duplicate" button
                LinkButton btnIgnoreDuplicate = e.Row.ControlsOfTypeRecursive<LinkButton>().FirstOrDefault( a => a.ID == "btnIgnoreDuplicate" );
                btnIgnoreDuplicate.Visible = isComparePerson;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNotDuplicate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNotDuplicate_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var personDuplicateService = new PersonDuplicateService( rockContext );
            int personDuplicateId = ( sender as LinkButton ).CommandArgument.AsInteger();
            var personDuplicate = personDuplicateService.Get( personDuplicateId );
            personDuplicate.IsConfirmedAsNotDuplicate = true;
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnIgnoreDuplicate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnIgnoreDuplicate_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var personDuplicateService = new PersonDuplicateService( rockContext );
            int personDuplicateId = ( sender as LinkButton ).CommandArgument.AsInteger();
            var personDuplicate = personDuplicateService.Get( personDuplicateId );
            personDuplicate.IgnoreUntilScoreChanges = true;
            rockContext.SaveChanges();

            BindGrid();
        }
    }
}