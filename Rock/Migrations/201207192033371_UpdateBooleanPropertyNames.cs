namespace Rock.Migrations
    
    using System.Data.Entity.Migrations;
#pragma warning disable 1591
    public partial class UpdateBooleanPropertyNames : DbMigration
        
        public override void Up()
            
            RenameColumn( "cmsBlock", "System", "IsSystem" );
            RenameColumn( "cmsBlockInstance", "System", "IsSystem" );
            RenameColumn( "cmsFile", "System", "IsSystem" );
            RenameColumn( "cmsFile", "Temporary", "IsTemporary" );
            RenameColumn( "cmsHtmlContent", "Approved", "IsApproved" );
            RenameColumn( "cmsPage", "System", "IsSystem" );
            RenameColumn( "cmsPageRoute", "System", "IsSystem" );
            RenameColumn( "cmsSite", "System", "IsSystem" );
            RenameColumn( "cmsSiteDomain", "System", "IsSystem" );
            RenameColumn( "coreAttribute", "GridColumn", "IsGridColumn" );
            RenameColumn( "coreAttribute", "MultiValue", "IsMultiValue" );
            RenameColumn( "coreAttribute", "Required", "IsRequired" );
            RenameColumn( "coreAttribute", "System", "IsSystem" );
            RenameColumn( "coreAttributeQualifier", "System", "IsSystem" );
            RenameColumn( "coreAttributeValue", "System", "IsSystem" );
            RenameColumn( "coreDefinedType", "System", "IsSystem" );
            RenameColumn( "coreDefinedValue", "System", "IsSystem" );
            RenameColumn( "coreFieldType", "System", "IsSystem" );
            RenameColumn( "crmCampus", "System", "IsSystem" );
            RenameColumn( "crmEmailTemplate", "System", "IsSystem" );
            RenameColumn( "crmPerson", "EmailIsActive", "IsEmailActive" );
            RenameColumn( "crmPerson", "System", "IsSystem" );
            RenameColumn( "crmPhoneNumber", "System", "IsSystem" );
            RenameColumn( "financialBatch", "Closed", "IsClosed" );
            RenameColumn( "financialFund", "Active", "IsActive" );
            RenameColumn( "financialFund", "Pledgable", "IsPledgable" );
            RenameColumn( "financialFund", "TaxDeductible", "IsTaxDeductible" );
            RenameColumn( "groupsGroup", "System", "IsSystem" );
            RenameColumn( "groupsGroupRole", "System", "IsSystem" );
            RenameColumn( "groupsGroupType", "System", "IsSystem" );
            RenameColumn( "groupsMember", "System", "IsSystem" );
            RenameColumn( "utilJob", "Active", "IsActive" );
            RenameColumn( "utilJob", "System", "IsSystem" );
        }

        public override void Down()
            
            RenameColumn( "cmsBlock", "IsSystem", "System" );
            RenameColumn( "cmsBlockInstance", "IsSystem", "System" );
            RenameColumn( "cmsFile", "IsSystem", "System" );
            RenameColumn( "cmsFile", "IsTemporary", "Temporary" );
            RenameColumn( "cmsHtmlContent", "IsApproved", "Approved" );
            RenameColumn( "cmsPage", "IsSystem", "System" );
            RenameColumn( "cmsPageRoute", "IsSystem", "System" );
            RenameColumn( "cmsSite", "IsSystem", "System" );
            RenameColumn( "cmsSiteDomain", "IsSystem", "System" );
            RenameColumn( "coreAttribute", "IsGridColumn", "GridColumn" );
            RenameColumn( "coreAttribute", "IsMultiValue", "MultiValue" );
            RenameColumn( "coreAttribute", "IsRequired", "Required" );
            RenameColumn( "coreAttribute", "IsSystem", "System" );
            RenameColumn( "coreAttributeQualifier", "IsSystem", "System" );
            RenameColumn( "coreAttributeValue", "IsSystem", "System" );
            RenameColumn( "coreDefinedType", "IsSystem", "System" );
            RenameColumn( "coreDefinedValue", "IsSystem", "System" );
            RenameColumn( "coreFieldType", "IsSystem", "System" );
            RenameColumn( "crmCampus", "IsSystem", "System" );
            RenameColumn( "crmEmailTemplate", "IsSystem", "System" );
            RenameColumn( "crmPerson", "IsEmailActive", "EmailIsActive" );
            RenameColumn( "crmPerson", "IsSystem", "System" );
            RenameColumn( "crmPhoneNumber", "IsSystem", "System" );
            RenameColumn( "financialBatch", "IsClosed", "Closed" );
            RenameColumn( "financialFund", "IsActive", "Active" );
            RenameColumn( "financialFund", "IsPledgable", "Pledgable" );
            RenameColumn( "financialFund", "IsTaxDeductible", "TaxDeductible" );
            RenameColumn( "groupsGroup", "IsSystem", "System" );
            RenameColumn( "groupsGroupRole", "IsSystem", "System" );
            RenameColumn( "groupsGroupType", "IsSystem", "System" );
            RenameColumn( "groupsMember", "IsSystem", "System" );
            RenameColumn( "utilJob", "IsActive", "Active" );
            RenameColumn( "utilJob", "IsSystem", "System" );
        }
    }
}
