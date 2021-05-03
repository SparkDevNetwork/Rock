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
    public partial class MyWellRename : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            Sql( @"
IF EXISTS (
		SELECT id
		FROM EntityType
		WHERE name = 'Rock.TransNational.Pi.PiGateway'
		)
BEGIN
	DELETE
	FROM [EntityType]
	WHERE [Name] = 'Rock.MyWell.MyWellGateway';

	UPDATE [EntityType]
	SET [Guid] = 'E81ED723-E807-4BDE-ADF1-AB9686241637'
		,[Name] = 'Rock.MyWell.MyWellGateway'
		,[FriendlyName] = 'My Well Gateway'
		,AssemblyName = NULL
	WHERE [Name] = 'Rock.TransNational.Pi.PiGateway';
END

IF NOT EXISTS (
		SELECT id
		FROM EntityType
		WHERE name = 'Rock.MyWell.MyWellGateway'
		)
BEGIN
	INSERT INTO [EntityType] (
		[Name]
		,[FriendlyName]
		,[IsEntity]
		,[IsSecured]
		,[IsCommon]
		,[Guid]
		)
	VALUES (
		'Rock.MyWell.MyWellGateway'
		,'My Well Gateway'
		,0
		,1
		,0
		,'E81ED723-E807-4BDE-ADF1-AB9686241637'
		)
END
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
