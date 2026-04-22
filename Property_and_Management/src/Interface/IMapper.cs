namespace Property_and_Management.Src.Interface
{
    public interface IMapper<TDomainModel, TDTO>
        where TDomainModel : IEntity
        where TDTO : IDTO<TDomainModel>
    {
        TDTO ToDTO(TDomainModel sourceModel);

        TDomainModel ToModel(TDTO sourceDataTransferObject);
    }
}