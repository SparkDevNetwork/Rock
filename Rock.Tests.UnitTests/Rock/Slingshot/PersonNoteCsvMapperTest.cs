using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Slingshot;
using Slingshot.Core.Model;

namespace Rock.Tests.UnitTests.Rock.Slingshot
{
    [TestClass]
    public class PersonNoteCsvMapperTest
    {
        Dictionary<string, object> csvEntry = new Dictionary<string, object>();
        Dictionary<string, string> headerMapper = new Dictionary<string, string>();

        [Ignore] // TODO: figure out how to mock the Cache classes
        [TestMethod]
        public void ShouldDefaultNoteTypeToPersonTimelineNoteIfNotPresent()
        {
            headerMapper["Id"] = "Id";
            headerMapper["Note"] = "Text";

            csvEntry["Id"] = "45";
            csvEntry["Text"] = "The note to be inserted";

            HashSet<string> parserErrors = new HashSet<string>();

            PersonNote personNote = PersonNoteCsvMapper.Map( csvEntry, headerMapper, ref parserErrors );

            Assert.AreEqual( 45, personNote.PersonId );
            Assert.AreEqual( "The note to be inserted", personNote.Text );
            Assert.AreEqual( "PERSON_TIMELINE_NOTE", personNote.NoteType );
        }

        [Ignore]
        [TestMethod]
        public void ShouldGenerateIdIfNotPresent()
        {
            headerMapper["Id"] = "Id";
            headerMapper["Note"] = "Text";

            csvEntry["Id"] = "45";
            csvEntry["Text"] = "The note to be inserted";

            HashSet<string> parserErrors = new HashSet<string>();

            PersonNote personNote = PersonNoteCsvMapper.Map( csvEntry, headerMapper, ref parserErrors );

            Assert.AreEqual( 45, personNote.PersonId );
            Assert.AreEqual( "The note to be inserted", personNote.Text );
            Assert.IsNotNull( personNote.Id );
        }
    }
}
