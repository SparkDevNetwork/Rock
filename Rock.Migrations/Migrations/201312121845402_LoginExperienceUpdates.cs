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
    public partial class LoginExperienceUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"
    UPDATE AttributeValue
  SET [Value] = '<div class=''alert alert-info''>Enter your email address below and we''ll send your account information to you right away.</div>'
  WHERE [Guid] = '362598D2-D0EC-4995-BA50-CB52B3EC6E1C'

  UPDATE AttributeValue
  SET [Value] = 'Sorry, we could not find an account for the email address you entered.'
  WHERE [Guid] = 'FE2E6205-D888-4A4D-8B3C-38F2E0D8BA9B'

  UPDATE AttributeValue
  SET [Value] = 'Your user name has been sent with instructions on how to change your password if needed.'
  WHERE [Guid] = '88388EE1-20CD-446C-AA30-1FB8A6BC8B3C'

  UPDATE Page
  SET LayoutId = 4
  WHERE Id = 56
");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
