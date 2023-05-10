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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Security Change Audit List
    /// </summary>
    [DisplayName( "Security Change Audit List" )]
    [Category( "Security" )]
    [Description( "Block for Security Change Audit List." )]
    [Rock.SystemGuid.BlockTypeGuid( "9F577C39-19FB-4C33-804B-35023284B856" )]
    public partial class SecurityChangeAuditList : RockBlock
    {
        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            public const string Date = "Date";
            public const string EntityType = "EntityType";
            public const string EntityId = "EntityId";
            public const string ChangedBy = "ChangedBy";
        }

        #endregion UserPreferanceKeys

        private static class LabelTypeString
        {
            public const string Success = "success";
            public const string Danger = "danger";
            public const string Info = "info";
        }

        private static class PermissionTypeString
        {
            public const string Allow = "Allow";
            public const string Deny = "Deny";
        }

        private const string RIGHT_ARROW_INDICATOR = " &rarr; ";
        private const string LABEL_MARKUP = @"<span class='label label-{1}'>{0}</span>";

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            base.OnInit( e );

            gfFiler.ApplyFilterClick += gfFiler_ApplyFilterClick;
            gfFiler.DisplayFilterValue += gfFiler_DisplayFilterValue;
            gfFiler.ClearFilterClick += gfFiler_ClearFilterClick;
            gSecurityChangeAudit.DataKeyNames = new string[] { "Id" };
            gSecurityChangeAudit.IsDeleteEnabled = false;
            gSecurityChangeAudit.GridRebind += gSecurityChangeAudit_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Gfs the settings_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfFiler_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case UserPreferenceKey.EntityType:
                    {
                        var entityTypeId = e.Value.AsIntegerOrNull();
                        if ( entityTypeId.HasValue && entityTypeId.Value != default( int ) )
                        {
                            e.Value = EntityTypeCache.Get( entityTypeId.Value ).FriendlyName;
                        }

                        break;
                    }

                case UserPreferenceKey.Date:
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );

                        break;
                    }

                case UserPreferenceKey.ChangedBy:
                    {
                        var rockContext = new RockContext();
                        int? personId = e.Value.AsIntegerOrNull();
                        e.Value = null;
                        if ( personId.HasValue )
                        {
                            var person = new PersonService( rockContext ).Get( personId.Value );
                            if ( person != null )
                            {
                                e.Value = person.ToString();
                            }
                        }

                        break;
                    }

                case UserPreferenceKey.EntityId:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFiler control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFiler_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFiler.SetFilterPreference( UserPreferenceKey.EntityType, etpEntityType.SelectedValue );
            gfFiler.SetFilterPreference( UserPreferenceKey.Date, drpDate.DelimitedValues );
            gfFiler.SetFilterPreference( UserPreferenceKey.EntityId, nbEntityId.Text );
            gfFiler.SetFilterPreference( UserPreferenceKey.ChangedBy, ppChangedBy.PersonId.ToStringSafe() );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfFiler control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFiler_ClearFilterClick( object sender, EventArgs e )
        {
            gfFiler.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSecurityChangeAudit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gSecurityChangeAudit_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the UserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindFilter();
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var rockContext = new RockContext();
            var entityTypes = new EntityTypeService( rockContext ).GetEntities()
                .OrderBy( t => t.FriendlyName )
                .ToList();
            etpEntityType.EntityTypes = entityTypes;
            etpEntityType.SelectedValue = gfFiler.GetFilterPreference( UserPreferenceKey.EntityType );
            drpDate.DelimitedValues = gfFiler.GetFilterPreference( UserPreferenceKey.Date );
            nbEntityId.Text = gfFiler.GetFilterPreference( UserPreferenceKey.EntityId );
            int? personId = gfFiler.GetFilterPreference( UserPreferenceKey.ChangedBy ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                ppChangedBy.SetValue( new PersonService( rockContext ).Get( personId.Value ) );
            }
            else
            {
                ppChangedBy.SetValue( null );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var authAuditLogQry = new AuthAuditLogService( rockContext ).Queryable();

            // Filter by Date Range
            if ( drpDate.LowerValue.HasValue )
            {
                authAuditLogQry = authAuditLogQry.Where( t => t.ChangeDateTime >= drpDate.LowerValue.Value );
            }

            if ( drpDate.UpperValue.HasValue )
            {
                DateTime upperDate = drpDate.UpperValue.Value.Date.AddDays( 1 );
                authAuditLogQry = authAuditLogQry.Where( t => t.ChangeDateTime < upperDate );
            }

            var entityTypeId = etpEntityType.SelectedEntityTypeId;
            if ( entityTypeId.HasValue )
            {
                authAuditLogQry = authAuditLogQry.Where( a => a.EntityTypeId == entityTypeId );
            }

            var entityId = nbEntityId.IntegerValue;
            if ( entityId.HasValue )
            {
                authAuditLogQry = authAuditLogQry.Where( a => a.EntityId == entityId );
            }

            var personId = ppChangedBy.PersonId;
            if ( personId.HasValue )
            {
                authAuditLogQry = authAuditLogQry.Where( a => a.ChangeByPersonAlias != null && a.ChangeByPersonAlias.PersonId == personId );
            }

            // Sort
            SortProperty sortProperty = gSecurityChangeAudit.SortProperty;
            if ( sortProperty != null )
            {
                authAuditLogQry = authAuditLogQry.Sort( sortProperty );
            }
            else
            {
                authAuditLogQry = authAuditLogQry.OrderByDescending( p => p.ChangeDateTime );
            }

            gSecurityChangeAudit.EntityTypeId = EntityTypeCache.Get<AuthAuditLog>().Id;
            gSecurityChangeAudit.DataSource = authAuditLogQry.ToList();
            gSecurityChangeAudit.DataBind();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gSecurityChangeAudit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gSecurityChangeAudit_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var authAuditLog = e.Row.DataItem as AuthAuditLog;
            var lAccess = e.Row.FindControl( "lAccess" ) as Literal;
            var lOrder = e.Row.FindControl( "lOrder" ) as Literal;
            var lChange = e.Row.FindControl( "lChange" ) as Literal;
            if ( authAuditLog != null )
            {
                var changeLabelStr = LabelTypeString.Success;
                if ( authAuditLog.ChangeType == ChangeType.Delete )
                {
                    changeLabelStr = LabelTypeString.Danger;
                }
                else if ( authAuditLog.ChangeType == ChangeType.Modify )
                {
                    changeLabelStr = LabelTypeString.Info;
                }

                lChange.Text = string.Format( LABEL_MARKUP, authAuditLog.ChangeType, changeLabelStr );

                if ( authAuditLog.PreAllowOrDeny != authAuditLog.PostAllowOrDeny )
                {
                    string accessStr = GetAccessString( authAuditLog.PreAllowOrDeny );
                    accessStr += RIGHT_ARROW_INDICATOR;
                    accessStr += GetAccessString( authAuditLog.PostAllowOrDeny );
                    lAccess.Text = accessStr;
                }

                if ( authAuditLog.PreOrder != authAuditLog.PostOrder )
                {
                    var orderStr = authAuditLog.PreOrder.HasValue ? authAuditLog.PreOrder.ToString() : string.Empty;
                    orderStr += RIGHT_ARROW_INDICATOR;
                    orderStr += authAuditLog.PostOrder.HasValue ? authAuditLog.PostOrder.ToString() : string.Empty;
                    lOrder.Text = orderStr;
                }
            }
        }

        /// <summary>
        /// Get the access string.
        /// </summary>
        private string GetAccessString( string allowOrDeny )
        {
            string accessStr;
            if ( allowOrDeny.IsNullOrWhiteSpace() )
            {
                accessStr = string.Empty;
            }
            else
            {
                accessStr = allowOrDeny == "A" ? PermissionTypeString.Allow : PermissionTypeString.Deny;
            }

            return accessStr;
        }

        #endregion
    }
}