namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// 
    /// </summary>
    public partial class CreateDatabase : DbMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.cmsAuth",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityType = c.String(nullable: false, maxLength: 200),
                        EntityId = c.Int(),
                        Order = c.Int(nullable: false),
                        Action = c.String(nullable: false, maxLength: 50),
                        AllowOrDeny = c.String(nullable: false, maxLength: 1),
                        SpecialRole = c.Int(nullable: false),
                        PersonId = c.Int(),
                        GroupId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .ForeignKey("groupsGroup", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId)
                .Index(t => t.GroupId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.crmPerson",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        RecordTypeId = c.Int(),
                        RecordStatusId = c.Int(),
                        RecordStatusReasonId = c.Int(),
                        PersonStatusId = c.Int(),
                        TitleId = c.Int(),
                        GivenName = c.String(maxLength: 50),
                        NickName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        SuffixId = c.Int(),
                        PhotoId = c.Int(),
                        BirthDay = c.Int(),
                        BirthMonth = c.Int(),
                        BirthYear = c.Int(),
                        Gender = c.Int(),
                        MaritalStatusId = c.Int(),
                        AnniversaryDate = c.DateTime(),
                        GraduationDate = c.DateTime(),
                        Email = c.String(maxLength: 75),
                        EmailIsActive = c.Boolean(),
                        EmailNote = c.String(maxLength: 250),
                        DoNotEmail = c.Boolean(nullable: false),
                        SystemNote = c.String(maxLength: 1000),
                        ViewedCount = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("coreDefinedValue", t => t.MaritalStatusId)
                .ForeignKey("coreDefinedValue", t => t.PersonStatusId)
                .ForeignKey("coreDefinedValue", t => t.RecordStatusId)
                .ForeignKey("coreDefinedValue", t => t.RecordStatusReasonId)
                .ForeignKey("coreDefinedValue", t => t.RecordTypeId)
                .ForeignKey("coreDefinedValue", t => t.SuffixId)
                .ForeignKey("coreDefinedValue", t => t.TitleId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.MaritalStatusId)
                .Index(t => t.PersonStatusId)
                .Index(t => t.RecordStatusId)
                .Index(t => t.RecordStatusReasonId)
                .Index(t => t.RecordTypeId)
                .Index(t => t.SuffixId)
                .Index(t => t.TitleId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.cmsUser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 255),
                        AuthenticationType = c.Int(nullable: false),
                        Password = c.String(nullable: false, maxLength: 128),
                        IsConfirmed = c.Boolean(),
                        LastActivityDate = c.DateTime(),
                        LastLoginDate = c.DateTime(),
                        LastPasswordChangedDate = c.DateTime(),
                        CreationDate = c.DateTime(),
                        IsOnLine = c.Boolean(),
                        IsLockedOut = c.Boolean(),
                        LastLockedOutDate = c.DateTime(),
                        FailedPasswordAttemptCount = c.Int(),
                        FailedPasswordAttemptWindowStart = c.DateTime(),
                        ApiKey = c.String(maxLength: 50),
                        PersonId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.PersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.crmEmailTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        PersonId = c.Int(),
                        Category = c.String(maxLength: 100),
                        Title = c.String(nullable: false, maxLength: 100),
                        From = c.String(maxLength: 200),
                        To = c.String(),
                        Cc = c.String(),
                        Bcc = c.String(),
                        Subject = c.String(nullable: false, maxLength: 200),
                        Body = c.String(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.PersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.crmPhoneNumber",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        PersonId = c.Int(nullable: false),
                        Number = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.PersonId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.PersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.groupsMember",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        GroupId = c.Int(nullable: false),
                        PersonId = c.Int(nullable: false),
                        GroupRoleId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .ForeignKey("groupsGroup", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("groupsGroupRole", t => t.GroupRoleId)
                .Index(t => t.PersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId)
                .Index(t => t.GroupId)
                .Index(t => t.GroupRoleId);
            
            CreateTable(
                "dbo.groupsGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        ParentGroupId = c.Int(),
                        GroupTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsSecurityRole = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .ForeignKey("groupsGroup", t => t.ParentGroupId)
                .ForeignKey("groupsGroupType", t => t.GroupTypeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId)
                .Index(t => t.ParentGroupId)
                .Index(t => t.GroupTypeId);
            
            CreateTable(
                "dbo.groupsGroupType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        DefaultGroupRoleId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .ForeignKey("groupsGroupRole", t => t.DefaultGroupRoleId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId)
                .Index(t => t.DefaultGroupRoleId);
            
            CreateTable(
                "dbo.groupsGroupRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        Name = c.String(maxLength: 100),
                        Description = c.String(nullable: false),
                        Order = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.financialPledge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(),
                        FundId = c.Int(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        FrequencyTypeId = c.Int(),
                        FrequencyAmount = c.Decimal(precision: 18, scale: 2),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.PersonId)
                .ForeignKey("financialFund", t => t.FundId)
                .ForeignKey("coreDefinedValue", t => t.FrequencyTypeId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.PersonId)
                .Index(t => t.FundId)
                .Index(t => t.FrequencyTypeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.financialFund",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        PublicName = c.String(maxLength: 50),
                        Description = c.String(maxLength: 250),
                        ParentFundId = c.Int(),
                        TaxDeductible = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Pledgable = c.Boolean(nullable: false),
                        GlCode = c.String(maxLength: 50),
                        FundTypeId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.Int(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("financialFund", t => t.ParentFundId)
                .ForeignKey("coreDefinedValue", t => t.FundTypeId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.ParentFundId)
                .Index(t => t.FundTypeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.coreDefinedValue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        DefinedTypeId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("coreDefinedType", t => t.DefinedTypeId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.DefinedTypeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.coreDefinedType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        FieldTypeId = c.Int(),
                        Order = c.Int(nullable: false),
                        Category = c.String(maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("coreFieldType", t => t.FieldTypeId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.FieldTypeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.coreFieldType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Assembly = c.String(nullable: false, maxLength: 100),
                        Class = c.String(nullable: false, maxLength: 100),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.coreAttribute",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        FieldTypeId = c.Int(nullable: false),
                        Entity = c.String(maxLength: 50),
                        EntityQualifierColumn = c.String(maxLength: 50),
                        EntityQualifierValue = c.String(maxLength: 200),
                        Key = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 100),
                        Category = c.String(maxLength: 100),
                        Description = c.String(),
                        Order = c.Int(nullable: false),
                        GridColumn = c.Boolean(nullable: false),
                        DefaultValue = c.String(),
                        MultiValue = c.Boolean(nullable: false),
                        Required = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("coreFieldType", t => t.FieldTypeId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.FieldTypeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.coreAttributeQualifier",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        AttributeId = c.Int(nullable: false),
                        Key = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 100),
                        Value = c.String(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("coreAttribute", t => t.AttributeId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.AttributeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.coreAttributeValue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        AttributeId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        Order = c.Int(),
                        Value = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("coreAttribute", t => t.AttributeId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.AttributeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.financialTransactionFund",
                c => new
                    {
                        TransactionId = c.Int(nullable: false),
                        FundId = c.Int(nullable: false),
                        Amount = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.TransactionId, t.FundId })
                .ForeignKey("financialTransaction", t => t.TransactionId, cascadeDelete: true)
                .ForeignKey("financialFund", t => t.FundId, cascadeDelete: true)
                .Index(t => t.TransactionId)
                .Index(t => t.FundId);
            
            CreateTable(
                "dbo.financialTransaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(maxLength: 250),
                        TransactionDate = c.DateTime(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.Int(),
                        BatchId = c.Int(),
                        CurrencyTypeId = c.Int(),
                        CreditCardTypeId = c.Int(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RefundTransactionId = c.Int(),
                        TransactionImageId = c.Int(),
                        TransactionCode = c.String(maxLength: 50),
                        GatewayId = c.Int(),
                        SourceTypeId = c.Int(),
                        Summary = c.String(maxLength: 500),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("financialBatch", t => t.BatchId)
                .ForeignKey("coreDefinedValue", t => t.CurrencyTypeId)
                .ForeignKey("coreDefinedValue", t => t.CreditCardTypeId)
                .ForeignKey("financialGateway", t => t.GatewayId)
                .ForeignKey("coreDefinedValue", t => t.SourceTypeId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.BatchId)
                .Index(t => t.CurrencyTypeId)
                .Index(t => t.CreditCardTypeId)
                .Index(t => t.GatewayId)
                .Index(t => t.SourceTypeId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.financialBatch",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        BatchDate = c.DateTime(),
                        Closed = c.Boolean(nullable: false),
                        CampusId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.Int(),
                        ForeignReference = c.String(maxLength: 50),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.financialGateway",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Description = c.String(maxLength: 500),
                        ApiUrl = c.String(maxLength: 100),
                        ApiKey = c.String(maxLength: 100),
                        ApiSecret = c.String(maxLength: 100),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.financialTransactionDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Summary = c.String(maxLength: 500),
                        ModifiedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("financialTransaction", t => t.TransactionId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.TransactionId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.fiancialPersonAccountLookup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(),
                        Account = c.String(maxLength: 50),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.PersonId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.cmsBlock",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        Path = c.String(nullable: false, maxLength: 200),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.cmsBlockInstance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        PageId = c.Int(),
                        Layout = c.String(maxLength: 100),
                        BlockId = c.Int(nullable: false),
                        Zone = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        OutputCacheDuration = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("cmsBlock", t => t.BlockId, cascadeDelete: true)
                .ForeignKey("cmsPage", t => t.PageId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.BlockId)
                .Index(t => t.PageId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.cmsHtmlContent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlockId = c.Int(nullable: false),
                        EntityValue = c.String(maxLength: 200),
                        Version = c.Int(nullable: false),
                        Content = c.String(nullable: false),
                        Approved = c.Boolean(nullable: false),
                        ApprovedByPersonId = c.Int(),
                        ApprovedDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        StartDateTime = c.DateTime(),
                        ExpireDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("cmsBlockInstance", t => t.BlockId, cascadeDelete: true)
                .ForeignKey("crmPerson", t => t.ApprovedByPersonId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.BlockId)
                .Index(t => t.ApprovedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.cmsPage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Title = c.String(maxLength: 100),
                        System = c.Boolean(nullable: false),
                        ParentPageId = c.Int(),
                        SiteId = c.Int(),
                        Layout = c.String(maxLength: 100),
                        RequiresEncryption = c.Boolean(nullable: false),
                        EnableViewState = c.Boolean(nullable: false),
                        MenuDisplayDescription = c.Boolean(nullable: false),
                        MenuDisplayIcon = c.Boolean(nullable: false),
                        MenuDisplayChildPages = c.Boolean(nullable: false),
                        DisplayInNavWhen = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        OutputCacheDuration = c.Int(nullable: false),
                        Description = c.String(),
                        IncludeAdminFooter = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        IconUrl = c.String(maxLength: 150),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("cmsPage", t => t.ParentPageId)
                .ForeignKey("cmsSite", t => t.SiteId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.ParentPageId)
                .Index(t => t.SiteId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.cmsPageRoute",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        PageId = c.Int(nullable: false),
                        Route = c.String(nullable: false, maxLength: 200),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("cmsPage", t => t.PageId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.PageId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.cmsSite",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Theme = c.String(maxLength: 100),
                        DefaultPageId = c.Int(),
                        FaviconUrl = c.String(maxLength: 150),
                        AppleTouchIconUrl = c.String(maxLength: 150),
                        FacebookAppId = c.String(maxLength: 25),
                        FacebookAppSecret = c.String(maxLength: 50),
                        LoginPageReference = c.String(maxLength: 10),
                        RegistrationPageReference = c.String(maxLength: 10),
                        ErrorPage = c.String(maxLength: 200),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("cmsPage", t => t.DefaultPageId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.DefaultPageId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.cmsSiteDomain",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(nullable: false),
                        SiteId = c.Int(nullable: false),
                        Domain = c.String(nullable: false, maxLength: 200),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("cmsSite", t => t.SiteId)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.SiteId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.cmsFile",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Temporary = c.Boolean(nullable: false),
                        System = c.Boolean(nullable: false),
                        Data = c.Binary(),
                        Url = c.String(maxLength: 255),
                        FileName = c.String(nullable: false, maxLength: 255),
                        MimeType = c.String(nullable: false, maxLength: 255),
                        Description = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.coreEntityChange",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChangeSet = c.Guid(nullable: false),
                        ChangeType = c.String(nullable: false, maxLength: 10),
                        EntityType = c.String(nullable: false, maxLength: 100),
                        EntityId = c.Int(nullable: false),
                        Property = c.String(nullable: false, maxLength: 100),
                        OriginalValue = c.String(),
                        CurrentValue = c.String(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .Index(t => t.CreatedByPersonId);
            
            CreateTable(
                "dbo.coreExceptionLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentId = c.Int(),
                        SiteId = c.Int(),
                        PageId = c.Int(),
                        ExceptionDate = c.DateTime(nullable: false),
                        HasInnerException = c.Boolean(),
                        PersonId = c.Int(),
                        StatusCode = c.String(maxLength: 10),
                        ExceptionType = c.String(maxLength: 150),
                        Description = c.String(),
                        Source = c.String(maxLength: 50),
                        StackTrace = c.String(),
                        PageUrl = c.String(maxLength: 250),
                        ServerVariables = c.String(),
                        QueryString = c.String(),
                        Form = c.String(),
                        Cookies = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.PersonId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.coreServiceLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Time = c.DateTime(),
                        Input = c.String(),
                        Type = c.String(maxLength: 50),
                        Name = c.String(maxLength: 50),
                        Result = c.String(maxLength: 50),
                        Success = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.crmAddress",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Raw = c.String(maxLength: 400),
                        Street1 = c.String(maxLength: 100),
                        Street2 = c.String(maxLength: 100),
                        City = c.String(maxLength: 50),
                        State = c.String(maxLength: 50),
                        Country = c.String(maxLength: 50),
                        Zip = c.String(maxLength: 10),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        StandardizeAttempt = c.DateTime(),
                        StandardizeService = c.String(maxLength: 50),
                        StandardizeResult = c.String(maxLength: 50),
                        StandardizeDate = c.DateTime(),
                        GeocodeAttempt = c.DateTime(),
                        GeocodeService = c.String(maxLength: 50),
                        GeocodeResult = c.String(maxLength: 50),
                        GeocodeDate = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.crmPersonTrail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CurrentId = c.Int(nullable: false),
                        CurrentGuid = c.Guid(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.crmPersonViewed",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ViewerPersonId = c.Int(),
                        TargetPersonId = c.Int(),
                        ViewDateTime = c.DateTime(),
                        IpAddress = c.String(maxLength: 25),
                        Source = c.String(maxLength: 50),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.ViewerPersonId)
                .ForeignKey("crmPerson", t => t.TargetPersonId)
                .Index(t => t.ViewerPersonId)
                .Index(t => t.TargetPersonId);
            
            CreateTable(
                "dbo.utilJob",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        System = c.Boolean(),
                        Active = c.Boolean(),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Assemby = c.String(maxLength: 100),
                        Class = c.String(nullable: false, maxLength: 100),
                        CronExpression = c.String(nullable: false, maxLength: 120),
                        LastSuccessfulRun = c.DateTime(),
                        LastRunDate = c.DateTime(),
                        LastRunDuration = c.Int(),
                        LastStatus = c.String(maxLength: 50),
                        LastStatusMessage = c.String(),
                        LastRunSchedulerName = c.String(maxLength: 40),
                        NotificationEmails = c.String(maxLength: 1000),
                        NotificationStatus = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        ModifiedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("crmPerson", t => t.CreatedByPersonId)
                .ForeignKey("crmPerson", t => t.ModifiedByPersonId)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.ModifiedByPersonId);
            
            CreateTable(
                "dbo.groupsGroupTypeAssociation",
                c => new
                    {
                        ChildGroupTypeId = c.Int(nullable: false),
                        ParentGroupTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ChildGroupTypeId, t.ParentGroupTypeId })
                .ForeignKey("groupsGroupType", t => t.ChildGroupTypeId)
                .ForeignKey("groupsGroupType", t => t.ParentGroupTypeId)
                .Index(t => t.ChildGroupTypeId)
                .Index(t => t.ParentGroupTypeId);
            
            CreateTable(
                "dbo.groupsGroupTypeRole",
                c => new
                    {
                        GroupTypeId = c.Int(nullable: false),
                        GroupRoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupTypeId, t.GroupRoleId })
                .ForeignKey("groupsGroupRole", t => t.GroupTypeId, cascadeDelete: true)
                .ForeignKey("groupsGroupType", t => t.GroupRoleId, cascadeDelete: true)
                .Index(t => t.GroupTypeId)
                .Index(t => t.GroupRoleId);

            CreateIndexes();
            AddData();
            
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteData();
            DropIndexes();

            DropIndex("groupsGroupTypeRole", new[] { "GroupRoleId" });
            DropIndex("groupsGroupTypeRole", new[] { "GroupTypeId" });
            DropIndex("groupsGroupTypeAssociation", new[] { "ParentGroupTypeId" });
            DropIndex("groupsGroupTypeAssociation", new[] { "ChildGroupTypeId" });
            DropIndex("utilJob", new[] { "ModifiedByPersonId" });
            DropIndex("utilJob", new[] { "CreatedByPersonId" });
            DropIndex("crmPersonViewed", new[] { "TargetPersonId" });
            DropIndex("crmPersonViewed", new[] { "ViewerPersonId" });
            DropIndex("crmAddress", new[] { "ModifiedByPersonId" });
            DropIndex("crmAddress", new[] { "CreatedByPersonId" });
            DropIndex("coreExceptionLog", new[] { "PersonId" });
            DropIndex("coreEntityChange", new[] { "CreatedByPersonId" });
            DropIndex("cmsFile", new[] { "ModifiedByPersonId" });
            DropIndex("cmsFile", new[] { "CreatedByPersonId" });
            DropIndex("cmsSiteDomain", new[] { "ModifiedByPersonId" });
            DropIndex("cmsSiteDomain", new[] { "CreatedByPersonId" });
            DropIndex("cmsSiteDomain", new[] { "SiteId" });
            DropIndex("cmsSite", new[] { "ModifiedByPersonId" });
            DropIndex("cmsSite", new[] { "CreatedByPersonId" });
            DropIndex("cmsSite", new[] { "DefaultPageId" });
            DropIndex("cmsPageRoute", new[] { "ModifiedByPersonId" });
            DropIndex("cmsPageRoute", new[] { "CreatedByPersonId" });
            DropIndex("cmsPageRoute", new[] { "PageId" });
            DropIndex("cmsPage", new[] { "ModifiedByPersonId" });
            DropIndex("cmsPage", new[] { "CreatedByPersonId" });
            DropIndex("cmsPage", new[] { "SiteId" });
            DropIndex("cmsPage", new[] { "ParentPageId" });
            DropIndex("cmsHtmlContent", new[] { "ModifiedByPersonId" });
            DropIndex("cmsHtmlContent", new[] { "CreatedByPersonId" });
            DropIndex("cmsHtmlContent", new[] { "ApprovedByPersonId" });
            DropIndex("cmsHtmlContent", new[] { "BlockId" });
            DropIndex("cmsBlockInstance", new[] { "ModifiedByPersonId" });
            DropIndex("cmsBlockInstance", new[] { "CreatedByPersonId" });
            DropIndex("cmsBlockInstance", new[] { "PageId" });
            DropIndex("cmsBlockInstance", new[] { "BlockId" });
            DropIndex("cmsBlock", new[] { "ModifiedByPersonId" });
            DropIndex("cmsBlock", new[] { "CreatedByPersonId" });
            DropIndex("fiancialPersonAccountLookup", new[] { "PersonId" });
            DropIndex("financialTransactionDetail", new[] { "ModifiedByPersonId" });
            DropIndex("financialTransactionDetail", new[] { "CreatedByPersonId" });
            DropIndex("financialTransactionDetail", new[] { "TransactionId" });
            DropIndex("financialGateway", new[] { "ModifiedByPersonId" });
            DropIndex("financialGateway", new[] { "CreatedByPersonId" });
            DropIndex("financialBatch", new[] { "ModifiedByPersonId" });
            DropIndex("financialBatch", new[] { "CreatedByPersonId" });
            DropIndex("financialTransaction", new[] { "ModifiedByPersonId" });
            DropIndex("financialTransaction", new[] { "CreatedByPersonId" });
            DropIndex("financialTransaction", new[] { "SourceTypeId" });
            DropIndex("financialTransaction", new[] { "GatewayId" });
            DropIndex("financialTransaction", new[] { "CreditCardTypeId" });
            DropIndex("financialTransaction", new[] { "CurrencyTypeId" });
            DropIndex("financialTransaction", new[] { "BatchId" });
            DropIndex("financialTransactionFund", new[] { "FundId" });
            DropIndex("financialTransactionFund", new[] { "TransactionId" });
            DropIndex("coreAttributeValue", new[] { "ModifiedByPersonId" });
            DropIndex("coreAttributeValue", new[] { "CreatedByPersonId" });
            DropIndex("coreAttributeValue", new[] { "AttributeId" });
            DropIndex("coreAttributeQualifier", new[] { "ModifiedByPersonId" });
            DropIndex("coreAttributeQualifier", new[] { "CreatedByPersonId" });
            DropIndex("coreAttributeQualifier", new[] { "AttributeId" });
            DropIndex("coreAttribute", new[] { "ModifiedByPersonId" });
            DropIndex("coreAttribute", new[] { "CreatedByPersonId" });
            DropIndex("coreAttribute", new[] { "FieldTypeId" });
            DropIndex("coreFieldType", new[] { "ModifiedByPersonId" });
            DropIndex("coreFieldType", new[] { "CreatedByPersonId" });
            DropIndex("coreDefinedType", new[] { "ModifiedByPersonId" });
            DropIndex("coreDefinedType", new[] { "CreatedByPersonId" });
            DropIndex("coreDefinedType", new[] { "FieldTypeId" });
            DropIndex("coreDefinedValue", new[] { "ModifiedByPersonId" });
            DropIndex("coreDefinedValue", new[] { "CreatedByPersonId" });
            DropIndex("coreDefinedValue", new[] { "DefinedTypeId" });
            DropIndex("financialFund", new[] { "ModifiedByPersonId" });
            DropIndex("financialFund", new[] { "CreatedByPersonId" });
            DropIndex("financialFund", new[] { "FundTypeId" });
            DropIndex("financialFund", new[] { "ParentFundId" });
            DropIndex("financialPledge", new[] { "ModifiedByPersonId" });
            DropIndex("financialPledge", new[] { "CreatedByPersonId" });
            DropIndex("financialPledge", new[] { "FrequencyTypeId" });
            DropIndex("financialPledge", new[] { "FundId" });
            DropIndex("financialPledge", new[] { "PersonId" });
            DropIndex("groupsGroupRole", new[] { "ModifiedByPersonId" });
            DropIndex("groupsGroupRole", new[] { "CreatedByPersonId" });
            DropIndex("groupsGroupType", new[] { "DefaultGroupRoleId" });
            DropIndex("groupsGroupType", new[] { "ModifiedByPersonId" });
            DropIndex("groupsGroupType", new[] { "CreatedByPersonId" });
            DropIndex("groupsGroup", new[] { "GroupTypeId" });
            DropIndex("groupsGroup", new[] { "ParentGroupId" });
            DropIndex("groupsGroup", new[] { "ModifiedByPersonId" });
            DropIndex("groupsGroup", new[] { "CreatedByPersonId" });
            DropIndex("groupsMember", new[] { "GroupRoleId" });
            DropIndex("groupsMember", new[] { "GroupId" });
            DropIndex("groupsMember", new[] { "ModifiedByPersonId" });
            DropIndex("groupsMember", new[] { "CreatedByPersonId" });
            DropIndex("groupsMember", new[] { "PersonId" });
            DropIndex("crmPhoneNumber", new[] { "ModifiedByPersonId" });
            DropIndex("crmPhoneNumber", new[] { "CreatedByPersonId" });
            DropIndex("crmPhoneNumber", new[] { "PersonId" });
            DropIndex("crmEmailTemplate", new[] { "ModifiedByPersonId" });
            DropIndex("crmEmailTemplate", new[] { "PersonId" });
            DropIndex("crmEmailTemplate", new[] { "CreatedByPersonId" });
            DropIndex("cmsUser", new[] { "ModifiedByPersonId" });
            DropIndex("cmsUser", new[] { "CreatedByPersonId" });
            DropIndex("cmsUser", new[] { "PersonId" });
            DropIndex("crmPerson", new[] { "ModifiedByPersonId" });
            DropIndex("crmPerson", new[] { "CreatedByPersonId" });
            DropIndex("crmPerson", new[] { "TitleId" });
            DropIndex("crmPerson", new[] { "SuffixId" });
            DropIndex("crmPerson", new[] { "RecordTypeId" });
            DropIndex("crmPerson", new[] { "RecordStatusReasonId" });
            DropIndex("crmPerson", new[] { "RecordStatusId" });
            DropIndex("crmPerson", new[] { "PersonStatusId" });
            DropIndex("crmPerson", new[] { "MaritalStatusId" });
            DropIndex("cmsAuth", new[] { "PersonId" });
            DropIndex("cmsAuth", new[] { "GroupId" });
            DropIndex("cmsAuth", new[] { "ModifiedByPersonId" });
            DropIndex("cmsAuth", new[] { "CreatedByPersonId" });
            DropForeignKey( "dbo.groupsGroupTypeRole", "GroupTypeId", "groupsGroupType" );
            DropForeignKey( "dbo.groupsGroupTypeRole", "GroupRoleId", "groupsGroupRole" );
            DropForeignKey( "dbo.groupsGroupTypeAssociation", "ParentGroupTypeId", "groupsGroupType");
            DropForeignKey( "dbo.groupsGroupTypeAssociation", "ChildGroupTypeId", "groupsGroupType");
            DropForeignKey( "dbo.utilJob", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.utilJob", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.crmPersonViewed", "TargetPersonId", "crmPerson");
            DropForeignKey( "dbo.crmPersonViewed", "ViewerPersonId", "crmPerson");
            DropForeignKey( "dbo.crmAddress", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.crmAddress", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreExceptionLog", "PersonId", "crmPerson");
            DropForeignKey( "dbo.coreEntityChange", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsFile", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsFile", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsSiteDomain", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsSiteDomain", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsSiteDomain", "SiteId", "cmsSite");
            DropForeignKey( "dbo.cmsSite", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsSite", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsSite", "DefaultPageId", "cmsPage");
            DropForeignKey( "dbo.cmsPageRoute", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsPageRoute", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsPageRoute", "PageId", "cmsPage");
            DropForeignKey( "dbo.cmsPage", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsPage", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsPage", "SiteId", "cmsSite");
            DropForeignKey( "dbo.cmsPage", "ParentPageId", "cmsPage");
            DropForeignKey( "dbo.cmsHtmlContent", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsHtmlContent", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsHtmlContent", "ApprovedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsHtmlContent", "BlockId", "cmsBlockInstance");
            DropForeignKey( "dbo.cmsBlockInstance", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsBlockInstance", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsBlockInstance", "PageId", "cmsPage");
            DropForeignKey( "dbo.cmsBlockInstance", "BlockId", "cmsBlock");
            DropForeignKey( "dbo.cmsBlock", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsBlock", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.fiancialPersonAccountLookup", "PersonId", "crmPerson");
            DropForeignKey( "dbo.financialTransactionDetail", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialTransactionDetail", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialTransactionDetail", "TransactionId", "financialTransaction");
            DropForeignKey( "dbo.financialGateway", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialGateway", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialBatch", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialBatch", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialTransaction", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialTransaction", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialTransaction", "SourceTypeId", "coreDefinedValue");
            DropForeignKey( "dbo.financialTransaction", "GatewayId", "financialGateway");
            DropForeignKey( "dbo.financialTransaction", "CreditCardTypeId", "coreDefinedValue");
            DropForeignKey( "dbo.financialTransaction", "CurrencyTypeId", "coreDefinedValue");
            DropForeignKey( "dbo.financialTransaction", "BatchId", "financialBatch");
            DropForeignKey( "dbo.financialTransactionFund", "FundId", "financialFund");
            DropForeignKey( "dbo.financialTransactionFund", "TransactionId", "financialTransaction");
            DropForeignKey( "dbo.coreAttributeValue", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreAttributeValue", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreAttributeValue", "AttributeId", "coreAttribute");
            DropForeignKey( "dbo.coreAttributeQualifier", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreAttributeQualifier", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreAttributeQualifier", "AttributeId", "coreAttribute");
            DropForeignKey( "dbo.coreAttribute", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreAttribute", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreAttribute", "FieldTypeId", "coreFieldType");
            DropForeignKey( "dbo.coreFieldType", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreFieldType", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreDefinedType", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreDefinedType", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreDefinedType", "FieldTypeId", "coreFieldType");
            DropForeignKey( "dbo.coreDefinedValue", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreDefinedValue", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.coreDefinedValue", "DefinedTypeId", "coreDefinedType");
            DropForeignKey( "dbo.financialFund", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialFund", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialFund", "FundTypeId", "coreDefinedValue");
            DropForeignKey( "dbo.financialFund", "ParentFundId", "financialFund");
            DropForeignKey( "dbo.financialPledge", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialPledge", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.financialPledge", "FrequencyTypeId", "coreDefinedValue");
            DropForeignKey( "dbo.financialPledge", "FundId", "financialFund");
            DropForeignKey( "dbo.financialPledge", "PersonId", "crmPerson");
            DropForeignKey( "dbo.groupsGroupRole", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.groupsGroupRole", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.groupsGroupType", "DefaultGroupRoleId", "groupsGroupRole");
            DropForeignKey( "dbo.groupsGroupType", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.groupsGroupType", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.groupsGroup", "GroupTypeId", "groupsGroupType");
            DropForeignKey( "dbo.groupsGroup", "ParentGroupId", "groupsGroup");
            DropForeignKey( "dbo.groupsGroup", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.groupsGroup", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.groupsMember", "GroupRoleId", "groupsGroupRole");
            DropForeignKey( "dbo.groupsMember", "GroupId", "groupsGroup");
            DropForeignKey( "dbo.groupsMember", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.groupsMember", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.groupsMember", "PersonId", "crmPerson");
            DropForeignKey( "dbo.crmPhoneNumber", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.crmPhoneNumber", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.crmPhoneNumber", "PersonId", "crmPerson");
            DropForeignKey( "dbo.crmEmailTemplate", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.crmEmailTemplate", "PersonId", "crmPerson");
            DropForeignKey( "dbo.crmEmailTemplate", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsUser", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsUser", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsUser", "PersonId", "crmPerson");
            DropForeignKey( "dbo.crmPerson", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.crmPerson", "CreatedByPersonId", "crmPerson");
            DropForeignKey( "dbo.crmPerson", "TitleId", "coreDefinedValue");
            DropForeignKey( "dbo.crmPerson", "SuffixId", "coreDefinedValue");
            DropForeignKey( "dbo.crmPerson", "RecordTypeId", "coreDefinedValue");
            DropForeignKey( "dbo.crmPerson", "RecordStatusReasonId", "coreDefinedValue");
            DropForeignKey( "dbo.crmPerson", "RecordStatusId", "coreDefinedValue");
            DropForeignKey( "dbo.crmPerson", "PersonStatusId", "coreDefinedValue");
            DropForeignKey( "dbo.crmPerson", "MaritalStatusId", "coreDefinedValue");
            DropForeignKey( "dbo.cmsAuth", "PersonId", "crmPerson");
            DropForeignKey( "dbo.cmsAuth", "GroupId", "groupsGroup");
            DropForeignKey( "dbo.cmsAuth", "ModifiedByPersonId", "crmPerson");
            DropForeignKey( "dbo.cmsAuth", "CreatedByPersonId", "crmPerson");
            DropTable( "dbo.groupsGroupTypeRole" );
            DropTable( "dbo.groupsGroupTypeAssociation" );
            DropTable( "dbo.utilJob" );
            DropTable( "dbo.crmPersonViewed" );
            DropTable( "dbo.crmPersonTrail" );
            DropTable( "dbo.crmAddress" );
            DropTable( "dbo.coreServiceLog" );
            DropTable( "dbo.coreExceptionLog" );
            DropTable( "dbo.coreEntityChange" );
            DropTable( "dbo.cmsFile" );
            DropTable( "dbo.cmsSiteDomain" );
            DropTable( "dbo.cmsSite" );
            DropTable( "dbo.cmsPageRoute" );
            DropTable( "dbo.cmsPage" );
            DropTable( "dbo.cmsHtmlContent" );
            DropTable( "dbo.cmsBlockInstance" );
            DropTable( "dbo.cmsBlock" );
            DropTable( "dbo.fiancialPersonAccountLookup" );
            DropTable( "dbo.financialTransactionDetail" );
            DropTable( "dbo.financialGateway" );
            DropTable( "dbo.financialBatch" );
            DropTable( "dbo.financialTransaction" );
            DropTable( "dbo.financialTransactionFund" );
            DropTable( "dbo.coreAttributeValue" );
            DropTable( "dbo.coreAttributeQualifier" );
            DropTable( "dbo.coreAttribute" );
            DropTable( "dbo.coreFieldType" );
            DropTable( "dbo.coreDefinedType" );
            DropTable( "dbo.coreDefinedValue" );
            DropTable( "dbo.financialFund" );
            DropTable( "dbo.financialPledge" );
            DropTable( "dbo.groupsGroupRole" );
            DropTable( "dbo.groupsGroupType" );
            DropTable( "dbo.groupsGroup" );
            DropTable( "dbo.groupsMember" );
            DropTable( "dbo.crmPhoneNumber" );
            DropTable( "dbo.crmEmailTemplate" );
            DropTable( "dbo.cmsUser" );
            DropTable( "dbo.crmPerson" );
            DropTable( "dbo.cmsAuth" );
        }
    }
}
