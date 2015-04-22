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
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Block for users to create, edit, and view benevolence requests.
    /// </summary>
    [DisplayName( "Benevolence Request Detail" )]
    [Category( "Finance" )]
    [Description( "Block for users to create, edit, and view benevolence requests." )]
    [GroupField( "Case Worker Group", "The group to draw case workers from", true, "26E7148C-2059-4F45-BCFE-32230A12F0DC" )]
    public partial class BenevolenceRequestDetail : Rock.Web.UI.RockBlock
    {
        #region ViewState and Dynamic Controls

        /// <summary>
        /// ViewState of BenevolenceResultInfos for BenevolenceRequest
        /// </summary>
        /// <value>
        /// The state of the BenevolenceResultInfos for BenevolenceRequest.
        /// </value>
        public List<BenevolenceResultInfo> BenevolenceResultsState
        {
            get
            {
                List<BenevolenceResultInfo> result = ViewState["BenevolenceResultInfoState"] as List<BenevolenceResultInfo>;
                if ( result == null )
                {
                    result = new List<BenevolenceResultInfo>();
                }

                return result;
            }

            set
            {
                ViewState["BenevolenceResultInfoState"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            gResults.DataKeyNames = new string[] { "TempGuid" };
            gResults.Actions.AddClick += gResults_AddClick;
            gResults.Actions.ShowAdd = true;
            gResults.IsDeleteEnabled = true;

            // Gets any existing results and places them into the ViewState
            BenevolenceRequest benevolenceRequest = null;
            int benevolenceRequestId = PageParameter( "BenevolenceRequestId" ).AsInteger();
            if ( !benevolenceRequestId.Equals( 0 ) )
            {
                benevolenceRequest = new BenevolenceRequestService( new RockContext() ).Get( benevolenceRequestId );
            }

            if ( benevolenceRequest == null )
            {
                benevolenceRequest = new BenevolenceRequest { Id = 0 };
            }

            if ( ViewState["BenevolenceResultInfoState"] == null )
            {
                List<BenevolenceResultInfo> brInfoList = new List<BenevolenceResultInfo>();
                foreach ( BenevolenceResult benevolenceResult in benevolenceRequest.BenevolenceResults )
                {
                    BenevolenceResultInfo benevolenceResultInfo = new BenevolenceResultInfo();
                    benevolenceResultInfo.ResultId = benevolenceResult.Id;
                    benevolenceResultInfo.Amount = benevolenceResult.Amount;
                    benevolenceResultInfo.TempGuid = benevolenceResult.Guid;
                    benevolenceResultInfo.ResultSummary = benevolenceResult.ResultSummary;
                    benevolenceResultInfo.ResultTypeValueId = benevolenceResult.ResultTypeValueId;
                    benevolenceResultInfo.ResultTypeName = benevolenceResult.ResultTypeValue.Value;
                    brInfoList.Add( benevolenceResultInfo );
                }

                BenevolenceResultsState = brInfoList;
            }
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
                ShowDetail( PageParameter( "BenevolenceRequestId" ).AsInteger() );
            }
            else
            {
                confirmExit.Enabled = true;
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
            ShowDetail( PageParameter( "BenevolenceRequestId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the AddClick event of the gResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gResults_AddClick( object sender, EventArgs e )
        {
            ddlResultType.Items.Clear();
            ddlResultType.AutoPostBack = false;
            ddlResultType.Required = true;
            ddlResultType.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) ), true );
            dtbResultSummary.Text = string.Empty;
            dtbAmount.Text = string.Empty;

            mdAddResult.Show();
        }

        /// <summary>
        /// Handles the RowSelected event of the gResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gResults_RowSelected( object sender, RowEventArgs e )
        {
            Guid? infoGuid = e.RowKeyValue as Guid?;
            List<BenevolenceResultInfo> resultList = BenevolenceResultsState;
            var resultInfo = resultList.FirstOrDefault( r => r.TempGuid == infoGuid );
            if ( resultInfo != null )
            {
                ddlResultType.Items.Clear();
                ddlResultType.AutoPostBack = false;
                ddlResultType.Required = true;
                ddlResultType.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) ), true );
                ddlResultType.SelectedValue = resultInfo.ResultTypeValueId.ToString();
                dtbResultSummary.Text = resultInfo.ResultSummary;
                dtbAmount.Text = resultInfo.Amount.ToString();
                hfInfoGuid.Value = e.RowKeyValue.ToString();
                mdAddResult.Show();
            }
        }

        /// <summary>
        /// Handles the DeleteClick event of the gResult control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gResults_DeleteClick( object sender, RowEventArgs e )
        {
            Guid? infoGuid = e.RowKeyValue as Guid?;
            List<BenevolenceResultInfo> resultList = BenevolenceResultsState;
            var resultInfo = resultList.FirstOrDefault( r => r.TempGuid == infoGuid );
            if ( resultInfo != null )
            {
                resultList.Remove( resultInfo );
            }

            BenevolenceResultsState = resultList;
            BindGridFromViewState();
        }

        /// <summary>
        /// Handles the AddClick event of the mdAddResult control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnAddResults_Click( object sender, EventArgs e )
        {
            int? resultType = ddlResultType.SelectedItem.Value.AsIntegerOrNull();
            List<BenevolenceResultInfo> benevolenceResultInfoViewStateList = BenevolenceResultsState;
            Guid? infoGuid = hfInfoGuid.Value.AsGuidOrNull();

            if ( infoGuid != null )
            {
                var resultInfo = benevolenceResultInfoViewStateList.FirstOrDefault( r => r.TempGuid == infoGuid );
                if ( resultInfo != null )
                {
                    resultInfo.Amount = dtbAmount.Text.AsDecimalOrNull();
                    resultInfo.ResultSummary = dtbResultSummary.Text;
                    if ( resultType != null )
                    {
                        resultInfo.ResultTypeValueId = resultType.Value;
                    }

                    resultInfo.ResultTypeName = ddlResultType.SelectedItem.Text;
                }
            }
            else
            {
                BenevolenceResultInfo benevolenceResultInfo = new BenevolenceResultInfo();

                benevolenceResultInfo.Amount = dtbAmount.Text.AsDecimalOrNull();

                benevolenceResultInfo.ResultSummary = dtbResultSummary.Text;
                if ( resultType != null )
                {
                    benevolenceResultInfo.ResultTypeValueId = resultType.Value;
                }

                benevolenceResultInfo.ResultTypeName = ddlResultType.SelectedItem.Text;
                benevolenceResultInfo.TempGuid = Guid.NewGuid();
                benevolenceResultInfoViewStateList.Add( benevolenceResultInfo );
            }

            BenevolenceResultsState = benevolenceResultInfoViewStateList;

            mdAddResult.Hide();
            pnlView.Visible = true;
            BindGridFromViewState();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                RockContext rockContext = new RockContext();
                BenevolenceRequestService benevolenceRequestService = new BenevolenceRequestService( rockContext );
                BenevolenceResultService benevolenceResultService = new BenevolenceResultService( rockContext );

                BenevolenceRequest benevolenceRequest = null;
                int benevolenceRequestId = PageParameter( "BenevolenceRequestId" ).AsInteger();

                if ( !benevolenceRequestId.Equals( 0 ) )
                {
                    benevolenceRequest = benevolenceRequestService.Get( benevolenceRequestId );
                }

                if ( benevolenceRequest == null )
                {
                    benevolenceRequest = new BenevolenceRequest { Id = 0 };
                }

                benevolenceRequest.FirstName = dtbFirstName.Text;
                benevolenceRequest.LastName = dtbLastName.Text;
                benevolenceRequest.Email = ebEmail.Text;
                benevolenceRequest.RequestText = dtbRequestText.Text;
                benevolenceRequest.ResultSummary = dtbSummary.Text;
                benevolenceRequest.GovernmentId = dtbGovernmentId.Text;

                if ( lapAddress.Location != null )
                {
                    benevolenceRequest.LocationId = lapAddress.Location.Id;
                }

                benevolenceRequest.RequestedByPersonAliasId = ppPerson.PersonAliasId;
                benevolenceRequest.CaseWorkerPersonAliasId = ddlCaseWorker.SelectedItem.Value.AsIntegerOrNull();
                benevolenceRequest.RequestStatusValueId = ddlRequestStatus.SelectedItem.Value.AsIntegerOrNull();
                benevolenceRequest.ConnectionStatusValueId = ddlConnectionStatus.SelectedItem.Value.AsIntegerOrNull();

                if ( dpRequestDate.SelectedDate.HasValue )
                {
                    benevolenceRequest.RequestDateTime = dpRequestDate.SelectedDate.Value;
                }

                benevolenceRequest.HomePhoneNumber = pnbHomePhone.Number;
                benevolenceRequest.CellPhoneNumber = pnbCellPhone.Number;
                benevolenceRequest.WorkPhoneNumber = pnbWorkPhone.Number;

                List<BenevolenceResultInfo> resultList = BenevolenceResultsState;
                BenevolenceResult benevolenceResult = null;

                foreach ( BenevolenceResult result in benevolenceRequest.BenevolenceResults.ToList() )
                {
                    if ( resultList.FirstOrDefault( r => r.ResultId == result.Id ) == null )
                    {
                        benevolenceRequest.BenevolenceResults.Remove( result );
                        benevolenceResultService.Delete( result );
                    }
                }

                foreach ( BenevolenceResultInfo benevolenceResultInfo in resultList )
                {
                    if ( benevolenceResultInfo.ResultId == null )
                    {
                        benevolenceResult = new BenevolenceResult();
                        benevolenceResult.Amount = benevolenceResultInfo.Amount;
                        benevolenceResult.ResultSummary = benevolenceResultInfo.ResultSummary;
                        benevolenceResult.ResultTypeValueId = benevolenceResultInfo.ResultTypeValueId;
                        benevolenceResult.BenevolenceRequestId = benevolenceRequest.Id;
                        benevolenceRequest.BenevolenceResults.Add( benevolenceResult );
                    }
                }

                if ( benevolenceRequest.IsValid )
                {
                    if ( benevolenceRequest.Id.Equals( 0 ) )
                    {
                        benevolenceRequestService.Add( benevolenceRequest );
                    }

                    rockContext.SaveChanges();

                    var personId = this.PageParameter( "PersonId" ).AsIntegerOrNull();
                    var qryParams = new Dictionary<string, string>();
                    if ( personId.HasValue )
                    {
                        qryParams.Add( "PersonId", personId.ToString() );
                    }

                    NavigateToParentPage( qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            var personId = this.PageParameter( "PersonId" ).AsIntegerOrNull();
            var qryParams = new Dictionary<string, string>();
            if ( personId.HasValue )
            {
                qryParams.Add( "PersonId", personId.ToString() );
            }

            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppPerson.PersonId != null )
            {
                Person person = new PersonService( new RockContext() ).Get( ppPerson.PersonId.Value );
                if ( person != null )
                {
                    dtbFirstName.Text = person.FirstName;
                    dtbFirstName.Enabled = false;

                    dtbLastName.Text = person.LastName;
                    dtbLastName.Enabled = false;

                    ddlConnectionStatus.SelectedValue = person.ConnectionStatusValueId.ToString();
                    ddlConnectionStatus.Enabled = false;

                    var homePhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                    if ( homePhoneType != null )
                    {
                        var homePhone = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == homePhoneType.Id );
                        if ( homePhone != null )
                        {
                            pnbHomePhone.Text = homePhone.NumberFormatted;
                            pnbHomePhone.Enabled = false;
                        }
                    }

                    var mobilePhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    if ( mobilePhoneType != null )
                    {
                        var mobileNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneType.Id );
                        if ( mobileNumber != null )
                        {
                            pnbCellPhone.Text = mobileNumber.NumberFormatted;
                            pnbCellPhone.Enabled = false;
                        }
                    }

                    var workPhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                    if ( workPhoneType != null )
                    {
                        var workPhone = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == workPhoneType.Id );
                        if ( workPhone != null )
                        {
                            pnbWorkPhone.Text = workPhone.NumberFormatted;
                            pnbWorkPhone.Enabled = false;
                        }
                    }

                    ebEmail.Text = person.Email;
                    ebEmail.Enabled = false;

                    lapAddress.SetValue( person.GetHomeLocation() );
                    lapAddress.Enabled = false;
                }
            }
            else
            {
                dtbFirstName.Enabled = true;
                dtbLastName.Enabled = true;
                ddlConnectionStatus.Enabled = true;
                pnbHomePhone.Enabled = true;
                pnbCellPhone.Enabled = true;
                pnbWorkPhone.Enabled = true;
                ebEmail.Enabled = true;
                lapAddress.Enabled = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="benevolenceRequestId">The benevolence request identifier</param>
        public void ShowDetail( int benevolenceRequestId )
        {
            BenevolenceRequest benevolenceRequest = null;
            var rockContext = new RockContext();
            BenevolenceRequestService benevolenceRequestService = new BenevolenceRequestService( rockContext );
            if ( !benevolenceRequestId.Equals( 0 ) )
            {
                benevolenceRequest = benevolenceRequestService.Get( benevolenceRequestId );
            }

            if ( benevolenceRequest == null )
            {
                benevolenceRequest = new BenevolenceRequest { Id = 0 };
                benevolenceRequest.RequestDateTime = RockDateTime.Now;
                var personId = this.PageParameter( "PersonId" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var person = new PersonService( rockContext ).Get( personId.Value );
                    if ( person != null )
                    {
                        benevolenceRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
                        benevolenceRequest.RequestedByPersonAlias = person.PrimaryAlias;
                    }
                }
            }

            dtbFirstName.Text = benevolenceRequest.FirstName;
            dtbLastName.Text = benevolenceRequest.LastName;
            dtbGovernmentId.Text = benevolenceRequest.GovernmentId;
            ebEmail.Text = benevolenceRequest.Email;
            dtbRequestText.Text = benevolenceRequest.RequestText;
            dtbSummary.Text = benevolenceRequest.ResultSummary;
            dpRequestDate.SelectedDate = benevolenceRequest.RequestDateTime;

            if ( benevolenceRequest.RequestedByPersonAlias != null )
            {
                ppPerson.SetValue( benevolenceRequest.RequestedByPersonAlias.Person );
                ppPerson_SelectPerson( null, null );
            }
            else
            {
                ppPerson.SetValue( null );
            }

            if ( benevolenceRequest.HomePhoneNumber != null )
            {
                pnbHomePhone.Text = benevolenceRequest.HomePhoneNumber;
            }

            if ( benevolenceRequest.CellPhoneNumber != null )
            {
                pnbCellPhone.Text = benevolenceRequest.CellPhoneNumber;
            }

            if ( benevolenceRequest.WorkPhoneNumber != null )
            {
                pnbWorkPhone.Text = benevolenceRequest.WorkPhoneNumber;
            }

            lapAddress.SetValue( benevolenceRequest.Location );

            LoadDropDowns( benevolenceRequest );

            if ( benevolenceRequest.RequestStatusValueId != null )
            {
                ddlRequestStatus.SelectedValue = benevolenceRequest.RequestStatusValueId.ToString();

                if ( benevolenceRequest.RequestStatusValue.Value == "Approved" )
                {
                    hlStatus.Text = "Approved";
                    hlStatus.LabelType = LabelType.Success;
                }

                if ( benevolenceRequest.RequestStatusValue.Value == "Denied" )
                {
                    hlStatus.Text = "Denied";
                    hlStatus.LabelType = LabelType.Danger;
                }
            }

            if ( benevolenceRequest.ConnectionStatusValueId != null )
            {
                ddlConnectionStatus.SelectedValue = benevolenceRequest.ConnectionStatusValueId.ToString();
            }

            ddlCaseWorker.SelectedValue = benevolenceRequest.CaseWorkerPersonAliasId.ToString();

            BindGridFromViewState();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGridFromViewState()
        {
            List<BenevolenceResultInfo> benevolenceResultInfoViewStateList = BenevolenceResultsState;
            gResults.DataSource = benevolenceResultInfoViewStateList;
            gResults.DataBind();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( BenevolenceRequest benevolenceRequest )
        {
            ddlRequestStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS ) ), false );
            ddlConnectionStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ), true );
            Guid groupGuid = GetAttributeValue( "CaseWorkerGroup" ).AsGuid();
            var listData = new GroupMemberService( new RockContext() ).Queryable( "Person, Group" )
                .Where( gm => gm.Group.Guid == groupGuid )
                .Select( gm => gm.Person )
                .ToList();
            ddlCaseWorker.DataSource = listData;
            ddlCaseWorker.DataTextField = "FullName";
            ddlCaseWorker.DataValueField = "PrimaryAliasId";
            ddlCaseWorker.DataBind();
            ddlCaseWorker.Items.Insert( 0, new ListItem() );
        }

        #endregion

        #region BenevolenceResultInfo

        /// <summary>
        /// The class used to store BenevolenceResult info.
        /// </summary>
        [Serializable]
        public class BenevolenceResultInfo
        {
            [DataMember]
            public int? ResultId { get; set; }

            [DataMember]
            public int ResultTypeValueId { get; set; }

            [DataMember]
            public string ResultTypeName { get; set; }

            [DataMember]
            public decimal? Amount { get; set; }

            [DataMember]
            public Guid TempGuid { get; set; }

            [DataMember]
            public string ResultSummary { get; set; }
        }

        #endregion
    }
}