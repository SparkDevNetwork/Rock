//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class FixSystemData : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE [BlockType] SET [IsSystem] = 1 WHERE [Guid] IN (
        '605389F5-5BC5-438F-8757-110328B0CED3',
        '67E9E493-1D11-4C73-8E59-6D3C2C25CA25',
        '4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6',
        '584DC3C4-5B58-4467-BF58-3E49FDA05655',
        '98D27912-C4BD-4E94-AEE1-AFBF688D7264',
        'B3F7A325-24DB-4A80-ADFD-1E8E1C85217D',
        'B3E4584A-D3C3-4F68-9B7C-D1641B9B08CF',
        'C6DFE5AE-8C4C-49AD-8EC9-11CE03146F53',
        '005E5980-E2D2-4958-ACB6-BECBC6D1F5C4'
    )

    UPDATE [Attribute] SET [IsSystem] = 1 WHERE [Guid] IN (
        'CB6EB814-9833-4B60-9232-730BFF709C00',
        'C28B47BB-253E-49F2-954E-7652600B1012',
        '2D5FF74A-D316-4924-BCD2-6AA338D8DAAC',
        'F76C5EEF-FD45-4BD6-A903-ED5AB53BB928',
        '620957FF-BC28-4A89-A74F-C917DA5CFD47',
        '985128EE-D40C-4598-B14B-7AD728ECCE38',
        'AF67C486-A4D2-43B6-A2F6-794C84174A33',
        '4E85A2CD-2BF9-448A-BF51-8FE32DC6C968',
        'A29F64C3-298B-40F8-B873-48FA91A1308F',
        'A7252F4B-4342-4816-A74F-423AF31EDA87',
        '76621982-26A5-4A78-8771-B49E34179E72',
        'CB22C98C-BF7D-4BB5-8C1E-E8E859D621D2'
    )

    UPDATE [Block] SET [IsSystem] = 1 WHERE [Guid] IN (
        '77F8F713-A82C-457F-9EF7-92D8BDD7B196'
    )
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
