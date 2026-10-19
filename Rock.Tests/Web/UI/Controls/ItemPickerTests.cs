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
using System.IO;
using System.Web.UI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Web.UI.Controls;

namespace Rock.Tests.Web.UI.Controls
{
    [TestClass]
    public class ItemPickerTests
    {
        [TestMethod]
        public void RenderBaseControl_Enabled_EncodesItemName()
        {
            var picker = new TestItemPicker
            {
                ID = "picker",
                ItemName = "<b>X1</b>"
            };

            var html = RenderBaseControl( picker );

            Assert.IsTrue( html.Contains( "class='selected-names'>&lt;b&gt;X1&lt;/b&gt;</span>" ), html );
            Assert.IsFalse( html.Contains( "<b>X1</b>" ), html );
        }

        [TestMethod]
        public void RenderBaseControl_ReadOnly_EncodesItemName()
        {
            var picker = new TestItemPicker
            {
                ID = "picker",
                Enabled = false,
                ItemName = "<b>X1</b>"
            };

            var html = RenderBaseControl( picker );

            Assert.IsTrue( html.Contains( "<span>&lt;b&gt;X1&lt;/b&gt;</span>" ), html );
            Assert.IsFalse( html.Contains( "<b>X1</b>" ), html );
        }

        [TestMethod]
        public void RenderBaseControl_Ampersand_SingleEncoded()
        {
            var picker = new TestItemPicker
            {
                ID = "picker",
                ItemName = "Youth & Family"
            };

            var html = RenderBaseControl( picker );

            Assert.IsTrue( html.Contains( "class='selected-names'>Youth &amp; Family</span>" ), html );
            Assert.IsFalse( html.Contains( "&amp;amp;" ), html );
        }

        [TestMethod]
        public void RenderBaseControl_IconCssClass_StaysMarkup()
        {
            var enabledPicker = new TestItemPicker
            {
                ID = "picker",
                IconCssClass = "fa fa-users",
                ItemName = "Youth"
            };

            var readOnlyPicker = new TestItemPicker
            {
                ID = "picker",
                Enabled = false,
                IconCssClass = "fa fa-users",
                ItemName = "Youth"
            };

            Assert.IsTrue( RenderBaseControl( enabledPicker ).Contains( "<i class='fa fa-users" ) );
            Assert.IsTrue( RenderBaseControl( readOnlyPicker ).Contains( "<i class='fa fa-users" ) );
        }

        private static string RenderBaseControl( ItemPicker picker )
        {
            using ( var stringWriter = new StringWriter() )
            using ( var writer = new HtmlTextWriter( stringWriter ) )
            {
                picker.RenderBaseControl( writer );
                writer.Flush();

                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// A minimal item picker that renders without a page or script manager.
        /// </summary>
        private class TestItemPicker : ItemPicker
        {
            public override string ItemRestUrl => "~/api/test";

            protected override void RegisterJavaScript()
            {
            }

            protected override void SetValueOnSelect()
            {
            }

            protected override void SetValuesOnSelect()
            {
            }
        }
    }
}
