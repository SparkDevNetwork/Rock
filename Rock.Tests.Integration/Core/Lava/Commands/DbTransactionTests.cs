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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Lava;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Integration.Core.Lava.Commands
{
    /// <summary>
    /// Tests for the dbtransaction Lava block working with the entity modify commands.
    /// </summary>
    [TestClass]
    public class DbTransactionTests : LavaIntegrationTestBase
    {
        private const string AllergyAttributeKey = "Allergy";

        /// <summary>
        /// A rolled back transaction must undo both the entity row change and the
        /// attribute value change made by a modify command inside it.
        /// </summary>
        [TestMethod]
        public void DbTransaction_WithForceRollback_RollsBackEntityPropertyAndAttributeValue()
        {
            var personId = GetTedDeckerPersonId();
            var original = GetPersonValues( personId );
            var newMiddleName = "Mid" + Guid.NewGuid().ToString( "N" ).Substring( 0, 8 );
            var newAllergy = "Allergy " + Guid.NewGuid().ToString( "N" );

            var template = $@"
{{% dbtransaction forcerollback:'true' %}}
    {{% modifyperson id:'{personId}' %}}
        [[ property name:'MiddleName' ]]{newMiddleName}[[ endproperty ]]
        [[ attribute key:'{AllergyAttributeKey}' ]]{newAllergy}[[ endattribute ]]
    {{% endmodifyperson %}}
{{% enddbtransaction %}}";

            try
            {
                TestHelper.ExecuteForActiveEngines( engine =>
                {
                    TestHelper.GetTemplateOutput( engine, template,
                        new LavaTestRenderOptions { EnabledCommands = "RockEntityModify" } );

                    var after = GetPersonValues( personId );

                    Assert.AreEqual( original.MiddleName, after.MiddleName, "The entity property was not rolled back." );
                    Assert.AreEqual( original.Allergy, after.Allergy, "The attribute value was not rolled back." );
                } );
            }
            finally
            {
                RestorePersonValues( personId, original );
            }
        }

        /// <summary>
        /// A committed transaction must keep both the entity row change and the
        /// attribute value change made by a modify command inside it.
        /// </summary>
        [TestMethod]
        public void DbTransaction_WithCommit_SavesEntityPropertyAndAttributeValue()
        {
            var personId = GetTedDeckerPersonId();
            var original = GetPersonValues( personId );
            var newMiddleName = "Mid" + Guid.NewGuid().ToString( "N" ).Substring( 0, 8 );
            var newAllergy = "Allergy " + Guid.NewGuid().ToString( "N" );

            var template = $@"
{{% dbtransaction %}}
    {{% modifyperson id:'{personId}' %}}
        [[ property name:'MiddleName' ]]{newMiddleName}[[ endproperty ]]
        [[ attribute key:'{AllergyAttributeKey}' ]]{newAllergy}[[ endattribute ]]
    {{% endmodifyperson %}}
{{% enddbtransaction %}}";

            try
            {
                TestHelper.ExecuteForActiveEngines( engine =>
                {
                    TestHelper.GetTemplateOutput( engine, template,
                        new LavaTestRenderOptions { EnabledCommands = "RockEntityModify" } );

                    var after = GetPersonValues( personId );

                    Assert.AreEqual( newMiddleName, after.MiddleName, "The entity property was not saved." );
                    Assert.AreEqual( newAllergy, after.Allergy, "The attribute value was not saved." );
                } );
            }
            finally
            {
                RestorePersonValues( personId, original );
            }
        }

        #region Support Methods

        /// <summary>
        /// Gets the identifier of the Ted Decker sample person.
        /// </summary>
        private static int GetTedDeckerPersonId()
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var person = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker.AsGuid() );

                Assert.IsNotNull( person, "The Ted Decker sample person was not found." );

                return person.Id;
            }
        }

        /// <summary>
        /// Reads the current values from the database using a fresh context so
        /// nothing cached by the render is seen.
        /// </summary>
        private static (string MiddleName, string Allergy) GetPersonValues( int personId )
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var person = new PersonService( rockContext ).Get( personId );

                person.LoadAttributes( rockContext );

                return (person.MiddleName, person.GetAttributeValue( AllergyAttributeKey ));
            }
        }

        /// <summary>
        /// Puts the person back the way it was so other tests are not affected.
        /// </summary>
        private static void RestorePersonValues( int personId, (string MiddleName, string Allergy) values )
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var person = new PersonService( rockContext ).Get( personId );

                person.MiddleName = values.MiddleName;
                rockContext.SaveChanges();

                person.LoadAttributes( rockContext );
                person.SetAttributeValue( AllergyAttributeKey, values.Allergy );
                person.SaveAttributeValues( rockContext );
            }
        }

        #endregion
    }
}
