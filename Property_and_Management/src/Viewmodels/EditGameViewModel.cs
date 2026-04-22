using System;
using System.Collections.Generic;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Viewmodels
{
    public class EditGameViewModel
    {
        private const int MissingOwnerId = 0;
        private const int NoValidationErrors = 0;
        private const decimal ZeroPriceForEmptyOrInvalidInput = 0m;

        private readonly IGameService gameListingService;

        public int EditedGameId { get; private set; }
        public int EditedGameOwnerId { get; private set; }

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

        public EditGameViewModel(IGameService gameListingService)
        {
            this.gameListingService = gameListingService;
        }

        public void LoadGame(int gameIdToLoad)
        {
            var loadedGame = gameListingService.GetGameByIdentifier(gameIdToLoad);
            if (loadedGame == null)
            {
                return;
            }

            EditedGameId = loadedGame.Id;
            EditedGameOwnerId = loadedGame.Owner?.Id ?? MissingOwnerId;

            GameName = loadedGame.Name;
            GamePrice = loadedGame.Price;
            MinimumPlayersRequired = loadedGame.MinimumPlayerNumber;
            MaximumPlayersAllowed = loadedGame.MaximumPlayerNumber;
            GameDescription = loadedGame.Description;
            IsGameActive = loadedGame.IsActive;
            GameImage = loadedGame.Image;
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

        public ViewOperationResult SubmitGameUpdate()
        {
            var gameValidationErrors = ValidateGameInputs();
            if (gameValidationErrors.Count > NoValidationErrors)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    string.Join(Environment.NewLine, gameValidationErrors));
            }

            UpdateGame();
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

        public GameDTO UpdateGame()
        {
            if (ValidateGameInputs().Count > NoValidationErrors)
            {
                return null;
            }

            GameImage = GameInputHelper.EnsureImageOrDefault(GameImage, AppDomain.CurrentDomain.BaseDirectory);

            var updatedGameDataTransferObject = new GameDTO
            {
                Id = EditedGameId,
                Owner = new UserDTO { Id = EditedGameOwnerId },
                Name = GameName,
                Price = GamePrice,
                MinimumPlayerNumber = MinimumPlayersRequired,
                MaximumPlayerNumber = MaximumPlayersAllowed,
                Description = GameDescription,
                Image = GameImage,
                IsActive = IsGameActive
            };

            gameListingService.UpdateGameByIdentifier(EditedGameId, updatedGameDataTransferObject);
            return updatedGameDataTransferObject;
        }
    }
}