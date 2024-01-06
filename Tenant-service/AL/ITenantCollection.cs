using DTO;

namespace AL
{
    public interface ITenantCollection
    {
        public TenantDTO Create(TenantDTO dto);
    }
}