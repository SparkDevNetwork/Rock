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
using System.ComponentModel;
using System.Data.Entity;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// </summary>
    [DisplayName( "Data View Results" )]
    [Category( "Reporting" )]
    [Description( "Shows the details of the given data view." )]

    [IntegerField(
        "Database Timeout",
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 0 )]

    [BooleanField(
        "Enable Counting Data View Statistics",
        Key = AttributeKey.EnableCountingDataViewStatistics,
        Description = "Set this to false to prevent this block from counting data view statistics",
        DefaultBooleanValue = true,
        Order = 1
        )]
    [Rock.SystemGuid.BlockTypeGuid( "61CDA12E-A19F-4299-AF3E-4F7E2B8F5866" )]
    public partial class DataViewResults : RockBlock, ICustomGridColumns, ISecondaryBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
            public const string EnableCountingDataViewStatistics = "EnableCountingDataViewStatistics";
        }

        #endregion Attribute Keys

        #region View State Keys

        private static class ViewStateKey
        {
            public const string ShowResults = "ShowResults";
        }

        #endregion View State Keys

        #region UserPreference Keys

        private static class UserPreferenceKey
        {
            public const string ShowResults = "data-view-show-results";
        }

        #endregion UserPreference Keys

        #region PageParameterKey

        private static class PageParameterKey
        {
            public const string DataViewId = "DataViewId";
        }

        #endregion PageParameterKey

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gDataViewResults.GridRebind += gDataViewResults_GridRebind;

            //// Set postback timeout and request-timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            int databaseTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
            }

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
            if ( !Page.IsPostBack )
            {
                var dataViewId = this.PageParameter( PageParameterKey.DataViewId ).AsIntegerOrNull();
                hfDataViewId.Value = dataViewId.ToString();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the GridRebind event of the gDataViewResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDataViewResults_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnToggleResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnToggleResults_Click( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();
            var showResults = preferences.GetValue( UserPreferenceKey.ShowResults ).AsBoolean( true );

            preferences.SetValue( UserPreferenceKey.ShowResults, ( !showResults ).ToString() );
            preferences.Save();

            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var dataViewId = hfDataViewId.Value.AsIntegerOrNull();
            pnlResultsGrid.Visible = false;

            pnlView.Visible = false;
            if ( !dataViewId.HasValue )
            {
                return;
            }

            var dataView = DataViewCache.Get( dataViewId.Value );

            if ( dataView == null )
            {
                return;
            }

            if ( !dataView.EntityTypeId.HasValue )
            {
                return;
            }

            var dataViewEntityType = EntityTypeCache.Get( dataView.EntityTypeId.Value );

            if ( dataViewEntityType == null || dataViewEntityType.AssemblyName == null )
            {
                return;
            }

            Type dataViewEntityTypeType = dataViewEntityType.GetEntityType();
            if ( dataViewEntityTypeType == null )
            {
                return;
            }

            pnlView.Visible = true;

            gDataViewResults.DataSource = null;

            // Only respect the ShowResults option if fetchRowCount is null
            var preferences = GetBlockPersonPreferences();
            var showResults = preferences.GetValue( UserPreferenceKey.ShowResults ).AsBooleanOrNull() ?? true;

            if ( showResults )
            {
                btnToggleResults.Text = "Hide Results <i class='fa fa-chevron-up'></i>";
                btnToggleResults.ToolTip = "Hide Results";
                btnToggleResults.RemoveCssClass( "btn-primary" );
                btnToggleResults.AddCssClass( "btn-default" );
            }
            else
            {
                btnToggleResults.Text = "Show Results <i class='fa fa-chevron-down'></i>";
                btnToggleResults.RemoveCssClass( "btn-default" );
                btnToggleResults.AddCssClass( "btn-primary" );
                btnToggleResults.ToolTip = "Show Results";
            }

            if ( !showResults )
            {
                return;
            }

            if ( !dataView.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                return;
            }

            gDataViewResults.EntityTypeId = dataView.EntityTypeId;
            bool isPersonDataSet = dataView.EntityTypeId == EntityTypeCache.GetId<Rock.Model.Person>();

            if ( isPersonDataSet )
            {
                gDataViewResults.PersonIdField = "Id";
                gDataViewResults.DataKeyNames = new string[] { "Id" };
            }
            else
            {
                gDataViewResults.PersonIdField = null;
            }

            var entityTypeCache = EntityTypeCache.Get( dataView.EntityTypeId.Value );
            if ( entityTypeCache != null )
            {
                gDataViewResults.RowItemText = entityTypeCache.FriendlyName;
            }

            pnlResultsGrid.Visible = true;

            var enableCountingDataViewStatistics = this.GetAttributeValue( AttributeKey.EnableCountingDataViewStatistics ).AsBooleanOrNull() ?? true;

            try
            {
                gDataViewResults.CreatePreviewColumns( dataViewEntityTypeType );
                var dataViewGetQueryArgs = new GetQueryableOptions
                {
                    SortProperty = gDataViewResults.SortProperty,
                    DatabaseTimeoutSeconds = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180,
                    DataViewFilterOverrides = new DataViewFilterOverrides
                    {
                        ShouldUpdateStatics = enableCountingDataViewStatistics
                    }
                };

                var qry = dataView.GetQuery( dataViewGetQueryArgs );

                gDataViewResults.SetLinqDataSource( qry.AsNoTracking() );
                gDataViewResults.DataBind();
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );
                var errorBox = nbGridError;

                if ( sqlTimeoutException != null )
                {
                    errorBox.NotificationBoxType = NotificationBoxType.Warning;
                    errorBox.Text = "This data view did not complete in a timely manner. You can try again or adjust the timeout setting of this block.";
                    return;
                }
                else
                {
                    if ( ex is RockDataViewFilterExpressionException )
                    {
                        RockDataViewFilterExpressionException rockDataViewFilterExpressionException = ex as RockDataViewFilterExpressionException;
                        errorBox.Text = rockDataViewFilterExpressionException.GetFriendlyMessage( dataView );
                    }
                    else
                    {
                        errorBox.Text = "There was a problem with one of the filters for this data view.";
                    }

                    errorBox.NotificationBoxType = NotificationBoxType.Danger;

                    errorBox.Details = ex.Message;
                    errorBox.Visible = true;
                    return;
                }
            }

            gDataViewResults.RowItemText = dataViewEntityType.FriendlyName;

            if ( gDataViewResults.DataSource != null )
            {
                gDataViewResults.ExportFilename = dataView.Name;
            }
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlView.Visible = visible;
        }

        #endregion
    }
}