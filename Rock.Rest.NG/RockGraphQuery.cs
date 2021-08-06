using System;
using System.Linq;

using HotChocolate;

namespace Rock.Rest
{
    /// <summary>
    /// The root query object for the Rock GraphQL API.
    /// </summary>
    [GraphQueryRoot]
    public class RockRestQueryRoot
    {
        public IQueryable<Rock.Model.Group> GetGroup( [Service] Rock.Data.RockContext rockContext )
        {
            return new Rock.Model.GroupService( rockContext ).Queryable();
        }

        public IQueryable<Rock.Model.GroupType> GetGroupType( [Service] Rock.Data.RockContext rockContext )
        {
            return new Rock.Model.GroupTypeService( rockContext ).Queryable();
        }

        public IQueryable<Rock.Model.Person> GetPerson( [Service] Rock.Data.RockContext rockContext )
        {
            return new Rock.Model.PersonService( rockContext ).Queryable();
        }
    }

    [GraphQueryRoot]
    public class TestQueryRoot
    {
        public DateTime CurrentDateTime => RockDateTime.Now;
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class GraphQueryRootAttribute : System.Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class GraphQueryTypeAttribute : System.Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field )]
    public class GraphQueryIncludeAttribute : System.Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field )]
    public class GraphQueryExcludeAttribute : System.Attribute
    {
    }

}
