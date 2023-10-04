using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace org.lakepointe.Workflow.Prayer
{
    [ActionCategory("Prayer")]
    [Description("Creates a new prayer request.")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Prayer Request Create")]

    [WorkflowAttribute("Person", "The Person attribute that contains the person tha tthe prayer request should be creaed for.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonTypeField" })]
    [WorkflowAttribute("Prayer Category", "The category for the prayer requests.", true, "", "", 1, null, new string[] { "Rock.Field.Types.CategoryFieldType" })]
    [WorkflowAttribute("Is Public", "Is this a public prayer request.", false, "false", "", 2, null, new string[] { "Rock.Field.Types.BooleanFieldType" })]
    [WorkflowAttribute("Is Urgent", "Is this an urgent prayer request", false, "false", "", 3, null, new string[] { "Rock.Field.Types.BooleanFieldType" })]
    [WorkflowAttribute("Is Approved", "Is this request automatically approved.", false, "false", "", 4, null, new string[] { "Rock.Field.Types.BooleanFieldType" })]
    [WorkflowAttribute("Active Days", "How many days should this request be active. Default is 14 days.", false, "14", "", 5, null, new string[] { "Rock.Field.Types." })]
    [WorkflowAttribute("Campus", "Selected Campus", false, "", "", 6, null, new string[] { "Rock.Field.Types.CampusFieldType" })]
    [WorkflowAttribute("Request", "The prayer requests.", false, "", "", 7, null, new string[] { "Rock.Field.Types.MemoFieldType" })]

    public class CreatePrayerRequest : ActionComponent
    {
        public override bool Execute(RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            
            var personAliasGuid = action.GetWorklowAttributeValue(GetAttributeValue(action, "Person").AsGuid()).AsGuid();
            var personAlias = new PersonAliasService(rockContext).Get(personAliasGuid);

            return false;

        }
    }
}
