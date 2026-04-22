using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Mapper
{
    public class NotificationMapper : IMapper<Notification, NotificationDTO>
    {
        private readonly IMapper<User, UserDTO> notificationRecipientUserMapper;

        public NotificationMapper(IMapper<User, UserDTO> notificationRecipientUserMapper)
        {
            this.notificationRecipientUserMapper = notificationRecipientUserMapper;
        }

        public NotificationDTO ToDTO(Notification notificationModel)
        {
            if (notificationModel == null)
            {
                return null;
            }

            return new NotificationDTO
            {
                Id = notificationModel.Id,
                User = notificationRecipientUserMapper.ToDTO(notificationModel.User),
                Timestamp = notificationModel.Timestamp,
                Title = notificationModel.Title,
                Body = notificationModel.Body,
                Type = notificationModel.Type,
                RelatedRequestId = notificationModel.RelatedRequestId
            };
        }

        public Notification ToModel(NotificationDTO notificationDto)
        {
            if (notificationDto == null)
            {
                return null;
            }

            return new Notification
            {
                Id = notificationDto.Id,
                User = notificationRecipientUserMapper.ToModel(notificationDto.User),
                Timestamp = notificationDto.Timestamp,
                Title = notificationDto.Title,
                Body = notificationDto.Body,
                Type = notificationDto.Type,
                RelatedRequestId = notificationDto.RelatedRequestId
            };
        }
    }
}