using System.Diagnostics;

using Quartz;

using Rock.Attribute;

namespace Rock.Jobs
{
    [BooleanField("Test Boolean Field")]
    [TextField( "Text Field" )]
    [BooleanField( "Test Boolean Field 2" )]
    [BooleanField( "Test Boolean Field 3" )]
    [BooleanField( "Test Boolean Field 4" )]
    [DisallowConcurrentExecution]
    public class TestObsoleteQuartzPluginJob : IJob
    {
        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            Debug.WriteLine( "Hello World" );
            var dataMap = context.JobDetail.JobDataMap;
            var boolField1 = dataMap.GetString( "TestBooleanField" ).AsBoolean();
            var textField1 = dataMap.GetString( "TextField" );
            var boolField2 = dataMap.GetString( "TestBooleanField2" ).AsBoolean();
            var boolField3 = dataMap.GetString( "TestBooleanField3" ).AsBoolean();
            var boolField4 = dataMap.GetString( "TestBooleanField4" ).AsBoolean();
        }
    }
}
