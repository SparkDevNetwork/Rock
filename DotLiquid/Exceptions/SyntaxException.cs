using System;

namespace DotLiquid.Exceptions
{
	[Serializable]
	public class SyntaxException : LiquidException
	{
        public string LavaStackTrace { get; set; }

        public SyntaxException(string message, params string[] args)
			: base(string.Format(message, args))
		{
		}

        public override string StackTrace => this.LavaStackTrace + Environment.NewLine + base.StackTrace;
    }
}