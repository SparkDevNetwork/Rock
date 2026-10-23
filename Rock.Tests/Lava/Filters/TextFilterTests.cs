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
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Lava.Shared;

using Rock.Lava;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Filters
{
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class TextFilterTests
    {
        private const string _TestTextParagraph = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Odio eu feugiat pretium nibh.Semper risus in hendrerit gravida.Enim diam vulputate ut pharetra. Massa tincidunt nunc pulvinar sapien et ligula ullamcorper. Morbi tristique senectus et netus et malesuada.Praesent semper feugiat nibh sed pulvinar proin gravida hendrerit.Ultrices in iaculis nunc sed.Tortor id aliquet lectus proin nibh nisl condimentum id.Vel pretium lectus quam id leo in vitae.In mollis nunc sed id.";

        /// <summary>
        /// For complex objects, the filter should return the .NET ToString() result for the object.
        /// </summary>
        /// <remarks>
        /// This filter has an identical implementation to "ToString", so we only need to test that it is accessible.
        /// </remarks>
        [TestMethod]
        public void AsString_AnonymousObjectWithToStringMethodOverride_ReturnsToStringForObject()
        {
            var person = LavaTestData.GetTestPersonAlishaMarble();

            var mergeValues = new LavaDataDictionary { { "CurrentPerson", person } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ CurrentPerson | AsString }}", options );

                Assert.AreEqual( "Alisha Marble", output );
            } );
        }

        #region Filter Tests: Humanize

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized.
        /// </summary>
        [TestMethod]
        public void Humanize_CamelCase_ProducesSeparatedWords()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'camelCase' | Humanize }}" );

                Assert.AreEqual( "Camel case", output );
            } );
        }

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized.
        /// </summary>
        [TestMethod]
        public void Humanize_Underscore_ProducesSeparatedWords()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'underscore_a_point' | Humanize }}" );

                Assert.AreEqual( "underscore a point", output );
            } );
        }

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized.
        /// </summary>
        [TestMethod]
        public void Humanize_DashSeparator_ProducesSeparatedWords()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'css-classes' | Humanize }}" );

                Assert.AreEqual( "css classes", output );
            } );
        }

        #endregion

        /// <summary>
        /// The input sentence should be formatted with only the first letter of the sentence capitalized.
        /// </summary>
        [TestMethod]
        public void SentenceCase_LowerCaseString_ProducesSentenceCase()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'good to great' | SentenceCase }}" );

                Assert.AreEqual( "Good to great", output );
            } );
        }

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized.
        /// </summary>
        [TestMethod]
        public void TitleCase_MixedCaseString_ProducesTitleCase()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Job posting for groundskeeper' | TitleCase }}" );

                Assert.AreEqual( "Job Posting For Groundskeeper", output );
            } );
        }

        #region Filter Tests: TruncateWords

        [TestMethod]
        public void TruncateWords_WithLongerText_AddsEllipis()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'one two three four five' | TruncateWords:3 }}" );

                Assert.AreEqual( "one two three...", output );
            } );
        }

        [TestMethod]
        public void TruncateWords_WithShorterText_DoesNotTruncate()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'one two three four five' | TruncateWords:6 }}" );

                Assert.AreEqual( "one two three four five", output );
            } );
        }

        [TestMethod]
        public void TruncateWords_WithEmptyString_HasNoEffect()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '' | TruncateWords:1 }}" );

                Assert.AreEqual( "", output );
            } );
        }

        #endregion

        /// <summary>
        /// A lower-case string should be formatted with the first letter of each word capitalized and all whitespace removed.
        /// </summary>
        [TestMethod]
        public void ToPascal_LowerCaseString_ProducesPascalCase()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'community participant' | ToPascal }}" );

                Assert.AreEqual( "CommunityParticipant", output );
            } );
        }

        /// <summary>
        /// A numeric input should return a text string.
        /// </summary>
        [TestMethod]
        public void ToString_NumericInput_ProducesText()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 1234567.89 | ToString }}" );

                Assert.AreEqual( "1234567.89", output );
            } );
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void ObfuscateEmail_ReadableEmailAddress_IsObfuscated()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'ted@rocksolidchurchdemo.com' | ObfuscateEmail }}" );

                Assert.AreEqual( "txxxxx@rocksolidchurchdemo.com", output );
            } );
        }

        #region Filter Tests: Pluralize

        /// <summary>
        /// Providing a singular noun as input produces a collective noun as output.
        /// </summary>
        [TestMethod]
        public void Pluralize_SingularTerm_ProducesPluralizedTerm()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'goose' | Pluralize }}" );

                Assert.AreEqual( "geese", output );
            } );
        }

        /// <summary>
        /// Providing a collective noun as input produces unchanged output.
        /// </summary>
        [TestMethod]
        public void Pluralize_PluralTerm_ProducesUnchangedOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'requests' | Pluralize }}" );

                Assert.AreEqual( "requests", output );
            } );
        }

        /// <summary>
        /// Providing a collective noun as input produces unchanged output.
        /// </summary>
        [TestMethod]
        public void Pluralize_EmptyInput_ProducesEmptyOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '' | Pluralize }}" );

                Assert.AreEqual( "", output );
            } );
        }

        #endregion

        #region Filter Tests: PluralizeForQuantity

        /// <summary>
        /// Providing an input argument for quantity of 3 produces a collective noun as output.
        /// </summary>
        [TestMethod]
        public void PluralizeForQuantity_QuantityOf3_ProducesPluralizedTerm()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Leader' | PluralizeForQuantity:3 }}" );

                Assert.AreEqual( "Leaders", output );
            } );
        }

        /// <summary>
        /// Providing an input argument for quantity of 1 produces a singular noun as output.
        /// </summary>
        [TestMethod]
        public void PluralizeForQuantity_QuantityOf1_ProducesSingularTerm()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Leader' | PluralizeForQuantity:1 }}" );

                Assert.AreEqual( "Leader", output );
            } );
        }

        /// <summary>
        /// PluralizeForQuantity should return correct plural form for negative and positive decimals.
        /// </summary>
        [TestMethod]
        [DataRow( "-2", "degrees" )]
        [DataRow( "-1.5", "degrees" )]
        [DataRow( "-1", "degree" )]
        [DataRow( "-0.5", "degrees" )]
        [DataRow( "0", "degrees" )]
        [DataRow( "0.5", "degrees" )]
        [DataRow( "1", "degree" )]
        [DataRow( "1.5", "degrees" )]
        [DataRow( "2", "degrees" )]
        public void PluralizeForQuantity_QuantityOfPositiveOrNegative_ProducesCorrectPluralizedTerm( string input, string expected )
        {
            var template = "{% assign x = <input> %}{{ 'degree' | PluralizeForQuantity:x }}"
               .Replace( "<input>", input );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        #endregion

        /// <summary>
        /// A noun ending with the letter "S" as input should produce a possessive form having a trailing apostrophe.
        /// </summary>
        [TestMethod]
        public void Possessive_NameEndingWithS_ProducesPossessiveFormWithTrailingApostrophe()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Charles' | Possessive }}" );

                Assert.AreEqual( "Charles’", output );
            } );
        }

        /// <summary>
        /// A noun ending with the letter "S" as input should produce a possessive form having a trailing apostrophe.
        /// </summary>
        [TestMethod]
        public void Possessive_NameNotEndingWithS_ProducesPossessiveForm()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Ted' | Possessive }}" );

                Assert.AreEqual( "Ted’s", output );
            } );
        }

        #region Filter Tests: Read Time

        private static readonly string[] _timeSpanOutputFormats = new string[] { "m' mins'", "m' min'", "s' secs'", "s' sec'" };

        /// <summary>
        /// The read time for a known piece of text using the default settings should return a number of minutes.
        /// </summary>
        [TestMethod]
        public void ReadTime_CalculateReadTimeForAverageReader_ProducesTimeInSeconds()
        {
            // Create a large document.
            var documentText = GetRepeatString( _TestTextParagraph, 5 );

            var template = "{{ '<content>' | ReadTime }}"
                .Replace( "<content>", documentText );

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.IsFalse( string.IsNullOrWhiteSpace( output ) );

                var readTime = TimeSpan.ParseExact( output, _timeSpanOutputFormats, CultureInfo.CurrentCulture );

                Assert.That.AreProximate( 120, readTime.TotalSeconds, 10 );
            } );
        }

        /// <summary>
        /// The average read time for a known piece of text should return a specific number.
        /// </summary>
        [TestMethod]
        public void ReadTime_CalculateReadTimeForSlowReader_ProducesTimeInMinutes()
        {
            // Create a large document.
            var documentText = GetRepeatString( _TestTextParagraph, 5 );

            var template = "{{ '<content>' | ReadTime:5,30 }}"
                .Replace( "<content>", documentText );

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.IsFalse( string.IsNullOrWhiteSpace( output ) );

                var readTime = TimeSpan.ParseExact( output, _timeSpanOutputFormats, CultureInfo.CurrentCulture );

                Assert.That.AreProximate( 360, readTime.TotalSeconds, 10 );
            } );
        }

        /// <summary>
        /// The average read time for a known piece of text should return a specific number.
        /// </summary>
        [TestMethod]
        public void ReadTime_CalculateReadTimeForFastReader_ProducesTimeInSeconds()
        {
            // Create a large document.
            var documentText = GetRepeatString( _TestTextParagraph, 5 );

            var template = "{{ '<content>' | ReadTime:500,6 }}"
                .Replace( "<content>", documentText );

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );
                var readTime = TimeSpan.ParseExact( output, _timeSpanOutputFormats, CultureInfo.CurrentCulture );

                Assert.That.AreProximate( 50, readTime.TotalSeconds, 10 );
            } );
        }

        /// <summary>
        /// Create a string by concatenating the input string to itself a specified number of times.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private string GetRepeatString( string value, int count )
        {
            string output = string.Empty;

            for ( int i = 0; i < count; i++ )
            {
                output += value;
            }

            return output;
        }

        #endregion

        #region Filter Tests: Regular Expressions

        /// <summary>
        /// Various email address formats can be matched using a regular expression.
        /// </summary>
        [TestMethod]
        [DataRow( "ted@rocksolidchurchdemo.com", true )]
        [DataRow( "has_underscore@rocksolidchurchdemo.com", true )]
        [DataRow( "has.dot@rocksolidchurchdemo.com", true )]
        [DataRow( "no_at_symbol-rocksolidchurchdemo.com", false )]
        [DataRow( "extra_at_symbol@@rocksolidchurchdemo.com", false )]
        [DataRow( "has.no.domain.separator@rocksolidchurchdemo", false )]
        public void RegExMatch_EmailAddressValidationSucceeds( string input, bool isMatch )
        {
            // This regular expression is the same one used in Rock for email validation.
            // We need to use a capture to store the regex to pass into the filter, because a string literal containing the \w escape sequence will throw a parsing error in Fluid.
            var template = @"{% capture regex %}\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*{% endcapture %}{{ '<input>' | RegExMatch:regex }}"
                           .Replace( "<input>", input );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( isMatch.ToString().ToLower(), output );
            } );
        }

        /// <summary>
        /// Regular expression match is found.
        /// </summary>
        [TestMethod]
        public void RegExMatchValue_FindsFirstMatchOnly()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, @"{% capture regex %}\d+{% endcapture %}{{ 'group 12345' | RegExMatchValue:regex }}" );

                Assert.AreEqual( "12345", output );
            } );
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, @"{% capture regex %}\b\w+day\b{% endcapture %}{{ 'Services on Saturday and Sunday' | RegExMatchValue:regex }}" );

                Assert.AreEqual( "Saturday", output );
            } );
        }

        /// <summary>
        /// Multiple Regular expression matches are found.
        /// </summary>
        [TestMethod]
        public void RegExMatchValues_FindsAllMatches()
        {
            var template = @"
{% capture regex %}\b\w+day\b{% endcapture %}{% assign days = 'Services on Saturday and Sunday and now also on Monday!' | RegExMatchValues:regex %}
{% for day in days %}{{ day }},{% endfor %}
";
            template = template.Replace( "\n", string.Empty ).Replace( "\r", string.Empty );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( "Saturday,Sunday,Monday,", output );
            } );
        }

        [TestMethod]
        public void RegExReplace_WithIgnoreCaseOption_ReplacesAllCases()
        {
            var template = @"
{{ 'Testing: one, two, One, two, ONE, two...' | RegExReplace:'one','ONE','i' }}
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( "\nTesting: ONE, two, ONE, two, ONE, two...\n", output );
            } );
        }

        [TestMethod]
        public void RegExReplace_WithCaptureGroup_EmitsExpectedOutput()
        {
            var template = @"
{% capture regex %}[Hh]ello (\w+){% endcapture %}{{ 'Hello Ted, how are you?' | RegExReplace:regex,'Greetings $1' }}
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( "\nGreetings Ted, how are you?\n", output );
            } );
        }

        #endregion

        /// <summary>
        /// Last instance of search string should be replaced with replacement string.
        /// </summary>
        [TestMethod]
        [DataRow( "Blue, Blue, Red, Red", "Blue, Blue, Red, Green" )]
        [DataRow( "Red, Red, Blue, Blue", "Red, Green, Blue, Blue" )]
        [DataRow( "Red, Blue, Blue, Blue", "Green, Blue, Blue, Blue" )]
        [DataRow( "Blue, Blue, Blue, Blue", "Blue, Blue, Blue, Blue" )]
        [DataRow( "Red", "Green" )]
        public void ReplaceLast_LastInstanceOfRedIsReplacedWithGreen( string input, string expected )
        {
            var template = "{{ '<input>' | ReplaceLast:'Red','Green' }}"
                           .Replace( "<input>", input );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        #region Filter Tests: Right

        /// <summary>
        /// The rightmost part of the input sentence should be returned in the output.
        /// </summary>
        [TestMethod]
        public void Right_LessThanStringLength_ProducesSubstring()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Decker' | Right:4 }}" );

                Assert.AreEqual( "cker", output );
            } );
        }

        /// <summary>
        /// If the requested number of characters exceeds the string length, the entire string should be returned.
        /// </summary>
        [TestMethod]
        public void Right_GreaterThanStringLength_ProducesEntireString()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Decker' | Right:10 }}" );

                Assert.AreEqual( "Decker", output );
            } );
        }

        /// <summary>
        /// If the input is an empty string, the output is also an empty string.
        /// </summary>
        [TestMethod]
        public void Right_EmptyString_ProducesEmptyString()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '' | Right:10 }}" );

                Assert.AreEqual( "", output );
            } );
        }

        #endregion

        #region Filter Tests: Singularize

        /// <summary>
        /// Providing a singular noun as input produces unchanged output.
        /// </summary>
        [TestMethod]
        public void Singularize_SingularTerm_ProducesUnchangedOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'goose' | Singularize }}" );

                Assert.AreEqual( "goose", output );
            } );
        }

        /// <summary>
        /// Providing a collective noun as input produces a singular noun as output.
        /// </summary>
        [TestMethod]
        public void Singularize_PluralTerm_ProducesSingularTerm()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'geese' | Singularize }}" );

                Assert.AreEqual( "goose", output );
            } );
        }

        #endregion

        #region Filter Tests: Split

        /// <summary>
        /// Split filter should retain or remove zero-length items in accordance with the specified "removeEmpty" parameter when handling an empty string or empty list.
        /// </summary>
        /// <remarks>
        /// The default Liquid language behavior for this filter is to remove empty entries.
        /// </remarks>
        [TestMethod]
        [DataRow( ",", "','", "0" )]
        [DataRow( ",", "',',true", "0" )]
        [DataRow( ",", "',',false", "2" )]
        [DataRow( "", "','", "0" )]
        [DataRow( "", "',',true", "0" )]
        [DataRow( "", "',',false", "1" )]
        public void Split_WithRemoveEmptyEntriesOption_RetainsOrRemovesEmptyEntries_WhenEmptyString( string inputString, string filterArgsString, string expectedOutput )
        {
            // Note: This test is different than the other Split tests so we
            // we can truly detect the empty list case.
            var template = "{{ '<inputString>' | Split:<args> | Size }}";

            template = template.Replace( "<inputString>", inputString );
            template = template.Replace( "<args>", filterArgsString );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Split filter should retain or remove zero-length items in accordance with the specified "removeEmpty" parameter.
        /// </summary>
        /// <remarks>
        /// The default Liquid language behavior for this filter is to remove empty entries.
        /// </remarks>
        [TestMethod]
        [DataRow( ",1,,3,4,5,6,7,,9,", "','", "1+3+4+5+6+7+9" )]
        [DataRow( ",1,,3,4,5,6,7,,9,", "',',true", "1+3+4+5+6+7+9" )]
        [DataRow( ",1,,3,4,5,6,7,,9,", "',','true'", "1+3+4+5+6+7+9" )]
        [DataRow( ",1,,3,4,5,6,7,,9,", "',',false", "+1++3+4+5+6+7++9+" )]
        [DataRow( ",1,,3,4,5,6,7,,9,", "',','false'", "+1++3+4+5+6+7++9+" )]
        public void Split_WithRemoveEmptyEntriesOption_RetainsOrRemovesEmptyEntries( string inputString, string filterArgsString, string expectedOutput )
        {
            // The trim markers keep the rendered output to just the filter result,
            // so the data row values describe what Split returns rather than how
            // the surrounding template happens to be laid out.
            var template = @"
{%- assign items = '<inputString>' | Split:<args> -%}
{%- for item in items -%}
{{ item }}
{%- if forloop.last == false -%}+{%- endif -%}
{%- endfor -%}
";

            template = template.Replace( "<inputString>", inputString );
            template = template.Replace( "<args>", filterArgsString );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Split filter should only return the specified number of substrings if the "count" parameter is specified.
        /// The remainder text is included in the last element of the array.
        /// </summary>
        [TestMethod]
        [DataRow( "1,2,3,4,5,6,7,8,9", "',',false,0", "" )]
        [DataRow( "1,2,3,4,5,6,7,8,9", "',',false,1", "1,2,3,4,5,6,7,8,9" )]
        [DataRow( "1,2,3,4,5,6,7,8,9", "',',false,2", "1+2,3,4,5,6,7,8,9" )]
        [DataRow( "1,2,3,4,5,6,7,8,9", "',',false,3", "1+2+3,4,5,6,7,8,9" )]
        [DataRow( "1,2,3,4,5,6,7,8,9", "',',false,10", "1+2+3+4+5+6+7+8+9" )]
        [DataRow( "1,2,3,4,5,6,7,8,9", "','", "1+2+3+4+5+6+7+8+9" )]
        public void Split_WithCountOption_ReturnsSpecifiedNumberOfSubstrings( string inputString, string filterArgsString, string expectedOutput )
        {
            // The trim markers keep the rendered output to just the filter result,
            // so the data row values describe what Split returns rather than how
            // the surrounding template happens to be laid out.
            var template = @"
{%- assign items = '<inputString>' | Split:<args> -%}
{%- for item in items -%}
{{ item }}
{%- if forloop.last == false -%}+{%- endif -%}
{%- endfor -%}
";

            template = template.Replace( "<inputString>", inputString );
            template = template.Replace( "<args>", filterArgsString );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Split filter applied with option to remove empty entries should eliminate zero-length items.
        /// </summary>
        [TestMethod]
        public void Split_DocumentationExamples_ProduceExpectedSampleOutput()
        {
            var expected = "\n1 and 3 and 4 and 5 and 6 and 7 and 9\n";

            var template = @"
{{ '1,,3,4,5,6,7,,9' | Split:',',true | Join:' and ' }}
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        #endregion

        #region Filter Tests: ToCssClass

        /// <summary>
        /// ToCssClass filter returns expected output.
        /// </summary>
        ///
        [TestMethod]
        [DataRow( "Community Participant", "community-participant" )]
        [DataRow( "community--participant", "community-participant" )]
        [DataRow( "1234", "-x-1234" )]
        [DataRow( "abc$$!!123", "abc-123" )]
        [DataRow( "   ", "" )]
        [DataRow( "", "" )]
        [DataRow( null, "" )]
        public void ToCssClass_VariousInputs_ReturnExpectedOutput( string input, string expected )
        {
            var template = "{{ '" + input + "' | ToCssClass }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        #endregion

        #region Filter Tests: Trim

        /// <summary>
        /// Leading and trailing whitespace should be removed, while internal whitespace is preserved.
        /// </summary>
        [TestMethod]
        [DataRow( "Ted Decker    ", "Ted Decker" )]
        [DataRow( "   Ted Decker", "Ted Decker" )]
        [DataRow( "   Ted Decker    ", "Ted Decker" )]
        [DataRow( "   ", "" )]
        [DataRow( "", "" )]
        public void Trim_WithNoParameters_RemovesLeadingAndTrailingWhitespace( string input, string expected )
        {
            var template = "{{ '" + input + "' | Trim }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        /// <summary>
        /// Leading and trailing character sequences should be removed, while other characters are preserved.
        /// </summary>
        [TestMethod]
        [DataRow( "Ted#Decker", "#", "Ted#Decker" )]
        [DataRow( "#Ted Decker", "#", "Ted Decker" )]
        [DataRow( "#Ted Decker#", "#", "Ted Decker" )]
        [DataRow( "###Ted##Decker###", "##", "#Ted##Decker#" )]
        [DataRow( "###", "#", "" )]
        [DataRow( "#$#$Ted#$Decker#$#$", "#$", "Ted#$Decker" )]
        [DataRow( "#$#$#$#$", "#$", "" )]
        [DataRow( "", "###", "" )]
        public void Trim_WithSpecifiedTextToRemove_RemovesTextFromStartAndEndOfString( string input, string textToRemove, string expected )
        {
            var template = "{{ '" + input + "' | Trim:'" + textToRemove + "' }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        /// <summary>
        /// Leading and trailing whitespace should be removed, while internal whitespace is preserved.
        /// </summary>
        [TestMethod]
        [DataRow( "Ted Decker   ", "Ted Decker" )]
        [DataRow( "   Ted Decker", "   Ted Decker" )]
        [DataRow( "   Ted Decker   ", "   Ted Decker" )]
        [DataRow( "   ", "" )]
        [DataRow( "", "" )]
        public void TrimEnd_WithNoParameters_RemovesTrailingWhitespace( string input, string expected )
        {
            var template = "{{ '" + input + "' | TrimEnd }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        /// <summary>
        /// Trailing characters should be removed, while other characters are preserved.
        /// </summary>
        [TestMethod]
        [DataRow( "Ted#Decker", "#", "Ted#Decker" )]
        [DataRow( "Ted Decker#", "#", "Ted Decker" )]
        [DataRow( "#Ted Decker#", "#", "#Ted Decker" )]
        [DataRow( "###Ted##Decker###", "##", "###Ted##Decker#" )]
        [DataRow( "###", "#", "" )]
        [DataRow( "#$#$Ted#$Decker#$#$", "#$", "#$#$Ted#$Decker" )]
        [DataRow( "#$#$#$#$", "#$", "" )]
        [DataRow( "", "###", "" )]
        public void TrimEnd_WithSpecifiedTextToRemove_RemovesTextFromEndOfString( string input, string textToRemove, string expected )
        {
            var template = "{{ '" + input + "' | TrimEnd:'" + textToRemove + "' }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        /// <summary>
        /// Leading and trailing whitespace should be removed, while internal whitespace is preserved.
        /// </summary>
        [TestMethod]
        [DataRow( "Ted Decker   ", "Ted Decker   " )]
        [DataRow( "   Ted Decker", "Ted Decker" )]
        [DataRow( "   ", "" )]
        [DataRow( "", "" )]
        public void TrimStart_WithNoParameters_RemovesLeadingWhitespace( string input, string expected )
        {
            var template = "{{ '" + input + "' | TrimStart }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        /// <summary>
        /// Leading characters should be removed, while other characters are preserved.
        /// </summary>
        [TestMethod]
        [DataRow( "Ted*Decker", "*", "Ted*Decker" )]
        [DataRow( "*Ted Decker", "*", "Ted Decker" )]
        [DataRow( "*Ted Decker*", "*", "Ted Decker*" )]
        [DataRow( "***Ted**Decker***", "**", "*Ted**Decker***" )]
        [DataRow( "***", "*", "" )]
        [DataRow( "*$*$Ted*$Decker*$*$", "*$", "Ted*$Decker*$*$" )]
        [DataRow( "*$*$*$*$", "*$", "" )]
        [DataRow( "", "***", "" )]
        public void TrimStart_WithSpecifiedTextToRemove_RemovesTextFromStartOfString( string input, string textToRemove, string expected )
        {
            var template = "{{ '" + input + "' | TrimStart:'" + textToRemove + "' }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        #endregion

        /// <summary>
        /// Url parts queries return the correct segment of the Url.
        /// </summary>
        [TestMethod]
        public void Url_SimplePartQuery_ReturnCorrectPart()
        {
            var url = "https://www.rockrms.com/WorkflowEntry/35?PersonId=2";

            VerifyUrlPart( url, "host", "", "www.rockrms.com" );
            VerifyUrlPart( url, "port", "", "443" );
            VerifyUrlPart( url, "scheme", "", "https" );
            VerifyUrlPart( url, "protocol", "", "https" );
            VerifyUrlPart( url, "localpath", "", "/WorkflowEntry/35" );
            VerifyUrlPart( url, "pathandquery", "", "/WorkflowEntry/35?PersonId=2" );
            VerifyUrlPart( url, "queryparameter", "", "" );
            VerifyUrlPart( url, "queryparameter", "PersonId", "2" );
            VerifyUrlPart( url, "url", "", "https://www.rockrms.com/WorkflowEntry/35?PersonId=2" );
            VerifyUrlPart( url, "invalid_part", "", "" );
        }

        /// <summary>
        /// Url Segments query returns a collection of all segments of the Url.
        /// </summary>        
        [TestMethod]
        public void Url_SegmentsQuery_ReturnCorrectParts()
        {
            var url = "https://www.rockrms.com/WorkflowEntry/35?PersonId=2";

            var template = "{{ '<url>' | Url:'segments' | Join:'|' }}";

            template = template.Replace( "<url>", url );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( "/|WorkflowEntry/|35", output );
            } );
        }

        private void VerifyUrlPart( string url, string part, string key, string expected )
        {
            var template = "{{ '<url>' | Url:<options> }}";

            var options = $"'{part}'";

            if ( key != null )
            {
                options += $",'{key}'";
            }

            template = template.Replace( "<url>", url );
            template = template.Replace( "<options>", options );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        /// <summary>
        /// Success text is appended when input text contains value.
        /// </summary>
        [TestMethod]
        public void WithFallback_InputTextContainsValue_SuccessTextIsAppended()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Ted' | WithFallback:', are', 'Are', 'append' }} you interested in baptism?" );

                Assert.AreEqual( "Ted, are you interested in baptism?", output );
            } );
        }

        /// <summary>
        /// Fallback text is appended when input text is empty.
        /// </summary>
        [TestMethod]
        public void WithFallback_InputTextIsEmpty_FallbackTextIsAppended()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '' | WithFallback:', are', 'Are' }} you interested in baptism?" );

                Assert.AreEqual( "Are you interested in baptism?", output );
            } );
        }
        /// <summary>
        /// Success text is prepended when input text contains value.
        /// </summary>
        [TestMethod]
        public void WithFallback_InputTextContainsValue_SuccessTextIsPrepended()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "Welcome{{ 'Ted' | WithFallback:' back ', ' stranger', 'prepend' }}!" );

                Assert.AreEqual( "Welcome back Ted!", output );
            } );
        }

        /// <summary>
        /// Fallback text is prepended when input text is empty.
        /// </summary>
        [TestMethod]
        public void WithFallback_InputTextIsEmpty_FallbackTextIsPrepended()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "Welcome{{ '' | WithFallback:' back ', ' stranger', 'prepend' }}!" );

                Assert.AreEqual( "Welcome stranger!", output );
            } );
        }

        #region Filter Tests: ToMarkdown

        /// <summary>
        /// An HTML heading followed by plain text should be converted to Markdown heading syntax.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_HtmlHeadingAndText_ProducesMarkdownHeading()
        {
            var template = "{{ '<h1>Hello World</h1>Now is the time for all good men...' | ToMarkdown }}";
            var expected = "# Hello World\n\nNow is the time for all good men...";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        /// <summary>
        /// A simple bold tag should convert to Markdown emphasis syntax.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_BoldTag_ProducesMarkdownStrong()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<strong>bold text</strong>' | ToMarkdown }}" );

                Assert.AreEqual( "**bold text**", output );
            } );
        }

        /// <summary>
        /// A simple italic tag should convert to Markdown emphasis syntax.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_ItalicTag_ProducesMarkdownEmphasis()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<i>italic text</i>' | ToMarkdown }}" );

                Assert.AreEqual( "*italic text*", output );
            } );
        }

        /// <summary>
        /// An anchor tag should convert to Markdown link syntax.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_AnchorTag_ProducesMarkdownLink()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<a href=\"https://www.rockrms.com\">Rock RMS</a>' | ToMarkdown }}" );

                Assert.AreEqual( "[Rock RMS](https://www.rockrms.com)", output );
            } );
        }

        /// <summary>
        /// A horizontal rule tag should convert to Markdown HR syntax.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_HorizontalRuleTag_ProducesMarkdownHorizontalRule()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<hr />' | ToMarkdown }}" );

                Assert.AreEqual( "***", output );
            } );
        }

        /// <summary>
        /// A blockquote tag should convert to Markdown blockquote syntax.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_BlockquoteTag_ProducesMarkdownBlockquote()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<blockquote>Blockquote</blockquote>' | ToMarkdown }}" );

                Assert.AreEqual( "> Blockquote", output );
            } );
        }

        /// <summary>
        /// A simple inline code tag should convert to Markdown code syntax.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_InlineCodeTag_ProducesMarkdownCode()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<code>Person.FirstName</code> is the Lava property' | ToMarkdown }}" );

                Assert.AreEqual( "`Person.FirstName` is the Lava property", output );
            } );
        }

        /// <summary>
        /// A preformatted code block should convert to Markdown code block syntax.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_PreCodeTag_ProducesMarkdownPreCode()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<pre><code>Person.FirstName</code></pre>' | ToMarkdown }}" );

                Assert.AreEqual( "```\nPerson.FirstName\n```", output );
            } );
        }

        /// <summary>
        /// An unordered list should convert to Markdown list syntax.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_UnorderedList_ProducesMarkdownList()
        {
            var template = "{{ '<ul><li>One</li><li>Two</li></ul>' | ToMarkdown }}";
            var expected = "- One\n- Two";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expected, output );
            } );
        }

        /// <summary>
        /// Passing an explicit sourceType of "html" should produce the same result as omitting the parameter.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_ExplicitHtmlSourceType_ProducesMarkdown()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<b>bold</b>' | ToMarkdown:'html' }}" );

                Assert.AreEqual( "**bold**", output );
            } );
        }

        /// <summary>
        /// The sourceType parameter should be treated as case-insensitive.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_MixedCaseSourceType_IsCaseInsensitive()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<b>bold</b>' | ToMarkdown:'HTML' }}" );

                Assert.AreEqual( "**bold**", output );
            } );
        }

        /// <summary>
        /// An empty input string should produce an empty output.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_EmptyInput_ProducesEmptyOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '' | ToMarkdown }}" );

                Assert.AreEqual( "", output );
            } );
        }

        /// <summary>
        /// Plain text without any HTML tags should be returned unchanged.
        /// </summary>
        [TestMethod]
        public void ToMarkdown_PlainText_ProducesUnchangedOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Now is the time for all good men.' | ToMarkdown }}" );

                Assert.AreEqual( "Now is the time for all good men.", output );
            } );
        }

        /// <summary>
        /// An unrecognized sourceType should return the input unchanged (forward-compatibility behavior).
        /// </summary>
        [TestMethod]
        public void ToMarkdown_UnsupportedSourceType_ReturnsInputUnchanged()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '<h1>Hello</h1>' | ToMarkdown:'unknown' }}" );

                Assert.AreEqual( "<h1>Hello</h1>", output );
            } );
        }

        #endregion

    }
}
