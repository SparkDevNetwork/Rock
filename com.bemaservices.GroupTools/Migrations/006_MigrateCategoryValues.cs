using Rock.Plugin;

namespace com.bemaservices.GroupTools
{
    [MigrationNumber( 6, "1.9.4" )]
    public class MigrateCategoryValues : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var groupTypeGuid = "50FCFB30-F51A-49DF-86F4-2B176EA1820B";
            var lifeGroupTypeGuidObject = SqlScalar( "Select Top 1 [Guid] From GroupType Where Guid = 'a4f16049-2525-426e-a6e8-cdfb7b198664'" );
            if ( lifeGroupTypeGuidObject != null )
            {
                groupTypeGuid = lifeGroupTypeGuidObject.ToString();
            }

            Sql( string.Format( @"Declare @GroupTypeGuid uniqueidentifier = '{0}'

Declare @OldAttributeKey nvarchar(max) = 'ClassCategory'
Declare @NewAttributeKey nvarchar(max) = 'Category'

Declare @DefinedTypeGuid uniqueidentifier = '8BF1BECD-9B0C-4689-8204-8405327DCBCF'
Declare @AttributeId int = (Select top 1 a.Id
					From Attribute a
					Join GroupType gt on convert(nvarchar(max),gt.Id) = a.EntityTypeQualifierValue
					Where a.EntityTypeQualifierColumn = 'GroupTypeId'
					And a.[Key] = @NewAttributeKey
					and gt.Guid = @GroupTypeGuid)



Declare @ValueTable table(
	GroupId int,
	CurrentValue nvarchar(max),
	NewValue nvarchar(max)
	)
Insert Into @ValueTable
Select g.Id, String_agg(splitValues.value, ',') as OriginalValue, String_agg(convert(nvarchar(max),dv.Guid),',') as NewValue
From [Group] g
Join GroupType gt on gt.Id = g.GroupTypeId
Join AttributeValue av on av.EntityId = g.Id
Join Attribute a on a.Id = av.AttributeId and a.[Key] = @OldAttributeKey
Cross Apply string_split(av.Value, ',') as splitValues
Join DefinedValue dv on dv.Value = splitValues.value
Join DefinedType dt on dt.Id = dv.DefinedTypeId and dt.Guid = @DefinedTypeGuid
Where gt.Guid = @GroupTypeGuid
Group by g.Id

INSERT INTO [dbo].[AttributeValue]
           ([IsSystem]
           ,[AttributeId]
           ,[EntityId]
           ,[Value]
           ,[Guid])
     Select
           0
           ,@AttributeId
           , vt.GroupId
           ,vt.NewValue
           ,newId()
	From @ValueTable vt
", groupTypeGuid ) );
        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}

