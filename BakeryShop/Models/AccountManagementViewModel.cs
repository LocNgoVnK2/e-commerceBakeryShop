namespace BakeryShop.Models
{
    public class AccountManagementViewModel
    {
        public int? AccountID { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? EmployeeID { get; set; }
        public string? FirstName { get; set;}
        public string? LastName { get; set; }
        public string? Position { get; set;}
        public string? PhoneNumber { get; set; }
        public bool? IsActivate { get; set;}
        public int? IdStore { get; set; }
        public string? StoreName { get; set; }
        public IEnumerable<StoreViewModel>? Stores { get; set; }
    }
}
