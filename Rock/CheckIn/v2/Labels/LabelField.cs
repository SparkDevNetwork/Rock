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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Rock.Enums.CheckIn.Labels;
using Rock.Lava;
using Rock.Reporting;
using Rock.ViewModels.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// The implementation of a single field for a check-in label that provides
    /// access to all the configuration data as well as the formatted values if
    /// this is a text field.
    /// </summary>
    internal class LabelField
    {
        /// <summary>
        /// Contains any previously parsed configuration objects that match
        /// the specified type. Normally this will only ever contain a single
        /// configuration type, but this allows us to not have to worry about
        /// type checking and also makes us thread safe at the same time.
        /// </summary>
        private readonly ConcurrentDictionary<Type, object> _configurationCache = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// The cached function that can be called internally to determine if
        /// this field matches the label data object.
        /// </summary>
        private Func<object, bool> _isMatchFunction;

        /// <summary>
        /// The bag that describes the details about this field.
        /// </summary>
        public LabelFieldBag Field { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="LabelField"/>.
        /// </summary>
        /// <param name="field">The <see cref="LabelFieldBag"/> object that defines the configuration of the field.</param>
        public LabelField( LabelFieldBag field )
        {
            Field = field;
        }

        /// <summary>
        /// Gets the field configuration values cast to type <typeparamref name="TConfiguration"/>.
        /// </summary>
        /// <typeparam name="TConfiguration">The type of configuration object to get.</typeparam>
        /// <returns>The configuration of type <typeparamref name="TConfiguration"/>.</returns>
        public virtual TConfiguration GetConfiguration<TConfiguration>()
            where TConfiguration : IFieldConfiguration, new()
        {
            return ( TConfiguration ) _configurationCache.GetOrAdd( typeof( TConfiguration ), _ =>
            {
                var bag = new TConfiguration();

                bag.Initialize( Field.ConfigurationValues );

                return bag;
            } );
        }

        /// <summary>
        /// Retrieves a list of formatted values for the <paramref name="printRequest"/>.
        /// </summary>
        /// <param name="printRequest">The print request containing the data for formatting.</param>
        /// <returns>A list of formatted string values.</returns>
        public virtual List<string> GetFormattedValues( PrintLabelRequest printRequest )
        {
            if ( Field.FieldType != LabelFieldType.Text )
            {
                return new List<string>( new[] { string.Empty } );
            }

            var config = GetConfiguration<TextFieldConfiguration>();

            if ( Field.FieldSubType == 0 )
            {
                if ( config.IsDynamicText )
                {
                    var mergeFields = printRequest.GetMergeFields();
                    var text = config.DynamicTextTemplate.ResolveMergeFields( mergeFields );

                    return new List<string>( new[] { text } );
                }
                else
                {
                    return new List<string>( new[] { config.StaticText ?? string.Empty } );
                }
            }

            if ( !printRequest.DataSources.TryGetValue( config.SourceKey, out var source ) )
            {
                return new List<string> { string.Empty };
            }

            var values = source.GetValues( this, printRequest );

            // Deal with any bad field sources. This may due to test prints or
            // in some cases getting external data that is an empty array.
            if ( values == null || values.Count == 0 )
            {
                return new List<string> { string.Empty };
            }

            if ( source.Formatter != null )
            {
                return source.Formatter.GetFormattedValues( values, config.FormatterOptionKey, this, printRequest );
            }
            else
            {
                return values.Select( v => v.ToStringSafe() ).ToList();
            }
        }

        /// <summary>
        /// Checks if this field conditional criteria matches the label data.
        /// </summary>
        /// <param name="labelData">The label data for this label.</param>
        /// <returns><c>true</c> if this field matches; otherwise <c>false</c>.</returns>
        public virtual bool IsMatch( object labelData )
        {
            if ( _isMatchFunction == null )
            {
                if ( Field.ConditionalVisibility == null )
                {
                    _isMatchFunction = _ => true;
                }
                else
                {
                    var builder = new FieldFilterExpressionBuilder();

                    _isMatchFunction = builder.GetIsMatchFunction( Field.ConditionalVisibility, labelData.GetType() );
                }
            }

            return _isMatchFunction( labelData );
        }
    }
}
