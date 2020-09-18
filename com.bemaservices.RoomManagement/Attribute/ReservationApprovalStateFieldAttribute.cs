﻿// <copyright>
// Copyright by BEMA Software Services
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
using Rock;
using Rock.Attribute;

namespace com.bemaservices.RoomManagement.Attribute
{
    /// <summary>
    /// Field Attribute to select a connection state.
    /// Stored as ReservationApprovalState enum int value
    /// </summary>
    public class ReservationApprovalStateFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationApprovalStateFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="fieldTypeAssembly">The field type assembly.</param>
        public ReservationApprovalStateFieldAttribute( string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null, string fieldTypeAssembly = "com.bemaservices.RoomManagement" )
            : base( name, description, required, defaultValue, category, order, key, typeof( com.bemaservices.RoomManagement.Field.Types.ReservationApprovalStateFieldType ).FullName, fieldTypeAssembly )
        {
        }
    }
}