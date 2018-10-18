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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Campus Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular campus." )]
    public partial class CampusDetail : RockBlock, IDetailBlock
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
                LoadDropDowns();
                ShowDetail( PageParameter( "campusId" ).AsInteger() );
            }
            else
            {
                if ( pnlDetails.Visible )
                {
                    var rockContext = new RockContext();
                    Campus campus;
                    string itemId = PageParameter( "campusId" );
                    if ( !string.IsNullOrWhiteSpace( itemId ) && int.Parse( itemId ) > 0 )
                    {
                        campus = new CampusService( rockContext ).Get( int.Parse( PageParameter( "campusId" ) ) );
                    }
                    else
                    {
                        campus = new Campus { Id = 0 };
                    }
                    campus.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( campus, phAttributes, false, BlockValidationGroup );
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
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Campus campus;
            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );
            var locationService = new LocationService( rockContext );

            int campusId = int.Parse( hfCampusId.Value );

            if ( campusId == 0 )
            {
                campus = new Campus();
                campusService.Add( campus );
                var orders = campusService.Queryable()
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
            campus.Url = tbUrl.Text;

            campus.PhoneNumber = tbPhoneNumber.Text;

            campus.LocationId = lpLocation.Location.Id;

            campus.ShortCode = tbCampusCode.Text;
            campus.TimeZoneId = ddlTimeZone.SelectedValue;

            var personService = new PersonService( rockContext );
            var leaderPerson = personService.Get( ppCampusLeader.SelectedValue ?? 0 );
            campus.LeaderPersonAliasId = leaderPerson != null ? leaderPerson.PrimaryAliasId : null;

            campus.ServiceTimes = kvlServiceTimes.Value;

            campus.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, campus );

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

            NavigateToParentPage();
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
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        public void ShowDetail( int campusId )
        {
            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            Campus campus = null;

            if ( !campusId.Equals( 0 ) )
            {
                campus = new CampusService( new RockContext() ).Get( campusId );
                lActionTitle.Text = ActionTitle.Edit( Campus.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( campus, ResolveRockUrl( "~" ) );
            }

            if ( campus == null )
            {
                campus = new Campus { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( Campus.FriendlyTypeName ).FormatAsHtmlTitle();
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfCampusId.Value = campus.Id.ToString();
            tbCampusName.Text = campus.Name;
            cbIsActive.Checked = !campus.IsActive.HasValue || campus.IsActive.Value;
            tbDescription.Text = campus.Description;
            tbUrl.Text = campus.Url;
            tbPhoneNumber.Text = campus.PhoneNumber;
            lpLocation.Location = campus.Location;

            tbCampusCode.Text = campus.ShortCode;
            ddlTimeZone.SetValue( campus.TimeZoneId );
            ppCampusLeader.SetValue( campus.LeaderPersonAlias != null ? campus.LeaderPersonAlias.Person : null );
            kvlServiceTimes.Value = campus.ServiceTimes;

            campus.LoadAttributes();
            phAttributes.Controls.Clear();
            var excludeForEdit = campus.Attributes.Where( a => !a.Value.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList();
            Rock.Attribute.Helper.AddEditControls( campus, phAttributes, true, BlockValidationGroup, excludeForEdit );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
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
                lActionTitle.Text = ActionTitle.View( Campus.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbCampusName.ReadOnly = readOnly;
            btnSave.Visible = !readOnly;
        }

        #endregion
    }
}