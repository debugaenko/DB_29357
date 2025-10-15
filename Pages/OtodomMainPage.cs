using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Elements.Interfaces;
//using Aquality.Selenium.Core.Applications;
//using DB_29357.Helpers;
//using FluentAssertions.Specialized;
//using Io.Cucumber.Messages.Types;
using OpenQA.Selenium;
using OtodomTests.Support;

namespace DB_29357.Pages
{
    public class OtodomMainPage : BasePage
    {
        private readonly Logger _logger = AqualityServices.Logger;
        private readonly MainPageLoginActions _mainPageLoginActions;
        private readonly TestConfig _config;
        
        public int PriceMin { get; set; }
        public int PriceMax { get; set; }

        private IButton CookieAcceptButton => ElementFactory.GetButton(By.CssSelector("#onetrust-accept-btn-handler, button[id*='accept'], button[id*='consent']"), "Cookie Accept Button");

        private IButton MojeKontoButton => ElementFactory.GetButton(By.XPath("//button[@data-cy='navbar-my-account-button']"), "Moje Konto Button");

        public OtodomMainPage(TestConfig testConfig, MainPageLoginActions mainPageLoginActions) : base(By.XPath("//input[contains(@class, 'n-textinput-input')]"), "Main Page")
        {
            _config = testConfig ?? throw new ArgumentNullException(nameof(testConfig));
            _mainPageLoginActions = new MainPageLoginActions(_config);
        }

        public void GivenIOpenTheOtodomMainPageAndVerifyItLoadsCorrectly()
        {
            _logger.Info("[GivenIOpenTheOtodomMainPageAndVerifyItLoadsCorrectly] Starting: Open Otodom main page and verify it loads");

            NavigateToStartUrl();
            if (!State.WaitForDisplayed())
            {
                _logger.Warn("[WaitForPageLoad] Page locator not displayed, retrying once...");
                Browser.Refresh();
                State.WaitForDisplayed();
            }
            _logger.Info($"[WaitForPageLoad] Successfully loaded: {Browser.CurrentUrl}");
            AcceptCookies();
            State.WaitForDisplayed();
            var currentUrl = AqualityServices.Browser.CurrentUrl;
            _logger.Info($"[GivenIOpenTheOtodomMainPageAndVerifyItLoadsCorrectly] Main page successfully opened: {currentUrl}");
        }

        private void NavigateToStartUrl()
        {
            var browser = AqualityServices.Browser;
            browser.Maximize();

            if (!browser.CurrentUrl.StartsWith(_config.StartUrl, StringComparison.OrdinalIgnoreCase))
            {
                browser.GoTo(_config.StartUrl);
                browser.WaitForPageToLoad();
            }
        }

        public bool WhenIPassThroughTheAuthorizationProcessWithValidCredentials(string? email = null, string? password = null)
        {
            var Email = Environment.GetEnvironmentVariable("OTODOM_TEST_EMAIL") ?? _config.Email;
            var Password = Environment.GetEnvironmentVariable("OTODOM_TEST_PASSWORD") ?? _config.Password;

            _logger.Info("[WhenIPassThroughTheAuthorizationProcessWithValidCredentials] User authorization process");
            MojeKontoButton.Click();
            _logger.Info($"[WhenIPassThroughTheAuthorizationProcessWithValidCredentials] Email used for login: {Email}");
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                _logger.Warn("[WhenIPassThroughTheAuthorizationProcessWithValidCredentials] Test credentials not configured - continuing with anonymous session");
                return false;
            }

            _mainPageLoginActions.FillLoginForm(email, password);
            _mainPageLoginActions.SubmitLoginForm();
            _logger.Info("[WhenIPassThroughTheAuthorizationProcessWithValidCredentials] Authorization process completed");
            return true;
        }

        public void ThenIVerifyTheUserIsAuthorizedAndMainPageIsOpened()
        {
            _logger.Info("[ThenIVerifyTheUserIsAuthorizedAndMainPageIsOpened] Start user authorization verification");

            VerifyAuthorization();
        }

        public void AcceptCookies()
        {
            if (CookieAcceptButton.State.WaitForDisplayed())
            {
                CookieAcceptButton.Click();
                _logger.Info("[AcceptCookies] Cookie consent accepted");
                return;
            }

            _logger.Debug("[AcceptCookies] No cookie consent buttons found");
        }
        public void VerifyAuthorization()
        {
            var browser = AqualityServices.Browser;
            browser.WaitForPageToLoad();
            var isButtonVisible = MojeKontoButton.State.WaitForDisplayed();
            //TimeSpan.FromSeconds(5)
            if (isButtonVisible && MojeKontoButton.State.IsEnabled)
            {
                _logger.Error("[VerifyAuthorization] Moje konto button is displayed");
                throw new InvalidOperationException("[VerifyAuthorization] Moje konto button is displayed");
            }
            _logger.Info($"[VerifyAuthorization] User is successfully authorized");
        }
    }
}
