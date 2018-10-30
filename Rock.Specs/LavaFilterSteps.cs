using System;
using Rock.Lava;
using TechTalk.SpecFlow;
using Xunit;

namespace Rock.Specs
{
    [Binding]
    public class LavaFilterSteps
    {
        private int lavaValue = 0;
        private int lavaParam = 0;
        private object sum = 0;

        [Given(@"I have a value of (.*)")]
        public void GivenIHaveAValueOf(int p0)
        {
            lavaValue = p0;
        }
        
        [Given(@"I have entered (.*) as a parameter")]
        public void GivenIHaveEnteredAsAParameter(int p0)
        {
            lavaParam = p0;
        }
        
        [When(@"The lava resolves")]
        public void WhenTheLavaResolves()
        {
            sum = RockFilters.Plus( lavaValue, lavaParam );
        }
        
        [Then(@"the result should be (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(int p0)
        {
            Assert.Equal( p0, sum );
        }
    }
}
