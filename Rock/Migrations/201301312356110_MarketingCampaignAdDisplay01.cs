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
    public partial class MarketingCampaignAdDisplay01 : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
INSERT INTO [dbo].[FieldType]([IsSystem],[Name],[Description],[Assembly],[Class],[Guid])
VALUES
    (1,'Audience Type','Audience Type','Rock','Rock.Field.Types.AudiencePrimarySecondaryField','AFFC7F00-CED0-4C07-8140-A1F400DABA63'),
    (1,'Audience','Audience','Rock','Rock.Field.Types.AudiencesField','CEC19E37-1CE6-469A-B863-C5BFE558658D'),
    (1,'Campuses','Campuses','Rock','Rock.Field.Types.CampusesField','69254F91-C97F-4C2D-9ACB-1683B088097B'),
    (1,'Ad Type','Ad Type','Rock','Rock.Field.Types.MarketingCampaignAdTypesField','F61722B7-CD11-4FA2-85E2-0D711616253A'),
    (1,'Ad Image Attribute Key','Ad Image Attribute Key','Rock','Rock.Field.Types.MarketingCampaignAdImageAttributeNameField','10E0786E-3202-400D-AFB6-6A8A8DDD2040')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
delete from [dbo].[Attribute] where [FieldTypeId] in (select [Id] from [FieldType] where [Guid] in (
    'AFFC7F00-CED0-4C07-8140-A1F400DABA63', 
    'CEC19E37-1CE6-469A-B863-C5BFE558658D', 
    '69254F91-C97F-4C2D-9ACB-1683B088097B', 
    'F61722B7-CD11-4FA2-85E2-0D711616253A',
    '10E0786E-3202-400D-AFB6-6A8A8DDD2040')
)

delete from [dbo].[FieldType] where [Guid] in (
    'AFFC7F00-CED0-4C07-8140-A1F400DABA63', 
    'CEC19E37-1CE6-469A-B863-C5BFE558658D', 
    '69254F91-C97F-4C2D-9ACB-1683B088097B', 
    'F61722B7-CD11-4FA2-85E2-0D711616253A',
    '10E0786E-3202-400D-AFB6-6A8A8DDD2040'
)
" );
        }
    }
}
