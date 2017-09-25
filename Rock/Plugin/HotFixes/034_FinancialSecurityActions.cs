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
    [MigrationNumber( 34, "1.6.9" )]
    public class FinancialSecurityActions : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DECLARE @RockAdminGroupId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' )
    DECLARE @FinanceAdminGroupId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '6246A7EF-B7A3-4C8C-B1E4-3FF114B84559' )
    DECLARE @FinanceUsersGroupId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9' )

    -- Batch Entity Type 'Delete' Action
    DECLARE @EntityTypeIdFinancialBatch INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.FinancialBatch' )
    IF NOT EXISTS ( SELECT [Id] FROM [Auth] WHERE [EntityTypeId] = @EntityTypeIdFinancialBatch AND [EntityId] = 0 AND [Action] = 'Delete' )
    BEGIN
	    INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
        VALUES
            ( @EntityTypeIdFinancialBatch, 0, 0, 'Delete', 'A', 0, @FinanceUsersGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialBatch, 0, 1, 'Delete', 'A', 0, @FinanceAdminGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialBatch, 0, 2, 'Delete', 'A', 0, @RockAdminGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialBatch, 0, 3, 'Delete', 'D', 1, NULL, NEWID() )
    END

    -- Financial Transaction Entity Type 'Refund' Action
    DECLARE @EntityTypeIdFinancialTxn INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.FinancialTransaction' )
    IF NOT EXISTS ( SELECT [Id] FROM [Auth] WHERE [EntityTypeId] = @EntityTypeIdFinancialTxn AND [EntityId] = 0 AND [Action] = 'Refund' )
    BEGIN
	    INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
        VALUES
            ( @EntityTypeIdFinancialTxn, 0, 0, 'Refund', 'A', 0, @FinanceUsersGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialTxn, 0, 1, 'Refund', 'A', 0, @FinanceAdminGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialTxn, 0, 2, 'Refund', 'A', 0, @RockAdminGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialTxn, 0, 3, 'Refund', 'D', 1, NULL, NEWID() )
    END

	-- Transaction List Block 'Filter By Person' Action
    DECLARE @EntityTypeIdBlock INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block' )
    DECLARE @TransactionListBlockTypeId INT = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'E04320BC-67C3-452D-9EF6-D74D8C177154' )
    IF NOT EXISTS ( SELECT A.[Id] FROM [Auth] A INNER JOIN [Block] B ON B.[Id] = A.[EntityId] 
        WHERE EntityTypeId = @EntityTypeIdBlock AND B.[BlockTypeId] = @TransactionListBlockTypeId AND [Action] = 'FilterByPerson' )
    BEGIN
	
        INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    SELECT @EntityTypeIdBlock, [Id], 0, 'FilterByPerson', 'A', 0, @FinanceUsersGroupId, NEWID()
	    FROM [Block] WHERE [BlockTypeId] = @TransactionListBlockTypeId

        INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    SELECT @EntityTypeIdBlock, [Id], 1, 'FilterByPerson', 'A', 0, @FinanceAdminGroupId, NEWID()
	    FROM [Block] WHERE [BlockTypeId] = @TransactionListBlockTypeId

        INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    SELECT @EntityTypeIdBlock, [Id], 2, 'FilterByPerson', 'A', 0, @RockAdminGroupId, NEWID()
	    FROM [Block] WHERE [BlockTypeId] = @TransactionListBlockTypeId

        INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    SELECT @EntityTypeIdBlock, [Id], 3, 'FilterByPerson', 'D', 1, NULL, NEWID()
	    FROM [Block] WHERE [BlockTypeId] = @TransactionListBlockTypeId

    END
" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
