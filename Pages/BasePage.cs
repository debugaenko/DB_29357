using Aquality.Selenium.Forms;
using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using OpenQA.Selenium;

namespace OtodomTests_29357.Pages
{
    public abstract class BasePage : Form
    {
        protected new static readonly Logger Logger = AqualityServices.Get<Logger>();

        protected Browser Browser => AqualityServices.Browser;

        protected BasePage(By locator, string name) : base(locator, name) { }

    }
}
