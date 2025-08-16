using System.Configuration;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.AI.Agent.Tests.Skills.GroupManager;

[TestClass]
[MethodIgnoreIf( nameof( HasRequiredConfiguration ), "Missing configuration settings in app.TestSettings.config file." )]
public class GroupManagerSkillTests : BaseFunctionCallTests
{
    /// <summary>
    /// Checks if the required configuration settings for Azure OpenAI are present.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; <c>false</c> otherwise</returns>
    public static bool HasRequiredConfiguration()
    {
        return !string.IsNullOrWhiteSpace( ConfigurationManager.AppSettings["AzureOpenAIApiKey"] )
            && !string.IsNullOrWhiteSpace( ConfigurationManager.AppSettings["AzureOpenAIEndpoint"] )
            && !ConfigurationManager.AppSettings["SkipAzureOpenAI"].ToStringSafe().AsBoolean();
    }

    [ConditionalTestMethod]
    public async Task GroupTypeListOutput_DoesNotContainIds()
    {

    }
}
