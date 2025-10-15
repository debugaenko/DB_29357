using Aquality.Selenium.Browsers;
using NUnit.Framework;
using Reqnroll;
using Aquality.Selenium.Core.Logging;

namespace DB_29357.Hooks
{
    [Binding]
    public class TestHooks
    {
        private static readonly Logger Logger = Logger.Instance;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            Logger.Info("=== Starting Test Run ===");
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            Logger.Info("=== Test Run Complete ===");
        }

        [BeforeScenario]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            Logger.Info($"=== Scenario: {scenarioContext.ScenarioInfo.Title} ===");
        }

        [AfterScenario]
        public void AfterScenario(ScenarioContext scenarioContext)
        {
            if (AqualityServices.IsBrowserStarted)
            {
                AqualityServices.Browser.Quit();
            }
            Logger.Info($"=== Scenario Complete: {scenarioContext.ScenarioInfo.Title} ===");
        }

        [AfterStep]
        public void AfterStep(ScenarioContext scenarioContext)
        {
            if (scenarioContext.TestError != null)
            {
                var stepInfo = scenarioContext.StepContext.StepInfo;
                Logger.Error($"Step failed: {stepInfo.StepDefinitionType} {stepInfo.Text}");
                Logger.Error($"Error: {scenarioContext.TestError.Message}");
                
                try
                {
                    if (AqualityServices.IsBrowserStarted)
                    {
                        var screenshot = AqualityServices.Browser.Driver.GetScreenshot();
                        var fileName = $"failure_{DateTime.Now:yyyyMMdd_HHmmss}_{scenarioContext.ScenarioInfo.Title.Replace(" ", "_")}.png";
                        screenshot.SaveAsFile(fileName);
                        Logger.Info($"Screenshot saved: {fileName}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to take screenshot: {ex.Message}"); 
                }
            }
        }
    }
}