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
using System.Linq.Expressions;
using System.Web.UI;

using Rock.Attribute;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to allow the user to watch a video and have the amount they
    /// watched recorded.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class MediaWatchFieldType : FieldType
    {
        #region Configuration

        /// <summary>
        /// Configuration Key for MediaElement to be watched.
        /// </summary>
        public static readonly string CONFIG_MEDIA = "media";

        /// <summary>
        /// Configuration Key for specifying how much they need to watch
        /// in order for the field to be considered valid if it is required.
        /// </summary>
        public static readonly string CONFIG_COMPLETION_PERCENTAGE = "completionPercentage";

        /// <summary>
        /// Configuration Key for specifying how many days to look back for
        /// the auto-resume feature. If not set then 0 is assumed.
        /// </summary>
        public static readonly string CONFIG_AUTO_RESUME_IN_DAYS = "autoResumeInDays";

        /// <summary>
        /// Configuration Key for specifying the maximum width of the video.
        /// This is helpful in things like workflows where you won't want the
        /// video to end up taking over the whole screen.
        /// </summary>
        public static readonly string CONFIG_MAX_WIDTH = "maxWidth";

        /// <summary>
        /// Configuration Key for the validation message to display.
        /// </summary>
        public static readonly string CONFIG_VALIDATION_MESSAGE = "validationMessage";

        /// <inheritdoc/>
        public override bool HasDefaultControl => false;

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            return new List<string>
            {
                CONFIG_MEDIA,
                CONFIG_COMPLETION_PERCENTAGE,
                CONFIG_AUTO_RESUME_IN_DAYS,
                CONFIG_MAX_WIDTH,
                CONFIG_VALIDATION_MESSAGE
            };
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var mpMedia = new MediaElementPicker();
            mpMedia.Label = "Media";
            mpMedia.Help = "The media file that will be watched by the individual.";
            mpMedia.Required = true;
            controls.Add( mpMedia );

            var nbCompletionPercentage = new NumberBox();
            nbCompletionPercentage.NumberType = System.Web.UI.WebControls.ValidationDataType.Double;
            nbCompletionPercentage.Label = "Completion Percentage";
            nbCompletionPercentage.Help = "The percentage of the video that the individual must view in order for the video to be considered watched. Instead of setting this to 100% you probably want a few points below that.";
            nbCompletionPercentage.AppendText = "%";
            nbCompletionPercentage.MaximumValue = 100.ToString();
            nbCompletionPercentage.MinimumValue = 0.ToString();
            nbCompletionPercentage.CssClass = "input-width-sm";
            controls.Add( nbCompletionPercentage );

            var nbAutoResume = new NumberBox();
            nbAutoResume.NumberType = System.Web.UI.WebControls.ValidationDataType.Integer;
            nbAutoResume.Label = "Auto Resume In Days";
            nbAutoResume.Help = "The video player will look back this many days for a previous watch session and attempt to auto-resume from that point.";
            nbAutoResume.MaximumValue = 3650.ToString();
            nbAutoResume.MinimumValue = ( -1 ).ToString();
            nbAutoResume.CssClass = "input-width-sm";
            controls.Add( nbAutoResume );

            var tbMaxWidth = new RockTextBox();
            tbMaxWidth.Label = "Maximum Video Width";
            tbMaxWidth.Help = "The maximum width of the video. This unit can be expressed in pixels (e.g. 250px) or percent (e.g. 75%). If no unit is provided pixels is assumed.";
            tbMaxWidth.CssClass = "input-width-sm";
            controls.Add( tbMaxWidth );

            var tbValidationMessage = new RockTextBox();
            tbValidationMessage.Label = "Validation Message";
            tbValidationMessage.Help = "The message that should be show when the individual does not watch the required amount of the video.";
            tbValidationMessage.TextMode = System.Web.UI.WebControls.TextBoxMode.MultiLine;
            tbValidationMessage.Rows = 3;
            controls.Add( tbValidationMessage );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( CONFIG_MEDIA, new ConfigurationValue( "Media", "The Media file to use for the video.", "" ) );
            configurationValues.Add( CONFIG_COMPLETION_PERCENTAGE, new ConfigurationValue( "Completion Percentage", "The percentage of the video that the individual must view in order for the video to be considered watched.", "" ) );
            configurationValues.Add( CONFIG_AUTO_RESUME_IN_DAYS, new ConfigurationValue( "Auto Resume In Days", "The video player will look back this many days for a previous watch session and attempt to auto-resume from that point.", "" ) );
            configurationValues.Add( CONFIG_MAX_WIDTH, new ConfigurationValue( "Maximum Video Width", "The maximum width of the video. This unit can be expressed in pixels (e.g. 250px) or percent (e.g. 75%). If no unit is provided pixels is assumed.", "" ) );
            configurationValues.Add( CONFIG_VALIDATION_MESSAGE, new ConfigurationValue( "Validation Message", "The message that should be show when the individual does not watch the required amount of the video.", "" ) );

            if ( controls == null )
            {
                return configurationValues;
            }

            if ( controls.Count >= 1 && controls[0] is MediaElementPicker mpMedia )
            {
                configurationValues[CONFIG_MEDIA].Value = mpMedia.MediaElementId.ToString();
            }

            if ( controls.Count >= 2 && controls[1] is NumberBox nbCompletionPercentage )
            {
                configurationValues[CONFIG_COMPLETION_PERCENTAGE].Value = nbCompletionPercentage.Text;
            }

            if ( controls.Count >= 3 && controls[2] is NumberBox nbAutoResume )
            {
                configurationValues[CONFIG_AUTO_RESUME_IN_DAYS].Value = nbAutoResume.Text;
            }

            if ( controls.Count >= 4 && controls[3] is RockTextBox tbMaxWidth )
            {
                configurationValues[CONFIG_MAX_WIDTH].Value = tbMaxWidth.Text;
            }

            if ( controls.Count >= 5 && controls[4] is RockTextBox tbValidationMessage )
            {
                configurationValues[CONFIG_VALIDATION_MESSAGE].Value = tbValidationMessage.Text;
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls == null || configurationValues == null )
            {
                return;
            }

            if ( controls.Count >= 1 && controls[0] is MediaElementPicker mpMedia )
            {
                if ( configurationValues.ContainsKey( CONFIG_MEDIA ) == true )
                {
                    mpMedia.MediaElementId = configurationValues[CONFIG_MEDIA].Value.AsIntegerOrNull();
                }
            }

            if ( controls.Count >= 2 && controls[1] is NumberBox nbCompletionPercentage )
            {
                if ( configurationValues.ContainsKey( CONFIG_COMPLETION_PERCENTAGE ) == true )
                {
                    nbCompletionPercentage.Text = configurationValues[CONFIG_COMPLETION_PERCENTAGE].Value;
                }
            }

            if ( controls.Count >= 3 && controls[2] is NumberBox nbAutoResume )
            {
                if ( configurationValues.ContainsKey( CONFIG_AUTO_RESUME_IN_DAYS ) == true )
                {
                    nbAutoResume.Text = configurationValues[CONFIG_AUTO_RESUME_IN_DAYS].Value;
                }
            }

            if ( controls.Count >= 4 && controls[3] is RockTextBox tbMaxWidth )
            {
                if ( configurationValues.ContainsKey( CONFIG_MAX_WIDTH ) == true )
                {
                    tbMaxWidth.Text = configurationValues[CONFIG_MAX_WIDTH].Value;
                }
            }

            if ( controls.Count >= 5 && controls[4] is RockTextBox tbValidationMessage )
            {
                if ( configurationValues.ContainsKey( CONFIG_VALIDATION_MESSAGE ) == true )
                {
                    tbValidationMessage.Text = configurationValues[CONFIG_VALIDATION_MESSAGE].Value;
                }
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( value.IsNotNullOrWhiteSpace() )
            {
                return $"{value}%{(condensed ? string.Empty : " watched")}";
            }

            return value;
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> then the value will be displayed in a condensed area; otherwise <c>false</c>.</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            if ( value.IsNotNullOrWhiteSpace() )
            {
                return $"{value}%{(condensed ? string.Empty : " watched")}";
            }

            return value;
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editControl = new MediaPlayer
            {
                ID = id,
                RequiredWatchPercentage = 0,
                RequiredErrorMessage = null,
                AutoResumeInDays = 0,
                CombinePlayStatisticsInDays = 0
            };

            if ( configurationValues?.ContainsKey( CONFIG_MEDIA ) == true )
            {
                editControl.MediaElementId = configurationValues[CONFIG_MEDIA].Value.AsIntegerOrNull();
            }

            if ( configurationValues?.ContainsKey( CONFIG_COMPLETION_PERCENTAGE ) == true )
            {
                var watchPercentage = configurationValues[CONFIG_COMPLETION_PERCENTAGE].Value.AsDoubleOrNull() ?? 0;
                editControl.RequiredWatchPercentage = watchPercentage / 100.0;
            }

            if ( configurationValues?.ContainsKey( CONFIG_AUTO_RESUME_IN_DAYS ) == true )
            {
                int autoResumeInDays = configurationValues[CONFIG_AUTO_RESUME_IN_DAYS].Value.AsIntegerOrNull() ?? 0;
                editControl.AutoResumeInDays = autoResumeInDays;
                editControl.CombinePlayStatisticsInDays = autoResumeInDays;
            }

            if ( configurationValues?.ContainsKey( CONFIG_MAX_WIDTH ) == true )
            {
                editControl.MaxVideoWidth = configurationValues[CONFIG_MAX_WIDTH].Value;
            }

            if ( configurationValues?.ContainsKey( CONFIG_VALIDATION_MESSAGE ) == true )
            {
                editControl.RequiredErrorMessage = configurationValues[CONFIG_VALIDATION_MESSAGE].Value;
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user.
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is MediaPlayer mpMedia )
            {
                return ( mpMedia.WatchedPercentage * 100.0 ).ToString();
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            // There is really nothing to do. We don't support resume in this
            // configuration.
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var nbPercentage = new NumberBox();

            nbPercentage.ID = string.Format( "{0}_nb", id );
            nbPercentage.AppendText = "%";
            nbPercentage.CssClass = "js-filter-control input-width-sm";

            return nbPercentage;
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            return value + "% watched";
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is NumberBox )
            {
                NumberBox nb = ( NumberBox ) control;
                return nb.Text;
            }

            return null;
        }

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get { return ComparisonHelper.NumericFilterComparisonTypes; }
        }


        /// <summary>
        /// Gets the name of the attribute value field that should be bound to (Value, ValueAsDateTime, ValueAsBoolean, or ValueAsNumeric)
        /// </summary>
        /// <value>
        /// The name of the attribute value field.
        /// </value>
        public override string AttributeValueFieldName
        {
            get
            {
                return "ValueAsNumeric";
            }
        }

        /// <summary>
        /// Gets a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            if ( filterValues.Count == 1 )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "ValueAsNumeric" );
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, AttributeConstantExpression( filterValues[0] ) );
            }

            return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
        }

        /// <summary>
        /// Attributes the constant expression.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override ConstantExpression AttributeConstantExpression( string value )
        {
            return Expression.Constant( value.AsDecimal(), typeof( decimal ) );
        }

        /// <summary>
        /// Gets the type of the attribute value field.
        /// </summary>
        /// <value>
        /// The type of the attribute value field.
        /// </value>
        public override Type AttributeValueFieldType
        {
            get
            {
                return typeof( decimal? );
            }
        }

        #endregion
    }
}
