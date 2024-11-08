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
    public partial class Rollup_20231116 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddSMSPasswordlessSMSUpdate();
            RemoveUnnecessaryTextFromPasswordlessLoginConfirmationCommunicationTemplate();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// GJ: Add SMS Passwordless SMS Update 
        /// </summary>
        private void AddSMSPasswordlessSMSUpdate()
        {
            Sql( @"
                UPDATE [SystemCommunication]
                SET [SMSMessage] = N'Your {{ ''Global'' | Attribute:''OrganizationName'' }} verification code is: {{ Code }}'
                WHERE [Guid] = 'A7AD9FD5-A343-4ADA-868D-A3528D650143'
                    AND [SMSMessage] = N'{{ Code }} is your {{ ''Global'' | Attribute:''OrganizationName'' }} verification code.'" );
        }

        /// <summary>
        /// JMH: Removes the unnecessary text from passwordless login confirmation communication template.
        /// </summary>
        private void RemoveUnnecessaryTextFromPasswordlessLoginConfirmationCommunicationTemplate()
        {
            var guid = SystemGuid.SystemCommunication.SECURITY_CONFIRM_LOGIN_PASSWORDLESS;

            Sql( $@"
-- Use a temporary newline representation in the search string so we can search with CRLF, CR, or LF.
DECLARE @NewLinePlaceholder AS nvarchar(50) = N'<<NewLine-D30626C4-9C9F-4898-BA41-FE3E975CDBD8>>';
DECLARE @SearchString AS nvarchar(max) = N'<p>If you have trouble with the button above please use the link below:</p>' + @NewLinePlaceholder +
N'<p>' + @NewLinePlaceholder +
N'<a href=""{{{{ Link }}}}"">{{{{ Link }}}}</a>' + @NewLinePlaceholder +
N'</p>';
-- Create the actual search strings with CRLF, CR, or LF in place of the newline placeholders.
DECLARE @SearchStringCrLf AS nvarchar(max) = REPLACE( @SearchString, @NewLinePlaceholder, char(13)+char(10) );
DECLARE @SearchStringCr AS nvarchar(max) = REPLACE( @SearchString, @NewLinePlaceholder, char(13) );
DECLARE @SearchStringLf AS nvarchar(max) = REPLACE( @SearchString, @NewLinePlaceholder, char(10) );
IF EXISTS ( SELECT * FROM [SystemCommunication] WHERE [Guid] = '{guid}' AND [Body] LIKE '%' + @SearchStringCrLf + '%' )
BEGIN
    UPDATE [SystemCommunication]
       SET [Body] = REPLACE( [Body], @SearchStringCrLf, '' )
     WHERE [Guid] = '{guid}'
END
ELSE IF EXISTS ( SELECT * FROM [SystemCommunication] WHERE [Guid] = '{guid}' AND [Body] LIKE '%' + @SearchStringCr + '%' )
BEGIN
    UPDATE [SystemCommunication]
       SET [Body] = REPLACE( [Body], @SearchStringCr, '' )
     WHERE [Guid] = '{guid}'
END
ELSE IF EXISTS ( SELECT * FROM [SystemCommunication] WHERE [Guid] = '{guid}' AND [Body] LIKE '%' + @SearchStringLf + '%' )
BEGIN
    UPDATE [SystemCommunication]
       SET [Body] = REPLACE( [Body], @SearchStringLf, '' )
     WHERE [Guid] = '{guid}'
END" );
}
    }
}
