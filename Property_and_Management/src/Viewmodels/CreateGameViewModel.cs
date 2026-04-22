using System;
using System.Collections.Generic;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Viewmodels
{
    public class CreateGameViewModel
    {
        private const int NoValidationErrors = 0;
        private const int NewGameId = 0;
        private const decimal ZeroPriceForEmptyOrInvalidInput = 0m;

        private readonly IGameService gameListingService;
        private readonly ICurrentUserContext currentUserContext;

        public string GameName { get; set; } = string.Empty;
        public decimal GamePrice { get; set; }
        public double GamePriceAsDouble
        {
            get => (double)GamePrice;
            set => GamePrice = (decimal)value;
        }
        public int MinimumPlayersRequired { get; set; } = Constants.GameValidation.DefaultMinimumPlayers;
        public int MaximumPlayersAllowed { get; set; } = Constants.GameValidation.DefaultMaximumPlayers;
        public string GameDescription { get; set; } = string.Empty;
        public bool IsGameActive { get; set; } = true;
        public byte[] GameImage { get; set; } = null;

        public int CurrentUserId => currentUserContext.CurrentUserId;

        public CreateGameViewModel(IGameService gameListingService, ICurrentUserContext currentUserContext)
        {
            this.gameListingService = gameListingService;
            this.currentUserContext = currentUserContext;
        }

        public List<string> ValidateGameInputs()
        {
            return GameInputHelper.BuildValidationErrors(
                GameName,
                GamePrice,
                MinimumPlayersRequired,
                MaximumPlayersAllowed,
                GameDescription,
                Constants.GameValidation.MinimumNameLength,
                Constants.GameValidation.MaximumNameLength,
                Constants.GameValidation.MinimumAllowedPrice,
                Constants.GameValidation.MinimumPlayerCount,
                Constants.GameValidation.MinimumDescriptionLength,
                Constants.GameValidation.MaximumDescriptionLength);
        }

        public ViewOperationResult SubmitCreateGame()
        {
            var gameValidationErrors = ValidateGameInputs();
            if (gameValidationErrors.Count > NoValidationErrors)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    string.Join(Environment.NewLine, gameValidationErrors));
            }

            SaveGame();
            return ViewOperationResult.Success();
        }

        public void SetGamePriceFromText(string rawPriceText)
        {
            if (PriceInputParser.TryParsePriceInput(rawPriceText, out var parsedPriceAsDouble))
            {
                GamePriceAsDouble = parsedPriceAsDouble;
                return;
            }

            GamePrice = ZeroPriceForEmptyOrInvalidInput;
        }

        public GameDTO SaveGame()
        {
            if (ValidateGameInputs().Count > NoValidationErrors)
            {
                return null;
            }

            GameImage = GameInputHelper.EnsureImageOrDefault(GameImage, AppDomain.CurrentDomain.BaseDirectory);

            var newGameDataTransferObject = new GameDTO
            {
                Id = NewGameId,
                Owner = new UserDTO { Id = CurrentUserId },
                Name = GameName,
                Price = GamePrice,
                MinimumPlayerNumber = MinimumPlayersRequired,
                MaximumPlayerNumber = MaximumPlayersAllowed,
                Description = GameDescription,
                Image = GameImage,
                IsActive = IsGameActive
            };

            gameListingService.AddGame(newGameDataTransferObject);
            return newGameDataTransferObject;
        }
    }
}