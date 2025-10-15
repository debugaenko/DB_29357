using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Elements.Interfaces;
using Aquality.Selenium.Forms;
using OpenQA.Selenium;
using OtodomTests.Support;

namespace DB_29357.Pages
{
    public sealed class MainPageLoginActions : Form
    {
        private readonly Logger _logger;
        private readonly TestConfig _config;

        public MainPageLoginActions(TestConfig config) : base(By.CssSelector("form[data-testid='login-form'], button[data-testid='login-submit-button']"), "Login Page")
        {
            _logger = AqualityServices.Logger;
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private ITextBox EmailInput => ElementFactory.GetTextBox(By.CssSelector("#username, input#username[name='username'][type='email']"), "Email Input");

        private ITextBox PasswordInput => ElementFactory.GetTextBox(By.CssSelector("#password, input#password[name='password'][type='password']"), "Password Input");

        private IButton SubmitButton => ElementFactory.GetButton(By.CssSelector("button#Login[data-testid='login-submit-button'], button[data-testid='login-submit-button']"), "Login Button");

        public bool FillLoginForm(string? email = null, string? password = null)
        {

            var Email = email ?? _config.Email;
            var Password = password ?? _config.Password;
         
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                throw new InvalidOperationException("[FillLoginForm] Login credentials are not configured in TestConfig or environment variables.");
            }

            _logger.Info("[FillLoginForm] Filling login credentials");

            if (!EmailInput.State.WaitForDisplayed())
            {// TimeSpan.FromSeconds(5)
                _logger.Warn("[FillLoginForm] Email input not found");
                throw new InvalidOperationException("[FillLoginForm] Email input not found on login form.");
            }

            EmailInput.ClearAndType(Email);
            PasswordInput.ClearAndType(Password);

            _logger.Info($"[FillLoginForm] Credentials entered (email: {_config.Email}, password length: {_config.Password?.Length ?? 0})");
            return true;
        }

        public void SubmitLoginForm()
        {
            _logger.Info("[SubmitLoginForm] Submitting login form");

            if (!SubmitButton.State.IsDisplayed || !SubmitButton.State.IsClickable)
            {
                _logger.Warn("[SubmitLoginForm] Submit button is not clickable");
                return;
            }

            SubmitButton.Click();
            EmailInput.State.WaitForNotDisplayed();
        }
    }
}
