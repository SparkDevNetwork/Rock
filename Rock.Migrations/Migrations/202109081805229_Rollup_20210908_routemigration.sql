
-- Update Capitalization on Routes
UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/steps/{StepTypeId}/{StepId}' WHERE ([Guid]='6BA3B394-C827-4548-94AE-CA9AD585CF3A')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/steps' WHERE ([Guid]='181A8246-0F80-44BE-A448-DADF680E6F73')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/security' WHERE ([Guid]='3F1A5C3F-53B6-46CA-AA66-6587C8FC56BC')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/persondocs' WHERE ([Guid]='AF2B7CB5-9CBA-41C4-A2DE-AB84FB5C3552')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/history' WHERE ([Guid]='F014966D-6F7D-4EFD-A90C-2B4371066448')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/groups' WHERE ([Guid]='78774FA5-2E1C-49F4-BE0A-F627D3CC0893')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/edit' WHERE ([Guid]='FCC0CCFF-8E18-48D8-A5EB-3D0F81D68280')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/extendedattributes' WHERE ([Guid]='4BEA223E-3D83-4388-9208-9926E5B8D1AC')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/contributions' WHERE ([Guid]='CE3819B7-1888-4384-AF52-18C5E78B0B04')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}/benevolence' WHERE ([Guid]='6839368F-9C0A-4811-BCF7-BF95441CBBE8')

UPDATE [PageRoute] SET [Route]=N'person/{PersonId}' WHERE ([Guid]='7E97823A-78A8-4E8E-A337-7A20F2DA9E52')

UPDATE [PageRoute] SET [Route]=N'group/{groupId}' WHERE ([Guid]='2BC75AF5-44AD-4BA3-90D3-15D936F722E8')

-- Remove GroupType Route, readd at end
DELETE FROM [PageRoute] WHERE ([Guid]='796D5B39-FF89-49E1-878C-D338FDD4D82C')

-- Remove Steps Route, readd at end
DELETE FROM [PageRoute] WHERE ([Guid]='4E4280B8-0A10-401A-9D69-687CA66A7B76')
-- Remove Steps Program, readd at end
DELETE FROM [PageRoute] WHERE ([Guid]='0B796F9D-1294-40E7-B264-D460D62B4F2F')
-- Remove Steps Record, readd at end
DELETE FROM [PageRoute] WHERE ([Guid]='C72F337F-4320-4CED-B5FF-20A443268123')
-- Remove Steps Type, readd at end
DELETE FROM [PageRoute] WHERE ([Guid]='74DF0B98-B980-4EF7-B879-7A028535C3FA')


DECLARE @PageId int

-- Page: Intranet
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0C4B3F4D-53FD-4A65-8C93-3868CE4DA137')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'intranet')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'intranet', '716897EE-4731-7670-379E-F3E61FC6916E' )

-- Page: Shared Documents
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FBC16153-897B-457C-A35F-28FDFDC466B6')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'intranet/shared-documents')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'intranet/shared-documents', '9458AEEA-186F-1286-39F3-C4F1CBBF251B' )

-- Page: Employee Resources
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '895F58FB-C1C4-4399-A4D8-A9A10225EA09')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'intranet/employee-resources')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'intranet/employee-resources', 'DD2F3317-404F-51AA-4781-2EC418EA7819' )

-- Page: Org Chart
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C3909F1A-6908-4035-BB93-EC4FBFDCC536')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'intranet/org-chart')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'intranet/org-chart', '452A4A87-062B-69FA-6C74-84950FB58D3B' )

-- Page: Employee Details
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'DA8E33F3-2EEF-4C4B-87F3-715C3F107CAF')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'intranet/org-chart/{GroupId}/member/{GroupMemberId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'intranet/org-chart/{GroupId}/member/{GroupMemberId}', 'B75DA909-55AC-25D3-617B-DAD994E03286' )

-- Page: Tools
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '164C7A7F-8C55-4E20-B582-D84D83174F2C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'intranet/tools')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'intranet/tools', 'AE3A1F9C-4AEA-35D6-3C1B-C4052C8A7561' )

-- Page: Weekly Metrics
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6E1DDCE6-F941-4AA9-8514-942E76AE3081')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'intranet/tools/weekly-metrics')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'intranet/tools/weekly-metrics', 'E15A5654-0981-1755-0B6F-13E783102A6A' )

-- Page: People
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '97ECDC48-6DF6-492E-8C72-161F76AE111B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people', 'FFE67E43-4E6B-9464-4F63-942BA61315D1' )

-- Page: Manage
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B0F4B33D-DD11-4CCC-B79D-9342831B8701')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/manage')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/manage', '609F9314-5D58-970A-2526-B9B592309B96' )

-- Page: Directory
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '215932E5-0FFB-48A4-B867-5DD7AD71216A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/directory')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/directory', '9017E738-0560-A4C5-691E-90C318E009CC' )

-- Page: New Family
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6A11A13D-05AB-4982-A4C2-67A8B1950C74')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/new-family')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/new-family', 'A8A40320-47FE-7E7E-8E23-79183EDC03EE' )

-- Page: Group Viewer
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4E237286-B715-4109-A578-C1445EC02707')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups', 'C3DEB34A-8359-2CDA-6EBB-81862D3209EB' )

-- Page: Group Member Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '3905C63F-4D57-40F0-9721-C60A2F681911')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/member/{Id}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/member/{Id}', 'C8480A7B-1688-4E5C-A537-90DD9CED1F5C' )

-- Page: Group Map
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '60995C8C-862F-40F5-AFBB-13B49CDA77EB')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/map')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/map', '7CD8EE68-2053-3E9A-40B8-6600F7928F88' )

-- Page: Group Attendance
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7EA94B4F-013B-4A79-8D01-86994EB04604')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/attendance')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/attendance', '03CE6C61-4D6C-04FA-984B-C9E8F98B27D0' )

-- Page: Attendance
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D2A75147-B031-4DF7-8E04-FDDEAE2575F1')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/attendance/{xxx}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/attendance/{xxx}', 'CB90A173-2A91-9767-9093-9CB5144E07C0' )

-- Page: Fundraising
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '3E0F2EF9-DC32-4DFD-B213-A410AE5B6AB7')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/fundraising')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/fundraising', '1AABDEF2-3123-06C1-50D5-3FBF04ED4A42' )

-- Page: Group History
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FCCF2570-DC09-4129-87BE-F1CAE25F1B9D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/history')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/history', 'A3D6AE7C-84EA-7B72-6FEC-E44F791B392B' )

-- Page: Group History Grid
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FB9A6BC0-0B51-4A92-A32C-58AC822CD2D0')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/history/grid')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/history/grid', '04BB812D-6E30-0C71-0F4E-4E5F9E5F6D56' )

-- Page: Group Member History
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'EAAB757E-524F-4DB9-A124-D5EFBCDCA63B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/history/member/{id}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/history/member/{id}', '46F5F6D6-8B4C-482B-4F7B-004F30C20E5C' )

-- Page: Group RSVP List
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '69285A6B-4DBB-43BB-8B0D-08DEBB860AEA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/rsvp')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/rsvp', '31E626BC-853C-98BA-4C89-AF3A915DA780' )

-- Page: Group RSVP Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '40E60703-CF52-4742-BDA6-65FB0CF198CB')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/groups/{GroupId}/rsvp/{id}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/groups/{GroupId}/rsvp/{id}', 'C4B394D7-2847-09E6-7E86-058617E76056' )

-- Page: Tags
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2654EBE9-F585-4E64-93F3-102357F89660')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/tags')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/tags', 'FBC5229D-4696-6FB6-66AA-6E2EC32D0F29' )

-- Page: Prayer
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1A3437C8-D4CB-4329-A366-8D6A4CBF79BF')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/prayer')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/prayer', '763A387F-8244-400E-4343-9B0C05499DE6' )

-- Page: Tag Details
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D258BF5B-B585-4C5B-BDCD-99F7519D45E2')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/tags/{TagId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/tags/{TagId}', 'D8D7B063-477C-22D2-39C4-C410E3B16C3A' )

-- Page: Add Prayer Request
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '36E22C5D-FC31-4754-8583-B63079217528')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/prayer/add')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/prayer/add', 'F5E526F5-3BAA-3D47-A008-8ACABC9491EF' )

-- Page: Prayer Requests
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '3149959B-EFAC-4C2D-B0E8-8CF4FA1BB2FF')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/prayer/list')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/prayer/list', 'D8441DAD-6049-3B2C-1181-F02250323203' )

-- Page: Prayer Request Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/prayer/list/{PrayerRequestId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/prayer/list/{PrayerRequestId}', '25084FC6-6FBC-8F1E-7685-8105AFD317EA' )

-- Page: Prayer Comments
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D10364AD-5E65-484B-967C-B52475E91B4C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/prayer/comments')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/prayer/comments', '7E65CEB0-58B6-289E-9674-ADD63ED461EF' )

-- Page: Prayer Request Attributes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C39C3E88-F423-424D-AA21-EB5CA7871A7B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/prayer/attributes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/prayer/attributes', '3A49BC3A-2624-1A0C-27C9-9652759EA156' )

-- Page: Merge People
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F0C4E25F-83DF-44FF-AB5A-EF6C3044FAD3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/merge')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/merge', '3ADD1F12-25FB-49FC-306F-1466D2225486' )

-- Page: Rapid Attendance Entry
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '78B79290-3234-4D8C-96D3-1901901BA1DD')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/rapid-attendance')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/rapid-attendance', '5A78DD2C-1099-57FA-95F5-CDCF31B46D1C' )

-- Page: Attendance List
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D56CD916-C3C7-4277-BEBA-0FA4C21A758D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/rapid-attendance/list')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/rapid-attendance/list', 'A0381C6A-1DCC-7A43-8317-C7ED4B659E2E' )

-- Page: Group Scheduling
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '896ED8DA-46A5-440B-92A0-76459869D921')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/group-scheduling')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/group-scheduling', '06B1361E-1ECA-84D1-71DA-EC0037129458' )

-- Page: Group Scheduler
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1815D8C6-7C4A-4C05-A810-CF23BA937477')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/group-scheduling/scheduler')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/group-scheduling/scheduler', 'ED73C8EE-5086-9E77-9B2D-DF4636E26B7D' )

-- Page: Group Schedule Status Board
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '31576E5D-7B6C-46D1-89F4-A14F4F8095D1')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/group-scheduling/status-board')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/group-scheduling/status-board', 'AC1F35D0-93E6-2D1B-4518-6F2E37FE5566' )

-- Page: Group Scheduler Analytics
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1E031B86-1476-4C72-9115-F94056398444')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/group-scheduling/analytics')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/group-scheduling/analytics', '8E34ECAB-A07E-4E0A-623E-505BBCC411D1' )

-- Page: Group Schedule Roster
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '37AE5C9E-7075-4F22-BDC6-189FA2584183')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/group-scheduling/roster')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/group-scheduling/roster', 'B0924C2B-5952-190F-2FC8-AE176D2E1FB3' )

-- Page: Group Schedule Communication
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'AFC2DA5B-B1D0-408C-ADBD-23E5D7A7AC67')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/group-scheduling/communication')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/group-scheduling/communication', 'ACC99CD4-3409-0F95-40DF-8683EEEC0008' )

-- Page: Achievements
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FCE0D006-F854-4107-9298-667563FA8D77')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/achievements')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/achievements', '406BCC27-9F59-49EA-8852-F3C81BB521AF' )

-- Page: Achievement Type
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1C378B3C-9721-4A9B-857A-E3C5188C5BF8')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/achievements/type/{AchievementTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/achievements/type/{AchievementTypeId}', '7774D6ED-74D2-4429-8E53-EAC5D44A54A0' )

-- Page: Attempt
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '75CDD408-3E1B-4EF3-9A6F-4DC76B92A80F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/achievements/attempt/{AchievementAttemptId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/achievements/attempt/{AchievementAttemptId}', '1AB49E07-3A83-2043-2155-B17155E3726A' )

-- Page: Streaks
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F81097ED-3C96-45F2-A4F8-7D4D4F3D17F3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/streaks')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/streaks', '9429846B-3C0A-A637-05DD-634B1F0160DE' )

-- Page: Streak Type
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CA566B33-0265-45C5-B1B2-6FFA6D4743F4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/streaks/type/{StreakTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/streaks/type/{StreakTypeId}', '6EDB4A02-28A9-45E7-204D-1262933B5CEB' )

-- Page: Map Editor
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E7D5B636-5F44-46D3-AE9F-E2681ACC7039')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/streaks/type/{StreakTypeId}/map-editor')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/streaks/type/{StreakTypeId}/map-editor', '6754D62D-2E6B-829F-25C2-71918FE9A2E9' )

-- Page: Exclusions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1EEDBA14-0EE1-43F7-BB8D-70455FD425E5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/streaks/type/{StreakTypeId}/exclusions')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/streaks/type/{StreakTypeId}/exclusions', '0DA9EFF8-7E47-723A-2335-0A6B32361844' )

-- Page: Exclusion
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '68EF459F-5D23-4930-8EA8-80CDF986BB94')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/streaks/type/{StreakTypeId}/exclusions/{StreakTypeExclusionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/streaks/type/{StreakTypeId}/exclusions/{StreakTypeExclusionId}', 'CE1284BC-0FE8-3D5C-A415-F94C118407B5' )

-- Page: Enrollment
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '488BE67C-EDA0-489E-8D80-8CC67F5854D4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/streaks/type/{StreakTypeId}/enrollment/{StreakId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/streaks/type/{StreakTypeId}/enrollment/{StreakId}', '3E2981EB-2B9C-6A83-8500-E77A7EC710F7' )

-- Page: Achievement Attempts
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4AC3D8B7-1A8A-40F9-8F51-8E09B863BA40')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/streaks/type/{StreakTypeId}/enrollment/{StreakId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/streaks/type/{StreakTypeId}/enrollment/{StreakId}', 'E9895796-70DA-6615-0563-6AF2A7625CD4' )

-- Page: Steps
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F5E8A369-4856-42E5-B187-276DFCEB1F3F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/steps')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/steps', '3E5D77C7-2140-1EED-66BC-22E63B3F1F4F' )

-- Page: Step Program
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6E46BC35-1FCB-4619-84F0-BB6926D2DDD5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/steps/program/{ProgramId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/steps/Program/{ProgramId}', 'F38326C5-17AB-6FCB-29B0-B9A9C3B71248' )

-- Page: Step Type
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '8E78F9DC-657D-41BF-BE0F-56916B6BF92F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/steps/type/{StepTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/steps/type/{StepTypeId}', '50487DE5-39EC-3346-6B73-ED56410D19B4' )

-- Page: Step Entry
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2109228C-D828-4B58-9310-8D93D10B846E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/steps/record/{StepId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/steps/record/{StepId}', 'E823B59A-819A-A443-A05D-3CE2A692851D' )

-- Page: Bulk Entry
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '8224D858-04B3-4DCD-9C73-F9868DF29C95')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/steps/type/{StepTypeId}/bulk-entry')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/steps/type/{StepTypeId}/bulk-entry', 'D5EA0874-8431-1096-704B-F38F567180DA' )

-- Page: Connections
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2A0C135A-8421-4125-A484-83C8B4FB3D34')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/connections')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/connections', 'CDCEA82F-484B-0FCD-0E7E-1F959E0C28F9' )

-- Page: Connection Board
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4FBCEB52-8892-4035-BDEA-112A494BE81F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/connections/board')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/connections/board', '18B7FF61-442C-00DB-8B20-191DFBA92B2C' )

-- Page: Connection Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/connections/types')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/connections/types', '197D9507-795B-3F50-92F0-E6E0D41661A2' )

-- Page: Connection Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'DEFF1AFE-2C33-4E56-B0F5-BE3B75224186')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/connections/types/{ConnectionTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/connections/types/{ConnectionTypeId}', '950DBEC4-7524-7B59-0651-B796F9219829' )

-- Page: Connection Opportunity Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0E5797FF-A507-4E02-891F-B80AF353E585')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/connections/types/{ConnectionTypeId}/opportunity/{ConnectionOpportunityId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/connections/types/{ConnectionTypeId}/opportunity/{ConnectionOpportunityId}', 'D03DD848-7DD8-4EED-19DD-410595D46632' )

-- Page: Connection Campaigns
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B252FAA6-0E9D-41CD-A00D-E7159E881714')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/connections/campaigns')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/connections/campaigns', '82CC5086-6687-51D7-86E8-62AA7B1F9DE2' )

-- Page: Campaign Configuration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A22133B5-B5C6-455A-A300-690F7926356D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'people/connections/campaigns/{ConnectionCampaignGuid}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'people/connections/campaigns/{ConnectionCampaignGuid}', '17DBF7D7-41C6-9F93-950B-0D52AC6D3120' )

-- Page: Communication Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'communications/{CommunicationId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'communications/{CommunicationId}', 'CDBC4305-9DFC-35F8-7305-4ECBED604A0A' )

-- Page: New Communication
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'communications/new')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'communications/new', '01A3891B-9998-7E30-20DC-58081A239D65' )

-- Page: Simple Communication
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7E8408B2-354C-4A5A-8707-36754AE80B9A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'communications/new/simple')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'communications/new/simple', '6A9D686F-98E0-69E9-80EB-69BA1F5C1B2F' )

-- Page: Communication History
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CADB44F2-2453-4DB5-AB11-DADA5162AB79')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'communications')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'communications', 'FC3D117D-3A8C-2C23-34C0-FEF3B4B457FF' )

-- Page: SMS Conversations
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '275A5175-60E0-40A2-8C63-4E9D9CD39036')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'communications/sms-conversations')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'communications/sms-conversations', '36E8AD99-0F7B-56E8-9F16-D1B227C6878D' )

-- Page: Mass Push Notification
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '3D97725E-5E17-411F-856C-F4B79B9BFF15')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'communications/push-notification')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'communications/push-notification', '64C32DAE-79FB-8E98-2DF8-C452D0435BDD' )

-- Page: Email Analytics
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'DF014200-72A3-48A0-A953-E594E5410E36')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'communications/email-analytics')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'communications/email-analytics', 'EF2F8375-26E6-8F3E-A64A-BABBCBFF563F' )

-- Page: Person Profile
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '08DBD8A5-2C35-4146-B4A8-0F7652348B25')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}', '8DFFFC2C-0C92-932B-44FB-24070BE752EB' )

-- Page: Edit Person
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'AD899394-13AD-4CAB-BCB3-CFFD79C9ADCC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/edit')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/edit', 'F8EF6314-5940-1C3C-251B-353F1A381D24' )

-- Page: Edit Family
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E9E1E5F2-467D-47CB-AF41-B4D9EF8B0B27')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/editfamily/{GroupId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/editfamily/{GroupId}', '3333886B-021D-7DA3-A5A1-CBC1F5292FE4' )

-- Page: Extended Attributes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1C737278-4CBA-404B-B6B3-E3F0E05AB5FE')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/extended-attributes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/extended-attributes', 'DDBE17D5-09D5-0B03-806A-9E14CF776F43' )

-- Page: Steps
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CB9ABA3B-6962-4A42-BDA1-EA71B7309232')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/steps')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/steps', 'EE5339A3-5CB3-3D61-9F4C-597E8A209E94' )

-- Page: Groups
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/groups')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/groups', '1980C9CC-19E6-A399-7672-F085808767D3' )

-- Page: Documents
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6155FBC2-03E9-48C1-B2E7-554CBB7589A5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/documents')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/documents', '18C1BF08-29B2-16E9-68F3-4BB047691B59' )

-- Page: Contributions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '53CF4CBE-85F9-4A50-87D7-0D72A3FB2892')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/contributions')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/contributions', '976180D6-22C6-533A-1BBA-2DD4B9660424' )

-- Page: Benevolence
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '15FA4176-1C8E-409D-8B47-85ADA35DE5D2')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/benevolence')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/benevolence', '79FA360F-68AA-61F3-612B-F0137CD59706' )

-- Page: Benevolence Request Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '648CA58C-EB12-4479-9994-F064070E3A32')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/benevolence/{BenevolenceRequestId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/benevolence/{BenevolenceRequestId}', '49EFE0A8-84CE-5DA8-1EC6-6BB5A496A3E9' )

-- Page: Security
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0E56F56E-FB32-4827-A69A-B90D43CB47F5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/security')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/security', '770EDB7F-712F-9FF4-72AA-32FEFD0D1D1D' )

-- Page: Person Viewed Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '48A9DF54-CC19-42FA-BDC6-97AF3E63029D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{TargetId}/security/viewed-by/{ViewerId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{TargetId}/security/viewed-by/{ViewerId}', '710514C7-4733-A015-7D74-C6FDB8ED6F94' )

-- Page: History
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/history')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/history', '719A59D8-1A33-04A6-8CDB-A368311D7EE7' )

-- Page: Document
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C6503D6B-F61A-4A8A-BDD1-11F9FB65B66F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'person/{PersonId}/history/documents/{SignatureDocumentId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'person/{PersonId}/history/documents/{SignatureDocumentId}', '443E245E-4431-7F83-94D4-DEAA15AD3924' )

-- Page: Finance
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7BEB7569-C485-40A0-A609-B0678F6F7240')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance', '80C8056A-6766-4267-5801-3AE448C455BB' )

-- Page: Functions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '142627AE-6590-48E3-BFCA-3669260B8CF2')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/functions')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/functions', '504C9A14-381E-4E5C-01CC-BF2070993280' )

-- Page: Transactions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7CA317B5-5C47-465D-B407-7D614F2A568F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/transactions')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/transactions', 'C157C9F3-62E0-A1B3-5B69-0A330A7E1A5C' )

-- Page: Transaction Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B67E38CB-2EF1-43EA-863A-37DAA1C7340F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/transactions/{TransactionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/transactions/{TransactionId}', 'A3B46501-1D86-26F5-48B8-AC0682749E0A' )

-- Page: Pledges
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/pledges')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/pledges', 'B9D27463-098E-7DFF-3E5E-8C5B37DB196A' )

-- Page: Pledge Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'EF7AA296-CA69-49BC-A28B-901A8AAA9466')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/pledges/{pledgeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/pledges/{pledgeId}', 'BEF1BD41-1885-7AEB-9DAA-E3A816E64F88' )

-- Page: Scheduled Transactions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F23C5BF7-4F52-448F-8445-6BAEEE3030AB')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/scheduled-transactions')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/scheduled-transactions', '03FE8A48-0B88-780D-2E37-BF5A6FA83852' )

-- Page: Scheduled Transaction
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '996F5541-D2E1-47E4-8078-80A388203CEC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/scheduled-transactions/{ScheduledTransactionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/scheduled-transactions/{ScheduledTransactionId}', '2938B6D2-4F09-403E-2B16-18267AD259DA' )

-- Page: Edit Scheduled Transaction
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F1C3BBD3-EE91-4DDD-8880-1542EBCD8041')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/scheduled-transactions/{ScheduledTransactionId}/edit')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/scheduled-transactions/{ScheduledTransactionId}/edit', '6E274C6D-9C2E-9FEA-54DF-3C8F20CF35D4' )

-- Page: Batches
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'EF65EFF2-99AC-4081-8E09-32A04518683A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/batches')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/batches', '9FF66C42-A128-5066-05C3-C939DFB14553' )

-- Page: Financial Batch Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '606BDA31-A8FE-473A-B3F8-A00ECF7E06EC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/batches/{batchId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/batches/{batchId}', '0553B322-1096-41D4-615F-C5355C89073D' )

-- Page: Transaction Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '97716641-D003-4663-9EA2-D9BB94E7955B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/batches/{batchId}/transaction/{transactionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/batches/{batchId}/transaction/{transactionId}', '59574C13-5B9D-3FC1-7B7E-ED3C26A7874E' )

-- Page: Transaction Matching
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CD18FE52-8D6A-49C9-81BF-DF97C5BA0302')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/batches/{batchId}/matching')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/batches/{batchId}/matching', '43603C2E-493B-4A9C-9ED7-F9842FED5B62' )

-- Page: Audit Log
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CBE0C5ED-744E-4392-A9D4-0DC57AF11D33')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/batches/{BatchId}/audit')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/batches/{BatchId}/audit', 'FEE76A3D-5A6F-9187-40EA-751F710D3287' )

-- Page: Benevolence
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D893CCCC-368A-42CF-B36E-69991128F016')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/benevolence')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/benevolence', '273530EB-1623-59C6-66CE-FDF411FE09E8' )

-- Page: Benevolence Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6DC7BAED-CA01-4703-B679-EC81143CDEDD')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/benevolence/{BenevolenceRequestId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/benevolence/{BenevolenceRequestId}', '6DF1E78E-7146-62BB-5355-2EA747A2A4B1' )

-- Page: Benevolence Request Summary
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D676A464-29A0-49F1-BA8C-752D9FE21026')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/benevolence/{benevolencerequestid}/summary')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/benevolence/{benevolencerequestid}/summary', 'A02DBEAD-846E-4918-1F70-4AA56DB634F3' )

-- Page: Fundraising Matching
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A3EF32AC-B0FE-4140-A6F4-134FDD247CBD')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/fundraising-matching')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/fundraising-matching', 'F0AD93D9-7193-6CBF-33C7-C6D415623C7A' )

-- Page: Event Registration Matching
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '507F7AC2-75A2-49AA-9EE4-F6DFCD34A3DC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/event-registration-matching')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/event-registration-matching', '487290C6-4883-2099-1FCE-4BD5D068302B' )

-- Page: Transaction Fee Report
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A3E321E9-2FBB-4BB9-8AEE-E810B7CC5914')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/fee-report')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/fee-report', 'E67B3532-46E4-099C-5614-33D324F9421E' )

-- Page: Administration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/administration')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/administration', '114FEFC1-5EC6-9276-70E0-883CCF354695' )

-- Page: Accounts
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2B630A3B-E081-4204-A3E4-17BB3A5F063D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/accounts')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/accounts', 'A900EC6D-609C-032D-08E6-5FA9548B2361' )

-- Page: Order Top-Level Accounts
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'AD1ED5A5-2E43-433F-B1C3-E6052213EF71')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/accounts/order')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/accounts/order', '6E62358A-5BEA-6B7B-8D24-A3762F64333A' )

-- Page: Businesses
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F4DF4899-2D44-4997-BA9B-9D2C64958A20')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/businesses')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/businesses', '6D1B8D8B-3D62-2EFD-4E17-C5FDC64984C6' )

-- Page: Business Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D2B43273-C64F-4F57-9AAE-9571E1982BAC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/business/{BusinessId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/business/{BusinessId}', '6E177F5C-0BBD-7B31-466D-948F12E60C35' )

-- Page: Transaction Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CC7BE14E-3680-4E78-AACC-A57A8D42350F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/business/{PersonId}/transaction/{TransactionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/business/{PersonId}/transaction/{TransactionId}', '1F1885E1-6A40-0688-937D-B00822A3038F' )

-- Page: Business Conversion
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '94B07FB1-41C1-4755-87E4-0892406D1F3D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/convert-business')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/convert-business', 'C35C68CC-8D47-3F13-3E86-1B69177232CC' )

-- Page: Business Merge
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0B863363-CCA3-4EDE-9ABA-7ED22A88F503')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/businesses/merge/{Set}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/businesses/merge/{Set}', '72A3F06C-7BC5-47AF-4CC9-5DF5F1C04996' )

-- Page: Giving Analytics
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D34B3916-1ABD-4F16-B820-5AAAA761F77F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/giving-analytics')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/giving-analytics', 'DC876773-1B2F-289D-8486-2D5961A55284' )

-- Page: Download Payments
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '720819FC-1730-444A-9DE8-C98D29954170')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/download-payments')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/download-payments', '88CF6CEB-2136-3BFA-5252-9865B8E1469F' )

-- Page: Pledge Analytics
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FEB2332D-4605-4E2B-8EF2-2C6B1A9612C3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/pledge-analytics')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/pledge-analytics', 'D8721076-5020-794C-0927-B391DEEF9E0D' )

-- Page: Financial Settings
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '90723727-56EC-494D-9708-E188869D900C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/settings')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/settings', 'C8D98763-5AB6-8FCB-9FFA-1EFBFD2F4B7A' )

-- Page: Contribution Templates
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D5269942-0B3B-4447-8EE9-F5DEB7657003')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/settings/contribution-templates')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/settings/contribution-templates', '4E527CE0-1CBA-4C78-2EC7-F523DAF87407' )

-- Page: Contribution Template Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D4CB4CE6-FBF9-4FBD-B8C4-08BE022F97D7')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/settings/contribution-templatess/{StatementTemplateId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'finance/settings/contribution-templatess/{StatementTemplateId}', '0490DB2A-2076-4CC5-72A1-5A0143743AD6' )

-- Page: Reporting
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'BB0ACD18-24FB-42BA-B89A-2FFD80472F5B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting', '2CA6D2F5-873D-85C1-9961-25950CD127D8' )

-- Page: Data Views
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4011CB37-28AA-46C4-99D5-826F4A9CADF5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/dataviews')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/dataviews', '9F0FD4F6-4B82-5F9B-3698-C8BF05B03163' )

-- Page: Reports
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/reports')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/reports', '8FA468DC-0C40-0C77-8EE9-EEA35443A2D6' )

-- Page: Metrics
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '78D84825-EB1A-43C6-9AD5-5F0F84CC9A53')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/metrics')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/metrics', '2E181C13-5E0A-8DB1-5412-9F4BE5DC94F1' )

-- Page: Metrics
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '78D84825-EB1A-43C6-9AD5-5F0F84CC9A53')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/metrics/{MetricCategoryId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/metrics/{MetricCategoryId}', 'BAB5CB01-88BB-3166-9269-866DEF802294' )

-- Page: Attendance Analytics
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7A3CF259-1090-403C-83B7-2DB3A53DEE26')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/attendance-analytics')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/attendance-analytics', '4611CC6C-3CD4-2345-2001-DDEE9D05427E' )

-- Page: Data Integrity
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '84FD84DF-F58B-4B9D-A407-96276C40AB7E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity', '2CB9EC95-357C-18D1-81E3-2E258F1626BA' )

-- Page: Duplicate Finder
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '21E94BF1-C594-44B6-AD91-939ABD04D36E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/duplicate-finder')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/duplicate-finder', 'E7E72397-0643-8BF1-A309-AE5C1F0F38DA' )

-- Page: Duplicate Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6F9CE971-75DF-4F2A-BD5E-A12B149A442E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/duplicate-finder/{PersonId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/duplicate-finder/{PersonId}', '1565630A-7922-545D-779A-3D69A87220A2' )

-- Page: Reports
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '134D8730-6AF5-4518-89EE-7370FA78676E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/reports')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/reports', '6318AB32-9B6D-5670-6FA8-E1C226CA5B0F' )

-- Page: Report Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'DB58BC69-01FA-4F3E-832B-B1D0DE915C21')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/reports/{ReportId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/reports/{ReportId}', 'B6771022-9787-5416-93B4-2286C8FB7472' )

-- Page: Workflows
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '90C32D5E-A5D5-4CE4-AAB0-E31B43B585E4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/workflows')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/workflows', '5E1C99E6-7F3A-7CA7-3FD2-7132FE0A7554' )

-- Page: Location Editor
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '47BFA50A-68D8-4841-849B-75AB3E5BCD6D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/locations')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/locations', 'A44CDC58-A434-5E8C-8AD5-1368FA891CC9' )

-- Page: Location Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1602C1CA-2EC7-4163-B0E1-1FE7306AC2B4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/locations/{LocationId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/locations/{LocationId}', 'B3E5323A-33FB-1602-A71E-070F56EF8AF0' )

-- Page: Photo Requests
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '325B50D6-545D-461A-9CB7-72B001E82F21')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/photo-requests')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/photo-requests', '80ABFE1B-42F1-190A-95C6-C00D099C198E' )

-- Page: Verify Photos
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '07E4BA19-614A-42D0-9D75-DFB31374844D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/photo-requests/verify')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/photo-requests/verify', 'E35FA751-24E2-832E-2C75-B9F9C0A7754A' )

-- Page: Photo Request Application Group
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '372BAF1A-F619-46FC-A69A-61E2A0A82F0E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/photo-requests/list')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/photo-requests/list', '01022F49-44A4-4271-4E01-C593CEDA398E' )

-- Page: Group Member Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '34491B77-E94D-4DA6-9E74-0F6086522E4C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/photo-requests/list/{GroupMemberId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/photo-requests/list/{GroupMemberId}', 'B4AFCB3C-7031-3AE5-2FC2-9E907BA458C3' )

-- Page: Merge Requests
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5180AE8E-BF1C-444F-A154-14E5A8A4ACC9')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/merge-requests')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/merge-requests', '6F4FCA32-4E58-5C69-5392-4B692724569B' )

-- Page: Data Automation
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A2D5F989-1E30-47B9-AAFC-F7EC627AFF21')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/data-automation')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/data-automation', 'CFCD8824-8E46-15DF-036C-9512EFF739F7' )

-- Page: NCOA Results
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B0C9AF70-752E-0F9A-4BA5-29B2E4C69B11')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/ncoa-results')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/ncoa-results', '3465CB0E-445A-0504-0DA4-BC29AFE58600' )

-- Page: Connection Status Changes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '97624123-900C-4442-B42E-19CF95877E04')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/data-integrity/connection-status-changes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/data-integrity/connection-status-changes', '815AED73-23D2-02A4-9052-6F8D975C4EA6' )

-- Page: Interactions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A9661D86-83B6-4AC1-B988-B5CC942A9ED6')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/interactions')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/interactions', '93A8B696-9D8E-2E79-50E3-3CE660CF617D' )

-- Page: Sessions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '756D37B7-7BE2-497D-8D37-CC273FE29659')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/interactions/sessions/{ChannelId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/interactions/sessions/{ChannelId}', 'DCE30C9B-64A8-8517-5481-E26B09E88F37' )

-- Page: Channel Details
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'AF2FBEB8-1E47-4F51-A503-3D73C0D66B4E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/interactions/channels/{ChannelId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/interactions/channels/{ChannelId}', '82F3FB8E-0FBA-8B24-89B0-5B5BBE467D5D' )

-- Page: Component Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '9043D8F9-F9FD-4BE6-A50B-ABF9821EC0CD')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/interactions/components/{ComponentId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/interactions/components/{ComponentId}', 'E72F3CDB-9939-8377-5641-8FE7454A295F' )

-- Page: Interaction Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B6F6AB6F-A572-45FE-A143-2E4B8F192C8D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'reporting/interactions/interactions/{InteractionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'reporting/interactions/interactions/{InteractionId}', '8DAC03E9-9BFA-3A60-A36E-30C1E0DE1435' )

-- Page: HTML Content Approval
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '9DF95EFF-88B4-401A-8F5F-E3B8DB02A308')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/html-content-approval')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/html-content-approval', '72FDCCD9-7712-421F-1812-B5825BD09DE3' )

-- Page: Content
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '117B547B-9D71-4EE9-8047-176676F5DC8C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/content')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/content', '940F96A6-8D55-4655-061B-42B2336A20E0' )

-- Page: Calendars
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '63990874-0DFF-45FC-9F09-81B0B0D375B4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/calendars')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/calendars', '4A1EE472-47D5-46B3-8F3C-AFE176805151' )

-- Page: Event Calendar
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B54725E1-3640-4419-B580-2AF77DAF6568')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/calendars/{EventCalendarId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/calendars/{EventCalendarId}', 'B607DEEF-7927-1EFD-92E0-054D2874A723' )

-- Page: Event Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7FB33834-F40A-4221-8849-BB8C06903B04')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/calendars/{EventCalendarId}/event/{EventItemId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/calendars/{EventCalendarId}/event/{EventItemId}', 'D8A5E048-4652-4E1C-3801-D77CDD9E4BCE' )

-- Page: Event Occurrence
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4B0C44EE-28E3-4753-A95B-8C57CD958FD1')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/calendars/{EventCalendarId}/event/{EventItemId}/occurrence/{EventItemOccurrenceId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/calendars/{EventCalendarId}/event/{EventItemId}/occurrence/{EventItemOccurrenceId}', '126E95EB-3814-9B2F-185E-5AA5FD8838C9' )

-- Page: Content Channel Item
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6DFA80C3-E2A4-479F-ADDF-98EAC31169E0')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/calendars/{EventCalendarId}/event/{EventItemId}/occurrence/{EventItemOccurrenceId}/contentchannelitem/{ContentChannelId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/calendars/{EventCalendarId}/event/{EventItemId}/occurrence/{EventItemOccurrenceId}/contentchannelitem/{ContentChannelId}', 'FF172D4E-712A-3DD3-4C63-08DC5A2B5D3E' )

-- Page: Calendar Attributes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '9C610805-BE44-42DF-A73F-2C6D0014AD49')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/calendars/attributes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/calendars/attributes', 'E5B9B0E6-9D2D-17AD-54A3-79B86CD1413F' )

-- Page: Event Registration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '614AF351-6C48-4B6B-B50E-9F7E03BC00A4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations', 'E577E9CF-7EE9-3350-2CE9-F9E570549AA0' )

-- Page: Registrations
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '844DC54B-DAEC-47B3-A63A-712DD6D57793')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}', '27CE7638-1DE1-91D4-7AD8-8F916EA99C6A' )

-- Page: Registration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FC81099A-2F98-4EBA-AC5A-8300B2FE46C4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/registration/{RegistrationId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/registration/{RegistrationId}', 'FCF247C2-9088-15BC-8E95-891141A238F2' )

-- Page: Registrant
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '52CA0336-FC25-4131-BB5A-94A628C0EE77')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationId}/registrant/{RegistrantId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationId}/registrant/{RegistrantId}', '4CD69CD2-57DB-150F-3747-5DE721D3000C' )

-- Page: Registration Audit Log
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '747C1DAA-1E77-45CB-99C5-7F4D030F824E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/registration/{RegistrationId}/audit-log')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/registration/{RegistrationId}/audit-log', '47F893B8-1184-7A3F-2ECF-CA62B319100F' )

-- Page: Payment Reminders
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2828BBCF-B3FC-4707-B063-086748853978')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/reminders')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/reminders', 'B96C3A60-2C6C-0CE7-3561-5F25D3872D05' )

-- Page: Registrants
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6138DA76-BD9A-4373-A55C-F88F155E1B13')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/registrants')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/registrants', '73242BCD-66B2-585D-3F4E-89908DE81C05' )

-- Page: Payments
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '562D6252-D614-4ED4-B602-D8160066611D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/payments')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/payments', '44EEE6D6-8680-20FA-16CE-8FDFFF6731E6' )

-- Page: Fees
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B0576A70-CCB3-4E98-B6C4-3D758DD5F609')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/fees')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/fees', '849D5EB0-131E-36D1-6801-1F10B02E3077' )

-- Page: Discounts
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6EE74759-D11B-4911-9BC8-CF23DE5534B2')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/discounts')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/discounts', '776B5324-3F80-2C32-8BF4-41A1757E0E16' )

-- Page: Linkages
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '8C2C0EDB-60AD-4FA3-AEDA-45B972CA8CC5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/linkages')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/linkages', '94170F30-6682-16B1-808C-A8F759A47BCC' )

-- Page: Linkage
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'DE4B12F0-C3E6-451C-9E35-7E9E66A01F4E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/linkages/{LinkageId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/linkages/{LinkageId}', '188F1C36-8008-64BC-01A8-E27791B7A0A9' )

-- Page: Wait List
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E17883C2-6442-4AE5-B561-2C783F7F89C9')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/wait-list')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/wait-list', '7EFB3E32-254F-2D32-6F0A-3AC24658165D' )

-- Page: Placement Groups
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0CD950D7-033D-42B1-A53E-108F311DC5BF')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-registrations/{RegistrationInstanceId}/placements/{RegistrationTemplatePlacementId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-registrations/{RegistrationInstanceId}/placements/{RegistrationTemplatePlacementId}', 'F71FB02D-33C4-51B5-32A2-50E8DF7DA0BE' )

-- Page: Event Wizard
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7F889C16-0656-4015-8A90-B43D3BD2467E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/event-wizard')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'web/event-wizard', '9F84BD5F-7FD7-2024-058A-F5495EB60147' )

-- Page: General Settings
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0B213645-FA4E-44A5-8E4C-B2D8EF054985')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general', 'B76C85E1-53E1-6DAB-30E4-234CE4B22FF2' )

-- Page: Rock Update
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A3990266-CB0D-4FB5-882C-3852ED5D96AB')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/update')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/update', '5D6F0F4B-1BB7-4766-7021-6261F94B8FB6' )

-- Page: Global Attributes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A2753E03-96B1-4C83-AA11-FCD68C631571')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/global-attributes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/global-attributes', '6D27F23F-0840-016D-0ECD-D5AEF98E0656' )

-- Page: Defined Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/defined-types')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/defined-types', '0B8B2D30-515D-01F6-0AD5-2AEB9BD368CD' )

-- Page: Defined Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '60C0C193-61CF-4B34-A0ED-67EF8FD44867')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/defined-types/{definedTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/defined-types/{definedTypeId}', 'EA7212EE-700E-0DA9-81EA-A794773414B6' )

-- Page: Group Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '40899BCD-82B0-47F2-8F2A-B6AA3877B445')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/group-types')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/group-types', '667796DA-4E88-97EB-942E-9CFD292423A3' )

-- Page: Group Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/group-types/{groupTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/group-types/{groupTypeId}', 'E4F573DF-7D92-0F00-2DCB-5B2A762F9C09' )

-- Page: Campuses
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5EE91A54-C750-48DC-9392-F1F0F0581C3A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/campuses')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/campuses', 'C63ACBD7-5A23-657D-7290-BF0D2B1F5869' )

-- Page: Campus Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'BDD7B906-4D42-43C0-8DBB-B89A566734D8')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/campuses/{campusId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/campuses/{campusId}', 'DF183C66-A0A0-9659-8124-BB9BE7FB1EB7' )

-- Page: Group Member Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'EB135AE0-5BAC-458B-AD5B-47460C2BFD31')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/campuses/{campusId}/members/{GroupMemberId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/campuses/{campusId}/members/{GroupMemberId}', '83A9D656-69EB-8BE3-5011-8642153067F4' )

-- Page: Tags
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F111791B-6A58-4388-8533-00E913F48F41')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/tags')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/tags', '6E297376-65DE-008B-38C6-66E3C7381767' )

-- Page: Tag
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F3BD2F37-F16A-4C98-8A4C-C14A16AAFA3A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/tags/{TagId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/tags/{TagId}', 'D3233340-09D7-82E4-A107-F5C393CC890A' )

-- Page: Workflow Configuration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/workflows')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/workflows', '561306E2-4F89-13EA-23A8-E031ADC9208E' )

-- Page: Workflow Triggers
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1A233978-5BF4-4A09-9B86-6CC4C081F48B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/workflow-triggers')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/workflow-triggers', 'A308A838-87F4-48EF-161C-5FA2E5E6868A' )

-- Page: Workflow Trigger
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '04D844EA-7780-427B-8912-FA5EB7C74439')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/workflow-triggers/{WorkflowTriggerId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/workflow-triggers/{WorkflowTriggerId}', '5AEA99A4-0A53-8057-2FB7-5C91EB6C7B3C' )

-- Page: File Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '66031C31-B397-4F78-8AB2-389B7D8731AA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/file-types')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/file-types', '44C3277B-3E52-0D8E-128D-6001F8BD338D' )

-- Page: File Type
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '19CAC4D5-FE82-4AE0-BFD3-3C12E3024574')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/file-types/{BinaryFileTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/file-types/{BinaryFileTypeId}', 'A0A8314B-0C90-3EB4-45E3-D3F818636076' )

-- Page: Named Locations
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2BECFB85-D566-464F-B6AC-0BE90189A418')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/locations')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/locations', '2F4897EF-1656-705E-7765-7893F3E31B10' )

-- Page: Devices
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/devices')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/devices', '76004E74-54B6-6DCB-5380-EB82813C1840' )

-- Page: Device Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'EE0CC3F3-50FD-4161-BA5C-A852D4A10E7B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/devices/{DeviceId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/devices/{DeviceId}', '99D4BE70-9B22-097E-22F0-EC47BB6A5137' )

-- Page: Schedules
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/schedules')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/schedules', '9B6FDFBA-9D52-5313-24CB-CB2125D11BD2' )

-- Page: Attribute Categories
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '220D72F5-B589-4378-9852-BBB6F145AD7F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/attribute-categories')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/attribute-categories', '60E89391-0D1C-4361-40C0-F1D593C23D1E' )

-- Page: Prayer Categories
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FA2A1171-9308-41C7-948C-C9EBEA5BD668')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/prayer-categories')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/prayer-categories', '68A25E05-1995-3DD5-6B17-8B5E2E039F98' )

-- Page: Person Attributes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7BA1FAF4-B63C-4423-A818-CC794DDB14E3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/person-attributes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/person-attributes', 'D41EAA48-9C8E-1B5D-5DB7-5F6CE0C05BA0' )

-- Page: Badges
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '26547B83-A92D-4D7E-82ED-691F403F16B6')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/badges')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/badges', '91FBB1B1-4765-0CF5-6929-23A73FFF2098' )

-- Page: Badge
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D376EFD7-5B0D-44BF-A44D-03C466D2D30D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/badges/{BadgeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/badges/{BadgeId}', '6E84A513-921A-7377-A0BC-A2F290784A33' )

-- Page: Merge Templates
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '679AF013-0093-435E-AA49-E73B99EB9710')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/merge-templates')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/merge-templates', 'B6735C5A-0ABC-20AB-7028-BD4D9ACF0606' )

-- Page: Group Requirement Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0EFB7285-5D70-4798-ADE9-908311ECC074')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/group-requirement-types')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/group-requirement-types', '42B2FD16-955B-65EE-597D-882061307AED' )

-- Page: Group Requirement Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E64270AA-9246-4BA4-B1A9-EC2212F586DC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/group-requirement-types/{GroupRequirementTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/group-requirement-types/{GroupRequirementTypeId}', '8FBBBC1A-26AC-3338-72AA-B2BB37CE78F3' )

-- Page: Signature Documents
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CB1C42A2-285C-4BC5-BB2C-DC442C8A97C2')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/signature-documents')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/signature-documents', '8AE1793F-2495-A216-9C76-D5C8AD3C3C1C' )

-- Page: Document Templates
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7096FA12-07A5-489C-83B0-EE55494A3484')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/signature-documents/{SignatureDocumentTemplateId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/signature-documents/{SignatureDocumentTemplateId}', 'B11EF3AD-004B-2F3E-6D5A-4A6AE09B9D16' )

-- Page: Universal Search Control Panel
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7AE403F2-4328-4168-A941-0A506F1AAE14')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/universal-search')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/universal-search', 'FE815D23-3882-7B52-7F98-D8B07D8101FD' )

-- Page: Attribute Matrix Templates
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6C43A9B6-EADC-4E32-854A-B40376CF8CAF')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/attribute-matrix')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/attribute-matrix', 'DF607508-070A-32A0-9AAB-50FD9B7E1BE1' )

-- Page: Attribute Matrix Template Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '601DE4F6-2290-4A5A-AC96-32FB6A133C28')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/attribute-matrix/{AttributeMatrixTemplateId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/attribute-matrix/{AttributeMatrixTemplateId}', '43AF1A33-39F1-02A5-A4C4-971E082D98B4' )

-- Page: Tag Categories
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '86C9ECAC-3EC2-4588-AF1B-E94F0428EA1F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/tag-categories')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/tag-categories', '35B9599D-72FC-43C1-320B-A43DD9540439' )

-- Page: Archived Groups
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '93C79597-2274-4291-BE4F-E84569BB9B27')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/archived-groups')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/archived-groups', '592995AE-814D-4364-39A9-5BF4D35A803C' )

-- Page: Group Member Schedule Templates
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1F50B5C5-2486-4D8F-9435-27BDF8302683')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/schedule-templates')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/schedule-templates', '2D68BC0E-A232-5D05-2568-0E6E57771FF2' )

-- Page: Group Member Schedule Template Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B7B0864D-91F2-4B24-A7B0-FC7BEE769FA0')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/schedule-templates/{GroupMemberScheduleTemplateId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/schedule-templates/{GroupMemberScheduleTemplateId}', 'F49CB40B-42B9-5162-198D-DCF543D20B67' )

-- Page: Document Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '8C199C6C-7457-4256-9ABB-83DABD2E6282')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/document-types')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/document-types', '77F8B6F6-58DD-8A78-9F64-FF678B1D803B' )

-- Page: Document Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FF0A71AD-6282-49E4-BD35-E84369E0D94A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/document-types/{DocumentTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/document-types/{DocumentTypeId}', 'BC0912EB-5F0F-823B-3DBC-FEFCFB2900DD' )

-- Page: Legacy Rock Updater
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'EA9AE18F-3DBF-494D-947D-31BCE363DF39')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/general/update/legacy')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/general/update/legacy', '6590F803-8D60-64F1-826F-5E3C3EE589E4' )

-- Page: CMS Configuration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B4A24AB7-9369-4055-883F-4F4892C39AE3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms', '795F2537-9CBF-A3C1-70D3-6B81A9DC642D' )

-- Page: Routes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4A833BE3-7D5E-4C38-AF60-5706260015EA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/routes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/routes', 'A6A25EC9-4E26-33C9-75BF-3D0B65CE669F' )

-- Page: Page Route Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '649A2B1E-7A15-4DA8-AF67-17874B6FE98F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/routes/{pageRouteId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/routes/{pageRouteId}', '990B3E95-95B2-81BC-675F-65346A5E4D21' )

-- Page: Sites
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7596D389-4EAB-4535-8BEE-229737F46F44')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/sites')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/sites', 'E939B79F-559B-4B21-4102-5107D70A8DD6' )

-- Page: Site Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A2991117-0B85-4209-9008-254929C6E00F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/sites/{SiteId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/sites/{SiteId}', '80E63E02-391D-9F65-5B0E-449E90567C16' )

-- Page: Layout Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E6217A2B-B16F-4E84-BF67-795CA7F5F9AA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/sites/layouts/{LayoutId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/sites/layouts/{LayoutId}', '77C967B1-25BD-81BF-6A13-90D4A1DD48BC' )

-- Page: Pages
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1C763885-291F-44B7-A5E3-539584E07085')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/sites/{SiteId}/pages')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/sites/{SiteId}/pages', 'D7CF8CB0-74DB-59C0-26B1-9C920D387FB3' )

-- Page: Block Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5FBE9019-862A-41C6-ACDC-287D7934757D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/block-types')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/block-types', '5D7F595C-6FE1-9E30-1295-3CDAC4A66E61' )

-- Page: Block Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C694AD7C-46DD-47FE-B2AC-1CF158FA6504')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/block-types/{blockTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/block-types/{blockTypeId}', '15F9757C-0546-9B40-491D-B4BDD7C02FFA' )

-- Page: Pages
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'EC7A06CD-AAB5-4455-962E-B4043EA2440E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/pages')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/pages', '306ADCD2-7C11-68CC-4D90-41676CEF69CE' )

-- Page: Page Views
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E556D6C5-E2DB-4041-81AB-4F582008155C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/pages/{Page}/views')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/pages/{Page}/views', '1F2351E5-3D71-76DF-9585-4C3FBF73971A' )

-- Page: Content Channel Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '37E3D602-5D7D-4818-BCAA-C67EBB301E55')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/content-channel-type')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/content-channel-type', '24FD1E37-9866-8AA1-6571-D11193A785CB' )

-- Page: Content Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '91EAB2A2-4D44-4701-9ABE-37AE3E7A1B8F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/content-channel-type/{typeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/content-channel-type/{typeId}', '2DB339B2-7C40-984D-2098-8373410C71C2' )

-- Page: Content Channels
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '8ADCE4B2-8E95-4FA3-89C4-06A883E8145E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/content-channels')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/content-channels', 'EFAA0125-1F7F-A293-1EC2-CFA1EF5E1D34' )

-- Page: Content Channel Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4AE244F5-A5BF-48CF-B53B-785148EC367D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/content-channels/{contentChannelId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/content-channels/{contentChannelId}', '0265B86C-474F-9DE3-4BCC-CA8D57580D80' )

-- Page: Content Item Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'ABF26679-1051-4F4F-8A67-5958E5BF71F8')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/content-channels/items/{ContentItemId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/content-channels/items/{ContentItemId}', '3AEC8A19-17FA-1106-1EE4-5B5C358E9451' )

-- Page: File Manager
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/file-manager')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/file-manager', 'DAD838AD-1CB4-6348-630E-D857021CA08F' )

-- Page: File Editor
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '053C3F1D-8BF2-48B2-A8E6-55184F8A87F4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/file-manager/edit')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/file-manager/edit', '3F6ED309-4E9C-75CC-8C6F-FF27DBE041B3' )

-- Page: Themes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'BC2AFAEF-712C-4173-895E-81347F6B0B1C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/themes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/themes', '48B4B23D-05AC-831B-0173-0B34B13B6D54' )

-- Page: Theme Styler
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A74EEC7C-4F9E-48F5-A996-74A856981B4C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/themes/{EditTheme}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/themes/{EditTheme}', '132C10E9-4BDC-8141-7BF3-1CBB7875512A' )

-- Page: Short Links
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '8C0114FF-31CF-443E-9278-3F9E6087140C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/short-links')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/short-links', '87A329D4-6358-3BB2-0F05-51C814773DC7' )

-- Page: Link
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '47D5293B-A041-43A4-915A-FB1D156F265E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/short-links/{ShortLinkId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/short-links/{ShortLinkId}', 'BB4D63AA-7160-99A3-76DF-664B50651C48' )

-- Page: Lava Shortcodes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6CFF2C81-6303-4477-A7EC-156DDBF8BE64')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/lava-shortcodes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/lava-shortcodes', '75C9DFD5-16BD-949E-9DFD-F4A432665BF5' )

-- Page: Lava Shortcode Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1E30B9E7-0951-45FC-8637-8ADCBE782A30')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/lava-shortcodes/{LavaShortcodeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/lava-shortcodes/{LavaShortcodeId}', 'CC8D9181-1EE9-62BF-5B98-5010DA937A4F' )

-- Page: Control Gallery
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '706C0584-285F-4014-BA61-EC42C8F6F76B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/control-gallery')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/control-gallery', 'E4DAF94C-283C-2E61-6FCB-F85DCDE09D6D' )

-- Page: Font Awesome Settings
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'BB2AF2B3-6D06-48C6-9895-EDF2BA254533')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/font-awesome')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/font-awesome', '41BFFFDE-508B-7C7C-6F21-3748B154732F' )

-- Page: HTTP Modules
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '39F928A5-1374-4380-B807-EADF145F18A1')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/http-modules')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/http-modules', 'BAE167C1-9941-54AC-9598-1905CEC394F7' )

-- Page: Cache Manager
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4B8691C7-537F-4B6E-9ED1-E3BA3FA0051E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/cache-manager')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/cache-manager', 'DDA62C57-A4CA-4FB3-4724-44035EFA6F05' )

-- Page: Asset Manager
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D2B919E2-3725-438F-8A86-AC87F81A72EB')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/asset-manager')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/asset-manager', 'F3562D43-A30B-4B5F-878A-149050062C85' )

-- Page: Content Component Templates
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F1ED10C2-A17D-4310-9F86-76E11A4A7ED2')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/content-components')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/content-components', '41C3662A-7B37-0086-8394-C98BDF3A8F9B' )

-- Page: Content Channel Item Attribute Categories
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'BBDE39C3-01C9-4C9E-9506-C2205508BC77')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/content-channel-item-attribute-categories')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/content-channel-item-attribute-categories', '572C5B12-94BF-3779-A32B-F28B93163C23' )

-- Page: Mobile Applications
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '784259EC-46B7-4DE3-AC37-E8BFDB0B90A6')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/mobile-applications')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/mobile-applications', '077F1E30-4803-79E6-0658-6A906C1143C5' )

-- Page: Application Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A4B0BCBB-721D-439C-8566-24F604DD4A1C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/mobile-applications/{SiteId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/mobile-applications/{SiteId}', '4437150B-A6A9-8302-9F57-91D4D8343FA6' )

-- Page: Layouts
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5583A55D-7398-48E9-971F-6A1EF8158943')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/mobile-applications/{SiteId}/layouts/{LayoutId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/mobile-applications/{SiteId}/layouts/{LayoutId}', '1F58E8D5-8A2B-1610-5E51-3EFAF4369B61' )

-- Page: Pages
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '37E21200-DF91-4426-89CC-7D067237A037')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/mobile-applications/{SiteId}/layouts/{Page}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/mobile-applications/{SiteId}/layouts/{Page}', '5179EB97-935E-8DA1-5589-72D9181B2863' )

-- Page: Persisted Datasets
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '37C20B91-737B-42D1-907D-9868104DBA7B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/persisted-datasets')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/persisted-datasets', 'D4490F8F-51C6-1C4E-7A3B-D73CAE3F947E' )

-- Page: Persisted Dataset Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0ED8A471-B177-4AC3-933E-DFAB965E2E0D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/persisted-datasets/{PersistedDatasetId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/persisted-datasets/{PersistedDatasetId}', 'B54DB5A7-7ACD-6B0D-7769-381AB56C7C1D' )

-- Page: Content Channel Categories
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0F1B45B8-032D-4306-834D-670FA3933589')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/content-channel-categories')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/content-channel-categories', 'EF98F151-8241-07FA-1058-F5A376B88967' )

-- Page: Media Accounts
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '07CB7BB5-1465-4E75-8DD4-28FA6EA48222')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/media-accounts')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/media-accounts', 'B12F5786-88A4-6D48-01EA-6E32514C220E' )

-- Page: Media Account
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '52548B49-6D09-467E-BEA9-04DD6F51637D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/media-accounts/{MediaAccountId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/media-accounts/{MediaAccountId}', '806E12B6-5D32-7595-6168-DB7A064B97C7' )

-- Page: Media Folder
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '65DE6218-2850-4924-AA55-6F6FB572E9A3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/media-accounts/folders/{MediaFolderId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/media-accounts/folders/{MediaFolderId}', '1FDD5179-6319-4549-1E78-307B142B1BBA' )

-- Page: Media Element
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F1AB34EE-941F-41D6-9BA1-22348D09724C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/media-accounts/items/{MediaElementId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/media-accounts/items/{MediaElementId}', 'AD6A4CCB-26D6-1F52-720C-A951CCFC15D2' )

-- Page: Shared Links
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C206A96E-6926-4EB9-A30F-E5FCE559D180')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/shared-links')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/shared-links', '8CA62420-36F2-0410-9989-D4EDAB645057' )

-- Page: Shared Links Section Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '776704b9-17f8-467e-aabc-b4e19ff28960')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/cms/shared-links/{SectionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/cms/shared-links/{SectionId}', '44E5CE18-24C4-426C-5FC1-4C838311618A' )

-- Page: Security
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security', '25D1D562-7815-764B-919A-73EEBCFB96CF' )

-- Page: User Accounts
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '306BFEF8-596C-482A-8DEC-34A7B622E688')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/accounts')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/accounts', '81F63E0D-5A38-8844-8B9B-65DBE59479C1' )

-- Page: Security Roles
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'D9678FEF-C086-4232-972C-5DBAC14BFEE6')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/roles')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/roles', '39AD3EC3-2A12-9468-438D-670E6FFA13C5' )

-- Page: Security Roles Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '48AAD428-A9C9-4BBB-A80F-B85F28D31240')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/roles/{GroupId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/roles/{GroupId}', 'F0A4DDBD-152F-3359-96E2-B64D3D233124' )

-- Page: Group Member Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '45899E6A-7CEC-44EC-8DBA-BD8850262C04')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/roles/{GroupId}/members/{GroupMemberId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/roles/{GroupId}/members/{GroupMemberId}', '57B54199-7907-95B0-3378-79A3FB629B46' )

-- Page: REST Controllers
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0D51F443-1C0D-4C71-8BAE-E5F5A35E8B79')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/rest')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/rest', 'A77A7296-9053-4541-7D91-3681D3834F8F' )

-- Page: REST Controller Actions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7F5EF1AA-0E27-4AA1-A5E1-1CD6DDDCDDC5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/rest/{controller}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/rest/{controller}', '84CF5E9B-3A0D-6915-0D21-D6C9D6D19740' )

-- Page: Audit Information
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4D7F3953-0BD9-4B4B-83F9-5FCC6B2BBE30')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/audit')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/audit', 'DF5EC6A3-3B1F-0054-37E7-BFA4659D9494' )

-- Page: Entity Administration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F7F41856-F7EA-49A8-9D9B-917AC1964602')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/entity')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/entity', 'D4EF1A5C-A6E5-7298-92AB-0813DE473B64' )

-- Page: Authentication Services
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CE2170A9-2C8E-40B1-A42E-DFA73762D01D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/authentication')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/authentication', '53B37CC1-8FA5-41A0-578F-0781C87B8B50' )

-- Page: REST Keys
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '881AB1C2-4E00-4A73-80CC-9886B3717A20')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/rest-keys')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/rest-keys', 'B6CF8806-4FCE-8A9D-2FB7-51CF4F7C0BD7' )

-- Page: REST Key Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '594692AA-5647-4F9A-9488-AADB990FDE56')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/rest-keys/{restUserId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/rest-keys/{restUserId}', '9809E592-6322-0940-0A4C-0A94994D5389' )

-- Page: REST CORS Domains
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B03A8C4E-E394-44B0-B7CC-89B74C79C325')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/rest-cors')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/rest-cors', '4567A778-22D7-2AD5-7C23-CDEC4A2936E9' )

-- Page: Inspect Security
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B8CACE4E-1B10-46F4-B147-31F32B442915')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/inspect')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/inspect', 'BF22025B-0B7C-0A07-6042-CE42C95B2743' )

-- Page: Person Signal Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'EA6B3CF2-9DE2-4CF0-8EFA-01B76B51C329')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/signals')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/signals', '324AE3F4-627C-1DCF-9227-0B9D694AA4E6' )

-- Page: Person Signal Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '67AF60BC-D814-4DBC-BA64-D12128CCF52C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/signals/{SignalTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/signals/{SignalTypeId}', '9F0424AB-A53C-25FE-1AC8-FD455E743C00' )

-- Page: OpenID Connect Clients
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0A18B520-915E-429B-AC49-7A7F73B19BAA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/openid')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/openid', '8591E4F6-4D03-73F1-12EC-560CBE1A346E' )

-- Page: OpenID Connect Scopes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '06FA872A-18B0-431A-917E-6F7B2EA8ED95')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/openid/scopes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/openid/scopes', 'ED2FC52A-A1E4-14BF-4681-6FCDD7288E2D' )

-- Page: OpenID Connect Scope Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '55E70873-B882-4864-8B97-66F8ED3588C7')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/openid/scopes/{ScopeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/openid/scopes/{ScopeId}', '6B435980-31BB-0847-264E-F50D443B04AB' )

-- Page: OpenID Connect Client Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '41E6A833-1697-4463-9962-01DFD123D4C9')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/openid/clients/{AuthClientId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/security/openid/clients/{AuthClientId}', 'DF7004F7-A4DE-8231-6608-9E207B8F07FE' )

-- Page: Communications
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '199DC522-F4D6-4D82-AF44-3C16EE9D2CDA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications', 'F06A5D9F-89F9-8783-77DA-5160BEA55ABF' )

-- Page: Communication Templates
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '39F75137-90D2-4E6F-8613-F19344767594')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/templates')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/templates', 'FDAE346F-43F3-3C48-0D91-3A1B4D3097F4' )

-- Page: Template Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '924036BE-6B89-4C60-96ED-0A9AF1201CC4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/templates/{TemplateId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/templates/{TemplateId}', 'DBFBEC14-877D-77E7-6F69-28A2DE4249B9' )

-- Page: System Communications
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '14D8F894-F70F-44F7-9F0C-2545F87256FF')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/system')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/system', '1572D656-7F9B-951F-3AE6-37402BE46B5A' )

-- Page: System Communication Details
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2FE2D59E-2737-49C8-AF1B-4366A8371A8E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/system/{CommunicationId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/system/{CommunicationId}', '689F22E5-2A81-0882-A339-CD91EB4C4697' )

-- Page: System Emails (Legacy)
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '89B7A631-EA6F-4DA3-9380-04EE67B63E9E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/email-legacy')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/email-legacy', '4E800BB8-5769-65CB-65E0-C545E4C06E1D' )

-- Page: System Email Details (Legacy)
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '588C72A8-7DEC-405F-BA4A-FE64F87CB817')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/email-legacy/{emailId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/email-legacy/{emailId}', '3928CD60-6B98-A597-492C-554A86083B8F' )

-- Page: Communication Mediums
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6FF35C53-F89F-4601-8543-2E2328C623F8')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/mediums')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/mediums', '375E66FE-5BF0-3D49-7DB5-B06A997B5C0E' )

-- Page: Communication Transports
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '29CC8A0B-6476-4200-8B93-DC9BA8767D59')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/transports')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/transports', 'C49771BC-46EC-36AB-40D6-F72DCFD43F44' )

-- Page: SMS Phone Numbers
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '3F1EA6E5-6C61-444A-A80E-5B66F96F521B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/sms-numbers')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/sms-numbers', '44B077C9-64AC-5D18-58DD-16DA7890A71C' )

-- Page: Safe Sender Domains
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B90576B0-110E-4DC0-8EB8-4668C5238508')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/safe-sender')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/safe-sender', 'F2CE08E6-0061-4D2E-714F-C54D0F114E71' )

-- Page: Send Photo Requests
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B64D0429-488C-430E-8C32-5C7F32589F73')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/send-photo-requests')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/send-photo-requests', 'FADAF01F-3F91-3878-1ADA-B53140110B62' )

-- Page: System Communication Categories
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B55323CD-F494-43E7-97BF-4E13DAB58E0B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/system-communication-categories')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/system-communication-categories', 'C01F47F4-4746-3434-6B6D-707723A00BCE' )

-- Page: Communication Queue
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '285ED8C0-0471-4503-95DE-A8E3F179206C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/queue')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/queue', '8B0819F0-4FF4-12E2-9A0D-4FC6F4E9972F' )

-- Page: Communication List Categories
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '307570FD-9472-48D5-A67F-80B2056C5308')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/communication-list-categories')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/communication-list-categories', 'B2FD21FE-2019-4127-486E-F6D968F7244F' )

-- Page: Communication Lists
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '002C9991-523A-478C-B19B-E9DF2B977481')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/communication-lists')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/communication-lists', 'D7681B38-7406-3DE6-012B-7D307F97A29E' )

-- Page: Communication List Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '60216406-5BD6-4253-B891-262717C07A00')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/communication-lists/{GroupId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/communication-lists/{GroupId}', 'D8430322-4C8C-0C0F-3B78-B3B4C7F111F2' )

-- Page: Group Member Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FB3FCA8D-2011-42B5-A9F4-2657C4F856AC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/communication-lists/{GroupId}/member/{GroupMemberId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/communication-lists/{GroupId}/member/{GroupMemberId}', '8A541DF7-75C6-403F-A65A-445EB5767650' )

-- Page: Communication Template Categories
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4D6DEAB3-46A0-4B27-B67B-71383EFE1171')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/communication-template-categories')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/communication-template-categories', '6EB7194A-6085-06D6-9CEE-0616AB8430A2' )

-- Page: SMS Pipeline
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2277986A-F53D-4E46-B6EC-6BAD1111DA39')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/sms-pipeline')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/sms-pipeline', '9C498150-4295-84F3-7DB6-9C589EF045DD' )

-- Page: SMS Pipeline Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FCE39659-4D86-48D7-9C48-D837D3588C42')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/sms-pipeline/{SmsPipelineId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/sms-pipeline/{SmsPipelineId}', '569DD0CC-71FA-754C-8B57-425FCC122970' )

-- Page: Nameless People
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '62F18233-0395-4BEA-ADC7-BC08271EDAF1')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/nameless-people')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/nameless-people', 'BCE19B57-6297-93DB-2868-B05AAF89254B' )

-- Page: Communications Settings
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '436D1D6F-2EB9-43F3-8EEE-359DC0B09360')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/communications/settings')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/communications/settings', '11ACE75E-5437-97E0-7E87-0C10FCD982F2' )

-- Page: Check-in
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '66C5DD58-094C-4FF9-9AFB-44801FCFCC2D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin', 'E74E637E-161C-7EF9-3E54-2FB903B3A206' )

-- Page: Check-in Configuration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C646A95A-D12D-4A67-9BE6-C9695C0267ED')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/configuration')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/configuration', '3F9DFC83-20C3-79D1-6C0E-FD92F3DB3B36' )

-- Page: Named Locations
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '96501070-BB46-4432-AA3C-A8C496691629')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/named-locations')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/named-locations', '48B2967B-2506-1FB4-4D78-39A04558081C' )

-- Page: Schedules
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'AFFFB245-A0EB-4002-B736-A2D52DD692CF')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/schedules')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/schedules', '196D8FF9-7797-3159-9160-D53A091B3DE7' )

-- Page: Devices
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5A06C807-251C-4155-BBE7-AAC73D0745E3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/devices')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/devices', '3CB60F52-570A-47A6-1BAC-CE862D1A8818' )

-- Page: Device Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7D5311B3-F526-4E22-8153-EA1799467886')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/devices/{DeviceId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/devices/{DeviceId}', '7AB3B7B8-9D30-0DFB-4384-96B3732E379B' )

-- Page: Check-in Labels
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7C093A63-F2AC-4FE3-A826-8BF06D204EA2')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/labels')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/labels', 'B43C6BE4-5FFD-127A-1FFE-8E30010703A9' )

-- Page: Check-in Label
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B565FDF8-959F-4AC8-ACDF-3B1B5CFE79F5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/labels/{BinaryFileId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/labels/{BinaryFileId}', '8AEF349D-2BF3-2100-4F4A-DD2A4D096F6E' )

-- Page: Edit Label
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '15D3766A-6026-4F29-B5C6-5944204642F3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/labels/{BinaryFileId}/edit')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/labels/{BinaryFileId}/edit', 'EE688EAE-9006-6BC3-048E-F855A7CF73FE' )

-- Page: Ability Levels
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '9DD78A23-BE4B-474E-BCBC-F06AAABB67FA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/ability-levels')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/ability-levels', '5930BF23-85CC-20E9-5249-8EC32B3D2121' )

-- Page: Label Merge Fields
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1DED4B72-1784-4781-A836-83D705B153FC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/label-merge-fields')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/label-merge-fields', '663766D7-0207-051D-6B2A-D71FB59134D0' )

-- Page: Search Type
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '3E0327B1-EE0E-41DC-87DB-C4C14922A7CA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/checkin/search-types')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/checkin/search-types', '4DD83241-8916-97DC-687C-2BE4517C373E' )

-- Page: Power Tools
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7F1F4130-CB98-473B-9DE1-7A886D2283ED')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/power-tools/')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/power-tools/', '59174143-5F96-746F-8A8C-E3325F6C5ED3' )

-- Page: SQL Command
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '03C49950-9C4C-4668-9C65-9A0DF43D1B33')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/power-tools/sql')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/power-tools/sql', '926F6D75-1753-0ECD-868D-C7DE06C400DB' )

-- Page: External Applications
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5A676DCC-37F0-4624-8CCD-408A5A471D8A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/power-tools/apps')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/power-tools/apps', '2326623E-72C6-4BF2-4805-49AC42793590' )

-- Page: Sample Data
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '844ABF2A-D085-4370-945B-86C89580C6D5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/power-tools/sample-data')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/power-tools/sample-data', '38F140C2-91DB-6FD9-5348-EC816AC25887' )

-- Page: Model Map
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '67DBC902-BCD5-449E-8A1F-888A3CF9875E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/power-tools/model-map')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/power-tools/model-map', '8039B732-3A9B-0DB8-37C8-3AE060B91E2D' )

-- Page: Power BI Registration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'BB65848A-3EBD-D181-4150-956A39FFE57E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/power-tools/power-bi')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/power-tools/power-bi', 'E34DDA6D-055C-6ED4-3025-9EDC093A927B' )

-- Page: Workflow Import/Export
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B6096C72-FE05-472F-B668-B31253DD5E25')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/power-tools/workflow-import')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/power-tools/workflow-import', '78A62504-6EB4-45BB-8E41-972C44350337' )

-- Page: System Settings
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C831428A-6ACD-4D49-9B2D-046D399E3123')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system', '2F4E4FEC-245C-8987-71FC-880D8D5338EE' )

-- Page: Location Services
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1FD5698F-7279-463F-9637-9A80DB86BB86')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/location-services')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/location-services', '4BFB59AE-3DCD-2230-6103-8F114A4B0005' )

-- Page: Entity Attributes
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '23507C90-3F78-40D4-B847-6FE8941FCD32')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/entity-attributes')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/entity-attributes', 'A64F3C4C-5113-3C04-9338-1C673F47887E' )

-- Page: Search Services
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1719F597-5BA9-458D-9362-9C3E558E5C82')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/search-services')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/search-services', '212BE867-52CE-030B-45FF-D01BB1221696' )

-- Page: Jobs Administration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C58ADA1A-6322-4998-8FED-C3565DE87EFA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/jobs')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/jobs', '9378251F-04D8-3EAC-2094-EBFF73400F9B' )

-- Page: Scheduled Job Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E18AC09D-45CD-49CF-8874-157B32556B7D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/jobs/{ServiceJobId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/jobs/{ServiceJobId}', 'BB71FA17-3D00-33B2-5306-657165F26BCD' )

-- Page: Scheduled Job History
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B388793F-077C-4E5C-95CA-C331B00DF986')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/jobs/{ServiceJobId}/history')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/jobs/{ServiceJobId}/history', 'A3A37F4A-5673-611A-631C-5053BB1F99EF' )

-- Page: Data Filters
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5537F375-B652-4603-8E04-119C74414CD7')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/data-filters')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/data-filters', 'ABB326B8-3C5F-674B-6ACF-AB86C9889E0D' )

-- Page: Data Transformations
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '9C569E6B-F745-40E4-B91B-A518CD6C2922')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/data-transformations')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/data-transformations', '26D04AC1-3A12-1505-5723-67338CC736A4' )

-- Page: Data Selects
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '227FDFB9-8C29-4B34-ABE5-E0579A3A6018')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/data-selects')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/data-selects', '9C356689-A49E-6532-7C10-C3F618366218' )

-- Page: File Storage Providers
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FEA8D6FC-B26F-48D5-BE69-6BCEF7CDC4E5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/file-storage')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/file-storage', '985DA231-9741-36CF-5EE3-663A3611974F' )

-- Page: Exception List
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '21DA6141-0A03-4F00-B0A8-3B110FBE2438')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/exceptions')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/exceptions', '78E30A12-237F-06AC-6C3B-2D2D0BFB0966' )

-- Page: Exception Occurrences
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F95539C3-03C8-422B-B586-EF4C2FE91CF4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/exceptions/{ExceptionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/exceptions/{ExceptionId}', '07E24380-378A-891C-65D1-BD4E805C7D30' )

-- Page: Exception Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F1F58172-E03E-4299-910A-ED34F857DAFB')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/exceptions/detail/{ExceptionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/exceptions/detail/{ExceptionId}', '69840E6D-9554-80AB-1A79-E45EED419CD0' )

-- Page: Protect My Ministry
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E7F4B733-60FF-4FA3-AB17-0832E123F6F2')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/protect-my-ministry')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/protect-my-ministry', '2BB14E39-6AEE-4379-8B92-ACB5EF3F700B' )

-- Page: Checkr
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6076DB93-7C1A-44F3-BE40-5E517B59ABD0')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/checkr')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/checkr', 'BE94BC3F-873D-1F0D-3828-03CF38163261' )

-- Page: Financial Gateways
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F65AA215-8B46-4E34-B709-FA956BF62C30')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/gateways')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/gateways', 'BC5F11DD-08A0-9F2A-0AC5-5284052D558C' )

-- Page: Gateway Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '24DE6092-CE91-468C-8E49-94DB3875B9B7')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/gateways/{GatewayId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/gateways/{GatewayId}', '01C4E83B-2033-908C-5259-5D5A78168A02' )

-- Page: Background Check Providers
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '53F1B7D9-806A-4541-93BC-4CCF5DFF90B3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/background-check-providers')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/background-check-providers', 'B422B5B9-7A72-54E7-0B4D-A1F66190739F' )

-- Page: Signature Document Providers
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FAA6A2F2-4CFD-4B97-A0C2-8F4F9CE841F3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/signature-documents')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/signature-documents', 'CD178F4B-566C-5CF7-9756-C27406173FF2' )

-- Page: Category Manager
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '95ACFF8C-B9EE-41C6-BAC0-D117D6E1FADC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/category-manager')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/category-manager', 'AAAEC8E5-26A2-2797-8D21-EF2AA27E2867' )

-- Page: System Configuration
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '7BFD28F2-6B90-4191-B7C1-2CDBFA23C3FA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/configuration')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/configuration', '00F1E715-4125-3FEB-3AD8-35661F1719E7' )

-- Page: Application Groups
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'BA078BB8-7205-46F4-9530-B2FB9EAD3E57')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/application-groups')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/application-groups', '4963D01B-925A-31C5-1473-FD00815D1661' )

-- Page: Application Group Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'E9737442-E6A9-47D5-A842-11C1AE1CF43F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/application-groups/{GroupId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/application-groups/{GroupId}', '33CFC69F-7BF3-54FA-95AC-D3BD69AC0514' )

-- Page: Group Member Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C920AA8F-A8CA-4984-95EC-58B7309E670E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/application-groups/members/{GroupMemberId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/application-groups/members/{GroupMemberId}', 'CD12C1B4-A4FD-781D-47FF-9206FDB50149' )

-- Page: Merge Template Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '42717D07-3744-4187-89EC-F01EDD0FF5AD')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/merge-templates')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/merge-templates', '183C78A9-9699-7FB8-5280-800AE25A6EEC' )

-- Page: Note Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B0E5876F-E29E-477B-8874-482DEDD3A6C5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/note-types')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/note-types', 'EA6D38A7-5297-201B-7536-CDC2833F8F6E' )

-- Page: Note Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '421C838D-F6BA-46C5-8DBF-36CA0CC17B77')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/note-types/{NoteTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/note-types/{NoteTypeId}', '5BE865C3-1B67-60AA-8886-9EC2720B0350' )

-- Page: Following Events
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '283D2756-7686-4ED5-AE44-4B8811E3956F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/following-events')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/following-events', '2DB23BAC-4ECA-0DEF-667E-8CFC75035CEE' )

-- Page: Following Event
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C68D4CA0-9B2D-4B85-AC5B-361126E787CC')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/following-events/{eventId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/following-events/{eventId}', 'A8A6F3F9-1A98-0A88-6D70-2B2C557030AE' )

-- Page: Following Suggestions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '3FD46CEF-113E-4A19-B9B7-D9A1BCA9C043')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/following-suggestions')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/following-suggestions', '455A8BED-1591-23D9-34A2-BEC9E19F92EE' )

-- Page: Following Suggestion
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '9593F41C-23A2-4F65-BBD4-634A06380E2E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/following-suggestions/{eventId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/following-suggestions/{eventId}', '92477AAF-7392-04E0-A5EA-A305769329BE' )

-- Page: Universal Search Index Components
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FF26DBAC-7E4B-4C55-8EE0-2277187D06F3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/universal-search')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/universal-search', 'C0205D6C-4402-4669-0AF4-3790027E0ABF' )

-- Page: Calendar Dimension Settings
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2660D554-D161-44A1-9763-A73C60559B50')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/calendar-dimension')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/calendar-dimension', 'FDE96C3A-326E-7BE2-8698-FD6E0ADFA50A' )

-- Page: Phone Systems
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A33C221B-F361-437A-BDC1-E46BB3B532EF')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/phone-systems')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/phone-systems', '008C70FB-6300-30AE-0640-BAB6FFA92A20' )

-- Page: Note Watches
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '74FB3214-8F11-4D40-A0E9-1AEA377E9217')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/note-watches')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/note-watches', 'F33F450B-068B-222A-872F-6790B88939BB' )

-- Page: Note Watch Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6717F2F8-85C8-404A-B4CD-683379A2A487')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/note-watches/{NoteWatchId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/note-watches/{NoteWatchId}', 'C7C7CEC4-7223-28F8-959A-F716467D6F34' )

-- Page: Spark Data Settings
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0591E498-0AD6-45A5-B8CA-9BCA5C771F03')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/spark-data')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/spark-data', 'D34B7F39-5085-6178-1218-C50147ED28D3' )

-- Page: Asset Storage Providers
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '1F5D5991-C586-45FC-A5AC-B7CD4D533990')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/asset-storage')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/asset-storage', 'C0E20419-6AEE-6606-898E-E1FE62CA7ABD' )

-- Page: Asset Storage Provider Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '299751A1-EBE2-467C-8271-44BA13278331')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/asset-storage/{assetStorageProviderId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/asset-storage/{assetStorageProviderId}', '592BC24C-65D9-0523-90FD-CF4144B89698' )

-- Page: Assessment Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CC59F2B4-16B4-47BE-B8A0-E417EABA068F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/assessments')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/assessments', 'EB40CF90-6462-A778-2CE3-B3A04CD82DE0' )

-- Page: Assessment Type Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F3C96663-1079-4F20-BABA-3F3203AFCFF3')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/assessments/{AssessmentTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/assessments/{AssessmentTypeId}', '558E7F39-026D-566A-14A1-592F65667652' )

-- Page: Rock Logs
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '82EC7718-6549-4531-A0AB-7957919AE71C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/rock-logs')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/rock-logs', 'D009B039-03C5-6E77-60CC-3CC63F6F45B9' )

-- Page: Message Bus
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0FF43CC8-1C29-4882-B2F6-7B6F4C25FE41')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/message-bus')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/message-bus', '56C61FD9-3C85-2A3A-3E9E-BAB2C57C756C' )

-- Page: Transport
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '10E34A5D-D967-457D-9DF1-A1D33DA9D100')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/message-bus/transports')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/message-bus/transports', '26D32AEB-778C-1A5C-93F9-6AEB84159CAB' )

-- Page: Queue
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '45E865C0-CD2D-43CD-AA8A-BF5DBF537587')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/message-bus/queue/{QueueKey}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/message-bus/queue/{QueueKey}', 'CCF9711E-1399-5BC1-8FBC-72E5D674890A' )

-- Page: Web Farm
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '249BE98D-9DDE-4B19-9D97-9C76D9EA3056')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/web-farm')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/web-farm', 'D7199E8A-1CB3-28AE-86BF-3814DF0C62EF' )

-- Page: Web Farm Node
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '63698D5C-7C73-44A4-A27D-A7EB777EB2A2')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/system/web-farm/{WebFarmNodeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/system/web-farm/{WebFarmNodeId}', '56BF1F32-70F3-4A38-7B0D-54A0196C9628' )

-- Page: Installed Plugins
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5B6DBC42-8B03-4D15-8D92-AAFA28FD8616')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/plugins')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/plugins', '84A616D7-2281-8869-9CB1-91876CDE0A05' )

-- Page: My Settings
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'CF54E680-2E02-4F16-B54B-A2F2D29CD932')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my', '70A3DCBA-7DCD-0C38-070A-D68B226D05BA' )

-- Page: Change Password
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '4508223C-2989-4592-B764-B3F372B6051B')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/password')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/password', '23A33B7E-0C83-4984-17D5-48CE431572A4' )

-- Page: Communication Templates
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'EA611245-7A5E-4995-A3C6-EB97C6FD7C8D')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/communication-templates')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/communication-templates', '0ECBCE83-4AE3-364F-913D-6FCDDC0B703D' )

-- Page: Template Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '753D62FD-A06F-43A3-B9D2-0A728FF2809A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/communication-templates/{TemplateId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/communication-templates/{TemplateId}', '9B36A70B-A6CD-A3DA-20CE-2E3A305440EF' )

-- Page: Merge Templates
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '23F81A62-617A-498B-AAAC-D748F721176A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/merge-templates')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/merge-templates', 'FAFCC1C1-5A3C-7A01-79F8-77A187040C10' )

-- Page: Merge Template Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F29C7AF7-6436-4C4B-BD17-330A487A4BF4')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/merge-templates/{MergeTemplateId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/merge-templates/{MergeTemplateId}', 'CC9A9337-74EA-6A04-A691-31D17CA1612F' )

-- Page: Following Settings
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '18B8EB25-B9F2-48C6-B047-51A512A8F1C9')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/following-settings')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/following-settings', '0643C12E-1AFD-43DF-84CE-1988012892DA' )

-- Page: Following
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A6AE67F7-0B46-4F9A-9C96-054E1E82F784')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/following')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/following', '765B5EF8-5606-A203-3C6D-25339FC05D6D' )

-- Page: Personal Links
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'ED1B85B7-C76A-4624-B644-ABC1CD4BDEAE')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/personal-links')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/personal-links', '23DD416D-45EF-76C2-9246-D54651AD1A00' )

-- Page: Section Detail
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B0866B52-290B-4623-A123-2AD913BB905C')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/personal-links/{SectionId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/personal-links/{SectionId}', '89E7A7E1-060E-4601-50F2-57B7C2DD829F' )

-- Page: My Dashboard
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'AE1818D8-581C-4599-97B9-509EA450376A')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/dashboard')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/dashboard', '1ACA4E4B-5CBF-232C-45EC-5ECFAF92A14A' )

-- Page: Following Suggestions
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '50BAAD66-46AB-4968-AFD6-254C536ACEC8')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'my/following')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'my/following', 'A6E16654-579A-572C-9601-3C0B69FF511E' )


-- Page: API Docs
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'C132F1D5-9F43-4AEB-9172-CD45138B4CEA')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/power-tools/api-docs')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'admin/power-tools/api-docs', '1D908AEC-7C61-4C7D-9BE3-5ADC19B249C4' )



-- READD Page: Group Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2109228C-D828-4B58-9310-8D93D10B846E')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'steps/record/{StepId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'steps/record/{StepId}', 'C72F337F-4320-4CED-B5FF-20A443268123' )

-- READD Page: Step Type
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '8E78F9DC-657D-41BF-BE0F-56916B6BF92F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'steps/type/{StepTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'steps/type/{StepTypeId}', '74DF0B98-B980-4EF7-B879-7A028535C3FA' )

-- READD Page: Step Program
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '6E46BC35-1FCB-4619-84F0-BB6926D2DDD5')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'steps/program/{ProgramId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'steps/program/{ProgramId}', '0B796F9D-1294-40E7-B264-D460D62B4F2F' )

-- READD Page: Group Types
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'GroupType/{GroupTypeId}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'GroupType/{GroupTypeId}', '796D5B39-FF89-49E1-878C-D338FDD4D82C' )

-- READD Page: Steps
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'F5E8A369-4856-42E5-B187-276DFCEB1F3F')
IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'steps')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, 'steps', '4E4280B8-0A10-401A-9D69-687CA66A7B76' )