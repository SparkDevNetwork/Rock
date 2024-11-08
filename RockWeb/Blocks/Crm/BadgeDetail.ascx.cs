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
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using System.Linq;
using System.Data.Entity;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Badge Detail" )]
    [Category( "CRM" )]
    [Description( "Shows the details of a particular badge." )]

    [Rock.SystemGuid.BlockTypeGuid( "A79336CD-2265-4E36-B915-CF49956FD689" )]
    public partial class BadgeDetail : RockBlock
    {

        #region Properties

        public int BadgeId
        {
            get { return ViewState["BadgeId"] as int? ?? 0; }
            set { ViewState["BadgeId"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindEntityTypes();
                lActionTitle.Text = ActionTitle.Add( Badge.FriendlyTypeName ).FormatAsHtmlTitle();

                BadgeId = PageParameter( "BadgeId" ).AsInteger();
                if ( BadgeId != 0 )
                {
                    var badge = new BadgeService( new RockContext() ).Get( BadgeId );
                    if ( badge != null )
                    {
                        lActionTitle.Text = ActionTitle.Edit( badge.Name ).FormatAsHtmlTitle();

                        tbName.Text = badge.Name;
                        tbDescription.Text = badge.Description;
                        rtbQualifierValue.Text = badge.EntityTypeQualifierValue;
                        rtbQualifierColumn.Text = badge.EntityTypeQualifierColumn;
                        etpEntityType.SelectedEntityTypeId = badge.EntityTypeId;
                        SyncBadgeComponentsWithEntityType();

                        var badgeComponentType = EntityTypeCache.Get( badge.BadgeComponentEntityTypeId );
                        compBadgeType.SelectedValue = badgeComponentType.Guid.ToString().ToUpper();

                        BuildEditControls( badge, true );
                        pdAuditDetails.SetEntity( badge, ResolveRockUrl( "~" ) );
                    }
                }
                else
                {
                    // hide the panel drawer that show created and last modified dates
                    pdAuditDetails.Visible = false;
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( compBadgeType.SelectedValue ) )
                {
                    var badgeType = EntityTypeCache.Get( compBadgeType.SelectedValue.AsGuid() );
                    if ( badgeType != null )
                    {
                        var badge = new Badge { BadgeComponentEntityTypeId = badgeType.Id };
                        BuildEditControls( badge, false );
                    }
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the etpEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void etpEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            SyncBadgeComponentsWithEntityType();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the compBadgeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void compBadgeType_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( compBadgeType.SelectedValue ) )
            {
                var badgeComponentType = EntityTypeCache.Get( compBadgeType.SelectedValue.AsGuid() );
                if ( badgeComponentType != null )
                {
                    var badge = new Badge { BadgeComponentEntityTypeId = badgeComponentType.Id };
                    BuildEditControls( badge, true );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Badge badge = null;
            var rockContext = new RockContext();
            var badgeService = new BadgeService( rockContext );

            if ( BadgeId != 0 )
            {
                badge = badgeService.Get( BadgeId );
            }

            if ( badge == null )
            {
                badge = new Badge();
                badgeService.Add( badge );
            }

            badge.Name = tbName.Text;
            badge.Description = tbDescription.Text;
            badge.EntityTypeQualifierColumn = rtbQualifierColumn.Text;
            badge.EntityTypeQualifierValue = rtbQualifierValue.Text;
            badge.EntityTypeId = etpEntityType.SelectedEntityTypeId;

            if ( !string.IsNullOrWhiteSpace( compBadgeType.SelectedValue ) )
            {
                var badgeType = EntityTypeCache.Get( compBadgeType.SelectedValue.AsGuid() );
                if ( badgeType != null )
                {
                    badge.BadgeComponentEntityTypeId = badgeType.Id;
                }
            }

            badge.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, badge );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !badge.IsValid )
            {
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                badge.SaveAttributeValues( rockContext );
            } );

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Synchronizes the available badge components with the selected entity type.
        /// </summary>
        private void SyncBadgeComponentsWithEntityType()
        {
            var entityTypeId = etpEntityType.SelectedEntityTypeId;

            if ( entityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( entityTypeId.Value );

                if ( entityType != null )
                {
                    var type = entityType.GetEntityType();

                    if ( type != null )
                    {
                        var typeName = type.FullName;
                        compBadgeType.AppliesToEntityType = typeName;
                        return;
                    }
                }
            }

            compBadgeType.AppliesToEntityType = string.Empty;
        }

        /// <summary>
        /// Builds the edit controls.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildEditControls( Badge badge, bool setValues )
        {
            badge.LoadAttributes();
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( badge, phAttributes, setValues, BlockValidationGroup, new List<string> { "Active", "Order" } );
        }

        /// <summary>
        /// Provide the options for the entity type picker
        /// </summary>
        private void BindEntityTypes()
        {
            var rockContext = new RockContext();
            var entityTypes = new EntityTypeService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( e => e.IsEntity )
                .OrderBy( t => t.FriendlyName )
                .ToList();

            etpEntityType.EntityTypes = entityTypes;
        }

        #endregion        
    }
}