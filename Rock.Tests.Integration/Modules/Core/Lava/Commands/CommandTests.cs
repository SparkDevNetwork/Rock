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

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Model;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Commands
{
    /// <summary>
    /// Tests for Lava-specific commands implemented as Liquid custom blocks and tags.
    /// </summary>
    [TestClass]
    public class CommandTests : LavaIntegrationTestBase
    {
        #region Command Internals

        [TestMethod]
        public void Command_MultipleInstancesOfCustomBlock_ResolvesAllInstances()
        {
            var input = @"
{% javascript %}
    alert('Message 1');
{% endjavascript %}
{% javascript %}
    alert('Message 2');
{% endjavascript %}
";

            var expectedOutput = @"
<script>
    (function(){
        alert('Message 1');
    })();
</script>
<script>
    (function(){
        alert('Message 2');
    })();
</script>

";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #endregion

        #region Entity

        [TestMethod]
        public void EntityBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% person where: 'LastName == ""Decker""' %}
    {% for person in personItems %}
        {{ person.FullName }} < br />
    {% endfor %}
{% endperson %}
            ";

            var expectedOutput = "The Lava command 'rockentity' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void EntityBlock_WithNestedEntityBlock_ProducesExpectedOutput()
        {

            var input = @"
{% note expression:'NoteType.Name == ""Personal Note""' sort:'Id' limit:'3' %}
    {% for note in noteItems %}
        {% case note.NoteType.EntityType.FriendlyName %}
            {% when 'Person' %}
                {% person id:'{{ note.EntityId }}' %}
                    [{{person.FullName}}]: {{note.Text}}<br>
                {% endperson %}
        {% endcase %}
    {% endfor %}
{% endnote %}
";

            var expectedOutput = @"
[Ted Decker]: Talked to Ted today about starting a new Young Adults ministry<br>
[Ted Decker]: Called Ted and heard that his mother is in the hospital and could use prayer.<br>
[Daniel Peak]: Called Daniel to see if he would be interested in joining our team as the Communications Director.<br>
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "RockEntity" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void EntityBlock_PersonWhereLastNameIsDecker_ReturnsDeckers()
        {
            var input = @"
{% person where:'LastName == ""Decker""' %}
    {% for person in personItems %}
        {{ person.FullName }} <br/>
    {% endfor %}
{% endperson %}
            ";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var context = engine.NewRenderContext();

                context.SetEnabledCommands( "RockEntity" );

                var output = TestHelper.GetTemplateOutput( engine, input, context );

                TestHelper.DebugWriteRenderResult( engine, input, output );

                Assert.IsTrue( output.Contains( "Ted Decker" ), "Expected person not found." );
                Assert.IsTrue( output.Contains( "Cindy Decker" ), "Expected person not found." );
            } );
        }

        #endregion

        #region Execute

        [TestMethod]
        public void ExecuteBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% execute %}
    return ""Hello World!"";
{% endexecute %}
            ";

            var expectedOutput = "The Lava command 'execute' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ExecuteBlock_HelloWorld_ReturnsExpectedOutput()
        {
            var input = @"
{% execute %}
    return ""Hello World!"";
{% endexecute %}
            ";

            var expectedOutput = @"Hello World!";

            var options = new LavaTestRenderOptions() { EnabledCommands = "execute" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void ExecuteBlock_WithImports_ReturnsExpectedOutput()
        {
            var input = @"
{% execute import:'Newtonsoft.Json,Newtonsoft.Json.Linq' %}

    JArray itemArray = JArray.Parse( ``['Banana','Orange','Apple']`` );

    return ``Fruit: `` + itemArray[1];

{% endexecute %}
    ";

            input = input.Replace( "``", @"""" );

            var expectedOutput = @"Fruit: Orange";

            var options = new LavaTestRenderOptions() { EnabledCommands = "execute" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void ExecuteBlock_ClassType_ReturnsExpectedOutput()
        {
            var input = @"
{% execute type:'class' %}
    using Rock;
    using Rock.Data;
    using Rock.Model;
    
    public class MyScript 
    {
        public string Execute() {
            using(RockContext rockContext = new RockContext()) {
                var person = new PersonService(rockContext).Get(""<PersonGuid>"".AsGuid());
                
                return person.FullName;
            }
        }
    }
{% endexecute %}
";

            input = input.Replace( "<PersonGuid>", TestHelper.GetTestPersonTedDecker().Guid );

            var expectedOutput = @"Ted Decker";

            var options = new LavaTestRenderOptions() { EnabledCommands = "execute" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void ExecuteBlock_WithContextValues_ResolvesContextValuesCorrectly()
        {
            var input = @"
{% execute type:'class' %}
    using Rock;
    using Rock.Data;
    using Rock.Model;
    
    public class MyScript 
    {
        public string Execute() {
            using(RockContext rockContext = new RockContext()) {
                var person = new PersonService(rockContext).Get(""{{ Person | Property: 'Guid' }}"".AsGuid());
                
                return person.FullName;
            }
        }
    }
{% endexecute %}
";

            var expectedOutput = @"Ted Decker";

            var context = new LavaDataDictionary();

            context.Add( "Person", TestHelper.GetTestPersonTedDecker() );

            var options = new LavaTestRenderOptions() { EnabledCommands = "execute", MergeFields = context };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }
        #endregion

        #region InteractionWrite

        [TestMethod]
        public void InteractionWriteBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% interactionwrite channeltypemediumvalueid:'1' channelentityid:'1' channelname:'Some Channel' componententitytypeid:'1' interactionentitytypeid:'1' componententityid:'1' componentname:'Some Component' entityid:'1' operation:'View' summary:'Viewed Some Page' relatedentitytypeid:'1' relatedentityid:'1' channelcustom1:'Some Custom Value' channelcustom2:'Another Custom Value' channelcustomindexed1:'Some Indexed Custom Value'  personaliasid:'10' %}
    Here is the interaction data.
{% endinteractionwrite %}
";

            var expectedOutput = "The Lava command 'interactionwrite' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void InteractionWriteBlock_ForEntityCommandResult_IsCached()
        {
            var input = @"
{% interactionwrite channeltypemediumvalueid:'1' channelentityid:'1' channelname:'Some Channel' componententitytypeid:'1' interactionentitytypeid:'1' componententityid:'1' componentname:'Some Component' entityid:'1' operation:'View' summary:'Viewed Some Page' relatedentitytypeid:'1' relatedentityid:'1' channelcustom1:'Some Custom Value' channelcustom2:'Another Custom Value' channelcustomindexed1:'Some Indexed Custom Value' personaliasid:'10' %}
    Here is the interaction data.
{% endinteractionwrite %}
";

            var expectedOutput = @"
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "InteractionWrite" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void InteractionWriteBlock_ForMultipleChannelInteractions_WritesInteractionsToCorrectChannels()
        {
            var interaction1Summary = Guid.NewGuid().ToString();
            var interaction2Summary = Guid.NewGuid().ToString();
            var interaction3Summary = Guid.NewGuid().ToString();
            var input = @"
{% interactionwrite channeltypemediumvalueid:'1' channelname:'Channel 1' componentname:'Component 1' operation:'View' summary:'<Summary1>' %}
findme-interactiontest1
{% endinteractionwrite %}  
{% interactionwrite channeltypemediumvalueid:'1' channelname:'Channel 2' componentname:'Component 2' operation:'View' summary:'<Summary2>' %}
findme-interactiontest2
{% endinteractionwrite %}
{% interactionwrite channeltypemediumvalueid:'1' channelname:'Channel 2' componentname:'Component 1' operation:'View' summary:'<Summary3>' %}
findme-interactiontest3
{% endinteractionwrite %}";
            input = input
                .Replace( "<Summary1>", interaction1Summary )
                .Replace( "<Summary2>", interaction2Summary )
                .Replace( "<Summary3>", interaction3Summary );
            var expectedOutput = @"";
            var options = new LavaTestRenderOptions() { EnabledCommands = "InteractionWrite" };

            // Process the Lava template to create the interactions.
            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, input, options );

            // Process the transaction queue to ensure that the interactions are created.
            var exceptions = new List<Exception>();
            Rock.Transactions.RockQueue.Drain( ( e ) => { exceptions.Add( e ); } );
            Assert.IsTrue( exceptions.Count == 0, "Interaction transaction processing failed." );

            // Verify the interactions are assigned to the correct channel and component.
            Interaction interaction;
            interaction = GetInteractionBySummary( interaction1Summary );
            Assert.IsTrue( interaction.InteractionComponent.InteractionChannel.Name == "Channel 1" && interaction.InteractionComponent.Name == "Component 1" );
            interaction = GetInteractionBySummary( interaction2Summary );
            Assert.IsTrue( interaction.InteractionComponent.InteractionChannel.Name == "Channel 2" && interaction.InteractionComponent.Name == "Component 2" );
            interaction = GetInteractionBySummary( interaction3Summary );
            Assert.IsTrue( interaction.InteractionComponent.InteractionChannel.Name == "Channel 2" && interaction.InteractionComponent.Name == "Component 1" );
        }

        private Interaction GetInteractionBySummary( string summary )
        {
            var rockContext = new RockContext();
            var interactionService = new InteractionService( rockContext );
            var interactions = interactionService
                .Queryable()
                .Where( x => x.InteractionSummary == summary )
                .ToList();

            Assert.IsTrue( interactions.Count == 1, $"Expected Interaction not found. [Interaction Summary={summary}]" );

            return interactions.First();
        }

        #endregion

        #region InteractionContentChannelItemWrite

        [TestMethod]
        public void InteractionContentChannelItemWriteTag_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% interactioncontentchannelitemwrite contentchannelitemid:'1' operation:'View' summary:'Viewed content channel item #1' personaliasid:'10' %}
";

            var expectedOutput = "The Lava command 'interactioncontentchannelitemwrite' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void InteractionContentChannelItemWriteTag_ForEntityCommandResult_IsCached()
        {
            var input = @"
{% interactioncontentchannelitemwrite contentchannelitemid:'1' operation:'View' summary:'Viewed content channel item #1' personaliasid:'10' %}
";

            var expectedOutput = @"
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "InteractionContentChannelItemWrite" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region Javascript

        [TestMethod]
        public void JavascriptBlock_HelloWorld_ReturnsJavascriptScript()
        {
            var input = @"
{% javascript %}
    alert('Hello world!');
{% endjavascript %}
";

            var expectedOutput = @"
<script>
    (function(){
        alert('Hello world!');
    })();
</script>
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #endregion

        #region JsonProperty

        [TestMethod]
        public void JsonPropertyBlock_WithNumberType_EmitsJsonNumberProperty()
        {
            var input = @"
{% jsonproperty name:'mynumber' type:'number' %}
123
{% endjsonproperty %}
";

            var expectedOutput = @"`mynumber`:`123`"
                .Replace("`", @"""" );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #endregion

        #region Search

        [TestMethod]
        public void SearchBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% search query: 'ted decker' %}
    {% for result in results %}
        {{ result.DocumentName }}
    {% endfor %}
{% endsearch %}
";

            var expectedOutput = "The Lava command 'search' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        [Ignore("Add code to correctly set Universal Search component to disabled status.")]
        public void SearchBlock_UniversalSearchNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% search query:'ted decker' %}
    {% for result in results %}
        {{ result.DocumentName }}
    {% endfor %}
{% endsearch %}
";

            var expectedOutput = "Search results not available. Universal search is not enabled for this Rock instance.";

            var options = new LavaTestRenderOptions { EnabledCommands = "Search", OutputMatchType = LavaTestOutputMatchTypeSpecifier.Contains };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion Search

        #region SQL

        [TestMethod]
        public void SqlBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% sql %}
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
    ORDER BY [NickName]
{% endsql %}
";

            var expectedOutput = "The Lava command 'sql' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void SqlBlock_PersonWhereLastNameIsDecker_ReturnsDeckers()
        {
            var input = @"
{% sql %}
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
    ORDER BY [NickName]
{% endsql %}

{% for item in results %}{{ item.NickName }}_{{ item.LastName }};{% endfor %}
";

            var expectedOutput = @"Alex_Decker;Ted_Decker;";

            var options = new LavaTestRenderOptions { EnabledCommands = "Sql" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void SqlBlock_NullColumnValueInResult_IsRenderedAsEmptyString()
        {
            var input = @"
{% sql %}
    SELECT   [NickName], [LastName]
    FROM     [Person] 
    WHERE    [LastName] = 'Decker'
    AND      [NickName] IN ('Ted', 'Alex')
UNION
    SELECT   null as [NickName], null as [LastName]
    ORDER BY [NickName]
{% endsql %}

{% for item in results %}{{ item.NickName }}_{{ item.LastName }};{% endfor %}
";

            var expectedOutput = @"_;Alex_Decker;Ted_Decker;";

            var options = new LavaTestRenderOptions { EnabledCommands = "Sql" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// Verify that a nullable database column is mapped to a Nullable<> System.Type.
        /// </summary>
        [TestMethod]
        public void SqlBlock_NullableDatabaseColumn_IsReturnedAsNullableType()
        {
            var input = @"
{% sql return:'Items' %}
    SELECT TOP 1 p.[PhotoId] FROM [Person] as p WHERE [PhotoId] IS NULL
{% endsql %}
{% for item in Items %}
    {% if item.PhotoId == null %}
    Is Null
    {% endif %}
{% endfor %}
";

            var expectedOutput = @"Is Null";

            var options = new LavaTestRenderOptions { EnabledCommands = "Sql" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// Verify that the Select filter operates correctly on the result set returned by the Sql Lava block.
        /// Verifies Issue #4938 ⁃ Select Lava Filter Does Not See Values In A SQL Results Array.
        /// Refer https://github.com/SparkDevNetwork/Rock/issues/4938.
        /// </summary>
        [TestMethod]
        public void SqlBlock_SelectFilterAppliedToResultSet_ReturnsSelectedField()
        {
            var input = @"
{% sql %}
SELECT [FirstName], [LastName] FROM Person
WHERE [FirstName] IN ('Brian','Daniel','Nancy', 'William')
ORDER BY [FirstName]
{% endsql %}
{% assign firstNames = results | Select:'FirstName' | Uniq %}
{% for firstName in firstNames %}
{{ firstName }};
{% endfor %}
";

            var expectedOutput = @"
Brian;Daniel;Nancy;William;
";

            var options = new LavaTestRenderOptions { EnabledCommands = "Sql" };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region Stylesheet

        [TestMethod]
        public void StylesheetBlock_HelloWorld_ReturnsJavascriptScript()
        {
            var input = @"
{% stylesheet %}
#content-wrapper {
    background-color: red !important;
    color: #fff;
}
{% endstylesheet %}
";

            var expectedOutput = @"
<style>
    #content-wrapper {background-color:red!important;color:#fff;}
</style> 
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #endregion

        #region TagList

        [TestMethod]
        public void TagListTag_InTemplate_ReturnsListOfTags()
        {
            var input = @"
{% taglist %}
";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var result = engine.RenderTemplate( input );

                TestHelper.DebugWriteRenderResult( engine, input, result.Text );

                var output = result.Text.Replace( " ", string.Empty );

                if ( engine.GetType() == typeof ( RockLiquidEngine ) )
                {
                    Assert.IsTrue( output.Contains( "person-Rock.Lava.RockLiquid.Blocks.RockEntity" ), "Expected Entity Tag not found." );
                    Assert.IsTrue( output.Contains( "cache-Rock.Lava.RockLiquid.Blocks.Cache" ), "Expected Command Block not found." );
                    Assert.IsTrue( output.Contains( "interactionwrite-Rock.Lava.RockLiquid.Blocks.InteractionWrite" ), "Expected Command Tag not found." );
                }
                else
                {
                    Assert.IsTrue( output.Contains( "person-Rock.Lava.Blocks.RockEntity" ), "Expected Entity Tag not found." );
                    Assert.IsTrue( output.Contains( "cache-Rock.Lava.Blocks.Cache" ), "Expected Command Block not found." );
                    Assert.IsTrue( output.Contains( "interactionwrite-Rock.Lava.Blocks.InteractionWrite" ), "Expected Command Tag not found." );
                }
            } );
        }

        #endregion

        #region Web Request

        [TestMethod]
        public void WebRequestBlock_CommandNotEnabled_ReturnsConfigurationErrorMessage()
        {
            var input = @"
{% webrequest url:'https://api.github.com/repos/SparkDevNetwork/Rock/git/commits/88b33817b02b798679d75f237970649f25332fe1' return:'commit' %}
    {{ commit.message }}
{% endwebrequest %}  
";

            var expectedOutput = "The Lava command 'webrequest' is not configured for this template.";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void WebRequestBlock_GetRockRepoCommits_ReturnsValidResponse()
        {
            var input = @"
{% webrequest url:'https://api.github.com/repos/SparkDevNetwork/Rock/git/commits/88b33817b02b798679d75f237970649f25332fe1' return:'commit' %}
    {{ commit.message }}
{% endwebrequest %}  
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WebRequest" };

            TestHelper.AssertTemplateOutput( "readme", input, options );
        }

        #endregion

        #region WorkflowActivate

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
                { "WorkflowName", "Ted's Workflow" }
            };

            // Activate Workflow: IT Support
            var input = @"
{% workflowactivate workflowtype:'51FE9641-FB8F-41BF-B09E-235900C3E53E' workflowname:'{{WorkflowName}}' %}
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

        #endregion


        /// <summary>
        /// The "return" tag can be used at the root level of a document.
        /// </summary>
        [TestMethod]
        public void ReturnTag_AtRootLevel_TerminatesRenderingImmediately()
        {
            var template = @"
12345
{% return %}
67890
";

            var expectedOutput = @"12345";

            TestHelper.AssertTemplateOutput( expectedOutput, template );
        }

        /// <summary>
        /// The "return" tag can be used to exit immediately from a nested block.
        /// </summary>
        [TestMethod]
        public void ReturnTag_InNestedBlock_TerminatesRenderingImmediately()
        {
            var template = @"
{% assign isTrue = true %}
{% assign isFalse = false %}
Step 1
{% if isTrue == false %}
    {% return %}
{% endif %}
<hr>
Step 2
{% if isTrue == true %}
    {% return %}
{% endif %}
<hr>
Step 3
";

            var expectedOutput = @"Step1<hr>Step2";

            TestHelper.AssertTemplateOutput( expectedOutput, template );
        }

        /// <summary>
        /// The "return" tag can be used to exit immediately from within a loop.
        /// </summary>
        [TestMethod]
        public void ReturnTag_InForLoop_TerminatesDocumentRenderImmediately()
        {
            var template = @"
{% assign list = '10,9,8,7,6,5,4,3,2,1' | Split: ',' %}
{% for i in list %}{{ i }}...{% if i == 1 %}{% return %}{% endif %}{% endfor %}
Lift-Off!
";

            var expectedOutput = @"10...9...8...7...6...5...4...3...2...1...";

            TestHelper.AssertTemplateOutput( expectedOutput, template );
        }
    }
}
