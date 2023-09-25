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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Jobs;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Integration.Core.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.BugFixes
{
    /// <summary>
    /// Tests that verify specific bug fixes for a Rock version.
    /// </summary>
    /// <remarks>
    /// These tests are developed to verify bugs and fixes that are difficult or time-consuming to reproduce.
    /// They are only relevant to the Rock version in which the bug is fixed, and should be removed in subsequent versions.
    /// </remarks>
    /// 
    [TestClass]
    [RockObsolete( "1.16" )]
    public class BugFixVerificationTests_v16 : LavaIntegrationTestBase
    {
        /// <summary>
        /// Verifies the resolution of Issue #5389.
        /// </summary>
        [TestMethod]
        public void Issue5389_ChildContentChannelsProperty_IsAvailableInLava()
        {
            /* Issue:
             * The ContentChannel.ChildContentChannels property is visible in the Model Map,
             * but is not consistently accessible in Lava.
             * For details, see https://github.com/SparkDevNetwork/Rock/issues/5389.
             * 
             * Resolution:
             * The ChildContentChannels property should be accessible in Lava using
             * standard property notation.
             */

            var input = @"
{% contentchannel where:'[Name] == ""Messages""' %}
    Dot Notation: {{ contentchannel.ChildContentChannels | Size }}<br>
    {% assign childChannels = contentchannel | Property:'ChildContentChannels' %}
    Property Filter: {{ childChannels | Size }}
{% endcontentchannel %}
";

            var expectedOutput = @"
Dot Notation: 1
<br>
Property Filter: 1
";

            var options = new LavaTestRenderOptions
            {
                EnabledCommands = "RockEntity"
            };
            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// Verifies the resolution of a specific Issue.
        /// </summary>
        [TestMethod]
        public void Issue5560_LavaCommentsDisplayedInOutput()
        {
            /* The Lava Engine may render inline comments to output where an unmatched quote delimiter is present in the preceding template text.
             * For details, see https://github.com/SparkDevNetwork/Rock/issues/5560.
             * 
             * Resolution: This issue is caused by the inadequacy of Regex to encapsulate the complex logic required to identify comments vs literal text.
             * This issue has been fixed for the Fluid Engine by implementing shorthand comments in the custom parser.
             * A fix for DotLiquid would require additional work to replace Regex with a custom parser to strip comments from the source template,
             * which is not justified because the DotLiquid engine will be removed in v17.
             */

            var engineOptions = new LavaEngineConfigurationOptions
            {
                InitializeDynamicShortcodes = false
            };
            var engine = LavaService.NewEngineInstance( typeof( FluidEngine ), engineOptions );

            LavaIntegrationTestHelper.SetEngineInstance( engine );

            var template = @"
<h3>Testing issue 5560</h3>

{% comment %}By Jim M...{% endcomment %}
{% comment %}By Stan Y...{% endcomment %}
{% comment %} By Jim M Jan 2021. This block gets a person's Explo Online group, Zoom Link, schedule, and Leader details.{% endcomment %} 

/- GroupType 67 = Explo Online - assume person is in only 1 group of this type -/
Did you see those comments ^^^

{% assign groupMember = CurrentPerson | Groups: ""67"" | First %}
{% assign grp = groupMember.Group.Id | GroupById %}

            //- proceed if we found a group

            {% if grp != null and grp != empty %}
    < b > Welcome...</ b >
{% endif %}
";
            var expectedOutput = @"
<h3>Testing issue 5560</h3>
Did you see those comments ^^^
";
            var actualOutput = LavaService.RenderTemplate( template ).Text;

            Assert.That.AreEqualIgnoreWhitespace( expectedOutput, actualOutput );
        }
    }
}
