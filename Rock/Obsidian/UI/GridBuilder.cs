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

using Rock.ViewModels.Core.Grid;

namespace Rock.Obsidian.UI
{
    /// <summary>
    ///     <para>
    ///     Provides the base functionality to building Grids in Obsidian. This
    ///     class allows you to define the structure (fields) of the grid and
    ///     then generate the data that should be passed to the grid.
    ///     </para>
    ///     <para>
    ///     A grid is comprised of fields that describe the structure of the row
    ///     objects that will be sent to the client. These fields may not be a
    ///     1:1 mapping to the columns displayed by the grid.
    ///     </para>
    /// </summary>
    /// <typeparam name="T">The type of the source collection that will be used to populate the grid.</typeparam>
    public class GridBuilder<T>
    {
        #region Fields

        /// <summary>
        /// The field actions that will be called to generate the cell data for
        /// each field in the grid.
        /// </summary>
        private readonly Dictionary<string, Func<T, object>> _fieldActions = new Dictionary<string, Func<T, object>>();

        /// <summary>
        /// The definition actions that will provide additional field definition
        /// information dynamically. These are primarily used to populate the
        /// attribute data.
        /// </summary>
        private readonly List<Action<GridDefinitionBag>> _definitionActions = new List<Action<GridDefinitionBag>>();

        #endregion

        #region Methods

        /// <summary>
        /// Adds a generic field to the grid definition.
        /// </summary>
        /// <param name="name">The unique name of the field, which will also be used as the row object key.</param>
        /// <param name="valueExpression">The expression used to generate the cell data from the source object.</param>
        /// <returns>A reference to the original <see cref="GridBuilder{T}"/> object that can be used to chain calls.</returns>
        public GridBuilder<T> AddField( string name, Func<T, object> valueExpression )
        {
            if ( name.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( "Field name must not be null, empty or whitespace.", name );
            }

            if ( _fieldActions.ContainsKey( name ) )
            {
                throw new ArgumentException( $"Field '{name}' already exists in grid definition.", name );
            }

            _fieldActions.Add( name, valueExpression );

            AddDefinitionAction( definition =>
            {
                definition.Fields.Add( new FieldDefinitionBag
                {
                    Name = name
                } );
            } );

            return this;
        }

        /// <summary>
        /// Adds a custom definition action. This will be called when building
        /// the grid definition to allow additional modifications to the
        /// definitions before it is returned.
        /// </summary>
        /// <param name="action">The action to be performed on the grid definition.</param>
        /// <returns>A reference to the original <see cref="GridBuilder{T}"/> object that can be used to chain calls.</returns>
        public GridBuilder<T> AddDefinitionAction( Action<GridDefinitionBag> action )
        {
            _definitionActions.Add( action );

            return this;
        }

        /// <summary>
        /// Builds the rows based on the previously defined fields.
        /// </summary>
        /// <param name="items">The items that will be used as the source data.</param>
        /// <returns>A <see cref="GridDataBag"/> of represent the data to display in the grid.</returns>
        public GridDataBag Build( IEnumerable<T> items )
        {
            var fieldKeys = _fieldActions.Keys.ToArray();

            var rows = items
                .Select( item =>
                {
                    // Allocate the row dictionary for the correct size. This
                    // is nearly twice as fast as creating an empty dictionary
                    // and then adding keys to it (175ns vs 289ns). And we are
                    // going to be doing this potentially 100,000 times. That
                    // could save a total of 12ms.
                    var row = new Dictionary<string, object>( fieldKeys.Length );

                    foreach ( var key in fieldKeys )
                    {
                        var value = _fieldActions[key]( item );

                        row[key] = value;
                    }

                    return row;
                } )
                .ToList();

            return new GridDataBag
            {
                Rows = rows
            };
        }

        /// <summary>
        /// Builds the grid definition. This is used to provide additional
        /// structure information about the grid.
        /// </summary>
        /// <returns>A <see cref="GridDefinitionBag"/> object that describes the grid structure.</returns>
        public GridDefinitionBag BuildDefinition()
        {
            var definition = new GridDefinitionBag
            {
                Fields = new List<FieldDefinitionBag>(),
                DynamicFields = new List<DynamicFieldDefinitionBag>(),
                AttributeFields = new List<AttributeFieldDefinitionBag>(),
                ActionUrls = new Dictionary<string, string>()
            };

            foreach ( var action in _definitionActions )
            {
                action( definition );
            }

            return definition;
        }

        #endregion
    }
}
