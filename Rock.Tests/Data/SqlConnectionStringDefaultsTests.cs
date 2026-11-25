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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;

namespace Rock.Tests.Data;

/// <summary>
/// Tests that <see cref="SqlConnectionStringDefaults"/> trusts the server
/// certificate only where <c>System.Data.SqlClient</c> would not check it.
/// </summary>
[TestClass]
public class SqlConnectionStringDefaultsTests
{
    [TestMethod]
    [DataRow( "Server=db;Database=Rock", DisplayName = "No Encrypt" )]
    [DataRow( "Server=db;Database=Rock;TrustServerCertificate=False", DisplayName = "No Encrypt, explicit TrustServerCertificate=False" )]
    [DataRow( "Server=db;Database=Rock;Encrypt=False;TrustServerCertificate=False", DisplayName = "Encrypt=False" )]
    [DataRow( "Server=db;Database=Rock;Encrypt=Optional", DisplayName = "Encrypt=Optional" )]
    public void TrustServerCertificate_EncryptionNotRequested_IsTrusted( string connectionString )
    {
        Assert.IsTrue( Apply( connectionString ).TrustServerCertificate, "System.Data.SqlClient would not check the certificate, so it is trusted." );
    }

    [TestMethod]
    [DataRow( "Server=db;Database=Rock;Encrypt=True;TrustServerCertificate=False", DisplayName = "Azure style" )]
    [DataRow( "Server=db;Database=Rock;Encrypt=True", DisplayName = "Encrypt=True, TrustServerCertificate not set" )]
    [DataRow( "Server=db;Database=Rock;Encrypt=Mandatory", DisplayName = "Encrypt=Mandatory" )]
    public void TrustServerCertificate_EncryptionRequestedWithoutTrust_IsNotTrusted( string connectionString )
    {
        Assert.IsFalse( Apply( connectionString ).TrustServerCertificate, "The site asked for encryption, so the certificate is checked as the site set it." );
    }

    [TestMethod]
    public void TrustServerCertificate_EncryptionRequestedWithTrust_KeepsTrust()
    {
        Assert.IsTrue( Apply( "Server=db;Database=Rock;Encrypt=True;TrustServerCertificate=True" ).TrustServerCertificate );
    }

    private static SqlConnectionStringBuilder Apply( string connectionString )
    {
        var builder = new SqlConnectionStringBuilder( connectionString );

        SqlConnectionStringDefaults.TrustServerCertificateUnlessEncryptionRequested( builder );

        return builder;
    }
}
