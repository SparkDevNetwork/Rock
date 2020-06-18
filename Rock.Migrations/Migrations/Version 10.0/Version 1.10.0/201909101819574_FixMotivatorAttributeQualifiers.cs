﻿// <copyright>
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
    public partial class FixMotivatorAttributeQualifiers : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateAttributeQualifierValueDefinedTypeIDs();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// ED: Correct the DefinedType ID being used for Motivator Theme and Top 5 Motivators AttributeQualifiers
        /// </summary>
        private void UpdateAttributeQualifierValueDefinedTypeIDs()
        {
            Sql( @"
                -- Fix the Motivator Theme attribute qualifier for Theme DefinedType (6C084DB5-5EC0-4E73-BAE7-775AE429C852)
                DECLARE @ThemeAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = 'A20E6DB1-B830-4D41-9003-43A184E4C910')
                DECLARE @MotivatorsThemeDefinedTypeId INT = (SELECT [ID] FROM [dbo].[DefinedType] WHERE [Guid] = '354715FA-564A-420A-8324-0411988AE7AB')

                IF NOT EXISTS(SELECT * FROM [dbo].[AttributeQualifier] WHERE [Key] = 'definedtype' AND [AttributeId] = @ThemeAttributeId)
                BEGIN
                    INSERT INTO [dbo].[AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                    VALUES(1, @ThemeAttributeId, 'definedtype', @MotivatorsThemeDefinedTypeId, '6C084DB5-5EC0-4E73-BAE7-775AE429C852')
                END
                ELSE
                BEGIN
                    UPDATE [dbo].[AttributeQualifier]
	                SET [Value] = @MotivatorsThemeDefinedTypeId
                    WHERE [Key] = 'definedtype' AND [AttributeId] = @ThemeAttributeId
                END

                -- Fix the Top 5 Motivators attribute qualifier for Motivator Defined Type (E3ADC996-626F-460B-9F20-635EE5FFF881)
                DECLARE @TopFiveMotivatorsAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '402308F6-44BB-46CF-ADF9-6F62406C9923')
                DECLARE @MotivatorTypeDefinedTypeId INT = (SELECT [Id] FROM [dbo].[DefinedType] WHERE [Guid] = '1DFF1804-0055-491E-9559-54EA3F8F89D1')

                IF NOT EXISTS(SELECT * FROM [dbo].[AttributeQualifier] WHERE [Key] = 'definedtype' AND [AttributeId] = @TopFiveMotivatorsAttributeId )
                BEGIN
                    INSERT INTO [dbo].[AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
                    VALUES(1, @TopFiveMotivatorsAttributeId, 'definedtype', @MotivatorTypeDefinedTypeId, 'E3ADC996-626F-460B-9F20-635EE5FFF881')
                END
                ELSE
                BEGIN
                    UPDATE [dbo].[AttributeQualifier]
	                SET [Value] = @MotivatorTypeDefinedTypeId
                    WHERE [Key] = 'definedtype' AND [AttributeId] = @TopFiveMotivatorsAttributeId
                END" );
        }
    }
}
