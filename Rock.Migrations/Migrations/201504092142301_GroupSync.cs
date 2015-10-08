// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class GroupSync : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //
            // Database Changes
            //
            
            AddColumn("dbo.Group", "WelcomeSystemEmailId", c => c.Int());
            AddColumn("dbo.Group", "ExitSystemEmailId", c => c.Int());
            AddColumn("dbo.Group", "SyncDataViewId", c => c.Int());
            AddColumn("dbo.Group", "AddUserAccountsDuringSync", c => c.Boolean());
            CreateIndex("dbo.Group", "WelcomeSystemEmailId");
            CreateIndex("dbo.Group", "ExitSystemEmailId");
            CreateIndex("dbo.Group", "SyncDataViewId");
            AddForeignKey("dbo.Group", "ExitSystemEmailId", "dbo.SystemEmail", "Id");
            AddForeignKey("dbo.Group", "SyncDataViewId", "dbo.DataView", "Id");
            AddForeignKey("dbo.Group", "WelcomeSystemEmailId", "dbo.SystemEmail", "Id");

            //
            // Service Job
            //

            Sql( @"INSERT INTO [ServiceJob]
                       ([IsSystem]
                       ,[IsActive]
                       ,[Name]
                       ,[Description]
                       ,[Assembly]
                       ,[Class]
                       ,[CronExpression]
                       ,[Guid]
		               ,[NotificationStatus])
                 VALUES
                       (1
                       ,1
                       ,'Group Sync'
                       ,'Processes groups that are marked to be synced with a data view.'
                       ,''
                       ,'Rock.Jobs.GroupSync'
                       ,'0 0/20 * 1/1 * ? *'
                       ,'57B539BC-7C4D-25BB-4EEB-39DF0EF62EBC'
		               ,3)" );

            //
            // Create System Emails
            //

            Sql( @" DECLARE @GroupCategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'B31064D2-F2EF-43AA-8BEA-14DF257CBC59')

                    INSERT INTO [SystemEmail]
                               ([IsSystem]
                               ,[Title]
                               ,[From]
                               ,[To]
                               ,[Cc]
                               ,[Bcc]
                               ,[Subject]
                               ,[Body]
                               ,[Guid]
		                       ,[CategoryId])
                         VALUES
                               (1
                               ,'Sync Exit From Group'
                               ,''
                               ,''
                               ,''
                               ,''
                               ,'{{ ''Global'' | Attribute:''OrganizationAbbreviation'' }}: Removed From {{ Group.Name }}'
                               ,'{{ ''Global'' | Attribute:''EmailHeader'' }}

                    <p>
                        {{ Person.NickName }}, you have been removed from the {{ Group.GroupType.GroupTerm }}: <em>{{ Group.Name }}</em>.
                    </p>

                    {{ ''Global'' | Attribute:''EmailFooter'' }}'
                               ,'9AC62F1B-16E0-3886-4CE6-2B9290D6B135'
                               ,@GroupCategoryId)



                    INSERT INTO [SystemEmail]
                               ([IsSystem]
                               ,[Title]
                               ,[From]
                               ,[To]
                               ,[Cc]
                               ,[Bcc]
                               ,[Subject]
                               ,[Body]
                               ,[Guid]
		                       ,[CategoryId])
                         VALUES
                               (1
                               ,'Sync Welcome To Group'
                               ,''
                               ,''
                               ,''
                               ,''
                               ,'{{ ''Global'' | Attribute:''OrganizationAbbreviation'' }}: Added To {{ Group.Name }}'
                               ,'{{ ''Global'' | Attribute:''EmailHeader'' }}

                    <p>
                        {{ Person.NickName }}, you have been added to the {{ Group.GroupType.GroupTerm }}: <em>{{ Group.Name }}</em>.

                    {% if CreateLogin == true %}
                        {% if NewPassword == '''' %}
                            You can use your {{ ''Global'' | Attribute:''OrganizationName'' }} login to access this {{ Group.GroupType.GroupTerm }}. If
                            you have forgotten your password you can use the password reminder feature of our website. Your username is provided below:
                            </p>
        
                            <p>
                                <strong>Username:</strong> {{ Person.Users[0].UserName }}
                            </p>
                        {% else %}
                            A user account has been created for you to access this group information. The details of this account can be found below.
        
                            </p>
        
                            <p>
                                <strong>Username:</strong> {{ Person.Users[0].UserName }} <br />
                                <strong>Password:</strong> {{ NewPassword }}
                            </p>
                        {% endif %}
                    {% endif %}



                    {{ ''Global'' | Attribute:''EmailFooter'' }}'
                               ,'F66D7DAE-89C1-E8BC-48F8-A0D6B849615F'
                               ,@GroupCategoryId) " );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
            // Database Changes
            //
            
            DropForeignKey("dbo.Group", "WelcomeSystemEmailId", "dbo.SystemEmail");
            DropForeignKey("dbo.Group", "SyncDataViewId", "dbo.DataView");
            DropForeignKey("dbo.Group", "ExitSystemEmailId", "dbo.SystemEmail");
            DropIndex("dbo.Group", new[] { "SyncDataViewId" });
            DropIndex("dbo.Group", new[] { "ExitSystemEmailId" });
            DropIndex("dbo.Group", new[] { "WelcomeSystemEmailId" });
            DropColumn("dbo.Group", "AddUserAccountsDuringSync");
            DropColumn("dbo.Group", "SyncDataViewId");
            DropColumn("dbo.Group", "ExitSystemEmailId");
            DropColumn("dbo.Group", "WelcomeSystemEmailId");

            //
            // Delete Service Job
            //
            Sql( @"DELETE FROM [ServiceJob] WHERE [Guid] = '57B539BC-7C4D-25BB-4EEB-39DF0EF62EBC'" );

            // 
            // Delete System Emails
            //
            Sql( @"DELETE FROM [SystemEmail] WHERE [Guid] in ('F66D7DAE-89C1-E8BC-48F8-A0D6B849615F', '9AC62F1B-16E0-3886-4CE6-2B9290D6B135')" );


        }
    }
}
