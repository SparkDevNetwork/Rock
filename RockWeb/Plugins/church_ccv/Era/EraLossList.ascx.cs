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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using church.ccv.Datamart.Model;

namespace RockWeb.Plugins.church_ccv.Era
{
    /// <summary>
    /// This block lists ERA losses based on filters
    /// </summary>
    [DisplayName( "ERA Loss List" )]
    [Category( "CCV > ERA" )]
    [Description( "This block lists ERA losses based on filters." )]
    public partial class EraLossList : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;

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

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var datamartEraLossQry = new DatamartEraLossService( rockContext ).Queryable();
            var datamartFamilyQry = new DatamartFamilyService( rockContext ).Queryable();
            var datamartNeighborhoodQry = new DatamartNeighborhoodService( rockContext ).Queryable();
            var datamartEraQry = new DatamartERAService( rockContext ).Queryable();
            var datamartPersonQry = new DatamartPersonService( rockContext ).Queryable();

            var joinEra = datamartEraLossQry.GroupJoin(
                datamartEraQry,
                l => l.FamilyId,
                e => e.FamilyId,
                ( l, e ) => new
                {
                    DatamartEraLoss = l,
                    DatamartEra = e.Where( x => x.WeekendDate == e.Max( xx => xx.WeekendDate ) ).FirstOrDefault()
                } );

            // do outer join in case they aren't in a neighboorhood
            var joinFamilyNeighboorHood =
                from f in datamartFamilyQry
                join n in datamartNeighborhoodQry on f.NeighborhoodId equals n.Id into fn
                from n in fn.DefaultIfEmpty()
                select new
                {
                    f.Id,
                    f.HHPersonId,
                    f.HHFullName,
                    f.AdultNames,
                    f.ChildNames,
                    n.NeighborhoodPastorId,
                    n.NeighborhoodPastorName
                };

            var joinEraFamily = joinEra.Join(
                joinFamilyNeighboorHood,
                e => e.DatamartEraLoss.FamilyId,
                f => f.Id,
                ( e, f ) => new
                {
                    Era = e,
                    Family = f
                } );

            var joinQry = joinEraFamily.Join(
                datamartPersonQry,
                ef => ef.Family.HHPersonId,
                p => p.Id,
                ( ef, p ) => new
                {
                    ef.Era.DatamartEraLoss.Id,
                    ef.Era.DatamartEraLoss.Processed,
                    ef.Era.DatamartEraLoss.SendEmail,
                    HHPerson = new
                    {
                        Id = ef.Family.HHPersonId,
                        FullName = ef.Family.HHFullName
                    },
                    ef.Family.AdultNames,
                    ef.Family.ChildNames,
                    ef.Era.DatamartEraLoss.LossDate,
                    ef.Era.DatamartEra.FirstAttended,
                    ef.Era.DatamartEra.LastAttended,
                    ef.Era.DatamartEra.LastGave,
                    ef.Era.DatamartEra.TimesGaveLastYear,
                    p.StartingPointDate,
                    p.BaptismDate,
                    NeighborhoodPastor = new
                    {
                        Id = ef.Family.NeighborhoodPastorId,
                        FullName = ef.Family.NeighborhoodPastorName
                    },
                    p.InNeighborhoodGroup
                } );

            if ( gList.SortProperty != null )
            {
                joinQry = joinQry.Sort( gList.SortProperty );
            }
            else
            {
                joinQry = joinQry.OrderByDescending( o => o.LossDate ).ThenBy( o => o.HHPerson.FullName );
            }

            gList.SetLinqDataSource( joinQry );
            gList.DataBind();
        }

        /// <summary>
        /// Handles the OnFormatDataValue event of the NeighborhoodPastor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CallbackField.CallbackEventArgs"/> instance containing the event data.</param>
        protected void NeighborhoodPastor_OnFormatDataValue( object sender, CallbackField.CallbackEventArgs e )
        {
            if ( e.DataValue != null )
            {
                string url = this.ResolveRockUrl( string.Format( "~/Person/{0}", e.DataValue.GetPropertyValue( "Id" ) ) );
                e.FormattedValue = string.Format( "<a href='{0}'>{1}</a>", url, e.DataValue.GetPropertyValue( "FullName" ) );
            }
        }

        /// <summary>
        /// Handles the OnFormatDataValue event of the HHPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CallbackField.CallbackEventArgs"/> instance containing the event data.</param>
        protected void HHPerson_OnFormatDataValue( object sender, CallbackField.CallbackEventArgs e )
        {
            if ( e.DataValue != null )
            {
                string url = this.ResolveRockUrl( string.Format( "~/Person/{0}", e.DataValue.GetPropertyValue( "Id" ) ) );
                e.FormattedValue = string.Format( "<a href='{0}'>{1}</a>", url, e.DataValue.GetPropertyValue( "FullName" ) );
            }
        }

        #endregion


    }
}