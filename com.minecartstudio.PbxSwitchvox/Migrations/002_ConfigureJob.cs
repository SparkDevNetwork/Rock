using System;
using Rock.Plugin;

namespace com.minecartstudio.PbxSwitchvox.Migrations
{
    [MigrationNumber( 2, "1.7.0" )]
    public class ConfigureJob : Rock.Plugin.Migration
    {
        public override void Up()
        {
            Sql( @"
                IF NOT EXISTS(SELECT *  FROM [Attribute] WHERE [EntityTypeQualifierValue] = 'Rock.Jobs.PbxCdrDownload' AND [EntityTypeQualifierColumn] = 'Class' )
BEGIN
	DECLARE @FieldTypeId int = (SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = 'A7486B0E-4CA2-4E00-A987-5544C7DABA76')
	DECLARE @EntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '52766196-A72F-4F60-997A-78E19508843D')
	INSERT INTO [Attribute] 
	([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], 
	     [Description], [Guid], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory])
	VALUES
	(1, @FieldTypeId, @EntityTypeId, 'Class', 'Rock.Jobs.PbxCdrDownload', 'PbxComponent', 'PBX Component', 
	'The PBX type to process.', 'EC2E0BC5-7074-E1A3-4EB0-4FD3B4D39D54', 0, 0, '', 0, 1, 0, 0, 0)
END

DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeQualifierValue] = 'Rock.Jobs.PbxCdrDownload' AND [EntityTypeQualifierColumn] = 'Class')
PRINT @AttributeId

INSERT INTO [ServiceJob] ( [IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [Guid], [NotificationStatus] )
VALUES ( 0, 1, 'Swichvox CDR Job', 'Downloads the CDR records from Switchvox.',
        'Rock.Jobs.PbxCdrDownload', '0 15 4 1/1 * ? *','9A40094B-54C9-399E-49CB-DDDA3F570350', 4 )
DECLARE @JobId int = SCOPE_IDENTITY()

DECLARE @ComponentGuid uniqueidentifier = ( SELECT TOP 1 [Guid] FROM [EntityType] WHERE [Name] = 'com.minecartstudio.PbxSwitchvox.Pbx.Provider.Switchvox' )
        
INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
    VALUES ( 0, @AttributeId, @JobId, convert(nvarchar(50), @ComponentGuid), '9F6A7E90-7007-42A3-4E18-B626A7A70D79' )

        " );

        }
    

        public override void Down()
        {
            @Sql( "DELETE FROM [ServiceJob] WHERE [Guid] = '9A40094B-54C9-399E-49CB-DDDA3F570350'" );
        }

        
    }
}
