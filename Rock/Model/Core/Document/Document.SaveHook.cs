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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Document
    {
        /// <summary>
        /// Save hook implementation for <see cref="Document"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Document>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var document = this.Entity as Document;

                if ( State != EntityContextState.Deleted )
                {
                    if ( !document.IsValidDocument( ( RockContext ) this.RockContext, out string errorMessage ) )
                    {
                        var ex = new DocumentValidationException( errorMessage );
                        ExceptionLogService.LogException( ex );
                        throw ex;
                    }
                }
            }
        }

        #region Exceptions

        /// <summary>
        /// Exception to throw if Document validation rules are invalid (and can't be checked using .IsValid)
        /// </summary>
        /// <seealso cref="System.Exception" />
        public class DocumentValidationException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DocumentValidationException"/> class.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public DocumentValidationException( string message ) : base( message )
            {
            }
        }

        #endregion
    }
}
