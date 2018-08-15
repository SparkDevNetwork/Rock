using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Workflow;

namespace org.newpointe.ExtraActions
{
    /// <summary>
    /// InjectHtml
    /// </summary>
    [ActionCategory( "Extra Actions" )]
    [Description( "Saves a Metric Value." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Add Metric Value" )]

    [MetricCategoriesField( "Metric", "The Metric to add a Value for.", false )]
    [WorkflowAttribute( "Metric Attribute", "A Workflow Attribute that has the Metric to add a Value for. <span class='tip tip-lava'></span>", false, "", "", 1 )]

    [CustomDropdownListField( "Value Type", "The type of Value to add.", "Measure,Goal", false, "", "", 2 )]
    [WorkflowAttribute( "Value Type Attribute", "A Workflow Attribute that has the type of Value to add. <span class='tip tip-lava'></span>", false, "", "", 3 )]

    [DateField( "Value Date", "The Date to add the Value for.", false, "", "", 4 )]
    [WorkflowAttribute( "Value Date Attribute", "A Workflow Attribute that has the Date to add the Value for. <span class='tip tip-lava'></span>", false, "", "", 5 )]

    [MemoField( "Partition Config", "A list of partition types and values. One per line, in `Partition Label: Entity Id` format. <span class='tip tip-lava'></span>", false, "Campus: 1\nService: 42", "", 6 )]

    [WorkflowTextOrAttribute( "Metric Value", "Metric Value Attribute", "The value to save in the metric. <span class='tip tip-lava'></span>", true, "", "", 7, "MetricValue" )]

    [MemoField( "Value Note", "The Note to include with the added Value. <span class='tip tip-lava'></span>", false, "", "", 8 )]
    [WorkflowAttribute( "Value Note Attribute", "A Workflow Attribute that has the Note to include with the added Value. <span class='tip tip-lava'></span>", false, "", "", 9 )]
    public class SaveMetricValue : ActionComponent
    {

        private static T? EnumConverter<T>( string val, Dictionary<string, object> mergeFields ) where T : struct
        {
            return Enum.TryParse( val.ResolveMergeFields( mergeFields ), true, out T enumOut ) ? enumOut : (T?)null;
        }

        private static string StringConverter( string val, Dictionary<string, object> mergeFields )
        {
            return string.IsNullOrWhiteSpace( val ) ? null : val.ResolveMergeFields( mergeFields );
        }

        private static DateTime? DateConverter( string val, Dictionary<string, object> mergeFields )
        {
            return val.ResolveMergeFields( mergeFields ).AsDateTime();
        }

        private class MetricListConverterData
        {
            public Dictionary<string, object> MergeFields { get; set; }
            public MetricService MetricService { get; set; }
        }

        private static List<Metric> MetricListConverter( string val, MetricListConverterData data )
        {
            List<Metric> mList = data.MetricService.GetByGuids( MetricCategoriesFieldAttribute.GetValueAsGuidPairs( val.ResolveMergeFields( data.MergeFields ) ).Select( g => g.MetricGuid ).ToList() ).ToList();
            return mList.Any() ? mList : null;
        }


        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            Dictionary<string, object> mergeFields = GetMergeFields( action );

            List<Metric> metricList = GetFromAttribute( action, "Metric", "MetricAttribute", MetricListConverter, new MetricListConverterData { MergeFields = mergeFields, MetricService = new MetricService( rockContext ) } );
            MetricValueType? metricValueType = GetFromAttribute( action, "ValueType", "ValueTypeAttribute", EnumConverter<MetricValueType>, mergeFields );
            DateTime? metricValueDate = GetFromAttribute( action, "ValueDate", "ValueDateAttribute", DateConverter, mergeFields );
            string metricValueNote = GetFromAttribute( action, "ValueNote", "ValueNoteAttribute", StringConverter, mergeFields );

            var partitionsConfig = GetAttributeValue( action, "PartitionConfig" )
                .ResolveMergeFields( mergeFields )
                .Split( new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( s => s.Split( ':' ) )
                .Where( s => s.Length == 2 )
                .Select( c => new
                {
                    Label = c[0],
                    Value = c[1].AsIntegerOrNull()
                } )
                .Where( c => c.Value != null )
                .ToList();

            string mValStr = GetAttributeValue( action, "MetricValue" );
            if ( Guid.TryParse( mValStr, out Guid mValGuid ) )
            {
                mValStr = action.GetWorklowAttributeValue( mValGuid );
            }
            decimal? metricValueValue = mValStr.ResolveMergeFields( mergeFields ).AsDecimalOrNull();


            if ( metricList == null )
                errorMessages.Add( "No Metric(s) selected." );

            if ( metricValueType == null )
                errorMessages.Add( "No Value Type selected." );

            if ( metricValueDate == null )
                errorMessages.Add( "No Value Date selected." );

            if ( metricValueValue == null )
                errorMessages.Add( "No Value set." );

            if ( metricList == null || metricValueType == null || metricValueDate == null || metricValueValue == null || errorMessages.Any() )
            {
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }


            new MetricValueService( rockContext ).AddRange( metricList.Select( m =>
                    new MetricValue
                    {
                        Metric = m,
                        MetricValueDateTime = metricValueDate,
                        MetricValueType = metricValueType.Value,
                        Note = metricValueNote,
                        YValue = metricValueValue,
                        MetricValuePartitions = partitionsConfig
                            .Select( p => new MetricValuePartition
                            {
                                EntityId = p.Value,
                                MetricPartition = m.MetricPartitions.FirstOrDefault( mp => mp.Label == p.Label )
                            } )
                            .Where( mvp => mvp.MetricPartition != null ).ToList()
                    }

            ) );

            rockContext.SaveChanges();

            return true;
        }

        public T GetFromAttribute<T, T2>( WorkflowAction action, string attributeKey, string attributeAttributeKey, Func<string, T2, T> valueConverter, T2 valueConverterData )
        {
            // Try getting from Workflow Action's Attribute
            var attributeStr = GetAttributeValue( action, attributeKey );
            if ( !string.IsNullOrWhiteSpace( attributeStr ) )
            {
                var attributeVal = valueConverter( attributeStr, valueConverterData );
                if ( attributeVal != null )
                {
                    return attributeVal;
                }
            }

            // Fall back to Workflow Action's Workflow Attribute
            return valueConverter( action.GetWorklowAttributeValue( GetAttributeValue( action, attributeAttributeKey ).AsGuid() ), valueConverterData );
        }
    }
}
