<%@ WebHandler Language="C#" Class="SmsToWorkflow" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

public class SmsToWorkflow : IHttpHandler
{    
    private HttpRequest request;
    private HttpResponse response;

    public void ProcessRequest( HttpContext context )
    {
        request = context.Request;
        response = context.Response;

        response.ContentType = "text/plain";

        if ( request.HttpMethod != "POST" )
        {
            response.Write( "Invalid request type." );
            return;
        }

        if ( request.Form["SmsStatus"] != null )
        {
            
            IDictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var k in request.Form.AllKeys)
            { 
                dict.Add(k, request.Form[k]);
            }
            
            dict.Add("RawPost", request.Form.ToJson());
            dict.Add("Webhook", request.Url.PathAndQuery);

            launchWorkflow(new Guid(Rock.Web.Cache.GlobalAttributesCache.Read().GetValue("SMSToWorkflowWorkflowType")), dict);

            response.StatusCode = 200;
        }
        else
        {
            response.StatusCode = 500;
        }

    }
    
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }


    protected void launchWorkflow(Guid workflowGuid, IDictionary<string, string> attributes)
    {
        
        var _rockContext = new RockContext();
        
        

        WorkflowService _workflowService = new WorkflowService(_rockContext);
        WorkflowTypeService _workflowTypeService = new WorkflowTypeService(_rockContext);
        WorkflowType _workflowType = _workflowTypeService.Get(workflowGuid);

        Workflow _workflow = Rock.Model.Workflow.Activate(_workflowType, "New Test" + _workflowType.WorkTerm);


        foreach (KeyValuePair<string, string> attribute in attributes)
        {
            _workflow.SetAttributeValue(attribute.Key, attribute.Value);
        }


        List<string> errorMessages;
        if (_workflowService.Process(_workflow, out errorMessages))
        {
            // If the workflow type is persisted, save the workflow
            if (_workflow.IsPersisted || _workflowType.IsPersisted)
            {
                if (_workflow.Id == 0)
                {
                    _workflowService.Add(_workflow);
                }

                _rockContext.WrapTransaction(() =>
                {
                    _rockContext.SaveChanges();
                    _workflow.SaveAttributeValues(_rockContext);
                    foreach (var activity in _workflow.Activities)
                    {
                        activity.SaveAttributeValues(_rockContext);
                    }
                });

            }
        }
    }
}