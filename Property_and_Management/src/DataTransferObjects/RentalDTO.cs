using System;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.DataTransferObjects
{
    public class RentalDTO : IDTO<Rental>
    {
        private const string ShortDateDisplayFormat = "dd/MM";
        private const string LongDateDisplayFormat = "dd/MM/yyyy";
        private const string StartDateLabelPrefix = "Start: ";
        private const string EndDateLabelPrefix = "End: ";

        public int Id { get; set; }
        public GameDTO Game { get; set; }
        public UserDTO Renter { get; set; }
        public UserDTO Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string StartDateDisplay => StartDate.ToString(ShortDateDisplayFormat);
        public string EndDateDisplay => EndDate.ToString(ShortDateDisplayFormat);
        public string StartDateDisplayLong => $"{StartDateLabelPrefix}{StartDate.ToString(LongDateDisplayFormat)}";
        public string EndDateDisplayLong => $"{EndDateLabelPrefix}{EndDate.ToString(LongDateDisplayFormat)}";
        public bool IsExpired => EndDate < DateTime.UtcNow;

        public RentalDTO()
        {
        }
    }
}