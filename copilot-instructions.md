# Copilot Instructions – Otodom.pl Autotest

These instructions combine acceptance criteria for the Otodom.pl apartment search autotest and general best practices for test automation. Follow them strictly when generating or modifying test code.

# 1. Test Flow (Acceptance Criteria)

# Open Website

Navigate to [https://www.otodom.pl/](https://www.otodom.pl/).
Ensure the main page is successfully loaded.

# Authorization

Perform user authorization.
Expected result: User is authorized and the main page is displayed.

# Location and Price Filters

Apply Location and Price filters.
Click **Search**.
Expected result:Results page opens and apartment prices are within the selected range.

# Surface Filter

Remove the price filter.
Retrieve the minimum and maximum apartment surfaces (m²) from the first results page.
Apply these values to the Surface filter.
Expected result: Results page opens and apartment surfaces are within the selected range.

# Offer Details Verification

Select a random offer.
Save price, number of rooms, and surface (m²) in scenario context.
Open the selected offer page.
Expected result: Price, number of rooms, and surface match saved values.

# 2. Technical Stack

Language/Frameworks: C#, NUnit, NLog, Reqnroll (BDD for .NET)
UI Testing: Selenium with Aquality Automation Framework

# 3. Implementation Rules

Form elements must be defined as **fields or properties** in Page Object classes, not in methods.
Avoid common UI checks unless you understand and can implement them correctly. If unsure, ask for page HTML.
Do not use brute force code like `Driver.FindElements` in step implementations. Move such logic into Page Objects or utility classes.
Replace all `Thread.Sleep` calls with `AqualityServices.Get<IAnimationWait>().Wait()`.
Follow **KISS, SOLID, DRY** principles.
Use fields or properties for static web elements that are reused across methods.
References to other projects must be added in `reqnroll.json` as assemblies.

# 4. Project Guidelines

# Structure

Organize logically: **Pages, StepDefinitions, Helpers, Tests, Features, Resources, Utils**.
Maintain separation of concerns for easier maintenance and extension.

# Credentials

* Store in JSON file, allow override via environment variables.
* Never hardcode sensitive data.

# Logging

Use Aquality built-in logger (`Logger.Instance.Info("message")`).
Do not create custom loggers or use `Console.WriteLine`.

# Locators

Use stable attributes (`id`, `data-testid`, `aria-label`).
Avoid auto-generated CSS selectors or deeply nested selectors.

# Page Verification

Check page by key elements, not by URL comparison.

# Page Object Encapsulation

Do not expose raw `IWebElement`. Use wrappers like `IButton`, `ICheckBox`, `ILabel`.

# Dependency Injection

Use DI for maintainability. Avoid manual instantiation of pages in tests.

# Size Limits

Classes: 100–150 lines.
Methods: 20–30 lines.

# Assertions

Use FluentAssertions with `AssertionScope`.
Avoid `Assert.AreEqual`, `Assert.IsTrue`, etc.

# Exceptions

Do not use try-catch in test flow. Exceptions must propagate.
Only acceptable in utility or infrastructure code.

# WebDriver Access

Do not access driver directly.
Use `elementFactory.FindElements` or parent-child search via Aquality abstractions.

# Data Sharing

Share parameters between steps using **context objects**.
Avoid static variables or global state.

# 5. Summary Checklist

Proper structure (Pages, Steps, Utilities)
Credentials securely stored and overridable
Aquality logger used
Stable locators only
Page verification via elements, not URLs
No raw `IWebElement` outside Page Objects
Dependency Injection applied
Classes ≤150 lines, methods ≤30 lines
FluentAssertions with AssertionScope used
No try-catch in test flow
No direct WebDriver usage
Context-based data sharing

# 6. Notes

These practices are mandatory to ensure maintainability, stability, and consistency across the Otodom.pl test automation project. Always follow them when generating or updating code.
