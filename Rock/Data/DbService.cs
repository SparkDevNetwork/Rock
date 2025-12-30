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
using System.Data.SqlClient;

using Microsoft.EntityFrameworkCore;

using Rock.Configuration;

namespace Rock.Data
{
    /// <summary>
    ///  Helper class to provide native SQL methods 
    /// </summary>
    public class DbService
    {
        private DbContext Context { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbService"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public DbService( DbContext dbContext )
        {
            this.Context = dbContext;
        }

        /// <summary>
        /// Gets the data table from SQL command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataTable.</returns>
        public DataTable GetDataTableFromSqlCommand( string commandText, CommandType commandType, Dictionary<string, object> parameters )
        {
            var dataSet = GetDataSetFromSqlCommand( commandText, commandType, parameters );

            if ( dataSet.Tables.Count > 0 )
            {
                return dataSet.Tables[0];
            }

            return null;
        }

        /// <summary>
        /// Gets the data set from a SQL command.
        /// </summary>
        /// <param name="commandText">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataSet.</returns>
        public DataSet GetDataSetFromSqlCommand( string commandText, CommandType commandType, Dictionary<string, object> parameters )
        {
            string connectionString = this.Context.Database.Connection.ConnectionString;
            if ( !string.IsNullOrWhiteSpace( connectionString ) )
            {
                using ( SqlConnection con = new SqlConnection( connectionString ) )
                {
                    con.Open();

                    using ( SqlCommand sqlCommand = new SqlCommand( commandText, con ) )
                    {

                        if ( Context.Database.GetCommandTimeout().HasValue )
                        {
                            sqlCommand.CommandTimeout = Context.Database.GetCommandTimeout().Value;
                        }

                        sqlCommand.CommandType = commandType;

                        if ( parameters != null )
                        {
                            foreach ( var parameter in parameters )
                            {
                                if ( parameter.Value is SqlParameter existingParam )
                                {
                                    if ( string.IsNullOrWhiteSpace( existingParam.ParameterName ) )
                                    {
                                        existingParam.ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key;
                                    }
                                    sqlCommand.Parameters.Add( existingParam );
                                }
                                else
                                {
                                    SqlParameter sqlParam = new SqlParameter();
                                    sqlParam.ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key;
                                    sqlParam.Value = parameter.Value;
                                    sqlCommand.Parameters.Add( sqlParam );
                                }
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

        #region Methods

        /// <summary>
        /// Gets a data reader
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static IDataReader GetDataReader( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            string connectionString = RockApp.Current.InitializationSettings.ConnectionString;
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
        /// Static method to get a data table. See also <seealso cref="GetDataTableFromSqlCommand(string, CommandType, Dictionary{string, object})"/>.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static DataTable GetDataTable( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            return GetDataTable( query, commandType, parameters, null, false );
        }

        /// <summary>
        /// Static method to get a data table. See also <seealso cref="GetDataTableFromSqlCommand(string, CommandType, Dictionary{string, object})"/>.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="timeOut">The time out.</param>
        /// <returns></returns>
        public static DataTable GetDataTable( string query, CommandType commandType, Dictionary<string, object> parameters, int? timeOut )
        {
            return GetDataTable( query, commandType, parameters, timeOut, false );
        }

        /// <summary>
        /// Static method to get a data table. See also <seealso cref="GetDataTableFromSqlCommand(string, CommandType, Dictionary{string, object})"/>.
        /// </summary>
        /// <param name="sqlQuery">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="timeOut">The time out.</param>
        /// <param name="rollbackTransaction">if set to <c>true</c> the transaction is rolled back (to prevent any nefarious update from occurring).</param>
        /// <returns></returns>
        public static DataTable GetDataTable( string sqlQuery, CommandType commandType, Dictionary<string, object> parameters, int? timeOut, bool rollbackTransaction = false )
        {
            string connectionString = RockApp.Current.InitializationSettings.ConnectionString;
            if ( string.IsNullOrWhiteSpace( connectionString ) )
            {
                throw new ArgumentException( "connectionString cannot be null/empty", nameof( connectionString ) );
            }

            DataTable resultTable = new DataTable();

            using ( SqlConnection con = new SqlConnection( connectionString ) )
            {
                using ( SqlCommand sqlCommand = new SqlCommand( sqlQuery, con ) )
                {
                    sqlCommand.CommandType = commandType;

                    if ( timeOut.HasValue )
                    {
                        sqlCommand.CommandTimeout = timeOut.Value;
                    }

                    // Add parameters from the dictionary to the SqlCommand
                    if ( parameters != null )
                    {
                        foreach ( KeyValuePair<string, object> parameter in parameters )
                        {
                            // The parameter key should start with "@" for named parameters
                            SqlParameter sqlParam = new SqlParameter();
                            sqlParam.ParameterName = parameter.Key.StartsWith( "@" ) ? parameter.Key : "@" + parameter.Key;
                            sqlParam.Value = parameter.Value;
                            sqlCommand.Parameters.Add( sqlParam );
                        }
                    }

                    using ( SqlDataAdapter adapter = new SqlDataAdapter( sqlCommand ) )
                    {
                        con.Open();

                        if ( rollbackTransaction )
                        {
                            using ( var tran = con.BeginTransaction( System.Data.IsolationLevel.ReadCommitted ) )
                            {
                                try
                                {
                                    sqlCommand.Transaction = tran;
                                    adapter.Fill( resultTable );
                                }
                                finally
                                {
                                    tran.Rollback();
                                }
                            }
                        }
                        else
                        {
                            adapter.Fill( resultTable );
                        }
                    }
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Static method to get a data set. See also <seealso cref="GetDataSetFromSqlCommand(string, CommandType, Dictionary{string, object})"/>.
        /// This method will wrap the query in a transaction and roll it back to prevent any unexpected updates.
        ///
        /// NOTE: This is used by the SQL Command block.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="timeOut">The time out in seconds.</param>
        /// <returns>the DataSet results for the given query.</returns>
        public static DataSet GetDataSetReadOnly( string query, CommandType commandType, Dictionary<string, object> parameters, int? timeOut = null )
        {
            return GetDataSet( query, commandType, parameters, timeOut, false, rollbackTransaction: true );
        }

        /// <summary>
        /// Static method to get a data set. See also <seealso cref="GetDataSetFromSqlCommand(string, CommandType, Dictionary{string, object})"/>.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="timeOut">The time out in seconds.</param>
        /// <returns>the DataSet results for the given query.</returns>
        public static DataSet GetDataSet( string query, CommandType commandType, Dictionary<string, object> parameters, int? timeOut = null )
        {
            return GetDataSet( query, commandType, parameters, timeOut, false, false );
        }

        /// <summary>
        /// Gets only the schema information (columns, etc) that would result from query. Handy if you need to know which columns the query would return.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="timeOut">The time out in seconds.</param>
        /// <returns>the DataSet results for the given query.</returns>
        public static DataSet GetDataSetSchema( string query, CommandType commandType, Dictionary<string, object> parameters, int? timeOut = null )
        {
            return GetDataSet( query, commandType, parameters, timeOut, true, false );
        }

        /// <summary>
        /// Gets a data set.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="timeOut">The time out in seconds.</param>
        /// <param name="schemaOnly">if set to <c>true</c> only schema is returned.</param>
        /// <param name="rollbackTransaction">if set to <c>true</c> the transaction is rolled back (to prevent any nefarious update from occurring).</param>
        /// <returns>DataSet.</returns>
        private static DataSet GetDataSet( string query, CommandType commandType, Dictionary<string, object> parameters, int? timeOut = null, bool schemaOnly = false, bool rollbackTransaction = false )
        {
            string connectionString = RockApp.Current.InitializationSettings.ConnectionString;
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
                        if ( rollbackTransaction )
                        {
                            using ( var tran = con.BeginTransaction( System.Data.IsolationLevel.ReadCommitted ) )
                            {
                                try
                                {
                                    sqlCommand.Transaction = tran;
                                    FillDataSetOrSchema( adapter, dataSet, schemaOnly );
                                }
                                finally
                                {
                                    tran.Rollback();
                                }
                            }
                        }
                        else
                        {
                            FillDataSetOrSchema( adapter, dataSet, schemaOnly );
                        }
                        return dataSet;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Helper method to keep GetDataSet method clean and DRY.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="dataSet"></param>
        /// <param name="schemaOnly"></param>
        private static void FillDataSetOrSchema( SqlDataAdapter adapter, DataSet dataSet, bool schemaOnly )
        {
            if ( schemaOnly )
            {
                adapter.FillSchema( dataSet, SchemaType.Source );
            }
            else
            {
                adapter.Fill( dataSet );
            }
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
            var connectionString = RockApp.Current.InitializationSettings.ConnectionString;
            return ExecuteCommand( connectionString, query, commandType, parameters, commandTimeout );
        }

        /// <summary>
        /// Executes the query, and returns number of rows affected
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandTimeout">The command timeout (seconds)</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static int ExecuteCommand( string connectionString, string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null, int? commandTimeout = null )
        {
            if ( string.IsNullOrWhiteSpace( connectionString ) )
            {
                return 0;
            }

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

                    if ( commandTimeout.HasValue )
                    {
                        sqlCommand.CommandTimeout = commandTimeout.Value;
                    }

                    return sqlCommand.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [RockObsolete( "1.16" )]
        [Obsolete( @"Please use the static method ExecuteScalar( string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null, int? commandTimeout = null ) instead" )]
        public static object ExecuteScaler( string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null )
        {
            return ExecuteScalar( query, commandType, parameters, null );
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandTimeout">The command timeout (seconds)</param>
        /// <returns></returns>
        public static object ExecuteScalar( string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null, int? commandTimeout = null )
        {
            string connectionString = RockApp.Current.InitializationSettings.ConnectionString;
            return ExecuteScalar( connectionString, query, commandType, parameters, commandTimeout );
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object ExecuteScalar( string connectionString, string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null )
        {
            return ExecuteScalar( connectionString, query, commandType, parameters, null );
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandTimeout">The command timeout (seconds)</param>
        /// <returns></returns>
        public static object ExecuteScalar( string connectionString, string query, CommandType commandType, Dictionary<string, object> parameters, int? commandTimeout )
        {
            if ( string.IsNullOrWhiteSpace( connectionString ) )
            {
                return null;
            }

            using ( SqlConnection con = new SqlConnection( connectionString ) )
            {
                return ExecuteScalar( con, query, commandType, parameters, commandTimeout );
            }
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object ExecuteScalar( SqlConnection connection, string query, CommandType commandType = CommandType.Text, Dictionary<string, object> parameters = null )
        {
            return ExecuteScalar( connection, query, commandType, parameters, null );
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandTimeout">The command timeout (seconds)</param>
        /// <returns></returns>
        public static object ExecuteScalar( SqlConnection connection, string query, CommandType commandType, Dictionary<string, object> parameters, int? commandTimeout )
        {
            if ( connection == null )
            {
                return null;
            }

            if ( connection.State == System.Data.ConnectionState.Closed )
            {
                connection.Open();
            }

            using ( SqlCommand sqlCommand = new SqlCommand( query, connection ) )
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

                if ( commandTimeout.HasValue )
                {
                    sqlCommand.CommandTimeout = commandTimeout.Value;
                }

                return sqlCommand.ExecuteScalar();
            }
        }

        #endregion
    }
}
