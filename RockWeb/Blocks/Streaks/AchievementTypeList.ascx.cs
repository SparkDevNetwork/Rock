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
using Rock.Attribute;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;
using System.Collections.Generic;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Achievement Type List" )]
    [Category( "Streaks" )]
    [Description( "Shows a list of all achievement types." )]

    [LinkedPage(
        name: "Detail Page",
        key: AttributeKey.DetailPage )]

    public partial class AchievementTypeList : RockBlock, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The detail page
            /// </summary>
            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Page Param Keys
        /// </summary>
        private static class PageParamKey
        {
            /// <summary>
            /// The streak type achievement type identifier
            /// </summary>
            public const string StreakTypeAchievementTypeId = "StreakTypeAchievementTypeId";

            /// <summary>
            /// The streak type identifier
            /// </summary>
            public const string StreakTypeId = "StreakTypeId";
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

            SetTitlePrefix();

            gAchievements.DataKeyNames = new string[] { "Id" };
            gAchievements.Actions.ShowAdd = !GetAttributeValue( AttributeKey.DetailPage ).IsNullOrWhiteSpace();
            gAchievements.Actions.AddClick += gAchievements_Add;
            gAchievements.GridReorder += gAchievements_GridReorder;
            gAchievements.GridRebind += gAchievements_GridRebind;
            gAchievements.RowItemText = "Achievement Type";

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gAchievements.Actions.ShowAdd = canAddEditDelete;
            gAchievements.IsDeleteEnabled = canAddEditDelete;
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

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gAchievements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAchievements_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, new Dictionary<string, string> {
                { PageParamKey.StreakTypeAchievementTypeId, default(int).ToString() },
                { PageParamKey.StreakTypeId, PageParameter( PageParamKey.StreakTypeId ) }
            } );
        }

        /// <summary>
        /// Handles the Edit event of the gAchievements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAchievements_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParamKey.StreakTypeAchievementTypeId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gAchievements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAchievements_Delete( object sender, RowEventArgs e )
        {
            var rockContext = GetRockContext();
            var streakTypeAchievementTypeService = GetAchievementTypeService();
            var streakTypeAchievementType = streakTypeAchievementTypeService.Get( e.RowKeyId );

            if ( streakTypeAchievementType != null )
            {
                string errorMessage;

                if ( !streakTypeAchievementTypeService.CanDelete( streakTypeAchievementType, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                streakTypeAchievementTypeService.Delete( streakTypeAchievementType );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gAchievements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gAchievements_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = GetRockContext();
            var streakTypeAchievementTypes = GetAchievementTypes();
            GetAchievementTypeService().Reorder( streakTypeAchievementTypes.ToList(), e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAchievements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gAchievements_GridRebind( object sender, EventArgs e )
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
            gAchievements.SetLinqDataSource( GetGridViewModels() );
            gAchievements.DataBind();

            var streakTypeId = PageParameter( PageParamKey.StreakTypeId ).AsIntegerOrNull();

            if ( streakTypeId.HasValue )
            {
                var streakTypeNameCol = gAchievements.ColumnsOfType<RockBoundField>().FirstOrDefault( rbf => rbf.DataField == "StreakTypeName" );

                if ( streakTypeNameCol != null )
                {
                    streakTypeNameCol.Visible = false;
                }
            }
        }

        /// <summary>
        /// Gets the achievement types.
        /// </summary>
        /// <returns></returns>
        private IOrderedQueryable<StreakTypeAchievementType> GetAchievementTypes()
        {
            var streakTypeId = PageParameter( PageParamKey.StreakTypeId ).AsIntegerOrNull();

            return GetAchievementTypeService()
                .Queryable()
                .Where( stat => !streakTypeId.HasValue || stat.StreakTypeId == streakTypeId.Value )
                .OrderBy( stat => stat.Id );
        }

        /// <summary>
        /// Gets the grid view models.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IQueryable<AchievementTypeViewModel> GetGridViewModels()
        {
            return GetAchievementTypes().Select( stat => new AchievementTypeViewModel
            {
                Id = stat.Id,
                ComponentName = stat.AchievementEntityType.FriendlyName,
                IconCssClass = stat.AchievementIconCssClass,
                StreakTypeName = stat.StreakType.Name,
                Name = stat.Name,
                IsActive = stat.IsActive
            } );
        }

        /// <summary>
        /// Gets the achievement types.
        /// </summary>
        /// <returns></returns>
        private void SetTitlePrefix()
        {
            var streakTypeId = PageParameter( PageParamKey.StreakTypeId ).AsIntegerOrNull();
            var streakTypeCache = StreakTypeCache.Get( streakTypeId ?? 0 );
            lTitlePrefix.Text = streakTypeCache == null ? string.Empty : streakTypeCache.Name;
        }

        #endregion        

        #region Data Interface

        /// <summary>
        /// Get the rock context
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
        /// Get the achievement type service
        /// </summary>
        /// <returns></returns>
        private StreakTypeAchievementTypeService GetAchievementTypeService()
        {
            if ( _achievementTypeService == null )
            {
                var rockContext = GetRockContext();
                _achievementTypeService = new StreakTypeAchievementTypeService( rockContext );
            }

            return _achievementTypeService;
        }
        private StreakTypeAchievementTypeService _achievementTypeService = null;

        #endregion Data Interface

        #region View Models

        /// <summary>
        /// View model for the grid line item
        /// </summary>
        private class AchievementTypeViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is active.
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            /// Gets or sets the name of the streak type.
            /// </summary>
            public string StreakTypeName { get; set; }

            /// <summary>
            /// Gets or sets the icon CSS class.
            /// </summary>
            public string IconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the name of the component.
            /// </summary>
            public string ComponentName { get; set; }
        }

        #endregion View Models
    }
}