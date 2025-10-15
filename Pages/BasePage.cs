using Aquality.Selenium.Forms;
using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Elements.Interfaces;
using OpenQA.Selenium;

namespace DB_29357.Pages
{
    public abstract class BasePage : Form
    {
        protected new static readonly Logger Logger = AqualityServices.Get<Logger>();

        protected Browser Browser => AqualityServices.Browser;

        protected BasePage(By locator, string name) : base(locator, name) { }

    }
}
