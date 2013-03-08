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
    public partial class StandardizeFieldTypeNames : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

update [FieldType] set [Class] = 'Rock.Field.Types.TextFieldType' where [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA';
update [FieldType] set [Class] = 'Rock.Field.Types.SelectMultiFieldType' where [Guid] = 'BD0D9B57-2A41-4490-89FF-F01DAB7D4904';
update [FieldType] set [Class] = 'Rock.Field.Types.BooleanFieldType' where [Guid] = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';
update [FieldType] set [Class] = 'Rock.Field.Types.ColorFieldType' where [Guid] = 'D747E6AE-C383-4E22-8846-71518E3DD06F';
update [FieldType] set [Class] = 'Rock.Field.Types.SelectSingleFieldType' where [Guid] = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0';
update [FieldType] set [Class] = 'Rock.Field.Types.IntegerFieldType' where [Guid] = 'A75DFC58-7A1B-4799-BF31-451B2BBE38FF';
update [FieldType] set [Class] = 'Rock.Field.Types.PageReferenceFieldType' where [Guid] = 'BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108';
update [FieldType] set [Class] = 'Rock.Field.Types.ImageFieldType' where [Guid] = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D';
update [FieldType] set [Class] = 'Rock.Field.Types.DateFieldType' where [Guid] = '6B6AA175-4758-453F-8D83-FCD8044B5F36';
update [FieldType] set [Class] = 'Rock.Field.Types.VideoFieldType' where [Guid] = 'FA398F9D-5B01-41EA-9A93-112F910A277D';
update [FieldType] set [Class] = 'Rock.Field.Types.CurrencyFieldType' where [Guid] = '50EABC9A-A29D-4A65-984A-87891B230533';
update [FieldType] set [Class] = 'Rock.Field.Types.DecimalFieldType' where [Guid] = 'C757A554-3009-4214-B05D-CEA2B2EA6B8F';
update [FieldType] set [Class] = 'Rock.Field.Types.DefinedTypeFieldType' where [Guid] = 'BC48720C-3610-4BCF-AE66-D255A17F1CDF';
update [FieldType] set [Class] = 'Rock.Field.Types.DefinedValueFieldType' where [Guid] = '59D5A94C-94A0-4630-B80A-BB25697D74C7';
update [FieldType] set [Class] = 'Rock.Field.Types.DocumentFieldType' where [Guid] = '11B0FC7B-6FD9-4D6C-AE41-C8B6EF6D4892';
update [FieldType] set [Class] = 'Rock.Field.Types.PersonFieldType' where [Guid] = 'E4EAB7B2-0B76-429B-AFE4-AD86D7428C70';
update [FieldType] set [Class] = 'Rock.Field.Types.UrlFieldType' where [Guid] = '85B95F22-587B-4968-851D-9196FA1FA03F';
update [FieldType] set [Class] = 'Rock.Field.Types.EntityTypeFieldType' where [Guid] = '3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB';
update [FieldType] set [Class] = 'Rock.Field.Types.KeyValueListFieldType' where [Guid] = '0BFEA28A-811E-49F8-AAD5-1DBF2046CCF3';


update [FieldType] set [Class] = 'Rock.Field.Types.HtmlFieldType' where [Guid] = 'DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF';
update [FieldType] set [Class] = 'Rock.Field.Types.MemoFieldType' where [Guid] = 'C28C7BF3-A552-4D77-9408-DEDCF760CED0';
update [FieldType] set [Class] = 'Rock.Field.Types.GroupTypesFieldType' where [Guid] = 'F725B854-A15E-46AE-9D4C-0608D4154F1E';
update [FieldType] set [Class] = 'Rock.Field.Types.GroupFieldType' where [Guid] = 'F4399CEF-827B-48B2-A735-F7806FCFE8E8';
update [FieldType] set [Class] = 'Rock.Field.Types.AudiencePrimarySecondaryFieldType' where [Guid] = 'AFFC7F00-CED0-4C07-8140-A1F400DABA63';
update [FieldType] set [Class] = 'Rock.Field.Types.AudiencesFieldType' where [Guid] = 'CEC19E37-1CE6-469A-B863-C5BFE558658D';
update [FieldType] set [Class] = 'Rock.Field.Types.CampusesFieldType' where [Guid] = '69254F91-C97F-4C2D-9ACB-1683B088097B';
update [FieldType] set [Class] = 'Rock.Field.Types.MarketingCampaignAdTypesFieldType' where [Guid] = 'F61722B7-CD11-4FA2-85E2-0D711616253A';
update [FieldType] set [Class] = 'Rock.Field.Types.MarketingCampaignAdImageAttributeNameFieldType' where [Guid] = '10E0786E-3202-400D-AFB6-6A8A8DDD2040';
update [FieldType] set [Class] = 'Rock.Field.Types.EmailTemplateFieldType' where [Guid] = '90E51D85-DF2F-451B-BFEC-B2E4100CDCB2';
update [FieldType] set [Class] = 'Rock.Field.Types.PersonFieldType' where [Guid] = 'C042C6B4-B7A7-441F-85D0-A7EFCEDF5771';
update [FieldType] set [Class] = 'Rock.Field.Types.FileFieldType' where [Guid] = '40E5B4E8-AD3E-481E-BDAE-D940C4048565';

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"

update [FieldType] set [Class] = 'Rock.Field.Types.Text' where [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA';
update [FieldType] set [Class] = 'Rock.Field.Types.SelectMulti' where [Guid] = 'BD0D9B57-2A41-4490-89FF-F01DAB7D4904';
update [FieldType] set [Class] = 'Rock.Field.Types.Boolean' where [Guid] = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';
update [FieldType] set [Class] = 'Rock.Field.Types.Color' where [Guid] = 'D747E6AE-C383-4E22-8846-71518E3DD06F';
update [FieldType] set [Class] = 'Rock.Field.Types.SelectSingle' where [Guid] = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0';
update [FieldType] set [Class] = 'Rock.Field.Types.Integer' where [Guid] = 'A75DFC58-7A1B-4799-BF31-451B2BBE38FF';
update [FieldType] set [Class] = 'Rock.Field.Types.PageReference' where [Guid] = 'BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108';
update [FieldType] set [Class] = 'Rock.Field.Types.Image' where [Guid] = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D';
update [FieldType] set [Class] = 'Rock.Field.Types.Date' where [Guid] = '6B6AA175-4758-453F-8D83-FCD8044B5F36';
update [FieldType] set [Class] = 'Rock.Field.Types.Video' where [Guid] = 'FA398F9D-5B01-41EA-9A93-112F910A277D';
update [FieldType] set [Class] = 'Rock.Field.Types.Currency' where [Guid] = '50EABC9A-A29D-4A65-984A-87891B230533';
update [FieldType] set [Class] = 'Rock.Field.Types.Decimal' where [Guid] = 'C757A554-3009-4214-B05D-CEA2B2EA6B8F';
update [FieldType] set [Class] = 'Rock.Field.Types.DefinedType' where [Guid] = 'BC48720C-3610-4BCF-AE66-D255A17F1CDF';
update [FieldType] set [Class] = 'Rock.Field.Types.DefinedValue' where [Guid] = '59D5A94C-94A0-4630-B80A-BB25697D74C7';
update [FieldType] set [Class] = 'Rock.Field.Types.Document' where [Guid] = '11B0FC7B-6FD9-4D6C-AE41-C8B6EF6D4892';
update [FieldType] set [Class] = 'Rock.Field.Types.Person' where [Guid] = 'E4EAB7B2-0B76-429B-AFE4-AD86D7428C70';
update [FieldType] set [Class] = 'Rock.Field.Types.Url' where [Guid] = '85B95F22-587B-4968-851D-9196FA1FA03F';
update [FieldType] set [Class] = 'Rock.Field.Types.EntityType' where [Guid] = '3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB';
update [FieldType] set [Class] = 'Rock.Field.Types.KeyValueList' where [Guid] = '0BFEA28A-811E-49F8-AAD5-1DBF2046CCF3';


update [FieldType] set [Class] = 'Rock.Field.Types.HtmlField' where [Guid] = 'DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF';
update [FieldType] set [Class] = 'Rock.Field.Types.MemoField' where [Guid] = 'C28C7BF3-A552-4D77-9408-DEDCF760CED0';
update [FieldType] set [Class] = 'Rock.Field.Types.GroupTypesField' where [Guid] = 'F725B854-A15E-46AE-9D4C-0608D4154F1E';
update [FieldType] set [Class] = 'Rock.Field.Types.GroupField' where [Guid] = 'F4399CEF-827B-48B2-A735-F7806FCFE8E8';
update [FieldType] set [Class] = 'Rock.Field.Types.AudiencePrimarySecondaryField' where [Guid] = 'AFFC7F00-CED0-4C07-8140-A1F400DABA63';
update [FieldType] set [Class] = 'Rock.Field.Types.AudiencesField' where [Guid] = 'CEC19E37-1CE6-469A-B863-C5BFE558658D';
update [FieldType] set [Class] = 'Rock.Field.Types.CampusesField' where [Guid] = '69254F91-C97F-4C2D-9ACB-1683B088097B';
update [FieldType] set [Class] = 'Rock.Field.Types.MarketingCampaignAdTypesField' where [Guid] = 'F61722B7-CD11-4FA2-85E2-0D711616253A';
update [FieldType] set [Class] = 'Rock.Field.Types.MarketingCampaignAdImageAttributeNameField' where [Guid] = '10E0786E-3202-400D-AFB6-6A8A8DDD2040';
update [FieldType] set [Class] = 'Rock.Field.Types.EmailTemplateField' where [Guid] = '90E51D85-DF2F-451B-BFEC-B2E4100CDCB2';
update [FieldType] set [Class] = 'Rock.Field.Types.PersonField' where [Guid] = 'C042C6B4-B7A7-441F-85D0-A7EFCEDF5771';
update [FieldType] set [Class] = 'Rock.Field.Types.FileField' where [Guid] = '40E5B4E8-AD3E-481E-BDAE-D940C4048565';

" );
        }
    }
}
