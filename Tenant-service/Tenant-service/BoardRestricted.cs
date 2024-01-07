using DTO;

namespace Tenant_service
{
    public class BoardRestricted
    {
        public BoardRestricted() { }
        public BoardRestricted(int? id, string name, string description, string company, bool isPublic = false) 
        {
            ID = id;
            Name = name;
            Description = description;
            Company = company;
            IsPublic = isPublic;
        }
        public BoardRestricted(TenantDTO dto)
        {
            ID = dto.ID;
            Name = dto.Name;
            Description = dto.Description;
            Company = dto.Company;
            IsPublic = dto.IsPublic;
        }

        public int? ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Company { get; set; }
        public bool IsPublic { get; set; }


        public TenantDTO ToDTO()
        {
            return new TenantDTO()
            {
                ID = ID,
                Name = Name,
                Description = Description,
                Company = Company,
                IsPublic = IsPublic,                
            };
        }
    }
}
