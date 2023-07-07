using Entities.Intertfaces;

namespace Entities.DomainObjects
{
    public class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }
    }
}
