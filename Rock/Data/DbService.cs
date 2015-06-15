// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Rock.Data
{
    /// <summary>
    ///  Helper class to provide native SQL methods 
    /// </summary>
    public class DbService
    {

        #region Methods

        /// <summary>
        /// Gets a data reader.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static IDataReader GetDataReader( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            string connectionString = GetConnectionString();
            if ( !string.IsNullOrWhiteSpace( connectionString ) )
            {
                SqlConnection con = new SqlConnection( connectionString );
                con.Open();

                SqlCommand sqlCommand = new SqlCommand( query, con );
                sqlCommand.CommandType = commandType;

                if ( parameters != null )
                {
                    foreach ( var parameter in parameters )
                    {
                        SqlParameter sqlParam = new SqlParameter();
                        sqlParam.ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key;
                        sqlParam.Value = parameter.Value;
                        sqlCommand.Parameters.Add( sqlParam );
                    }
                }

                return sqlCommand.ExecuteReader( CommandBehavior.CloseConnection );
            }

            return null;
        }

        /// <summary>
        /// Gets a data table.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static DataTable GetDataTable( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            DataSet dataSet = DbService.GetDataSet( query, commandType, parameters );
            if ( dataSet.Tables.Count > 0 )
            {
                return dataSet.Tables[0];
            }

            return null;
        }

        /// <summary>
        /// Gets a data set.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="timeOut">The time out.</param>
        /// <returns></returns>
        public static DataSet GetDataSet( string query, CommandType commandType, Dictionary<string, object> parameters, int? timeOut = null )
        {
            string connectionString = GetConnectionString();
            if ( !string.IsNullOrWhiteSpace( connectionString ) )
            {
                using ( SqlConnection con = new SqlConnection( connectionString ) )
                {
                    con.Open();

                    using ( SqlCommand sqlCommand = new SqlCommand( query, con ) )
                    {
                        if ( timeOut.HasValue )
                        {
                            sqlCommand.CommandTimeout = timeOut.Value;
                        }
                        sqlCommand.CommandType = commandType;

                        if ( parameters != null )
                        {
                            foreach ( var parameter in parameters )
                            {
                                SqlParameter sqlParam = new SqlParameter();
                                sqlParam.ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key;
                                sqlParam.Value = parameter.Value;
                                sqlCommand.Parameters.Add( sqlParam );
                            }
                        }

                        SqlDataAdapter adapter = new SqlDataAdapter( sqlCommand );
                        DataSet dataSet = new DataSet( "rockDs" );
                        adapter.Fill( dataSet );
                        return dataSet;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Executes the query, and returns number of rows affected
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandTimeout">The command timeout (seconds)</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static int ExecuteCommand( string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null, int? commandTimeout = null )
        {
            string connectionString = GetConnectionString();
            if ( !string.IsNullOrWhiteSpace( connectionString ) )
            {
                using ( SqlConnection con = new SqlConnection( connectionString ) )
                {
                    con.Open();

                    using ( SqlCommand sqlCommand = new SqlCommand( query, con ) )
                    {
                        sqlCommand.CommandType = commandType;

                        if ( parameters != null )
                        {
                            foreach ( var parameter in parameters )
                            {
                                SqlParameter sqlParam = new SqlParameter();
                                sqlParam.ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key;
                                sqlParam.Value = parameter.Value;
                                sqlCommand.Parameters.Add( sqlParam );
                            }
                        }

                        if (commandTimeout.HasValue)
                        {
                            sqlCommand.CommandTimeout = commandTimeout.Value;
                        }

                        return sqlCommand.ExecuteNonQuery();
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object ExecuteScaler( string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null )
        {
            string connectionString = GetConnectionString();
            if ( !string.IsNullOrWhiteSpace( connectionString ) )
            {
                using ( SqlConnection con = new SqlConnection( connectionString ) )
                {
                    con.Open();

                    using ( SqlCommand sqlCommand = new SqlCommand( query, con ) )
                    {
                        sqlCommand.CommandType = commandType;

                        if ( parameters != null )
                        {
                            foreach ( var parameter in parameters )
                            {
                                SqlParameter sqlParam = new SqlParameter();
                                sqlParam.ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key;
                                sqlParam.Value = parameter.Value;
                                sqlCommand.Parameters.Add( sqlParam );
                            }
                        }

                        return sqlCommand.ExecuteScalar();
                    }
                }
            }

            return null;
        }

        private static string GetConnectionString()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RockContext"];
            if ( connectionString != null )
            {
                return connectionString.ConnectionString;
            }
            return null;
        }

        #endregion
    }
}
