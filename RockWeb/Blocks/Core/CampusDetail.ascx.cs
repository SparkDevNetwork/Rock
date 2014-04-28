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
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
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
                string itemId = PageParameter( "campusId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "campusId", int.Parse( itemId ) );
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
            CampusService campusService = new CampusService( rockContext );

            int campusId = int.Parse( hfCampusId.Value );

            if ( campusId == 0 )
            {
                campus = new Campus();
                campusService.Add( campus);
            }
            else
            {
                campus = campusService.Get( campusId );
            }

            campus.Name = tbCampusName.Text;
            campus.ShortCode = tbCampusCode.Text;
            campus.PhoneNumber = tbPhoneNumber.Text;
            campus.LeaderPersonAliasId = ppCampusLeader.PersonId;

            if ( !campus.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            rockContext.SaveChanges();

            Rock.Web.Cache.CampusCache.Flush( campus.Id );

            NavigateToParentPage();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            // return if unexpected itemKey 
            if ( itemKey != "campusId" )
            {
                return;
            }

            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            Campus campus = null;
            if ( !itemKeyValue.Equals( 0 ) )
            {
                campus = new CampusService( new RockContext() ).Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit(Campus.FriendlyTypeName).FormatAsHtmlTitle();
            }
            else
            {
                campus = new Campus { Id = 0 };
                lActionTitle.Text = ActionTitle.Add(Campus.FriendlyTypeName).FormatAsHtmlTitle();
            }

            hfCampusId.Value = campus.Id.ToString();
            tbCampusName.Text = campus.Name;
            tbCampusCode.Text = campus.ShortCode;
            tbPhoneNumber.Text = campus.PhoneNumber;
            if ( campus.LeaderPersonAliasId != null )
            {
                ppCampusLeader.PersonId = campus.LeaderPersonAliasId;
                ppCampusLeader.PersonName = campus.Leader.FullName;
            }

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