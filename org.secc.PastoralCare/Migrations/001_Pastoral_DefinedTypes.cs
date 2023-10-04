// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using Rock.Web.Cache;

namespace org.secc.PastoralCare.Migrations
{
    [MigrationNumber( 1, "1.2.0" )]
    class Pastoral_DefinedTypes : Migration
    {
        public override void Up()
        {
            // Make sure this is unique
            if ( DefinedTypeCache.Get( new Guid("0913F7A9-A2BF-479C-96EC-6CDB56310A83") ) == null )
            { 
                RockMigrationHelper.AddDefinedType( "Global", "Hospitals", "Hospital List", "0913F7A9-A2BF-479C-96EC-6CDB56310A83", @"" );
                RockMigrationHelper.AddDefinedTypeAttribute( "0913F7A9-A2BF-479C-96EC-6CDB56310A83", "9C204CD0-1233-41C5-818A-C5DA439445AA", "City", "Qualifier2", "", 0, "", "CEDC60C1-0F9E-4FE2-BE62-41716813C968" );
                RockMigrationHelper.AddDefinedTypeAttribute( "0913F7A9-A2BF-479C-96EC-6CDB56310A83", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Notes", "Qualifier8", "", 0, "", "C47A879E-F737-4156-A1FF-B7C465FDB9BC" );
                RockMigrationHelper.AddDefinedTypeAttribute( "0913F7A9-A2BF-479C-96EC-6CDB56310A83", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Phone", "Qualifier5", "", 0, "", "A4E41679-2CE6-479F-84D4-6821B25E3648" );
                RockMigrationHelper.AddDefinedTypeAttribute( "0913F7A9-A2BF-479C-96EC-6CDB56310A83", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Speed Dial", "Qualifier6", "", 0, "", "D97EC9DE-5D6A-42FD-B4CE-0516FD5455F6" );
                RockMigrationHelper.AddDefinedTypeAttribute( "0913F7A9-A2BF-479C-96EC-6CDB56310A83", "9C204CD0-1233-41C5-818A-C5DA439445AA", "State", "Qualifier3", "", 0, "", "239E507C-7C1B-4B4D-84D4-33C368843F04" );
                RockMigrationHelper.AddDefinedTypeAttribute( "0913F7A9-A2BF-479C-96EC-6CDB56310A83", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Street Address", "Qualifier1", "", 0, "", "73AC0DCE-CE90-4835-AAE7-E98B08F52E9C" );
                RockMigrationHelper.AddDefinedTypeAttribute( "0913F7A9-A2BF-479C-96EC-6CDB56310A83", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Zip", "Qualifier4", "", 0, "", "46A83E00-D530-48AD-B935-52C015DCA901" );
                RockMigrationHelper.AddDefinedValue( "0913F7A9-A2BF-479C-96EC-6CDB56310A83", "Sample Hospital", "", "0997ACB6-A4B4-4766-B573-3C44D14DF342", false );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF342", "239E507C-7C1B-4B4D-84D4-33C368843F04", @"KY" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF342", "46A83E00-D530-48AD-B935-52C015DCA901", @"40023" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF342", "73AC0DCE-CE90-4835-AAE7-E98B08F52E9C", @"1 Main St" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF342", "A4E41679-2CE6-479F-84D4-6821B25E3648", @"(502) 111-1111" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF342", "C47A879E-F737-4156-A1FF-B7C465FDB9BC", @"" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF342", "CEDC60C1-0F9E-4FE2-BE62-41716813C968", @"Louisville" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF342", "D97EC9DE-5D6A-42FD-B4CE-0516FD5455F6", @"" );
            }

            // Make sure this is unique
            if ( DefinedTypeCache.Get( new Guid( "4573E600-4E00-4BE9-BA92-D17093C735D6" ) ) == null )
            {
                RockMigrationHelper.AddDefinedType( "Global", "Nursing Homes", "Nursing Home List", "4573E600-4E00-4BE9-BA92-D17093C735D6", @"" );
                RockMigrationHelper.AddDefinedTypeAttribute( "4573E600-4E00-4BE9-BA92-D17093C735D6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "City", "Qualifier2", "", 0, "", "CEDC60C1-0F9E-4FE2-BE62-41716813C969" );
                RockMigrationHelper.AddDefinedTypeAttribute( "4573E600-4E00-4BE9-BA92-D17093C735D6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Notes", "Qualifier8", "", 0, "", "C47A879E-F737-4156-A1FF-B7C465FDB9BD" );
                RockMigrationHelper.AddDefinedTypeAttribute( "4573E600-4E00-4BE9-BA92-D17093C735D6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Phone", "Qualifier5", "", 0, "", "A4E41679-2CE6-479F-84D4-6821B25E3649" );
                RockMigrationHelper.AddDefinedTypeAttribute( "4573E600-4E00-4BE9-BA92-D17093C735D6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Pastoral Minister", "Qualifier6", "", 0, "", "D97EC9DE-5D6A-42FD-B4CE-0516FD5455F7" );
                RockMigrationHelper.AddDefinedTypeAttribute( "4573E600-4E00-4BE9-BA92-D17093C735D6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "State", "Qualifier3", "", 0, "", "239E507C-7C1B-4B4D-84D4-33C368843F05" );
                RockMigrationHelper.AddDefinedTypeAttribute( "4573E600-4E00-4BE9-BA92-D17093C735D6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Street Address", "Qualifier1", "", 0, "", "73AC0DCE-CE90-4835-AAE7-E98B08F52E9D" );
                RockMigrationHelper.AddDefinedTypeAttribute( "4573E600-4E00-4BE9-BA92-D17093C735D6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Zip", "Qualifier4", "", 0, "", "46A83E00-D530-48AD-B935-52C015DCA902" );
                RockMigrationHelper.AddDefinedValue( "4573E600-4E00-4BE9-BA92-D17093C735D6", "Sample Nursing Home", "", "0997ACB6-A4B4-4766-B573-3C44D14DF343", false );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF343", "239E507C-7C1B-4B4D-84D4-33C368843F05", @"KY" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF343", "46A83E00-D530-48AD-B935-52C015DCA902", @"40023" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF343", "73AC0DCE-CE90-4835-AAE7-E98B08F52E9D", @"2 Main St" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF343", "A4E41679-2CE6-479F-84D4-6821B25E3649", @"(502) 111-1111" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF343", "C47A879E-F737-4156-A1FF-B7C465FDB9BD", @"" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF343", "CEDC60C1-0F9E-4FE2-BE62-41716813C969", @"Louisville" );
                RockMigrationHelper.AddDefinedValueAttributeValue( "0997ACB6-A4B4-4766-B573-3C44D14DF343", "D97EC9DE-5D6A-42FD-B4CE-0516FD5455F7", @"John Adams" );
            }
            // Make sure to clear the Rock Cache so the defined type above will be there
            RockCache.ClearAllCachedItems();
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedType( "0913F7A9-A2BF-479C-96EC-6CDB56310A83" );
            RockMigrationHelper.DeleteDefinedType( "4573E600-4E00-4BE9-BA92-D17093C735D6" );
        }
    }
}

