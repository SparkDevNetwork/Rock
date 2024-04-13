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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Commands
{
    /// <summary>
    /// Tests for Lava-specific commands implemented as Liquid custom blocks and tags.
    /// </summary>
    [TestClass]
    public class WorkflowActivateTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void WorkflowActivateBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% workflowactivate workflowtype:'8fedc6ee-8630-41ed-9fc5-c7157fd1eaa4' %}
  Activated new workflow with the id of #{{ Workflow.Id }}.
{% endworkflowactivate %}
";

            // TODO: If the security check fails, the content of the block is still returned with the error message.
            // Is this correct behavior, or should the content of the block be hidden?
            var expectedOutput = "The Lava command 'workflowactivate' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void WorkflowActivateBlock_ActivateSupportWorkflow_CreatesNewWorkflow()
        {
            // Activate Workflow: IT Support
            var input = @"
{% workflowactivate workflowtype:'51FE9641-FB8F-41BF-B09E-235900C3E53E' %}
  Activated new workflow with the name '{{ Workflow.Name }}'.
{% endworkflowactivate %}
";

            var expectedOutput = @"Activated new workflow with the name 'IT Support'.";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void WorkflowActivateBlock_WithDelimiterInWorkflowName_EvaluatesWorkflowNameCorrectly()
        {
            var mergeFields = new LavaDataDictionary
            {
                { "WorkflowName", "Ted's Workflow" },
                { "ItSupportWorkflowTypeGuid", "51FE9641-FB8F-41BF-B09E-235900C3E53E" }
            };

            // Activate Workflow: IT Support
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
        public void WorkflowActivateBlock_WithAttributeParameterNamesAsMixedCase_PassesAttributeValuesCorrectly()
        {
            // Activate Workflow: IT Support
            var input = @"
{% workflowactivate WorkflowType:'51FE9641-FB8F-41BF-B09E-235900C3E53E' summary:'Test Workflow' DETAILS:'Here are the details...' %}
    Title: {{ Workflow | Attribute:'Summary' }}<br>
    Details: {{ Workflow | Attribute:'Details' }}
{% endworkflowactivate %}
";

            var expectedOutput = @"
Title: Test Workflow<br>
Details: Here are the details...
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }
    }
}
