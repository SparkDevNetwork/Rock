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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// </summary>
    [Description( "Select people that have notes returned by a specified Data View" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person with Note" )]
    public class NoteDataViewFilter : RelatedDataViewFilterBase<Rock.Model.Person, Rock.Model.Note>
    {
        #region Overrides
    
        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var settings = new FilterSettings( selection );

            var context = ( RockContext ) serviceInstance.Context;

            // Get the Data View that defines the set of related records from which entities can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the note records.
            var noteService = new NoteService( context );

            var personNoteQuery = noteService.Queryable();

            if ( dataView != null )
            {
                personNoteQuery = DataComponentSettingsHelper.FilterByDataView( personNoteQuery, dataView, noteService );
            }

            // Get all of the People corresponding to the qualifying note records.
            var personService = new PersonService( ( RockContext ) serviceInstance.Context );

            var personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();
            personNoteQuery = personNoteQuery.Where( a => a.NoteType.EntityTypeId == personEntityTypeId );

            var qry = personService.Queryable()
                        .Where( p => personNoteQuery.Any( xx => xx.EntityId == p.Id) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}