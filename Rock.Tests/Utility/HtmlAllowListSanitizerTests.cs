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
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;
using Rock.Utility.HtmlSanitization;

namespace Rock.Tests.Utility
{
    /// <summary>
    /// The shared parity contract for the allowlist sanitizer. Each row is
    /// (input, options name, expected output); the TypeScript Jest tests run
    /// the same rows with the same options names.
    /// </summary>
    [TestClass]
    public class HtmlAllowListSanitizerTests
    {
        private static readonly Dictionary<string, HtmlSanitizeOptions> NamedOptions = new Dictionary<string, HtmlSanitizeOptions>
        {
            ["Simple"] = HtmlSanitizeOptions.Simple,
            ["Rich"] = HtmlSanitizeOptions.Rich,
            ["Text"] = new HtmlSanitizeOptions().MakeReadOnly(),
            ["SimpleUnwrap"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple )
                .SetDefaultRemovalMode( HtmlTagRemovalMode.Unwrap )
                .MakeReadOnly(),
            ["RichUnwrap"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Rich )
                .SetDefaultRemovalMode( HtmlTagRemovalMode.Unwrap )
                .MakeReadOnly(),
            ["SimpleUnwrapBlink"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple )
                .SetRemovalMode( "blink", HtmlTagRemovalMode.Unwrap )
                .MakeReadOnly(),
            ["NoteText"] = new HtmlSanitizeOptions()
                .AllowTag( "br" )
                .AllowTag( "div" )
                .AllowTag( "span" )
                .AllowAttribute( "span", "class" )
                .AllowAttribute( "span", "data-identifier" )
                .SetDefaultRemovalMode( HtmlTagRemovalMode.Unwrap )
                .MakeReadOnly(),
            ["SimpleFont"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple )
                .AllowTag( "font" )
                .AllowAttribute( "font", "color" )
                .MakeReadOnly(),
            ["SimpleCite"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple )
                .AllowAttribute( "blockquote", "cite" )
                .MakeReadOnly(),
            ["SimpleNoTitle"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple )
                .AllowAttribute( "a", "title" )
                .DisallowAttribute( "", "title" )
                .MakeReadOnly(),
            ["RichVideo"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Rich )
                .AllowTag( "video" )
                .AllowAttribute( "video", "poster" )
                .MakeReadOnly(),
            ["RichNoImg"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Rich )
                .DisallowTag( "img" )
                .MakeReadOnly(),
            ["RichNoImgAlt"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Rich )
                .DisallowAttribute( "img", "alt" )
                .MakeReadOnly(),
            ["RichNoData"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Rich )
                .DisallowDataAttributes()
                .MakeReadOnly(),
            ["RichMath"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Rich )
                .AllowTag( "math" )
                .AllowTag( "mi" )
                .AllowTag( "mrow" )
                .MakeReadOnly(),
            ["SimpleNoMailto"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple )
                .DisallowUrlScheme( "mailto" )
                .MakeReadOnly(),
            ["SimpleNoColor"] = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple )
                .DisallowCssProperty( "color" )
                .MakeReadOnly()
        };

        private static void AssertSanitized( string html, string optionsName, string expected )
        {
            var sanitizer = new HtmlAllowListSanitizer();
            var options = NamedOptions[optionsName];

            var actual = sanitizer.Sanitize( html, options );

            Assert.AreEqual( expected, actual );
            Assert.AreEqual( expected, sanitizer.Sanitize( actual, options ), "Sanitizing the output again changed it." );
        }

        [TestMethod]
        [DataRow( "<span>a</span><b>b</b><strong>c</strong><i>d</i><em>e</em><u>f</u><s>g</s><strike>h</strike><small>i</small><sub>j</sub><sup>k</sup><mark>l</mark><abbr>m</abbr><q>n</q><cite>o</cite><code>p</code><kbd>q</kbd><samp>r</samp><var>s</var>", "Simple", "<span>a</span><b>b</b><strong>c</strong><i>d</i><em>e</em><u>f</u><s>g</s><strike>h</strike><small>i</small><sub>j</sub><sup>k</sup><mark>l</mark><abbr>m</abbr><q>n</q><cite>o</cite><code>p</code><kbd>q</kbd><samp>r</samp><var>s</var>" )]
        [DataRow( "<span>a</span><b>b</b><strong>c</strong><i>d</i><em>e</em><u>f</u><s>g</s><strike>h</strike><small>i</small><sub>j</sub><sup>k</sup><mark>l</mark><abbr>m</abbr><q>n</q><cite>o</cite><code>p</code><kbd>q</kbd><samp>r</samp><var>s</var>", "Rich", "<span>a</span><b>b</b><strong>c</strong><i>d</i><em>e</em><u>f</u><s>g</s><strike>h</strike><small>i</small><sub>j</sub><sup>k</sup><mark>l</mark><abbr>m</abbr><q>n</q><cite>o</cite><code>p</code><kbd>q</kbd><samp>r</samp><var>s</var>" )]
        [DataRow( "<p>a<br>b</p><hr><blockquote>c</blockquote><pre>d</pre>", "Simple", "<p>a<br>b</p><hr><blockquote>c</blockquote><pre>d</pre>" )]
        [DataRow( "<p>a<br>b</p><hr><blockquote>c</blockquote><pre>d</pre>", "Rich", "<p>a<br>b</p><hr><blockquote>c</blockquote><pre>d</pre>" )]
        [DataRow( "<a href=\"https://example.com/\" target=\"_self\" rel=\"nofollow\">x</a>", "Simple", "<a href=\"https://example.com/\" target=\"_self\" rel=\"nofollow\">x</a>" )]
        [DataRow( "<span class=\"c\" title=\"t\" dir=\"rtl\" lang=\"en\" style=\"color: red\">x</span>", "Simple", "<span class=\"c\" title=\"t\" dir=\"rtl\" lang=\"en\" style=\"color: red\">x</span>" )]
        [DataRow( "<p class=\"c\" title=\"t\" dir=\"rtl\" lang=\"en\" style=\"color: red\">x</p>", "Rich", "<p class=\"c\" title=\"t\" dir=\"rtl\" lang=\"en\" style=\"color: red\">x</p>" )]
        [DataRow( "<h1>a</h1><h2>b</h2><h3>c</h3><h4>d</h4><h5>e</h5><h6>f</h6>", "Rich", "<h1>a</h1><h2>b</h2><h3>c</h3><h4>d</h4><h5>e</h5><h6>f</h6>" )]
        [DataRow( "<ul><li>a</li></ul><ol start=\"2\" type=\"a\" reversed=\"\"><li>b</li></ol><dl><dt>c</dt><dd>d</dd></dl>", "Rich", "<ul><li>a</li></ul><ol start=\"2\" type=\"a\" reversed=\"\"><li>b</li></ol><dl><dt>c</dt><dd>d</dd></dl>" )]
        [DataRow( "<ol reversed start=\"3\"><li>x</li></ol>", "Rich", "<ol reversed=\"\" start=\"3\"><li>x</li></ol>" )]
        [DataRow( "<div id=\"x\" data-toggle=\"modal\" data-a-b=\"y\">z</div>", "Rich", "<div id=\"x\" data-toggle=\"modal\" data-a-b=\"y\">z</div>" )]
        [DataRow( "<table><caption>c</caption><colgroup span=\"2\"><col span=\"1\"></colgroup><thead><tr><th scope=\"col\" colspan=\"2\">h</th></tr></thead><tbody><tr><td rowspan=\"1\" headers=\"h\" align=\"left\" valign=\"top\">d</td></tr></tbody><tfoot><tr><td>f</td></tr></tfoot></table>", "Rich", "<table><caption>c</caption><colgroup span=\"2\"><col span=\"1\"></colgroup><thead><tr><th scope=\"col\" colspan=\"2\">h</th></tr></thead><tbody><tr><td rowspan=\"1\" headers=\"h\" align=\"left\" valign=\"top\">d</td></tr></tbody><tfoot><tr><td>f</td></tr></tfoot></table>" )]
        [DataRow( "<table><td>a</td></table>", "Rich", "<table><tbody><tr><td>a</td></tr></tbody></table>" )]
        [DataRow( "<img src=\"https://example.com/a.png\" alt=\"a\" width=\"10\" height=\"20\">", "Rich", "<img src=\"https://example.com/a.png\" alt=\"a\" width=\"10\" height=\"20\">" )]
        [DataRow( "<a name=\"n\" href=\"/x\">y</a>", "Rich", "<a name=\"n\" href=\"/x\">y</a>" )]
        [DataRow( "<div class=\"alert alert-success\">a</div>", "Simple", "<div class=\"alert alert-success\">a</div>" )]
        [DataRow( "<center>a</center><span tabindex=\"0\" aria-haspopup=\"true\">b</span>", "Simple", "<center>a</center><span tabindex=\"0\" aria-haspopup=\"true\">b</span>" )]
        [DataRow( "<input type=\"email\" required=\"\"><select required=\"\"><option>a</option></select><textarea required=\"\"></textarea>", "Rich", "<input type=\"email\" required=\"\"><select required=\"\"><option>a</option></select><textarea required=\"\"></textarea>" )]
        [DataRow( "<svg viewBox=\"0 0 8 8\"><path d=\"M0 0L8 8\" stroke=\"#000\" stroke-miterlimit=\"10\"/></svg>", "Rich", "<svg viewBox=\"0 0 8 8\"><path d=\"M0 0L8 8\" stroke=\"#000\" stroke-miterlimit=\"10\"></path></svg>" )]
        [DataRow( "<span role=\"note\" aria-label=\"l\" aria-hidden=\"true\" aria-expanded=\"false\" aria-controls=\"c\" aria-describedby=\"d\" aria-labelledby=\"b\">a</span>", "Simple", "<span role=\"note\" aria-label=\"l\" aria-hidden=\"true\" aria-expanded=\"false\" aria-controls=\"c\" aria-describedby=\"d\" aria-labelledby=\"b\">a</span>" )]
        [DataRow( "<span aria-live=\"polite\" aria-label=\"l\">a</span>", "Simple", "<span aria-label=\"l\">a</span>" )]
        [DataRow( "<div class=\"progress-bar\" role=\"progressbar\" aria-valuemin=\"0\" aria-valuemax=\"100\" aria-valuenow=\"40\">40%</div>", "Simple", "<div class=\"progress-bar\" role=\"progressbar\" aria-valuemin=\"0\" aria-valuemax=\"100\" aria-valuenow=\"40\">40%</div>" )]
        [DataRow( "<img src=\"/a.png\" loading=\"lazy\"><span loading=\"lazy\">a</span>", "Rich", "<img src=\"/a.png\" loading=\"lazy\"><span>a</span>" )]
        [DataRow( "<section><article><header><hgroup><h1>a</h1></hgroup></header><nav>b</nav><main>c</main><aside>d</aside><figure><img src=\"/a.png\"><figcaption>e</figcaption></figure><details><summary>f</summary>g</details><time datetime=\"2026-10-19\">h</time><footer>i</footer></article></section>", "Rich", "<section><article><header><hgroup><h1>a</h1></hgroup></header><nav>b</nav><main>c</main><aside>d</aside><figure><img src=\"/a.png\"><figcaption>e</figcaption></figure><details><summary>f</summary>g</details><time datetime=\"2026-10-19\">h</time><footer>i</footer></article></section>" )]
        [DataRow( "<span datetime=\"2026-10-19\">a</span><time datetime=\"2026-10-19\">b</time>c", "Simple", "<span>a</span>c" )]
        public void Sanitize_AllowedGroups_PassUnchanged( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<h1>a</h1><ul><li>b</li></ul><ol><li>c</li></ol><dl><dt>d</dt></dl><section>e</section><table><tr><td>f</td></tr></table><img src=\"/a.png\">g", "Simple", "g" )]
        [DataRow( "<span id=\"x\" data-a=\"b\">c</span><a name=\"n\" href=\"/x\">y</a>", "Simple", "<span>c</span><a href=\"/x\">y</a>" )]
        [DataRow( "<span style=\"position: absolute; top: 0; z-index: 9; overflow: hidden; transform: none; border-collapse: collapse; table-layout: fixed; flex: 1; justify-content: center; gap: 1px; color: red\">x</span>", "Simple", "<span style=\"color: red\">x</span>" )]
        [DataRow( "<span style=\"position: absolute\">x</span>", "Simple", "<span>x</span>" )]
        public void Sanitize_RichOnlyItemsUnderSimple_AreRemoved( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<font>a</font><blink>b</blink><marquee>c</marquee>d", "Simple", "d" )]
        [DataRow( "<font>a</font><blink>b</blink><marquee>c</marquee>d", "Rich", "d" )]
        [DataRow( "<font>a</font><blink>b</blink><marquee>c</marquee>d", "SimpleUnwrap", "abcd" )]
        [DataRow( "<font>a</font><blink>b</blink><marquee>c</marquee>d", "RichUnwrap", "abcd" )]
        [DataRow( "<font>a</font><blink>b</blink><marquee>c</marquee>d", "SimpleUnwrapBlink", "bd" )]
        [DataRow( "<b><font>x</font>y</b>", "Simple", "<b>y</b>" )]
        [DataRow( "<b><font color=\"red\" onclick=\"x\">x<script>s</script><i title=\"t\" onclick=\"y\">i</i></font>y</b>", "SimpleUnwrap", "<b>x<i title=\"t\">i</i>y</b>" )]
        [DataRow( "<table><tr><td>a</td><td>b</td></tr></table>", "SimpleUnwrap", "ab" )]
        [DataRow( "<b>bold</b> and <i>it</i>", "Text", " and " )]
        public void Sanitize_DisallowedElements_AreDroppedOrUnwrapped( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "a<script>b</script><style>c</style><iframe>d</iframe><object>e</object><embed><form>f</form><link><meta><base><svg><text>j</text></svg><math><mi>k</mi></math><noscript>l</noscript><template>m</template><applet>n</applet><xmp>o</xmp><noembed>p</noembed><noframes>q</noframes><title>r</title>z", "Simple", "az" )]
        [DataRow( "a<script>b</script><style>c</style><iframe>d</iframe><object>e</object><embed><form>f</form><link><meta><base><svg><text>j</text></svg><math><mi>k</mi></math><noscript>l</noscript><template>m</template><applet>n</applet><xmp>o</xmp><noembed>p</noembed><noframes>q</noframes><title>r</title>z", "Rich", "a<svg></svg>z" )]
        [DataRow( "a<script>b</script><style>c</style><iframe>d</iframe><object>e</object><embed><form>f</form><link><meta><base><svg><text>j</text></svg><math><mi>k</mi></math><noscript>l</noscript><template>m</template><applet>n</applet><xmp>o</xmp><noembed>p</noembed><noframes>q</noframes><title>r</title>z", "SimpleUnwrap", "az" )]
        [DataRow( "a<script>b</script><style>c</style><iframe>d</iframe><object>e</object><embed><form>f</form><link><meta><base><svg><text>j</text></svg><math><mi>k</mi></math><noscript>l</noscript><template>m</template><applet>n</applet><xmp>o</xmp><noembed>p</noembed><noframes>q</noframes><title>r</title>z", "RichUnwrap", "a<svg></svg>z" )]
        [DataRow( "a<script>b</script><style>c</style><iframe>d</iframe><object>e</object><embed><form>f</form><link><meta><base><svg><text>j</text></svg><math><mi>k</mi></math><noscript>l</noscript><template>m</template><applet>n</applet><xmp>o</xmp><noembed>p</noembed><noframes>q</noframes><title>r</title>z", "Text", "az" )]
        [DataRow( "a<plaintext>b</plaintext>c", "Simple", "a" )]
        [DataRow( "a<plaintext>b</plaintext>c", "RichUnwrap", "a" )]
        [DataRow( "<b>a<script>alert(1)</script></b>", "Simple", "<b>a</b>" )]
        [DataRow( "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\"></noscript>", "Simple", "\"&gt;" )]
        [DataRow( "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\"></noscript>", "RichUnwrap", "<img src=\"x\">\"&gt;" )]
        [DataRow( "<svg><style><img src=x onerror=alert(1)></style></svg>x", "Simple", "x" )]
        [DataRow( "<svg><style><img src=x onerror=alert(1)></style></svg>x", "RichUnwrap", "<svg></svg><img src=\"x\">x" )]
        [DataRow( "<math><mtext><table><mglyph><style><img src=x onerror=alert(1)>", "RichUnwrap", "" )]
        [DataRow( "<form><math><mtext></form><form><mglyph><style></math><img src onerror=alert(1)>", "RichUnwrap", "" )]
        public void Sanitize_NeverPassElements_AreDroppedWithContent( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<label for=\"q\">Find</label><input type=\"search\" name=\"q\" value=\"a\" placeholder=\"p\" autofocus=\"\" maxlength=\"9\"><button type=\"submit\" name=\"go\" value=\"1\" class=\"btn\">Go</button>", "Rich", "<label for=\"q\">Find</label><input type=\"search\" name=\"q\" value=\"a\" placeholder=\"p\" autofocus=\"\" maxlength=\"9\"><button type=\"submit\" name=\"go\" value=\"1\" class=\"btn\">Go</button>" )]
        [DataRow( "<input type=\"password\" name=\"p\"><input type=\"hidden\" name=\"h\" value=\"v\"><input type=\"checkbox\" checked=\"\" disabled=\"\">", "Rich", "<input type=\"password\" name=\"p\"><input type=\"hidden\" name=\"h\" value=\"v\"><input type=\"checkbox\" checked=\"\" disabled=\"\">" )]
        [DataRow( "<select name=\"s\" multiple=\"\" size=\"2\"><optgroup label=\"g\"><option value=\"1\" selected=\"\">a</option><option value=\"2\" disabled=\"\">b</option></optgroup></select>", "Rich", "<select name=\"s\" multiple=\"\" size=\"2\"><optgroup label=\"g\"><option value=\"1\" selected=\"\">a</option><option value=\"2\" disabled=\"\">b</option></optgroup></select>" )]
        [DataRow( "<textarea name=\"t\" rows=\"2\" cols=\"9\" readonly=\"\">a</textarea>", "Rich", "<textarea name=\"t\" rows=\"2\" cols=\"9\" readonly=\"\">a</textarea>" )]
        [DataRow( "<button formaction=\"https://evil.example/\" formmethod=\"post\" formtarget=\"_blank\" form=\"f\" onclick=\"x()\">a</button>", "Rich", "<button>a</button>" )]
        [DataRow( "<input formaction=\"https://evil.example/\" type=\"submit\" onfocus=\"x()\">", "Rich", "<input type=\"submit\">" )]
        [DataRow( "<textarea><img src=x onerror=alert(1)></textarea>", "Rich", "<textarea>&lt;img src=x onerror=alert(1)&gt;</textarea>" )]
        [DataRow( "<select><img src=x onerror=alert(1)><option>a</option></select>", "Rich", "<select><option>a</option></select>" )]
        [DataRow( "<label for=\"q\">a</label><input name=\"q\"><button>b</button><select><option>c</option></select><textarea>d</textarea>e", "Simple", "e" )]
        public void Sanitize_FormControls_PassUnderRichWithoutFormAction( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<input name=\"submit\"><button name=\"reset\">a</button><input id=\"addEventListener\"><input name=\"querySelector\">", "Rich", "<input><button>a</button><input><input>" )]
        [DataRow( "<input name=\"__EVENTTARGET\" value=\"x\"><input type=\"hidden\" name=\"__VIEWSTATE\" value=\"y\"><span id=\"__proto__\">z</span>", "Rich", "<input value=\"x\"><input type=\"hidden\" value=\"y\"><span>z</span>" )]
        [DataRow( "<input type=\"hidden\" name=\"ctl00$main$ctl05$cbIsActive\" value=\"on\"><a name=\"a$b\">x</a>", "Rich", "<input type=\"hidden\" value=\"on\"><a>x</a>" )]
        [DataRow( "<input name=\"q\" id=\"search-box\"><input name=\"Submit\"><input name=\"name\"><a name=\"top\">x</a>", "Rich", "<input name=\"q\" id=\"search-box\"><input name=\"Submit\"><input name=\"name\"><a name=\"top\">x</a>" )]
        public void Sanitize_NameAndId_CanNotHideFormMethodsOrJoinPostbacks( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<div style=\"position: fixed; top: 0; left: 0; z-index: 99999; color: red\">a</div>", "Rich", "<div style=\"top: 0; left: 0; z-index: 99999; color: red\">a</div>" )]
        [DataRow( "<div style=\"POSITION: Fixed !important\">a</div><div style=\"position: absolute\">b</div><div style=\"position: sticky\">c</div>", "Rich", "<div>a</div><div style=\"position: absolute\">b</div><div style=\"position: sticky\">c</div>" )]
        public void Sanitize_FixedPosition_IsRemoved( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        /// <summary>
        /// Published foreign-content (SVG and MathML) and namespace-confusion
        /// vectors, plus shipped template SVGs. Every expected output was also
        /// parsed by Chrome, which built the same tree and found nothing
        /// script-capable.
        /// </summary>
        [TestMethod]
        [DataRow( "<svg><style><img src=x onerror=alert(1)></style></svg>", "Simple", "" )]
        [DataRow( "<math><mtext><table><mglyph><style><img src=x onerror=alert(1)>", "Simple", "" )]
        [DataRow( "<math><mtext><table><mglyph><style><!--</style><img title=\"--&gt;&lt;img src=1 onerror=alert(1)&gt;\">", "Simple", "" )]
        [DataRow( "<svg></p><style><a id=\"</style><img src=1 onerror=alert(1)>\">", "Simple", "<p></p>\"&gt;" )]
        [DataRow( "<form><math><mtext></form><form><mglyph><style></math><img src onerror=alert(1)>", "Simple", "" )]
        [DataRow( "<math><mtext><h1><a><h6></a></h6><mglyph><svg><mtext><style><a title=\"</style><img src onerror=alert(1)>\"></style></h1>", "Simple", "" )]
        [DataRow( "<svg><foreignObject><img src=x onerror=alert(1)></foreignObject></svg>", "Simple", "" )]
        [DataRow( "<svg><foreignObject><p>ok</p></foreignObject><path d=\"M0 0\"/></svg>", "Simple", "" )]
        [DataRow( "<math><annotation-xml encoding=\"text/html\"><img src=x onerror=alert(1)></annotation-xml></math>", "Simple", "" )]
        [DataRow( "<math><mi><img src=x onerror=alert(1)></mi></math>", "Simple", "" )]
        [DataRow( "<svg><desc><img src=x onerror=alert(1)></desc></svg>", "Simple", "" )]
        [DataRow( "<svg><title><img src=x onerror=alert(1)></title></svg>", "Simple", "" )]
        [DataRow( "<svg><script>alert(1)</script></svg>", "Simple", "" )]
        [DataRow( "<svg onload=alert(1)><path d=\"M0 0\"/></svg>", "Simple", "" )]
        [DataRow( "<svg><a xlink:href=\"javascript:alert(1)\"><text x=\"20\" y=\"20\">x</text></a></svg>", "Simple", "" )]
        [DataRow( "<svg><a href=\"javascript:alert(1)\"><circle r=\"10\"/></a></svg>", "Simple", "" )]
        [DataRow( "<svg><a><set attributeName=\"href\" to=\"javascript:alert(1)\"/><circle r=\"10\"/></a></svg>", "Simple", "" )]
        [DataRow( "<svg><a><animate attributeName=\"href\" values=\"javascript:alert(1)\"/><circle r=\"10\"/></a></svg>", "Simple", "" )]
        [DataRow( "<svg><use href=\"data:image/svg+xml;base64,PHN2ZyBvbmxvYWQ9YWxlcnQoMSk+\"/></svg>", "Simple", "" )]
        [DataRow( "<svg><image href=\"javascript:alert(1)\"/><image xlink:href=\"data:image/svg+xml,x\"/></svg>", "Simple", "" )]
        [DataRow( "<svg><path d=\"M0 0\" style=\"fill: url(javascript:alert(1))\" fill=\"url(https://evil.example/x.svg#a)\"/></svg>", "Simple", "" )]
        [DataRow( "<svg><![CDATA[<img src=x onerror=alert(1)>]]></svg>", "Simple", "" )]
        [DataRow( "<svg><desc><![CDATA[</desc><img src=x onerror=alert(1)>]]></desc></svg>", "Simple", "" )]
        [DataRow( "<svg><p><style><img src=x onerror=alert(1)></style></p></svg>", "Simple", "<p></p>" )]
        [DataRow( "<svg><textarea><img src=x onerror=alert(1)></textarea></svg>", "Simple", "" )]
        [DataRow( "<math><style><img src=x onerror=alert(1)></style></math>", "Simple", "" )]
        [DataRow( "<svg><svg><circle r=\"5\"/></svg></svg>", "Simple", "" )]
        [DataRow( "<svg><g><path d=\"M0 0\"/></g></svg><p>after</p>", "Simple", "<p>after</p>" )]
        [DataRow( "<table><svg><path d=\"M0 0\"/></svg></table>", "Simple", "" )]
        [DataRow( "<select><svg><path d=\"M0 0\"/></svg></select>", "Simple", "" )]
        [DataRow( "<textarea><svg onload=alert(1)></textarea>", "Simple", "" )]
        [DataRow( "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\">", "Simple", "\"&gt;" )]
        [DataRow( "<svg><a><circle r=\"10\"/><text>x</text></a></svg>", "Simple", "" )]
        [DataRow( "<svg width=\"48\" height=\"48\" viewBox=\"0 0 48 48\"><g fill=\"none\" fill-rule=\"evenodd\"><path d=\"M24 4L29 18H44L32 27L36 42L24 33L12 42L16 27L4 18H19Z\" fill=\"#f5a623\"/></g></svg>", "Simple", "" )]
        [DataRow( "<span class=\"badge\"><svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\" role=\"img\"><path d=\"M8 0a8 8 0 100 16A8 8 0 008 0z\" stroke=\"#333\" stroke-width=\"1.5\" stroke-linecap=\"round\"/><circle cx=\"8\" cy=\"8\" r=\"3\"/></svg> Done</span>", "Simple", "<span class=\"badge\"> Done</span>" )]
        [DataRow( "<svg viewBox=\"0 0 24 24\"><rect x=\"2\" y=\"2\" width=\"20\" height=\"20\" rx=\"3\"/><line x1=\"0\" y1=\"0\" x2=\"24\" y2=\"24\"/><polyline points=\"0,0 1,1\"/><polygon points=\"0,0 1,1 2,0\"/><ellipse cx=\"1\" cy=\"1\" rx=\"1\" ry=\"2\"/></svg>", "Simple", "" )]
        [DataRow( "<svg><style><img src=x onerror=alert(1)></style></svg>", "Rich", "<svg></svg><img src=\"x\">" )]
        [DataRow( "<math><mtext><table><mglyph><style><img src=x onerror=alert(1)>", "Rich", "" )]
        [DataRow( "<math><mtext><table><mglyph><style><!--</style><img title=\"--&gt;&lt;img src=1 onerror=alert(1)&gt;\">", "Rich", "" )]
        [DataRow( "<svg></p><style><a id=\"</style><img src=1 onerror=alert(1)>\">", "Rich", "<svg></svg><p></p><img src=\"1\">\"&gt;" )]
        [DataRow( "<form><math><mtext></form><form><mglyph><style></math><img src onerror=alert(1)>", "Rich", "" )]
        [DataRow( "<math><mtext><h1><a><h6></a></h6><mglyph><svg><mtext><style><a title=\"</style><img src onerror=alert(1)>\"></style></h1>", "Rich", "" )]
        [DataRow( "<svg><foreignObject><img src=x onerror=alert(1)></foreignObject></svg>", "Rich", "<svg></svg>" )]
        [DataRow( "<svg><foreignObject><p>ok</p></foreignObject><path d=\"M0 0\"/></svg>", "Rich", "<svg><path d=\"M0 0\"></path></svg>" )]
        [DataRow( "<math><annotation-xml encoding=\"text/html\"><img src=x onerror=alert(1)></annotation-xml></math>", "Rich", "" )]
        [DataRow( "<math><mi><img src=x onerror=alert(1)></mi></math>", "Rich", "" )]
        [DataRow( "<svg><desc><img src=x onerror=alert(1)></desc></svg>", "Rich", "<svg></svg>" )]
        [DataRow( "<svg><title><img src=x onerror=alert(1)></title></svg>", "Rich", "<svg></svg>" )]
        [DataRow( "<svg><script>alert(1)</script></svg>", "Rich", "<svg></svg>" )]
        [DataRow( "<svg onload=alert(1)><path d=\"M0 0\"/></svg>", "Rich", "<svg><path d=\"M0 0\"></path></svg>" )]
        [DataRow( "<svg><a xlink:href=\"javascript:alert(1)\"><text x=\"20\" y=\"20\">x</text></a></svg>", "Rich", "<svg><a></a></svg>" )]
        [DataRow( "<svg><a href=\"javascript:alert(1)\"><circle r=\"10\"/></a></svg>", "Rich", "<svg><a><circle r=\"10\"></circle></a></svg>" )]
        [DataRow( "<svg><a><set attributeName=\"href\" to=\"javascript:alert(1)\"/><circle r=\"10\"/></a></svg>", "Rich", "<svg><a><circle r=\"10\"></circle></a></svg>" )]
        [DataRow( "<svg><a><animate attributeName=\"href\" values=\"javascript:alert(1)\"/><circle r=\"10\"/></a></svg>", "Rich", "<svg><a><circle r=\"10\"></circle></a></svg>" )]
        [DataRow( "<svg><use href=\"data:image/svg+xml;base64,PHN2ZyBvbmxvYWQ9YWxlcnQoMSk+\"/></svg>", "Rich", "<svg></svg>" )]
        [DataRow( "<svg><image href=\"javascript:alert(1)\"/><image xlink:href=\"data:image/svg+xml,x\"/></svg>", "Rich", "<svg></svg>" )]
        [DataRow( "<svg><path d=\"M0 0\" style=\"fill: url(javascript:alert(1))\" fill=\"url(https://evil.example/x.svg#a)\"/></svg>", "Rich", "<svg><path d=\"M0 0\" fill=\"url(https://evil.example/x.svg#a)\"></path></svg>" )]
        [DataRow( "<svg><![CDATA[<img src=x onerror=alert(1)>]]></svg>", "Rich", "<svg>&lt;img src=x onerror=alert(1)&gt;</svg>" )]
        [DataRow( "<svg><desc><![CDATA[</desc><img src=x onerror=alert(1)>]]></desc></svg>", "Rich", "<svg></svg>" )]
        [DataRow( "<svg><p><style><img src=x onerror=alert(1)></style></p></svg>", "Rich", "<svg></svg><p></p>" )]
        [DataRow( "<svg><textarea><img src=x onerror=alert(1)></textarea></svg>", "Rich", "<svg><textarea></textarea></svg><img src=\"x\">" )]
        [DataRow( "<math><style><img src=x onerror=alert(1)></style></math>", "Rich", "<img src=\"x\">" )]
        [DataRow( "<svg><svg><circle r=\"5\"/></svg></svg>", "Rich", "<svg><svg><circle r=\"5\"></circle></svg></svg>" )]
        [DataRow( "<svg><g><path d=\"M0 0\"/></g></svg><p>after</p>", "Rich", "<svg><g><path d=\"M0 0\"></path></g></svg><p>after</p>" )]
        [DataRow( "<table><svg><path d=\"M0 0\"/></svg></table>", "Rich", "<svg><path d=\"M0 0\"></path></svg><table></table>" )]
        [DataRow( "<select><svg><path d=\"M0 0\"/></svg></select>", "Rich", "<select></select>" )]
        [DataRow( "<textarea><svg onload=alert(1)></textarea>", "Rich", "<textarea>&lt;svg onload=alert(1)&gt;</textarea>" )]
        [DataRow( "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\">", "Rich", "<img src=\"x\">\"&gt;" )]
        [DataRow( "<svg><a><circle r=\"10\"/><text>x</text></a></svg>", "Rich", "<svg><a><circle r=\"10\"></circle></a></svg>" )]
        [DataRow( "<svg width=\"48\" height=\"48\" viewBox=\"0 0 48 48\"><g fill=\"none\" fill-rule=\"evenodd\"><path d=\"M24 4L29 18H44L32 27L36 42L24 33L12 42L16 27L4 18H19Z\" fill=\"#f5a623\"/></g></svg>", "Rich", "<svg width=\"48\" height=\"48\" viewBox=\"0 0 48 48\"><g fill=\"none\" fill-rule=\"evenodd\"><path d=\"M24 4L29 18H44L32 27L36 42L24 33L12 42L16 27L4 18H19Z\" fill=\"#f5a623\"></path></g></svg>" )]
        [DataRow( "<span class=\"badge\"><svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\" role=\"img\"><path d=\"M8 0a8 8 0 100 16A8 8 0 008 0z\" stroke=\"#333\" stroke-width=\"1.5\" stroke-linecap=\"round\"/><circle cx=\"8\" cy=\"8\" r=\"3\"/></svg> Done</span>", "Rich", "<span class=\"badge\"><svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\" role=\"img\"><path d=\"M8 0a8 8 0 100 16A8 8 0 008 0z\" stroke=\"#333\" stroke-width=\"1.5\" stroke-linecap=\"round\"></path><circle cx=\"8\" cy=\"8\" r=\"3\"></circle></svg> Done</span>" )]
        [DataRow( "<svg viewBox=\"0 0 24 24\"><rect x=\"2\" y=\"2\" width=\"20\" height=\"20\" rx=\"3\"/><line x1=\"0\" y1=\"0\" x2=\"24\" y2=\"24\"/><polyline points=\"0,0 1,1\"/><polygon points=\"0,0 1,1 2,0\"/><ellipse cx=\"1\" cy=\"1\" rx=\"1\" ry=\"2\"/></svg>", "Rich", "<svg viewBox=\"0 0 24 24\"><rect x=\"2\" y=\"2\" width=\"20\" height=\"20\" rx=\"3\"></rect><line x1=\"0\" y1=\"0\" x2=\"24\" y2=\"24\"></line><polyline points=\"0,0 1,1\"></polyline><polygon points=\"0,0 1,1 2,0\"></polygon><ellipse cx=\"1\" cy=\"1\" rx=\"1\" ry=\"2\"></ellipse></svg>" )]
        [DataRow( "<svg><style><img src=x onerror=alert(1)></style></svg>", "RichUnwrap", "<svg></svg><img src=\"x\">" )]
        [DataRow( "<math><mtext><table><mglyph><style><img src=x onerror=alert(1)>", "RichUnwrap", "" )]
        [DataRow( "<math><mtext><table><mglyph><style><!--</style><img title=\"--&gt;&lt;img src=1 onerror=alert(1)&gt;\">", "RichUnwrap", "" )]
        [DataRow( "<svg></p><style><a id=\"</style><img src=1 onerror=alert(1)>\">", "RichUnwrap", "<svg></svg><p></p><img src=\"1\">\"&gt;" )]
        [DataRow( "<form><math><mtext></form><form><mglyph><style></math><img src onerror=alert(1)>", "RichUnwrap", "" )]
        [DataRow( "<math><mtext><h1><a><h6></a></h6><mglyph><svg><mtext><style><a title=\"</style><img src onerror=alert(1)>\"></style></h1>", "RichUnwrap", "" )]
        [DataRow( "<svg><foreignObject><img src=x onerror=alert(1)></foreignObject></svg>", "RichUnwrap", "<svg></svg>" )]
        [DataRow( "<svg><foreignObject><p>ok</p></foreignObject><path d=\"M0 0\"/></svg>", "RichUnwrap", "<svg><path d=\"M0 0\"></path></svg>" )]
        [DataRow( "<math><annotation-xml encoding=\"text/html\"><img src=x onerror=alert(1)></annotation-xml></math>", "RichUnwrap", "" )]
        [DataRow( "<math><mi><img src=x onerror=alert(1)></mi></math>", "RichUnwrap", "" )]
        [DataRow( "<svg><desc><img src=x onerror=alert(1)></desc></svg>", "RichUnwrap", "<svg></svg>" )]
        [DataRow( "<svg><title><img src=x onerror=alert(1)></title></svg>", "RichUnwrap", "<svg></svg>" )]
        [DataRow( "<svg><script>alert(1)</script></svg>", "RichUnwrap", "<svg></svg>" )]
        [DataRow( "<svg onload=alert(1)><path d=\"M0 0\"/></svg>", "RichUnwrap", "<svg><path d=\"M0 0\"></path></svg>" )]
        [DataRow( "<svg><a xlink:href=\"javascript:alert(1)\"><text x=\"20\" y=\"20\">x</text></a></svg>", "RichUnwrap", "<svg><a></a></svg>" )]
        [DataRow( "<svg><a href=\"javascript:alert(1)\"><circle r=\"10\"/></a></svg>", "RichUnwrap", "<svg><a><circle r=\"10\"></circle></a></svg>" )]
        [DataRow( "<svg><a><set attributeName=\"href\" to=\"javascript:alert(1)\"/><circle r=\"10\"/></a></svg>", "RichUnwrap", "<svg><a><circle r=\"10\"></circle></a></svg>" )]
        [DataRow( "<svg><a><animate attributeName=\"href\" values=\"javascript:alert(1)\"/><circle r=\"10\"/></a></svg>", "RichUnwrap", "<svg><a><circle r=\"10\"></circle></a></svg>" )]
        [DataRow( "<svg><use href=\"data:image/svg+xml;base64,PHN2ZyBvbmxvYWQ9YWxlcnQoMSk+\"/></svg>", "RichUnwrap", "<svg></svg>" )]
        [DataRow( "<svg><image href=\"javascript:alert(1)\"/><image xlink:href=\"data:image/svg+xml,x\"/></svg>", "RichUnwrap", "<svg></svg>" )]
        [DataRow( "<svg><path d=\"M0 0\" style=\"fill: url(javascript:alert(1))\" fill=\"url(https://evil.example/x.svg#a)\"/></svg>", "RichUnwrap", "<svg><path d=\"M0 0\" fill=\"url(https://evil.example/x.svg#a)\"></path></svg>" )]
        [DataRow( "<svg><![CDATA[<img src=x onerror=alert(1)>]]></svg>", "RichUnwrap", "<svg>&lt;img src=x onerror=alert(1)&gt;</svg>" )]
        [DataRow( "<svg><desc><![CDATA[</desc><img src=x onerror=alert(1)>]]></desc></svg>", "RichUnwrap", "<svg></svg>" )]
        [DataRow( "<svg><p><style><img src=x onerror=alert(1)></style></p></svg>", "RichUnwrap", "<svg></svg><p></p>" )]
        [DataRow( "<svg><textarea><img src=x onerror=alert(1)></textarea></svg>", "RichUnwrap", "<svg><textarea></textarea></svg><img src=\"x\">" )]
        [DataRow( "<math><style><img src=x onerror=alert(1)></style></math>", "RichUnwrap", "<img src=\"x\">" )]
        [DataRow( "<svg><svg><circle r=\"5\"/></svg></svg>", "RichUnwrap", "<svg><svg><circle r=\"5\"></circle></svg></svg>" )]
        [DataRow( "<svg><g><path d=\"M0 0\"/></g></svg><p>after</p>", "RichUnwrap", "<svg><g><path d=\"M0 0\"></path></g></svg><p>after</p>" )]
        [DataRow( "<table><svg><path d=\"M0 0\"/></svg></table>", "RichUnwrap", "<svg><path d=\"M0 0\"></path></svg><table></table>" )]
        [DataRow( "<select><svg><path d=\"M0 0\"/></svg></select>", "RichUnwrap", "<select></select>" )]
        [DataRow( "<textarea><svg onload=alert(1)></textarea>", "RichUnwrap", "<textarea>&lt;svg onload=alert(1)&gt;</textarea>" )]
        [DataRow( "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\">", "RichUnwrap", "<img src=\"x\">\"&gt;" )]
        [DataRow( "<svg><a><circle r=\"10\"/><text>x</text></a></svg>", "RichUnwrap", "<svg><a><circle r=\"10\"></circle></a></svg>" )]
        [DataRow( "<svg width=\"48\" height=\"48\" viewBox=\"0 0 48 48\"><g fill=\"none\" fill-rule=\"evenodd\"><path d=\"M24 4L29 18H44L32 27L36 42L24 33L12 42L16 27L4 18H19Z\" fill=\"#f5a623\"/></g></svg>", "RichUnwrap", "<svg width=\"48\" height=\"48\" viewBox=\"0 0 48 48\"><g fill=\"none\" fill-rule=\"evenodd\"><path d=\"M24 4L29 18H44L32 27L36 42L24 33L12 42L16 27L4 18H19Z\" fill=\"#f5a623\"></path></g></svg>" )]
        [DataRow( "<span class=\"badge\"><svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\" role=\"img\"><path d=\"M8 0a8 8 0 100 16A8 8 0 008 0z\" stroke=\"#333\" stroke-width=\"1.5\" stroke-linecap=\"round\"/><circle cx=\"8\" cy=\"8\" r=\"3\"/></svg> Done</span>", "RichUnwrap", "<span class=\"badge\"><svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\" role=\"img\"><path d=\"M8 0a8 8 0 100 16A8 8 0 008 0z\" stroke=\"#333\" stroke-width=\"1.5\" stroke-linecap=\"round\"></path><circle cx=\"8\" cy=\"8\" r=\"3\"></circle></svg> Done</span>" )]
        [DataRow( "<svg viewBox=\"0 0 24 24\"><rect x=\"2\" y=\"2\" width=\"20\" height=\"20\" rx=\"3\"/><line x1=\"0\" y1=\"0\" x2=\"24\" y2=\"24\"/><polyline points=\"0,0 1,1\"/><polygon points=\"0,0 1,1 2,0\"/><ellipse cx=\"1\" cy=\"1\" rx=\"1\" ry=\"2\"/></svg>", "RichUnwrap", "<svg viewBox=\"0 0 24 24\"><rect x=\"2\" y=\"2\" width=\"20\" height=\"20\" rx=\"3\"></rect><line x1=\"0\" y1=\"0\" x2=\"24\" y2=\"24\"></line><polyline points=\"0,0 1,1\"></polyline><polygon points=\"0,0 1,1 2,0\"></polygon><ellipse cx=\"1\" cy=\"1\" rx=\"1\" ry=\"2\"></ellipse></svg>" )]
        public void Sanitize_ForeignContentVectors_GiveVerifiedOutput( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<svg><path d=\"M0 0\"/></svg><math><mi>x</mi></math>a", "SimpleUnwrap", "a" )]
        [DataRow( "<svg><path d=\"M0 0\"/></svg><math><mi>x</mi></math>a", "RichUnwrap", "<svg><path d=\"M0 0\"></path></svg>a" )]
        [DataRow( "<svg><desc><b>x</b></desc><text>y</text></svg>a", "RichUnwrap", "<svg></svg>a" )]
        [DataRow( "<svg><g><p>x</p></g></svg>", "Rich", "<svg><g></g></svg><p>x</p>" )]
        public void Sanitize_ForeignContentNotAllowed_IsDroppedNeverUnwrapped( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<math><mi><b>x</b></mi><mrow><mi>y</mi></mrow></math>", "RichMath", "<math><mi><b>x</b></mi><mrow><mi>y</mi></mrow></math>" )]
        [DataRow( "<math><mrow></p></mrow></math>", "RichMath", "<math><mrow></mrow></math><p></p>" )]
        [DataRow( "<math><mrow><b>z</b></mrow></math>", "RichMath", "<math><mrow></mrow></math><b>z</b>" )]
        [DataRow( "<svg><font>x</font><path d=\"M0 0\"/></svg>", "Rich", "<svg><path d=\"M0 0\"></path></svg>" )]
        public void Sanitize_ForeignContent_PassesTheNamespaceCheck( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<span style=\"font-family: a[1]; color: red\">a</span><span style=\"color: red{x}; width: 1px\">b</span>", "Rich", "<span style=\"font-family: a[1]; color: red\">a</span><span style=\"color: red{x}; width: 1px\">b</span>" )]
        [DataRow( "<span style=\"color: [red; width: 1px\">c</span><span style=\"color: {red; width: 1px\">d</span>", "Rich", "<span>c</span><span>d</span>" )]
        public void Sanitize_CssBracketsAndBraces_SplitOnlyOutsideThem( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<a href=\"mailto:a@example.com\">a</a><a href=\"https://example.com/\">b</a>", "SimpleNoMailto", "<a>a</a><a href=\"https://example.com/\">b</a>" )]
        [DataRow( "<span style=\"color: red; font-weight: bold\">a</span>", "SimpleNoColor", "<span style=\"font-weight: bold\">a</span>" )]
        public void Sanitize_DisallowedSchemeOrCssProperty_IsRemoved( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "a<!-- c -->b<?php x ?>c", "Simple", "abc" )]
        [DataRow( "<b><!--[if IE]><script>x</script><![endif]-->a</b>", "RichUnwrap", "<b>a</b>" )]
        [DataRow( "<b onclick=\"x\" OnMouseOver=\"y\" ONLOAD=\"z\" foo=\"z\">x</b>", "Simple", "<b>x</b>" )]
        [DataRow( "<img src=x onerror=alert(1)>", "Rich", "<img src=\"x\">" )]
        [DataRow( "<b class=\"x\" CLASS=\"y\">z</b>", "Simple", "<b class=\"x\">z</b>" )]
        [DataRow( "<sup>a</sup><SUB>b</SUB>", "Simple", "<sup>a</sup><sub>b</sub>" )]
        [DataRow( "<a href=/x \" title=y>z</a><span x\"y=\"1\">w</span>", "Simple", "<a href=\"/x\" title=\"y\">z</a><span>w</span>" )]
        [DataRow( "<span title=\"&{alert(1)}\">a</span><span class=\"x&amp;{y}\">b</span>", "Simple", "<span>a</span><span>b</span>" )]
        [DataRow( "<span title=\" a \">x</span>", "Simple", "<span title=\" a \">x</span>" )]
        [DataRow( "<img srcset=\"a.png 1x\" src=\"/b.png\"><a ping=\"https://x/\" href=\"/y\">z</a>", "Rich", "<img src=\"/b.png\"><a href=\"/y\">z</a>" )]
        [DataRow( "<a href=\"https://x/\" xlink:href=\"javascript:x\">y</a>", "Simple", "<a href=\"https://x/\">y</a>" )]
        [DataRow( "<div data-=\"x\" data-toggle=\"modal\" id=\"i\">z</div>", "RichNoData", "<div id=\"i\">z</div>" )]
        public void Sanitize_CommentsAndDisallowedAttributes_AreRemoved( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<a href=\"/relative\">a</a><a href=\"//cdn.example.com/x\">b</a><a href=\"#frag\">c</a><a href=\"mailto:a@b.c\">d</a><a href=\"tel:+1\">e</a><a href=\"HTTPS://x\">f</a><a href=\"page.html\">g</a><a href=\"http://x/\">h</a>", "Simple", "<a href=\"/relative\">a</a><a href=\"//cdn.example.com/x\">b</a><a href=\"#frag\">c</a><a href=\"mailto:a@b.c\">d</a><a href=\"tel:+1\">e</a><a href=\"HTTPS://x\">f</a><a href=\"page.html\">g</a><a href=\"http://x/\">h</a>" )]
        [DataRow( "<a href=\"javascript:alert(1)\">a</a><a href=\"JaVaScRiPt:alert(1)\">b</a><a href=\"vbscript:x\">c</a><a href=\"file:///c:/x\">d</a>", "Simple", "<a>a</a><a>b</a><a>c</a><a>d</a>" )]
        [DataRow( "<a href=\"data:text/html,x\">a</a><a href=\"DATA:image/png;base64,AA\">b</a><a href=\" data:x\">c</a><a href=\"data:image/png;base64,iVBOR\">d</a><a href=\"dAtA:,x\">e</a><a href=\"data&#58;x\">f</a>", "Simple", "<a>a</a><a>b</a><a>c</a><a>d</a><a>e</a><a>f</a>" )]
        [DataRow( "<a href=\"javascript&#58;alert(1)\">a</a><a href=\"javascript&amp;#58;alert(1)\">b</a><a href=\"javascript&amp;#x3A;alert(1)\">c</a><a href=\"javascript&amp;#0058;x\">d</a><a href=\"&#106;avascript:alert(1)\">e</a><a href=\"javascript&colon;alert(1)\">f</a>", "Simple", "<a>a</a><a>b</a><a>c</a><a>d</a><a>e</a><a>f</a>" )]
        [DataRow( "<a href=\"javascript&amp;colon;alert(1)\">a</a>", "Simple", "<a href=\"javascript&amp;colon;alert(1)\">a</a>" )]
        [DataRow( "<a href=\"  https://example.com/\">a</a><a href=\"&#9;/relative\">b</a><a href=\"&#9;javascript:x\">c</a><a href=\" \n javascript:x\">d</a>", "Simple", "<a href=\"https://example.com/\">a</a><a href=\"/relative\">b</a><a>c</a><a>d</a>" )]
        [DataRow( "<a href=\"java&#9;script:alert(1)\">a</a><a href=\"java&#10;script:x\">b</a><a href=\"java&#0;script:x\">c</a><a href=\"java script:x\">d</a><a href=\"java\u0001script:x\">e</a><a href=\"javascript\u000B:x\">f</a><a href=\"jav&#x09;ascript:alert(1)\">g</a>", "Simple", "<a>a</a><a>b</a><a>c</a><a>d</a><a>e</a><a>f</a><a>g</a>" )]
        [DataRow( "<img src=\"javascript:alert(1)\" alt=\"a\"><img src=\"data:image/gif;base64,R0lGOD\">", "Rich", "<img alt=\"a\"><img>" )]
        [DataRow( "<a href=\"https://x/\">a</a>", "Text", "" )]
        public void Sanitize_UrlSchemes_FollowTheUrlRule( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<blockquote cite=\"javascript:alert(1)\">a</blockquote><blockquote cite=\" https://x/\">b</blockquote>", "SimpleCite", "<blockquote>a</blockquote><blockquote cite=\"https://x/\">b</blockquote>" )]
        [DataRow( "<blockquote cite=\"https://x/\">a</blockquote>", "Simple", "<blockquote>a</blockquote>" )]
        [DataRow( "<video poster=\"javascript:alert(1)\">a</video><video poster=\"/p.png\">b</video>", "RichVideo", "<video>a</video><video poster=\"/p.png\">b</video>" )]
        public void Sanitize_UrlAttributesAllowedByACopy_FollowTheUrlRule( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<a href=\"https://ex ample.com/\">a</a><a href=\"https://example.com/a b\">b</a><a href=\"https://example.com/\u0001\">c</a>", "Simple", "<a href=\"https://ex ample.com/\">a</a><a href=\"https://example.com/a b\">b</a><a href=\"https://example.com/\u0001\">c</a>" )]
        [DataRow( "<a href=\"page?time=10:30\">a</a><a href=\"/page?time=10:30\">b</a><a href=\"#a:b\">c</a><a href=\"https://x/?a=b:c\">d</a><a href=\"mailto:a@b.c?subject=a:b\">e</a>", "Simple", "<a>a</a><a href=\"/page?time=10:30\">b</a><a href=\"#a:b\">c</a><a href=\"https://x/?a=b:c\">d</a><a href=\"mailto:a@b.c?subject=a:b\">e</a>" )]
        [DataRow( "<a href=\"https://example.com/[x\">a</a><a href=\"http://[::1\">b</a><a href=\"https://example.com/a]b)\">c</a><a href=\"https://[example.com/\">d</a><a href=\"https://example.com/a(b\">e</a>", "Simple", "<a href=\"https://example.com/[x\">a</a><a href=\"http://[::1\">b</a><a href=\"https://example.com/a]b)\">c</a><a href=\"https://[example.com/\">d</a><a href=\"https://example.com/a(b\">e</a>" )]
        [DataRow( "<a href=\"https://example.com/%zz\">a</a><a href=\"https://example.com/%\">b</a><a href=\"/%E0%A4%A\">c</a>", "Simple", "<a href=\"https://example.com/%zz\">a</a><a href=\"https://example.com/%\">b</a><a href=\"/%E0%A4%A\">c</a>" )]
        [DataRow( "<a href=\":x\">a</a><a href=\"\">b</a>", "Simple", "<a>a</a><a href=\"\">b</a>" )]
        public void Sanitize_MalformedUrls_GiveDefinedOutput( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<a href=\"/x\" target=\"_blank\" rel=\"nofollow\">a</a>", "Simple", "<a href=\"/x\" target=\"_blank\" rel=\"noopener noreferrer\">a</a>" )]
        [DataRow( "<a rel=\"opener\" href=\"/x\" target=\"_BLANK\">a</a>", "Simple", "<a rel=\"noopener noreferrer\" href=\"/x\" target=\"_BLANK\">a</a>" )]
        [DataRow( "<a target=\"_blank\">a</a>", "Simple", "<a target=\"_blank\" rel=\"noopener noreferrer\">a</a>" )]
        [DataRow( "<a href=\"/x\" target=\"_new\" rel=\"nofollow\">a</a><a target=\"blank\">b</a><a target=\"\">c</a>", "Simple", "<a href=\"/x\" rel=\"nofollow\">a</a><a>b</a><a>c</a>" )]
        [DataRow( "<a href=\"/x\" target=\"_self\">a</a><a target=\"_Top\">b</a><a target=\"_parent\">c</a>", "Simple", "<a href=\"/x\" target=\"_self\">a</a><a target=\"_Top\">b</a><a target=\"_parent\">c</a>" )]
        public void Sanitize_Target_IsLimitedAndBlankGetsRel( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "a</body>b", "Simple", "ab" )]
        [DataRow( "<html><body>a</body></html>b", "Simple", "ab" )]
        [DataRow( "<head><title>t</title></head>x", "Simple", "x" )]
        [DataRow( "<frameset><frame></frameset>x", "Simple", "x" )]
        [DataRow( "a<frame>b<frameset>c</frameset>d", "RichUnwrap", "abcd" )]
        [DataRow( "<p>a<frameset>b</frameset></p>", "Simple", "<p>ab</p>" )]
        [DataRow( "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>T</title><base href=\"https://evil/\"><link rel=\"stylesheet\" href=\"x.css\"><style>body{}</style><script>alert(1)</script></head><body class=\"b\" onload=\"x()\"><p>Hello</p></body></html>", "Simple", "<p>Hello</p>" )]
        [DataRow( "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>T</title><base href=\"https://evil/\"><link rel=\"stylesheet\" href=\"x.css\"><style>body{}</style><script>alert(1)</script></head><body class=\"b\" onload=\"x()\"><p>Hello</p></body></html>", "SimpleUnwrap", "<p>Hello</p>" )]
        [DataRow( "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>T</title><base href=\"https://evil/\"><link rel=\"stylesheet\" href=\"x.css\"><style>body{}</style><script>alert(1)</script></head><body class=\"b\" onload=\"x()\"><p>Hello</p></body></html>", "RichUnwrap", "<p>Hello</p>" )]
        public void Sanitize_DocumentMarkup_GivesDocumentParseResult( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<span style=\"color: red; background: none; background-color: #fff; background-image: none; background-position: top; background-repeat: no-repeat; background-size: cover\">a</span>", "Simple", "<span style=\"color: red; background: none; background-color: #fff; background-image: none; background-position: top; background-repeat: no-repeat; background-size: cover\">a</span>" )]
        [DataRow( "<span style=\"font: 12px serif; font-family: Arial, 'Times New Roman'; font-size: 1em; font-style: italic; font-variant: small-caps; font-weight: bold\">a</span>", "Simple", "<span style=\"font: 12px serif; font-family: Arial, 'Times New Roman'; font-size: 1em; font-style: italic; font-variant: small-caps; font-weight: bold\">a</span>" )]
        [DataRow( "<span style=\"text-align: center; text-indent: 1em; text-transform: uppercase; text-shadow: 1px 1px #000; text-overflow: ellipsis; text-decoration: underline; text-decoration-line: underline; text-decoration-color: red; text-decoration-style: dotted; text-decoration-thickness: 2px\">a</span>", "Simple", "<span style=\"text-align: center; text-indent: 1em; text-transform: uppercase; text-shadow: 1px 1px #000; text-overflow: ellipsis; text-decoration: underline; text-decoration-line: underline; text-decoration-color: red; text-decoration-style: dotted; text-decoration-thickness: 2px\">a</span>" )]
        [DataRow( "<span style=\"line-height: 1.5; letter-spacing: 1px; word-spacing: 2px; vertical-align: top; white-space: nowrap; word-break: break-all; overflow-wrap: anywhere; word-wrap: break-word\">a</span>", "Simple", "<span style=\"line-height: 1.5; letter-spacing: 1px; word-spacing: 2px; vertical-align: top; white-space: nowrap; word-break: break-all; overflow-wrap: anywhere; word-wrap: break-word\">a</span>" )]
        [DataRow( "<span style=\"margin: 0; margin-top: 1px; margin-right: 2px; margin-bottom: 3px; margin-left: 4px; padding: 0; padding-top: 1px; padding-right: 2px; padding-bottom: 3px; padding-left: 4px\">a</span>", "Simple", "<span style=\"margin: 0; margin-top: 1px; margin-right: 2px; margin-bottom: 3px; margin-left: 4px; padding: 0; padding-top: 1px; padding-right: 2px; padding-bottom: 3px; padding-left: 4px\">a</span>" )]
        [DataRow( "<span style=\"border: 1px solid red; border-width: 1px; border-style: solid; border-color: red; border-top: none; border-top-width: 1px; border-top-style: solid; border-top-color: red; border-right: none; border-right-width: 1px; border-right-style: solid; border-right-color: red; border-bottom: none; border-bottom-width: 1px; border-bottom-style: solid; border-bottom-color: red; border-left: none; border-left-width: 1px; border-left-style: solid; border-left-color: red\">a</span>", "Simple", "<span style=\"border: 1px solid red; border-width: 1px; border-style: solid; border-color: red; border-top: none; border-top-width: 1px; border-top-style: solid; border-top-color: red; border-right: none; border-right-width: 1px; border-right-style: solid; border-right-color: red; border-bottom: none; border-bottom-width: 1px; border-bottom-style: solid; border-bottom-color: red; border-left: none; border-left-width: 1px; border-left-style: solid; border-left-color: red\">a</span>" )]
        [DataRow( "<span style=\"border-radius: 2px; border-top-left-radius: 1px; border-top-right-radius: 1px; border-bottom-right-radius: 1px; border-bottom-left-radius: 1px\">a</span>", "Simple", "<span style=\"border-radius: 2px; border-top-left-radius: 1px; border-top-right-radius: 1px; border-bottom-right-radius: 1px; border-bottom-left-radius: 1px\">a</span>" )]
        [DataRow( "<span style=\"width: 1px; height: 2px; min-width: 1px; min-height: 2px; max-width: 10px; max-height: 20px; box-sizing: border-box; display: block; float: left; clear: both; list-style: none; list-style-type: disc; list-style-position: inside; box-shadow: none; opacity: 0.5; visibility: hidden\">a</span>", "Simple", "<span style=\"width: 1px; height: 2px; min-width: 1px; min-height: 2px; max-width: 10px; max-height: 20px; box-sizing: border-box; display: block; float: left; clear: both; list-style: none; list-style-type: disc; list-style-position: inside; box-shadow: none; opacity: 0.5; visibility: hidden\">a</span>" )]
        [DataRow( "<div style=\"position: absolute; top: 0; right: 0; bottom: 0; left: 0; z-index: 9; overflow: hidden; overflow-x: auto; overflow-y: scroll; transform: rotate(1deg)\">x</div>", "Rich", "<div style=\"position: absolute; top: 0; right: 0; bottom: 0; left: 0; z-index: 9; overflow: hidden; overflow-x: auto; overflow-y: scroll; transform: rotate(1deg)\">x</div>" )]
        [DataRow( "<table style=\"border-collapse: collapse; border-spacing: 0; table-layout: fixed; caption-side: bottom; empty-cells: hide\"><tr><td>x</td></tr></table>", "Rich", "<table style=\"border-collapse: collapse; border-spacing: 0; table-layout: fixed; caption-side: bottom; empty-cells: hide\"><tbody><tr><td>x</td></tr></tbody></table>" )]
        [DataRow( "<div style=\"display: flex; flex: 1; flex-grow: 1; flex-shrink: 0; flex-basis: auto; flex-direction: row; flex-wrap: wrap; justify-content: center; align-items: center; align-self: end; gap: 1px; row-gap: 1px; column-gap: 1px; order: 2\">x</div>", "Rich", "<div style=\"display: flex; flex: 1; flex-grow: 1; flex-shrink: 0; flex-basis: auto; flex-direction: row; flex-wrap: wrap; justify-content: center; align-items: center; align-self: end; gap: 1px; row-gap: 1px; column-gap: 1px; order: 2\">x</div>" )]
        [DataRow( "<span style=\"COLOR: Red; Font-Weight: bold !important;\">x</span>", "Simple", "<span style=\"color: Red; font-weight: bold !important\">x</span>" )]
        [DataRow( "<span style=\"  color:red ;; width:1px  \">x</span>", "Simple", "<span style=\"color: red; width: 1px\">x</span>" )]
        [DataRow( "<span style=\"behavior: url(x.htc); -moz-binding: url(x); cursor: pointer; pointer-events: none; content: 'x'; filter: none; --x: 1; -webkit-box-shadow: none; inset: 0; margin-inline-start: 1px; color: red\">x</span>", "Simple", "<span style=\"color: red\">x</span>" )]
        [DataRow( "<span style=\"font-family: '<b>'; color: red\">a</span><span style=\"font-family: '&gt;'; color: blue\">b</span>", "Simple", "<span style=\"color: red\">a</span><span style=\"color: blue\">b</span>" )]
        [DataRow( "<span style=\"width: expression(alert(1)); color: red\">a</span><span style=\"width: EXPRESSION(1)\">b</span><span style=\"width: \uFF45\uFF58\uFF50\uFF52\uFF45\uFF53\uFF53\uFF49\uFF4F\uFF4E(alert(1))\">c</span>", "Simple", "<span style=\"color: red\">a</span><span>b</span><span>c</span>" )]
        [DataRow( "<span style=\"background-image: url(https://example.com/a.png)\">a</span><span style=\"background-image: url('/a.png')\">b</span><span style=\"background: url(&quot;a.png&quot;) no-repeat\">c</span><span style=\"background-image: url()\">d</span>", "Simple", "<span style=\"background-image: url(https://example.com/a.png)\">a</span><span style=\"background-image: url('/a.png')\">b</span><span style=\"background: url(&quot;a.png&quot;) no-repeat\">c</span><span style=\"background-image: url()\">d</span>" )]
        [DataRow( "<span style=\"background-image: url(javascript:alert(1)); color: red\">a</span><span style=\"background-image: url(data:image/png;base64,AA)\">b</span><span style=\"background-image: url( java script:alert(1) )\">c</span><span style=\"background-image: URL(javascript:x)\">d</span><span style=\"background-image: u\u0280\u029F(javascript:x)\">e</span><span style=\"background: url(/ok.png), url('vbscript:x')\">f</span>", "Simple", "<span style=\"color: red\">a</span><span>b</span><span>c</span><span>d</span><span>e</span><span>f</span>" )]
        [DataRow( "<span style=\"foo: bar\">a</span><span style=\"   \">b</span><span style=\";;\">c</span><span style=\"color\">d</span><span style=\"color:\">e</span>", "Simple", "<span>a</span><span>b</span><span>c</span><span>d</span><span>e</span>" )]
        [DataRow( "<span style=\"font-family: &quot;a;b&quot;; color: red\">a</span><span style=\"color: rgb(1,2,3); width: calc(1px + 2px)\">b</span>", "Simple", "<span style=\"font-family: &quot;a;b&quot;; color: red\">a</span><span style=\"color: rgb(1,2,3); width: calc(1px + 2px)\">b</span>" )]
        [DataRow( "<span style=\"font-family: &quot;a; color: red\">a</span><span style=\"background-image: url(a&quot;b); position: fixed\">b</span>", "Rich", "<span>a</span><span>b</span>" )]
        [DataRow( "<span style=\"color: rgb(1]; width: 1px\">a</span><span style=\"font-family: &quot;a&#10;; position: fixed\">b</span><span style=\"color: red); width: 1px\">c</span>", "Rich", "<span style=\"width: 1px\">a</span><span>b</span><span style=\"width: 1px\">c</span>" )]
        [DataRow( "<span style=\"color: red\" style=\"width: 1px\">a</span>", "Simple", "<span style=\"color: red\">a</span>" )]
        public void Sanitize_Css_FollowsTheCssRule( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        /// <summary>
        /// The raw-text CSS rule: a declaration whose value contains a
        /// backslash escape, a comment or <c>@</c> is removed without decoding
        /// it, a value no CSS parser would accept is kept as written, and an
        /// empty style attribute is removed.
        /// </summary>
        [TestMethod]
        [DataRow( "<span style=\"color: red\\9\">a</span><span style=\"color: red; width: e\\78 pression(1)\">b</span><span style=\"background: u\\72l(javascript:x); color: blue\">c</span>", "Simple", "<span>a</span><span style=\"color: red\">b</span><span style=\"color: blue\">c</span>" )]
        [DataRow( "<span style=\"font-family: a@b; color: red\">a</span><span style=\"color: red /* c */\">b</span><span style=\"color: red */\">c</span><span style=\"color: /**/red; width: 1px\">d</span>", "Simple", "<span style=\"color: red\">a</span><span>b</span><span>c</span><span style=\"width: 1px\">d</span>" )]
        [DataRow( "<span style=\"color: banana\">a</span>", "Simple", "<span style=\"color: banana\">a</span>" )]
        [DataRow( "<span style=\"\">a</span>", "Simple", "<span>a</span>" )]
        public void Sanitize_Css_RawTextRule( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<b>a</b><font color=\"red\">b</font><font size=\"1\">c</font>", "SimpleFont", "<b>a</b><font color=\"red\">b</font><font>c</font>" )]
        [DataRow( "<span class=\"mention\" data-identifier=\"abc\">@Ted</span><span data-other=\"x\">y</span><b data-identifier=\"z\">z</b>", "NoteText", "<span class=\"mention\" data-identifier=\"abc\">@Ted</span><span>y</span>z" )]
        [DataRow( "<span class=\"mention\" data-identifier=\"abc\" style=\"color: red\" title=\"t\">@Ted</span>", "NoteText", "<span class=\"mention\" data-identifier=\"abc\">@Ted</span>" )]
        [DataRow( "a\n<div class=\"c\" style=\"color: red\">b</div><br class=\"c\">c &amp; &lt; &nbsp;", "NoteText", "a\n<div>b</div><br>c &amp; &lt; &nbsp;" )]
        [DataRow( "<b style=\"color: red\">a</b><a href=\"https://x\">b</a><h1>c</h1>", "NoteText", "abc" )]
        [DataRow( "a<img src=\"/x.png\">b<script>x</script><style>y</style>c", "NoteText", "abc" )]
        [DataRow( "Tom & Jerry say 3 < 5", "NoteText", "Tom &amp; Jerry say 3 &lt; 5" )]
        [DataRow( "<span class=\"mention\" data-identifier=\"abc\">@Ted</span>", "Simple", "<span class=\"mention\">@Ted</span>" )]
        [DataRow( "<p>a<img src=\"/a.png\" alt=\"x\">b</p>", "RichNoImg", "<p>ab</p>" )]
        [DataRow( "<img src=\"/a.png\" alt=\"x\" width=\"1\">", "RichNoImgAlt", "<img src=\"/a.png\" width=\"1\">" )]
        [DataRow( "<a href=\"/x\" title=\"t\">a</a><b title=\"t\" class=\"c\">b</b>", "SimpleNoTitle", "<a href=\"/x\">a</a><b class=\"c\">b</b>" )]
        public void Sanitize_EditedOptions_ApplyTheirRules( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "<td>a</td><li>b</li>", "Rich", "a<li>b</li>" )]
        [DataRow( "<td>a</td><li>b</li>", "Simple", "a" )]
        [DataRow( "<tr><td>a</td></tr>", "RichUnwrap", "a" )]
        [DataRow( "<p>a<p>b", "Simple", "<p>a</p><p>b</p>" )]
        [DataRow( "<b>a<i>b</b>c</i>", "Simple", "<b>a<i>b</i></b><i>c</i>" )]
        [DataRow( "<a href=\"/a\"><b>x</a>y</b>", "Simple", "<a href=\"/a\"><b>x</b></a><b>y</b>" )]
        [DataRow( "<pre>\nx</pre>", "Simple", "<pre>x</pre>" )]
        public void Sanitize_MisnestedOrOutOfContextMarkup_IsHandledByTheParser( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        [DataRow( "plain text, nothing to do.", "Simple", "plain text, nothing to do." )]
        [DataRow( "plain\r\ntext", "Simple", "plain\r\ntext" )]
        [DataRow( "a\r\nb<br>", "Simple", "a\nb<br>" )]
        [DataRow( "a & b", "Simple", "a &amp; b" )]
        [DataRow( "a > b", "Simple", "a &gt; b" )]
        [DataRow( "a < b", "Simple", "a &lt; b" )]
        [DataRow( "a\u00A0b", "Simple", "a&nbsp;b" )]
        [DataRow( "a\0b", "Simple", "ab" )]
        [DataRow( "a &amp; b &lt; c &gt; d&nbsp;e", "Simple", "a &amp; b &lt; c &gt; d&nbsp;e" )]
        [DataRow( "&lt;script&gt;alert(1)&lt;/script&gt;", "Simple", "&lt;script&gt;alert(1)&lt;/script&gt;" )]
        [DataRow( "<p>a</p>\n<p>b</p>", "Simple", "<p>a</p>\n<p>b</p>" )]
        [DataRow( "", "Simple", "" )]
        [DataRow( "   ", "Simple", "   " )]
        [DataRow( " \t\n ", "Rich", " \t\n " )]
        public void Sanitize_TextAndFastPath_GiveTheFullPathResult( string html, string optionsName, string expected )
        {
            AssertSanitized( html, optionsName, expected );
        }

        [TestMethod]
        public void Sanitize_UnwrapAcrossAParserScope_MayReshapeOnceOnly()
        {
            var sanitizer = new HtmlAllowListSanitizer();
            var options = NamedOptions["RichUnwrap"];

            var first = sanitizer.Sanitize( "<p><marquee><p>x</p></marquee></p>", options );
            var second = sanitizer.Sanitize( first, options );

            Assert.AreEqual( "<p><p>x</p></p>", first );
            Assert.AreEqual( "<p></p><p>x</p><p></p>", second );
            Assert.AreEqual( second, sanitizer.Sanitize( second, options ) );
        }

        [TestMethod]
        public void Sanitize_NestingAtTheLimit_IsKept()
        {
            var html = Nest( "b", HtmlAllowListTables.MaxNestingDepth, "x" );

            AssertSanitized( html, "Rich", html );
        }

        [TestMethod]
        public void Sanitize_NestingPastTheLimit_RemovesTheDeeperElementWithItsContent()
        {
            var html = Nest( "b", HtmlAllowListTables.MaxNestingDepth + 1, "x" );

            AssertSanitized( html, "Rich", Nest( "b", HtmlAllowListTables.MaxNestingDepth, string.Empty ) );
        }

        [TestMethod]
        public void Sanitize_DeepestParsedNestingOnASmallStack_DoesNotOverflow()
        {
            var depth = HtmlAllowListTables.MaxEstimatedNestingDepth;

            Assert.AreEqual( Nest( "b", HtmlAllowListTables.MaxNestingDepth, string.Empty ), SanitizeOnSmallStack( Nest( "b", depth, "x" ), "Rich" ) );
            Assert.AreEqual( string.Empty, SanitizeOnSmallStack( Nest( "font", depth, "x" ), "RichUnwrap" ) );
        }

        [TestMethod]
        public void Sanitize_VeryDeepNestingOnASmallStack_ComesBackAsText()
        {
            var html = Nest( "b", 50000, "x" );

            Assert.AreEqual( AsText( html ), SanitizeOnSmallStack( html, "Rich" ) );
        }

        private static string SanitizeOnSmallStack( string html, string optionsName )
        {
            string output = null;

            // IIS request threads have a 256 KB stack; a stack overflow there can not be caught and stops the process.
            var thread = new System.Threading.Thread( () => output = new HtmlAllowListSanitizer().Sanitize( html, NamedOptions[optionsName] ), 256 * 1024 );
            thread.Start();
            thread.Join();

            return output;
        }

        [TestMethod]
        public void Sanitize_AtTheEstimatedDepthLimit_IsParsed()
        {
            AssertSanitized( Nest( "div", HtmlAllowListTables.MaxEstimatedNestingDepth, "x" ), "Rich", Nest( "div", HtmlAllowListTables.MaxNestingDepth, string.Empty ) );
        }

        [TestMethod]
        public void Sanitize_PastTheEstimatedDepthLimit_ComesBackAsText()
        {
            var html = Nest( "div", HtmlAllowListTables.MaxEstimatedNestingDepth + 1, "x" );

            AssertSanitized( html, "Rich", AsText( html ) );
        }

        [TestMethod]
        public void Sanitize_PastTheEstimatedDepthLimit_EncodesLikeTheSerializer()
        {
            var tags = Repeat( "<div>", HtmlAllowListTables.MaxEstimatedNestingDepth + 1 );

            AssertSanitized( "a & b c\r\nd\re\0f" + tags, "Rich", "a &amp; b&nbsp;c\nd\nef" + AsText( tags ) );
        }

        [TestMethod]
        public void Sanitize_DepthHiddenByMarkupTricks_IsStillCounted()
        {
            foreach ( var repeated in new[] { "<div a=\"</div>\">", "<div a='</div>'>", "<div></x>", "<div><!-- </div> -->" } )
            {
                var html = Repeat( repeated, HtmlAllowListTables.MaxEstimatedNestingDepth + 1 ) + "x";

                AssertSanitized( html, "Rich", AsText( html ) );
            }
        }

        [TestMethod]
        public void Sanitize_DepthAfterABangClosedComment_IsStillCounted()
        {
            var html = "<!-- a --!>" + Repeat( "<div>", HtmlAllowListTables.MaxEstimatedNestingDepth + 1 ) + "x";

            AssertSanitized( html, "Rich", AsText( html ) );
        }

        [TestMethod]
        public void Sanitize_ManyUnclosedListItemsAndVoidElements_AreParsed()
        {
            AssertSanitized( "<ul>" + Repeat( "<li>a", 2000 ) + "</ul>", "Rich", "<ul>" + Repeat( "<li>a</li>", 2000 ) + "</ul>" );
            AssertSanitized( Repeat( "<br>", 2000 ), "Rich", Repeat( "<br>", 2000 ) );
        }

        private static string Repeat( string text, int count )
        {
            return string.Concat( Enumerable.Repeat( text, count ) );
        }

        private static string AsText( string html )
        {
            return html.Replace( "&", "&amp;" ).Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
        }

        private static string Nest( string tag, int depth, string content )
        {
            return string.Concat( Enumerable.Repeat( $"<{tag}>", depth ) ) + content + string.Concat( Enumerable.Repeat( $"</{tag}>", depth ) );
        }

        [TestMethod]
        public void Sanitize_NullInput_ReturnsNull()
        {
            Assert.IsNull( new HtmlAllowListSanitizer().Sanitize( null, HtmlSanitizeOptions.Simple ) );
        }

        [TestMethod]
        public void SanitizeHtmlExtension_UsesTheRegisteredSanitizer()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                Assert.AreEqual( "<b>x</b>", "<b onclick=\"y\">x</b><script>z</script>".SanitizeHtml( HtmlSanitizeOptions.Simple ) );
            }
        }

        [TestMethod]
        public void SanitizeHtmlExtension_WithoutRegistration_Throws()
        {
            using ( TestHelper.CreateScopedRockApp( sc => sc.RemoveAll<IHtmlAllowListSanitizer>() ) )
            {
                Assert.ThrowsExactly<InvalidOperationException>( () => "<b>x</b>".SanitizeHtml( HtmlSanitizeOptions.Simple ) );
            }
        }

        [TestMethod]
        public void Sanitize_ManyParallelCalls_GiveSequentialResults()
        {
            var sanitizer = new HtmlAllowListSanitizer();
            var options = new HtmlSanitizeOptions( HtmlSanitizeOptions.Rich ).AllowAttribute( "span", "data-identifier" );
            var inputs = new[]
            {
                "<p onclick=\"x\" style=\"color: red; position: fixed\">a<script>b</script></p>",
                "<a href=\"javascript:x\" target=\"_blank\">c</a><img src=\"/a.png\" onerror=\"x\">",
                "<table><td style=\"width: expression(1)\">d</td></table><span data-identifier=\"1\">e</span>",
                "<font>f</font>g &amp; h"
            };

            var expected = inputs.Select( input => sanitizer.Sanitize( input, options ) ).ToArray();
            var actual = new string[400];

            Parallel.For( 0, actual.Length, i =>
            {
                actual[i] = sanitizer.Sanitize( inputs[i % inputs.Length], options );
            } );

            CollectionAssert.AreEqual(
                new[]
                {
                    "<p style=\"color: red\">a</p>",
                    "<a target=\"_blank\" rel=\"noopener noreferrer\">c</a><img src=\"/a.png\">",
                    "<table><tbody><tr><td>d</td></tr></tbody></table><span data-identifier=\"1\">e</span>",
                    "g &amp; h"
                },
                expected );

            for ( var i = 0; i < actual.Length; i++ )
            {
                Assert.AreEqual( expected[i % inputs.Length], actual[i] );
            }
        }
    }
}
