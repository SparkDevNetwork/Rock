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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
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

    [DecimalField( "Match Percent High", "The minimum percent score required to be considered a likely match", true, 80.00 )]
    [DecimalField( "Match Percent Low", "The max percent score required to be considered an unlikely match", true, 40.00 )]
    [LinkedPage("Detail Page")]
    public partial class PersonDuplicateList : RockBlock
    {
        /// <summary>
        /// Gets the match label class.
        /// </summary>
        /// <param name="score">The score.</param>
        /// <returns></returns>
        public string GetMatchLabelClass( double? percent )
        {
            if ( percent >= this.GetAttributeValue( "MatchPercentHigh" ).AsDoubleOrNull() )
            {
                return "label label-success";
            }
            else if ( percent <= this.GetAttributeValue( "MatchPercentLow" ).AsDoubleOrNull() )
            {
                return "label label-default";
            }
            else
            {
                return "label label-warning";
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

            var personDuplicateQry = personDuplicateService.Queryable().Where( a => !a.IsConfirmedAsNotDuplicate );

            var groupByQry = personDuplicateQry.GroupBy( a => a.PersonAlias.Person );

            var qry = groupByQry.Select( a => new
            {
                PersonId = a.Key.Id,
                LastName = a.Key.LastName,
                FirstName = a.Key.FirstName,
                MatchCount = a.Count(),
                MaxScorePercent = a.Max( s=> s.Capacity > 0 ? s.Score / ( s.Capacity * .01 ) : (double?)null),
                PersonModifiedDateTime = a.Key.ModifiedDateTime,
                CreatedByPerson = a.Key.CreatedByPersonAlias.Person.FirstName + " " + a.Key.CreatedByPersonAlias.Person.LastName
            } );

            double? matchPercentLow = GetAttributeValue( "MatchPercentLow" ).AsDoubleOrNull();
            if (matchPercentLow.HasValue)
            {
                qry = qry.Where( a => a.MaxScorePercent >= matchPercentLow );
            }


            SortProperty sortProperty = gList.SortProperty;
            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( a => a.MaxScorePercent ).ThenBy( a => a.LastName ).ThenBy( a => a.FirstName );
            }

            gList.DataSource = qry.ToList();
            gList.DataBind();
        }

        #endregion
    }
}