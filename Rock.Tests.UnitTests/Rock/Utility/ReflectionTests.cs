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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.UnitTests.Rock.Utility
{
    #region Test Classes

    /// <summary>
    /// 
    /// </summary>
    internal class RootTestClass
    {
        /// <summary>
        /// 
        /// </summary>
        internal class SecondTestClass
        {
            /// <summary>
            /// 
            /// </summary>
            internal class ThirdTestClass
            {
                /// <summary>
                /// 
                /// </summary>
                internal class FourthTestClass
                {
                }

                /// <summary>
                /// 
                /// </summary>
                /// <typeparam name="T_A">The type of a.</typeparam>
                /// <typeparam name="T_B">The type of the b.</typeparam>
                /// <typeparam name="T_C">The type of the c.</typeparam>
                internal class GenericInnerClass<T_A, T_B, T_C>
                {
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class OtherRootTestClass
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T_A">The type of a.</typeparam>
    /// <typeparam name="T_B">The type of the b.</typeparam>
    /// <typeparam name="T_C">The type of the c.</typeparam>
    internal class GenericRootTestClass<T_A, T_B, T_C>
    {
    }

    #endregion Test Classes

    #region GetFriendlyName

    /// <summary>
    /// Tests for Reflection.cs
    /// </summary>
    [TestClass]
    public class ReflectionTests
    {
        /// <summary>
        /// GetFriendlyName should handle simple types.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="type">The type.</param>
        [TestMethod]
        [DataRow( "", null )]
        [DataRow( "RootTestClass", typeof( RootTestClass ) )]
        [DataRow( "OtherRootTestClass", typeof( OtherRootTestClass ) )]
        public void GetFriendlyName_ShouldHandleSimpleTypes( string expected, Type type )
        {
            Assert.AreEqual( expected, Reflection.GetFriendlyName( type ) );
        }

        /// <summary>
        /// GetFriendlyName should handle nested types.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="type">The type.</param>
        [TestMethod]
        [DataRow( "RootTestClass.SecondTestClass", typeof( RootTestClass.SecondTestClass ) )]
        [DataRow( "RootTestClass.SecondTestClass.ThirdTestClass", typeof( RootTestClass.SecondTestClass.ThirdTestClass ) )]
        [DataRow( "RootTestClass.SecondTestClass.ThirdTestClass.FourthTestClass", typeof( RootTestClass.SecondTestClass.ThirdTestClass.FourthTestClass ) )]
        public void GetFriendlyName_ShouldHandleNesting( string expected, Type type )
        {
            Assert.AreEqual( expected, Reflection.GetFriendlyName( type ) );
        }

        /// <summary>
        /// GetFriendlyName should handle generics.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="type">The type.</param>
        [TestMethod]
        [DataRow( "GenericRootTestClass<RootTestClass, RootTestClass, OtherRootTestClass>", typeof( GenericRootTestClass<RootTestClass, RootTestClass, OtherRootTestClass> ) )]
        [DataRow( "GenericRootTestClass<RootTestClass, RootTestClass.SecondTestClass, OtherRootTestClass>", typeof( GenericRootTestClass<RootTestClass, RootTestClass.SecondTestClass, OtherRootTestClass> ) )]
        [DataRow( "GenericRootTestClass<RootTestClass, RootTestClass.SecondTestClass.ThirdTestClass.GenericInnerClass<RootTestClass, RootTestClass.SecondTestClass, OtherRootTestClass>, OtherRootTestClass>", typeof( GenericRootTestClass<RootTestClass, RootTestClass.SecondTestClass.ThirdTestClass.GenericInnerClass<RootTestClass, RootTestClass.SecondTestClass, OtherRootTestClass>, OtherRootTestClass> ) )]
        [DataRow( "RootTestClass.SecondTestClass.ThirdTestClass.GenericInnerClass<RootTestClass, RootTestClass.SecondTestClass.ThirdTestClass.GenericInnerClass<RootTestClass, RootTestClass.SecondTestClass, OtherRootTestClass>, OtherRootTestClass>", typeof( RootTestClass.SecondTestClass.ThirdTestClass.GenericInnerClass<RootTestClass, RootTestClass.SecondTestClass.ThirdTestClass.GenericInnerClass<RootTestClass, RootTestClass.SecondTestClass, OtherRootTestClass>, OtherRootTestClass> ) )]
        public void GetFriendlyName_ShouldHandleGenerics( string expected, Type type )
        {
            Assert.AreEqual( expected, Reflection.GetFriendlyName( type ) );
        }
    }

    #endregion GetFriendlyName
}
