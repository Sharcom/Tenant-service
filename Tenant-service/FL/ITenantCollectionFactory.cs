using AL;
using DAL;
using DTO;
using Microsoft.EntityFrameworkCore;

namespace FL
{
    public static class ITenantCollectionFactory
    {
        public static ITenantCollection Get(DbContext context)
        {
            return new TenantDAL(context);
        }
    }
}