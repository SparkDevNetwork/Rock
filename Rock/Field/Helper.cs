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
            Type type = Type.GetType(thetype);

            if ( type != null )
                return ( IFieldType )Activator.CreateInstance( type );
            else
                return ( IFieldType )Activator.CreateInstance( typeof( Rock.Field.Types.TextFieldType ) );
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
            var items = new Dictionary<string, string>();

            if ( configurationValues.ContainsKey( propertyName ) )
            {
                string listSource = configurationValues[ propertyName ].Value;

                var options = new Lava.CommonMergeFieldsOptions();
                options.GetLegacyGlobalMergeFields = false;
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, options );

                listSource = listSource.ResolveMergeFields( mergeFields );

                if ( listSource.ToUpper().Contains( "SELECT" ) && listSource.ToUpper().Contains( "FROM" ) )
                {
                    var tableValues = new List<string>();
                    DataTable dataTable = Rock.Data.DbService.GetDataTable( listSource, CommandType.Text, null );
                    if ( dataTable != null && dataTable.Columns.Contains( "Value" ) && dataTable.Columns.Contains( "Text" ) )
                    {
                        foreach ( DataRow row in dataTable.Rows )
                        {
                            items.AddOrIgnore( row["value"].ToString(), row["text"].ToString() );
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
                            items.AddOrIgnore( keyValueArray[0].Trim(), keyValueArray.Length > 1 ? keyValueArray[1].Trim() : keyValueArray[0].Trim() );
                        }
                    }
                }
            }

            return items;
        }
    }
}