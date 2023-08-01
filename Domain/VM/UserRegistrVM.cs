namespace Domain.VM
{
    public class UserRegistrVM
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Ts { get; set; }    
    }
}