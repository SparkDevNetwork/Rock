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

using Microsoft.Data.SqlClient;

namespace Rock.BaselineTool;

/// <summary>
/// A connection to the baseline database with the few query shapes the
/// commands need. Every command runs on one connection and, when given,
/// one transaction.
/// </summary>
internal sealed class BaselineDatabase : IDisposable
{
    private readonly SqlConnection _connection;

    /// <summary>
    /// The open connection.
    /// </summary>
    public SqlConnection Connection => _connection;

    /// <summary>
    /// The transaction commands run in, or <c>null</c>.
    /// </summary>
    public SqlTransaction Transaction { get; set; }

    /// <summary>
    /// Opens a connection. The server certificate is trusted, as Rock's
    /// EF6 workload does, and the application is named for the server's
    /// session list.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    public BaselineDatabase( string connectionString )
    {
        _connection = new SqlConnection( new SqlConnectionStringBuilder( connectionString )
        {
            TrustServerCertificate = true,
            ApplicationName = "Rock Baseline Tool"
        }.ConnectionString );
        _connection.Open();
    }

    /// <summary>
    /// Runs a statement and returns the number of rows affected.
    /// </summary>
    public int Execute( string sql, params (string Name, object Value)[] parameters )
    {
        using var command = CreateCommand( sql, parameters );
        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Runs a query and returns the first column of the first row.
    /// </summary>
    public object Scalar( string sql, params (string Name, object Value)[] parameters )
    {
        using var command = CreateCommand( sql, parameters );
        var value = command.ExecuteScalar();

        return value == DBNull.Value ? null : value;
    }

    /// <summary>
    /// Runs a query and returns each row as an array of values, with
    /// <c>DBNull</c> as <c>null</c>.
    /// </summary>
    public List<object[]> Query( string sql, params (string Name, object Value)[] parameters )
    {
        var rows = new List<object[]>();

        using var command = CreateCommand( sql, parameters );
        using var reader = command.ExecuteReader();
        while ( reader.Read() )
        {
            var values = new object[reader.FieldCount];
            reader.GetValues( values );

            for ( var i = 0; i < values.Length; i++ )
            {
                values[i] = values[i] == DBNull.Value ? null : values[i];
            }

            rows.Add( values );
        }

        return rows;
    }

    /// <summary>
    /// Whether a user table exists in the <c>dbo</c> schema.
    /// </summary>
    public bool TableExists( string table )
    {
        return Scalar( "SELECT OBJECT_ID( @Name, N'U' );", ("@Name", $"[dbo].[{table}]") ) != null;
    }

    /// <summary>
    /// Whether a column exists on a <c>dbo</c> table.
    /// </summary>
    public bool ColumnExists( string table, string column )
    {
        return Scalar( "SELECT COL_LENGTH( @Table, @Column );", ("@Table", $"[dbo].[{table}]"), ("@Column", column) ) != null;
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public void Dispose()
    {
        Transaction?.Dispose();
        _connection.Dispose();
    }

    /// <summary>
    /// Creates a command with a generous timeout, since cleanup and
    /// export work on whole tables.
    /// </summary>
    private SqlCommand CreateCommand( string sql, (string Name, object Value)[] parameters )
    {
        var command = new SqlCommand( sql, _connection, Transaction ) { CommandTimeout = 600 };

        foreach ( var (name, value) in parameters )
        {
            command.Parameters.AddWithValue( name, value ?? DBNull.Value );
        }

        return command;
    }
}
