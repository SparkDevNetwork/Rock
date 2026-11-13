using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;
using Rock.Utility;

namespace Rock.Tests.Utility
{
    /// <summary>
    /// A test class for testing the Scripturize utility.
    /// </summary>
    [TestClass]
    public class ScripturizeTests
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
            Assert.AreEqual( expected, output );

            output = Scripturize.Parse( "Jn 3:16" );
            expected = "<a href=\"https://www.bible.com/bible/116/JHN.3.16.NLT\"  title=\"YouVersion\">Jn 3:16</a>";
            Assert.AreEqual( expected, output );

            output = Scripturize.Parse( "John 3" );
            expected = "<a href=\"https://www.bible.com/bible/116/JHN.3.NLT\"  title=\"YouVersion\">John 3</a>";
            Assert.AreEqual( expected, output );
        }

        /// <summary>
        /// Scripturizes for YouVersion format with chapter-only references, which
        /// must not include an empty verse segment in the link.
        /// </summary>
        /// <remarks>
        /// Fixes Issue #7160. (refer https://github.com/SparkDevNetwork/Rock/issues/7160)
        /// </remarks>
        [TestMethod]
        public void Scripturize_YouVersion_ChapterOnly()
        {
            var output = Scripturize.Parse( "Genesis 2", "NIV" );
            var expected = "<a href=\"https://www.bible.com/bible/111/GEN.2.NIV\"  title=\"YouVersion\">Genesis 2</a>";
            Assert.AreEqual( expected, output );

            output = Scripturize.Parse( "1 John 2 (ESV)" );
            expected = "<a href=\"https://www.bible.com/bible/59/1JN.2.ESV\"  title=\"YouVersion\">1 John 2 (ESV)</a>";
            Assert.AreEqual( expected, output );

            output = Scripturize.Parse( "Genesis 2:4", "NIV" );
            expected = "<a href=\"https://www.bible.com/bible/111/GEN.2.4.NIV\"  title=\"YouVersion\">Genesis 2:4</a>";
            Assert.AreEqual( expected, output );
        }

        /// <summary>
        /// Scripturizes for Bible Gateway format with chapter-only and verse references.
        /// </summary>
        [TestMethod]
        public void Scripturize_BibleGateway_ChapterOnlyAndVerses()
        {
            var output = Scripturize.Parse( "Genesis 2", "NIV", LandingSite.BibleGateway );
            var expected = "<a href=\"http://biblegateway.com/bible?version=NIV&passage=Genesis+2\"  title=\"Bible Gateway\">Genesis 2</a>";
            Assert.AreEqual( expected, output );

            output = Scripturize.Parse( "Genesis 2:4-7", "NIV", LandingSite.BibleGateway );
            expected = "<a href=\"http://biblegateway.com/bible?version=NIV&passage=Genesis+2:4-7\"  title=\"Bible Gateway\">Genesis 2:4-7</a>";
            Assert.AreEqual( expected, output );
        }

        /// <summary>
        /// Scripturizes for YouVersion format with the Bible translation-version being inferred from the given text.
        /// </summary>
        [TestMethod]
        public void Scripturize_YouVersion_TranslationInfer()
        {
            var output = Scripturize.Parse( "Jn 3:16 (NIV)" );
            var expected = "<a href=\"https://www.bible.com/bible/111/JHN.3.16.NIV\"  title=\"YouVersion\">Jn 3:16 (NIV)</a>";
            Assert.AreEqual( expected, output );
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
            Assert.AreEqual( expected, output );
        }

        /// <summary>
        /// Scripturizes YouVersion format with multiple verses.
        /// </summary>
        [TestMethod]
        public void Scripturize_YouVersion_Multiple()
        {
            var output = Scripturize.Parse( "John 3:16-18, 1 Peter 1:1-10" ).Replace( "  ", " " );
            var expected = "<a href=\"https://www.bible.com/bible/116/JHN.3.16-18.NLT\"  title=\"YouVersion\">John 3:16-18</a>, <a href=\"https://www.bible.com/bible/116/1PE.1.1-10.NLT\"  title=\"YouVersion\">1 Peter 1:1-10</a>".Replace( "  ", " " );
            Assert.AreEqual( expected, output );
        }

        /// <summary>
        /// Scripturizes YouVersion format with references to both John's Gospel and Epistles.
        /// </summary>
        /// <remarks>
        /// Fixes Issue #5053. (refer https://github.com/SparkDevNetwork/Rock/issues/5053)
        /// </remarks>
        [TestMethod]
        public void Scripturize_YouVersion_JohnsGospelAndEpistles()
        {
            var output = Scripturize.Parse( "John 3:16-18, 1 John 1:1-5, John 3:16-18" ).Replace( "  ", " " );
            var expected = @"
<a href=""https://www.bible.com/bible/116/JHN.3.16-18.NLT""  title=""YouVersion"">John 3:16-18</a>,
<a href=""https://www.bible.com/bible/116/1JN.1.1-5.NLT"" title=""YouVersion"">1 John 1:1-5</a>,
<a href=""https://www.bible.com/bible/116/JHN.3.16-18.NLT""  title=""YouVersion"">John 3:16-18</a>
";
            Assert.That.AreEqualIgnoreWhitespace( expected, output );
        }

        #endregion

    }
}
