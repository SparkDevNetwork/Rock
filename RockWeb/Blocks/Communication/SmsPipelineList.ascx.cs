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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for managing the system emails
    /// </summary>
    [DisplayName( "SMS Pipeline List" )]
    [Category( "Communication" )]
    [Description( "Lists the SMS Pipelines currently in the system." )]

    [LinkedPage( "SMS Pipeline Detail",
        Key = AttributeKey.DetailPage )]
    [Rock.SystemGuid.BlockTypeGuid( "DB6FD0BF-FDCE-48DA-919C-240F029518A2" )]
    public partial class SmsPipelineList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "SMSPipelineDetail";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string EntityId = "SmsPipelineId";
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

            gSmsPipelines.DataKeyNames = new string[] { "Id" };
            gSmsPipelines.Actions.ShowAdd = true;
            gSmsPipelines.Actions.AddClick += gSmsPipelines_AddClick;
            gSmsPipelines.GridRebind += gSmsPipelines_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the AddClick event of the gSmsPipelines control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSmsPipelines_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.EntityId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSmsPipelines control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSmsPipelines_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.EntityId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSmsPipeline control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSmsPipelines_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var smsPipelineService = new SmsPipelineService( rockContext );
            var smsPipeline = smsPipelineService.Get( e.RowKeyId );
            if ( smsPipeline != null )
            {
                smsPipelineService.Delete( smsPipeline );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSmsPipelines control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSmsPipelines_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var smsPipelineService = new SmsPipelineService( new RockContext() );
            SortProperty sortProperty = gSmsPipelines.SortProperty;

            var smsPipelines = smsPipelineService.Queryable();

            if ( sortProperty != null )
            {
                gSmsPipelines.DataSource = smsPipelines.Sort( sortProperty ).ToList();
            }
            else
            {
                gSmsPipelines.DataSource = smsPipelines.OrderBy( a => a.Name ).ToList();
            }

            gSmsPipelines.EntityTypeId = EntityTypeCache.Get<SmsPipeline>().Id;
            gSmsPipelines.DataBind();
        }

        #endregion
    }
}