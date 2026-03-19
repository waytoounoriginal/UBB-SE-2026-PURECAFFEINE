using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.DTO
{
    public class RequestDTO
    {
        private int _requestId;
        private int _gameId;
        private int _renterId;
        private int _ownerId;
        private DateTime _startDate;
        private DateTime _endDate;

        public RequestDTO(int requestId, int gameId, int renterId, int ownerId, DateTime startDate, DateTime endDate)

        {
            _requestId = requestId;
            _gameId = gameId;
            _renterId = renterId;
            _ownerId = ownerId;
            _startDate = startDate;
            _endDate = endDate;
        }

        public int GetRequestId() => _requestId;
        public int GetGameId() => _gameId;
        public int GetRenterId() => _renterId;
        public int GetOwnerId() => _ownerId;
        public DateTime GetStartDate() => _startDate;
        public DateTime GetEndDate() => _endDate;
    }
}
