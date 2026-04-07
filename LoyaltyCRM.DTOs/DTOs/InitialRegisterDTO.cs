public class InitialRegisterDTO
{
    public UserNameRegisterDTO Papa { get; set; } = new();

    public UserNameRegisterDTO Bartender { get; set; } = new();

    public class UserNameRegisterDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}