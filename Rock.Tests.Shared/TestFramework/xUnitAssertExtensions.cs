using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    public static partial class AssertExtensions
    {
        public static void Equal<T>( this Assert assert, T expected, T actual )
        {
            Assert.AreEqual( expected, actual );
        }
        public static void Equal( this Assert assert, System.String expected, System.String actual, System.Boolean ignoreCase )
        {
            Assert.AreEqual( expected, actual, ignoreCase );
        }
        public static void Equal( this Assert assert, System.Single expected, System.Single actual, System.Single delta )
        {
            Assert.AreEqual( expected, actual, delta );
        }
        public static void Equal( this Assert assert, System.Double expected, System.Double actual, System.Double delta, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, delta, message, parameters );
        }
        public static void Equal( this Assert assert, System.Object expected, System.Object actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, message, parameters );
        }
        public static void Equal( this Assert assert, System.Single expected, System.Single actual, System.Single delta, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, delta, message, parameters );
        }
        public static void Equal<T>( this Assert assert, T expected, T actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, message, parameters );
        }
        public static void Equal( this Assert assert, System.Double expected, System.Double actual, System.Double delta )
        {
            Assert.AreEqual( expected, actual, delta );
        }
        public static void Equal( this Assert assert, System.Object expected, System.Object actual )
        {
            Assert.AreEqual( expected, actual );
        }
        public static void Equal( this Assert assert, System.String expected, System.String actual, System.Boolean ignoreCase, System.Globalization.CultureInfo culture )
        {
            Assert.AreEqual( expected, actual, ignoreCase, culture );
        }
        public static void Equal( this Assert assert, System.String expected, System.String actual, System.Boolean ignoreCase, System.Globalization.CultureInfo culture, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, ignoreCase, culture, message, parameters );
        }
        public static void Equal( this Assert assert, System.String expected, System.String actual, System.Boolean ignoreCase, System.String message, params System.Object[] parameters )
        {
            Assert.AreEqual( expected, actual, ignoreCase, message, parameters );
        }
        public static void NotEqual( this Assert assert, System.String notExpected, System.String actual, System.Boolean ignoreCase, System.Globalization.CultureInfo culture )
        {
            Assert.AreNotEqual( notExpected, actual, ignoreCase, culture );
        }
        public static void NotEqual( this Assert assert, System.String notExpected, System.String actual, System.Boolean ignoreCase, System.Globalization.CultureInfo culture, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, ignoreCase, culture, message, parameters );
        }
        public static void NotEqual( this Assert assert, System.Single notExpected, System.Single actual, System.Single delta )
        {
            Assert.AreNotEqual( notExpected, actual, delta );
        }
        public static void NotEqual( this Assert assert, System.String notExpected, System.String actual, System.Boolean ignoreCase )
        {
            Assert.AreNotEqual( notExpected, actual, ignoreCase );
        }
        public static void NotEqual( this Assert assert, System.Double notExpected, System.Double actual, System.Double delta )
        {
            Assert.AreNotEqual( notExpected, actual, delta );
        }
        public static void NotEqual( this Assert assert, System.Double notExpected, System.Double actual, System.Double delta, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, delta, message, parameters );
        }
        public static void NotEqual( this Assert assert, System.Single notExpected, System.Single actual, System.Single delta, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, delta, message, parameters );
        }
        public static void NotEqual( this Assert assert, System.String notExpected, System.String actual, System.Boolean ignoreCase, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, ignoreCase, message, parameters );
        }
        public static void NotEqual<T>( this Assert assert, T notExpected, T actual )
        {
            Assert.AreNotEqual( notExpected, actual );
        }
        public static void NotEqual<T>( this Assert assert, T notExpected, T actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, message, parameters );
        }
        public static void NotEqual( this Assert assert, System.Object notExpected, System.Object actual, System.String message, params System.Object[] parameters )
        {
            Assert.AreNotEqual( notExpected, actual, message, parameters );
        }
        public static void NotEqual( this Assert assert, System.Object notExpected, System.Object actual )
        {
            Assert.AreNotEqual( notExpected, actual );
        }
        public static void False( this Assert assert, System.Boolean condition )
        {
            Assert.IsFalse( condition );
        }
        public static void False( this Assert assert, System.Boolean condition, System.String message, params System.Object[] parameters )
        {
            Assert.IsFalse( condition, message, parameters );
        }
        public static void NotNull( this Assert assert, System.Object value, System.String message, params System.Object[] parameters )
        {
            Assert.IsNotNull( value, message, parameters );
        }
        public static void NotNull( this Assert assert, System.Object value )
        {
            Assert.IsNotNull( value );
        }
        public static void Null( this Assert assert, System.Object value, System.String message, params System.Object[] parameters )
        {
            Assert.IsNull( value, message, parameters );
        }
        public static void Null( this Assert assert, System.Object value )
        {
            Assert.IsNull( value );
        }
        public static void True( this Assert assert, System.Boolean condition, System.String message, params System.Object[] parameters )
        {
            Assert.IsTrue( condition, message, parameters );
        }
        public static void True( this Assert assert, System.Boolean condition )
        {
            Assert.IsTrue( condition );
        }

        public static void Empty( this Assert assert, System.Collections.IEnumerable collection )
        {
            Assert.That.IsEmpty( collection );
        }
    }
}
