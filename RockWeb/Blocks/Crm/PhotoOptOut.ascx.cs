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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Takes the given person (via Person.UrlEncodedKey) and opts them out of future photo requests.
    /// </summary>
    [DisplayName( "Photo Opt-Out" )]
    [Category( "CRM > PhotoRequest" )]
    [Description( "Allows a person to opt-out of future photo requests." )]

    [Rock.SystemGuid.BlockTypeGuid( "14293AEB-B0F5-434B-844A-66592AE3A416" )]
    public partial class PhotoOptOut : Rock.Web.UI.RockBlock
    {
        #region Fields

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( ! Page.IsPostBack )
            {
                OptOutThePerson();
            }

            base.OnLoad( e );
        }


        /// <summary>
        /// Opts the given person out of the Photo Request group making them inactive.
        /// </summary>
        private void OptOutThePerson()
        {
            RockContext rockContext = new RockContext();
            Person targetPerson = null;

            string personKey = PageParameter( "Person" );
            if ( personKey != null )
            {
                try
                {
                    targetPerson = new PersonService( rockContext ).GetByPersonActionIdentifier( personKey, "OptOut" );
                    if ( targetPerson == null )
                    {
                        targetPerson = new PersonService( rockContext ).GetByUrlEncodedKey( personKey );
                    }
                }
                catch ( System.FormatException )
                {
                    nbWarning.Visible = true;
                }
                catch ( Exception ex )
                {
                    nbWarning.Visible = true;
                    LogException( ex );
                }
            }

            if ( targetPerson != null )
            {
                try
                {
                    GroupService service = new GroupService( rockContext );
                    Group photoRequestGroup = service.GetByGuid( Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid() );
                    var groupMember = photoRequestGroup.Members.Where( m => m.PersonId == targetPerson.Id ).FirstOrDefault();
                    if ( groupMember == null )
                    {
                        groupMember = new GroupMember();
                        groupMember.GroupId = photoRequestGroup.Id;
                        groupMember.PersonId = targetPerson.Id;
                        groupMember.GroupRoleId = photoRequestGroup.GroupType.DefaultGroupRoleId ?? 0;
                        photoRequestGroup.Members.Add( groupMember );
                    }

                    groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;

                    rockContext.SaveChanges();
                    nbMessage.Visible = true;
                }
                catch ( Exception ex )
                {
                    LogException( ex );
                    nbMessage.Visible = true;
                    nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbMessage.Text = "Something went wrong and we could not save your request. If it happens again please contact our office at the number below.";
                }
            }
            else
            {
                nbMessage.Visible = true;
                nbMessage.NotificationBoxType = NotificationBoxType.Info;
                nbMessage.Text = "That's odd. We could not find your record in our system. Please contact our office at the number below.";
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

        }

        #endregion

        #region Methods


        #endregion
    }
}