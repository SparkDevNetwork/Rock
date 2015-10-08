// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.PersonProfile;
using Rock.Security;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Person Badge List" )]
    [Category( "CRM" )]
    [Description( "Shows a list of all person badges." )]

    [LinkedPage("Detail Page")]
    public partial class PersonBadgeList : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
           
            gPersonBadge.DataKeyNames = new string[] { "Id" };
            gPersonBadge.Actions.ShowAdd = true;
            gPersonBadge.Actions.AddClick += gPersonBadge_Add;
            gPersonBadge.GridReorder += gPersonBadge_GridReorder;
            gPersonBadge.GridRebind += gPersonBadge_GridRebind;
            gPersonBadge.RowItemText = "Person Badge";

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPersonBadge.Actions.ShowAdd = canAddEditDelete;
            gPersonBadge.IsDeleteEnabled = canAddEditDelete;

            SecurityField securityField = gPersonBadge.Columns[3] as SecurityField;
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.PersonBadge ) ).Id;
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
        /// Handles the Add event of the gPersonBadge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPersonBadge_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "PersonBadgeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPersonBadge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonBadge_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "PersonBadgeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gPersonBadge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPersonBadge_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            PersonBadgeService personBadgeService = new PersonBadgeService( rockContext );
            PersonBadge personBadge = personBadgeService.Get( e.RowKeyId );

            if ( personBadge != null )
            {
                string errorMessage;
                if ( !personBadgeService.CanDelete( personBadge, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                PersonBadgeCache.Flush( personBadge.Id );

                personBadgeService.Delete( personBadge );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        void gPersonBadge_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new PersonBadgeService( rockContext );
            var personBadges = service.Queryable().OrderBy( b => b.Order );
            service.Reorder( personBadges.ToList(), e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            foreach ( var personBadge in personBadges )
            {
                PersonBadgeCache.Flush( personBadge.Id );
            }

            BindGrid();

        }
        
        /// <summary>
        /// Handles the GridRebind event of the gPersonBadge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gPersonBadge_GridRebind( object sender, EventArgs e )
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
            gPersonBadge.DataSource = new PersonBadgeService( new RockContext() )
                .Queryable().OrderBy( b => b.Order ).ToList();
            gPersonBadge.DataBind();
        }

        #endregion
    }

    class PersonBadgeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int Order { get; set; }

        public PersonBadgeInfo(PersonBadgeCache badge)
        {
            Id = badge.Id;
            Name = badge.Name;
            Description = badge.Description;
            
            badge.LoadAttributes();

            IsActive = badge.BadgeComponent != null ? badge.BadgeComponent.IsActive : false;
            Order = badge.BadgeComponent != null ? badge.BadgeComponent.Order : 0;
        }
    }


}