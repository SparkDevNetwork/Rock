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
using System.Web.UI;
using com.ccvonline.Residency.Data;
using com.ccvonline.Residency.Model;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_ccvonline.Residency
{
    [DisplayName( "Period Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a residency period.  For example: Fall 2013/Spring 2014" )]

    public partial class PeriodDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "PeriodId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "PeriodId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetEditMode( false );

            if ( hfPeriodId.ValueAsInt().Equals( 0 ) )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ResidencyService<Period> service = new ResidencyService<Period>( new ResidencyContext() );
                Period item = service.Get( hfPeriodId.ValueAsInt() );
                ShowReadonlyDetails( item );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ResidencyService<Period> service = new ResidencyService<Period>( new ResidencyContext() );
            Period item = service.Get( hfPeriodId.ValueAsInt() );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var residencyContext = new ResidencyContext();

            Period period;
            ResidencyService<Period> periodService = new ResidencyService<Period>( residencyContext );

            int periodId = int.Parse( hfPeriodId.Value );

            if ( periodId == 0 )
            {
                period = new Period();
                periodService.Add( period );
            }
            else
            {
                period = periodService.Get( periodId );
            }

            period.Name = tbName.Text;
            period.Description = tbDescription.Text;
            period.StartDate = dpStartEndDate.LowerValue;
            period.EndDate = dpStartEndDate.UpperValue;

            if ( !period.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            residencyContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["PeriodId"] = period.Id.ToString();
            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            // return if unexpected itemKey 
            if ( itemKey != "PeriodId" )
            {
                return;
            }

            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            Period period = null;
            if ( !itemKeyValue.Equals( 0 ) )
            {
                period = new ResidencyService<Period>( new ResidencyContext() ).Get( itemKeyValue );
            }
            else
            {
                period = new Period { Id = 0 };
            }

            hfPeriodId.Value = period.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Period.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( period );
            }
            else
            {
                btnEdit.Visible = true;
                if ( period.Id > 0 )
                {
                    ShowReadonlyDetails( period );
                }
                else
                {
                    ShowEditDetails( period );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="period">The residency period.</param>
        private void ShowEditDetails( Period period )
        {
            if ( period.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Period.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = period.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = period.Name;
            tbDescription.Text = period.Description;
            dpStartEndDate.LowerValue= period.StartDate;
            dpStartEndDate.UpperValue = period.EndDate;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="period">The residency project.</param>
        private void ShowReadonlyDetails( Period period )
        {
            lReadOnlyTitle.Text = period.Name.FormatAsHtmlTitle();
            
            SetEditMode( false );

            lblMainDetailsCol1.Text = new DescriptionList()
                .Add( "Description", period.Description ).Html;

            lblMainDetailsCol2.Text = new DescriptionList()
                .Add( "Start Date", period.StartDate )
                .Add( "End Date", period.EndDate )
                .Html;
        }

        #endregion
    }
}