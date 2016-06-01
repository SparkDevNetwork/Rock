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
    public partial class NoteContextFix : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

    DECLARE @AttributeID int
    SET @AttributeID = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'F1BCF615-FBCA-4BC2-A912-C35C0DC04174')

    DECLARE @BlockID int
    SET @BlockID = (SELECT [Id] FROM [Block] WHERE [Guid] = '0B2B550C-B0C9-420E-9CF3-BEC8979108F2')

    DELETE [AttributeValue] WHERE [AttributeId] = @AttributeID AND [EntityId] = @BlockID
    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Order],[Value],[Guid])
    VALUES (1, @AttributeID, @BlockID, 0, '72657ED8-D16E-492E-AC12-144C5E7567E7', '1A1B58C4-C461-40EC-9C10-724A18347183')

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
