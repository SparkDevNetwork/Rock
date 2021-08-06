using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class FieldType : IFieldType
    {
        public virtual ComparisonType FilterComparisonType => ComparisonHelper.BinaryFilterComparisonTypes;

        public string AttributeValueFieldName => "Value";

        public Type AttributeValueFieldType => typeof( string );

        public event EventHandler QualifierUpdated;

        public Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
            => throw new NotImplementedException();

        public List<string> ConfigurationKeys()
            => new List<string>();

        public List<object> ConfigurationControls()
            => throw new NotImplementedException();

        public Dictionary<string, ConfigurationValue> ConfigurationValues( List<object> controls )
            => new Dictionary<string, ConfigurationValue>();

        public void SetConfigurationValues( List<object> controls, Dictionary<string, ConfigurationValue> configurationValues )
            => throw new NotImplementedException();

        public string FormatFilterValues( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
            => throw new NotImplementedException();

        public string FormatValue( string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
            => value;

        public string FormatValue( object parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
            => value;

        public string FormatValue( object parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
            => value;

        public string FormatValueAsHtml( object parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
            => value;

        public string FormatValueAsHtml( object parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
            => value;

        public virtual string GetCopyValue( string originalValue, Rock.Data.RockContext rockContext )
            => originalValue;

        public bool HasDefaultControl => false;

        public object EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
            => throw new NotImplementedException();

        public string GetEditValue( object control, Dictionary<string, ConfigurationValue> configurationValues )
            => throw new NotImplementedException();

        public void SetEditValue( object control, Dictionary<string, ConfigurationValue> configurationValues, string value )
            => throw new NotImplementedException();

        public bool HasChangeHandler( object editControl )
            => false;

        public void AddChangeHandler( object editControl, Action action )
            => throw new NotImplementedException();

        public bool IsValid( string value, bool required, out string message )
        {
            message = null;
            return true;
        }

        public bool HasFilterControl()
            => false;

        public object FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filtermode )
            => throw new NotImplementedException();

        public Rock.Model.ComparisonType FilterComparisionType => ComparisonHelper.BinaryFilterComparisonTypes;

        public List<string> GetFilterValues( object filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode )
            => throw new NotImplementedException();

        public string GetFilterCompareValue( object control, FilterMode filterMode )
            => throw new NotImplementedException();

        public string GetFilterValueValue( object control, Dictionary<string, ConfigurationValue> configurationValues )
            => throw new NotImplementedException();

        public string GetEqualToCompareValue()
            => throw new NotImplementedException();

        public void SetFilterValues( object filterControl, Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
            => throw new NotImplementedException();

        public string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
            => throw new NotImplementedException();

        public System.Linq.IQueryable<T> ApplyAttributeQueryFilter<T>( System.Linq.IQueryable<T> qry, object filterControl, Rock.Web.Cache.AttributeCache attribute, Rock.Data.IService serviceInstance, Rock.Reporting.FilterMode filterMode )
            where T : Rock.Data.Entity<T>, new()
            => throw new NotImplementedException();

        public bool IsComparedToValue( List<string> filterValues, string value )
            => IsEqualToValue( filterValues, value );

        public bool IsEqualToValue( List<string> filterValues, string value )
        {
            if ( filterValues == null || filterValues.Count != 2 )
            {
                return false;
            }

            ComparisonType? filterComparisonType = filterValues[0].ConvertToEnumOrNull<ComparisonType>();
            ComparisonType? equalToCompareValue = GetEqualToCompareValue().ConvertToEnumOrNull<ComparisonType>();

            if ( filterComparisonType != equalToCompareValue )
            {
                return false;
            }

            return filterValues[1] == value;
        }

        public bool IsSensitive()
            => false;

        public Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
            => Rock.Utility.ExpressionHelper.PropertyFilterExpression( filterValues, parameterExpression, propertyName, propertyType );

        public object SortValue( object parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
            => FormatValue( parentControl, value, configurationValues, true );

        public object ValueAsFieldType( string value, Dictionary<string, ConfigurationValue> configurationValues )
            => value;

        public object ValueAsFieldType( object parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
            => value;
    }
}

namespace Rock.Field.Types
{
    public partial class BlockTemplateFieldType
    {
        private static readonly Guid _CustomGuid = new Guid( "ffffffff-ffff-ffff-ffff-ffffffffffff" );

        /// <summary>
        /// Gets the template value from either the pre-defined template or the custom template content.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The content of the selected template.</returns>
        public static string GetTemplateContent( string value )
        {
            var values = value.Split( new[] { '|' }, 2 );

            if ( values.Length >= 1 )
            {
                if ( values[0].AsGuid() == _CustomGuid && values.Length >= 2 )
                {
                    return values[1];
                }
                else
                {
                    return DefinedValueCache.Get( values[0].AsGuid() )?.Description ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }

    public partial class KeyValueListFieldType
    {
        public List<KeyValuePair<string, object>> GetValuesFromString( object ignored, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return new List<KeyValuePair<string, object>>();
        }
    }

    public partial class SSNFieldType
    {
        /// <summary>
        /// Unencrypts and strips any non-numeric characters from value.
        /// </summary>
        /// <param name="encryptedValue">The encrypted value.</param>
        /// <returns></returns>
        public static string UnencryptAndClean( string encryptedValue )
        {
            if ( encryptedValue.IsNotNullOrWhiteSpace() )
            {
                string ssn = Rock.Security.Encryption.DecryptString( encryptedValue );
                if ( !string.IsNullOrEmpty( ssn ) )
                {
                    return ssn.AsNumeric(); ;
                }
            }

            return string.Empty;
        }
    }

    public partial class StepProgramStepTypeFieldType
    {
        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="stepProgramGuid">The step program unique identifier.</param>
        /// <param name="stepTypeGuid">The step type unique identifier.</param>
        public static void ParseDelimitedGuids( string value, out Guid? stepProgramGuid, out Guid? stepTypeGuid )
        {
            var parts = ( value ?? string.Empty ).Split( '|' );

            if ( parts.Length == 1 )
            {
                // If there is only one guid, assume it is the type
                stepProgramGuid = null;
                stepTypeGuid = parts[0].AsGuidOrNull();
                return;
            }

            stepProgramGuid = parts.Length > 0 ? parts[0].AsGuidOrNull() : null;
            stepTypeGuid = parts.Length > 1 ? parts[1].AsGuidOrNull() : null;
        }
    }

    public partial class StepProgramStepStatusFieldType
    {
        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="stepProgramGuid">The step program unique identifier.</param>
        /// <param name="stepStatusGuid">The step status unique identifier.</param>
        public static void ParseDelimitedGuids( string value, out Guid? stepProgramGuid, out Guid? stepStatusGuid )
        {
            var parts = ( value ?? string.Empty ).Split( '|' );

            if ( parts.Length == 1 )
            {
                // If there is only one guid, assume it is the status
                stepProgramGuid = null;
                stepStatusGuid = parts[0].AsGuidOrNull();
                return;
            }

            stepProgramGuid = parts.Length > 0 ? parts[0].AsGuidOrNull() : null;
            stepStatusGuid = parts.Length > 1 ? parts[1].AsGuidOrNull() : null;
        }
    }

    public partial class ValueFilterFieldType
    {
        /// <summary>
        /// Gets the filter object that can be used to evaluate an object against the filter.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>A CompoundFilter object that can be used to evaluate the truth of the filter.</returns>
        public static FilterExpression GetFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            return FilterExpression.FromJsonOrNull( value );
        }
    }

    public enum DataEntryRequirementLevelSpecifier
    {
        /// <summary>
        /// No requirement level has been specified for this data element.
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// The data element is available but not required.
        /// </summary>
        Optional = 1,
        /// <summary>
        /// The data element is available and required.
        /// </summary>
        Required = 2,
        /// <summary>
        /// The data element is not available.
        /// </summary>
        Unavailable = 3
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
