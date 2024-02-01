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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents the root Kml element.
    /// </summary>
    [XmlRoot( "kml", Namespace = "http://www.opengis.net/kml/2.2" )]
    public class Kml
    {
        /// <summary>
        /// Gets or sets data from the ZipCodeBoundaryDocument node.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        [XmlElement( "Document" )]
        public ZipCodeBoundaryDocument Document { get; set; }
    }

    /// <summary>
    /// The ZipCodeBoundaryDocument node.
    /// </summary>
    public class ZipCodeBoundaryDocument
    {
        /// <summary>
        /// Gets or sets data from the Folder node.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        [XmlElement( "Folder" )]
        public Folder Folder { get; set; }
    }

    /// <summary>
    /// The Folder node.
    /// </summary>
    public class Folder
    {
        /// <summary>
        /// Gets or sets data from the Placemarks node.
        /// </summary>
        /// <value>
        /// The placemarks.
        /// </value>
        [XmlElement( "Placemark" )]
        public List<Placemark> Placemarks { get; set; }
    }

    /// <summary>
    /// The Placemarks node.
    /// </summary>
    public class Placemark
    {
        /// <summary>
        /// Gets or sets data from the ExtendedData node.
        /// </summary>
        /// <value>
        /// The extended data.
        /// </value>
        [XmlElement( "ExtendedData" )]
        public ExtendedData ExtendedData { get; set; }

        /// <summary>
        /// Gets or sets data from the MultiGeometry node.
        /// </summary>
        /// <value>
        /// The multi geometry.
        /// </value>
        [XmlElement( "MultiGeometry" )]
        public MultiGeometry MultiGeometry { get; set; }

        /// <summary>
        /// Gets or sets data from the Polygon node.
        /// </summary>
        /// <value>
        /// The polygon.
        /// </value>
        [XmlElement( "Polygon" )]
        public Polygon Polygon { get; set; }
    }

    /// <summary>
    /// The ExtendedData node.
    /// </summary>
    public class ExtendedData
    {
        /// <summary>
        /// Gets or sets data from the SchemaData node.
        /// </summary>
        /// <value>
        /// The schema data.
        /// </value>
        [XmlElement( "SchemaData" )]
        public SchemaData SchemaData { get; set; }
    }

    /// <summary>
    /// The SchemaData node.
    /// </summary>
    public class SchemaData
    {
        /// <summary>
        /// Gets or sets the simple data list, acts as a sort of Key/Value pair with the Name representing the Key
        /// and the value, the value and contains the ZipCode's data such as SquareMiles and State.
        /// </summary>
        /// <value>
        /// The simple data list.
        /// </value>
        [XmlElement( "SimpleData" )]
        public List<SimpleData> SimpleDataList { get; set; }
    }

    /// <summary>
    /// Acts as a sort of Key/Value pair with the Name representing the Key and the value, the value
    /// It contains the ZipCode's data such as SquareMiles and State.
    /// </summary>
    public class SimpleData
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute( "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [XmlText]
        public string Value { get; set; }
    }

    /// <summary>
    /// The Geographical information of the ZipCode
    /// </summary>
    public class MultiGeometry
    {
        /// <summary>
        /// Gets or sets the polygon information used to create the Geofence.
        /// </summary>
        /// <value>
        /// The polygon.
        /// </value>
        [XmlElement( "Polygon" )]
        public Polygon Polygon { get; set; }
    }

    /// <summary>
    /// The polygon information used to create the Geofence.
    /// </summary>
    public class Polygon
    {
        /// <summary>
        /// Gets or sets the outer boundary is.
        /// </summary>
        /// <value>
        /// The outer boundary is.
        /// </value>
        [XmlElement( "outerBoundaryIs" )]
        public OuterBoundaryIs OuterBoundaryIs { get; set; }
    }

    /// <summary>
    /// The outer boundary is.
    /// </summary>
    public class OuterBoundaryIs
    {
        /// <summary>
        /// Gets or sets the linear ring.
        /// </summary>
        /// <value>
        /// The linear ring.
        /// </value>
        [XmlElement( "LinearRing" )]
        public LinearRing LinearRing { get; set; }
    }

    /// <summary>
    /// The linear ring, contains the Longitude and Latitude coordinates
    /// </summary>
    public class LinearRing
    {
        /// <summary>
        /// Gets or sets the Longitude and Latitude coordinates.
        /// </summary>
        /// <value>
        /// The coordinates.
        /// </value>
        [XmlElement( "coordinates" )]
        public string Coordinates { get; set; }
    }
}
