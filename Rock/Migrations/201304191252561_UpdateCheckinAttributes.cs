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
    public partial class UpdateCheckinAttributes : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                DECLARE @BlockTypeId int

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '3B5FBE9A-2904-4220-92F3-47DD16E805C0')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'E3A99534-6FD9-49AD-AC52-32D53B2CEDD7')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '6B050E12-A232-41F6-94C5-B190F4520607')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '34B48E0F-5E37-425E-9588-E612ED34DB03')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'
                
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '7E20E97E-63F2-413D-9C2C-16FF34023F70')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '933418C1-448E-4825-8D3D-BDE23E968483')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'D2348D51-B13A-4069-97AD-369D9615A711')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '18911F1B-294E-48D6-9E6B-0F72BF6C9491')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '0DF27F26-691D-41F8-B0F7-987E4FEC375C')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%PageUrl'
" );

            // Admin
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "D46F3099-5700-4CCD-8B6C-F1F306BA02B8" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "36C334EF-E723-4065-9C39-BD5663582751" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "7675AE35-1A61-460E-8FF6-B2A5C473F319" );
            AddBlockAttributeValue( "87E1FCDA-41B9-4F1C-A910-BAC2918BE6DB", "D46F3099-5700-4CCD-8B6C-F1F306BA02B8", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "87E1FCDA-41B9-4F1C-A910-BAC2918BE6DB", "36C334EF-E723-4065-9C39-BD5663582751", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "87E1FCDA-41B9-4F1C-A910-BAC2918BE6DB", "7675AE35-1A61-460E-8FF6-B2A5C473F319", "7B7207D0-B905-4836-800E-A24DDC6FE445" );

            // Welcome
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "E7F05E05-3FAA-4332-9E06-2D69F35CA6D7" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "11D60BFC-383E-452D-8DC3-6575B54D8D23" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "7F4B3918-25F4-4F36-B7BA-645AA8DA7F47" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Family Select Page", "FamilySelectPage", "", "", 0, "", "A637E55F-830E-49AE-8924-E4103E6B9DB2" );
            AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "E7F05E05-3FAA-4332-9E06-2D69F35CA6D7", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "11D60BFC-383E-452D-8DC3-6575B54D8D23", "D47858C0-0E6E-46DC-AE99-8EC84BA5F45F" );
            AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "7F4B3918-25F4-4F36-B7BA-645AA8DA7F47", "7B7207D0-B905-4836-800E-A24DDC6FE445" );
            AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "A637E55F-830E-49AE-8924-E4103E6B9DB2", "10C97379-F719-4ACB-B8C6-651957B660A4" );

            // Search
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "23EA5174-A5D8-4161-94C4-F70AB827FCF1" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "5B25560D-2CA2-422B-89B1-928D17005CD3" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "17D71A46-A1D4-4CF3-9353-5339E487BA75" );
            AddBlockAttributeValue( "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "23EA5174-A5D8-4161-94C4-F70AB827FCF1", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "5B25560D-2CA2-422B-89B1-928D17005CD3", "10C97379-F719-4ACB-B8C6-651957B660A4" );
            AddBlockAttributeValue( "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "17D71A46-A1D4-4CF3-9353-5339E487BA75", "432B615A-75FF-4B14-9C99-3E769F866950" );

            // Family Select
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "47EE6878-36A1-4A55-A634-584ADD852822" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "90ECD00A-9570-4986-B32F-02F32B656A2A" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "2578A0C1-3236-409D-AECE-154C98429628" );
            AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "47EE6878-36A1-4A55-A634-584ADD852822", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "90ECD00A-9570-4986-B32F-02F32B656A2A", "BB8CF87F-680F-48F9-9147-F4951E033D17" );
            AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "2578A0C1-3236-409D-AECE-154C98429628", "D47858C0-0E6E-46DC-AE99-8EC84BA5F45F" );

            // Person Select
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "97433068-DC3F-461A-AAFB-3DF83B1E3B2F" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "F680429D-A228-43FE-A54E-927F95ACC030" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "A3F0C33B-F380-46AF-BCDA-100F18F8889E" );
            AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "97433068-DC3F-461A-AAFB-3DF83B1E3B2F", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "F680429D-A228-43FE-A54E-927F95ACC030", "60E3EA1F-FD6B-4F0E-9C72-A9960E13427C" );
            AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "A3F0C33B-F380-46AF-BCDA-100F18F8889E", "10C97379-F719-4ACB-B8C6-651957B660A4" );

            // Group Type Select
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "98DBFE23-80D5-47EB-AE3F-5381C024F23D" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "F3D66EC8-E1CF-4C28-B55A-C1F49E4633A0" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "39D260A5-A976-4DA9-B3E0-7381E9B8F3D5" );
            AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "98DBFE23-80D5-47EB-AE3F-5381C024F23D", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "F3D66EC8-E1CF-4C28-B55A-C1F49E4633A0", "043BB717-5799-446F-B8DA-30E575110B0C" );
            AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "39D260A5-A976-4DA9-B3E0-7381E9B8F3D5", "BB8CF87F-680F-48F9-9147-F4951E033D17" );

            // Location Select
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "051CEDD9-2FB5-4873-8FBB-B5F5671EF044" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "39246677-8451-4422-B384-C7AD9DA6C649" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "569E033B-A2D5-4C15-8CD5-7F1336C22871" );
            AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "051CEDD9-2FB5-4873-8FBB-B5F5671EF044", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "39246677-8451-4422-B384-C7AD9DA6C649", "6F0CB22B-E05B-42F1-A329-9219E81F6C34" );
            AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "569E033B-A2D5-4C15-8CD5-7F1336C22871", "60E3EA1F-FD6B-4F0E-9C72-A9960E13427C" );

            // Group Select
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "C318262A-1B9D-4B54-B2CC-3971F4E8636F" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "E4F7B489-39B8-49F9-8C8C-533275FAACDF" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "795530E8-9395-4360-99B6-376A4BF40C5A" );
            AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "C318262A-1B9D-4B54-B2CC-3971F4E8636F", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "E4F7B489-39B8-49F9-8C8C-533275FAACDF", "C0AFA081-B64E-4006-BFFC-A350A51AE4CC" );
            AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "795530E8-9395-4360-99B6-376A4BF40C5A", "043BB717-5799-446F-B8DA-30E575110B0C" );

            // Time Select
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "D5AFB471-3EE2-44D5-BC66-F4EFD26FD394" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "840898DB-A9AB-45C9-9894-0A1E816EFC4C" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "DE808D50-0861-4E24-A483-F1C74C1FFDE8" );
            AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "D5AFB471-3EE2-44D5-BC66-F4EFD26FD394", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "840898DB-A9AB-45C9-9894-0A1E816EFC4C", "E08230B8-35A4-40D6-A0BB-521418314DA9" );
            AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "DE808D50-0861-4E24-A483-F1C74C1FFDE8", "6F0CB22B-E05B-42F1-A329-9219E81F6C34" );

            // Success
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "C7B04F35-8244-4370-B2D3-DFEAC0E5F367" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "1E497EDB-4EC9-4EC5-9511-68C3EF37703B" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "81538773-80CF-421B-84F4-E12CE7630FC5" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Select Page", "PersonSelectPage", "", "", 0, "", "C0FED78A-88FF-482A-9C57-B020694FDFAA" );
            AddBlockAttributeValue( "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "C7B04F35-8244-4370-B2D3-DFEAC0E5F367", "432B615A-75FF-4B14-9C99-3E769F866950" );
            AddBlockAttributeValue( "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "1E497EDB-4EC9-4EC5-9511-68C3EF37703B", "E08230B8-35A4-40D6-A0BB-521418314DA9" );
            AddBlockAttributeValue( "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "81538773-80CF-421B-84F4-E12CE7630FC5", "C0AFA081-B64E-4006-BFFC-A350A51AE4CC" );
            AddBlockAttributeValue( "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "C0FED78A-88FF-482A-9C57-B020694FDFAA", "BB8CF87F-680F-48F9-9147-F4951E033D17" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"

                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                DECLARE @BlockTypeId int

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '3B5FBE9A-2904-4220-92F3-47DD16E805C0')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'E3A99534-6FD9-49AD-AC52-32D53B2CEDD7')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '6B050E12-A232-41F6-94C5-B190F4520607')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '34B48E0F-5E37-425E-9588-E612ED34DB03')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'
                
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '7E20E97E-63F2-413D-9C2C-16FF34023F70')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '933418C1-448E-4825-8D3D-BDE23E968483')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'D2348D51-B13A-4069-97AD-369D9615A711')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '18911F1B-294E-48D6-9E6B-0F72BF6C9491')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'

                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '0DF27F26-691D-41F8-B0F7-987E4FEC375C')
                DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar) AND [Key] LIKE '%Page'
" );

            // Admin
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "D46F3099-5700-4CCD-8B6C-F1F306BA02B8" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "36C334EF-E723-4065-9C39-BD5663582751" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "7675AE35-1A61-460E-8FF6-B2A5C473F319" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "3BB20D7C-EC22-4D26-905F-0B53E3C19C1F" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "1E281BDC-DD22-4A87-9AC3-D5A29D6EF2C2" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "637CD12E-6CA3-4F9E-8C71-5BAADAABFB1A" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "284281A6-13EB-4D9B-87B0-373A5141299F" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "ED8F8C44-21F1-45EE-9462-A041DE5BB37A" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "F6334979-2433-4A99-BE13-9B0C1F18C298" );
            AddBlockTypeAttribute( "3B5FBE9A-2904-4220-92F3-47DD16E805C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "CDB7C8F6-3EE9-4207-8DCA-DDDCA4A32DC8" );
            
            // Welcome
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "E7F05E05-3FAA-4332-9E06-2D69F35CA6D7" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "11D60BFC-383E-452D-8DC3-6575B54D8D23" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "7F4B3918-25F4-4F36-B7BA-645AA8DA7F47" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "A637E55F-830E-49AE-8924-E4103E6B9DB2" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "CAB91DC0-3B3C-4A95-8C3A-B17F5B80C1E2" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "ACCB94A4-18A8-4D7E-892C-430963967EB6" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "BD661E59-4042-4458-A9B8-CB6B3D72103C" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "751B2A77-0926-481C-8642-E1D5A50E302B" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "E4A57FB7-46DD-4AC6-8EDC-109665658ADA" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "59FA474A-BA42-4055-8BB5-14975F6E2065" );
            
            // Search
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "23EA5174-A5D8-4161-94C4-F70AB827FCF1" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "5B25560D-2CA2-422B-89B1-928D17005CD3" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "17D71A46-A1D4-4CF3-9353-5339E487BA75" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "300B1FDC-85C2-4A42-BB16-65E57C3558F5" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "EC562A5F-25B1-46E1-9CC7-E6754540D2B0" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "79AD8D63-BD07-48E1-B1B7-A2783D3A113C" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "A50B6B17-A6DB-4435-AF1D-C05F4E09CC99" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "A2DE836C-6D79-4963-8A9F-B3B75F03ADA3" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "11D53557-54E3-4043-B30C-D77BA8343FD4" );
            AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "F67A94EC-36AD-440D-B43F-0B23B8F6A6AE" );
            
            // Family Select
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "47EE6878-36A1-4A55-A634-584ADD852822" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "90ECD00A-9570-4986-B32F-02F32B656A2A" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "2578A0C1-3236-409D-AECE-154C98429628" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "F5E619A3-06A5-4030-88C2-A5E20FC25E9E" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "F60A443E-6594-43D2-8A0A-29F8C804AB47" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "69E86233-6488-43D4-961A-381BEA843064" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "F9FF3AFC-1862-4BF9-8091-B560E038BC99" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "BB690FA6-843E-48CE-A5F6-FCF3C331F8D7" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "3C153051-256F-409D-A80C-70F7A3FFFA04" );
            AddBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "9E5B276D-F5DC-48F1-A57A-161185B10DF4" );
            
            // Person Select
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "97433068-DC3F-461A-AAFB-3DF83B1E3B2F" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "F680429D-A228-43FE-A54E-927F95ACC030" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "A3F0C33B-F380-46AF-BCDA-100F18F8889E" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "B582F9B3-4AFF-49A2-84D3-BD412E5BD228" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "682C0B02-67A5-42A0-91DE-3D261334F41D" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "15692693-284F-488A-922C-B7E420124135" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "2652E79E-84AF-4939-B44D-5B5279ECF0ED" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "6F6BE210-50D2-4C7C-A696-7EB633EFCD91" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "B1722771-B3B8-4E43-A0F7-D75D9520EE17" );
            AddBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "27D50ABD-DBCB-44A5-B895-376C5D25213D" );
            
            // Group Type Select
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "98DBFE23-80D5-47EB-AE3F-5381C024F23D" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "F3D66EC8-E1CF-4C28-B55A-C1F49E4633A0" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "39D260A5-A976-4DA9-B3E0-7381E9B8F3D5" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "A93DF365-D13A-4FFF-BE44-D6B030BED97B" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "3EE61DB6-0BAE-4500-9FB3-5CFCBBA1CCA4" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "594E0986-9E2F-4DD7-9EEC-9AE2AD9DA3C1" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "0F758F9E-674D-4BA4-9F13-940B0448A408" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "848151D1-7E3F-4848-B2F9-0C2A1CFAD1C3" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "1995A297-566B-49F8-9351-D4CFFD0D876D" );
            AddBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "EEFE7EBC-814B-4C2C-A01C-39D0FA1674F6" );
           
            // Location Select
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "051CEDD9-2FB5-4873-8FBB-B5F5671EF044" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "39246677-8451-4422-B384-C7AD9DA6C649" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "569E033B-A2D5-4C15-8CD5-7F1336C22871" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "81663DB7-7856-4387-B297-BB73AE188248" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "92605E4B-C736-4EE9-B9C8-3EEDF2F460E8" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "FBDB3CFB-8330-4396-8EB4-025793755CFE" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "6355BCDC-07A4-46BC-A2CC-2332EFB5E5F7" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "DD2DECE5-B718-436C-9E7C-F1F3A9EFFCE6" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "687F8772-B417-41C8-822B-2DA5EBBD7D0B" );
            AddBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "155A3B81-3EBA-43A1-B12E-DADBAEECDF0B" );
            
            // Group Select
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "C318262A-1B9D-4B54-B2CC-3971F4E8636F" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "E4F7B489-39B8-49F9-8C8C-533275FAACDF" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "795530E8-9395-4360-99B6-376A4BF40C5A" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "1539EDD0-4B3F-47AF-9ED2-AD0650432893" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "E649B2BD-4EED-4FBC-9DE8-5A6C8C85CAFF" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "4B8CBDE5-FAD4-4B80-8571-635A5470486C" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "057A26FC-0C5E-40E0-80BF-7EC46C929D8C" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "01F1B628-AC5B-41E7-8851-11CC7F283578" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "D4FE2DB3-3F7E-404B-878D-B00CC06C3432" );
            AddBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "D7A73A41-F11E-44EA-942B-058FE9F85C8A" );
            
            // Time Select
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "D5AFB471-3EE2-44D5-BC66-F4EFD26FD394" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "840898DB-A9AB-45C9-9894-0A1E816EFC4C" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "DE808D50-0861-4E24-A483-F1C74C1FFDE8" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "9A955A2D-2F87-45DE-95EC-F493618FBE35" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "1C854CCA-00BD-4DF4-A80F-A6B80B1032B5" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "850F9009-58CE-4212-8729-15DF8D6557C3" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "4536BF02-0B1D-43C5-8711-5D556940053A" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "E2EC80FA-DB21-4604-A4EF-3701579B2126" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "FB3C4F01-0C95-461E-9634-8865EFF23FD8" );
            AddBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "1E436CB6-E8B1-4659-A0C4-0769A37211A3" );
            
            // Success
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "", "The url of the check-in admin page", 0, "~/checkin", "C7B04F35-8244-4370-B2D3-DFEAC0E5F367" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "", "The url of the check-in welcome page", 1, "~/checkin/welcome", "1E497EDB-4EC9-4EC5-9511-68C3EF37703B" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "", "The url of the check-in search page", 2, "~/checkin/search", "81538773-80CF-421B-84F4-E12CE7630FC5" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "", "The url of the check-in family select page", 3, "~/checkin/family", "C0FED78A-88FF-482A-9C57-B020694FDFAA" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "", "The url of the check-in person select page", 4, "~/checkin/person", "37089679-E78C-4223-9E87-327B67BD0EDD" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "", "The url of the check-in group type select page", 5, "~/checkin/grouptype", "53A90267-14CE-4F96-8CD1-E4F03B564DEE" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "", "The url of the check-in location select page", 6, "~/checkin/location", "31E74B57-9057-4272-A0F2-A60925C6F286" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "", "The url of the check-in group select page", 7, "~/checkin/group", "E3EFAEEA-8000-44C3-81B9-9F52DF0DFCD7" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "", "The url of the check-in time select page", 8, "~/checkin/time", "F0D97313-BAEA-4C27-98D6-0D669AFEC168" );
            AddBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "", "The url of the check-in success page", 9, "~/checkin/success", "1C5D8786-C52F-436B-8BFB-68B4E439C354" );
            
        }
    }
}
