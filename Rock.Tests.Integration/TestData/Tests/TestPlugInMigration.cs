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
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.TestData
{
    /// <summary>
    /// A harness for testing plugin migrations that provides access to the Rock migration helper.
    /// 
    /// To test a new migration:
    /// 1. Add your test migration code in the Test_Up() and Test_Down() methods.
    /// 2. Run the unit test <see cref="LocalDbTestActions.TestPluginMigrations"> to execute the migration against the configured test database image.
    /// 3. After finalizing your migration, start the Rock application to confirm that the migration is successful.
    /// </summary>
    public class TestPlugInMigration : Rock.Plugin.Migration
    {
        public override void Up()
        {
            LogHelper.Log( "Executing Plugin Migration (Up)..." );

            Test_Up();
            Test_Down();
        }

        public override void Down()
        {
            LogHelper.Log( "Executing Plugin Migration (Down)..." );

            Test_Down();
        }

        private void Test_Up()
        {
            // Add your test migration code here...
        }

        private void Test_Down()
        {
            // Add your test migration code here...
        }
    }
}