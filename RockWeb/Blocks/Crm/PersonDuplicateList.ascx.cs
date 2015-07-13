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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

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
    /// List of person records that have possible duplicates
    /// </summary>
    [DisplayName( "Person Duplicate List" )]
    [Category( "CRM" )]
    [Description( "List of person records that have possible duplicates" )]

    [DecimalField( "Confidence Score High", "The minimum confidence score required to be considered a likely match", true, 80.00 )]
    [DecimalField( "Confidence Score Low", "The maximum confidence score required to be considered an unlikely match. Values lower than this will not be shown in the grid.", true, 60.00 )]
    [LinkedPage( "Detail Page" )]
    public partial class PersonDuplicateList : RockBlock
    {
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

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "PersonId", e.RowKeyId );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            var personDuplicateService = new PersonDuplicateService( rockContext );
            int recordStatusInactiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

            // list duplicates that aren't confirmed as NotDuplicate and aren't IgnoreUntilScoreChanges. Also, don't include records where both the Person and Duplicate are inactive
            var personDuplicateQry = personDuplicateService.Queryable()
                .Where( a => !a.IsConfirmedAsNotDuplicate )
                .Where( a => !a.IgnoreUntilScoreChanges )
                .Where( a => !(a.PersonAlias.Person.RecordStatusValueId == recordStatusInactiveId && a.DuplicatePersonAlias.Person.RecordStatusValueId == recordStatusInactiveId) );

            double? confidenceScoreLow = GetAttributeValue( "ConfidenceScoreLow" ).AsDoubleOrNull();
            if (confidenceScoreLow.HasValue)
            {
                personDuplicateQry = personDuplicateQry.Where( a => a.ConfidenceScore > confidenceScoreLow );
            }

            var groupByQry = personDuplicateQry.GroupBy( a => a.PersonAlias.PersonId );

            var qryPerson = new PersonService( rockContext ).Queryable();

            var qry = groupByQry.Select( a => new
            {
                PersonId = a.Key,
                MatchCount = a.Count(),
                MaxConfidenceScore = a.Max( s => s.ConfidenceScore ),
            } ).Join(
            qryPerson,
            k1 => k1.PersonId,
            k2 => k2.Id,
            ( personDuplicate, person ) => 
                new 
                { 
                    PersonId = person.Id,
                    person.LastName,
                    person.FirstName,
                    PersonModifiedDateTime = person.ModifiedDateTime,
                    CreatedByPerson = person.CreatedByPersonAlias.Person.FirstName + " " + person.CreatedByPersonAlias.Person.LastName,
                    personDuplicate.MatchCount,
                    personDuplicate.MaxConfidenceScore
                } );

            SortProperty sortProperty = gList.SortProperty;
            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( a => a.MaxConfidenceScore ).ThenBy( a => a.LastName ).ThenBy( a => a.FirstName );
            }

            gList.SetLinqDataSource( qry );
            gList.DataBind();
        }

        #endregion
    }
}