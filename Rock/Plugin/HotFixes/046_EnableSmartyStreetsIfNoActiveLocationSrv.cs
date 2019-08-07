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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 46, "1.7.0" )]
    public class EnableSmartyStreetsIfNoActiveLocationSrv : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //Sql( @"-- First see if any location services are enabled
            //    DECLARE @EnabledLocationServicesCount INT =
	           //     (SELECT COUNT(*)
	           //     FROM [EntityType] t
	           //     JOIN [Attribute] a on t.[Id] = a.[EntityTypeId]
	           //     JOIN [AttributeValue] v on a.[Id] = v.[AttributeId]
	           //     WHERE t.[Name] LIKE 'Rock.Address.%'
		          //      AND a.[Key] = 'Active'
		          //      AND v.[Value] = 'True')

            //    -- If nothing is enabled then find the SmartyStreets AttributeValue and set it to True
            //    IF (@EnabledLocationServicesCount = 0)
            //    BEGIN
	           //     DECLARE @SmartyStreetsAttributeValueId INT =
		          //      (SELECT v.[Id]
		          //      FROM [EntityType] t
		          //      JOIN [Attribute] a ON t.[Id] = a.[EntityTypeId]
		          //      JOIN [AttributeValue] v ON a.[Id] = v.[AttributeId]
		          //      WHERE t.[Name] = 'Rock.Address.SmartyStreets'
			         //       AND a.[Key] = 'Active')

	           //     -- If an attibute value already exists then update
	           //     IF (@SmartyStreetsAttributeValueId > 0)
	           //     BEGIN
		          //      UPDATE [AttributeValue]
		          //      SET [Value] = 'True'
		          //      WHERE [Id] = @SmartyStreetsAttributeValueId
	           //     END
	           //     ELSE
	           //     BEGIN
	           //     -- Otherwise insert
		          //      DECLARE @SmartyStreetsEntityId INT =
			         //       (SELECT [Id]
			         //       FROM [EntityType]
			         //       WHERE [Name] = 'Rock.Address.SmartyStreets')

		          //      DECLARE @SmartyStreetsAttributeId INT =
			         //       (SELECT [Id]
			         //       FROM [Attribute]
			         //       WHERE [EntityTypeId] = @SmartyStreetsEntityId
					       //         AND [Key] = 'Active')

		          //      IF (@SmartyStreetsAttributeId > 0)
		          //      BEGIN
			         //       INSERT INTO [AttributeValue]([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
			         //       VALUES (
				        //        0 --[IsSystem]
				        //        , @SmartyStreetsAttributeId --[AttributeId]
				        //        , 0 --[EntityId]
				        //        , 'True' --[Value]
				        //        , NEWID()) --[Guid]
		          //      END
	           //     END
            //    END" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}
