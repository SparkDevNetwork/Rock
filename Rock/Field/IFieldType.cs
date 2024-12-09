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
using Rock.Reporting;

namespace Rock.Field
{
    /// <summary>
    /// Interface that a custom field type must implement
    /// </summary>
    public interface IFieldType
    {
        #region Configuration

        /// <summary>
        /// Gets a list of the configuration keys.
        /// </summary>
        /// <returns></returns>
        List<string> ConfigurationKeys();

        /// <summary>
        /// Gets the public configuration values. This data will be sent to remote systems
        /// which have no security protection. Sensitive data should not
        /// be included or should be encrypted before storing in the dictionary.
        /// </summary>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <param name="usage">The way the public configuration values are intended to be used.</param>
        /// <param name="internalValue">The current private value. This is required when <paramref name="usage"/> is <see cref="ConfigurationValueUsage.View"/> and will be <c>null</c> in other cases.</param>
        /// <returns>The configuration values that are safe for public use.</returns>
        Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string internalValue );

        /// <summary>
        /// Get the edit configuration properties that are safe to send to a remote
        /// device when editing a field type. This is custom data your field type
        /// can use to provide, for example, a list of options to pick from. This
        /// method may be called multiple times while editing a field type.
        /// </summary>
        /// <remarks>
        ///     <para>This method is used during the editing of a field type's configuration.</para>
        ///     <para>
        ///         The return value should include all data required to display
        ///         the current selections to the user, even if they wouldn't
        ///         normally have access to make those selections. This ensures
        ///         configuration is not accidentally wiped out by the person if
        ///         they don't have access to something.
        ///     </para>
        /// </remarks>
        /// <param name="privateConfigurationValues">The private configuration values that are currently selected.</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> of custom key and value pairs.</returns>
        Dictionary<string, string> GetPublicEditConfigurationProperties( Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Gets the private configuration options that will be saved to the
        /// database.
        /// </summary>
        /// <remarks>
        ///     <para>This method is used during the editing of a field type's configuration.</para>
        ///     <para>
        ///         Calling this method with the results from <see cref="GetPublicConfigurationValues(Dictionary{string, string}, ConfigurationValueUsage, string)"/>
        ///         should return the same data that was originally passed to <see cref="GetPublicConfigurationValues(Dictionary{string, string}, ConfigurationValueUsage, string)"/>.
        ///     </para>
        /// </remarks>
        /// <param name="publicConfigurationValues">The public configuration values.</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> of options that are safe to store to the database.</returns>
        Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues );

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        List<Control> ConfigurationControls();

        /// <summary>
        /// Gets the configuration values
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls );

        /// <summary>
        /// Sets the configuration values.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues );

        #endregion

        #region Formatting

        /// <summary>
        /// Gets the align value that should be used when displaying value
        /// </summary>
        System.Web.UI.WebControls.HorizontalAlign AlignValue { get; }

        /// <summary>
        /// Formats the value into a user-friendly string of plain text.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A plain string of text.</returns>
        string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a string of HTML text that can be rendered
        /// on a web page.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string of HTML text.</returns>
        string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a condensed user-friendly string of plain text.
        /// This value will be used when space is limited.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A plain string of text.</returns>
        string GetCondensedTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a string of HTML text that can be rendered
        /// on a web page. This value will be used when space is limited.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string of HTML text.</returns>
        string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Gets the value that will be sent to remote devices. This value is
        /// used by those devices to do custom formatting when displaying the value.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string of text to send to the device.</returns>
        string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value based on the type and qualifiers
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed );

        /// <summary>
        /// Formats the value.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        string FormatValue( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed );

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false );

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false );

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        object ValueAsFieldType( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        object ValueAsFieldType( string value, Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Setting to determine whether the value from this control is sensitive.  This is used for determining
        /// whether or not the value of this attribute is logged when changed.
        /// </summary>
        /// <returns>
        ///   <c>false</c> By default, any field is not sensitive.
        /// </returns>
        bool IsSensitive();

        #endregion

        #region Edit Control

        /// <summary>
        /// Gets a value indicating whether this field has a control to configure the default value
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has default control; otherwise, <c>false</c>.
        /// </value>
        bool HasDefaultControl { get; }

        /// <summary>
        /// Gets the value that will be sent to remote devices. This value is
        /// used for custom formatting as well as device-side editing.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string of text to send to the remote device.</returns>
        string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Gets the value to be stored in the database from the value sent by
        /// a device at the end of an edit.
        /// </summary>
        /// <param name="publicValue">The public value received from a remote device.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string value to store in the database.</returns>
        string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Creates an HTML control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id );

        /// <summary>
        /// Reads the value of the control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value );

        /// <summary>
        /// Determines whether this FieldType supports doing PostBack for the editControl 
        /// </summary>
        /// <param name="editControl">The edit control.</param>
        /// <returns>
        ///   <c>true</c> if [has change handler] [the specified control]; otherwise, <c>false</c>.
        /// </returns>
        bool HasChangeHandler( Control editControl );

        /// <summary>
        /// Specifies an action to perform when the EditControl's Value is changed. See also <seealso cref="HasChangeHandler(Control)"/>
        /// </summary>
        /// <param name="editControl">The edit control.</param>
        /// <param name="action">The action.</param>
        void AddChangeHandler( Control editControl, Action action );

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        bool IsValid( string value, bool required, out string message );

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the value that will be sent to remote devices. This value will
        /// be used when editing the filter value on the device.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A <see cref="ComparisonValue"/> that has been parsed and sanitized.</returns>
        ComparisonValue GetPublicFilterValue( string privateValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Gets the filter value to be stored in the database from the value sent
        /// by a device at the end of an edit on a filter control.
        /// </summary>
        /// <param name="publicValue">The public value received from a remote device.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string value to store in the database.</returns>
        string GetPrivateFilterValue( ComparisonValue publicValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type using the specified FilterMode
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        Control FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode );

        /// <summary>
        /// Returns the ComparisonType options that the field supports
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        Rock.Model.ComparisonType FilterComparisonType { get; }

        /// <summary>
        /// Determines whether this filter type has a FilterControl
        /// </summary>
        /// <returns></returns>
        bool HasFilterControl();

        /// <summary>
        /// Gets the filter values.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode );

        /// <summary>
        /// Gets the filter compare value (int or string version of <seealso cref="Rock.Model.ComparisonType"/> as a string)
        /// </summary>
        /// <param name="control">The control that has the comparison options (or null if this fieldtype doesn't have one).</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        string GetFilterCompareValue( Control control, FilterMode filterMode );

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The filter value control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Gets the equal to compare value (types that don't support an equalto comparison (i.e. singleselect) should return null
        /// </summary>
        /// <returns></returns>
        string GetEqualToCompareValue();

        /// <summary>
        /// Sets the filter value.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        void SetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues );

        /// <summary>
        /// Formats the filter values.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <returns></returns>
        string FormatFilterValues( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues );

        /// <summary>
        /// Gets the filter format script.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title );

        /// <summary>
        /// Gets a filter expression for an entity property value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType );

        /// <summary>
        /// Gets a filter expression to be used as part of a AttributeValue Query or EntityAttributeQueryExpression
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression );

        /// <summary>
        /// Applies the attribute query filter based on the values configured in the filterControl
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="qry">The qry.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        System.Linq.IQueryable<T> ApplyAttributeQueryFilter<T>( System.Linq.IQueryable<T> qry, Control filterControl, Rock.Web.Cache.AttributeCache attribute, Rock.Data.IService serviceInstance, Rock.Reporting.FilterMode filterMode ) where T : Rock.Data.Entity<T>, new();

        /// <summary>
        /// Determines whether the filter is an 'Equal To' comparison and the filtered value is equal to the specified value.
        /// </summary>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is equal to value] [the specified filter values]; otherwise, <c>false</c>.
        /// </returns>
        bool IsEqualToValue(List<string> filterValues, string value);

        /// <summary>
        /// Determines whether the filter's comparison type and filter compare value(s) evaluates to true for the specified value
        /// </summary>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is compared to value] [the specified filter values]; otherwise, <c>false</c>.
        /// </returns>
        bool IsComparedToValue( List<string> filterValues, string value );

        /// <summary>
        /// Gets the name of the attribute value field.
        /// </summary>
        /// <value>
        /// The name of the attribute value field.
        /// </value>
        string AttributeValueFieldName { get; }

        /// <summary>
        /// Gets the type of the attribute value field.
        /// </summary>
        /// <value>
        /// The type of the attribute value field.
        /// </value>
        Type AttributeValueFieldType { get; }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when a qualifier is updated.
        /// </summary>
        event EventHandler QualifierUpdated;

        #endregion

        #region Persistence

        /// <summary>
        /// Determines whether this field type supports persisted values when
        /// using the given configuration values.
        /// </summary>
        /// <param name="privateConfigurationValues">The private configuration values that describe the field type settings.</param>
        /// <returns><c>true</c> if persisted values are supported; otherwise, <c>false</c>.</returns>
        bool IsPersistedValueSupported( Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Determines whether the persisted values are considered volatile and
        /// might change from outside influence. Volatile field types will have
        /// their persisted values updated periodically by a job.
        /// </summary>
        /// <remarks>
        /// An example of a volatile field type would be one that gets its
        /// possible values from a SQL query. Outside influence might cause
        /// the persisted values to change even if the raw value stays the same.
        /// </remarks>
        /// <param name="privateConfigurationValues">The private configuration values that describe the field type settings.</param>
        /// <returns><c>true</c> if the persisted values are considered volatile; otherwise, <c>false</c>.</returns>
        bool IsPersistedValueVolatile( Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Gets the persisted value placeholder text when persisted values are
        /// not supported by the field type. This will be used for all persisted
        /// value properties so that when they are displayed it is apparent to
        /// the individual that they are not seeing a real value.
        /// </summary>
        /// <param name="privateConfigurationValues">The private configuration values that describe the field type settings.</param>
        /// <returns>The <see cref="string"/> to use as a placeholder.</returns>
        string GetPersistedValuePlaceholder( Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Determines whether any persisted values for this field type should
        /// be invalidated and marked dirty due to the change in configuration.
        /// </summary>
        /// <param name="oldPrivateConfigurationValues">The old private configuration values before the change.</param>
        /// <param name="newPrivateConfigurationValues">The new private configuration values after the change.</param>
        /// <returns><c>true</c> if the change in configuration should cause all persisted values to be recalculated; otherwise, <c>false</c>.</returns>
        bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues );

        /// <summary>
        /// Gets all the persisted values for the private database value. This will be
        /// called when the raw value changes and new persisted values need to be
        /// calculated. Subclasses that need to hit the database should override this
        /// method so they can do a single query instead of 4 separate queries.
        /// </summary>
        /// <param name="privateValue">The raw value.</param>
        /// <param name="privateConfigurationValues">The private configuration values.</param>
        /// <param name="cache">An extremely short term cache that can be used to store and retrieve data instead of hitting the database. This value may be null if no cache should be used.</param>
        /// <returns>An instance of <see cref="PersistedValues"/> that contains all the values to be persisted.</returns>
        PersistedValues GetPersistedValues( string privateValue, Dictionary<string, string> privateConfigurationValues, IDictionary<string, object> cache );

        #endregion
    }
}
