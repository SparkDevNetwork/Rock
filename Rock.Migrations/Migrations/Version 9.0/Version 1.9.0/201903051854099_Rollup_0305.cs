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
    public partial class Rollup_0305 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ReleaseNotification();
            InsertCategoryForExistingChildAttributes( "4ABF0BF2-49BA-4363-9D85-AC48A0F7E92A", "672715D8-F632-4CC7-B7DA-C65758438835" );
            InsertCategoryForExistingChildAttributes( "DBD192C9-0AA1-46EC-92AB-A3DA8E056D31", "672715D8-F632-4CC7-B7DA-C65758438835" );
            InsertCategoryForExistingChildAttributes( "F832AB6F-B684-4EEA-8DB4-C54B895C79ED", "672715D8-F632-4CC7-B7DA-C65758438835" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void ReleaseNotification()
        {
            Sql (@"UPDATE [Attribute] 
SET [Description] = N'As of Rock v10 only NoLegacy will be supported.
Old Lava syntax will be ignored and not loaded.' 
WHERE [Guid] = 'C8E30F2B-7476-4B02-86D4-3E5057F03FD5'");
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void InsertCategoryForExistingChildAttributes( string attributeGuid, string categoryGuid )
        {
            Sql( string.Format( @"
                    DECLARE @AttributeId int
                    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{0}')

                    DECLARE @CategoryId int
                    SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{1}')

                    IF (
		                    @AttributeId IS NOT NULL
		                    AND @CategoryId IS NOT NULL
		                    )
                    BEGIN
	                    IF NOT EXISTS (
		                    SELECT *
		                    FROM [AttributeCategory]
		                    WHERE [AttributeId] = @AttributeId
		                    AND [CategoryId] = @CategoryId )
	                    BEGIN
		                    INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
		                    VALUES( @AttributeId, @CategoryId )
	                    END
                    END",
                            attributeGuid,
                            categoryGuid ) );
        }
    }
}
