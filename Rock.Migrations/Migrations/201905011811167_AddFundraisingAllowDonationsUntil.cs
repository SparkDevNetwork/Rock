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
    public partial class AddFundraisingAllowDonationsUntil : Rock.Migrations.RockMigration
    {
        private const string AllowDonationsUntilGuid = "73AA96AC-BD64-4655-947D-AF6144F143CC";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGroupTypeGroupAttribute( SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY,
                SystemGuid.FieldType.DATE,
                "Allow Donations Until",
                "Donations to members of this group will be allowed up to and including the date specified.",
                0,
                string.Empty,
                AllowDonationsUntilGuid,
                true );

            Sql( $@"
DECLARE @FundraisingGroupTypeId int = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '4BE7FC44-332D-40A8-978E-47B7035D7A0C')
DECLARE @FundraisingGroupTypeIds TABLE ([Id] int)
DECLARE @OpportunityDateRangeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '237463F7-A206-4B43-AFDD-84E422527E87')
DECLARE @AllowDonationsUntilId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{AllowDonationsUntilGuid}')
DECLARE @Order int = (SELECT [Order] + 1 FROM [Attribute] WHERE [Guid] = '7C6FF01B-F68E-4A83-A96D-85071A92AAF1')
DECLARE @DefaultDate varchar(27) = '2025-12-31T00:00:00.000000'

-- Get all fundraising group type ids
;WITH CTE AS (
    SELECT [GroupTypeId],[ChildGroupTypeId] FROM [GroupTypeAssociation] WHERE [GroupTypeId] = @FundraisingGroupTypeId
    UNION ALL
    SELECT [a].[GroupTypeId],[a].[ChildGroupTypeId] FROM [GroupTypeAssociation] [a]
    JOIN CTE acte ON acte.[ChildGroupTypeId] = [a].[GroupTypeId]
    WHERE acte.[ChildGroupTypeId] <> acte.[GroupTypeId]
    -- and the child group type can't be a parent group type
    AND [a].[ChildGroupTypeId] <> acte.[GroupTypeId]
 )
INSERT INTO @FundraisingGroupTypeIds
SELECT [Id]
FROM [GroupType]
WHERE [Id] IN ( SELECT [ChildGroupTypeId] FROM CTE )

-- Update Order on the new attribute
UPDATE [Attribute]
    SET [Order] = @Order
WHERE [Id] = @AllowDonationsUntilId

-- Update Order on the Show Public and Registration Notes attributes
UPDATE [Attribute]
    SET [Order] = [Order] + 1
WHERE [Guid] IN ('BBD6C818-765C-43FB-AA72-5AF66F91B499', '7360CF56-7DF5-42E9-AD2B-AD839E0D4EDB') AND [Order] >= @Order

-- Insert default values for fundraising opportunities where we have an Opportunity Date
INSERT INTO [AttributeValue]
    ([AttributeId], [IsSystem], [EntityId], [Value], [CreatedDateTime], [Guid])
    SELECT
        @AllowDonationsUntilId,
        0,
        [EntityId],
        CASE
            WHEN CHARINDEX(',', [Value]) = 0 THEN @DefaultDate
            WHEN CHARINDEX(',', [Value]) = LEN([Value]) THEN @DefaultDate
            ELSE SUBSTRING([Value], CHARINDEX(',', [Value])+1, 99)
        END,
        GETDATE(),
        NEWID()
    FROM AttributeValue WHERE [AttributeId] = @OpportunityDateRangeId

-- Insert default values for fundraising opportunities where we don't have an Opportunity Date
INSERT INTO [AttributeValue]
    ([AttributeId], [IsSystem], [EntityId], [Value], [CreatedDateTime], [Guid])
    SELECT
        @AllowDonationsUntilId, 0, [Id], @DefaultDate, GETDATE(), NEWID()
    FROM [Group]
    WHERE [GroupTypeId] IN (SELECT [Id] FROM @FundraisingGroupTypeIds)
      AND [Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @OpportunityDateRangeId)
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( AllowDonationsUntilGuid );
        }
    }
}
