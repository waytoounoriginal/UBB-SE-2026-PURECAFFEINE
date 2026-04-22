using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Model
{
    public class User : IEntity
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public User()
        {
        }

        public User(int id)
        {
            this.Id = id;
        }

        public User(int id, string displayName)
        {
            this.Id = id;
            DisplayName = displayName;
        }
    }
}