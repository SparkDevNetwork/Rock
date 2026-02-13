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

namespace Rock.Data
{
    /// <summary>
    /// <para>
    /// Used to decorate an extension method to indicate it can be expanded
    /// with the <c>.AsExpandable()</c> method. Methods that are expanded are
    /// replaced with a LINQ expression tree provided by the method specified
    /// in the attribute.
    /// </para>
    ///
    /// <para>
    /// Requirements to expression method:
    /// <list type="bullet">
    ///   <item>Expression method should be in the same class and replaced property of method.</item>
    ///   <item>Method can be private.</item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// When applied to method:
    /// <list type="bullet">
    ///   <item>Expression method should return function expression with the same return type as method return type.</item>
    ///   <item>Method cannot have void return type.</item>
    ///   <item>Parameters in expression method should go in the same order as in substituted method.</item>
    /// </list>
    /// </para>
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true, Inherited = true )]
    internal class ExpandableAttribute : System.Attribute
    {
        /// <summary>
        /// Creates instance of attribute.
        /// </summary>
        /// <param name="methodName">Name of method in the same class that returns substitution expression.</param>
        public ExpandableAttribute( string methodName )
        {
            MethodName = methodName;
        }

        /// <summary>
        /// Name of method in the same class that returns substitution expression.
        /// </summary>
        public string MethodName { get; set; }
    }
}
