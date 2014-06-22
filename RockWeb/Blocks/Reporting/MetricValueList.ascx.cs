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
using System.Reflection;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Metric Value List" )]
    [Category( "Reporting" )]
    [Description( "Displays a list of metric values." )]

    [LinkedPage( "Detail Page" )]
    public partial class MetricValueList : RockBlock, ISecondaryBlock
    {
        #region fields

        private Dictionary<int, string> _entityNameLookup;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMetricValues.DataKeyNames = new string[] { "id" };
            gMetricValues.Actions.AddClick += gMetricValues_Add;
            gMetricValues.GridRebind += gMetricValues_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gMetricValues.Actions.ShowAdd = canAddEditDelete;
            gMetricValues.IsDeleteEnabled = canAddEditDelete;
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

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMetricValues_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "MetricValueId", 0, "MetricCategoryId", hfMetricCategoryId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Edit event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMetricValues_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "MetricValueId", e.RowKeyId, "MetricCategoryId", hfMetricCategoryId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Delete event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMetricValues_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            MetricValueService metricValueService = new MetricValueService( rockContext );
            MetricValue metricValue = metricValueService.Get( e.RowKeyId );
            if ( metricValue != null )
            {
                string errorMessage;
                if ( !metricValueService.CanDelete( metricValue, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                metricValueService.Delete( metricValue );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMetricValues_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gMetricValues_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.DataItem != null )
            {
                var entityColumn = gMetricValues.Columns.OfType<BoundField>().FirstOrDefault( a => a.DataField == "EntityId" );
                if ( entityColumn.Visible )
                {
                    e.Row.Cells[gMetricValues.Columns.IndexOf( entityColumn )].Text = GetSeriesName( e.Row.DataItem.GetPropertyValue( "EntityId" ) as int? );
                }
            }
        }

        /// <summary>
        /// Gets the name of the series.
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="seriesId">The series identifier.</param>
        /// <returns></returns>
        private string GetSeriesName( int? seriesId )
        {
            if ( _entityNameLookup != null && seriesId.HasValue )
            {
                if ( _entityNameLookup.ContainsKey( seriesId.Value ) )
                {
                    return _entityNameLookup[seriesId.Value];
                }
            }

            return null;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            MetricValueService metricValueService = new MetricValueService( rockContext );
            SortProperty sortProperty = gMetricValues.SortProperty;
            var qry = metricValueService.Queryable();

            // in case called normally
            int? metricId = PageParameter( "MetricId" ).AsIntegerOrNull();

            // in case called from CategoryTreeView
            int? metricCategoryId = PageParameter( "MetricCategoryId" ).AsIntegerOrNull();
            MetricCategory metricCategory = null;
            if ( metricCategoryId.HasValue )
            {
                if ( metricCategoryId.Value > 0 )
                {
                    // editing a metric, but get the metricId from the metricCategory
                    metricCategory = new MetricCategoryService( rockContext ).Get( metricCategoryId.Value );
                    if ( metricCategory != null )
                    {
                        metricId = metricCategory.MetricId;
                    }
                }
                else
                {
                    // adding a new metric. Block will (hopefully) not be shown
                    metricId = 0;
                }
            }

            hfMetricId.Value = metricId.ToString();
            hfMetricCategoryId.Value = metricCategoryId.ToString();

            this.Visible = metricId.HasValue;

            if ( !metricId.HasValue )
            {
                return;
            }

            qry = qry.Where( a => a.MetricId == metricId );

            var entityColumn = gMetricValues.Columns.OfType<BoundField>().FirstOrDefault( a => a.DataField == "EntityId" );
            var metric = new MetricService( rockContext ).Get( metricId ?? 0 );
            entityColumn.Visible = metric != null && metric.EntityTypeId.HasValue;

            if ( metric != null )
            {
                _entityNameLookup = new Dictionary<int, string>();

                var entityTypeCache = EntityTypeCache.Read( metric.EntityTypeId ?? 0 );
                if ( entityTypeCache != null )
                {
                    entityColumn.HeaderText = entityTypeCache.FriendlyName;
                    IQueryable<IEntity> entityQry = null;
                    if ( entityTypeCache.GetEntityType() == typeof( Rock.Model.Group ) )
                    {
                        // special case for Group since there could be a very large number (especially if you include families), so limit to GroupType.ShowInGroupList
                        entityQry = new GroupService( rockContext ).Queryable().Where( a => a.GroupType.ShowInGroupList );
                    }
                    else
                    {
                        Type[] modelType = { entityTypeCache.GetEntityType() };
                        Type genericServiceType = typeof( Rock.Data.Service<> );
                        Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                        var serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;
                        MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                        entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;
                    }

                    if ( entityQry != null )
                    {
                        var entityList = entityQry.ToList();
                        foreach ( var e in entityList )
                        {
                            _entityNameLookup.AddOrReplace( e.Id, e.ToString() );
                        }
                    }
                }
            }

            if ( sortProperty != null )
            {
                gMetricValues.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gMetricValues.DataSource = qry.OrderBy( s => s.Order ).ThenBy( s => s.XValue ).ThenBy( s => s.MetricValueDateTime ).ThenBy( s => s.YValue ).ToList();
            }

            gMetricValues.DataBind();
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        #endregion
    }
}