using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Browsers;
using NUnit.Framework;
using Reqnroll;
using OtodomTests_29357.Pages;

namespace OtodomTests_29357.StepDefinitions
{
    [Binding]
    [Category("BDD")]
    [Category("Verification")]
    public class OtodomVerificationSteps
    {
        private static readonly Logger Logger = AqualityServices.Logger;
        private readonly OfferSelectionService _offerSelectionService;
        private readonly ValidateDetailsPage _validateDetailsPage;

        public OtodomVerificationSteps(
            OfferSelectionService offerSelectionService,
            ValidateDetailsPage validateDetailsPage)
        {
            _offerSelectionService = offerSelectionService;
            _validateDetailsPage = validateDetailsPage;
        }

        [When(@"I get a random offer and save price, rooms, surface to scenario context")]
        public void WhenIGetRandomOfferAndSavePriceRoomsSurfaceToScenarioContext()
        {
            Logger.Info("Starting random offer selection");
            _offerSelectionService.SelectAndSaveRandomOffer();
        }

        [When(@"I click on the selected offer")]
        public void WhenIClickOnTheSelectedOffer()
        {
            Logger.Info("Navigate to offer details");
            _offerSelectionService.NavigateToSelectedOffer();
        }

        [Then(@"I verify the offer page opened and price, rooms, surface match the saved context")]
        public void ThenIVerifyOfferPageOpenedAndDataMatches()
        {
            Logger.Info("Starting offer details verification");
            _validateDetailsPage.VerifyOfferDetailsMatch();
        }
    }
}