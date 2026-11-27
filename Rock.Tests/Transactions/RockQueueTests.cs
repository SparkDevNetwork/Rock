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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Transactions;

namespace Rock.Tests.Transactions
{
    [TestClass]
    public class RockQueueTests
    {
        private const string MarkerValue = "transaction-body-marker-41ad";

        [TestInitialize]
        public void TestInitialize()
        {
            RockQueue.Clear();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            RockQueue.Clear();
        }

        [TestMethod]
        public void Drain_WithFailingTransaction_ReportsTypeAndInnerException()
        {
            var errors = new List<Exception>();

            RockQueue.Enqueue( new ThrowingTransaction { Marker = MarkerValue } );
            RockQueue.Drain( e => errors.Add( e ) );

            Assert.AreEqual( 1, errors.Count );
            StringAssert.Contains( errors[0].Message, nameof( ThrowingTransaction ) );
            Assert.IsInstanceOfType( errors[0].InnerException, typeof( InvalidOperationException ) );
        }

        [TestMethod]
        public void Drain_WithFailingTransaction_DoesNotIncludeTransactionContents()
        {
            var errors = new List<Exception>();

            RockQueue.Enqueue( new ThrowingTransaction { Marker = MarkerValue } );
            RockQueue.Drain( e => errors.Add( e ) );

            Assert.AreEqual( 1, errors.Count );
            Assert.IsFalse( errors[0].Message.Contains( MarkerValue ), "The error message should not contain the transaction contents." );
        }

        public class ThrowingTransaction : ITransaction
        {
            public string Marker { get; set; }

            public void Execute()
            {
                throw new InvalidOperationException( "Test failure." );
            }
        }
    }
}
