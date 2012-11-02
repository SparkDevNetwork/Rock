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
    public partial class MarketingSystemData : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Event Attendees Group Type
            Sql( @"INSERT INTO [dbo].[crmGroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[DefaultGroupRoleId]
           ,[Guid])
     VALUES
           (1
           ,'Event Attendees'
           ,'Event Attendees'
           ,null
           ,'3311132B-268D-44E9-811A-A56A0835E50A')" );

            AddDefinedType( "Marketing Campaign", "Audience Type", "Audience Type", "799301A3-2026-4977-994E-45DC68502559" );
            AddDefinedValue( "799301A3-2026-4977-994E-45DC68502559", "Kids", "Kids", "F2BFF319-A109-4B42-BEC2-76590E11627B", false );
            AddDefinedValue( "799301A3-2026-4977-994E-45DC68502559", "Adults", "Adults", "95E49778-AE72-454F-91CC-2FC864557DEC", false );
            AddDefinedValue( "799301A3-2026-4977-994E-45DC68502559", "Staff", "Staff", "833EE2C7-F83A-4744-AD14-6907554DF8AE", false );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "833EE2C7-F83A-4744-AD14-6907554DF8AE" );
            DeleteDefinedValue( "95E49778-AE72-454F-91CC-2FC864557DEC" );
            DeleteDefinedValue( "F2BFF319-A109-4B42-BEC2-76590E11627B" );
            DeleteDefinedType( "799301A3-2026-4977-994E-45DC68502559" );
            
            // Event Attendees Group Type
            Sql( @"DELETE FROM [dbo].[crmGroupType] where [Guid] = '3311132B-268D-44E9-811A-A56A0835E50A'" );
        }
    }
}
