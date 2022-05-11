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
    public partial class Rollup_20220426 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixedMailgunUnauthorizedException();
            UpdateFormBuilderPageRoute();
            UpdateDisplayUrlDisplayCasing();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// SK: Fixed Mailgun Unauthorized exception when sending email
        /// </summary>
        private void FixedMailgunUnauthorizedException()
        {
            Sql( @"
                DECLARE @MailgunHttpEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.MailgunHttp' )
                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Key]='Resource' AND [EntityTypeId] = @MailgunHttpEntityTypeId)
                IF EXISTS( SELECT [Id] FROM [Attribute] WHERE [Key]='Resource' AND [EntityTypeId] = @MailgunHttpEntityTypeId )
                BEGIN
	                UPDATE
		                [Attribute]
	                SET [DefaultValue] = '{domain}/messages'
	                WHERE
		                [Key]='Resource' AND [EntityTypeId] = @MailgunHttpEntityTypeId;

	                UPDATE
		                [AttributeValue]
	                SET [Value] = '{domain}/messages'
	                WHERE
		                [AttributeId]=@AttributeId AND [Value] = '{domian}/messages';
                END" );
        }

        /// <summary>
        /// ED: Update FormBuilder PageRoute
        /// </summary>
        private void UpdateFormBuilderPageRoute()
        {
            Sql( @"
                UPDATE [PageRoute]
                SET [Route] = 'workflow/form-builder'
                Where [Route] = 'admin/general/form-builder'" );
        }

        /// <summary>
        /// SC:  Update "Url" Casing on Display Titles
        /// </summary>
        private void UpdateDisplayUrlDisplayCasing()
        {
            Sql( @"
                UPDATE [FieldType] SET [Name] = 'URL Link' WHERE [Guid] = 'C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2';
                UPDATE [FieldType] SET [Name] = 'Audio URL' WHERE [Guid] = '3B2D8714-421C-4CB8-A892-58B83521EF8A';
                UPDATE [FieldType] SET [Name] = 'Video URL' WHERE [Guid] = 'E6FD57F3-1704-4E96-91A7-3D3E85346393';" );
        }
    }
}
