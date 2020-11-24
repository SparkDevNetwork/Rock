using Rock.Plugin;

namespace com.bemaservices.GroupTools
{
    [MigrationNumber( 8, "1.9.4" )]
    public class RegistrationAttributes : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var groupTypeGuid = "50FCFB30-F51A-49DF-86F4-2B176EA1820B";
            var groupTypeId = SqlScalar( "Select Top 1 [Id] From GroupType Where Guid = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'" ).ToString();
            var lifeGroupTypeGuidObject = SqlScalar( "Select Top 1 [Guid] From GroupType Where Guid = 'a4f16049-2525-426e-a6e8-cdfb7b198664'" );
            if ( lifeGroupTypeGuidObject != null )
            {
                groupTypeGuid = lifeGroupTypeGuidObject.ToString();
                groupTypeId = SqlScalar( "Select Top 1 [Id] From GroupType Where Guid = 'a4f16049-2525-426e-a6e8-cdfb7b198664'" ).ToString();
            }

            RockMigrationHelper.AddGroupTypeGroupAttribute( groupTypeGuid, Rock.SystemGuid.FieldType.URL_LINK, "Registration Link", @"", 8, "", "47241C0F-CCA3-4707-B4CF-273104984BCB", "RegistrationLink" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( groupTypeGuid, Rock.SystemGuid.FieldType.URL_LINK, "Childcare Registration Link", @"", 9, "", "C206AF62-2946-4393-9784-B8439D1894BC", "ChildcareRegistrationLink" );

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "75A7DCFB-AF3C-492A-81EB-F48BAF606BBB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Link", "RegistrationLink", "", 7, @"", "B32D12B4-8514-4C57-AEA3-A3E5619123E5", false ); // Group Registration:Registration Link
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "75A7DCFB-AF3C-492A-81EB-F48BAF606BBB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Childcare Registration Link", "ChildcareRegistrationLink", "", 8, @"", "4D4118BD-186B-440F-960F-8B9F2788C429", false ); // Group Registration:Childcare Registration Link

            RockMigrationHelper.AddAttributeQualifier( "B32D12B4-8514-4C57-AEA3-A3E5619123E5", "ispassword", @"False", "A494732F-711B-4ABD-97E0-2B47A00BC65F" ); // Group Registration:Registration Link:ispassword
            RockMigrationHelper.AddAttributeQualifier( "B32D12B4-8514-4C57-AEA3-A3E5619123E5", "maxcharacters", @"", "F0C60929-02DB-46B5-B134-B40AAB716927" ); // Group Registration:Registration Link:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "B32D12B4-8514-4C57-AEA3-A3E5619123E5", "showcountdown", @"False", "A039F62D-2BF7-4872-94F7-9BA0255DBC34" ); // Group Registration:Registration Link:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "4D4118BD-186B-440F-960F-8B9F2788C429", "ispassword", @"False", "B0A76779-C14C-4C12-9A14-5B87E9E71CA2" ); // Group Registration:Childcare Registration Link:ispassword
            RockMigrationHelper.AddAttributeQualifier( "4D4118BD-186B-440F-960F-8B9F2788C429", "maxcharacters", @"", "DE988402-6249-453F-A531-5471DF01B078" ); // Group Registration:Childcare Registration Link:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "4D4118BD-186B-440F-960F-8B9F2788C429", "showcountdown", @"False", "2805EEB7-F98D-4035-B972-892AAC593CCB" ); // Group Registration:Childcare Registration Link:showcountdown
            RockMigrationHelper.UpdateWorkflowActionForm( @"{% include '~/Plugins/com_bemaservices/GroupTools/Assets/Lava/GroupDetail.lava' %}", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your information has been submitted successfully.|", "", false, "", "3AF3F318-3F2F-4C99-BACD-119A1F142F35" ); // Group Registration:Start:Form
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3AF3F318-3F2F-4C99-BACD-119A1F142F35", "B32D12B4-8514-4C57-AEA3-A3E5619123E5", 7, false, true, false, false, @"", @"", "134F9E89-1ACB-4E8E-81B6-AB7A2C0236D9" ); // Group Registration:Start:Form:Registration Link
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3AF3F318-3F2F-4C99-BACD-119A1F142F35", "4D4118BD-186B-440F-960F-8B9F2788C429", 8, false, true, false, false, @"", @"", "3C593AEA-DDD2-416B-BC3E-079F93416416" ); // Group Registration:Start:Form:Childcare Registration Link
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Set Registration Link", 0, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "BCF3ED9B-4CE6-46B8-ACA1-84D89E596580" ); // Group Registration:Start:Set Registration Link
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Set Childcare Registration Link", 1, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "0636DCF0-6F30-4B39-A921-4A4F0F22EDE3" ); // Group Registration:Start:Set Childcare Registration Link
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Set Person to Current Person", 2, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "AFCB3C45-1E0E-4305-840D-03430BEC0DA0" ); // Group Registration:Start:Set Person to Current Person
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Set First Name", 3, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "04A2157A-ACC8-4409-B2A0-55D2BA2E9412", 64, "", "4DD5666C-87BC-42DB-AF07-F46365FE2602" ); // Group Registration:Start:Set First Name
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Set Last Name", 4, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "04A2157A-ACC8-4409-B2A0-55D2BA2E9412", 64, "", "3CF03806-935F-43C5-90B7-4E4681BC2D49" ); // Group Registration:Start:Set Last Name
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Set Email", 5, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "04A2157A-ACC8-4409-B2A0-55D2BA2E9412", 64, "", "7C03D8A8-BED6-4D57-8FFD-82D8F46CE3C6" ); // Group Registration:Start:Set Email
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Set Phone Number", 6, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "04A2157A-ACC8-4409-B2A0-55D2BA2E9412", 64, "", "BB75155B-17E5-4E9C-8AC7-150F35F5F96E" ); // Group Registration:Start:Set Phone Number
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Display Registration Html", 7, "497746B4-8B0C-4D7B-9AF7-B42CEDD6C37C", true, true, "", "B32D12B4-8514-4C57-AEA3-A3E5619123E5", 64, "", "CC3EAC91-1CD0-4E25-8892-4CCBF09BF77B" ); // Group Registration:Start:Display Registration Html
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Form", 8, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "3AF3F318-3F2F-4C99-BACD-119A1F142F35", "B32D12B4-8514-4C57-AEA3-A3E5619123E5", 32, "", "6BEF7D69-971F-4DD0-9FC1-239B8E043A2B" ); // Group Registration:Start:Form
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Show Childcare Registration Html", 9, "497746B4-8B0C-4D7B-9AF7-B42CEDD6C37C", true, false, "", "4D4118BD-186B-440F-960F-8B9F2788C429", 64, "", "B82B864F-EF81-4DE2-AB3B-E9DC3BC24E3E" ); // Group Registration:Start:Show Childcare Registration Html
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Get Person From Fields", 10, "E5E7CA24-7030-4D48-9C39-04B5809E71A8", true, false, "", "", 1, "", "B27D7891-86B6-427B-9D65-BBD9F2F27361" ); // Group Registration:Start:Get Person From Fields
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Add Person to Group", 11, "BD53F375-78A2-4A54-B1D1-2D805F3FCD44", true, false, "", "", 32, "", "621CA59C-24F0-4011-B819-F851C2CA6B28" ); // Group Registration:Start:Add Person to Group
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Set Group Member Status", 12, "F09E2C90-5FB1-4236-B146-848B983CC3A8", true, false, "", "", 64, "", "90AA3071-B8B2-4744-8FC4-DAD2266B0B68" ); // Group Registration:Start:Set Group Member Status
            RockMigrationHelper.UpdateWorkflowActionType( "54894C02-6CC1-49BA-97D0-56832BC2B0BD", "Set Preferred Contact Method", 13, "5A0885C7-2D6F-4F04-BBE1-78AE61108C51", true, true, "", "", 1, "", "61D3F9A4-61F2-439F-88E2-92ABEFB17594" ); // Group Registration:Start:Set Preferred Contact Method
            RockMigrationHelper.AddActionTypeAttributeValue( "BCF3ED9B-4CE6-46B8-ACA1-84D89E596580", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign group = Workflow | Attribute:'Group','Object' %}{{group | Attribute:'RegistrationLink','RawValue'}}" ); // Group Registration:Start:Set Registration Link:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "BCF3ED9B-4CE6-46B8-ACA1-84D89E596580", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Group Registration:Start:Set Registration Link:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BCF3ED9B-4CE6-46B8-ACA1-84D89E596580", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"b32d12b4-8514-4c57-aea3-a3e5619123e5" ); // Group Registration:Start:Set Registration Link:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0636DCF0-6F30-4B39-A921-4A4F0F22EDE3", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign group = Workflow | Attribute:'Group','Object' %}{{group | Attribute:'ChildcareRegistrationLink','RawValue'}}" ); // Group Registration:Start:Set Childcare Registration Link:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "0636DCF0-6F30-4B39-A921-4A4F0F22EDE3", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Group Registration:Start:Set Childcare Registration Link:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0636DCF0-6F30-4B39-A921-4A4F0F22EDE3", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"4d4118bd-186b-440f-960f-8b9f2788c429" ); // Group Registration:Start:Set Childcare Registration Link:Attribute

            RockMigrationHelper.AddActionTypeAttributeValue( "CC3EAC91-1CD0-4E25-8892-4CCBF09BF77B", "640FBD13-FEEB-4313-B6AC-6E5CF6E005DF", @"{% include '~/Plugins/com_bemaservices/GroupTools/Assets/Lava/GroupDetail.lava' %}
<div class=""actions"">
    <a href=""{{ Workflow | Attribute:'RegistrationLink' }}"" class=""btn btn-primary"">Register</a> 
</div>" ); // Group Registration:Start:Display Registration Html:HTML
            RockMigrationHelper.AddActionTypeAttributeValue( "CC3EAC91-1CD0-4E25-8892-4CCBF09BF77B", "05673872-1E8D-42CD-9517-7CAFBC6976F9", @"False" ); // Group Registration:Start:Display Registration Html:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CC3EAC91-1CD0-4E25-8892-4CCBF09BF77B", "46ACD91A-9455-41D2-8849-C2305F364418", @"True" ); // Group Registration:Start:Display Registration Html:Hide Status Message
            RockMigrationHelper.AddActionTypeAttributeValue( "6BEF7D69-971F-4DD0-9FC1-239B8E043A2B", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Group Registration:Start:Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B82B864F-EF81-4DE2-AB3B-E9DC3BC24E3E", "640FBD13-FEEB-4313-B6AC-6E5CF6E005DF", @"<div class='alert alert-success'>Your information has been submitted successfully. Click <a href=""{{Workflow | Attribute:'ChildcareRegistrationLink' }}"">here</a> to register for childcare.</div>" ); // Group Registration:Start:Show Childcare Registration Html:HTML
            RockMigrationHelper.AddActionTypeAttributeValue( "B82B864F-EF81-4DE2-AB3B-E9DC3BC24E3E", "05673872-1E8D-42CD-9517-7CAFBC6976F9", @"False" ); // Group Registration:Start:Show Childcare Registration Html:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B82B864F-EF81-4DE2-AB3B-E9DC3BC24E3E", "46ACD91A-9455-41D2-8849-C2305F364418", @"True" ); // Group Registration:Start:Show Childcare Registration Html:Hide Status Message


        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "C206AF62-2946-4393-9784-B8439D1894BC" );
            RockMigrationHelper.DeleteAttribute( "47241C0F-CCA3-4707-B4CF-273104984BCB" );
        }
    }
}

