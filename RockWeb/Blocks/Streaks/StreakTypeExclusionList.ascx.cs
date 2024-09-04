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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Streak Type Exclusion List" )]
    [Category( "Streaks" )]
    [Description( "Lists all the exclusions for a streak type." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "4266D56C-EAB9-4D37-BD74-EBAD9233F8F2" )]
    public partial class StreakTypeExclusionList : RockBlock, ISecondaryBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The detail page attribute key
            /// </summary>
            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The streak type id page parameter key
            /// </summary>
            public const string StreakTypeId = "StreakTypeId";

            /// <summary>
            /// The streak type exclusion id page parameter key
            /// </summary>
            public const string StreakTypeExclusionId = "StreakTypeExclusionId";
        }

        #endregion Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            InitializeGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var canView = CanView();
                pnlContent.Visible = canView;

                if ( canView )
                {
                    BindGrid();
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid

        /// <summary>
        /// Handles the Click event of the delete button in the grid
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gExclusions_Delete( object sender, RowEventArgs e )
        {
            var rockContext = GetRockContext();
            var service = GetStreakTypeExclusionService();
            var exclusion = service.Get( e.RowKeyId );

            if ( exclusion != null )
            {
                var errorMessage = string.Empty;
                if ( !service.CanDelete( exclusion, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( exclusion );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gExclusions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gExclusions_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.StreakTypeExclusionId, 0, PageParameterKey.StreakTypeId, GetStreakType().Id );
        }

        /// <summary>
        /// Handles the Edit event of the gExclusions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gExclusions_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.StreakTypeExclusionId, e.RowKeyId );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            return _rockContext;
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Retrieve a singleton exclusion service for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private StreakTypeExclusionService GetStreakTypeExclusionService()
        {
            if ( _streakTypeExclusionService == null )
            {
                var rockContext = GetRockContext();
                _streakTypeExclusionService = new StreakTypeExclusionService( rockContext );
            }

            return _streakTypeExclusionService;
        }
        private StreakTypeExclusionService _streakTypeExclusionService = null;

        /// <summary>
        /// Retrieve a singleton streak type service for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private StreakTypeService GetStreakTypeService()
        {
            if ( _streakTypeService == null )
            {
                var rockContext = GetRockContext();
                _streakTypeService = new StreakTypeService( rockContext );
            }

            return _streakTypeService;
        }
        private StreakTypeService _streakTypeService = null;

        /// <summary>
        /// Retrieve a singleton streak type for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private StreakType GetStreakType()
        {
            if ( _streakType == null )
            {
                var streakTypeId = PageParameter( PageParameterKey.StreakTypeId ).AsIntegerOrNull();

                if ( streakTypeId.HasValue )
                {
                    var service = GetStreakTypeService();
                    _streakType = service.Get( streakTypeId.Value );
                }
            }

            return _streakType;
        }
        private StreakType _streakType = null;

        /// <summary>
        /// Can the current user view the exclusions
        /// </summary>
        /// <returns></returns>
        private bool CanView()
        {
            var streakType = GetStreakType();
            return streakType != null && streakType.IsAuthorized( Authorization.VIEW, CurrentPerson );
        }

        /// <summary>
        /// Initialize the grid
        /// </summary>
        private void InitializeGrid()
        {
            var streakType = GetStreakType();

            gExclusions.DataKeyNames = new string[] { "Id" };
            gExclusions.Actions.AddClick += gExclusions_AddClick;
            gExclusions.RowItemText = "Streak Type Exclusion";
            gExclusions.ShowConfirmDeleteDialog = true;

            var canEditBlock =
                IsUserAuthorized( Authorization.EDIT ) ||
                streakType.IsAuthorized( Authorization.EDIT, CurrentPerson ) ||
                streakType.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson );

            gExclusions.Actions.ShowAdd = canEditBlock;
            gExclusions.IsDeleteEnabled = canEditBlock;
        }

        /// <summary>
        /// Handles the BlockUpdated event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the exclusion grid.
        /// </summary>
        protected void BindGrid( bool isExporting = false, bool isCommunication = false )
        {
            var streakType = GetStreakType();

            if ( streakType == null )
            {
                pnlExclusions.Visible = false;
                return;
            }

            pnlExclusions.Visible = true;
            gExclusions.Visible = true;
            lHeading.Text = string.Format( "{0} Exclusions", streakType.Name );

            var exclusionService = GetStreakTypeExclusionService();

            var query = exclusionService.Queryable()
                .AsNoTracking()
                .Include( soe => soe.Location )
                .Where( soe => soe.StreakTypeId == streakType.Id );

            // Sort the grid
            gExclusions.EntityTypeId = new StreakTypeExclusion().TypeId;
            var sortProperty = gExclusions.SortProperty;

            if ( sortProperty != null )
            {
                query = query.Sort( sortProperty );
            }
            else
            {
                query = query.OrderBy( soe => soe.Location.Name );
            }

            var viewModelQuery = query.Select( soe => new ExclusionViewModel
            {
                Id = soe.Id,
                LocationName = soe.Location == null ? " -- " : soe.Location.Name
            } );

            gExclusions.SetLinqDataSource( viewModelQuery );
            gExclusions.DataBind();
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion

        #region View Models

        /// <summary>
        /// Represents an exclusion for a row in the grid
        /// </summary>
        public class ExclusionViewModel
        {
            public int Id { get; set; }
            public string LocationName { get; set; }
        }

        #endregion View Models
    }
}