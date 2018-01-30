// <copyright>
// Copyright by the Central Christian Church
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
using Rock.Plugin;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 17, "1.6.0" )]
    public class AddDetailPageToReservationList : Migration
    {
        public override void Up()
        {
            AddBlockAttributeValue( true, "4D4882F8-5ACC-4AE1-BC75-4FFDDA26F270", "3DD653FB-771D-4EE5-8C75-1BF1B6F773B8", @"4cbd2b96-e076-46df-a576-356bca5e577f,893ff97e-57d2-42e0-bf9a-6027d673773c" ); // Detail Page

        }
        public override void Down()
        {

        }

        // Temporarily copying a v7 method here
        public void AddBlockAttributeValue( bool skipIfAlreadyExists, string blockGuid, string attributeGuid, string value, bool appendToExisting = false )
        {
            var addBlockValueSQL = string.Format( @"

                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                IF @BlockId IS NOT NULL AND @AttributeId IS NOT NULL
                BEGIN

                    DECLARE @TheValue NVARCHAR(MAX) = '{2}'

                    -- If appendToExisting (and any current value exists), get the current value before we delete it...
                    IF 1 = {3} AND EXISTS (SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
                    BEGIN
                        SET @TheValue = (SELECT [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
                        -- If the new value is not in the old value, append it.
                        IF CHARINDEX( '{2}', @TheValue ) = 0
                        BEGIN
                            SET @TheValue = (SELECT @TheValue + ',' + '{2}' )
                        END
                    END

                    -- Delete existing attribute value first (might have been created by Rock system)
                    DELETE [AttributeValue]
                    WHERE [AttributeId] = @AttributeId
                    AND [EntityId] = @BlockId

                    INSERT INTO [AttributeValue] (
                        [IsSystem],[AttributeId],[EntityId],
                        [Value],
                        [Guid])
                    VALUES(
                        1,@AttributeId,@BlockId,
                        @TheValue,
                        NEWID())
                END
",
                    blockGuid,
                    attributeGuid,
                    value.Replace( "'", "''" ),
                    ( appendToExisting ? "1" : "0" )
                );

            if ( skipIfAlreadyExists )
            {
                addBlockValueSQL = $@"IF NOT EXISTS (
		SELECT *
		FROM [AttributeValue] av
		INNER JOIN [Attribute] a ON av.AttributeId = a.Id
		INNER JOIN [Block] b ON av.EntityId = b.Id
		WHERE b.[Guid] = '{blockGuid}'
			AND a.[Guid] = '{attributeGuid}'
		)
BEGIN
" + addBlockValueSQL + "\nEND";
            }

            Sql( addBlockValueSQL );
        }
    }
}
