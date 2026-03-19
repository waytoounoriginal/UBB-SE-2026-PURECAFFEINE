using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.Interface
{
    public interface IRepository<T> where T : notnull, IEntity
    {
        /// <summary>
        /// Retrieves all items in the collection as an immutable list.
        /// </summary>
        /// <returns>An <see cref="ImmutableList{T}"/> of all items; empty if none exist.</returns>
        ImmutableList<T> GetAll();

        /// <summary>
        /// Adds the specified entity to the collection.
        /// </summary>
        /// <param name="newEntity">The entity to add. Cannot be null.</param>
        void Add(T newEntity);

        /// <summary>
        /// Removes and returns the entity with the specified identifier from the repository.
        /// </summary>
        /// <param name="removedEntityId">The identifier of the entity to remove.</param>
        /// <returns>The removed entity instance.</returns>
        T Delete(int removedEntityId);

        /// <summary>
        /// Replaces the entity with the specified identifier with the provided new entity.
        /// </summary>
        /// <param name="updatedEntityId">The identifier of the entity to update.</param>
        /// <param name="newEntity">The new entity data that will replace the existing entity.</param>
        void Update(int updatedEntityId, T newEntity);

        /// <summary>
        /// Retrieves the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve.</param>
        /// <returns>The entity matching the specified <paramref name="id"/>.</returns>
        /// <remarks>
        /// If no entity with the given id exists, implementations may throw <see cref="KeyNotFoundException"/>.
        /// </remarks>
        T Get(int id);
    }
}
