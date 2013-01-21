using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

using Rock.Model;

namespace Rock.Reporting.Person
{
    public class Gender
    {
        public Expression GetExpression()
        {
            ParameterExpression pe = Expression.Parameter( typeof( Person.Gender ), "Gender" );
            Expression left = Expression.Property( pe, "Gender");
            Expression right = Expression.Constant( Rock.Model.Gender.Male );
            Expression e1 = Expression.Equal( left, right );
            return  Expression.Lambda<Func<Person.Gender, bool>>( e1, pe );
        }
    }
}