using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace church.ccv.MarketingSite.Rest
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    public partial class MarketingSiteController : Rock.Rest.ApiControllerBase
    {
        /// <summary>
        /// Forms the post to workflow.
        /// </summary>
        /// <param name="formData">The form data.</param>
        /// <returns></returns>
        [HttpPost]
        [System.Web.Http.Route( "api/CCV/MarketingSite/FormPostToWorkflow" )]
        public System.Net.Http.HttpResponseMessage FormPostToWorkflow( FormDataCollection formData )
        {
            var workflowTypeGuid = formData["WorkflowTypeGuid"].AsGuidOrNull();
            var successRedirectUrl = formData["success-redirect"];
            var errorRedirectUrl = formData["error-redirect"];

            var rockContext = new Rock.Data.RockContext();
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( workflowTypeGuid ?? Guid.Empty );

            if ( workflowType != null )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, "Workflow From REST" );

                // set workflow attributes from querystring
                foreach ( var parm in formData )
                {
                    workflow.SetAttributeValue( parm.Key, parm.Value );
                }

                // save -> run workflow
                List<string> workflowErrors;
                new Rock.Model.WorkflowService( rockContext ).Process( workflow, out workflowErrors );

                if ( !workflowErrors.Any() )
                {
                    if ( !string.IsNullOrWhiteSpace( successRedirectUrl ) )
                    {
                        var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Redirect );
                        response.Headers.Location = new Uri( successRedirectUrl );
                        return response;
                    }
                }
                else
                {
                    if ( !string.IsNullOrWhiteSpace( errorRedirectUrl ) )
                    {
                        var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Redirect );
                        response.Headers.Location = new Uri( errorRedirectUrl );
                        return response;
                    }
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( errorRedirectUrl ) )
                {
                    var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Redirect );
                    response.Headers.Location = new Uri( errorRedirectUrl );
                    return response;
                }
            }

            return ControllerContext.Request.CreateResponse( HttpStatusCode.NotFound );
        }

        /// <summary>
        /// Gets the external web site ads.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/CCV/MarketingSite/GetExternalWebSiteAds" )]
        public List<Rock.Model.ContentChannelItem> GetExternalWebSiteAds( int? maxItems = 6 )
        {
            var rockContext = new RockContext();
            rockContext.Configuration.ProxyCreationEnabled = false;

            var contentChannelItemService = new ContentChannelItemService( rockContext );
            var qry = contentChannelItemService.Queryable();

            // Limit to External Website Ads
            var externalWebSiteAdsGuid = new Guid( "8E213BB1-9E6F-40C1-B468-B3F8A60D5D24" );
            qry = qry.Where( a => a.ContentChannel.Guid == externalWebSiteAdsGuid );

            // Limit to Approved
            qry = qry.Where( a => a.Status == ContentChannelItemStatus.Approved );

            // Primary Audience is "Homepage - Rotator" OR Secondary Audiences is "Homepage - Rotator"
            var homepageRotatorAudienceGuid = "b364cdee-f000-4965-ae67-0c80dda365dc";
            var allChurchAudienceGuid = "6107ea37-5dd3-4e4f-a2d0-1d4010811d4d";

            var entityTypeIdContentChannelItem = EntityTypeCache.GetId<ContentChannelItem>();

            var primaryAudienceValueIds = new AttributeValueService( rockContext ).Queryable()
                .Where( a => a.Attribute.Key == "PrimaryAudience" )
                .Where( a => a.Attribute.EntityTypeId == entityTypeIdContentChannelItem )
                .Where( a => a.Value.Contains( homepageRotatorAudienceGuid ) || a.Value.Contains( allChurchAudienceGuid ) )
                .Where( a => a.EntityId.HasValue )
                .Select( a => a.EntityId.Value );

            var secondaryAudiencesValueIds = new AttributeValueService( rockContext ).Queryable()
                .Where( a => a.Attribute.Key == "SecondaryAudiences" )
                .Where( a => a.Attribute.EntityTypeId == entityTypeIdContentChannelItem )
                .Where( a => a.Value.Contains( homepageRotatorAudienceGuid ) || a.Value.Contains( allChurchAudienceGuid ) )
                .Where( a => a.EntityId.HasValue )
                .Select( a => a.EntityId.Value );

            qry = qry.Where( a => primaryAudienceValueIds.Contains( a.Id ) || secondaryAudiencesValueIds.Contains( a.Id ) );

            var currentDateTime = RockDateTime.Now;

            // ExpireDateTime is blank OR ExpireDateTime > CurrentTime
            qry = qry.Where( a => a.ExpireDateTime == null || a.ExpireDateTime > currentDateTime );

            // StartDateTime <= CurrentTime
            qry = qry.Where( a => a.StartDateTime <= currentDateTime );

            // sort by Priority Ascending, and max Items
            var list = qry.OrderBy( a => a.Priority ).Take( maxItems ?? 6 ).ToList();


            foreach ( var item in list )
            {
                item.LoadAttributes( rockContext );

                // don't include Attributes, just AttributeValues
                item.Attributes = null;
            }

            // TODO
            return list;
        }
    }
}
