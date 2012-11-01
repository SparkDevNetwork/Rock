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
    public partial class RenameGroupForeignKeys : RockMigration_2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"sp_rename '[FK_cmsAuth_groupsGroup_GroupId]', 'FK_cmsAuth_crmGroup_GroupId' " );
            Sql( @"sp_rename '[FK_dbo.groupGroupLocation_dbo.coreDefinedValue_LocationTypeId]', 'FK_crmGroupLocation_coreDefinedValue_LocationTypeId' " );
            Sql( @"sp_rename '[FK_dbo.groupGroupLocation_dbo.crmLocation_LocationId]', 'FK_crmGroupLocation_crmLocation_LocationId' " );
            Sql( @"sp_rename '[FK_dbo.groupGroupLocation_dbo.groupsGroup_GroupId]', 'FK_crmGroupLocation_crmGroup_GroupId' " );
            Sql( @"sp_rename '[FK_dbo.groupsGroup_dbo.crmCampus_CampusId]', 'FK_crmGroup_crmCampus_CampusId' " );
            Sql( @"sp_rename '[FK_dbo.groupsGroupRole_dbo.groupsGroupType_GroupTypeId]', 'FK_crmGroupRole_crmGroupType_GroupTypeId' " );
            Sql( @"sp_rename '[FK_groupsGroup_groupsGroup_ParentGroupId]', 'FK_crmGroup_crmGroup_ParentGroupId' " );
            Sql( @"sp_rename '[FK_groupsGroup_groupsGroupType_GroupTypeId]', 'FK_crmGroup_crmGroupType_GroupTypeId' " );
            Sql( @"sp_rename '[FK_groupsGroupType_groupsGroupRole_DefaultGroupRoleId]', 'FK_crmGroupType_crmGroupRole_DefaultGroupRoleId' " );
            Sql( @"sp_rename '[FK_groupsGroupTypeAssociation_groupsGroupType_ChildGroupTypeId]', 'FK_crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId' " );
            Sql( @"sp_rename '[FK_groupsGroupTypeAssociation_groupsGroupType_ParentGroupTypeId]', 'FK_crmGroupTypeAssociation_crmGroupType_ParentGroupTypeId' " );
            Sql( @"sp_rename '[FK_groupsMember_crmPerson_PersonId]', 'FK_crmGroupMember_crmPerson_PersonId' " );
            Sql( @"sp_rename '[FK_groupsMember_groupsGroup_GroupId]', 'FK_crmGroupMember_crmGroup_GroupId' " );
            Sql( @"sp_rename '[FK_groupsMember_groupsGroupRole_GroupRoleId]', 'FK_crmGroupMember_crmGroupRole_GroupRoleId' " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"sp_rename '[FK_cmsAuth_crmGroup_GroupId]','FK_cmsAuth_groupsGroup_GroupId' " );
            Sql( @"sp_rename '[FK_crmGroupLocation_coreDefinedValue_LocationTypeId]','FK_dbo.groupGroupLocation_dbo.coreDefinedValue_LocationTypeId' " );
            Sql( @"sp_rename '[FK_crmGroupLocation_crmLocation_LocationId]','FK_dbo.groupGroupLocation_dbo.crmLocation_LocationId' " );
            Sql( @"sp_rename '[FK_crmGroupLocation_crmGroup_GroupId]','FK_dbo.groupGroupLocation_dbo.groupsGroup_GroupId' " );
            Sql( @"sp_rename '[FK_crmGroup_crmCampus_CampusId]','FK_dbo.groupsGroup_dbo.crmCampus_CampusId' " );
            Sql( @"sp_rename '[FK_crmGroupRole_crmGroupType_GroupTypeId]','FK_dbo.groupsGroupRole_dbo.groupsGroupType_GroupTypeId' " );
            Sql( @"sp_rename '[FK_crmGroup_crmGroup_ParentGroupId]','FK_groupsGroup_groupsGroup_ParentGroupId' " );
            Sql( @"sp_rename '[FK_crmGroup_crmGroupType_GroupTypeId]','FK_groupsGroup_groupsGroupType_GroupTypeId' " );
            Sql( @"sp_rename '[FK_crmGroupType_crmGroupRole_DefaultGroupRoleId]','FK_groupsGroupType_groupsGroupRole_DefaultGroupRoleId' " );
            Sql( @"sp_rename '[FK_crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId]','FK_groupsGroupTypeAssociation_groupsGroupType_ChildGroupTypeId' " );
            Sql( @"sp_rename '[FK_crmGroupTypeAssociation_crmGroupType_ParentGroupTypeId]','FK_groupsGroupTypeAssociation_groupsGroupType_ParentGroupTypeId' " );
            Sql( @"sp_rename '[FK_crmGroupMember_crmPerson_PersonId]','FK_groupsMember_crmPerson_PersonId' " );
            Sql( @"sp_rename '[FK_crmGroupMember_crmGroup_GroupId]','FK_groupsMember_groupsGroup_GroupId' " );
            Sql( @"sp_rename '[FK_crmGroupMember_crmGroupRole_GroupRoleId]','FK_groupsMember_groupsGroupRole_GroupRoleId' " );
        }
    }
}
