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
using System.Web.UI;
using Rock;
using Rock.Bus.Queue;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Bus
{
    [DisplayName( "Queue Detail" )]
    [Category( "Bus" )]
    [Description( "Displays the details of the given Queue for editing." )]

    public partial class QueueDetail : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string QueueKey = "QueueKey";
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
            var queue = GetQueue();
            queue.TimeToLiveSeconds = nTimeToLive.IntegerValue;

            IsEditMode = false;
            RenderState();
        }

        /// <summary>
        /// Called by a related block to show the detail for a specific entity.
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
            nbEditModeMessage.Text = string.Empty;

            if ( IsEditMode )
            {
                ShowEditMode();
            }
            else if ( IsViewMode() )
            {
                ShowViewMode();
            }
            else
            {
                nbEditModeMessage.Text = "The queue was not found";
                pnlEditDetails.Visible = false;
                pnlViewDetails.Visible = false;
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

            nTimeToLive.IntegerValue = GetQueue().TimeToLiveSeconds ?? 0;
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

            pnlEditDetails.Visible = false;
            pnlViewDetails.Visible = true;
            HideSecondaryBlocks( false );

            btnEdit.Visible = canEdit;

            var queue = GetQueue();
            lReadOnlyTitle.Text = queue.Name.FormatAsHtmlTitle();

            var descriptionList = new DescriptionList();

            var timeToLiveValue = ( queue.TimeToLiveSeconds.HasValue && queue.TimeToLiveSeconds >= 1 ) ?
                string.Format( "{0} second{1}", queue.TimeToLiveSeconds, queue.TimeToLiveSeconds == 1 ? string.Empty : "s" ) :
                "Indefinite";
            descriptionList.Add( "Time to Live", timeToLiveValue );

            var statLog = queue.StatLog;
            if ( statLog != null )
            {
                if ( statLog.MessagesConsumedLastMinute.HasValue )
                {
                    descriptionList.Add( "Messages Last Minute", statLog.MessagesConsumedLastMinute );
                }

                if ( statLog.MessagesConsumedLastHour.HasValue )
                {
                    descriptionList.Add( "Messages Last Hour", statLog.MessagesConsumedLastHour );
                }

                if ( statLog.MessagesConsumedLastDay.HasValue )
                {
                    descriptionList.Add( "Messages Last Day", statLog.MessagesConsumedLastDay );
                }
            }

            lDescription.Text = descriptionList.Html;
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Can the user edit the streak type
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetQueue() != null;
        }

        /// <summary>
        /// Is the block currently showing information about a streak type
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return GetQueue() != null && !IsEditMode;
        }

        #endregion State Determining Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the queue
        /// </summary>
        /// <returns></returns>
        private IRockQueue GetQueue()
        {
            if ( _rockQueue == null )
            {
                var queueKey = PageParameter( PageParameterKey.QueueKey );

                if ( queueKey.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                _rockQueue = Rock.Bus.Queue.RockQueue.Get( queueKey );
            }

            return _rockQueue;
        }
        private IRockQueue _rockQueue = null;

        #endregion Data Interface Methods
    }
}