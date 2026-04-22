using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Viewmodels
{
    public class CreateRequestViewModel : INotifyPropertyChanged
    {
        private readonly IGameService gameListingService;
        private readonly IRequestService rentalRequestService;
        private readonly ICurrentUserContext currentUserContext;

        public int CurrentUserId => currentUserContext.CurrentUserId;

        public ObservableCollection<GameDTO> AvailableGamesToRequest { get; set; } = new();

        private GameDTO selectedGameToRequest;
        public GameDTO SelectedGame
        {
            get => selectedGameToRequest;
            set
            {
                selectedGameToRequest = value;
                OnPropertyChanged();
            }
        }

        private System.DateTimeOffset? requestedStartDate;
        public System.DateTimeOffset? StartDate
        {
            get => requestedStartDate;
            set
            {
                requestedStartDate = value;
                OnPropertyChanged();
            }
        }

        private System.DateTimeOffset? requestedEndDate;
        public System.DateTimeOffset? EndDate
        {
            get => requestedEndDate;
            set
            {
                requestedEndDate = value;
                OnPropertyChanged();
            }
        }

        public CreateRequestViewModel(IGameService gameListingService, IRequestService rentalRequestService,
                                      ICurrentUserContext currentUserContext)
        {
            this.gameListingService = gameListingService;
            this.rentalRequestService = rentalRequestService;
            this.currentUserContext = currentUserContext;
            LoadAvailableGames();
        }

        public void LoadAvailableGames()
        {
            AvailableGamesToRequest.Clear();
            var gamesOwnedByOtherUsers = gameListingService.GetAllGames()
                .Where(game => game.IsActive && game.Owner?.Id != CurrentUserId);
            foreach (var availableGame in gamesOwnedByOtherUsers)
            {
                AvailableGamesToRequest.Add(availableGame);
            }
        }

        public bool ValidateRequestInputs()
        {
            if (SelectedGame == null)
            {
                return false;
            }

            return DateRangeValidationHelper.HasValidFutureDateRange(StartDate, EndDate);
        }

        public ViewOperationResult SubmitRequest()
        {
            if (!ValidateRequestInputs())
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    Constants.DialogMessages.CreateRequestValidationError);
            }

            var requestCreationResult = rentalRequestService.CreateRequest(
                SelectedGame.Id,
                CurrentUserId,
                SelectedGame.Owner.Id,
                StartDate.Value.DateTime,
                EndDate.Value.DateTime);

            if (requestCreationResult.IsSuccess)
            {
                return ViewOperationResult.Success();
            }

            return ViewOperationResult.Failure(
                Constants.DialogTitles.RequestFailed,
                BuildCreateRequestErrorMessage(requestCreationResult.Error));
        }

        private static string BuildCreateRequestErrorMessage(CreateRequestError createRequestError)
        {
            return createRequestError switch
            {
                CreateRequestError.OwnerCannotRent => "You cannot rent your own game.",
                CreateRequestError.DatesUnavailable => "The selected dates are not available.",
                CreateRequestError.GameDoesNotExist => "The selected game no longer exists.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }

        public string? TrySubmitRequest()
        {
            var requestSubmissionResult = SubmitRequest();
            return requestSubmissionResult.IsSuccess ? null : requestSubmissionResult.DialogMessage;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}