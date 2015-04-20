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
    public partial class RemoveUnknownMaritalStatus : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // DT: Remove the Unknown marital status
            Sql( @"
    DECLARE @UnknownMaritalStatusId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'D9CFD343-6A56-45F6-9E26-3269BA4FBC02' )
    UPDATE [Person] SET [MaritalStatusValueId] = NULL WHERE [MaritalStatusValueId] = @UnknownMaritalStatusId
    DELETE [DefinedValue] WHERE [Id] = @UnknownMaritalStatusId
");

            // JE: Moving Photo Requests Pages
            Sql( @"
  UPDATE [Page]
	SET [ParentPageId] = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '199DC522-F4D6-4D82-AF44-3C16EE9D2CDA')
	, [PageTitle] = 'Send Photo Requests'
	, [BrowserTitle] = 'Send Photo Requests'
	, [InternalName] = 'Send Photo Requests'
	, [IconCssClass] = 'fa fa-camera'
	, [Order] = 9
	WHERE [Guid] = 'B64D0429-488C-430E-8C32-5C7F32589F73'

	UPDATE [Page]
		SET [ParentPageId] = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '84FD84DF-F58B-4B9D-A407-96276C40AB7E')
		WHERE [Guid] = '325B50D6-545D-461A-9CB7-72B001E82F21'
" );

            // JE: Global Attribute 'Organization Abbreviation' missing category
            Sql( @"
  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '555306F1-6117-48B9-B184-D48DC1EC445F')
  DECLARE @CategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '89E54497-5E98-4F1B-B83A-95BFB685DA91')
  DECLARE @AttribCategoryExists int = (SELECT COUNT(*) FROM [AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId)
  
  IF (@AttribCategoryExists = 0)
  BEGIN
	  INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId])
	  VALUES (@AttributeId, @CategoryId)
  END
" );
            // JE: Hide the Rock Shop
            Sql( @"
  UPDATE [Page]
	SET [DisplayInNavWhen] = 2
	WHERE [Guid] = 'B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
