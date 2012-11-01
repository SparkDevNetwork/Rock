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
    public partial class RenameGroupForeignKeys : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"sp_rename '[FK_dbo.cmsAuth_groupsGroup_GroupId]', 'FK_dbo.cmsAuth_crmGroup_GroupId' " );
            Sql( @"sp_rename '[FK_dbo.groupGroupLocation_dbo.coreDefinedValue_LocationTypeId]', 'FK_dbo.crmGroupLocation_coreDefinedValue_LocationTypeId' " );
            Sql( @"sp_rename '[FK_dbo.groupGroupLocation_dbo.crmLocation_LocationId]', 'FK_dbo.crmGroupLocation_crmLocation_LocationId' " );
            Sql( @"sp_rename '[FK_dbo.groupGroupLocation_dbo.groupsGroup_GroupId]', 'FK_dbo.crmGroupLocation_crmGroup_GroupId' " );
            Sql( @"sp_rename '[FK_dbo.groupsGroup_dbo.crmCampus_CampusId]', 'FK_dbo.crmGroup_crmCampus_CampusId' " );
            Sql( @"sp_rename '[FK_dbo.groupsGroupRole_dbo.groupsGroupType_GroupTypeId]', 'FK_dbo.crmGroupRole_crmGroupType_GroupTypeId' " );
            Sql( @"sp_rename '[FK_dbo.groupsGroup_groupsGroup_ParentGroupId]', 'FK_dbo.crmGroup_crmGroup_ParentGroupId' " );
            Sql( @"sp_rename '[FK_dbo.groupsGroup_groupsGroupType_GroupTypeId]', 'FK_dbo.crmGroup_crmGroupType_GroupTypeId' " );
            Sql( @"sp_rename '[FK_dbo.groupsGroupType_groupsGroupRole_DefaultGroupRoleId]', 'FK_dbo.crmGroupType_crmGroupRole_DefaultGroupRoleId' " );
            Sql( @"sp_rename '[FK_dbo.groupsGroupTypeAssociation_groupsGroupType_ChildGroupTypeId]', 'FK_dbo.crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId' " );
            Sql( @"sp_rename '[FK_dbo.groupsGroupTypeAssociation_groupsGroupType_ParentGroupTypeId]', 'FK_dbo.crmGroupTypeAssociation_crmGroupType_ParentGroupTypeId' " );
            Sql( @"sp_rename '[FK_dbo.groupsMember_crmPerson_PersonId]', 'FK_dbo.crmGroupMember_crmPerson_PersonId' " );
            Sql( @"sp_rename '[FK_dbo.groupsMember_groupsGroup_GroupId]', 'FK_dbo.crmGroupMember_crmGroup_GroupId' " );
            Sql( @"sp_rename '[FK_dbo.groupsMember_groupsGroupRole_GroupRoleId]', 'FK_dbo.crmGroupMember_crmGroupRole_GroupRoleId' " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"sp_rename '[FK_dbo.cmsAuth_crmGroup_GroupId]','FK_dbo.cmsAuth_groupsGroup_GroupId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupLocation_coreDefinedValue_LocationTypeId]','FK_dbo.groupGroupLocation_dbo.coreDefinedValue_LocationTypeId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupLocation_crmLocation_LocationId]','FK_dbo.groupGroupLocation_dbo.crmLocation_LocationId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupLocation_crmGroup_GroupId]','FK_dbo.groupGroupLocation_dbo.groupsGroup_GroupId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroup_crmCampus_CampusId]','FK_dbo.groupsGroup_dbo.crmCampus_CampusId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupRole_crmGroupType_GroupTypeId]','FK_dbo.groupsGroupRole_dbo.groupsGroupType_GroupTypeId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroup_crmGroup_ParentGroupId]','FK_dbo.groupsGroup_groupsGroup_ParentGroupId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroup_crmGroupType_GroupTypeId]','FK_dbo.groupsGroup_groupsGroupType_GroupTypeId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupType_crmGroupRole_DefaultGroupRoleId]','FK_dbo.groupsGroupType_groupsGroupRole_DefaultGroupRoleId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupTypeAssociation_crmGroupType_ChildGroupTypeId]','FK_dbo.groupsGroupTypeAssociation_groupsGroupType_ChildGroupTypeId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupTypeAssociation_crmGroupType_ParentGroupTypeId]','FK_dbo.groupsGroupTypeAssociation_groupsGroupType_ParentGroupTypeId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupMember_crmPerson_PersonId]','FK_dbo.groupsMember_crmPerson_PersonId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupMember_crmGroup_GroupId]','FK_dbo.groupsMember_groupsGroup_GroupId' " );
            Sql( @"sp_rename '[FK_dbo.crmGroupMember_crmGroupRole_GroupRoleId]','FK_dbo.groupsMember_groupsGroupRole_GroupRoleId' " );
        }
    }
}
