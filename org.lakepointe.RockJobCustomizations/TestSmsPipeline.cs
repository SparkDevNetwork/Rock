using System;

using Quartz;
using Rock.Data;
using Rock.Attribute;
using Rock.Model;
using Rock.Communication.SmsActions;

namespace org.lakepointe.RockJobCustomizations
{
    [IntegerField(
        name: "SMS Pipeline ID",
        description: "The ID of the SMS Pipeline the message should be sent through.",
        key: "SmsPipelineId",
        required: true,
        order: 0 )]

    [TextField(
        name: "From Number",
        description: "The number the test message should come from. The number must be formatted correctly. (ex: +15705550100).",
        key: "FromNumber",
        required: true,
        order: 1 )]

    [TextField(
        name: "To Number",
        description: "The number the test message should be sent to. The number must be formatted correctly. (ex: +15705550100).",
        key: "ToNumber",
        required: true,
        order: 2 )]

    [MemoField(
        name: "Message",
        description: "The message that the test message should contain.",
        key: "Message",
        required: true,
        order: 3 )]

    [MemoField(
        name: "Expected Response",
        description: "The expected reply. If this does not match the actual reply, an exception will be thrown.",
        key: "ExpectedResponse",
        required: true,
        order: 4 )]

    [DisallowConcurrentExecution]
    public class TestSmsPipeline : IJob
    {
        int _smsPipelineId = 0;
        string _fromNumber = null;
        string _toNumber = null;
        string _message = null;
        string _expectedResponse = null;

        public void Execute( IJobExecutionContext context )
        {
            // Get Parameters
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            _smsPipelineId = dataMap.GetInt( "SmsPipelineId" );
            _fromNumber = dataMap.GetString( "FromNumber" );
            _toNumber = dataMap.GetString( "ToNumber" );
            _message = dataMap.GetString( "Message" );
            _expectedResponse = dataMap.GetString( "ExpectedResponse" );

            // Verify Paramters Have Values
            if ( _smsPipelineId <= 0
                || _fromNumber == null || _fromNumber == ""
                || _toNumber == null || _toNumber == ""
                || _message == null || _message == ""
                || _expectedResponse == null || _expectedResponse == "" )
            {
                throw new ArgumentException( "All required fields must have a value." );
            }

            // Build the Test Message
            var message = new SmsMessage
            {
                FromNumber = _fromNumber,
                ToNumber = _toNumber,
                Message = _message
            };

            if ( message.FromNumber.StartsWith( "+" ) )
            {
                message.FromPerson = new PersonService( new RockContext() ).GetPersonFromMobilePhoneNumber( message.FromNumber.Substring( 1 ), true );
            }
            else
            {
                message.FromPerson = new PersonService( new RockContext() ).GetPersonFromMobilePhoneNumber( message.FromNumber, true );
            }

            // Send the Test Message
            var outcomes = SmsActionService.ProcessIncomingMessage( message, _smsPipelineId );
            var response = SmsActionService.GetResponseFromOutcomes( outcomes );

            // Check the Response Against the Expected Response
            if ( response != null )
            {
                if ( response.Message != _expectedResponse )
                {
                    throw new ArgumentException( $"Response does not match the expected response. \nMessage: \"{_message}\" \nExpected Response: \"{_expectedResponse}\" \nResponse: \"{response.Message}\"" );
                }
            }
            else
            {
                throw new ArgumentNullException( $"No response was recieved. \nMessage: \"{_message}\" \nExpected Response: \"{_expectedResponse}\" \nResponse: null" );
            }

            // No Issues Found
            context.Result = "Response matched the expected response.";
        }
    }
}

