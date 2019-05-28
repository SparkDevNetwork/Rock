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
    public partial class Assessments2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddInternalPagesUp();
            UpdateAssessmentTestsPageLayout();
            UpdateSpiritualGiftsRoute();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddInternalPagesDown();
        }

        /// <summary>
        /// Adds the Assessment internal pages.
        /// </summary>
        private void AddInternalPagesUp()
        {
            RockMigrationHelper.AddPage( true, "BF04BB7E-BE3A-4A38-A37C-386B55496303","D65F783D-87A9-4CC9-8110-E83466A0EADB","Assessments","","985D7F56-9FD6-421B-B406-2D6B87CAFAE9",""); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "985D7F56-9FD6-421B-B406-2D6B87CAFAE9","D65F783D-87A9-4CC9-8110-E83466A0EADB","Conflict Profile Assessment","","218F78A5-3274-4A45-927E-EBC7B26E28EC",""); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "985D7F56-9FD6-421B-B406-2D6B87CAFAE9","D65F783D-87A9-4CC9-8110-E83466A0EADB","Emotional Intelligence Assessment","","6B8DB6BD-B747-453E-B116-153C60A48448",""); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "985D7F56-9FD6-421B-B406-2D6B87CAFAE9","D65F783D-87A9-4CC9-8110-E83466A0EADB","DISC Assessment","","E64562B2-8D5F-40FD-B064-F249F76EF3BE",""); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "985D7F56-9FD6-421B-B406-2D6B87CAFAE9","D65F783D-87A9-4CC9-8110-E83466A0EADB","Gifts Assessment","","112F0A18-8F8A-4A06-B597-F0CF66480DA8",""); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "985D7F56-9FD6-421B-B406-2D6B87CAFAE9","D65F783D-87A9-4CC9-8110-E83466A0EADB","Motivators Assessment","","57BBC962-2F87-4A76-9647-26C6524EF87E",""); // Site:Rock RMS

            RockMigrationHelper.AddPageRoute("218F78A5-3274-4A45-927E-EBC7B26E28EC","ConflictProfile","0DA62733-6C44-40AD-B699-B2932B953722");// for Page:Conflict Profile Assessment
            RockMigrationHelper.AddPageRoute("218F78A5-3274-4A45-927E-EBC7B26E28EC","ConflictProfile/{rckipid}","ABDD2648-8EF1-4339-B8D6-0898EF09006A");// for Page:Conflict Profile Assessment
            RockMigrationHelper.AddPageRoute("6B8DB6BD-B747-453E-B116-153C60A48448","EQ","4C0DD8AB-67DF-435E-99CD-990C383B956E");// for Page:Emotional Intelligence Assessment
            RockMigrationHelper.AddPageRoute("6B8DB6BD-B747-453E-B116-153C60A48448","EQ/{rckipid}","B0545679-CC90-475B-9C83-DFFC8676B88C");// for Page:Emotional Intelligence Assessment
            RockMigrationHelper.AddPageRoute("112F0A18-8F8A-4A06-B597-F0CF66480DA8","SpiritualGifts","47E04A33-6AD1-4E31-9CE1-8D956339EFFE");// for Page:Gifts Assessment
            RockMigrationHelper.AddPageRoute("112F0A18-8F8A-4A06-B597-F0CF66480DA8","SpiritualGifts/{rckipid}","3FE0AAD9-2C78-44B6-9A59-039F677F6FCE");// for Page:Gifts Assessment
            RockMigrationHelper.AddPageRoute("57BBC962-2F87-4A76-9647-26C6524EF87E","Motivators","E32CC839-58DA-461F-B988-DAC4DD7A30F7");// for Page:Motivators Assessment
            RockMigrationHelper.AddPageRoute("57BBC962-2F87-4A76-9647-26C6524EF87E","Motivators/{rckipid}","72E55DBD-2DD6-4D19-BFD8-26D932EC1FD0");// for Page:Motivators Assessment
            RockMigrationHelper.AddPageRoute("E64562B2-8D5F-40FD-B064-F249F76EF3BE","DISC","606D5685-8030-4BB9-BD0B-D6B8F06BA551");// for Page:Motivators Assessment
            RockMigrationHelper.AddPageRoute("E64562B2-8D5F-40FD-B064-F249F76EF3BE","DISC/{rckipid}","8B16CACD-466B-4670-9F66-CBEEA87715E0");// for Page:Motivators Assessment

            // Add Block to Page: Conflict Profile Assessment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "218F78A5-3274-4A45-927E-EBC7B26E28EC".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"91473D2F-607D-4260-9C6A-DD3762FE472D".AsGuid(), "Conflict Profile","Main",@"",@"",0,"934514C9-7973-4705-BFAF-D3DD05E16E2C"); 
            // Add Block to Page: Emotional Intelligence Assessment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6B8DB6BD-B747-453E-B116-153C60A48448".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"040CFD6D-5155-4BC9-BAEE-A53219A7BECE".AsGuid(), "EQ Assessment","Main",@"",@"",0,"E25272A2-FE32-4A94-BDE9-437189A87993"); 
            // Add Block to Page: DISC Assessment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E64562B2-8D5F-40FD-B064-F249F76EF3BE".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"A161D12D-FEA7-422F-B00E-A689629680E4".AsGuid(), "Disc","Main",@"",@"",0,"5D52E8D4-63E8-48F2-B016-923301E61B9C"); 
            // Add Block to Page: Gifts Assessment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "112F0A18-8F8A-4A06-B597-F0CF66480DA8".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"A7E86792-F0ED-46F2-988D-25EBFCD1DC96".AsGuid(), "Gifts Assessment","Main",@"",@"",0,"47DC0C1F-B94B-4A58-8ECD-6DBE934ECFF9"); 
            // Add Block to Page: Motivators Assessment Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "57BBC962-2F87-4A76-9647-26C6524EF87E".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"18CF8DA8-5DE0-49EC-A279-D5507CFA5713".AsGuid(), "Motivators Assessment","Main",@"",@"",0,"02898091-CF95-4303-884D-9019666096AE");
        }

        /// <summary>
        /// Removes the Assessment internal pages.
        /// </summary>
        private void AddInternalPagesDown()
        {
            // Remove Block: Motivators Assessment, from Page: Motivators Assessment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("02898091-CF95-4303-884D-9019666096AE");
            // Remove Block: Gifts Assessment, from Page: Gifts Assessment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("47DC0C1F-B94B-4A58-8ECD-6DBE934ECFF9");
            // Remove Block: Disc, from Page: DISC Assessment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("5D52E8D4-63E8-48F2-B016-923301E61B9C");
            // Remove Block: EQ Assessment, from Page: Emotional Intelligence Assessment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("E25272A2-FE32-4A94-BDE9-437189A87993");
            // Remove Block: Conflict Profile, from Page: Conflict Profile Assessment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("934514C9-7973-4705-BFAF-D3DD05E16E2C");

            RockMigrationHelper.DeletePage("57BBC962-2F87-4A76-9647-26C6524EF87E"); //  Page: Motivators Assessment, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("112F0A18-8F8A-4A06-B597-F0CF66480DA8"); //  Page: Gifts Assessment, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("E64562B2-8D5F-40FD-B064-F249F76EF3BE"); //  Page: DISC Assessment, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("6B8DB6BD-B747-453E-B116-153C60A48448"); //  Page: Emotional Intelligence Assessment, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("218F78A5-3274-4A45-927E-EBC7B26E28EC"); //  Page: Conflict Profile Assessment, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("985D7F56-9FD6-421B-B406-2D6B87CAFAE9"); //  Page: Assessments, Layout: Full Width, Site: Rock RMS
        }

        /// <summary>
        /// ED: Update Assessment test pages to use Full Width Narrow
        /// </summary>
        private void UpdateAssessmentTestsPageLayout()
        {
            // Change layout for DISC Assessment page to Full Width Narrow
            RockMigrationHelper.UpdatePageLayout( "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1", "BE15B7BC-6D64-4880-991D-FDE962F91196" );
            
            // Change layout for Motivators Assessment page to Full Width Narrow
            RockMigrationHelper.UpdatePageLayout( "0E6AECD6-675F-4908-9FA3-C7E46040527C", "BE15B7BC-6D64-4880-991D-FDE962F91196" );
        }

        /// <summary>
        /// ED: Update Spiritual Gifts route for the external page.
        /// </summary>
        private void UpdateSpiritualGiftsRoute()
        {
            RockMigrationHelper.UpdatePageRoute( "1B580CA3-F1DB-443F-ABA4-F9C7EC6A8A1B", "06410598-3DA4-4710-A047-A518157753AB", "SpiritualGifts" );
            RockMigrationHelper.UpdatePageRoute( "B991B18C-9B71-4BA9-8149-760CF15F37F3", "06410598-3DA4-4710-A047-A518157753AB", "SpiritualGifts/{rckipid}" );
            Sql( $@"
                UPDATE [dbo].[AssessmentType]
                SET [AssessmentPath] = 'SpiritualGifts'
                    , [AssessmentResultsPath] = 'SpiritualGifts'
                WHERE [Guid] = 'B8FBD371-6B32-4BE5-872F-51400D16EC5D'" );
        }
    }
}
