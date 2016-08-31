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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class LoginExperienceUpdates : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"
    UPDATE AttributeValue
  SET [Value] = '<div class=''alert alert-info''>Enter your email address below and we''ll send your account information to you right away.</div>'
  WHERE [Guid] = '362598D2-D0EC-4995-BA50-CB52B3EC6E1C'

  UPDATE AttributeValue
  SET [Value] = 'Sorry, we could not find an account for the email address you entered.'
  WHERE [Guid] = 'FE2E6205-D888-4A4D-8B3C-38F2E0D8BA9B'

  UPDATE AttributeValue
  SET [Value] = 'Your user name has been sent with instructions on how to change your password if needed.'
  WHERE [Guid] = '88388EE1-20CD-446C-AA30-1FB8A6BC8B3C'

  UPDATE Page
  SET LayoutId = 4
  WHERE Id = 56
");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
