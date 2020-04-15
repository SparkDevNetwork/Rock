using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    public static partial class AssertExtensions
    {
        public static void AreEqual<T>( this Assert assert, IEnumerable<T> expected, IEnumerable<T> actual )
        {
            CollectionAssert.AreEquivalent( expected.ToList(), actual.ToList() );
        }
        public static void AreEqual<T>( this Assert assert, List<T> expected, List<T> actual )
        {
            CollectionAssert.AreEquivalent( expected, actual );
        }

        public static void AreEqual( this Assert assert, IEnumerable<dynamic> expected, IEnumerable<dynamic> actual )
        {
            CollectionAssert.AreEquivalent( expected.ToList(), actual.ToList() );
        }

        public static void Contains( this Assert assert, System.Collections.ICollection collection, object element )
        {
            CollectionAssert.Contains( collection, element );
        }
        public static void DoesNotContain( this Assert assert, System.Collections.ICollection collection, object element )
        {
            CollectionAssert.DoesNotContain( collection, element );
        }

        public static void IsEmpty( this Assert assert, System.Collections.IEnumerable collection )
        {
            var enumerator = collection.GetEnumerator();

            if ( enumerator.MoveNext() )
            {
                // Contains at least one element.
                throw new AssertFailedException( "Empty collection expected, but one or more elements exist." );
            }
        }

        public static void Single( this Assert assert, System.Collections.IEnumerable collection )
        {
            var enumerator = collection.GetEnumerator();

            if ( enumerator.MoveNext() )
            {
                if ( enumerator.MoveNext() )
                {
                    // More than one element.
                    throw new AssertFailedException( "More than one element exists in the collection." );
                }
            }
            else
            {
                // No elements
                throw new AssertFailedException( "No element exists in the collection." );
            }

            return;
        }
    }
}
