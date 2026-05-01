namespace DCMS.Domain.Entities;

public class Guest : User
{
    public string SessionId { get; set; } = string.Empty;
}
