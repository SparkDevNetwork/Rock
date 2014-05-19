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
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GradeEmailTemplate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
DELETE FROM [EmailTemplate] WHERE [Guid] = 'CCEDEC52-EC8A-41BF-9F78-C60418835257'

INSERT INTO [EmailTemplate] ([IsSystem], [PersonId], [Category], [Title], [From], [To], [Cc], [Bcc], [Subject], [Body], [Guid]) 
VALUES (1, NULL, 'Residency', 'Project Grade Request', 'rock@sparkdevnetwork.com', '', '', '', 'Project Grade Request', 
'{{ EmailHeader }}

{{ Facilitator.FirstName }},<br/><br/>

{{Resident.FullName}} requests that you <a href=''{{ GradeDetailPageUrl }}''>grade</a> {{ Project.Name }} - {{ Project.Description}} 
<br/>
<br/>
Thank-you,<br/>
{{ OrganizationName }}  

{{ EmailFooter }}', 'CCEDEC52-EC8A-41BF-9F78-C60418835257')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [EmailTemplate] WHERE [Guid] = 'CCEDEC52-EC8A-41BF-9F78-C60418835257'" );
        }
    }
}
