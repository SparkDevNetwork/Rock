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
using System.Linq.Expressions;

namespace Rock.Reporting
{
    /// <summary>
    /// Interface for Data Select components that can be used as a communication recipient column
    /// </summary>
    public interface IRecipientDataSelect
    {
        /// <summary>
        /// Gets the type of the recipient column field.
        /// </summary>
        /// <value>
        /// The type of the recipient column field.
        /// </value>
        Type RecipientColumnFieldType { get; }

        /// <summary>
        /// Gets the recipient person identifier expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        Expression GetRecipientPersonIdExpression( System.Data.Entity.DbContext context, MemberExpression entityIdProperty, string selection );
    }
}
