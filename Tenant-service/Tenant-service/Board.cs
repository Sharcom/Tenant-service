using DTO;

namespace Tenant_service
{
    public class Board
    {
        public Board(TenantDTO dto)
        {
            ID = dto.ID;
            Name = dto.Name;
            Description = dto.Description;
            Company = dto.Company;
            IsPublic = dto.IsPublic;
            Members = dto.Members ?? new List<string>();
            Administrators = dto.Administrators ?? new List<string>();
        }

        public int? ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Company { get; set; }
        public bool IsPublic { get; set; }
        public List<string> Members { get; set; }
        public List<string> Administrators { get; set; }


        public TenantDTO ToDTO()
        {
            return new TenantDTO()
            {
                ID = ID,
                Name = Name,
                Description = Description,
                Company = Company,
                IsPublic = IsPublic,   
                Members = Members,
                Administrators = Administrators
            };
        }
    }
}
