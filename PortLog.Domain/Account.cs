
namespace PortLog.Domain;
class Account
{
    public enum AccountStatus
    {
        ONLINE,
        OFFLINE,
        IDLE,
        DO_NOT_DISTURB
    }
    public enum Role
    {
        MANAGER,
        CAPTAIN
    }

    private Guid _id;
    private string _name;
    private string _email;
    private string _password;
    private Role _role;
    private AccountStatus _status;
    private string _company;
    private DateTime _createdAt;
    private DateTime _lastUpdated;
    private DateTime _lastLogin;

    public Guid Id
    {
        get => _id;
    }
    public string Name
    {
        get => _name;
    }
    public string Email
    {
        get => _email;
    }
    private string Password
    {
        get => _password;
        set => _password = value;
    }
    public Role AccountRole
    {
        get => _role;
        set => _role = value;
    }
    public AccountStatus Status
    {
        get => _status;
        set => _status = value;
    }
    public string Company
    {
        get => _company;
    }
    public DateTime CreatedAt
    {
        get => _createdAt;
    }
    public DateTime LastUpdated
    {
        get => _lastUpdated;
    }
    public DateTime LastLogin
    {
        get => _lastLogin;
    }

    public void Register() { }
    public Boolean Login(string email, string password) { return false; }
    public void Logout() { }
    public void UpdateProfile(Account data) { }
    public void AssignToShip(Account account, Ship ship) { }
    public Ship[] GetAssignedShip(Guid accountId) { return []; }
    public void RemoveAssignment(Guid accountId, Guid shipId) { }
}