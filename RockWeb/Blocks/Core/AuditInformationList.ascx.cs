//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
using Rock.Model;
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

            etpEntityTypeFilter.EntityTypes = new EntityTypeService().GetEntities().OrderBy( t => t.FriendlyName ).ToList();

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gAuditInformationList.DataKeyNames = new string[] { "id" };
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
                            var person = new PersonService().Get(personId);
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
                var person = new PersonService().Get( personId );
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
            var qry = new AuditService().Queryable();

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
                qry = qry.Where( a => a.PersonId == personId );
            }
 
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
                    PersonId = a.PersonId,
                    PersonName = a.Person.NickName + " " + a.Person.LastName + ( a.Person.SuffixValueId.HasValue ? " " + a.Person.SuffixValue.Name : "" )
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

            gAuditInformationList.DataSource = queryable.ToList();
            gAuditInformationList.DataBind();
        }

        /// <summary>
        /// Binds the properties.
        /// </summary>
        /// <param name="auditId">The audit identifier.</param>
        private void BindProperties (int auditId)
        {
            gProperties.DataSource = new AuditDetailService().Queryable()
                .Where( d => d.AuditId == auditId )
                .OrderBy( d => d.Property ).ToList();
            gProperties.DataBind();
        }

        #endregion
    }
}