using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Property_and_Management.src.Interface
{
    public interface IDTO<T> where T : IEntity
    {
        /// <summary>
        /// Method for creating a DTO from a model
        /// </summary>
        /// <param name="model"></param>
        /// <returns>The newly created DTO</returns>
        abstract static IDTO<T> FromModel(T model);

        /// <summary>
        /// Converts the current instance to a model of type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>An object of type <typeparamref name="T"/> representing the model equivalent of the current instance.</returns>
        T ToModel();
    }
}
