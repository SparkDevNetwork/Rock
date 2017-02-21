-- NOTE: if you have SSMS 2012 or newer, you might what to turn on Options > Query Results > Sql Server > Results to Grid > 'Retain CR/LF on copy or save'
-- This will allow you to Copy and Paste directly from the Grid results

declare
-- set to 1 to include any block attribute values updated in the last 60 minutes, even if their BlockType Attribute is IsSystem=1
@forceIncludeRecentlyUpdatedBlockAttributeValues bit = 0

set nocount on
declare
@crlf varchar(2) = char(13) + char(10)

begin

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    DROP TABLE #codeTable

IF OBJECT_ID('tempdb..#blocksTemp') IS NOT NULL
    DROP TABLE #blocksTemp

IF OBJECT_ID('tempdb..#knownGuidsToIgnore') IS NOT NULL
    DROP TABLE #knownGuidsToIgnore

create table #knownGuidsToIgnore(
    [Guid] UniqueIdentifier, 
    CONSTRAINT [pk_knownGuidsToIgnore] PRIMARY KEY CLUSTERED  ( [Guid]) 
);

-- External Site Layouts
insert into #knownGuidsToIgnore values 
('9D21BE8A-FD3A-4B91-8594-53C1528468A1'), --Dialog
('8378B293-4642-4F1B-90A8-502187984FCB'), --Three Column
('22F952AB-DE33-4607-9867-75AC5E97A928'), --Error
('ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED') --Right Sidebar

-- Internal Site Layouts
insert into #knownGuidsToIgnore values 
('9433041E-FD96-4DD8-A60F-A641C48BED7D'), --Error
('1A8A455E-619F-437E-A7BD-50C09B5B3576') --Homepage

-- Internal Site Pages
insert into #knownGuidsToIgnore values 
('0C4B3F4D-53FD-4A65-8C93-3868CE4DA137'), --Intranet
('7F2581A1-941E-4D51-8A9D-5BE9B881B003'), --Office Info
('895F58FB-C1C4-4399-A4D8-A9A10225EA09'), --Employee Resources
('FBC16153-897B-457C-A35F-28FDFDC466B6') --Shared Documents

-- External Site Pages
insert into #knownGuidsToIgnore values 
('1615E090-1889-42FF-AB18-5F7BE9F24498'), --Give Now
('A974A965-414B-47A6-9CC1-D3A175DA965B'), --Pledge
('EBAA5140-4B8F-44B8-B1E8-C73B654E4B22'), --Support Pages
('D025E14C-F385-43FB-A735-ABD24ADC1480'), --Login
('BBAD3127-8629-400C-BD11-9A554AA107C7'), --Account Registration
('5EB07686-D032-41A5-95C0-FD36F939FA52'), --Ad Details
('21C1D31E-BB66-4050-A2AC-3A057B484596'), --New Here?
('111FEDB6-7149-4C6B-93BE-2ECAE806D4F3'), --Resources
('288DBEC5-8A43-4133-9313-AA2FE81FBA86'), --Account Confirmation
('CAECAA2E-24B3-460D-966C-A96284A5D1B0'), --Forgot Password
('FAD4F98A-2CBC-4C3E-B597-6C63E2177E7D'), --Change Password
('8BB303AF-743C-49DC-A7FF-CC1236B4B1D9'), --Give
('7625A63E-6650-4886-B605-53C2234FA5E1'), --Connect
('A7EA053F-BD8B-42D1-8575-9C37C8298F46'), --Children
('DB9F7118-6E41-4B36-AA0E-E25942207E4F'), --Students
('FAE003BC-EC59-4AFD-8D08-AB092899CB73'), --Adults
('7019736A-8F30-4402-8A48-CE5308218618'), --Prayer
('5A8FBB92-85E5-4FD3-AF88-F3897C6CBC35'), --Missions
('2AA719FD-5B9F-4A9A-A8BF-C135EEA02BC8'), --Serve
('EA515FD1-7D71-4E24-A09D-EA9EC34BEC71'), --Small Groups
('59C38C86-AAB2-4864-AE05-04508BD783F0'), --Prayer Team
('621E03C5-6586-450B-A340-CC7D9727B69A'), --View My Giving Page
('FFFDCE23-7B67-4B0D-8DA0-E44D883708CC'), --Manage Giving Profiles
('2072F4BC-53B4-4481-BC15-38F14425C6C9'), --Edit Giving Profile
('C0854F84-2E8B-479C-A3FB-6B47BE89B795') --My Account

-- External site blocks
insert into #knownGuidsToIgnore values 
('A2AEC655-DC6A-46B6-9236-209C8610AF29'), -- Block:Ad List, Page: External Homepage, Site: External Website
('18F2C1E7-48E2-4A98-B184-8C15D6F3D433'), -- Block:Ad Rotator, Page: External Homepage, Site: External Website
('932F769B-2575-4F31-8993-C5BB4F6DBA6F'), -- Block:Add Prayer Request, Page: Prayer, Site: External Website
('6E5950E9-E854-43CA-A10F-302FD4EBB834'), -- Block:Change Password, Page: Change Password, Site: External Website
('EA7C6D4A-143F-47FF-B0A8-D1329FD8AF95'), -- Block:Confirm Account, Page: Account Confirmation, Site: External Website
('0AC35C5D-C395-40B0-9293-88153DF1D1B3'), -- Block:Content, Page: Give, Site: External Website
('186D89D6-8366-401D-82CD-4367355AA2D6'), -- Block:Content, Page: Prayer, Site: External Website
('7917DFDE-D2E2-4506-AD7C-B00E44FEC3DD'), -- Block:Content, Page: Connect, Site: External Website
('0E1F83FA-7F45-4A45-BE8C-AF38F620FBCF'), -- Block:Content, Page: Children, Site: External Website
('BD3401DC-5CF7-4844-84C9-3335F5F9DCDD'), -- Block:Content, Page: Students, Site: External Website
('2A61CF99-A58B-4D93-8989-89176504F465'), -- Block:Content, Page: Adults, Site: External Website
('D43D2BD0-4D68-4C75-9126-40DEC929CF5E'), -- Block:Content, Page: Missions, Site: External Website
('96A49053-75EF-4E2F-87DA-2458D7DA2859'), -- Block:Content, Page: Serve, Site: External Website
('9432D804-7DE1-4B09-86F8-0FBD1B944DAC'), -- Block:Content, Page: Small Groups, Site: External Website
('43D4DB36-A796-4870-B276-A4949174869F'), -- Block:Footer Address, Layout: LeftSidebar, Site: External Website
('CFECD435-FF91-4DF6-9305-ADD03A4245E0'), -- Block:Footer Address, Layout: FullWidth, Site: External Website
('0C8E7905-273C-4CDD-B2C2-F76167296FB0'), -- Block:Footer Text, Layout: LeftSidebar, Site: External Website
('6A56D10F-4DB3-4474-8B0C-BE765DA56567'), -- Block:Footer Text, Layout: FullWidth, Site: External Website
('B1CC5208-C9C5-43D5-B503-22CD5414755B'), -- Block:Footer Text, Layout: Homepage, Site: External Website
('0A53F510-E12F-42B4-BF68-B192EB92354E'), -- Block:Forgot UserName, Page: Forgot Password, Site: External Website
('CABFB331-8878-46BF-AAE0-65E28560AEBB'), -- Block:Header Text, Layout: Homepage, Site: External Website
('CF069ED5-FE65-4F0F-93E3-86E5BAB28396'), -- Block:Header Text, Layout: FullWidth, Site: External Website
('18D5CAD5-AC7B-46C3-B981-0B70D11EF20C'), -- Block:Header Text, Layout: LeftSidebar, Site: External Website
('A8E221F0-DE4E-4B0F-B660-BC7AC2298EF8'), -- Block:Login, Page: Login, Site: External Website
('5A5C6063-EA0D-4EDD-A394-4B1B772F2041'), -- Block:Login Status, Layout: Homepage, Site: External Website
('5CE3D668-85BF-4B3F-91BE-AB4BF8BA24B9'), -- Block:Login Status, Layout: FullWidth, Site: External Website
('AD5D5155-D5AC-4445-A2C1-C4E8DC6CF23E'), -- Block:Login Status, Layout: LeftSidebar, Site: External Website
('7B8BD953-EAD9-4C32-BD4B-06C39DF85125'), -- Block:Marketing Campaign Ad Detail, Page: Ad Details, Site: External Website
('9D2CEF00-3A7E-461A-A245-98F3A6637BE4'), -- Block:Navigation, Layout: Homepage, Site: External Website
('CB1C0E03-E590-451D-A6DA-7EEEFF91A531'), -- Block:New Account, Page: Account Registration, Site: External Website
('DDFEAD2B-0278-44E8-9552-4D6E9F6D5E45'), -- Block:Org Info, Layout: Homepage, Site: External Website
('C8B7DE9D-90FA-45A1-B6CE-E9949CF7EE47'), -- Block:Page Content, Page: New Here?, Site: External Website
('EA592AE0-0E56-472B-9FA4-F32A4AC3A790'), -- Block:Page Content, Page: Resources, Site: External Website
('453D10D8-0C30-4721-8446-E4636969A524'), -- Block:Page Menu, Layout: LeftSidebar, Site: External Website
('76ABCFB9-3D1D-4A2B-9012-6ECFFCB00DE9'), -- Block:Page Nav, Layout: FullWidth, Site: External Website
('2B56A2F3-8CB9-433E-84FF-DEA95F592CBD'), -- Block:Separator, Page: Prayer, Site: External Website
('C716BDE0-F30B-4233-8587-13340B78F789'), -- Block:Sub Nav, Page: Missions, Site: External Website
('33938CB4-4CCC-45B6-89D8-A5A14EAC04EE'), -- Block:Sub Nav, Page: Serve, Site: External Website
('83A981BE-D0D2-4031-8230-6E97E578BCFF'), -- Block:Sub Nav, Page: Small Groups, Site: External Website
('5E2F9D04-BE2C-4CE7-9C96-EBC5D8FE9513'), -- Block:Sub Nav, Page: Children, Site: External Website
('F437F79A-9452-4932-80AA-CC16E7B84C87'), -- Block:Sub Nav, Page: Students, Site: External Website
('92970C55-6978-4BD7-832A-983295052303'), -- Block:Sub Nav, Page: Prayer, Site: External Website
('DB36C97A-D462-49B7-8735-66EC2B1D3092'), -- Block:Sub Nav, Page: Adults, Site: External Website
('ABB78DC8-3E91-4778-BF4F-7A161F977BDA'), -- Block:Sub Nav, Page: Prayer, Site: External Website
('CCE62545-6B2A-418C-BAAC-07521A6F4F16'), -- Block:Sub Nav, Page: Give, Site: External Website
('C6007437-A565-4144-9DB3-DD590D62D5E2'), -- Block:Sub Nav, Page: Pledge, Site: External Website
('E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD'), -- Block to Page: Give, Site: External Website
('8A5E5144-3054-4FC9-AD8A-B0F4813C94E4'), -- Block to Page: View My Giving, Site: External Website
('0B62A727-1AEB-4134-AFAE-1EBB73A6B098'), -- Block to Page: View My Giving, Site: External Website
('B4FADF76-ED25-4641-A041-4AE2D46FD689'), -- Block to Page: View My Giving, Site: External Website
('95C60041-E6C7-4011-8841-6243E2C0208C'), -- Block to Page: Give Now, Site: External Website
('01AA807E-DD75-4C1B-96E0-760D1AD06015'), -- Block to Page: Manage Giving Profiles, Site: External Website
('88415BD1-A458-4111-BDC9-3F66DC782E71'), -- Block to Page: Manage Giving Profiles, Site: External Website
('0D91DD2F-519C-4A4A-AB03-0933FC12BE7E'), -- Block to Page: Manage Giving Profiles, Site: External Website
('60304123-B27F-4A7E-825B-5B285E6CCF13'), -- Block to Page: Edit Giving Profile, Site: External Website
('75F15397-3B82-4879-B069-DABD3619FAA3'), -- Block to Page: Edit Giving Profile, Site: External Website
('4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9'), -- Block to Page: Pledge, Site: External Website
('83D0018A-CAE4-469F-84A7-A113CD2EC033'), -- Block to Page: Pledge, Site: External Website
('23057924-104C-43B4-86CE-297B8B236CD2'), -- Block:SubNav, Page: Connect, Site: External Website
('095027CB-9114-4CD5-ABE8-1E8882422DCF'), -- Block to Page: External Homepage, Site: External Website
('2E0FFD29-B4AF-4A5E-B528-667168762ABC') -- Block to Page: External Homepage, Site: External Website

-- Internal site PersonDetail blocks
insert into #knownGuidsToIgnore values 
('7FB0BE55-6695-48B6-9EA3-D5A533752ED8'), -- Page: Extended Attributes, Block:MemberShip
('8E86FDCD-4189-4EA4-8370-24ABD6463516'), -- Page: Extended Attributes, Block:Visit Information
('441F849F-37C2-4709-B9BB-417204AF3168'), -- Page: Extended Attributes, Block:Childhood Information
('46D254C2-5A36-4F99-97A3-45DA8A49DB90'), -- Page: Extended Attributes, Block:Employment
('FFC9DF57-3E18-492C-B622-3EA167D7EBA1') -- Page: Extended Attributes, Block:Education

-- Internal site blocks
insert into #knownGuidsToIgnore values 
('718C516F-0A1D-4DBC-A939-1D9777208FEC'), -- Page: Intranet > Employee Resources, Block:HR INfo
('B8224C72-4168-40F0-96BE-38F2AFD525F5'), -- Page: Intranet > Shared Docs, 
('6A648E77-ABA9-4AAF-A8BB-027A12261ED9'), -- Internal Home Page > HTML Content Quick Links
('CB8F9152-08BB-4576-B7A1-B0DDD9880C44'), -- Internal Home Page > Active Users Internal
('03FCBF5A-42E0-4F45-B670-BC8E324BD573')  -- Internal Home Page > Active Users External

-- Internal site PersonDetail block attribute values
insert into #knownGuidsToIgnore values 
('e919e722-f895-44a4-b86d-38db8fba1844'), -- Attribute:Category Page , Page: Extended Attributes, Block:MemberShip
('7b879922-5da6-41ee-ac0b-45ceffb99458'), -- Attribute:Category Page , Page: Extended Attributes, Block:Visit Information
('752dc692-836e-4a3e-b670-4325cd7724bf'), -- Attribute:Category Page , Page: Extended Attributes, Block:Childhood Information
('f6b98d0c-197d-433a-917b-0c39a80a79e8'), -- Attribute:Category Page , Page: Extended Attributes, Block:Employment
('9af28593-e631-41e4-b696-78015a4d6f7b') -- Attribute:Category Page , Page: Extended Attributes, Block:Education

-- External site block attribute values
insert into #knownGuidsToIgnore values 
('54179287-4757-4069-8D9C-043CAA3DA096'), -- Attribute/Value:Ad Types/2, Block:Ad List, Page: External Homepage, Site: External Website
('601B5A8E-4072-48BC-B9C0-183B901BA088'), -- Attribute/Value:Audience/57b2a23f-3b0c-43a8-9f45-332120d, Block:Ad List, Page: External Homepage, Site: External Website
('72BA58E7-A6E5-49E1-975F-342A6B13985A'), -- Attribute/Value:Audience Primary Secondary/1,2, Block:Ad List, Page: External Homepage, Site: External Website
('3ABCF0EA-5159-45E9-831D-D7A43A695668'), -- Attribute/Value:Campuses/1, Block:Ad List, Page: External Homepage, Site: External Website
('9789D3BC-8878-450E-B664-88CEACCCCB89'), -- Attribute/Value:Detail Page/5eb07686-d032-41a5-95c0-fd36f93, Block:Ad List, Page: External Homepage, Site: External Website
('D0564600-361D-452B-98B8-2593B25ADD71'), -- Attribute/Value:Enable Debug/False, Block:Ad List, Page: External Homepage, Site: External Website
('41D569A7-C983-40B1-B34B-36E939E2255A'), -- Attribute/Value:Image Height/, Block:Ad List, Page: External Homepage, Site: External Website
('5664C99F-32E7-4C76-85C8-43F0CB030F32'), -- Attribute/Value:Image Types/PromotionImage, Block:Ad List, Page: External Homepage, Site: External Website
('D606EB8C-0403-403D-8328-F55C6BD9900E'), -- Attribute/Value:Image Width/400, Block:Ad List, Page: External Homepage, Site: External Website
('D84646CE-5DC4-4BF2-AF29-8323C509FE6F'), -- Attribute/Value:Max Items/3, Block:Ad List, Page: External Homepage, Site: External Website
('8445CE32-274E-4BA4-B921-B00CDE3F58BE'), -- Attribute/Value:Template/{% include 'AdList' %}, Block:Ad List, Page: External Homepage, Site: External Website
('C076F51A-6FB3-48D3-AB12-8B187B70796E'), -- Attribute/Value:Ad Types/2, Block:Ad Rotator, Page: External Homepage, Site: External Website
('9B1A9A0D-8BA6-4308-A5B5-8A097D22CB40'), -- Attribute/Value:Audience/b364cdee-f000-4965-ae67-0c80dda, Block:Ad Rotator, Page: External Homepage, Site: External Website
('654B2E0F-4E40-4CCB-AC2F-46A6967376D2'), -- Attribute/Value:Audience Primary Secondary/1,2, Block:Ad Rotator, Page: External Homepage, Site: External Website
('61A5C352-57F1-47FB-B54B-C0DB9D7A172F'), -- Attribute/Value:Campuses/1, Block:Ad Rotator, Page: External Homepage, Site: External Website
('A0273246-E0E5-46BF-B284-A80690309ED9'), -- Attribute/Value:Detail Page/5eb07686-d032-41a5-95c0-fd36f93, Block:Ad Rotator, Page: External Homepage, Site: External Website
('3B0101ED-7B4A-48C1-B554-D4EEDF8CA6CC'), -- Attribute/Value:Enable Debug/False, Block:Ad Rotator, Page: External Homepage, Site: External Website
('413634C1-0456-4E22-8460-DC299E4F14FD'), -- Attribute/Value:Image Height/, Block:Ad Rotator, Page: External Homepage, Site: External Website
('7354DAA2-6EC2-45B2-8A71-786FAE8B3E01'), -- Attribute/Value:Image Types/PromotionImage, Block:Ad Rotator, Page: External Homepage, Site: External Website
('481A8666-9480-48C4-8BFF-22F3D72143FE'), -- Attribute/Value:Image Width/, Block:Ad Rotator, Page: External Homepage, Site: External Website
('B417DA04-D080-4EC0-B37E-41FDEC050952'), -- Attribute/Value:Max Items/7, Block:Ad Rotator, Page: External Homepage, Site: External Website
('00A7246D-26FC-41C2-AF0F-879CF25654FD'), -- Attribute/Value:Template/{% include 'AdRotator' %}, Block:Ad Rotator, Page: External Homepage, Site: External Website
('3444AE29-8604-40C7-A91A-24E423591F33'), -- Attribute/Value:Cache Duration/3600, Block:Footer Address, Layout: LeftSidebar, Site: External Website
('D3098198-9BF8-41F4-AFB1-635A657C30A7'), -- Attribute/Value:Cache Duration/3600, Block:Footer Address, Layout: FullWidth, Site: External Website
('EE056117-7708-433E-BF66-4DC6EA1C6276'), -- Attribute/Value:Context Name/FooterAddress, Block:Footer Address, Layout: FullWidth, Site: External Website
('BD30EF37-EC9E-4BA8-82B6-49EB7D34914C'), -- Attribute/Value:Context Name/FooterAddress, Block:Footer Address, Layout: LeftSidebar, Site: External Website
('A3C1012E-23C5-4AC0-B020-DAC504C14583'), -- Attribute/Value:Context Parameter/, Block:Footer Address, Layout: LeftSidebar, Site: External Website
('906DCF51-E2C9-4239-9541-7E2B9EF83F6C'), -- Attribute/Value:Context Parameter/, Block:Footer Address, Layout: FullWidth, Site: External Website
('BCBA58E4-3E58-4159-8797-A76AC826A4CB'), -- Attribute/Value:Post-Text/</div>, Block:Footer Address, Layout: FullWidth, Site: External Website
('773448D0-1CA8-4B7B-843F-FAFD5C085776'), -- Attribute/Value:Post-Text/</div>, Block:Footer Address, Layout: LeftSidebar, Site: External Website
('A528F988-876B-4549-B52A-F1F2EF229CF3'), -- Attribute/Value:Pre-Text/<div class="footer-address">, Block:Footer Address, Layout: LeftSidebar, Site: External Website
('B75FCEBE-45DE-4EC7-9BE5-C92E08DE2BE2'), -- Attribute/Value:Pre-Text/<div class="footer-address">, Block:Footer Address, Layout: FullWidth, Site: External Website
('CE4B3596-BBFD-4CF7-84D0-9280F3E9CF01'), -- Attribute/Value:Require Approval/False, Block:Footer Address, Layout: LeftSidebar, Site: External Website
('82FF28E2-AC49-4B5C-8B3F-4ABACB3BD013'), -- Attribute/Value:Require Approval/False, Block:Footer Address, Layout: FullWidth, Site: External Website
('49C5B3FE-2F41-426B-8E4F-D583C43D7985'), -- Attribute/Value:Support Versions/False, Block:Footer Address, Layout: FullWidth, Site: External Website
('BD7DD9B5-ECDD-4183-9955-BCFB0EFB2A86'), -- Attribute/Value:Support Versions/False, Block:Footer Address, Layout: LeftSidebar, Site: External Website
('2840A605-10F8-4F2F-B3E1-1A2DF5407D87'), -- Attribute/Value:Cache Duration/3600, Block:Footer Text, Layout: LeftSidebar, Site: External Website
('4ED4A0A0-C73B-4DB1-B99E-7CA637417791'), -- Attribute/Value:Cache Duration/3600, Block:Footer Text, Layout: FullWidth, Site: External Website
('2C2CF5D9-A96E-4AC9-AB8E-D36EAEA20B12'), -- Attribute/Value:Cache Duration/3600, Block:Footer Text, Layout: Homepage, Site: External Website
('27271CFC-1EB7-4825-88FB-C7AB2E4DF31E'), -- Attribute/Value:Context Name/ExternalSiteFooterText, Block:Footer Text, Layout: FullWidth, Site: External Website
('FCF574C9-ACF5-4110-B8DF-699BBB6D28A9'), -- Attribute/Value:Context Name/ExternalSiteFooterText, Block:Footer Text, Layout: Homepage, Site: External Website
('C8E456D3-B82F-4E74-B582-1A456E925152'), -- Attribute/Value:Context Name/ExternalSiteFooterText, Block:Footer Text, Layout: LeftSidebar, Site: External Website
('08F7F6F0-CD0E-4AF6-97E6-A2D5191E9762'), -- Attribute/Value:Context Parameter/, Block:Footer Text, Layout: FullWidth, Site: External Website
('7415DF5D-F750-4D75-8829-6944BDBBBAEB'), -- Attribute/Value:Context Parameter/, Block:Footer Text, Layout: LeftSidebar, Site: External Website
('DA72CDC0-6A96-4CF3-A646-ECDF2750E967'), -- Attribute/Value:Context Parameter/, Block:Footer Text, Layout: Homepage, Site: External Website
('EEB2BD3E-9294-45E9-8E05-EB7210026867'), -- Attribute/Value:Post-Text/</div>, Block:Footer Text, Layout: FullWidth, Site: External Website
('6A2E326B-A289-4C40-8313-F50DC9DD3EC3'), -- Attribute/Value:Post-Text/</div>, Block:Footer Text, Layout: Homepage, Site: External Website
('420D2277-3B4B-4CE9-B14B-02C562908943'), -- Attribute/Value:Post-Text/</div>, Block:Footer Text, Layout: LeftSidebar, Site: External Website
('3DAC02DA-5862-4DAB-BB3D-24E94269862A'), -- Attribute/Value:Pre-Text/<div class="footer-message">, Block:Footer Text, Layout: LeftSidebar, Site: External Website
('BCF70066-BC60-41BD-8E5F-2306D815E77D'), -- Attribute/Value:Pre-Text/<div class="footer-message">, Block:Footer Text, Layout: Homepage, Site: External Website
('6E2988AB-FA5B-4323-A256-BDF60084BE26'), -- Attribute/Value:Pre-Text/<div class="footer-message">, Block:Footer Text, Layout: FullWidth, Site: External Website
('CB85FF84-BF74-4761-9D9E-BB2C36E91A1D'), -- Attribute/Value:Require Approval/False, Block:Footer Text, Layout: FullWidth, Site: External Website
('8E9EA642-EAD8-478A-9BFB-F8CA403513DE'), -- Attribute/Value:Require Approval/False, Block:Footer Text, Layout: LeftSidebar, Site: External Website
('C75B643C-4E5A-45B8-8AA0-B0D6E45B7AA3'), -- Attribute/Value:Require Approval/False, Block:Footer Text, Layout: Homepage, Site: External Website
('C49B4533-80D5-4F40-A3FC-D52123A2D511'), -- Attribute/Value:Support Versions/False, Block:Footer Text, Layout: Homepage, Site: External Website
('0209B270-1E00-4F6D-B8B3-A5F95615530B'), -- Attribute/Value:Support Versions/False, Block:Footer Text, Layout: LeftSidebar, Site: External Website
('5E833E45-5DB3-4CCB-8BDB-C73432903DBC'), -- Attribute/Value:Support Versions/False, Block:Footer Text, Layout: FullWidth, Site: External Website
('FF11F1B3-DE16-4B62-8754-47826B442E7C'), -- Attribute/Value:Confirmation Page/288dbec5-8a43-4133-9313-aa2fe81, Block:Forgot UserName, Page: Forgot Password, Site: External Website
('362598D2-D0EC-4995-BA50-CB52B3EC6E1C'), -- Attribute/Value:Heading Caption/Enter your email address below , Block:Forgot UserName, Page: Forgot Password, Site: External Website
('FE2E6205-D888-4A4D-8B3C-38F2E0D8BA9B'), -- Attribute/Value:Invalid Email Caption/There are not any accounts for , Block:Forgot UserName, Page: Forgot Password, Site: External Website
('88388EE1-20CD-446C-AA30-1FB8A6BC8B3C'), -- Attribute/Value:Success Caption/Your user name has been sent to, Block:Forgot UserName, Page: Forgot Password, Site: External Website
('C20EB204-8179-47C4-8CF2-FEF64185D3A1'), -- Attribute/Value:Cache Duration/3600, Block:Header Text, Layout: Homepage, Site: External Website
('56CBF9F5-9F65-4E9A-AAA2-0D2001516DB1'), -- Attribute/Value:Cache Duration/3600, Block:Header Text, Layout: FullWidth, Site: External Website
('46D2C04F-AC24-4FA8-9FF8-A3FE3E7A47D8'), -- Attribute/Value:Cache Duration/3600, Block:Header Text, Layout: LeftSidebar, Site: External Website
('F0E784C1-1BDB-4E80-A9CF-9E982EEB732D'), -- Attribute/Value:Context Name/ExternalSiteHeaderText, Block:Header Text, Layout: FullWidth, Site: External Website
('7EA0E882-FC1B-44CD-ADF3-AE9AE2F57AB8'), -- Attribute/Value:Context Name/ExternalSiteHeaderText, Block:Header Text, Layout: Homepage, Site: External Website
('8472D215-7F6C-4260-8F9C-470300EC73EF'), -- Attribute/Value:Context Name/ExternalSiteHeaderText, Block:Header Text, Layout: LeftSidebar, Site: External Website
('60F5D72C-78F7-47DE-9071-F74FF7BF6252'), -- Attribute/Value:Context Parameter/, Block:Header Text, Layout: LeftSidebar, Site: External Website
('92DFF4C0-8883-4E6F-ABEC-FD2D8E22A16C'), -- Attribute/Value:Context Parameter/, Block:Header Text, Layout: Homepage, Site: External Website
('BB5F66E9-9AAC-4EE0-AB0D-84FB522DCB52'), -- Attribute/Value:Context Parameter/, Block:Header Text, Layout: FullWidth, Site: External Website
('6B30C737-4F4D-4937-80C3-924CD1EED8E9'), -- Attribute/Value:Post-Text/, Block:Header Text, Layout: LeftSidebar, Site: External Website
('68C88842-44C5-40CB-87B8-7EE44EBB6CF2'), -- Attribute/Value:Post-Text/, Block:Header Text, Layout: FullWidth, Site: External Website
('42D5F939-1600-4EC4-BEAA-A8D9FF1831FC'), -- Attribute/Value:Post-Text/, Block:Header Text, Layout: Homepage, Site: External Website
('5CCD4D47-B9F5-479A-BE83-64C4BBB7B99E'), -- Attribute/Value:Pre-Text/, Block:Header Text, Layout: Homepage, Site: External Website
('DF1DE8AD-A721-4B5E-A749-74B5A6ED7F05'), -- Attribute/Value:Pre-Text/, Block:Header Text, Layout: FullWidth, Site: External Website
('D1BB6C19-EE8E-47EA-AE1E-09F321EF1636'), -- Attribute/Value:Pre-Text/, Block:Header Text, Layout: LeftSidebar, Site: External Website
('70BEAD4D-8C45-4D16-A1BF-9045E7A304A7'), -- Attribute/Value:Require Approval/False, Block:Header Text, Layout: FullWidth, Site: External Website
('D79D63AA-AD3E-4567-B59F-14F69A66122E'), -- Attribute/Value:Require Approval/False, Block:Header Text, Layout: LeftSidebar, Site: External Website
('B1C69347-7AB6-4B64-893E-793BEF404BA3'), -- Attribute/Value:Require Approval/False, Block:Header Text, Layout: Homepage, Site: External Website
('1DD85446-D480-4DB7-9543-D9B70C1EB8F3'), -- Attribute/Value:Support Versions/False, Block:Header Text, Layout: Homepage, Site: External Website
('17C0842E-1DA2-498D-935D-FC393BCF2153'), -- Attribute/Value:Support Versions/False, Block:Header Text, Layout: LeftSidebar, Site: External Website
('46EC67E0-7654-4E36-BD41-4311A5CD398B'), -- Attribute/Value:Support Versions/False, Block:Header Text, Layout: FullWidth, Site: External Website
('3CE5B8F3-3BF5-4694-9E0A-AE426E0FD695'), -- Attribute/Value:Layout/<img src="/GetImage.ashx?Id={{ , Block:Marketing Campaign Ad Detail, Page: Ad Details, Site: External Website
('08E3C319-26E1-4F35-961B-C8580E75C415'), -- Attribute/Value:CSS File/, Block:Navigation, Layout: Homepage, Site: External Website
('3C3BB76C-B07F-429C-BE8E-E82DE8034584'), -- Attribute/Value:Include Current Parameters/False, Block:Navigation, Layout: Homepage, Site: External Website
('A564E3D5-45FB-4A99-BC41-72C731122ACA'), -- Attribute/Value:Include Current QueryString/False, Block:Navigation, Layout: Homepage, Site: External Website
('C45C72C2-FB0D-4D0A-819D-CD9E40F4BB4C'), -- Attribute/Value:Number of Levels/1, Block:Navigation, Layout: Homepage, Site: External Website
('83E2F855-85E1-4D81-97A4-0ACD487FEEA3'), -- Attribute/Value:Root Page/85f25819-e948-4960-9ddf-00f54d3, Block:Navigation, Layout: Homepage, Site: External Website
('C4F65E1D-FD93-4CF6-A5C4-03453B3797C8'), -- Attribute/Value:Show Debug/False, Block:Navigation, Layout: Homepage, Site: External Website
('C34BC5FA-68F4-41A3-865D-8DFA7C65522D'), -- Attribute/Value:Template/{% include 'PageNav' %}, Block:Navigation, Layout: Homepage, Site: External Website
('AEB294CA-FF2F-4279-89C9-5D3C1D8CFD6E'), -- Attribute/Value:Check for Duplicates/True, Block:New Account, Page: Account Registration, Site: External Website
('1D850DCB-A4FD-4D81-8C77-982061E57EA5'), -- Attribute/Value:Confirm Caption/Because you've selected an exis, Block:New Account, Page: Account Registration, Site: External Website
('B930F10C-6EBC-41E8-A578-A36641EE4F3D'), -- Attribute/Value:Confirmation Page/288dbec5-8a43-4133-9313-aa2fe81, Block:New Account, Page: Account Registration, Site: External Website
('84B1060E-CB28-4378-9275-1A9145A7372A'), -- Attribute/Value:Existing Account Caption/{0}, you already have an existi, Block:New Account, Page: Account Registration, Site: External Website
('B4B96706-7DAA-4184-B8BC-C07189FEEBBE'), -- Attribute/Value:Found Duplicate Caption/There are already one or more p, Block:New Account, Page: Account Registration, Site: External Website
('1E5FB18B-293C-4012-B764-673D1C3F2C33'), -- Attribute/Value:Login Page/d025e14c-f385-43fb-a735-abd24ad, Block:New Account, Page: Account Registration, Site: External Website
('7FA841F8-FFC7-4BA6-9A84-5C07AAB0D501'), -- Attribute/Value:Sent Login Caption/Your username has been emailed , Block:New Account, Page: Account Registration, Site: External Website
('96C6F58A-A483-436D-8C53-00C0EBB8BD0E'), -- Attribute/Value:Success Caption/{0}, Your account has been crea, Block:New Account, Page: Account Registration, Site: External Website
('F8BE651D-F4B4-42A2-9A53-9545E66CB050'), -- Attribute/Value:Cache Duration/3600, Block:Org Info, Layout: Homepage, Site: External Website
('314FEBAF-2BF3-40BE-8834-74A7E7AB519B'), -- Attribute/Value:Context Name/FooterAddress, Block:Org Info, Layout: Homepage, Site: External Website
('36D4CE63-B236-4AEC-8489-60E03874A27B'), -- Attribute/Value:Context Parameter/, Block:Org Info, Layout: Homepage, Site: External Website
('EE48E27E-8AFB-4599-A467-73E4733230D1'), -- Attribute/Value:Post-Text/</div>, Block:Org Info, Layout: Homepage, Site: External Website
('5D1BF876-222D-413B-A592-6015A9AB0359'), -- Attribute/Value:Pre-Text/<div class="footer-address">, Block:Org Info, Layout: Homepage, Site: External Website
('74798A3C-61FA-4CAF-99CB-F04C506CD323'), -- Attribute/Value:Require Approval/False, Block:Org Info, Layout: Homepage, Site: External Website
('5A08E73B-ED91-4629-891E-667A271F59C1'), -- Attribute/Value:Support Versions/False, Block:Org Info, Layout: Homepage, Site: External Website
('11849F2D-D6E4-4128-9E98-5D50E846D8EB'), -- Attribute/Value:CSS File/, Block:Page Menu, Layout: LeftSidebar, Site: External Website
('638D690A-A094-40FE-A51E-260D9DBBB6FB'), -- Attribute/Value:Include Current Parameters/True, Block:Page Menu, Layout: LeftSidebar, Site: External Website
('4BF97016-808E-40BA-B261-4ED01A922DBD'), -- Attribute/Value:Include Current QueryString/True, Block:Page Menu, Layout: LeftSidebar, Site: External Website
('66191B9D-7C5B-4864-BEE8-2F91ECF4C13A'), -- Attribute/Value:Number of Levels/1, Block:Page Menu, Layout: LeftSidebar, Site: External Website
('C571277B-B34B-415A-BC34-01E800B841F1'), -- Attribute/Value:Root Page/85f25819-e948-4960-9ddf-00f54d3, Block:Page Menu, Layout: LeftSidebar, Site: External Website
('EC8B992F-1049-4D95-86EE-2F64BE23FD9F'), -- Attribute/Value:Template/{% include 'PageNav' %}, Block:Page Menu, Layout: LeftSidebar, Site: External Website
('FED834C3-F4E1-4B92-9697-A4AB8BC7795D'), -- Attribute/Value:CSS File/, Block:Page Nav, Layout: FullWidth, Site: External Website
('BB4E5568-6CE9-4EAF-AE05-A5A453ED593B'), -- Attribute/Value:Include Current Parameters/False, Block:Page Nav, Layout: FullWidth, Site: External Website
('B4BC194B-AD07-4779-A31F-B008BC9B0B40'), -- Attribute/Value:Include Current QueryString/False, Block:Page Nav, Layout: FullWidth, Site: External Website
('CB7E3B98-26FC-4CDD-88D1-F435089FEF14'), -- Attribute/Value:Number of Levels/1, Block:Page Nav, Layout: FullWidth, Site: External Website
('09DA599C-4930-4A3D-BC68-82E0495AC4F1'), -- Attribute/Value:Root Page/85f25819-e948-4960-9ddf-00f54d3, Block:Page Nav, Layout: FullWidth, Site: External Website
('F5379FE8-F61B-4075-A30A-3010590D0559'), -- Attribute/Value:Template/{% include 'PageNav' %}, Block:Page Nav, Layout: FullWidth, Site: External Website
('A6E893F4-8A8A-441C-9925-751F8403F82E'), -- Attribute/Value:CSS File/, Block:Sub Nav, Page: Give, Site: External Website
('24EF8C49-1D25-4206-A2C4-D4FAB1402695'), -- Attribute/Value:CSS File/, Block:Sub Nav, Page: Children, Site: External Website
('F591E966-514B-4F63-93A2-83012AE1972D'), -- Attribute/Value:CSS File/, Block:Sub Nav, Page: Students, Site: External Website
('4907AD06-E65B-4422-BD59-D4298A4B136F'), -- Attribute/Value:CSS File/, Block:Sub Nav, Page: Adults, Site: External Website
('D5B83B5B-EFB5-4CBA-9234-FFE320E8B9A7'), -- Attribute/Value:CSS File/, Block:Sub Nav, Page: Prayer, Site: External Website
('D2A44FDA-EFC3-45DC-81CF-2FC464A2F3E2'), -- Attribute/Value:CSS File/, Block:Sub Nav, Page: Missions, Site: External Website
('9DE04FE6-0402-4C26-B4E2-9D5653D551F2'), -- Attribute/Value:CSS File/, Block:Sub Nav, Page: Serve, Site: External Website
('52769399-83AF-4769-B5F2-DA8BA30CFEB2'), -- Attribute/Value:CSS File/, Block:Sub Nav, Page: Small Groups, Site: External Website
('27DE9C63-7514-4DEB-B20E-ADB99CE932BD'), -- Attribute/Value:CSS File/, Block:Sub Nav, Page: Prayer, Site: External Website
('DA9A5961-2AFD-4948-BCA7-4FEF3E16F22C'), -- Attribute/Value:Include Current Parameters/False, Block:Sub Nav, Page: Prayer, Site: External Website
('E4090A57-DBC8-4BDA-B367-8FEACD462B8B'), -- Attribute/Value:Include Current Parameters/False, Block:Sub Nav, Page: Small Groups, Site: External Website
('2BE3E6D0-4461-48E9-8E6D-D25BFF21F24A'), -- Attribute/Value:Include Current Parameters/False, Block:Sub Nav, Page: Serve, Site: External Website
('F7AABB9E-D595-48F1-A103-579B4715AEAF'), -- Attribute/Value:Include Current Parameters/False, Block:Sub Nav, Page: Missions, Site: External Website
('B46A14B6-A493-4DE9-8823-30B6E1CF958D'), -- Attribute/Value:Include Current Parameters/False, Block:Sub Nav, Page: Prayer, Site: External Website
('926C817E-ACF7-4400-AD52-725FA55317A1'), -- Attribute/Value:Include Current Parameters/False, Block:Sub Nav, Page: Adults, Site: External Website
('975B61C1-4C75-4650-A226-B0985D7F8633'), -- Attribute/Value:Include Current Parameters/False, Block:Sub Nav, Page: Students, Site: External Website
('1C65A0EA-FCD7-43A1-8C19-C64698AD1D52'), -- Attribute/Value:Include Current Parameters/False, Block:Sub Nav, Page: Children, Site: External Website
('BCC7C8E4-F08C-4A7E-BA83-3D017EEEC79F'), -- Attribute/Value:Include Current Parameters/False, Block:Sub Nav, Page: Give, Site: External Website
('658D0054-2CBF-4870-8F58-64CB79B73DA8'), -- Attribute/Value:Include Current QueryString/False, Block:Sub Nav, Page: Give, Site: External Website
('DD9CAF2A-76C2-4441-946B-729F920EBFA9'), -- Attribute/Value:Include Current QueryString/False, Block:Sub Nav, Page: Children, Site: External Website
('4DCB88B6-19C2-494E-B938-1B2D1C1EE57E'), -- Attribute/Value:Include Current QueryString/False, Block:Sub Nav, Page: Students, Site: External Website
('96B7DB77-1267-4DFF-8CC5-B53E4DC80D42'), -- Attribute/Value:Include Current QueryString/False, Block:Sub Nav, Page: Adults, Site: External Website
('06EDCFBA-AA8B-4C3E-9001-496B97B6B153'), -- Attribute/Value:Include Current QueryString/False, Block:Sub Nav, Page: Prayer, Site: External Website
('D136723B-104A-42BE-9D6D-FF54521CF35B'), -- Attribute/Value:Include Current QueryString/False, Block:Sub Nav, Page: Missions, Site: External Website
('570F0362-44B7-484D-AC2A-FB26EF1D1F77'), -- Attribute/Value:Include Current QueryString/False, Block:Sub Nav, Page: Serve, Site: External Website
('684D9500-9F6C-458E-8268-0EE182F380AA'), -- Attribute/Value:Include Current QueryString/False, Block:Sub Nav, Page: Small Groups, Site: External Website
('0411E1BA-42EE-4BFC-84F7-5DDA650E3AC8'), -- Attribute/Value:Include Current QueryString/False, Block:Sub Nav, Page: Prayer, Site: External Website
('CC3BF787-9198-4B37-9099-D7EF59D59562'), -- Attribute/Value:Number of Levels/1, Block:Sub Nav, Page: Prayer, Site: External Website
('6A0D988C-5B8B-4CCC-9402-A90AB607BD63'), -- Attribute/Value:Number of Levels/1, Block:Sub Nav, Page: Small Groups, Site: External Website
('FC2B83A1-943B-4984-AEB1-103C1938FBAA'), -- Attribute/Value:Number of Levels/1, Block:Sub Nav, Page: Serve, Site: External Website
('6C58AD49-E1E2-4067-89D3-89327F562916'), -- Attribute/Value:Number of Levels/1, Block:Sub Nav, Page: Missions, Site: External Website
('19901DEB-B4FF-4EC8-9ADC-0FF680BD471D'), -- Attribute/Value:Number of Levels/1, Block:Sub Nav, Page: Adults, Site: External Website
('ED96DFAD-C79E-4DA1-A8E6-708A3CA9CB28'), -- Attribute/Value:Number of Levels/1, Block:Sub Nav, Page: Prayer, Site: External Website
('BE0EAB28-1462-441E-B218-038347ECB86E'), -- Attribute/Value:Number of Levels/1, Block:Sub Nav, Page: Children, Site: External Website
('F258B890-1E01-45C3-A58E-F7336671A8D6'), -- Attribute/Value:Number of Levels/1, Block:Sub Nav, Page: Students, Site: External Website
('715CD0C3-816F-4561-A9B2-1AE4F3D2011D'), -- Attribute/Value:Number of Levels/2, Block:Sub Nav, Page: Give, Site: External Website
('781611EE-9F3B-4524-AFCF-71709042EC88'), -- Attribute/Value:Root Page/8bb303af-743c-49dc-a7ff-cc1236b, Block:Sub Nav, Page: Give, Site: External Website
('77850D6A-C0A0-4DE0-9C11-10F885864D18'), -- Attribute/Value:Root Page/7625a63e-6650-4886-b605-53c2234, Block:Sub Nav, Page: Students, Site: External Website
('54EC4622-C2E4-4685-BFEA-53B439A536BE'), -- Attribute/Value:Root Page/7625a63e-6650-4886-b605-53c2234, Block:Sub Nav, Page: Prayer, Site: External Website
('643FC81E-1158-4578-B26F-7A90882BD042'), -- Attribute/Value:Root Page/7625a63e-6650-4886-b605-53c2234, Block:Sub Nav, Page: Adults, Site: External Website
('865D4B46-C558-4C43-A492-EEC1368BE656'), -- Attribute/Value:Root Page/7625a63e-6650-4886-b605-53c2234, Block:Sub Nav, Page: Children, Site: External Website
('E8399D68-352E-45E8-8E6E-511E6536CC6C'), -- Attribute/Value:Root Page/7625a63e-6650-4886-b605-53c2234, Block:Sub Nav, Page: Missions, Site: External Website
('BEA25503-E247-4A9B-B53C-0F2E041604D5'), -- Attribute/Value:Root Page/7625a63e-6650-4886-b605-53c2234, Block:Sub Nav, Page: Serve, Site: External Website
('2B17B87C-EBB9-40A4-AA7C-DC271759567E'), -- Attribute/Value:Root Page/7625a63e-6650-4886-b605-53c2234, Block:Sub Nav, Page: Small Groups, Site: External Website
('A7BE6AE0-1E93-4F06-999B-638B9F227FA8'), -- Attribute/Value:Root Page/, Block:Sub Nav, Page: Prayer, Site: External Website
('A18CBB26-D13A-4D8E-A0DA-4CBD5E8D3795'), -- Attribute/Value:Template/{% include 'PageSubNav'  %}, Block:Sub Nav, Page: Prayer, Site: External Website
('7D19954A-97D1-4D4F-ABF5-9CE6E7811673'), -- Attribute/Value:Template/{% include 'PageSubNav' %}, Block:Sub Nav, Page: Small Groups, Site: External Website
('68021A71-3886-4D78-91E0-481151298577'), -- Attribute/Value:Template/{% include 'PageSubNav' %}, Block:Sub Nav, Page: Missions, Site: External Website
('4C6BBDAB-5FED-4B47-8DA8-C6A1DB7F9367'), -- Attribute/Value:Template/{% include 'PageSubNav' %}, Block:Sub Nav, Page: Serve, Site: External Website
('81655F14-7184-4A28-831A-F1E6741746F6'), -- Attribute/Value:Template/{% include 'PageSubNav' %}, Block:Sub Nav, Page: Children, Site: External Website
('0AAB22C0-FCE9-45DE-922A-F2BF4D24EBCB'), -- Attribute/Value:Template/{% include 'PageSubNav' %}, Block:Sub Nav, Page: Prayer, Site: External Website
('E58939AB-3958-4295-8611-6883002F4FB8'), -- Attribute/Value:Template/{% include 'PageSubNav' %}, Block:Sub Nav, Page: Adults, Site: External Website
('E9193A6C-35B5-47CE-B866-DE3BC58EC522'), -- Attribute/Value:Template/{% include 'PageSubNav' %}, Block:Sub Nav, Page: Students, Site: External Website
('6462E0AB-B2EB-4F03-B437-261F8A9799B7'), -- Attribute/Value:Template/{% include 'PageSubNav'  %}, Block:Sub Nav, Page: Give, Site: External Website
('3AF7D978-DC88-4DA7-A056-1BBB1FB97228'), -- Attribute/Value:CSS File/, Block:SubNav, Page: Connect, Site: External Website
('020D9209-F597-4002-9CAA-5F86332A973D'), -- Attribute/Value:Include Current Parameters/False, Block:SubNav, Page: Connect, Site: External Website
('D1BF07FA-B941-4EAA-99C8-067709055951'), -- Attribute/Value:Include Current QueryString/False, Block:SubNav, Page: Connect, Site: External Website
('7A04096E-3AE2-4439-A47B-C8E6D77A00E8'), -- Attribute/Value:Number of Levels/3, Block:SubNav, Page: Connect, Site: External Website
('A483E4A7-6E1F-4586-B981-82226F3B3FFD'), -- Attribute/Value:Root Page/, Block:SubNav, Page: Connect, Site: External Website
('ADEB95F0-8D19-4CB7-8220-5CAE817B9D02'), -- Attribute/Value:Template/{% include 'PageSubNav' %}, Block:SubNav, Page: Connect, Site: External Website
('13601B14-C4FA-452D-96A9-08F4103FED0E'), -- Attrib Value for Block:Create Pledge, Attribute:Account Page: Pledge, Site: Rock Solid Church
('FB060774-CEA8-4EF6-B4F9-AB82FEB2B5FA'), -- Attrib Value for Block:Create Pledge, Attribute:Show Pledge Frequency Page: Pledge, Site: Rock Solid Church
('075F7965-7FB5-4EE5-98ED-BC455F68A0EF'), -- Attrib Value for Block:Create Pledge, Attribute:Enable Smart Names Page: Pledge, Site: Rock Solid Church
('14565642-0367-4284-BC34-BD57F80381C8'), -- Attrib Value for Block:Create Pledge, Attribute:New Connection Status Page: Pledge, Site: Rock Solid Church
('4C35C9D1-F008-43DD-B04C-C12D25F0DE62'), -- Attrib Value for Block:Create Pledge, Attribute:Pledge Date Range Page: Pledge, Site: Rock Solid Church
('CA69B385-696E-4BDB-8091-4B3956C7E5EA'), -- Attrib Value for Block:Create Pledge, Attribute:Receipt Text Page: Pledge, Site: Rock Solid Church
('75E1BFA0-29E3-4629-BC1C-46EFD193C111'), -- Attrib Value for Block:Create Pledge, Attribute:Enable Debug Page: Pledge, Site: Rock Solid Church
('119381CC-263B-4AC2-BD4E-4753EAAAFDE2'), -- Attrib Value for Block:Create Pledge, Attribute:Require Pledge Frequency Page: Pledge, Site: Rock Solid Church
('0BB9E8A4-8709-4920-A0B2-A43A27BB2A89'), -- Attrib Value for Block:Create Pledge, Attribute:Save Button Text Page: Pledge, Site: Rock Solid Church
('4CAB6DCC-E58C-46A1-8C14-32CEDC378EE9'), -- Attrib Value for Block:Create Pledge, Attribute:Note Message Page: Pledge, Site: Rock Solid Church
('B8F49AA3-3713-4506-BFFC-7E15646D6395') -- Attrib Value for Block:Create Pledge, Attribute:Confirmation Email Template Page: Pledge, Site: Rock Solid Church


create table #codeTable (
    Id int identity(1,1) not null,
    CodeText nvarchar(max),
    CONSTRAINT [pk_codeTable] PRIMARY KEY CLUSTERED  ( [Id]) );
    
	-- layouts
    insert into #codeTable
    SELECT '            RockMigrationHelper.AddLayout("' +
        CONVERT( nvarchar(50), [s].[Guid]) + '","'+ 
        [l].[FileName] +  '","'+
        [l].[Name] +  '","'+
        ISNULL([l].[Description],'')+  '","'+
        CONVERT( nvarchar(50), [l].[Guid])+  '");' + ' // Site:' + s.Name 
    FROM [Layout] [l]
    join [Site] [s] on [s].[Id] = [l].[SiteId]
    where [l].[IsSystem] = 0
    and [l].[Guid] not in (select [Guid] from #knownGuidsToIgnore)
    order by [l].[Id]

    insert into #codeTable
    SELECT @crlf

	-- pages
    insert into #codeTable
    SELECT '            RockMigrationHelper.AddPage("' +
        CONVERT( nvarchar(50), [pp].[Guid]) + '","'+ 
        CONVERT( nvarchar(50), [l].[Guid]) + '","'+ 
        [p].[InternalName]+  '","'+  
        ISNULL([p].[Description],'')+  '","'+
        CONVERT( nvarchar(50), [p].[Guid])+  '","'+  
        ISNULL([p].[IconCssClass],'')+ '");' + ' // Site:' + s.Name 
    FROM [Page] [p]
    join [Page] [pp] on [pp].[Id] = [p].[ParentPageId]
	join [Layout] [l] on [l].[Id] = [p].[layoutId]
	join [site] [s] on [s].[Id] = [l].[siteId]
    where [p].[IsSystem] = 0
    and [p].[Guid] not in (select [Guid] from #knownGuidsToIgnore)
    order by [p].[Id]

    insert into #codeTable
    SELECT @crlf

    -- page routes
    insert into #codeTable
    SELECT '            RockMigrationHelper.AddPageRoute("' +
        CONVERT( nvarchar(50), [p].[Guid])+  '","'+  
        CONVERT( nvarchar(50), [pr].[Route]) + '","'+
		CONVERT( nvarchar(50), [pr].[Guid])+  '");'+ '// for Page:' + p.InternalName
    FROM [Page] [p]
    join [PageRoute] [pr] on [pr].[PageId] = [p].[Id]
    where [p].[IsSystem] = 0
    and [p].[Guid] not in (select [Guid] from #knownGuidsToIgnore)
    order by [p].[Id]

    insert into #codeTable
    SELECT @crlf

    -- block types
    insert into #codeTable
    SELECT '            RockMigrationHelper.UpdateBlockType("'+
        [Name]+ '","'+  
        ISNULL([Description],'')+ '","'+  
        [Path]+ '","'+  
        [Category]+ '","'+  
        CONVERT( nvarchar(50),[Guid])+ '");'
    from [BlockType]
    where [IsSystem] = 0
    and [Guid] not in (select [Guid] from #knownGuidsToIgnore) -- shouldn't happen
    order by [Id]

    insert into #codeTable
    SELECT @crlf

    -- blocks
	SELECT p.InternalName [p.InternalName]
			,l.NAME [l.Name]
			,s.NAME [s.Name]
			,p.Id [p.Id]
 			,p.[Guid] [p.Guid]
			,l.[Guid] [l.Guid]
			,bt.[Guid] [bt.Guid]
			,b.Name [b.Name]
			,b.Zone [b.Zone]
			,b.PreHtml [b.PreHtml]
			,b.PostHtml [b.PostHtml]
			,b.[Order] [b.Order]
			,b.[Guid] [b.Guid]
			,b.Id
			into #blocksTemp
		FROM [Block] [b]
		JOIN [BlockType] [bt] ON [bt].[Id] = [b].[BlockTypeId]
		LEFT OUTER JOIN [Page] [p] ON [p].[Id] = [b].[PageId]
		LEFT OUTER JOIN [Layout] [l] ON [l].[Id] = [b].[LayoutId]
		LEFT OUTER JOIN [Layout] [pl] ON [pl].[Id] = [p].[LayoutId]
		JOIN [site] [s] ON [s].[Id] = [l].[siteId]
			OR [s].[Id] = [pl].[siteId]
		WHERE [b].[IsSystem] = 0
			AND [b].[Guid] NOT IN (
				SELECT [Guid]
				FROM #knownGuidsToIgnore
				)
		ORDER BY [b].[Id]
	
    
    insert into #codeTable
    SELECT 
        '            // Add Block to ' + ISNULL('Page: ' + [p.InternalName],'') + ISNULL('Layout: ' + [l.Name], '') + ISNULL(', Site: ' + [s.Name], '') +
        @crlf + 
		'            RockMigrationHelper.AddBlock("'+
        ISNULL(CONVERT(nvarchar(50), [p.Guid]),'') + '","'+ 
        ISNULL(CONVERT(nvarchar(50), [l.Guid]),'') + '","'+
        CONVERT(nvarchar(50), [bt.Guid])+ '","'+
        [b.Name]+ '","'+
        [b.Zone]+ '",@"'+
		ISNULL(replace([b.PreHtml], '"', '""'), '')+ '",@"'+
		ISNULL(replace([b.PostHtml], '"', '""'), '')+ '",'+
        CONVERT(varchar, [b.Order])+ ',"'+
        CONVERT(nvarchar(50), [b.Guid])+ '"); '+
        @crlf
    from #blocksTemp

	if exists (select * from #blocksTemp bt 
	join [Block] b on b.PageId = bt.[p.Id] and b.Zone = bt.[b.Zone]
	where b.PageId in (select PageId from (
		select count(*) [PageBlockCount], bb.PageId, bb.Zone from [Block] bb group by bb.PageId, bb.Zone) a
		where a.PageBlockCount > 1
	    and PageId is not null)) begin
		insert into #codeTable select '            // update block order for pages with new blocks if the page,zone has multiple blocks'
	end

	
	declare @tempBlockOrderCode table (f nvarchar(max) )

	insert into @tempBlockOrderCode
	select concat('            Sql(@"', 'UPDATE [Block] SET [Order] = ', b.[Order], ' WHERE [Guid] = ''', b.[Guid]
	, '''");  // Page: ', bt.[p.InternalName]
	, ',  Zone: ', b.Zone
	, ',  Block: ', b.Name
	) 
	from #blocksTemp bt 
	join [Block] b on b.PageId = bt.[p.Id] and b.Zone = bt.[b.Zone]
	where b.PageId in (select PageId from (
		select count(*) [PageBlockCount], bb.PageId, bb.Zone from [Block] bb group by bb.PageId, bb.Zone) a
		where a.PageBlockCount > 1
	    and PageId is not null)
	order by bt.[p.InternalName], b.Zone, b.[Order]

	insert into #codeTable select distinct * from @tempBlockOrderCode

	insert into #codeTable
    SELECT @crlf
	
	DROP TABLE #blocksTemp

    -- html content blocks
    insert into #codeTable
    SELECT
        '            // Add/Update HtmlContent for ' + ISNULL('Block: ' + b.Name,'') +
        @crlf + 
		'            RockMigrationHelper.UpdateHtmlContentBlock("' +
        ISNULL(CONVERT(nvarchar(50), [b].[Guid]),'') + '",@"'+ 
        replace(h.Content, '"', '""') + '","'+
        CONVERT(nvarchar(50), [h].[Guid])+ '"); '+
        @crlf
    from [Block] [b]
    join [BlockType] [bt] on [bt].[Id] = [b].[BlockTypeId]
    join [HtmlContent] [h] on [h].BlockId = b.Id
    where [b].[IsSystem] = 0
    and [b].[Guid] not in (select [Guid] from #knownGuidsToIgnore)
    order by [b].[Id]

    -- attributes
    if object_id('tempdb..#attributeIds') is not null
    begin
      drop table #attributeIds
    end

    select * 
	into #attributeIds 
	from (
		select A.[Id] 
		from [dbo].[Attribute] A
		inner join [EntityType] E 
			ON E.[Id] = A.[EntityTypeId]
		where A.[IsSystem] = 0
        and [A].[Guid] not in (select [Guid] from #knownGuidsToIgnore)
		and E.[Name] = 'Rock.Model.Block'
		and A.EntityTypeQualifierColumn = 'BlockTypeId'
	) [newattribs]
    
    insert into #codeTable
    SELECT @crlf

    insert into #codeTable
    SELECT 
        '            // Attrib for BlockType: ' + bt.Name + ':'+ a.Name+
        @crlf +
        '            RockMigrationHelper.UpdateBlockTypeAttribute("'+ 
        CONVERT(nvarchar(50), bt.Guid)+ '","'+   
        CONVERT(nvarchar(50), ft.Guid)+ '","'+     
        a.Name+ '","'+  
        a.[Key]+ '","'+ 
        ''+ '","'+ 
        --ISNULL(a.Category,'')+ '","'+ 
        ISNULL(a.Description,'')+ '",'+ 
        CONVERT(varchar, a.[Order])+ ',@"'+ 
        ISNULL(a.DefaultValue,'')+ '","'+
        CONVERT(nvarchar(50), a.Guid)+ '");' +
        @crlf
    from [Attribute] [a]
    left outer join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    left outer join [BlockType] [bt] on [bt].[Id] = cast([a].[EntityTypeQualifierValue] as int)
    where EntityTypeQualifierColumn = 'BlockTypeId'
    and [a].[id] in (select [Id] from #attributeIds)

    insert into #codeTable
    SELECT @crlf

    -- attributes values (just Block Attributes)    
    insert into #codeTable
    SELECT 
        '            // Attrib Value for Block:'+ b.Name + ', Attribute:'+ a.Name + ' ' + ISNULL('Page: ' + p.InternalName,'') + ISNULL(', Layout: ' + l.Name, '') +  ISNULL(', Site: ' + s.Name, '') +
        @crlf +
        '            RockMigrationHelper.AddBlockAttributeValue("'+     
        CONVERT(nvarchar(50), b.Guid)+ '","'+ 
        CONVERT(nvarchar(50), a.Guid)+ '",@"'+ 
        ISNULL(replace(av.Value, '"', '""'),'')+ '");'+
        @crlf
    from [AttributeValue] [av]
    join Block b on b.Id = av.EntityId
    join Attribute a on a.id = av.AttributeId
    left outer join [Page] [p] on [p].[Id] = [b].[PageId]
	left outer join [Layout] [l] on [l].[Id] = [b].[LayoutId]
	left outer join [Layout] [pl] on [pl].[Id] = [p].[LayoutId]
	join [site] [s] on [s].[Id] = [l].[siteId] or [s].[Id] = [pl].[siteId]
    where ([av].[AttributeId] in (select [Id] from #attributeIds) 
          -- also include AttributeValues for non-system/non-shipping blocks
          or ((b.IsSystem = 0 and [b].[Guid] not in (select [Guid] from #knownGuidsToIgnore) ) and a.EntityTypeQualifierColumn = 'BlockTypeId' )

		  -- if enabled, include all block attribute values updated in the last 60 minutes, even if their BlockType Attribute is already IsSystem=1
		  or (@forceIncludeRecentlyUpdatedBlockAttributeValues = 1 and DATEDIFF(minute, SYSDATETIME(),av.ModifiedDateTime) <= 60)
          )
    and [av].[Guid] not in (select [Guid] from #knownGuidsToIgnore) 
    order by b.Id
    
    drop table #attributeIds

    insert into #codeTable
    SELECT @crlf

	-- Field Types
	insert into #codeTable
	SELECT
        '            RockMigrationHelper.UpdateFieldType("'+    
        ft.Name+ '","'+ 
        ISNULL(ft.Description,'')+ '","'+ 
        ft.Assembly+ '","'+ 
        ft.Class+ '","'+ 
        CONVERT(nvarchar(50), ft.Guid)+ '");'+
        @crlf
    from [FieldType] [ft]
    where (ft.IsSystem = 0)

    insert into #codeTable
    SELECT @crlf
    
    -- Page Contexts
    insert into #codeTable
      SELECT '            // Add/Update PageContext for Page:' + p.InternalName + ', Entity: ' + pc.Entity + ', Parameter: ' + pc.IdParameter  
      + @crlf +
      + '            RockMigrationHelper.UpdatePageContext( "' + convert(nvarchar(max), p.Guid) + '", "' + pc.Entity +  '", "' + pc.IdParameter +  '", "' + convert(nvarchar(max), pc.Guid) + '");'
      + @crlf
    FROM [dbo].[PageContext] [pc]
    join [Page] [p]
    on [p].[Id] = [pc].[PageId]
    where [pc].[IsSystem] = 0

    select CodeText [MigrationUp] from #codeTable 
    where REPLACE(CodeText, @crlf, '') != ''
    order by Id
    delete from #codeTable

    -- generate MigrationDown

    insert into #codeTable SELECT         
        '            // Attrib for BlockType: ' + bt.Name + ':'+ a.Name+
        @crlf +
        '            RockMigrationHelper.DeleteAttribute("'+ 
		CONVERT(nvarchar(50), [A].[Guid]) + '");' 
		from [Attribute] [A]
		inner join [EntityType] E 
			ON E.[Id] = A.[EntityTypeId]
		left outer join [BlockType] [bt] on [bt].[Id] = cast([a].[EntityTypeQualifierValue] as int)
		where A.[IsSystem] = 0
        and [A].[Guid] not in (select [Guid] from #knownGuidsToIgnore) 
		and E.[Name] = 'Rock.Model.Block'
		and A.EntityTypeQualifierColumn = 'BlockTypeId'
       
		order by [A].[Id] desc   

    insert into #codeTable
    SELECT @crlf

    insert into #codeTable 
	SELECT 
        '            // Remove Block: ' + b.Name + ', from ' + ISNULL('Page: ' + p.InternalName,'') + ISNULL(', Layout: ' + l.Name, '')  +  ISNULL(', Site: ' + s.Name, '') +
        @crlf + 
		'            RockMigrationHelper.DeleteBlock("'+ CONVERT(nvarchar(50), [b].[Guid])+ '");'
        from [Block] [b]
		left outer join [Page] [p] on [p].[Id] = [b].[PageId]
		left outer join [Layout] [l] on [l].[Id] = [b].[LayoutId]
		left outer join [Layout] [pl] on [pl].[Id] = [p].[LayoutId]
		join [site] [s] on [s].[Id] = [l].[siteId] or [s].[Id] = [pl].[siteId]
		where [b].[IsSystem] = 0
        and [b].[Guid] not in (select [Guid] from #knownGuidsToIgnore) 
		order by [b].[Id] desc

    insert into #codeTable 
	SELECT 
		'            RockMigrationHelper.DeleteBlockType("'+ CONVERT(nvarchar(50), [Guid])+ '"); // '+ 
		[Name] 
	from [BlockType] 
	where [IsSystem] = 0
    and [Guid] not in (select [Guid] from #knownGuidsToIgnore) 
	order by [Id] desc

    insert into #codeTable
    SELECT @crlf

    insert into #codeTable 
	SELECT 
		'            RockMigrationHelper.DeletePage("'+ CONVERT(nvarchar(50), [p].[Guid])+ '"); // ' + ISNULL(' Page: ' + p.InternalName,'') + ISNULL(', Layout: ' + l.Name, '')  +  ISNULL(', Site: ' + s.Name, '') 
	from [Page] [p]
	join [Layout] [l] on [l].[Id] = [p].[layoutId]
	join [site] [s] on [s].[Id] = [l].[siteId]
    where [p].[IsSystem] = 0
    and [p].[Guid] not in (select [Guid] from #knownGuidsToIgnore) 
	order by [p].[Id] desc 

    insert into #codeTable
    SELECT @crlf

    insert into #codeTable
    SELECT 
	'            RockMigrationHelper.DeleteLayout("'+ CONVERT(nvarchar(50), [l].[Guid])+ '"); // '  + ISNULL(' Layout: ' + l.Name, '')  +  ISNULL(', Site: ' + s.Name, '')  
    FROM [Layout] [l]
    join [Site] [s] on [s].[Id] = [l].[SiteId]
    where [l].[IsSystem] = 0
    and [l].[Guid] not in (select [Guid] from #knownGuidsToIgnore) 
    order by [l].[Id] desc

    insert into #codeTable
    SELECT '            // Delete PageContext for Page:' + p.InternalName + ', Entity: ' + pc.Entity + ', Parameter: ' + pc.IdParameter  + @crlf +
    + '            RockMigrationHelper.DeletePageContext( "' + convert(nvarchar(max), pc.Guid) + '");'
    + @crlf  
    FROM [dbo].[PageContext] [pc]
    join [Page] [p]
    on [p].[Id] = [pc].[PageId]
    where [pc].[IsSystem] = 0

    select CodeText [MigrationDown] from #codeTable
    where REPLACE(CodeText, @crlf, '') != ''
    order by Id

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    DROP TABLE #codeTable

IF OBJECT_ID('tempdb..#knownGuidsToIgnore') IS NOT NULL
    DROP TABLE #knownGuidsToIgnore

end