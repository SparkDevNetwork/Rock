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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

using Rock;

/// <summary>
/// MEF Directory Catalog that will handle outdated MEF Components
/// </summary>
public class SafeDirectoryCatalog : ComposablePartCatalog
{
    private readonly List<ComposablePartDefinition> _parts;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeDirectoryCatalog"/> class.
    /// </summary>
    /// <param name="baseType">Type of the base.</param>
    public SafeDirectoryCatalog( Type baseType )
    {
        var assemblies = Reflection.GetPluginAssemblies();

        // Add Rock.dll
        assemblies.Add( typeof( SafeDirectoryCatalog ).Assembly );

        string baseTypeAssemblyName = baseType.Assembly.GetName().Name;

        // The contract this container actually composes against (e.g. Rock.Workflow.ActionComponent).
        // Only exports matching this contract have their metadata read during composition, so this is
        // the only set of parts we need to validate below.
        string targetContractName = AttributedModelServices.GetContractName( baseType );

        _parts = new List<ComposablePartDefinition>();

        foreach ( var assembly in assemblies.ToList() )
        {
            try
            {
                // only attempt to load the catalog if the assembly is or references the basetype assembly
                if ( assembly == baseType.Assembly || assembly.GetReferencedAssemblies().Any( a => a.Name.Equals( baseTypeAssemblyName, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    AssemblyCatalog assemblyCatalog = new AssemblyCatalog( assembly );

                    // Force MEF to load the plugin and figure out if there are any exports.
                    // Good assemblies will not throw the RTLE exception during part discovery.
                    var parts = assemblyCatalog.Parts.ToList();

                    if ( parts.Count > 0 )
                    {
                        AddSafeParts( parts, assembly, targetContractName );
                    }
                }
            }
            catch ( ReflectionTypeLoadException e )
            {
                foreach ( var loaderException in e.LoaderExceptions )
                {
                    Rock.Model.ExceptionLogService.LogException( new Exception( "Unable to load MEF from " + assembly.FullName, loaderException ) );
                }

                string msg = e.Message;
            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( new Exception( $"Unable to load MEF from {assembly.FullName}", ex ) );
            }
        }
    }

    /// <summary>
    /// Adds the parts that can be safely composed, skipping any that throw when the export
    /// metadata this container will actually compose against is evaluated.
    /// </summary>
    /// <param name="parts">The candidate parts discovered in the assembly.</param>
    /// <param name="assembly">The assembly the parts were discovered in (used for logging).</param>
    /// <param name="targetContractName">The export contract this container composes against; only exports with this contract are validated.</param>
    private void AddSafeParts( IEnumerable<ComposablePartDefinition> parts, Assembly assembly, string targetContractName )
    {
        /*
            9/3/26 - DH

            Part discovery (assemblyCatalog.Parts above) does not evaluate a part's export
            metadata; MEF defers that until composition, when it reads the metadata to match
            imports against exports. Evaluating the metadata reflects over the exported member
            and instantiates every custom attribute applied to it. A plugin that was compiled
            against an older version of an attribute (for example a ...FieldAttribute whose
            constructor signature has since changed) throws MissingMethodException at that point.
            The same happens for any type-initialization failure (e.g. a bad static constructor)
            reached through that reflection. Because it happened during composition rather than
            discovery, a single bad plugin component would take down the entire MEF container -
            and with it Rock's startup.

            To isolate the failure, we force the relevant metadata to evaluate now, inside a
            per-part try/catch, and add only the parts that survive. A part that throws is logged
            and skipped, so one broken plugin component no longer prevents every other component
            (core or plugin) from loading.

            We only evaluate exports whose contract matches what this container composes against
            (targetContractName). MEF short-circuits on the contract name before reading metadata,
            so those are the only exports composition would ever touch here. This keeps the check
            perf-neutral - it does exactly the metadata work composition would do, just early
            enough to contain the failure. A component of some other type that is broken is caught
            the same way by its own container.

            Reason: Prevent a single incompatible plugin component from crashing the whole MEF
            container (and Rock startup) during composition, without adding startup overhead.
        */
        foreach ( var part in parts )
        {
            try
            {
                foreach ( var exportDefinition in part.ExportDefinitions )
                {
                    if ( exportDefinition.ContractName == targetContractName )
                    {
                        ForceMetadataEvaluation( exportDefinition.Metadata );
                    }
                }

                _parts.Add( part );
            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( new Exception( $"Unable to load MEF component '{part}' from {assembly.FullName}. The component will be skipped.", ex ) );
            }
        }
    }

    /// <summary>
    /// Enumerates the metadata dictionary so that any values MEF computes lazily (which is what
    /// instantiates the exporting member's custom attributes) are realized now.
    /// </summary>
    /// <param name="metadata">The export metadata to evaluate.</param>
    private static void ForceMetadataEvaluation( IDictionary<string, object> metadata )
    {
        if ( metadata == null )
        {
            return;
        }

        foreach ( var value in metadata.Values )
        {
            // Touching the value is enough to force it to be computed; nothing else is needed.
            _ = value;
        }
    }

    /// <summary>
    /// Gets the part definitions that are contained in the catalog.
    /// </summary>
    /// <returns>The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> contained in the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" />.</returns>
    public override IQueryable<ComposablePartDefinition> Parts
    {
        get { return _parts.AsQueryable(); }
    }
}
