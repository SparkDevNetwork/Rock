#region License
/* 
 * Copyright 2009- Marko Lahma
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not 
 * use this file except in compliance with the License. You may obtain a copy 
 * of the License at 
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0 
 *   
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations 
 * under the License.
 * 
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Security;

using Quartz.Spi;

namespace Quartz.Simpl
{
    /// <summary>
    /// Scheduler exporter that exports scheduler to remoting context.
    /// </summary>
    /// <author>Marko Lahma</author>
    public class RemotingSchedulerExporter : ISchedulerExporter
    {
        public const string ChannelTypeTcp = "tcp";
        public const string ChannelTypeHttp = "http";
        private const string DefaultBindName = "QuartzScheduler";
        private const string DefaultChannelName = "http";

        private int port = -1;
        private string bindName = DefaultBindName;
        private string channelName = DefaultChannelName;
        private string channelType = ChannelTypeTcp;
        private TypeFilterLevel typeFilgerLevel = TypeFilterLevel.Full;
        private static readonly Dictionary<string, object> registeredChannels = new Dictionary<string, object>();

        public RemotingSchedulerExporter()
        {
        }

        public virtual void Bind(IRemotableQuartzScheduler scheduler)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            if (!typeof(MarshalByRefObject).IsAssignableFrom(scheduler.GetType()))
            {
                throw new ArgumentException("Exported scheduler must be of type MarshallByRefObject", "scheduler");
            }

            RegisterRemotingChannelIfNeeded();

            try
            {
                RemotingServices.Marshal((MarshalByRefObject)scheduler, bindName);
            }
            catch (RemotingException)
            {

            }
            catch (SecurityException)
            {

            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Registers remoting channel if needed. This is determined
        /// by checking whether there is a positive value for port.
        /// </summary>
        protected virtual void RegisterRemotingChannelIfNeeded()
        {
            if (port > -1 && channelType != null)
            {
                // try remoting bind

                IDictionary props = new Hashtable();
                props["port"] = port;
                props["name"] = channelName;

                // use binary formatter
                BinaryServerFormatterSinkProvider formatprovider = new BinaryServerFormatterSinkProvider(props, null);
                formatprovider.TypeFilterLevel = typeFilgerLevel;

                string channelRegistrationKey = channelType + "_" + port;
                if (registeredChannels.ContainsKey(channelRegistrationKey))
                {
                    return;
                }
                IChannel chan;
                if (channelType == ChannelTypeHttp)
                {
                    chan = new HttpChannel(props, null, formatprovider);
                }
                else if (channelType == ChannelTypeTcp)
                {
                    chan = new TcpChannel(props, null, formatprovider);
                }
                else
                {
                    throw new ArgumentException("Unknown remoting channel type '" + channelType + "'");
                }

                ChannelServices.RegisterChannel(chan, false);

                registeredChannels.Add(channelRegistrationKey, new object());
            }
        }

        public virtual void UnBind(IRemotableQuartzScheduler scheduler)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            if (!typeof(MarshalByRefObject).IsAssignableFrom(scheduler.GetType()))
            {
                throw new ArgumentException("Exported scheduler must be of type MarshallByRefObject", "scheduler");
            }

            try
            {
                RemotingServices.Disconnect((MarshalByRefObject)scheduler);

            }
            catch (ArgumentException)
            {

            }
            catch (SecurityException)
            {

            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Gets or sets the port used for remoting.
        /// </summary>
        public virtual int Port
        {
            get { return port; }
            set { port = value; }
        }

        /// <summary>
        /// Gets or sets the name to use when exporting
        /// scheduler to remoting context.
        /// </summary>
        public virtual string BindName
        {
            get { return bindName; }
            set { bindName = value; }
        }

        /// <summary>
        /// Gets or sets the name to use when binding to 
        /// tcp channel.
        /// </summary>
        public virtual string ChannelName
        {
            get { return channelName; }
            set { channelName = value; }
        }

        /// <summary>
        /// Sets the channel type when registering remoting.
        /// 
        /// </summary>
        public virtual string ChannelType
        {
            get { return channelType; }
            set { channelType = value; }
        }

        /// <summary>
        /// Sets the <see cref="TypeFilterLevel" /> used when
        /// exporting to remoting context. Defaults to
        /// <see cref="System.Runtime.Serialization.Formatters.TypeFilterLevel.Full" />.
        /// </summary>
        public virtual TypeFilterLevel TypeFilterLevel
        {
            set { typeFilgerLevel = value; }
            get { return typeFilgerLevel; }
        }
    }
}
