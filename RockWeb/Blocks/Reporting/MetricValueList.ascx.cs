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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field;
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

        private Dictionary<int, IQueryable<IEntity>> _entityTypeEntityLookupQry = null;
        private Dictionary<int, Dictionary<int, string>> _entityTypeEntityNameLookup;

        /// <summary>
        /// Gets the entity type entity preference key.
        /// </summary>
        /// <value>
        /// The entity type entity preference key.
        /// </value>
        private string EntityTypeEntityPreferenceKey
        {
            get
            {
                return string.Format( "EntityTypeEntityForMetric:{0}", hfMetricId.Value );
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfMetricValues.ApplyFilterClick += gfMetricValues_ApplyFilterClick;
            gfMetricValues.DisplayFilterValue += gfMetricValues_DisplayFilterValue;

            gMetricValues.DataKeyNames = new string[] { "Id" };
            gMetricValues.Actions.AddClick += gMetricValues_Add;
            gMetricValues.GridRebind += gMetricValues_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                SetHiddenFieldValues();
                this.Visible = hfMetricId.Value.AsIntegerOrNull().HasValue;
            }

            CreateDynamicControls( hfMetricId.Value.AsIntegerOrNull() );
            CreateEntityValueLookups( hfMetricId.Value.AsIntegerOrNull() );
            SetGridSecurity();

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Filter

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpDates.DelimitedValues = gfMetricValues.GetUserPreference( "Date Range" );

            ddlGoalMeasure.Items.Clear();
            ddlGoalMeasure.Items.Add( new ListItem( string.Empty, string.Empty ) );
            ddlGoalMeasure.Items.Add( new ListItem( MetricValueType.Goal.ConvertToString(), MetricValueType.Goal.ConvertToInt().ToString() ) );
            ddlGoalMeasure.Items.Add( new ListItem( MetricValueType.Measure.ConvertToString(), MetricValueType.Measure.ConvertToInt().ToString() ) );

            ddlGoalMeasure.SelectedValue = gfMetricValues.GetUserPreference( "Goal/Measure" );

            var metric = new MetricService( new RockContext() ).Get( hfMetricId.Value.AsInteger() );

            var entityTypeEntityUserPreference = gfMetricValues.GetUserPreference( this.EntityTypeEntityPreferenceKey ) ?? string.Empty;

            var entityTypeEntityList = entityTypeEntityUserPreference.Split( ',' ).Select( a => a.Split( '|' ) ).Where( a => a.Length == 2 ).Select( a =>
                new
                {
                    EntityTypeId = a[0].AsIntegerOrNull(),
                    EntityId = a[1].AsIntegerOrNull()
                } ).ToList();


            if ( metric != null )
            {
                foreach ( var metricPartition in metric.MetricPartitions )
                {
                    var metricPartitionEntityType = EntityTypeCache.Read( metricPartition.EntityTypeId ?? 0 );
                    var controlId = string.Format( "metricPartition{0}_entityTypeEditControl", metricPartition.Id );
                    Control entityTypeEditControl = phMetricValuePartitions.FindControl( controlId );

                    int? entityId = entityTypeEntityList.Where( a => a.EntityTypeId == metricPartition.EntityTypeId ).Select( a => a.EntityId ).FirstOrDefault();

                    if ( metricPartitionEntityType != null && metricPartitionEntityType.SingleValueFieldType != null && metricPartitionEntityType.SingleValueFieldType.Field is IEntityFieldType )
                    {
                        ( metricPartitionEntityType.SingleValueFieldType.Field as IEntityFieldType ).SetEditValueFromEntityId( entityTypeEditControl, new Dictionary<string, ConfigurationValue>(), entityId );
                    }
                }
            }
        }

        /// <summary>
        /// Creates the dynamic controls.
        /// </summary>
        /// <param name="metricID">The metric identifier.</param>
        private void CreateDynamicControls( int? metricID )
        {
            Metric metric = new MetricService( new RockContext() ).Get( metricID ?? 0 );
            if ( metric != null )
            {
                foreach ( var metricPartition in metric.MetricPartitions )
                {
                    if ( metricPartition.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Read( metricPartition.EntityTypeId.Value );
                        if ( entityTypeCache != null && entityTypeCache.SingleValueFieldType != null )
                        {
                            var fieldType = entityTypeCache.SingleValueFieldType;

                            Dictionary<string, Rock.Field.ConfigurationValue> configurationValues;
                            if ( fieldType.Field is IEntityQualifierFieldType )
                            {
                                configurationValues = ( fieldType.Field as IEntityQualifierFieldType ).GetConfigurationValuesFromEntityQualifier( metricPartition.EntityTypeQualifierColumn, metricPartition.EntityTypeQualifierValue );
                            }
                            else
                            {
                                configurationValues = new Dictionary<string, ConfigurationValue>();
                            }

                            var entityTypeEditControl = fieldType.Field.EditControl( configurationValues, string.Format( "metricPartition{0}_entityTypeEditControl", metricPartition.Id ) );
                            var panelCol4 = new Panel { CssClass = "col-md-4" };

                            if ( entityTypeEditControl != null )
                            {
                                phMetricValuePartitions.Controls.Add( entityTypeEditControl );
                                if ( entityTypeEditControl is IRockControl )
                                {
                                    var entityTypeRockControl = ( entityTypeEditControl as IRockControl );
                                    entityTypeRockControl.Label = metricPartition.Label;
                                }
                            }
                            else
                            {
                                var errorControl = new LiteralControl();
                                errorControl.Text = string.Format("<span class='label label-danger'>Unable to create Partition control for {0}. Verify that the metric partition settings are set correctly</span>", metricPartition.Label);
                                phMetricValuePartitions.Controls.Add( errorControl );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gfs the metric values_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfMetricValues_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == "Date Range" )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == "Goal/Measure" )
            {
                var metricValueType = e.Value.ConvertToEnumOrNull<MetricValueType>();
                if ( metricValueType.HasValue )
                {
                    e.Value = metricValueType.Value.ConvertToString();
                }
                else
                {
                    e.Value = null;
                }
            }
            else if ( e.Key == this.EntityTypeEntityPreferenceKey )
            {
                var entityTypeEntityUserPreference = gfMetricValues.GetUserPreference( this.EntityTypeEntityPreferenceKey ) ?? string.Empty;

                var entityTypeEntityList = ( e.Value ?? string.Empty ).Split( ',' ).Select( a => a.Split( '|' ) ).Where( a => a.Length == 2 ).Select( a =>
                    new MetricValuePartition
                    {
                        MetricPartition = new MetricPartition { EntityTypeId = a[0].AsIntegerOrNull() },
                        EntityId = a[1].AsIntegerOrNull()
                    } ).ToList();

                e.Name = "Partitions";
                e.Value = GetSeriesName( entityTypeEntityList );
            }
            else
            {
                e.Value = null;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfMetricValues_ApplyFilterClick( object sender, EventArgs e )
        {
            gfMetricValues.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            gfMetricValues.SaveUserPreference( "Goal/Measure", ddlGoalMeasure.SelectedValue );

            var metric = new MetricService( new RockContext() ).Get( hfMetricId.Value.AsInteger() );

            var entityTypeEntityFilters = new Dictionary<int, int?>();
            foreach ( var metricPartition in metric.MetricPartitions )
            {
                var metricPartitionEntityType = EntityTypeCache.Read( metricPartition.EntityTypeId ?? 0 );
                var controlId = string.Format( "metricPartition{0}_entityTypeEditControl", metricPartition.Id );
                Control entityTypeEditControl = phMetricValuePartitions.FindControl( controlId );

                int? entityId;

                if ( metricPartitionEntityType != null && metricPartitionEntityType.SingleValueFieldType != null && metricPartitionEntityType.SingleValueFieldType.Field is IEntityFieldType )
                {
                    entityId = ( metricPartitionEntityType.SingleValueFieldType.Field as IEntityFieldType ).GetEditValueAsEntityId( entityTypeEditControl, new Dictionary<string, ConfigurationValue>() );

                    entityTypeEntityFilters.AddOrIgnore( metricPartitionEntityType.Id, entityId );
                }
            }

            var entityTypeEntityUserPreferenceValue = entityTypeEntityFilters
                .Select( a => new { EntityTypeId = a.Key, EntityId = a.Value } )
                .Select( a => string.Format( "{0}|{1}", a.EntityTypeId, a.EntityId ) )
                .ToList().AsDelimited( "," );

            gfMetricValues.SaveUserPreference( this.EntityTypeEntityPreferenceKey, entityTypeEntityUserPreferenceValue );

            BindGrid();
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
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "MetricValueId", 0.ToString() );
            qryParams.Add( "MetricCategoryId", hfMetricCategoryId.Value );
            qryParams.Add( "ExpandedIds", PageParameter( "ExpandedIds" ) );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Edit event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMetricValues_Edit( object sender, RowEventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "MetricValueId", e.RowKeyId.ToString() );
            qryParams.Add( "MetricCategoryId", hfMetricCategoryId.Value );
            qryParams.Add( "ExpandedIds", PageParameter( "ExpandedIds" ) );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Delete event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMetricValues_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var metricValueService = new MetricValueService( rockContext );
            var metricValuePartitionService = new MetricValuePartitionService( rockContext );

            var metricValue = metricValueService.Get( e.RowKeyId );
            if ( metricValue != null )
            {
                string errorMessage;
                if ( !metricValueService.CanDelete( metricValue, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    metricValuePartitionService.DeleteRange( metricValue.MetricValuePartitions );
                    metricValueService.Delete( metricValue );
                    rockContext.SaveChanges();

                } );

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
                var lMetricValuePartitions = e.Row.FindControl( "lMetricValuePartitions" ) as Literal;
                if ( lMetricValuePartitions != null )
                {
                    lMetricValuePartitions.Text = GetSeriesName( ( e.Row.DataItem as MetricValue ).MetricValuePartitions );
                }
            }
        }

        /// <summary>
        /// Gets the name of the series.
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="seriesId">The series identifier.</param>
        /// <returns></returns>
        private string GetSeriesName( ICollection<MetricValuePartition> metricValuePartitions )
        {
            if ( _entityTypeEntityNameLookup != null && metricValuePartitions != null )
            {
                List<string> seriesNames = new List<string>();
                foreach ( var metricValuePartition in metricValuePartitions.Where( a => a.EntityId.HasValue && a.MetricPartition.EntityTypeId.HasValue ) )
                {
                    var entityNameLookup = _entityTypeEntityNameLookup[metricValuePartition.MetricPartition.EntityTypeId.Value];
                    if ( !entityNameLookup.ContainsKey( metricValuePartition.EntityId.Value ) )
                    {
                        string value = string.Empty;

                        var entityItem = _entityTypeEntityLookupQry[metricValuePartition.MetricPartition.EntityTypeId.Value].Where( a => a.Id == metricValuePartition.EntityId.Value ).FirstOrDefault();
                        if ( entityItem != null )
                        {
                            value = entityItem.ToString();
                        }

                        entityNameLookup.AddOrIgnore( metricValuePartition.EntityId.Value, value );
                    }

                    seriesNames.Add( entityNameLookup[metricValuePartition.EntityId.Value] );
                }

                return seriesNames.AsDelimited( ", ", " and " );
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
            int? metricId = hfMetricId.Value.AsIntegerOrNull();

            if ( !metricId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            SortProperty sortProperty = gMetricValues.SortProperty;
            MetricValueService metricValueService = new MetricValueService( rockContext );
            var qry = metricValueService.Queryable().Include( a => a.MetricValuePartitions );

            qry = qry.Where( a => a.MetricId == metricId );

            var metricValuePartitionsColumn = gMetricValues.Columns.OfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lMetricValuePartitions" );
            var metric = new MetricService( rockContext ).Get( metricId ?? 0 );
            metricValuePartitionsColumn.Visible = metric != null && metric.MetricPartitions.Any( a => a.EntityTypeId.HasValue );

            var drp = new DateRangePicker();
            drp.DelimitedValues = gfMetricValues.GetUserPreference( "Date Range" );
            if ( drp.LowerValue.HasValue )
            {
                qry = qry.Where( a => a.MetricValueDateTime >= drp.LowerValue.Value );
            }

            if ( drp.UpperValue.HasValue )
            {
                DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                qry = qry.Where( a => a.MetricValueDateTime < upperDate );
            }

            var metricValueType = gfMetricValues.GetUserPreference( "Goal/Measure" ).ConvertToEnumOrNull<MetricValueType>();
            if ( metricValueType.HasValue )
            {
                qry = qry.Where( a => a.MetricValueType == metricValueType.Value );
            }

            var entityTypeEntityUserPreference = gfMetricValues.GetUserPreference( this.EntityTypeEntityPreferenceKey ) ?? string.Empty;

            var entityTypeEntityList = entityTypeEntityUserPreference.Split( ',' ).Select( a => a.Split( '|' ) ).Where( a => a.Length == 2 ).Select( a =>
                new
                {
                    EntityTypeId = a[0].AsIntegerOrNull(),
                    EntityId = a[1].AsIntegerOrNull()
                } );

            foreach ( var entityTypeEntity in entityTypeEntityList )
            {
                if ( entityTypeEntity.EntityTypeId.HasValue && entityTypeEntity.EntityId.HasValue )
                {
                    qry = qry.Where( a => a.MetricValuePartitions.Any( x => x.MetricPartition.EntityTypeId == entityTypeEntity.EntityTypeId && x.EntityId == entityTypeEntity.EntityId ) );
                }
            }

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( s => s.MetricValueDateTime ).ThenBy( s => s.YValue ).ThenBy( s => s.XValue ).ThenByDescending( s => s.ModifiedDateTime );
            }

            gMetricValues.SetLinqDataSource( qry );

            gMetricValues.DataBind();
        }

        /// <summary>
        /// Creates the entity value lookups.
        /// </summary>
        /// <param name="metricID">The metric identifier.</param>
        private void CreateEntityValueLookups(int? metricID )
        {
            Metric metric = new MetricService( new RockContext() ).Get( metricID ?? 0 );
            if ( metric != null )
            {
                var rockContext = new RockContext();
                _entityTypeEntityNameLookup = new Dictionary<int, Dictionary<int, string>>();
                _entityTypeEntityLookupQry = new Dictionary<int, IQueryable<IEntity>>();

                foreach ( var metricPartition in metric.MetricPartitions.Where( a => a.EntityTypeId.HasValue ) )
                {
                    var entityTypeCache = EntityTypeCache.Read( metricPartition.EntityTypeId ?? 0 );

                    _entityTypeEntityNameLookup.AddOrIgnore( entityTypeCache.Id, new Dictionary<int, string>() );
                    _entityTypeEntityLookupQry.AddOrIgnore( entityTypeCache.Id, null );
                    if ( entityTypeCache != null )
                    {
                        if ( entityTypeCache.GetEntityType() == typeof( Rock.Model.Group ) )
                        {
                            _entityTypeEntityLookupQry[entityTypeCache.Id] = new GroupService( rockContext ).Queryable();
                        }
                        else
                        {
                            Type[] modelType = { entityTypeCache.GetEntityType() };
                            Type genericServiceType = typeof( Rock.Data.Service<> );
                            Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                            var serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;
                            MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                            _entityTypeEntityLookupQry[entityTypeCache.Id] = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the hidden field values.
        /// </summary>
        private void SetHiddenFieldValues()
        {
            var rockContext = new RockContext();

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
        }

        /// <summary>
        /// Set the appropriate security access to the gMetricValues grid buttons.
        /// </summary>
        private void SetGridSecurity()
        {
            int metricId = hfMetricId.ValueAsInt();
            gMetricValues.Actions.ShowAdd = false;
            gMetricValues.IsDeleteEnabled = false;

            if ( metricId > 0 )
            {
                var metric = new MetricService( new RockContext() ).Get( metricId );
                if ( UserCanEdit || ( metric != null && metric.IsAuthorized( Authorization.EDIT, CurrentPerson ) ) )
                {
                    // Block Security and special attributes (RockPage takes care of View)
                    gMetricValues.Actions.ShowAdd = true;
                    gMetricValues.IsDeleteEnabled = true;
                }
            }
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