using System.Collections.Immutable;
using System.Linq;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Viewmodels
{
    public class RentalsFromOthersViewModel : PagedViewModel<RentalDTO>
    {
        private readonly IRentalService rentalLookupService;
        private readonly ICurrentUserContext currentUserContext;

        public int CurrentRenterUserId { get; private set; }

        public RentalsFromOthersViewModel(IRentalService rentalLookupService, ICurrentUserContext currentUserContext)
        {
            this.rentalLookupService = rentalLookupService;
            this.currentUserContext = currentUserContext;
            Reload();
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} rentals";

        public void LoadRentals() => Reload();

        protected override void Reload()
        {
            CurrentRenterUserId = currentUserContext.CurrentUserId;
            var currentUserRentalsSortedByNewest = rentalLookupService
                .GetRentalsForRenter(CurrentRenterUserId)
                .OrderByDescending(rental => rental.StartDate)
                .ToImmutableList();
            SetAllItems(currentUserRentalsSortedByNewest);
        }
    }
}