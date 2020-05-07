using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Utility.ExtensionMethods
{
    [TestClass]
    public class TypeExtensionsTests
    {
        #region IsDescendentOf

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.IsDescendentOf(Type, Type)"/> method
        /// and verifies that it returns true when testing a non-generic class
        /// against another non-generic class that it does inherit from.
        /// </summary>
        [TestMethod]
        public void IsDescendentOf_ValidSimpleClass()
        {
            var type = typeof( System.Reflection.MethodInfo );
            var baseType = typeof( System.Reflection.MemberInfo );

            Assert.That.IsTrue( type.IsDescendentOf( baseType ) );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.IsDescendentOf(Type, Type)"/> method
        /// and verifies that it returns false when testing a non-generic class
        /// against another non-generic class that it does not inherit from.
        /// </summary>
        [TestMethod]
        public void IsDescendentOf_InvalidSimpleClass()
        {
            var type = typeof( System.Reflection.MethodInfo );
            var baseType = typeof( System.Reflection.PropertyInfo );

            Assert.That.IsFalse( type.IsDescendentOf( baseType ) );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.IsDescendentOf(Type, Type)"/> method
        /// and verifies that it returns true when testing a non-generic class
        /// against a generic class that it does inherit from.
        /// </summary>
        [TestMethod]
        public void IsDescendentOf_ValidGenericBaseClass()
        {
            var type = typeof( Web.Cache.DefinedValueCache );
            var baseType = typeof( Web.Cache.EntityCache<,> );

            Assert.That.IsTrue( type.IsDescendentOf( baseType ) );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.IsDescendentOf(Type, Type)"/> method
        /// and verifies that it returns true when testing a non-generic class
        /// against a generic class that it does not inherit from.
        /// </summary>
        [TestMethod]
        public void IsDescendentOf_InvalidGenericBaseClass()
        {
            var type = typeof( System.Reflection.PropertyInfo );
            var baseType = typeof( Web.Cache.EntityCache<,> );

            Assert.That.IsFalse( type.IsDescendentOf( baseType ) );
        }

        #endregion

        #region GetGenericArgumentsOfBaseType

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.GetGenericArgumentsOfBaseType(Type, Type)"/>
        /// method and verifies that it throws an exception when passed a
        /// non-generic base type.
        /// </summary>
        [TestMethod]
        public void GetGenericArgumentsOfBaseType_InvalidNotGenericType()
        {
            var type = typeof( Web.Cache.DefinedValueCache );
            var baseType = typeof( System.Reflection.PropertyInfo );

            Assert.That.ThrowsException<ArgumentException>( () => type.GetGenericArgumentsOfBaseType( baseType ) );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.GetGenericArgumentsOfBaseType(Type, Type)"/>
        /// method and verifies that it throws an exception when passed a
        /// generic base type that the type does not descend from.
        /// </summary>
        [TestMethod]
        public void GetGenericArgumentsOfBaseType_InvalidNotDescendentOf()
        {
            var type = typeof( Web.Cache.DefinedValueCache );
            var baseType = typeof( System.Collections.Generic.List<> );

            Assert.That.ThrowsException<ArgumentException>( () => type.GetGenericArgumentsOfBaseType( baseType ) );
        }

        /// <summary>
        /// Tests the <see cref="global::Rock.ExtensionMethods.GetGenericArgumentsOfBaseType(Type, Type)"/>
        /// method and verifies that it returns the expected argument types.
        /// </summary>
        [TestMethod]
        public void GetGenericArgumentsOfBaseType_Valid()
        {
            var type = typeof( Web.Cache.DefinedValueCache );
            var baseType = typeof( Web.Cache.EntityCache<,> );

            var types = type.GetGenericArgumentsOfBaseType( baseType );

            Assert.That.AreEqual( 2, types.Length );
            Assert.That.AreEqual( typeof( Web.Cache.DefinedValueCache ), types[0] );
            Assert.That.AreEqual( typeof( global::Rock.Model.DefinedValue ), types[1] );
        }

        #endregion
    }
}
