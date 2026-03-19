using System;
using System.Collections.Generic;
using Property_and_Management.src.Interface;

namespace Property_and_Management.src.Model
{
    public class User : IEntity
    {
        public int Id { get; set; }

        public User() { }

        public User(int id)
        {
            Id = id;
        }
    }
}
