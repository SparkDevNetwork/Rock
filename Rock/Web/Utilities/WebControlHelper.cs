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
        public static TControl GetByName<TControl>( this ControlCollection controls, string controlName )
            where TControl : System.Web.UI.Control
        {
            var controlsList = new List<Control>();

            foreach ( var c in controls )
            {
                controlsList.Add( c as Control );
            }

            return GetByName<TControl>( controlsList.ToArray(), controlName );
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
        public static TControl TryGetByName<TControl>( this ControlCollection controls, string controlName )
            where TControl : System.Web.UI.Control
        {
            try
            {
                return GetByName<TControl>( controls, controlName );
            }
            catch
            {
                // Ignore errors.
                return null;
            }
        }

        /// <summary>
        ///     Retrieves a control from the specified collection by Name.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="controlName">
        ///     The name of the control which distinguishes it from other controls in the supplied
        ///     collection. The parent name prefix is not required if the controls in the collection have the same parent.
        /// </param>
        /// <returns></returns>
        public static Control TryGetByName( this ControlCollection controls, string controlName )
        {
            return GetByName<Control>( controls, controlName );
        }

        /// <summary>
        /// Specifies the depth of a search in a hierarchy of container controls.
        /// </summary>
        public enum SearchDepthSpecifier
        {
            /// <summary>
            /// Include child controls
            /// </summary>
            IncludeChildControls = 0,

            /// <summary>
            /// Exclude child controls
            /// </summary>
            ExcludeChildControls = 1
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
        /// <param name="searchDepth">The depth of search to perform.</param>
        /// <returns></returns>
        public static TControl GetByName<TControl>( this IEnumerable<Control> controls, string controlName, SearchDepthSpecifier searchDepth = SearchDepthSpecifier.IncludeChildControls )
            where TControl : System.Web.UI.Control
        {
            List<Control> matchingControls;

            if ( searchDepth == SearchDepthSpecifier.IncludeChildControls )
            {
                matchingControls = new List<Control>();

                GetByNameRecursive<TControl>( controls, controlName, matchingControls );
            }
            else
            {
                matchingControls = controls.ToList();
            }

            matchingControls = matchingControls.Where( x => x.ID.Equals( controlName ) || x.ID.EndsWith( "_" + controlName ) ).ToList();

            if ( matchingControls.Count == 0 )
            {
                throw new Exception( string.Format( "Control Name \"{0}\" could not be found.", controlName ) );
            }
            if ( matchingControls.Count > 1 )
            {
                throw new Exception( string.Format( "Control Name \"{0}\" is not unique in this collection.", controlName ) );
            }

            object control = matchingControls.First() as TControl;

            if ( control == null )
            {
                throw new Exception( string.Format( "Control Name \"{0}\" does not match specified Type \"{1}\".", controlName, typeof( TControl ).Name ) );
            }

            return ( TControl ) control;
        }

        /// <summary>
        /// Retrieves a control from the specified collection by Name.
        /// </summary>
        /// <typeparam name="TControl">The type of the t control.</typeparam>
        /// <param name="controls">The controls.</param>
        /// <param name="controlName">The name of the control which distinguishes it from other controls in the supplied
        /// collection. The parent name prefix is not required if the controls in the collection have the same parent.</param>
        /// <param name="searchDepth">The search depth.</param>
        /// <returns>TControl.</returns>
        public static TControl TryGetByName<TControl>( this IEnumerable<Control> controls, string controlName, SearchDepthSpecifier searchDepth = SearchDepthSpecifier.IncludeChildControls )
            where TControl : System.Web.UI.Control
        {
            try
            {
                return GetByName<TControl>( controls, controlName, searchDepth );
            }
            catch
            {
                // Ignore errors.
                return null;
            }
        }

        /// <summary>
        /// Gets all controls in a container having the specified Id.
        /// </summary>
        /// <param name="controls">The control collection.</param>
        /// <param name="id">The id of the control to find.</param>
        /// <param name="resultCollection">The result collection.</param>
        private static void GetByNameRecursive<TControl>( IEnumerable<System.Web.UI.Control> controls, string id, List<System.Web.UI.Control> resultCollection )
            where TControl : System.Web.UI.Control
        {
            foreach ( System.Web.UI.Control control in controls )
            {
                if ( control is TControl )
                {
                    if ( control.ID != null
                         && ( control.ID.Equals( id, StringComparison.OrdinalIgnoreCase ) || control.ID.EndsWith( "_" + id ) ) )
                    {
                        resultCollection.Add( control );
                    }
                }
                if ( control.HasControls() )
                {
                    var childControls = control.Controls.Cast<Control>().ToList();

                    GetByNameRecursive<TControl>( childControls, id, resultCollection );
                }
            }
        }

        /// <summary>
        ///     Retrieves a control from the specified collection by Name.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="controlName">
        ///     The name of the control which distinguishes it from other controls in the supplied
        ///     collection. The parent name prefix is not required if the controls in the collection have the same parent.
        /// </param>
        /// <returns></returns>
        public static System.Web.UI.Control GetByName( this ControlCollection controls, string controlName )
        {
            var controlsList = new List<Control>();

            foreach ( var c in controls )
            {
                controlsList.Add( c as Control );
            }

            return GetByName( controlsList.ToArray(), controlName );
        }

        /// <summary>
        ///     Retrieves a control from the specified collection by Name.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="controlName">
        ///     The name of the control which distinguishes it from other controls in the supplied
        ///     collection. The parent name prefix is not required if the controls in the collection have the same parent.
        /// </param>
        /// <param name="searchDepth">The depth of search to perform.</param>
        /// <returns></returns>
        public static System.Web.UI.Control GetByName( this IEnumerable<Control> controls, string controlName, SearchDepthSpecifier searchDepth = SearchDepthSpecifier.IncludeChildControls )
        {
            List<Control> matchingControls;

            if ( searchDepth == SearchDepthSpecifier.IncludeChildControls )
            {
                matchingControls = new List<Control>();

                GetByNameRecursive( controls, controlName, matchingControls );
            }
            else
            {
                matchingControls = controls.ToList();
            }

            matchingControls = matchingControls.Where( x => x.ID.Equals( controlName ) || x.ID.EndsWith( "_" + controlName ) ).ToList();

            if ( matchingControls.Count == 0 )
            {
                throw new Exception( string.Format( "Control Name \"{0}\" could not be found.", controlName ) );
            }
            if ( matchingControls.Count > 1 )
            {
                throw new Exception( string.Format( "Control Name \"{0}\" is not unique in this collection.", controlName ) );
            }

            var control = matchingControls.First() as System.Web.UI.Control;

            return control;
        }

        /// <summary>
        /// Gets all controls in a container having the specified Id.
        /// </summary>
        /// <param name="controls">The control collection.</param>
        /// <param name="id">The id of the control to find.</param>
        /// <param name="resultCollection">The result collection.</param>
        private static void GetByNameRecursive( IEnumerable<System.Web.UI.Control> controls, string id, List<System.Web.UI.Control> resultCollection )
        {
            foreach ( System.Web.UI.Control control in controls )
            {
                if ( control.ID != null
                        && ( control.ID.Equals( id, StringComparison.OrdinalIgnoreCase ) || control.ID.EndsWith( "_" + id ) ) )
                {
                    resultCollection.Add( control );
                }

                if ( control.HasControls() )
                {
                    var childControls = control.Controls.Cast<Control>().ToList();

                    GetByNameRecursive( childControls, id, resultCollection );
                }
            }
        }
    }
}