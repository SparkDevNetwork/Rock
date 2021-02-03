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

namespace Rock.Tests.UnitTests.Lava
{
    [TestClass]
    public class LavaFileSystemTests : LavaUnitTestBase
    {
        #region Constructors

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            _helper.LavaEngine.RegisterSafeType( typeof( TestPerson ) );
            _helper.LavaEngine.RegisterSafeType( typeof( TestCampus ) );

        }

        #endregion

        /// <summary>
        /// Referencing a valid property of an input object should return the property value.
        /// </summary>
        [TestMethod]
        public void LavaFileSystem_SimpleFileAccess_ReturnsFileContents()
        {
            throw new System.Exception();

            System.Diagnostics.Debug.Print( _helper.GetTestPersonTedDecker().ToString() );

            var mergeValues = new LavaDictionary { { "CurrentPerson", _helper.GetTestPersonTedDecker() } };

            _helper.AssertTemplateOutput( "Decker", "{{ CurrentPerson.LastName }}", mergeValues );
        }
    }
}
