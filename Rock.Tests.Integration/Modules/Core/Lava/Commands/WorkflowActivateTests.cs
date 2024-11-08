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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Lava.Commands
{
    /// <summary>
    /// Tests for Lava-specific commands implemented as Liquid custom blocks and tags.
    /// </summary>
    [TestClass]
    public class WorkflowActivateTests : LavaIntegrationTestBase
    {
        private const string WorkflowTypeItSupportGuid = "51FE9641-FB8F-41BF-B09E-235900C3E53E";

        [TestMethod]
        public void WorkflowActivateBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% workflowactivate workflowtype:'$ItSupportWorkflowGuid' %}
  Activated new workflow with the id of #{{ Workflow.Id }}.
{% endworkflowactivate %}
";
            input = input.Replace( "$ItSupportWorkflowGuid", WorkflowTypeItSupportGuid );

            // TODO: If the security check fails, the content of the block is still returned with the error message.
            // Is this correct behavior, or should the content of the block be hidden?
            var expectedOutput = "The Lava command 'workflowactivate' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void WorkflowActivateBlock_WithSpecifiedWorkflowType_CreatesNewWorkflow()
        {
            // Activate Workflow: IT Support
            var input = @"
{% workflowactivate workflowtype:'$ItSupportWorkflowGuid' workflowname:'My IT Support Request' %}
  Activated new workflow with the name '{{ Workflow.Name }}'.
{% endworkflowactivate %}
";
            input = input.Replace( "$ItSupportWorkflowGuid", WorkflowTypeItSupportGuid );

            var expectedOutput = @"Activated new workflow with the name 'My IT Support Request'.";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void WorkflowActivateBlock_WithDelimiterInWorkflowName_EvaluatesWorkflowNameCorrectly()
        {
            var mergeFields = new LavaDataDictionary
            {
                { "WorkflowName", "Ted's Workflow" },
                { "ItSupportWorkflowTypeGuid", WorkflowTypeItSupportGuid }
            };

            var input = @"
{% workflowactivate workflowtype:'{{ItSupportWorkflowTypeGuid}}' workflowname:'{{WorkflowName}}' %}
  Activated new workflow with the name '{{ Workflow.Name }}'.
{% endworkflowactivate %}
";

            var expectedOutput = @"Activated new workflow with the name 'Ted's Workflow'.";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate", MergeFields = mergeFields };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void WorkflowActivateBlock_WithSpecifiedActivityType_SetsActivityToActive()
        {
            var workflow = CreateNewWorkflowInstance( WorkflowTypeItSupportGuid, "IT Support (Test)" );

            var workflowType = WorkflowTypeCache.Get( WorkflowTypeItSupportGuid );
            var workflowActivity = workflowType.ActivityTypes.FirstOrDefault( at => at.Name == "Assign Worker" );

            Assert.That.IsNotNull( workflowActivity );

            var mergeFields = new LavaDataDictionary
            {
                { "WorkflowId", workflow.Id },
                { "WorkflowActivityGuid", workflowActivity.Guid }
            };

            var input = @"
{% workflowactivate workflowid:{{ WorkflowId }} activitytype:'{{ WorkflowActivityGuid }}' %}
Workflow: {{ Workflow.Name }}
Activity: {{ Activity.ActivityType.Name }}
{% endworkflowactivate %}
";

            var expectedOutput = @"
Workflow: IT Support (Test)
Activity: Assign Worker
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate", MergeFields = mergeFields };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        private Rock.Model.Workflow CreateNewWorkflowInstance( string workflowTypeGuid, string name )
        {
            // Activate a new instance of Workflow: IT Support
            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );
            var workflow = Rock.Model.Workflow.Activate( workflowType, name );

            var rockContext = new RockContext();
            var workflowService = new WorkflowService( rockContext );

            workflowService.Add( workflow );
            rockContext.SaveChanges();

            return workflow;
        }

        [TestMethod]
        public void WorkflowActivateBlock_WorkflowActivatedWithAttributeParameters_PassesAttributeValuesCorrectly()
        {
            var input = @"
{% workflowactivate WorkflowType:'$ItSupportWorkflowGuid' summary:'Help!' details:'Now!' %}
    Summary: {{ Workflow | Attribute:'Summary' }}
    Details: {{ Workflow | Attribute:'Details' }}
{% endworkflowactivate %}
";

            input = input.Replace( "$ItSupportWorkflowGuid", WorkflowTypeItSupportGuid );
            var expectedOutput = @"
Summary: Help!
Details: Now!
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void WorkflowActivateBlock_ActivityActivatedWithAttributeParameters_PassesAttributeValuesCorrectly()
        {
            // Create a new instance of workflow "IT Support" and find the activity "Assign Worker".
            var workflow = CreateNewWorkflowInstance( WorkflowTypeItSupportGuid, "IT Support (Test)" );

            var workflowType = WorkflowTypeCache.Get( WorkflowTypeItSupportGuid );
            var activityType = workflowType.ActivityTypes.FirstOrDefault( at => at.Name == "Open" );

            Assert.That.IsNotNull( activityType );

            var mergeFields = new LavaDataDictionary
            {
                { "WorkflowId", workflow.Id },
                { "ActivityTypeGuid", activityType.Guid }
            };

            // Add a new instance of the "Assign Worker" activity to the workflow and activate it.
            var input = @"
{% workflowactivate workflowid:{{ WorkflowId }} activitytype:'{{ ActivityTypeGuid }}' SelectedAction:'Done' %}
Workflow: {{ Workflow.Name }}
Activity: {{ Activity.ActivityType.Name }}
Selected Action: {{ Activity | Attribute:'SelectedAction' }}
{% endworkflowactivate %}
";

            var expectedOutput = @"
Workflow: IT Support (Test)
Activity: Open
Selected Action: Done
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate", MergeFields = mergeFields };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void WorkflowActivateBlock_WithAttributeParameterNamesAsMixedCase_PassesAttributeValuesCorrectly()
        {
            var input = @"
{% workflowactivate WorkflowType:'$ItSupportWorkflowGuid' summary:'Test Workflow' DETAILS:'Here are the details...' %}
    Title: {{ Workflow | Attribute:'Summary' }}<br>
    Details: {{ Workflow | Attribute:'Details' }}
{% endworkflowactivate %}
";

            var expectedOutput = @"
Title: Test Workflow<br>
Details: Here are the details...
";
            input = input.Replace( "$ItSupportWorkflowGuid", WorkflowTypeItSupportGuid );

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void WorkflowActivateBlock_WithInvalidAttributeParameter_IgnoresInvalidAttribute()
        {
            var input = @"
{% workflowactivate WorkflowType:'$ItSupportWorkflowGuid' summary:'Help!' invalidattribute:'InvalidValue' %}
    Workflow: {{ Workflow.Name }}
    Summary: {{ Workflow | Attribute:'Summary' }}
    Invalid Attribute: {{ Workflow | Attribute:'InvalidValue' }}(empty)
{% endworkflowactivate %}
";
            input = input.Replace( "$ItSupportWorkflowGuid", WorkflowTypeItSupportGuid );

            var expectedOutput = @"
Workflow: IT Support
Summary: Help!
Invalid Attribute: (empty)
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }
    }
}
