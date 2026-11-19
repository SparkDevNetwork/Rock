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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.PersonSkill;

public partial class PersonSkillTests
{
    #region UpdatePhoneNumber

    [TestMethod]
    public void UpdatePhoneNumber_OtherPersonWithoutEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var phoneDefinedType = SeedDefinedType( rockContext, 1, SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() );
        var phoneType = MockData.CreateDefinedValue( rockContext, Guid.NewGuid(), "Mobile", phoneDefinedType.Id );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var target = CreatePersonWithEmail( rockContext, "Target" );
        var phone = SeedPhoneNumber( rockContext, target, phoneType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.UpdatePhoneNumber( IdHasher.Instance.GetHash( target.Id ), NewNumber, IdHasher.Instance.GetHash( phoneType.Id ) );

        AssertNotAuthorized( result );
        Assert.AreEqual( OriginalNumber, phone.Number );
        Assert.AreEqual( 1, rockContext.Set<PhoneNumber>().Count() );
    }

    [TestMethod]
    public void UpdatePhoneNumber_SelfWithNoPhoneOfType_AddsPhone()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var phoneDefinedType = SeedDefinedType( rockContext, 1, SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() );
        var phoneType = MockData.CreateDefinedValue( rockContext, Guid.NewGuid(), "Mobile", phoneDefinedType.Id );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.UpdatePhoneNumber( IdHasher.Instance.GetHash( caller.Id ), NewNumber, IdHasher.Instance.GetHash( phoneType.Id ), isMessagingEnabled: true );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );

        var phone = rockContext.Set<PhoneNumber>().Single();
        Assert.AreEqual( caller.Id, phone.PersonId );
        Assert.AreEqual( phoneType.Id, phone.NumberTypeValueId );
        Assert.AreEqual( NewNumber, phone.Number );
        Assert.IsTrue( phone.IsMessagingEnabled );
    }

    [TestMethod]
    public void UpdatePhoneNumber_NonPhoneTypeValue_ReturnsLookupsRequired()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var phoneDefinedType = SeedDefinedType( rockContext, 1, SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() );
        var otherDefinedType = SeedDefinedType( rockContext, 2, Guid.NewGuid() );
        MockData.CreateDefinedValue( rockContext, Guid.NewGuid(), "Mobile", phoneDefinedType.Id );
        var nonPhoneType = MockData.CreateDefinedValue( rockContext, Guid.NewGuid(), "Active", otherDefinedType.Id );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.UpdatePhoneNumber( IdHasher.Instance.GetHash( caller.Id ), NewNumber, IdHasher.Instance.GetHash( nonPhoneType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        CollectionAssert.AreEqual( new List<string> { LookupsRequiredMessage }, result.GetErrorMessages().ToList() );
        Assert.AreEqual( 0, rockContext.Set<PhoneNumber>().Count() );
    }

    #endregion

    #region UpdatePhoneNumber Support

    private static DefinedType SeedDefinedType( RockContext rockContext, int id, Guid guid )
    {
        var definedType = new DefinedType
        {
            Id = id,
            Guid = guid,
            Name = $"Type{id}",
            IsActive = true
        };

        rockContext.Set<DefinedType>().Add( definedType );

        return definedType;
    }

    private static PhoneNumber SeedPhoneNumber( RockContext rockContext, Rock.Model.Person person, int numberTypeValueId )
    {
        var phone = new PhoneNumber
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonId = person.Id,
            Person = person,
            NumberTypeValueId = numberTypeValueId,
            Number = OriginalNumber,
            CountryCode = "1"
        };

        rockContext.Set<PhoneNumber>().Add( phone );

        return phone;
    }

    #endregion
}
