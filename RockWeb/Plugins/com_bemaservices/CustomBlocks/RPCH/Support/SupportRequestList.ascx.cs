using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI.WebControls;
using RestSharp;
using RestSharp.Deserializers;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.bemaservices.Support.Utility;

namespace RockWeb.Plugins.com_bemaservices.Support
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Support Request List" )]
    [Category( "BEMA Services > Support" )]
    [Description( "Displays list of current Support Requests" )]
    [SecurityRoleField( "Admin Security Role", "Security role that is required to view ALL Support Requests", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS )]
    public partial class SupportRequestList : RockBlock
    {
        private const string APIURLAttribute = "BEMASupportAPIURL";
        private const string APIKeyAttribute = "BEMASupportAPIKey";
        private string BEMASupportAPIURL;
        private string BEMASupportAPIKey;
        private bool isAdmin = false;
        private RestClient restClient;
        private APIResponse apiResponse;
        private int mode;
        private int currentSupportRequestId;


        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSupportRequests.ApplyFilterClick += gfSupportRequests_ApplyFilterClick;
            gSupportRequests.RowDataBound += gSupportRequests_RowDataBound;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            lbAllSupportRequest.Visible = false;
            isAdmin = false;

            // Checking if user is an Admin
            if ( GetAttributeValue( "AdminSecurityRole" ).AsGuidOrNull() != null )
            {
                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );

                var groupObject = groupService.Get( GetAttributeValue( "AdminSecurityRole" ).AsGuid() );
                if ( groupObject != null )
                {
                    if ( groupObject.Members.Where( x => x.PersonId == CurrentPersonId ).Any() )
                    {
                        lbAllSupportRequest.Visible = true;
                        isAdmin = true;
                    }
                }
            }

            // Getting Info from BEMA
            if ( GetBEMASupportInfo() )
            {
                GetSupportRequests();
            }
            else
            {
                upnlHeader.Visible = false;
                return;
            }


            if ( !IsPostBack )
            {
                // Finding which panel needs to be displayed
                if ( Request.QueryString["SupportRequestId"] == null )
                {
                    DisplaySupportRequestsList();
                }
                else
                {
                    DisplaySupportRequestDetails();
                }

                // Loading Default Values
                ddlUrgency.Items.Add( "Normal" );
                ddlUrgency.Items.Add( "Emergency" );
            }

            // Setting Default ViewState
            if ( ViewState["Mode"] == null )
            {
                ViewState["Mode"] = Modes.MY;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            var temp = ViewState["Mode"];

            mode = ( int ) ViewState["Mode"];
        }

        /// <summary>
        /// Saves any server control view-state changes that have occurred since the time the page was posted back to the server.
        /// </summary>
        /// <returns>
        /// Returns the server control's current view state. If there is no view state associated with the control, this method returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            if ( ( int ) mode > -1 )
            {
                ViewState["Mode"] = mode;
            }

            return base.SaveViewState();
        }

        /// <summary>
        /// Gets configuration information and creates REST Client with correct base adddress
        /// </summary>
        /// <returns>
        /// Returns true if Global Attributes exist. Returns False if they do not exist
        /// </returns>
        private bool GetBEMASupportInfo()
        {
            var urlAttributeValue = GlobalAttributesCache.Value( APIURLAttribute );
            var keyAttributeValue = GlobalAttributesCache.Value( APIKeyAttribute );

            if ( urlAttributeValue.Any() && keyAttributeValue.Any() )
            {
                // Setting global values
                BEMASupportAPIURL = urlAttributeValue;
                BEMASupportAPIKey = keyAttributeValue;

                // Creating universal rest client
                restClient = new RestClient( BEMASupportAPIURL );

                return true;
            }
            else
            {
                upnlFirstTimeSetup.Visible = true;
                return false;
            }
        }

        /// <summary>
        /// Gets Support Request from BEMA using API Key
        /// </summary>
        private void GetSupportRequests()
        {
            var request = new RestRequest( "Webhooks/Lava.ashx/support-request" );
            request.AddHeader( "Authorization-Token", BEMASupportAPIKey );
            var response = restClient.Execute( request );

            if ( response != null && response.StatusCode == HttpStatusCode.OK )
            {
                if ( response.Content == "" )
                {
                    lAlerts.Text = @"<div class='alert alert-danger'>An unexpected error has occured. Please contact BEMA Support.</div>";
                    return;
                }

                try
                {
                    apiResponse = new JsonDeserializer().Deserialize<APIResponse>( response );
                }
                catch ( Exception ex )
                {
                    // Creating Exception is Exception Log
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );

                    lAlerts.Text = @"<div class='alert alert-danger'>An unexpected error has occured. Please check your exception log for more detail.</div>";
                    return;
                }

                if ( !apiResponse.Success )
                {
                    lAlerts.Text = @"<div class='alert alert-danger'>An unexpected error has occured. Please validate your API Keys and try again.</div>";
                    return;
                }
            }
        }

        /// <summary>
        /// Renders the correct view depending on mode.
        /// </summary>
        private void DisplaySupportRequestsList()
        {
            IEnumerable<SupportRequest> supportRequestsUnfiltered;
            List<SupportRequest> supportRequestsList;

            switch ( mode )
            {
                case ( int ) Modes.MY:
                    supportRequestsUnfiltered = apiResponse.SupportRequests.Where( x => x.SubmitterPersonAliasGuid == CurrentPerson.PrimaryAlias.Guid.ToString() && x.Status != "Completed" );
                    supportRequestsList = FilterGrid( supportRequestsUnfiltered );
                    gSupportRequests.DataSource = supportRequestsList;
                    gSupportRequests.DataBind();

                    pnlSupportRequests.Visible = true;
                    lbMySupportRequestsParent.AddCssClass( "active" );
                    lbMyCompletedSupportRequestParent.RemoveCssClass( "active" );
                    lbAllSupportRequestParent.RemoveCssClass( "active" );
                    upnlContent.Visible = true;
                    break;
                case ( int ) Modes.COMPLETED:
                    supportRequestsUnfiltered = apiResponse.SupportRequests.Where( x => x.SubmitterPersonAliasGuid == CurrentPerson.PrimaryAlias.Guid.ToString() && x.Status == "Completed" );
                    supportRequestsList = FilterGrid( supportRequestsUnfiltered );
                    gSupportRequests.DataSource = supportRequestsList;
                    gSupportRequests.DataBind();

                    pnlSupportRequests.Visible = true;
                    lbMySupportRequestsParent.RemoveCssClass( "active" );
                    lbMyCompletedSupportRequestParent.AddCssClass( "active" );
                    lbAllSupportRequestParent.RemoveCssClass( "active" );
                    upnlContent.Visible = true;
                    break;
                case ( int ) Modes.ALL:
                    if ( isAdmin )
                    {
                        // If user is admin, displaying All Support Requests
                        supportRequestsUnfiltered = apiResponse.SupportRequests;
                        supportRequestsList = FilterGrid( supportRequestsUnfiltered );
                        gSupportRequests.DataSource = supportRequestsList;
                        gSupportRequests.DataBind();

                        pnlSupportRequests.Visible = true;
                        lbMySupportRequestsParent.RemoveCssClass( "active" );
                        lbMyCompletedSupportRequestParent.RemoveCssClass( "active" );
                        lbAllSupportRequestParent.AddCssClass( "active" );
                        upnlContent.Visible = true;
                    }
                    else
                    {
                        // If user is NOT admin, displaying My Support Requests
                        supportRequestsUnfiltered = apiResponse.SupportRequests.Where( x => x.SubmitterPersonAliasGuid == CurrentPerson.PrimaryAlias.Guid.ToString() && x.Status != "Completed" );
                        supportRequestsList = FilterGrid( supportRequestsUnfiltered );
                        gSupportRequests.DataSource = supportRequestsList;
                        gSupportRequests.DataBind();

                        pnlSupportRequests.Visible = true;
                        lbMySupportRequestsParent.AddCssClass( "active" );
                        lbMyCompletedSupportRequestParent.RemoveCssClass( "active" );
                        lbAllSupportRequestParent.RemoveCssClass( "active" );
                        upnlContent.Visible = true;
                    }
                    break;

                default:
                    supportRequestsUnfiltered = apiResponse.SupportRequests.Where( x => x.SubmitterPersonAliasGuid == CurrentPerson.PrimaryAlias.Guid.ToString() && x.Status != "Completed" );
                    supportRequestsList = FilterGrid( supportRequestsUnfiltered );
                    gSupportRequests.DataSource = supportRequestsList;
                    gSupportRequests.DataBind();

                    pnlSupportRequests.Visible = true;
                    lbMySupportRequestsParent.AddCssClass( "active" );
                    lbMyCompletedSupportRequestParent.RemoveCssClass( "active" );
                    lbAllSupportRequestParent.RemoveCssClass( "active" );
                    upnlContent.Visible = true;
                    break;
            }
        }

        /// <summary>
        /// Displays Support Request details
        /// </summary>
        private void DisplaySupportRequestDetails()
        {
            var supportRequestId = Request.QueryString["SupportRequestId"].AsIntegerOrNull();
            if ( supportRequestId != null )
            {
                SupportRequest supportRequestDetails = GetSupportRequestDetails( supportRequestId.Value );

                // Validating request was found
                if ( supportRequestDetails != null )
                {
                    detailStatus.Text = supportRequestDetails.Status;
                    if ( supportRequestDetails.Status == "Active" )
                    {
                        detailStatus.LabelType = LabelType.Info;
                    }
                    else
                    {
                        detailStatus.LabelType = LabelType.Default;
                    }
                    detailCreated.InnerText = supportRequestDetails.Created.ToShortDateTimeString();
                    detailDescription.InnerText = Server.HtmlDecode( supportRequestDetails.SupportRequestDescription );
                    detailUrgency.Text = supportRequestDetails.Urgency;
                    if ( supportRequestDetails.Urgency == "Normal" )
                    {
                        detailUrgency.LabelType = LabelType.Default;
                    }
                    else
                    {
                        detailUrgency.LabelType = LabelType.Danger;
                    }
                    detailEmail.InnerText = Server.HtmlDecode( supportRequestDetails.Email );
                    detailRockVersion.InnerText = supportRequestDetails.RockVersion;
                    detailSubject.InnerText = Server.HtmlDecode( supportRequestDetails.Subject );
                    detailSubmitter.InnerText = Server.HtmlDecode( supportRequestDetails.Submitter );
                    currentSupportRequestId = supportRequestDetails.Id;

                    rSupportRequestNotes.DataSource = supportRequestDetails.Notes.ToList();
                    rSupportRequestNotes.DataBind();

                    upnlDetails.Visible = true;
                }
            }
            else
            {
                lErrors.Text = string.Format( "<div class='alert alert-warning'>Support Request <strong>{0}</strong> not found.</div>", supportRequestId );
                upnlDetails.Visible = false;
                upnlError.Visible = true;
            }

        }

        /// <summary>
        /// Gets the Support Request information for a particular Support Request
        /// </summary>
        /// <param name="supportRequestId">The Support Request Id we are going to pull information for</param>
        /// <returns>
        /// SupportRequest object containing Support Request details
        /// </returns>
        private SupportRequest GetSupportRequestDetails( int supportRequestId )
        {
            SupportRequest output = null;

            var supportRequestDetails = apiResponse.SupportRequests.Where( x => x.Id == supportRequestId );
            if ( supportRequestDetails.Any() )
            {
                if ( supportRequestDetails.FirstOrDefault().SubmitterPersonAliasGuid == CurrentPerson.PrimaryAlias.Guid.ToString() || isAdmin )
                {
                    output = supportRequestDetails.FirstOrDefault();
                }
            }

            return output;
        }

        /// <summary>
        /// Filters Support Request based on selected filters
        /// </summary>
        /// <param name="supportRequests">The collection of Support Request we are going to filter</param>
        /// <returns>
        /// List of filtered Support Request
        /// </returns>
        private List<SupportRequest> FilterGrid( IEnumerable<SupportRequest> supportRequests )
        {
            IEnumerable<SupportRequest> filteredSupportRequests = supportRequests.OrderByDescending( x => x.Created );

            // Loading grid filters
            if ( !string.IsNullOrEmpty( gfSupportRequests.GetUserPreference( "rtbFilterSubject" ) ) )
            {
                rtbFilterSubject.Text = gfSupportRequests.GetUserPreference( "rtbFilterSubject" );
            }
            if ( !string.IsNullOrEmpty( gfSupportRequests.GetUserPreference( "cblFilterUrgency" ) ) )
            {
                var urgencyValueList = gfSupportRequests.GetUserPreference( "cblFilterUrgency" ).Split( ',' ).ToList();
                foreach ( var selectedValue in urgencyValueList )
                {
                    for ( var i = 0; i < cblFilterUrgency.Items.Count; i++ )
                    {
                        if ( cblFilterUrgency.Items[i].Text == selectedValue )
                        {
                            cblFilterUrgency.Items[i].Selected = true;
                        }

                    }
                }
            }
            if ( !string.IsNullOrEmpty( gfSupportRequests.GetUserPreference( "rtbFilterSubmitter" ) ) )
            {
                rtbFilterSubmitter.Text = gfSupportRequests.GetUserPreference( "rtbFilterSubmitter" );
            }


            // Filtering Subject
            if ( !string.IsNullOrEmpty( rtbFilterSubject.Text ) )
            {
                filteredSupportRequests = supportRequests.Where( x => x.Subject.ToLower().Contains( rtbFilterSubject.Text.ToLower() ) );
            }

            // Filtering Urgency
            List<string> urgencyValues = cblFilterUrgency.SelectedValues;
            if ( urgencyValues.Count == 1 )
            {
                if ( urgencyValues[0] == "Normal" || urgencyValues[0] == "Emergency" )
                {
                    filteredSupportRequests = filteredSupportRequests.Where( x => x.Urgency == urgencyValues[0] );
                }
            }

            // Filtering Submitter
            if ( !string.IsNullOrEmpty( rtbFilterSubmitter.Text ) )
            {
                filteredSupportRequests = supportRequests.Where( x => x.Submitter.ToLower().Contains( rtbFilterSubmitter.Text.ToLower() ) );
            }

            return filteredSupportRequests.ToList();
        }

        /// <summary>
        /// Pushes update to Support Request to BEMA
        /// </summary>
        private void PushSupportRequestUpdate()
        {
            var supportRequestId = Request.QueryString["SupportRequestId"].AsIntegerOrNull();
            if ( supportRequestId != null )
            {
                SupportRequest supportRequestDetails = GetSupportRequestDetails( supportRequestId.Value );

                if ( supportRequestDetails != null )
                {
                    var url = string.Format( "Webhooks/Lava.ashx/support-request/{0}", supportRequestDetails.Id );
                    var request = new RestRequest( url );
                    request.Method = Method.POST;
                    request.RequestFormat = DataFormat.Json;
                    request.AddBody( new { Update = rtbUpdate.Text } );
                    request.AddHeader( "Authorization-Token", BEMASupportAPIKey );
                    var response = restClient.Execute( request );

                    if ( response != null && response.StatusCode == HttpStatusCode.OK )
                    {
                        if ( !string.IsNullOrEmpty( response.Content ) )
                        {
                            UpdateResponse updateResponse;
                            try
                            {
                                updateResponse = new JsonDeserializer().Deserialize<UpdateResponse>( response );
                            }
                            catch ( Exception ex )
                            {
                                // Creating Exception is Exception Log
                                HttpContext context2 = HttpContext.Current;
                                ExceptionLogService.LogException( ex, context2 );

                                lAlerts.Text = @"<div class='alert alert-danger'>An unexpected error has occured. Please check your exception log for more detail.</div>";
                                return;
                            }

                            if ( updateResponse.Success )
                            {
                                dNewNote.Visible = false;

                                // Fetching new copy of data (including new Update)
                                DisplaySupportRequestDetails();
                            }
                        }
                    }
                }
            }

            Response.Redirect( Request.RawUrl );
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Shows My Support Request List
        /// </summary>
        protected void lbMySupportRequests_Click( object sender, EventArgs e )
        {
            mode = ( int ) Modes.MY;

            DisplaySupportRequestsList();
            lGridTitle.Text = "My Support Requests";
        }

        /// <summary>
        /// Shows My Completed Support Request List
        /// </summary>
        protected void lbMyCompletedSupportRequest_Click( object sender, EventArgs e )
        {
            mode = ( int ) Modes.COMPLETED;

            DisplaySupportRequestsList();
            lGridTitle.Text = "Completed Support Requests";
        }

        /// <summary>
        /// Shows Completed Support Request List
        /// </summary>
        protected void lbAllSupportRequest_Click( object sender, EventArgs e )
        {
            mode = ( int ) Modes.ALL;

            DisplaySupportRequestsList();
            lGridTitle.Text = "Support Requests";
        }

        /// <summary>
        /// Handles clicking on Support Request
        /// </summary>
        protected void gSupportRequests_RowSelected( object sender, RowEventArgs e )
        {
            var supportRequestId = e.RowKeyId;

            // Building new query string
            var queryString = HttpUtility.ParseQueryString( String.Empty );
            queryString.Set( "SupportRequestId", supportRequestId.ToString() );
            Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
        }

        /// <summary>
        /// Handles applying filters to Support Request List
        /// </summary>
        protected void gfSupportRequests_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSupportRequests.SaveUserPreference( "rtbFilterSubject", "Subject", rtbFilterSubject.Text );
            gfSupportRequests.SaveUserPreference( "cblFilterUrgency", "Urgency", cblFilterUrgency.SelectedValues.AsDelimited( "," ) );
            gfSupportRequests.SaveUserPreference( "rtbFilterSubmitter", "Subbmitter", rtbFilterSubmitter.Text );

            if ( GetBEMASupportInfo() )
            {
                GetSupportRequests();
                DisplaySupportRequestsList();
            }
        }

        /// <summary>
        /// Handles clicking cancel button during update process
        /// </summary>
        protected void btnCancelUpdate_Click( object sender, EventArgs e )
        {
            rtbUpdate.Text = null;
            dNewNote.Visible = !dNewNote.Visible;
        }

        /// <summary>
        /// Handles clicking Save button during update process
        /// </summary>
        protected void btnSaveUpdate_Click( object sender, EventArgs e )
        {
            PushSupportRequestUpdate();
        }

        /// <summary>
        /// Handles clicking Create Support Request
        /// </summary>
        protected void btnCreateSupportRequest_Click( object sender, EventArgs e )
        {
            upnlDetails.Visible = false;
            upnlContent.Visible = false;
            upnlAdd.Visible = true;
            btnCreateSupportRequest.Visible = false;
            lAlerts.Text = "";
        }

        /// <summary>
        /// Handles clicking Add Note Button in existing Support Request
        /// </summary>
        protected void btnAddNote_Click( object sender, EventArgs e )
        {
            dNewNote.Visible = true;
        }

        /// <summary>
        /// Handles clicking Submit button during new Support Request process
        /// </summary>
        protected void btnSubmitNew_Click( object sender, EventArgs e )
        {
            if ( rtbSubject.Text != "" && rtbDescription.Text != "" && ddlUrgency.SelectedIndex != -1 )
            {
                var newSupportRequest = new NewSupportRequest();
                newSupportRequest.Subject = rtbSubject.Text;
                newSupportRequest.SupportRequestDescription = rtbDescription.Text;
                newSupportRequest.Urgency = ddlUrgency.Items[ddlUrgency.SelectedIndex].Text;
                newSupportRequest.Submitter = CurrentPerson.FullName;
                newSupportRequest.SubmitterEmail = CurrentPerson.Email;
                newSupportRequest.RockVersion = Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber();
                newSupportRequest.SubmitterPersonAliasGuid = CurrentPerson.PrimaryAlias.Guid.ToString();
                newSupportRequest.SupportPluginVersion = com.bemaservices.Support.Utility.Version.SUPPORT_CURRENT_VERSION;

                var url = string.Format( "Webhooks/Lava.ashx/support-request" );
                var request = new RestRequest( url );
                request.Method = Method.POST;
                request.RequestFormat = DataFormat.Json;
                request.AddBody( newSupportRequest );
                request.AddHeader( "Authorization-Token", BEMASupportAPIKey );
                var response = restClient.Execute( request );
                CreateResponse responseObject;

                if ( response != null && response.StatusCode == HttpStatusCode.OK )
                {
                    if ( response.Content == "" )
                    {
                        lAlerts.Text = @"<div class='alert alert-danger'>An unexpected error has occured. Please contact BEMA Support.</div>";
                        return;
                    }

                    try
                    {
                        responseObject = new JsonDeserializer().Deserialize<CreateResponse>( response );
                    }
                    catch ( Exception ex )
                    {
                        // Creating Exception is Exception Log
                        HttpContext context2 = HttpContext.Current;
                        ExceptionLogService.LogException( ex, context2 );

                        lAlerts.Text = @"<div class='alert alert-danger'>An unexpected error has occured. Please check your exception log for more detail.</div>";
                        return;
                    }

                    // Checking if creation was successful
                    if ( responseObject.Success )
                    {
                        lAlerts.Text = @"<div class='alert alert-success'>Your Support Request has been submitted.  Someone from the BEMA Support Team will reach out to soon.</div>";
                    }
                    else
                    {
                        lAlerts.Text = @"<div class='alert alert-danger'>An unexpected error has occured while creating your Support Request. Please contact BEMA Support.</div>";
                    }
                }

                rtbSubject.Text = "";
                rtbDescription.Text = "";
                ddlUrgency.ClearSelection();

                upnlContent.Visible = true;
                upnlAdd.Visible = false;
                btnCreateSupportRequest.Visible = true;

                // Reloading Support Requests
                GetSupportRequests();
                DisplaySupportRequestsList();
            }
            else
            {
                lAlerts.Text = @"<div class='alert alert-info'>You must provide a <strong>Subject</strong>, <strong>Description</strong>, and <strong>Urgency</strong> to submit a Support Request</div>";
            }
        }

        /// <summary>
        /// Handles clicking cancel button during new Support Request process
        /// </summary>
        protected void btnCancelNew_Click( object sender, EventArgs e )
        {
            rtbSubject.Text = "";
            rtbDescription.Text = "";
            ddlUrgency.ClearSelection();

            upnlDetails.Visible = false;
            upnlContent.Visible = true;
            upnlAdd.Visible = false;
            btnCreateSupportRequest.Visible = true;
        }

        /// <summary>
        /// Handles clicking the register button during the signup process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbRegister_Click( object sender, EventArgs e )
        {
            NewSignupRequest signup = new NewSignupRequest();
            signup.FirstName = tbFirstName.Text;
            signup.LastName = tbLastName.Text;
            signup.Email = ebEmail.Text;
            signup.JobTitle = tbJobTitle.Text;
            signup.Organization = tbOrganization.Text;
            signup.ContactNumber = pnContactNumber.Text;
            signup.Address = acAddress.Street1 + " " + acAddress.Street2 + " " + acAddress.City + ", " + acAddress.State + ' ' + acAddress.PostalCode;

            BEMASupportAPIURL = GlobalAttributesCache.Value( APIURLAttribute );
            restClient = new RestClient( BEMASupportAPIURL );
            var url = string.Format( "Webhooks/Lava.ashx/signup-request" );
            var request = new RestRequest( url );
            request.Method = Method.POST;
            request.RequestFormat = DataFormat.Json;
            request.AddBody( signup );
            request.AddHeader( "Authorization-Token", BEMASupportAPIKey );
            var response = restClient.Execute( request );
            CreateResponse responseObject;

            pnlSignupDetails.Visible = false;

            if ( response != null && response.StatusCode == HttpStatusCode.OK )
            {
                if ( response.Content == "" )
                {
                    lSignupAlerts.Text = @"<div class='alert alert-danger'>Signup request could not be submitted.</div>";
                    return;
                }

                try
                {
                    responseObject = new JsonDeserializer().Deserialize<CreateResponse>( response );
                }
                catch ( Exception ex )
                {
                    // Creating Exception is Exception Log
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );

                    lSignupAlerts.Text = @"<div class='alert alert-danger'>An unexpected error has occured. Please check your exception log for more detail.</div>";
                    return;
                }

                // Checking if creation was successful
                if ( responseObject.Success )
                {
                    lSignupAlerts.Text = @"<div class='alert alert-success'>Thank you for signing up to BEMA's support services.  Someone from our support team will reach out to you in the next one to two business days.</div>";
                }
                else
                {
                    lSignupAlerts.Text = @"<div class='alert alert-danger'>An unexpected error has occured during signup.  Please try again in a few minutes.</div>";
                }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gSupportRequests_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                SupportRequest request = ( SupportRequest ) e.Row.DataItem;

                if ( request.Urgency == "Emergency" )
                {
                    e.Row.AddCssClass( "danger" );
                }
            }
        }

        #endregion

        #region Models

        public class APIResponse
        {
            public APIResponse()
            {
                SupportRequests = new List<SupportRequest>();
            }
            public List<SupportRequest> SupportRequests { get; set; }
            public bool Success { get; set; }
        }

        public class SupportRequest
        {
            public SupportRequest()
            {
                Notes = new List<Note>();
            }
            public int Id { get; set; }
            public string Subject { get; set; }
            public string SupportRequestDescription { get; set; }
            public string Urgency { get; set; }
            public string Submitter { get; set; }
            public string SubmitterPersonAliasGuid { get; set; }
            public string Email { get; set; }
            public DateTime Created { get; set; }
            public string RockVersion { get; set; }
            public string Status { get; set; }
            public List<Note> Notes { get; set; }
        }

        public class UpdateResponse
        {
            public int? Id { get; set; }
            public bool Success { get; set; }
            public int? Error { get; set; } // Used for troubleshooting

        }

        public class CreateResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public class Note
        {
            public string Worker { get; set; }
            public string UpdateText { get; set; }
            public DateTime Created { get; set; }
        }

        public class NewSupportRequest
        {
            public string Subject { get; set; }
            public string SupportRequestDescription { get; set; }
            public string Urgency { get; set; }
            public string Submitter { get; set; }
            public string SubmitterEmail { get; set; }
            public string RockVersion { get; set; }
            public string SubmitterPersonAliasGuid { get; set; }
            public string SupportPluginVersion { get; set; }
        }

        public class NewSignupRequest
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string JobTitle { get; set; }
            public string Organization { get; set; }
            public string ContactNumber { get; set; }
            public string Address { get; set; }
        }

        public enum Modes
        {
            MY,
            ALL,
            COMPLETED
        }

        #endregion
    }
}