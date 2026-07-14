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
using System.Data;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Field
{
    /// <summary>
    /// Helper class for use with <see cref="IFieldType"/> classes
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// Instantiates an instance of a specific field type based on the assembly and class name of the field type
        /// </summary>
        /// <param name="assembly">Assembly Name (i.e. Rock)</param>
        /// <param name="typeName">Class Name (i.e. Rock.Field.Types.Text)</param>
        /// <returns></returns>
        public static IFieldType InstantiateFieldType( string assembly, string typeName )
        {
            string thetype = string.Format( "{0}, {1}", typeName, assembly );
            Type type = Type.GetType( thetype )
                ?? Type.GetType( "Rock.Field.Types.TextFieldType, Rock" );

            if ( type != null )
            {
                return ( IFieldType ) Activator.CreateInstance( type );
            }
            else
            {
                throw new InvalidOperationException( $"Could not create an instance of the field type {thetype}." );
            }
        }

        /// <summary>
        /// Gets the configured values.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetConfiguredValues( Dictionary<string, string> configurationValues )
        {
            return GetConfiguredValues( configurationValues, "values" );
        }

        /// <summary>
        /// Gets the configured values.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetConfiguredValues( Dictionary<string, ConfigurationValue> configurationValues )
        {
            return GetConfiguredValues( configurationValues, "values" );
        }

        /// <summary>
        /// Gets the configured values.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetConfiguredValues( Dictionary<string, ConfigurationValue> configurationValues, string propertyName )
        {
            return GetConfiguredValues( configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ), propertyName );
        }

        /// <summary>
        /// Gets the configured values.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetConfiguredValues( Dictionary<string, string> configurationValues, string propertyName )
        {
            var items = new Dictionary<string, string>();

            if ( configurationValues.ContainsKey( propertyName ) )
            {
                string listSource = configurationValues[ propertyName ];

                var options = new Lava.CommonMergeFieldsOptions();
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, options );

                listSource = listSource.ResolveMergeFields( mergeFields );

                if ( listSource.ToUpper().Contains( "SELECT" ) && listSource.ToUpper().Contains( "FROM" ) )
                {
                    var tableValues = new List<string>();
                    DataTable dataTable = Rock.Data.DbService.GetDataTable( listSource, CommandType.Text, null, null, rollbackTransaction: true );
                    if ( dataTable != null && dataTable.Columns.Contains( "Value" ) && dataTable.Columns.Contains( "Text" ) )
                    {
                        foreach ( DataRow row in dataTable.Rows )
                        {
                            items.TryAdd( row["value"].ToString(), row["text"].ToString() );
                        }
                    }
                }

                else
                {
                    foreach ( string keyvalue in listSource.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                    {
                        var keyValueArray = keyvalue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                        if ( keyValueArray.Length > 0 )
                        {
                            items.TryAdd( keyValueArray[0].Trim(), keyValueArray.Length > 1 ? keyValueArray[1].Trim() : keyValueArray[0].Trim() );
                        }
                    }
                }
            }

            return items;
        }

        #region Field Value Helpers

        /// <summary>
        /// Gets the values from string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public static List<KeyValuePair<string, object>> GetKeyValueListValuesFromString( string value, Dictionary<string, string> configurationValues, bool condensed )
        {
            List<KeyValuePair<string, object>> values = new List<KeyValuePair<string, object>>();

            bool isDefinedType = configurationValues != null && configurationValues.ContainsKey( "definedtype" ) && configurationValues["definedtype"].AsIntegerOrNull().HasValue;

            string[] nameValues = value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

            // url decode array items just in case they were UrlEncoded (in the KeyValueList controls)
            nameValues = nameValues.Select( s => System.Web.HttpUtility.UrlDecode( s ) ).ToArray();

            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' } );
                if ( nameAndValue.Length == 2 )
                {
                    if ( isDefinedType )
                    {
                        var definedValue = DefinedValueCache.Get( nameAndValue[1].AsInteger() );
                        if ( definedValue != null )
                        {
                            values.Add( new KeyValuePair<string, object>( nameAndValue[0], definedValue ) );
                        }
                        else
                        {
                            values.Add( new KeyValuePair<string, object>( nameAndValue[0], nameAndValue[1] ) );
                        }
                    }
                    else
                    {
                        values.Add( new KeyValuePair<string, object>( nameAndValue[0], nameAndValue[1] ) );
                    }
                }
                else
                {
                    values.Add( new KeyValuePair<string, object>( nameAndValue[0], null ) );
                }
            }

            return values;
        }

        /// <summary>
        /// Splits the persisted pipe-delimited value into its four guid slots.
        /// </summary>
        /// <param name="value">The persisted attribute value.</param>
        /// <param name="connectionTypeGuid">The Connection Type guid, if present.</param>
        /// <param name="connectionOpportunityGuid">The Connection Opportunity guid, if present.</param>
        /// <param name="connectionStatusGuid">The Connection Status guid, if present.</param>
        /// <param name="connectionTypeSourceGuid">The Connection Type Source guid, if present.</param>
        public static void ParseConnectionTypeSettingsDelimitedGuids( string value, out Guid? connectionTypeGuid, out Guid? connectionOpportunityGuid, out Guid? connectionStatusGuid, out Guid? connectionTypeSourceGuid )
        {
            var parts = ( value ?? string.Empty ).Split( '|' );
            connectionTypeGuid = parts.Length > 0 ? parts[0].AsGuidOrNull() : null;
            connectionOpportunityGuid = parts.Length > 1 ? parts[1].AsGuidOrNull() : null;
            connectionStatusGuid = parts.Length > 2 ? parts[2].AsGuidOrNull() : null;
            connectionTypeSourceGuid = parts.Length > 3 ? parts[3].AsGuidOrNull() : null;
        }

        /// <summary>
        /// Get a value for this field type from a serialized representation, or return the specified default value.
        /// </summary>
        /// <param name="serialized"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetEnumDeserializedValue<T>( string serialized, T defaultValue )
            where T : struct
        {
            T enumValue;

            var isValid = Enum.TryParse( serialized, out enumValue );

            if ( isValid )
            {
                return enumValue;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Adds the defined value to the attribute configuration. This only
        /// updates the configuration if it is required. If the id already is
        /// selected or the configuration already specifies all values to be
        /// shown then no changes are made. This makes the change but does not
        /// save the changes to the database.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="definedValueId">The defined value identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if SaveChanges() should be called, <c>false</c> otherwise.</returns>
        internal static bool AddDefinedValueToAttributeConfiguration( int attributeId, int definedValueId, RockContext rockContext )
        {
            var qualifier = new AttributeQualifierService( rockContext )
                .Queryable()
                .Where( q => q.AttributeId == attributeId && q.Key == "SelectableDefinedValuesId" )
                .FirstOrDefault();

            if ( qualifier == null || qualifier.Value.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var ids = qualifier.Value.SplitDelimitedValues().AsIntegerList();

            if ( ids.Contains( definedValueId ) )
            {
                return false;
            }

            ids.Add( definedValueId );

            qualifier.Value = string.Join( ",", ids.Select( id => id.ToString() ) );

            return true;
        }

        /// <summary>
        /// Unencrypts and strips any non-numeric characters from value.
        /// </summary>
        /// <param name="encryptedValue">The encrypted value.</param>
        /// <returns></returns>
        internal static string UnencryptAndCleanSocialSecurityNumber( string encryptedValue )
        {
            if ( encryptedValue.IsNotNullOrWhiteSpace() )
            {
                string ssn = Rock.Security.Encryption.DecryptString( encryptedValue );
                if ( !string.IsNullOrEmpty( ssn ) )
                {
                    return ssn.AsNumeric();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="stepProgramGuid">The step program unique identifier.</param>
        /// <param name="stepStatusGuid">The step status unique identifier.</param>
        internal static void ParseStepProgramStatusDelimitedGuids( string value, out Guid? stepProgramGuid, out Guid? stepStatusGuid )
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

        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="stepProgramGuid">The step program unique identifier.</param>
        /// <param name="stepTypeGuid">The step type unique identifier.</param>
        internal static void ParseStepProgramStepTypeDelimitedGuids( string value, out Guid? stepProgramGuid, out Guid? stepTypeGuid )
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

        /// <summary>
        /// Determines whether the Attribute Configuration for the field has IsPassword = True
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        internal static bool IsTextFieldPassword( Dictionary<string, string> configurationValues )
        {
            if ( configurationValues != null && configurationValues.ContainsKey( "ispassword" ) )
            {
                return configurationValues["ispassword"].AsBoolean();
            }

            return false;
        }

        #endregion
    }
}