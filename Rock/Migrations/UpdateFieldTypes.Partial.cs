//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public partial class UpdateFieldTypes
    {
        /// <summary>
        /// Datas up.
        /// </summary>
        public void DataUp()
        {
            Sql( @"
DECLARE @AdminID int
SELECT @AdminID = [Id] FROM [crmPerson] WHERE [Guid] = 'AD28DA19-4AF1-408F-9090-2672F8376F27'

PRINT 'Updating Field Types'
UPDATE [coreFieldType] SET [Class]='Rock.Field.Types.Boolean',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='1EDAFDED-DFE6-4334-B019-6EECBA89E05A'
UPDATE [coreFieldType] SET [Class]='Rock.Field.Types.Color',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='D747E6AE-C383-4E22-8846-71518E3DD06F'
UPDATE [coreFieldType] SET [Class]='Rock.Field.Types.Date',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='6B6AA175-4758-453F-8D83-FCD8044B5F36'
UPDATE [coreFieldType] SET [Class]='Rock.Field.Types.Image',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D'
UPDATE [coreFieldType] SET [Name]='Integer',[Class]='Rock.Field.Types.Integer',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='A75DFC58-7A1B-4799-BF31-451B2BBE38FF'
UPDATE [coreFieldType] SET [Class]='Rock.Field.Types.PageReference',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108'
UPDATE [coreFieldType] SET [Class]='Rock.Field.Types.SelectMulti',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='BD0D9B57-2A41-4490-89FF-F01DAB7D4904'
UPDATE [coreFieldType] SET [Class]='Rock.Field.Types.SelectSingle',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='7525C4CB-EE6B-41D4-9B64-A08048D5A5C0'
UPDATE [coreFieldType] SET [Class]='Rock.Field.Types.Text',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='9C204CD0-1233-41C5-818A-C5DA439445AA'
UPDATE [coreFieldType] SET [Class]='Rock.Field.Types.Video',[ModifiedDateTime]='Jul 19 2012  6:00:00:000AM',[ModifiedByPersonId]=@AdminID WHERE [Guid]='FA398F9D-5B01-41EA-9A93-112F910A277D'

PRINT 'Adding New FieldTypes'
INSERT INTO [coreFieldType] ([System],[Name],[Description],[Assembly],[Class],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'Currency','A Currency Field','Rock','Rock.Field.Types.Currency','Jul 19 2012  6:00:00:000AM','Jul 19 2012  6:00:00:000AM',@AdminID,@AdminID,'50EABC9A-A29D-4A65-984A-87891B230533')
INSERT INTO [coreFieldType] ([System],[Name],[Description],[Assembly],[Class],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'Decimal','A Decimal Field','Rock','Rock.Field.Types.Decimal','Jul 19 2012  6:00:00:000AM','Jul 19 2012  6:00:00:000AM',@AdminID,@AdminID,'C757A554-3009-4214-B05D-CEA2B2EA6B8F')
INSERT INTO [coreFieldType] ([System],[Name],[Description],[Assembly],[Class],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'Defined Type','A Defined Type Field','Rock','Rock.Field.Types.DefinedType','Jul 19 2012  6:00:00:000AM','Jul 19 2012  6:00:00:000AM',@AdminID,@AdminID,'BC48720C-3610-4BCF-AE66-D255A17F1CDF')
INSERT INTO [coreFieldType] ([System],[Name],[Description],[Assembly],[Class],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'Defined Value','A Defined Value Field','Rock','Rock.Field.Types.DefinedValue','Jul 19 2012  6:00:00:000AM','Jul 19 2012  6:00:00:000AM',@AdminID,@AdminID,'59D5A94C-94A0-4630-B80A-BB25697D74C7')
INSERT INTO [coreFieldType] ([System],[Name],[Description],[Assembly],[Class],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'Document','A Document Field','Rock','Rock.Field.Types.Document','Jul 19 2012  6:00:00:000AM','Jul 19 2012  6:00:00:000AM',@AdminID,@AdminID,'11B0FC7B-6FD9-4D6C-AE41-C8B6EF6D4892')
INSERT INTO [coreFieldType] ([System],[Name],[Description],[Assembly],[Class],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'Person','A Person Field','Rock','Rock.Field.Types.Person','Jul 19 2012  6:00:00:000AM','Jul 19 2012  6:00:00:000AM',@AdminID,@AdminID,'E4EAB7B2-0B76-429B-AFE4-AD86D7428C70')
INSERT INTO [coreFieldType] ([System],[Name],[Description],[Assembly],[Class],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])VALUES(1,'URL','A URL Field','Rock','Rock.Field.Types.Url','Jul 19 2012  6:00:00:000AM','Jul 19 2012  6:00:00:000AM',@AdminID,@AdminID,'85B95F22-587B-4968-851D-9196FA1FA03F')
" );
        }

        /// <summary>
        /// Datas down.
        /// </summary>
        public void DataDown()
        {
            Sql( @"
DECLARE @AdminID int
SELECT @AdminID = [Id] FROM [crmPerson] WHERE [Guid] = 'AD28DA19-4AF1-408F-9090-2672F8376F27'

PRINT 'Updating Field Types'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.Boolean',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='1EDAFDED-DFE6-4334-B019-6EECBA89E05A'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.Color',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='D747E6AE-C383-4E22-8846-71518E3DD06F'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.Date',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='6B6AA175-4758-453F-8D83-FCD8044B5F36'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.Image',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.Integer',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='A75DFC58-7A1B-4799-BF31-451B2BBE38FF'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.SelectMulti',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='BD0D9B57-2A41-4490-89FF-F01DAB7D4904'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.PageReference',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.SelectSingle',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='7525C4CB-EE6B-41D4-9B64-A08048D5A5C0'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.Text',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='9C204CD0-1233-41C5-818A-C5DA439445AA'
UPDATE [coreFieldType] SET [Class]='Rock.FieldTypes.Video',[ModifiedDateTime]='2012-07-13 13:00:19.380',[ModifiedByPersonId]=@AdminID WHERE [Guid]='FA398F9D-5B01-41EA-9A93-112F910A277D'

PRINT 'Deleting New FieldTypes'
DELETE [coreFieldType] WHERE [Guid]='50EABC9A-A29D-4A65-984A-87891B230533'
DELETE [coreFieldType] WHERE [Guid]='C757A554-3009-4214-B05D-CEA2B2EA6B8F'
DELETE [coreFieldType] WHERE [Guid]='BC48720C-3610-4BCF-AE66-D255A17F1CDF'
DELETE [coreFieldType] WHERE [Guid]='59D5A94C-94A0-4630-B80A-BB25697D74C7'
DELETE [coreFieldType] WHERE [Guid]='11B0FC7B-6FD9-4D6C-AE41-C8B6EF6D4892'
DELETE [coreFieldType] WHERE [Guid]='E4EAB7B2-0B76-429B-AFE4-AD86D7428C70'
DELETE [coreFieldType] WHERE [Guid]='85B95F22-587B-4968-851D-9196FA1FA03F'
" );
        }
    }
}