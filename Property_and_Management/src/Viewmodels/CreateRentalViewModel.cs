using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Viewmodels
{
    public class CreateRentalViewModel : INotifyPropertyChanged
    {
        private const string ValidationFailedMessage = "Validation failed.";

        private readonly IGameService gameListingService;
        private readonly IRentalService rentalCreationService;
        private readonly IUserService userLookupService;
        private readonly ICurrentUserContext currentUserContext;

        public int CurrentUserId => currentUserContext.CurrentUserId;

        public ObservableCollection<GameDTO> OwnedActiveGames { get; set; } = new();
        public ObservableCollection<UserDTO> AvailableRenters { get; set; } = new();

        private GameDTO selectedGameToRent;
        public GameDTO SelectedGameToRent
        {
            get => selectedGameToRent;
            set
            {
                selectedGameToRent = value;
                OnPropertyChanged();
            }
        }

        private UserDTO selectedRenterUser;
        public UserDTO SelectedRenter
        {
            get => selectedRenterUser;
            set
            {
                selectedRenterUser = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset? rentalStartDate;
        public DateTimeOffset? StartDate
        {
            get => rentalStartDate;
            set
            {
                rentalStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset? rentalEndDate;
        public DateTimeOffset? EndDate
        {
            get => rentalEndDate;
            set
            {
                rentalEndDate = value;
                OnPropertyChanged();
            }
        }

        public CreateRentalViewModel(IGameService gameListingService, IRentalService rentalCreationService,
                                     IUserService userLookupService, ICurrentUserContext currentUserContext)
        {
            this.gameListingService = gameListingService;
            this.rentalCreationService = rentalCreationService;
            this.userLookupService = userLookupService;
            this.currentUserContext = currentUserContext;
            LoadRentalFormData();
        }

        public void LoadRentalFormData()
        {
            OwnedActiveGames.Clear();
            foreach (var ownedGame in gameListingService.GetGamesForOwner(CurrentUserId))
            {
                if (ownedGame.IsActive)
                {
                    OwnedActiveGames.Add(ownedGame);
                }
            }

            AvailableRenters.Clear();
            foreach (var potentialRenter in userLookupService.GetUsersExcept(CurrentUserId))
            {
                AvailableRenters.Add(potentialRenter);
            }
        }

        public bool ValidateRentalInputs()
        {
            if (SelectedGameToRent == null)
            {
                return false;
            }

            if (SelectedRenter == null)
            {
                return false;
            }

            return DateRangeValidationHelper.HasValidFutureDateRange(StartDate, EndDate);
        }

        public ViewOperationResult CreateRental()
        {
            if (!ValidateRentalInputs())
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    Constants.DialogMessages.CreateRentalValidationError);
            }

            try
            {
                rentalCreationService.CreateConfirmedRental(
                    SelectedGameToRent.Id,
                    SelectedRenter.Id,
                    CurrentUserId,
                    StartDate.Value.DateTime,
                    EndDate.Value.DateTime);
                return ViewOperationResult.Success();
            }
            catch (Exception rentalCreationException)
            {
                return ViewOperationResult.Failure(Constants.DialogTitles.RentalFailed, rentalCreationException.Message);
            }
        }

        public string? SaveRental()
        {
            var rentalCreationResult = CreateRental();
            if (rentalCreationResult.IsSuccess)
            {
                return null;
            }

            if (rentalCreationResult.DialogTitle == Constants.DialogTitles.ValidationError)
            {
                return ValidationFailedMessage;
            }

            return rentalCreationResult.DialogMessage;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}