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
using System.ComponentModel;
using System.IO;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Labels;
using Rock.CheckIn.v2.Labels.Renderers;
using Rock.Data;
using Rock.Enums.CheckIn.Labels;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.ViewModels.Blocks.CheckIn.Configuration.LabelDesigner;
using Rock.ViewModels.CheckIn.Labels;
using Rock.ViewModels.Reporting;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// Designs a check-in label with a nice drag and drop experience.
    /// </summary>

    [DisplayName( "Label Designer" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Designs a check-in label with a nice drag and drop experience." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3f477b52-6062-4af4-abb7-b8c153f6242a" )]
    [Rock.SystemGuid.BlockTypeGuid( "8c4ad18f-9f81-4145-8ad0-ab90e451d0d6" )]
    public class LabelDesigner : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The list of page parameters we expect.
        /// </summary>
        private static class PageParameterKey
        {
            public const string CheckInLabelId = "CheckInLabelId";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var label = new CheckInLabelService( RockContext ).Get( PageParameter( PageParameterKey.CheckInLabelId ), !RequestContext.Page.Layout.Site.DisablePredictableIds );

            if ( label == null )
            {
                return new { };
            }

            var dataSources = FieldSourceHelper.GetDataSources( label?.LabelType ?? LabelType.Family )
                .Select( ds => ToDataSourceBag( ds ) )
                .OrderByDescending( ds => ds.Category == "Common" )
                .ThenByDescending( ds => ds.Category.Contains( "Properties" ) )
                .ThenBy( ds => ds.Category )
                .ThenBy( ds => ds.Name )
                .ToList();

            var filterSources = FieldSourceHelper.GetFilterSources( label?.LabelType ?? LabelType.Family )
                .OrderByDescending( s => s.Category == "Common" )
                .ThenBy( s => s.Category )
                .ThenBy( s => s.Property?.Title ?? s.Attribute?.Name )
                .ToList();

            var returnUrl = this.GetParentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.CheckInLabelId] = label.IdKey
            } );

            return new LabelDesignerOptionsBag
            {
                IdKey = label.IdKey,
                IsSystem = label.IsSystem,
                Label = GetLabelDetailBag( label ),
                LabelName = label.Name,
                LabelType = label.LabelType,
                DataSources = dataSources,
                FilterSources = filterSources,
                Icons = GetIconList(),
                ReturnUrl = returnUrl
            };
        }

        /// <summary>
        /// Converts a field data source into an object that will be understood
        /// by the Obsidian client.
        /// </summary>
        /// <param name="dataSource">The data source to be converted.</param>
        /// <returns>A new instance of <see cref="DataSourceBag"/>.</returns>
        private DataSourceBag ToDataSourceBag( FieldDataSource dataSource )
        {
            return new DataSourceBag
            {
                Key = dataSource.Key,
                Name = dataSource.Name,
                TextSubType = dataSource.TextSubType,
                IsCollection = dataSource.IsCollection,
                Category = dataSource.Category,
                CustomFields = dataSource.Formatter?.CustomFields,
                FormatterOptions = dataSource.Formatter?.Options ?? new List<DataFormatterOptionBag>()
            };
        }

        /// <summary>
        /// Gets the list of icons that are supported.
        /// </summary>
        /// <returns>A list of <see cref="IconItemBag"/> objects.</returns>
        private List<IconItemBag> GetIconList()
        {
            return LabelIcon.StandardIcons
                .Select( a => new IconItemBag
                {
                    Value = a.Value,
                    Text = a.Name,
                    Weight = a.IsBold ? 900 : 400,
                    Code = a.Code
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the <see cref="LabelDetailBag"/> that can be understood by
        /// the Obsidian code. This also handles converting the private rule
        /// data into public data.
        /// </summary>
        /// <param name="checkInLabel">The check-in label that needs to be edited.</param>
        /// <returns>A new instance of <see cref="LabelDetailBag"/>.</returns>
        private LabelDetailBag GetLabelDetailBag( CheckInLabel checkInLabel )
        {
            var designedLabel = checkInLabel.Content.FromJsonOrNull<DesignedLabelBag>();

            // Ensure we have the required properties set.
            designedLabel = designedLabel ?? new DesignedLabelBag();
            designedLabel.Fields = designedLabel.Fields ?? new List<LabelFieldBag>();

            var converter = new FieldFilterPublicConverter( r => FieldSourceHelper.GetEntityFieldForRule( checkInLabel.LabelType, r ) );

            foreach ( var field in designedLabel.Fields )
            {
                field.ConditionalVisibility = converter.ToPublicBag( field.ConditionalVisibility );
            }

            return new LabelDetailBag
            {
                LabelData = designedLabel,
                ConditionalVisibility = converter.ToPublicBag( checkInLabel.GetConditionalPrintCriteria() )
            };
        }

        /// <summary>
        /// Gets a fake label data object that can be used to perform basic
        /// previewing of labels.
        /// </summary>
        /// <param name="labelType">The type of label to be previewed.</param>
        /// <param name="currentPerson">The person to that is currently logged in.</param>
        /// <param name="rockContext">The context to access the database with.</param>
        /// <returns>A label data object or <c>null</c>.</returns>
        internal static object GetPreviewLabelData( LabelType labelType, Person currentPerson, RockContext rockContext )
        {
            if ( currentPerson.Attributes == null )
            {
                currentPerson.LoadAttributes( rockContext );
            }

            if ( labelType == LabelType.Family )
            {
                return new FamilyLabelData( currentPerson.PrimaryFamily,
                    new List<LabelAttendanceDetail>(),
                    rockContext );
            }
            else if ( labelType == LabelType.Person )
            {
                return new PersonLabelData( currentPerson,
                    currentPerson.PrimaryFamily,
                    new List<LabelAttendanceDetail>(),
                    rockContext );
            }
            else if ( labelType == LabelType.Attendance )
            {
                var attendance = new LabelAttendanceDetail
                {
                    Person = currentPerson
                };

                return new AttendanceLabelData( attendance,
                    currentPerson.PrimaryFamily,
                    new List<LabelAttendanceDetail>(),
                    rockContext );
            }
            else if ( labelType == LabelType.Checkout )
            {
                var attendance = new LabelAttendanceDetail
                {
                    Person = currentPerson
                };

                return new CheckoutLabelData( attendance,
                    currentPerson.PrimaryFamily,
                    rockContext );
            }
            else if ( labelType == LabelType.PersonLocation )
            {
                var locationId = new LocationService( rockContext )
                    .Queryable()
                    .Where( l => !string.IsNullOrEmpty( l.Name ) && l.IsActive )
                    .OrderBy( l => l.Id )
                    .Select( l => l.Id )
                    .FirstOrDefault();

                return new PersonLocationLabelData( currentPerson,
                    NamedLocationCache.Get( locationId ),
                    new List<LabelAttendanceDetail>(),
                    rockContext );
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves a <see cref="CheckInLabel"/> from changes made in the
        /// designer interface.
        /// </summary>
        /// <param name="key">The identifier of the label to be saved.</param>
        /// <param name="label">The details about the label.</param>
        /// <param name="previewData">The base64 encoded preview image data in PNG format.</param>
        /// <returns>The result of the operation.</returns>
        [BlockAction]
        public BlockActionResult Save( string key, LabelDetailBag label, string previewData )
        {
            var entityService = new CheckInLabelService( RockContext );
            var checkInLabel = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( checkInLabel == null )
            {
                return ActionBadRequest( $"{CheckInLabel.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${CheckInLabel.FriendlyTypeName}." );
            }

            if ( checkInLabel.IsSystem )
            {
                return ActionBadRequest( "Not allowed to edit system labels." );
            }

            if ( label == null || label.LabelData == null )
            {
                return ActionBadRequest( "Invalid data provided." );
            }

            // Note: We don't do the ValidProperties<> boxing around the label
            // because it would get very complicated to deal with all the child
            // properties that are nested multiple layers deep. This is a special
            // purpose block that is tied closely to the Obsidian implementation
            // so other code should not just use the block actions and assume
            // they will work.

            var converter = new FieldFilterPublicConverter( r => FieldSourceHelper.GetEntityFieldForRule( checkInLabel.LabelType, r ) );

            if ( label.LabelData.Fields != null )
            {
                foreach ( var field in label.LabelData.Fields )
                {
                    field.ConditionalVisibility = converter.ToPrivateBag( field.ConditionalVisibility );
                }
            }

            checkInLabel.Content = label.LabelData.ToJson();
            checkInLabel.SetConditionalPrintCriteria( converter.ToPrivateBag( label.ConditionalVisibility ?? new FieldFilterGroupBag() ) );

            if ( previewData.IsNotNullOrWhiteSpace() )
            {
                checkInLabel.PreviewImage = Convert.FromBase64String( previewData );
            }
            else
            {
                checkInLabel.PreviewImage = new byte[0];
            }

            RockContext.SaveChanges();

            var returnUrl = this.GetParentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.CheckInLabelId] = checkInLabel.IdKey
            } );

            return ActionOk( returnUrl );
        }

        /// <summary>
        /// Generate the ZPL that can be used as a preview of how this label
        /// will look.
        /// </summary>
        /// <param name="key">The key that identifies the label to preview.</param>
        /// <param name="label">The label details as configured in the UI.</param>
        /// <param name="attendanceId">The attendance identifer to use when rendering the preview. If not specified then a generic preview is generated.</param>
        /// <returns>The result of the operation.</returns>
        [BlockAction]
        public BlockActionResult Preview( string key, LabelDetailBag label, string attendanceId )
        {
            var entityService = new CheckInLabelService( RockContext );
            var checkInLabel = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( checkInLabel == null )
            {
                return ActionBadRequest( $"{CheckInLabel.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${CheckInLabel.FriendlyTypeName}." );
            }

            if ( label == null || label.LabelData == null )
            {
                return ActionBadRequest( "Invalid data provided." );
            }

            // Note: We don't do the ValidProperties<> boxing around the label
            // because it would get very complicated to deal with all the child
            // properties that are nested multiple layers deep. This is a special
            // purpose block that is tied closely to the Obsidian implementation
            // so other code should not just use the block actions and assume
            // they will work.

            var converter = new FieldFilterPublicConverter( r => FieldSourceHelper.GetEntityFieldForRule( checkInLabel.LabelType, r ) );

            if ( label.LabelData.Fields != null )
            {
                foreach ( var field in label.LabelData.Fields )
                {
                    field.ConditionalVisibility = converter.ToPrivateBag( field.ConditionalVisibility );
                }
            }

            if ( attendanceId.IsNullOrWhiteSpace() )
            {
                using ( var memoryStream = new MemoryStream() )
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var request = new PrintLabelRequest
                    {
                        Capabilities = new PrinterCapabilities(),
                        DataSources = FieldSourceHelper.GetCachedDataSources( checkInLabel.LabelType ),
                        Label = label.LabelData,
                        LabelData = GetPreviewLabelData( checkInLabel.LabelType, RequestContext.CurrentPerson, RockContext )
                    };

                    var renderer = new ZplLabelRenderer();

                    renderer.BeginLabel( memoryStream, request );

                    foreach ( var field in request.Label.Fields )
                    {
                        var labelField = new LabelField( field );

                        renderer.WriteField( labelField );
                    }

                    renderer.EndLabel();

                    var zpl = System.Text.Encoding.UTF8.GetString( memoryStream.ToArray() );
                    sw.Stop();

                    return ActionOk( new
                    {
                        Content = zpl,
                        Duration = sw.ElapsedMilliseconds
                    } );
                }
            }
            else
            {
                checkInLabel.Content = label.LabelData.ToJson();

                var attendance = new AttendanceService( RockContext ).Get( attendanceId.AsInteger() );

                if ( attendance == null )
                {
                    return ActionBadRequest( "Attendance record was not found." );
                }

                var isValidAttendance = attendance.PersonAliasId.HasValue
                    && attendance.Occurrence?.ScheduleId.HasValue == true
                    && attendance.Occurrence?.LocationId.HasValue == true
                    && attendance.Occurrence?.GroupId.HasValue == true;

                if ( !isValidAttendance )
                {
                    return ActionBadRequest( "Attendance record is not a valid check-in attendance." );
                }

                var attendanceLabel = new LabelAttendanceDetail( attendance, RockContext );
                var director = new CheckInDirector( RockContext );

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var data = director.LabelProvider.RenderLabelUnconditionally( checkInLabel,
                    attendanceLabel,
                    new List<LabelAttendanceDetail> { attendanceLabel },
                    attendance.SearchResultGroup,
                    null );
                sw.Stop();

                var zpl = System.Text.Encoding.UTF8.GetString( data.Data );

                return ActionOk( new
                {
                    Content = zpl,
                    Duration = sw.ElapsedMilliseconds
                } );
            }
        }

        #endregion
    }
}
