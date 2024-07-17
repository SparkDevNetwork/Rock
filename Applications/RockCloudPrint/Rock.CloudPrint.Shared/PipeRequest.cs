namespace Rock.CloudPrint.Shared
{
    internal class PipeRequest
    {
        public int Type { get; set; }
    }

    internal class PipeStatusResponse
    {
        public bool IsConnected { get; set; }
    }
}
