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
    public partial class CreateDatabase : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Auth",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        Order = c.Int(nullable: false),
                        Action = c.String(nullable: false, maxLength: 50),
                        AllowOrDeny = c.String(nullable: false, maxLength: 1),
                        SpecialRole = c.Int(nullable: false),
                        PersonId = c.Int(),
                        GroupId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .Index(t => t.GroupId)
                .Index(t => t.PersonId)
                .Index(t => t.EntityTypeId);
            
            CreateIndex( "dbo.Auth", "Guid", true );
            CreateTable(
                "dbo.Group",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        ParentGroupId = c.Int(),
                        GroupTypeId = c.Int(nullable: false),
                        CampusId = c.Int(),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsSecurityRole = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.ParentGroupId)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId)
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .Index(t => t.ParentGroupId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.CampusId);
            
            CreateIndex( "dbo.Group", "Guid", true );
            CreateTable(
                "dbo.GroupMember",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        GroupId = c.Int(nullable: false),
                        PersonId = c.Int(nullable: false),
                        GroupRoleId = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.GroupRole", t => t.GroupRoleId)
                .Index(t => t.PersonId)
                .Index(t => t.GroupId)
                .Index(t => t.GroupRoleId);
            
            CreateIndex( "dbo.GroupMember", "Guid", true );
            CreateTable(
                "dbo.Person",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
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
                        Gender = c.Int(nullable: false),
                        MaritalStatusId = c.Int(),
                        AnniversaryDate = c.DateTime(),
                        GraduationDate = c.DateTime(),
                        Email = c.String(maxLength: 75),
                        IsEmailActive = c.Boolean(),
                        EmailNote = c.String(maxLength: 250),
                        DoNotEmail = c.Boolean(nullable: false),
                        SystemNote = c.String(maxLength: 1000),
                        ViewedCount = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedValue", t => t.MaritalStatusId)
                .ForeignKey("dbo.DefinedValue", t => t.PersonStatusId)
                .ForeignKey("dbo.DefinedValue", t => t.RecordStatusId)
                .ForeignKey("dbo.DefinedValue", t => t.RecordStatusReasonId)
                .ForeignKey("dbo.DefinedValue", t => t.RecordTypeId)
                .ForeignKey("dbo.DefinedValue", t => t.SuffixId)
                .ForeignKey("dbo.DefinedValue", t => t.TitleId)
                .ForeignKey("dbo.BinaryFile", t => t.PhotoId)
                .Index(t => t.MaritalStatusId)
                .Index(t => t.PersonStatusId)
                .Index(t => t.RecordStatusId)
                .Index(t => t.RecordStatusReasonId)
                .Index(t => t.RecordTypeId)
                .Index(t => t.SuffixId)
                .Index(t => t.TitleId)
                .Index(t => t.PhotoId);
            
            CreateIndex( "dbo.Person", "Guid", true );
            CreateTable(
                "dbo.UserLogin",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServiceType = c.Int(nullable: false),
                        ServiceName = c.String(nullable: false, maxLength: 200),
                        UserName = c.String(nullable: false, maxLength: 255),
                        Password = c.String(maxLength: 128),
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
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.UserLogin", "Guid", true );
            CreateTable(
                "dbo.EmailTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        PersonId = c.Int(),
                        Category = c.String(maxLength: 100),
                        Title = c.String(nullable: false, maxLength: 100),
                        From = c.String(maxLength: 200),
                        To = c.String(),
                        Cc = c.String(),
                        Bcc = c.String(),
                        Subject = c.String(nullable: false, maxLength: 200),
                        Body = c.String(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.EmailTemplate", "Guid", true );
            CreateTable(
                "dbo.PhoneNumber",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        PersonId = c.Int(nullable: false),
                        Number = c.String(nullable: false, maxLength: 20),
                        Extension = c.String(maxLength: 20),
                        NumberTypeId = c.Int(),
                        IsMessagingEnabled = c.Boolean(nullable: false),
                        IsUnlisted = c.Boolean(nullable: false),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedValue", t => t.NumberTypeId)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.NumberTypeId)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.PhoneNumber", "Guid", true );
            CreateTable(
                "dbo.DefinedValue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        DefinedTypeId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedType", t => t.DefinedTypeId)
                .Index(t => t.DefinedTypeId);
            
            CreateIndex( "dbo.DefinedValue", "Guid", true );
            CreateTable(
                "dbo.DefinedType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        FieldTypeId = c.Int(),
                        Order = c.Int(nullable: false),
                        Category = c.String(maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FieldType", t => t.FieldTypeId)
                .Index(t => t.FieldTypeId);
            
            CreateIndex( "dbo.DefinedType", "Guid", true );
            CreateTable(
                "dbo.FieldType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Assembly = c.String(nullable: false, maxLength: 100),
                        Class = c.String(nullable: false, maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.FieldType", "Guid", true );
            CreateTable(
                "dbo.Pledge",
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
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .ForeignKey("dbo.Fund", t => t.FundId)
                .ForeignKey("dbo.DefinedValue", t => t.FrequencyTypeId)
                .Index(t => t.PersonId)
                .Index(t => t.FundId)
                .Index(t => t.FrequencyTypeId);
            
            CreateIndex( "dbo.Pledge", "Guid", true );
            CreateTable(
                "dbo.Fund",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        PublicName = c.String(maxLength: 50),
                        Description = c.String(maxLength: 250),
                        ParentFundId = c.Int(),
                        IsTaxDeductible = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        IsPledgable = c.Boolean(nullable: false),
                        GlCode = c.String(maxLength: 50),
                        FundTypeId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Fund", t => t.ParentFundId)
                .ForeignKey("dbo.DefinedValue", t => t.FundTypeId)
                .Index(t => t.ParentFundId)
                .Index(t => t.FundTypeId);
            
            CreateIndex( "dbo.Fund", "Guid", true );
            CreateTable(
                "dbo.FinancialTransactionFund",
                c => new
                    {
                        TransactionId = c.Int(nullable: false),
                        FundId = c.Int(nullable: false),
                        Amount = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.TransactionId, t.FundId })
                .ForeignKey("dbo.FinancialTransaction", t => t.TransactionId, cascadeDelete: true)
                .ForeignKey("dbo.Fund", t => t.FundId, cascadeDelete: true)
                .Index(t => t.TransactionId)
                .Index(t => t.FundId);
            
            CreateTable(
                "dbo.FinancialTransaction",
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
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialBatch", t => t.BatchId)
                .ForeignKey("dbo.DefinedValue", t => t.CurrencyTypeId)
                .ForeignKey("dbo.DefinedValue", t => t.CreditCardTypeId)
                .ForeignKey("dbo.PaymentGateway", t => t.GatewayId)
                .ForeignKey("dbo.DefinedValue", t => t.SourceTypeId)
                .Index(t => t.BatchId)
                .Index(t => t.CurrencyTypeId)
                .Index(t => t.CreditCardTypeId)
                .Index(t => t.GatewayId)
                .Index(t => t.SourceTypeId);
            
            CreateIndex( "dbo.FinancialTransaction", "Guid", true );
            CreateTable(
                "dbo.FinancialBatch",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        BatchDate = c.DateTime(),
                        IsClosed = c.Boolean(nullable: false),
                        CampusId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.Int(),
                        ForeignReference = c.String(maxLength: 50),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.FinancialBatch", "Guid", true );
            CreateTable(
                "dbo.PaymentGateway",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        Description = c.String(maxLength: 500),
                        ApiUrl = c.String(maxLength: 100),
                        ApiKey = c.String(maxLength: 100),
                        ApiSecret = c.String(maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.PaymentGateway", "Guid", true );
            CreateTable(
                "dbo.FinancialTransactionDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(),
                        Entity = c.String(maxLength: 50),
                        EntityId = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Summary = c.String(maxLength: 500),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialTransaction", t => t.TransactionId)
                .Index(t => t.TransactionId);
            
            CreateIndex( "dbo.FinancialTransactionDetail", "Guid", true );
            CreateTable(
                "dbo.PersonAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(),
                        Account = c.String(maxLength: 50),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.PersonAccount", "Guid", true );
            CreateTable(
                "dbo.BinaryFile",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsTemporary = c.Boolean(nullable: false),
                        IsSystem = c.Boolean(nullable: false),
                        Data = c.Binary(),
                        Url = c.String(maxLength: 255),
                        FileName = c.String(nullable: false, maxLength: 255),
                        MimeType = c.String(nullable: false, maxLength: 255),
                        LastModifiedTime = c.DateTime(),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.BinaryFile", "Guid", true );
            CreateTable(
                "dbo.GroupRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        GroupTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        SortOrder = c.Int(),
                        MaxCount = c.Int(),
                        MinCount = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId, cascadeDelete: true)
                .Index(t => t.GroupTypeId);
            
            CreateIndex( "dbo.GroupRole", "Guid", true );
            CreateTable(
                "dbo.GroupType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        DefaultGroupRoleId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupRole", t => t.DefaultGroupRoleId)
                .Index(t => t.DefaultGroupRoleId);
            
            CreateIndex( "dbo.GroupType", "Guid", true );
            CreateTable(
                "dbo.GroupTypeLocationType",
                c => new
                    {
                        GroupTypeId = c.Int(nullable: false),
                        LocationTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupTypeId, t.LocationTypeId })
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.LocationTypeId, cascadeDelete: true)
                .Index(t => t.GroupTypeId)
                .Index(t => t.LocationTypeId);
            
            CreateTable(
                "dbo.GroupLocation",
                c => new
                    {
                        GroupId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        LocationTypeId = c.Int(),
                    })
                .PrimaryKey(t => new { t.GroupId, t.LocationId })
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Location", t => t.LocationId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.LocationTypeId)
                .Index(t => t.GroupId)
                .Index(t => t.LocationId)
                .Index(t => t.LocationTypeId);
            
            CreateTable(
                "dbo.Location",
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
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        ParcelId = c.String(maxLength: 50),
                        StandardizeAttempt = c.DateTime(),
                        StandardizeService = c.String(maxLength: 50),
                        StandardizeResult = c.String(maxLength: 50),
                        StandardizeDate = c.DateTime(),
                        GeocodeAttempt = c.DateTime(),
                        GeocodeService = c.String(maxLength: 50),
                        GeocodeResult = c.String(maxLength: 50),
                        GeocodeDate = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.Location", "Guid", true );
            CreateTable(
                "dbo.Campus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.Campus", "Name", true );
            CreateIndex( "dbo.Campus", "Guid", true );
            CreateTable(
                "dbo.EntityType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        FriendlyName = c.String(maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.EntityType", "Name", true );
            CreateIndex( "dbo.EntityType", "Guid", true );
            CreateTable(
                "dbo.BlockType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Path = c.String(nullable: false, maxLength: 200),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.BlockType", "Guid", true );
            CreateTable(
                "dbo.Block",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        PageId = c.Int(),
                        Layout = c.String(maxLength: 100),
                        BlockTypeId = c.Int(nullable: false),
                        Zone = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        OutputCacheDuration = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BlockType", t => t.BlockTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Page", t => t.PageId, cascadeDelete: true)
                .Index(t => t.BlockTypeId)
                .Index(t => t.PageId);
            
            CreateIndex( "dbo.Block", "Guid", true );
            CreateTable(
                "dbo.HtmlContent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlockId = c.Int(nullable: false),
                        EntityValue = c.String(maxLength: 200),
                        Version = c.Int(nullable: false),
                        Content = c.String(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                        ApprovedByPersonId = c.Int(),
                        ApprovedDateTime = c.DateTime(),
                        StartDateTime = c.DateTime(),
                        ExpireDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Block", t => t.BlockId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.ApprovedByPersonId)
                .Index(t => t.BlockId)
                .Index(t => t.ApprovedByPersonId);
            
            CreateIndex( "dbo.HtmlContent", "Guid", true );
            CreateTable(
                "dbo.Page",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Title = c.String(maxLength: 100),
                        IsSystem = c.Boolean(nullable: false),
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
                        IconUrl = c.String(maxLength: 150),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Page", t => t.ParentPageId)
                .ForeignKey("dbo.Site", t => t.SiteId)
                .Index(t => t.ParentPageId)
                .Index(t => t.SiteId);
            
            CreateIndex( "dbo.Page", "Guid", true );
            CreateTable(
                "dbo.PageRoute",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        PageId = c.Int(nullable: false),
                        Route = c.String(nullable: false, maxLength: 200),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Page", t => t.PageId, cascadeDelete: true)
                .Index(t => t.PageId);
            
            CreateIndex( "dbo.PageRoute", "Guid", true );
            CreateTable(
                "dbo.PageContext",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        PageId = c.Int(nullable: false),
                        Entity = c.String(nullable: false, maxLength: 200),
                        IdParameter = c.String(nullable: false, maxLength: 100),
                        CreatedDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Page", t => t.PageId, cascadeDelete: true)
                .Index(t => t.PageId);
            
            CreateIndex( "dbo.PageContext", "Guid", true );
            CreateTable(
                "dbo.Site",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
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
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Page", t => t.DefaultPageId)
                .Index(t => t.DefaultPageId);
            
            CreateIndex( "dbo.Site", "Guid", true );
            CreateTable(
                "dbo.SiteDomain",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        SiteId = c.Int(nullable: false),
                        Domain = c.String(nullable: false, maxLength: 200),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Site", t => t.SiteId, cascadeDelete: true)
                .Index(t => t.SiteId);
            
            CreateIndex( "dbo.SiteDomain", "Guid", true );
            CreateTable(
                "dbo.MarketingCampaign",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        ContactPersonId = c.Int(),
                        ContactEmail = c.String(maxLength: 254),
                        ContactPhoneNumber = c.String(maxLength: 20),
                        ContactFullName = c.String(maxLength: 152),
                        EventGroupId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.ContactPersonId)
                .ForeignKey("dbo.Group", t => t.EventGroupId)
                .Index(t => t.ContactPersonId)
                .Index(t => t.EventGroupId);
            
            CreateIndex( "dbo.MarketingCampaign", "Guid", true );
            CreateTable(
                "dbo.MarketingCampaignAd",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketingCampaignId = c.Int(nullable: false),
                        MarketingCampaignAdTypeId = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        MarketingCampaignAdStatus = c.Byte(nullable: false),
                        MarketingCampaignStatusPersonId = c.Int(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Url = c.String(maxLength: 2000),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MarketingCampaign", t => t.MarketingCampaignId, cascadeDelete: true)
                .ForeignKey("dbo.MarketingCampaignAdType", t => t.MarketingCampaignAdTypeId)
                .Index(t => t.MarketingCampaignId)
                .Index(t => t.MarketingCampaignAdTypeId);
            
            CreateIndex( "dbo.MarketingCampaignAd", "Guid", true );
            CreateTable(
                "dbo.MarketingCampaignAdType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        DateRangeType = c.Byte(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.MarketingCampaignAdType", "Guid", true );
            CreateTable(
                "dbo.MarketingCampaignAudience",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketingCampaignId = c.Int(nullable: false),
                        AudienceTypeValueId = c.Int(nullable: false),
                        IsPrimary = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MarketingCampaign", t => t.MarketingCampaignId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.AudienceTypeValueId, cascadeDelete: true)
                .Index(t => t.MarketingCampaignId)
                .Index(t => t.AudienceTypeValueId);
            
            CreateIndex( "dbo.MarketingCampaignAudience", "Guid", true );
            CreateTable(
                "dbo.MarketingCampaignCampus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MarketingCampaignId = c.Int(nullable: false),
                        CampusId = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MarketingCampaign", t => t.MarketingCampaignId, cascadeDelete: true)
                .ForeignKey("dbo.Campus", t => t.CampusId, cascadeDelete: true)
                .Index(t => t.MarketingCampaignId)
                .Index(t => t.CampusId);
            
            CreateIndex( "dbo.MarketingCampaignCampus", "Guid", true );
            CreateTable(
                "dbo.Attribute",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        FieldTypeId = c.Int(nullable: false),
                        EntityTypeId = c.Int(),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
                        Key = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 100),
                        Category = c.String(maxLength: 100),
                        Description = c.String(),
                        Order = c.Int(nullable: false),
                        IsGridColumn = c.Boolean(nullable: false),
                        DefaultValue = c.String(),
                        IsMultiValue = c.Boolean(nullable: false),
                        IsRequired = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.FieldType", t => t.FieldTypeId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.FieldTypeId);
            
            CreateIndex( "dbo.Attribute", "Guid", true );
            CreateTable(
                "dbo.AttributeQualifier",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        AttributeId = c.Int(nullable: false),
                        Key = c.String(nullable: false, maxLength: 50),
                        Value = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Attribute", t => t.AttributeId, cascadeDelete: true)
                .Index(t => t.AttributeId);
            
            CreateIndex( "dbo.AttributeQualifier", "Guid", true );
            CreateTable(
                "dbo.AttributeValue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        AttributeId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        Order = c.Int(),
                        Value = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Attribute", t => t.AttributeId, cascadeDelete: true)
                .Index(t => t.AttributeId);
            
            CreateIndex( "dbo.AttributeValue", "Guid", true );
            CreateTable(
                "dbo.Audit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 200),
                        AuditType = c.Int(nullable: false),
                        Properties = c.String(),
                        DateTime = c.DateTime(),
                        PersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.EntityTypeId)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.Audit", "Guid", true );
            CreateTable(
                "dbo.Category",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        ParentCategoryId = c.Int(),
                        EntityTypeId = c.Int(nullable: false),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
                        Name = c.String(maxLength: 100),
                        FileId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.ParentCategoryId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.BinaryFile", t => t.FileId)
                .Index(t => t.ParentCategoryId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.FileId);
            
            CreateIndex( "dbo.Category", "Guid", true );
            CreateTable(
                "dbo.EntityChange",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChangeSet = c.Guid(nullable: false),
                        ChangeType = c.String(nullable: false, maxLength: 10),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        Property = c.String(nullable: false, maxLength: 100),
                        OriginalValue = c.String(),
                        CurrentValue = c.String(),
                        CreatedDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.Person", t => t.CreatedByPersonId, cascadeDelete: true)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CreatedByPersonId);
            
            CreateIndex( "dbo.EntityChange", "Guid", true );
            CreateTable(
                "dbo.ExceptionLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentId = c.Int(),
                        SiteId = c.Int(),
                        PageId = c.Int(),
                        ExceptionDate = c.DateTime(nullable: false),
                        CreatedByPersonId = c.Int(),
                        HasInnerException = c.Boolean(),
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
                .ForeignKey("dbo.Person", t => t.CreatedByPersonId, cascadeDelete: true)
                .Index(t => t.CreatedByPersonId);
            
            CreateIndex( "dbo.ExceptionLog", "Guid", true );
            CreateTable(
                "dbo.Metric",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Type = c.Boolean(nullable: false),
                        Category = c.String(maxLength: 100),
                        Title = c.String(nullable: false, maxLength: 100),
                        Subtitle = c.String(maxLength: 100),
                        Description = c.String(),
                        MinValue = c.Int(),
                        MaxValue = c.Int(),
                        CollectionFrequencyId = c.Int(),
                        LastCollected = c.DateTime(),
                        Source = c.String(maxLength: 100),
                        SourceSQL = c.String(),
                        Order = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedValue", t => t.CollectionFrequencyId)
                .Index(t => t.CollectionFrequencyId);
            
            CreateIndex( "dbo.Metric", "Guid", true );
            CreateTable(
                "dbo.MetricValue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        MetricId = c.Int(nullable: false),
                        Value = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        xValue = c.String(nullable: false),
                        isDateBased = c.Boolean(nullable: false),
                        Label = c.String(),
                        Order = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Metric", t => t.MetricId, cascadeDelete: true)
                .Index(t => t.MetricId);
            
            CreateIndex( "dbo.MetricValue", "Guid", true );
            CreateTable(
                "dbo.ServiceLog",
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
            
            CreateIndex( "dbo.ServiceLog", "Guid", true );
            CreateTable(
                "dbo.Tag",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
                        Name = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        OwnerId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.OwnerId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true)
                .Index(t => t.OwnerId)
                .Index(t => t.EntityTypeId);
            
            CreateIndex( "dbo.Tag", "Guid", true );
            CreateTable(
                "dbo.TaggedItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        TagId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tag", t => t.TagId, cascadeDelete: true)
                .Index(t => t.TagId);
            
            CreateIndex( "dbo.TaggedItem", "Guid", true );
            CreateTable(
                "dbo.PersonMerged",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CurrentId = c.Int(nullable: false),
                        CurrentGuid = c.Guid(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.PersonMerged", "Guid", true );
            CreateTable(
                "dbo.PersonViewed",
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
                .ForeignKey("dbo.Person", t => t.ViewerPersonId)
                .ForeignKey("dbo.Person", t => t.TargetPersonId)
                .Index(t => t.ViewerPersonId)
                .Index(t => t.TargetPersonId);
            
            CreateIndex( "dbo.PersonViewed", "Guid", true );
            CreateTable(
                "dbo.WorkflowAction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActivityId = c.Int(nullable: false),
                        ActionTypeId = c.Int(nullable: false),
                        LastProcessedDateTime = c.DateTime(),
                        CompletedDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WorkflowActivity", t => t.ActivityId, cascadeDelete: true)
                .ForeignKey("dbo.WorkflowActionType", t => t.ActionTypeId)
                .Index(t => t.ActivityId)
                .Index(t => t.ActionTypeId);
            
            CreateIndex( "dbo.WorkflowAction", "Guid", true );
            CreateTable(
                "dbo.WorkflowActivity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkflowId = c.Int(nullable: false),
                        ActivityTypeId = c.Int(nullable: false),
                        ActivatedDateTime = c.DateTime(),
                        LastProcessedDateTime = c.DateTime(),
                        CompletedDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workflow", t => t.WorkflowId, cascadeDelete: true)
                .ForeignKey("dbo.WorkflowActivityType", t => t.ActivityTypeId)
                .Index(t => t.WorkflowId)
                .Index(t => t.ActivityTypeId);
            
            CreateIndex( "dbo.WorkflowActivity", "Guid", true );
            CreateTable(
                "dbo.Workflow",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkflowTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Status = c.String(nullable: false, maxLength: 100),
                        IsProcessing = c.Boolean(nullable: false),
                        ActivatedDateTime = c.DateTime(),
                        LastProcessedDateTime = c.DateTime(),
                        CompletedDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WorkflowType", t => t.WorkflowTypeId, cascadeDelete: true)
                .Index(t => t.WorkflowTypeId);
            
            CreateIndex( "dbo.Workflow", "Guid", true );
            CreateTable(
                "dbo.WorkflowType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        IsActive = c.Boolean(),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CategoryId = c.Int(),
                        Order = c.Int(nullable: false),
                        FileId = c.Int(),
                        WorkTerm = c.String(nullable: false, maxLength: 100),
                        ProcessingIntervalSeconds = c.Int(),
                        IsPersisted = c.Boolean(nullable: false),
                        LoggingLevel = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.BinaryFile", t => t.FileId)
                .Index(t => t.CategoryId)
                .Index(t => t.FileId);
            
            CreateIndex( "dbo.WorkflowType", "Guid", true );
            CreateTable(
                "dbo.WorkflowActivityType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsActive = c.Boolean(),
                        WorkflowTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActivatedWithWorkflow = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WorkflowType", t => t.WorkflowTypeId, cascadeDelete: true)
                .Index(t => t.WorkflowTypeId);
            
            CreateIndex( "dbo.WorkflowActivityType", "Guid", true );
            CreateTable(
                "dbo.WorkflowActionType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActivityTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        IsActionCompletedOnSuccess = c.Boolean(nullable: false),
                        IsActivityCompletedOnSuccess = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WorkflowActivityType", t => t.ActivityTypeId, cascadeDelete: true)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true)
                .Index(t => t.ActivityTypeId)
                .Index(t => t.EntityTypeId);
            
            CreateIndex( "dbo.WorkflowActionType", "Guid", true );
            CreateTable(
                "dbo.WorkflowLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkflowId = c.Int(nullable: false),
                        LogDateTime = c.DateTime(nullable: false),
                        LogText = c.String(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Workflow", t => t.WorkflowId, cascadeDelete: true)
                .Index(t => t.WorkflowId);
            
            CreateIndex( "dbo.WorkflowLog", "Guid", true );
            CreateTable(
                "dbo.ServiceJob",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        IsActive = c.Boolean(),
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
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.ServiceJob", "Guid", true );
            CreateTable(
                "dbo.WorkflowTrigger",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
                        WorkflowTypeId = c.Int(nullable: false),
                        WorkflowTriggerType = c.Int(nullable: false),
                        WorkflowName = c.String(nullable: false, maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.WorkflowType", t => t.WorkflowTypeId, cascadeDelete: true)
                .Index(t => t.EntityTypeId)
                .Index(t => t.WorkflowTypeId);
            
            CreateIndex( "dbo.WorkflowTrigger", "Guid", true );
            CreateTable(
                "dbo.GroupTypeAssociation",
                c => new
                    {
                        GroupTypeId = c.Int(nullable: false),
                        ChildGroupTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupTypeId, t.ChildGroupTypeId })
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId)
                .ForeignKey("dbo.GroupType", t => t.ChildGroupTypeId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.ChildGroupTypeId);

            Sql( @"

-- disable constraints on all
alter table [Auth] nocheck constraint all
alter table [Block] nocheck constraint all
alter table [BlockType] nocheck constraint all
alter table [BinaryFile] nocheck constraint all
alter table [HtmlContent] nocheck constraint all
alter table [MarketingCampaign] nocheck constraint all
alter table [MarketingCampaignAd] nocheck constraint all
alter table [MarketingCampaignAdType] nocheck constraint all
alter table [MarketingCampaignAudience] nocheck constraint all
alter table [MarketingCampaignCampus] nocheck constraint all
alter table [Page] nocheck constraint all
alter table [PageContext] nocheck constraint all
alter table [PageRoute] nocheck constraint all
alter table [Site] nocheck constraint all
alter table [SiteDomain] nocheck constraint all
alter table [UserLogin] nocheck constraint all
alter table [Attribute] nocheck constraint all
alter table [AttributeQualifier] nocheck constraint all
alter table [AttributeValue] nocheck constraint all
alter table [Audit] nocheck constraint all
alter table [Category] nocheck constraint all
alter table [DefinedType] nocheck constraint all
alter table [DefinedValue] nocheck constraint all
alter table [EntityChange] nocheck constraint all
alter table [EntityChange] nocheck constraint all
alter table [EntityType] nocheck constraint all
alter table [ExceptionLog] nocheck constraint all
alter table [FieldType] nocheck constraint all
alter table [Metric] nocheck constraint all
alter table [MetricValue] nocheck constraint all
alter table [ServiceLog] nocheck constraint all
alter table [Tag] nocheck constraint all
alter table [TaggedItem] nocheck constraint all
alter table [Campus] nocheck constraint all
alter table [EmailTemplate] nocheck constraint all
alter table [Group] nocheck constraint all
alter table [GroupLocation] nocheck constraint all
alter table [GroupMember] nocheck constraint all
alter table [GroupRole] nocheck constraint all
alter table [GroupType] nocheck constraint all
alter table [GroupTypeAssociation] nocheck constraint all
alter table [GroupTypeLocationType] nocheck constraint all
alter table [Location] nocheck constraint all
alter table [Person] nocheck constraint all
alter table [PersonMerged] nocheck constraint all
alter table [PersonViewed] nocheck constraint all
alter table [PhoneNumber] nocheck constraint all
alter table [FinancialBatch] nocheck constraint all
alter table [Fund] nocheck constraint all
alter table [PaymentGateway] nocheck constraint all
alter table [PersonAccount] nocheck constraint all
alter table [Pledge] nocheck constraint all
alter table [FinancialTransaction] nocheck constraint all
alter table [FinancialTransactionDetail] nocheck constraint all
alter table [FinancialTransactionFund] nocheck constraint all
alter table [WorkflowAction] nocheck constraint all
alter table [WorkflowActionType] nocheck constraint all
alter table [WorkflowActivity] nocheck constraint all
alter table [WorkflowActivityType] nocheck constraint all
alter table [ServiceJob] nocheck constraint all
alter table [Workflow] nocheck constraint all
alter table [WorkflowLog] nocheck constraint all
alter table [WorkflowTrigger] nocheck constraint all
alter table [WorkflowType] nocheck constraint all

-- Add 50 rows to [BlockType]
SET IDENTITY_INSERT [BlockType] ON
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (2, 1, N'~/Blocks/PersonEdit.ascx', N'Person Edit', N'Edit Person Information', 'ddc0ac78-41a2-4c68-9685-eaa4ee9bf8d0')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (3, 1, N'~/Blocks/Administration/Sites.ascx', N'Sites', N'Site Administration', '3e0afd6e-3e3d-4ff9-9bc6-387afbf9acb6')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (4, 1, N'~/Blocks/Security/Login.ascx', N'Login', N'Provides ability to login to site.', '7b83d513-1178-429e-93ff-e76430e038e4')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (5, 1, N'~/Blocks/Security/CreateAccount.ascx', N'Create Account', N'Create new user accounts', '292d3578-bc27-4dab-bfc3-6d249e0905e0')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (6, 1, N'~/Blocks/Cms/HtmlContent.ascx', N'HTML Content', N'A block that displays HTML', '19b61d65-37e3-459f-a44f-def0089118a3')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (7, 1, N'~/Blocks/Administration/Roles.ascx', N'Roles', N'Role Administration', 'ea5f81b5-9086-4d34-8339-46d26b5f775e')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (8, 1, N'~/Blocks/Administration/BlockTypes.ascx', N'Block Types', N'Block Type Administration', '0244a072-6216-49f4-92ec-e6b5ffff03b5')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (14, 1, N'~/Blocks/Cms/PageXslt.ascx', N'Page Xslt Transformation', N'Used for page navigation controls', 'f49ad5f8-1e45-41e7-a88e-8cd285815bd9')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (17, 1, N'~/Blocks/Administration/Financials.ascx', N'Financial Transactions', N'View and search financial transactions', '18ee7010-e8cf-4b61-bfda-e014ccfc9e6d')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (18, 1, N'~/Blocks/Administration/ZoneBlocks.ascx', N'Zone Blocks', N'Allows you to manage blocks in a zone.', '72caaf77-a015-45f0-a549-f941b9ab4d75')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (21, 1, N'~/Blocks/Administration/BlockProperties.ascx', N'Block Properties', N'Allows you to administrate a blocks properties.', '5ec45388-83d4-4e99-bf25-3fa00327f08b')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (24, 1, N'~/Blocks/Administration/Security.ascx', N'Security', N'Block that allows the management of security on an object.', '20474b3d-0de7-4b63-b7b9-e042dbef788c')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (25, 1, N'~/Blocks/Administration/Pages.ascx', N'Pages', N'Allows for the administration of pages.', 'aefc2dbe-37b6-4cab-882c-b214f587bf2e')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (26, 1, N'~/Blocks/Administration/PageProperties.ascx', N'Page Properties', N'Gives the user the ability to edit the properties for a page.', 'c7988c3e-822d-4e73-882e-9b7684398baa')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (32, 1, N'~/Blocks/Administration/Address/Geocoding.ascx', N'Geocoding', N'Administrates geocoding providers.', '340f1474-3403-4426-a8f0-2e33c1b4bf2f')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (33, 1, N'~/Blocks/Administration/Address/Standardization.ascx', N'Standardization', N'Administrates address standardization providers.', '363c3382-5148-4097-82b2-c85a4910a837')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (34, 1, N'~/Blocks/Cms/Redirect.ascx', N'Redirect', N'Redirects the user to a provided URL.', 'b97fb779-5d3e-4663-b3b5-3c2c227ae14a')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (35, 1, N'~/Blocks/Security/LoginStatus.ascx', N'Login Status', N'Shows is a person is logged in or not.', '04712f3d-9667-4901-a49d-4507573ef7ad')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (36, 1, N'~/Blocks/Administration/Attributes.ascx', N'Attributes', N'Allows for the managing of attribues.', 'e5ea2f6d-43a2-48e0-b59c-4409b78ac830')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (37, 1, N'~/Blocks/Administration/AttributeValues.ascx', N'Attribute Values', N'Allows for providing values for attributes.', 'b084f060-ece4-462a-b6d0-35b2a30af3df')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (39, 1, N'~/Blocks/Security/ConfirmAccount.ascx', N'Confirm Account', N'Block that confirms a user''s account/login, usually from an email sent to their account.', '734dff21-7465-4e02-bfc3-d40f7a65fb60')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (40, 1, N'~/Blocks/Security/NewAccount.ascx', N'New Account', N'Allows a user to create a new account.', '99362b60-71a5-44c6-bcfe-dda9b00cc7f3')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (41, 1, N'~/Blocks/Security/ChangePassword.ascx', N'Change Password', N'Allows a user to change their password.', '3c12de99-2d1b-40f2-a9b8-6fe7c2524b37')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (42, 1, N'~/Blocks/Security/ForgotUserName.ascx', N'Forgot UserName', N'Allows a user to get their account information email to them.', '02b3d7d1-23ce-4154-b602-f4a15b321757')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (53, 1, N'~/Blocks/Administration/Components.ascx', N'Components', N'Block to administrate MEF plugins.', '21f5f466-59bc-40b2-8d73-7314d936c3cb')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (54, 1, N'~/Blocks/Administration/EmailTemplates.ascx', N'Email Templates', N'Allows the administration of email templates.', '10dc44e9-ecc1-4679-8a07-c098a0dcd82e')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (59, 1, N'~/Blocks/Core/ContextAttributeValues.ascx', N'ContextAttributeValues', N'Used to set the values of an attribute for a specific content context.', 'e9f76934-2c7d-4064-8e28-9410d17b8574')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (62, 1, N'~/Blocks/Group/ClassVideo.ascx', N'Class Video', N'E-Learning block to show videos and add viewers of the videos to a group for tracking.', 'a2cc955c-dfbe-4187-a57b-31bb76b75bbb')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (63, 1, N'~/Blocks/Administration/DefinedTypes.ascx', N'DefinedTypes', N'Allows for the administration of defined types.', '0a43050f-bf0b-49f7-9d8c-2761976f160f')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (64, 1, N'~/Blocks/Administration/Campuses.ascx', N'Campuses', N'Allows for the administration of campuses.', '0d0dc731-e282-44ea-ad1e-89d16ab20192')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (65, 1, N'~/Blocks/Administration/SystemInfo.ascx', N'System Information', N'Displays status and performance information for the currently running instance of Rock ChMS', 'de08efd7-4cf9-4bd5-9f72-c0151fd08523')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (66, 1, N'~/Blocks/Administration/PluginManager.ascx', N'Plugin Manager', N'Allows installed plugins to be viewed or removed and new ones to be added from the RockQuary.', 'f80268e6-2625-4565-aa2e-790c5e40a119')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (67, 1, N'~/Blocks/Crm/PersonSearch.ascx', N'Person Search', N'Displays list of people that match a given search type and term.', '764d3e67-2d01-437a-9f45-9f8c97878434')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (68, 1, N'~/Blocks/Administration/Tags.ascx', N'Tag Administration', N'Administer tags for specific entity and owner (or system)', '9bc9b09f-eddb-40f8-9939-ff881a7874db')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (69, 1, N'~/Blocks/Crm/PersonDetail/Bio.ascx', N'Person Bio', N'Person biographic/demographic information and picture (Person detail page)', '0f5922bb-cd68-40ac-bf3c-4aab1b98760c')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (70, 1, N'~/Blocks/Crm/PersonDetail/ContactInfo.ascx', N'Person Contact Info', N'Person contact information(Person Detail Page)', 'd0d57b02-6ee7-4e4b-b80b-a8640d326572')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (73, 1, N'~/Blocks/Crm/PersonDetail/GroupMembers.ascx', N'Person Group Members', N'Person Group Members (Person Detail Page)', '3e14b410-22cb-49cc-8a1f-c30ecd0e816a')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (75, 1, N'~/Blocks/Crm/PersonDetail/KeyAttributes.ascx', N'Person Key Attributes', N'Person key attributes (Person Detail Page)', '23ce11a0-6c5c-4189-8e8c-6f3c9c9e4178')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (77, 1, N'~/Blocks/Crm/PersonDetail/Notes.ascx', N'Person Notes', N'Person notes (Person Detail Page)', '6a0b3ed6-c6ca-40d4-91e0-b7b2823cc708')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (78, 1, N'~/Blocks/Core/Tags.ascx', N'Tags', N'Add tags to current context object', '351004ff-c2d6-4169-978f-5888beff982f')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (79, 1, N'~/Blocks/Administration/Metrics.ascx', N'Metrics', N'Settings for displaying and changing metrics and values.', 'ccd4f459-2e0a-40c6-8de3-ad512ae9ca74')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (80, 1, N'~/Blocks/Administration/PageRoutes.ascx', N'Page Routes', N'Allows for configuration of Page Routes', 'fee08a28-b774-4294-9f77-697fe66ca5b5')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (81, 1, N'~/Blocks/Core/AttributeCategoryView.ascx', N'Attribute Category View', N'Displays attributes and values for the selected entity and category', 'dae0f74c-dd0d-4cd4-b30f-eb487f3dd7ff')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (82, 1, N'~/Blocks/Crm/GroupTypes.ascx', N'Group Types', N'Allows for configuration of Group Types', 'c443d72b-1a9e-41e7-8e70-4e9d39ae6ac3')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (83, 1, N'~/Blocks/Crm/Groups.ascx', N'Groups', N'Allows for the configuration of Groups', '3d7fb6be-6bbd-49f7-96b4-96310af3048a')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (84, 1, N'~/Blocks/Crm/GroupRoles.ascx', N'GroupRoles', N'Allows for the configuration fof Group Roles', '89315ebc-d4bd-41e6-b1f1-929d19e66608')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (85, 1, N'~/Blocks/Administration/MarketingCampaigns.ascx', N'Marketing Campaigns', N'Allows for configuration of Marketing Campaigns', 'd99313e1-eff0-4339-8296-4fa4922b48b7')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (86, 1, N'~/Blocks/Administration/MarketingCampaignAdTypes.ascx', N'Marketing Campaign Ad Types', N'Manage Marketing Campaign Ad Types', 'd780a160-621f-4531-8369-b5dec6e7174a')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (87, 1, N'~/Blocks/Administration/ScheduledJobs.ascx', N'Scheduled Jobs', N'Manage automated jobs', 'ed2063b5-9839-46d1-8419-fe36d3b54708')
INSERT INTO [BlockType] ([Id], [IsSystem], [Path], [Name], [Description], [Guid]) VALUES (88, 1, N'~/Blocks/Administration/SiteMap.ascx', N'Site Map', N'Shows a map of Pages and Blocks', '2700a1b8-bd1a-40f1-a660-476da86d0432')
SET IDENTITY_INSERT [BlockType] OFF

-- Add 3 rows to [MarketingCampaignAdType]
SET IDENTITY_INSERT [MarketingCampaignAdType] ON
INSERT INTO [MarketingCampaignAdType] ([Id], [IsSystem], [Name], [DateRangeType], [Guid]) VALUES (1, 0, N'Bulletin', 1, '3ed7fecc-bca3-4014-a55e-93ff0d2263c9')
INSERT INTO [MarketingCampaignAdType] ([Id], [IsSystem], [Name], [DateRangeType], [Guid]) VALUES (2, 0, N'Website', 2, '851745bf-ca38-46dc-b2e4-84f4c5990f54')
INSERT INTO [MarketingCampaignAdType] ([Id], [IsSystem], [Name], [DateRangeType], [Guid]) VALUES (3, 0, N'Facebook', 2, '4d23c6c8-7457-483c-b749-330d63f3b171')
SET IDENTITY_INSERT [MarketingCampaignAdType] OFF

-- Add 30 rows to [EntityType]
SET IDENTITY_INSERT [EntityType] ON
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (1, N'Global', NULL, '70813c11-fa8c-4b3b-976e-2676e9b273f7')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (2, N'Rock.Cms.Page', NULL, 'e104dcdf-247c-4ced-a119-8cc51632761f')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (3, N'Rock.Cms.Site', NULL, '7244c10b-5d87-467b-a7f5-12dc29910ca8')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (4, N'Rock.Address.Geocode.ServiceObjects', NULL, '5222d370-20d5-4d18-a6bf-4f1fa571321d')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (5, N'Rock.Address.Geocode.StrikeIron', NULL, 'e5c50259-609e-4294-837b-26ade92bdf4e')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (6, N'Rock.Address.Geocode.TelaAtlas', NULL, 'dd23b7d2-3a12-432c-b1ee-722379ad808d')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (7, N'Rock.Address.Standardize.MelissaData', NULL, '51c31f4e-943b-4b83-956f-41685a16fcbc')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (8, N'Rock.Address.Standardize.StrikeIron', NULL, 'c4ce3228-ef10-4b4d-9fbc-95466134f4ac')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (9, N'Rock.Cms.Block', NULL, 'd89555ca-9ae4-4d62-8af1-e5e463c1ef65')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (10, N'Rock.Component.Geocode.ServiceObjects', NULL, '049faf56-e816-447a-aa60-bb22ce18bf87')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (11, N'Rock.Component.Geocode.StrikeIron', NULL, '23ea1d4f-3f7a-48e4-bb06-03ae42c669be')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (12, N'Rock.Component.Geocode.TelaAtlas', NULL, '4ad002e9-a577-475e-93a8-98084cd10cbb')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (13, N'Rock.Component.Standardize.MelissaData', NULL, '1aeb3602-0aec-49a3-b6a5-ef43900adf05')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (14, N'Rock.Component.Standardize.StrikeIron', NULL, 'eb81c9ae-7122-4f7f-af2b-414e4b53067c')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (15, N'Rock.Crm.Person', NULL, '72657ed8-d16e-492e-ac12-144c5e7567e7')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (16, N'Rock.Groups.Group', NULL, '9bbfda11-0d22-40d5-902f-60adfbc88987')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (17, N'Rock.MEF.Geocode.ServiceObjects', NULL, 'b90973e1-afc5-49dd-9145-79058c025811')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (18, N'Rock.MEF.Geocode.StrikeIron', NULL, 'b4c00a2d-06ca-4b62-8bbd-1d4fa8a3cdf4')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (19, N'Rock.MEF.Geocode.TelaAtlas', NULL, 'f71fdab3-5e7f-4db4-ae4c-f60d5f74463a')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (20, N'Rock.MEF.Standardize.MelissaData', NULL, '0ad91b7f-860e-4ae6-8d4d-1c12bf134523')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (21, N'Rock.MEF.Standardize.StrikeIron', NULL, 'e71993c1-9476-47d3-bf88-eac28d9fe65b')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (22, N'Rock.Search.Person.Address', NULL, 'c2a24344-014e-4a45-bc38-08ddbd9521c3')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (23, N'Rock.Search.Person.Email', NULL, '00095c10-72c9-4c82-844e-ae8b146de4f1')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (24, N'Rock.Search.Person.Name', NULL, '3b1d679a-290f-4a53-8e11-159bf0517a19')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (25, N'Rock.Search.Person.Phone', NULL, '5f92ecc3-4ebd-4c41-a691-c03f1da4f7bf')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (26, N'Rock.Security.Authentication.ActiveDirectory', NULL, '8057abab-6aac-4872-a11f-ac0d52ab40f6')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (27, N'Rock.Security.Authentication.Database', NULL, '4e9b798f-bb68-4c0e-9707-0928d15ab020')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (28, N'Rock.Security.ExternalAuthentication.Facebook', NULL, '2486ab81-eb35-4788-aecd-f16c5d7362f0')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (29, N'Rock.Util.ServiceJob', NULL, '52766196-a72f-4f60-997a-78e19508843d')
INSERT INTO [EntityType] ([Id], [Name], [FriendlyName], [Guid]) VALUES (30, N'Rock.Cms.MarketingCampaignAd', NULL, '048ceb0e-0673-42c5-8935-2a06ad03b850')
SET IDENTITY_INSERT [EntityType] OFF

-- Add 18 rows to [FieldType]
SET IDENTITY_INSERT [FieldType] ON
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (1, 1, N'Text', N'A Text Field', N'Rock', N'Rock.Field.Types.Text', '9c204cd0-1233-41c5-818a-c5da439445aa')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (2, 1, N'Multi-Select', N'Renders a list of checkboxes for user to select one or more values from', N'Rock', N'Rock.Field.Types.SelectMulti', 'bd0d9b57-2a41-4490-89ff-f01dab7d4904')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (3, 1, N'Boolean', N'True / False', N'Rock', N'Rock.Field.Types.Boolean', '1edafded-dfe6-4334-b019-6eecba89e05a')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (4, 1, N'Color', N'List of colors to choose from', N'Rock', N'Rock.Field.Types.Color', 'd747e6ae-c383-4e22-8846-71518e3dd06f')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (6, 1, N'Single-Select', N'Renders either a drop down list, or a list of radio-buttons for user to select one value from', N'Rock', N'Rock.Field.Types.SelectSingle', '7525c4cb-ee6b-41d4-9b64-a08048d5a5c0')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (7, 1, N'Integer', N'An integer value (whole number).', N'Rock', N'Rock.Field.Types.Integer', 'a75dfc58-7a1b-4799-bf31-451b2bbe38ff')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (8, 1, N'Page Reference', N'A reference to a specific page and route', N'Rock', N'Rock.Field.Types.PageReference', 'bd53f9c9-eba9-4d3f-82ea-de5dd34a8108')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (10, 1, N'Image', N'An image stored in the database.', N'Rock', N'Rock.Field.Types.Image', '97f8157d-a8c8-4ab3-96a2-9cb2a9049e6d')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (11, 1, N'Date', N'A date field', N'Rock', N'Rock.Field.Types.Date', '6b6aa175-4758-453f-8d83-fcd8044b5f36')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (12, 1, N'Video', N'A Video field', N'Rock', N'Rock.Field.Types.Video', 'fa398f9d-5b01-41ea-9a93-112f910a277d')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (13, 1, N'Currency', N'A Currency Field', N'Rock', N'Rock.Field.Types.Currency', '50eabc9a-a29d-4a65-984a-87891b230533')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (14, 1, N'Decimal', N'A Decimal Field', N'Rock', N'Rock.Field.Types.Decimal', 'c757a554-3009-4214-b05d-cea2b2ea6b8f')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (15, 1, N'Defined Type', N'A Defined Type Field', N'Rock', N'Rock.Field.Types.DefinedType', 'bc48720c-3610-4bcf-ae66-d255a17f1cdf')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (16, 1, N'Defined Value', N'A Defined Value Field', N'Rock', N'Rock.Field.Types.DefinedValue', '59d5a94c-94a0-4630-b80a-bb25697d74c7')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (17, 1, N'Document', N'A Document Field', N'Rock', N'Rock.Field.Types.Document', '11b0fc7b-6fd9-4d6c-ae41-c8b6ef6d4892')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (18, 1, N'Person', N'A Person Field', N'Rock', N'Rock.Field.Types.Person', 'e4eab7b2-0b76-429b-afe4-ad86d7428c70')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (19, 1, N'URL', N'A URL Field', N'Rock', N'Rock.Field.Types.Url', '85b95f22-587b-4968-851d-9196fa1fa03f')
INSERT INTO [FieldType] ([Id], [IsSystem], [Name], [Description], [Assembly], [Class], [Guid]) VALUES (20, 1, N'Html', N'An Html Field', N'Rock', N'Rock.Field.Types.HtmlField', 'dd7ed4c0-a9e0-434f-acfe-bd4f56b043df')
SET IDENTITY_INSERT [FieldType] OFF

-- Add 2 rows to [ServiceJob]
SET IDENTITY_INSERT [ServiceJob] ON
INSERT INTO [ServiceJob] ([Id], [IsSystem], [IsActive], [Name], [Description], [Assemby], [Class], [CronExpression], [LastSuccessfulRun], [LastRunDate], [LastRunDuration], [LastStatus], [LastStatusMessage], [LastRunSchedulerName], [NotificationEmails], [NotificationStatus], [Guid]) VALUES (1, 1, 1, N'Job Pulse', N'System job that allows Rock to monitor the jobs engine.', N'', N'Rock.Jobs.JobPulse', N'30 * * ? * SUN,MON,TUE,WED,THU,FRI,SAT', '2012-07-13 16:39:30.000', '2012-07-13 16:39:30.000', 2, N'Success', N'', N'RockSchedulerIIS', NULL, 4, 'cb24ff2a-5ad3-4976-883f-daf4efc1d7c7')
INSERT INTO [ServiceJob] ([Id], [IsSystem], [IsActive], [Name], [Description], [Assemby], [Class], [CronExpression], [LastSuccessfulRun], [LastRunDate], [LastRunDuration], [LastStatus], [LastStatusMessage], [LastRunSchedulerName], [NotificationEmails], [NotificationStatus], [Guid]) VALUES (7, 1, 1, N'Rock Cleanup', N'General job to clean up various areas of Rock.', NULL, N'Rock.Jobs.RockCleanup', N'0 0 1 * * ?', '2012-07-13 01:00:00.010', '2012-07-13 01:00:00.010', 0, N'Success', N'', N'RockSchedulerIIS', NULL, 4, '1a8238b1-038a-4295-9fde-c6d93002a5d7')
SET IDENTITY_INSERT [ServiceJob] OFF

-- Add 190 rows to [Attribute]
SET IDENTITY_INSERT [Attribute] ON
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (2, 1, 1, 9, N'BlockTypeId', N'6', N'PreText', N'Pre-Text', N'', N'HTML text to render before the blocks main content.', 0, 1, N'', 0, 0, '15e874b8-ff76-40fb-8713-6d0c98609734')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (4, 1, 1, 9, N'BlockTypeId', N'6', N'PostText', N'Post-Text', N'', N'HTML text to render after the blocks main content.', 1, 1, N'', 0, 0, '2e9af795-68fe-4bd6-af8b-8848cd796af5')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (17, 1, 3, 9, N'BlockTypeId', N'4', N'FacebookEnabled', N'Enable Facebook Login', N'', N'Enables the user to login using Facebook.  This assumes that the site is configured with both a Facebook App Id and Secret.', 0, 1, N'True', 0, 0, 'c5192287-f27e-4b91-97b5-a1c15490a4b9')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (18, 1, 1, 9, N'BlockTypeId', N'14', N'RootPage', N'Root Page', N'XML', N'The root page to use for the page collection. Defaults to the current page instance if not set.', 1, 0, N'', 0, 0, 'dd516fa7-966e-4c80-8523-beac91c8eeda')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (19, 1, 1, 9, N'BlockTypeId', N'14', N'XSLTFile', N'XSLT File', N'Menu XSLT', N'The path to the XSLT File', 0, 0, N'~/Assets/XSLT/PageList.xslt', 0, 1, 'd8a029f8-83be-454a-99d3-94d879ebf87c')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (20, 1, 1, 9, N'BlockTypeId', N'14', N'NumberofLevels', N'Number of Levels', N'XML', N'Number of parent-child page levels to display. Default 3.', 2, 0, N'3', 0, 1, '9909e07f-0e68-43b8-a151-24d03c795093')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (22, 1, 1, NULL, N'', N'', N'JobPulse', N'Job Pulse', N'Jobs', N'Date and time the last job pulse job ran.  This job allows an administrator to be notified if the jobs stop running.', 1, 1, N'1/1/1900', 0, 0, '254f45ee-071c-4337-a522-dfdc20b7966a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (23, 1, 1, 29, N'Class', N'Rock.Jobs.TestJob', N'EmailServerPort', N'Port', N'Email Server', N'Port of the email server', 1, 0, N'25', 0, 1, '8d27e44d-a088-496e-b708-75be93ba6651')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (24, 1, 1, 29, N'Class', N'Rock.Jobs.TestJob', N'EmailServer', N'Domain', N'Email Server', N'Domain name of your SMTP server', 0, 0, N'smtp.yourdomain.com', 0, 1, '9c06aa19-ee09-4aba-a200-fb15443d3bdc')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (33, 1, 1, 4, N'', N'', N'LicenseKey', N'License Key', N'Security', N'The Service Objects License Key', 2, 0, N'', 0, 1, '72ccd974-783d-49f9-aa37-0e609153db58')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (34, 1, 1, 4, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '590f48e8-4b53-497a-956c-69d2813baee9')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (36, 1, 1, 5, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '5262a57f-b46d-4e69-8570-622ff6a6f0b5')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (37, 1, 1, 5, N'', N'', N'UserID', N'User ID', N'Security', N'The Strike Iron User ID', 1, 0, N'', 0, 1, 'd9c9259c-5cee-461a-87b6-2da1dc0718f5')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (38, 1, 1, 5, N'', N'', N'Password', N'Password', N'Security', N'The Strike Iron Password', 2, 0, N'', 0, 1, '50517637-8ad0-4eeb-b991-d22734ef7815')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (40, 1, 3, 4, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '19e4bf22-8094-4766-9253-9408b521b032')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (41, 1, 3, 5, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, 'd79c10ab-a799-4311-8f23-914ba7c60533')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (42, 1, 1, 8, N'', N'', N'UserID', N'User ID', N'Security', N'The Strike Iron User ID', 1, 0, N'', 0, 1, '9301064d-87ae-4690-bd27-4bb010567973')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (43, 1, 1, 8, N'', N'', N'Password', N'Password', N'Security', N'The Strike Iron Password', 2, 0, N'', 0, 1, '34edaf13-628b-4af9-822c-c32a5a206a12')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (44, 1, 3, 8, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '80388551-c6e9-426a-9493-5abfe80345a6')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (45, 1, 1, 8, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '5c6019a9-3d14-400e-b2ad-f512f52bad96')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (49, 1, 1, 7, N'', N'', N'CustomerId', N'Customer Id', N'Security', N'The Melissa Data Customer ID', 1, 0, N'', 0, 1, '3e8309df-b3ad-428f-adbf-2d3bab03bc7e')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (50, 1, 1, 7, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, 'fec235d7-07a9-4ffb-b8f9-49abc25b6272')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (51, 1, 3, 7, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, 'bfb6d110-7b0d-43ea-8b8a-bd4bd54243d0')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (52, 1, 1, 6, N'', N'', N'UserName', N'User Name', N'Security', N'The Tele Atlas User Name', 1, 0, N'', 0, 1, 'b968f46d-be79-457a-a763-5cf0891863e6')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (53, 1, 1, 6, N'', N'', N'Password', N'Password', N'Security', N'The Tele Atlas Password', 2, 0, N'', 0, 1, '7485fc34-d23c-4267-b0c7-0097017652f4')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (54, 1, 1, 6, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, 'cd533421-f3e9-480a-b37f-eb19e83e92d2')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (55, 1, 3, 6, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '7a9209a2-3071-4327-9d7d-ab466d923bb7')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (56, 1, 1, 6, N'', N'', N'EZLocateService', N'EZ-Locate Service', N'Service', N'The EZ-Locate Service to use (default: USA_Geo_002)', 2, 0, N'USA_Geo_002', 0, 1, 'c72b7ead-e8a1-4558-b63b-85a8656ef0e3')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (57, 1, 1, 9, N'BlockTypeId', N'36', N'EntityQualifierColumn', N'Entity Qualifier Column', N'Applies To', N'The entity column to evaluate when determining if this attribute applies to the entity', 1, 0, N'', 0, 0, 'ecd5b86c-2b48-4548-9fe9-7ac6f6fa8106')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (58, 1, 1, 9, N'BlockTypeId', N'36', N'EntityQualifierValue', N'Entity Qualifier Value', N'Applies To', N'The entity column value to evaluate.  Attributes will only apply to entities with this value', 2, 0, N'', 0, 0, 'fce1e87d-f816-4ad5-ae60-1e71942c547c')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (59, 1, 1, 9, N'BlockTypeId', N'36', N'Entity', N'Entity', N'Applies To', N'Entity Name', 0, 0, N'', 0, 0, '5b33fe25-6bf0-4890-91c6-49fb1629221e')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (60, 1, 1, 9, N'BlockTypeId', N'37', N'EntityQualifierValue', N'Entity Qualifier Value', N'Applies To', N'The entity column value to evaluate.  Attributes will only apply to entities with this value', 2, 0, N'', 0, 0, '26280214-aaa0-475d-b3e2-f887085551c5')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (61, 1, 1, 9, N'BlockTypeId', N'37', N'Entity', N'Entity', N'Applies To', N'Entity Name', 0, 0, N'', 0, 0, '981d5d1c-504d-4a0b-8dc7-6d01f4e51af8')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (62, 1, 1, 9, N'BlockTypeId', N'37', N'EntityId', N'Entity Id', N'Entity', N'The entity id that values apply to', 3, 0, N'', 0, 0, '680ed2db-049b-4958-b36a-f4f0ccdf6daa')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (63, 1, 1, 9, N'BlockTypeId', N'37', N'EntityQualifierColumn', N'Entity Qualifier Column', N'Applies To', N'The entity column to evaluate when determining if this attribute applies to the entity', 1, 0, N'', 0, 0, '070a37ab-1f87-4f21-b27e-bccd83f5db7e')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (64, 1, 1, 9, N'BlockTypeId', N'40', N'SentLoginCaption', N'Sent Login', N'Captions', N'', 4, 0, N'Your username has been emailed to you.  If you''ve forgotten your password, the email includes a link to reset your password.', 0, 0, '4a2c2a1b-f0ce-483a-82b6-54ec740ae0ee')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (65, 1, 1, 9, N'BlockTypeId', N'40', N'ConfirmCaption', N'Confirm', N'Captions', N'', 5, 0, N'Because you''ve selected an existing person, we need to have you confirm the email address you entered belongs to you. We''ve sent you an email that contains a link for confirming.  Please click the link in your email to continue.', 0, 0, '27a35511-8263-41e1-88f8-f284e2339248')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (66, 1, 1, 9, N'BlockTypeId', N'40', N'SuccessCaption', N'Success', N'Captions', N'', 6, 0, N'{0}, Your account has been created', 0, 0, '9c8ff3e7-2b9a-4652-ab7d-bd7d570ac68f')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (67, 1, 3, 9, N'BlockTypeId', N'40', N'Duplicates', N'Check for Duplicates', N'', N'Should people with the same email and last name be presented as a possible pre-existing record for user to choose from.', 0, 0, N'true', 0, 0, '029da832-10ec-49c9-8a16-b10126759a9a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (68, 1, 1, 9, N'BlockTypeId', N'40', N'FoundDuplicateCaption', N'Found Duplicate', N'Captions', N'', 2, 0, N'There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?', 0, 0, '65fcfd11-9fee-42f5-9cf5-a6237462d2cb')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (69, 1, 1, 9, N'BlockTypeId', N'40', N'ExistingAccountCaption', N'Existing Account', N'Captions', N'', 3, 0, N'{0}, you already have an existing account.  Would you like us to email you the username?', 0, 0, '3465db21-3139-4eaa-933d-fb40cc5b2ab7')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (70, 1, 1, NULL, N'', N'', N'OrganizationName', N'Organization Name', N'Organization', N'The name of your organization', 0, 0, N'Our Organization Name', 0, 0, '410bf494-0714-4e60-afbd-ad65899a12be')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (71, 1, 1, 9, N'BlockTypeId', N'39', N'InvalidCaption', N'Invalid', N'Captions', N'', 5, 0, N'The confirmation code you''ve entered is not valid.  Please enter a valid confirmation code or <a href=''{0}''>create a new account</a>', 0, 0, 'add1758c-31ea-4e75-8f33-468181d2ecde')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (72, 1, 1, 9, N'BlockTypeId', N'39', N'DeleteCaption', N'Delete', N'Captions', N'', 3, 0, N'Are you sure you want to delete the ''{0}'' account?', 0, 0, '72386ce2-df26-4fbb-ba42-5c5eafcaac94')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (73, 1, 1, 9, N'BlockTypeId', N'39', N'DeletedCaption', N'Deleted', N'Captions', N'', 4, 0, N'The account has been deleted.', 0, 0, '9279a85f-1443-4f42-960c-33cb0f608111')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (74, 1, 1, 9, N'BlockTypeId', N'39', N'ConfirmedCaption', N'Confirmed', N'Captions', N'', 0, 0, N'{0}, Your account has been confirmed.  Thank you for creating the account', 0, 0, '22fcb059-aa40-45d9-becb-c22d15a3d41a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (75, 1, 1, NULL, N'', N'', N'SMTPServer', N'SMTP Server', N'EmailConfig', N'The server to use for relaying SMTP Messages', 10, 0, N'', 0, 0, '1c4e71dd-ed38-4586-93cf-a847003ec594')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (76, 1, 7, NULL, N'', N'', N'SMTPPort', N'SMTP Port', N'EmailConfig', N'The Port to use for SMTP Relaying', 11, 0, N'25', 0, 0, '3c5f2bf8-8d8a-46d4-9182-2a25d32851ea')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (77, 1, 1, NULL, N'', N'', N'SMTPUserName', N'SMTP User Name', N'EmailConfig', N'The Username to use when relaying SMTP Messages', 12, 0, N'', 0, 0, '40690f08-1433-4046-8f22-b4b16075f1cf')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (78, 1, 1, NULL, N'', N'', N'SMTPPassword', N'SMTP Password', N'EmailConfig', N'The password to use when relaying SMTP Messages', 13, 0, N'', 0, 0, '996b04c9-45e5-4dc1-a84b-27d14b53dcc6')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (79, 1, 3, NULL, N'', N'', N'SMTPUseSSL', N'SMTP Use SSL', N'EmailConfig', N'Should SSL be used when relaying SMTP Messages', 14, 0, N'False', 0, 0, '10dd8248-dc68-4206-abfd-da4e8bb849e3')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (80, 1, 1, 9, N'BlockTypeId', N'40', N'ConfirmRoute', N'Confirm Route', N'', N'The URL Route for Confirming an account', 1, 0, N'', 0, 1, 'b9e9ee84-7b64-4ac2-9c61-8228700954ba')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (81, 1, 1, 9, N'BlockTypeId', N'41', N'SuccessCaption', N'Success', N'Captions', N'', 2, 0, N'Your password has been changed', 0, 0, '727fe7da-c624-4590-94e1-c206324725cb')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (82, 1, 1, 9, N'BlockTypeId', N'41', N'InvalidUserNameCaption', N'Invalid UserName', N'Captions', N'', 0, 0, N'The User Name/Password combination is not valid.', 0, 0, 'b254930d-9bbd-4bb5-b25f-84088a9dce28')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (83, 1, 1, 9, N'BlockTypeId', N'41', N'InvalidPasswordCaption', N'Invalid Password', N'Captions', N'', 1, 0, N'The User Name/Password combination is not valid.', 0, 0, '979a3dca-146e-47e7-bf08-dc7025dc8e22')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (84, 1, 1, 9, N'BlockTypeId', N'39', N'ResetPasswordCaption', N'Reset Password', N'Captions', N'', 1, 0, N'{0}, Enter a new password for your ''{1}'' account', 0, 0, 'c3fb6a2b-7711-4c52-af10-0b1511198cdd')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (85, 1, 1, 9, N'BlockTypeId', N'39', N'PasswordResetCaption', N'Password Reset', N'Captions', N'', 2, 0, N'{0}, The password for your ''{1}'' account has been changed', 0, 0, '040d281f-b535-452e-b59e-7c45985c2937')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (86, 1, 1, 9, N'BlockTypeId', N'42', N'SuccessCaption', N'Success', N'Captions', N'', 2, 0, N'Your user name has been sent to the email address you entered', 0, 0, '488e438f-3ba3-4d3b-a1b0-d11d85752e06')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (87, 1, 1, 9, N'BlockTypeId', N'42', N'HeadingCaption', N'Heading', N'Captions', N'', 0, 0, N'Enter your email address below and we''ll send you your account user name', 0, 0, '6efaf3cd-327a-4472-aa20-09af1ef8bc78')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (88, 1, 1, 9, N'BlockTypeId', N'42', N'InvalidEmailCaption', N'Invalid Email', N'Captions', N'', 1, 0, N'There are not any accounts for the email address you entered', 0, 0, '87e7485a-ff22-48e7-bb4a-58e66b305d62')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (89, 1, 1, 19, N'', N'', N'Password', N'Password', N'Security', N'The Tele Atlas Password', 2, 0, N'', 0, 1, '90432af9-f419-412e-87c4-23d0f91ac02a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (90, 1, 1, 19, N'', N'', N'EZLocateService', N'EZ-Locate Service', N'Service', N'The EZ-Locate Service to use (default: USA_Geo_002)', 2, 0, N'USA_Geo_002', 0, 1, '51f322ba-6b8f-43e8-b78d-37ad75640997')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (91, 1, 1, 19, N'', N'', N'UserName', N'User Name', N'Security', N'The Tele Atlas User Name', 1, 0, N'', 0, 1, 'd36ae1c2-1a2b-484b-b0a2-50ae37acc3d6')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (92, 1, 3, 19, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, 'edf90832-4f9c-41c4-bdd1-367bf0887d8d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (93, 1, 1, 19, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '5da149e7-1043-40b4-a288-266271f5cc6d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (94, 1, 1, 17, N'', N'', N'LicenseKey', N'License Key', N'Security', N'The Service Objects License Key', 2, 0, N'', 0, 1, '44e19cec-ed1c-4997-a7b0-f30a2c83cb89')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (95, 1, 3, 17, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, 'bc31ed74-d1ef-45f6-809c-ce5f5788dfa3')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (96, 1, 1, 17, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '6ec59763-50bf-4eea-8424-acda293f01c5')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (97, 1, 1, 18, N'', N'', N'UserID', N'User ID', N'Security', N'The Strike Iron User ID', 1, 0, N'', 0, 1, '82f06c17-f314-4086-8ff8-2e75a5c68a99')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (98, 1, 1, 18, N'', N'', N'Password', N'Password', N'Security', N'The Strike Iron Password', 2, 0, N'', 0, 1, '48cb3154-b2bc-43a2-9f1e-cea9e81c1c18')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (99, 1, 3, 18, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '342fb5b8-b551-415d-93f3-1aa89b3e5bc5')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (100, 1, 1, 18, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '16fa8cc4-fdf8-4e1b-b7bd-bec602e6c563')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (101, 1, 1, 21, N'', N'', N'Password', N'Password', N'Security', N'The Strike Iron Password', 2, 0, N'', 0, 1, 'f108d456-5c77-42d0-a7eb-ea1189b6ef2a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (102, 1, 1, 21, N'', N'', N'UserID', N'User ID', N'Security', N'The Strike Iron User ID', 1, 0, N'', 0, 1, 'bec1b322-09e0-4333-943d-b9170edbddbb')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (103, 1, 3, 21, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '0b8e0e4b-4a6c-4da8-bb65-e3d2b38bc44e')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (104, 1, 1, 21, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '84c1e686-b9a0-41ed-87f4-ff2482103b7e')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (105, 1, 1, 20, N'', N'', N'CustomerId', N'Customer Id', N'Security', N'The Melissa Data Customer ID', 1, 0, N'', 0, 1, 'b728ac57-729b-4596-833a-64914d609096')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (106, 1, 3, 20, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, 'bf0424e3-26e2-46f7-b289-d050ad733294')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (107, 1, 1, 20, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '0f607291-e63e-4766-bdf6-c5bb88b57df9')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (108, 1, 1, 12, N'', N'', N'EZLocateService', N'EZ-Locate Service', N'Service', N'The EZ-Locate Service to use (default: USA_Geo_002)', 2, 0, N'USA_Geo_002', 0, 1, 'a7f71469-5739-460f-9adb-dc399cd9b1a5')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (109, 1, 1, 12, N'', N'', N'UserName', N'User Name', N'Security', N'The Tele Atlas User Name', 1, 0, N'', 0, 1, 'ad5fa137-5622-49af-844e-6a973ec0403a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (110, 1, 1, 12, N'', N'', N'Password', N'Password', N'Security', N'The Tele Atlas Password', 2, 0, N'', 0, 1, '7f251cff-1fef-407d-9722-8840b6918c30')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (111, 1, 1, 12, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '0b4688a9-37d6-48ac-bef8-64ff63971cc9')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (112, 1, 3, 12, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, 'ea32844c-c0d1-456a-b1b9-110a271ce9a8')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (113, 1, 1, 10, N'', N'', N'LicenseKey', N'License Key', N'Security', N'The Service Objects License Key', 2, 0, N'', 0, 1, '0ce7689e-fb9f-4a48-8542-744384290320')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (114, 1, 1, 10, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '0f9d7966-9d0a-420c-a0ae-260c82613a3d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (115, 1, 3, 10, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '0627ae3b-f2d6-44fa-8b6f-1441f2e46b8d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (116, 1, 1, 11, N'', N'', N'UserID', N'User ID', N'Security', N'The Strike Iron User ID', 1, 0, N'', 0, 1, 'ecacb606-0f86-41de-928a-bb995113714d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (117, 1, 1, 11, N'', N'', N'Password', N'Password', N'Security', N'The Strike Iron Password', 2, 0, N'', 0, 1, '93987b6a-343a-42bf-92c7-ea7c9533b092')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (118, 1, 1, 11, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, 'ff93823d-0acd-4e45-8b90-3add4a20b79c')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (119, 1, 3, 11, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '04b0c668-5725-4180-9342-26c7862d7577')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (120, 1, 1, 14, N'', N'', N'UserID', N'User ID', N'Security', N'The Strike Iron User ID', 1, 0, N'', 0, 1, '01bb9e72-ac57-4d95-922d-2dbc1dee311e')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (121, 1, 1, 14, N'', N'', N'Password', N'Password', N'Security', N'The Strike Iron Password', 2, 0, N'', 0, 1, 'df56d543-9465-4f27-9a42-0f094db92299')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (122, 1, 1, 14, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, 'e93f399f-ee85-4ccc-85a7-4938e92234ea')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (123, 1, 3, 14, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, 'c0168a17-fa44-45bb-b12a-f6a24230a15e')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (124, 1, 1, 13, N'', N'', N'CustomerId', N'Customer Id', N'Security', N'The Melissa Data Customer ID', 1, 0, N'', 0, 1, '8774a579-894c-46ff-83b5-bcc21cb6ff30')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (125, 1, 1, 13, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '3dc7bd74-f7e3-4735-a6d2-5921adaf28e6')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (126, 1, 3, 13, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '7137ce8a-85e0-4811-a6df-699bda2301cc')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (127, 1, 1, 9, N'BlockTypeId', N'53', N'ComponentContainer', N'Component Container', N'', N'The Rock Extension Component Container to manage', 0, 0, N'', 0, 1, '259af14d-0214-4be4-a7bf-40423ea07c99')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (129, 1, 10, NULL, N'', N'', N'EmailHeaderLogo', N'Email Header Logo', N'EmailFormat', N'Logo image to be used at the top off all emails.', 1, 0, N'assets/images/email-header.jpg', 0, 0, 'b95c446d-6a3c-4672-8718-cf988c447d0d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (130, 1, 1, NULL, N'', N'', N'EmailBackgroundColor', N'Email Background Color', N'EmailFormat', N'Background color (format #ffffff) that will be used for default emails.', 2, 0, N'#cccccc', 0, 0, '56c8ec3f-1f7b-410a-a742-42ea217e3302')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (131, 1, 1, NULL, N'', N'', N'OrganizationEmail', N'Organization Email', N'Organization', N'The primary email address for the organization.', 0, 0, N'info@organizationname.com', 0, 0, '6837554f-93b3-4d46-ba48-a4059fa1766f')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (132, 1, 1, NULL, N'', N'', N'OrganizationWebsite', N'Organization Website', N'Organization', N'The primary website for the organization.', 0, 0, N'www.organization.com', 0, 0, '118a083b-3f28-4d17-8b19-cc6859f89f33')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (133, 1, 1, NULL, N'', N'', N'OrganizationPhone', N'Organization Phone', N'Organization', N'The primary phone number of the organization.', 0, 0, N'', 0, 0, '85716596-6aea-4887-830f-744d22e28a0d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (134, 1, 1, NULL, N'', N'', N'EmailBodyTextColor', N'Email Body Text Color', N'EmailFormat', N'The text color (format #000000) to use for the body font of the email.', 0, 0, N'#000000', 0, 0, '724fa692-8e3e-4f43-b3ca-0d6767aad53a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (135, 1, 1, NULL, N'', N'', N'EmailBodyTextLinkColor', N'Email Body Text Link Color', N'EmailFormat', N'The text link color (format #000000) to use for the HTML anchors in the body of the email.', 0, 0, N'#006699', 0, 0, 'a910c483-0be0-400f-b25d-131d78f124a0')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (136, 1, 1, NULL, N'', N'', N'EmailFooterTextColor', N'Email Footer Text Color', N'EmailFormat', N'The text color (format #000000) to use for the footer font of the email.', 0, 0, N'#4f4e4e', 0, 0, '61e60e2a-ea86-4769-9029-f803b06849de')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (137, 1, 1, NULL, N'', N'', N'EmailFooterTextLinkColor', N'Email Footer Text Link Color', N'EmailFormat', N'The text link color (format #000000) to use for the HTML anchors in the footer of the email.', 0, 0, N'#212937', 0, 0, 'd64c84bf-972d-4458-a2c5-b8e05002d833')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (138, 1, 7, 29, N'Class', N'Rock.Jobs.RockCleanup', N'HoursKeepUnconfirmedAccounts', N'Hours to Keep Unconfirmed Accounts', N'General', N'The number of hours to keep user accounts that have not been confirmed (default is 48 hours.)', 0, 0, N'48', 0, 0, '6c0e84a3-e2f8-480a-ab96-34d24c2acf40')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (139, 1, 1, NULL, N'', N'', N'EmailBodyBackgroundColor', N'Email Body Background Color', N'EmailFormat', N'The background color of the body of the email.', 0, 0, N'#fff', 0, 0, 'dc2a8545-d61c-4d45-b593-a7326e3be3a4')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (140, 1, 1, NULL, N'', N'', N'EmailHeader', N'Email Header', N'EmailFormat', N'The standard email header that wraps the email text and shows a header image.', 0, 0, N'<style>   body {   background-color: {{ EmailBodyBackgroundColor }};             font-family: Verdana, Arial, Helvetica, sans-serif;             font-size: 12px;             line-height: 1.3em;             margin: 0;   padding: 0;  }    a {   color: {{ EmailBodyTextLinkColor }};  }   </style>    <table style=''text-align: center; background-color: {{ EmailBackgroundColor }}; margin: 0pt'' border=0 cellSpacing=0 cellPadding=0 width=''100%'' align=center>           <tbody>                <tr>                     <td style=''background-color: {{ EmailBackgroundColor }}; margin: 0pt auto'' valign=top align=middle>    <!-- Begin Layout -->                       <table style=''text-align: left; background-color: {{ EmailBodyBackgroundColor }}; margin: 0px auto; width: 550px'' border=0 cellspacing=0 cellpadding=0 width=550>    <tbody>                               <tr>                                    <td valign=top align=left>       <!-- Header Start -->                                     <table style=''width: 100%'' border=0 cellSpacing=0 cellPadding=0 width=''100%''>                                         <tbody>                                              <tr>                                                   <td style=''height: 51px''>          <img style=''border-bottom: medium none; border-left: medium none; padding-bottom: 0pt; margin: 0px; padding-left: 0pt; padding-right: 0pt; border-top: medium none; border-right: medium none; padding-top: 0pt'' src=''{{ Config_BaseUrl }}{{ EmailHeaderLogo }}''>          </td>                                              </tr>                                         </tbody>                                     </table>                                     <!-- Header End -->                                                     <table style=''padding-bottom: 18px; width: 100%; background-color: {{ EmailBodyBackgroundColor }};'' cellspacing=0 cellpadding=20 >                                         <tbody>                                              <tr>                                                   <td>                                                  <!-- Main Text Start -->', 0, 0, 'ebc67f76-7305-4108-ad32-e2531eab1637')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (141, 1, 1, NULL, N'', N'', N'EmailFooter', N'Email Footer', N'EmailFormat', N'Standard footer text that contains the name of the organization, etc.', 0, 0, N'<!-- Main Text End -->         </td>                                  </tr>                                                          </tbody>                         </table>              <!-- Footer Start -->                                     <table cellpadding=20 align=center style=''background-color: {{ EmailBackgroundColor }}; width: 100%;''>                                         <tbody>                                              <tr>                                                   <td>                                                    <p style=''text-align: center; color: {{ EmailFooterTextColor }}''><span style=''font-size: 16px''>{{ OrganizationName }} | {{ OrganizationPhone }} <br>            <a href=''mailto:{{ OrganizationEmail }}''>{{ OrganizationEmail }}</A> | <span style=''color: {{ EmailFooterTextLink Color }};''><a style=''color: {{ EmailFooterTextLinkColor }}'' href=''{{ OrganizationWebsite }}''>{{ OrganizationWebsite }}</A></span></span></p>                                                   </td>                                              </tr>                                         </tbody>                                     </table>                                        <!-- Footer End -->     <!-- End Layout -->     </td>                   </tr>              </tbody>          </table>', 0, 0, 'ed326066-4a91-412a-805c-40dedae8f61a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (142, 1, 7, 29, N'Class', N'Rock.Jobs.RockCleanup', N'DaysKeepExceptions', N'Days to Keep Exceptions in Log', N'General', N'The number of days to keep exceptions in the exception log (default is 14 days.)', 0, 0, N'14', 0, 0, '643902ea-2534-4d5d-ac5b-ceff53d98ea8')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (143, 1, 3, NULL, N'', N'', N'Log404AsException', N'Log 404s As Exceptions', N'Config', N'Add 404 (File Not Found) errors as exceptions in the exception log. Warning this will impact performance.', 0, 0, N'False', 0, 0, 'b4947ce4-3e1b-4679-8b7d-b44d0d4a7d97')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (144, 1, 1, NULL, N'', N'', N'EmailExceptionsList', N'Email Exceptions List', N'Config', N'Comma separated list of email addresses to send exception notifications to.', 0, 0, N'', 0, 0, 'f7d2fe87-537d-4452-b503-3991d15bd242')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (150, 1, 1, NULL, N'', N'', N'SendGridUsername', N'SendGrid Username', N'EmailConfig', N'Username of the SendGrid account.', 0, 0, N'', 0, 0, 'e49fe2ed-f67a-4e60-b297-a7c5220c056c')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (151, 1, 1, NULL, N'', N'', N'SendGridPassword', N'SendGrid Password', N'EmailConfig', N'SendGrid account password.', 0, 0, N'', 0, 0, 'a96616c8-efc1-4e7d-a6e1-76fca2e5ab52')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (152, 1, 12, 16, N'GroupTypeId', N'9', N'Video', N'Video', N'', N'The Video URL', 0, 0, N'', 0, 0, 'c5e3b8b6-f5f7-4304-9bd5-90faae39830f')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (153, 1, 12, 16, N'GroupTypeId', N'9', N'VideoDuration', N'Video Duration', N'', N'The Duration of the video', 0, 0, N'00:00:00', 0, 0, '5315301b-7829-4a67-bdf6-5c133bbf1826')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (154, 1, 7, 9, N'BlockTypeId', N'58', N'GroupRole', N'Group Role', N'Behavior', N'The Group Role to use when person is added to group', 2, 0, N'', 0, 1, 'f02349d9-6114-4db6-83ac-29e6d0ed3eab')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (155, 1, 1, 9, N'BlockTypeId', N'58', N'DurationAttributeKey', N'Duration Attribute Key', N'Behavior', N'The key of the duration attribute', 3, 0, N'Duration', 0, 0, '03d603c0-e5dd-443f-aac3-67dacc18639f')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (156, 1, 1, 9, N'BlockTypeId', N'58', N'VideoAttributeKey', N'Video Attribute Key', N'Behavior', N'The key of the video attribute', 3, 0, N'Video', 0, 0, '33bcc2d1-c653-4712-a148-9a24946c7854')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (157, 1, 7, 9, N'BlockTypeId', N'58', N'GroupLevels', N'Group Levels', N'Behavior', N'The Group Role to use when person is added to group', 1, 0, N'', 0, 1, 'a85e7efa-63af-47ea-95b7-c3ef8dab86d5')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (158, 1, 7, 9, N'BlockTypeId', N'58', N'GroupId', N'Group Id', N'Behavior', N'The Group Id of the parent group', 0, 0, N'', 0, 0, 'c0cf2f3c-53d6-423b-8634-a31c9ba78bbc')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (160, 1, 7, 9, N'BlockTypeId', N'6', N'CacheDuration', N'Cache Duration', N'', N'Number of seconds to cache the content.', 2, 0, N'0', 0, 0, '4dfdb295-6d0f-40a1-bef9-7b70c56f66c4')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (161, 1, 3, 9, N'BlockTypeId', N'6', N'RequireApproval', N'Require Approval', N'Advanced', N'Require that content be approved?', 6, 0, N'False', 0, 0, 'ec2b701b-4c1d-4f3f-9c77-a73c75d7ff7a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (162, 1, 3, 9, N'BlockTypeId', N'6', N'SupportVersions', N'Support Versions', N'Advanced', N'Support content versioning?', 5, 0, N'False', 0, 0, '7c1ce199-86cf-4eae-8ab3-848416a72c58')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (163, 1, 1, 9, N'BlockTypeId', N'6', N'ContextParameter', N'Context Parameter', N'', N'Query string parameter to use for ''personalizing'' content based on unique values.', 3, 0, N'', 0, 0, '3ffc512d-a576-4289-b648-905fd7a64abb')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (164, 1, 1, 9, N'BlockTypeId', N'6', N'ContextName', N'Context Name', N'', N'Name to use to further ''personalize'' content.  Blocks with the same name, and referenced with the same context parameter will share html values.', 4, 0, N'', 0, 0, '466993f7-d838-447a-97e7-8bbda6a57289')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (165, 1, 7, 9, N'BlockTypeId', N'62', N'GroupRole', N'Group Role', N'Behavior', N'The Group Role to use when person is added to group', 2, 0, N'', 0, 1, '260bd7b0-6c0e-45e2-8d0e-5a5d831ed42b')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (166, 1, 1, 9, N'BlockTypeId', N'62', N'DurationAttributeKey', N'Duration Attribute Key', N'Behavior', N'The key of the duration attribute', 3, 0, N'Duration', 0, 0, '77920017-e8a3-401a-a488-73fae0d86a4f')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (167, 1, 1, 9, N'BlockTypeId', N'62', N'VideoAttributeKey', N'Video Attribute Key', N'Behavior', N'The key of the video attribute', 3, 0, N'Video', 0, 0, '6b3f7660-e7fb-49a4-becf-da3f4bc3b642')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (168, 1, 7, 9, N'BlockTypeId', N'62', N'GroupLevels', N'Group Levels', N'Behavior', N'The Group Role to use when person is added to group', 1, 0, N'', 0, 1, '554e21e7-d466-4d7c-897a-3bb71b2eef70')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (169, 1, 7, 9, N'BlockTypeId', N'62', N'GroupId', N'Group Id', N'Behavior', N'The Group Id of the parent group', 0, 0, N'', 0, 0, '89e9ac8e-9d19-4fc3-9f6d-29a7e20b932e')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (170, 1, 1, 9, N'BlockTypeId', N'59', N'EntityQualifierValue', N'Entity Qualifier Value', N'Filter', N'The entity column value to evaluate.  Attributes will only apply to entities with this value', 2, 0, N'', 0, 0, 'a553e5aa-67e7-4d0c-8a1f-b046f9447c0a')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (171, 1, 1, 9, N'BlockTypeId', N'59', N'AttributeCategory', N'Attribute Category', N'Filter', N'Attribute Category', 3, 0, N'', 0, 1, '082b2466-16ec-4a4e-86a7-705ea2b6d794')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (172, 1, 1, 9, N'BlockTypeId', N'59', N'Entity', N'Entity', N'Filter', N'Entity Name', 0, 0, N'', 0, 0, 'ff203a96-e0c4-45cc-a495-5a80b29e1bee')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (173, 1, 1, 9, N'BlockTypeId', N'59', N'EntityQualifierColumn', N'Entity Qualifier Column', N'Filter', N'The entity column to evaluate when determining if this attribute applies to the entity', 1, 0, N'', 0, 0, '7e71ab14-6e87-4a2e-97c4-52b82e47e181')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (174, 1, 11, 15, N'', N'', N'BaptismDate', N'Baptism Date', N'Membership', N'The date the person was baptized.', 0, 0, N'', 0, 0, 'd42763fa-28e9-4a55-a25a-48998d7d7fef')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (175, 1, 1, NULL, N'', N'', N'PackageSourceUrl', N'Package Source URL', N'Config', N'URL to the Rock Quarry plugin source repository API (v2)', 0, 0, N'http://quarry.rockchms.com/api/v2/', 0, 0, '306e7e7c-9416-4098-9c25-488380b940a5')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (176, 1, 1, 9, N'BlockTypeId', N'36', N'EntityId', N'Entity Id', N'Set Values', N'The entity id that values apply to', 4, 0, N'', 0, 0, 'cbb56d68-3727-42b9-bf13-0b2b593fb328')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (177, 1, 3, 9, N'BlockTypeId', N'36', N'SetValues', N'Allow Setting of Values', N'Set Values', N'Should UI be available for setting values of the specified Entity ID?', 3, 0, N'false', 0, 0, '018c0016-c253-44e4-84db-d166084c5cad')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (178, 1, 1, 24, N'', N'', N'SearchLabel', N'Search Label', N'Behavior', N'The text to display in the search type dropdown', 1, 0, N'Name', 0, 0, '3fdd185d-3fe4-4be9-b769-26c6fc5c15f9')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (179, 1, 1, 24, N'', N'', N'ResultURL', N'Result URL', N'Behavior', N'The url to redirect user to after they have entered search text.  (use ''{0}'' for the search text)', 2, 0, N'', 0, 1, '62fe83a4-e36f-4810-b650-6f8d75f53f54')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (180, 1, 1, 24, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, 'c9df58f0-49a6-4f09-8174-08dcb38c95c7')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (181, 1, 1, 24, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '655a2303-4c31-4ae4-a3eb-bce266f7266b')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (182, 1, 1, 25, N'', N'', N'SearchLabel', N'Search Label', N'Behavior', N'The text to display in the search type dropdown', 1, 0, N'Phone', 0, 0, '02f3eb66-fddc-497b-a288-27a96b75bf42')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (183, 1, 1, 25, N'', N'', N'ResultURL', N'Result URL', N'Behavior', N'The url to redirect user to after they have entered search text.  (use ''{0}'' for the search text)', 2, 0, N'', 0, 1, '9bda29dd-f408-4e5a-983f-d053c9bba5ad')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (184, 1, 1, 25, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '67b770a1-7e61-4a60-a894-ba23ec9c85d1')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (185, 1, 1, 25, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '8a652104-6031-4e87-ba03-8fc3dafe4eee')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (186, 1, 1, 23, N'', N'', N'SearchLabel', N'Search Label', N'Behavior', N'The text to display in the search type dropdown', 1, 0, N'Email', 0, 0, '047de9db-8b67-4318-a4cb-baac2dc5e132')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (187, 1, 1, 23, N'', N'', N'ResultURL', N'Result URL', N'Behavior', N'The url to redirect user to after they have entered search text.  (use ''{0}'' for the search text)', 2, 0, N'', 0, 1, '73429747-d55e-43f1-8ca4-e1def3ecf9a7')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (188, 1, 1, 23, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '1a01a90f-14dd-4a48-9311-88112acd7a66')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (189, 1, 1, 23, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '3bb014b2-c8f5-4480-9db4-a740d6a53716')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (190, 1, 1, 22, N'', N'', N'SearchLabel', N'Search Label', N'Behavior', N'The text to display in the search type dropdown', 1, 0, N'Address', 0, 0, 'a179f2ba-ce19-4bc7-a926-af36e8103364')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (191, 1, 1, 22, N'', N'', N'ResultURL', N'Result URL', N'Behavior', N'The url to redirect user to after they have entered search text.  (use ''{0}'' for the search text)', 2, 0, N'', 0, 1, '087e5d6e-7662-4d47-bd10-a22e113c3ed2')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (192, 1, 1, 22, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, 'c377e604-ef85-4b15-bd16-b7c96bcf0dee')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (193, 1, 1, 22, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, 'ebd8d448-6df2-4d0a-823d-ca969501510f')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (194, 1, 1, 9, N'BlockTypeId', N'68', N'Entity', N'Entity', N'Entity', N'Entity Name', 0, 0, N'', 0, 0, '94d56bcc-8e62-495b-b722-6ce0b5eedef4')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (195, 1, 1, 9, N'BlockTypeId', N'68', N'EntityQualifierColumn', N'Entity Qualifier Column', N'Entity', N'The entity column to evaluate when determining if this attribute applies to the entity', 1, 0, N'', 0, 0, 'b4d5a749-20d3-4a55-987b-147cf4ee2b3f')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (196, 1, 1, 9, N'BlockTypeId', N'68', N'EntityQualifierValue', N'Entity Qualifier Value', N'Entity', N'The entity column value to evaluate.  Attributes will only apply to entities with this value', 2, 0, N'', 0, 0, 'b4ecff00-2843-4ade-94cf-443fb95c4eba')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (197, 1, 3, 9, N'BlockTypeId', N'68', N'GlobalTags', N'Global Tags', N'Entity', N'Edit global tags (vs. personal tags)?', 3, 0, N'false', 0, 0, '6df40dd2-ac15-47d6-86a0-f23c333aa47c')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (198, 1, 1, 9, N'BlockTypeId', N'78', N'ContextEntityType', N'Context Entity Type', N'Filter', N'Context Entity Type', 0, 0, N'', 0, 0, 'e7e7a6e4-445b-4a19-874f-2aeb510c8d7d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (199, 1, 1, 9, N'BlockTypeId', N'78', N'EntityQualifierColumn', N'Entity Qualifier Column', N'Filter', N'The entity column to evaluate when determining if this attribute applies to the entity', 1, 0, N'', 0, 0, '237a123c-bfd4-43f6-860f-3c61815ac8ab')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (200, 1, 1, 9, N'BlockTypeId', N'78', N'EntityQualifierValue', N'Entity Qualifier Value', N'Filter', N'The entity column value to evaluate.  Attributes will only apply to entities with this value', 2, 0, N'', 0, 0, '75fa3234-76ce-45dc-82ba-14615b3e60c4')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (201, 1, 7, 9, N'BlockTypeId', N'73', N'GroupType', N'Group Type', N'Behavior', N'The type of group to display.  Any group of this type that person belongs to will be displayed', 0, 0, N'0', 0, 0, 'b84eb1cb-e719-4444-b739-b0112aa20bba')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (202, 1, 1, 9, N'BlockTypeId', N'73', N'GroupRoleFilter', N'GroupRoleFilter', N'Behavior', N'Delimited list of group role id''s that if entered, will only show groups where selected person is one of the roles.', 1, 0, N'', 0, 0, '19eaafbb-0669-4bc7-b69c-4dadb904ba8b')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (203, 1, 3, 9, N'BlockTypeId', N'73', N'IncludeSelf', N'IncludeSelf', N'Behavior', N'Should the current person be included in list of group members?', 2, 0, N'false', 0, 0, 'bd82a9ca-bb0c-47b4-90fd-3a8d4fdbdcea')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (204, 1, 3, 9, N'BlockTypeId', N'73', N'IncludeLocations', N'IncludeLocations', N'Behavior', N'Should locations be included?', 3, 0, N'false', 0, 0, '0504fb69-c7ee-432f-b232-a705aacd4858')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (205, 1, 1, 9, N'BlockTypeId', N'73', N'XsltFile', N'XsltFile', N'Behavior', N'XSLT File to use.', 4, 0, N'GroupMembers.xslt', 0, 0, '69a88bcd-02da-4600-ae9b-adf30d41ee58')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (206, 1, 1, 9, N'BlockTypeId', N'75', N'XsltFile', N'XsltFile', N'Behavior', N'XSLT File to use.', 1, 0, N'AttributeValues.xslt', 0, 0, 'd06fad77-7233-4eb9-b25b-3fd641c1dff0')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (207, 1, 1, 9, N'BlockTypeId', N'81', N'ContextEntityType', N'Context Entity Type', N'Filter', N'Context Entity Type', 0, 0, N'', 0, 0, 'df7e74c5-37e1-4ae9-9c8a-a5636d3d645d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (208, 1, 1, 9, N'BlockTypeId', N'81', N'EntityQualifierColumn', N'Entity Qualifier Column', N'Filter', N'The entity column to evaluate when determining if this attribute applies to the entity', 1, 0, N'', 0, 0, '9226e136-850c-4b50-b435-a54984f6a761')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (209, 1, 1, 9, N'BlockTypeId', N'81', N'EntityQualifierValue', N'Entity Qualifier Value', N'Filter', N'The entity column value to evaluate.  Attributes will only apply to entities with this value', 2, 0, N'', 0, 0, '13034637-bd75-49f1-b0bb-cc035f817ee9')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (210, 1, 1, 9, N'BlockTypeId', N'81', N'AttributeCategories', N'Attribute Categories', N'Filter', N'Delimited List of Attribute Category Names', 3, 0, N'', 0, 0, '9626c22f-12f5-4854-bbc5-f8983c73f1da')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (211, 1, 1, 9, N'BlockTypeId', N'81', N'XsltFile', N'XsltFile', N'Behavior', N'XSLT File to use.', 4, 0, N'AttributeValues.xslt', 0, 0, '458cf961-c9ef-4cdd-9a71-527cf9db7d0e')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (212, 1, 3, 26, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '0b1a8fa0-bd26-45cd-9a2b-d2879fb15c45')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (213, 1, 1, 26, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, '733a5d67-a809-46fb-b932-a95904fcecb7')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (214, 1, 1, 26, N'', N'', N'Server', N'Server', N'Server', N'The Active Directory server name', 1, 0, N'', 0, 1, 'a09421a8-b37c-42aa-86fe-700195c9e410')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (215, 1, 1, 26, N'', N'', N'Domain', N'Domain', N'Server', N'The network domain that users belongs to', 2, 0, N'', 0, 1, '8af7de76-a294-4946-8921-09cdbd180021')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (216, 1, 3, 27, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, '0fb80cb1-d065-462b-a5a0-7a6af712025d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (217, 1, 1, 27, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, 'd3c9c482-de0b-491f-a18a-6c64e78ea6de')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (218, 1, 3, 28, N'', N'', N'Active', N'Active', N'', N'Should Service be used?', 0, 0, N'False', 0, 0, 'bae112ee-40d4-4f86-aed8-81c3942ff87d')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (219, 1, 1, 28, N'', N'', N'Order', N'Order', N'', N'The order that this service should be used (priority)', 0, 0, N'0', 0, 0, 'ad8f8ed6-698b-47e7-950a-7cadced70226')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (220, 1, 1, 28, N'', N'', N'AppID', N'App ID', N'Facebook', N'The Facebook App ID', 1, 0, N'', 0, 1, '73d53921-4af9-4ebf-b84b-107d2a40d073')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (221, 1, 1, 28, N'', N'', N'AppSecret', N'App Secret', N'Faceboook', N'The Facebook App Secret', 2, 0, N'', 0, 1, '12211dbc-a51d-4fd8-b89a-a45189a94c6f')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (222, 1, 1, 30, N'', N'', N'SummaryText', N'Summary Text', N'', N'', 0, 0, N'', 0, 0, '763175ed-a67f-415e-8843-d2c50ef12b33')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (223, 1, 20, 30, N'', N'', N'DetailHtml', N'Detail Html', N'', N'', 0, 0, N'', 0, 0, '58c196e9-a731-40bb-a015-8a63c98b6733')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (224, 1, 10, 30, N'', N'', N'SummaryImage', N'Summary Image', N'', N'', 0, 0, N'', 0, 0, 'f090d30a-5bca-4a73-a42f-54744e78a8f2')
INSERT INTO [Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid]) VALUES (225, 1, 10, 30, N'', N'', N'DetailImage', N'Detail Image', N'', N'', 0, 0, N'', 0, 0, 'c5c19024-25ee-4a9a-9dcc-bfb148c84b8e')
SET IDENTITY_INSERT [Attribute] OFF

-- Add 14 rows to [DefinedType]
SET IDENTITY_INSERT [DefinedType] ON
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (1, 1, NULL, 0, N'Person', N'Record Type', N'Record Types', '26be73a6-a9c5-4e94-ae00-3afdcf8c9275')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (2, 1, NULL, 0, N'Person', N'Record Status', N'Record Status', '8522badd-2871-45a5-81dd-c76da07e2e7e')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (3, 1, NULL, 0, N'Person', N'Record Status Reason', N'Record Status Reason', 'e17d5988-0372-4792-82cf-9e37c79f7319')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (4, 1, NULL, 0, N'Person', N'Status', N'Status', '2e6540ea-63f0-40fe-be50-f2a84735e600')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (5, 1, NULL, 0, N'Person', N'Title', N'Person Title (i.e. Mr., Mrs., Dr., etc)', '4784cd23-518b-43ee-9b97-225bf6e07846')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (6, 1, NULL, 0, N'Person', N'Suffix', N'Person Suffix (i.e. Sr., Jr. etc)', '16f85b3c-b3e8-434c-9094-f3d41f87a740')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (7, 1, NULL, 0, N'Person', N'Marital Status', N'Marital Status', 'b4b92c3f-a935-40e1-a00b-ba484ead613b')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (10, 1, NULL, 0, N'Financial', N'Currency Type', N'Test Currency Type', '1d1304de-e83a-44af-b11d-0c66dd600b81')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (11, 1, NULL, 0, N'Financial', N'Credit Card Type', N'Test Credit Card Type', '2bd4ffb0-6c7f-4890-8d08-00f0bb7b43e9')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (12, 1, NULL, 0, N'Financial', N'Source Type', N'Test Source Type', '4f02b41e-ab7d-4345-8a97-3904ddd89b01')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (13, 1, 1, 1, N'Person', N'Phone Type', N'Type of phone number', '8345dd45-73c6-4f5e-bebd-b77fc83f18fd')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (14, 1, NULL, 0, N'Metric', N'Frequency', N'Types of frequencies', '526cb333-2c64-4486-8469-7f7ea9366254')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (15, 1, 1, 2, N'Location', N'Location Type', N'Location Types', '2e68d37c-fb7b-4aa5-9e09-3785d52156cb')
INSERT INTO [DefinedType] ([Id], [IsSystem], [FieldTypeId], [Order], [Category], [Name], [Description], [Guid]) VALUES (16, 1, 1, 3, N'Marketing Campaign', N'Audience Type', N'Audience Type', '799301a3-2026-4977-994e-45dc68502559')
SET IDENTITY_INSERT [DefinedType] OFF

-- Add 145 rows to [AttributeValue]
SET IDENTITY_INSERT [AttributeValue] ON
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (3, 0, 2, 12, NULL, N'<div class=""test"">', '73155181-bdc6-47b1-8de1-d403e5a4ec6e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (4, 0, 4, 12, NULL, N'</div>', '5730337b-2172-4eb6-a0b2-f8e3f856be83')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (23, 0, 18, 20, NULL, N'12', 'a6c3a68f-407a-4f59-a3de-a8751a0a0858')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (24, 1, 19, 20, NULL, N'~/Assets/XSLT/PageNav.xslt', 'dd1473b4-9657-4ef4-b499-b8724685a89e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (30, 0, 2, 35, NULL, N'', '8641d846-0e8a-4921-aa25-918bd488c927')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (31, 0, 4, 35, NULL, N'', 'fcef61fd-b5df-4935-9b80-d46f4c5c211d')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (34, 1, 22, NULL, NULL, N'7/13/2012 4:58:30 PM', '67d29acf-b1d6-411d-a7fc-3db551f99f85')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (41, 0, 36, 0, NULL, N'1', 'c6e7ce4c-d093-4f42-bb51-f5a14dc1b384')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (42, 0, 41, 0, NULL, N'False', '26700cb1-af2c-4462-baaf-04865d7b77ed')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (45, 0, 34, 0, NULL, N'0', 'a24677be-2bad-45c2-8f4e-9eadf8363db8')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (46, 0, 40, 0, NULL, N'False', 'f4e51db5-4048-4472-b050-7f8430aec91a')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (48, 0, 44, 0, NULL, N'False', '55171284-c3f3-45e6-8b57-efd32d4fc5c0')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (49, 0, 45, 0, NULL, N'1', '91a343ba-3872-46b7-8644-30cb86a26921')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (52, 0, 50, 0, NULL, N'0', '222a4077-60e9-46de-88ee-e122d889e748')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (53, 0, 51, 0, NULL, N'False', 'f53eb389-12fb-4e79-9241-2eabb7174afc')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (55, 0, 54, 0, NULL, N'2', '7f3b9b34-5ab2-4442-a2b2-aed541ff0b70')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (56, 0, 55, 0, NULL, N'False', '5d25579b-276b-4fcb-b2b0-bac8b67a4fd5')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (60, 0, 67, 54, NULL, N'True', 'c3b3e4ca-577b-4417-bbd1-9302de921b72')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (61, 1, 68, 54, NULL, N'There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?', 'f2a02d0b-37be-46d4-aa83-0a767cab8630')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (62, 1, 69, 54, NULL, N'{0}, you already have an existing account.  Would you like us to email you the username?', 'b788129c-0164-4faf-9a56-67d1e8d3598a')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (63, 1, 64, 54, NULL, N'Your username has been emailed to you.  If you''ve forgotten your password, the email includes a link to reset your password.', '64a1bd51-c8ed-4ce9-83d6-1b02187efb7e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (64, 1, 65, 54, NULL, N'Because you''ve selected an existing person, we need to have you confirm the email address you entered belongs to you. We''ve sent you an email that contains a link for confirming.  Please click the link in your email to continue.', 'df12a2e5-e7c1-4d13-95fa-d9a2b6433d3a')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (65, 1, 66, 54, NULL, N'{0}, Your account has been created', '603ac593-f9aa-4bb5-9c8e-e6c3dabd72c5')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (66, 0, 70, NULL, NULL, N'', '07fcbf84-7628-4396-a604-a8f5b68967dc')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (67, 0, 75, NULL, NULL, N'', 'f2a0ad0f-cd5d-4832-bc15-a981c2d552fc')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (68, 0, 77, NULL, NULL, N'', '2bdad18b-1371-4e7c-b22c-49a102326dab')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (69, 0, 78, NULL, NULL, N'', '1528d184-eaad-4453-8a4f-2e2dda2a8806')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (70, 0, 76, NULL, NULL, N'25', '8bc8ed53-d6c3-4991-84ba-d710ed148e07')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (71, 0, 79, NULL, NULL, N'false', '28a36af7-c5b4-43af-b7f9-70a4585c83b3')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (72, 0, 80, 54, NULL, N'Page/54', '334cde25-6067-44f3-8c03-11dfbdb03efa')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (73, 0, 127, 73, NULL, N'Rock.Address.StandardizeContainer, Rock', '8f31d5a0-3e24-4aea-858a-42d9426c8e85')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (74, 0, 127, 74, NULL, N'Rock.Address.GeocodeContainer, Rock', '364ef9f3-b5a2-4a1f-983d-a3480ac438ad')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (75, 0, 127, 75, NULL, N'Rock.Address.StandardizeContainer, Rock', '376c1716-715a-4ece-85bf-bae7a9e53eaf')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (76, 0, 130, NULL, NULL, N'', '40e95fa4-8a78-4a26-9036-ebc6bd108e8c')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (77, 0, 131, NULL, NULL, N'', '5b9821cf-7a0f-4132-b18a-a0f58a68db8f')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (78, 0, 132, NULL, NULL, N'', 'e257083e-b0c2-479b-880b-e8702a6e25a3')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (79, 0, 136, NULL, NULL, N'', '4ebcbb69-d9df-4a84-9a62-29ac0e401370')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (80, 0, 129, NULL, NULL, N'', 'd5b76afc-474f-4ff4-8c40-a129e889b4c4')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (81, 0, 133, NULL, NULL, N'', 'aee8aad5-97bf-43b4-ba10-34f03588a141')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (82, 0, 139, NULL, NULL, N'', 'c45c6118-d95a-4b30-924a-697fc4fe2c9b')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (83, 0, 134, NULL, NULL, N'', '38389864-6305-40e6-9650-5f1fcbe45054')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (84, 0, 135, NULL, NULL, N'', '7ea870d5-feaf-4ad8-8396-a4b7c2b363c5')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (85, 0, 137, NULL, NULL, N'', '7811dfab-7cb8-4a46-8f9b-2c3c12c51c9a')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (86, 0, 143, NULL, NULL, N'False', 'acdf0398-550c-4817-88cb-0e7d6dff1afd')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (87, 0, 144, NULL, NULL, N'', '70100dcd-ba2d-4812-82de-d5268eca8448')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (90, 0, 2, 79, NULL, N'', '60655895-cde0-4947-b641-438b3bce9717')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (91, 0, 4, 79, NULL, N'', '8a70fa1b-f0cd-4ecb-8b7e-2ba46f4d8ed4')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (97, 0, 2, 49, NULL, N'', 'e0cc4365-af31-4e31-b3f6-8fd9bfd50b8a')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (98, 0, 4, 49, NULL, N'', '538ef869-ee20-4134-905d-5fdf70301ef7')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (102, 0, 150, NULL, NULL, N'', '444777d8-9457-4671-b670-70674d479ab6')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (103, 0, 151, NULL, NULL, N'', '02418e6b-12dc-48eb-8535-b6baaa4c555a')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (106, 0, 153, 15, NULL, N'00:02:30', '691965b6-12f9-4573-b71f-bf8f33e10cd6')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (107, 0, 153, 16, NULL, N'00:01:15', '3dc4e0e2-f82c-483f-854a-1f73322f1700')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (108, 0, 158, 82, NULL, N'10', 'b3c1dfc4-b6da-4523-8fb2-34a40d185351')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (109, 0, 157, 82, NULL, N'5', 'e2f368f6-b047-479b-a1f7-88f469126bf3')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (110, 0, 154, 82, NULL, N'2', '3466294c-85cf-43c5-b00b-dbf106dcd59c')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (111, 0, 155, 82, NULL, N'VideoDuration', 'b2be9ea8-6bc0-44dd-8190-908a10a7c3f7')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (112, 0, 156, 82, NULL, N'Video', 'c7886cdb-fbfa-4f43-ba6b-a767a62760ae')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (113, 0, 162, 49, 0, N'True', '2e894676-ce3b-45a4-b91a-0f2a5595f135')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (114, 0, 161, 49, 0, N'False', 'c024b060-c801-49eb-8fda-dcadddc42752')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (115, 0, 160, 49, 0, N'0', 'd26d86b9-c8e0-4104-b472-b22b6eb85692')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (116, 0, 163, 49, 0, N'', 'c4529048-488c-4480-8b8e-88a823b500db')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (117, 0, 164, 49, 0, N'', 'f01dccd9-bebb-447f-956d-e75840af18d2')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (118, 0, 169, 84, 0, N'10', 'c92da4bd-bb73-4512-a6f2-aa2685513943')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (119, 0, 168, 84, 0, N'5', '17d71f12-8e07-418f-9772-ed633cc6a8b3')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (120, 0, 165, 84, 0, N'2', '088f066a-fbb8-47ba-bffd-3b93b60a0aa7')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (121, 0, 166, 84, 0, N'VideoDuration', 'b59f1984-4e06-43a0-adf9-cd3212086d8b')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (122, 0, 167, 84, 0, N'Video', '38597fc9-7189-4dc0-90ae-d3d25313dc04')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (126, 0, 59, 88, 0, N'Rock.Crm.Person', '8d4747ba-b1b9-47d1-97c1-7f55e61ba4e9')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (127, 0, 58, 88, NULL, N'', '374e5598-ab60-4402-a355-8bce0f9c9646')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (129, 0, 57, 88, NULL, N'', '14e3c3f7-3580-4418-a949-a8ba5292c756')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (130, 0, 172, 85, 0, N'Rock.Crm.Person', '88448c0b-5ec3-4620-a944-63c08f728305')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (131, 0, 173, 85, 0, N'', '5d1871c1-1fc3-4f1c-aa3a-200ad12a6122')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (132, 0, 170, 85, 0, N'', '0e0374cd-1b96-43e7-a03e-be2ae4137915')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (133, 0, 171, 85, 0, N'Membership', 'e4df2cf0-df99-4b43-8c60-e3b4855a2fb9')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (134, 0, 174, 1, 0, N'1/2/2012', 'f265ac14-0a80-4e9f-a53a-05e6013f183e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (135, 0, 19, 90, 0, N'~/Assets/XSLT/PageListAsBlocks.xslt', '26af0210-f562-40fe-a171-62295d87836d')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (136, 0, 18, 90, 0, N'', 'd07710c4-5a96-4027-9d98-79c1ef9e71c9')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (137, 0, 20, 90, 0, N'1', 'a7f79fff-dfab-447f-81d6-07d1be667b1b')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (138, 0, 19, 91, 0, N'~/Assets/XSLT/PageListAsBlocks.xslt', '84a04615-8a6d-40df-8b96-49732d197279')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (139, 0, 18, 91, 0, N'78', '4e79908c-aced-488d-b089-020383123049')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (140, 0, 20, 91, 0, N'2', '087f4737-38cf-4846-9629-4e63f614fbe3')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (141, 0, 19, 92, 0, N'~/Assets/XSLT/PageListAsBlocks.xslt', '1e484a84-8a4c-4e01-824e-e75736033711')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (142, 0, 18, 92, 0, N'76', '8263a93e-653e-4fc1-a261-e18eb025a2dd')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (143, 0, 20, 92, 0, N'2', '0a38dd79-f27b-40a6-a7bc-6909c0714d29')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (144, 0, 19, 93, 0, N'~/Assets/XSLT/PageListAsBlocks.xslt', '60807ffe-2dc7-41c4-8654-b70b7f535988')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (145, 0, 18, 93, 0, N'79', '9f2529a6-f6e9-4128-9aa7-3f18efff1ae9')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (146, 0, 20, 93, 0, N'2', '44495373-201b-4d76-b8bd-769d760a9524')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (147, 0, 175, NULL, NULL, N'http://quarry.rockchms.com/api/v2/', '5cb48974-6bb6-435b-a04a-2bf9b7cd778e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (148, 1, 176, 51, 0, N'', '8ab246e3-5197-4f47-8c98-d4eb704fbc57')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (149, 1, 177, 51, 0, N'True', 'fc554edd-2130-4fae-94a9-c4d9691def9e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (150, 1, 127, 102, 0, N'Rock.Search.SearchContainer, Rock', '7dc40ba8-5ecc-40ce-99c9-72a72a87d835')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (151, 1, 178, 0, 0, N'Name', 'db0d1010-abd2-4c9c-ad8c-361cb24b2b74')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (152, 1, 179, 0, 0, N'Person/Search/name/{0}', 'ec970033-6afe-4d19-af66-417b970308a2')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (153, 1, 180, 0, 0, N'0', '15994b25-b942-4365-a69d-6f9b359d0f94')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (154, 1, 181, 0, 0, N'True', '657ed531-09fb-426a-93e0-c4baee585e34')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (155, 1, 182, 0, 0, N'Phone', '7047b6f5-3809-4b8e-aea1-c6fd4be60d6b')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (156, 1, 183, 0, 0, N'Person/Search/phone/{0}', 'b60bb5e5-612d-4b74-9562-64afcef94b4f')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (157, 1, 184, 0, 0, N'1', '4606922c-9129-4671-b164-ac0102386102')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (158, 1, 185, 0, 0, N'True', '977a4a2f-09d9-4b68-b549-c97951f2049e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (159, 1, 186, 0, 0, N'Email', '890d933d-cbe6-42ed-90b4-b88d6bddc277')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (160, 1, 187, 0, 0, N'Person/Search/email/{0}', '343fbfef-bb95-4f59-a901-e0dad3d8ec2f')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (161, 1, 188, 0, 0, N'2', '241e69e5-ee47-411b-9f18-7059f1acd60f')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (162, 1, 189, 0, 0, N'True', 'cd0b2fd2-92d9-4f8b-b076-1be2d67ca71c')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (163, 1, 190, 0, 0, N'Address', '7789cebb-fc31-4507-b8eb-02272ab0a5a9')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (164, 1, 191, 0, 0, N'Person/Search/address/{0}', '93adf726-4e0a-448a-a84c-614a02c9b354')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (165, 1, 192, 0, 0, N'3', '5415e35b-8611-4a59-ae31-960555484254')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (166, 1, 193, 0, 0, N'True', '99658f7f-bffb-493d-89cd-e0986cf9b9c0')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (167, 1, 18, 104, 0, N'', '7bf448d8-b5b3-4fe6-837e-e0f0f5e7bfe4')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (168, 1, 19, 104, 0, N'~/Assets/XSLT/PageListAsBlocks.xslt', '081238ce-cbed-467a-b069-18333354ae2c')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (169, 1, 20, 104, 0, N'1', 'f30c112a-6e96-496b-90e5-bc5587c2cd9c')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (170, 1, 194, 105, 0, N'Rock.Crm.Person', '724788ac-45bd-41c3-acdf-dc05504ce584')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (171, 1, 195, 105, 0, N'', '0e57e823-e9bb-4c54-ba62-6140a5a87981')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (172, 1, 196, 105, 0, N'', '8345aa0c-09c2-41ac-8175-4b0a3c36a7ed')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (173, 1, 197, 105, 0, N'True', 'c03129ba-84fd-4076-aa58-edee83da7c39')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (174, 1, 18, 107, 0, N'12', 'e74cb550-db23-4a38-83c9-0fe0a121af15')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (175, 1, 19, 107, 0, N'~/Assets/XSLT/PageNav.xslt', 'e7411f9d-7525-4902-a7ac-e8e1dc822612')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (176, 1, 20, 107, 0, N'3', 'd00c9f3b-db0d-4183-953a-22e30dbd24cb')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (177, 1, 198, 114, 0, N'Rock.Crm.Person', '33685ef1-c363-46f5-95eb-29f81791af4a')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (178, 1, 199, 114, 0, N'', 'dbabf79e-b47f-454a-9584-5286ec2d82b7')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (179, 1, 200, 114, 0, N'', '46d74a2e-e73b-4356-b2c0-a1f57c9125d8')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (180, 1, 201, 110, 0, N'10', 'c642f82f-f899-4866-b634-541e374d7f1d')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (181, 1, 207, 112, 0, N'Rock.Crm.Person', 'b3f73ddd-9535-4b5f-84b4-2fad8458a7ae')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (182, 1, 208, 112, 0, N'', '49e0e1df-583a-4d97-91d2-eb2c46ddf214')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (183, 1, 209, 112, 0, N'', 'd5a95116-1d8a-4492-80bf-4ada2e348d8a')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (184, 1, 210, 112, 0, N'Membership,Documents', '24bd13b1-dda2-4776-9618-89b88cebea17')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (185, 1, 211, 112, 0, N'AttributeValues_ValueFirst.xslt', '0b7bfd68-f712-41bf-8829-444f7329e7d9')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (186, 1, 202, 110, 0, N'', '4519285a-4cbf-4df4-bf7f-86ee855b6e93')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (187, 1, 203, 110, 0, N'False', 'e6c5f19d-ccd7-4d73-81a8-7eb66832953b')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (188, 1, 204, 110, 0, N'True', 'f5f9e6dc-cfbc-46e2-88c6-1314f5b9dcd5')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (189, 1, 205, 110, 0, N'PersonDetail/FamilyMembers.xslt', '6ab54054-d310-43a1-9855-e0d1c6266156')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (190, 1, 201, 118, 0, N'12', 'd02790f0-a3b8-4526-acf7-dddb38d4d5ca')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (191, 1, 202, 118, 0, NULL, '32553eaa-4fda-4c0c-b441-2236514ced24')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (192, 1, 203, 118, 0, N'False', '8214fed1-8781-4fdc-8306-26cadc9a7e9e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (193, 1, 204, 118, 0, N'False', '03336f50-1977-48cd-9c36-6e83364a55a7')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (194, 1, 205, 118, 0, N'PersonDetail/ImpliedRelationships.xslt', '9efd608d-4526-4ea6-ba4f-9671c93811aa')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (195, 1, 201, 117, 0, N'11', '7af2baa9-080a-4603-9e2d-304ae8dbda17')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (196, 1, 202, 117, 0, N'5', '9d0021d8-89ab-4427-bd14-a2fff044e97e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (197, 1, 203, 117, 0, N'False', 'a3ad56a4-0bf2-40c6-9d53-31d4cce906ab')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (198, 1, 204, 117, 0, N'False', '11705db6-6e3e-452d-b332-9a0e34cec6ef')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (199, 1, 205, 117, 0, N'GroupMembers.xslt', 'e40f7f0c-993a-401a-b5be-f2d4491dc803')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (200, 1, 127, 121, 0, N'Rock.Security.AuthenticationContainer, Rock', '16c4f4c8-9b6f-4300-b1bf-e39513d0d35e')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (201, 1, 127, 122, 0, N'Rock.Security.ExternalAuthenticationContainer, Rock', 'a578fb11-4f4f-467d-bdd8-a15d39f4afb0')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (202, 1, 213, 0, 0, N'1', '04d349b5-fcad-45f3-8275-859a46b825da')
INSERT INTO [AttributeValue] ([Id], [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid]) VALUES (203, 1, 216, 0, 0, N'True', '3a1ffa35-4cdd-4ef3-9c88-7c7d054f98fb')
SET IDENTITY_INSERT [AttributeValue] OFF

-- Add 25 rows to [DefinedValue]
SET IDENTITY_INSERT [DefinedValue] ON
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (1, 1, 1, 0, N'Person', N'A Person Record', '36cf10d6-c695-413d-8e7c-4546efef385e')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (2, 1, 1, 1, N'Business', N'A Business Record', 'bf64add3-e70a-44ce-9c4b-e76bbed37550')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (3, 1, 2, 0, N'Active', N'An Active Record', '618f906c-c33d-4fa3-8aef-e58cb7b63f1e')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (4, 1, 2, 1, N'Inactive', N'An Inactive Record', '1dad99d5-41a9-4865-8366-f269902b80a4')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (5, 1, 2, 3, N'Pending', N'Pending Record Status', '283999ec-7346-42e3-b807-bce9b2babb49')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (6, 1, 10, 0, N'Cash', N'Cash', 'f3adc889-1ee8-4eb6-b3fd-8c10f3c8af93')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (7, 1, 11, 0, N'Visa', N'Visa', 'fc66b5f8-634f-4800-a60d-436964d27b64')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (8, 1, 11, 0, N'MasterCard', N'MasterCard', '6373a4b6-4dca-4eb6-9ade-b30e8a7f8621')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (9, 1, 10, 0, N'Check', N'Check', '8b086a19-405a-451f-8d44-174e92d6b402')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (10, 1, 12, 0, N'Website', N'Website', '7d705ce7-7b11-4342-a58e-53617c5b4e69')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (11, 1, 12, 0, N'Mailer', N'Mailer', '0149eb64-00c4-4c69-b1a6-2fd0edfc6acb')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (12, 1, 13, 0, N'Primary', N'Primary Phone Number', '407e7e45-7b2e-4fcd-9605-ecb1339f2453')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (13, 1, 13, 1, N'Secondary', N'Secondary Phone Number', 'aa8732fb-2cea-4c76-8d6d-6aaa2c6a4303')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (14, 1, 14, 0, N'Hourly', N'Hourly', '78cf66eb-1a65-42cc-a05e-3bf6de515049')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (15, 1, 14, 0, N'Weekly', N'Weekly', '41663b95-8271-40e9-b1b6-0d14ea45d68d')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (16, 1, 14, 0, N'Monthly', N'Monthly', '0bc11625-f8c4-4032-8b27-537d67941489')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (17, 1, 14, 0, N'Yearly', N'Yearly', '305cbfa3-3168-40af-abcf-f5dff9dc13c2')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (18, 1, 14, 0, N'Manually', N'Manually', '338f29e5-05c4-40a5-a669-c098787e2adf')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (19, 1, 15, 0, N'Home', N'Home', '8c52e53c-2a66-435a-ae6e-5ee307d9a0dc')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (20, 1, 15, 1, N'Office', N'Office', 'e071472a-f805-4fc4-917a-d5e3c095c35c')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (21, 1, 15, 2, N'Business', N'Business', 'c89d123c-8645-4b96-8c71-6c87b5a96525')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (22, 1, 15, 3, N'Sports Field', N'Sports Field', 'f560dc25-e964-46c4-8cef-0e67bb922163')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (23, 0, 16, 0, N'Kids', N'Kids', 'f2bff319-a109-4b42-bec2-76590e11627b')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (24, 0, 16, 1, N'Adults', N'Adults', '95e49778-ae72-454f-91cc-2fc864557dec')
INSERT INTO [DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid]) VALUES (25, 0, 16, 2, N'Staff', N'Staff', '833ee2c7-f83a-4744-ad14-6907554df8ae')
SET IDENTITY_INSERT [DefinedValue] OFF

-- Add 5 rows to [Metric]
SET IDENTITY_INSERT [Metric] ON
INSERT INTO [Metric] ([Id], [IsSystem], [Type], [Category], [Title], [Subtitle], [Description], [MinValue], [MaxValue], [CollectionFrequencyId], [LastCollected], [Source], [SourceSQL], [Order], [Guid]) VALUES (1, 0, 0, N'Core', N'Salvations', NULL, N'Salvations', NULL, NULL, 18, NULL, NULL, NULL, 0, 'e3752bfb-6cd5-4dd9-8536-333e44746c0a')
INSERT INTO [Metric] ([Id], [IsSystem], [Type], [Category], [Title], [Subtitle], [Description], [MinValue], [MaxValue], [CollectionFrequencyId], [LastCollected], [Source], [SourceSQL], [Order], [Guid]) VALUES (2, 0, 0, N'Core', N'Baptisms', NULL, N'Baptisms', NULL, NULL, 18, NULL, NULL, NULL, 0, '0f33c2fd-49cc-4f4c-9da8-fd558fcca5df')
INSERT INTO [Metric] ([Id], [IsSystem], [Type], [Category], [Title], [Subtitle], [Description], [MinValue], [MaxValue], [CollectionFrequencyId], [LastCollected], [Source], [SourceSQL], [Order], [Guid]) VALUES (3, 0, 0, N'Core', N'Weekly Volunteers', NULL, N'Weekly Volunteers', NULL, NULL, 15, NULL, NULL, NULL, 0, '53861ebc-3e98-4ba9-bac6-d8648b9f26ed')
INSERT INTO [Metric] ([Id], [IsSystem], [Type], [Category], [Title], [Subtitle], [Description], [MinValue], [MaxValue], [CollectionFrequencyId], [LastCollected], [Source], [SourceSQL], [Order], [Guid]) VALUES (4, 0, 0, N'Core', N'Weekly Attendance', NULL, N'Weekly Attendance', NULL, NULL, 15, NULL, NULL, NULL, 0, '50986e58-b3ac-4907-add0-89083e69dcb7')
INSERT INTO [Metric] ([Id], [IsSystem], [Type], [Category], [Title], [Subtitle], [Description], [MinValue], [MaxValue], [CollectionFrequencyId], [LastCollected], [Source], [SourceSQL], [Order], [Guid]) VALUES (5, 0, 0, N'Core', N'Monthly Contributions', NULL, N'Monthly Contributions', NULL, NULL, 16, NULL, NULL, NULL, 0, '558671bd-9952-494f-a1a9-a37af0e40155')
SET IDENTITY_INSERT [Metric] OFF

-- Add 1 row to [Person]
SET IDENTITY_INSERT [Person] ON
INSERT INTO [Person] ([Id], [IsSystem], [RecordTypeId], [RecordStatusId], [RecordStatusReasonId], [PersonStatusId], [TitleId], [GivenName], [NickName], [LastName], [SuffixId], [PhotoId], [BirthDay], [BirthMonth], [BirthYear], [Gender], [MaritalStatusId], [AnniversaryDate], [GraduationDate], [Email], [IsEmailActive], [EmailNote], [DoNotEmail], [SystemNote], [ViewedCount], [Guid]) VALUES (1, 1, NULL, NULL, NULL, NULL, NULL, N'Admin', N'Admin', N'Admin', NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, N'admin@organization.com', NULL, NULL, 0, NULL, NULL, 'ad28da19-4af1-408f-9090-2672f8376f27')
SET IDENTITY_INSERT [Person] OFF

-- Add 1 row to [UserLogin]
SET IDENTITY_INSERT [UserLogin] ON
INSERT INTO [UserLogin] ([Id], [ServiceType], [ServiceName], [UserName], [Password], [IsConfirmed], [LastActivityDate], [LastLoginDate], [LastPasswordChangedDate], [CreationDate], [IsOnLine], [IsLockedOut], [LastLockedOutDate], [FailedPasswordAttemptCount], [FailedPasswordAttemptWindowStart], [ApiKey], [PersonId], [Guid]) VALUES (1, 0, N'Rock.Security.Authentication.Database', N'admin', N'xTa7rQK3k6AxSlvacgE5BNl5mNk=', 1, '2012-07-13 15:15:30.813', '2012-07-13 14:57:54.513', '2012-01-23 03:43:25.170', '2011-03-19 07:34:15.327', NULL, NULL, '2011-12-15 02:45:54.937', 1, '2012-06-07 15:25:06.977', N'CcvRockApiKey', 1, '7e10a764-ef6b-431f-87c7-861053c84131')
SET IDENTITY_INSERT [UserLogin] OFF

-- Add 4 rows to [EmailTemplate]
SET IDENTITY_INSERT [EmailTemplate] ON
INSERT INTO [EmailTemplate] ([Id], [IsSystem], [PersonId], [Category], [Title], [From], [To], [Cc], [Bcc], [Subject], [Body], [Guid]) VALUES (1, 1, NULL, N'Security', N'Forgot Usernames', N'rock@sparkdevnetwork.com', N'', N'', N'', N'Login Request', N'{{ EmailHeader }}

Below are your current usernames at {{ OrganizationName }}<br/>

{% for Person in Persons %}
	<br/>
	For {{ Person.FirstName }} {{ Person.LastName }},<br/><br/>
	{% for User in Person.Users %}
		{{ User.UserName }} <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=reset''>Reset Password</a><br/>
	{% endfor %}
	<br/>
{% endfor %}

{{ EmailFooter }}', '113593ff-620e-4870-86b1-7a0ec0409208')
INSERT INTO [EmailTemplate] ([Id], [IsSystem], [PersonId], [Category], [Title], [From], [To], [Cc], [Bcc], [Subject], [Body], [Guid]) VALUES (3, 1, NULL, N'Security', N'Confirm Account', N'rock@sparkdevnetwork.com', N'', N'', N'', N'Account Confirmation', N'{{ EmailHeader }}

{{ Person.FirstName }},<br/><br/>

Thank-you for creating an account at {{ OrganizationName }}. Please <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=confirm''>confirm</a> that you are the owner of this email address.<br/><br/>If you did not create this account, you can <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete''>Delete It</a>.<br/><br/>If the above links do not work, you can also go to {{ ConfirmAccountUrl }} and enter the following confirmation code:<br/>{{ User.ConfirmationCode }}<br/><br/>

Thank-you,<br/>
{{ OrganizationName }}  

{{ EmailFooter }}', '17aaceef-15ca-4c30-9a3a-11e6cf7e6411')
INSERT INTO [EmailTemplate] ([Id], [IsSystem], [PersonId], [Category], [Title], [From], [To], [Cc], [Bcc], [Subject], [Body], [Guid]) VALUES (7, 1, NULL, N'Security', N'Account Created', N'Rock@SparkDevNetwork', N'', N'', N'', N'Account Created', N'{{ EmailHeader }}

{{ Person.FirstName }},<br/><br/>

Thank-you for creating a new account at {{ OrganizationName }}.  Your ''{{ User.UserName }}'' username is now active and can be used to login to our site and access your information.<br/><br/>  If you did not create this account you can <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete''>Delete it here</a><br/><br/>  Thanks.
Thank-you,<br/>
{{ OrganizationName }}  

{{ EmailFooter }}', '84e373e9-3aaf-4a31-b3fb-a8e3f0666710')
INSERT INTO [EmailTemplate] ([Id], [IsSystem], [PersonId], [Category], [Title], [From], [To], [Cc], [Bcc], [Subject], [Body], [Guid]) VALUES (8, 1, NULL, N'System', N'Exception Notification', N'rock@sparkdevnetwork.org', N'', N'', N'', N'Rock ChMS Exception Notification', N'{{ EmailHeader }}

An exception has occurred in the Rock ChMS.  Details of this error can be found below: 

<p>{{ ExceptionDetails }}<p>  

{{ EmailFooter }}', '75cb0a4a-b1c5-4958-adeb-8621bd231520')
SET IDENTITY_INSERT [EmailTemplate] OFF

-- Add 23 rows to [Auth]
SET IDENTITY_INSERT [Auth] ON
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (800, 1, 0, 0, N'View', N'A', 1, NULL, NULL, '200e1903-b8d3-47d9-9e88-1e3ede62ec2d')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (801, 1, 0, 0, N'Edit', N'A', 0, NULL, 2, '2ba4a7b9-cb2d-4f3d-a9c2-5a3cbe5c010b')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (802, 1, 0, 1, N'Edit', N'D', 1, NULL, NULL, '60987f75-069a-465a-a1e4-3bd502c4f4e9')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (803, 1, 0, 0, N'Configure', N'A', 0, NULL, 2, '36c9ffb4-24de-49b2-93a1-7788ec5adff4')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (804, 1, 0, 1, N'Configure', N'D', 1, NULL, NULL, 'be3eec8b-bb8c-43c6-92cd-45adbd499fad')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (805, 3, 1, 0, N'View', N'A', 2, NULL, NULL, 'c3e70f27-df27-4ff8-9cab-2fdc4d51030a')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (806, 3, 1, 1, N'View', N'D', 1, NULL, NULL, '0310c1a5-9c87-43de-b319-8e9e1d54c23f')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (807, 3, 1, 0, N'Edit', N'A', 0, NULL, 2, '95b7433d-70b2-4dbd-a5b7-2be0807d3f7e')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (808, 3, 1, 1, N'Edit', N'D', 1, NULL, NULL, 'a5b6a118-bdf0-48bc-aed0-e88045c09331')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (809, 3, 1, 0, N'Configure', N'A', 0, NULL, 2, '18f7cb9f-a2c0-444d-a51f-e1417c02914c')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (810, 3, 1, 1, N'Configure', N'D', 1, NULL, NULL, '56117fd1-ec9a-42cb-84af-72d4e7a2e913')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (811, 3, 1, 0, N'Approve', N'A', 0, NULL, 2, '42941e3f-2203-4144-8c21-a267011ac51c')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (812, 3, 1, 1, N'Approve', N'D', 1, NULL, NULL, '38fbfb05-be64-492e-9af2-3be357acb5d3')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (813, 2, 3, 0, N'View', N'A', 1, NULL, NULL, 'addb4ee5-ec9e-4196-a390-c9ab87425f14')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (814, 2, 4, 0, N'View', N'A', 1, NULL, NULL, 'e6073656-a6e0-4464-ac2d-5dc48f5cd097')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (815, 2, 54, 0, N'View', N'A', 1, NULL, NULL, 'eeb951c6-73c1-408a-951a-8e11cad25afb')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (816, 2, 56, 0, N'View', N'A', 1, NULL, NULL, '7192559d-c801-4701-8886-b189f67c86a5')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (817, 2, 55, 0, N'View', N'A', 2, NULL, NULL, '93811c65-a7b9-470f-bd82-f4372c4d7881')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (818, 2, 57, 0, N'View', N'A', 2, NULL, NULL, '88dcc3c3-d22d-4a5b-a977-718a86169ca8')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (819, 2, 43, 0, N'View', N'A', 0, NULL, 2, '33ab869e-fad1-4103-a39e-e801864b9e2d')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (820, 2, 43, 1, N'View', N'D', 1, NULL, NULL, 'cd34f6a0-a285-41f1-b4f6-a0df8024fac2')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (821, 2, 44, 0, N'View', N'A', 0, NULL, 2, 'c6e9e32a-38a7-4c78-9fdd-e1ab75118b24')
INSERT INTO [Auth] ([Id], [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [PersonId], [GroupId], [Guid]) VALUES (822, 2, 44, 1, N'View', N'D', 1, NULL, NULL, '5252983f-7613-41d2-b1af-6099231d9372')
SET IDENTITY_INSERT [Auth] OFF

-- Add 64 rows to [Block]
SET IDENTITY_INSERT [Block] ON
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (8, 1, 2, NULL, 3, N'Content', 0, N'a', 0, 'b31ae932-f065-4500-8524-2182431cd18c')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (10, 1, 3, NULL, 4, N'Content', 0, N'b', 0, '3d325bb3-e1c9-4194-8e9b-11bffc347dc3')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (13, 1, 7, NULL, 8, N'Content', 0, N'd', 0, '74b3b85e-33b6-4acf-8338-cbc12888bc74')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (16, 0, 9, NULL, 2, N'Content', 0, N'Person Edit', 0, '6e189d68-c4ec-443f-b409-1eec0f12d427')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (20, 1, NULL, N'Default', 14, N'Menu', 1, N'Menu', 0, 'cc8f4186-870d-4cf3-8226-d49f1a0d0ddf')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (22, 1, 16, NULL, 18, N'Content', 0, N'l', 0, 'a5e0dd78-bb67-41e5-bdda-73e2277482da')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (35, 1, 1, NULL, 6, N'ContentRight', 0, N'Html', 0, '53e8e3a7-5ce8-493a-96ac-c1a7ecfca5c7')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (40, 1, 23, NULL, 21, N'Content', 0, N'Block Properties', 0, '3d128ae6-08de-4108-a888-f97c66b21996')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (43, 1, 28, NULL, 24, N'Content', 0, N'Security', 0, '2ca65e58-5ed1-4bbb-b5e1-82f3eba8ee0c')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (44, 1, 29, NULL, 25, N'Content', 0, N'Child Pages', 0, '7b13773c-7477-42b6-b6b9-c69a19fb64ea')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (45, 1, 37, NULL, 26, N'Content', 0, N'Page Properties', 0, '46376422-c33d-4fff-8005-e7116781b466')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (49, 1, 12, NULL, 6, N'Content', 0, N'Welcome', 0, '5f0dbb84-bfef-43ed-9e51-e245dc85b7b5')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (50, 1, NULL, N'Default', 35, N'zHeader', 0, N'Login Status', 0, 'c62e5ee6-104e-4741-86df-b9484f597c2c')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (51, 1, 51, NULL, 36, N'Content', 0, N'Global Attributes', 0, '3cbb177b-dbfb-4fb2-a1a7-957dc6c350eb')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (53, 1, NULL, N'Default', 6, N'Footer', 0, N'Footer Content', 0, '3fca7657-a18c-400a-8a5a-29a8680c15e6')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (54, 1, 4, NULL, 40, N'Content', 0, N'New Account', 0, '0d192bf7-584a-4229-84a6-a02c7caceebf')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (55, 1, 54, NULL, 39, N'Content', 0, N'Confirm', 0, '3137bf4e-5735-4db7-b708-3b6f80da5505')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (56, 1, 55, NULL, 41, N'Content', 0, N'Rest Password', 0, '150496f4-4798-4bb5-b796-405de11b5ed1')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (57, 1, 56, NULL, 42, N'Content', 0, N'Forgot User Name', 0, '33c61662-b42a-483b-91f1-c10955c5e5a9')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (58, 0, 57, NULL, 6, N'Content', 0, N'My Account', 0, '71477d00-6a2b-4736-9f7d-2001b76da638')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (74, 0, 38, NULL, 53, N'Content', 1, N'Geocoding Services', 0, '803ce253-3ada-4c2a-b62f-ec4d5b7b7257')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (75, 0, 46, NULL, 53, N'Content', 1, N'Standardization Services', 0, '60ded2d5-0675-452a-b82b-781b044bb856')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (76, 0, 61, NULL, 54, N'Content', 0, N'Email Templates', 0, '845100e9-30d6-45a8-9b80-052395735982')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (79, 0, 12, NULL, 6, N'Content', 0, N'Test Block', 0, 'd5d2048c-52c6-4827-a817-9b84e0525510')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (88, 0, 71, NULL, 36, N'Content', 0, N'Person Properties', 0, 'bde5e15d-0cc7-4164-837f-91ada64a15d9')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (90, 0, 77, NULL, 14, N'Content', 0, N'Page List', 0, '091bdfec-f76b-416b-b8f3-0a9db93af606')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (91, 0, 78, NULL, 14, N'Content', 0, N'Page List', 0, '8afeaede-187a-4ee6-bb0e-702c582e8e02')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (92, 0, 76, NULL, 14, N'Content', 0, N'Page List', 0, 'bedff750-3eb8-4ee7-a8b4-23863fb0315d')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (93, 0, 79, NULL, 14, N'Content', 0, N'Page List', 0, '261ea4cf-f7cd-47dc-bc69-3b2d6eb87dd5')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (94, 0, 20, NULL, 6, N'Content', 0, N'HR Info', 0, '718c516f-0a1d-4dbc-a939-1d9777208fec')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (95, 0, 19, NULL, 6, N'Content', 0, N'Content', 0, 'b8224c72-4168-40f0-96be-38f2afd525f5')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (96, 0, 81, NULL, 7, N'Content', 0, N'Roles', 0, '6c805618-75e7-470f-ac3e-390529ed94f1')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (97, 0, 83, NULL, 17, N'Content', 0, N'Financial Transactions', 0, 'b447ab11-3a19-4527-921a-2266a6b4e181')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (98, 0, 84, NULL, 63, N'Content', 0, N'Defined Types', 0, 'b8d83a2c-31f2-48c6-bebc-753bcdc2a30c')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (99, 1, 85, NULL, 64, N'Content', 0, N'Campuses', 0, 'cb71352b-c10b-453a-8879-0eff8707355a')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (100, 1, 86, NULL, 65, N'Content', 0, N'SystemInfo', 0, 'd1fd9a6b-c213-4074-8d84-ee5353635443')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (101, 1, 87, NULL, 66, N'Content', 0, N'Plugin Manager', 0, '8b083cea-0548-4af2-86f7-46a88fde07d5')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (102, 1, 88, NULL, 53, N'Content', 0, N'Search Components', 0, '73ce9f13-43f1-4dd4-aa5b-70a48c5a6d85')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (103, 1, 89, NULL, 67, N'Content', 0, N'Person Search', 0, '434cb505-016b-418a-b27a-d0fdd07dd928')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (104, 1, 90, NULL, 14, N'Content', 0, N'Person Tags', 0, 'b551e9d1-304c-4e13-8cc5-318899ff2741')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (105, 1, 91, NULL, 68, N'Content', 0, N'Person Tags', 0, 'd464b931-7783-4912-98db-e895643044b0')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (106, 1, NULL, N'PersonDetail', 35, N'zHeader', 0, N'Login Status', 0, '19c2140d-498a-4675-b8a2-18b281736f6e')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (107, 1, NULL, N'PersonDetail', 14, N'Menu', 0, N'Menu', 0, '148e5996-00de-4341-8541-20cb3ffb7c74')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (108, 1, NULL, N'PersonDetail', 6, N'Footer', 0, N'Footer Content', 0, 'ae29a24e-6f85-4bc8-8c14-a8bf97a5d263')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (109, 1, 93, NULL, 69, N'Bio', 0, N'Bio', 0, 'b5c1fdb6-0224-43e4-8e26-6b2eaf86253a')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (110, 1, 93, NULL, 73, N'BioDetails', 1, N'Family', 0, '44bdb19e-9967-45b9-a272-81f9c12ffe20')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (111, 1, 93, NULL, 70, N'BioDetails', 0, N'Contact Info', 0, '47b76e96-5641-486f-94b2-1e799a092be0')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (112, 1, 93, NULL, 81, N'BioDetails', 2, N'Dates', 0, '64366596-3301-4247-8819-4086ea86d1b6')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (114, 1, 93, NULL, 78, N'Tags', 0, N'Tags', 0, '961623ac-1243-44a2-9ecb-685a7ede2424')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (115, 1, 93, NULL, 77, N'Notes', 0, N'Notes', 0, 'cceb85c0-45b4-4508-8331-da59b7f573b6')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (116, 1, 93, NULL, 75, N'SupplementalInfo', 0, N'Key Attributes', 0, 'c9386df7-8acb-46e3-89dd-b00cb0648184')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (117, 1, 93, NULL, 73, N'SupplementalInfo', 1, N'Know Relationships', 0, '192b987f-94c0-4fa6-8795-ff1cee89fdb0')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (118, 1, 93, NULL, 73, N'SupplementalInfo', 2, N'Implied Relationships', 0, '72dd0749-1298-4c12-a336-9e1f49852bd4')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (119, 0, 94, NULL, 79, N'Content', 0, N'Metrics', 0, '9126cfa2-9b26-4fbb-bb87-f76514221dbe')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (120, 1, 95, NULL, 80, N'Content', 0, N'Page Route Block', 0, '09dc13af-8bf8-4a65-b3df-77f17c5650d6')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (121, 1, 96, NULL, 53, N'Content', 0, N'Internal', 0, '10d2886b-40f6-47ee-b137-23595fac224d')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (122, 1, 96, NULL, 53, N'Content', 1, N'External', 0, 'fa273fe7-c278-4a41-967b-c7ed85c48b3b')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (123, 1, 97, NULL, 82, N'Content', 0, N'Group Types', 0, 'f3b2fc30-5ace-4d1d-87f3-9712723d903f')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (124, 1, 98, NULL, 83, N'Content', 0, N'Groups', 0, '52b774fe-9abf-4852-9496-6fad4646f949')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (125, 1, 99, NULL, 84, N'Content', 0, N'Group Roles', 0, '1064932b-f0db-4f39-b438-24703a14198b')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (126, 1, 100, NULL, 85, N'Content', 0, N'Marketing Campaigns', 0, 'c26c8eb3-0bf3-4d5e-a685-bc6c9b9246d8')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (127, 1, 101, NULL, 86, N'Content', 0, N'Marketing Campaign Ad Type', 0, 'ec20dbd8-9c33-45ff-b4ae-64d460350fe0')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (128, 1, 102, NULL, 87, N'Content', 0, N'Jobs', 0, '01727331-4f96-43cd-8585-791b35d86487')
INSERT INTO [Block] ([Id], [IsSystem], [PageId], [Layout], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid]) VALUES (129, 1, 103, NULL, 88, N'Content', 0, N'Site Map', 0, '68192536-3ce8-433b-9df8-a895ef037fd7')
SET IDENTITY_INSERT [Block] OFF

-- Add 9 rows to [HtmlContent]
SET IDENTITY_INSERT [HtmlContent] ON
INSERT INTO [HtmlContent] ([Id], [BlockId], [EntityValue], [Version], [Content], [IsApproved], [ApprovedByPersonId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid]) VALUES (31, 35, N'', 0, N'<p>  The Right Side - yoyo</p> ', 1, 1, NULL, NULL, NULL, '553bed67-9e53-4d2b-a1c9-1499ff76356d')
INSERT INTO [HtmlContent] ([Id], [BlockId], [EntityValue], [Version], [Content], [IsApproved], [ApprovedByPersonId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid]) VALUES (32, 35, NULL, 1, N'.', 1, 1, NULL, NULL, NULL, '9cd7dc41-4d32-4ad2-8eb4-d57ee96e3a30')
INSERT INTO [HtmlContent] ([Id], [BlockId], [EntityValue], [Version], [Content], [IsApproved], [ApprovedByPersonId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid]) VALUES (34, 53, N'', 0, N'<p>  Copyright&nbsp; 2012 Spark Development Network</p> ', 1, 1, NULL, NULL, NULL, 'cbe60b6e-8667-46fd-a37e-4185c7002241')
INSERT INTO [HtmlContent] ([Id], [BlockId], [EntityValue], [Version], [Content], [IsApproved], [ApprovedByPersonId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid]) VALUES (35, 58, N'', 0, N'<a href=""ChangePassword"">Change Password</a></p> ', 1, 1, NULL, NULL, NULL, '50ce66fe-e130-41e0-bddf-40e58572606c')
INSERT INTO [HtmlContent] ([Id], [BlockId], [EntityValue], [Version], [Content], [IsApproved], [ApprovedByPersonId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid]) VALUES (39, 79, N'', 1, N'<p>   This is the first version</p>  ', 0, 1, NULL, NULL, NULL, '62516f40-9aa7-4b62-9b07-fa5a62e724f3')
INSERT INTO [HtmlContent] ([Id], [BlockId], [EntityValue], [Version], [Content], [IsApproved], [ApprovedByPersonId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid]) VALUES (52, 49, N'', 13, N'<h2>  Welcome to the default page of Rock ChMS</h2> <p>  This is the default page.&nbsp; The navigation menu is also now active.&nbsp; If you don&#39;t see an Administration option above (and you&#39;re an administrator), make sure to login.</p> <p>  v2</p> <p>  &nbsp;</p> <p>  &nbsp;</p> <p>  &nbsp;</p> ', 1, 1, '2012-05-24 14:47:04.740', NULL, NULL, 'f76b1c2c-baf1-45e1-844c-459417fefc69')
INSERT INTO [HtmlContent] ([Id], [BlockId], [EntityValue], [Version], [Content], [IsApproved], [ApprovedByPersonId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid]) VALUES (53, 94, N'', 0, N'<p>  A starter page for HR information from the organization. Think about adding:</p> <ul>  <li>   Commonly used forms</li>  <li>   Important dates</li>  <li>   Links to service providers</li>  <li>   Org Charts</li>  <li>   Payroll&nbsp; schedules</li>  <li>   Government forms</li>  <li>   Process overviews (think new-hire process)</li> </ul> ', 1, 1, '2012-07-06 09:38:33.243', NULL, NULL, '4966b869-a757-48dd-84f1-9807dffa7985')
INSERT INTO [HtmlContent] ([Id], [BlockId], [EntityValue], [Version], [Content], [IsApproved], [ApprovedByPersonId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid]) VALUES (54, 95, N'', 0, N'<p>  Page for posting common documents that are used often.&nbsp; Suggestions:</p> <ul>  <li>   Staff Phone Lists</li>  <li>   Refferal Lists</li>  <li>   Policies and Procedures</li>  <li>   Holiday Schedules</li>  <li>   etc.</li> </ul> ', 1, 1, '2012-07-06 14:57:55.483', NULL, NULL, '48bdec27-77be-4094-9ef9-157e268223b4')
INSERT INTO [HtmlContent] ([Id], [BlockId], [EntityValue], [Version], [Content], [IsApproved], [ApprovedByPersonId], [ApprovedDateTime], [StartDateTime], [ExpireDateTime], [Guid]) VALUES (55, 108, N'', 0, N'<p>  Copyright&nbsp; 2012 Spark Development Network</p> ', 1, 1, '2012-11-29 13:51:50.147', NULL, NULL, 'cf2cbf21-4902-4621-90e5-8b55f963b56a')
SET IDENTITY_INSERT [HtmlContent] OFF

-- Add 59 rows to [Page]
SET IDENTITY_INSERT [Page] ON
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (1, N'External Website Homepage', N'Home', 1, NULL, 1, N'Default', 0, 1, 0, 0, 0, 0, 1, 0, N'This is the description of the default page', 1, NULL, '85f25819-e948-4960-9ddf-00f54d32444e')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (2, N'Sites', N'Sites', 1, 76, 1, N'Default', 0, 1, 1, 0, 0, 0, 6, 0, N'Manage websites', 1, NULL, '7596d389-4eab-4535-8bee-229737f46f44')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (3, N'Login', N'Login', 1, 48, 1, N'Splash', 0, 1, 0, 0, 0, 0, 0, 0, N'Login', 1, NULL, '03cb988a-138c-448b-a43d-8891844eeb18')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (4, N'New Account', N'Create Account', 1, 48, 1, N'Default', 0, 1, 0, 0, 0, 0, 2, 0, N'Create Account', 1, NULL, '7d4e2142-d24e-4dd2-84bc-b34c5c3d0d46')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (7, N'Block Types', N'Block Types', 1, 76, 1, N'Default', 0, 1, 1, 1, 1, 0, 7, 0, N'Manage Block Types', 1, NULL, '5fbe9019-862a-41c6-acdc-287d7934757d')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (9, N'Person', N'Person', 1, NULL, 1, N'Default', 0, 1, 0, 0, 0, 0, 5, 0, N'Person Edit', 1, NULL, 'f8657cb3-c97b-4f24-82c4-b93579a38b4f')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (12, N'Rock ChMS Homepage', N'Rock ChMS', 1, NULL, 1, N'Default', 0, 1, 0, 0, 1, 0, 8, 0, N'Main Rock ChMS', 1, NULL, '20f97a93-7949-4c2a-8a5e-c756fe8585ca')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (16, N'ZoneBlocks', N'Zone Blocks', 1, 74, 1, N'Dialog', 0, 1, 0, 0, 0, 2, 9, 0, N'Admin page for administering the blocks in a zone', 0, NULL, '9f36531f-c1b5-4e23-8fa3-18b6daff1b0b')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (17, N'Intranet', N'Intranet', 1, 12, 1, N'Default', 0, 1, 0, 0, 1, 0, 0, 0, N'Top level nav', 1, NULL, '0c4b3f4d-53fd-4a65-8c93-3868ce4da137')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (18, N'Office Information', N'Office Information', 1, 17, 1, N'Default', 0, 1, 1, 1, 1, 0, 0, 0, N'Below are links to commonly used office information and resources.', 1, N'~/Themes/RockChMS/Assets/Mock-Images/staff-directory.png', '7f2581a1-941e-4d51-8a9d-5be9b881b003')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (19, N'Shared Documents', N'Shared Documents', 1, 18, 1, N'Default', 0, 1, 0, 0, 0, 0, 1, 0, N'', 1, NULL, 'fbc16153-897b-457c-a35f-28fdfdc466b6')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (20, N'Employee Resources', N'Employee Resources', 1, 18, 1, N'Default', 0, 1, 0, 0, 0, 0, 2, 0, N'Human resources information from the organization.', 1, NULL, '895f58fb-c1c4-4399-a4d8-a9a10225ea09')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (23, N'Block Properties', N'Block Properties', 1, 74, 1, N'Dialog', 0, 1, 0, 0, 0, 2, 11, 0, N'Lists the attributes for a block instance', 0, NULL, 'f0b34893-9550-4864-adb4-ee860e4e427c')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (28, N'Security', N'Manage Security', 1, 48, 1, N'Dialog', 0, 1, 0, 0, 0, 0, 13, 0, N'Used to manage security for an entity', 0, NULL, '86d5e33e-e351-4ca5-9925-849c6c467257')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (29, N'ChildPages', N'Child Pages', 1, 74, 1, N'Dialog', 0, 1, 0, 0, 0, 2, 14, 0, N'Manage child pages', 0, NULL, 'd58f205e-e9cc-4bd9-bc79-f3da86f6e346')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (37, N'Page Properties', N'Page Properties', 1, 74, 1, N'Dialog', 0, 1, 0, 0, 0, 2, 15, 0, N'Page Properties', 0, NULL, '37759b50-db4a-440d-a83b-4ef3b4727b1e')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (38, N'Geocoding Services', N'Geocoding Services', 1, 77, 1, N'Default', 0, 1, 1, 0, 0, 0, 3, 0, N'Configuration settings for geocoding webservices.', 1, NULL, '1fd5698f-7279-463f-9637-9a80db86bb86')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (43, N'Finance', N'Finance', 1, 12, 1, N'Default', 0, 1, 0, 0, 1, 0, 4, 0, NULL, 1, NULL, '7beb7569-c485-40a0-a609-b0678f6f7240')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (44, N'Administration', N'Administration', 1, 12, 1, N'Default', 0, 1, 0, 0, 1, 0, 5, 0, NULL, 1, NULL, '84e12152-e456-478e-af68-ba640e9ce65b')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (46, N'Standardization Services', N'Standardization Services', 1, 77, 1, N'Default', 0, 1, 1, 0, 0, 0, 4, 0, N'Configuration settings for address standardization webservices.', 1, NULL, '7112fb95-1f52-448f-9bbc-2ff8b6a3f0a6')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (48, N'Security', N'Security', 1, 44, 1, N'Default', 0, 1, 0, 0, 0, 2, 3, 0, NULL, 1, NULL, '8c71a7e2-18a8-41c0-ab40-ad85cf90ca81')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (49, N'Rock ChMS Settings', N'Rock ChMS Settings', 1, 44, 1, N'Default', 0, 1, 1, 0, 1, 0, 0, 0, N'Used to organized the various settings and configuration pages for the Rock ChMS system.', 1, NULL, '550a898c-edea-48b5-9c58-b20ec13af13b')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (51, N'Global Attributes', N'Global Attributes', 1, 77, 1, N'Default', 0, 1, 1, 0, 0, 0, 0, 0, N'Add / manage Global Attributes.', 1, NULL, 'a2753e03-96b1-4c83-aa11-fcd68c631571')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (54, N'Confirm', N'Confirm', 1, 48, 1, N'Default', 0, 1, 0, 0, 0, 0, 14, 0, NULL, 1, NULL, 'd73f83b4-e20e-4f95-9a2c-511fb669f44c')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (55, N'Change Password', N'Change Password', 1, 48, 1, N'Default', 0, 1, 0, 0, 0, 0, 15, 0, NULL, 1, NULL, '4508223c-2989-4592-b764-b3f372b6051b')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (56, N'Forgot User Name', N'Forgot User Name', 1, 48, 1, N'Default', 0, 1, 0, 0, 0, 0, 16, 0, NULL, 1, NULL, 'c6628fbd-f297-4c23-852e-40f1369c23a8')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (57, N'My Account', N'My Account', 1, 48, 1, N'Default', 0, 1, 0, 0, 0, 0, 17, 0, NULL, 1, NULL, '290c53dc-0960-484c-b314-8301882a454c')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (61, N'Email Templates', N'Email Templates', 1, 79, 1, N'Default', 0, 1, 1, 0, 0, 0, 18, 0, N'Manage email templates', 1, NULL, '89b7a631-ea6f-4da3-9380-04ee67b63e9e')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (71, N'Person Properties', N'Person Properties', 1, 78, 1, N'Default', 0, 1, 1, 0, 0, 0, 0, 0, N'Add and manage Person Properties.', 1, NULL, '7ba1faf4-b63c-4423-a818-cc794ddb14e3')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (72, N'Plugin Settings', N'Plugin Settings', 1, 44, 1, N'Default', 0, 1, 1, 0, 1, 0, 0, 0, N'Settings for custom and third-party plugins and blocks.', 1, NULL, '1afda740-8119-45b8-af4d-58856d469be5')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (73, N'Functions', N'Functions', 1, 43, 1, N'Default', 0, 1, 1, 0, 1, 0, 0, 0, N'Commonly used finance functions.', 1, NULL, '142627ae-6590-48e3-bfca-3669260b8cf2')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (74, N'System Dialogs', N'System Dialogs', 1, 12, 1, N'Default', 0, 1, 0, 0, 0, 2, 6, 0, NULL, 1, NULL, 'e7bd353c-91a6-4c15-a6c8-f44d0b16d16e')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (76, N'CMS Configuration', N'CMS Configuration', 1, 49, 1, N'Default', 0, 1, 1, 0, 1, 0, 1, 0, N'', 1, NULL, 'b4a24ab7-9369-4055-883f-4f4892c39ae3')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (77, N'General Settings', N'General Settings', 1, 49, 1, N'Default', 0, 1, 1, 0, 1, 0, 0, 0, N'The items below represent general configuration settings for the Rock ChMS. Please use caution when making these changes as improper values could cause the system to become unresponsive.', 1, NULL, '0b213645-fa4e-44a5-8e4c-b2d8ef054985')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (78, N'Person Settings', N'Person Settings', 1, 49, 1, N'Default', 0, 1, 1, 0, 1, 0, 2, 0, NULL, 1, NULL, '91ccb1c9-5f9f-44f5-8be2-9ec3a3cfd46f')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (79, N'Communication Settings', N'Communication Settings', 1, 49, 1, N'Default', 0, 1, 1, 0, 1, 0, 3, 0, NULL, 1, NULL, '199dc522-f4d6-4d82-af44-3c16ee9d2cda')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (81, N'Security Roles', N'Security Roles', 1, 77, 1, N'Default', 0, 1, 1, 0, 0, 0, 2, 0, N'Managing security role for pages and blocks.', 1, NULL, 'bed5e775-e630-42e0-8a92-0806e513eaa2')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (82, N'Administration', N'Administration', 0, 43, 1, N'Default', 0, 1, 1, 0, 1, 0, 1, 0, N'Used to make configuration changes to the finance module.', 1, NULL, '18c9e5c3-3e28-4aa3-84f6-78cd4ea2dd3c')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (83, N'Transactions', N'Transactions', 0, 73, 1, N'Default', 0, 1, 0, 0, 0, 0, 0, 0, NULL, 1, NULL, '7ca317b5-5c47-465d-b407-7d614f2a568f')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (84, N'Defined Types', N'Defined Types', 0, 77, 1, N'Default', 0, 1, 1, 0, 0, 0, 5, 0, N'Screen to administrate defined types and values that will be used throughout the application.', 1, NULL, 'e0e1de66-b825-4bfb-a0b3-6e069aa9aa40')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (85, N'Campuses', N'Campuses', 1, 77, 1, N'Default', 0, 1, 1, 0, 0, 0, 6, 0, N'Screen to administrate campuses.', 1, NULL, '5ee91a54-c750-48dc-9392-f1f0f0581c3a')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (86, N'System Information', N'System Information', 1, 74, 1, N'Dialog', 0, 1, 0, 0, 0, 2, 16, 0, N'Displays status and performance information for the currently running instance of Rock ChMS', 0, NULL, '8a97cc93-3e93-4286-8440-e5217b65a904')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (87, N'Plugin Manager', N'Plugin Manager', 1, 72, 1, N'Default', 0, 1, 0, 0, 0, 0, 0, 0, N'Screen to administrate plugins.', 1, NULL, 'b13fcf9a-fae5-4e53-af7c-32df9ca5aae3')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (88, N'Search Services', N'Search Services', 1, 77, 1, N'Default', 0, 1, 1, 0, 0, 0, 7, 0, N'Manage the search interfaces', 1, NULL, '1719f597-5ba9-458d-9362-9c3e558e5c82')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (89, N'Person Search', N'Person Search', 1, NULL, 1, N'Default', 0, 1, 1, 0, 0, 0, 0, 0, N'Screen to administrate campuses.', 1, NULL, '5e036ade-c2a4-4988-b393-dac58230f02e')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (90, N'Tags', N'Tags', 1, 77, 1, N'Default', 0, 1, 1, 0, 1, 0, 8, 0, N'Administer Tags', 1, N'', 'f111791b-6a58-4388-8533-00e913f48f41')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (91, N'Person Tags', N'Person Tags', 1, 90, 1, N'Default', 0, 1, 1, 0, 1, 0, 0, 0, N'Tags related to a person', 1, N'', '9ec914af-d726-4715-934d-49d9f41bf039')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (92, N'Person Pages', N'Person Pages', 1, 12, 1, N'Default', 0, 1, 1, 0, 1, 2, 7, 0, N'Container page for person related pages', 1, N'', 'bf04bb7e-be3a-4a38-a37c-386b55496303')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (93, N'Person Detail', N'Person Detail', 1, 92, 1, N'PersonDetail', 0, 1, 1, 0, 1, 0, 0, 0, N'Displays information about a person', 1, N'', '08dbd8a5-2c35-4146-b4a8-0f7652348b25')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (94, N'Metrics    ', N'Metrics', 0, 77, 1, N'Default', 0, 1, 1, 0, 0, 0, 8, 0, N'Settings for displaying and changing metrics and values.', 1, NULL, '84db9ba0-2725-40a5-a3ca-9a1c043c31b0')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (95, N'Page Routes', N'Page Routes', 1, 76, 1, N'Default', 0, 1, 1, 0, 1, 0, 8, 0, N'List of Page Routes', 1, N'', '4a833be3-7d5e-4c38-af60-5706260015ea')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (96, N'Authentication Services', N'Authentication Services', 1, 77, 1, N'Default', 0, 1, 1, 0, 1, 0, 9, 0, N'List of services used to authenticate user', 1, N'', 'ce2170a9-2c8e-40b1-a42e-dfa73762d01d')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (97, N'Group Types', N'Group Types', 1, 78, 1, N'Default', 0, 1, 1, 0, 1, 0, 1, 0, N'Manage Group Types', 1, N'', '40899bcd-82b0-47f2-8f2a-b6aa3877b445')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (98, N'Groups', N'Groups', 1, 78, 1, N'Default', 0, 1, 1, 0, 1, 0, 2, 0, N'Manage Groups and Group Members', 1, N'', '4d7624fb-a9ae-40bd-82cb-84c22f64343e')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (99, N'Group Roles', N'Group Roles', 1, 78, 1, N'Default', 0, 1, 1, 0, 1, 0, 3, 0, N'Manage Group Roles', 1, N'', 'bbd61bb9-7be0-4f16-9615-91d38f3ae9c9')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (100, N'Marketing Campaigns', N'Marketing Campaigns', 1, 76, 1, N'Default', 0, 1, 1, 0, 1, 0, 9, 0, N'Manage Marketing Campaigns', 1, N'', '74345663-5bca-493c-a2fb-80dc9cc8e70c')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (101, N'Marketing Campaign Ad Types', N'Marketing Campaign Ad Types', 1, 76, 1, N'Default', 0, 1, 1, 0, 1, 0, 10, 0, N'Manage Marketing Campaign Ad Types', 1, N'', 'e6f5f06b-65ee-4949-aa56-1fe4e2933c63')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (102, N'Jobs Administration', N'Jobs Administration', 1, 77, 1, N'Default', 0, 1, 1, 0, 1, 0, 10, 0, N'Administer the automated jobs that run in the background.', 1, N'', 'c58ada1a-6322-4998-8fed-c3565de87efa')
INSERT INTO [Page] ([Id], [Name], [Title], [IsSystem], [ParentPageId], [SiteId], [Layout], [RequiresEncryption], [EnableViewState], [MenuDisplayDescription], [MenuDisplayIcon], [MenuDisplayChildPages], [DisplayInNavWhen], [Order], [OutputCacheDuration], [Description], [IncludeAdminFooter], [IconUrl], [Guid]) VALUES (103, N'Site Map', N'Site Map', 1, 76, 1, N'Default', 0, 1, 1, 0, 1, 0, 11, 0, N'Current Pages and Blocks', 1, N'', 'ec7a06cd-aab5-4455-962e-b4043ea2440e')
SET IDENTITY_INSERT [Page] OFF

-- Add 2 rows to [PageContext]
SET IDENTITY_INSERT [PageContext] ON
INSERT INTO [PageContext] ([Id], [IsSystem], [PageId], [Entity], [IdParameter], [CreatedDateTime], [Guid]) VALUES (1, 1, 9, N'Rock.Crm.Person', N'PersonId', '2012-07-31 06:00:00.000', '09cca802-ee6e-48fb-ad1b-aa603ba13b99')
INSERT INTO [PageContext] ([Id], [IsSystem], [PageId], [Entity], [IdParameter], [CreatedDateTime], [Guid]) VALUES (2, 1, 93, N'Rock.Crm.Person', N'PersonId', '2012-11-29 13:51:50.213', '68e043c8-c7c8-4c1f-b260-8a5ab8e6b3cb')
SET IDENTITY_INSERT [PageContext] OFF

-- Add 20 rows to [PageRoute]
SET IDENTITY_INSERT [PageRoute] ON
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (1, 1, 2, N'Site/{Action}', '6695d509-5ee5-4498-81a1-d0875aecc223')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (2, 1, 2, N'Site/{Action}/{SiteId}', '1580c995-ba49-4612-b291-c7d3ea35d180')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (3, 1, 2, N'Sites', '2444f430-a4bd-4b1f-b9bc-0c872814f77c')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (4, 1, 3, N'Login', '4257a25e-8f4b-4e6c-9e09-822804c01891')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (5, 1, 4, N'NewAccount', '6b940c77-21c7-4f25-beac-c05152d30033')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (6, 1, 7, N'Bloc/{Action}', '9c5f6cd4-e3ba-49b9-8065-129e486f682d')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (7, 1, 7, N'Bloc/{Action}/{BlockId}', 'd079cebc-d820-46db-8572-f36ce47d35bd')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (8, 1, 7, N'Blocs', '5dcf79f0-cbc3-40e9-ba21-51043bdf8573')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (10, 1, 16, N'ZoneBlocks/{EditPage}/{ZoneName}', '1f1a13e4-823c-43f7-b05f-aebc012b7ddd')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (11, 1, 23, N'BlockProperties/{BlockId}', '6438c940-96f7-4a7e-9da5-a30fd4ff143a')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (12, 1, 28, N'Secure/{EntityType}/{EntityId}', '45bd998e-68f8-4aaf-a094-12e4aee217a3')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (13, 1, 29, N'Pages/{EditPage}', '926df96e-55da-467a-864f-6740c04ba400')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (14, 1, 37, N'PageProperties/{Page}', '3676bb9b-c96f-4a8d-a418-fbe223020d8d')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (15, 1, 55, N'ChangePassword', '5230092f-126d-4169-a060-3b65211dcb71')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (16, 1, 54, N'ConfirmAccount', '3c084922-df00-40e6-971b-72ff4234a54f')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (17, 1, 56, N'ForgotUserName', 'a56cf405-ca08-4991-a8d4-e90584e3adab')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (18, 1, 57, N'MyAccount', '8f2692be-fe9a-4715-aa9f-b7412b2fe69a')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (19, 1, 86, N'SystemInfo', '617cf50f-c199-470a-8b32-f9115bdd02c0')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (20, 1, 93, N'Person/{PersonId}', '7e97823a-78a8-4e8e-a337-7a20f2da9e52')
INSERT INTO [PageRoute] ([Id], [IsSystem], [PageId], [Route], [Guid]) VALUES (21, 1, 89, N'Person/Search/{SearchType}/{SearchTerm}', '1d9a7766-71d4-4cc8-a8a5-71c2d100922c')
SET IDENTITY_INSERT [PageRoute] OFF

-- Add 1 row to [Site]
SET IDENTITY_INSERT [Site] ON
INSERT INTO [Site] ([Id], [IsSystem], [Name], [Description], [Theme], [DefaultPageId], [FaviconUrl], [AppleTouchIconUrl], [FacebookAppId], [FacebookAppSecret], [LoginPageReference], [RegistrationPageReference], [ErrorPage], [Guid]) VALUES (1, 1, N'Rock Internal', N'The internal Rock ChMS site.', N'RockChMS', 12, N'Themes/RockChMS/Assets/Icons/favicon.ico', N'Themes/RockChMS/Assets/Icons/apple-touch.png', N'', N'', NULL, NULL, NULL, 'c2d29296-6a87-47a9-a753-ee4e9159c4c4')
SET IDENTITY_INSERT [Site] OFF

-- Add 1 row to [SiteDomain]
SET IDENTITY_INSERT [SiteDomain] ON
INSERT INTO [SiteDomain] ([Id], [IsSystem], [SiteId], [Domain], [Guid]) VALUES (1, 1, 1, N'localhost', 'e07f3ef9-aec3-4ac8-b065-704736b04bea')
SET IDENTITY_INSERT [SiteDomain] OFF

-- Add 6 rows to [Group]
SET IDENTITY_INSERT [Group] ON
INSERT INTO [Group] ([Id], [IsSystem], [ParentGroupId], [GroupTypeId], [CampusId], [Name], [Description], [IsSecurityRole], [IsActive], [Guid]) VALUES (2, 1, NULL, 1, NULL, N'Administrative Users', N'Group of people who are admins on the site', 1, 0, '628c51a8-4613-43ed-a18d-4a6fb999273e')
INSERT INTO [Group] ([Id], [IsSystem], [ParentGroupId], [GroupTypeId], [CampusId], [Name], [Description], [IsSecurityRole], [IsActive], [Guid]) VALUES (3, 1, NULL, 1, NULL, N'Staff Access', N'Used to give rights to the organization''s staff members.', 1, 0, '2c112948-ff4c-46e7-981a-0257681eadf4')
INSERT INTO [Group] ([Id], [IsSystem], [ParentGroupId], [GroupTypeId], [CampusId], [Name], [Description], [IsSecurityRole], [IsActive], [Guid]) VALUES (4, 1, NULL, 1, NULL, N'Website Administrator', N'Group of individuals who administrate portals. They have access to add, remove, update pages and their settings as well as the content on the page.', 1, 0, '1918e74f-c00d-4ddd-94c4-2e7209ce12c3')
INSERT INTO [Group] ([Id], [IsSystem], [ParentGroupId], [GroupTypeId], [CampusId], [Name], [Description], [IsSecurityRole], [IsActive], [Guid]) VALUES (5, 1, NULL, 1, NULL, N'Website Content Editors', N'Group of individuals who have access to edit content on pages, but can not modify settings or add pages.', 1, 0, 'cdf68207-2795-42de-b060-fe01c33beaea')
INSERT INTO [Group] ([Id], [IsSystem], [ParentGroupId], [GroupTypeId], [CampusId], [Name], [Description], [IsSecurityRole], [IsActive], [Guid]) VALUES (18, 1, NULL, 1, NULL, N'Finance Administration', N'Group of individuals who can administrate the various parts of the finance functionality.', 1, 0, '6246a7ef-b7a3-4c8c-b1e4-3ff114b84559')
INSERT INTO [Group] ([Id], [IsSystem], [ParentGroupId], [GroupTypeId], [CampusId], [Name], [Description], [IsSecurityRole], [IsActive], [Guid]) VALUES (19, 1, NULL, 1, NULL, N'Finance User', N'Group of individuals who have basic access to the finance functionality.', 1, 0, '2539cf5d-e2ce-4706-8bbf-4a9df8e763e9')
SET IDENTITY_INSERT [Group] OFF

-- Add 1 row to [GroupMember]
SET IDENTITY_INSERT [GroupMember] ON
INSERT INTO [GroupMember] ([Id], [IsSystem], [GroupId], [PersonId], [GroupRoleId], [Guid]) VALUES (3, 1, 2, 1, 1, '6f23caca-6749-4454-85df-5a55251b644c')
SET IDENTITY_INSERT [GroupMember] OFF

-- Add 6 rows to [GroupRole]
SET IDENTITY_INSERT [GroupRole] ON
INSERT INTO [GroupRole] ([Id], [IsSystem], [GroupTypeId], [Name], [Description], [SortOrder], [MaxCount], [MinCount], [Guid]) VALUES (1, 1, 1, N'Member', N'Member of a group', 1, NULL, NULL, '00f3ac1c-71b9-4ee5-a30e-4c48c8a0bf1f')
INSERT INTO [GroupRole] ([Id], [IsSystem], [GroupTypeId], [Name], [Description], [SortOrder], [MaxCount], [MinCount], [Guid]) VALUES (2, 0, 9, N'Attendee', N'A class attendee', 1, NULL, NULL, 'faf28845-3f76-404e-9613-507c9ff8e135')
INSERT INTO [GroupRole] ([Id], [IsSystem], [GroupTypeId], [Name], [Description], [SortOrder], [MaxCount], [MinCount], [Guid]) VALUES (3, 1, 10, N'Adult', N'Adult Family Member', 0, NULL, NULL, '2639f9a5-2aae-4e48-a8c3-4ffe86681e42')
INSERT INTO [GroupRole] ([Id], [IsSystem], [GroupTypeId], [Name], [Description], [SortOrder], [MaxCount], [MinCount], [Guid]) VALUES (4, 1, 10, N'Child', N'Child Family Member', 1, NULL, NULL, 'c8b1814f-6aa7-4055-b2d7-48fe20429cb9')
INSERT INTO [GroupRole] ([Id], [IsSystem], [GroupTypeId], [Name], [Description], [SortOrder], [MaxCount], [MinCount], [Guid]) VALUES (5, 1, 11, N'Owner', N'Owner of Known Relationships', 0, NULL, NULL, '7bc6c12e-0cd1-4dfd-8d5b-1b35ae714c42')
INSERT INTO [GroupRole] ([Id], [IsSystem], [GroupTypeId], [Name], [Description], [SortOrder], [MaxCount], [MinCount], [Guid]) VALUES (6, 1, 12, N'Owner', N'Owner of Implied Relationships', 0, NULL, NULL, 'cb9a0e14-6fcf-4c07-a49a-d7873f45e196')
SET IDENTITY_INSERT [GroupRole] OFF

-- Add 7 rows to [GroupType]
SET IDENTITY_INSERT [GroupType] ON
INSERT INTO [GroupType] ([Id], [IsSystem], [Name], [Description], [DefaultGroupRoleId], [Guid]) VALUES (1, 1, N'Security Roles', N'', 1, 'aece949f-704c-483e-a4fb-93d5e4720c4c')
INSERT INTO [GroupType] ([Id], [IsSystem], [Name], [Description], [DefaultGroupRoleId], [Guid]) VALUES (5, 0, N'Class Category', N'Online Class Category', NULL, 'bc46862a-fc37-43ff-a107-d2d7460bdde5')
INSERT INTO [GroupType] ([Id], [IsSystem], [Name], [Description], [DefaultGroupRoleId], [Guid]) VALUES (9, 0, N'Class Session', N'Online Class Session', 2, 'c663546d-ac65-48bf-be3a-8bfc3f69ceb3')
INSERT INTO [GroupType] ([Id], [IsSystem], [Name], [Description], [DefaultGroupRoleId], [Guid]) VALUES (10, 1, N'Family', N'Family Members', NULL, '790e3215-3b10-442b-af69-616c0dcb998e')
INSERT INTO [GroupType] ([Id], [IsSystem], [Name], [Description], [DefaultGroupRoleId], [Guid]) VALUES (11, 1, N'Known Relationships', N'Manually configured relationships', NULL, 'e0c5a0e2-b7b3-4ef4-820d-bbf7f9a374ef')
INSERT INTO [GroupType] ([Id], [IsSystem], [Name], [Description], [DefaultGroupRoleId], [Guid]) VALUES (12, 1, N'Implied Relationships', N'System discovered relationships', NULL, '8c0e5852-f08f-4327-9aa5-87800a6ab53e')
INSERT INTO [GroupType] ([Id], [IsSystem], [Name], [Description], [DefaultGroupRoleId], [Guid]) VALUES (13, 1, N'Event Attendees', N'Event Attendees', NULL, '3311132b-268d-44e9-811a-a56a0835e50a')
SET IDENTITY_INSERT [GroupType] OFF

-- Add 2 rows to [GroupTypeAssociation]
INSERT INTO [GroupTypeAssociation] ([GroupTypeId], [ChildGroupTypeId]) VALUES (5, 5)
INSERT INTO [GroupTypeAssociation] ([GroupTypeId], [ChildGroupTypeId]) VALUES (9, 5)

-- disable constraints on all
alter table [Auth] check constraint all
alter table [Block] check constraint all
alter table [BlockType] check constraint all
alter table [BinaryFile] check constraint all
alter table [HtmlContent] check constraint all
alter table [MarketingCampaign] check constraint all
alter table [MarketingCampaignAd] check constraint all
alter table [MarketingCampaignAdType] check constraint all
alter table [MarketingCampaignAudience] check constraint all
alter table [MarketingCampaignCampus] check constraint all
alter table [Page] check constraint all
alter table [PageContext] check constraint all
alter table [PageRoute] check constraint all
alter table [Site] check constraint all
alter table [SiteDomain] check constraint all
alter table [UserLogin] check constraint all
alter table [Attribute] check constraint all
alter table [AttributeQualifier] check constraint all
alter table [AttributeValue] check constraint all
alter table [Audit] check constraint all
alter table [Category] check constraint all
alter table [DefinedType] check constraint all
alter table [DefinedValue] check constraint all
alter table [EntityChange] check constraint all
alter table [EntityType] check constraint all
alter table [ExceptionLog] check constraint all
alter table [FieldType] check constraint all
alter table [Metric] check constraint all
alter table [MetricValue] check constraint all
alter table [ServiceLog] check constraint all
alter table [Tag] check constraint all
alter table [TaggedItem] check constraint all
alter table [Campus] check constraint all
alter table [EmailTemplate] check constraint all
alter table [Group] check constraint all
alter table [GroupLocation] check constraint all
alter table [GroupMember] check constraint all
alter table [GroupRole] check constraint all
alter table [GroupType] check constraint all
alter table [GroupTypeAssociation] check constraint all
alter table [GroupTypeLocationType] check constraint all
alter table [Location] check constraint all
alter table [Person] check constraint all
alter table [PersonMerged] check constraint all
alter table [PersonViewed] check constraint all
alter table [PhoneNumber] check constraint all
alter table [FinancialBatch] check constraint all
alter table [Fund] check constraint all
alter table [PaymentGateway] check constraint all
alter table [PersonAccount] check constraint all
alter table [Pledge] check constraint all
alter table [FinancialTransaction] check constraint all
alter table [FinancialTransactionDetail] check constraint all
alter table [FinancialTransactionFund] check constraint all
alter table [WorkflowAction] check constraint all
alter table [WorkflowActionType] check constraint all
alter table [WorkflowActivity] check constraint all
alter table [WorkflowActivityType] check constraint all
alter table [ServiceJob] check constraint all
alter table [Workflow] check constraint all
alter table [WorkflowLog] check constraint all
alter table [WorkflowTrigger] check constraint all
alter table [WorkflowType] check constraint all
" );
            // Stored Procs
            Sql( @"
CREATE PROCEDURE [BinaryFile_sp_getByID]
    @Id int,
    @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id > 0
    BEGIN
        SELECT Data, FileName, MimeType FROM [BinaryFile] WHERE [Id] = @Id
    END
    ELSE
    BEGIN
        SELECT Data, FileName, MimeType FROM [BinaryFile] WHERE [Guid] = @Guid
    END
END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}
