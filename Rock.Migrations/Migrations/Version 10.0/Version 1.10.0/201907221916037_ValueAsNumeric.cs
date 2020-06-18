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
    public partial class ValueAsNumeric : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Drop ValueAsNumeric computed column, then re-add it as a non-computed column
            Sql( @"
IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_ValueAsNumeric'
            AND object_id = OBJECT_ID('AttributeValue')
        )
BEGIN
    DROP INDEX [IX_ValueAsNumeric] ON [AttributeValue]
END
" );
            Sql( @"ALTER TABLE [AttributeValue] DROP COLUMN [ValueAsNumeric]" );

            Sql( @"ALTER TABLE dbo.AttributeValue ADD ValueAsNumeric decimal(18, 2) NULL" );

            Sql( @"CREATE NONCLUSTERED INDEX [IX_ValueAsNumeric] ON [dbo].[AttributeValue] ([ValueAsNumeric] ASC)" );

            AddJobToUpdateAttributeValueValueAsNumericCalculation();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // no need for Down() since going back Up() again will still work
        }

        /// <summary>
        /// MP: Fixup AttributeValue.ValueAsNumeric to work with negative numbers
        /// </summary>
        private void AddJobToUpdateAttributeValueValueAsNumericCalculation()
        {
            Sql( @"IF NOT EXISTS (
  SELECT[Id]
  FROM[ServiceJob]
  WHERE[Class] = 'Rock.Jobs.PostV100DataMigrationsValueAsNumeric'
   AND[Guid] = '0A7573C9-D977-4A7E-BDD6-66DD36CBF6F3'
  )
BEGIN
 INSERT INTO[ServiceJob](
  [IsSystem]
  ,[IsActive]
  ,[Name]
  ,[Description]
  ,[Class]
  ,[CronExpression]
  ,[NotificationStatus]
  ,[Guid]
  )
 VALUES(
  0
  ,1
  ,'Rock Update Helper v10.0 - AttributeValue.ValueAsNumeric'
  ,'Runs data updates to AttributeValue.ValueAsNumeric that need to occur after updating to v10.0.'
  ,'Rock.Jobs.PostV100DataMigrationsValueAsNumeric'
  ,'0 0 21 1/1 * ? *'
  ,1
  ,'0A7573C9-D977-4A7E-BDD6-66DD36CBF6F3'
  );
        END" );
        }
    }
}
