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
using System.Dynamic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Quartz;
using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com.centralaz.ChurchMetrics.Jobs
{
    /// <summary>
    /// For any people that share an email, this job sets everyone's preference to the most restrictive setting that one of them has.
    /// </summary>
    [TextField( "User Email", "The ChurchMetrics user email.", order: 0 )]
    [TextField( "User Api Key", "The ChurchMetrics user api key.", order: 1 )]
    [AttributeField( "00096BED-9587-415E-8AD4-4E076AE8FBF0", "Campus Foreign Id Attribute", order: 2 )]
    [CategoryField( "Ahwatukee Service Times", "The category for Ahwatukee service times", false, "Rock.Model.Schedule", required: false, order: 3 )]
    [CategoryField( "Gilbert Service Times", "The category for Gilbert service times", false, "Rock.Model.Schedule", required: false, order: 4 )]
    [CategoryField( "Glendale Service Times", "The category for Glendale service times", false, "Rock.Model.Schedule", required: false, order: 5 )]
    [CategoryField( "Mesa Service Times", "The category for Mesa service times", false, "Rock.Model.Schedule", required: false, order: 6 )]
    [CategoryField( "Queen Creek Service Times", "The category for Queen Creek service times", false, "Rock.Model.Schedule", required: false, order: 7 )]
    [CategoryField( "Event Schedules", "The category for event schedules", false, "Rock.Model.Schedule", required: false, order: 8 )]
    [DisallowConcurrentExecution]
    public class ImportMetricValues : IJob
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchEmailPreference"/> class.
        /// </summary>
        public ImportMetricValues()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );
            var metricPartitionService = new MetricPartitionService( rockContext );
            var metricValueService = new MetricValueService( rockContext );
            var metricValuePartitionService = new MetricValuePartitionService( rockContext );
            var scheduleService = new ScheduleService( rockContext );

            try
            {
                //Get Records
                var campusAttribute = AttributeCache.Read( dataMap.Get( "CampusForeignIdAttribute" ).ToString().AsGuid() );

                // Get the schedules
                var ahwatukeeCategoryId = CategoryCache.Read( dataMap.Get( "AhwatukeeServiceTimes" ).ToString().AsGuid() ).Id;
                var gilbertCategoryId = CategoryCache.Read( dataMap.Get( "GilbertServiceTimes" ).ToString().AsGuid() ).Id;
                var glendaleCategoryId = CategoryCache.Read( dataMap.Get( "GlendaleServiceTimes" ).ToString().AsGuid() ).Id;
                var mesaCategoryId = CategoryCache.Read( dataMap.Get( "MesaServiceTimes" ).ToString().AsGuid() ).Id;
                var queenCreekCategoryId = CategoryCache.Read( dataMap.Get( "QueenCreekServiceTimes" ).ToString().AsGuid() ).Id;
                var eventCategoryId = CategoryCache.Read( dataMap.Get( "EventSchedules" ).ToString().AsGuid() ).Id;

                var ahwatukeeServiceTimes = scheduleService.Queryable()
                    .Where( s => s.CategoryId == ahwatukeeCategoryId )
                    .ToList();
                var gilbertServiceTimes = scheduleService.Queryable()
                                .Where( s => s.CategoryId == gilbertCategoryId )
                                .ToList();
                var glendaleServicesTimes = scheduleService.Queryable()
                                .Where( s => s.CategoryId == glendaleCategoryId )
                                .ToList();
                var mesaServiceTimes = scheduleService.Queryable()
                                .Where( s => s.CategoryId == mesaCategoryId )
                                .ToList();
                var queenCreekServiceTimes = scheduleService.Queryable()
                                .Where( s => s.CategoryId == queenCreekCategoryId )
                                .ToList();
                var eventSchedules = scheduleService.Queryable()
                                .Where( s => s.CategoryId == eventCategoryId )
                                .ToList();

                var existingMappedCampusAttributeValues = new AttributeValueService( rockContext ).Queryable()
                    .Where( av => av.AttributeId == campusAttribute.Id )
                    .ToList();
                var mappedMetrics = metricService.Queryable()
                    .Where( m => m.ForeignId != null )
                    .ToList();

                foreach ( var mappedMetric in mappedMetrics )
                {
                    List<ExpandoObject> recordResults = new List<ExpandoObject>();
                    var campusPartition = mappedMetric.MetricPartitions.Where( mp => mp.Label == "Campus" ).FirstOrDefault();
                    var servicePartition = mappedMetric.MetricPartitions.Where( mp => mp.Label == "Service" ).FirstOrDefault();

                    try
                    {
                        recordResults = GetResults( String.Format( "records.json?category_id={0}&page=1&per_page=100", mappedMetric.ForeignId ), dataMap.Get( "UserEmail" ).ToString(), dataMap.Get( "UserApiKey" ).ToString() );
                    }
                    catch
                    {
                    }

                    var pageNumber = 2;
                    while ( recordResults.Count > 0 )
                    {
                        foreach ( dynamic record in recordResults )
                        {
                            int? foreignId = Convert.ToInt32( record.id );
                            if ( foreignId != null )
                            {
                                var metricValue = metricValueService.Queryable().Where( m => m.ForeignId == foreignId ).FirstOrDefault();
                                if ( metricValue == null )
                                {
                                    if ( !Convert.ToBoolean( record.replaces ) )
                                    {
                                        string campusId = Convert.ToString( record.campus.id );
                                        var mappedCampusAttributeValue = existingMappedCampusAttributeValues.Where( av => av.Value == campusId ).FirstOrDefault();
                                        if ( mappedCampusAttributeValue != null )
                                        {
                                            var recordDateTime = record.service_date_time as DateTime?;
                                            if ( recordDateTime.HasValue )
                                            {
                                                Schedule schedule = null;
                                                var metricServiceDateTime = recordDateTime.Value.AddHours( -7 );
                                                var campus = CampusCache.Read( mappedCampusAttributeValue.EntityId.Value );
                                                if ( record.@event == null )
                                                {
                                                    switch ( campus.Name )
                                                    {
                                                        case "Ahwatukee":
                                                            schedule = ahwatukeeServiceTimes.Where( s => s.WeeklyDayOfWeek == metricServiceDateTime.DayOfWeek &&
                                                             s.StartTimeOfDay <= metricServiceDateTime.TimeOfDay &&
                                                             s.StartTimeOfDay >= metricServiceDateTime.AddMinutes( -30 ).TimeOfDay ).FirstOrDefault();
                                                            break;
                                                        case "Gilbert":
                                                            schedule = gilbertServiceTimes.Where( s => s.WeeklyDayOfWeek == metricServiceDateTime.DayOfWeek &&
                                                             s.StartTimeOfDay <= metricServiceDateTime.TimeOfDay &&
                                                             s.StartTimeOfDay >= metricServiceDateTime.AddMinutes( -30 ).TimeOfDay ).FirstOrDefault();
                                                            break;
                                                        case "Glendale":
                                                            schedule = glendaleServicesTimes.Where( s => s.WeeklyDayOfWeek == metricServiceDateTime.DayOfWeek &&
                                                             s.StartTimeOfDay <= metricServiceDateTime.TimeOfDay &&
                                                             s.StartTimeOfDay >= metricServiceDateTime.AddMinutes( -30 ).TimeOfDay ).FirstOrDefault();
                                                            break;
                                                        case "Mesa":
                                                            schedule = mesaServiceTimes.Where( s => s.WeeklyDayOfWeek == metricServiceDateTime.DayOfWeek &&
                                                             s.StartTimeOfDay <= metricServiceDateTime.TimeOfDay &&
                                                             s.StartTimeOfDay >= metricServiceDateTime.AddMinutes( -30 ).TimeOfDay ).FirstOrDefault();
                                                            break;
                                                        case "Queen Creek":
                                                            schedule = queenCreekServiceTimes.Where( s => s.WeeklyDayOfWeek == metricServiceDateTime.DayOfWeek &&
                                                             s.StartTimeOfDay <= metricServiceDateTime.TimeOfDay &&
                                                             s.StartTimeOfDay >= metricServiceDateTime.AddMinutes( -30 ).TimeOfDay ).FirstOrDefault();
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    schedule = eventSchedules.Where( s => s.EffectiveStartDate.Value.DayOfWeek == metricServiceDateTime.DayOfWeek &&
                                                             s.StartTimeOfDay <= metricServiceDateTime.TimeOfDay &&
                                                             s.StartTimeOfDay >= metricServiceDateTime.AddMinutes( -30 ).TimeOfDay ).FirstOrDefault();
                                                }

                                                if ( schedule != null )
                                                {
                                                    if ( metricServiceDateTime.DayOfWeek == DayOfWeek.Saturday )
                                                    {
                                                        metricServiceDateTime = metricServiceDateTime.AddDays( 1 );
                                                    }

                                                    metricValue = new MetricValue();
                                                    metricValue.MetricValueType = MetricValueType.Measure;
                                                    metricValue.MetricId = mappedMetric.Id;
                                                    metricValue.YValue = Convert.ToInt32( record.value );
                                                    metricValue.CreatedDateTime = record.created_at;
                                                    metricValue.ModifiedDateTime = record.updated_at;
                                                    metricValue.ForeignId = foreignId;
                                                    metricValue.MetricValueDateTime = metricServiceDateTime.Date;
                                                    metricValue.MetricValuePartitions = new List<MetricValuePartition>();

                                                    var campusValuePartition = new MetricValuePartition();
                                                    campusValuePartition.MetricPartitionId = campusPartition.Id;
                                                    campusValuePartition.EntityId = mappedCampusAttributeValue.EntityId;
                                                    metricValue.MetricValuePartitions.Add( campusValuePartition );

                                                    var serviceValuePartition = new MetricValuePartition();
                                                    serviceValuePartition.MetricPartitionId = servicePartition.Id;
                                                    serviceValuePartition.EntityId = schedule.Id;
                                                    metricValue.MetricValuePartitions.Add( serviceValuePartition );

                                                    metricValueService.Add( metricValue );
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if ( Convert.ToBoolean( record.replaces ) )
                                    {
                                        metricValuePartitionService.DeleteRange( metricValue.MetricValuePartitions );
                                        metricValueService.Delete( metricValue );
                                    }
                                    else
                                    {
                                        if ( metricValue.CreatedDateTime > RockDateTime.Now.AddYears( -1 ).AddDays( -2 ) )
                                        {
                                            if ( metricValue.YValue != Convert.ToInt32( record.value ) )
                                            {
                                                metricValue.YValue = Convert.ToInt32( record.value );
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        recordResults = new List<ExpandoObject>();
                        try
                        {
                            recordResults = GetResults( String.Format( "records.json?category_id={0}&page={1}&per_page=100", mappedMetric.ForeignId, pageNumber ), dataMap.Get( "UserEmail" ).ToString(), dataMap.Get( "UserApiKey" ).ToString() );
                            pageNumber++;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
                return;
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the results.
        /// </summary>
        /// <param name="apiQuery">The API query.</param>
        /// <param name="email">The email.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns></returns>
        public dynamic GetResults( String apiQuery, String email, String apiKey )
        {
            dynamic data = new List<ExpandoObject>();

            var converter = new ExpandoObjectConverter();
            var restClient = new RestClient( string.Format( "https://churchmetrics.com/api/v1/{0}", apiQuery ) );
            var restRequest = new RestRequest( Method.GET );
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader( "Accept", "application/json" );
            restRequest.AddHeader( "X-Auth-User", email );
            restRequest.AddHeader( "X-Auth-Key", apiKey );
            var restResponse = restClient.Execute( restRequest );
            if ( restResponse.StatusCode == HttpStatusCode.OK )
            {
                data = JsonConvert.DeserializeObject<List<ExpandoObject>>( restResponse.Content, converter );
            }

            return data;
        }
    }
}