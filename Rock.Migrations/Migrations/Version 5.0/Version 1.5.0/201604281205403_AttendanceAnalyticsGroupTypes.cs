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
    public partial class AttendanceAnalyticsGroupTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", Rock.SystemGuid.FieldType.GROUP_TYPES, "Group Types", "GroupTypes", "",
                "Optional List of specific group types that should be included. If none are selected, an option to select an attendance type will be displayed and all of that attendance type's areas will be available.",
                0, @"", "69268CB9-AF6D-4CCD-952A-F8D5DAE1B4BF" );

            Sql( @"
    DECLARE @OldAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '6CD6EDD9-5DDD-4EAA-9447-A7B61091754D' )
    DECLARE @NewAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '69268CB9-AF6D-4CCD-952A-F8D5DAE1B4BF' )

    IF @OldAttributeId IS NOT NULL AND @NewAttributeId IS NOT NULL
    BEGIN

	    DECLARE @AttributeValueId int	
	    DECLARE @AttributeValue varchar(max)

	    DECLARE ValueCursor INSENSITIVE CURSOR FOR
	    SELECT [Id], [Value]
	    FROM [AttributeValue] 
	    WHERE [AttributeId] = @OldAttributeId

	    OPEN ValueCursor
	    FETCH NEXT FROM ValueCursor
	    INTO @AttributeValueId, @AttributeValue

	    WHILE (@@FETCH_STATUS <> -1)
	    BEGIN

		    IF (@@FETCH_STATUS = 0)
		    BEGIN

			    DECLARE @NewValue varchar(max)

			    SELECT @NewValue = COALESCE( @NewValue + ',', '' ) + LOWER(CAST(C.[Guid] AS VARCHAR(40)))
			    FROM [GroupType] T
			    INNER JOIN [GroupTypeAssociation] A ON A.[GroupTypeId] = T.[Id]
			    INNER JOIN [GroupType] C ON C.[Id] = A.[ChildGroupTypeId]
			    WHERE CAST(T.[Guid] AS VARCHAR(40)) = @AttributeValue
		
			    UPDATE [AttributeValue] SET
				    [AttributeId] = @NewAttributeId,
				    [Value] = @NewValue
			    WHERE [Id] = @AttributeValueId

		    FETCH NEXT FROM ValueCursor
		    INTO @AttributeValueId, @AttributeValue

		    END
	
	    END

	    CLOSE ValueCursor
	    DEALLOCATE ValueCursor

    END
" );

            Sql( MigrationSQL._201604281205403_AttendanceAnalyticsGroupTypes_GroupTypeAttendance );
            Sql( MigrationSQL._201604281205403_AttendanceAnalyticsGroupTypes_AttendeeFirstDates );
            Sql( MigrationSQL._201604281205403_AttendanceAnalyticsGroupTypes_NonAttendee );

            // DT: Add blank NMI Gateway
            Sql( @"
    DECLARE @NMIGatewayEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'B8282486-7866-4ED5-9F24-093D25FF0820' )
    IF @NMIGatewayEntityTypeId IS NOT NULL AND NOT EXISTS ( SELECT [Id] FROM [FinancialGateway] WHERE [EntityTypeId] = @NMIGatewayEntityTypeId )
    BEGIN
	    INSERT INTO [FinancialGateway] ( [Name], [EntityTypeId], [BatchTimeOffsetTicks], [IsActive], [Guid] )
	    VALUES ( 'Network Merchants (NMI)', @NMIGatewayEntityTypeId, 0, 0, NEWID() )
    END
" );

            // JE: Make GroupSync Job non-system
            Sql( @"
  UPDATE [ServiceJob] SET [IsSystem] = 0
  WHERE [Guid] = '57B539BC-7C4D-25BB-4EEB-39DF0EF62EBC'
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
