// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class HistoryCategories : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add history category for event registrations
            Sql( @" DECLARE @CategoryEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Category')
    DECLARE @HistoryEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.History' )
    DECLARE @PersonCategoryId INT = ( SELECT [Id] FROM [Category] WHERE [Guid] = '6F09163D-7DDD-4E1E-8D18-D7CAA04451A7' )

    IF @HistoryEntityTypeId IS NOT NULL AND @PersonCategoryId IS NOT NULL
    BEGIN
        
        DECLARE @CategoryId INT
        DECLARE @CategoryOrder INT = ( SELECT MAX([Order]) FROM [Category] WHERE [ParentCategoryId] = @PersonCategoryId )
        INSERT INTO [Category] ([IsSystem], [ParentCategoryId], [EntityTypeId], [Name], [Guid], [Order]) VALUES
            (1, @PersonCategoryId, @HistoryEntityTypeId, 'Registration', 'DA9C0CC7-7B31-4E1E-BBA5-50405B2D9EFE' , COALESCE( @CategoryOrder + 1, 0) )
        SET @CategoryId = SCOPE_IDENTITY()

        DECLARE @RegistrationPageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'FC81099A-2F98-4EBA-AC5A-8300B2FE46C4')
        DECLARE @AttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @CategoryEntityTypeId AND [Key] = 'UrlMask' )

        IF @CategoryId IS NOT NULL AND @RegistrationPageId IS NOT NULL AND @AttributeId IS NOT NULL 
        BEGIN
            INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
            VALUES( 1, @AttributeId, @CategoryId,'~/page/' + CAST(@RegistrationPageId as varchar) + '?RegistrationId={0}', NEWID())

        END
    END");

            // add history category for person analytics
            Sql( @" DECLARE @CategoryEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Category')
    DECLARE @HistoryEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.History' )
    DECLARE @PersonCategoryId INT = ( SELECT [Id] FROM [Category] WHERE [Guid] = '6F09163D-7DDD-4E1E-8D18-D7CAA04451A7' )

    IF @HistoryEntityTypeId IS NOT NULL AND @PersonCategoryId IS NOT NULL
    BEGIN
        
        DECLARE @CategoryId INT
        DECLARE @CategoryOrder INT = ( SELECT MAX([Order]) FROM [Category] WHERE [ParentCategoryId] = @PersonCategoryId )
        INSERT INTO [Category] ([IsSystem], [ParentCategoryId], [EntityTypeId], [Name], [Guid], [Order]) VALUES
            (1, @PersonCategoryId, @HistoryEntityTypeId, 'Person Analytics', 'C1524D2E-3E8F-3D83-45F8-526B749D79F0' , COALESCE( @CategoryOrder + 1, 0) )
    END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete history category - event reg
            Sql( @"DELETE FROM [Category] WHERE [Guid] = 'DA9C0CC7-7B31-4E1E-BBA5-50405B2D9EFE'" );

            // delete history category - person analytics
            Sql( @"DELETE FROM [Category] WHERE [Guid] = 'C1524D2E-3E8F-3D83-45F8-526B749D79F0'" );
        }
    }
}
