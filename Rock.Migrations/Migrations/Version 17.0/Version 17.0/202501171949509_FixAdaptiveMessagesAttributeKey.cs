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
    public partial class FixAdaptiveMessagesAttributeKey : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixCallToActionAttributeUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            FixCallToActionAttributeDown();
        }

        #region KH: Fix the Call To Action Attribute Key for Adaptive Messages.

        private void FixCallToActionAttributeUp()
        {
            Sql( "UPDATE [Attribute] SET [Key]='CallToAction' WHERE [Guid]='9E67B39B-C95C-464B-AC7B-CB191834EF85'" );
        }

        private void FixCallToActionAttributeDown()
        {
            Sql( "UPDATE [Attribute] SET [Key]='CallToActionText' WHERE [Guid]='9E67B39B-C95C-464B-AC7B-CB191834EF85'" );
        }

        #endregion
    }
}
