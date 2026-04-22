using System.Collections.Generic;
using System.Collections.Immutable;

namespace Property_and_Management.Src.Interface
{
    public interface IRepository<TRepositoryItem>
        where TRepositoryItem : notnull, IEntity
    {
        ImmutableList<TRepositoryItem> GetAll();

        void Add(TRepositoryItem itemToAdd);

        TRepositoryItem Delete(int itemIdentifierToRemove);

        void Update(int itemIdentifierToUpdate, TRepositoryItem replacementItem);

        TRepositoryItem Get(int itemIdentifier);
    }
}