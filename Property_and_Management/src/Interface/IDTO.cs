namespace Property_and_Management.Src.Interface
{
    public interface IDTO<TDomainModel>
        where TDomainModel : IEntity
    {
        int Id { get; set; }
    }
}