// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Labels;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.CheckIn.CheckInKiosk;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Prints next-gen check-in labels for an existing attendance record.
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Prints next-gen check-in labels for an existing attendance record." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Print Labels" )]

    [WorkflowAttribute( "Attendance",
        Description = "The attendance record to print labels for.",
        IsRequired = true,
        FieldTypeClassNames = new[] { "Rock.Field.Types.AttendanceFieldType" },
        Key = AttributeKey.Attendance,
        Order = 0 )]

    [DeviceField( "Kiosk",
        Description = "The kiosk to print the labels for. This is used to determine the default printer and printer settings.  Either Kiosk or Printer must be specified.",
        IsRequired = false,
        DeviceTypeGuid = SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK,
        Key = AttributeKey.Kiosk,
        Order = 1 )]

    [DeviceField( "Printer",
        Description = "Overrides the normal printer that would be used to print the labels on. Either Kiosk or Printer must be specified.",
        IsRequired = false,
        DeviceTypeGuid = SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER,
        Key = AttributeKey.Printer,
        Order = 2 )]

    [BooleanField( "Print Checkout Labels",
        Description = "This will cause the checkout labels to be printed instead of the check-in labels.",
        Key = AttributeKey.PrintCheckoutLabels,
        Order = 3 )]

    [Rock.SystemGuid.EntityTypeGuid( "5978a2d6-7f0f-465e-b469-7d883742c853" )]
    public class PrintLabels : ActionComponent
    {
        private class AttributeKey
        {
            public const string Attendance = "Attendance";

            public const string Kiosk = "Kiosk";

            public const string Printer = "Printer";

            public const string PrintCheckoutLabels = "PrintCheckoutLabels";
        }

        /// <inheritdoc/>
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = null;

            var attendanceGuid = GetAttributeValue( action, AttributeKey.Attendance, true ).AsGuid();
            var kiosk = DeviceCache.Get( GetAttributeValue( action, AttributeKey.Kiosk ).AsGuid(), rockContext );
            var printer = DeviceCache.Get( GetAttributeValue( action, AttributeKey.Printer ).AsGuid(), rockContext );
            var checkout = GetAttributeValue( action, AttributeKey.PrintCheckoutLabels ).AsBoolean();

            var director = new CheckInDirector( rockContext );
            var attendanceId = new AttendanceService( rockContext ).GetId( attendanceGuid );

            if ( kiosk == null && printer == null )
            {
                errorMessages = new List<string> { "At least one of kiosk or printer must be specified." };
                return false;
            }

            if ( !attendanceId.HasValue )
            {
                errorMessages = new List<string> { "The specified attendance record could not be found." };
                return false;
            }

            var task = Task.Run( async () =>
            {
                return await PrintLabelsForAttendanceId( director, kiosk, printer, attendanceId.Value, checkout );
            } );

            // This is not ideal, but is a dependable way to wait for the
            // asynchronous task to complete within this synchronous context.
            while ( !task.IsCompleted )
            {
                Thread.Sleep( 50 );
            }

            // When completed, the task will be in one of the three final states:
            // RanToCompletion, Faulted, or Canceled.
            if ( task.IsFaulted )
            {
                errorMessages = new List<string>
                {
                    "An error occurred while printing labels: " + task.Exception?.GetBaseException().Message
                };

                return false;
            }

            errorMessages = task.Result;

            return true;
        }

        /// <summary>
        /// Prints the labels for the specified attendance identifier.
        /// </summary>
        /// <param name="director">The instance handling the check-in process.</param>
        /// <param name="kiosk">The kiosk that we will be printing labels for.</param>
        /// <param name="printer">The printer that will be used as an override for where to print if not <c>null</c>.</param>
        /// <param name="attendanceId">The attendance identifier to print labels for.</param>
        /// <param name="checkout">If <c>true</c> then the checkout labels will be printed instead of the check-in labels.</param>
        /// <returns>An instance of <see cref="PrintResponseBag"/> that contains the result of the operation.</returns>
        private async Task<List<string>> PrintLabelsForAttendanceId( CheckInDirector director, DeviceCache kiosk, DeviceCache printer, int attendanceId, bool checkout )
        {
            var labels = director.LabelProvider.RenderLabels( new List<int> { attendanceId }, kiosk, printer, checkout );

            if ( !labels.Any() )
            {
                Logger.LogInformation( "No labels were rendered for attendance ID {AttendanceId}.", attendanceId );
                return null;
            }

            var errorMessages = labels.Where( l => l.Error.IsNotNullOrWhiteSpace() )
                .Select( l => l.Error )
                .ToList();

            labels = labels.Where( l => l.Error.IsNullOrWhiteSpace() ).ToList();

            // Print the labels with a 5 second timeout.
            var cts = new CancellationTokenSource( 5_000 );
            var printProvider = new LabelPrintProvider();

            try
            {
                // Print all labels, even if they were set as client labels.
                var serverLabels = labels;
                var printerErrors = await printProvider.PrintLabelsAsync( serverLabels, cts.Token );

                Logger.LogInformation( "Printed {LabelCount} labels for attendance ID {AttendanceId}.", serverLabels.Count, attendanceId );

                errorMessages.AddRange( printerErrors );
            }
            catch ( TaskCanceledException ) when ( cts.IsCancellationRequested )
            {
                errorMessages.Add( "Timeout waiting for labels to print." );
            }

            return errorMessages;
        }
    }
}
