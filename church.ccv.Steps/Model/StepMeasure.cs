using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Rock;
using Rock.Data;

namespace church.ccv.Steps.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_church_ccv_Steps_StepMeasure" )]
    [DataContract]
    public class StepMeasure : Model<StepMeasure>, IRockEntity
    {
        #region Entity Properties


        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        [DataMember]
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is TBD.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is TBD; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsTbd
        {
            get { return _isTbd; }
            set { _isTbd = value; }
        }
        /// <summary>
        /// The _is TBD
        /// </summary>
        private bool _isTbd = true;
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the step measure values.
        /// </summary>
        /// <value>
        /// The step measure values.
        /// </value>
        [DataMember]
        public virtual ICollection<StepMeasureValue> StepMeasureValues
        {
            get { return _StepMeasureValues ?? (_StepMeasureValues = new Collection<StepMeasureValue>()); }
            set { _StepMeasureValues = value; }
        }
        private ICollection<StepMeasureValue> _StepMeasureValues;

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class StepMeasureConfiguration : EntityTypeConfiguration<StepMeasure>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepMeasureConfiguration"/> class.
        /// </summary>
        public StepMeasureConfiguration()
        {
        }
    }
}
