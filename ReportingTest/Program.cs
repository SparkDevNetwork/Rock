using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock;
using Rock.Reporting;
using System.Diagnostics;
using System.IO;
using Rock.Reporting.DataSelect.Person;

namespace ReportingTest
{
    class Program
    {
        static void Main( string[] args )
        {
            using ( new UnitOfWorkScope() )
            {
                List<DataSelectComponent> reportDataSelectComponents = new List<DataSelectComponent>();

                reportDataSelectComponents.Add( new LastContributionSelect() );
                reportDataSelectComponents.Add( new FamilyNameSelect() );

                ReportExpressionSelector r = new ReportExpressionSelector();

                var reportExpression = r.GetReportSelectExpression( reportDataSelectComponents );

                var personQry = new PersonService().Queryable().Take( 1000 );
                var reportQry = personQry.Select( reportExpression );

                var list = reportQry.AsNoTracking().ToList();

                int count = list.Count();
            }
        }
    }
}
