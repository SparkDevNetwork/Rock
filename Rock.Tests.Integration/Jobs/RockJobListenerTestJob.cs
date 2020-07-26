﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Attribute;
using Rock.Jobs;

namespace Rock.Tests.Integration.Jobs
{
    public enum TestResultType
    {
        Exception,
        MultipleAggregateException,
        SingleAggregateException,
        Warning,
        WarningWithMultipleAggregateException,
        WarningWithSingleAggregateException,
        Success
    }

    public static class TestJobAttributeKey
    {
        /// <summary>
        /// The execution result
        /// </summary>
        public const string ExecutionResult = "ExecutionResult";
        public const string ExecutionMessage = "ExecutionMessage";
    }

    public class RockJobListenerTestJob : IJob
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        

        #endregion Attribute Keys
        public void Execute( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;
            TestResultType? executionResult = ( TestResultType? ) dataMap.GetString( TestJobAttributeKey.ExecutionResult ).AsIntegerOrNull();
            var exceptionMessage = dataMap.GetString( TestJobAttributeKey.ExecutionMessage );

            switch ( executionResult )
            {
                case TestResultType.Exception:
                    throw new Exception( exceptionMessage );
                case TestResultType.MultipleAggregateException:
                    var Exception1 = new Exception( $"{exceptionMessage} 1" );
                    var Exception2 = new Exception( $"{exceptionMessage} 2" );
                    throw new AggregateException( Exception1, Exception2 );
                case TestResultType.SingleAggregateException:
                    var singleException1 = new Exception( exceptionMessage );
                    throw new AggregateException( singleException1 );
                case TestResultType.Warning:
                    throw new RockJobWarningException( exceptionMessage );
                case TestResultType.WarningWithMultipleAggregateException:
                    var WarningException1 = new Exception( $"{exceptionMessage} 3" );
                    var WarningException2 = new Exception( $"{exceptionMessage} 4" );
                    throw new RockJobWarningException( exceptionMessage, new AggregateException( WarningException1, WarningException2 ) );
                case TestResultType.WarningWithSingleAggregateException:
                    var WarningSingleException = new Exception( $"{exceptionMessage} 5" );
                    throw new RockJobWarningException( exceptionMessage, new AggregateException( WarningSingleException ) );

                default:
                    context.Result = exceptionMessage;
                    break;
            }

        }
    }
}
