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

using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Factory methods for seeding commonly needed entities into a mocked
    /// <see cref="RockContext"/>. Each method adds the entity (and any related
    /// rows) to the appropriate DbSet, wires navigation properties explicitly
    /// (the mocked context performs no FK or navigation-property fixup), assigns
    /// a unique non-zero Id, and returns the created entity so the caller can
    /// reference it or wire it into further relationships.
    /// </summary>
    public static class MockData
    {
        #region Methods

        /// <summary>
        /// Creates a <see cref="Person"/> together with a primary
        /// <see cref="PersonAlias"/>, adding both to the context and wiring the
        /// alias so that queries over <c>PersonAlias.Person</c> and the person's
        /// <c>Aliases</c> collection resolve against the mocked context.
        /// </summary>
        /// <param name="rockContext">The mocked context to seed.</param>
        /// <param name="firstName">The person's first name.</param>
        /// <param name="lastName">The person's last name.</param>
        /// <param name="guid">The person's unique identifier; a new one is generated when not supplied.</param>
        /// <returns>The created <see cref="Person"/> with its primary alias attached.</returns>
        public static Person CreatePerson( RockContext rockContext, string firstName = "Test", string lastName = "Person", Guid? guid = null )
        {
            var person = new Person
            {
                Id = GetNextId<Person>( rockContext ),
                Guid = guid ?? Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName
            };
            rockContext.Set<Person>().Add( person );

            var personAlias = new PersonAlias
            {
                Id = GetNextId<PersonAlias>( rockContext ),
                Guid = Guid.NewGuid(),
                PersonId = person.Id,
                AliasPersonId = person.Id,
                AliasPersonGuid = person.Guid,
                Person = person
            };
            rockContext.Set<PersonAlias>().Add( personAlias );

            person.Aliases = new List<PersonAlias> { personAlias };

            // Wire the primary alias so code that resolves the person's acting alias
            // (for example the authorization self-lockout guard) has one to read.
            person.PrimaryAliasId = personAlias.Id;

            return person;
        }

        /// <summary>
        /// Creates a <see cref="DefinedValue"/> with the specified unique
        /// identifier and value, adding it to the context.
        /// </summary>
        /// <param name="rockContext">The mocked context to seed.</param>
        /// <param name="guid">The unique identifier for the defined value.</param>
        /// <param name="value">The value of the defined value.</param>
        /// <param name="definedTypeId">The optional identifier of the owning defined type.</param>
        /// <returns>The created <see cref="DefinedValue"/>.</returns>
        public static DefinedValue CreateDefinedValue( RockContext rockContext, Guid guid, string value, int? definedTypeId = null )
        {
            var definedValue = new DefinedValue
            {
                Id = GetNextId<DefinedValue>( rockContext ),
                Guid = guid,
                Value = value,
                DefinedTypeId = definedTypeId ?? 0
            };

            // Wire the DefinedType navigation when the owning type has been seeded,
            // since the mocked context performs no navigation-property fixup.
            if ( definedTypeId.HasValue )
            {
                definedValue.DefinedType = rockContext.Set<DefinedType>().FirstOrDefault( dt => dt.Id == definedTypeId.Value );
            }

            rockContext.Set<DefinedValue>().Add( definedValue );

            return definedValue;
        }

        /// <summary>
        /// Creates a <see cref="FieldType"/> and adds it to the context so that
        /// <see cref="Rock.Web.Cache.FieldTypeCache"/> can resolve it (report and
        /// attribute code resolves a field's type through that cache).
        /// </summary>
        /// <param name="rockContext">The mocked context to seed.</param>
        /// <param name="guid">The field type's unique identifier (typically a <see cref="Rock.SystemGuid.FieldType"/> value).</param>
        /// <param name="name">The field type's name.</param>
        /// <param name="className">The fully qualified field type class name (for example <c>Rock.Field.Types.TextFieldType</c>).</param>
        /// <param name="assembly">The assembly the field type class lives in. Defaults to <c>Rock</c>.</param>
        /// <returns>The created <see cref="FieldType"/>.</returns>
        public static FieldType CreateFieldType( RockContext rockContext, Guid guid, string name, string className, string assembly = "Rock" )
        {
            var fieldType = new FieldType
            {
                Id = GetNextId<FieldType>( rockContext ),
                Guid = guid,
                Name = name,
                Class = className,
                Assembly = assembly
            };

            rockContext.Set<FieldType>().Add( fieldType );

            return fieldType;
        }

        /// <summary>
        /// Creates a <see cref="Rock.Model.Attribute"/> and adds it to the context so
        /// that <see cref="Rock.Web.Cache.AttributeCache"/> can resolve it.
        /// </summary>
        /// <param name="rockContext">The mocked context to seed.</param>
        /// <param name="key">The attribute key.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="fieldTypeId">The field type id, or <c>0</c> when no field type is needed.</param>
        /// <param name="entityTypeId">The owning entity type id, or <c>null</c> for a global attribute.</param>
        /// <param name="defaultValue">The attribute's default value, if any.</param>
        /// <param name="isActive">Whether the attribute is active.</param>
        /// <returns>The created <see cref="Rock.Model.Attribute"/>.</returns>
        public static Rock.Model.Attribute CreateAttribute( RockContext rockContext, string key, string name, int fieldTypeId = 0, int? entityTypeId = null, string defaultValue = null, bool isActive = true )
        {
            var attribute = new Rock.Model.Attribute
            {
                Id = GetNextId<Rock.Model.Attribute>( rockContext ),
                Guid = Guid.NewGuid(),
                Key = key,
                Name = name,
                FieldTypeId = fieldTypeId,
                EntityTypeId = entityTypeId,
                DefaultValue = defaultValue,
                IsActive = isActive
            };

            rockContext.Set<Rock.Model.Attribute>().Add( attribute );

            return attribute;
        }

        /// <summary>
        /// Gets the next available integer identifier for the entity type in the
        /// mocked context. This lets seeded entities have a unique, non-zero Id
        /// without requiring a <c>SaveChanges</c> call.
        /// </summary>
        /// <typeparam name="T">The entity type being seeded.</typeparam>
        /// <param name="rockContext">The mocked context.</param>
        /// <returns>The next Id to assign.</returns>
        private static int GetNextId<T>( RockContext rockContext ) where T : class, IEntity
        {
            var set = rockContext.Set<T>();

            return set.Any() ? set.Max( e => e.Id ) + 1 : 1;
        }

        #endregion Methods
    }
}
