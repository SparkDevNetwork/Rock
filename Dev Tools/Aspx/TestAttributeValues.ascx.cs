using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Examples
{
    [DisplayName( "Test Attribute Values" )]
    [Category( "Examples" )]
    [Description( "Tests new attribute value code." )]
    [Rock.SystemGuid.BlockTypeGuid( "cad1b43e-4a40-4af7-94a8-c4da1af9b846" )]
    public partial class TestAttributeValues : RockBlock
    {
        private static CancellationTokenSource _cancellationTokenSource;

        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                _cancellationTokenSource?.Cancel();

                etPicker.Visible = ddlMethod.SelectedValueAsInt() == 2;

                using ( var rockContext = new RockContext() )
                {
                    etPicker.EntityTypes = new EntityTypeService( rockContext ).GetEntities().ToList();
                }

                //ProblemDemo();
            }

            base.OnLoad( e );
        }

        public void CompareOneEntityType( TaskActivityProgress progress, CancellationToken cancellationToken )
        {
            var entityType = EntityTypeCache.Get<Group>();

            Action<int, int> updateCount = ( int processed, int total ) =>
            {
                progress.ReportProgressUpdate( processed, total, $"{entityType.Name}: {processed:N0} of {total:N0}" );
            };

            if ( !CompareEntities<ContentChannelItem>( 502, out var count, out var errorMessage, updateCount, cancellationToken ) )
            {
                progress.StopTask( errorMessage, new string[] { errorMessage }, null );
            }
            else
            {
                progress.StopTask( $"{count:N0} records were equal." );
            }
        }

        private void CompareAllEntityTypes( string startAtName, TaskActivityProgress progress, CancellationToken cancellationToken )
        {
            var entityTypes = EntityTypeCache.All()
                .Where( et => typeof( IHasAttributes ).IsAssignableFrom( et.GetEntityType() ) )
                .Where( et => typeof( IEntity ).IsAssignableFrom( et.GetEntityType() ) )
                .Where( et => et.Name != "Rock.Model.Attribute"
                    && et.Name != "Rock.Model.AttributeValue"
                    && et.Name != "Rock.Model.BinaryFileData"
                    && et.Name != "Rock.Model.UserLoginWithPlainTextPassword"
                    && et.Name != "Rock.Model.History"
                    && et.Name != "Rock.Model.IdentityVerificationCode"
                    && et.Name != "Rock.Model.Interaction"
                    && et.Name != "Rock.Model.InteractionSession"
                    && et.Name != "Rock.Model.InteractionSessionLocation"
                    && et.Name != "Rock.Model.ServiceLog"
                    && et.Name != "Rock.Model.WebFarmNodeMetric"
                    && et.Name != "Rock.Rest.Controllers.MetricYTDData" )
                .OrderBy( et => et.Name )
                .ToList();

            if ( startAtName != null )
            {
                entityTypes = entityTypes.SkipWhile( et => et.Name != startAtName ).ToList();
            }

            var messages = new List<string>();
            var method = GetType()
                .GetMethods( System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance )
                .Where( m => m.Name == nameof( CompareEntities ) )
                .Where( m => m.GetParameters().Length == 5 )
                .First();

            for ( int i = 0; i < entityTypes.Count; i++ )
            {
                if ( cancellationToken.IsCancellationRequested )
                {
                    progress.StopTask( "Cancelled" );
                    return;
                }

                var entityType = entityTypes[i];
                Action<int, int> updateCount = ( int processed, int total ) =>
                {
                    progress.ReportProgressUpdate( i, entityTypes.Count, $"{entityType.Name}: {processed:N0} of {total:N0}" );
                };

                progress.ReportProgressUpdate( i, entityTypes.Count, $"{entityType.Name} (loading)" );

                var mi = method.MakeGenericMethod( entityType.GetEntityType() );
                var parameters = new object[] { null, null, null, updateCount, cancellationToken };

                var result = ( bool ) mi.Invoke( this, parameters );

                if ( result )
                {
                    var count = ( int ) parameters[1];
                    messages.Add( $"{entityType.Name}: {count:N0} records were equal." );
                }
                else
                {
                    var errorMessage = ( string ) parameters[2];

                    messages.Add( $"{entityType.Name}: {errorMessage}" );

                    progress.StopTask( $"{entityType.Name}: {errorMessage}", messages );

                    return;
                }
            }

            progress.StopTask( "Completed", null, messages );
        }

        protected bool CompareEntities<TEntity>( int? entityId, out int count, out string errorMessage, Action<int, int> updateCount, CancellationToken cancellationToken )
            where TEntity : class, IHasAttributes, new()
        {
            var lastEntityId = 0;
            var batchSize = 1000;

            count = 0;

            using ( var rockContextA = new RockContext() )
            {
                using ( var rockContextB = new RockContext() )
                {
                    using ( var rockContextC = new RockContext() )
                    {
                        var totalCount = rockContextA.Set<TEntity>().Where( a => !entityId.HasValue || a.Id == entityId ).Count();

                        updateCount( 0, totalCount );

                        while ( true )
                        {
                            // Set A uses the new LoadAttributes() call on the enumerable to load
                            // them all at once.
                            var serviceA = rockContextA.Set<TEntity>();
                            var setA = serviceA.Where( a => !entityId.HasValue || a.Id == entityId )
                                .Where( a => a.Id > lastEntityId )
                                .OrderBy( a => a.Id )
                                .Take( batchSize )
                                .AsNoTracking()
                                .ToList();

                            setA.LoadAttributes( rockContextA );

                            // Set B uses the legacy LoadAttributes() call on each individual entity.
                            var serviceB = rockContextB.Set<TEntity>();
                            var setB = serviceB.Where( a => !entityId.HasValue || a.Id == entityId )
                                .Where( a => a.Id > lastEntityId )
                                .OrderBy( a => a.Id )
                                .Take( batchSize )
                                .AsNoTracking()
                                .ToList();

                            // Set C will use the new SQL Attribute Value Views.
                            var serviceC = rockContextC.Set<TEntity>();
                            var setC = serviceC.Where( a => !entityId.HasValue || a.Id == entityId )
                                .Where( a => a.Id > lastEntityId )
                                .OrderBy( a => a.Id )
                                .Take( batchSize )
                                .AsNoTracking()
                                .ToList();

                            if ( setA.Count == 0 && setB.Count == 0 && setC.Count == 0 )
                            {
                                break;
                            }

                            if ( !CompareEntitySets( setA, setB, setC, rockContextB, rockContextC, out errorMessage, cancellationToken ) )
                            {
                                count = 0;
                                return false;
                            }

                            count += setA.Count;
                            lastEntityId = setA.Last().Id;

                            updateCount( count, totalCount );
                        }
                    }
                }
            }

            errorMessage = null;
            return true;
        }

        private bool CompareEntitySets<TEntity>( List<TEntity> setA, List<TEntity> setB, List<TEntity> setC, RockContext rockContextB, RockContext rockContextC, out string errorMessage, CancellationToken cancellationToken )
            where TEntity : IHasAttributes
        {
            if ( setA.Count != setB.Count || setA.Count != setC.Count )
            {
                errorMessage = "Entity set count mismatch.";
                return false;
            }

            for ( int i = 0; i < setA.Count; i++ )
            {
                if ( cancellationToken.IsCancellationRequested )
                {
                    errorMessage = "Cancelled";
                    return false;
                }

                if ( !CompareEntityAttributes<TEntity>( setA[i], setB[i], setC[i], rockContextB, rockContextC, out errorMessage ) )
                {
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        private bool CompareEntityAttributes<TEntity>( IHasAttributes entityA, IHasAttributes entityB, IHasAttributes entityC, RockContext rockContextB, RockContext rockContextC, out string errorMessage )
        {
            // Skip TPT instances that don't match the queried type.
            if ( entityA.GetType().BaseType != typeof( TEntity ) || entityB.GetType().BaseType != typeof( TEntity ) )
            {
                errorMessage = null;
                return true;
            }

            if ( entityA.Id != entityB.Id || entityA.Id != entityC.Id )
            {
                errorMessage = $"Identifier mismatch: '{entityA}:{entityA.Id}', '{entityB}:{entityB.Id}', '{entityC}:{entityC.Id}'.";
                return false;
            }

            entityB.LoadAttributes( rockContextB );

            var entityCAttributes = Helper.LoadAttributes( typeof( TEntity ), entityC.Id, rockContextC );

            if ( entityA.Attributes.Count != entityB.Attributes.Count || entityA.Attributes.Count != entityCAttributes.Attributes.Count )
            {
                errorMessage = $"Attribute count mismatch in '{entityA}:{entityA.Id}' ({entityA.Attributes.Count}, {entityB.Attributes.Count}, {entityCAttributes.Attributes.Count}).";
                return false;
            }

            var keysA = entityA.Attributes.Keys.OrderBy( k => k ).ToList();
            var keysB = entityB.Attributes.Keys.OrderBy( k => k ).ToList();
            var keysC = entityCAttributes.Attributes.Keys.OrderBy( k => k ).ToList();

            for ( int i = 0; i < keysA.Count; i++ )
            {
                if ( keysA[i] != keysB[i] || keysA[i] != keysC[i] )
                {
                    errorMessage = $"Attribute index {i} mismatched keys: '{keysA[i]}', '{keysB[i]}', '{keysC[i]}' in '{entityA}:{entityA.Id}'.";
                    return false;
                }

                var key = keysA[i];
                var valueA = entityA.GetAttributeValue( key );
                var valueB = entityB.GetAttributeValue( key );
                var valueC = entityCAttributes.GetAttributeValue( key );

                if ( valueA.IsNullOrWhiteSpace() && valueB.IsNullOrWhiteSpace() && valueC.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                if ( valueA != valueB || valueA != valueB )
                {
                    errorMessage = $"Attribute '{key}' value mismatch: {QuotedValue( valueA )}, {QuotedValue( valueB )}, {QuotedValue( valueC )} in '{entityA}:{entityA.Id}'.";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        private static string QuotedValue( string value )
        {
            return value == null ? "null" : $"'{value}'";
        }

        private static void ProblemDemo()
        {
            using ( var rockContext = new RockContext() )
            {
                var entityId = 257678;

                var expectedToBeGroup = new GroupService( rockContext ).Get( entityId );
                var expectedToBeGroupType = expectedToBeGroup.GetType().FullName;
                var expectedToBeGroupBaseType = expectedToBeGroup.GetType().BaseType.FullName;
                // expectedToBeGroupBaseType = "Rock.Model.LearningClass"

                var groupEntityTypeId = new Group().TypeId;
                var learningClassEntityTypeId = new LearningClass().TypeId;
                // groupEntityTypeId = 16
                // learningClassEntityTypeId = 16

                var groupTypeName = new Group().TypeName;
                var learningClassTypeName = new LearningClass().TypeName;
                // groupTypeName = "Rock.Model.Group"
                // learningClassTypeName = "Rock.Model.Group"

                var learningClassEntityTypeName = EntityTypeCache.Get( new LearningClass().TypeId ).Name;
                var learningClassEntityTypeNameTwo = EntityTypeCache.Get( new LearningClass().GetType() ).Name;
                // learningClassEntityTypeName = "Rock.Model.Group"
                // learningClassEntityTypeNameTwo = "Rock.Model.LearningClass"

                var group = new Group
                {
                    Id = expectedToBeGroup.Id,
                    GroupTypeId = expectedToBeGroup.GroupTypeId
                };

                group.LoadAttributes( rockContext );
                expectedToBeGroup.LoadAttributes( rockContext );

                var groupAttributeCount = group.Attributes.Count;
                var expectedToBeGroupAttributeCount = expectedToBeGroup.Attributes.Count;
                // groupAttributeCount = 0
                // expectedToBeGroupAttributeCount = 1


                var groups = new GroupService( rockContext ).Queryable().Take( 100 ).ToList();
                // groups[0-49] = typeof Group
                // groups[50] = typeof LearningClass
                // groups[51-99] = typeof Group

                groups.LoadAttributes( rockContext );

                // groups[50] will have wrong attribute values loaded.
            }
        }

        protected void btnRun_Click( object sender, EventArgs e )
        {
            ltMessage.Text = "";

            if ( tapReporter.ConnectionId.IsNullOrWhiteSpace() )
            {
                ltMessage.Text = "Real-time not connected.";
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            var progress = new TaskActivityProgress( RealTimeHelper.GetTopicContext<ITaskActivityProgress>().Clients.Client( tapReporter.ConnectionId ) )
            {
                StartNotificationDelayMilliseconds = 0,
                NotificationIntervalMilliseconds = 250
            };
            tapReporter.TaskId = progress.TaskId;

            if ( ddlMethod.SelectedValueAsInt() == 1 )
            {
                // Define a background task for the bulk update process, because it may take considerable time.
                Task.Run( async () =>
                {
                    // Wait for the browser to finish loading.
                    await Task.Delay( 1000 );

                    progress.StartTask();
                    try
                    {
                        CompareOneEntityType( progress, _cancellationTokenSource.Token );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex );
                        progress.StopTask( ex.Message, new string[] { ex.Message } );
                    }
                } );
            }
            else
            {
                // Define a background task for the bulk update process, because it may take considerable time.
                Task.Run( async () =>
                {
                    // Wait for the browser to finish loading.
                    await Task.Delay( 1000 );

                    progress.StartTask();
                    try
                    {
                        string startingEntityType = null;

                        if ( etPicker.SelectedValueAsInt().HasValue )
                        {
                            startingEntityType = EntityTypeCache.Get( etPicker.SelectedValueAsInt().Value ).Name;
                        }

                        CompareAllEntityTypes( startingEntityType, progress, _cancellationTokenSource.Token );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex );
                        progress.StopTask( ex.Message, new string[] { ex.Message } );
                    }
                } );
            }
        }

        protected void ddlMethod_SelectedIndexChanged( object sender, EventArgs e )
        {
            etPicker.Visible = ddlMethod.SelectedValueAsInt() == 2;
        }
    }
}
