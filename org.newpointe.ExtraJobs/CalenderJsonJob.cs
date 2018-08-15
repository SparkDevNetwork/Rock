using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using org.newpointe.ServiceUCalendar.Model;
using Quartz;
using RestSharp;
using Rock;
using Rock.Attribute;

namespace org.newpointe.ServiceUCalendar.Data
{

    /// <summary>
    /// Summary description for CustomJob
    /// </summary>
    /// 
    [TextField("ServiceU Key", "The key needed to retrieve calendar info.", true, "195d2f5d-2cc0-48c8-a533-0c990a59a46f", "", 0)]
    [ValueListField("Department Ids", "The ids of the departments to get events for.", true, "67713|53103|62004|51774|67714|51773", "Id", "", "", "", 1)]
    public class CalendarJsonJob : IJob
    {
        private const string _baseUrl = "http://api.serviceu.com";
        private const string _calendarApi = "rest/events/occurrences?startDate={2}&endDate={3}&departmentids={1}&orgkey={0}&format=json";

        private DateTime today = DateTime.Today;

        public void Execute( IJobExecutionContext context )
        {

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string serviceUKeys = dataMap.GetString( "ServiceUKey" );
            string departmentIds = String.Join(",", dataMap.GetString( "DepartmentIds" ).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

            //Get Start and End Dates
            var firstDay = new DateTime( today.Year, today.Month, 1 );
            var lastDay = firstDay.AddYears( 1 );
            var lastDayShort = firstDay.AddMonths( 6 );

            string formattedFirstDay = string.Format( "{0:MM/dd/yyyy}", firstDay );
            string formattedLastDay = string.Format( "{0:MM/dd/yyyy}", lastDay );
            string formattedLastDayShort = string.Format( "{0:MM/dd/yyyy}", lastDayShort );


            //create smaller calendar.json file 6 months out
            createCalendarJsonFle( System.Web.Hosting.HostingEnvironment.MapPath( "~/Assets/calendar.json" ), formattedFirstDay, formattedLastDayShort, serviceUKeys, departmentIds );
            //create  calendar-full.json file 1 year out
            createCalendarJsonFle( System.Web.Hosting.HostingEnvironment.MapPath( "~/Assets/calendar-full.json" ), formattedFirstDay, formattedLastDay, serviceUKeys, departmentIds );
        }


        private void createCalendarJsonFle( string filename, string firstDay, string lastDay, string serviceUKeys, string departmentIds )
        {

            //new restsharp 
            var client = new RestClient( _baseUrl );
            ArrayList jsonCalendar = new ArrayList();
            //get calendar data from my service u
            var request = new RestRequest( string.Format( _calendarApi, serviceUKeys, departmentIds, firstDay, lastDay ) );
            var response = client.Execute<List<Calendar>>( request );

            //Delete Old File.
            try
            {
                File.Delete( filename );
            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( ex, null );
            }

            //build new json Calendar
            foreach ( Calendar calendar in response.Data )
            {

                DateTime baseDate = new DateTime( 1970, 1, 1 );
                TimeSpan startDiff = Convert.ToDateTime( calendar.OccurrenceStartTime ) - baseDate;
                TimeSpan endDiff = Convert.ToDateTime( calendar.OccurrenceEndTime ) - baseDate;
                jsonCalendar.Add( new
                {
                    id = calendar.EventId,
                    title = calendar.Name,
                    url = calendar.EventId,
                    start = startDiff.TotalMilliseconds.ToString(),
                    end = endDiff.TotalMilliseconds.ToString(),
                    startDate = calendar.ResourceStartTime,
                    endDate = calendar.ResourceEndTime,
                    @class = getCssClass( calendar.CategoryList ),
                    departmentname = calendar.DepartmentName,
                    description = calendar.Description,
                    locationaddress = calendar.LocationAddress,
                    locationaddress2 = calendar.LocationAddress2,
                    locationcity = calendar.LocationCity,
                    locationname = calendar.LocationName,
                    locationstate = calendar.LocationState,
                    locationzip = calendar.LocationZip
                } );
            }
            //write new JSON files.
            using ( FileStream fs = File.Open( filename, FileMode.CreateNew ) )
            using ( StreamWriter sw = new StreamWriter( fs ) )
            using ( JsonWriter jw = new JsonTextWriter( sw ) )
            {
                jw.Formatting = Formatting.Indented;
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize( jw, jsonCalendar );
            }
        }

        /// <summary>
        /// Determines CSS class for Calendar based on Categories for when building the new Calender.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private string getCssClass( string category )
        {
            if ( category.Contains( "Adults" ) )
            {
                return "event-success";
            }

            else if ( category.Contains( "Children" ) )
            {
                return "event-important";
            }

            else if ( category.Contains( "Kids" ) )
            {
                return "event-important";
            }

            else if ( category.Contains( "Students" ) )
            {
                return "event-info";
            }

            else
            {
                return "event-inverse";
            }
        }
    }
}