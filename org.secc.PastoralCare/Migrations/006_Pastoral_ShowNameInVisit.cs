using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace org.secc.PastoralCare.Migrations
{
    [MigrationNumber( 6, "1.2.0" )]
    class Pastoral_ShowNameInVisit : Migration
    {

        public override void Up()
        {
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "41C3DD25-249D-42E7-90E4-F2ED09A40938", "8665F2E0-FF2C-423E-9B07-E8C0A751EE7F", 10, true, false, true, false, @"<div class=""panel panel-default""> <div class=""panel-heading""> <div class=""panel-title""><i class=""fa fa-heart""></i> Visit Information for {{Workflow | Attribute: 'PersonToVisit', 'FullName'}}</div> </div> <div class=""panel-body""> <div class=""row""> <div class=""col-sm-6"">", @"</div>", "F1EA7FC2-36E4-4DA0-AA89-30B56BDB635D" ); // Nursing Home Resident: Add Name To Visit Info
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "62E4A560-CBBA-4ABB-9BA0-36E3408B7B49", "64CB035C-7614-4B4C-8B8D-BC1EC35CF2EE", 10, true, false, true, false, @"<div class=""panel panel-default""> <div class=""panel-heading""> <div class=""panel-title""><i class=""fa fa-heart""></i> Visit Information for {{Workflow | Attribute: 'PersonToVisit', 'FullName'}}</div> </div> <div class=""panel-body""> <div class=""row""> <div class=""col-sm-6"">", @"</div>", "614572D3-7A48-40A7-B1D8-CF3F42EB51FB" ); // Hospital Admission: Add Name To Visit Info
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "D8CC1582-5F0A-4270-931D-C9AE1BF91672", "9772094D-6FEF-4E99-A782-BBF04D29BA7F", 8, true, false, true, false, @"<div class=""panel panel-default""> <div class=""panel-heading""> <div class=""panel-title""><i class=""fa fa-heart""></i> Visit Information for {{Workflow | Attribute: 'PersonToVisit', 'FullName'}}</div> </div> <div class=""panel-body""> <div class=""row""> <div class=""col-sm-6"">", @"</div>", "ECC91844-BB35-4130-BC11-1A422EED81DA" ); // Homebound Resident: Add Name To Visit Info
        }
        public override void Down()
        {
        }
    }
}
