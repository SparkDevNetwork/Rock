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

using Docker.DotNet;

using DotNet.Testcontainers.Configurations;

namespace Rock.Tests.Integration.TestFramework.Database
{
    /// <summary>
    /// Creates a Docker client that talks to the same Docker host the test
    /// containers are started on.
    /// </summary>
    static class TestDockerClientFactory
    {
        /// <summary>
        /// Creates the configuration for a Docker client.
        /// </summary>
        /// <returns>
        /// The configuration. It carries the credentials the daemon is reached
        /// with, so the caller disposes it once it is finished with the client it
        /// created; disposing the client alone does not release them.
        /// </returns>
        public static DockerClientConfiguration CreateConfiguration()
        {
            /*
                9/26/26 - CLAUDE

                The test framework reaches Docker two ways. Containers are started
                through Testcontainers, which finds the Docker host by working
                through DOCKER_HOST, the ~/.testcontainers.properties file, and the
                well-known socket locations. Listing and committing images is done
                through Docker.DotNet, whose parameterless configuration ignores all
                of that and always uses the local default - the named pipe on
                Windows, /var/run/docker.sock elsewhere.

                Those two agree only when Docker is in its default local location,
                so any setup that moves it - a remote host, Colima, Rancher Desktop,
                Podman, rootless Docker - has containers starting on one daemon
                while the image is looked for and committed on another.

                Asking Testcontainers which host it settled on keeps them together,
                and brings TLS credentials along with the address. The resolved
                configuration is null when no daemon answered, and in that case the
                local default still applies so the caller reports the same "Docker
                Client is not available" message it always has.

                Reason: Both Docker clients have to address the same daemon.
            */
            var authConfig = TestcontainersSettings.OS.DockerEndpointAuthConfig;

            if ( authConfig?.Endpoint != null )
            {
                return new DockerClientConfiguration( authConfig.Endpoint, authConfig.Credentials );
            }

            // Testcontainers only reports a host it was able to reach, so nothing
            // comes back when the daemon is down. Use DOCKER_HOST anyway if it is
            // set, so that the connection error names the host the developer
            // configured rather than the local default they did not.
            var dockerHost = Environment.GetEnvironmentVariable( "DOCKER_HOST" );

            if ( Uri.TryCreate( dockerHost, UriKind.Absolute, out var dockerHostUri ) )
            {
                return new DockerClientConfiguration( dockerHostUri );
            }

            return new DockerClientConfiguration();
        }
    }
}
