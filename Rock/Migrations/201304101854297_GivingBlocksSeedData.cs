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
    public partial class GivingBlocksSeedData : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add DefinedType and DefinedValues for giving recurrences
            AddDefinedType( "Financial", "Transaction Frequency Type", "Types of recurring transaction frequencies", "1F645CFB-5BBD-4465-B9CA-0D2104A1479B" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Weekly", "Every Week", "35711E44-131B-4534-B0B2-F0A749292362" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Bi-Weekly", "Every Two Weeks", "72990023-0D43-4554-8D32-28461CAB8920" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Monthly", "Once a Month", "1400753C-A0F9-4A45-8A1D-81C98450BD1F" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Twice a Month", "Twice a Month", "791C863D-2600-445B-98F8-3E5B66A3DEC4" );

            Sql( @"
                INSERT INTO [Fund] ([Name], [PublicName], [Description], [IsTaxDeductible], [Order], [IsActive], [IsPledgable], [Guid]) 
                    VALUES ('General Fund', 'General Fund', 'General giving fund', 1, 0, 1,	1, '4410306F-3FB5-4A57-9A80-09A3F9D40D0C')
                INSERT INTO [Fund] ([Name], [PublicName], [Description], [IsTaxDeductible], [Order], [IsActive], [IsPledgable], [Guid]) 
                    VALUES ('Building Fund', 'Building Fund', 'Building giving fund', 1, 0, 1, 1, '67C6181C-1D8C-44D7-B262-B81E746F06D8')
                INSERT INTO [Fund] ([Name], [PublicName], [Description], [IsTaxDeductible], [Order], [IsActive], [IsPledgable], [Guid]) 
                    VALUES ('Mission Fund', 'Mission Fund', 'Missions giving fund', 1, 0, 1, 1, 'BAB250EE-CAE6-4A41-9756-AD9327408BE0')
            " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @" DELETE FROM [dbo].[Fund] 
                WHERE [Guid] = '4410306F-3FB5-4A57-9A80-09A3F9D40D0C'
                OR [Guid] = '67C6181C-1D8C-44D7-B262-B81E746F06D8'
                OR [Guid] = 'BAB250EE-CAE6-4A41-9756-AD9327408BE0' " );

            DeleteDefinedType( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B" ); // Transaction Frequency Type
            DeleteDefinedValue( "35711E44-131B-4534-B0B2-F0A749292362" );
            DeleteDefinedValue( "72990023-0D43-4554-8D32-28461CAB8920" );
            DeleteDefinedValue( "1400753C-A0F9-4A45-8A1D-81C98450BD1F" );
            DeleteDefinedValue( "791C863D-2600-445B-98F8-3E5B66A3DEC4" );
        }
    }
}
