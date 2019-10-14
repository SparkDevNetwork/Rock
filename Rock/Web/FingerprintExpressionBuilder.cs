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
using System.CodeDom;
using System.ComponentModel;
using System.Web.Compilation;
using System.Web.UI;

namespace Rock.Web
{
    /// <summary>
    /// An ExpressionBuilder that can be used to fingerprint URL properties of server controls
    /// </summary>
    [ExpressionPrefix( "Fingerprint" )]
    [ExpressionEditor( "FingerprintExpressionBuilder" )]
    public class FingerprintExpressionBuilder : ExpressionBuilder
    {
        /// <summary>
        /// Gets the eval data.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="target">The target.</param>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        public static object GetEvalData( string expression, Type target, string entry )
        {
            string result = Fingerprint.Tag( System.Web.VirtualPathUtility.ToAbsolute( expression ) );
            return result;
        }

        /// <summary>
        /// When overridden in a derived class, returns code that is used during page execution to obtain the evaluated expression.
        /// </summary>
        /// <param name="entry">The object that represents information about the property bound to by the expression.</param>
        /// <param name="parsedData">The object containing parsed data as returned by <see cref="M:System.Web.Compilation.ExpressionBuilder.ParseExpression(System.String,System.Type,System.Web.Compilation.ExpressionBuilderContext)" />.</param>
        /// <param name="context">Contextual information for the evaluation of the expression.</param>
        /// <returns>
        /// A <see cref="T:System.CodeDom.CodeExpression" /> that is used for property assignment.
        /// </returns>
        public override CodeExpression GetCodeExpression( BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context )
        {
            // from http://msdn.microsoft.com/en-us/library/system.web.compilation.expressionbuilder.getcodeexpression.aspx
            Type type1 = entry.DeclaringType;
            PropertyDescriptor descriptor1 = TypeDescriptor.GetProperties( type1 )[entry.PropertyInfo.Name];
            CodeExpression[] expressionArray1 = new CodeExpression[3];
            expressionArray1[0] = new CodePrimitiveExpression( entry.Expression.Trim() );
            expressionArray1[1] = new CodeTypeOfExpression( type1 );
            expressionArray1[2] = new CodePrimitiveExpression( entry.Name );
            return new CodeCastExpression( descriptor1.PropertyType, new CodeMethodInvokeExpression( new CodeTypeReferenceExpression( base.GetType() ), "GetEvalData", expressionArray1 ) );
        }


        /// <summary>
        /// When overridden in a derived class, returns a value indicating whether the current <see cref="T:System.Web.Compilation.ExpressionBuilder" /> object supports no-compile pages.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.Compilation.ExpressionBuilder" /> supports expression evaluation; otherwise, false.</returns>
        public override bool SupportsEvaluate
        {
            get { return true; }
        }
    }
}
