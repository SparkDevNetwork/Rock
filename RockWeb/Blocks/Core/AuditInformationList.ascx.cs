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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// An audit log of all the entities and their properties that have been added, updated, or deleted.
    /// </summary>
    [DisplayName( "Audit Information List" )]
    [Category( "Core" )]
    [Description( "An audit log of all the entities and their properties that have been added, updated, or deleted." )]

    public partial class AuditInformationList : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            etpEntityTypeFilter.EntityTypes = new EntityTypeService( new RockContext() ).GetEntities().OrderBy( t => t.FriendlyName ).ToList();

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gAuditInformationList.DataKeyNames = new string[] { "Id" };
            gAuditInformationList.GridRebind += gAuditInformationList_GridRebind;
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Entity Type", etpEntityTypeFilter.SelectedValue );
            gfSettings.SaveUserPreference( "Entity Id", tbEntityIdFilter.Text );
            int? personId = ppWhoFilter.PersonId;
            gfSettings.SaveUserPreference( "Who", personId.HasValue ?  personId.ToString() : string.Empty );

            BindGrid();
        }

        /// <summary>
        /// Gs the audit information list filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfSettings_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Entity Type":
                    {
                        if ( e.Value != "" )
                        {
                            var entityType = Rock.Web.Cache.EntityTypeCache.Read( int.Parse( e.Value ) );
                            if ( entityType != null )
                            {
                                e.Value = entityType.FriendlyName;
                            }
                        }
                        break;
                    }
                case "Who":
                    {
                        int personId = int.MinValue;
                        if (int.TryParse(e.Value, out personId))
                        {
                            var person = new PersonService( new RockContext() ).Get( personId );
                            if (person != null)
                            {
                                e.Value = person.FullName;
                            }
                        }
                        break;
                    }
                case "Entity Id":
                    {
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gAuditInformationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gAuditInformationList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gAuditInformationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAuditInformationList_Select( object sender, RowEventArgs e )
        {
            BindProperties(e.RowKeyId);
            mdProperties.Show();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            etpEntityTypeFilter.SelectedValue = gfSettings.GetUserPreference( "Entity Type" );
            tbEntityIdFilter.Text = gfSettings.GetUserPreference( "Entity Id" );
            int personId = int.MinValue;
            if ( int.TryParse(  gfSettings.GetUserPreference( "Who" ), out personId ) )
            {
                var person = new PersonService( new RockContext() ).Get( personId );
                if ( person != null )
                {
                    ppWhoFilter.SetValue( person );
                }
                else
                {
                    gfSettings.SaveUserPreference( "Who", string.Empty );
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var qry = new AuditService( new RockContext() ).Queryable( "PersonAlias.Person" );

            int entityTypeId = int.MinValue;
            if (int.TryParse( gfSettings.GetUserPreference( "Entity Type" ), out entityTypeId))
            {
                qry = qry.Where( a => a.EntityTypeId == entityTypeId );
            }

            int entityId = int.MinValue;
            if (int.TryParse( gfSettings.GetUserPreference( "Entity Id" ), out entityId))
            {
                qry = qry.Where( a => a.EntityId == entityId );
            }

            int personId = int.MinValue;
            if ( int.TryParse( gfSettings.GetUserPreference( "Who" ), out personId ) )
            {
                qry = qry.Where( a => a.PersonAlias != null && a.PersonAlias.PersonId == personId );
            }

            int? nullInt = null;
            var queryable = qry.Select( a =>
                new
                {
                    a.Id,
                    a.DateTime,
                    a.AuditType,
                    EntityType = a.EntityType.FriendlyName,
                    a.EntityId,
                    a.Title,
                    Properties = a.Details.Count(),
                    PersonId = a.PersonAlias != null ? a.PersonAlias.PersonId : nullInt,
                    PersonName = (a.PersonAlias != null && a.PersonAlias.Person != null) ? 
                        a.PersonAlias.Person.NickName + " " + a.PersonAlias.Person.LastName + ( a.PersonAlias.Person.SuffixValueId.HasValue ? " " + a.PersonAlias.Person.SuffixValue.Value : "" ) :
                        ""
                } );


            SortProperty sortProperty = gAuditInformationList.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderByDescending( q => q.Id );
            }

            gAuditInformationList.EntityTypeId = EntityTypeCache.Read<Rock.Model.Audit>().Id;
            gAuditInformationList.DataSource = queryable.ToList();
            gAuditInformationList.DataBind();
        }

        /// <summary>
        /// Binds the properties.
        /// </summary>
        /// <param name="auditId">The audit identifier.</param>
        private void BindProperties (int auditId)
        {
            gProperties.DataSource = new AuditDetailService( new RockContext() ).Queryable()
                .Where( d => d.AuditId == auditId )
                .OrderBy( d => d.Property ).ToList();
            gProperties.DataBind();
        }

        #endregion
    }
}