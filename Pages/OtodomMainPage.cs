using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Elements.Interfaces;
using OpenQA.Selenium;
using OtodomTests.Support;

namespace OtodomTests_29357.Pages
{
    public class OtodomMainPage : BasePage
    {
        private readonly Logger _logger = AqualityServices.Logger;
        private readonly TestConfig _config;

        public int PriceMin { get; set; }
        public int PriceMax { get; set; }

        private IButton CookieAcceptButton => ElementFactory.GetButton(
            By.CssSelector("#onetrust-accept-btn-handler, button[id*='accept'], button[id*='consent']"),
            "Cookie Accept Button");

        private IButton MojeKontoButton => ElementFactory.GetButton(
            By.XPath("//button[@data-cy='navbar-my-account-button']"),
            "Moje Konto Button");

        private ITextBox EmailInput => ElementFactory.GetTextBox(
            By.CssSelector("#username, input#username[name='username'][type='email']"),
            "Email Input");

        private ITextBox PasswordInput => ElementFactory.GetTextBox(
            By.CssSelector("#password, input#password[name='password'][type='password']"),
            "Password Input");

        private IButton LoginSubmitButton => ElementFactory.GetButton(
            By.CssSelector("button#Login[data-testid='login-submit-button'], button[data-testid='login-submit-button']"),
            "Login Button");

        public OtodomMainPage(TestConfig testConfig)
            : base(By.XPath("//input[contains(@class, 'n-textinput-input')]"), "Main Page")
        {
            _config = testConfig ?? throw new ArgumentNullException(nameof(testConfig));
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

            if (!browser.CurrentUrl.StartsWith(_config.StartUrl, StringComparison.OrdinalIgnoreCase))
            {
                browser.GoTo(_config.StartUrl);
                browser.WaitForPageToLoad();
            }
        }

        public bool WhenIPassThroughTheAuthorizationProcessWithValidCredentials(string? email = null, string? password = null)
        {
            var Email = email ?? _config.Email;
            var Password = password ?? _config.Password;

            _logger.Info("[WhenIPassThroughTheAuthorizationProcessWithValidCredentials] User authorization process");
            MojeKontoButton.Click();
            _logger.Info($"[WhenIPassThroughTheAuthorizationProcessWithValidCredentials] Email used for login: {Email}");

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                _logger.Warn("[WhenIPassThroughTheAuthorizationProcessWithValidCredentials] Test credentials not configured - continuing with anonymous session");
                return false;
            }

            FillLoginForm(Email, Password);
            SubmitLoginForm();
            _logger.Info("[WhenIPassThroughTheAuthorizationProcessWithValidCredentials] Authorization process completed");
            return true;
        }

        private bool FillLoginForm(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("[FillLoginForm] Login credentials are not configured in TestConfig or environment variables.");
            }

            _logger.Info("[FillLoginForm] Filling login credentials");

            if (!EmailInput.State.WaitForDisplayed())
            {
                _logger.Warn("[FillLoginForm] Email input not found");
                throw new InvalidOperationException("[FillLoginForm] Email input not found on login form.");
            }

            EmailInput.ClearAndType(email);
            PasswordInput.ClearAndType(password);

            _logger.Info($"[FillLoginForm] Credentials entered (email: {email}, password length: {password.Length})");
            return true;
        }

        private void SubmitLoginForm()
        {
            _logger.Info("[SubmitLoginForm] Submitting login form");

            if (!LoginSubmitButton.State.IsDisplayed || !LoginSubmitButton.State.IsClickable)
            {
                _logger.Warn("[SubmitLoginForm] Submit button is not clickable");
                return;
            }

            LoginSubmitButton.Click();
            EmailInput.State.WaitForNotDisplayed();
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

            if (isButtonVisible && MojeKontoButton.State.IsEnabled)
            {
                _logger.Error("[VerifyAuthorization] Moje konto button is displayed");
                throw new InvalidOperationException("[VerifyAuthorization] Moje konto button is displayed");
            }
            _logger.Info($"[VerifyAuthorization] User is successfully authorized");
        }
    }
}