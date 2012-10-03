using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Migrations
{
    public partial class Metric2
    {
        /// <summary>
        /// Adds the metric data.
        /// </summary>
        public void DataUp()
        {
            Sql( @"
DECLARE @AdminID int
SELECT @AdminID = [Id] FROM [crmPerson] WHERE [Guid] = 'AD28DA19-4AF1-408F-9090-2672F8376F27'

PRINT 'Adding New DefinedTypes'
INSERT INTO [coreDefinedType] ([IsSystem],[Order],[Category],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES (1,0,'Metric','Frequency','Types of frequencies','2012-07-13 12:58:15.913','2012-07-13 12:58:15.913',@AdminID,@AdminID,'526CB333-2C64-4486-8469-7F7EA9366254')

DECLARE @DefinedTypeId int
SELECT @DefinedTypeId = [Id] from [coreDefinedType] WHERE [Guid] = '526CB333-2C64-4486-8469-7F7EA9366254'

PRINT 'Adding New DefinedValues'
INSERT INTO [coreDefinedValue] ([IsSystem],[DefinedTypeId],[Order],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES (1,@DefinedTypeId,0,'Hourly','Hourly','2012-07-13 12:58:15.913','2012-07-13 12:58:15.913',@AdminID,@AdminID,'78CF66EB-1A65-42CC-A05E-3BF6DE515049')
INSERT INTO [coreDefinedValue] ([IsSystem],[DefinedTypeId],[Order],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES(1,@DefinedTypeId,0,'Weekly','Weekly','2012-07-13 12:58:15.913','2012-07-13 12:58:15.913',@AdminID,@AdminID,'41663B95-8271-40E9-B1B6-0D14EA45D68D')
INSERT INTO [coreDefinedValue] ([IsSystem],[DefinedTypeId],[Order],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES(1,@DefinedTypeId,0,'Monthly','Monthly','2012-07-13 12:58:15.913','2012-07-13 12:58:15.913',@AdminID,@AdminID,'0BC11625-F8C4-4032-8B27-537D67941489')
INSERT INTO [coreDefinedValue] ([IsSystem],[DefinedTypeId],[Order],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES(1,@DefinedTypeId,0,'Yearly','Yearly','2012-07-13 12:58:15.913','2012-07-13 12:58:15.913',@AdminID,@AdminID,'305CBFA3-3168-40AF-ABCF-F5DFF9DC13C2')
INSERT INTO [coreDefinedValue] ([IsSystem],[DefinedTypeId],[Order],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES(1,@DefinedTypeId,0,'Manually','Manually','2012-07-13 12:58:15.913','2012-07-13 12:58:15.913',@AdminID,@AdminID,'338F29E5-05C4-40A5-A669-C098787E2ADF')

DECLARE @Weekly int
SELECT @Weekly = [Id] FROM [coreDefinedValue] WHERE [Guid] = '41663B95-8271-40E9-B1B6-0D14EA45D68D'
DECLARE @Monthly int
SELECT @Monthly = [Id] FROM [coreDefinedValue] WHERE [Guid] = '0BC11625-F8C4-4032-8B27-537D67941489'
DECLARE @Manually int
SELECT @Manually = [Id] FROM [coreDefinedValue] WHERE [Guid] = '338F29E5-05C4-40A5-A669-C098787E2ADF'

PRINT 'Adding New Metrics'
INSERT INTO [coreMetric] ([IsSystem],[Type],[Category],[Title],[Description],[CollectionFrequencyId],[Order],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES (0,0,'Core','Salvations','Salvations',@Manually,0,'2012-09-17 15:39:10.007','2012-09-17 15:39:10.007',@AdminId,@AdminId,'E3752BFB-6CD5-4DD9-8536-333E44746C0A')
INSERT INTO [coreMetric] ([IsSystem],[Type],[Category],[Title],[Description],[CollectionFrequencyId],[Order],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES (0,0,'Core','Baptisms','Baptisms',@Manually,0,'2012-09-17 15:39:10.007','2012-09-17 15:39:10.007',@AdminId,@AdminId,'0F33C2FD-49CC-4F4C-9DA8-FD558FCCA5DF')
INSERT INTO [coreMetric] ([IsSystem],[Type],[Category],[Title],[Description],[CollectionFrequencyId],[Order],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES (0,0,'Core','Weekly Volunteers','Weekly Volunteers',@Weekly,0,'2012-09-17 15:39:10.007','2012-09-17 15:39:10.007',@AdminId,@AdminId,'53861EBC-3E98-4BA9-BAC6-D8648B9F26ED')
INSERT INTO [coreMetric] ([IsSystem],[Type],[Category],[Title],[Description],[CollectionFrequencyId],[Order],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES (0,0,'Core','Weekly Attendance','Weekly Attendance',@Weekly,0,'2012-09-17 15:39:10.007','2012-09-17 15:39:10.007',@AdminId,@AdminId,'50986E58-B3AC-4907-ADD0-89083E69DCB7')
INSERT INTO [coreMetric] ([IsSystem],[Type],[Category],[Title],[Description],[CollectionFrequencyId],[Order],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES (0,0,'Core','Monthly Contributions','Monthly Contributions',@Monthly,0,'2012-09-17 15:39:10.007','2012-09-17 15:39:10.007',@AdminId,@AdminId,'558671BD-9952-494F-A1A9-A37AF0E40155')

PRINT 'Adding Metric Display Screens'
INSERT INTO [cmsBlock] ([IsSystem],[Path],[Name],[Description],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES(1,'~/Blocks/Administration/Metrics.ascx','Metrics','Settings for displaying and changing metrics and values.','2012-09-17 15:39:10.007','2012-09-17 15:39:10.007',@AdminID,@AdminID,'CCD4F459-2E0A-40C6-8DE3-AD512AE9CA74')  

DECLARE @ParentId int
SElECT @ParentId = [Id] FROM [cmsPage] WHERE [Guid] = '0B213645-FA4E-44A5-8E4C-B2D8EF054985'

INSERT INTO [cmsPage] ([Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],[RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],[Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES ('Metrics    ','Metrics',    0,@ParentId,1,'Default',0,1,1,0,0,0,8,0,'Settings for displaying and changing metrics and values.',1,'2012-09-17 15:39:10.007','2012-09-17 15:39:10.007',@AdminId,@AdminId,'84DB9BA0-2725-40A5-A3CA-9A1C043C31B0')

DECLARE @PageId int
SELECT @PageId = [Id] FROM [cmsPage] WHERE [Guid] = '84DB9BA0-2725-40A5-A3CA-9A1C043C31B0'
DECLARE @BlockId int
SELECT @BlockId = [Id] FROM [cmsBlock] WHERE [Guid] = 'CCD4F459-2E0A-40C6-8DE3-AD512AE9CA74'

INSERT INTO [cmsBlockInstance] ([IsSystem],[PageId],[BlockId],[Zone],[Order],[Name],[OutputCacheDuration],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
VALUES(0,@PageId,@BlockId,'Content',0,'Metrics',    0,'2012-09-17 15:39:10.007','2012-09-17 15:39:10.007',@AdminID,@AdminId,'9126CFA2-9B26-4FBB-BB87-F76514221DBE')
" );
        }

        /// <summary>
        /// Deletes the metric data.
        /// </summary>
        public void DataDown()
        {
            Sql( @"
PRINT 'Deleting Metric Display Screens'
DELETE [cmsBlockInstance] WHERE [Guid]='9126CFA2-9B26-4FBB-BB87-F76514221DBE'
DELETE [cmsPage] WHERE [Guid]='84DB9BA0-2725-40A5-A3CA-9A1C043C31B0'
DELETE [cmsBlock] WHERE [Guid]='CCD4F459-2E0A-40C6-8DE3-AD512AE9CA74'

PRINT 'Deleting New Metrics'
DELETE [coreMetric] WHERE [Guid]='E3752BFB-6CD5-4DD9-8536-333E44746C0A'
DELETE [coreMetric] WHERE [Guid]='0F33C2FD-49CC-4F4C-9DA8-FD558FCCA5DF'
DELETE [coreMetric] WHERE [Guid]='53861EBC-3E98-4BA9-BAC6-D8648B9F26ED'
DELETE [coreMetric] WHERE [Guid]='50986E58-B3AC-4907-ADD0-89083E69DCB7'
DELETE [coreMetric] WHERE [Guid]='558671BD-9952-494F-A1A9-A37AF0E40155'

PRINT 'Deleting New DefinedValues'
DELETE [coreDefinedValue] WHERE [Guid]='78CF66EB-1A65-42CC-A05E-3BF6DE515049'
DELETE [coreDefinedValue] WHERE [Guid]='41663B95-8271-40E9-B1B6-0D14EA45D68D'
DELETE [coreDefinedValue] WHERE [Guid]='0BC11625-F8C4-4032-8B27-537D67941489'
DELETE [coreDefinedValue] WHERE [Guid]='305CBFA3-3168-40AF-ABCF-F5DFF9DC13C2'
DELETE [coreDefinedValue] WHERE [Guid]='338F29E5-05C4-40A5-A669-C098787E2ADF'

PRINT 'Deleting New DefinedTypes'
DELETE [coreDefinedType] WHERE [Guid]='526CB333-2C64-4486-8469-7F7EA9366254'
" );
        }
    }
}