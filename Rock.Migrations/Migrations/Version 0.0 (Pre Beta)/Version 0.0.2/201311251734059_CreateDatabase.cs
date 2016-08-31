// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CreateDatabase : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AttendanceCode",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IssueDateTime = c.DateTime(nullable: false),
                        Code = c.String(maxLength: 10),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.AttendanceCode", "Code", true );
            
            CreateIndex( "dbo.AttendanceCode", "Guid", true );
            
            CreateTable(
                "dbo.Attendance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocationId = c.Int(),
                        ScheduleId = c.Int(),
                        GroupId = c.Int(),
                        PersonId = c.Int(),
                        DeviceId = c.Int(),
                        SearchTypeValueId = c.Int(),
                        AttendanceCodeId = c.Int(),
                        QualifierValueId = c.Int(),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(),
                        DidAttend = c.Boolean(nullable: false),
                        Note = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AttendanceCode", t => t.AttendanceCodeId)
                .ForeignKey("dbo.Device", t => t.DeviceId)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Location", t => t.LocationId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.QualifierValueId)
                .ForeignKey("dbo.Schedule", t => t.ScheduleId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.SearchTypeValueId)
                .Index(t => t.AttendanceCodeId)
                .Index(t => t.DeviceId)
                .Index(t => t.GroupId)
                .Index(t => t.LocationId)
                .Index(t => t.PersonId)
                .Index(t => t.QualifierValueId)
                .Index(t => t.ScheduleId)
                .Index(t => t.SearchTypeValueId);
            
            CreateIndex( "dbo.Attendance", "Guid", true );
            
            CreateTable(
                "dbo.Device",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        DeviceTypeValueId = c.Int(nullable: false),
                        LocationId = c.Int(),
                        IPAddress = c.String(maxLength: 45),
                        PrinterDeviceId = c.Int(),
                        PrintFrom = c.Int(nullable: false),
                        PrintToOverride = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedValue", t => t.DeviceTypeValueId)
                .ForeignKey("dbo.Location", t => t.LocationId)
                .ForeignKey("dbo.Device", t => t.PrinterDeviceId)
                .Index(t => t.DeviceTypeValueId)
                .Index(t => t.LocationId)
                .Index(t => t.PrinterDeviceId);
            
            CreateIndex( "dbo.Device", "Name", true );
            
            CreateIndex( "dbo.Device", "Guid", true );
            
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
                .ForeignKey("dbo.DefinedType", t => t.DefinedTypeId, cascadeDelete: true)
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
                "dbo.Location",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentLocationId = c.Int(),
                        Name = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        LocationTypeValueId = c.Int(),
                        GeoPoint = c.Geography(),
                        GeoFence = c.Geography(),
                        IsNamedLocation = c.Boolean(nullable: false),
                        Street1 = c.String(maxLength: 100),
                        Street2 = c.String(maxLength: 100),
                        City = c.String(maxLength: 50),
                        State = c.String(maxLength: 50),
                        Country = c.String(maxLength: 50),
                        Zip = c.String(maxLength: 10),
                        FullAddress = c.String(maxLength: 400),
                        AssessorParcelId = c.String(maxLength: 50),
                        StandardizeAttemptedDateTime = c.DateTime(),
                        StandardizeAttemptedServiceType = c.String(maxLength: 50),
                        StandardizeAttemptedResult = c.String(maxLength: 50),
                        StandardizedDateTime = c.DateTime(),
                        GeocodeAttemptedDateTime = c.DateTime(),
                        GeocodeAttemptedServiceType = c.String(maxLength: 50),
                        GeocodeAttemptedResult = c.String(maxLength: 50),
                        GeocodedDateTime = c.DateTime(),
                        PrinterDeviceId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Location", t => t.ParentLocationId)
                .ForeignKey("dbo.Device", t => t.PrinterDeviceId)
                .Index(t => t.ParentLocationId)
                .Index(t => t.PrinterDeviceId);
            
            CreateIndex( "dbo.Location", "Guid", true );
            
            CreateTable(
                "dbo.GroupLocation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        GroupLocationTypeValueId = c.Int(),
                        IsMailingLocation = c.Boolean(nullable: false),
                        IsMappedLocation = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Location", t => t.LocationId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.GroupLocationTypeValueId)
                .Index(t => t.GroupId)
                .Index(t => t.LocationId)
                .Index(t => t.GroupLocationTypeValueId);
            
            CreateIndex( "dbo.GroupLocation", "Guid", true );
            
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
                        Order = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId)
                .ForeignKey("dbo.Group", t => t.ParentGroupId)
                .Index(t => t.CampusId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.ParentGroupId);
            
            CreateIndex( "dbo.Group", "Guid", true );
            
            CreateTable(
                "dbo.Campus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        ShortCode = c.String(maxLength: 50),
                        LocationId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Location", t => t.LocationId)
                .Index(t => t.LocationId);
            
            CreateIndex( "dbo.Campus", "Name", true );
            
            CreateIndex( "dbo.Campus", "Guid", true );
            
            CreateTable(
                "dbo.GroupType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        GroupTerm = c.String(nullable: false, maxLength: 100),
                        GroupMemberTerm = c.String(nullable: false, maxLength: 100),
                        DefaultGroupRoleId = c.Int(),
                        AllowMultipleLocations = c.Boolean(nullable: false),
                        ShowInGroupList = c.Boolean(nullable: false),
                        ShowInNavigation = c.Boolean(nullable: false),
                        IconSmallFileId = c.Int(),
                        IconLargeFileId = c.Int(),
                        IconCssClass = c.String(),
                        TakesAttendance = c.Boolean(nullable: false),
                        AttendanceRule = c.Int(nullable: false),
                        AttendancePrintTo = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        InheritedGroupTypeId = c.Int(),
                        LocationSelectionMode = c.Int(nullable: false),
                        GroupTypePurposeValueId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupTypeRole", t => t.DefaultGroupRoleId)
                .ForeignKey("dbo.DefinedValue", t => t.GroupTypePurposeValueId)
                .ForeignKey("dbo.BinaryFile", t => t.IconLargeFileId)
                .ForeignKey("dbo.BinaryFile", t => t.IconSmallFileId)
                .ForeignKey("dbo.GroupType", t => t.InheritedGroupTypeId)
                .Index(t => t.DefaultGroupRoleId)
                .Index(t => t.GroupTypePurposeValueId)
                .Index(t => t.IconLargeFileId)
                .Index(t => t.IconSmallFileId)
                .Index(t => t.InheritedGroupTypeId);
            
            CreateIndex( "dbo.GroupType", "Guid", true );
            
            CreateTable(
                "dbo.GroupTypeRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        GroupTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Order = c.Int(nullable: false),
                        MaxCount = c.Int(),
                        MinCount = c.Int(),
                        IsLeader = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId, cascadeDelete: true)
                .Index(t => t.GroupTypeId);
            
            CreateIndex( "dbo.GroupTypeRole", "Guid", true );
            
            CreateTable(
                "dbo.BinaryFile",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsTemporary = c.Boolean(nullable: false),
                        IsSystem = c.Boolean(nullable: false),
                        BinaryFileTypeId = c.Int(nullable: false),
                        Url = c.String(maxLength: 255),
                        FileName = c.String(nullable: false, maxLength: 255),
                        MimeType = c.String(nullable: false, maxLength: 255),
                        LastModifiedDateTime = c.DateTime(),
                        Description = c.String(),
                        StorageEntityTypeId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFileType", t => t.BinaryFileTypeId)
                .ForeignKey("dbo.EntityType", t => t.StorageEntityTypeId)
                .Index(t => t.BinaryFileTypeId)
                .Index(t => t.StorageEntityTypeId);
            
            CreateIndex( "dbo.BinaryFile", "Guid", true );
            
            CreateTable(
                "dbo.BinaryFileType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IconSmallFileId = c.Int(),
                        IconLargeFileId = c.Int(),
                        IconCssClass = c.String(),
                        StorageEntityTypeId = c.Int(),
                        AllowCaching = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFile", t => t.IconLargeFileId)
                .ForeignKey("dbo.BinaryFile", t => t.IconSmallFileId)
                .ForeignKey("dbo.EntityType", t => t.StorageEntityTypeId)
                .Index(t => t.IconLargeFileId)
                .Index(t => t.IconSmallFileId)
                .Index(t => t.StorageEntityTypeId);
            
            CreateIndex( "dbo.BinaryFileType", "Name", true );
            
            CreateIndex( "dbo.BinaryFileType", "Guid", true );
            
            CreateTable(
                "dbo.EntityType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        AssemblyName = c.String(maxLength: 260),
                        FriendlyName = c.String(maxLength: 100),
                        IsEntity = c.Boolean(nullable: false),
                        IsSecured = c.Boolean(nullable: false),
                        IsCommon = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.EntityType", "Name", true );
            
            CreateIndex( "dbo.EntityType", "Guid", true );
            
            CreateTable(
                "dbo.BinaryFileData",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Content = c.Binary(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFile", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);
            
            CreateIndex( "dbo.BinaryFileData", "Guid", true );
            
            CreateTable(
                "dbo.GroupTypeLocationType",
                c => new
                    {
                        GroupTypeId = c.Int(nullable: false),
                        LocationTypeValueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupTypeId, t.LocationTypeValueId })
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.LocationTypeValueId, cascadeDelete: true)
                .Index(t => t.GroupTypeId)
                .Index(t => t.LocationTypeValueId);
            
            CreateTable(
                "dbo.GroupMember",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        GroupId = c.Int(nullable: false),
                        PersonId = c.Int(nullable: false),
                        GroupRoleId = c.Int(nullable: false),
                        GroupMemberStatus = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.GroupTypeRole", t => t.GroupRoleId)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.GroupRoleId)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.GroupMember", "Guid", true );
            
            CreateTable(
                "dbo.Person",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        RecordTypeValueId = c.Int(),
                        RecordStatusValueId = c.Int(),
                        RecordStatusReasonValueId = c.Int(),
                        PersonStatusValueId = c.Int(),
                        IsDeceased = c.Boolean(),
                        TitleValueId = c.Int(),
                        GivenName = c.String(maxLength: 50),
                        NickName = c.String(maxLength: 50),
                        MiddleName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        SuffixValueId = c.Int(),
                        PhotoId = c.Int(),
                        BirthDay = c.Int(),
                        BirthMonth = c.Int(),
                        BirthYear = c.Int(),
                        Gender = c.Int(nullable: false),
                        MaritalStatusValueId = c.Int(),
                        AnniversaryDate = c.DateTime(storeType: "date"),
                        GraduationDate = c.DateTime(storeType: "date"),
                        GivingGroupId = c.Int(),
                        Email = c.String(maxLength: 75),
                        IsEmailActive = c.Boolean(),
                        EmailNote = c.String(maxLength: 250),
                        DoNotEmail = c.Boolean(nullable: false),
                        SystemNote = c.String(maxLength: 1000),
                        ViewedCount = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.GivingGroupId)
                .ForeignKey("dbo.DefinedValue", t => t.MaritalStatusValueId)
                .ForeignKey("dbo.DefinedValue", t => t.PersonStatusValueId)
                .ForeignKey("dbo.BinaryFile", t => t.PhotoId)
                .ForeignKey("dbo.DefinedValue", t => t.RecordStatusReasonValueId)
                .ForeignKey("dbo.DefinedValue", t => t.RecordStatusValueId)
                .ForeignKey("dbo.DefinedValue", t => t.RecordTypeValueId)
                .ForeignKey("dbo.DefinedValue", t => t.SuffixValueId)
                .ForeignKey("dbo.DefinedValue", t => t.TitleValueId)
                .Index(t => t.GivingGroupId)
                .Index(t => t.MaritalStatusValueId)
                .Index(t => t.PersonStatusValueId)
                .Index(t => t.PhotoId)
                .Index(t => t.RecordStatusReasonValueId)
                .Index(t => t.RecordStatusValueId)
                .Index(t => t.RecordTypeValueId)
                .Index(t => t.SuffixValueId)
                .Index(t => t.TitleValueId);
            
            CreateIndex( "dbo.Person", "Guid", true );
            
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
                        NumberTypeValueId = c.Int(),
                        IsMessagingEnabled = c.Boolean(nullable: false),
                        IsUnlisted = c.Boolean(nullable: false),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedValue", t => t.NumberTypeValueId)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.NumberTypeValueId)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.PhoneNumber", "Guid", true );
            
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
                        LastActivityDateTime = c.DateTime(),
                        LastLoginDateTime = c.DateTime(),
                        LastPasswordChangedDateTime = c.DateTime(),
                        CreationDateTime = c.DateTime(),
                        IsOnLine = c.Boolean(),
                        IsLockedOut = c.Boolean(),
                        LastLockedOutDateTime = c.DateTime(),
                        FailedPasswordAttemptCount = c.Int(),
                        FailedPasswordAttemptWindowStartDateTime = c.DateTime(),
                        LastPasswordExpirationWarningDateTime = c.DateTime(),
                        ApiKey = c.String(maxLength: 50),
                        PersonId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.UserLogin", "Guid", true );
            
            CreateTable(
                "dbo.Schedule",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        iCalendarContent = c.String(),
                        CheckInStartOffsetMinutes = c.Int(),
                        CheckInEndOffsetMinutes = c.Int(),
                        EffectiveStartDate = c.DateTime(storeType: "date"),
                        EffectiveEndDate = c.DateTime(storeType: "date"),
                        CategoryId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .Index(t => t.CategoryId);
            
            CreateIndex( "dbo.Schedule", "Guid", true );
            
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
                        Name = c.String(nullable: false, maxLength: 100),
                        IconSmallFileId = c.Int(),
                        IconLargeFileId = c.Int(),
                        IconCssClass = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.BinaryFile", t => t.IconLargeFileId)
                .ForeignKey("dbo.BinaryFile", t => t.IconSmallFileId)
                .ForeignKey("dbo.Category", t => t.ParentCategoryId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.IconLargeFileId)
                .Index(t => t.IconSmallFileId)
                .Index(t => t.ParentCategoryId);
            
            CreateIndex( "dbo.Category", "Guid", true );
            
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
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.EntityTypeId)
                .Index(t => t.GroupId)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.Auth", "Guid", true );
            
            CreateTable(
                "dbo.Block",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        PageId = c.Int(),
                        LayoutId = c.Int(),
                        BlockTypeId = c.Int(nullable: false),
                        Zone = c.String(nullable: false, maxLength: 100),
                        Order = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        CssClass = c.String(maxLength: 100),
                        OutputCacheDuration = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BlockType", t => t.BlockTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Layout", t => t.LayoutId, cascadeDelete: true)
                .ForeignKey("dbo.Page", t => t.PageId, cascadeDelete: true)
                .Index(t => t.BlockTypeId)
                .Index(t => t.LayoutId)
                .Index(t => t.PageId);
            
            CreateIndex( "dbo.Block", "Guid", true );
            
            CreateTable(
                "dbo.BlockType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Path = c.String(nullable: false, maxLength: 260),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.BlockType", "Guid", true );
            
            CreateTable(
                "dbo.Layout",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        SiteId = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 260),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Site", t => t.SiteId, cascadeDelete: true)
                .Index(t => t.SiteId);
            
            CreateTable(
                "dbo.Page",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        ParentPageId = c.Int(),
                        Title = c.String(maxLength: 100),
                        IsSystem = c.Boolean(nullable: false),
                        LayoutId = c.Int(nullable: false),
                        RequiresEncryption = c.Boolean(nullable: false),
                        EnableViewState = c.Boolean(nullable: false),
                        PageDisplayTitle = c.Boolean(nullable: false),
                        PageDisplayBreadCrumb = c.Boolean(nullable: false),
                        PageDisplayIcon = c.Boolean(nullable: false),
                        PageDisplayDescription = c.Boolean(nullable: false),
                        DisplayInNavWhen = c.Int(nullable: false),
                        MenuDisplayDescription = c.Boolean(nullable: false),
                        MenuDisplayIcon = c.Boolean(nullable: false),
                        MenuDisplayChildPages = c.Boolean(nullable: false),
                        BreadCrumbDisplayName = c.Boolean(nullable: false),
                        BreadCrumbDisplayIcon = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        OutputCacheDuration = c.Int(nullable: false),
                        Description = c.String(),
                        IconFileId = c.Int(),
                        IconCssClass = c.String(),
                        IncludeAdminFooter = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFile", t => t.IconFileId)
                .ForeignKey("dbo.Layout", t => t.LayoutId)
                .ForeignKey("dbo.Page", t => t.ParentPageId)
                .Index(t => t.IconFileId)
                .Index(t => t.LayoutId)
                .Index(t => t.ParentPageId);
            
            CreateIndex( "dbo.Page", "Guid", true );
            
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
                "dbo.Site",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Theme = c.String(maxLength: 100),
                        DefaultPageId = c.Int(nullable: false),
                        DefaultPageRouteId = c.Int(),
                        LoginPageId = c.Int(),
                        LoginPageRouteId = c.Int(),
                        RegistrationPageId = c.Int(),
                        RegistrationPageRouteId = c.Int(),
                        FaviconUrl = c.String(maxLength: 150),
                        AppleTouchIconUrl = c.String(maxLength: 150),
                        FacebookAppId = c.String(maxLength: 25),
                        FacebookAppSecret = c.String(maxLength: 50),
                        ErrorPage = c.String(maxLength: 260),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Page", t => t.DefaultPageId)
                .ForeignKey("dbo.PageRoute", t => t.DefaultPageRouteId)
                .ForeignKey("dbo.Page", t => t.LoginPageId)
                .ForeignKey("dbo.PageRoute", t => t.LoginPageRouteId)
                .ForeignKey("dbo.Page", t => t.RegistrationPageId)
                .ForeignKey("dbo.PageRoute", t => t.RegistrationPageRouteId)
                .Index(t => t.DefaultPageId)
                .Index(t => t.DefaultPageRouteId)
                .Index(t => t.LoginPageId)
                .Index(t => t.LoginPageRouteId)
                .Index(t => t.RegistrationPageId)
                .Index(t => t.RegistrationPageRouteId);
            
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
                "dbo.CommunicationRecipient",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        CommunicationId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        StatusNote = c.String(),
                        AdditionalMergeValuesJson = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Communication", t => t.CommunicationId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.CommunicationId)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.CommunicationRecipient", "Guid", true );
            
            CreateTable(
                "dbo.Communication",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SenderPersonId = c.Int(),
                        Subject = c.String(maxLength: 100),
                        FutureSendDateTime = c.DateTime(),
                        Status = c.Int(nullable: false),
                        ReviewerPersonId = c.Int(),
                        ReviewedDateTime = c.DateTime(),
                        ReviewerNote = c.String(),
                        ChannelEntityTypeId = c.Int(),
                        ChannelDataJson = c.String(),
                        AdditionalMergeFieldsJson = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.ChannelEntityTypeId)
                .ForeignKey("dbo.Person", t => t.ReviewerPersonId)
                .ForeignKey("dbo.Person", t => t.SenderPersonId)
                .Index(t => t.ChannelEntityTypeId)
                .Index(t => t.ReviewerPersonId)
                .Index(t => t.SenderPersonId);
            
            CreateIndex( "dbo.Communication", "Guid", true );
            
            CreateTable(
                "dbo.DataViewFilter",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExpressionType = c.Int(nullable: false),
                        ParentId = c.Int(),
                        EntityTypeId = c.Int(),
                        Selection = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.DataViewFilter", t => t.ParentId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.ParentId);
            
            CreateIndex( "dbo.DataViewFilter", "Guid", true );
            
            CreateTable(
                "dbo.DataView",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CategoryId = c.Int(),
                        EntityTypeId = c.Int(nullable: false),
                        DataViewFilterId = c.Int(),
                        TransformEntityTypeId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.DataViewFilter", t => t.DataViewFilterId, cascadeDelete: true)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.EntityType", t => t.TransformEntityTypeId)
                .Index(t => t.CategoryId)
                .Index(t => t.DataViewFilterId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.TransformEntityTypeId);
            
            CreateIndex( "dbo.DataView", "Guid", true );
            
            CreateTable(
                "dbo.ExceptionLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentId = c.Int(),
                        SiteId = c.Int(),
                        PageId = c.Int(),
                        ExceptionDateTime = c.DateTime(nullable: false),
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
                .ForeignKey("dbo.Page", t => t.PageId, cascadeDelete: true)
                .ForeignKey("dbo.Site", t => t.SiteId, cascadeDelete: true)
                .Index(t => t.CreatedByPersonId)
                .Index(t => t.PageId)
                .Index(t => t.SiteId);
            
            CreateIndex( "dbo.ExceptionLog", "Guid", true );
            
            CreateTable(
                "dbo.FinancialAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentAccountId = c.Int(),
                        CampusId = c.Int(),
                        Name = c.String(nullable: false, maxLength: 50),
                        PublicName = c.String(maxLength: 50),
                        Description = c.String(),
                        IsTaxDeductible = c.Boolean(nullable: false),
                        GlCode = c.String(maxLength: 50),
                        Order = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        StartDate = c.DateTime(storeType: "date"),
                        EndDate = c.DateTime(storeType: "date"),
                        AccountTypeValueId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedValue", t => t.AccountTypeValueId)
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.FinancialAccount", t => t.ParentAccountId)
                .Index(t => t.AccountTypeValueId)
                .Index(t => t.CampusId)
                .Index(t => t.ParentAccountId);
            
            CreateIndex( "dbo.FinancialAccount", "Guid", true );
            
            CreateTable(
                "dbo.FinancialBatch",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        BatchStartDateTime = c.DateTime(),
                        BatchEndDateTime = c.DateTime(),
                        CreatedByPersonId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        CampusId = c.Int(),
                        AccountingSystemCode = c.String(maxLength: 100),
                        ControlAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.Person", t => t.CreatedByPersonId)
                .Index(t => t.CampusId)
                .Index(t => t.CreatedByPersonId);
            
            CreateIndex( "dbo.FinancialBatch", "Guid", true );
            
            CreateTable(
                "dbo.FinancialTransaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuthorizedPersonId = c.Int(nullable: false),
                        BatchId = c.Int(),
                        GatewayEntityTypeId = c.Int(),
                        TransactionDateTime = c.DateTime(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransactionCode = c.String(maxLength: 50),
                        Summary = c.String(),
                        TransactionTypeValueId = c.Int(nullable: false),
                        CurrencyTypeValueId = c.Int(),
                        CreditCardTypeValueId = c.Int(),
                        SourceTypeValueId = c.Int(),
                        CheckMicrEncrypted = c.String(),
                        ScheduledTransactionId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.AuthorizedPersonId)
                .ForeignKey("dbo.FinancialBatch", t => t.BatchId)
                .ForeignKey("dbo.DefinedValue", t => t.CreditCardTypeValueId)
                .ForeignKey("dbo.DefinedValue", t => t.CurrencyTypeValueId)
                .ForeignKey("dbo.EntityType", t => t.GatewayEntityTypeId)
                .ForeignKey("dbo.FinancialScheduledTransaction", t => t.ScheduledTransactionId)
                .ForeignKey("dbo.DefinedValue", t => t.SourceTypeValueId)
                .ForeignKey("dbo.DefinedValue", t => t.TransactionTypeValueId)
                .Index(t => t.AuthorizedPersonId)
                .Index(t => t.BatchId)
                .Index(t => t.CreditCardTypeValueId)
                .Index(t => t.CurrencyTypeValueId)
                .Index(t => t.GatewayEntityTypeId)
                .Index(t => t.ScheduledTransactionId)
                .Index(t => t.SourceTypeValueId)
                .Index(t => t.TransactionTypeValueId);
            
            CreateIndex( "dbo.FinancialTransaction", "Guid", true );
            
            CreateTable(
                "dbo.FinancialTransactionImage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(nullable: false),
                        BinaryFileId = c.Int(nullable: false),
                        TransactionImageTypeValueId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId)
                .ForeignKey("dbo.FinancialTransaction", t => t.TransactionId)
                .ForeignKey("dbo.DefinedValue", t => t.TransactionImageTypeValueId)
                .Index(t => t.BinaryFileId)
                .Index(t => t.TransactionId)
                .Index(t => t.TransactionImageTypeValueId);
            
            CreateIndex( "dbo.FinancialTransactionImage", "Guid", true );
            
            CreateTable(
                "dbo.FinancialTransactionRefund",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        RefundReasonValueId = c.Int(),
                        RefundReasonSummary = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedValue", t => t.RefundReasonValueId)
                .ForeignKey("dbo.FinancialTransaction", t => t.Id, cascadeDelete: true)
                .Index(t => t.RefundReasonValueId)
                .Index(t => t.Id);
            
            CreateIndex( "dbo.FinancialTransactionRefund", "Guid", true );
            
            CreateTable(
                "dbo.FinancialScheduledTransaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuthorizedPersonId = c.Int(nullable: false),
                        TransactionFrequencyValueId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false, storeType: "date"),
                        EndDate = c.DateTime(storeType: "date"),
                        NumberOfPayments = c.Int(),
                        NextPaymentDate = c.DateTime(storeType: "date"),
                        LastStatusUpdateDateTime = c.DateTime(storeType: "date"),
                        IsActive = c.Boolean(nullable: false),
                        GatewayEntityTypeId = c.Int(),
                        TransactionCode = c.String(maxLength: 50),
                        GatewayScheduleId = c.String(),
                        CardReminderDate = c.DateTime(storeType: "date"),
                        LastRemindedDate = c.DateTime(storeType: "date"),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.AuthorizedPersonId)
                .ForeignKey("dbo.EntityType", t => t.GatewayEntityTypeId)
                .ForeignKey("dbo.DefinedValue", t => t.TransactionFrequencyValueId)
                .Index(t => t.AuthorizedPersonId)
                .Index(t => t.GatewayEntityTypeId)
                .Index(t => t.TransactionFrequencyValueId);
            
            CreateIndex( "dbo.FinancialScheduledTransaction", "Guid", true );
            
            CreateTable(
                "dbo.FinancialScheduledTransactionDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ScheduledTransactionId = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Summary = c.String(maxLength: 500),
                        EntityTypeId = c.Int(),
                        EntityId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialAccount", t => t.AccountId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.FinancialScheduledTransaction", t => t.ScheduledTransactionId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.ScheduledTransactionId);
            
            CreateIndex( "dbo.FinancialScheduledTransactionDetail", "Guid", true );
            
            CreateTable(
                "dbo.FinancialTransactionDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Summary = c.String(maxLength: 500),
                        EntityTypeId = c.Int(),
                        EntityId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialAccount", t => t.AccountId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.FinancialTransaction", t => t.TransactionId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.TransactionId);
            
            CreateIndex( "dbo.FinancialTransactionDetail", "Guid", true );
            
            CreateTable(
                "dbo.FinancialPersonBankAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        AccountNumberSecured = c.String(nullable: false, maxLength: 40),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.FinancialPersonBankAccount", "AccountNumberSecured", true );
            
            CreateIndex( "dbo.FinancialPersonBankAccount", "Guid", true );
            
            CreateTable(
                "dbo.FinancialPersonSavedAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        FinancialTransactionId = c.Int(nullable: false),
                        ReferenceNumber = c.String(),
                        Name = c.String(nullable: false, maxLength: 50),
                        MaskedAccountNumber = c.String(maxLength: 100),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialTransaction", t => t.FinancialTransactionId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.FinancialTransactionId)
                .Index(t => t.PersonId);
            
            CreateIndex( "dbo.FinancialPersonSavedAccount", "Guid", true );
            
            CreateTable(
                "dbo.FinancialPledge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(),
                        AccountId = c.Int(),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PledgeFrequencyValueId = c.Int(),
                        StartDate = c.DateTime(nullable: false, storeType: "date"),
                        EndDate = c.DateTime(nullable: false, storeType: "date"),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialAccount", t => t.AccountId)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .ForeignKey("dbo.DefinedValue", t => t.PledgeFrequencyValueId)
                .Index(t => t.AccountId)
                .Index(t => t.PersonId)
                .Index(t => t.PledgeFrequencyValueId);
            
            CreateIndex( "dbo.FinancialPledge", "Guid", true );
            
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
                .ForeignKey("dbo.Person", t => t.ApprovedByPersonId)
                .ForeignKey("dbo.Block", t => t.BlockId, cascadeDelete: true)
                .Index(t => t.ApprovedByPersonId)
                .Index(t => t.BlockId);
            
            CreateIndex( "dbo.HtmlContent", "Guid", true );
            
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
                        StartDate = c.DateTime(nullable: false, storeType: "date"),
                        EndDate = c.DateTime(nullable: false, storeType: "date"),
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
                .ForeignKey("dbo.DefinedValue", t => t.AudienceTypeValueId, cascadeDelete: true)
                .ForeignKey("dbo.MarketingCampaign", t => t.MarketingCampaignId, cascadeDelete: true)
                .Index(t => t.AudienceTypeValueId)
                .Index(t => t.MarketingCampaignId);
            
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
                .ForeignKey("dbo.Campus", t => t.CampusId, cascadeDelete: true)
                .ForeignKey("dbo.MarketingCampaign", t => t.MarketingCampaignId, cascadeDelete: true)
                .Index(t => t.CampusId)
                .Index(t => t.MarketingCampaignId);
            
            CreateIndex( "dbo.MarketingCampaignCampus", "Guid", true );
            
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
                        CollectionFrequencyValueId = c.Int(),
                        LastCollectedDateTime = c.DateTime(),
                        Source = c.String(maxLength: 100),
                        SourceSQL = c.String(),
                        Order = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedValue", t => t.CollectionFrequencyValueId)
                .Index(t => t.CollectionFrequencyValueId);
            
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
                "dbo.Note",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        NoteTypeId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        SourceTypeValueId = c.Int(),
                        Caption = c.String(maxLength: 200),
                        CreationDateTime = c.DateTime(nullable: false),
                        IsAlert = c.Boolean(),
                        Text = c.String(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NoteType", t => t.NoteTypeId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.SourceTypeValueId)
                .Index(t => t.NoteTypeId)
                .Index(t => t.SourceTypeValueId);
            
            CreateIndex( "dbo.Note", "Guid", true );
            
            CreateTable(
                "dbo.NoteType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        SourcesTypeId = c.Int(),
                        EntityTypeQualifierColumn = c.String(maxLength: 50),
                        EntityTypeQualifierValue = c.String(maxLength: 200),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.DefinedType", t => t.SourcesTypeId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.SourcesTypeId);
            
            CreateIndex( "dbo.NoteType", "Guid", true );
            
            CreateTable(
                "dbo.PersonMerged",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PreviousPersonId = c.Int(nullable: false),
                        PreviousPersonGuid = c.Guid(nullable: false),
                        NewPersonId = c.Int(nullable: false),
                        NewPersonGuid = c.Guid(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex( "dbo.PersonMerged", "PreviousPersonId", true );
            
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
                .ForeignKey("dbo.Person", t => t.TargetPersonId)
                .ForeignKey("dbo.Person", t => t.ViewerPersonId)
                .Index(t => t.TargetPersonId)
                .Index(t => t.ViewerPersonId);
            
            CreateIndex( "dbo.PersonViewed", "Guid", true );
            
            CreateTable(
                "dbo.PrayerRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        Email = c.String(maxLength: 254),
                        RequestedByPersonId = c.Int(),
                        CategoryId = c.Int(),
                        Text = c.String(nullable: false),
                        Answer = c.String(),
                        EnteredDate = c.DateTime(nullable: false, storeType: "date"),
                        ExpirationDate = c.DateTime(storeType: "date"),
                        GroupId = c.Int(),
                        AllowComments = c.Boolean(),
                        IsUrgent = c.Boolean(),
                        IsPublic = c.Boolean(),
                        IsActive = c.Boolean(),
                        IsApproved = c.Boolean(),
                        FlagCount = c.Int(),
                        PrayerCount = c.Int(),
                        ApprovedByPersonId = c.Int(),
                        ApprovedOnDate = c.DateTime(storeType: "date"),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.ApprovedByPersonId)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.RequestedByPersonId)
                .Index(t => t.ApprovedByPersonId)
                .Index(t => t.CategoryId)
                .Index(t => t.GroupId)
                .Index(t => t.RequestedByPersonId);
            
            CreateIndex( "dbo.PrayerRequest", "Guid", true );
            
            CreateTable(
                "dbo.ReportField",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReportId = c.Int(nullable: false),
                        ReportFieldType = c.Int(nullable: false),
                        ShowInGrid = c.Boolean(nullable: false),
                        DataSelectComponentEntityTypeId = c.Int(),
                        Selection = c.String(),
                        Order = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityType", t => t.DataSelectComponentEntityTypeId)
                .ForeignKey("dbo.Report", t => t.ReportId, cascadeDelete: true)
                .Index(t => t.DataSelectComponentEntityTypeId)
                .Index(t => t.ReportId);
            
            CreateIndex( "dbo.ReportField", "Guid", true );
            
            CreateTable(
                "dbo.Report",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        CategoryId = c.Int(),
                        EntityTypeId = c.Int(),
                        DataViewId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.DataView", t => t.DataViewId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .Index(t => t.CategoryId)
                .Index(t => t.DataViewId)
                .Index(t => t.EntityTypeId);
            
            CreateIndex( "dbo.Report", "Guid", true );
            
            CreateTable(
                "dbo.ServiceJob",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        IsActive = c.Boolean(),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Assembly = c.String(maxLength: 260),
                        Class = c.String(nullable: false, maxLength: 100),
                        CronExpression = c.String(nullable: false, maxLength: 120),
                        LastSuccessfulRunDateTime = c.DateTime(),
                        LastRunDateTime = c.DateTime(),
                        LastRunDurationSeconds = c.Int(),
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
                "dbo.ServiceLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LogDateTime = c.DateTime(),
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
                "dbo.TaggedItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsSystem = c.Boolean(nullable: false),
                        TagId = c.Int(nullable: false),
                        EntityGuid = c.Guid(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tag", t => t.TagId, cascadeDelete: true)
                .Index(t => t.TagId);
            
            CreateIndex( "dbo.TaggedItem", "Guid", true );
            
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
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.Person", t => t.OwnerId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.OwnerId);
            
            CreateIndex( "dbo.Tag", "Guid", true );
            
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
                .ForeignKey("dbo.WorkflowActionType", t => t.ActionTypeId)
                .ForeignKey("dbo.WorkflowActivity", t => t.ActivityId, cascadeDelete: true)
                .Index(t => t.ActionTypeId)
                .Index(t => t.ActivityId);
            
            CreateIndex( "dbo.WorkflowAction", "Guid", true );
            
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
                        WorkTerm = c.String(nullable: false, maxLength: 100),
                        ProcessingIntervalSeconds = c.Int(),
                        IsPersisted = c.Boolean(nullable: false),
                        LoggingLevel = c.Int(nullable: false),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .Index(t => t.CategoryId);
            
            CreateIndex( "dbo.WorkflowType", "Guid", true );
            
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
                .ForeignKey("dbo.WorkflowActivityType", t => t.ActivityTypeId)
                .ForeignKey("dbo.Workflow", t => t.WorkflowId, cascadeDelete: true)
                .Index(t => t.ActivityTypeId)
                .Index(t => t.WorkflowId);
            
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
                        WorkflowName = c.String(maxLength: 100),
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
            
            CreateTable(
                "dbo.GroupLocationSchedule",
                c => new
                    {
                        GroupLocationId = c.Int(nullable: false),
                        ScheduleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupLocationId, t.ScheduleId })
                .ForeignKey("dbo.GroupLocation", t => t.GroupLocationId, cascadeDelete: true)
                .ForeignKey("dbo.Schedule", t => t.ScheduleId, cascadeDelete: true)
                .Index(t => t.GroupLocationId)
                .Index(t => t.ScheduleId);
            
            CreateTable(
                "dbo.DeviceLocation",
                c => new
                    {
                        DeviceId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DeviceId, t.LocationId })
                .ForeignKey("dbo.Device", t => t.DeviceId, cascadeDelete: true)
                .ForeignKey("dbo.Location", t => t.LocationId, cascadeDelete: true)
                .Index(t => t.DeviceId)
                .Index(t => t.LocationId);
            
            CreateTable(
                "dbo.AttributeCategory",
                c => new
                    {
                        AttributeId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AttributeId, t.CategoryId })
                .ForeignKey("dbo.Attribute", t => t.AttributeId, cascadeDelete: true)
                .ForeignKey("dbo.Category", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.AttributeId)
                .Index(t => t.CategoryId);


            /* Manually Added Schema Changes */

            /* Computed Columns */
            Sql( @"
    ALTER TABLE PERSON ADD [FirstName] AS (isnull(nullif([NickName],''),[GivenName]));
	ALTER TABLE PERSON ADD [FullName]  AS ((isnull(isnull(nullif([NickName],''),[GivenName]),'')+' ')+isnull([LastName],''));
	ALTER TABLE PERSON ADD [FullNameLastFirst]  AS ((isnull([LastName],'')+', ')+isnull(isnull(nullif([NickName],''),[GivenName]),''));
" );

            /* Manually added Indexes*/
            Sql( @"
CREATE UNIQUE INDEX IX_FieldTypeClass ON dbo.FieldType ([Class], [Assembly]);
CREATE INDEX FirstLastName ON dbo.Person (IsDeceased, FirstName, LastName);
CREATE INDEX LastFirstName ON dbo.Person (IsDeceased, LastName, FirstName);
CREATE UNIQUE INDEX IX_UserName ON dbo.UserLogin (UserName);
CREATE UNIQUE INDEX IX_GroupId_PersonId_GroupRoleId ON dbo.GroupMember (GroupId, PersonId, GroupRoleId);
CREATE INDEX EntityAttribute ON dbo.AttributeValue (EntityId, AttributeId);
CREATE INDEX [IX_Name] ON [dbo].[Schedule]([Name] ASC);

" );

            /* Database Functions and Stored Procedures */
            Sql( @"
create function [dbo].[ufn_csv_to_table] ( @input varchar(max) )
returns @outputTable table ( [id] int )
as
begin
    declare @numericstring varchar(10)

    while LEN(@input) > 0
    begin
        set @numericString      = LEFT(@input, 
                                ISNULL(NULLIF(CHARINDEX(',', @input) - 1, -1),
                                LEN(@input)))
        set @input = SUBSTRING(@input,
                                     ISNULL(NULLIF(CHARINDEX(',', @input), 0),
                                     LEN(@input)) + 1, LEN(@input))

        insert into @OutputTable ( [id] )
        values ( CAST(@numericString as int) )
    end
    
    return
end
" );

            Sql( @"
create function [dbo].[ufn_person_group_to_person_names] 
( 
@personId int, -- NULL means generate person names from Group Members. NOT-NULL means just get FullName from Person
@groupId int
)
returns @personNamesTable table ( PersonNames varchar(max))
as
begin
    declare @personNames varchar(max); 
    declare @adultLastNameCount int;
    declare @groupFirstNames varchar(max) = '';
    declare @groupLastName varchar(max);
    declare @groupAdultFullNames varchar(max) = '';
    declare @groupNonAdultFullNames varchar(max) = '';
    declare @groupMemberTable table (LastName varchar(max), FirstName varchar(max), FullName varchar(max), GroupRoleGuid uniqueidentifier );
    declare @GROUPTYPEROLE_FAMILY_MEMBER_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    if (@personId is not null) 
    begin
        -- just getting the Person Names portion of the address for an individual person
        select @personNames = [FullName] from [Person] where [Id] = @personId;
    end
    else
    begin
        -- populate a table variable with the data we'll need for the different cases
        insert into @groupMemberTable 
        select 
            [p].[LastName], [p].[FirstName], [p].[FullName], [gr].[Guid]
        from 
            [GroupMember] [gm] 
        join 
            [Person] [p] 
        on 
            [p].[Id] = [gm].[PersonId] 
        join
            [GroupTypeRole] [gr]
        on 
            [gm].[GroupRoleId] = [gr].[Id]
        where 
            [GroupId] = @groupId;
        
        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        select 
            @adultLastNameCount = count(distinct [LastName])
            ,@groupLastName = max([LastName])
        from 
            @groupMemberTable
        where
            [GroupRoleGuid] = @GROUPTYPEROLE_FAMILY_MEMBER_ADULT;  

        if @adultLastNameCount > 0 
        begin
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            select 
                @groupFirstNames = @groupFirstNames + [FirstName] + ' & '
                ,@groupAdultFullNames = @groupAdultFullNames + [FullName] + ' & '
            from      
                @groupMemberTable
            where
                [GroupRoleGuid] = @GROUPTYPEROLE_FAMILY_MEMBER_ADULT;

            -- cleanup the trailing ' &'s
            if len(@groupFirstNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupFirstNames = SUBSTRING(@groupFirstNames, 0, len(@groupFirstNames) - 1)
            end 

            if len(@groupAdultFullNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupAdultFullNames = SUBSTRING(@groupAdultFullNames, 0, len(@groupAdultFullNames) - 1)  
            end
        end             

        if @adultLastNameCount = 0        
        begin
            -- get the NonAdultFullNames for use in the case of families without adults 
            select 
                @groupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
            from 
                @groupMemberTable
            order by [FullName]

            if len(@groupNonAdultFullNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupNonAdultFullNames = SUBSTRING(@groupNonAdultFullNames, 0, len(@groupNonAdultFullNames) - 1)  
            end
        end

        if (@adultLastNameCount = 1)
        begin
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
            set @personNames = @groupFirstNames + ' ' + @groupLastName;
        end
        else if (@adultLastNameCount = 0)
        begin
             -- no adults in family, list all members of the family in 'Fullname & FullName & ...' format
            set @personNames = @groupNonAdultFullNames;
        end
        else
        begin
            -- multiple adult lastnames
            set @personNames = @groupAdultFullNames;
        end 
    end

    insert into @personNamesTable ( [PersonNames] ) values ( @personNames);

  return
end
" );

            Sql( @"
CREATE PROCEDURE [BinaryFile_sp_getByID]
    @Id int,
    @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    /* NOTE!: Column Order cannot be changed without changing GetFile.ashx.cs due to CommandBehavior.SequentialAccess */
    SELECT 
        bf.Id,
        bf.[FileName], 
        bf.MimeType,
        bf.Url,
        /* if the BinaryFile as StorageEntityTypeId set, use that. Otherwise use the default StorageEntityTypeId from BinaryFileType  */
        COALESCE (bfse.Name,bftse.Name ) as StorageTypeName,
        bfd.Content
    FROM BinaryFile bf 
    LEFT JOIN BinaryFileData bfd
        ON bf.Id = bfd.Id
    LEFT JOIN EntityType bfse
        ON bf.StorageEntityTypeId = bfse.Id
    LEFT JOIN BinaryFileType bft
        on bf.BinaryFileTypeId = bft.Id
    LEFT JOIN EntityType bftse
        ON bft.StorageEntityTypeId = bftse.Id
    WHERE 
        (@Id > 0 and bf.Id = @Id)
        or
        (bf.[Guid] = @Guid)
END
" );

            Sql( @"

CREATE PROC [dbo].[Person_sp_Merge]
@OldId int, 
@NewId int, 
@DeleteOldPerson bit

AS

DECLARE @OldGuid uniqueidentifier
DECLARE @NewGuid uniqueidentifier

SET @OldGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @OldId )
SET @NewGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @NewId )

IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
BEGIN

	DECLARE @PersonEntityTypeId INT
	SET @PersonEntityTypeId = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )

	DECLARE @PersonFieldTypeId INT
	SET @PersonFieldTypeId = ( SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.PersonFieldType' )


	--BEGIN TRANSACTION


	-- Attribute Value
	-----------------------------------------------------------------------------------------------
	-- Update Attribute Values associated with person 
	-- The new user's attribute value will only get updated if the old user has a value, and the 
	-- new user does not (determining the correct value will eventually be decided by user in a UI)
	UPDATE AVO
		SET [EntityId] = @NewId
	FROM [Attribute] A
	INNER JOIN [AttributeValue] AVO
		ON AVO.[EntityId] = @OldId
		AND AVO.[AttributeId] = A.[Id]
	LEFT OUTER JOIN [AttributeValue] AVN
		ON AVO.[EntityId] = @NewId
		AND AVN.[AttributeId] = A.[Id]
	WHERE A.[EntityTypeId] = @PersonEntityTypeId
	AND AVN.[Id] IS NULL

	-- Delete any attribute values that were not updated (due to new person already having existing 
	-- value)
	DELETE AV
	FROM [Attribute] A
	INNER JOIN [AttributeValue] AV
		ON AV.[EntityId] = @OldId
		AND AV.[AttributeId] = A.[Id]
	WHERE A.[EntityTypeId] = @PersonEntityTypeId

	-- Update Attribute Values that have person as a value
	-- NOTE: BECAUSE VALUE IS A VARCHAR(MAX) COLUMN WE CANT ADD AN INDEX FOR ATTRIBUTEID AND
	-- VALUE.  THIS UPDATE COULD POTENTIALLY BE A BOTTLE-NECK FOR MERGES
	UPDATE AV
		SET [Value] = CAST( @NewGuid AS VARCHAR(64) )
	FROM [Attribute] A
	INNER JOIN [AttributeValue] AV
		ON AV.[AttributeId] = A.[Id]
		AND AV.[Value] = CAST( @OldGuid AS VARCHAR(64) )
	WHERE A.[FieldTypeId] = @PersonFieldTypeId


	-- Authorization
	-----------------------------------------------------------------------------------------------
	-- Update any authorizations associated to old person that do not already have a matching 
	-- authorization for the new person
	UPDATE AO
		SET [PersonId] = @NewId
	FROM [Auth] AO
	LEFT OUTER JOIN [Auth] AN
		ON AN.[PersonId] = @NewId
		AND AN.[EntityTypeId] = AO.[EntityTypeId]
		AND AN.[EntityId] = AO.[EntityId]
		AND AN.[Action] = AO.[Action]
		AND AN.[AllowOrDeny] = AO.[AllowOrDeny]
		AND AN.[SpecialRole] = AO.[SpecialRole]
	WHERE AO.[PersonId] = @OldId
	AND AN.[Id] IS NULL

	-- Delete any authorizations not updated to new person
	DELETE [Auth]
	WHERE [PersonId] = @OldId


	-- Category
	-----------------------------------------------------------------------------------------------
	-- Currently UI does not allow categorizing people, but if it does in the future, would need 
	-- to add script to handle merge


	-- Communication Recipient
	-----------------------------------------------------------------------------------------------
	-- Update any communication recipients associated to old person to the new person where the new
	-- person does not already have the recipient record
	UPDATE CRO
		SET [PersonId] = @NewId
	FROM [CommunicationRecipient] CRO
	LEFT OUTER JOIN [CommunicationRecipient] CRN
		ON CRN.[CommunicationId] = CRO.[CommunicationId]
		AND CRN.[PersonId] = @NewId
	WHERE CRO.[PersonId] = @OldId
	AND CRN.[Id] IS NULL

	-- Delete any remaining recipents that were not updated
	DELETE [CommunicationRecipient]
	WHERE [PersonId] = @OldId

	-- Group Member
	-----------------------------------------------------------------------------------------------
	-- Update any group members associated to old person to the new person where the new is not 
	-- already in the group with the same role
	UPDATE GMO
		SET [PersonId] = @NewId
	FROM [GroupMember] GMO
	LEFT OUTER JOIN [GroupMember] GMN
		ON GMN.[GroupId] = GMO.[GroupId]
		AND GMN.[PersonId] = @NewId
		AND GMN.[GroupRoleId] = GMO.[GroupRoleId] -- If person can be in group twice with diff role
	WHERE GMO.[PersonId] = @OldId
	AND GMN.[Id] IS NULL

	-- Delete any group members not updated (already existed with new id)
	DELETE [GroupMember]
	WHERE [PersonId] = @OldId


	-- Note
	-----------------------------------------------------------------------------------------------
	-- Update any note that is associated to the old person to be associated to the new person
	UPDATE N
		SET [EntityId] = @NewId
	FROM [NoteType] NT
	INNER JOIN [Note] N
		ON N.[NoteTypeId] = NT.[Id]
		AND N.[EntityId] = @OldId
	WHERE NT.[EntityTypeId] = @PersonEntityTypeId


	-- Phone Numbers
	-----------------------------------------------------------------------------------------------
	-- Update any phone numbers associated to the old person that do not already exist for the new
	-- person
	UPDATE PNO
		SET [PersonId] = @NewId
	FROM [PhoneNumber] PNO
	INNER JOIN [PhoneNumber] PNN
		ON PNN.[PersonId] = @NewId
		AND PNN.[Number] = PNO.[Number]
		AND PNN.[Extension] = PNO.[Extension]
		AND PNN.[NumberTypeValueId] = PNO.[NumberTypeValueId]
	WHERE PNO.[PersonId] = @OldId
	AND PNN.[Id] IS NULL

	-- Delete any numbers not updated (new person already had same number)
	DELETE [PhoneNumber]
	WHERE [PersonId] = @OldId


	-- Tags
	-----------------------------------------------------------------------------------------------
	-- Update any tags associated to the old person to be associated to the new person as long as 
	-- same tag does not already exist for new person
	UPDATE TIO
		SET [EntityGuid] = @NewGuid
	FROM [Tag] T
	INNER JOIN [TaggedItem] TIO
		ON TIO.[TagId] = T.[Id]
		AND TIO.[EntityGuid] = @OldGuid
	LEFT OUTER JOIN [TaggedItem] TIN
		ON TIN.[TagId] = T.[Id]
		AND TIN.[EntityGuid] = @NewGuid
	WHERE T.[EntityTypeId] = @PersonEntityTypeId
	AND TIN.[Id] IS NULL

	-- Delete any tagged items still associated with old person (new person had same tag)
	DELETE TIO
	FROM [Tag] T
	INNER JOIN [TaggedItem] TIO
		ON TIO.[TagId] = T.[Id]
		AND TIO.[EntityGuid] = @OldGuid
	WHERE T.[EntityTypeId] = @PersonEntityTypeId

	-- If old person and new person have tags with the same name for the same entity type,
	-- update the old person's tagged items to use the new person's tag
	UPDATE TIO
		SET [TagId] = TIN.[Id]
	FROM [Tag] T
	INNER JOIN [Tag] TN
		ON TN.[EntityTypeId] = T.[EntityTypeId]
		AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		AND TN.[Name] = T.[Name]
		AND TN.[OwnerId] = @NewId
	INNER JOIN [TaggedItem] TIO
		ON TIO.[TagId] = T.[Id]
	LEFT OUTER JOIN [TaggedItem] TIN
		ON TIN.[TagId] = TN.[Id]
	WHERE T.[OwnerId] = @OldId
	AND TIN.[Id] IS NULL

	-- Delete any of the old person's tags that have the same name and are associated to same 
	-- entity type as a tag used bo the new person
	DELETE T
	FROM [Tag] T
	INNER JOIN [Tag] TN
		ON TN.[EntityTypeId] = T.[EntityTypeId]
		AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		AND TN.[Name] = T.[Name]
		AND TN.[OwnerId] = @NewId
	WHERE T.[OwnerId] = @OldId


	-- Person Merged
	-----------------------------------------------------------------------------------------------
	-- Add a record to indicate that person was merged with old and new id's and guid's
	INSERT INTO PersonMerged (
		 [Guid]
		,[PreviousPersonId]
		,[PreviousPersonGuid]
		,[NewPersonId]
		,[NewPersonGuid]
	)
	VALUES (
		 NEWID()
		,@OldId
		,@OldGuid
		,@NewId
		,@NewGuid
	)


	-- Remaining Tables
	-----------------------------------------------------------------------------------------------
	-- Update any column on any table that has a foreign key relationship to the Person table's Id
	-- column  

	DECLARE @Sql varchar(max)

	DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
	SELECT 
		' UPDATE ' + tso.name +
		' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
		' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
	FROM sys.foreign_key_columns kc
	INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
	INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
	INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
	INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
	INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
	WHERE so.name = 'Person'
	AND rac.name = 'Id'
	AND tso.name NOT IN (
		 'Auth'
		,'CommunicationRecipient'
		,'GroupMember'
		,'PhoneNumber'
	)

	OPEN ForeignKeyCursor

	FETCH NEXT
	FROM ForeignKeyCursor
	INTO @Sql

	WHILE (@@FETCH_STATUS <> -1)
	BEGIN

		IF (@@FETCH_STATUS = 0)
		BEGIN

			EXEC(@Sql)
			
		END
		
		FETCH NEXT
		FROM ForeignKeyCursor
		INTO @Sql

	END

	CLOSE ForeignKeyCursor
	DEALLOCATE ForeignKeyCursor


	-- Person
	-----------------------------------------------------------------------------------------------
	-- Optionally delete the old person record.  By this time it should not have any relationships 
	-- with other tables (if Rock is being synced with other data source, the sync may handle the
	-- delete)
	
	IF @DeleteOldPerson = 1 
	BEGIN
		DELETE Person
		WHERE [Id] = @OldId
	END
	
	--COMMIT TRANSACTION


END
" );

            Sql( @"
create procedure [sp_get_contribution_person_group_address]
	@startDate datetime,
    @endDate datetime,
    @accountIds varchar(max), -- comma delimited list of integers. NULL means all
    @personId int, -- NULL means all persons
    @orderByZipCode bit
as
begin
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	set nocount on;

    ;with tranListCTE
    as
    (
        select  
            [AuthorizedPersonId] 
         from 
            [FinancialTransaction] [ft]
         inner join [FinancialTransactionDetail] [ftd]
         on [ft].[Id] = [ftd].[TransactionId]
         where 
            ([TransactionDateTime] >= @startDate and [TransactionDateTime] < @endDate)
         and 
            (
                (@accountIds is null)
                or
                (ftd.AccountId in (select * from ufn_csv_to_table(@accountIds)))
            )
    )

    select 
        [pg].[PersonId],
        [pg].[GroupId],
        [pn].[PersonNames] [AddressPersonNames],
        [l].[Street1],
        [l].[Street2],
        [l].[City],
        [l].[State],
        [l].[Zip],
        @startDate [StartDate],
        @endDate [EndDate],
        null [CustomMessage1],
        null [CustomMessage2]
    from (
        -- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
        -- These are Persons that give as part of a Group.  For example, Husband and Wife
        select distinct
            null [PersonId], 
            [g].[Id] [GroupId]
        from 
            [Person] [p]
        inner join 
            [Group] [g]
        on 
            [p].[GivingGroupId] = [g].[Id]
        where [p].[Id] in (select * from tranListCTE)
        union
        -- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
        -- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
        -- to determine which address(es) the statements need to be mailed to 
        select  
            [p].[Id] [PersonId],
            [g].[Id] [GroupId]
        from
            [Person] [p]
        join 
            [GroupMember] [gm]
        on 
            [gm].[PersonId] = [p].[Id]
        join 
            [Group] [g]
        on 
            [gm].[GroupId] = [g].[Id]
        where
            [p].[GivingGroupId] is null
        and
            [g].[GroupTypeId] = (select Id from GroupType where Guid = '790E3215-3B10-442B-AF69-616C0DCB998E' /* GROUPTYPE_FAMILY */)
        and [p].[Id] in (select * from tranListCTE)
    ) [pg]
    cross apply [ufn_person_group_to_person_names]([pg].[PersonId], [pg].[GroupId]) [pn]
    join 
        [GroupLocation] [gl] 
    on 
        [gl].[GroupId] = [pg].[GroupId]
    join
        [Location] [l]
    on 
        [l].[Id] = [gl].[LocationId]
    where 
        [gl].[IsMailingLocation] = 1
    and
        [gl].[GroupLocationTypeValueId] = (select Id from DefinedValue where Guid = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC' /* LOCATION_TYPE_HOME */)
    and
        (
            (@personId is null) 
        or 
            ([pg].[PersonId] = @personId)
        )
    order by
    case when @orderByZipCode = 1 then Zip end
end
" );

            /* Populate with Data */
            Sql( RockMigration.MigrationSQL._201311251734059_CreateDatabase_PopulateData );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
