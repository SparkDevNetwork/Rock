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
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.WebFarm;

namespace RockWeb.Blocks.Farm
{
    [DisplayName( "Web Farm Node Detail" )]
    [Category( "Farm" )]
    [Description( "Displays the details of the Web Farm Node." )]

    public partial class NodeDetail : RockBlock, IDetailBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string WebFarmNodeId = "WebFarmNodeId";
        }

        #endregion Keys

        #region View State

        /// <summary>
        /// Gets or sets a value indicating whether this instance is edit mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is edit mode; otherwise, <c>false</c>.
        /// </value>
        private bool IsEditMode
        {
            get
            {
                return CanEdit() && ViewState["IsEditMode"].ToStringSafe().AsBoolean();
            }
            set
            {
                ViewState["IsEditMode"] = value;
            }
        }

        #endregion View State

        #region Properties

        /// <summary>
        /// Gets the rock context.
        /// </summary>
        /// <value>
        /// The rock context.
        /// </value>
        private RockContext RockContext
        {
            get
            {
                if ( _rockContext == null )
                {
                    _rockContext = new RockContext();
                }

                return _rockContext;
            }
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Gets the web farm node.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private WebFarmNode WebFarmNode
        {
            get
            {
                if ( _node == null )
                {
                    var nodeId = GetWebFarmNodeId();
                    var service = new WebFarmNodeService( RockContext );
                    _node = service.Get( nodeId );
                }

                return _node;
            }
        }
        private WebFarmNode _node = null;

        #endregion Properties

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            InitializeSettingsNotification();
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
                RenderState();
            }
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification()
        {
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upUpdatePanel );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            IsEditMode = CanEdit();
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            IsEditMode = false;
            RenderState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RenderState();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            // Save settings here

            IsEditMode = false;
            RenderState();
        }

        /// <summary>
        /// This method satisfies the IDetailBlock requirement
        /// </summary>
        /// <param name="unused"></param>
        public void ShowDetail( int unused )
        {
            RenderState();
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        public void RenderState()
        {
            var node = WebFarmNode;

            if ( node == null )
            {
                nbMessage.Text = string.Format( "The node with id {0} was not found.", GetWebFarmNodeId() );
                nbMessage.Title = "Error";
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                return;
            }

            lNodeName.Text = node.NodeName;

            if ( IsEditMode )
            {
                ShowEditMode();
            }
            else if ( IsViewMode() )
            {
                ShowViewMode();
            }
        }

        /// <summary>
        /// Shows the mode where the user can edit an existing queue
        /// </summary>
        private void ShowEditMode()
        {
            if ( !IsEditMode )
            {
                return;
            }

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            // Set edit control values
        }

        /// <summary>
        /// Shows the mode where the user is only viewing an existing streak type
        /// </summary>
        private void ShowViewMode()
        {
            if ( !IsViewMode() )
            {
                return;
            }

            var canEdit = CanEdit();
            btnEdit.Visible = canEdit;

            pnlEditDetails.Visible = false;
            pnlViewDetails.Visible = true;
            HideSecondaryBlocks( false );

            // Bind view controls
        }

        /// <summary>
        /// Gets the web farm node identifier.
        /// </summary>
        /// <returns></returns>
        private int GetWebFarmNodeId()
        {
            return PageParameter( PageParameterKey.WebFarmNodeId ).AsInteger();
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Can the user edit the streak type
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate;
        }

        /// <summary>
        /// Is the block currently showing information about a streak type
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return !IsEditMode;
        }

        #endregion State Determining Methods
    }
}