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
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class LavaIntegrationTestBase
    {
        public static LavaIntegrationTestHelper TestHelper
        {
            get
            {
                return LavaIntegrationTestHelper.CurrentInstance;
            }
        }

        protected bool AssertCurrentEngineIs( LavaEngineTypeSpecifier validEngine )
        {
            return AssertCurrentEngineIs( new LavaEngineTypeSpecifier[] { validEngine } );
        }

        protected bool AssertCurrentEngineIsNot( LavaEngineTypeSpecifier invalidEngine )
        {
            return AssertCurrentEngineIsNot( new LavaEngineTypeSpecifier[] { invalidEngine } );
        }

        protected bool AssertCurrentEngineIs( IEnumerable<LavaEngineTypeSpecifier> validEngines )
        {
            if ( validEngines == null
                 || ( LavaService.CurrentEngineType != null && !validEngines.Contains( LavaService.CurrentEngineType.Value ) ) )
            {
                Debug.Write( $"This test is not applicable for the current Lava Engine \"{ LavaService.CurrentEngineName }\".", "warning" );
                return false;
            }

            return true;
        }

        protected bool AssertCurrentEngineIsNot( IEnumerable<LavaEngineTypeSpecifier> invalidEngines )
        {
            if ( invalidEngines != null
                 && LavaService.CurrentEngineType != null
                 && invalidEngines.Contains( LavaService.CurrentEngineType.Value ) )
            {
                Debug.Write( $"This test is not applicable for the current Lava Engine \"{ LavaService.CurrentEngineName }\".", "warning" );
                return true;
            }

            return false;
        }
    }
}
