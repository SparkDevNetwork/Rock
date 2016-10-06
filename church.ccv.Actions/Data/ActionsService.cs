// <copyright>
// Copyright 2016 by Christ's Church of the Valley
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Linq;

namespace church.ccv.Actions.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionsService<T> : Rock.Data.Service<T> where T : Rock.Data.Entity<T>, new()
    {
        public ActionsService( ActionsContext actionsContext )
            : base( actionsContext )
        {
            ActionsContext = actionsContext;
        }
        
        public ActionsContext ActionsContext { get; private set; }
    }
}
