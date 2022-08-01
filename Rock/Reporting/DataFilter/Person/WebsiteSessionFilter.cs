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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Reporting.DataFilter.Interaction
{
    /// <summary>
    /// Operates against Interaction, InteractionComponent, InteractionChannel, InteractionSession.
    /// </summary>
    [Description( "Filter people based on their website session interaction" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Website Session Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "50FDC068-D943-4673-B656-DFC2792BEEF7" )]
    public class WebsiteSessionFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return "Rock.Model.Person"; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string GetTitle( Type entityType )
        {
            return "Website Session";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The client format script.
        /// </returns>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
  
    var result = 'Interactions';

    var websiteNames = $('.js-websites', $content).find(':selected');
    if ( websiteNames.length > 0 ) {
        var websiteNamesDelimitedList = websiteNames.map(function() {{ return $(this).text() }}).get().join(', ');
        result += "" with: "" + websiteNamesDelimitedList +""."";
    }

    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val();
    if( dateRangeText ) {{
        result +=  "" Date Range: "" + dateRangeText
    }}

    let pagePicker = document.querySelector('.js-pages');
    let selectedNames = pagePicker.querySelector('.js-item-name-value').value;

    if(selectedNames){
        result += "" on pages: "" + selectedNames +""."";
    }

    return result;
}
";
        }

        /// <summary>
        /// Provides a user-friendly description of the specified filter values.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A string containing the user-friendly description of the settings.
        /// </returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var result = "Interactions";

            if ( selectionConfig != null )
            {
                var comparisonType = selectionConfig.ComparisonValue.ConvertToEnumOrNull<ComparisonType>();
                result = comparisonType == null ? "Interactions" : $"{comparisonType.ConvertToString()} {selectionConfig.ViewsCount} Interactions";

                if ( selectionConfig.WebsiteIds.Count > 0 )
                {
                    var websiteNames = new List<string>();
                    foreach ( var websiteId in selectionConfig.WebsiteIds )
                    {
                        var interactionChannel = InteractionChannelCache.Get( websiteId );
                        if ( interactionChannel != null )
                        {
                            websiteNames.Add( interactionChannel.Name );
                        }
                    }

                    result += string.Format( " with : {0}.", websiteNames.AsDelimited( "," ) );
                }

                if ( selectionConfig.DelimitedDateRangeValues.IsNotNullOrWhiteSpace() )
                {
                    var dateRangeString = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.DelimitedDateRangeValues );
                    if ( dateRangeString.IsNotNullOrWhiteSpace() )
                    {
                        result += $" Date Range: {dateRangeString}";
                    }
                }

                if ( selectionConfig.PageIds.Count > 0 )
                {
                    var pages = new List<string>();
                    foreach ( var pageId in selectionConfig.PageIds )
                    {
                        var page = PageCache.Get( pageId );
                        if ( page != null )
                        {
                            pages.Add( page.InternalName );
                        }
                    }

                    result += string.Format( " on pages: {0}.", pages.AsDelimited( "," ) );
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the model representation of the child controls used to display and edit the filter settings.
        /// Implement this version of CreateChildControls if your DataFilterComponent works the same in all filter modes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the filter controls.</param>
        /// <returns>
        /// The array of new controls created to implement the filter.
        /// </returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();
            var rockContext = new RockContext();

            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes | ComparisonType.StartsWith );
            ddlIntegerCompare.ID = string.Format( "{0}_{1}", filterControl.ID, "ddlIntegerCompare" );
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );
            controls.Add( ddlIntegerCompare );

            var numberBox = new NumberBox();
            numberBox.ID = string.Format( "{0}_{1}", filterControl.ID, "numberBox" );
            numberBox.AddCssClass( "js-filter-control" );
            filterControl.Controls.Add( numberBox );
            controls.Add( numberBox );

            numberBox.FieldName = "Count";

            var sessionsLabel = new Label();
            sessionsLabel.Text = " sessions on the ";
            filterControl.Controls.Add( sessionsLabel );
            controls.Add( sessionsLabel );

            var rlbWebsites = new RockListBox();
            rlbWebsites.ID = filterControl.GetChildControlInstanceName( "rlbWebsites" );
            rlbWebsites.CssClass = "js-websites";
            rlbWebsites.Items.Clear();
            rlbWebsites.Items.AddRange( GetInteractionChannelListItems( rockContext ).ToArray() );
            filterControl.Controls.Add( rlbWebsites );
            controls.Add( rlbWebsites );

            var websitesLabel = new Label();
            websitesLabel.Text = " websites(s) ";
            filterControl.Controls.Add( websitesLabel );
            controls.Add( websitesLabel );

            var dateRangeLabel = new Label();
            dateRangeLabel.Text = " in the following date range ";
            filterControl.Controls.Add( dateRangeLabel );
            controls.Add( dateRangeLabel );

            // Date Started
            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = filterControl.GetChildControlInstanceName( "slidingDateRangePicker" );
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Started";
            slidingDateRangePicker.Help = "The date range within which the website page was viewed";
            slidingDateRangePicker.Required = false;
            filterControl.Controls.Add( slidingDateRangePicker );
            controls.Add( slidingDateRangePicker );

            var optionallyLabel = new Label();
            optionallyLabel.Text = " optionally limited to the following pages ";
            filterControl.Controls.Add( optionallyLabel );
            controls.Add( optionallyLabel );

            var ppPages = new PagePicker();
            ppPages.ID = filterControl.GetChildControlInstanceName( "ppPages" );
            ppPages.AllowMultiSelect = true;
            ppPages.CssClass = "js-pages";
            filterControl.Controls.Add( ppPages );
            controls.Add( ppPages );

            return controls.ToArray();
        }

        private List<ListItem> GetInteractionChannelListItems( RockContext rockContext )
        {
            var websiteGuid = SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid();
            var activeSiteIds = SiteCache.All().Where( s => s.IsActive ).Select( s => s.Id );

            var channels = new InteractionChannelService( rockContext )
                .Queryable()
                .Where( ic => ic.ChannelTypeMediumValue.Guid == websiteGuid && ic.IsActive && activeSiteIds.Contains( ic.ChannelEntityId.Value ) )
                .Select( ic => new ListItem() { Text = ic.Name, Value = ic.Id.ToString() } )
                .ToList();

            return channels.OrderBy( m => m.Text ).ToList();
        }

        /// <summary>
        /// Renders the child controls used to display and edit the filter settings for HTML presentation.
        /// Implement this version of RenderControls if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the controls being rendered.</param>
        /// <param name="writer">The writer being used to generate the HTML for the output page.</param>
        /// <param name="controls">The model representation of the child controls for this component.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            Label sessionsLabel = controls[2] as Label;
            RockListBox rlbWebsites = controls[3] as RockListBox;
            Label websitesLabel = controls[4] as Label;
            Label dateRangeLabel = controls[5] as Label;
            SlidingDateRangePicker slidingDateRangePicker = controls[6] as SlidingDateRangePicker;
            Label optionallyLabel = controls[7] as Label;
            PagePicker ppPages = controls[8] as PagePicker;

            writer.AddAttribute( "class", "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // row

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // ddlCompare
            ddlCompare.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-1" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // nbValue
            nbValue.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-2 mt-1" );
            sessionsLabel.RenderControl( writer ); // sessionsLabel

            writer.AddAttribute( "class", "col-md-5" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // websites
            rlbWebsites.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-1 mt-1" );
            websitesLabel.RenderControl( writer ); // websitesLabel

            writer.RenderEndTag();  // row

            writer.AddAttribute( "class", "row mt-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // second row

            writer.AddAttribute( "class", "col-md-3 mt-4" );
            dateRangeLabel.RenderControl( writer ); // dateRangeLabel

            slidingDateRangePicker.RenderControl( writer ); // dateRange

            writer.RenderEndTag();  // third row

            writer.AddAttribute( "class", "row mt-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // second row

            writer.AddAttribute( "class", "col-md-4 mt-1" );
            optionallyLabel.RenderControl( writer ); // optionallyLabel

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // websites
            ppPages.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // third row
        }

        /// <summary>
        /// Gets the selection.
        /// Implement this version of GetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <returns>
        /// A formatted string representing the filter settings.
        /// </returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            RockListBox rlbWebsites = controls[3] as RockListBox;
            SlidingDateRangePicker dateRange = controls[6] as SlidingDateRangePicker;
            PagePicker ppPages = controls[8] as PagePicker;

            var selectionConfig = new SelectionConfig();
            selectionConfig.ComparisonValue = ddlCompare.SelectedValue;
            selectionConfig.DelimitedDateRangeValues = dateRange.DelimitedValues;
            selectionConfig.PageIds = ppPages.SelectedIds.ToList();
            selectionConfig.ViewsCount = nbValue.IntegerValue ?? 1;
            selectionConfig.WebsiteIds = rlbWebsites.SelectedValuesAsInt;

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// Implement this version of SetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            RockListBox rlbWebsites = controls[3] as RockListBox;
            SlidingDateRangePicker dateRange = controls[6] as SlidingDateRangePicker;
            PagePicker ppPages = controls[8] as PagePicker;

            ddlCompare.SelectedValue = selectionConfig.ComparisonValue;
            nbValue.IntegerValue = selectionConfig.ViewsCount;
            rlbWebsites.SetValues( selectionConfig.WebsiteIds );
            dateRange.DelimitedValues = selectionConfig.DelimitedDateRangeValues;
            ppPages.SetValues( selectionConfig.PageIds );
        }

        /// <summary>
        /// Creates a Linq Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A Linq Expression that can be used to filter an IQueryable.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );
            var comparisonType = selectionConfig.ComparisonValue.ConvertToEnumOrNull<ComparisonType>();
            var rockContext = ( RockContext ) serviceInstance.Context;
            rockContext.Database.Log = s => Debug.WriteLine( s );

            IQueryable<Rock.Model.Interaction> interactionQry;

            if ( selectionConfig.PageIds.Count > 0 )
            {
                interactionQry = new InteractionService( rockContext ).GetPageViewsByPage( selectionConfig.WebsiteIds.ToArray(), selectionConfig.PageIds.ToArray() );
            }
            else
            {
                interactionQry = new InteractionService( rockContext ).GetPageViewsBySite( selectionConfig.WebsiteIds.ToArray() );
            }

            if ( selectionConfig.DelimitedDateRangeValues.IsNotNullOrWhiteSpace() )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.DelimitedDateRangeValues );
                if ( dateRange.Start.HasValue )
                {
                    interactionQry = interactionQry.Where( n => n.CreatedDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    interactionQry = interactionQry.Where( n => n.CreatedDateTime <= dateRange.End.Value );
                }
            }

            var personQry = new PersonService( rockContext ).Queryable();

            if ( comparisonType != null )
            {
                switch ( comparisonType )
                {
                    case ComparisonType.EqualTo:
                        personQry = personQry.Where( p => interactionQry.Where( i => i.PersonAliasId == p.Id ).GroupBy( a => a.InteractionSessionId ).Count() == selectionConfig.ViewsCount );
                        break;
                    case ComparisonType.LessThan:
                        personQry = personQry.Where( p => interactionQry.Where( i => i.PersonAliasId == p.Id ).GroupBy( a => a.InteractionSessionId ).Count() < selectionConfig.ViewsCount );
                        break;
                    case ComparisonType.LessThanOrEqualTo:
                        personQry = personQry.Where( p => interactionQry.Where( i => i.PersonAliasId == p.Id ).GroupBy( a => a.InteractionSessionId ).Count() <= selectionConfig.ViewsCount );
                        break;
                    case ComparisonType.GreaterThan:
                        personQry = personQry.Where( p => interactionQry.Where( i => i.PersonAliasId == p.Id ).GroupBy( a => a.InteractionSessionId ).Count() > selectionConfig.ViewsCount );
                        break;
                    case ComparisonType.GreaterThanOrEqualTo:
                        personQry = personQry.Where( p => interactionQry.Where( i => i.PersonAliasId == p.Id ).GroupBy( a => a.InteractionSessionId ).Count() >= selectionConfig.ViewsCount );
                        break;
                }
            }

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personQry, parameterExpression, "p" );
        }

        #endregion

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
            }

            /// <summary>
            /// Gets or sets the note type identifiers.
            /// </summary>
            /// <value>
            /// The note type identifiers.
            /// </value>
            public List<int> WebsiteIds { get; set; }

            /// <summary>
            /// Gets or sets the note type identifiers.
            /// </summary>
            /// <value>
            /// The note type identifiers.
            /// </value>
            public List<int> PageIds { get; set; }

            /// <summary>
            /// Gets a pipe delimited string of the property values. This is to use the SlidingDateRangePicker's existing logic.
            /// </summary>
            /// <value>
            /// The delimited values.
            /// </value>
            public string DelimitedDateRangeValues { get; set; }

            /// <summary>
            /// Gets or sets the minimum count.
            /// </summary>
            /// <value>
            /// The minimum count.
            /// </value>
            public int ViewsCount { get; set; }

            /// <summary>
            /// The date range comparison value
            /// </summary>
            public string ComparisonValue { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                return selection.FromJsonOrNull<SelectionConfig>() ?? new SelectionConfig();
            }
        }
    }
}
