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
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Data
{
    /// <summary>
    /// Tests for <see cref="LinqRuntimeTypeBuilder"/>, which builds the
    /// dynamic row type used by reports.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Reporting )]
    public class LinqRuntimeTypeBuilderTests
    {
        /// <summary>
        /// Each field becomes a public field with a lowercase first letter and
        /// a matching property whose getter reads that field.
        /// </summary>
        [TestMethod]
        public void GetDynamicType_CreatesLowercaseFieldsAndPropertyGetters()
        {
            var fields = new Dictionary<string, Type>
            {
                ["Id"] = typeof( int ),
                ["Entity_FirstName_1"] = typeof( string ),
                [UniqueFieldName()] = typeof( int )
            };

            var dynamicType = LinqRuntimeTypeBuilder.GetDynamicType( fields );

            Assert.IsNotNull( dynamicType );

            var idField = dynamicType.GetField( "id" );
            var firstNameField = dynamicType.GetField( "entity_FirstName_1" );
            Assert.IsNotNull( idField );
            Assert.IsNotNull( firstNameField );
            Assert.AreEqual( typeof( int ), idField.FieldType );
            Assert.AreEqual( typeof( string ), firstNameField.FieldType );

            var idProperty = dynamicType.GetProperty( "Id" );
            var firstNameProperty = dynamicType.GetProperty( "Entity_FirstName_1" );
            Assert.IsNotNull( idProperty );
            Assert.IsNotNull( firstNameProperty );
            Assert.AreEqual( typeof( int ), idProperty.PropertyType );
            Assert.AreEqual( typeof( string ), firstNameProperty.PropertyType );

            var instance = Activator.CreateInstance( dynamicType );
            idField.SetValue( instance, 42 );
            firstNameField.SetValue( instance, "Ted" );

            Assert.AreEqual( 42, idProperty.GetValue( instance ) );
            Assert.AreEqual( "Ted", firstNameProperty.GetValue( instance ) );
        }

        /// <summary>
        /// Two requests with equal field lists get the same cached type.
        /// </summary>
        [TestMethod]
        public void GetDynamicType_SameFieldsReturnSameType()
        {
            var uniqueFieldName = UniqueFieldName();

            var first = LinqRuntimeTypeBuilder.GetDynamicType( new Dictionary<string, Type>
            {
                ["Id"] = typeof( int ),
                [uniqueFieldName] = typeof( string )
            } );

            var second = LinqRuntimeTypeBuilder.GetDynamicType( new Dictionary<string, Type>
            {
                ["Id"] = typeof( int ),
                [uniqueFieldName] = typeof( string )
            } );

            Assert.IsNotNull( first );
            Assert.AreSame( first, second );
        }

        /// <summary>
        /// Field lists that differ only in a field's type get different types.
        /// </summary>
        [TestMethod]
        public void GetDynamicType_DifferentFieldsReturnDifferentTypes()
        {
            var uniqueFieldName = UniqueFieldName();

            var withInt = LinqRuntimeTypeBuilder.GetDynamicType( new Dictionary<string, Type>
            {
                ["Id"] = typeof( int ),
                [uniqueFieldName] = typeof( int )
            } );

            var withNullableInt = LinqRuntimeTypeBuilder.GetDynamicType( new Dictionary<string, Type>
            {
                ["Id"] = typeof( int ),
                [uniqueFieldName] = typeof( int? )
            } );

            Assert.IsNotNull( withInt );
            Assert.IsNotNull( withNullableInt );
            Assert.AreNotSame( withInt, withNullableInt );
            Assert.AreEqual( typeof( int ), withInt.GetField( "e" + uniqueFieldName.Substring( 1 ) ).FieldType );
            Assert.AreEqual( typeof( int? ), withNullableInt.GetField( "e" + uniqueFieldName.Substring( 1 ) ).FieldType );
        }

        /// <summary>
        /// A field name that doesn't start with an uppercase letter can't be
        /// built, and the method returns null rather than throwing.
        /// </summary>
        [TestMethod]
        public void GetDynamicType_FieldNameStartingLowercase_ReturnsNull()
        {
            var dynamicType = LinqRuntimeTypeBuilder.GetDynamicType( new Dictionary<string, Type>
            {
                ["Id"] = typeof( int ),
                ["lowercase_" + Guid.NewGuid().ToString( "N" )] = typeof( string )
            } );

            Assert.IsNull( dynamicType );
        }

        /// <summary>
        /// Several requests asking for the same not-yet-cached type at the
        /// same moment must all get that type back. This is what happens when
        /// several people open the same report right after an app restart.
        /// </summary>
        [TestMethod]
        public void GetDynamicType_ConcurrentCallsForNewType_NeverReturnNull()
        {
            const int rounds = 50;
            const int threadCount = 16;

            for ( var round = 0; round < rounds; round++ )
            {
                var fields = new Dictionary<string, Type>
                {
                    ["Id"] = typeof( int ),
                    ["Entity_LastName_1"] = typeof( string ),
                    [UniqueFieldName()] = typeof( decimal? )
                };

                var results = new Type[threadCount];

                // Dedicated threads, not Parallel.For: the barrier needs every
                // participant running at once, and the thread pool may not
                // provide that many threads right away.
                using ( var barrier = new Barrier( threadCount ) )
                {
                    var threads = Enumerable.Range( 0, threadCount )
                        .Select( i => new Thread( () =>
                        {
                            barrier.SignalAndWait();
                            results[i] = LinqRuntimeTypeBuilder.GetDynamicType( new Dictionary<string, Type>( fields ) );
                        } ) )
                        .ToList();

                    threads.ForEach( t => t.Start() );
                    threads.ForEach( t => t.Join() );
                }

                var nullCount = results.Count( t => t == null );
                Assert.AreEqual( 0, nullCount, $"Round {round}: {nullCount} of {threadCount} concurrent calls returned null." );
                Assert.AreEqual( 1, results.Distinct().Count(), $"Round {round}: concurrent calls returned different types." );
            }
        }

        /// <summary>
        /// Returns a field name that has never been used before, so the field
        /// list it is part of is never already cached.
        /// </summary>
        private static string UniqueFieldName()
        {
            return $"Entity_Unique_{Guid.NewGuid():N}";
        }
    }
}
