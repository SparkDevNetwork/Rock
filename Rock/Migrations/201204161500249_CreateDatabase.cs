namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class CreateDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cmsAuth",
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
                "crmPerson",
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
                "cmsUser",
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
                "crmEmailTemplate",
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
                "crmPhoneNumber",
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
                "groupsMember",
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
                "groupsGroup",
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
                "groupsGroupType",
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
                "groupsGroupRole",
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
                "coreDefinedValue",
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
                "coreDefinedType",
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
                "coreFieldType",
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
                "coreAttribute",
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
                        DefaultValue = c.String(nullable: false),
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
                "coreAttributeQualifier",
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
                "coreAttributeValue",
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
                "cmsBlock",
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
                "cmsBlockInstance",
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
                "cmsHtmlContent",
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
                "cmsPage",
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
                "cmsPageRoute",
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
                "cmsSite",
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
                "cmsSiteDomain",
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
                "cmsFile",
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
                "coreEntityChange",
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
                "coreExceptionLog",
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
                "coreServiceLog",
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
                "crmAddress",
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
                "crmPersonTrail",
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
                "crmPersonViewed",
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
                "utilJob",
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
                "groupsGroupTypeAssociation",
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
                "groupsGroupTypeRole",
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
        }
        
        public override void Down()
        {
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
            DropForeignKey("groupsGroupTypeRole", "GroupRoleId", "groupsGroupType");
            DropForeignKey("groupsGroupTypeRole", "GroupTypeId", "groupsGroupRole");
            DropForeignKey("groupsGroupTypeAssociation", "ParentGroupTypeId", "groupsGroupType");
            DropForeignKey("groupsGroupTypeAssociation", "ChildGroupTypeId", "groupsGroupType");
            DropForeignKey("utilJob", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("utilJob", "CreatedByPersonId", "crmPerson");
            DropForeignKey("crmPersonViewed", "TargetPersonId", "crmPerson");
            DropForeignKey("crmPersonViewed", "ViewerPersonId", "crmPerson");
            DropForeignKey("crmAddress", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("crmAddress", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreExceptionLog", "PersonId", "crmPerson");
            DropForeignKey("coreEntityChange", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsFile", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsFile", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsSiteDomain", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsSiteDomain", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsSiteDomain", "SiteId", "cmsSite");
            DropForeignKey("cmsSite", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsSite", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsSite", "DefaultPageId", "cmsPage");
            DropForeignKey("cmsPageRoute", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsPageRoute", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsPageRoute", "PageId", "cmsPage");
            DropForeignKey("cmsPage", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsPage", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsPage", "SiteId", "cmsSite");
            DropForeignKey("cmsPage", "ParentPageId", "cmsPage");
            DropForeignKey("cmsHtmlContent", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsHtmlContent", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsHtmlContent", "ApprovedByPersonId", "crmPerson");
            DropForeignKey("cmsHtmlContent", "BlockId", "cmsBlockInstance");
            DropForeignKey("cmsBlockInstance", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsBlockInstance", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsBlockInstance", "PageId", "cmsPage");
            DropForeignKey("cmsBlockInstance", "BlockId", "cmsBlock");
            DropForeignKey("cmsBlock", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsBlock", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreAttributeValue", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("coreAttributeValue", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreAttributeValue", "AttributeId", "coreAttribute");
            DropForeignKey("coreAttributeQualifier", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("coreAttributeQualifier", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreAttributeQualifier", "AttributeId", "coreAttribute");
            DropForeignKey("coreAttribute", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("coreAttribute", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreAttribute", "FieldTypeId", "coreFieldType");
            DropForeignKey("coreFieldType", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("coreFieldType", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreDefinedType", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("coreDefinedType", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreDefinedType", "FieldTypeId", "coreFieldType");
            DropForeignKey("coreDefinedValue", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("coreDefinedValue", "CreatedByPersonId", "crmPerson");
            DropForeignKey("coreDefinedValue", "DefinedTypeId", "coreDefinedType");
            DropForeignKey("groupsGroupRole", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("groupsGroupRole", "CreatedByPersonId", "crmPerson");
            DropForeignKey("groupsGroupType", "DefaultGroupRoleId", "groupsGroupRole");
            DropForeignKey("groupsGroupType", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("groupsGroupType", "CreatedByPersonId", "crmPerson");
            DropForeignKey("groupsGroup", "GroupTypeId", "groupsGroupType");
            DropForeignKey("groupsGroup", "ParentGroupId", "groupsGroup");
            DropForeignKey("groupsGroup", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("groupsGroup", "CreatedByPersonId", "crmPerson");
            DropForeignKey("groupsMember", "GroupRoleId", "groupsGroupRole");
            DropForeignKey("groupsMember", "GroupId", "groupsGroup");
            DropForeignKey("groupsMember", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("groupsMember", "CreatedByPersonId", "crmPerson");
            DropForeignKey("groupsMember", "PersonId", "crmPerson");
            DropForeignKey("crmPhoneNumber", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("crmPhoneNumber", "CreatedByPersonId", "crmPerson");
            DropForeignKey("crmPhoneNumber", "PersonId", "crmPerson");
            DropForeignKey("crmEmailTemplate", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("crmEmailTemplate", "PersonId", "crmPerson");
            DropForeignKey("crmEmailTemplate", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsUser", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsUser", "CreatedByPersonId", "crmPerson");
            DropForeignKey("cmsUser", "PersonId", "crmPerson");
            DropForeignKey("crmPerson", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("crmPerson", "CreatedByPersonId", "crmPerson");
            DropForeignKey("crmPerson", "TitleId", "coreDefinedValue");
            DropForeignKey("crmPerson", "SuffixId", "coreDefinedValue");
            DropForeignKey("crmPerson", "RecordTypeId", "coreDefinedValue");
            DropForeignKey("crmPerson", "RecordStatusReasonId", "coreDefinedValue");
            DropForeignKey("crmPerson", "RecordStatusId", "coreDefinedValue");
            DropForeignKey("crmPerson", "PersonStatusId", "coreDefinedValue");
            DropForeignKey("crmPerson", "MaritalStatusId", "coreDefinedValue");
            DropForeignKey("cmsAuth", "PersonId", "crmPerson");
            DropForeignKey("cmsAuth", "GroupId", "groupsGroup");
            DropForeignKey("cmsAuth", "ModifiedByPersonId", "crmPerson");
            DropForeignKey("cmsAuth", "CreatedByPersonId", "crmPerson");
            DropTable("groupsGroupTypeRole");
            DropTable("groupsGroupTypeAssociation");
            DropTable("utilJob");
            DropTable("crmPersonViewed");
            DropTable("crmPersonTrail");
            DropTable("crmAddress");
            DropTable("coreServiceLog");
            DropTable("coreExceptionLog");
            DropTable("coreEntityChange");
            DropTable("cmsFile");
            DropTable("cmsSiteDomain");
            DropTable("cmsSite");
            DropTable("cmsPageRoute");
            DropTable("cmsPage");
            DropTable("cmsHtmlContent");
            DropTable("cmsBlockInstance");
            DropTable("cmsBlock");
            DropTable("coreAttributeValue");
            DropTable("coreAttributeQualifier");
            DropTable("coreAttribute");
            DropTable("coreFieldType");
            DropTable("coreDefinedType");
            DropTable("coreDefinedValue");
            DropTable("groupsGroupRole");
            DropTable("groupsGroupType");
            DropTable("groupsGroup");
            DropTable("groupsMember");
            DropTable("crmPhoneNumber");
            DropTable("crmEmailTemplate");
            DropTable("cmsUser");
            DropTable("crmPerson");
            DropTable("cmsAuth");
        }
    }
}
