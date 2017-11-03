using System;
using Rock.Lava;
using TechTalk.SpecFlow;
using Xunit;

namespace Rock.Specs.LavaFilters
{
    [Binding]
    [Scope(Feature ="Lava Plus Filter")]
    public class PlusSteps
    {
        object input;
        object parameter;
        object output;

        [Given(@"I have a value of (.*)")]
        public void GivenIHaveAValueOf(object p0)
        {
            input = p0;
        }
        
        [Given(@"I have entered (.*) as a parameter")]
        public void GivenIHaveEnteredAsAParameter(object p0)
        {
            parameter = p0;
        }
        
        [When(@"The lava resolves")]
        public void WhenTheLavaResolves()
        {
            output = RockFilters.Plus( input, parameter );
        }
        
        [Then(@"the result should be a decimal (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(decimal p0)
        {
            Assert.Equal( p0, output );
        }

        [Then( @"the result should be an integer (.*) on the screen" )]
        public void ThenTheResultShouldBeOnTheScreen( int p0 )
        {
            Assert.Equal( p0, output );
        }
    }
}
