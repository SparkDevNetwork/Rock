﻿// <copyright>
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

    [DecimalField( "Confidence Score High", "The minimum confidence score required to be considered a likely match", true, 80.00, order: 0 )]
    [DecimalField( "Confidence Score Low", "The maximum confidence score required to be considered an unlikely match. Values lower than this will not be shown in the grid.", true, 60.00, order: 1 )]
    [BooleanField( "Include Inactive", "Set to true to also include potential matches when both records are inactive.", false, order: 2 )]
    [BooleanField( "Include Businesses", "Set to true to also include potential matches when either record is a Business.", false, order: 3 )]
    [LinkedPage( "Detail Page", order: 4 )]
    public partial class PersonDuplicateList : RockBlock, ICustomGridColumns
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
            int recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
            int recordTypeBusinessId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            // list duplicates that:
            // - aren't confirmed as NotDuplicate and aren't IgnoreUntilScoreChanges,
            // - don't have the PersonAlias and DuplicatePersonAlias records pointing to the same person ( occurs after two people have been merged but before the Calculate Person Duplicates job runs).
            // - don't include records where both the Person and Duplicate are inactive (block option)
            var personDuplicateQry = personDuplicateService.Queryable()
                .Where( a => !a.IsConfirmedAsNotDuplicate )
                .Where( a => !a.IgnoreUntilScoreChanges )
                .Where( a => a.PersonAlias.PersonId != a.DuplicatePersonAlias.PersonId );

            if ( this.GetAttributeValue( "IncludeInactive" ).AsBoolean() == false )
            {
                personDuplicateQry = personDuplicateQry.Where( a => !( a.PersonAlias.Person.RecordStatusValueId == recordStatusInactiveId && a.DuplicatePersonAlias.Person.RecordStatusValueId == recordStatusInactiveId ) );
            }

            if ( this.GetAttributeValue( "IncludeBusinesses" ).AsBoolean() == false )
            {
                personDuplicateQry = personDuplicateQry.Where( a => !( a.PersonAlias.Person.RecordTypeValueId == recordTypeBusinessId || a.DuplicatePersonAlias.Person.RecordTypeValueId == recordTypeBusinessId ) );
            }

            double? confidenceScoreLow = GetAttributeValue( "ConfidenceScoreLow" ).AsDoubleOrNull();
            if ( confidenceScoreLow.HasValue )
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
                    person.SuffixValueId,
                    person.SuffixValue,
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

            // NOTE: Because the .Count() in the SetLinqDataSource() sometimes creates SQL that takes *significantly* 
            // longer (> 26 minutes) to execute than the actual query (< 1s), we're changing this to a 
            // simple .DataSource = ToList() for now until we have more time to consider an alternative solution.  
            // Examples of the SQL generated to select the data vs to get the count are documented in our private
            // Asana card here: https://app.asana.com/0/21779865363458/553205615179451   
            //gList.SetLinqDataSource( qry );
            gList.DataSource = qry.ToList();
            gList.DataBind();
        }

        #endregion
    }
}