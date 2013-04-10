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
    public partial class DateTimeNamingSweep : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.BinaryFile", "LastModifiedDateTime", c => c.DateTime());
            AddColumn("dbo.UserLogin", "LastActivityDateTime", c => c.DateTime());
            AddColumn("dbo.UserLogin", "LastLoginDateTime", c => c.DateTime());
            AddColumn("dbo.UserLogin", "LastPasswordChangedDateTime", c => c.DateTime());
            AddColumn("dbo.UserLogin", "CreationDateTime", c => c.DateTime());
            AddColumn("dbo.UserLogin", "LastLockedOutDateTime", c => c.DateTime());
            AddColumn("dbo.UserLogin", "FailedPasswordAttemptWindowStartDateTime", c => c.DateTime());
            AddColumn("dbo.ExceptionLog", "ExceptionDateTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.Metric", "LastCollectedDateTime", c => c.DateTime());
            AddColumn("dbo.Note", "CreationDateTime", c => c.DateTime(nullable: false));

            Sql( "update BinaryFile set LastModifiedDateTime=LastModifiedTime" );
            DropColumn( "dbo.BinaryFile", "LastModifiedTime" );

            Sql( "update UserLogin set LastActivityDateTime=LastActivityDate" );
            DropColumn( "dbo.UserLogin", "LastActivityDate" );

            Sql( "update UserLogin set LastLoginDateTime=LastLoginDate" );
            DropColumn( "dbo.UserLogin", "LastLoginDate" );

            Sql( "update UserLogin set LastPasswordChangedDateTime=LastPasswordChangedDate" );
            DropColumn( "dbo.UserLogin", "LastPasswordChangedDate" );

            Sql( "update UserLogin set CreationDateTime=CreationDate" );
            DropColumn( "dbo.UserLogin", "CreationDate" );

            Sql( "update UserLogin set LastLockedOutDateTime=LastLockedOutDate" );
            DropColumn( "dbo.UserLogin", "LastLockedOutDate" );

            Sql( "update UserLogin set FailedPasswordAttemptWindowStartDateTime=FailedPasswordAttemptWindowStart" );
            DropColumn( "dbo.UserLogin", "FailedPasswordAttemptWindowStart" );

            Sql( "update Metric set LastCollectedDateTime=LastCollected" );
            DropColumn( "dbo.Metric", "LastCollected" );

            Sql( "update Note set CreationDateTime=Date" );
            DropColumn( "dbo.Note", "Date" );

            Sql( "update ExceptionLog set ExceptionDateTime=ExceptionDate" );
            DropColumn("dbo.ExceptionLog", "ExceptionDate");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Note", "Date", c => c.DateTime(nullable: false));
            AddColumn("dbo.Metric", "LastCollected", c => c.DateTime());
            AddColumn("dbo.ExceptionLog", "ExceptionDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.UserLogin", "FailedPasswordAttemptWindowStart", c => c.DateTime());
            AddColumn("dbo.UserLogin", "LastLockedOutDate", c => c.DateTime());
            AddColumn("dbo.UserLogin", "CreationDate", c => c.DateTime());
            AddColumn("dbo.UserLogin", "LastPasswordChangedDate", c => c.DateTime());
            AddColumn("dbo.UserLogin", "LastLoginDate", c => c.DateTime());
            AddColumn("dbo.UserLogin", "LastActivityDate", c => c.DateTime());
            AddColumn("dbo.BinaryFile", "LastModifiedTime", c => c.DateTime());
            DropColumn("dbo.Note", "CreationDateTime");
            DropColumn("dbo.Metric", "LastCollectedDateTime");
            DropColumn("dbo.ExceptionLog", "ExceptionDateTime");
            DropColumn("dbo.UserLogin", "FailedPasswordAttemptWindowStartDateTime");
            DropColumn("dbo.UserLogin", "LastLockedOutDateTime");
            DropColumn("dbo.UserLogin", "CreationDateTime");
            DropColumn("dbo.UserLogin", "LastPasswordChangedDateTime");
            DropColumn("dbo.UserLogin", "LastLoginDateTime");
            DropColumn("dbo.UserLogin", "LastActivityDateTime");
            DropColumn("dbo.BinaryFile", "LastModifiedDateTime");
        }
    }
}
