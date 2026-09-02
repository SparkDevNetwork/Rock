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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Lava;

/// <summary>
/// Documents how Rock's Lava (Fluid) engine evaluates <c>{% if %}</c> conditionals
/// against null, undefined, empty, and populated values using the four operator
/// forms authors commonly reach for: truthy check, <c>== null</c>, <c>== empty</c>,
/// and <c>== ''</c>.
///
/// Each scenario is a <see cref="TestMethodAttribute"/> with one row per
/// operator so a failure in one operator does not mask the others. Rows carry
/// the expected rendered output for that operator (the literal "X" when the
/// conditional was true, or an empty string when it was false).
/// </summary>
[TestClass]
public class LavaConditionalLogicTests
{
    #region Helpers

    /// <summary>
    /// Renders <paramref name="template"/> against the supplied merge values
    /// through every active Lava engine and asserts the trimmed output equals
    /// <paramref name="expected"/>.
    /// </summary>
    private static void AssertConditionalOutput( string template, IDictionary<string, object> mergeValues, string expected, Action setup = null )
    {
        using var rockApp = TestHelper.CreateScopedRockApp();

        setup?.Invoke();

        var engines = LavaUnitTestHelper.CurrentInstance.CreateActiveTestEngines();

        foreach ( var engine in engines )
        {
            LavaService.SetCurrentEngine( engine );

            var result = template.ResolveMergeFields( mergeValues, "all" );

            Assert.AreEqual(
                expected,
                result?.Trim(),
                $"Engine '{engine.GetType().Name}' produced unexpected output for template: {template}" );
        }
    }

    /// <summary>
    /// Builds a minimal <see cref="Person"/> for tests that need a CurrentPerson
    /// with a known set of property values. The <see cref="Person.Attributes"/>
    /// and <see cref="Person.AttributeValues"/> dictionaries are initialized so
    /// the Attribute filter does not attempt to hit the (mocked) database via
    /// <c>LoadAttributes()</c>.
    /// </summary>
    private static Person CreateTestPerson( string middleName = null, int? recordStatusReasonValueId = null )
    {
        return new Person
        {
            Id = 1,
            Guid = new Guid( "11111111-1111-1111-1111-111111111111" ),
            FirstName = "Ted",
            NickName = "Ted",
            LastName = "Decker",
            MiddleName = middleName,
            RecordStatusReasonValueId = recordStatusReasonValueId,
            Attributes = new Dictionary<string, AttributeCache>(),
            AttributeValues = new Dictionary<string, AttributeValueCache>()
        };
    }

    #endregion

    #region Explicit Null

    [TestMethod]
    [DataRow( "", "{% if tNull %}X{% endif %}" )]
    [DataRow( "X", "{% if tNull == null %}X{% endif %}" )]
    [DataRow( "", "{% if tNull == empty %}X{% endif %}" )]
    [DataRow( "", "{% if tNull == '' %}X{% endif %}" )]
    public void Conditional_AgainstExplicitNull_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "tNull", null } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Undefined Variable

    [TestMethod]
    [DataRow( "", "{% if tDoesNotExist %}X{% endif %}" )]
    [DataRow( "X", "{% if tDoesNotExist == null %}X{% endif %}" )]
    [DataRow( "", "{% if tDoesNotExist == empty %}X{% endif %}" )]
    [DataRow( "", "{% if tDoesNotExist == '' %}X{% endif %}" )]
    public void Conditional_AgainstUndefinedVariable_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary();

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Empty String

    [TestMethod]
    [DataRow( "X", "{% if tEmpty %}X{% endif %}" )]
    [DataRow( "", "{% if tEmpty == null %}X{% endif %}" )]
    [DataRow( "X", "{% if tEmpty == empty %}X{% endif %}" )]
    [DataRow( "X", "{% if tEmpty == '' %}X{% endif %}" )]
    public void Conditional_AgainstEmptyString_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "tEmpty", string.Empty } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Non-Empty String

    [TestMethod]
    [DataRow( "X", "{% if tHasValue %}X{% endif %}" )]
    [DataRow( "", "{% if tHasValue == null %}X{% endif %}" )]
    [DataRow( "", "{% if tHasValue == empty %}X{% endif %}" )]
    [DataRow( "", "{% if tHasValue == '' %}X{% endif %}" )]
    public void Conditional_AgainstNonEmptyString_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "tHasValue", "Hello" } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Attribute Filter: Attribute Key Does Not Exist On Person

    [TestMethod]
    [DataRow( "X", "{% assign attributeNotExists = CurrentPerson | Attribute:'Nope' %}{% if attributeNotExists %}X{% endif %}" )]
    [DataRow( "", "{% assign attributeNotExists = CurrentPerson | Attribute:'Nope' %}{% if attributeNotExists == null %}X{% endif %}" )]
    [DataRow( "X", "{% assign attributeNotExists = CurrentPerson | Attribute:'Nope' %}{% if attributeNotExists == empty %}X{% endif %}" )]
    [DataRow( "X", "{% assign attributeNotExists = CurrentPerson | Attribute:'Nope' %}{% if attributeNotExists == '' %}X{% endif %}" )]
    public void Conditional_AgainstNonExistentPersonAttribute_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", CreateTestPerson() } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Attribute Filter: Attribute Value Is Blank

    [TestMethod]
    [DataRow( "X", "{% assign attributeBlank = CurrentPerson | Attribute:'TestGroup' %}{% if attributeBlank %}X{% endif %}" )]
    [DataRow( "", "{% assign attributeBlank = CurrentPerson | Attribute:'TestGroup' %}{% if attributeBlank == null %}X{% endif %}" )]
    [DataRow( "X", "{% assign attributeBlank = CurrentPerson | Attribute:'TestGroup' %}{% if attributeBlank == empty %}X{% endif %}" )]
    [DataRow( "X", "{% assign attributeBlank = CurrentPerson | Attribute:'TestGroup' %}{% if attributeBlank == '' %}X{% endif %}" )]
    public void Conditional_AgainstBlankPersonAttributeValue_ProducesExpectedOutput( string expected, string template )
    {
        var person = CreateTestPerson();
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", person } };

        AssertConditionalOutput( template, mergeValues, expected, () =>
        {
            var rockContext = RockApp.Current.CreateRockContext();

            var personEntityTypeId = EntityTypeCache.Get<Person>( true, rockContext ).Id;

            rockContext.Set<FieldType>().Add( new FieldType
            {
                Id = 1,
                Guid = Rock.SystemGuid.FieldType.TEXT.AsGuid(),
                Name = "Text",
                IsSystem = true,
            } );

            rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
            {
                Id = 1,
                EntityTypeId = personEntityTypeId,
                Key = "TestGroup",
                Name = "Test Group",
                FieldTypeId = 1,
                IsActive = true
            } );

            rockContext.Set<AttributeValue>().Add( new AttributeValue
            {
                EntityId = person.Id,
                AttributeId = 1,
                Value = string.Empty,
            } );

            person.LoadAttributes( rockContext );
        } );
    }

    #endregion

    #region Populated Person Attribute Value

    [TestMethod]
    [DataRow( "X", "{% assign attributePopulated = CurrentPerson | Attribute:'Position' %}{% if attributePopulated %}X{% endif %}" )]
    [DataRow( "", "{% assign attributePopulated = CurrentPerson | Attribute:'Position' %}{% if attributePopulated == null %}X{% endif %}" )]
    [DataRow( "", "{% assign attributePopulated = CurrentPerson | Attribute:'Position' %}{% if attributePopulated == empty %}X{% endif %}" )]
    [DataRow( "", "{% assign attributePopulated = CurrentPerson | Attribute:'Position' %}{% if attributePopulated == '' %}X{% endif %}" )]
    public void Conditional_AgainstPopulatedPersonAttributeValue_ProducesExpectedOutput( string expected, string template )
    {
        // Inverse of "Attribute Value Is Blank": confirms the Attribute filter's
        // non-empty success path flows through the conditional operators the
        // same way a plain non-empty string does, so the blank-attribute
        // assertions above cannot be masked by a bug that always returns "".
        var person = CreateTestPerson();
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", person } };

        AssertConditionalOutput( template, mergeValues, expected, () =>
        {
            var rockContext = RockApp.Current.CreateRockContext();

            var personEntityTypeId = EntityTypeCache.Get<Person>( true, rockContext ).Id;

            rockContext.Set<FieldType>().Add( new FieldType
            {
                Id = 1,
                Guid = Rock.SystemGuid.FieldType.TEXT.AsGuid(),
                Name = "Text",
                IsSystem = true,
            } );

            rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
            {
                Id = 1,
                EntityTypeId = personEntityTypeId,
                Key = "Position",
                Name = "Position",
                FieldTypeId = 1,
                IsActive = true
            } );

            rockContext.Set<AttributeValue>().Add( new AttributeValue
            {
                EntityId = person.Id,
                AttributeId = 1,
                Value = "Elder",
            } );

            person.LoadAttributes( rockContext );
        } );
    }

    #endregion

    #region Null Person Property (nullable int)

    [TestMethod]
    [DataRow( "", "{% if CurrentPerson.RecordStatusReasonValueId %}X{% endif %}" )]
    [DataRow( "X", "{% if CurrentPerson.RecordStatusReasonValueId == null %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.RecordStatusReasonValueId == empty %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.RecordStatusReasonValueId == '' %}X{% endif %}" )]
    public void Conditional_AgainstNullPersonProperty_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", CreateTestPerson( recordStatusReasonValueId: null ) } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Populated Person Property (nullable int)

    [TestMethod]
    [DataRow( "X", "{% if CurrentPerson.RecordStatusReasonValueId %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.RecordStatusReasonValueId == null %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.RecordStatusReasonValueId == empty %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.RecordStatusReasonValueId == '' %}X{% endif %}" )]
    public void Conditional_AgainstPopulatedNullableIntPersonProperty_ProducesExpectedOutput( string expected, string template )
    {
        // Inverse of "Null Person Property": confirms == null and == empty
        // discriminate correctly when the nullable int property actually
        // has a value.
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", CreateTestPerson( recordStatusReasonValueId: 5 ) } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Empty Person Property (string)

    [TestMethod]
    [DataRow( "X", "{% if CurrentPerson.MiddleName %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.MiddleName == null %}X{% endif %}" )]
    [DataRow( "X", "{% if CurrentPerson.MiddleName == empty %}X{% endif %}" )]
    [DataRow( "X", "{% if CurrentPerson.MiddleName == '' %}X{% endif %}" )]
    public void Conditional_AgainstEmptyPersonProperty_ProducesExpectedOutput( string expected, string template )
    {
        // This test is for a string property that is not null, but is empty.
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", CreateTestPerson( middleName: string.Empty ) } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Populated Person Property (string)

    [TestMethod]
    [DataRow( "X", "{% if CurrentPerson.LastName %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.LastName == null %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.LastName == empty %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.LastName == '' %}X{% endif %}" )]
    public void Conditional_AgainstPopulatedStringPersonProperty_ProducesExpectedOutput( string expected, string template )
    {
        // Inverse of "Empty Person Property" and "Invalid Person Property":
        // confirms property access on a Person actually reads the underlying
        // value and that a populated string flows through the conditional
        // operators the same way a plain non-empty string does. LastName is
        // "Decker" via CreateTestPerson.
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", CreateTestPerson() } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Invalid (missing) Person Property

    [TestMethod]
    [DataRow( "", "{% if CurrentPerson.Nope %}X{% endif %}" )]
    [DataRow( "X", "{% if CurrentPerson.Nope == null %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.Nope == empty %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson.Nope == '' %}X{% endif %}" )]
    public void Conditional_AgainstInvalidPersonProperty_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", CreateTestPerson() } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Populated Array (two items)

    [TestMethod]
    [DataRow( "X", "{% if testArray %}X{% endif %}" )]
    [DataRow( "", "{% if testArray == null %}X{% endif %}" )]
    [DataRow( "", "{% if testArray == empty %}X{% endif %}" )]
    [DataRow( "", "{% if testArray == '' %}X{% endif %}" )]
    public void Conditional_AgainstPopulatedArray_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "testArray", new List<object> { "one", "two" } } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Empty Array

    [TestMethod]
    [DataRow( "X", "{% if testArray %}X{% endif %}" )]
    [DataRow( "X", "{% if testArray == null %}X{% endif %}" )]
    [DataRow( "X", "{% if testArray == empty %}X{% endif %}" )]
    [DataRow( "", "{% if testArray == '' %}X{% endif %}" )]
    public void Conditional_AgainstEmptyArray_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "testArray", new List<object>() } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Populated Dictionary

    [TestMethod]
    [DataRow( "X", "{% if testDictionary %}X{% endif %}" )]
    [DataRow( "", "{% if testDictionary == null %}X{% endif %}" )]
    [DataRow( "", "{% if testDictionary == empty %}X{% endif %}" )]
    [DataRow( "", "{% if testDictionary == '' %}X{% endif %}" )]
    public void Conditional_AgainstPopulatedDictionary_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary
        {
            { "testDictionary", new Dictionary<string, object> { { "CalculatedValue", 89 } } }
        };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region Empty Dictionary

    [TestMethod]
    [DataRow( "X", "{% if testDictionary %}X{% endif %}" )]
    [DataRow( "X", "{% if testDictionary == null %}X{% endif %}" )]
    [DataRow( "X", "{% if testDictionary == empty %}X{% endif %}" )]
    [DataRow( "", "{% if testDictionary == '' %}X{% endif %}" )]
    public void Conditional_AgainstEmptyDictionary_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "testDictionary", new Dictionary<string, object>() } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region CurrentPerson Is Null

    [TestMethod]
    [DataRow( "", "{% if CurrentPerson %}X{% endif %}" )]
    [DataRow( "X", "{% if CurrentPerson == null %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson == empty %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson == '' %}X{% endif %}" )]
    public void Conditional_AgainstNullCurrentPerson_ProducesExpectedOutput( string expected, string template )
    {
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", null } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion

    #region CurrentPerson Is Populated

    [TestMethod]
    [DataRow( "X", "{% if CurrentPerson %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson == null %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson == empty %}X{% endif %}" )]
    [DataRow( "", "{% if CurrentPerson == '' %}X{% endif %}" )]
    public void Conditional_AgainstPopulatedCurrentPerson_ProducesExpectedOutput( string expected, string template )
    {
        // Inverse of "CurrentPerson is Null": confirms the object-level
        // conditional operators discriminate between a null and a populated
        // CurrentPerson reference.
        var mergeValues = new LavaDataDictionary { { "CurrentPerson", CreateTestPerson() } };

        AssertConditionalOutput( template, mergeValues, expected );
    }

    #endregion
}
