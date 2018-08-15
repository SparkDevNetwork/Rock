using System.Net;
using Newtonsoft.Json;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace org.newpointe.LiveService.Data
{

    /// <summary>
    /// Summary description for CustomJob
    /// </summary>
    /// 
    [UrlLinkField("ChurchOnline API Address","The URL to check for the ChurchOnline API",true, "http://live.newpointe.org/api/v1/events/current","General",1,"Address")]
    public class LiveServiceJsonJob : IJob
    {
        public string LivePlatformUrlJson;
        RockContext rockContext = new RockContext();
        
        public void Execute(IJobExecutionContext context)
        {

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string livePlatformUrl = dataMap.GetString("Address") ?? "http://live.newpointe.org/api/v1/events/current";


            //Check ChurchOnline Platform API to see if there is a live event

            using (WebClient wc = new WebClient())
            {
                LivePlatformUrlJson = wc.DownloadString(livePlatformUrl);
            }

            dynamic isServiceLive = JsonConvert.DeserializeObject(LivePlatformUrlJson);

            string isLive = isServiceLive.response.item.isLive.ToString();

            // specify which attribute key we want to work with
            var attributeKey = "LiveService";  //production

            var attributeValueService = new AttributeValueService(rockContext);

            // specify NULL as the EntityId since this is a GlobalAttribute
            var globalAttributeValue = attributeValueService.GetGlobalAttributeValue(attributeKey);

            if (globalAttributeValue != null)
            {
                // save updated value to database
                globalAttributeValue.Value = isLive;
                rockContext.SaveChanges();

                // flush the attributeCache for this attribute so it gets reloaded from the database
                //Rock.Web.Cache.AttributeCache.Flush();

                // flush the GlobalAttributeCache since this attribute is a GlobalAttribute
                Rock.Web.Cache.GlobalAttributesCache.Flush();
            }

        }

    }

}