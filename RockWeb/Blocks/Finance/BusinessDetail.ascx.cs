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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given business." )]
    public partial class BusinessDetail : Rock.Web.UI.RockBlock, IDetailBlock
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
                string itemId = PageParameter( "businessId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "businessId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Events

        protected void lbSave_Click( object sender, EventArgs e )
        {
            var businessService = new PersonService();
            var business = new Person();
            //tbBusinessName;
            //tbStreet1;
            //tbStreet2;
            //tbCity;
            //ddlState.SelectedValue;
            //tbZip;
            //tbPhone;
            //tbEmailAddress;
            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( tbPhone.Text ) ) )
            {
                var phoneNumber = new PhoneNumber();
                phoneNumber.Number = PhoneNumber.CleanNumber( tbPhone.Text );
            }
            business.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( hfBusinessId.ValueAsInt() != 0 )
            {
                var savedBusiness = new PersonService().Get( hfBusinessId.ValueAsInt() );
                ShowSummary( savedBusiness );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        protected void lbEdit_Click( object sender, EventArgs e )
        {

        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            pnlDetails.Visible = false;

            if ( !itemKey.Equals( "businessId" ) )
            {
                return;
            }

            bool editAllowed = true;

            Person business = null;     // A business is a person

            if ( !itemKeyValue.Equals( 0 ) )
            {
                business = new PersonService().Get( itemKeyValue );
                editAllowed = business.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
            else
            {
                business = new Person { Id = 0 };
            }

            if ( business == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfBusinessId.Value = business.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Person.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowSummary( business );
            }
            else
            {
                //lbEdit.Visible = true;
                if ( business.Id > 0 )
                {
                    ShowSummary( business );
                }
                else
                {
                    ShowEditDetails( business );
                }
            }
        }

        private void ShowSummary( Person business )
        {
            SetEditMode( false );
            hfBusinessId.SetValue( business.Id );
            lTitle.Text = String.Format( "Edit: {0}", business.FullName ).FormatAsHtmlTitle();


            //hfAccountId.SetValue( account.Id );
            //lActionTitle.Text = account.Name.FormatAsHtmlTitle();
            //hlInactive.Visible = !account.IsActive;
            //lAccountDescription.Text = account.Description;

            //DescriptionList descriptionList = new DescriptionList();
            //descriptionList.Add( "", "" );
            //lblMainDetails.Text = descriptionList.Html;
        }

        //private void ShowSummary( FinancialBatch financialBatch )
        //{
        //    string batchDate = string.Empty;
        //    if ( financialBatch.BatchStartDateTime != null )
        //    {
        //        batchDate = financialBatch.BatchStartDateTime.Value.ToShortDateString();
        //    }

        //    lTitle.Text = string.Format( "{0} <small>{1}</small>", financialBatch.Name.FormatAsHtmlTitle(), batchDate );

        //    SetEditMode( false );

        //    string campus = string.Empty;
        //    if ( financialBatch.CampusId.HasValue )
        //    {
        //        campus = financialBatch.Campus.ToString();
        //    }

        //    hfBatchId.SetValue( financialBatch.Id );
        //    lDetailsLeft.Text = new DescriptionList()
        //        .Add( "Title", financialBatch.Name )
        //        .Add( "Status", financialBatch.Status.ToString() )
        //        .Add( "Batch Start Date", Convert.ToDateTime( financialBatch.BatchStartDateTime ).ToString( "MM/dd/yyyy" ) )
        //        .Html;

        //    lDetailsRight.Text = new DescriptionList()
        //        .Add( "Control Amount", financialBatch.ControlAmount.ToString() )
        //        .Add( "Campus", campus )
        //        .Add( "Batch End Date", Convert.ToDateTime( financialBatch.BatchEndDateTime ).ToString( "MM/dd/yyyy" ) )
        //        .Html;
        //}

        private void ShowEditDetails( Person business )
        {
            if ( business.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( business.FullName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( "Business" ).FormatAsHtmlTitle();
            }

            SetEditMode( true );
        }

        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        #endregion
}
}