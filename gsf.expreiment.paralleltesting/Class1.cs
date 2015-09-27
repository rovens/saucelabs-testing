using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using TechTalk.SpecFlow;

namespace gsf.expreiment.paralleltesting
{
    public class Class1
    {
    }

    [Binding]
    public class GoogleSteps
    {
        readonly IWebDriver _driver;

        public GoogleSteps()
        {
            _driver = (IWebDriver)ScenarioContext.Current["driver"];
        }

        [Given(@"I am on the google page")]
        public void GivenIAmOnTheGooglePage()
        {
            _driver.Navigate().GoToUrl("http://www.google.com");
        }

        [When(@"I search the web")]
        public void WhenISearchTheWeb()
        {
            var q = _driver.FindElement(By.Name("q"));
            q.SendKeys("Kenneth Truyers");
            q.Submit();
        }

        [Then(@"I get search results")]
        public void ThenIGetSearchResults()
        {
            Assert.That(_driver.FindElement(By.Id("resultStats")).Text, Is.Not.Empty);
        }
    }

    [Binding]
    public class Setup
    {
        private IWebDriver driver;

        [BeforeScenario]
        public void BeforeScenario()
        {
            //driver = new OpenQA.Selenium.PhantomJS.PhantomJSDriver();
            var capabilities = new DesiredCapabilities();

            // construct the url to sauce labs
            Uri commandExecutorUri = new Uri("http://ondemand.saucelabs.com/wd/hub");


            capabilities.SetCapability("username", "globalx"); // supply sauce labs username
            capabilities.SetCapability("accessKey", "4817df63-14c6-40d1-9adf-1e421ad8c8eb");
                // supply sauce labs account key
            capabilities.SetCapability("name", TestContext.CurrentContext.Test.Name); // give the test a name
            capabilities.SetCapability("timeZone", "Queensland");
            capabilities.SetCapability("browserName", "Chrome");
            capabilities.SetCapability("platform", "Windows 10");
            capabilities.SetCapability("version", "42.0");
            capabilities.SetCapability("build", "Google");
            capabilities.SetCapability("name", ScenarioContext.Current.ScenarioInfo.Title);

            // start a new remote web driver session on sauce labs
            driver = new CustomeRemoteDriver(commandExecutorUri, capabilities);
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));



            //     driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
            ScenarioContext.Current["driver"] = driver;
        }


        [AfterScenario]
        public void AfterScenario()
        {

            var sessionId = ((CustomeRemoteDriver) driver).GetSessionId();
            var client = new RestClient("https://saucelabs.com/rest/v1/globalx/jobs");
            client.Authenticator = new HttpBasicAuthenticator("globalx", "4817df63-14c6-40d1-9adf-1e421ad8c8eb");

            var request = new RestRequest("{id}", Method.PUT);
            request.AddUrlSegment("id", sessionId);
            request.AddJsonBody(new {passed = ScenarioContext.Current.TestError == null});
            client.Execute(request);
            driver.Dispose();
        }


    }

    public class CustomeRemoteDriver : RemoteWebDriver
    {

        public CustomeRemoteDriver(ICapabilities desiredCapabilities) : base(desiredCapabilities)
        {
        }

        public CustomeRemoteDriver(ICommandExecutor commandExecutor, ICapabilities desiredCapabilities) : base(commandExecutor, desiredCapabilities)
        {
        }

        public CustomeRemoteDriver(Uri remoteAddress, ICapabilities desiredCapabilities) : base(remoteAddress, desiredCapabilities)
        {
        }

        public CustomeRemoteDriver(Uri remoteAddress, ICapabilities desiredCapabilities, TimeSpan commandTimeout) : base(remoteAddress, desiredCapabilities, commandTimeout)
        {
        }

        public string GetSessionId()
        {
            return base.SessionId.ToString();
        }
    }
}
