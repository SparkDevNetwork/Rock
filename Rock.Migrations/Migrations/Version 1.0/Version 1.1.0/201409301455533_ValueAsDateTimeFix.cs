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
    public partial class ValueAsDateTimeFix : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropColumn( "dbo.AttributeValue", "ValueAsDateTime" );
            DropColumn( "dbo.AttributeValue", "ValueAsNumeric" );
            Sql( @"alter table AttributeValue add ValueAsDateTime as case when (len(value) < 50 and ISDATE( value) = 1) then convert(datetime, value) else null end" );
            Sql( @"alter table AttributeValue add ValueAsNumeric as case when (len(value) < 100 and ISNUMERIC( value) = 1 and value not like '%[^0-9.]%') then convert(numeric(38,10), value ) else null end" );

            // Clean up Person records that have null values for some things
            Sql( @"
declare
  @ConnectionStatusValueId int = (select top 1 [Id] from [DefinedValue] where [Guid] = 'B91BA046-BC1E-400C-B85D-638C1F4E0CE2' /* Visitor */) 
  ,@RecordTypePersonId int = (select top 1 [Id] from [DefinedValue] where [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' /* Person */) 
  ,@MaritalStatusValueId int = (select top 1 [Id] from [DefinedValue] where [Guid] = 'D9CFD343-6A56-45F6-9E26-3269BA4FBC02' /* Unknown */) 
  ,@IsEmailActive int = 0

update Person set ConnectionStatusValueId = @ConnectionStatusValueId where ConnectionStatusValueId is null;
update Person set RecordTypeValueId = @RecordTypePersonId where RecordTypeValueId is null;
update Person set MaritalStatusValueId = @MaritalStatusValueId where MaritalStatusValueId is null;
update Person set IsEmailActive = @IsEmailActive where IsEmailActive is null;
" );

            // ensure Person records have a PersonAlias
            Sql( @"INSERT INTO [PersonAlias] ([PersonId],[AliasPersonId],[AliasPersonGuid],[Guid])
    SELECT [Id], [Id], [Guid], NEWID() FROM [Person] p where p.Id not in (select PersonId from PersonAlias where PersonId = AliasPersonId)" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
