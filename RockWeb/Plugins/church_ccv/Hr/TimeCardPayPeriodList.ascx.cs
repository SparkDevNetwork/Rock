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
using church.ccv.Hr.Data;
using church.ccv.Hr.Model;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Hr
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Time Card Pay Period List" )]
    [Category( "CCV > Time Card" )]
    [Description( "Lists all the time card pay periods." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve all timecards, regardless of department." )]
    [LinkedPage( "Detail Page" )]
    public partial class TimeCardPayPeriodList : Rock.Web.UI.RockBlock
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
            gList.DataKeyNames = new string[] { "Id" };

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
            BindGrid();
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

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "TimeCardPayPeriodId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, RowEventArgs e )
        {
            var hrContext = new HrContext();
            TimeCardPayPeriodService timeCardPayPeriodService = new TimeCardPayPeriodService( hrContext );
            TimeCardPayPeriod timeCardPayPeriod = timeCardPayPeriodService.Get( e.RowKeyId );
            if ( timeCardPayPeriod != null )
            {
                string errorMessage;
                if ( !timeCardPayPeriodService.CanDelete( timeCardPayPeriod, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                // check if any of the pay period' time cards have hours entered
                TimeCardService timeCardService = new TimeCardService( hrContext );
                if ( timeCardService.Queryable().Where( a => a.TimeCardPayPeriodId == timeCardPayPeriod.Id ).ToList().Any( a => a.HasHoursEntered() ) )
                {
                    mdGridWarning.Show( "Pay Period has time cards with hours entered", ModalAlertType.Information );
                    return;
                }

                timeCardPayPeriodService.Delete( timeCardPayPeriod );
                hrContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            HrContext hrContext = new HrContext();
            TimeCardPayPeriodService timeCardPayPeriodService = new TimeCardPayPeriodService( hrContext );
            TimeCardService timeCardService = new TimeCardService( hrContext );
            var payPeriodQry = timeCardPayPeriodService.Queryable().OrderByDescending( a => a.StartDate );
            
            // get non-empty timecards
            var timeCardQry = timeCardService.Queryable().WhereTimeCardsHaveHours();

            if ( !this.IsUserAuthorized( Authorization.APPROVE ) )
            {
                // unless the current user has the Global Approve role, limit cards to approvees
                var staffPersonIds = TimeCardPayPeriodService.GetApproveesForPerson( hrContext, this.CurrentPerson );
                timeCardQry = timeCardQry.Where( a => staffPersonIds.Contains( a.PersonAlias.PersonId ) );
            }

            var joinQry = payPeriodQry.GroupJoin(
                timeCardQry,
                innerKey => innerKey.Id,
                outerKey => outerKey.TimeCardPayPeriodId,
                ( PayPeriod, TimeCards ) => new
                {
                    PayPeriod,
                    TimeCards = TimeCards
                } );

            var selectQry = joinQry.Select( a => new
            {
                Id = a.PayPeriod.Id,
                PayPeriod = a.PayPeriod,
                CardsInProgressCount = a.TimeCards.Count( x => x.TimeCardStatus == TimeCardStatus.InProgress ),
                CardsSubmittedCount = a.TimeCards.Count( x => x.TimeCardStatus == TimeCardStatus.Submitted ),
                CardsApprovedCount = a.TimeCards.Count( x => x.TimeCardStatus == TimeCardStatus.Approved ),
                CardsPaidCount = a.TimeCards.Count( x => x.TimeCardStatus == TimeCardStatus.Exported ),
                CardsCount = a.TimeCards.Count(),
            } );

            gList.DataSource = selectQry.ToList();
            gList.DataBind();
        }

        #endregion
    }
}