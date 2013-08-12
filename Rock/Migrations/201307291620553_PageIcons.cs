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
    public partial class PageIcons : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
Update [Page] set [IconCssClass] = 'icon-eye-open' where [Guid] = '4011CB37-28AA-46C4-99D5-826F4A9CADF5' --Data Views
Update [Page] set [IconCssClass] = 'icon-list-alt' where [Guid] = '0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D' --Reports
Update [Page] set [IconCssClass] = 'icon-check' where [Guid] = '9DF95EFF-88B4-401A-8F5F-E3B8DB02A308' --HTML Content Approval
Update [Page] set [IconCssClass] = 'icon-exchange' where [Guid] = '4A833BE3-7D5E-4C38-AF60-5706260015EA' --Routes
Update [Page] set [IconCssClass] = 'icon-bullhorn' where [Guid] = '74345663-5BCA-493C-A2FB-80DC9CC8E70C' --Ad Campaigns
Update [Page] set [IconCssClass] = 'icon-gears' where [Guid] = 'DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A' --Workflows
Update [Page] set [IconCssClass] = 'icon-cloud-upload' where [Guid] = '1A3437C8-D4CB-4329-A366-8D6A4CBF79BF' --Prayer Administration
Update [Page] set [IconCssClass] = 'icon-group' where [Guid] = '4E237286-B715-4109-A578-C1445EC02707' --Group Viewer
Update [Page] set [IconCssClass] = 'icon-dollar' where [Guid] = '7CA317B5-5C47-465D-B407-7D614F2A568F' --Transactions
Update [Page] set [IconCssClass] = 'icon-list' where [Guid] = '1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867' --Pledge List
Update [Page] set [IconCssClass] = 'icon-calendar' where [Guid] = 'F23C5BF7-4F52-448F-8445-6BAEEE3030AB' --Scheduled Contributions
Update [Page] set [IconCssClass] = 'icon-building' where [Guid] = '2B630A3B-E081-4204-A3E4-17BB3A5F063D' --Accounts
Update [Page] set [IconCssClass] = 'icon-archive' where [Guid] = 'EF65EFF2-99AC-4081-8E09-32A04518683A' --Financial Batch
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
Update [Page] set [IconCssClass] = '' where [Guid] = '4011CB37-28AA-46C4-99D5-826F4A9CADF5' --Data Views
Update [Page] set [IconCssClass] = '' where [Guid] = '0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D' --Reports
Update [Page] set [IconCssClass] = '' where [Guid] = '9DF95EFF-88B4-401A-8F5F-E3B8DB02A308' --HTML Content Approval
Update [Page] set [IconCssClass] = '' where [Guid] = '4A833BE3-7D5E-4C38-AF60-5706260015EA' --Routes
Update [Page] set [IconCssClass] = '' where [Guid] = '74345663-5BCA-493C-A2FB-80DC9CC8E70C' --Ad Campaigns
Update [Page] set [IconCssClass] = '' where [Guid] = 'DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A' --Workflows
Update [Page] set [IconCssClass] = '' where [Guid] = '1A3437C8-D4CB-4329-A366-8D6A4CBF79BF' --Prayer Administration
Update [Page] set [IconCssClass] = '' where [Guid] = '4E237286-B715-4109-A578-C1445EC02707' --Group Viewer
Update [Page] set [IconCssClass] = '' where [Guid] = '7CA317B5-5C47-465D-B407-7D614F2A568F' --Transactions
Update [Page] set [IconCssClass] = '' where [Guid] = '1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867' --Pledge List
Update [Page] set [IconCssClass] = '' where [Guid] = 'F23C5BF7-4F52-448F-8445-6BAEEE3030AB' --Scheduled Contributions
Update [Page] set [IconCssClass] = '' where [Guid] = '2B630A3B-E081-4204-A3E4-17BB3A5F063D' --Accounts
Update [Page] set [IconCssClass] = '' where [Guid] = 'EF65EFF2-99AC-4081-8E09-32A04518683A' --Financial Batch
" );
        }
    }
}
