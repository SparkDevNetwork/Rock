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
using System.Runtime.Serialization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Enums.Lms;
using Rock.Lms;
using Rock.Model;

namespace Rock.Tests.Lms
{
    /// <summary>
    /// Tests for the <see cref="AssessmentComponent"/> learning activity component.
    /// </summary>
    [TestClass]
    public class AssessmentComponentTests
    {
        private static readonly Guid MultipleChoiceId = new Guid( "1E3D1C0B-3E4F-4B7A-9C11-0A6D0C1A0001" );
        private static readonly Guid ShortAnswerId = new Guid( "1E3D1C0B-3E4F-4B7A-9C11-0A6D0C1A0002" );
        private static readonly Guid SectionId = new Guid( "1E3D1C0B-3E4F-4B7A-9C11-0A6D0C1A0003" );
        private static readonly Guid UnknownId = new Guid( "1E3D1C0B-3E4F-4B7A-9C11-0A6D0C1A0099" );

        private const string Marker = "<img src=data:, onerror=console.log('T140LMS')>";

        #region Helpers

        /// <summary>
        /// Creates the component without running the base constructor, which
        /// needs a database to register the component attributes.
        /// </summary>
        private static AssessmentComponent CreateComponent()
        {
            return ( AssessmentComponent ) FormatterServices.GetUninitializedObject( typeof( AssessmentComponent ) );
        }

        /// <summary>
        /// Gets the configured component data: one multiple choice item
        /// (Red, Blue, Green with Blue correct), one section and one
        /// short answer item weighted 40 percent.
        /// </summary>
        private static Dictionary<string, string> GetComponentData()
        {
            var items = new JArray
            {
                new JObject
                {
                    ["type"] = 0,
                    ["uniqueId"] = MultipleChoiceId,
                    ["order"] = 0,
                    ["question"] = "Favorite color?",
                    ["answers"] = new JArray( "Red", "Blue", "Green" ),
                    ["correctAnswer"] = "Blue"
                },
                new JObject
                {
                    ["type"] = 1,
                    ["uniqueId"] = SectionId,
                    ["order"] = 1,
                    ["title"] = "Section",
                    ["summary"] = "Section summary"
                },
                new JObject
                {
                    ["type"] = 2,
                    ["uniqueId"] = ShortAnswerId,
                    ["order"] = 2,
                    ["question"] = "Explain.",
                    ["questionWeight"] = 40
                }
            };

            return new Dictionary<string, string>
            {
                ["items"] = items.ToString(),
                ["multipleChoiceWeight"] = "60"
            };
        }

        private static LearningClassActivityCompletion GetCompletion()
        {
            return new LearningClassActivityCompletion
            {
                LearningClassActivity = new LearningClassActivity
                {
                    Points = 30
                }
            };
        }

        private static Dictionary<string, string> GetCompletionValues( JArray completedItems )
        {
            return new Dictionary<string, string>
            {
                ["completedItems"] = completedItems.ToString(),
                ["multipleChoiceWeight"] = "100"
            };
        }

        private static List<JObject> GetStoredItems( Dictionary<string, string> completionData )
        {
            return JArray.Parse( completionData["completedItems"] ).Cast<JObject>().ToList();
        }

        private static JObject GetItem( List<JObject> items, Guid uniqueId )
        {
            return items.FirstOrDefault( i => i.GetValue( "uniqueId", StringComparison.OrdinalIgnoreCase )?.ToObject<Guid>() == uniqueId );
        }

        private static string GetString( JObject item, string name )
        {
            return item.GetValue( name, StringComparison.OrdinalIgnoreCase )?.ToObject<string>();
        }

        #endregion

        /// <summary>
        /// A student who changes the question, answers and correct answer in the
        /// posted completion has only the response stored. Everything else comes
        /// from the configured item.
        /// </summary>
        [TestMethod]
        public void GetCompletionData_StudentTampersWithItem_StoresConfiguredItemWithResponse()
        {
            var posted = new JArray
            {
                new JObject
                {
                    ["type"] = 0,
                    ["uniqueId"] = MultipleChoiceId,
                    ["question"] = Marker,
                    ["answers"] = new JArray( "Red", "Blue", Marker ),
                    ["correctAnswer"] = Marker,
                    ["response"] = Marker,
                    ["pointsEarned"] = 30,
                    ["hasBeenGraded"] = true
                },
                new JObject
                {
                    ["type"] = 2,
                    ["uniqueId"] = ShortAnswerId,
                    ["question"] = Marker,
                    ["response"] = "My answer"
                }
            };

            var result = CreateComponent().GetCompletionData( GetCompletion(), GetCompletionValues( posted ), GetComponentData(), PresentedFor.Student, null, null );
            var items = GetStoredItems( result );
            var multipleChoice = GetItem( items, MultipleChoiceId );

            Assert.IsNotNull( multipleChoice );
            Assert.AreEqual( "Favorite color?", GetString( multipleChoice, "question" ) );
            CollectionAssert.AreEqual( new[] { "Red", "Blue", "Green" }, multipleChoice.GetValue( "answers", StringComparison.OrdinalIgnoreCase ).ToObject<string[]>() );
            Assert.AreEqual( "Blue", GetString( multipleChoice, "correctAnswer" ) );
            Assert.AreEqual( Marker, GetString( multipleChoice, "response" ) );
            Assert.IsNull( multipleChoice.GetValue( "pointsEarned", StringComparison.OrdinalIgnoreCase )?.ToObject<decimal?>() );
            Assert.IsNull( multipleChoice.GetValue( "hasBeenGraded", StringComparison.OrdinalIgnoreCase )?.ToObject<bool?>() );

            var shortAnswer = GetItem( items, ShortAnswerId );
            Assert.AreEqual( "Explain.", GetString( shortAnswer, "question" ) );
            Assert.AreEqual( "My answer", GetString( shortAnswer, "response" ) );

            Assert.AreEqual( "60", result["multipleChoiceWeight"] );
        }

        /// <summary>
        /// A student who leaves out the short answer item still has it stored,
        /// so the completion requires grading.
        /// </summary>
        [TestMethod]
        public void GetCompletionData_StudentOmitsShortAnswer_StoresItemAndRequiresGrading()
        {
            var posted = new JArray
            {
                new JObject
                {
                    ["type"] = 0,
                    ["uniqueId"] = MultipleChoiceId,
                    ["correctAnswer"] = "Green",
                    ["response"] = "Green"
                }
            };

            var component = CreateComponent();
            var completion = GetCompletion();
            var componentData = GetComponentData();
            var result = component.GetCompletionData( completion, GetCompletionValues( posted ), componentData, PresentedFor.Student, null, null );
            var items = GetStoredItems( result );
            var shortAnswer = GetItem( items, ShortAnswerId );

            Assert.AreEqual( 3, items.Count );
            Assert.IsNotNull( shortAnswer );
            Assert.IsNull( GetString( shortAnswer, "response" ) );
            Assert.AreEqual( 12m, shortAnswer.GetValue( "pointsPossible", StringComparison.OrdinalIgnoreCase ).ToObject<decimal?>() );
            Assert.AreEqual( "Blue", GetString( GetItem( items, MultipleChoiceId ), "correctAnswer" ) );
            Assert.IsTrue( component.RequiresGrading( completion, result, componentData, null, null ) );
        }

        /// <summary>
        /// Posted items that are not configured are dropped and section items
        /// never keep a posted response.
        /// </summary>
        [TestMethod]
        public void GetCompletionData_StudentPostsUnknownItem_DropsItem()
        {
            var posted = new JArray
            {
                new JObject
                {
                    ["type"] = 0,
                    ["uniqueId"] = UnknownId,
                    ["question"] = Marker,
                    ["response"] = Marker
                },
                new JObject
                {
                    ["type"] = 1,
                    ["uniqueId"] = SectionId,
                    ["summary"] = Marker,
                    ["response"] = Marker
                }
            };

            var result = CreateComponent().GetCompletionData( GetCompletion(), GetCompletionValues( posted ), GetComponentData(), PresentedFor.Student, null, null );
            var items = GetStoredItems( result );

            Assert.IsNull( GetItem( items, UnknownId ) );
            Assert.IsFalse( result["completedItems"].Contains( "T140LMS" ) );
            Assert.AreEqual( "Section summary", GetString( GetItem( items, SectionId ), "summary" ) );
        }

        /// <summary>
        /// A completion without its class activity loaded does not throw and
        /// computes the short answer points from zero activity points.
        /// </summary>
        [TestMethod]
        public void GetCompletionData_MissingClassActivity_DoesNotThrow()
        {
            var result = CreateComponent().GetCompletionData( new LearningClassActivityCompletion(), GetCompletionValues( new JArray() ), GetComponentData(), PresentedFor.Student, null, null );
            var shortAnswer = GetItem( GetStoredItems( result ), ShortAnswerId );

            Assert.AreEqual( 0m, shortAnswer.GetValue( "pointsPossible", StringComparison.OrdinalIgnoreCase ).ToObject<decimal?>() );
        }

        /// <summary>
        /// A configuration without a multiple choice weight stores the default
        /// weight of 100 that the client used, never the posted weight.
        /// </summary>
        [TestMethod]
        public void GetCompletionData_ConfigurationMissingWeight_StoresDefaultWeight()
        {
            var componentData = GetComponentData();
            componentData.Remove( "multipleChoiceWeight" );

            var completionValues = GetCompletionValues( new JArray() );
            completionValues["multipleChoiceWeight"] = "5";

            var result = CreateComponent().GetCompletionData( GetCompletion(), completionValues, componentData, PresentedFor.Student, null, null );

            Assert.AreEqual( "100", result["multipleChoiceWeight"] );
        }
    }
}
