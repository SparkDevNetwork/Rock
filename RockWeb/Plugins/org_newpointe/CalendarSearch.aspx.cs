using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using org.newpointe.ServiceUCalendar.Model;
using Newtonsoft.Json;
using System.IO;


public partial class Plugins_org_newpointe_CalendarSearch : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
     
        if (!Page.IsPostBack)
        {
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8;";

            List<CalendarLite> calendarEvents = JsonConvert.DeserializeObject<List<CalendarLite>>(File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/Assets/calendar-full.json")));
            var query = Request.QueryString["query"];
            var id = Request.QueryString["id"];
            var date = Request.QueryString["date"];

            if (!string.IsNullOrEmpty(query))
            {

                DateTime dt = new DateTime(2012, 01, 01);

                var matches = calendarEvents.Select(c => new { value = c.title + " - " + FromMS(Convert.ToInt64(c.start)).ToString("MMM dd, yyyy hh:MM tt") + " - " + c.locationcity + ", " + c.locationstate, data = getIDString(c), date = FromMS(Convert.ToInt64(c.start)).ToString("MM/dd/yyyy") }).Where(
                    c => c.value.ToLower().Contains(query.ToLower()) || c.date.StartsWith(query)
                ).OrderBy(x => Convert.ToDateTime(x.date));
                Response.Write("{ \"query\" : \"" + query + "\", \"suggestions\" :  " + JsonConvert.SerializeObject(matches) + " }");
            }
            else if (!string.IsNullOrEmpty(id))
            {
                int? sid = 0;
                if (id.Split('-').Length == 1)
                {
                    sid = Convert.ToInt32(id);
                }

                var calevent = calendarEvents.Select(c => new { c.id, c.title, c.url, c.@class, start = FromMS(Convert.ToInt64(c.start)), startTime = FromMS(Convert.ToInt64(c.start)).ToString("%h:mmtt").ToLower(), endtime = FromMS(Convert.ToInt64(c.end)).ToString("%h:mm tt").ToLower(), end = FromMS(Convert.ToInt64(c.end)), c.departmentname, c.description, c.locationaddress, c.locationaddress2, c.locationcity, c.locationstate, c.locationzip, c.locationname })
                    .Where(c => (c.id.ToString() + '-' + c.start.ToShortDateString() == id || c.id == sid) && c.start > DateTime.Now).OrderBy(c=>c.start).Take(1);
                Response.Write(JsonConvert.SerializeObject(calevent));
            }
            else if (!string.IsNullOrEmpty(date))
            {
                try
                {
                    var searchDate = Convert.ToDateTime(date);
                    var matches = calendarEvents.Select(c => new { c.id, c.title, c.url, c.@class, start = FromMS(Convert.ToInt64(c.start)), startTime = FromMS(Convert.ToInt64(c.start)).ToString("%h:mmtt").ToLower(), endtime = FromMS(Convert.ToInt64(c.end)).ToString("%h:mm tt").ToLower(), end = FromMS(Convert.ToInt64(c.end)), c.departmentname, c.description, c.locationaddress, c.locationaddress2, c.locationcity, c.locationstate, c.locationzip, c.locationname }).Where(c => c.start.ToShortDateString() == searchDate.ToShortDateString()).OrderBy(x => x.start);
                    Response.Write(JsonConvert.SerializeObject(matches));
                }
                catch (Exception ex)
                {
                    Rock.Model.ExceptionLogService.LogException( ex, Context );
                }
            }

            Response.End();
        }

    }
    private string getIDString(CalendarLite c)
    {
        return c.id.ToString() + '-' + FromMS(Convert.ToInt64(c.start)).ToShortDateString();
    }
    private static DateTime FromMS(long microSec)
    {
        long milliSec = (long)(microSec);
        DateTime startTime = new DateTime(1970, 1, 1);

        TimeSpan time = TimeSpan.FromMilliseconds(milliSec);
        return startTime.Add(time);
    }
}