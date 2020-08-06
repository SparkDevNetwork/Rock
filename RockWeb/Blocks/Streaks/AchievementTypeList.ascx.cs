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
using Rock.Achievement;

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
            public const string AchievementTypeId = "AchievementTypeId";
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

            gAchievements.DataKeyNames = new string[] { "Id" };
            gAchievements.Actions.ShowAdd = !GetAttributeValue( AttributeKey.DetailPage ).IsNullOrWhiteSpace();
            gAchievements.Actions.AddClick += gAchievements_Add;
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
            var newParameters = new Dictionary<string, string>();
            var currentParameters = QueryParameters();

            foreach ( var kvp in currentParameters )
            {
                newParameters[kvp.Key] = kvp.Value.ToString();
            }

            newParameters[PageParamKey.AchievementTypeId] = default( int ).ToString();
            NavigateToLinkedPage( AttributeKey.DetailPage, newParameters );
        }

        /// <summary>
        /// Handles the Edit event of the gAchievements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAchievements_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParamKey.AchievementTypeId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gAchievements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAchievements_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var achievementTypeService = new AchievementTypeService( rockContext );
            var achievementType = achievementTypeService.Get( e.RowKeyId );

            if ( achievementType != null )
            {
                string errorMessage;

                if ( !achievementTypeService.CanDelete( achievementType, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                achievementTypeService.Delete( achievementType );
                rockContext.SaveChanges();
            }

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
            gAchievements.DataSource = GetGridViewModels();
            gAchievements.DataBind();
        }

        /// <summary>
        /// Gets the achievement types.
        /// </summary>
        /// <returns></returns>
        private List<AchievementTypeCache> GetAchievementTypes()
        {
            if ( _achievementTypes != null )
            {
                return _achievementTypes;
            }

            var filters = new List<KeyValuePair<string, string>>();

            foreach ( string key in Request.QueryString.Keys )
            {
                if ( key.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var value = Request.QueryString[key];
                filters.Add( new KeyValuePair<string, string>( key, value ) );
            }

            _achievementTypes = AchievementTypeCache.All()
            .Where( at => {
                if ( !filters.Any() )
                {
                    return true;
                }

                var component = at.AchievementComponent;
                return component != null && component.IsRelevantToAllFilters( at, filters );
            } )
            .OrderBy( at => at.Id )
            .ToList();

            return _achievementTypes;
        }
        private List<AchievementTypeCache> _achievementTypes = null;

        /// <summary>
        /// Gets the grid view models.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<AchievementTypeViewModel> GetGridViewModels()
        {
            return GetAchievementTypes().Select( at => new AchievementTypeViewModel
            {
                Id = at.Id,
                ComponentName = GetComponentName( at ),
                IconCssClass = at.AchievementIconCssClass,
                SourceName = GetSourceName( at ),
                Name = at.Name,
                IsActive = at.IsActive
            } ).ToList();
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        private string GetComponentName( AchievementTypeCache achievementTypeCache )
        {
            return AchievementContainer.GetComponentName( achievementTypeCache.AchievementEntityType.Name );
        }

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        private string GetSourceName( AchievementTypeCache achievementTypeCache )
        {
            var component = achievementTypeCache.AchievementComponent;

            if ( component != null )
            {
                return component.GetSourceName( achievementTypeCache );
            }

            return string.Empty;
        }

        #endregion

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
            /// Gets or sets the name of the source.
            /// </summary>
            public string SourceName { get; set; }

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