// <copyright>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Campus Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular campus." )]
    [Rock.SystemGuid.BlockTypeGuid( "E30354A1-A1B8-4BE5-ADCE-43EEDDEF6C65" )]
    public partial class CampusDetail : RockBlock
    {
        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
        }

        #endregion PageParameterKeys

        #region Fields

        private static readonly string _readOnlyIncludes = "LeaderPersonAlias,LeaderPersonAlias.Person,Location,TeamGroup";

        private List<CampusScheduleDTO> CampusSchedulesState { get; set; }

        private List<CampusTopicDTO> CampusTopicsState { get; set; }

        #endregion

        #region Properties

        private int HiddenCampusId
        {
            get
            {
                int campusId;
                int.TryParse( hfCampusId.Value, out campusId );

                return campusId;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            // NOTE: These things are converted to JSON prior to going into ViewState, so the json variable could be null or the string "null"!
            string json = ViewState["CampusSchedulesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                CampusSchedulesState = new List<CampusScheduleDTO>();
            }
            else
            {
                CampusSchedulesState = JsonConvert.DeserializeObject<List<CampusScheduleDTO>>( json );
            }

            LoadCampusTopicsState();
        }

        private void LoadCampusTopicsState()
        {
            string json = ViewState["CampusTopicsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                CampusTopicsState = new List<CampusTopicDTO>();
            }
            else
            {
                CampusTopicsState = JsonConvert.DeserializeObject<List<CampusTopicDTO>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Campus.FriendlyTypeName );

            gCampusSchedules.DataKeyNames = new string[] { "Guid" };
            gCampusSchedules.Actions.ShowAdd = true;
            gCampusSchedules.Actions.AddClick += gCampusSchedules_Add;
            gCampusSchedules.GridRebind += gCampusSchedules_GridRebind;

            gCampusTopics.DataKeyNames = new string[] { "Guid" };
            gCampusTopics.Actions.ShowAdd = true;
            gCampusTopics.Actions.AddClick += gCampusTopics_Add;
            gCampusTopics.GridRebind += GCampusTopics_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.CampusId ).AsInteger() );
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["CampusSchedulesState"] = JsonConvert.SerializeObject( CampusSchedulesState, Formatting.None, jsonSetting );
            ViewState["CampusTopicsState"] = JsonConvert.SerializeObject( CampusTopicsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion Control Methods

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var campus = GetUntrackedCampus( HiddenCampusId );
            ShowEditDetails( campus );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );
            var campus = new CampusService( rockContext ).Get( HiddenCampusId );

            if ( campus != null )
            {
                // Don't allow deleting the last campus
                if ( !campusService.Queryable().Where( c => c.Id != campus.Id ).Any() )
                {
                    mdDeleteWarning.Show( campus.Name + " is the only campus and cannot be deleted (Rock requires at least one campus).", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !campusService.CanDelete( campus, out errorMessage ))
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                campusService.Delete( campus );
                rockContext.SaveChanges();

                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            
            if ( !ValidateCampus() )
            {
                // Error messaging handled by ValidateCampus
                return;
            }

            if ( !IsFormValid() )
            {
                return;
            }

            Campus campus;
            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );
            var locationService = new LocationService( rockContext );

            int campusId = HiddenCampusId;

            if ( campusId == 0 )
            {
                campus = new Campus();
                campusService.Add( campus );
                var orders = campusService.Queryable()
                    .AsNoTracking()
                    .Select( t => t.Order )
                    .ToList();

                campus.Order = orders.Any() ? orders.Max( t => t ) + 1 : 0;
            }
            else
            {
                campus = campusService.Get( campusId );
            }

            campus.Name = tbCampusName.Text;
            campus.IsActive = cbIsActive.Checked;
            campus.Description = tbDescription.Text;
            campus.CampusStatusValueId = dvpCampusStatus.SelectedValueAsInt();
            campus.CampusTypeValueId = dvpCampusType.SelectedValueAsInt();
            campus.Url = urlCampus.Text;
            campus.PhoneNumber = PhoneNumber.FormattedNumber( pnbPhoneNumber.CountryCode, pnbPhoneNumber.Number, pnbPhoneNumber.CountryCode != PhoneNumber.DefaultCountryCode() );
            campus.LocationId = lpLocation.Location.Id;
            campus.ShortCode = tbCampusCode.Text;
            campus.TimeZoneId = ddlTimeZone.SelectedValue;

            var personService = new PersonService( rockContext );
            var leaderPerson = personService.GetNoTracking( ppCampusLeader.SelectedValue ?? 0 );
            campus.LeaderPersonAliasId = leaderPerson != null ? leaderPerson.PrimaryAliasId : null;

            campus.ServiceTimes = kvlServiceTimes.Value;


            // Remove any CampusSchedules that were removed in the UI
            var selectedSchedules = CampusSchedulesState.Select( s => s.Guid );
            var locationsToRemove = campus.CampusSchedules.Where( s => !selectedSchedules.Contains( s.Guid ) ).ToList();
            CampusScheduleService campusScheduleService = null;
            foreach ( var campusSchedule in locationsToRemove )
            {
                campusScheduleService = campusScheduleService ?? new CampusScheduleService( rockContext );
                campus.CampusSchedules.Remove( campusSchedule );
                campusScheduleService.Delete( campusSchedule );
            }

            // Add/Update any CampusSchedules that were added or changed in the UI.
            foreach ( var campusScheduleState in CampusSchedulesState )
            {
                var campusSchedule = campus.CampusSchedules.Where( s => s.Guid == campusScheduleState.Guid ).FirstOrDefault();
                if ( campusSchedule == null )
                {
                    campusSchedule = new CampusSchedule()
                    {
                        CampusId = campus.Id,
                        ScheduleId = campusScheduleState.ScheduleId,
                        ScheduleTypeValueId = campusScheduleState.ScheduleTypeId,
                        Order = campusScheduleState.Order,
                        Guid = Guid.NewGuid()
                    };
                    campus.CampusSchedules.Add( campusSchedule );
                }
                else
                {
                    campusSchedule.ScheduleId = campusScheduleState.ScheduleId;
                    campusSchedule.ScheduleTypeValueId = campusScheduleState.ScheduleTypeId;
                }
            }

            SaveCampusTopics(campus, rockContext);

            avcAttributes.GetEditValues( campus );

            if ( !campus.IsValid && campus.Location.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                campus.SaveAttributeValues( rockContext );
            } );

            NavigateToCurrentPage( new Dictionary<string, string> { { "CampusId", campus.Id.ToString() } } );
        }

        private void SaveCampusTopics(Campus campus, RockContext rockContext )
        {
            // Remove any CampusTopics that were removed in the UI
            var selectedTopics = CampusTopicsState.Select( s => s.Guid );
            var topicsToRemove = campus.CampusTopics.Where( s => !selectedTopics.Contains( s.Guid ) ).ToList();
            foreach ( var campusTopic in topicsToRemove )
            {
                CampusTopicService campusTopicService = new CampusTopicService( rockContext );
                campus.CampusTopics.Remove( campusTopic );
                campusTopicService.Delete( campusTopic );
            }

            // Add/Update any CampusTopics that were added or changed in the UI.
            foreach ( var campusTopicState in CampusTopicsState )
            {
                var campusTopic = campus.CampusTopics.FirstOrDefault( s => s.Guid == campusTopicState.Guid );
                if ( campusTopic == null )
                {
                    campusTopic = new CampusTopic()
                    {
                        CampusId = campus.Id,
                        Email = campusTopicState.Email,
                        IsPublic = campusTopicState.IsPublic,
                        TopicTypeValueId = campusTopicState.TopicTypeId,
                        Guid = Guid.NewGuid()
                    };

                    campus.CampusTopics.Add( campusTopic );
                }
                else
                {
                    campusTopic.Email = campusTopicState.Email;
                    campusTopic.TopicTypeValueId = campusTopicState.TopicTypeId;
                    campusTopic.IsPublic = campusTopicState.IsPublic;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( HiddenCampusId == 0 )
            {
                NavigateToParentPage();
            }
            else
            {
                var campus = GetUntrackedCampus( HiddenCampusId, _readOnlyIncludes );
                ShowReadOnlyDetails( campus );
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            ddlTimeZone.Items.Clear();
            ddlTimeZone.Items.Add( new ListItem() );

            foreach ( TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones() )
            {
                ddlTimeZone.Items.Add( new ListItem( timeZone.DisplayName, timeZone.Id ) );
            }

            ddlTimeZone.Visible = SystemSettings.GetValue( Rock.SystemKey.SystemSetting.ENABLE_MULTI_TIME_ZONE_SUPPORT ).AsBoolean();
            dvpCampusStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CAMPUS_STATUS.AsGuid() ).Id;
            dvpCampusType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid() ).Id;
            dvpScheduleType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHEDULE_TYPE.AsGuid() ).Id;
            dvpTopicType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.TOPIC_TYPE.AsGuid() ).Id;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="campusId">The <see cref="Campus"/> identifier.</param>
        public void ShowDetail( int campusId )
        {
            pnlDetails.Visible = false;

            Campus campus = null;

            if ( !campusId.Equals( 0 ) )
            {
                campus = GetUntrackedCampus( campusId, _readOnlyIncludes );

                pdAuditDetails.SetEntity( campus, ResolveRockUrl( "~" ) );
            }

            if ( campus == null )
            {
                campus = new Campus { Id = 0 };

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            if ( campus.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                pnlDetails.Visible = true;
                hfCampusId.Value = campus.Id.ToString();

                bool readOnly = false;

                if ( !campus.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Campus.FriendlyTypeName );
                }

                if ( campus.IsSystem )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Campus.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    ShowReadOnlyDetails( campus );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;

                    if ( campus.Id > 0 )
                    {
                        ShowReadOnlyDetails( campus );
                    }
                    else
                    {
                        ShowEditDetails( campus );
                    }
                }
            }
        }

        #region CampusSchedule Grid/Dialog Events

        /// <summary>
        /// Handles the Add event of the gCampusSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gCampusSchedules_Add( object sender, EventArgs e )
        {
            ShowCampusScheduleEdit( Guid.Empty );
        }

        private void gCampusTopics_Add( object sender, EventArgs e )
        {
            ShowCampusTopicEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gCampusSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCampusSchedules_Edit( object sender, RowEventArgs e )
        {
            Guid campusScheduleGuid = ( Guid ) e.RowKeyValue;
            ShowCampusScheduleEdit( campusScheduleGuid );
        }

        /// <summary>
        /// Handles the Edit event of the gCampusTopics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCampusTopics_Edit( object sender, RowEventArgs e )
        {
            Guid campusTopicGuid = ( Guid ) e.RowKeyValue;
            ShowCampusTopicEdit( campusTopicGuid );
        }

        /// <summary>
        /// Gets the CampusSchedule.
        /// </summary>
        /// <param name="campusScheduleGuid">The CampusSchedule guid.</param>
        private CampusScheduleDTO GetCampusSchedule( Guid campusScheduleGuid )
        {
            return CampusSchedulesState.Where( s => s.Guid == campusScheduleGuid ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the CampusTopic.
        /// </summary>
        /// <param name="campusScheduleGuid">The CampusSchedule guid.</param>
        private CampusTopicDTO GetCampusTopic( Guid campusScheduleGuid )
        {
            return CampusTopicsState.Find( s => s.Guid == campusScheduleGuid );
        }

        /// <summary>
        /// Handles the Delete event of the gCampusSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCampusSchedules_Delete( object sender, RowEventArgs e )
        {
            Guid campusScheduleGuid = ( Guid ) e.RowKeyValue;
            RemoveScheduleState( campusScheduleGuid );
            BindCampusSchedulesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gCampusTopics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCampusTopics_Delete( object sender, RowEventArgs e )
        {
            Guid campusTopicGuid = ( Guid )e.RowKeyValue;

            var item = CampusTopicsState.Find( a => a.Guid.Equals( campusTopicGuid ) );
            if ( item != null )
            {
                CampusTopicsState.Remove( item );
            }

            BindCampusTopicsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gCampusSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gCampusSchedules_GridRebind( object sender, EventArgs e )
        {
            BindCampusSchedulesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gCampusTopics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void GCampusTopics_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindCampusTopicsGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgSchedule_SaveClick( object sender, EventArgs e )
        {
            var campusScheduleGuid = hfCampusScheduleGuid.Value.AsGuidOrNull();

            CampusScheduleDTO campusSchedule;
            int scheduleId = spCampusSchedule.SelectedValueAsId().Value;
            int scheduleTypeId = dvpScheduleType.SelectedValueAsId().Value;
            Schedule schedule;
            DefinedValue scheduleType;
            using ( var rockContext = new RockContext() )
            {
                schedule = new ScheduleService( rockContext ).Get( scheduleId );
                scheduleType = new DefinedValueService( rockContext ).Get( scheduleTypeId );
            }

            if ( campusScheduleGuid == null || campusScheduleGuid == Guid.Empty)
            {
                campusSchedule = new CampusScheduleDTO()
                {
                    Guid = Guid.NewGuid(),
                    Order = CampusSchedulesState.Any() ? CampusSchedulesState.Max( s => s.Order ) + 1 : 0
                };

                CampusSchedulesState.Add( campusSchedule );
            }
            else
            {
                campusSchedule = CampusSchedulesState.FirstOrDefault( s => s.Guid.Equals( campusScheduleGuid ) );
            }

            campusSchedule.ScheduleId = scheduleId;
            campusSchedule.Schedule = schedule.Name;
            campusSchedule.ScheduleTypeId = scheduleTypeId;
            campusSchedule.ScheduleType = scheduleType.Value;

            dlgSchedule.Hide();
            BindCampusSchedulesGrid();
        }

        protected void dlgTopic_SaveClick( object sender, EventArgs e )
        {
            if ( !cvTopicType.IsValid )
            {
                return;
            }

            var campusTopicGuid = hfCampusTopicGuid.Value.AsGuidOrNull();

            int topicTypeId = dvpTopicType.SelectedDefinedValueId.Value;
            var topicType = new DefinedValueService( new RockContext() ).Get( topicTypeId );
            CampusTopicDTO campusTopic = CampusTopicsState.Find( t => t.TopicTypeId == topicTypeId );

            // If there is an existing campusTopic with the provided topic id just update the existing one, each campus can only
            // have one instance of a topic type
            if ( campusTopic == null )
            {
                if ( campusTopicGuid == null || campusTopicGuid == Guid.Empty )
                {
                    campusTopic = new CampusTopicDTO()
                    {
                        Guid = Guid.NewGuid(),
                        IsPersisted = false
                    };

                    CampusTopicsState.Add( campusTopic );
                }
                else
                {
                    campusTopic = CampusTopicsState.Find( t => t.Guid.Equals( campusTopicGuid ) );
                }
            }

            campusTopic.Email = ebEmail.Text;
            campusTopic.TopicTypeId = topicTypeId;
            campusTopic.IsPublic = cbIsPublic.Checked;
            campusTopic.TopicType = topicType.Value;

            // Clear controls
            cbIsPublic.Checked = false;
            ebEmail.Text = string.Empty;
            dvpTopicType.ClearSelection();

            dlgTopic.Hide();
            BindCampusTopicsGrid();
        }

        /// <summary>
        /// Handles the onServerValidate event of the cvTopicType control.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        protected void cvTopicType_ServerValidate( object source, ServerValidateEventArgs args )
        {
            int? topicTypeId = args.Value.AsIntegerOrNull();
            var topicInstance = CampusTopicsState.Find( t => t.TopicTypeId == topicTypeId );
            args.IsValid = topicInstance == null || topicInstance.IsPersisted;
        }

        #endregion CampusSchedule Grid/Dialog Events

        #endregion Edit Events

        #region Internal Methods

        /// <summary>
        /// Shows the read only details.
        /// </summary>
        /// <param name="campus">The <see cref="Campus"/>.</param>
        private void ShowReadOnlyDetails( Campus campus )
        {
            SetEditMode( false );

            if ( campus == null )
            {
                return;
            }

            lActionTitle.Text = campus.Name.FormatAsHtmlTitle();
            SetStatusLabel( campus );

            lDescription.Text = campus.Description;

            // left column (col-md-6)
            var dl = new DescriptionList();

            if ( campus.CampusStatusValueId.HasValue )
            {
                dl.Add( "Status", DefinedValueCache.GetValue( campus.CampusStatusValueId ) );
            }

            if ( !string.IsNullOrWhiteSpace( campus.ShortCode ) )
            {
                dl.Add( "Code", campus.ShortCode );
            }

            if ( !string.IsNullOrWhiteSpace( campus.TimeZoneId ) )
            {
                dl.Add( "Time Zone", campus.TimeZoneId );
            }
            
            if ( campus.LeaderPersonAlias != null )
            {
                dl.Add( "Campus Leader", campus.LeaderPersonAlias.Person.FullName );
            }

            var serviceTimes = GetReadOnlyServiceTimes( campus );
            if ( !string.IsNullOrWhiteSpace( serviceTimes ) )
            {
                dl.Add( "Service Times", serviceTimes );
            }

            if ( campus.CampusSchedules.Any() )
            {
                var scheduleText = campus.CampusSchedules.Select( s => s.Schedule.Name ).ToList().AsDelimited( ", " );
                dl.Add( "Campus Schedules", scheduleText );
            }

            if ( campus.CampusTopics.Any() )
            {
                dl.Add( "Topics", ConstructReadOnlyTopics( campus ) );
            }

            lMainDetailsLeft.Text = dl.Html;

            // right column (col-md-6)
            dl = new DescriptionList();

            if ( campus.CampusTypeValueId.HasValue )
            {
                dl.Add( "Type", DefinedValueCache.GetValue( campus.CampusTypeValueId ) );
            }

            if ( !string.IsNullOrWhiteSpace( campus.Url ) )
            {
                dl.Add( "URL", campus.Url );
            }

            if ( !string.IsNullOrWhiteSpace( campus.PhoneNumber ) )
            {
                dl.Add( "Phone Number", campus.PhoneNumber );
            }

            if ( campus.Location != null )
            {
                dl.Add( "Location", campus.Location.Name );
            }

            lMainDetailsRight.Text = dl.Html;
        }

        private string ConstructReadOnlyTopics( Campus campus )
        {
            StringBuilder result = new StringBuilder( "<div class='row'>" );

            const string template = @"<div class='col-md-3'>
                                        <span>{0}</span>
                                     </div>
                                     <div class='col-md-6'>
                                        <span>{1}</span>                            
                                     </div>
                                     <div class='col-md-3'> 
                                         <span>{2}</span>
                                     </div>";

            foreach ( var campusTopic in campus.CampusTopics )
            {
                string status = campusTopic.IsPublic ? "Public" : "Internal";
                result.AppendFormat( template, campusTopic.TopicTypeValue.Value, campusTopic.Email, status );
            }

            result.Append( "</div>" );

            return result.ToString();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="campus">The <see cref="Campus"/>.</param>
        private void ShowEditDetails( Campus campus )
        {
            if ( campus == null )
            {
                return;
            }

            if ( campus.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( Campus.FriendlyTypeName ).FormatAsHtmlTitle();
                hlStatus.Visible = false;
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( campus.Name ).FormatAsHtmlTitle();
                SetStatusLabel( campus );
            }

            SetEditMode( true );
            LoadDropDowns();

            tbCampusName.Text = campus.Name;
            cbIsActive.Checked = !campus.IsActive.HasValue || campus.IsActive.Value;
            tbDescription.Text = campus.Description;
            dvpCampusStatus.SetValue( campus.CampusStatusValueId );
            dvpCampusType.SetValue( campus.CampusTypeValueId );
            tbCampusCode.Text = campus.ShortCode;
            urlCampus.Text = campus.Url;

            var countryCodePart = "";
            var numberPart = "";
            var hasCountryCode = PhoneNumber.TryParseNumber( campus.PhoneNumber, out countryCodePart, out numberPart );
            if ( hasCountryCode )
            {
                // Reformat the number according to the country code.
                var formattedNumber = PhoneNumber.FormattedNumber( countryCodePart, numberPart, includeCountryCode: false );
                if ( !string.IsNullOrWhiteSpace( formattedNumber ) )
                {
                    numberPart = formattedNumber;
                }
                pnbPhoneNumber.CountryCode = countryCodePart;
                pnbPhoneNumber.Number = numberPart;
            }
            else
            {
                pnbPhoneNumber.Number = campus.PhoneNumber;
            }

            lpLocation.Location = campus.Location;

            ddlTimeZone.SetValue( campus.TimeZoneId );
            ppCampusLeader.SetValue( campus.LeaderPersonAlias != null ? campus.LeaderPersonAlias.Person : null );
            kvlServiceTimes.Value = campus.ServiceTimes;

            CampusSchedulesState = campus.CampusSchedules.OrderBy( s => s.Order ).ThenBy( s => s.Schedule.Name )
                .Select(
                s => new CampusScheduleDTO
                {
                    Guid = s.Guid,
                    Schedule = s.Schedule.Name,
                    ScheduleId = s.ScheduleId,
                    ScheduleType = s.ScheduleTypeValue.Value,
                    ScheduleTypeId = s.ScheduleTypeValueId.Value,
                    Order = s.Order
                } ).ToList();

            CampusTopicsState = campus.CampusTopics.Select(
                t => new CampusTopicDTO
                {
                    Email = t.Email,
                    Guid = t.Guid,
                    IsPublic = t.IsPublic,
                    TopicType = t.TopicTypeValue.Value,
                    TopicTypeId = t.TopicTypeValueId,
                    IsPersisted = true
                } ).ToList();

            BindCampusSchedulesGrid();
            BindCampusTopicsGrid();

            campus.LoadAttributes();
            avcAttributes.ExcludedAttributes = campus.Attributes.Where( a => !a.Value.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Value ).ToArray();
            avcAttributes.AddEditControls( campus );
        }

        private void BindCampusSchedulesGrid()
        {
            gCampusSchedules.DataSource = CampusSchedulesState
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Schedule )
                .ToList();
            gCampusSchedules.DataBind();

        }

        private void BindCampusTopicsGrid()
        {
            gCampusTopics.DataSource = CampusTopicsState
                .OrderBy( t => t.TopicType )
                .ToList();
            gCampusTopics.DataBind();
        }

        /// <summary>
        /// Gets the <see cref="Campus"/> with related entities to include.
        /// </summary>
        /// <param name="campusId">The <see cref="Campus"/> identifier.</param>
        /// <param name="includes">The comma-separated list of related Entities to include.</param>
        /// <returns>The <see cref="Campus"/> with the specified <paramref name="campusId"/>, along with any specified, related Entities.</returns>
        private Campus GetUntrackedCampus( int campusId, string includes = null )
        {
            return new CampusService( new RockContext() )
                .Queryable( includes )
                .AsNoTracking()
                .Where( c => c.Id == campusId )
                .FirstOrDefault();
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Sets the Active/Inactive status label.
        /// </summary>
        /// <param name="campus">The <see cref="Campus"/>.</param>
        private void SetStatusLabel( Campus campus )
        {
            if ( campus.IsActive == true )
            {
                hlStatus.Text = "Active";
                hlStatus.LabelType = LabelType.Success;
            }
            else
            {
                hlStatus.Text = "Inactive";
                hlStatus.LabelType = LabelType.Danger;
            }
        }

        /// <summary>
        /// Determines whether validatable controls are valid.
        /// </summary>
        private bool IsFormValid()
        {
            // trigger validation checks on relevant controls
            var phoneNumberIsValid = pnbPhoneNumber.IsValid;
            var urlIsValid = urlCampus.IsValid;

            return phoneNumberIsValid && urlIsValid;
        }

        /// <summary>
        /// Gets the service times formatted for read only view.
        /// </summary>
        /// <param name="campus">The campus.</param>
        /// <returns></returns>
        private string GetReadOnlyServiceTimes( Campus campus )
        {
            if ( campus == null || string.IsNullOrWhiteSpace( campus.ServiceTimes ) )
            {
                return null;
            }

            var sbServiceTimes = new StringBuilder();
            string[] serviceTimes = campus.ServiceTimes.Split( '|' );

            for ( int i = 0; i < serviceTimes.Length; i++ )
            {
                string[] dayTimeParts = serviceTimes[i].Split( '^' );
                string day = string.IsNullOrWhiteSpace( dayTimeParts[0] ) ? string.Empty : dayTimeParts[0].Trim();

                string time = null;
                if ( dayTimeParts.Length > 1 )
                {
                    time = string.IsNullOrWhiteSpace( dayTimeParts[1] ) ? string.Empty : dayTimeParts[1].Trim();
                }

                if ( string.IsNullOrWhiteSpace( day ) && string.IsNullOrWhiteSpace( time ))
                {
                    continue;
                }

                if ( i > 0 )
                {
                    sbServiceTimes.Append( "<br>" );
                }

                string space = string.IsNullOrWhiteSpace( day ) || string.IsNullOrWhiteSpace( time ) ? string.Empty : " ";
                sbServiceTimes.AppendFormat( "{0}{1}{2}", day, space, time );
            }

            return sbServiceTimes.ToString();
        }

        #endregion Edit Events

        #region Private Methods

        private bool ValidateCampus()
        {
            var campusLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_CAMPUS.AsGuid() );

            if ( campusLocationType.Id != lpLocation.Location.LocationTypeValueId )
            {
                nbEditModeMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                nbEditModeMessage.Text = string.Format( @"The named location ""{0}"" is not a 'Campus' location type.", lpLocation.Location.Name );
                return false;
            }

            int campusId = int.Parse( hfCampusId.Value );

            var existingCampus = campusId == 0 ?
                CampusCache.All( true ).Where( c => c.Name == tbCampusName.Text ).FirstOrDefault() :
                CampusCache.All( true ).Where( c => c.Name == tbCampusName.Text && c.Id != campusId ).FirstOrDefault();

            if ( existingCampus != null )
            {
                string activeString = existingCampus.IsActive ?? false ? "active" : "inactive";
                nbEditModeMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                nbEditModeMessage.Text = string.Format( @"The campus name ""{0}"" is already in use for an existing {1} campus.", tbCampusName.Text, activeString );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shows the CampusSchedule edit dialog.
        /// </summary>
        /// <param name="campusScheduleGuid">The CampusSchedule guid.</param>
        private void ShowCampusScheduleEdit( Guid campusScheduleGuid )
        {
            hfCampusScheduleGuid.Value = campusScheduleGuid.ToString();
            dvpScheduleType.DefinedTypeId = DefinedTypeCache.Get(Rock.SystemGuid.DefinedType.SCHEDULE_TYPE.AsGuid()).Id;
            dlgSchedule.Show();

            if ( campusScheduleGuid == Guid.Empty )
            {
                var scheduleTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.SCHEDULE_TYPE_WEEKEND_SERVICE.AsGuid() ).Id;
                spCampusSchedule.SetValue( null );
                dvpScheduleType.SelectedValue = scheduleTypeId.ToString();
            }
            else
            {
                var campusSchedule = GetCampusSchedule( campusScheduleGuid );
                spCampusSchedule.SetValue( campusSchedule.ScheduleId );
                dvpScheduleType.SelectedValue = campusSchedule.ScheduleTypeId.ToString();
            }
        }

        /// <summary>
        /// Shows the CampusTopic edit dialog.
        /// </summary>
        /// <param name="campusTopicGuid">The CampusTopic guid.</param>
        private void ShowCampusTopicEdit( Guid campusTopicGuid )
        {
            hfCampusTopicGuid.Value = campusTopicGuid.ToString();
            dvpTopicType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.TOPIC_TYPE.AsGuid() ).Id;
            dlgTopic.Show();

            if ( campusTopicGuid != Guid.Empty )
            {
                var campusTopic = GetCampusTopic( campusTopicGuid );
                dvpTopicType.SelectedValue = campusTopic.TopicTypeId.ToString();
                ebEmail.Text = campusTopic.Email;
                cbIsPublic.Checked = campusTopic.IsPublic;
            }
        }

        /// <summary>
        /// Removes the schedule.
        /// </summary>
        /// <param name="campusScheduleGuid">The CampusSchedule guid.</param>
        private void RemoveScheduleState( Guid campusScheduleGuid )
        {
            var item = CampusSchedulesState.FirstOrDefault( a => a.Guid.Equals( campusScheduleGuid ) );
            if ( item != null )
            {
                CampusSchedulesState.Remove( item );
            }
        }

        #endregion Private Methods

        #region Helper Class

        private class CampusScheduleDTO
        {
            public Guid Guid { get; set; }
            public string Schedule { get; set; }
            public int ScheduleId { get; set; }
            public string ScheduleType { get; set; }
            public int ScheduleTypeId { get; set; }
            public int Order { get; set; }
        }

        private class CampusTopicDTO
        {
            public Guid Guid { get; set; }
            public string TopicType { get; set; }
            public int TopicTypeId { get; set; }
            public string Email { get; set; }
            public bool IsPublic { get; set; }
            public int? CampusId { get; set; }
            public bool IsPersisted { get; set; }
        }

        #endregion Helper Class
    }

}