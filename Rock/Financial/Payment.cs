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
//
using System;
using System.Collections.Generic;

using DotLiquid;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a scheduled payment transaction that has been processed
    /// </summary>
    public class Payment: ILiquidizable
    {
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the gateway schedule id.
        /// </summary>
        public string GatewayScheduleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether schedule is still active.
        /// </summary>
        public bool ScheduleActive { get; set; }

        /// <summary>
        /// Gets or sets the additional transaction details.
        /// </summary>
        /// <value>
        /// The additional transaction details.
        /// </value>
        public Dictionary<string, string> AdditionalTransactionDetails { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Payment"/> class.
        /// </summary>
        public Payment()
        {
            AdditionalTransactionDetails = new Dictionary<string, string>();
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            var items = new Dictionary<string, object>();
            items.Add( "Amount", Amount );
            items.Add( "TransactionCode", TransactionCode );
            items.Add( "TransactionDateTime", TransactionDateTime );
            items.Add( "TransactionDay", TransactionDateTime.ToString( "MMdd" ) );
            items.Add( "GatewayScheduleId", GatewayScheduleId );
            items.Add( "ScheduleActive", ScheduleActive );

            foreach( var item in AdditionalTransactionDetails)
            {
                if ( !items.ContainsKey( item.Key ) )
                {
                    items.Add( item.Key, item.Value );
                }
            }

            return items;
        }
    }
}
