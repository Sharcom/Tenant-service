using DTO;
using System.Security.Cryptography.X509Certificates;

namespace DAL.Model
{
    public class Tenant
    {
        public Tenant() { }
        public Tenant(TenantDTO dto)
        {
            ID = dto.ID;
            Name = dto.Name;
            Company= dto.Company;
            IsPublic = dto.IsPublic;
            Members = dto.Members ?? new List<string>();
            Administrators = dto.Administrators ?? new List<string>();
            Description = dto.Description;
        }
        // Primary key
        public int? ID { get; set; }

        // Properties
        public string Name { get; set; }
        public string Company { get; set; }
        public bool IsPublic { get; set; }
        public string Description { get; set; }

        // External properties
        public List<string> Members { get; set; }
        public List<string> Administrators { get; set; }

        // Foreign keys

        // Navigational properties

        // Methods
        public TenantDTO ToDTO()
        {
            return new TenantDTO()
            {
                ID = ID,
                Name = Name,
                Company = Company,
                IsPublic = IsPublic,
                Members = Members.ToList(),
                Administrators = Administrators.ToList(),
                Description = Description
            };
        }
    }
}
