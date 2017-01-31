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
using System.Dynamic;
using React;

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
        public class DynamicProps : DynamicObject
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            // If you try to get a value of a property 
            // not defined in the class, this method is called.
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                // Converting the property name to lowercase
                // so that property names become case-insensitive.
                string name = binder.Name.ToLower();

                // If the property name is found in a dictionary,
                // set the result parameter to the property value and return true.
                // Otherwise, return false.
                return dictionary.TryGetValue(name, out result);
            }

            // If you try to set a value of a property that is
            // not defined in the class, this method is called.
            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                // Converting the property name to lowercase
                // so that property names become case-insensitive.
                dictionary[binder.Name.ToLower()] = value;

                // You can always add a value to a dictionary,
                // so this method always returns true.
                return true;
            }

        }

        protected dynamic Props = new DynamicProps();
        protected string Component = "";

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var name = TemplateSourceDirectory + "/" + BlockName.Replace(" ", string.Empty);

            this.Props.id = "bid_" + BlockId; ;
            this.Props.path = name;
            this.Component = name.Replace("/", ".").Remove(0, 1);
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
        public string Render()
        {
            var env = AssemblyRegistration.Container.Resolve<IReactEnvironment>();
            var reactComponent = env.CreateComponent(this.Component, this.Props);

            return reactComponent.RenderHtml();
        }

        #endregion
    }

}