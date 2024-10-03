using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CsvHelper;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Rock.AI.Automations;
using Rock.Data;
using Rock.Enums.AI;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Modules.Core.AI
{
    /// <summary>
    /// These test methods should only be run manually.
    /// They were written to take PrayerRequests and perform chat completions
    /// based on various configurations. The results should then be inspected to
    /// determine if the completion prompts should be adjusted.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.AICompletions )]
    public class AICompletionsUtility
    {
        #region Test methods (to be run manually)

        /// <summary>
        /// Performs an Analyzer Completion request per line in a file.
        /// <see cref="PrayerRequestAutomationTestingFile"/> for expected format.
        /// </summary>
        /// <remarks>
        /// This is used for one-off testing and testing of new analyzer completion templates.
        /// During testing the [Ignore] attribute should be replaced by [TestMethod] so it can be run via the
        /// Test Explorer. Once the testing is complete the [TestMethod] should be replaced by [Ignore]
        /// so the method is not run as part of the normal integration testing process.
        /// </remarks>
        [Ignore]
        public void RunOneTimeAnalyzerCompletions()
        {
            var inFile = @"C:\Temp\analysis_in.csv";
            var outFile = @"C:\Temp\analysis_out.csv";

            var options = new AICompletionsFromFileOptions
            {
                InFile = inFile,
                OutFile = outFile,
                LavaCompletionTemplate = prayerRequestAnalyzerTemplate,
                Model = "gpt-4o-mini",
                Categories = categoriesForSampleFile,
                MaxCompletions = 10
            };

            Rock.Utility.AsyncHelper.RunSync( () => RunOpenAIAnalyzerCompletionsFromFile( options ) );
        }

        /// <summary>
        /// Performs a Formatter Completion request per line in a file.
        /// <see cref="PrayerRequestAutomationTestingFile"/> for expected format.
        /// </summary>
        /// <remarks>
        /// This is used for one-off testing and testing of new text formatter completion templates.
        /// During testing the [Ignore] attribute should be replaced by [TestMethod] so it can be run via the
        /// Test Explorer. Once the testing is complete the [TestMethod] should be replaced by [Ignore]
        /// so the method is not run as part of the normal integration testing process.
        /// </remarks>
        [Ignore]
        public void RunOneTimeFormatterCompletions()
        {
            var inFile = @"C:\Temp\formatter_in.csv";
            var outFile = @"C:\Temp\formatter_out.csv";

            var options = new AICompletionsFromFileOptions
            {
                InFile = inFile,
                OutFile = outFile,
                LavaCompletionTemplate = prayerRequestFormatterTemplate,
                Model = "gpt-4o-mini",
                MaxCompletions = 10
            };

            Rock.Utility.AsyncHelper.RunSync( () => RunOpenAIFormatterCompletionsFromFile( options ) );
        }

        /// <summary>
        /// Converts a file in <see cref="PrayerRequestAutomationTestingFile"/> format
        /// to a .jsonl file for batch processing by OpenAI. At the time of writing
        /// the format of the file does not appear to work when uploading via the GUI
        /// (the API has not been tested). This might be due to no support for gpt-4o-mini model
        /// or it might be due to characters in the request text.
        /// </summary>
        /// <remarks>
        /// This is used for one-off testing and testing of new text formatter completion templates.
        /// During testing the [Ignore] attribute should be replaced by [TestMethod] so it can be run via the
        /// Test Explorer. Once the testing is complete the [TestMethod] should be replaced by [Ignore]
        /// so the method is not run as part of the normal integration testing process.
        /// </remarks>
        [Ignore]
        public void RunBatchConversion()
        {
            var inFile = @"C:\Temp\all_requests_in.csv";
            var outFile = @"C:\Temp\requests_batch.jsonl";
            var template = prayerRequestFormatterTemplate;
            var options = new AICompletionsFromFileOptions
            {
                InFile = inFile,
                OutFile = outFile,
                LavaCompletionTemplate = template,
                Model = "gpt-4o-mini"
            };

            ConvertFileToOpenAIBatchCompletions( options );
        }

        #endregion

        #region Private Members

        /// <summary>
        /// The analyzer template to use for PrayerRequest AI Completions.
        /// </summary>
        /// <remarks>
        /// The Lava Engine returns null so don't use any complex objects - instead hard-code categories and sentiments.
        /// </remarks>
        private static readonly string prayerRequestAnalyzerTemplate = @"
For all tasks refer to the prayer request text delimited by ```Prayer Request```.

Choose the Id of the category that most closely matches the main theme of the prayer request.

```Categories```
[
    { ""Id"": 1, ""CategoryName"" = ""Health & Hospitalization""},
    { ""Id"": 2, ""CategoryName"" = ""God's Will""},
    { ""Id"": 3, ""CategoryName"" = ""Death & Grief""},
    { ""Id"": 4, ""CategoryName"" = ""Family & Marriage""},
    { ""Id"": 5, ""CategoryName"" = ""Other""},
    { ""Id"": 6, ""CategoryName"" = ""Addiction""},
    { ""Id"": 7, ""CategoryName"" = ""Finances/Job""},
    { ""Id"": 8, ""CategoryName"" = ""Depression/Anxiety/Suicide""},
    { ""Id"": 9, ""CategoryName"" = ""Salvation/Rededication""},
    { ""Id"": 10, ""CategoryName"" = ""Praise Report""}
]
```Categories```

Choose the Id of the sentiment that most closely matches the prayer request text.

```Sentiments```
[
  {
    ""Id"": 1286,
    ""Sentiment"": ""Anger""
  },
  {
    ""Id"": 1287,
    ""Sentiment"": ""Anticipation""
  },
  {
    ""Id"": 1288,
    ""Sentiment"": ""Disgust""
  },
  {
    ""Id"": 1289,
    ""Sentiment"": ""Fear""
  },
  {
    ""Id"": 1290,
    ""Sentiment"": ""Joy""
  },
  {
    ""Id"": 1291,
    ""Sentiment"": ""Neutral""
  },
  {
    ""Id"": 1292,
    ""Sentiment"": ""Sadness""
  },
  {
    ""Id"": 1293,
    ""Sentiment"": ""Worry""
  }
]
```Sentiments```

Determine if the prayer request text is appropriate for public viewing being sensitive to privacy and legal concerns.
First names alone are ok, but pay attention to other details which might make it easy to uniquely identify an individual within a community.

```Prayer Request```
{{PrayerRequest.Text}}
```Prayer Request```

Respond with ONLY a VALID JSON object in the format below. Do not use backticks ```.
{
    ""sentimentId"": <The Id of the Sentiment from the list delimited by ```Sentiments``` that most closely matches the main theme of the prayer request text>,
    ""categoryId"": <The Id of the Category from the list delimited by ```Categories``` that most closely matches the main theme of the prayer request text>,
    ""isAppropriateForPublic"": <boolean value indicating whether the prayer request text is appropriate for public viewing>
}
";

        /// <summary>
        /// The (text) formatter template to use for PrayerRequest AI Completions.
        /// </summary>
        private static readonly string prayerRequestFormatterTemplate = @"
{%- comment -%}
    This is the lava template for the Text formatting AI automation that occurs in the PrayerRequest PostSave SaveHook.
    Available Lava Fields:

    PrayerRequest - The PrayerRequest entity object.
    EnableFixFormattingAndSpelling - True if the AI Automation AttributeValue for TextEnhancement is equal to MinorFormattingAndSpelling; otherwise false.
    EnableEnhancedReadability - True if the AI Automation AttributeValue for TextEnhancement is equal to EnhanceReadability; otherwise false.    
    EnableRemovalLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to LastNamesOnly; otherwise false.
    EnableRemovalFirstAndLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to FirstAndLastNames; otherwise false.
{%- endcomment -%}
Refer to the Prayer Request below, delimited by ```Prayer Request```. Return only the modified text without any additional comments.
{%- if EnableRemovalLastNames == true and EnableRemovalFirstAndLastNames == false %}
Remove surname and family names, but leave first names in their original form from the text below.
{%- endif -%}
{%- if EnableRemovalFirstAndLastNames == true %}
Remove names, both first and last, from the text below. If the text uses a pronoun or possessive pronoun continue to use that; otherwise use generic words like: ""an individual"", ""some individuals"", ""a family"" etc.
{%- endif -%}
{%- if EnableFixFormattingAndSpelling == true and EnableEnhancedReadability == false %}
Fix any formatting and spelling mistakes, but do not change the text.
{% endif -%}
{%- if EnableEnhancedReadability == true %}
Make the request more readable and polished. Do not change words if they significantly alter the perceived meaning.
If the request is not in English and a translation is included - leave the translation in it's original form.
{% endif -%}

```Prayer Request```
{{ PrayerRequest.Text }}
```Prayer Request```
";

        /// <summary>
        /// The list of Categories whose Parent Category is "All Church" by default.
        /// </summary>
        private static readonly List<Category> defaultAllChurchChildCategories = new List<Category> {
                new Category {  Id = 2, Name = "General"},
                new Category {  Id = 109, Name = "Addictive Behavior"},
                new Category {  Id = 110, Name = "Comfort/Grief"},
                new Category {  Id = 111, Name = "Current Events"},
                new Category {  Id = 112, Name = "Depression/Anxiety"},
                new Category {  Id = 113, Name = "Family Issues"},
                new Category {  Id = 114, Name = "Finances/Job"},
                new Category {  Id = 115, Name = "Global Outreach"},
                new Category {  Id = 116, Name = "God's Will"},
                new Category {  Id = 117, Name = "Health Issues"},
                new Category {  Id = 118, Name = "Hospitalization"},
                new Category {  Id = 119, Name = "Life Transitions"},
                new Category {  Id = 120, Name = "Marriage/Relationship Problems"},
                new Category {  Id = 121, Name = "Nation"},
                new Category {  Id = 122, Name = "Persecuted Church"},
                new Category {  Id = 124, Name = "Salvation/Rededication"},
                new Category {  Id = 125, Name = "Spiritual Warfare"},
                new Category {  Id = 126, Name = "Suicidal Tendencies"},
                new Category {  Id = 129, Name = "Travel"},
            };

        private static readonly List<Category> categoriesForSampleFile = new List<Category>
        {
            new Category { Id = 1, Name = "Health & Hospitalization" },
            new Category { Id = 2, Name = "God's Will" },
            new Category { Id = 3, Name = "Death & Grief" },
            new Category { Id = 4, Name = "Family & Marriage" },
            new Category { Id = 5, Name = "Other" },
            new Category { Id = 6, Name = "Addiction" },
            new Category { Id = 7, Name = "Finances/Job" },
            new Category { Id = 8, Name = "Depression/Anxiety/Suicide" },
            new Category { Id = 9, Name = "Salvation/Rededication" },
            new Category { Id = 10, Name = "Praise Report" }
        };

        /// <summary>
        /// The list of Sentiments to validate against.
        /// </summary>
        private static readonly List<DefinedValue> sentiments = new List<DefinedValue>
        {
            new DefinedValue {
                    Id = 1286,
                    Value =  "Anger"
                  },
                  new DefinedValue {
                    Id = 1287,
                    Value =  "Anticipation"
                  },
                  new DefinedValue {
                    Id = 1288,
                    Value =  "Disgust"
                  },
                  new DefinedValue {
                    Id = 1289,
                    Value =  "Fear"
                  },
                  new DefinedValue {
                    Id = 1290,
                    Value =  "Joy"
                  },
                  new DefinedValue {
                    Id = 1291,
                    Value =  "Neutral"
                  },
                  new DefinedValue {
                    Id = 1292,
                    Value =  "Sadness"
                  },
                  new DefinedValue {
                    Id = 1293,
                    Value =  "Worry"
                  }
        };

        /// <summary>
        /// Converts the specified <paramref name="inFile"/> to a jsonl file for batch processing by the OpenAI API.
        /// Uses the specified model (defaults to 'gpt-4o-mini').
        /// Each method call sets the values below based on the file contents:
        ///     PrayerRequest.Text to the <see cref="PrayerRequestAutomationTestingFile.Text"/> property value
        ///     aiAutomation.NameRemoval to the <see cref="PrayerRequestAutomationTestingFile.NameRemoval"/> property value
        ///     aiAutomation.TextEnhancement to the <see cref="PrayerRequestAutomationTestingFile.TextEnhancement"/> property value
        ///     CustomId of the batch will correspond to the RowNumber in the file.
        /// </summary>
        /// <param name="inFile">The full path to the file to read in.</param>
        /// <param name="outFile">The full path to the file to create containing .jsonl formatted file.</param>
        /// <param name="template">The lava template to use to generate the completion.</param>
        /// <param name="model">The OpenAI Model name to use to run the completions.</param>
        /// <returns>a string status message.</returns>
        private string ConvertFileToOpenAIBatchCompletions( AICompletionsFromFileOptions options )
        {
            if ( !File.Exists( options.InFile ) )
            {
                return "File does not exist or is inaccessible.";
            }

            if ( File.Exists( options.OutFile ) && new FileInfo( options.OutFile ).Length > 0 && !options.OverwriteOutFile )
            {
                // Don't overwrite files - if we've already run this
                // and there are values in the current file we don't want to lose those.
                // Let the caller do that so we can be sure the delete is intentional.
                return "Won't overwrite an existing file. Please rename the current file or delete it if it's not wanted.";
            }

            const string openAiProviderGuid = "2AA26B14-94CB-4A30-9E97-C7250BA464BB";
            var aiProvider = new AIProviderService( new RockContext() ).Get( openAiProviderGuid.AsGuid() );

            var aiConfig = new AIAutomation
            {
                AIModel = options.Model,
                AIProvider = aiProvider
            };

            var testEntity = new PrayerRequest();

            // Get a CSV reader and writer.
            // Auto-detect the encoding in the reader and use the same encoding for the writer.
            using ( StreamReader reader = new StreamReader( options.InFile, true ) )
            using ( StreamWriter writer = new StreamWriter( options.OutFile, false, reader.CurrentEncoding ) )
            using ( var csv = new CsvReader( reader ) )
            {
                while ( csv.Read() )
                {
                    var row = csv.GetRecord<PrayerRequestAutomationTestingFile>();
                    testEntity.Text = row.Text;
                    aiConfig.RemoveNames = row.NameRemoval ?? NameRemoval.NoChanges;
                    aiConfig.TextEnhancement = row.TextEnhancement ?? TextEnhancement.NoChanges;
                    var messages = PrayerRequestService.PrayerRequestFormatterMessages( testEntity, options.LavaCompletionTemplate, aiConfig );

                    var batchLine = new OpenAIBatchFileRow
                    {
                        CustomId = "request-" + row.RowNumber.ToString(),
                        Body = new OpenAIBatchBody
                        {
                            Model = options.Model,
                            Messages = messages.Select( m => new OpenAIRequestMessage
                            {
                                Role = m.Role.ToString().ToLower(),
                                Content = m.Content
                            } ),
                            MaxTokens = 1000
                        }
                    };

                    try
                    {
                        writer.WriteLine( JsonConvert.SerializeObject( batchLine ) );
                    }
                    catch ( Exception ex )
                    {
                        return ex.Message;
                    }
                }
            }

            return "File created at " + options.OutFile;
        }

        /// <summary>
        /// Reads the contents of a specified <paramref name="inputFilePath"/>
        /// and for each line calls the PrayerRequest.GetAIAutomationAnalyzerResults
        /// with the contents of the line as the "Text" of the PrayerRequest.
        /// The original CSV file is written to a new file specified by the <paramref name="options.OutFile"/> param.
        /// The new file includes the response from the completion.
        /// </summary>
        /// <param name="testAiConfig">The AIAutomation configuration containing the AIProvider and AI model to use.</param>
        /// <param name="options">The options to use for processing the completions file.</param>
        /// <returns>A status message.</returns>
        private async Task<string> ProcessAnalyzerCompletionsFile( AIAutomation testAiConfig, AICompletionsFromFileOptions options )
        {
            if ( !File.Exists( options.InFile ) )
            {
                return "File does not exist or is inaccessible.";
            }

            if ( File.Exists( options.OutFile ) && new FileInfo( options.OutFile ).Length > 0 && !options.OverwriteOutFile )
            {
                // Don't overwrite files - if we've already run this
                // and there are values in the current file we don't want to lose those.
                // Let the caller do that so we can be sure the delete is intentional.
                return "Won't overwrite an existing file. Please rename the current file or delete it if it's not wanted.";
            }

            var prayerRequestService = new PrayerRequestService( new RockContext() );
            var testEntity = new PrayerRequest();

            // Get a CSV reader and writer.
            // Auto-detect the encoding in the reader and use the same encoding for the writer.
            using ( StreamReader reader = new StreamReader( options.InFile, true ) )
            using ( StreamWriter writer = new StreamWriter( options.OutFile, false, reader.CurrentEncoding ) )
            using ( var csv = new CsvReader( reader ) )
            using ( var csvWriter = new CsvWriter( writer ) )
            {
                var counter = 0;
                csvWriter.WriteHeader<PrayerRequestAutomationTestingFile>();
                while ( csv.Read() && ( options.MaxCompletions == 0 || counter < options.MaxCompletions ) )
                {
                    var row = csv.GetRecord<PrayerRequestAutomationTestingFile>();
                    testEntity.Text = row.Text;

                    try
                    {
                        var response = await prayerRequestService.GetAIAutomationAnalyzerResults( testEntity, testAiConfig, options.LavaCompletionTemplate );

                        var category = options.Categories.FirstOrDefault( c => c.Id == response.CategoryId );
                        var sentiment = sentiments.FirstOrDefault( c => c.Id == response.SentimentId );
                        row.ResponseCategory = category.Name;
                        row.ResponseSentiment = sentiment.Value;
                        row.IsAppropriateForPublic = response.IsAppropriateForPublic;
                    }
                    catch ( Exception ex )
                    {
                        row.Response = "Error: " + ex.Message;
                    }
                    finally
                    {
                        csvWriter.WriteRecord( row );
                        counter++;
                    }
                }
            }

            return $"Results saved to '{options.OutFile}'.";
        }

        /// <summary>
        /// Reads the contents of a specified <paramref name="inputFilePath"/>
        /// and for each line calls the PrayerRequest.GetAIAutomationFormatterResults
        /// with the contents of the line as the "Text" of the PrayerRequest.
        /// The original CSV file is written to a new file specified by the <paramref name="options.OutFile"/> param.
        /// The new file includes the response from the completion.
        /// </summary>
        /// <param name="testAiConfig">The AIAutomation configuration containing the AIProvider and AI model to use.</param>
        /// <param name="options">The options to use for processing the completions file.</param>
        /// <returns>A status message.</returns>
        private async Task<string> ProcessFormatterCompletionsFile( AIAutomation testAiConfig, AICompletionsFromFileOptions options )
        {
            if ( !File.Exists( options.InFile ) )
            {
                return "File does not exist or is inaccessible.";
            }

            if ( File.Exists( options.OutFile ) && new FileInfo( options.OutFile ).Length > 0 && !options.OverwriteOutFile )
            {
                // Don't overwrite files - if we've already run this
                // and there are values in the current file we don't want to lose those.
                // Let the caller do that so we can be sure the delete is intentional.
                return "Won't overwrite an existing file. Please rename the current file or delete it if it's not wanted.";
            }

            var prayerRequestService = new PrayerRequestService( new RockContext() );
            var testEntity = new PrayerRequest();

            // Get a CSV reader and writer.
            // Auto-detect the encoding in the reader and use the same encoding for the writer.
            using ( StreamReader reader = new StreamReader( options.InFile, true ) )
            using ( StreamWriter writer = new StreamWriter( options.OutFile, false, reader.CurrentEncoding ) )
            using ( var csv = new CsvReader( reader ) )
            using ( var csvWriter = new CsvWriter( writer ) )
            {
                var counter = 0;
                csvWriter.WriteHeader<PrayerRequestAutomationTestingFile>();
                while ( csv.Read() && ( options.MaxCompletions == 0 || counter < options.MaxCompletions ) )
                {
                    var row = csv.GetRecord<PrayerRequestAutomationTestingFile>();
                    testEntity.Text = row.Text;
                    testAiConfig.RemoveNames = row.NameRemoval ?? NameRemoval.NoChanges;
                    testAiConfig.TextEnhancement = row.TextEnhancement ?? TextEnhancement.NoChanges;

                    try
                    {
                        var response = await prayerRequestService.GetAIAutomationFormatterResults( testEntity, testAiConfig, options.LavaCompletionTemplate );
                        row.Response = response.Content;
                    }
                    catch ( Exception ex )
                    {
                        row.Response = "Error: " + ex.Message;
                    }
                    finally
                    {
                        csvWriter.WriteRecord( row );
                        counter++;
                    }
                }
            }

            return $"Results saved to '{options.OutFile}'.";
        }

        /// <summary>
        /// Executes <see cref="GetAIAutomationAnalyzerResults"/> for each row in the specified <paramref name="inFile"/>.
        /// Uses the specified model (defaults to 'gpt-4o-mini').
        /// Each method call sets the values below based on the file contents:
        ///     PrayerRequest.Text to the <see cref="PrayerRequestAutomationTestingFile.Text"/> property value.
        /// </summary>
        private async Task RunOpenAIAnalyzerCompletionsFromFile( AICompletionsFromFileOptions options )
        {
            const string openAiProviderGuid = "2AA26B14-94CB-4A30-9E97-C7250BA464BB";
            var aiProvider = new AIProviderService( new RockContext() ).Get( openAiProviderGuid.AsGuid() );

            var aiConfig = new AIAutomation
            {
                AIModel = options.Model,
                AIProvider = aiProvider,
                ChildCategories = options.Categories,
                AutoCategorize = true,
                ClassifySentiment = true,
                CheckPublicAppropriateness = true
            };

            await ProcessAnalyzerCompletionsFile( aiConfig, options );
        }

        /// <summary>
        /// Executes <see cref="GetAIAutomationFormatterResults"/> for each row in the specified <paramref name="inFile"/>.
        /// Uses the specified model (defaults to 'gpt-4o-mini').
        /// Each method call sets the values below based on the file contents:
        ///     PrayerRequest.Text to the <see cref="PrayerRequestAutomationTestingFile.Text"/> property value
        ///     aiAutomation.NameRemoval to the <see cref="PrayerRequestAutomationTestingFile.NameRemoval"/> property value
        ///     aiAutomation.TextEnhancement to the <see cref="PrayerRequestAutomationTestingFile.TextEnhancement"/> property value
        /// </summary>
        private async Task RunOpenAIFormatterCompletionsFromFile( AICompletionsFromFileOptions options )
        {
            const string openAiProviderGuid = "2AA26B14-94CB-4A30-9E97-C7250BA464BB";
            var aiProvider = new AIProviderService( new RockContext() ).Get( openAiProviderGuid.AsGuid() );

            var aiConfig = new AIAutomation
            {
                AIModel = options.Model,
                AIProvider = aiProvider,
            };

            await ProcessFormatterCompletionsFile( aiConfig, options );
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// The options available for processing an AI Completions file.
        /// </summary>
        public class AICompletionsFromFileOptions
        {
            /// <summary>
            /// The full path to the file to read in.
            /// </summary>
            public string InFile { get; set; }

            /// <summary>
            /// The full path to the file to create containing the results.
            /// </summary>
            public string OutFile { get; set; }

            /// <summary>
            /// <see langword="true"/> if the OutFile should be overwritten; otherwise <see langword="false"/>.
            /// </summary>
            public bool OverwriteOutFile { get; set; }

            /// <summary>
            /// The lava template to use to generate the completion.
            /// </summary>
            public string LavaCompletionTemplate { get; set; }

            /// <summary>
            /// The maximum number of completions to run ( defaults to 3 ).
            /// Use a low number when researching completions/prompts/templates.
            /// </summary>
            public int MaxCompletions { get; set; } = 3;

            /// <summary>
            /// The OpenAI Model name to use to run the completions.
            /// </summary>
            public string Model { get; set; } = "gpt-4o-mini";

            /// <summary>
            /// Optional list of Categories to provide to the Analyzer completion to choose from.
            /// </summary>
            public IEnumerable<Category> Categories { get; set; }
        }

        /// <summary>
        /// Defines the shape of the test data for AI Completion testing.
        /// </summary>
        internal class PrayerRequestAutomationTestingFile
        {
            /// <summary>
            /// The row number (or uinique id) of the request.
            /// </summary>
            public int RowNumber { get; set; }

            /// <summary>
            /// The text of the PrayerRequest to test.
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// The category of the source PrayerRequest.
            /// </summary>
            public string Category { get; set; }

            /// <summary>
            /// The CategoryId of the source PrayerRequest.
            /// </summary>
            public int? CategoryId { get; set; }

            /// <summary>
            /// The length of the original text.
            /// </summary>
            public int? TextLength { get; set; }

            /// <summary>
            /// The CSV list of first names (if any) in the Text.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// The CSV list of last names (if any) in the Text.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Any notes for the tester. Currently used for identifying test cases like pluralized names, initials etc.
            /// </summary>
            public string Note { get; set; }

            /// <summary>
            /// The type of NameRemoval to perform on the completion for the text.
            /// </summary>
            public NameRemoval? NameRemoval { get; set; }

            /// <summary>
            /// The type of TextEnhancement to perform on the completion for the text.
            /// </summary>
            public TextEnhancement? TextEnhancement { get; set; }

            /// <summary>
            /// The response from the AI Completion endpoint.
            /// </summary>
            public string Response { get; set; }

            /// <summary>
            /// The Category chosen by the Anlayzer completion.
            /// </summary>
            public string ResponseCategory { get; set; }

            /// <summary>
            /// The Sentiment chosen by the Anlayzer completion.
            /// </summary>
            public string ResponseSentiment { get; set; }

            /// <summary>
            /// <see langword="true"/> if the text is appropriate for public viewing; otherwise , <see langword="false"/>.
            /// </summary>
            public bool? IsAppropriateForPublic { get; set; }
        }

        /// <summary>
        /// A line for a .jsonl file in the format expected by OpenAI for batch processing.
        /// </summary>
        internal class OpenAIBatchFileRow
        {
            /// <summary>
            /// The unique identifier of the row.
            /// </summary>
            [JsonProperty( "custom_id" )]
            public string CustomId { get; set; }

            /// <summary>
            /// The HTTP Method to be used with the endpoint.
            /// </summary>
            [JsonProperty( "method" )]
            public string HttpMethod { get; set; } = "POST";

            /// <summary>
            /// The endpoint to use for the row.
            /// </summary>
            [JsonProperty( "url" )]
            public string Endpoint { get; set; } = "/v1/chat/completions";

            /// <summary>
            /// The body to send to the endpoint.
            /// </summary>
            [JsonProperty( "body" )]
            public OpenAIBatchBody Body { get; set; }
        }

        /// <summary>
        /// The Body of an OpenAI Batch record.
        /// </summary>
        internal class OpenAIBatchBody
        {
            /// <summary>
            /// The model to use for the request.
            /// </summary>
            [JsonProperty( "model" )]
            public string Model { get; set; }

            /// <summary>
            /// The messages to send to the request.
            /// </summary>
            [JsonProperty( "messages" )]
            public IEnumerable<OpenAIRequestMessage> Messages { get; set; }

            /// <summary>
            /// The maximum tokens allowed for the request.
            /// </summary>
            [JsonProperty( "max_tokens" )]
            public int MaxTokens { get; set; } = 4096;
        }

        /// <summary>
        /// The RequestMessage for the Batch.
        /// </summary>
        internal class OpenAIRequestMessage
        {
            /// <summary>
            /// The role of the person writing the content (e.g. "System" or "User").
            /// </summary>
            [JsonProperty( "role" )]
            public string Role { get; set; }

            /// <summary>
            /// The content of the message.
            /// </summary>
            [JsonProperty( "content" )]
            public string Content { get; set; }
        }

        #endregion
    }
}
