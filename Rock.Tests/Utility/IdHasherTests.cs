using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Utility
{
    [TestClass]
    public class IdHasherTests
    {
        [TestMethod]
        public void GetHash_ReturnsTenCharacterHash()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;

            var hash = idHasher.GetHash( 123 );

            Assert.HasCount( 10, hash );
        }

        [TestMethod]
        public void GetHash_ReturnsExpectedHash()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;

            var hash = idHasher.GetHash( 123 );

            Assert.AreEqual( "lOjq1jxXNe", hash );
        }

        [TestMethod]
        public void GetId_WithValidHash_ReturnsId()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;
            var hash = "lOjq1jxXNe";

            var id = idHasher.GetId( hash );

            Assert.AreEqual( 123, id );
        }

        [TestMethod]
        public void GetId_WithInvalidHash_ReturnsNull()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;

            var id = idHasher.GetId( "hello world" );

            Assert.IsNull( id );
        }

        [TestMethod]
        public void GetId_WithEmptyHash_ReturnsNull()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;

            var id = idHasher.GetId( string.Empty );

            Assert.IsNull( id );
        }

        [TestMethod]
        public void GetId_WithNullHash_ReturnsNull()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;

            var id = idHasher.GetId( null );

            Assert.IsNull( id );
        }

        [TestMethod]
        public void TryGetId_WithValidHash_ReturnsTrueAndId()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;
            var hash = "lOjq1jxXNe";

            var result = idHasher.TryGetId( hash, out var id );

            Assert.IsTrue( result );
            Assert.AreEqual( 123, id );
        }

        [TestMethod]
        public void TryGetId_WithInvalidHash_ReturnsFalseAndZeroId()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;

            var result = idHasher.TryGetId( "hello world", out var id );

            Assert.IsFalse( result );
            Assert.AreEqual( 0, id );
        }

        [TestMethod]
        public void TryGetId_WithEmptyHash_ReturnsFalseAndZeroId()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;

            var result = idHasher.TryGetId( string.Empty, out var id );

            Assert.IsFalse( result );
            Assert.AreEqual( 0, id );
        }

        [TestMethod]
        public void TryGetId_WithNullHash_ReturnsFalseAndZeroId()
        {
            var idHasher = Rock.Utility.IdHasher.Instance;

            var result = idHasher.TryGetId( null, out var id );

            Assert.IsFalse( result );
            Assert.AreEqual( 0, id );
        }
    }
}
