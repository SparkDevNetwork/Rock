// <copyright>
// Copyright 2013 by the Spark Development Network
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Linq;
using System.Web.UI;

namespace Rock.Web.Utilities
{
    /// <summary>
    ///     A helper class for working with Web Controls.
    /// </summary>
    public static class WebControlHelper
    {
        /// <summary>
        ///     Creates a unique name for an instance of a child control in a standard format that can be parsed by other methods
        ///     in the WebControlHelper class.
        /// </summary>
        /// <param name="parentControl">The parent control of the control for which a new name is required.</param>
        /// <param name="childControlName">
        ///     A name by which the child control can be referenced that is unique amongst children of
        ///     the same parent.
        /// </param>
        /// <returns></returns>
        public static string GetChildControlInstanceName( this Control parentControl, string childControlName )
        {
            return string.Format( "{0}_{1}", parentControl.ID, childControlName );
        }

        /// <summary>
        ///     Retrieves a control from the specified collection by Name.
        /// </summary>
        /// <typeparam name="TControl"></typeparam>
        /// <param name="controls"></param>
        /// <param name="controlName">
        ///     The name of the control which distinguishes it from other controls in the supplied
        ///     collection. The parent name prefix is not required if the controls in the collection have the same parent.
        /// </param>
        /// <returns></returns>
        public static TControl GetByName<TControl>( this Control[] controls, string controlName )
            where TControl : class
        {
            var matchingControls = controls.Where( x => x.ID.Equals( controlName ) || x.ID.EndsWith( "_" + controlName ) ).ToList();

            if (matchingControls.Count == 0)
            {
                throw new Exception( string.Format( "Control Name \"{0}\" could not be found.", controlName ) );
            }
            if (matchingControls.Count > 1)
            {
                throw new Exception( string.Format( "Control Name \"{0}\" is not unique in this collection.", controlName ) );
            }

            object control = matchingControls.First() as TControl;

            if (control == null)
            {
                throw new Exception( string.Format( "Control Name \"{0}\" does not match specified Type \"{1}\".", controlName, typeof(TControl).Name ) );
            }

            return (TControl)control;
        }
    }
}