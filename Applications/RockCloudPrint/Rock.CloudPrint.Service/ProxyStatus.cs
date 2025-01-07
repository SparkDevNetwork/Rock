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
namespace Rock.CloudPrint.Service;

class ProxyStatus
{
    private readonly object _lock = new();

    public DateTimeOffset StartedDateTime { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? ConnectedDateTime { get; set; }

    public bool IsConnected { get; private set; }

    public int TotalPrinted { get; private set; }

    public void SetConnected( bool connected )
    {
        IsConnected = connected;

        if ( connected )
        {
            ConnectedDateTime = DateTimeOffset.Now;
        }
        else
        {
            ConnectedDateTime = null;
        }
    }

    public void AddLabel()
    {
        lock ( _lock )
        {
            TotalPrinted += 1;
        }
    }
}
