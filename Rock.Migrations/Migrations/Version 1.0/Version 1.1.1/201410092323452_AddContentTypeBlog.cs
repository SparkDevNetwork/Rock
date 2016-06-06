// <copyright>
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
    public partial class AddContentTypeBlog : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* Step up the following:
             *  1.) Add a content channel for bulletins (w/ security)
             *  2.) Create new content channel type for Blogs
             *  3.) Create a new content channel for the website blog (w/ security and 12 sample posts)
             *  4.) Create blog pages
             *  5.) Rename the channel type 'Bulletin' to 'Bulletins' to be consistant
             */
            Sql( @"
                -- rename the channel type 'bulletin' to 'bulletins'
UPDATE [ContentChannelType]
	SET [Name] = 'Bulletins'
	WHERE [Guid] = '206CFC34-1C86-46F5-A1EA-6D71B25A8D33'

DECLARE @AttributeId int
DECLARE @LastEntityId int
DECLARE @SummaryAttributeId int

DECLARE @ChannelItemEntityId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ContentChannelItem' )
DECLARE @ChannelEntityId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ContentChannel' )
DECLARE @BinaryFileTypeId int = ( SELECT [Id] FROM [BinaryFileType] WHERE [Guid] = '8DBF874C-F3C2-4848-8137-C963C431EB0B' )
DECLARE @MemoFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = 'C28C7BF3-A552-4D77-9408-DEDCF760CED0' )
DECLARE @ImageFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D' )
DECLARE @AdminPersonAliasId int = ( SELECT TOP 1 [Id] FROM [PersonAlias] ORDER BY [Id] )
DECLARE @BulletinChannelTypeId int = ( SELECT TOP 1 [Id] FROM [ContentChannelType] WHERE [Guid] = '206CFC34-1C86-46F5-A1EA-6D71B25A8D33' )
DECLARE @CommunicationAdminGroupId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = 'B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B' )

-- Create channel types
INSERT INTO [ContentChannelType] ( [IsSystem], [Name], [DateRangeType], [DisablePriority], [Guid] ) VALUES 
	( 0, 'Blogs', 1, 1, 'C0549458-DCCF-4E02-8978-B15932576F68' )
DECLARE @BlogsTypeId int = SCOPE_IDENTITy()

-- Create channel image attribute for blog type
INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
	[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
VALUES (0, @ImageFieldTypeId, @ChannelEntityId, 'ContentChannelTypeId', CAST(@BlogsTypeId as varchar),
	'BlogImage', 'Blog Image', '',1,0,'',0,0,'004D1F8A-749B-4F21-B93E-9F6A84E94651')
SET @AttributeId = SCOPE_IDENTITy()

INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid] )
VALUES (0, @AttributeId, 'binaryFileType', CAST(@BinaryFileTypeId AS varchar), NEWID() )


-- Create content item summary text attribute for blog type
INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
	[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
VALUES (0, @MemoFieldTypeId, @ChannelItemEntityId, 'ContentChannelTypeId', CAST(@BlogsTypeId as varchar),
	'Summary', 'Summary', 'Short description of the blog post',0,0,'',0,0,'AE455FB9-20FD-4767-9A65-4801B00FCD1A')
SET @SummaryAttributeId = SCOPE_IDENTITy()


-- Create content item image attribute for blog type
INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
	[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
VALUES (0, @ImageFieldTypeId, @ChannelItemEntityId, 'ContentChannelTypeId', CAST(@BlogsTypeId as varchar),
	'Image', 'Image', '',1,0,'',0,0,'41C3F412-6CF7-4ED1-B40C-501861BB5F3D')
SET @AttributeId = SCOPE_IDENTITy()

INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid] )
VALUES (0, @AttributeId, 'binaryFileType', CAST(@BinaryFileTypeId AS varchar), NEWID() )

-- add the bulletin channel
IF @BulletinChannelTypeId IS NOT NULL
BEGIN

    INSERT INTO [ContentChannel] ( [ContentChannelTypeId], [Name], [Description], [IconCssClass], [RequiresApproval], [EnableRss], [ChannelUrl], [ItemUrl] ,[TimeToLive], [ContentControlType], [RootImageDirectory], [Guid] )
    VALUES ( @BulletinChannelTypeId, 'Service Bulletin', 'Used to track bulletin requests for the main service.', 'fa fa-bookmark', 1, 0, '', '', 0, 1, '~/Content/ServiceBulletins', '3CBD6C7A-30B4-4CF5-B1B9-4216C4EEF371' )
    DECLARE @BulletinChannelId int = SCOPE_IDENTITY()

    -- set security for the bulletin channel
    IF @CommunicationAdminGroupId IS NOT NULL
    BEGIN
        INSERT INTO [Auth] 
	        ([EntityTypeId] ,[EntityId] ,[Order] ,[Action] ,[AllowOrDeny] ,[GroupId] ,[SpecialRole] ,[Guid] )
        VALUES 
	        (@ChannelEntityId, @BulletinChannelId, 0, 'Edit', 'A', @CommunicationAdminGroupId, 0, '64C0AA99-BFE7-4CE0-9579-62AED07BD5D7')

        INSERT INTO [Auth] 
	        ([EntityTypeId] ,[EntityId] ,[Order] ,[Action] ,[AllowOrDeny] ,[GroupId], [SpecialRole] ,[Guid] )
        VALUES 
	        (@ChannelEntityId, @BulletinChannelId, 1, 'Administrate', 'A', @CommunicationAdminGroupId, 0, '06DA6708-D142-4F32-A4B3-4180C5EE54C5')

        INSERT INTO [Auth] 
	        ([EntityTypeId] ,[EntityId] ,[Order] ,[Action] ,[AllowOrDeny] ,[GroupId] ,[SpecialRole] ,[Guid] )
        VALUES 
	        (@ChannelEntityId, @BulletinChannelId, 2, 'Approve', 'A', @CommunicationAdminGroupId, 0, 'B2D4A357-30B3-4312-AFB8-9C0CBCDAE5A8')
    END

END

-- add the external website blog
INSERT INTO [ContentChannel] ( [ContentChannelTypeId], [Name], [Description], [IconCssClass], [RequiresApproval], [EnableRss], [ChannelUrl], [ItemUrl] ,[TimeToLive], [ContentControlType], [RootImageDirectory], [Guid] )
VALUES ( @BlogsTypeId, 'Website Blog', 'Organization''s primary blog.', 'fa fa-rss-square', 1, 1, 'http://www.rocksolidchurchdemo.com/page/346', 'http://www.rocksolidchurchdemo.com/page/347', 60, 1, '~/Content/WebsiteBlog', '2B408DA7-BDD1-4E71-B6AC-F22D786B605F' )
DECLARE @BlogChannelId int = SCOPE_IDENTITY()

-- set security for the website blog
IF @CommunicationAdminGroupId IS NOT NULL
BEGIN
    INSERT INTO [Auth] 
	    ([EntityTypeId] ,[EntityId] ,[Order] ,[Action] ,[AllowOrDeny] ,[GroupId] ,[SpecialRole] ,[Guid] )
    VALUES 
	    (@ChannelEntityId, @BlogChannelId, 0, 'Edit', 'A', @CommunicationAdminGroupId, 0, '2B408DA7-BDD1-4E71-B6AC-F22D786B605F')

    INSERT INTO [Auth] 
	    ([EntityTypeId] ,[EntityId] ,[Order] ,[Action] ,[AllowOrDeny] ,[GroupId], [SpecialRole] ,[Guid] )
    VALUES 
	    (@ChannelEntityId, @BlogChannelId, 1, 'Administrate', 'A', @CommunicationAdminGroupId, 0, 'FD58183C-4B40-44FB-9418-867212341AEE')

    INSERT INTO [Auth] 
	    ([EntityTypeId] ,[EntityId] ,[Order] ,[Action] ,[AllowOrDeny] ,[GroupId] ,[SpecialRole] ,[Guid] )
    VALUES 
	    (@ChannelEntityId, @BlogChannelId, 2, 'Approve', 'A', @CommunicationAdminGroupId, 0, '2C2E1B6C-E08E-493E-A6AD-CC6A8A9755CA')
END

-- add blog content
------ item 1
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'Mea Animal Aperiam Ornatus Eu', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', 'E214F2A7-CB53-4640-ACD5-CEAD2190C99D', '
	<p>Dolor intellegebat ad vim. Clita tritani labores eu vis. Aeque aliquip detraxit vis id, eos ad nibh intellegat, vide officiis theophrastus at nec. Pri primis melius eu. Ea mea dicit copiosae scripserit, ad verear labitur ius, probo melius ne sit. Numquam voluptua per at, sit facete ornatus fastidii ne.</p>

	<p>Ex errem oratio ius. Pro et idque oratio scriptorem. Congue omnium urbanitas eu sea, affert altera audire cu cum, te alia augue facilis mel. Et est iudicabit vulputate, ne erant docendi contentiones mel. Nec omnes congue putant in, usu an dicit scripserit. Mel altera suscipit phaedrum ad, cu duo graece impetus.</p>

	<p>Mel ut facilis reprehendunt, pro in accusam persequeris, primis epicurei vis et. Prompta discere denique ea nam, et vis ullum quidam democritum. Mei latine sensibus ea, per dolor appetere expetenda id. Duo sale ullamcorper vituperatoribus ex, recteque incorrupte an qui. Amet ipsum ut nec, ius id eius minim vidisse. Soleat erroribus ne mei.</p>
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Dolor intellegebat ad vim. Clita tritani labores eu vis. Aeque aliquip detraxit vis id, eos ad nibh intellegat, vide officiis theophrastus at nec.', newid())

------ item 2
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'Probo Senserit Id Mea, Ut Sed Malis Postea,', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '095466ED-029C-48BC-BCBB-FCF79CF8A68B', '
	<p>In tritani eleifend comprehensam cum, pro autem cotidieque et. Malis affert eu sea, ne ponderum menandri incorrupte sed. Ne quod dico sea, no lorem homero assueverit eos. Has id feugiat facilisi reprehendunt, at eligendi incorrupte usu.</p>

	<p>Id vix regione eligendi vituperatoribus, ne qui appareat inciderint, ius an malis saepe civibus. Magna putant per ut, splendide inciderint vis ei. Aliquip inimicus maluisset vix id, pro ea rationibus constituto. Pro vidisse ancillae in. Veritus senserit abhorreant nec id. Ad paulo veniam officiis sea.</p>

	<p>Ex mea dicant docendi, natum prompta an est. Ius ei autem reprehendunt, velit detracto mei ad, at probo recteque cum. Eam no reque graece, enim agam sale ea per. Etiam deterruisset ad est. Eam cibo accusamus intellegat in.</p>')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'In tritani eleifend comprehensam cum, pro autem cotidieque et. Malis affert eu sea, ne ponderum menandri incorrupte sed. Ne quod dico sea, no lorem homero assueverit eos.', newid())

------ item 3
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'Ea Harum Albucius Mel', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', 'F946A31A-59CF-43A8-A595-B01AB2B69D6E', '
	<p>Ea principes voluptaria mei, malis vocibus cu usu, in oporteat consetetur mei. Ullum scribentur eu usu, inani appellantur vel id, vel aeque bonorum menandri ei. Te facete iisque tacimates mea, pri at virtute invenire liberavisse. Sea augue incorrupte at, everti audiam menandri cu eos. No ludus apeirian forensibus nec, mea cu atqui essent utroque. Ex laboramus sententiae eam, mel ut noster minimum.</p>

	<p>Qui ea velit oportere, et illud nonumy legere vel. Magna brute maluisset et has. Has natum alienum dolores no, id labores atomorum periculis eam. No ius natum admodum nusquam, eos an facilisi menandri salutatus. Eam ludus verterem ad, eu has populo recteque accommodare, soluta audire est ea.</p>

	<p>In modo ancillae eum, est tota ridens an. Mei mucius dignissim ad. Ne has dicat noluisse voluptaria, habeo mucius mnesarchum per in. Pro legere corpora voluptaria id, mea saperet intellegam et. Vis ut purto saepe graece.</p>
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Ea principes voluptaria mei, malis vocibus cu usu, in oporteat consetetur mei. Ullum scribentur eu usu, inani appellantur vel id, vel aeque bonorum menandri ei.', newid())


------ item 4
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'An Esse Tacimates Mel', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '4D9A94BE-33D9-4A38-8F36-393DE13CCFB7', '
		<p>Vim brute volumus no. Epicuri moderatius complectitur duo ex, has eu suscipit scripserit. In eius eligendi pro. At impedit pericula duo.</p>

		<p>Movet bonorum ex vix. Est et vocibus scribentur, sed ea labore senserit maluisset. Sea eu latine repudiare, mel omnis percipit ex. Quas platonem eum ea, reque dicta no qui. Utinam euismod torquatos eum no.</p>

		<p>Ex rationibus omittantur referrentur pro, elitr consequuntur ne vim, eam quod hinc veritus in. Tota utinam dictas ne nam. Ad sed feugait insolens deterruisset, etiam latine fastidii est ne. His laboramus similique adversarium at, noster ponderum an qui, eum cibo liber veniam no. Eos ei diceret euismod aliquid, eum case zril laoreet ne. Qui lucilius instructior te, pri utinam honestatis vituperatoribus id. Mea an sale primis efficiendi, cu mei maiorum laboramus prodesset, posse fabulas similique ne mei.</p>
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Vim brute volumus no. Epicuri moderatius complectitur duo ex, has eu suscipit scripserit. In eius eligendi pro. At impedit pericula duo.', newid())


------ item 5
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'Malis Possim Tritani Eos Ne', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '4134A71F-80DF-4482-BD33-3DB4DB9EC409', '
		<p>Ei sea discere persequeris deterruisset. Vim consul dolorum detraxit in, in quo diam rationibus. Nam ex alii bonorum dolorem, has malis urbanitas liberavisse id, amet purto eu vis. Vis eu accumsan hendrerit, ei his everti labores expetendis. In decore liberavisse sed, commodo aperiam et vel.</p>

		<p>Te dolore legere per, mel id essent phaedrum constituam, eu dicam soluta repudiare his. Petentium sapientem interpretaris vix te, has id tollit appareat tacimates. Putent adolescens consectetuer an qui, ei porro delectus indoctum vis. Facilisi qualisque eloquentiam cu his, sumo mucius atomorum sed ut, officiis probatus suavitate at mei. Nihil platonem eum ei, et mea augue comprehensam, aeque sadipscing ut usu.</p>

		<p>Pri at vivendum gubergren intellegebat, deleniti definiebas ne pri. Eu sed facete gloriatur, quis cetero et per. Te est democritum definiebas conclusionemque. Nec in quod persius scaevola, his eius recteque dissentiet ne, ius ne erant accommodare. Nobis appareat delicatissimi ea his, in probo dolore evertitur has.</p>
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Ei sea discere persequeris deterruisset. Vim consul dolorum detraxit in, in quo diam rationibus. Nam ex alii bonorum dolorem, has malis urbanitas liberavisse id, amet purto eu vis.', newid())

------ item 6
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'Expetenda Deterruisset Mea', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '9AA0E16D-FE65-4164-B3A9-406E53A40F99', '
	<p>Ferri harum ex his, veri autem vix te. Erant everti ex nec, quas porro zril eum at. Possit vulputate et usu. Alii posse doming ex vim. Pro an putent equidem commune, ea quaeque concludaturque mel. Has ex denique ocurreret, libris tacimates ne quo, vero tacimates reprimique duo ad. Per persius eripuit ne, mei nisl elitr eloquentiam ne.</p>

	<p>No appetere suscipiantur usu. Malis vulputate an vel, eos augue omnium no. Quo id persius scripserit accommodare, eum cu quas facer tantas, ex feugiat detraxit mel. Iudico ullamcorper sit ne. Unum eirmod tacimates has eu. At sea debitis rationibus. Labores delectus duo in.</p>

	<p>Per omnium legendos ex, sit invenire laboramus cu. An legere ignota reprimique pri. Diceret graecis disputationi ei mea. Ad iudico signiferumque ius, referrentur complectitur an qui. Ocurreret reprimique an per, ex duo vitae bonorum.</p>	
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Ferri harum ex his, veri autem vix te. Erant everti ex nec, quas porro zril eum at. Possit vulputate et usu. Alii posse doming ex vim.', newid())


------ item 7
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'At Est Mediocrem Gubergren Adversarium', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '5A95642A-A181-4043-A596-BD47D65B8654', '
		<p>Mucius recteque dissentiet his ut, ea elitr nusquam dissentiet vel, rebum sanctus te nec. Vim feugiat omnesque et. Te pri quidam timeam, usu assum laudem audire ut. Ex aperiam periculis sit, at laoreet propriae vim. Vis quodsi alterum disputando ex, eligendi accommodare ea vel. In quo omnes dolore oporteat, sed te falli malorum saperet.</p>

		<p>Vix enim percipit deserunt ea, quo quaeque blandit et, etiam electram temporibus sed no. Dico adhuc dissentias vis cu, vis eleifend dissentiunt id. An vis nibh persius vivendo, ne appareat oportere duo, feugiat erroribus definiebas cum an. Vim vero paulo prompta an. Te usu erat scripta, ut cum tale vero invenire. Ei assum audiam repudiare vel, putant efficiantur mel ei. Eam aliquid inermis dissentiet et.</p>

		<p>Quo omnis ubique cu, cum mollis corpora ex. Populo abhorreant intellegam at vim, no sale oratio accommodare cum. Qui ne porro facete alterum, duo ad quod necessitatibus. Ad affert equidem vel, wisi tamquam delicata et sit. Purto eligendi accusamus no eam.</p>	
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Mucius recteque dissentiet his ut, ea elitr nusquam dissentiet vel, rebum sanctus te nec. Vim feugiat omnesque et. Te pri quidam timeam, usu assum laudem audire ut.', newid())


------ item 8
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'Vis Ponderum Adipisci Cu', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', 'C9029380-7295-4A0B-9CB6-2C134AC12C72', '
		<p>Vix natum omittam detraxit ex, ut tale nostrum ius. Ex per brute tation delicatissimi, cum ludus iracundia pertinacia ex. Ius euripidis scriptorem te. Quo te copiosae officiis, sit audire aeterno adolescens ex, suavitate torquatos scribentur ad pro. Eos vero sanctus scriptorem id.</p>

		<p>Vide assum no vis. Vix ne erat dolores. Case sanctus efficiendi no duo, ipsum ullum pertinacia vel ne. Has an tota postea pertinacia, eum dicunt lucilius definitionem cu. Cetero utamur electram at vix. Eu voluptua ponderum concludaturque vis.</p>

		<p>Ferri audiam ullamcorper mea ut, sea unum bonorum ponderum et. Dicat philosophia ex his, eros constituto dissentiet ex per. Ullum postea vocibus te vix, at esse possim veritus vis, omnis solet iriure vis ad. Te saepe iriure eripuit vis, nibh appetere ex mel.</p>	
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Vix natum omittam detraxit ex, ut tale nostrum ius. Ex per brute tation delicatissimi, cum ludus iracundia pertinacia ex. Ius euripidis scriptorem te.', newid())

------ item 9
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'An Nec Wisi Accommodare', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '08E0E677-1F98-487F-ACCD-7B0EC5AD50D1', '
		<p>Reque saperet sit ex. Ad regione accusam oporteat pri, prima splendide comprehensam eos ea. Nisl persius impedit sed in, ut assum partem expetenda mea. Idque dicat error nec te. Est ad incorrupte voluptatibus, eum ea munere praesent.</p>

		<p>Falli mundi elaboraret ei his, ea quo brute legimus consectetuer, ut has purto ignota. In eum epicuri pertinacia definiebas. Nam id equidem intellegat dissentiet, vix minim veniam nonumes te. Mea eripuit philosophia te, pri at ferri oratio referrentur. Eum ut persius dissentiet, et per mollis reprehendunt. No qui wisi ignota, ei appellantur definitiones delicatissimi duo, fuisset periculis suscipiantur ne est.</p>

		<p>Ius at malorum scaevola, postea dicunt bonorum ne pro, an vix pericula abhorreant voluptatibus. Ut soleat nusquam pri, aperiri laoreet vim in, justo praesent te mea. Mea at esse munere dolorum, qui utroque quaerendum eu. Ut probo assum vis, velit legendos mea eu, ut eam idque tollit. Usu quot exerci posidonium eu, nam ne esse viris repudiare, id magna inimicus appellantur mea.</p>
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Reque saperet sit ex. Ad regione accusam oporteat pri, prima splendide comprehensam eos ea. Nisl persius impedit sed in, ut assum partem expetenda mea.', newid())


------ item 10
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'Tota Etiam In Pri Posse Malis Dictas His Te', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '8A3D944E-AD82-479F-89D5-9729CDB5D4C8', '
		<p>Malis utinam verear ad vim, postea vidisse debitis ne vim. Pericula erroribus mei ei, reque philosophia sit id, eos facilisis prodesset at. Dico nostrum ea mea. Ea wisi iuvaret omnesque usu. Ne sit scaevola partiendo moderatius, abhorreant intellegebat quo ut, pro ad legere discere. Ea suas propriae eloquentiam sit.</p>

		<p>Ei nam cibo sanctus detraxit, congue oratio his id. Amet reprimique ei eos, an rebum mollis commune vix. At pri veri efficiendi instructior, eos dolorum erroribus moderatius ei. At tollit splendide gloriatur vix. Ea vix minimum intellegam, nec quot cibo te.</p>

		<p>Nec no praesent assentior. Ea ius natum mundi atomorum, mel no augue impetus vocibus, vis soleat splendide cu. His ei minimum senserit, persecuti efficiantur reprehendunt te vis. At mucius iuvaret erroribus eam. Ad vis consetetur argumentum. Autem corpora quo ne.</p>
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Malis utinam verear ad vim, postea vidisse debitis ne vim. Pericula erroribus mei ei, reque philosophia sit id, eos facilisis prodesset at.', newid())

------ item 11
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'Mei Euismod Phaedrum Ne', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', '006D5288-FE49-4C5D-82B6-BAC36445CD83', '
		<p>Vis quot posse evertitur ut. Doctus vocibus ut pri. Ludus adipisci eos no, ex vix mutat discere facilisis, an sit partem tamquam dolorum. No sit accusam molestie, ius no illum tractatos neglegentur, eam cu stet postulant. Nec et ferri elitr quidam, ex pri quidam impetus maiorum. Est viderer petentium an.</p>

		<p>Vel prima labore audire cu. Cum et albucius suscipit quaestio. Est ex dictas pericula percipitur, reque lobortis dissentias pro te, vim in labores vituperatoribus. Nibh persius sed ea. Causae vocibus ad eam.</p>

		<p>Eos dicam ancillae accusata id. Eum et modus persecuti. Mei partem quaeque recusabo an, ea est velit corpora. Autem fabellas eloquentiam eum ei, vix pertinax posidonium te. Sea aeterno torquatos ne. At assum perpetua ius, elit timeam duo et.</p>		
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Vis quot posse evertitur ut. Doctus vocibus ut pri. Ludus adipisci eos no, ex vix mutat discere facilisis, an sit partem tamquam dolorum. No sit accusam molestie, ius no illum tractatos neglegentur, eam cu stet postulant.', newid())


------ item 12
INSERT INTO [ContentChannelItem] ( [ContentChannelId], [ContentChannelTypeId], [Title], [Priority], [Status], [ApprovedByPersonAliasId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid], [Content] )
VALUES
    ( @BlogChannelId, @BlogsTypeId, 'Ad Vis Populo Recteque', 100, 2, CAST(@AdminPersonAliasId as varchar), '2013-08-01 00:00:00.000', '2013-08-01 00:00:00.000', '2020-08-02 00:00:00.000', 'D5592927-2FAF-4131-8D6D-E9EA58165522', '
		<p>Pertinacia liberavisse te has, putant salutandi sadipscing cu eam. Quo id soluta meliore efficiantur, paulo dolorem accommodare vix ea, porro libris temporibus eu eos. Pro te epicuri reformidans, no sea congue signiferumque, ius ea magna option antiopam. Usu et ancillae albucius. Vix ea suas contentiones, no denique inimicus salutandi vis. Ut eum option offendit officiis, ut sea alterum civibus conceptam.</p>

		<p>Sea utamur suavitate ad, rebum velit invenire in his. Vel aeque timeam veritus ut. Regione civibus gubergren duo at, per legere meliore invenire ei. Cum ea vocent delicata. Qui quem natum graeci ut.</p>

		<p>Probo senserit id mea, ut sed malis postea, pri ei tempor similique. Ea harum albucius mel, ad insolens inimicus pro. Expetenda deterruisset mea ne, has in mentitum accommodare. Tale legere id vix, tota scriptorem cum ut, mea ei dico vidit nullam. Facete timeam forensibus te mea, vim harum equidem in. Sea adhuc nostro pericula cu, cum et duis errem prodesset.</p>			
')

SET @LastEntityId = SCOPE_IDENTITY()

------------ add summary
INSERT INTO [AttributeValue] ([AttributeId], [IsSystem], [EntityId], [Value], [Guid])
	VALUES (@SummaryAttributeId, 0, @LastEntityId, 'Pertinacia liberavisse te has, putant salutandi sadipscing cu eam. Quo id soluta meliore efficiantur, paulo dolorem accommodare vix ea, porro libris temporibus eu eos.', newid())

            " );

            RockMigrationHelper.AddPage("85F25819-E948-4960-9DDF-00F54D32444E","ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED","Blog","","4857A6C9-F194-4B64-A9FB-6A8DC7A1A671",""); // Site:External Website
            RockMigrationHelper.AddPage("4857A6C9-F194-4B64-A9FB-6A8DC7A1A671","ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED","Blog Details","","2D0D0FB0-68C4-47E1-8BC6-98F931497F5E",""); // Site:External Website
            // Add Block to Page: Blog, Site: External Website
            RockMigrationHelper.AddBlock("4857A6C9-F194-4B64-A9FB-6A8DC7A1A671","","143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","Blog Content","Main","","",0,"AE2D8454-AE86-40DF-A57A-C9F30B8AB50F"); 

            // Add Block to Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlock("2D0D0FB0-68C4-47E1-8BC6-98F931497F5E","","143A2345-3E26-4ED0-A2FE-42AAF11B4C0F","Blog Details","Main","","",0,"C146309D-E282-4FD5-94D9-CD4D0853AF09"); 

            // Add Block to Page: Blog, Site: External Website
            RockMigrationHelper.AddBlock("4857A6C9-F194-4B64-A9FB-6A8DC7A1A671","","19B61D65-37E3-459F-A44F-DEF0089118A3","Sidebar Content","Sidebar1","","",0,"65970B39-4DC3-4CFA-A8E7-49C23978E923"); 

            // Add Block to Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlock("2D0D0FB0-68C4-47E1-8BC6-98F931497F5E","","19B61D65-37E3-459F-A44F-DEF0089118A3","Sidebar Content","Sidebar1","","",0,"4A25275A-0CC8-4334-939E-C00320A016BD"); 

            // Attrib Value for Block:Blog Content, Attribute:Meta Image Attribute Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","A3510474-86E5-4AD2-BD4C-3C89E85795F5",@"I^Image");

            // Attrib Value for Block:Blog Content, Attribute:Rss Autodiscover Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","ABA69AFC-2C5E-4CF6-A156-EDA2752CCC86",@"False");

            // Attrib Value for Block:Blog Content, Attribute:Meta Description Attribute Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","E01AE3A7-2607-4DA5-AC98-3A368C900B64",@"I^Summary");

            // Attrib Value for Block:Blog Content, Attribute:Filter Id Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","618EFBDA-941D-4F60-9AA8-54955B7A03A2",@"49");

            // Attrib Value for Block:Blog Content, Attribute:Order Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","07ED420E-749C-4938-ADFD-1DDEA6B63014",@"");

            // Attrib Value for Block:Blog Content, Attribute:Merge Content Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","20BE4E0A-E84C-4AA1-9368-9732A834E1DE",@"False");

            // Attrib Value for Block:Blog Content, Attribute:Set Page Title Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","97161D67-EF24-4F21-9E6A-74B696DD33DE",@"True");

            // Attrib Value for Block:Blog Content, Attribute:Cache Duration Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","773BEFDD-EEBA-486C-98E6-AFD0D4156E22",@"3600");

            // Attrib Value for Block:Blog Content, Attribute:Channel Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE",@"2b408da7-bdd1-4e71-b6ac-f22d786b605f");

            // Attrib Value for Block:Blog Content, Attribute:Detail Page Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21",@"2d0d0fb0-68c4-47e1-8bc6-98f931497f5e");

            // Attrib Value for Block:Blog Content, Attribute:Enable Debug Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","72F4232B-8D2A-4823-B9F1-ED68F182C1A4",@"False");

            // Attrib Value for Block:Blog Content, Attribute:Status Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B",@"2");

            // Attrib Value for Block:Blog Content, Attribute:Template Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8",@"{% for item in Items %}
    <article class=""margin-b-lg"">
        <h3>{{ item.Title }}</h3>
    
        <div>
            {{ item.Summary }}
        </div>
    
        <p class=""margin-t-lg"">
            <a href=""{{ LinkedPages.DetailPage }}?Item={{ item.Id }}"" class=""btn btn-default btn-xs"">Read more <i class=""fa fa-chevron-right""></i></a>
        </p>
    </article>
{% endfor %}

<div class=""clearfix"">
    {% assign nextPageString = Pagination.NextPage | ToString %}
    {% assign prevPageString = Pagination.PreviousPage | ToString %}
    
    {% if {{Pagination.PreviousPage == -1 }} %}
        <div class=""btn btn-default pull-left""><i class=""fa fa-chevron-left""></i> Prev</div>
    {% else %}
        <a class=""btn btn-primary pull-left"" href=""{{Pagination.UrlTemplate | Replace:'PageNum', prevPageString}}""><i class=""fa fa-chevron-left""></i> Prev</a>
    {% endif %}
    
    {% if {{Pagination.NextPage == -1 }} %}
        <div class=""btn btn-default pull-right"">Next <i class=""fa fa-chevron-right""></i></div>
    {% else %}
        <a class=""btn btn-primary pull-right"" href=""{{Pagination.UrlTemplate | Replace:'PageNum', nextPageString}}"">Next <i class=""fa fa-chevron-right""></i></a>
    {% endif %}
</div>");

            // Attrib Value for Block:Blog Content, Attribute:Count Page: Blog, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("AE2D8454-AE86-40DF-A57A-C9F30B8AB50F","25A501FC-E269-40B8-9904-E20FA7A1ADB6",@"5");

            // Attrib Value for Block:Blog Details, Attribute:Count Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","25A501FC-E269-40B8-9904-E20FA7A1ADB6",@"5");

            // Attrib Value for Block:Blog Details, Attribute:Detail Page Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","2D7E6F55-B25E-4EA2-8F7E-F7E138E39E21",@"");

            // Attrib Value for Block:Blog Details, Attribute:Enable Debug Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","72F4232B-8D2A-4823-B9F1-ED68F182C1A4",@"False");

            // Attrib Value for Block:Blog Details, Attribute:Cache Duration Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","773BEFDD-EEBA-486C-98E6-AFD0D4156E22",@"3600");

            // Attrib Value for Block:Blog Details, Attribute:Channel Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","34EACB0F-DBC4-4F18-85C9-0D3EDFDF46BE",@"2b408da7-bdd1-4e71-b6ac-f22d786b605f");

            // Attrib Value for Block:Blog Details, Attribute:Status Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","DA1DEE5D-BCEF-4AA4-A9D9-EFD4DD64462B",@"2");

            // Attrib Value for Block:Blog Details, Attribute:Set Page Title Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","97161D67-EF24-4F21-9E6A-74B696DD33DE",@"True");

            // Attrib Value for Block:Blog Details, Attribute:Rss Autodiscover Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","ABA69AFC-2C5E-4CF6-A156-EDA2752CCC86",@"False");

            // Attrib Value for Block:Blog Details, Attribute:Order Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","07ED420E-749C-4938-ADFD-1DDEA6B63014",@"");

            // Attrib Value for Block:Blog Details, Attribute:Merge Content Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","20BE4E0A-E84C-4AA1-9368-9732A834E1DE",@"False");

            // Attrib Value for Block:Blog Details, Attribute:Filter Id Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","618EFBDA-941D-4F60-9AA8-54955B7A03A2",@"50");

            // Attrib Value for Block:Blog Details, Attribute:Meta Description Attribute Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","E01AE3A7-2607-4DA5-AC98-3A368C900B64",@"I^Summary");

            // Attrib Value for Block:Blog Details, Attribute:Meta Image Attribute Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","A3510474-86E5-4AD2-BD4C-3C89E85795F5",@"I^Image");

            // Attrib Value for Block:Blog Details, Attribute:Template Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("C146309D-E282-4FD5-94D9-CD4D0853AF09","8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8",@"{% for item in Items %}
    
    <div>
        {{ item.Image }}
    </div>
    
    {{ item.Content }}

{% endfor %}");

            // add content to new html blocks
            Sql( @"
    DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '4A25275A-0CC8-4334-939E-C00320A016BD')
    IF @BlockId IS NOT NULL
    BEGIN
        INSERT INTO [HtmlContent] ([BlockId], [Version], [Content], [IsApproved], [Guid])
        VALUES (@BlockId, 1, '<div class=""well"">
    <h3>Blog Sidebar</h3>
    
    <p>
        Sidebar content for the blog.
    </p>
</div>', 1, 'DDFC4AFB-6144-4FEA-85DD-A367216DDB26')
    END

    SET @BlockId = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '65970B39-4DC3-4CFA-A8E7-49C23978E923')
    IF @BlockId IS NOT NULL
    BEGIN
        INSERT INTO [HtmlContent] ([BlockId], [Version], [Content], [IsApproved], [Guid])
        VALUES (@BlockId, 1, '<div class=""well"">
    <h3>Blog Sidebar</h3>
    
    <p>
        Sidebar content for the blog.
    </p>
</div>', 1, '7237365D-09EE-4E55-ACB1-C58D3A5C10E3')
    END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Sidebar Content, from Page: Blog Details, Site: External Website
            RockMigrationHelper.DeleteBlock( "4A25275A-0CC8-4334-939E-C00320A016BD" );
            // Remove Block: Sidebar Content, from Page: Blog, Site: External Website
            RockMigrationHelper.DeleteBlock( "65970B39-4DC3-4CFA-A8E7-49C23978E923" );
            // Remove Block: Blog Details, from Page: Blog Details, Site: External Website
            RockMigrationHelper.DeleteBlock( "C146309D-E282-4FD5-94D9-CD4D0853AF09" );
            // Remove Block: Blog Content, from Page: Blog, Site: External Website
            RockMigrationHelper.DeleteBlock( "AE2D8454-AE86-40DF-A57A-C9F30B8AB50F" );
            RockMigrationHelper.DeletePage( "2D0D0FB0-68C4-47E1-8BC6-98F931497F5E" ); //  Page: Blog Details, Layout: Right Sidebar, Site: External Website
            RockMigrationHelper.DeletePage( "4857A6C9-F194-4B64-A9FB-6A8DC7A1A671" ); //  Page: Blog, Layout: Right Sidebar, Site: External Website

            Sql( @"
                -- TEMP DELETES
                delete from ContentChannelItem where [guid] in ('D5592927-2FAF-4131-8D6D-E9EA58165522', '006D5288-FE49-4C5D-82B6-BAC36445CD83', '8A3D944E-AD82-479F-89D5-9729CDB5D4C8', '08E0E677-1F98-487F-ACCD-7B0EC5AD50D1', 'C9029380-7295-4A0B-9CB6-2C134AC12C72', '5A95642A-A181-4043-A596-BD47D65B8654', '9AA0E16D-FE65-4164-B3A9-406E53A40F99', 'E214F2A7-CB53-4640-ACD5-CEAD2190C99D', '095466ED-029C-48BC-BCBB-FCF79CF8A68B', 'F946A31A-59CF-43A8-A595-B01AB2B69D6E', '4D9A94BE-33D9-4A38-8F36-393DE13CCFB7', '4134A71F-80DF-4482-BD33-3DB4DB9EC409')
                delete from [ContentChannel] where [Name] = 'Service Bulletin'
                delete from [ContentChannel] where [Name] = 'Website Blog'
                delete from [ContentChannelType] where [Guid] = 'C0549458-DCCF-4E02-8978-B15932576F68'
                delete from [Auth] where [Guid] in ('64C0AA99-BFE7-4CE0-9579-62AED07BD5D7','06DA6708-D142-4F32-A4B3-4180C5EE54C5',  'B2D4A357-30B3-4312-AFB8-9C0CBCDAE5A8', '2B408DA7-BDD1-4E71-B6AC-F22D786B605F', 'FD58183C-4B40-44FB-9418-867212341AEE', '2C2E1B6C-E08E-493E-A6AD-CC6A8A9755CA')
                delete from Attribute where [Guid] in ('FFDF621C-ECFF-4199-AB90-D678C36DCE38','AE455FB9-20FD-4767-9A65-4801B00FCD1A', '004D1F8A-749B-4F21-B93E-9F6A84E94651', '41C3F412-6CF7-4ED1-B40C-501861BB5F3D')
            " );

        
        }
    }
}
