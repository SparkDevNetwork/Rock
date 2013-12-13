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
    public partial class UpdateItemsToIsSystem : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
Update [Layout] set [IsSystem] = 1 where [Guid] = 'D65F783D-87A9-4CC9-8110-E83466A0EADB'
Update [Layout] set [IsSystem] = 1 where [Guid] = '195BCD57-1C10-4969-886F-7324B6287B75'
Update [Layout] set [IsSystem] = 1 where [Guid] = '0CB60906-6B74-44FD-AB25-026050EF70EB'
Update [Layout] set [IsSystem] = 1 where [Guid] = '6AC471A3-9B0E-459B-ADA2-F6E18F970803'
Update [Layout] set [IsSystem] = 1 where [Guid] = '22D220B5-0D34-429A-B9E3-59D80AE423E7'
Update [Layout] set [IsSystem] = 1 where [Guid] = 'BACA6FF2-A228-4C47-9577-2BBDFDFD26BA'
Update [Layout] set [IsSystem] = 1 where [Guid] = 'EDFE06F4-D329-4340-ACD8-68A60CD112E6'

Update [Page] set [IsSystem] = 1 where [Guid] = '5A676DCC-37F0-4624-8CCD-408A5A471D8A'
Update [Page] set [IsSystem] = 1 where [Guid] = '75130E27-405A-4935-AB27-0EDE11F6E8B3'

Update [BlockType] set [IsSystem] = 1 where [Guid] = '82A285C1-0D6B-41E0-B1AA-DD356021BDBF'
Update [BlockType] set [IsSystem] = 1 where [Guid] = 'B71FE9F2-0F90-497F-90FA-5A6148E8E116'
Update [BlockType] set [IsSystem] = 1 where [Guid] = '15572974-DD86-43C8-BBBF-5181EE76E2C9'
Update [BlockType] set [IsSystem] = 1 where [Guid] = '850A0541-D31A-4559-94D1-9DAD5F52EFDF'
Update [BlockType] set [IsSystem] = 1 where [Guid] = 'DCD63280-B661-48AA-8DEB-F5ED63C7AB77'
Update [BlockType] set [IsSystem] = 1 where [Guid] = '87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E'

Update [Block] set [IsSystem] = 1 where [Guid] = 'FF4E50B9-C8DD-4D8A-93A0-3F9E9B2AD254'
Update [Block] set [IsSystem] = 1 where [Guid] = '600D3412-28DD-4B85-AA99-2BF4A4F51FB3'
Update [Block] set [IsSystem] = 1 where [Guid] = '9BBF6F1D-261F-4E95-8652-1C34BD42C1A8'
Update [Block] set [IsSystem] = 1 where [Guid] = '0AEA6787-83F5-4981-B7B2-2A75721E3304'
Update [Block] set [IsSystem] = 1 where [Guid] = '61D575A2-1AC0-4A53-93E6-97AEE8540F5D'
Update [Block] set [IsSystem] = 1 where [Guid] = 'C46B5260-4320-474C-AAA7-D9B92064148C'
Update [Block] set [IsSystem] = 1 where [Guid] = '82AF461F-022D-4ADB-BB12-F220CD605459'
Update [Block] set [IsSystem] = 1 where [Guid] = '897E01AA-AD50-4D8D-BF9A-A848978A4022'
Update [Block] set [IsSystem] = 1 where [Guid] = '791A6AA0-D498-4795-BB5F-21609175826F'
Update [Block] set [IsSystem] = 1 where [Guid] = '36DD17C4-26D0-46C6-98FC-F7D2A5808C17'
Update [Block] set [IsSystem] = 1 where [Guid] = 'B266CE0F-DA0F-4D05-B785-1604B08F0E49'
Update [Block] set [IsSystem] = 1 where [Guid] = '2356DEDC-803F-4782-A8E9-D0D88393EC2E'
Update [Block] set [IsSystem] = 1 where [Guid] = 'FF319A20-396E-4BF2-BB84-A32339E8549E'
Update [Block] set [IsSystem] = 1 where [Guid] = 'DF44F3EA-2575-44E4-BE22-BC5AA2582C94'
Update [Block] set [IsSystem] = 1 where [Guid] = 'D90B2EBE-6D8D-4401-B9D8-931686EC6C4A'
Update [Block] set [IsSystem] = 1 where [Guid] = '64CE4B01-8C16-422C-8A76-46A9D43DDDF9'
Update [Block] set [IsSystem] = 1 where [Guid] = '2BE43272-DDB7-41DB-B77F-250A7E39A51F'
Update [Block] set [IsSystem] = 1 where [Guid] = 'DF0BFC68-512B-44DC-BE50-32C6684C2ABE'
Update [Block] set [IsSystem] = 1 where [Guid] = '373DE813-5080-491B-BCB6-AAECEA87B27B'
Update [Block] set [IsSystem] = 1 where [Guid] = 'EE504D50-6E6B-4230-A44B-7FC74194C133'
Update [Block] set [IsSystem] = 1 where [Guid] = 'E235CF09-35AF-4999-BA3A-014EE9983BAC'
Update [Block] set [IsSystem] = 1 where [Guid] = 'FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7'
Update [Block] set [IsSystem] = 1 where [Guid] = '7869E018-832D-455D-A493-D8B5C3B32D5D'
Update [Block] set [IsSystem] = 1 where [Guid] = '3E15815E-EFC0-4963-AF21-7E5AC173A885'
Update [Block] set [IsSystem] = 1 where [Guid] = '82929B59-7072-4066-91F6-E93CA7D94132'
Update [Block] set [IsSystem] = 1 where [Guid] = '62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB'
Update [Block] set [IsSystem] = 1 where [Guid] = '9382B285-3EF6-47F7-94BB-A47C498196A3'
Update [Block] set [IsSystem] = 1 where [Guid] = '6A648E77-ABA9-4AAF-A8BB-027A12261ED9'
Update [Block] set [IsSystem] = 1 where [Guid] = 'C79CD534-170C-4996-9B58-D7707C0CCFD9'
Update [Block] set [IsSystem] = 1 where [Guid] = 'FFD36984-B067-4402-8248-655A5801AA7C'
Update [Block] set [IsSystem] = 1 where [Guid] = 'AD9B2C35-AAC9-4560-82A8-04EE95784435'
Update [Block] set [IsSystem] = 1 where [Guid] = '33CA8857-F6C9-454C-B3C8-C974C0B42359'

Update [Attribute] set [IsSystem] = 1 where [Guid] = '49E4751F-9BBB-42DD-97B0-FA7DD2F50ADA'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '5340120D-B914-4689-B915-9C25865A3637'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '6E17FAE8-9EE9-49AC-9CC1-E4CECB2F730D'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '521E95E3-286C-4B6A-BB7D-A7DBD803C81D'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'EC43CF32-3BDF-4544-8B6A-CE9208DD7C81'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '992F693A-1019-468C-B7A7-B945A616BAF0'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'C2A1F6A2-A801-4B52-8CFB-9B394CCA2D17'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'EBBC10ED-18E4-4E7D-9467-E7C27F12A745'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '50DF7B49-FAF4-45D5-919F-14E589B37666'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '5FD00163-5EDC-4E36-93D3-D917EFDEF63B'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '169D731D-D7EE-42E6-9D5B-62E33E847A16'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '7182EBDD-8128-46E7-8635-D6C664E15F63'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'BC0D6B30-C82C-4FB8-B9DC-1C44184812B1'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '8A241135-19B3-4085-A26B-C762570B2333'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'F8F56AE0-2D67-493D-8902-4057401B3DCC'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '07C47FA5-4A1A-4B70-AA1B-BD042120233C'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'A1DD3409-4CD2-40CE-B64F-C9DD13123CA3'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'D2193E02-7E23-4EE3-8576-4559D0DA4BA9'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '23B72E87-2A99-406E-BFAF-578D2A993FF3'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'A7536B0E-C1C0-4F4B-941B-DCE1B2A427D1'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '140EE974-ED4D-4392-A84C-68717B2B903F'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '18117AFC-03FC-410E-BE46-E58BDC7DE388'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'C60C77D1-7BA1-4BF8-9C5C-D9A1C57499D0'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'FE64D72C-EA41-4C4E-9F0C-48048EEAB8A1'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'EAF2435D-FACE-40F2-832D-CDB5A4D51BF3'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '91C20C04-5F3B-4904-B8F7-47BEE8E0BAFF'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '9DF52F7A-E122-4205-83DF-F205367DD3D9'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '315BCFA9-AAB5-41D1-BC23-BCB14A6EBC25'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '909F86D7-C3BD-4202-99C0-4C73B0D29AC9'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '5BA2BC6A-1305-42CB-8894-DC3F4D2B6405'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'D85C2AAD-3635-46C5-8BBC-48C5A32F4AF0'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'A76E4E46-291E-4989-9FF2-5616528D5FD1'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '50AD37A6-3CAC-4278-976A-A566B71FB6AF'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '24C1C4A2-3823-4BFC-8BDA-D35C6B904E1C'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'FCDD819E-B566-4ED5-8252-BCD37ED043C1'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '2A790D3A-3DB2-49C6-A218-87CD629397FA'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '2EF904CD-976E-4489-8C18-9BA43885ACD9'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '075679A0-A811-47BC-B19C-1052374F9436'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '15192E4B-D2C7-4949-84C1-5D1D65EA98FB'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '42A9404C-835C-469C-AD85-D77573F76C3D'
Update [Attribute] set [IsSystem] = 1 where [Guid] = '6D54E02C-EE3A-4E5C-A0FA-42D96DB7A779'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'E5391623-81A4-4BC6-AA6B-81B1BE88655E'
Update [Attribute] set [IsSystem] = 1 where [Guid] = 'EB24B424-2E36-4769-BE92-64B9673AC469'

Update [AttributeValue] set [IsSystem] = 1 where [Guid] = 'F8E2F017-77C8-4C5F-A272-9F3603AD6933'
Update [AttributeValue] set [IsSystem] = 1 where [Guid] = 'E13BB299-A65E-45D8-ABA5-2C974CAD6A49'
Update [AttributeValue] set [IsSystem] = 1 where [Guid] = '498DE7AE-3373-4A27-AC66-FCA2AD3500D5'
Update [AttributeValue] set [IsSystem] = 1 where [Guid] = 'D5DBE6EB-B6BB-46AE-BFB7-DC050D440E0E'
Update [AttributeValue] set [IsSystem] = 1 where [Guid] = 'B8AC4F78-4EF6-432D-8A1E-1D7B767EAAE9'
" );

            // Attrib for BlockType: Utility - Category Tree View:Title
            AddBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to put in the panel header", 0, "", "99CC0287-DD53-4796-8FE8-04862DA1BA39" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Utility - Category Tree View:Title
            DeleteAttribute( "99CC0287-DD53-4796-8FE8-04862DA1BA39" );
            
            Sql( @"
Update [Layout] set [IsSystem] = 0 where [Guid] = 'D65F783D-87A9-4CC9-8110-E83466A0EADB'
Update [Layout] set [IsSystem] = 0 where [Guid] = '195BCD57-1C10-4969-886F-7324B6287B75'
Update [Layout] set [IsSystem] = 0 where [Guid] = '0CB60906-6B74-44FD-AB25-026050EF70EB'
Update [Layout] set [IsSystem] = 0 where [Guid] = '6AC471A3-9B0E-459B-ADA2-F6E18F970803'
Update [Layout] set [IsSystem] = 0 where [Guid] = '22D220B5-0D34-429A-B9E3-59D80AE423E7'
Update [Layout] set [IsSystem] = 0 where [Guid] = 'BACA6FF2-A228-4C47-9577-2BBDFDFD26BA'
Update [Layout] set [IsSystem] = 0 where [Guid] = 'EDFE06F4-D329-4340-ACD8-68A60CD112E6'

Update [Page] set [IsSystem] = 0 where [Guid] = '5A676DCC-37F0-4624-8CCD-408A5A471D8A'
Update [Page] set [IsSystem] = 0 where [Guid] = '75130E27-405A-4935-AB27-0EDE11F6E8B3'

Update [BlockType] set [IsSystem] = 0 where [Guid] = '82A285C1-0D6B-41E0-B1AA-DD356021BDBF'
Update [BlockType] set [IsSystem] = 0 where [Guid] = 'B71FE9F2-0F90-497F-90FA-5A6148E8E116'
Update [BlockType] set [IsSystem] = 0 where [Guid] = '15572974-DD86-43C8-BBBF-5181EE76E2C9'
Update [BlockType] set [IsSystem] = 0 where [Guid] = '850A0541-D31A-4559-94D1-9DAD5F52EFDF'
Update [BlockType] set [IsSystem] = 0 where [Guid] = 'DCD63280-B661-48AA-8DEB-F5ED63C7AB77'
Update [BlockType] set [IsSystem] = 0 where [Guid] = '87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E'

Update [Block] set [IsSystem] = 0 where [Guid] = 'FF4E50B9-C8DD-4D8A-93A0-3F9E9B2AD254'
Update [Block] set [IsSystem] = 0 where [Guid] = '600D3412-28DD-4B85-AA99-2BF4A4F51FB3'
Update [Block] set [IsSystem] = 0 where [Guid] = '9BBF6F1D-261F-4E95-8652-1C34BD42C1A8'
Update [Block] set [IsSystem] = 0 where [Guid] = '0AEA6787-83F5-4981-B7B2-2A75721E3304'
Update [Block] set [IsSystem] = 0 where [Guid] = '61D575A2-1AC0-4A53-93E6-97AEE8540F5D'
Update [Block] set [IsSystem] = 0 where [Guid] = 'C46B5260-4320-474C-AAA7-D9B92064148C'
Update [Block] set [IsSystem] = 0 where [Guid] = '82AF461F-022D-4ADB-BB12-F220CD605459'
Update [Block] set [IsSystem] = 0 where [Guid] = '897E01AA-AD50-4D8D-BF9A-A848978A4022'
Update [Block] set [IsSystem] = 0 where [Guid] = '791A6AA0-D498-4795-BB5F-21609175826F'
Update [Block] set [IsSystem] = 0 where [Guid] = '36DD17C4-26D0-46C6-98FC-F7D2A5808C17'
Update [Block] set [IsSystem] = 0 where [Guid] = 'B266CE0F-DA0F-4D05-B785-1604B08F0E49'
Update [Block] set [IsSystem] = 0 where [Guid] = '2356DEDC-803F-4782-A8E9-D0D88393EC2E'
Update [Block] set [IsSystem] = 0 where [Guid] = 'FF319A20-396E-4BF2-BB84-A32339E8549E'
Update [Block] set [IsSystem] = 0 where [Guid] = 'DF44F3EA-2575-44E4-BE22-BC5AA2582C94'
Update [Block] set [IsSystem] = 0 where [Guid] = 'D90B2EBE-6D8D-4401-B9D8-931686EC6C4A'
Update [Block] set [IsSystem] = 0 where [Guid] = '64CE4B01-8C16-422C-8A76-46A9D43DDDF9'
Update [Block] set [IsSystem] = 0 where [Guid] = '2BE43272-DDB7-41DB-B77F-250A7E39A51F'
Update [Block] set [IsSystem] = 0 where [Guid] = 'DF0BFC68-512B-44DC-BE50-32C6684C2ABE'
Update [Block] set [IsSystem] = 0 where [Guid] = '373DE813-5080-491B-BCB6-AAECEA87B27B'
Update [Block] set [IsSystem] = 0 where [Guid] = 'EE504D50-6E6B-4230-A44B-7FC74194C133'
Update [Block] set [IsSystem] = 0 where [Guid] = 'E235CF09-35AF-4999-BA3A-014EE9983BAC'
Update [Block] set [IsSystem] = 0 where [Guid] = 'FCFDFA6B-4E3D-40D8-86F7-F25F2F4833C7'
Update [Block] set [IsSystem] = 0 where [Guid] = '7869E018-832D-455D-A493-D8B5C3B32D5D'
Update [Block] set [IsSystem] = 0 where [Guid] = '3E15815E-EFC0-4963-AF21-7E5AC173A885'
Update [Block] set [IsSystem] = 0 where [Guid] = '82929B59-7072-4066-91F6-E93CA7D94132'
Update [Block] set [IsSystem] = 0 where [Guid] = '62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB'
Update [Block] set [IsSystem] = 0 where [Guid] = '9382B285-3EF6-47F7-94BB-A47C498196A3'
Update [Block] set [IsSystem] = 0 where [Guid] = '6A648E77-ABA9-4AAF-A8BB-027A12261ED9'
Update [Block] set [IsSystem] = 0 where [Guid] = 'C79CD534-170C-4996-9B58-D7707C0CCFD9'
Update [Block] set [IsSystem] = 0 where [Guid] = 'FFD36984-B067-4402-8248-655A5801AA7C'
Update [Block] set [IsSystem] = 0 where [Guid] = 'AD9B2C35-AAC9-4560-82A8-04EE95784435'
Update [Block] set [IsSystem] = 0 where [Guid] = '33CA8857-F6C9-454C-B3C8-C974C0B42359'

Update [Attribute] set [IsSystem] = 0 where [Guid] = '49E4751F-9BBB-42DD-97B0-FA7DD2F50ADA'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '5340120D-B914-4689-B915-9C25865A3637'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '6E17FAE8-9EE9-49AC-9CC1-E4CECB2F730D'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '521E95E3-286C-4B6A-BB7D-A7DBD803C81D'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'EC43CF32-3BDF-4544-8B6A-CE9208DD7C81'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '992F693A-1019-468C-B7A7-B945A616BAF0'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'C2A1F6A2-A801-4B52-8CFB-9B394CCA2D17'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'EBBC10ED-18E4-4E7D-9467-E7C27F12A745'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '50DF7B49-FAF4-45D5-919F-14E589B37666'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '5FD00163-5EDC-4E36-93D3-D917EFDEF63B'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '169D731D-D7EE-42E6-9D5B-62E33E847A16'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '7182EBDD-8128-46E7-8635-D6C664E15F63'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'BC0D6B30-C82C-4FB8-B9DC-1C44184812B1'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '8A241135-19B3-4085-A26B-C762570B2333'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'F8F56AE0-2D67-493D-8902-4057401B3DCC'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '07C47FA5-4A1A-4B70-AA1B-BD042120233C'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'A1DD3409-4CD2-40CE-B64F-C9DD13123CA3'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'D2193E02-7E23-4EE3-8576-4559D0DA4BA9'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '23B72E87-2A99-406E-BFAF-578D2A993FF3'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'A7536B0E-C1C0-4F4B-941B-DCE1B2A427D1'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '140EE974-ED4D-4392-A84C-68717B2B903F'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '18117AFC-03FC-410E-BE46-E58BDC7DE388'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'C60C77D1-7BA1-4BF8-9C5C-D9A1C57499D0'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'FE64D72C-EA41-4C4E-9F0C-48048EEAB8A1'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'EAF2435D-FACE-40F2-832D-CDB5A4D51BF3'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '91C20C04-5F3B-4904-B8F7-47BEE8E0BAFF'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '9DF52F7A-E122-4205-83DF-F205367DD3D9'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '315BCFA9-AAB5-41D1-BC23-BCB14A6EBC25'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '909F86D7-C3BD-4202-99C0-4C73B0D29AC9'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '5BA2BC6A-1305-42CB-8894-DC3F4D2B6405'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'D85C2AAD-3635-46C5-8BBC-48C5A32F4AF0'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'A76E4E46-291E-4989-9FF2-5616528D5FD1'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '50AD37A6-3CAC-4278-976A-A566B71FB6AF'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '24C1C4A2-3823-4BFC-8BDA-D35C6B904E1C'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'FCDD819E-B566-4ED5-8252-BCD37ED043C1'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '2A790D3A-3DB2-49C6-A218-87CD629397FA'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '2EF904CD-976E-4489-8C18-9BA43885ACD9'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '075679A0-A811-47BC-B19C-1052374F9436'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '15192E4B-D2C7-4949-84C1-5D1D65EA98FB'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '42A9404C-835C-469C-AD85-D77573F76C3D'
Update [Attribute] set [IsSystem] = 0 where [Guid] = '6D54E02C-EE3A-4E5C-A0FA-42D96DB7A779'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'E5391623-81A4-4BC6-AA6B-81B1BE88655E'
Update [Attribute] set [IsSystem] = 0 where [Guid] = 'EB24B424-2E36-4769-BE92-64B9673AC469'

Update [AttributeValue] set [IsSystem] = 0 where [Guid] = 'F8E2F017-77C8-4C5F-A272-9F3603AD6933'
Update [AttributeValue] set [IsSystem] = 0 where [Guid] = 'E13BB299-A65E-45D8-ABA5-2C974CAD6A49'
Update [AttributeValue] set [IsSystem] = 0 where [Guid] = '498DE7AE-3373-4A27-AC66-FCA2AD3500D5'
Update [AttributeValue] set [IsSystem] = 0 where [Guid] = 'D5DBE6EB-B6BB-46AE-BFB7-DC050D440E0E'
Update [AttributeValue] set [IsSystem] = 0 where [Guid] = 'B8AC4F78-4EF6-432D-8A1E-1D7B767EAAE9'
" );
        }
    }
}
