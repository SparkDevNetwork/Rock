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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Tests.Classes.Common;

/// <summary>
/// Tests for <see cref="KeyNameResult"/>, the lightweight reference shape returned
/// by agent tools.
/// </summary>
[TestClass]
public class KeyNameResultTests
{
    /// <summary>
    /// <see cref="KeyNameResult.FromEntity"/> is the construction path that
    /// populates the guid. Results built any other way must set it explicitly.
    /// </summary>
    [TestMethod]
    public void FromEntity_AssignsGuidAndId()
    {
        var entity = new Rock.Model.DefinedValue
        {
            Id = 7,
            Guid = new Guid( "0B2F6C41-8D3A-4E5B-9A17-2C4D6E8F1A03" ),
            Value = "Test"
        };

        var result = KeyNameResult.FromEntity( entity );

        Assert.IsNotNull( result.Guid );
        Assert.AreEqual( entity.Guid, result.Guid.Value );
        Assert.IsNotNull( result.Id );
        Assert.AreEqual( entity.Id, result.Id.Value );
    }

    /// <summary>
    /// A null entity is a normal case for an optional navigation property, so it
    /// returns null rather than throwing.
    /// </summary>
    [TestMethod]
    public void FromEntity_WithNullEntity_ReturnsNull()
    {
        Assert.IsNull( KeyNameResult.FromEntity( null ) );
    }

    /// <summary>
    /// The constructors leave the guid null. Only <see cref="KeyNameResult.FromEntity"/>
    /// and an explicit object initializer populate it.
    /// </summary>
    /// <remarks>
    /// The three argument constructor accepts a guid and does not assign it. That
    /// is asserted here rather than treated as a defect, so a caller who reaches
    /// for it can see from the tests that it will not do what the signature
    /// suggests.
    /// </remarks>
    [TestMethod]
    public void Constructors_LeaveGuidNull()
    {
        var fromId = new KeyNameResult( 1, "Test" );
        var fromKey = new KeyNameResult( "abc123", "Test" );
        var fromIdAndGuid = new KeyNameResult( 1, Guid.NewGuid(), "Test" );

        Assert.IsNull( fromId.Guid );
        Assert.IsNull( fromKey.Guid );
        Assert.IsNull( fromIdAndGuid.Guid, "The three argument constructor does not assign its guid argument." );
    }
}
