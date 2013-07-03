//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GroupTypesAttributeAsGuids : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // clear out any attribute values for GroupTypes FieldType, then repopulate the shipping ones
            Sql( @"
delete from [AttributeValue] where value != '' and [AttributeId] in (
SELECT [Id]
  FROM [dbo].[Attribute]
where FieldTypeId = (select [Id] from FieldType where [Guid] = 'F725B854-A15E-46AE-9D4C-0608D4154F1E')
)" );

            // Event List GroupTypes
            AddBlockAttributeValue( "A007FDC8-F3DE-457E-9097-68CD1355A454", "C3FD6CE3-D37F-4A53-B0D7-AB1B1F252324", "3311132b-268d-44e9-811a-a56a0835e50a" );

            // Event Detail GroupTypes
            AddBlockAttributeValue( "68D40E7D-EF38-4B53-8BD7-1D798F1C5B22", "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A", "3311132b-268d-44e9-811a-a56a0835e50a" );

            // delete dead 'Test Block"
            Sql( @"delete from [block] where [Guid] = 'D5D2048C-52C6-4827-A817-9B84E0525510'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // clear out any attribute values for GroupTypes FieldType, then repopulate the with the old shipping values (Ids)
            Sql( @"
delete from [AttributeValue] where value != '' and [AttributeId] in (
SELECT [Id]
  FROM [dbo].[Attribute]
where FieldTypeId = (select [Id] from FieldType where [Guid] = 'F725B854-A15E-46AE-9D4C-0608D4154F1E')
)" );
            // Event List GroupTypes
            AddBlockAttributeValue( "A007FDC8-F3DE-457E-9097-68CD1355A454", "C3FD6CE3-D37F-4A53-B0D7-AB1B1F252324", "13" );

            // Event Detail GroupTypes
            AddBlockAttributeValue( "68D40E7D-EF38-4B53-8BD7-1D798F1C5B22", "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A", "13" );
        }
    }
}
