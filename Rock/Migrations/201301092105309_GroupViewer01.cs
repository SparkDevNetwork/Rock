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
    public partial class GroupViewer01 : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlock( "", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "menu", "TwoColumnLeft", "Menu", 0, "224F437B-BB47-4F2A-B0A1-8B6B3DEEF8AC" );
            AddBlock( "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "footer", "TwoColumnLeft", "Footer", 0, "460B3BC8-AF20-4BF4-8982-EC96A03F184D" );

            // Attrib Value for menu:Root Page
            AddBlockAttributeValue("224F437B-BB47-4F2A-B0A1-8B6B3DEEF8AC","DD516FA7-966E-4C80-8523-BEAC91C8EEDA","12");
            // Attrib Value for menu:XSLT File
            AddBlockAttributeValue("224F437B-BB47-4F2A-B0A1-8B6B3DEEF8AC","D8A029F8-83BE-454A-99D3-94D879EBF87C","~/Assets/XSLT/PageNav.xslt");
            // Attrib Value for menu:Number of Levels
            AddBlockAttributeValue("224F437B-BB47-4F2A-B0A1-8B6B3DEEF8AC","9909E07F-0E68-43B8-A151-24D03C795093","3");
            // Attrib Value for Marketing Campaign Ad Types List:Detail Page Guid
            AddBlockAttributeValue("9F878185-9DAB-4866-B233-DFF0DA988AAC","302905C1-BD31-46D9-9082-663EF468C371","36826974-c613-48f2-877e-460c4ec90cce");

            Sql( @"
declare
  @blockId int
begin

select @blockId = Id from block where Guid = '460B3BC8-AF20-4BF4-8982-EC96A03F184D'

delete from [dbo].[HtmlContent] where BlockId = @blockId

INSERT INTO [dbo].[HtmlContent]
           ([BlockId]
           ,[EntityValue]
           ,[Version]
           ,[Content]
           ,[IsApproved]
           ,[ApprovedByPersonId]
           ,[ApprovedDateTime]
           ,[StartDateTime]
           ,[ExpireDateTime]
           ,[Guid])
     VALUES
           (@blockId
           ,''
           ,0
           ,'<p>Copyright&nbsp; 2012 Spark Development Network</p>'
           ,1
           ,1
           ,SYSDATETIME()
           ,null
           ,null
           ,NEWID()
           )
end
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
declare
  @blockId int
begin

select @blockId = Id from block where Guid = '460B3BC8-AF20-4BF4-8982-EC96A03F184D'

delete from [dbo].[HtmlContent] where BlockId = @blockId
" );
            
            DeleteBlock( "224F437B-BB47-4F2A-B0A1-8B6B3DEEF8AC" );
            DeleteBlock( "460B3BC8-AF20-4BF4-8982-EC96A03F184D" );
        }
    }
}
