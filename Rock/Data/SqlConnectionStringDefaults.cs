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
using Microsoft.Data.SqlClient;

namespace Rock.Data;

/// <summary>
/// Connection string defaults for code that connects with
/// <c>Microsoft.Data.SqlClient</c> using the connection string Rock's EF6
/// workload uses with <c>System.Data.SqlClient</c>.
/// </summary>
internal static class SqlConnectionStringDefaults
{
    /*
        11/25/26 - CLAUDE

        Microsoft.Data.SqlClient 4.0 and later encrypts by default and checks
        the server certificate, while System.Data.SqlClient, which RockContext
        uses, encrypts only when the connection string asks for it. Without
        Encrypt in the connection string, RockContext therefore connects to a
        server with a self-signed certificate (local development, Docker,
        on-premises), and Microsoft.Data.SqlClient would not.

        So the certificate is trusted only when the connection string does not
        ask for encryption. When it does (Encrypt=True or Strict, as Azure SQL
        connection strings do), TrustServerCertificate is left as the site set
        it, which is what RockContext already uses successfully. Forcing it on
        there would skip a certificate check the site chose.

        Reason: Connect wherever RockContext connects, without skipping a certificate check the site asked for.
    */

    /// <summary>
    /// Trusts the server certificate unless the connection string asks for
    /// encryption, matching what <c>System.Data.SqlClient</c> does with the
    /// same connection string. When it asks for encryption, the connection
    /// string's own <c>TrustServerCertificate</c> value is kept.
    /// </summary>
    /// <param name="builder">The connection string builder to update.</param>
    internal static void TrustServerCertificateUnlessEncryptionRequested( SqlConnectionStringBuilder builder )
    {
        var isEncryptionRequested = builder.ShouldSerialize( "Encrypt" )
            && builder.Encrypt != SqlConnectionEncryptOption.Optional;

        if ( isEncryptionRequested )
        {
            return;
        }

        builder.TrustServerCertificate = true;
    }
}
