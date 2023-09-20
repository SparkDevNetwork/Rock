using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace org.lakepointe.Checkin.Migrations
{
    [MigrationNumber(2, "1.9.4.1")]
    class MigrateGradeOffsetToUnassignedGroupGrade : Migration
    {
        #region Public Methods
        public override void Up()
        {
  
            //MigrateUnassignedGrade("EE3E9946-423A-4577-80D9-B092477AC087".AsGuid());
            //MigrateUnassignedGrade("4385DA6F-816C-4F2B-8A6F-F67F4E14DF9C".AsGuid());
            MigrateUnassignedGrade("FB2C4CC4-B221-4156-8031-C65E2296EC96".AsGuid());
        }


        public override void Down()
        {

        }
        #endregion

        #region Private Methods
        private void MigrateUnassignedGrade(Guid gradeOffsetAttributeGuid)
        {
            var rockContext = new RockContext();

            var groupIds = rockContext.Database.SqlQuery<int>(@"
                SELECT g.[Id]
                FROM [dbo].[Attribute] a 
                INNER JOIN [dbo].[AttributeValue] av on a.[Id] = av.[AttributeId]
                INNER JOIN [dbo].[Group] g on av.[EntityId] = g.[Id]
                WHERE a.[Guid] = @Guid
	                and av.[Value] <> '' ",
                new System.Data.SqlClient.SqlParameter("@Guid", gradeOffsetAttributeGuid)
                ).ToList();
            DefinedTypeCache schoolGrades = DefinedTypeCache.Get(Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid());


            foreach (var groupId in groupIds)
            {
                using (var groupContext = new RockContext())
                {
                    var group = new GroupService(groupContext).Get(groupId);
                    group.LoadAttributes();

                    var gradeOffsetAttributeValue = group.GetAttributeValue("GradeOffset");

                    var gradeOffsetDefinedValue = schoolGrades.DefinedValues.Where(v => v.Value == gradeOffsetAttributeValue).SingleOrDefault();

                    if (gradeOffsetDefinedValue != null)
                    {
                        var gradeRangeAttributeValue = string.Format("{0},{1}", gradeOffsetDefinedValue.Guid, gradeOffsetDefinedValue.Guid);
                        group.SetAttributeValue("UnassignedGroupGradeRange", gradeRangeAttributeValue);
                        group.SaveAttributeValue("UnassignedGroupGradeRange", groupContext); 
                    }                  
                }
            }
        }
        #endregion

    }
}
