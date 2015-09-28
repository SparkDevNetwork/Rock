using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 5, "1.0.14" )]
    public class LifeGroupAddHasChildren : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var typeId = new Rock.Model.DefinedTypeService( new Rock.Data.RockContext() ).GetByGuid( new Guid( "512F355E-9441-4C47-BE47-7FFE19209496" ) ).Id.ToString();
            Sql(String.Format( @"
UPDATE [AttributeQualifier]
SET Value = '{0}'
WHERE Guid = '9AF45EBE-3154-402D-B5C5-03BA338170AC'
" ,typeId));
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
