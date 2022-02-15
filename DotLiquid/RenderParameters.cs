using System;
using System.Collections.Generic;

namespace DotLiquid
{
	public class RenderParameters
	{
		/// <summary>
		/// If you provide a Context object, you do not need to set any other parameters.
		/// </summary>
		public Context Context { get; set; }

		public Hash LocalVariables { get; set; }
		public IEnumerable<Type> Filters { get; set; }
		public Hash Registers { get; set; }
        public Dictionary<Type, Func<object, object>> ValueTypeTransformers;

		/// <summary>
		/// Gets or sets a value that controls whether errors are thrown as exceptions.
		/// </summary>
		public bool RethrowErrors { get; set; }

		internal void Evaluate(Template template, out Context context, out Hash registers, out IEnumerable<Type> filters)
		{
			if (Context != null)
			{
				context = Context;
                if ( ValueTypeTransformers != null )
                {
                    context.ValueTypeTransformers = ValueTypeTransformers;
                }

                registers = null;
				filters = null;
				return;
			}

			List<Hash> environments = new List<Hash>();
			if (LocalVariables != null)
				environments.Add(LocalVariables);
			if (template.IsThreadSafe)
            {
                /*
                 * 2020-03-11 - JPH
                 * DotLiquid's Template/Context.Registers (Hash) properties were designed to be a catch-all place to
                 * store any "user-defined, internally-available variables". This Hash is a Dictionary<string, object>
                 * under the hood. We leverage this object to store the collection of "EnabledCommands" for a given
                 * Template. Within the context of a Thread-safe, cached Template, we generally don't want to share
                 * any of these Register entries bewteen Threads, as a given Thread will have unique entries, but the
                 * EnabledCommands entry is an exception to this rule; each time we re-use a cached Template, the
                 * Context needs to be aware of which Rock Commands are enabled for that Template.
                 *
                 * Reason: Issue #4084 (Weird Behavior with Lava Includes)
                 * https://github.com/SparkDevNetwork/Rock/issues/4084
                 * https://github.com/SparkDevNetwork/Rock/issues/4084#issuecomment-597199333
                 * https://github.com/dotliquid/dotliquid/blob/53556cb67cf2d08d66da129a1e5fdfa2cc182534/src/DotLiquid/Context.cs#L51
                 */
                Hash rockRegisters = new Hash();

                var enabledCommandsKey = "EnabledCommands";
                if ( template.Registers != null && template.Registers.ContainsKey( enabledCommandsKey ) )
                {
                    rockRegisters[enabledCommandsKey] = template.Registers[enabledCommandsKey];
                }

                context = new Context(environments, new Hash(), rockRegisters, RethrowErrors);
            }
            else
            {
                environments.Add(template.Assigns);
                context = new Context(environments, template.InstanceAssigns, template.Registers, RethrowErrors);
            }
            context.ValueTypeTransformers = ValueTypeTransformers;
			registers = Registers;
			filters = Filters;
		}

		public static RenderParameters FromContext(Context context)
		{
			return new RenderParameters { Context = context };
		}
	}
}