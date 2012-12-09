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
    public partial class ServiceJobColumnNames : RockMigration_1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ServiceJob", "Assembly", c => c.String(maxLength: 260));
            AddColumn("dbo.ServiceJob", "LastSuccessfulRunDateTime", c => c.DateTime());
            AddColumn("dbo.ServiceJob", "LastRunDateTime", c => c.DateTime());
            AddColumn("dbo.ServiceJob", "LastRunDurationSeconds", c => c.Int());
            AlterColumn("dbo.ServiceJob", "Description", c => c.String());
            DropColumnMoveDataUp("dbo.ServiceJob", "Assemby", "Assembly");
            DropColumnMoveDataUp( "dbo.ServiceJob", "LastSuccessfulRun", "LastSuccessfulRunDateTime" );
            DropColumnMoveDataUp( "dbo.ServiceJob", "LastRunDate", "LastRunDateTime" );
            DropColumnMoveDataUp( "dbo.ServiceJob", "LastRunDuration", "LastRunDurationSeconds" );
        }

        /// <summary>
        /// Drops the column move data up.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        private void DropColumnMoveDataUp( string tableName, string oldColumn, string newColumn )
        {
            string updateSql = "UPDATE {0} set {2} = {1}";
            Sql( string.Format( updateSql, tableName, oldColumn, newColumn ) );

            DropColumn( tableName, oldColumn );
        }

        /// <summary>
        /// Drops the column move data down.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        private void DropColumnMoveDataDown( string tableName, string oldColumn, string newColumn )
        {
            string updateSql = "UPDATE {0} set {1} = {2}";
            Sql( string.Format( updateSql, tableName, oldColumn, newColumn ) );

            DropColumn( tableName, newColumn );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.ServiceJob", "LastRunDuration", c => c.Int());
            AddColumn("dbo.ServiceJob", "LastRunDate", c => c.DateTime());
            AddColumn("dbo.ServiceJob", "LastSuccessfulRun", c => c.DateTime());
            AddColumn("dbo.ServiceJob", "Assemby", c => c.String(maxLength: 100));
            AlterColumn("dbo.ServiceJob", "Description", c => c.String(maxLength: 500));
            DropColumnMoveDataDown( "dbo.ServiceJob", "Assemby", "Assembly" );
            DropColumnMoveDataDown( "dbo.ServiceJob", "LastSuccessfulRun", "LastSuccessfulRunDateTime" );
            DropColumnMoveDataDown( "dbo.ServiceJob", "LastRunDate", "LastRunDateTime" );
            DropColumnMoveDataDown( "dbo.ServiceJob", "LastRunDuration", "LastRunDurationSeconds" );
        }
    }
}
