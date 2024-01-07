using AL;
using DAL.Model;
using DTO;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class TenantDAL : ITenantCollection
    {
        private readonly TenantContext _context;

        public TenantDAL(DbContext context)
        {
            _context = context as TenantContext ?? throw new ArgumentNullException(nameof(context));
        }

        public TenantDTO? Create(TenantDTO dto)
        {
            if (_context.Tenants.Where(x => x.Company == dto.Company).FirstOrDefault(x => x.Name == dto.Name) != null)
                return null;

            Tenant _tenant = new Tenant(dto);

            
            _context.Tenants.Add(_tenant);
            _context.SaveChanges();
            
            return _tenant.ToDTO();
        }
    }
}
