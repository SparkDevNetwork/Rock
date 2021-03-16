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
using System.Collections.Generic;
using Fluid;
using Fluid.Tags;
using Irony.Parsing;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A factoory responsible for creating new instances of the Lava Parser for Fluid.
    /// </summary>
    public class LavaFluidParserFactory : FluidParserFactoryBase
    {
        protected override IFluidParser OnCreateParser( LanguageData languageData, Dictionary<string, ITag> tags, Dictionary<string, ITag> blocks )
        {
            return new FluidParserEx( languageData, tags, blocks );
        }

    }

    /// <summary>
    /// This implementation is a direct replacement for the standard FluidParserFactory, but it allows a custom parser implementation to be returned.
    /// </summary>
    /* [2021-02-02] DJL
 * This code is largely identical to the FluidParserFactoryBase.cs file in release 1.0.0-beta-9660.
 * This implementation extends the factory to return a custom parser.
 */
    /// 
    public abstract class FluidParserFactoryBase : IFluidParserFactory
    {
        private readonly LavaFluidGrammar _grammar = new LavaFluidGrammar();
        private readonly Dictionary<string, ITag> _tags = new Dictionary<string, ITag>();
        private readonly Dictionary<string, ITag> _blocks = new Dictionary<string, ITag>();

        private LanguageData _languageData;

        public IFluidParser CreateParser()
        {
            if ( _languageData == null )
            {
                lock ( _grammar )
                {
                    if ( _languageData == null )
                    {
                        _languageData = new LanguageData( _grammar );
                    }
                }
            }

            return OnCreateParser( _languageData, _tags, _blocks );
        }

        protected abstract IFluidParser OnCreateParser( LanguageData languageData, Dictionary<string, ITag> tags, Dictionary<string, ITag> blocks );

        public void RegisterTag( string name, ITag tag )
        {
            lock ( _grammar )
            {
                _languageData = null;
                _tags[name] = tag;

                // Configure the grammar to add support for the custom syntax

                var terminal = new NonTerminal( name )
                {
                    Rule = _grammar.ToTerm( name ) + tag.GetSyntax( _grammar )
                };

                _grammar.KnownTags.Rule |= terminal;

                // Prevent the text from being added in the parsed tree.
                _grammar.MarkPunctuation( name );
            }
        }

        public void RegisterTag<T>( string name ) where T : ITag, new()
        {
            RegisterTag( name, new T() );
        }

        public void RegisterBlock( string name, ITag tag )
        {
            lock ( _grammar )
            {
                _languageData = null;
                _blocks[name] = tag;

                // Configure the grammar to add support for the custom syntax

                var terminal = new NonTerminal( name )
                {
                    Rule = _grammar.ToTerm( name ) + tag.GetSyntax( _grammar )
                };
                var endName = "end" + name;

                _grammar.KnownTags.Rule |= terminal;
                _grammar.KnownTags.Rule |= _grammar.ToTerm( endName );

                // Prevent the text from being added in the parsed tree.
                _grammar.MarkPunctuation( name );
            }

        }

        public void RegisterBlock<T>( string name ) where T : ITag, new()
        {
            RegisterBlock( name, new T() );
        }
    }
}
