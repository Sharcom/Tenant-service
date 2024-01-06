namespace DTO
{
    public class TenantDTO
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; } = false;
        public List<string>? Members { get; set; }
        public List<string>? Administrators { get; set; }
    }
}