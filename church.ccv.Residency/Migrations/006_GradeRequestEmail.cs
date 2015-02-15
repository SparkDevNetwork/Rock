using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.ccvonline.Residency.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 6, "1.0.8" )]
    public class GradeRequestEmail : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {

            Sql( @"
DELETE FROM [SystemEmail] WHERE [Guid] = 'CCEDEC52-EC8A-41BF-9F78-C60418835257'

INSERT INTO [SystemEmail] ([IsSystem], [Category], [Title], [From], [To], [Cc], [Bcc], [Subject], [Body], [Guid]) 
VALUES (1, 'Residency', 'Project Grade Request', 'rock@sparkdevnetwork.com', '', '', '', 'Project Grade Request', 
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
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}
