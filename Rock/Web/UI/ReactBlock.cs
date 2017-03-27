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
using React;
using Rock.Web.Cache;

namespace Rock.Web.UI
{
    /// <summary>
    /// A Block which supports rendering in React
    /// </summary>
    public class ReactBlock : RockBlock
    {
        #region Properties

        /// <summary>
        /// The props to be used for the component
        /// </summary>
        public class Props
        {
            /// <summary>
            /// Gets or sets the path.
            /// </summary>
            /// <value>
            /// The path.
            /// </value>
            public string path { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public string id { get; set; }
        }

        /// <summary>
        /// The component
        /// </summary>
        protected string Component = "";

        /// <summary>
        /// The path
        /// </summary>
        protected string Path = "";

        /// <summary>
        /// The identifier
        /// </summary>
        protected string Id = "";

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            string name = BlockCache.Read(BlockId).BlockType.Path.Replace("~", "").Replace(".ascx", "");
            Id = "bid_" + BlockId;
            Path = name;
            Component = name.Replace("/", ".").Remove(0, 1);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Renders the component and returns the markup
        /// </summary>
        /// <param name="initialProps">The initialProps for the component</param>
        /// <returns></returns>
        public string Render(Props initialProps)
        {
            var env = AssemblyRegistration.Container.Resolve<IReactEnvironment>();

            initialProps.id = Id;
            initialProps.path = Path;
            var reactComponent = env.CreateComponent(this.Component, initialProps);

            return reactComponent.RenderHtml();
        }

        #endregion
    }

}