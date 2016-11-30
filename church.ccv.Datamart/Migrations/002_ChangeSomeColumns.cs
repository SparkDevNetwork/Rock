using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace church.ccv.Datamart.Migrations
{
    [MigrationNumber( 2, "1.2.0" )]
    public class ChangeSomeColumns : Rock.Plugin.Migration
    {
        public override void Up()
        {
            Sql( "sp_RENAME '_church_ccv_Datamart_Person.2015Contrib' , 'Giving2015', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Person.2014Contrib' , 'Giving2014', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Person.2013Contrib' , 'Giving2013', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Person.2012Contrib' , 'Giving2012', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Person.2011Contrib' , 'Giving2011', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Person.2010Contrib' , 'Giving2010', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Person.2009Contrib' , 'Giving2009', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Person.2008Contrib' , 'Giving2008', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Person.2007Contrib' , 'Giving2007', 'COLUMN'" );

            Sql( "sp_RENAME '_church_ccv_Datamart_Family.2015Contrib' , 'Giving2015', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Family.2014Contrib' , 'Giving2014', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Family.2013Contrib' , 'Giving2013', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Family.2012Contrib' , 'Giving2012', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Family.2011Contrib' , 'Giving2011', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Family.2010Contrib' , 'Giving2010', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Family.2009Contrib' , 'Giving2009', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Family.2008Contrib' , 'Giving2008', 'COLUMN'" );
            Sql( "sp_RENAME '_church_ccv_Datamart_Family.2007Contrib' , 'Giving2007', 'COLUMN'" );

            Sql( "alter table _church_ccv_Datamart_Family add CampusId int null" );
            Sql( "alter table _church_ccv_Datamart_Person add ConnectionStatusValueId int null");
        }

        public override void Down()
        {
            //
        }
    }
}
