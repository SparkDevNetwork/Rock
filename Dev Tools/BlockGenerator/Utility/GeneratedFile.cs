namespace BlockGenerator.Utility
{
    public class GeneratedFile
    {
        public string FileName { get; }

        public string SolutionRelativePath { get; }

        public string Content { get; }

        public GeneratedFile( string filename, string relativeDirectory, string content )
        {
            FileName = filename;
            SolutionRelativePath = $"{relativeDirectory.Trim( '/', '\\' )}\\{filename}";
            Content = content;
        }
    }
}
