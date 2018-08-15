<%@ WebHandler Language="C#" Class="Calendar" %>

using System;
using System.Web;
using System.IO;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using org.newpointe.ServiceUCalendar.Model;

public class Calendar : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json; charset=utf-8;";
        List<CalendarLite> calendarEvents = JsonConvert.DeserializeObject<List<CalendarLite>>(File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/Assets/calendar-full.json")));
        var query = context.Request.QueryString["query"];
        var id = context.Request.QueryString["id"];
        var date = context.Request.QueryString["date"];

        if (!string.IsNullOrEmpty(query))
        {

            DateTime dt = new DateTime(2012, 01, 01);

            var matches = calendarEvents.Select(c => new { value = c.title + " - " + FromMS(Convert.ToInt64(c.start)).ToString("MMM dd, yyyy %h:mm tt") + " - " + c.locationcity + ", " + c.locationstate, data = getIDString(c), date = FromMS(Convert.ToInt64(c.start)).ToString("MM/dd/yyyy") }).Where(
                c => c.value.ToLower().Contains(query.ToLower()) || c.date.StartsWith(query)
            ).OrderBy(x => Convert.ToDateTime(x.date));
            context.Response.Write("{ \"query\" : \"" + query + "\", \"suggestions\" :  " + Newtonsoft.Json.JsonConvert.SerializeObject(matches) + " }");
        }
        else if (!string.IsNullOrEmpty(id))
        {
            int? sid = 0;
            if (id.Split('-').Length == 1)
            {
                sid = Convert.ToInt32(id);
            }

            var calevent = calendarEvents.Where(c => (c.id.ToString() + '-' + FromMS(Convert.ToInt64(c.start)).ToShortDateString() == id || c.id == sid) && FromMS(Convert.ToInt64(c.start)) > DateTime.Now).Select(c => new
            {
                c.id,
                c.title,
                c.url,
                c.@class,
                start = FromMS(Convert.ToInt64(c.start)),
                startTime = FromMS(Convert.ToInt64(c.start)).ToString("%h:mm tt").ToLower(),
                endtime = FromMS(Convert.ToInt64(c.end)).ToString("%h:mm tt").ToLower(),
                end = FromMS(Convert.ToInt64(c.end)),
                c.departmentname,
                description = GetAdditionalInfo(c.id, c.description, c.title),
                c.locationaddress,
                c.locationaddress2,
                c.locationcity,
                c.locationstate,
                c.locationzip,
                c.locationname
            })
                .OrderBy(c => c.start).Take(1);
            context.Response.Write(JsonConvert.SerializeObject(calevent));
        }
        else if (!string.IsNullOrEmpty(date))
        {
            try
            {
                var searchDate = Convert.ToDateTime(date);
                var matches = calendarEvents.Where(c => FromMS(Convert.ToInt64(c.start)).ToShortDateString() == searchDate.ToShortDateString()).Select(c => new
                {
                    c.id,
                    c.title,
                    c.url,
                    c.@class,
                    start = FromMS(Convert.ToInt64(c.start)),
                    startTime = FromMS(Convert.ToInt64(c.start)).ToString("%h:mm tt").ToLower(),
                    endtime = FromMS(Convert.ToInt64(c.end)).ToString("%h:mm tt").ToLower(),
                    end = FromMS(Convert.ToInt64(c.end)),
                    c.departmentname,
                    description = GetAdditionalInfo(c.id, c.description, c.title),
                    c.locationaddress,
                    c.locationaddress2,
                    c.locationcity,
                    c.locationstate,
                    c.locationzip,
                    c.locationname
                }).OrderBy(x => x.start);
                context.Response.Write(JsonConvert.SerializeObject(matches));
            }
            catch (Exception ex)
            {
                Rock.Model.ExceptionLogService.LogException( ex , context);
            }
        }

        context.Response.End();

    }

    private string GetAdditionalInfo(int? Id, string Description, string title)
    {
        // int eventid = 0;
        //var div = $("<div class='text-center'></div>");
        //    div.append("<img src='" + data[key].image + "' alt='" + data[key].title + "' />");
        string url = VirtualPathUtility.ToAbsolute("~/GetImage.ashx?guid={0}&width=900");
        string description = string.Empty;


        List<additionalInfo> result;
        using (var rc = new Rock.Data.RockContext())
        {
            result = rc.Database.SqlQuery<additionalInfo>("exec newpointe_getAdditionalInfoByEventID @id", new SqlParameter("id", Id)).ToList<additionalInfo>();
        }

        if (!string.IsNullOrEmpty(result[0].Image))
        {
            description = "<div class='text-center'><img src='" + string.Format(url, result[0].Image) + "' alt='" + title + "' /></div>";
        }

        description = description + "<p>" + Description + "</p>";

        if (!string.IsNullOrEmpty(result[0].Details))
        {
            description = description + System.Text.RegularExpressions.Regex.Replace(result[0].Details, @"\t|\n|\r", "");
        }

        if (!string.IsNullOrEmpty(result[0].RegistrationURL))
        {
            description = description + "<p><a class=\"btn btn-primary\" data-loading-text=\"&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Let's go!\" href=\" " + result[0].RegistrationURL + "\" id=\"ctl00_main_ctl23_ctl01_ctl06_lbSave\" onclick=\"Rock.controls.bootstrapButton.showLoading(this);\">Register Now</a></p>";
        }

        if (!string.IsNullOrEmpty(result[0].ShowReleaseForm) && result[0].ShowReleaseForm == "True")
        {
            description = description + "<p><a class=\"btn btn-primary\" data-loading-text=\"&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Let's go!\" href=\" " + "https://newpointe.org/form/163" + "\" id=\"ctl00_main_ctl23_ctl01_ctl06_lbSave\" onclick=\"Rock.controls.bootstrapButton.showLoading(this);\">Release Form</a></p>";
        } 

        return description;
    }
    private string getIDString(CalendarLite c)
    {
        return c.id.ToString() + '-' + FromMS(Convert.ToInt64(c.start)).ToShortDateString();
    }

    private static DateTime FromMS(long milliSec)
    {
        return new DateTime(1970, 1, 1).Add(TimeSpan.FromMilliseconds(milliSec));
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}

class additionalInfo
{
    public string Image { get; set; }
    public string Details { get; set; }
    public string RegistrationURL { get; set; }
    public string ShowReleaseForm { get; set; }
}
