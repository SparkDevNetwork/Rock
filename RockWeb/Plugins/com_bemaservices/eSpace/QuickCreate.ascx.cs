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
using System.Runtime.Serialization;
using System.Web.UI;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace RockWeb.Plugins.com_bemaservices.eSpace
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Quick Create Event" )]
    [Category( "BEMA Services > eSpace" )]
    [Description( "Form to create new events in eSpace" )]
    [BooleanField( "Require Number Of People", "Require that user sets the number of people for each event", defaultValue: true, order: 1 )]
    [BooleanField( "Require Categories", "Require that user sets at least one category for an event", defaultValue: true, order: 2 )]
    public partial class QuickCreate : Rock.Web.UI.RockBlock
    {

        private List<SelectedSpace> selectedSpaces
        {
            get
            {
                if ( ViewState["selectedSpaces"] == null )
                {
                    ViewState["selectedSpaces"] = new List<SelectedSpace>();
                }
                return ViewState["selectedSpaces"] as List<SelectedSpace>;
            }

            set
            {
                ViewState["selectedSpaces"] = value;
            }
        }

        private List<SelectedSpace> allSpaces
        {
            get
            {
                if ( ViewState["allSpaces"] == null )
                {
                    ViewState["allSpaces"] = new List<SelectedSpace>();
                }
                return ViewState["allSpaces"] as List<SelectedSpace>;
            }

            set
            {
                ViewState["allSpaces"] = value;
            }
        }

        private List<SelectedResource> selectedResources
        {
            get
            {
                if ( ViewState["selectedResources"] == null )
                {
                    ViewState["selectedResources"] = new List<SelectedResource>();
                }
                return ViewState["selectedResources"] as List<SelectedResource>;
            }

            set
            {
                ViewState["selectedResources"] = value;
            }
        }
        private List<SelectedResource> allResources
        {
            get
            {
                if ( ViewState["allResources"] == null )
                {
                    ViewState["allResources"] = new List<SelectedResource>();
                }
                return ViewState["allResources"] as List<SelectedResource>;
            }

            set
            {
                ViewState["allResources"] = value;
            }
        }

        private List<SelectedService> selectedServices
        {
            get
            {
                if ( ViewState["selectedServices"] == null )
                {
                    ViewState["selectedServices"] = new List<SelectedService>();
                }
                return ViewState["selectedServices"] as List<SelectedService>;
            }

            set
            {
                ViewState["selectedServices"] = value;
            }
        }
        private List<SelectedService> allServices
        {
            get
            {
                if ( ViewState["allServices"] == null )
                {
                    ViewState["allServices"] = new List<SelectedService>();
                }
                return ViewState["allServices"] as List<SelectedService>;
            }

            set
            {
                ViewState["allServices"] = value;
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
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlEntry );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {

            if( !IsPostBack )
            {
                var username = GetUserPreference( "com.bemaservices.eSpace.Username" );
                var password = Rock.Security.Encryption.DecryptString( GetUserPreference( "com.bemaservices.eSpace.Password" ) );

                //Require Number of People
                if ( GetAttributeValue( "RequireNumberOfPeople" ).AsBoolean() )
                {
                    numNumberOfPeople.Required = true;
                }
                //Require Category
                if ( GetAttributeValue( "RequireCategories" ).AsBoolean() )
                {
                    cblCategories.Required = true;
                }

                //if no username/password, 
                if ( !username.Any() || !password.Any() )
                {
                    ShowUsernamePasswordPanel();
                    
                }
                else //continue loading
                {
                    //Call Location and Category binder
                    BindLocationsAndCategories();
                }
            }
            

            base.OnLoad( e );
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
            
        }

        /// <summary>
        /// Saves any server control view-state changes that have occurred since the time the page was posted back to the server.
        /// </summary>
        /// <returns>
        /// Returns the server control's current view state. If there is no view state associated with the control, this method returns null.
        /// </returns>
        protected override object SaveViewState()
        {

            return base.SaveViewState();
        }

        //private void BindSpaces( )
        //{
        //    rptSpaces.DataSource = selectedSpaces;
        //    rptSpaces.DataBind();
        //}

        //private void BindResources()
        //{
        //    rptResources.DataSource = selectedResources;
        //    rptResources.DataBind();
        //}

        //private void BindServices()
        //{
        //    rptServices.DataSource = selectedServices;
        //    rptServices.DataBind();
        //}

        private void BindSpaceTree( List<SelectedSpace> list, TreeNode parentNode )
        {
            if ( parentNode == null )
            {
                treeSpaces.Nodes.Clear();
            }
            var nodes = list.Where( x => ( parentNode == null ) ? ( x.ParentId == null ) : ( x.ParentId == int.Parse( parentNode.Value ) ) ).ToList();
            foreach ( var node in nodes )
            {
                TreeNode newNode = new TreeNode( node.Name, node.Id.ToString() );
                if ( !node.IsSchedulable )
                {
                    newNode.ShowCheckBox = false;
                }

                if ( parentNode == null )
                {
                    treeSpaces.Nodes.Add( newNode );
                }
                else
                {
                    parentNode.ChildNodes.Add( newNode );
                }
                BindSpaceTree( list, newNode );
            }
        }

        private void BindResourceTree( List<SelectedResource> list, TreeNode parentNode )
        {
            if ( parentNode == null )
            {
                treeResources.Nodes.Clear();
            }
            var nodes = list.Where( x => ( parentNode == null ) ? ( x.ParentId == null ) : ( x.ParentId == int.Parse( parentNode.Value ) ) ).ToList();
            
            foreach ( var node in nodes )
            {
                TreeNode newNode = new TreeNode( node.Name, node.Id.ToString() );
                newNode.ToolTip = node.Id.ToString();
                if ( !node.IsSchedulable )
                {
                    newNode.ShowCheckBox = false;
                }

                if ( parentNode == null )
                {
                    treeResources.Nodes.Add( newNode );
                }
                else
                {
                    parentNode.ChildNodes.Add( newNode );
                }
                
                BindResourceTree( list, newNode );
            }
        }

        private void BindServiceTree( List<SelectedService> list, TreeNode parentNode )
        {
            if ( parentNode == null )
            {
                treeServices.Nodes.Clear();
            }
            var nodes = list.Where( x => ( parentNode == null ) ? ( x.ParentId == null ) : ( x.ParentId == int.Parse( parentNode.Value ) ) ).ToList();
            foreach ( var node in nodes )
            {
                TreeNode newNode = new TreeNode( node.Name, node.Id.ToString() );
                if ( !node.IsSchedulable )
                {
                    newNode.ShowCheckBox = false;
                }

                if ( parentNode == null )
                {
                    treeServices.Nodes.Add( newNode );
                }
                else
                {
                    parentNode.ChildNodes.Add( newNode );
                }
                BindServiceTree( list, newNode );
            }
        }

        private dynamic CallRestAPIeSPACE( string url )
        {
            var username = GetUserPreference( "com.bemaservices.eSpace.Username" );
            var password = Rock.Security.Encryption.DecryptString( GetUserPreference( "com.bemaservices.eSpace.Password" ) );

            using ( var httpClient = new WebClient() )
            {
                httpClient.BaseAddress = "https://api.espace.cool/api/v1/";
                httpClient.Credentials = new NetworkCredential( username, password );
                try
                {
                    var callResponse = httpClient.DownloadString( url );

                    dynamic callData = JsonConvert.DeserializeObject<dynamic>( callResponse ).Data;

                    return callData;
                }
                catch
                {
                    SetUserPreference( "com.bemaservices.eSpace.Password", "", true );
                    hfErrorMessage.Value = "Could Not Get Data; Please Re-Enter Password";
                    ShowUsernamePasswordPanel();
                }
                
            }
            return null;
        }

        private dynamic CallPOSTRestAPIeSPACE( string url, System.Collections.Specialized.NameValueCollection parameters, string data )
        {
            var username = GetUserPreference( "com.bemaservices.eSpace.Username" );
            var password = Rock.Security.Encryption.DecryptString( GetUserPreference( "com.bemaservices.eSpace.Password" ) );

            using ( var httpClient = new WebClient() )
            {
                httpClient.BaseAddress = "https://api.espace.cool/api/v1/";
                httpClient.Credentials = new NetworkCredential( username, password );
                try
                {
                    if ( parameters != null )
                    {
                        httpClient.QueryString = parameters;
                    }
                    httpClient.Headers["Content-Type"] = "application/json";
                    var callResponse = httpClient.UploadString( url, data );
                    dynamic callData = JsonConvert.DeserializeObject<dynamic>( callResponse ).Data;
                    return callData;
                }
                catch
                {
                    //TODO
                    SetUserPreference( "com.bemaservices.eSpace.Password", "", true );
                    hfErrorMessage.Value = "Could Not Get Data; Please Re-Enter Password";
                    ShowUsernamePasswordPanel();
                }

            }
            return null;
        }

        private dynamic CallPUTRestAPIeSPACE( string url, System.Collections.Specialized.NameValueCollection parameters, string data )
        {
            var username = GetUserPreference( "com.bemaservices.eSpace.Username" );
            var password = Rock.Security.Encryption.DecryptString( GetUserPreference( "com.bemaservices.eSpace.Password" ) );

            using ( var httpClient = new WebClient() )
            {
                httpClient.BaseAddress = "https://api.espace.cool/api/v1/";
                httpClient.Credentials = new NetworkCredential( username, password );
                try
                {
                    if ( parameters != null )
                    {
                        httpClient.QueryString = parameters;
                    }
                    httpClient.Headers["Content-Type"] = "application/json";
                    var callResponse = httpClient.UploadString( url, "PUT", data );
                    dynamic callData = JsonConvert.DeserializeObject<dynamic>( callResponse )/*.Data*/;
                    return callData;
                }
                catch
                {
                    //TODO
                    SetUserPreference( "com.bemaservices.eSpace.Password", "", true );
                    hfErrorMessage.Value = "Could Not Get Data; Please Re-Enter Password";
                    ShowUsernamePasswordPanel();
                }

            }
            return null;
        }

        private string CallDELETERestAPIeSPACE( string url )
        {
            var username = GetUserPreference( "com.bemaservices.eSpace.Username" );
            var password = Rock.Security.Encryption.DecryptString( GetUserPreference( "com.bemaservices.eSpace.Password" ) );

            using ( var httpClient = new WebClient() )
            {
                httpClient.BaseAddress = "https://api.espace.cool/api/v1/";
                httpClient.Credentials = new NetworkCredential( username, password );
                try
                {
                    var locationsResponse = httpClient.UploadString( url, "DELETE", "" );

                    return locationsResponse;
                }
                catch
                {
                    SetUserPreference( "com.bemaservices.eSpace.Password", "", true );
                    hfErrorMessage.Value = "Could Not Get Data; Please Re-Enter Password";
                    ShowUsernamePasswordPanel();
                }

            }
            return null;
        }

        private void ShowUsernamePasswordPanel()
        {
            var username = GetUserPreference( "com.bemaservices.eSpace.Username" );
            pnlAPIEntry.Visible = true;
            nbAlert.Text = "Please Enter Your eSPACE Credentials";
            if ( hfErrorMessage.Value.IsNotNullOrWhiteSpace() )
            {
                nbAlert.Text = hfErrorMessage.Value;
                hfErrorMessage.Value = string.Empty;
            }
            nbAlert.Visible = true;
            pnlEntry.Visible = false;
            pnlSpacesResourcesServices.Visible = false;

            //fill in username if available, if not use current person email on file
            tbAPIUsername.Text = username.IsNotNullOrWhiteSpace() ? username : CurrentPerson.Email;
            tbAPIPassword.Text = "";
            //Enable textbox and save button
            tbAPIUsername.Visible = true;
            tbAPIPassword.Visible = true;
            btnAPIKeySave.Visible = true;

            //Save button resets panels
        }

        private void BindLocationsAndCategories()
        {
            var locationsResponse = CallRestAPIeSPACE( "ministry/locations" );
            cblLocations.DataSource = locationsResponse;
            cblLocations.DataBind();

            var categoriesResponse = CallRestAPIeSPACE( "ministry/categories" );
            cblCategories.DataSource = categoriesResponse;
            cblCategories.DataBind();
        }

        private List<SelectedSpace> GetSpacesFromObject( dynamic space, string parents = "" )
        {
            List<SelectedSpace> listSelectedSpace = new List<SelectedSpace>();

            //if ( Convert.ToBoolean( space.IsSchedulable ) == true )
            //{
            //    listSelectedSpace.Add( new SelectedSpace { Id = space.ItemId, Name = parents + space.Name, ParentId = space.ParentId, LocCode = space.LocCode, IsSchedulable = space.IsSchedulable, HasSchedualbleChildren = space.HasSchedualbleChildren } );
            //}

            listSelectedSpace.Add( new SelectedSpace { Id = space.ItemId, Name = " " + space.Name, ParentId = space.ParentId, LocCode = space.LocCode, IsSchedulable = space.IsSchedulable, HasSchedualbleChildren = space.HasSchedualbleChildren } );

            if ( Convert.ToBoolean( space.HasSchedualbleChildren ) == true )
            {
                parents += space.Name + " > ";
                foreach ( dynamic child in space.Children )
                {
                    listSelectedSpace.AddRange( GetSpacesFromObject( child, parents ) );
                }
            }

            return listSelectedSpace;
        }

        private List<SelectedResource> GetResourcesFromObject( dynamic resource, string parents = "" )
        {
            List<SelectedResource> listSelectedResource = new List<SelectedResource>();

            //if ( Convert.ToBoolean( resource.IsSchedulable ) == true )
            //{
            //    listSelectedResource.Add( new SelectedResource { Id = resource.ItemId, Name = resource.Name, LocCode = resource.LocCode, ParentId = resource.ParentId, QuantityOnHand = resource.CurrentQtyOnHand, IsSchedulable = resource.IsSchedulable, HasSchedualbleChildren = resource.HasSchedualbleChildren } );
            //}

            listSelectedResource.Add( new SelectedResource { Id = resource.ItemId, Name = " " + resource.Name, LocCode = resource.LocCode, ParentId = resource.ParentId, QuantityOnHand = resource.CurrentQtyOnHand, IsSchedulable = resource.IsSchedulable, HasSchedualbleChildren = resource.HasSchedualbleChildren } );

            if ( Convert.ToBoolean( resource.HasSchedualbleChildren ) == true )
            {
                parents += resource.Name + " > ";
                foreach ( dynamic child in resource.Children )
                {
                    listSelectedResource.AddRange( GetResourcesFromObject( child, parents ) );
                }
            }

            return listSelectedResource;
        }

        private List<SelectedService> GetServicesFromObject( dynamic service, string parents = "" )
        {
            List<SelectedService> listSelectedService = new List<SelectedService>();

            //if ( Convert.ToBoolean( service.IsSchedulable ) == true )
            //{
            //    listSelectedService.Add( new SelectedService { Id = service.ItemId, Name = parents + service.Name, ParentId = service.ParentId, LocCode = service.LocCode, IsSchedulable = service.IsSchedulable, HasSchedualbleChildren = service.HasSchedualbleChildren } );
            //}

            listSelectedService.Add( new SelectedService { Id = service.ItemId, Name = " " + service.Name, ParentId = service.ParentId, LocCode = service.LocCode, IsSchedulable = service.IsSchedulable, HasSchedualbleChildren = service.HasSchedualbleChildren } );

            if ( Convert.ToBoolean( service.HasSchedualbleChildren ) == true )
            {
                parents += service.Name + " > ";
                foreach ( dynamic child in service.Children )
                {
                    listSelectedService.AddRange( GetServicesFromObject( child, parents ) );
                }
            }

            return listSelectedService;
        }

        private void FindAllSpacesResourcesServices()
        {
            nbAlert.Visible = false;
            int? eventId = hfEventId.Value.AsIntegerOrNull();
            int? scheduleId = hfScheduleId.Value.AsIntegerOrNull();

            allSpaces.Clear();
            allResources.Clear();
            allServices.Clear();

            //Find Spaces
            var allSpacesData = CallRestAPIeSPACE( "event/spaces?eventId=" + eventId.Value.ToStringSafe() + "&scheduleId=" + scheduleId.Value.ToStringSafe() );
            //allSpaces.Add( new SelectedSpace() );
            foreach ( var space in allSpacesData )
            {
                //space has ItemId, ItemType, Name, LocationName, LocationId, Parent_id, ParentName, and IsSchedulable
                allSpaces.AddRange( GetSpacesFromObject( space ) );
                
            }
            if ( allSpaces.Count() <= 1 )
            {
                //if all spaces doesnt have any content, disable it
                //rptSpaces.Visible = false;
            }
            else
            {
                BindSpaceTree( allSpaces, null );
            }

            //Find Resources
            var allResourcesData = CallRestAPIeSPACE( "event/resources?eventId=" + eventId.Value.ToStringSafe() + "&scheduleId=" + scheduleId.Value.ToStringSafe() );
            //allResources.Add( new SelectedResource() );
            foreach ( var resource in allResourcesData )
            {
                allResources.AddRange( GetResourcesFromObject( resource ) );  
            }
            if ( allResources.Count() <= 1 )
            {
                //if all spaces doesnt have any content, disable it
                //rptResources.Visible = false;
            }
            else
            {
                BindResourceTree( allResources, null );

                rptResourceQty.DataSource = allResources.Where( r => r.QuantityOnHand != null );
                rptResourceQty.DataBind();
            }

            //Find Services
            var allServicesData = CallRestAPIeSPACE( "event/services?eventId=" + eventId.Value.ToStringSafe() + "&scheduleId=" + scheduleId.Value.ToStringSafe() );
            //allServices.Add( new SelectedService() );
            foreach ( var service in allServicesData )
            {
                allServices.AddRange( GetServicesFromObject( service ) );
            }
            if ( allServices.Count() <= 1 )
            {
                //if all spaces doesnt have any content, disable it
                //rptServices.Visible = false;
            }
            else
            {
                BindServiceTree( allServices, null );
            }
        }

        private void CreateEvent()
        {
            nbAlert.Visible = false;
            string data = "";
            //dynamically represent the event data at runtime, to convert into json string
            dynamic eventData = new System.Dynamic.ExpandoObject();

            //Event Properties:
            eventData.EventName = tbEventName.Text;
            eventData.Description = tbDescription.Text;
            eventData.IsPublic = cbIsPublic.Checked;
            if ( cbIsPublic.Checked )
            {
                eventData.PublicLocations = cblLocations.SelectedValuesAsInt.ToArray();
                eventData.PublicNotes = "";
            }
            eventData.NumOfPeople = numNumberOfPeople.Text.AsInteger();
            eventData.IsAllDayEvent = cbAllDayEvent.Checked;
            eventData.Categories = cblCategories.SelectedValuesAsInt.ToArray();
            eventData.Locations = cblLocations.SelectedValuesAsInt.ToArray();

            //Event Schedule
            if( !dtpSetupStart.SelectedDateTimeIsBlank )
            {
                eventData.SetupStartDate = dtpSetupStart.SelectedDateTime.Value.ToString("o");
                eventData.SetupStartTime = dtpSetupStart.SelectedDateTime.Value.ToString("HH:mm:ss");
            }
            if( !dtpSetupEnd.SelectedDateTimeIsBlank )
            {
                eventData.TeardownEndDate = dtpSetupEnd.SelectedDateTime.Value.ToString("o");
                eventData.TearDownEndTime = dtpSetupEnd.SelectedDateTime.Value.ToString("HH:mm:ss");
            }
            if( dateEventEnd.SelectedDate.HasValue )
            {
                eventData.EventEndDate = dateEventEnd.SelectedDate.Value.ToString("o");
            }
            if( timeEventEnd.SelectedTime.HasValue )
            {
                eventData.EndTime = timeEventEnd.SelectedTime.Value.ToString( "c" );
            }
            //must have start date
            eventData.EventDate = dateEventStart.SelectedDate.Value.ToString("o");
            if( timeEventStart.SelectedTime.HasValue )
            {
                eventData.StartTime = timeEventStart.SelectedTime.Value.ToString( "c" );
            }
            

            //Parse iCalendar repeater for additional dates (only up to two years)
            if( scheduleBuilder.iCalendarContent.IsNotNullOrWhiteSpace() )
            {
                var iCalContent = scheduleBuilder.iCalendarContent;
                var calendarColleciton = DDay.iCal.iCalendar.LoadFromStream( iCalContent.ToStreamReader() );
                var iCal = calendarColleciton.First( c => c.iCalendar.IsNotNull() );
                var iCalEvent = iCal.Events.First();
                var occurrenceList = iCalEvent.GetOccurrences( dateEventStart.SelectedDate.Value, dateEventStart.SelectedDate.Value.AddYears( 2 ) );

                eventData.AdditionalDates = occurrenceList.Select( o => o.Period.StartTime.Date.ToString("o") ).ToArray();
            }
            

            //convert eventData to string json
            data = JsonConvert.SerializeObject( eventData );

            //Show Submitted panel
            //pnlSpacesResourcesServices.Visible = false;
            //pnlSubmitted.Visible = true;

            int? eventId = null;
            int? scheduleId = null;

            submitLiteral.Text = "<p>Creating Event</p>";
            var eventResponse =  CallPOSTRestAPIeSPACE( "event/create", null, data );
            try
            {
                eventId = eventResponse.EventId;
                scheduleId = eventResponse.ScheduleId;
            }
            catch ( Exception ex )
            {
                //Display Error
                nbAlert.Text = "<p>" + eventResponse + "</p>";
                nbAlert.Visible = true;
            }

            hfEventId.Value = eventId.ToStringSafe();
            hfScheduleId.Value = scheduleId.ToStringSafe();
            
        }

        /// <summary>
        /// Takes in selected spaces, resources, and services and then finally submits the event
        /// </summary>
        private void SubmitEvent()
        {
            GetSelectedItems();

            int? eventId = hfEventId.Value.AsIntegerOrNull();
            int? scheduleId = hfScheduleId.Value.AsIntegerOrNull();

            //if we created an event successfully, add in spaces, resources, and services
            if ( eventId.HasValue && scheduleId.HasValue && selectedSpaces.Any() )
            {
                submitLiteral.Text = "<p>Submitting Spaces</p>";
                var parameters = new System.Collections.Specialized.NameValueCollection();
                parameters.Add( "eventId", eventId.ToString() );
                parameters.Add( "scheduleId", scheduleId.ToString() );
                var spaceIds = JsonConvert.SerializeObject( selectedSpaces.Where( s => s.Id != 0 ).Select( s => s.Id ).ToList() );

                var spacesResponse = CallPOSTRestAPIeSPACE( "event/spaces/add", parameters, spaceIds );
            }

            if ( eventId.HasValue && scheduleId.HasValue && selectedResources.Any() )
            {
                submitLiteral.Text = "<p>Submitting Resources</p>";
                var parameters = new System.Collections.Specialized.NameValueCollection();
                parameters.Add( "eventId", eventId.ToString() );
                parameters.Add( "scheduleId", scheduleId.ToString() );
                var resources = JsonConvert.SerializeObject( selectedResources.Where( s => s.Id != 0 ).Select( s => new { Id = s.Id, Quantity = s.QuantitySelected } ).ToList() );

                var resourcesResponse = CallPOSTRestAPIeSPACE( "event/resources/add", parameters, resources );
            }

            if ( eventId.HasValue && scheduleId.HasValue && selectedServices.Any() )
            {
                submitLiteral.Text = "<p>Submitting Services</p>";
                var parameters = new System.Collections.Specialized.NameValueCollection();
                parameters.Add( "eventId", eventId.ToString() );
                parameters.Add( "scheduleId", scheduleId.ToString() );
                string csvServiceIds = JsonConvert.SerializeObject( selectedServices.Where( s => s.Id != 0 ).Select( s => s.Id ).ToList() );

                var servicesResponse = CallPOSTRestAPIeSPACE( "event/services/add", parameters, csvServiceIds );
            }

            //if we've got this far, submit event
            if ( eventId.HasValue && scheduleId.HasValue )
            {
                submitLiteral.Text = "<p>Submitting Event</p>";
                var parameters = new System.Collections.Specialized.NameValueCollection();
                parameters.Add( "eventId", eventId.ToString() );
                parameters.Add( "scheduleId", scheduleId.ToString() );
                dynamic submitRespObject = CallPUTRestAPIeSPACE( "event/submit", parameters, "" );
                try
                {
                    eventId = submitRespObject.Data.EventId;
                    scheduleId = submitRespObject.Data.ScheduleId;
                    submitLiteral.Text = "<p>Event Has Been Submitted to eSPACE</p>";
                    submitLiteral.Text += "<br/><a target='_blank' class='btn btn-primary' href='https://app.espace.cool/EventDetails?eventId=" + eventId + "&scheduleId=" + scheduleId + "'>eSPACE Event</a>";

                }
                catch
                {
                    submitLiteral.Text = "<p>Event Needs Further Information</p>";
                    submitLiteral.Text += "<p>" +  submitRespObject.Message + "<p>";
                    submitLiteral.Text += "<br/><a target='_blank' class='btn btn-primary' href='https://app.espace.cool/EventDetails?eventId=" + eventId + "&scheduleId=" + scheduleId  + "&page=setup" + "'>Click to Continue in eSpace</a>";

                }


                pnlSpacesResourcesServices.Visible = false;
                pnlSubmitted.Visible = true;
            }
        }


        private void GetSelectedItems()
        {
            List<int> selectedSpaceIds = new List<int>();
            foreach ( TreeNode space in treeSpaces.CheckedNodes )
            {
                selectedSpaceIds.Add( space.Value.AsInteger() );
            }

            selectedSpaces = allSpaces.Where( s => selectedSpaceIds.Contains( s.Id ) ).ToList();

            //Add Resources (and fetch qty)

            List<int> selectedResourceIds = new List<int>();
            foreach( TreeNode resource in treeResources.CheckedNodes )
            {
                selectedResourceIds.Add( resource.Value.AsInteger() );
                
            }
            foreach( RepeaterItem item in rptResourceQty.Items )
            {
                if ( item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem )
                {
                    HiddenField hfResourceId = ( HiddenField ) item.FindControl( "hfResourceId" );
                    TextBox nbQuantity = ( TextBox ) item.FindControl( "nbQuantity" );

                    var selectedResource = allResources.First( r => r.Id == hfResourceId.ValueAsInt() );
                    selectedResource.QuantitySelected = nbQuantity.Text.AsIntegerOrNull();
                }
            }
            selectedResources = allResources.Where( r => selectedResourceIds.Contains( r.Id ) ).ToList();

            //Add Services

            List<int> selectedServiceIds = new List<int>();
            foreach( TreeNode service in treeServices.CheckedNodes )
            {
                selectedServiceIds.Add( service.Value.AsInteger() );
            }

            selectedServices = allServices.Where( s => selectedServiceIds.Contains( s.Id ) ).ToList();
        }


        #endregion

        #region Event Handlers

        protected void btnAPIKeySave_Click( object sender, EventArgs e )
        {
            //Set user preferences, but encrypt the password
            SetUserPreference( "com.bemaservices.eSpace.Username", tbAPIUsername.Text, true );
            SetUserPreference( "com.bemaservices.eSpace.Password", Rock.Security.Encryption.EncryptString( tbAPIPassword.Text ), true );
            
            NavigateToCurrentPage(); //refresh page to get new API
        }

        /// <summary>
        /// All Day checkbox selected ( disable time controls )
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void cbAllDayEvent_CheckedChanged( object sender, EventArgs e )
        {
            // disable event end datetime and start time
            if ( cbAllDayEvent.Checked )
            {
                timeEventStart.Visible = false;
                timeEventStart.SelectedTime = null;
                timeEventEnd.SelectedTime = null;
                timeEventEnd.Visible = false;
            }
            else
            {
                timeEventStart.Visible = true;
                timeEventEnd.Visible = true;
            }
            
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void scheduleBuilder_Load( object sender, EventArgs e )
        {
            //Use recursive method to find the startdatetime control and fill it with the event start datetime (wont really be used in event submission)
            DateTimePicker dpStartDateTime = ( DateTimePicker ) FindControlRecursive(scheduleBuilder, "dpStartDateTime_scheduleBuilderPopupContents_scheduleBuilder" );
            if ( dpStartDateTime != null )
            {
                dpStartDateTime.SelectedDateTime = dateEventStart.SelectedDate;
            }
        }

        private Control FindControlRecursive( Control parentControl, string name )
        {
            foreach( Control child in parentControl.Controls )
            {
                if ( child.ID == name )
                {
                    return child;
                }
                else if ( child.Controls.Count > 0 )
                {
                    var returnedChild = FindControlRecursive( child, name );
                    if ( returnedChild != null )
                    {
                        return returnedChild;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void scheduleBuilder_SaveSchedule( object sender, EventArgs e )
        {
            //Compile into iCalendar object to later be calculated for eSpace API json object
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void btnAddSpaces_Click( object sender, EventArgs e )
        //{
        //    selectedSpaces.Add( new SelectedSpace() );
        //    BindSpaces();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void btnAddResources_Click( object sender, EventArgs e )
        //{
        //    selectedResources.Add( new SelectedResource() );
        //    BindResources();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void btnAddServices_Click( object sender, EventArgs e )
        //{
        //    selectedServices.Add( new SelectedService() );
        //    BindServices();
        //}

        /// <summary>
        /// For every item bound to spaces (including blanks), populate controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void rptSpaces_ItemDataBound( object sender, RepeaterItemEventArgs e )
        //{
        //    if( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
        //    {
        //        RockDropDownList ddlSpacesItem = ( RockDropDownList ) e.Item.FindControl( "ddlSpacesItem" );
        //        int? selectedValue = DataBinder.Eval( e.Item.DataItem, "Id" ).ToStringSafe().AsIntegerOrNull();
        //        ddlSpacesItem.DataSource = allSpaces.Where( r => r.Id == 0 || r.Id == selectedValue || !selectedSpaces.Any( s => s.Id == r.Id ) );
        //        ddlSpacesItem.DataBind();

        //        //if Id is not null, select it
        //        ddlSpacesItem.SelectedValue = DataBinder.Eval( e.Item.DataItem, "Id" ).ToStringSafe();
        //    }
            
        //}

        /// <summary>
        /// For every item bound to spaces (including blanks), populate controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void rptResources_ItemDataBound( object sender, RepeaterItemEventArgs e )
        //{
        //    if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
        //    {
        //        RockDropDownList ddlResourcesItem = ( RockDropDownList ) e.Item.FindControl( "ddlResourcesItem" );
        //        int? selectedValue = DataBinder.Eval( e.Item.DataItem, "Id" ).ToStringSafe().AsIntegerOrNull();
        //        ddlResourcesItem.DataSource = allResources.Where( r => r.Id == 0 || r.Id == selectedValue || !selectedResources.Any( s => s.Id == r.Id ) );
        //        ddlResourcesItem.DataBind();

        //        //if Id is not null, select it
        //        ddlResourcesItem.SelectedValue = DataBinder.Eval( e.Item.DataItem, "Id" ).ToStringSafe();

        //        int? onHand = DataBinder.Eval( e.Item.DataItem, "QuantityOnHand" ).ToStringSafe().AsIntegerOrNull();

        //        NumberBox nbQuantity = ( NumberBox ) e.Item.FindControl( "nbQuantity" );
        //        nbQuantity.Text = DataBinder.Eval( e.Item.DataItem, "QuantitySelected" ).ToStringSafe();
        //        if ( onHand.HasValue )
        //        {
        //            nbQuantity.Label = "Qty: " + onHand.ToStringSafe();
        //        }
        //        else
        //        {
        //            nbQuantity.Visible = false;
        //        }
                
        //    }

        //}

        /// <summary>
        /// For every item bound to spaces (including blanks), populate controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void rptServices_ItemDataBound( object sender, RepeaterItemEventArgs e )
        //{
        //    if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
        //    {
        //        RockDropDownList ddlServicesItem = ( RockDropDownList ) e.Item.FindControl( "ddlServicesItem" );
        //        int? selectedValue = DataBinder.Eval( e.Item.DataItem, "Id" ).ToStringSafe().AsIntegerOrNull();
        //        ddlServicesItem.DataSource = allServices.Where( r => r.Id == 0 || r.Id == selectedValue || !selectedServices.Any( s => s.Id == r.Id ) );
        //        ddlServicesItem.DataBind();

        //        //if Id is not null, select it
        //        ddlServicesItem.SelectedValue = DataBinder.Eval( e.Item.DataItem, "Id" ).ToStringSafe();
        //    }

        //}

        /// <summary>
        /// Validates Entry panel and preps spaces, resources, and services 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnEntryNext_Click( object sender, EventArgs e )
        {
            CreateEvent();
            if ( hfEventId.Value.IsNotNullOrWhiteSpace() && hfScheduleId.Value.IsNotNullOrWhiteSpace() )
            {
                FindAllSpacesResourcesServices();
                pnlEntry.Visible = false;
                pnlSpacesResourcesServices.Visible = true;
            }
        }


        ///// <summary>
        ///// Save selected space when item is changed
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //protected void ddlSpacesItem_SelectedIndexChanged( object sender, EventArgs e )
        //{
        //    RockDropDownList ddl = ( RockDropDownList ) sender;
        //    RepeaterItem rptItem = ( RepeaterItem ) ddl.Parent;
        //    try
        //    {
        //        selectedSpaces[rptItem.ItemIndex] = allSpaces.First( s => s.Id == ddl.SelectedValueAsId() );

        //    }
        //    catch
        //    {
        //        //failed for some reason
        //    }
        //    BindSpaces();
        //}

        /// <summary>
        /// Save selected space when item is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void ddlResourcesItem_SelectedIndexChanged( object sender, EventArgs e )
        //{
        //    RockDropDownList ddl = ( RockDropDownList ) sender;
        //    RepeaterItem rptItem = ( RepeaterItem ) ddl.Parent;
        //    try
        //    {
        //        selectedResources[rptItem.ItemIndex] = allResources.First( s => s.Id == ddl.SelectedValueAsId() );
        //    }
        //    catch
        //    {
        //        //failed for some reason
        //    }
        //    BindResources();
        //}


        //protected void nbQuantity_TextChanged( object sender, EventArgs e )
        //{
        //    NumberBox nb = ( NumberBox ) sender;
        //    RepeaterItem rptItem = ( RepeaterItem ) nb.Parent;
        //    try
        //    {
        //        selectedResources[rptItem.ItemIndex].QuantitySelected = nb.Text.AsIntegerOrNull();
        //    }
        //    catch
        //    {
        //        //failed for some reason
        //    }
        //    BindResources();
        //}

        /// <summary>
        /// Save selected space when item is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void ddlServicesItem_SelectedIndexChanged( object sender, EventArgs e )
        //{
        //    RockDropDownList ddl = ( RockDropDownList ) sender;
        //    RepeaterItem rptItem = ( RepeaterItem ) ddl.Parent;
        //    try
        //    {
        //        selectedServices[rptItem.ItemIndex] = allServices.First( s => s.Id == ddl.SelectedValueAsId() );
        //    }
        //    catch
        //    {
        //        //failed for some reason
        //    }
        //    BindServices();
        //}


        protected void btnItemsSubmit_Click( object sender, EventArgs e )
        {
            SubmitEvent();
        }


        //protected void spaces_delete_click( object sender, EventArgs e )
        //{
        //    BootstrapButton ddl = ( BootstrapButton ) sender;
        //    RepeaterItem rptItem = ( RepeaterItem ) ddl.Parent;
        //    selectedSpaces.RemoveAt( rptItem.ItemIndex );
        //    BindSpaces();
        //}


        //protected void resources_delete_click( object sender, EventArgs e )
        //{
        //    BootstrapButton ddl = ( BootstrapButton ) sender;
        //    RepeaterItem rptItem = ( RepeaterItem ) ddl.Parent;
        //    selectedResources.RemoveAt( rptItem.ItemIndex );
        //    BindResources();
        //}


        //protected void services_delete_click( object sender, EventArgs e )
        //{
        //    BootstrapButton ddl = ( BootstrapButton ) sender;
        //    RepeaterItem rptItem = ( RepeaterItem ) ddl.Parent;
        //    selectedServices.RemoveAt( rptItem.ItemIndex );
        //    BindServices();
        //}


        protected void btnItemsCancel_Click( object sender, EventArgs e )
        {
            int? eventId = hfEventId.Value.AsIntegerOrNull();

            pnlSpacesResourcesServices.Visible = false;
            pnlEntry.Visible = true;

            //Delete event
            var delete = CallDELETERestAPIeSPACE( "event/delete?eventId=" + eventId );

            
        }


        protected void btnNew_Click( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }
        
        protected void dateEventStart_TextChanged( object sender, EventArgs e )
        {
            if( dateEventStart.SelectedDate != null && dateEventEnd.SelectedDate == null )
            {
                dateEventEnd.SelectedDate = dateEventStart.SelectedDate;
            }
        }
        

        #endregion

        #region Models

        [Serializable]
        private class SelectedSpace
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int? ParentId { get; set; }

            public string LocCode { get; set; }

            public bool IsSchedulable { get; set; }

            public bool HasSchedualbleChildren { get; set; }

        }

        [Serializable]
        private class SelectedResource
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int? ParentId { get; set; }

            public int? QuantityOnHand { get; set; }

            public int? QuantitySelected { get; set; }

            public string LocCode { get; set; }

            public bool IsSchedulable { get; set; }

            public bool HasSchedualbleChildren { get; set; }

        }

        [Serializable]
        private class SelectedService
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int? ParentId { get; set; }

            public string LocCode { get; set; }

            public bool IsSchedulable { get; set; }

            public bool HasSchedualbleChildren { get; set; }
        }



        #endregion

    }
}
