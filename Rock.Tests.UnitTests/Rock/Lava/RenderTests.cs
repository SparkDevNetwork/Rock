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
using Rock.Model;

namespace Rock.Tests.UnitTests.Lava
{
    /// <summary>
    /// Tests specific aspects of rendering in Lava that are not specifically defined in the Liquid language.
    /// </summary>
    [TestClass]
    public class RenderTests : LavaUnitTestBase
    {
        /// <summary>
        /// Enum types should render as a name rather than an integer value.
        /// </summary>
        [TestMethod]
        public void Render_EnumType_RendersAsEnumName()
        {
            var enumValue = ContentChannelItemStatus.Approved;

            var mergeValues = new LavaDataDictionary { { "EnumValue", enumValue } };

            TestHelper.AssertTemplateOutput( "Approved", "{{ EnumValue }}", mergeValues );
        }
    }
}
