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

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Rock.CheckIn.v2.Labels;
using Rock.CheckIn.v2.Labels.Renderers;
using Rock.Cms.StructuredContent.BlockTypes;
using Rock.Data;
using Rock.Enums.CheckIn.Labels;
using Rock.Model;
using Rock.Observability;
using Rock.SystemKey;
using Rock.Utility;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.CheckIn.Labels;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Provides logic for generating labels for a set of attendance records.
    /// </summary>
    internal class DefaultLabelProvider
    {
        #region Properties

        /// <summary>
        /// Gets or sets the context to use when accessing the database.
        /// </summary>
        protected RockContext RockContext { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLabelProvider"/> class.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="rockContext"/> is <see langword="null"/>.</exception>
        public DefaultLabelProvider( RockContext rockContext )
        {
            RockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Renders all the labels for check-in operation result. This will
        /// also add any print errors to <paramref name="checkInResult"/>.
        /// </summary>
        /// <param name="checkInResult">The attendance records to render labels for.</param>
        /// <param name="kiosk">The kiosk requesting the print or <see langword="null"/> if not known.</param>
        /// <param name="printProvider">The instance that will handle sending data to the physical printers.</param>
        /// <param name="cancellationToken">A token that will be triggered if the operation should be aborted.</param>
        /// <returns>A list of <see cref="RenderedLabel"/> objects that should be printed on the client.</returns>
        public Task<List<RenderedLabel>> RenderAndPrintCheckInLabelsAsync( CheckInResultBag checkInResult, DeviceCache kiosk, LabelPrintProvider printProvider, CancellationToken cancellationToken = default )
        {
            List<RenderedLabel> labels;

            using ( var activity = ObservabilityHelper.StartActivity( "Render Labels" ) )
            {
                activity?.AddTag( "rock.checkin.print_provider", GetType().FullName );

                labels = RenderLabels( checkInResult.Attendances, kiosk, false );
            }

            using ( var activity = ObservabilityHelper.StartActivity( "Print Labels" ) )
            {
                activity?.AddTag( "rock.checkin.print_provider", GetType().FullName );

                return PrintLabelsAsync( labels, kiosk, printProvider, cancellationToken, msg =>
                        checkInResult.Messages.Add( msg ) );
            }
        }

        /// <summary>
        /// Renders all the labels for check-out operation. This will
        /// also add any print errors to <paramref name="checkOutResult"/>.
        /// </summary>
        /// <param name="checkOutResult">The result of the checkout operation to add additional messages to.</param>
        /// <param name="kiosk">The kiosk requesting the print or <see langword="null"/> if not known.</param>
        /// <param name="printProvider">The instance that will handle sending data to the physical printers.</param>
        /// <param name="cancellationToken">A token that will be triggered if the operation should be aborted.</param>
        /// <returns>A list of <see cref="RenderedLabel"/> objects that should be printed on the client.</returns>
        public Task<List<RenderedLabel>> RenderAndPrintCheckoutLabelsAsync( CheckoutResultBag checkOutResult, DeviceCache kiosk, LabelPrintProvider printProvider, CancellationToken cancellationToken = default )
        {
            List<RenderedLabel> labels;

            using ( var activity = ObservabilityHelper.StartActivity( "Render Labels" ) )
            {
                activity?.AddTag( "rock.checkin.print_provider", GetType().FullName );

                var attendanceIds = checkOutResult.Attendances
                    .Select( a => IdHasher.Instance.GetId( a.Id ) )
                    .Where( a => a.HasValue )
                    .Select( a => a.Value )
                    .ToList();
                labels = RenderLabels( attendanceIds, kiosk, checkout: true );
            }

            using ( var activity = ObservabilityHelper.StartActivity( "Print Labels" ) )
            {
                activity?.AddTag( "rock.checkin.print_provider", GetType().FullName );

                return PrintLabelsAsync( labels, kiosk, printProvider, cancellationToken, msg =>
                    checkOutResult.Messages.Add( msg ) );
            }
        }

        /// <summary>
        /// Renders all the labels for check-out operation.
        /// </summary>
        /// <param name="labels">The rendered labels that should be printed.</param>
        /// <param name="kiosk">The kiosk requesting the print or <see langword="null"/> if not known.</param>
        /// <param name="printProvider">The instance that will handle sending data to the physical printers.</param>
        /// <param name="cancellationToken">A token that will be triggered if the operation should be aborted.</param>
        /// <param name="messageCallback">The callback when a print related message needs to be recorded.</param>
        /// <returns>A list of <see cref="RenderedLabel"/> objects that should be printed on the client.</returns>
        private async Task<List<RenderedLabel>> PrintLabelsAsync( List<RenderedLabel> labels, DeviceCache kiosk, LabelPrintProvider printProvider, CancellationToken cancellationToken, Action<string> messageCallback )
        {
            // Add any error messages from labels that failed to render.
            var errorMessages = labels
                .Where( l => l.Error.IsNotNullOrWhiteSpace() )
                .Select( l => l.Error );

            foreach ( var msg in errorMessages )
            {
                messageCallback( msg );
            }

            // Print the labels and wait for completion.
            var labelsToPrint = labels
                .Where( l => l.Error.IsNullOrWhiteSpace() && l.Data != null && l.PrintFrom == PrintFrom.Server );

            try
            {
                var printErrorMessages = await printProvider.PrintLabelsAsync( labelsToPrint, cancellationToken );

                // Add any print failure messages.
                if ( printErrorMessages != null )
                {
                    foreach ( var msg in printErrorMessages )
                    {
                        messageCallback( msg );
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                messageCallback( "Timeout waiting for labels to print." );
            }

            // Return the labels that should be printed on the client.
            return labels.Where( l => l.Error.IsNullOrWhiteSpace()
                && l.Data != null
                && l.PrintFrom == PrintFrom.Client )
                .ToList();
        }

        /// <summary>
        /// Renders all the labels for the set of attendance records given.
        /// </summary>
        /// <param name="allRecordedAttendance">The attendance records to render labels for.</param>
        /// <param name="kiosk">The kiosk requesting the print or <see langword="null"/> if not known.</param>
        /// <param name="checkout"><c>true</c> if the labels to be rendered are for a checkout operation; otherwise <c>false</c>.</param>
        /// <returns>A list of <see cref="RenderedLabel"/> objects that contain all the information required to print the labels.</returns>
        public List<RenderedLabel> RenderLabels( List<RecordedAttendanceBag> allRecordedAttendance, DeviceCache kiosk, bool checkout )
        {
            var attendanceIds = allRecordedAttendance
                .Select( a => IdHasher.Instance.GetId( a.Attendance.Id ) )
                .Where( a => a.HasValue )
                .Select( a => a.Value )
                .ToList();
            var allAttendanceQry = GetAttendanceQuery();

            allAttendanceQry = CheckInDirector.WhereContains( allAttendanceQry, attendanceIds, a => a.Id );

            var allAttendance = allAttendanceQry.ToList();

            var attendanceLabels = allAttendance
                .Select( a => new LabelAttendanceDetail( a, allRecordedAttendance.First( ra => ra.Attendance.Id == a.IdKey), RockContext ) )
                .Where( a => a.Area != null && a.Group != null && a.Location != null && a.Schedule != null )
                .ToList();

            if ( !attendanceLabels.Any() )
            {
                return new List<RenderedLabel>();
            }

            attendanceLabels.Select( a => a.Person )
                .Where( p => p.Attributes == null )
                .DistinctBy( p => p.Id )
                .LoadAttributes( RockContext );

            var sessionFamily = allAttendance.Where( a => a.SearchResultGroupId.HasValue ).FirstOrDefault()?.SearchResultGroup;

            return !checkout
                ? RenderCheckInLabels( attendanceLabels, sessionFamily, kiosk )
                : RenderCheckOutLabels( attendanceLabels, sessionFamily, kiosk );
        }

        /// <summary>
        /// Renders all the labels for the set of existing attendance records.
        /// </summary>
        /// <param name="attendanceIds">The identifiers of the <see cref="Attendance"/> records to generate labels for.</param>
        /// <param name="kiosk">The kiosk requesting the print or <see langword="null"/> if not known.</param>
        /// <param name="checkout"><c>true</c> if the labels to be rendered are for a checkout operation; otherwise <c>false</c>.</param>
        /// <returns>A list of <see cref="RenderedLabel"/> objects that contain all the information required to print the labels.</returns>
        public List<RenderedLabel> RenderLabels( List<int> attendanceIds, DeviceCache kiosk, bool checkout )
        {
            var allAttendanceQry = GetAttendanceQuery();

            allAttendanceQry = CheckInDirector.WhereContains( allAttendanceQry, attendanceIds, a => a.Id );

            return RenderLabels( allAttendanceQry.ToList(), kiosk, checkout );
        }

        /// <summary>
        /// Renders all the labels for the set of existing attendance records.
        /// </summary>
        /// <param name="allAttendance">The <see cref="Attendance"/> records to generate labels for.</param>
        /// <param name="kiosk">The kiosk requesting the print or <see langword="null"/> if not known.</param>
        /// <param name="checkout"><c>true</c> if the labels to be rendered are for a checkout operation; otherwise <c>false</c>.</param>
        /// <returns>A list of <see cref="RenderedLabel"/> objects that contain all the information required to print the labels.</returns>
        private List<RenderedLabel> RenderLabels( List<Attendance> allAttendance, DeviceCache kiosk, bool checkout )
        {
            var attendanceLabels = allAttendance
                .Select( a => new LabelAttendanceDetail( a, RockContext ) )
                .Where( a => a.Area != null && a.Group != null && a.Location != null && a.Schedule != null )
                .ToList();

            if ( !attendanceLabels.Any() )
            {
                return new List<RenderedLabel>();
            }

            attendanceLabels.Select( a => a.Person )
                .Where( p => p.Attributes == null )
                .DistinctBy( p => p.Id )
                .LoadAttributes( RockContext );

            var sessionFamily = allAttendance.Where( a => a.SearchResultGroupId.HasValue ).FirstOrDefault()?.SearchResultGroup;

            return !checkout
                ? RenderCheckInLabels( attendanceLabels, sessionFamily, kiosk )
                : RenderCheckOutLabels( attendanceLabels, sessionFamily, kiosk );
        }

        /// <summary>
        /// Renders all the labels for the set of attendance records given.
        /// </summary>
        /// <param name="attendanceLabels">All attendance records to render labels for.</param>
        /// <param name="sessionFamily">The family that was matched during the check-in operation, may be <see langword="null"/>.</param>
        /// <param name="kiosk">The kiosk requesting the print or <see langword="null"/> if not known.</param>
        /// <returns>A list of <see cref="RenderedLabel"/> objects that contain all the information required to print the labels.</returns>
        private List<RenderedLabel> RenderCheckInLabels( List<LabelAttendanceDetail> attendanceLabels, Group sessionFamily, DeviceCache kiosk )
        {
            if ( attendanceLabels == null || attendanceLabels.Count == 0 )
            {
                return new List<RenderedLabel>();
            }

            var areaIds = attendanceLabels.Select( a => a.Area.Id ).Distinct().ToList();
            var groupTypeLabels = GetOrderedLabels( areaIds );

            // Get all the labels that will be printed for families.
            var familyLabelsToPrint = groupTypeLabels
                .Where( gtl => gtl.CheckInLabel.LabelType == LabelType.Family )
                .DistinctBy( gtl => gtl.CheckInLabel.Id )
                .OrderBy( gtl => gtl.Order )
                .ThenBy( gtl => gtl.CheckInLabel.Id )
                .ToList();

            // Get all the labels that will be printed for each person.
            var personLabelsToPrint = groupTypeLabels
                .Where( gtl => gtl.CheckInLabel.LabelType == LabelType.Person )
                .DistinctBy( gtl => new
                {
                    gtl.AreaId,
                    LabelId = gtl.CheckInLabel.Id
                } )
                .OrderBy( gtl => gtl.Order )
                .ThenBy( gtl => gtl.CheckInLabel.Id )
                .ToList();

            // Get all the labels that will be printed for each person and
            // location combination.
            var personLocationLabelsToPrint = groupTypeLabels
                .Where( gtl => gtl.CheckInLabel.LabelType == LabelType.PersonLocation )
                .DistinctBy( gtl => new
                {
                    gtl.AreaId,
                    LabelId = gtl.CheckInLabel.Id
                } )
                .OrderBy( gtl => gtl.Order )
                .ThenBy( gtl => gtl.CheckInLabel.Id )
                .ToList();

            // Get all the labels that will be printed for each attendance.
            // We don't distinct because we are allowed to print the same
            // label multiple times.
            var attendanceLabelsToPrint = groupTypeLabels
                .Where( gtl => gtl.CheckInLabel.LabelType == LabelType.Attendance )
                .OrderBy( gtl => gtl.Order )
                .ThenBy( gtl => gtl.CheckInLabel.Id )
                .ToList();

            var labels = new List<RenderedLabel>();
            var personIds = attendanceLabels.Select( a => a.Person.Id ).Distinct();

            // Print all family labels first.
            labels.AddRange( RenderLabels( familyLabelsToPrint,
                attendanceLabels,
                attendanceLabels,
                kiosk,
                sessionFamily ) );

            // Now print person and attendance labels, grouped by person.
            foreach ( var personId in personIds )
            {
                var attendanceLabelsForPerson = attendanceLabels.Where( a => a.Person.Id == personId ).ToList();

                // Print labels that get printed once per person.
                labels.AddRange( RenderLabels( personLabelsToPrint,
                    attendanceLabelsForPerson,
                    attendanceLabels,
                    kiosk,
                    sessionFamily,
                    preventDuplicateLabels: true ) );

                // Print labels that get printed once per location for the
                // person.
                var attendanceLabelsByLocations = attendanceLabelsForPerson
                    .GroupBy( a => a.Location.Id );
                foreach ( var attendanceLabelsByLocation in attendanceLabelsByLocations )
                {
                    labels.AddRange( RenderLabels( personLocationLabelsToPrint,
                        attendanceLabelsByLocation,
                        attendanceLabels,
                        kiosk,
                        sessionFamily,
                        preventDuplicateLabels: true ) );
                }

                // Print labels that get printed for every attendance record.
                foreach ( var personLabel in attendanceLabelsForPerson )
                {
                    labels.AddRange( RenderLabels( attendanceLabelsToPrint,
                        new List<LabelAttendanceDetail> { personLabel },
                        attendanceLabels,
                        kiosk,
                        sessionFamily ) );
                }
            }

            return labels;
        }

        /// <summary>
        /// Renders all the labels for the set of attendance records given.
        /// </summary>
        /// <param name="attendanceLabels">All attendance records to render labels for.</param>
        /// <param name="sessionFamily">The family that was matched during the check-in operation, may be <see langword="null"/>.</param>
        /// <param name="kiosk">The kiosk requesting the print or <see langword="null"/> if not known.</param>
        /// <returns>A list of <see cref="RenderedLabel"/> objects that contain all the information required to print the labels.</returns>
        private List<RenderedLabel> RenderCheckOutLabels( List<LabelAttendanceDetail> attendanceLabels, Group sessionFamily, DeviceCache kiosk )
        {
            if ( attendanceLabels == null || attendanceLabels.Count == 0 )
            {
                return new List<RenderedLabel>();
            }

            var areaIds = attendanceLabels.Select( a => a.Area.Id ).Distinct().ToList();
            var groupTypeLabels = GetOrderedLabels( areaIds );

            // Get all the labels that will be printed.
            var checkoutLabelsToPrint = groupTypeLabels
                .Where( gtl => gtl.CheckInLabel.LabelType == LabelType.Checkout )
                .OrderBy( gtl => gtl.Order )
                .ThenBy( gtl => gtl.CheckInLabel.Id )
                .ToList();

            var labels = new List<RenderedLabel>();

            // Print labels that get printed for every attendance record.
            foreach ( var attendanceLabel in attendanceLabels )
            {
                labels.AddRange( RenderLabels( checkoutLabelsToPrint,
                    new LabelAttendanceDetail[] { attendanceLabel },
                    attendanceLabels,
                    kiosk,
                    sessionFamily ) );
            }

            return labels;
        }

        /// <summary>
        /// Gets the labels to be printed for the specified area identifiers.
        /// The returned object contains the AreaId and the Label referenced
        /// by that area. This means a single <see cref="CheckInLabel"/> could
        /// be included in the list more than once if two areas point to the
        /// same label.
        /// </summary>
        /// <param name="areaIds">The area identifies to query for labels.</param>
        /// <returns>A list of check-in labels in print order.</returns>
        private List<OrderedAreaLabel> GetOrderedLabels( List<int> areaIds )
        {
            var groupTypeEntityTypeId = EntityTypeCache.Get<GroupType>( true, RockContext ).Id;
            var checkInLabelEntityTypeId = EntityTypeCache.Get<Model.CheckInLabel>( true, RockContext ).Id;

            var relatedEntityQry = new RelatedEntityService( RockContext )
                .Queryable()
                .Where( a => a.SourceEntityTypeId == groupTypeEntityTypeId
                    && a.TargetEntityTypeId == checkInLabelEntityTypeId
                    && a.PurposeKey == RelatedEntityPurposeKey.AreaCheckInLabel );

            relatedEntityQry = CheckInDirector.WhereContains( relatedEntityQry, areaIds, a => a.SourceEntityId );

            // TODO: This data needs to be cached on GroupTypeCache somehow,
            // it consumes about 40% of the total processing time.
            return new CheckInLabelService( RockContext )
                .Queryable()
                .Join( relatedEntityQry, cl => cl.Id, re => re.TargetEntityId, ( cl, re ) => new OrderedAreaLabel
                {
                    AreaId = re.SourceEntityId,
                    CheckInLabel = cl,
                    Order = re.Order
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the base query to use for retrieving attendance records. This
        /// will have all the proper navigation properties included.
        /// </summary>
        /// <returns>A queryable for <see cref="Attendance"/> records.</returns>
        private IQueryable<Attendance> GetAttendanceQuery()
        {
            return new AttendanceService( RockContext )
                .Queryable()
                .Include( a => a.Occurrence )
                .Include( a => a.AttendanceCode )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.SearchResultGroup );
        }

        /// <summary>
        /// Renders a single label and returns a reference to the data that
        /// should be sent to the printer.
        /// </summary>
        /// <param name="label">The label to be rendered.</param>
        /// <param name="attendanceLabel">The primary attendance record this label is being printed for, may be <see langword="null"/>.</param>
        /// <param name="attendanceLabels">All attendance records related to this check-in session.</param>
        /// <param name="sessionFamily">The family that was matched during the check-in operation, may be <see langword="null"/>.</param>
        /// <param name="printer">The device that the label will be sent to, may be <see langword="null"/>.</param>
        /// <returns>A new instance of <see cref="RenderedLabel"/> that contains either the data to be printed or an error message, will be <see langword="null"/> if the label conditions prevent rendering.</returns>
        public RenderedLabel RenderLabel( Rock.Model.CheckInLabel label, LabelAttendanceDetail attendanceLabel, List<LabelAttendanceDetail> attendanceLabels, Group sessionFamily, DeviceCache printer )
        {
            var labelData = GetLabelData( label.LabelType, attendanceLabel, attendanceLabels, sessionFamily );

            var filter = label.GetConditionalPrintCriteria();
            var builder = new Reporting.FieldFilterExpressionBuilder();
            var fn = builder.GetIsMatchFunction( filter, labelData.GetType() );

            if ( !fn( labelData ) )
            {
                return null;
            }

            return RenderLabel( label, labelData, printer );
        }

        /// <summary>
        /// Renders a single label and returns a reference to the data that
        /// should be sent to the printer. This will render the label even if
        /// it has conditional display filters that would normally prevent it.
        /// </summary>
        /// <param name="label">The label to be rendered.</param>
        /// <param name="attendanceLabel">The primary attendance record this label is being printed for, may be <see langword="null"/>.</param>
        /// <param name="attendanceLabels">All attendance records related to this check-in session.</param>
        /// <param name="sessionFamily">The family that was matched during the check-in operation, may be <see langword="null"/>.</param>
        /// <param name="printer">The device that the label will be sent to, may be <see langword="null"/>.</param>
        /// <returns>A new instance of <see cref="RenderedLabel"/> that contains either the data to be printed or an error message, will be <see langword="null"/> if the label conditions prevent rendering.</returns>
        public RenderedLabel RenderLabelUnconditionally( Rock.Model.CheckInLabel label, LabelAttendanceDetail attendanceLabel, List<LabelAttendanceDetail> attendanceLabels, Group sessionFamily, DeviceCache printer )
        {
            var people = new List<Person>( attendanceLabels.Count + 1 );

            people.AddRange( attendanceLabels.Where( a => a.Person != null ).Select( a => a.Person ) );

            if ( attendanceLabel.Person != null )
            {
                people.Add( attendanceLabel.Person );
            }

            if ( people.Count > 0 )
            {
                people.Where( p => p.Attributes == null )
                    .DistinctBy( p => p.Id )
                    .LoadAttributes( RockContext );
            }


            var labelData = GetLabelData( label.LabelType, attendanceLabel, attendanceLabels, sessionFamily );

            return RenderLabel( label, labelData, printer );
        }

        /// <summary>
        /// Renders a single label and returns a reference to the data that
        /// should be sent to the printer.
        /// </summary>
        /// <param name="label">The label to be rendered.</param>
        /// <param name="labelData">The label data to use when rendering.</param>
        /// <param name="printer">The device that the label will be sent to, may be <see langword="null"/>.</param>
        /// <returns>A new instance of <see cref="RenderedLabel"/> that contains either the data to be printed or an error message, will never be <see langword="null"/>.</returns>
        private RenderedLabel RenderLabel( Rock.Model.CheckInLabel label, object labelData, DeviceCache printer )
        {
            using ( var activity = ObservabilityHelper.StartActivity( label.Name ) )
            {
                if ( label.LabelFormat == LabelFormat.Zpl )
                {
                    var mergeFields = new Dictionary<string, object>();

                    foreach ( var prop in labelData.GetType().GetProperties() )
                    {
                        mergeFields.Add( prop.Name, prop.GetValue( labelData ) );
                    }

                    var zpl = label.Content.ResolveMergeFields( mergeFields );

                    return new RenderedLabel
                    {
                        Data = Encoding.UTF8.GetBytes( zpl ),
                        PrintTo = printer
                    };
                }

                // It is a designed label. Try to get the label data and if we
                // can't then return an error.
                var designedLabel = label.Content.FromJsonOrNull<DesignedLabelBag>();

                if ( designedLabel == null )
                {
                    return new RenderedLabel
                    {
                        Error = "Invalid label data."
                    };
                }

                var hasCutter = printer?.GetAttributeValue( DeviceAttributeKey.DEVICE_HAS_CUTTER ).AsBoolean() ?? false;

                var printRequest = new PrintLabelRequest
                {
                    Capabilities = new PrinterCapabilities
                    {
                        IsCutterSupported = hasCutter
                    },
                    RockContext = RockContext,
                    LabelData = labelData,
                    DataSources = FieldSourceHelper.GetCachedDataSources( label.LabelType ),
                    Label = designedLabel
                };

                var renderer = new ZplLabelRenderer();

                using ( var memoryStream = new MemoryStream() )
                {
                    renderer.BeginLabel( memoryStream, printRequest );

                    foreach ( var field in printRequest.Label.Fields )
                    {
                        var labelField = new LabelField( field );

                        if ( !labelField.IsMatch( labelData ) )
                        {
                            continue;
                        }

                        renderer.WriteField( labelField );
                    }

                    renderer.EndLabel();

                    return new RenderedLabel
                    {
                        LabelId = label.IdKey,
                        LabelName = label.Name,
                        Data = memoryStream.ToArray(),
                        PrintTo = printer
                    };
                }
            }
        }

        /// <summary>
        /// Renders all the labels specified by <paramref name="labelsToPrint"/>.
        /// These will contain all information required to print the labels.
        /// </summary>
        /// <param name="labelsToPrint">The set of label definitions to generate.</param>
        /// <param name="filteredAttendanceLabels">The <see cref="LabelAttendanceDetail"/> objects that have been filtered down for this operation, such as all records for a person.</param>
        /// <param name="attendanceLabels">All attendance data for the check-in session.</param>
        /// <param name="kiosk">The kiosk performing the operation, may be <see langword="null"/>.</param>
        /// <param name="sessionFamily">The family that was matched during the check-in operation, may be <see langword="null"/>.</param>
        /// <param name="preventDuplicateLabels">If <c>true</c> then duplicate <see cref="CheckInLabel"/> instances will be skipped.</param>
        /// <returns>A set of labels to be printed.</returns>
        private IEnumerable<RenderedLabel> RenderLabels( List<OrderedAreaLabel> labelsToPrint, IEnumerable<LabelAttendanceDetail> filteredAttendanceLabels, List<LabelAttendanceDetail> attendanceLabels, DeviceCache kiosk, Group sessionFamily, bool preventDuplicateLabels = false )
        {
            var renderedLabelIds = new List<int>( labelsToPrint.Count );

            foreach ( var label in labelsToPrint )
            {
                if ( preventDuplicateLabels && renderedLabelIds.Contains( label.CheckInLabel.Id ) )
                {
                    continue;
                }

                var attendanceLabel = filteredAttendanceLabels
                    .FirstOrDefault( al => al.Area.Id == label.AreaId );

                // This specific person might not have checked into a group
                // configured for this label.
                if ( attendanceLabel == null )
                {
                    continue;
                }

                renderedLabelIds.Add( label.CheckInLabel.Id );

                var printer = GetPrintToDevice( kiosk, attendanceLabel );
                var labelData = RenderLabel( label.CheckInLabel, attendanceLabel, attendanceLabels, sessionFamily, printer );

                if ( labelData != null )
                {
                    labelData.PrintFrom = kiosk?.PrintFrom ?? PrintFrom.Server;

                    yield return labelData;
                }
            }
        }

        /// <summary>
        /// Get the device to print the label to. This looks at various bits of
        /// information on both the kiosk and the attendance record to determine
        /// where the label should be printed.
        /// </summary>
        /// <param name="kiosk">The kiosk requesting the print or <see langword="null"/> if not known.</param>
        /// <param name="attendance">The attendance record related to the print request.</param>
        /// <returns>The device to send the label to or <see langword="null"/> if unknown.</returns>
        private DeviceCache GetPrintToDevice( DeviceCache kiosk, LabelAttendanceDetail attendance )
        {
            var printTo = kiosk?.PrintToOverride ?? PrintTo.Default;

            if ( printTo == PrintTo.Default )
            {
                printTo = attendance.Area.AttendancePrintTo;
            }

            if ( printTo == PrintTo.Kiosk && kiosk != null && kiosk.PrinterDeviceId.HasValue )
            {
                return DeviceCache.Get( kiosk.PrinterDeviceId.Value, RockContext );
            }
            else if ( printTo == PrintTo.Location && attendance.Location.PrinterDeviceId.HasValue )
            {
                return DeviceCache.Get( attendance.Location.PrinterDeviceId.Value, RockContext );
            }

            return null;
        }

        /// <summary>
        /// Gets the label data object for the specified label type. Each label
        /// type uses a different data object so this helps us construct them
        /// a bit more cleanly.
        /// </summary>
        /// <param name="labelType">The type of label data to build.</param>
        /// <param name="attendance">The primary attendance record for the label.</param>
        /// <param name="allAttendance">All attendance records related to this check-in session.</param>
        /// <param name="family">The family group that was used to search during the check-in session.</param>
        /// <returns>The label data object for the label type or <see langword="null"/> if <paramref name="labelType"/> was not valid.</returns>
        private object GetLabelData( LabelType labelType, LabelAttendanceDetail attendance, List<LabelAttendanceDetail> allAttendance, Group family )
        {
            if ( labelType == LabelType.Family )
            {
                return new FamilyLabelData( family, allAttendance, RockContext );
            }
            else if ( labelType == LabelType.Person )
            {
                return new PersonLabelData( attendance.Person, family, allAttendance, RockContext );
            }
            else if ( labelType == LabelType.Attendance )
            {
                return new AttendanceLabelData( attendance, family, allAttendance, RockContext );
            }
            else if ( labelType == LabelType.Checkout )
            {
                return new CheckoutLabelData( attendance, family, RockContext );
            }
            else if ( labelType == LabelType.PersonLocation )
            {
                return new PersonLocationLabelData( attendance.Person, attendance.Location, allAttendance, RockContext );
            }

            return null;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Helper POCO to pass label data around between various methods.
        /// </summary>
        private class OrderedAreaLabel
        {
            /// <summary>
            /// The area (group type) identifier this label came from.
            /// </summary>
            public int AreaId { get; set; }

            /// <summary>
            /// The check-in label definition.
            /// </summary>
            public Model.CheckInLabel CheckInLabel { get; set; }

            /// <summary>
            /// The order of this label in the area.
            /// </summary>
            public int Order { get; set; }
        }

        #endregion
    }
}
