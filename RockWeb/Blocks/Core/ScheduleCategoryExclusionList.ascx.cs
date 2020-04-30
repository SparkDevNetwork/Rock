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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;
using System.Data.Entity;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// List of dates that schedules are not active for an entire category.
    /// </summary>
    [DisplayName( "Schedule Category Exclusion List" )]
    [Category( "Core" )]
    [Description( "List of dates that schedules are not active for an entire category." )]

    [CategoryField( "Category",
        Description = "Optional Category to use (if not specified, query will be determined by query string).",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Schedule",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.Category )]

    public partial class ScheduleCategoryExclusionList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string Category = "Category";
        }

        #region Fields

        int? _categoryId = null;
        bool _canConfigure = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var categoryGuid = GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
            if ( categoryGuid.HasValue )
            {
                var category = CategoryCache.Get( categoryGuid.Value );
                if ( category != null )
                {
                    _categoryId = category.Id;
                }
            }

            if ( !_categoryId.HasValue )
            {
                _categoryId = PageParameter( "CategoryId" ).AsIntegerOrNull();
            }

            if ( _categoryId.HasValue )
            {
                _canConfigure = IsUserAuthorized( Authorization.ADMINISTRATE );

                if ( _canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "Id" };
                    rGrid.Actions.ShowAdd = true;

                    rGrid.Actions.AddClick += rGrid_Add;
                    rGrid.GridRebind += rGrid_GridRebind;

                    modalDetails.SaveClick += modalDetails_SaveClick;
                    modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );
                }
                else
                {
                    nbMessage.Text = "You are not authorized to configure this page";
                    nbMessage.Visible = true;
                }
            }
            else
            {
                this.Visible = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _canConfigure )
                {
                    BindGrid();
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfIdValue.Value ) )
                {
                    modalDetails.Show();
                }
            }


            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new ScheduleCategoryExclusionService( rockContext );

            var exclusion = service.Get( e.RowKeyId );
            if ( exclusion != null )
            {
                string errorMessage = string.Empty;
                if ( service.CanDelete( exclusion, out errorMessage ) )
                {
                    int categoryId = exclusion.CategoryId;

                    service.Delete( exclusion );
                    rockContext.SaveChanges();

                    Rock.CheckIn.KioskDevice.Clear();
                }
                else
                {
                    nbMessage.Text = errorMessage;
                    nbMessage.Visible = true;
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( null );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            int? exclusionId = hfIdValue.ValueAsInt();

            var rockContext = new RockContext();
            var service = new ScheduleCategoryExclusionService( rockContext );
            ScheduleCategoryExclusion exclusion = null;

            if ( exclusionId.HasValue )
            {
                exclusion = service.Get( exclusionId.Value );
            }

            if ( exclusion == null )
            {
                exclusion = new ScheduleCategoryExclusion();
                service.Add( exclusion );
            }

            exclusion.CategoryId = _categoryId.Value;
            exclusion.Title = tbTitle.Text;
            if ( drpExclusion.LowerValue.HasValue )
            {
                exclusion.StartDate = drpExclusion.LowerValue.Value.Date;
            }
            if ( drpExclusion.UpperValue.HasValue )
            {
                exclusion.EndDate = drpExclusion.UpperValue.Value.Date.AddDays( 1 ).AddSeconds( -1 );
            }

            if ( exclusion.IsValid )
            {
                rockContext.SaveChanges();

                Rock.CheckIn.KioskDevice.Clear();

                hfIdValue.Value = string.Empty;
                modalDetails.Hide();

                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var service = new ScheduleCategoryExclusionService( rockContext );
            rGrid.DataSource = service.Queryable().AsNoTracking()
                .Where( e => e.CategoryId == _categoryId.Value )
                .OrderByDescending( e => e.StartDate )
                .ToList();
            rGrid.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int? exclusionId )
        {
            ScheduleCategoryExclusion exclusion = null;
            if ( exclusionId.HasValue )
            {
                exclusion = new ScheduleCategoryExclusionService( new RockContext() ).Get( exclusionId.Value );
            }

            if ( exclusion != null )
            {
                tbTitle.Text = exclusion.Title;
                drpExclusion.LowerValue = exclusion.StartDate;
                drpExclusion.UpperValue = exclusion.EndDate;
            }
            else
            {
                tbTitle.Text = string.Empty;
                drpExclusion.DelimitedValues = string.Empty;
            }

            hfIdValue.Value = exclusionId.ToString();

            modalDetails.Show();
        }

        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        #endregion
    }
}