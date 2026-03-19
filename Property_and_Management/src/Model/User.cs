using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property_and_Management.src.Attributes;
using Property_and_Management.src.Interface;

namespace Property_and_Management.src.Model
{
    [SqlTableDefinition("Users")]
    public class User : IEntity
    {
        [SqlTableFieldDefinition("id", IsPrimaryKey = true)]
        public int Id { get; set; }

        public User(int id)
        {
            Id = id;
        }

        public static IEntity BuildFromParameters(Dictionary<string, object> parameters)
        {
            return new User(id: (int)parameters["id"]);
        }
    }
}
