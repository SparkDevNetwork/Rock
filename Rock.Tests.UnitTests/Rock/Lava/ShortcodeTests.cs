﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;
using Scripturize = Rock.Utility.Scripturize;

namespace Rock.Tests.Rock.Lava
{
    /// <summary>
    /// A test class for testing any Rock Shortcode Lava.
    /// </summary>
    [TestClass]
    public class ShortcodeTests
    {
        #region Scripturize Tests
        /// <summary>
        /// Scripturizes for YouVersion format with the simple cases from the documentation:
        /// John 3:16
        /// Jn 3:16
        /// Jn 3:16 (NIV)
        /// 1 Peter 1:1-10
        /// </summary>
        [TestMethod]
        public void Scripturize_YouVersion_SimpleCase()
        {
            var output = Scripturize.Parse( "John 3:16" );
            var expected = "<a href=\"https://www.bible.com/bible/116/JHN.3.16.NLT\"  title=\"YouVersion\">John 3:16</a>";
            Assert.That.AreEqual( expected, output );

            output = Scripturize.Parse( "Jn 3:16" );
            expected = "<a href=\"https://www.bible.com/bible/116/JHN.3.16.NLT\"  title=\"YouVersion\">Jn 3:16</a>";
            Assert.That.AreEqual( expected, output );

            output = Scripturize.Parse( "John 3" );
            expected = "<a href=\"https://www.bible.com/bible/116/JHN.3..NLT\"  title=\"YouVersion\">John 3</a>";
            Assert.That.AreEqual( expected, output );
        }

        /// <summary>
        /// Scripturizes for YouVersion format with the Bible translation-version being inferred from the given text.
        /// </summary>
        [TestMethod]
        public void Scripturize_YouVersion_TranslationInfer()
        {
            var output = Scripturize.Parse( "Jn 3:16 (NIV)" );
            var expected = "<a href=\"https://www.bible.com/bible/111/JHN.3.16.NIV\"  title=\"YouVersion\">Jn 3:16 (NIV)</a>";
            Assert.That.AreEqual( expected, output );
        }

        /// <summary>
        /// Scripturizes for YouVersion format with one book and many verses from the documentation:
        /// 1 Peter 1:1-10
        /// </summary>
        [TestMethod]
        public void Scripturize_YouVersion_ManyVerses()
        {
            var output = Scripturize.Parse( "1 Peter 1:1-10" );
            var expected = "<a href=\"https://www.bible.com/bible/116/1PE.1.1-10.NLT\"  title=\"YouVersion\">1 Peter 1:1-10</a>";
            Assert.That.AreEqual( expected, output );
        }

        /// <summary>
        /// Scripturizes YouVersion format with multiple verses.
        /// </summary>
        [TestMethod]
        public void Scripturize_YouVersion_Multiple()
        {
            var output = Scripturize.Parse( "John 3:16-18, 1 Peter 1:1-10" ).Replace( "  ", " " );
            var expected = "<a href=\"https://www.bible.com/bible/116/JHN.3.16-18.NLT\"  title=\"YouVersion\">John 3:16-18</a>, <a href=\"https://www.bible.com/bible/116/1PE.1.1-10.NLT\"  title=\"YouVersion\">1 Peter 1:1-10</a>".Replace( "  ", " " );
            Assert.That.AreEqual( expected, output );
        }

        #endregion

    }
}
