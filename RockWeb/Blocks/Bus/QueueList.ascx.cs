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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Bus.Queue;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Bus
{
    [DisplayName( "Queue List" )]
    [Category( "Bus" )]
    [Description( "Shows a list of all message bus queues." )]

    #region Block Attributes

    [LinkedPage(
        "Queue Detail Page",
        Key = AttributeKey.QueueDetailPage,
        Description = "The page where the queue can be configured",
        DefaultValue = Rock.SystemGuid.Page.BUS_QUEUE,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "F9872CD9-EF32-4791-B0A9-1D104250AB18" )]
    public partial class QueueList : RockBlock
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string QueueDetailPage = "QueueDetailPage";
        }

        /// <summary>
        /// Page Param Keys
        /// </summary>
        private static class PageParameterKey
        {
            public const string QueueKey = "QueueKey";
        }

        #endregion Keys

        #region Base Control Methods

        // overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;
            gList.RowItemText = "Queue";

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queueKey = e.RowKeyValue as string;
            NavigateToLinkedPage( AttributeKey.QueueDetailPage, new Dictionary<string, string> {
                { PageParameterKey.QueueKey, queueKey }
            } );
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            gList.DataKeyNames = new string[] { "QueueTypeName" };

            var viewModels = RockQueue.GetQueueTypes()
                .Select( qt =>
                {
                    var queue = RockQueue.Get( qt );

                    var queueType =
                        queue is ISendCommandQueue ? "Command" :
                        queue is IPublishEventQueue ? "Event" :
                        "Unknown";

                    return new QueueViewModel
                    {
                        TimeToLiveSeconds = queue.TimeToLiveSeconds,
                        QueueTypeName = qt.FullName,
                        QueueName = queue.Name,
                        QueueType = queueType,
                        RatePerDay = queue.StatLog.MessagesConsumedLastDay,
                        RatePerHour = queue.StatLog.MessagesConsumedLastHour,
                        RatePerMinute = queue.StatLog.MessagesConsumedLastMinute
                    };
                } )
                .AsQueryable();

            // Sort the query based on the column that was selected to be sorted
            var sortProperty = gList.SortProperty;

            if ( gList.AllowSorting && sortProperty != null )
            {
                viewModels = viewModels.Sort( sortProperty );
            }
            else
            {
                viewModels = viewModels.OrderBy( vm => vm.QueueName );
            }

            gList.DataSource = viewModels.ToList();
            gList.DataBind();
        }

        #endregion Methods

        #region ViewModels

        /// <summary>
        /// Queue View Model
        /// </summary>
        public sealed class QueueViewModel
        {
            /// <summary>
            /// Gets or sets the message TTL.
            /// </summary>
            /// <value>
            /// The message TTL.
            /// </value>
            public int? TimeToLiveSeconds { get; set; }

            /// <summary>
            /// Gets or sets the name of the queue.
            /// </summary>
            /// <value>
            /// The name of the queue.
            /// </value>
            public string QueueName { get; set; }

            /// <summary>
            /// Gets or sets the type of the queue.
            /// </summary>
            /// <value>
            /// The type of the queue.
            /// </value>
            public string QueueType { get; set; }

            /// <summary>
            /// Gets or sets the rate per minute.
            /// </summary>
            /// <value>
            /// The rate per minute.
            /// </value>
            public int? RatePerMinute { get; set; }

            /// <summary>
            /// Gets or sets the rate per hour.
            /// </summary>
            /// <value>
            /// The rate per hour.
            /// </value>
            public int? RatePerHour { get; set; }

            /// <summary>
            /// Gets or sets the rate per day.
            /// </summary>
            /// <value>
            /// The rate per day.
            /// </value>
            public int? RatePerDay { get; set; }

            /// <summary>
            /// Gets or sets the name of the queue type.
            /// </summary>
            /// <value>
            /// The name of the queue type.
            /// </value>
            public string QueueTypeName { get; set; }
        }

        #endregion ViewModels
    }
}