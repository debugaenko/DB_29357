using Aquality.Selenium.Core.Logging;
using NUnit.Framework;
using Reqnroll;

namespace DB_29357.StepDefinitions
{
    [Binding]
    [Category("BDD")]
    [Category("Verification")]
    public class OtodomVerificationSteps
    {
        private static readonly Logger Logger = Logger.Instance;
        private readonly ScenarioContext _scenarioContext;
        private readonly OfferSelectionService _offerSelectionService;
        private readonly OfferDataVerificationService _verificationService;
        private readonly NavigationService _navigationService;

        public OtodomVerificationSteps(
            ScenarioContext scenarioContext,
            OfferSelectionService offerSelectionService,
            OfferDataVerificationService verificationService,
            NavigationService navigationService)
        {
            _scenarioContext = scenarioContext;
            _offerSelectionService = offerSelectionService;
            _verificationService = verificationService;
            _navigationService = navigationService;
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
            _navigationService.NavigateToSelectedOffer();
        }

        [Then(@"I verify the offer page opened and price, rooms, surface match the saved context")]
        public void ThenIVerifyOfferPageOpenedAndDataMatches()
        {
            Logger.Info("Starting offer details verification");
            _verificationService.VerifyOfferDetailsMatch();
        }
    }
}