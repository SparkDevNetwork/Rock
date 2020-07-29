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
    public partial class CampusDetail : RockBlock, IDetailBlock
    {
        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
        }

        #endregion PageParameterKeys

        #region Fields

        private static readonly string _readOnlyIncludes = "LeaderPersonAlias,LeaderPersonAlias.Person,Location,TeamGroup";

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
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Campus.FriendlyTypeName );
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
                ShowDetail( PageParameter( PageParameterKey.CampusId ).AsInteger() );
            }
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
            campus.PhoneNumber = pnbPhoneNumber.Number;
            campus.LocationId = lpLocation.Location.Id;
            campus.ShortCode = tbCampusCode.Text;
            campus.TimeZoneId = ddlTimeZone.SelectedValue;

            var personService = new PersonService( rockContext );
            var leaderPerson = personService.GetNoTracking( ppCampusLeader.SelectedValue ?? 0 );
            campus.LeaderPersonAliasId = leaderPerson != null ? leaderPerson.PrimaryAliasId : null;

            campus.ServiceTimes = kvlServiceTimes.Value;

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

        #endregion

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
            
            lMainDetailsLeft.Text = dl.Html;

            // right column (col-md-6)
            dl = new DescriptionList();
            
            if ( campus.CampusTypeValueId.HasValue )
            {
                dl.Add( "Type", DefinedValueCache.GetValue( campus.CampusTypeValueId ) );
            }

            if ( !string.IsNullOrWhiteSpace( campus.Url ) )
            {
                dl.Add( "Url", campus.Url );
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
            pnbPhoneNumber.Number = campus.PhoneNumber;
            lpLocation.Location = campus.Location;

            ddlTimeZone.SetValue( campus.TimeZoneId );
            ppCampusLeader.SetValue( campus.LeaderPersonAlias != null ? campus.LeaderPersonAlias.Person : null );
            kvlServiceTimes.Value = campus.ServiceTimes;

            campus.LoadAttributes();
            avcAttributes.ExcludedAttributes = campus.Attributes.Where( a => !a.Value.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Value ).ToArray();
            avcAttributes.AddEditControls( campus );
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

        #endregion Private Methods

    }
}