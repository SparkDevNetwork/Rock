// <copyright>
// Copyright by Central Christian Church
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
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_centralaz.ChurchMetrics
{
    /// <summary>
    /// Takes a defined type and returns all defined values and merges them with a liquid template
    /// </summary>
    [DisplayName( "Import Metrics" )]
    [Category( "com_centralaz > ChurchMetrics" )]
    [Description( "Imports ChurchMetrics categories as Rock Metrics" )]
    [TextField( "User Email", "The ChurchMetrics user email.", order: 0 )]
    [TextField( "User Api Key", "The ChurchMetrics user api key.", order: 1 )]    
    [CategoryField( "Default Category", "The default category to add new metrics to when syncing metrics", false, "Rock.Model.MetricCategory", order: 3 )]
    public partial class ImportMetrics : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadContent();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }

        protected void btnGrabJson_Click( object sender, EventArgs e )
        {
            List<String> importedMetricList = new List<string>();

            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );
            var metricCategoryService = new MetricCategoryService( rockContext );            

            var manualSourceTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;
            var categoryId = new CategoryService( rockContext ).Get( GetAttributeValue( "DefaultCategory" ).AsGuid() ).Id;
            var campusEntityTypeId = EntityTypeCache.Read( "00096BED-9587-415E-8AD4-4E076AE8FBF0".AsGuid() ).Id;
            var scheduleEntityTypeId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.SCHEDULE.AsGuid() ).Id;

            dynamic metricResults = new List<ExpandoObject>();

            try
            {
                metricResults = GetResults( "categories.json" );
            }
            catch ( Exception ex )
            {
                nbWarning.Title = "Failed to get categories:";
                nbWarning.Text = ex.Message;
                nbWarning.Visible = true;
            }

            foreach ( dynamic churchMetricCategory in metricResults )
            {
                
                int? foreignId = Convert.ToInt32( churchMetricCategory.id );
                if ( foreignId != null )
                {
                    var metric = metricService.Queryable().Where( m => m.ForeignId == foreignId ).FirstOrDefault();
                    if ( metric == null )
                    {
                        metric = new Metric();
                        metric.Title = churchMetricCategory.name;
                        metric.IsCumulative = false;
                        metric.SourceValueTypeId = manualSourceTypeId;
                        metric.ForeignId = foreignId;
                        metric.MetricCategories = new List<MetricCategory>();
                        metric.MetricPartitions = new List<MetricPartition>();

                        var metricCategory = new MetricCategory();
                        metricCategory.CategoryId = categoryId;
                        metricCategory.Order = metricCategoryService.Queryable().Max( mc => mc.Order ) + 1;
                        metric.MetricCategories.Add( metricCategory );

                        var campusPartition = new MetricPartition();
                        campusPartition.Label = "Campus";
                        campusPartition.EntityTypeId = campusEntityTypeId;
                        campusPartition.IsRequired = true;
                        campusPartition.Order = 0;
                        metric.MetricPartitions.Add( campusPartition );

                        var servicePartition = new MetricPartition();
                        servicePartition.Label = "Service";
                        servicePartition.EntityTypeId = scheduleEntityTypeId;
                        servicePartition.IsRequired = true;
                        campusPartition.Order = 1;
                        metric.MetricPartitions.Add( servicePartition );

                        metricService.Add( metric );
                        importedMetricList.Add( String.Format( "Imported {0}", churchMetricCategory.name ) );
                    }
                }
            }

            rockContext.SaveChanges();

            lContent.Text = importedMetricList.AsDelimited( "\n" );
                        
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected void LoadContent()
        {

        }

        #endregion       

        /// <summary>
        /// Gets the results.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public dynamic GetResults( String apiQuery )
        {
            dynamic data = new List<ExpandoObject>();

            var converter = new ExpandoObjectConverter();
            var restClient = new RestClient( string.Format( "https://churchmetrics.com/api/v1/{0}", apiQuery ) );
            var restRequest = new RestRequest( Method.GET );
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader( "Accept", "application/json" );
            restRequest.AddHeader( "X-Auth-User", GetAttributeValue( "UserEmail" ) );
            restRequest.AddHeader( "X-Auth-Key", GetAttributeValue( "UserApiKey" ) );
            var restResponse = restClient.Execute( restRequest );
            if ( restResponse.StatusCode == HttpStatusCode.OK )
            {
                data = JsonConvert.DeserializeObject<List<ExpandoObject>>( restResponse.Content, converter );
            }

            return data;
        }
    }
}