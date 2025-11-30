using Xunit;
using PortLog.Services;
using PortLog.Models;
using PortLog.Enumerations;
using PortLog.Supabase;
using PortLog.Helpers;
using DotNetEnv;
using Supabase;

namespace PortLog.Testing.Services
{
    /// <summary>
    /// Integration tests for AccountService that interact with a real Supabase development database.
    /// These tests will create, read, update, and delete real data.
    /// </summary>
    public class AccountServiceIntegrationTests : IAsyncLifetime
    {
        private SupabaseService _supabaseService;
        private AccountService _accountService;
        private CompanyService _companyService;
        private readonly List<Guid> _testAccountIds = new();
        private readonly List<Guid> _testCompanyIds = new();

        public async Task InitializeAsync()
        {
            // Load environment variables
            Env.Load();

            string url = Env.GetString("SUPABASE_URL") ?? throw new Exception("SUPABASE_URL not found in .env");
            string key = Env.GetString("SUPABASE_KEY") ?? throw new Exception("SUPABASE_KEY not found in .env");

            // Initialize Supabase client
            var client = new Client(url, key, new SupabaseOptions
            {
                AutoConnectRealtime = false
            });
            await client.InitializeAsync();

            _supabaseService = new SupabaseService(client);
            _accountService = new AccountService(_supabaseService);
        }

        public async Task DisposeAsync()
        {
            // Cleanup: Delete all test accounts created during tests
            foreach (var accountId in _testAccountIds)
            {
                try
                {
                    var account = await _supabaseService
                        .Table<Account>()
                        .Where(a => a.Id == accountId)
                        .Single();

                    if (account != null)
                    {
                        await _supabaseService
                            .Table<Account>()
                            .Delete(account);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            // Cleanup: Delete all test companies
            foreach (var companyId in _testCompanyIds)
            {
                try
                {
                    var company = await _supabaseService
                        .Table<Company>()
                        .Where(c => c.Id == companyId)
                        .Single();

                    if (company != null)
                    {
                        await _supabaseService
                            .Table<Company>()
                            .Delete(company);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region Helper Methods

        private string GenerateUniqueEmail()
        {
            return $"test_{Guid.NewGuid().ToString().Substring(0, 8)}@test.com";
        }

        private async Task<Account> CreateTestAccountDirectly(string email, string password, AccountRole role, Guid? companyId = null)
        {
            var hashedPassword = PasswordHasher.Hash(password);
            var account = new Account
            {
                Email = email,
                Password = hashedPassword,
                Name = "Test User",
                Role = role.ToString(),
                CompanyId = companyId,
                Contact = "1234567890",
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            var result = await _supabaseService
                .Table<Account>()
                .Insert(account);

            var createdAccount = result.Models.First();
            _testAccountIds.Add(createdAccount.Id);
            return createdAccount;
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithEmptyEmail_ReturnsFailure()
        {
            // Act
            var result = await _accountService.LoginAsync("", "password123");

            // Assert
            Assert.False(result.success);
            Assert.Equal("Email atau password kosong!", result.error);
        }

        [Fact]
        public async Task LoginAsync_WithEmptyPassword_ReturnsFailure()
        {
            // Act
            var result = await _accountService.LoginAsync("test@example.com", "");

            // Assert
            Assert.False(result.success);
            Assert.Equal("Email atau password kosong!", result.error);
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentEmail_ReturnsFailure()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "password123";

            // Act
            var result = await _accountService.LoginAsync(email, password);

            // Assert
            Assert.False(result.success);
            Assert.Equal("Email atau password salah!", result.error);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var correctPassword = "correctpassword123";
            var wrongPassword = "wrongpassword123";

            await CreateTestAccountDirectly(email, correctPassword, AccountRole.CAPTAIN);

            // Act
            var result = await _accountService.LoginAsync(email, wrongPassword);

            // Assert
            Assert.False(result.success);
            Assert.Equal("Email atau password salah!", result.error);
        }

        [Fact]
        public async Task LoginAsync_WithCorrectCredentials_ReturnsSuccess()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "password123";

            await CreateTestAccountDirectly(email, password, AccountRole.CAPTAIN);

            // Act
            var result = await _accountService.LoginAsync(email, password);

            // Assert
            Assert.True(result.success);
            Assert.Empty(result.error);
            Assert.NotNull(_accountService.LoggedInAccount);
            Assert.Equal(email, _accountService.LoggedInAccount.Email);
        }

        [Fact]
        public async Task LoginAsync_SetsLoggedInAccountProperty()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "password123";
            var name = "Test Captain";

            var account = await CreateTestAccountDirectly(email, password, AccountRole.CAPTAIN);
            account.Name = name;
            await _supabaseService.Table<Account>().Update(account);

            // Act
            var result = await _accountService.LoginAsync(email, password);

            // Assert
            Assert.True(result.success);
            Assert.NotNull(_accountService.LoggedInAccount);
            Assert.Equal(email, _accountService.LoggedInAccount.Email);
            Assert.Equal(name, _accountService.LoggedInAccount.Name);
            Assert.Equal("CAPTAIN", _accountService.LoggedInAccount.Role);
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WithEmptyPassword_ReturnsFailure()
        {
            // Act
            var result = await _accountService.RegisterAsync(
                "Test User",
                GenerateUniqueEmail(),
                AccountRole.CAPTAIN,
                ""
            );

            // Assert
            Assert.False(result.success);
            Assert.Equal("Password kosong!", result.error);
        }

        [Fact]
        public async Task RegisterAsync_WithShortPassword_ReturnsFailure()
        {
            // Act
            var result = await _accountService.RegisterAsync(
                "Test User",
                GenerateUniqueEmail(),
                AccountRole.CAPTAIN,
                "short"
            );

            // Assert
            Assert.False(result.success);
            Assert.Equal("Password minimal 8 karakter!", result.error);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            await CreateTestAccountDirectly(email, "password123", AccountRole.CAPTAIN);

            // Act
            var result = await _accountService.RegisterAsync(
                "Another User",
                email,
                AccountRole.MANAGER,
                "password456"
            );

            // Assert
            Assert.False(result.success);
            Assert.Equal("Email telah dipakai!", result.error);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_CreatesAccountInDatabase()
        {
            // Arrange
            var name = "New User";
            var email = GenerateUniqueEmail();
            var password = "password123";
            var role = AccountRole.CAPTAIN;

            // Act
            var result = await _accountService.RegisterAsync(name, email, role, password);

            // Assert
            Assert.True(result.success);
            Assert.Empty(result.error);
            Assert.NotNull(_accountService.LoggedInAccount);

            // Track for cleanup
            _testAccountIds.Add(_accountService.LoggedInAccount.Id);

            // Verify in database
            var dbAccount = await _supabaseService
                .Table<Account>()
                .Where(a => a.Email == email)
                .Single();

            Assert.NotNull(dbAccount);
            Assert.Equal(email, dbAccount.Email);
            Assert.Equal(name, dbAccount.Name);
            Assert.Equal("CAPTAIN", dbAccount.Role);
            Assert.NotEqual(password, dbAccount.Password); // Password should be hashed
        }

        [Fact]
        public async Task RegisterAsync_HashesPasswordCorrectly()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "plainpassword123";

            // Act
            var result = await _accountService.RegisterAsync(
                "Test User",
                email,
                AccountRole.CAPTAIN,
                password
            );

            // Assert
            Assert.True(result.success);
            _testAccountIds.Add(_accountService.LoggedInAccount.Id);

            // Verify password is hashed (not plain text)
            var dbAccount = await _supabaseService
                .Table<Account>()
                .Where(a => a.Email == email)
                .Single();

            Assert.NotEqual(password, dbAccount.Password);
            Assert.True(PasswordHasher.Verify(password, dbAccount.Password));
        }

        [Fact]
        public async Task RegisterAsync_SetsLoggedInAccount()
        {
            // Arrange
            var name = "Test Manager";
            var email = GenerateUniqueEmail();
            var password = "password123";

            // Act
            var result = await _accountService.RegisterAsync(name, email, AccountRole.MANAGER, password);

            // Assert
            Assert.True(result.success);
            Assert.NotNull(_accountService.LoggedInAccount);
            Assert.Equal(email, _accountService.LoggedInAccount.Email);
            Assert.Equal(name, _accountService.LoggedInAccount.Name);
            Assert.Equal("MANAGER", _accountService.LoggedInAccount.Role);

            _testAccountIds.Add(_accountService.LoggedInAccount.Id);
        }

        [Fact]
        public async Task RegisterAsync_SetsCreatedAtAndLastUpdated()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var beforeRegistration = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var result = await _accountService.RegisterAsync(
                "Test User",
                email,
                AccountRole.CAPTAIN,
                "password123"
            );

            // Assert
            Assert.True(result.success);
            _testAccountIds.Add(_accountService.LoggedInAccount.Id);

            var afterRegistration = DateTime.UtcNow.AddSeconds(1);

            Assert.True(_accountService.LoggedInAccount.CreatedAt >= beforeRegistration);
            Assert.True(_accountService.LoggedInAccount.CreatedAt <= afterRegistration);
            Assert.True(_accountService.LoggedInAccount.LastUpdated >= beforeRegistration);
            Assert.True(_accountService.LoggedInAccount.LastUpdated <= afterRegistration);
        }

        #endregion

        #region UpdateUserCompanyAsync Tests

        [Fact]
        public async Task UpdateUserCompanyAsync_WithNoLoggedInUser_ReturnsFalse()
        {
            // Arrange
            _accountService.Logout();
            var companyId = Guid.NewGuid();

            // Act
            var result = await _accountService.UpdateUserCompanyAsync(companyId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateUserCompanyAsync_WithLoggedInUser_UpdatesDatabase()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "password123";
            var (company, err) = await _companyService.CreateCompanyAsync("Test Company 1", "123 Test St", "Test Province 1");

            var account = await CreateTestAccountDirectly(email, password, AccountRole.CAPTAIN);
            await _accountService.LoginAsync(email, password);

            // Act
            var result = await _accountService.UpdateUserCompanyAsync(company.Id);

            // Assert
            Assert.True(result);
            Assert.Equal(company.Id, _accountService.LoggedInAccount.CompanyId);

            // Verify in database
            var dbAccount = await _supabaseService
                .Table<Account>()
                .Where(a => a.Id == account.Id)
                .Single();

            Assert.Equal(company.Id, dbAccount.CompanyId);
        }

        [Fact]
        public async Task UpdateUserCompanyAsync_UpdatesLoggedInAccountProperty()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "password123";
            var (company, err) = await _companyService.CreateCompanyAsync("Test Company 2", "Test Street 987", "Test Province 2");

            await CreateTestAccountDirectly(email, password, AccountRole.CAPTAIN);
            await _accountService.LoginAsync(email, password);

            Assert.Null(_accountService.LoggedInAccount.CompanyId);

            // Act
            var result = await _accountService.UpdateUserCompanyAsync(company.Id);

            // Assert
            Assert.True(result);
            Assert.Equal(company.Id, _accountService.LoggedInAccount.CompanyId);
        }

        #endregion

        #region GetCaptainByIdAsync Tests

        [Fact]
        public async Task GetCaptainByIdAsync_WithValidId_ReturnsAccount()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var account = await CreateTestAccountDirectly(email, "password123", AccountRole.CAPTAIN);

            // Act
            var result = await _accountService.GetCaptainByIdAsync(account.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(account.Id, result.Id);
            Assert.Equal(email, result.Email);
            Assert.Equal("CAPTAIN", result.Role);
        }

        [Fact]
        public async Task GetCaptainByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _accountService.GetCaptainByIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCaptainByIdAsync_ReturnsManagerToo()
        {
            // Note: Method name is GetCaptainById but it actually returns any account by ID
            // Arrange
            var email = GenerateUniqueEmail();
            var account = await CreateTestAccountDirectly(email, "password123", AccountRole.MANAGER);

            // Act
            var result = await _accountService.GetCaptainByIdAsync(account.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(account.Id, result.Id);
            Assert.Equal("MANAGER", result.Role);
        }

        #endregion

        #region GetAccountsByCompanyIdAsync Tests

        [Fact]
        public async Task GetAccountsByCompanyIdAsync_ReturnsAllAccountsForCompany()
        {
            // Arrange
            var (company, err) = await _companyService.CreateCompanyAsync("Test Company 3", "Test Street 876", "Test Province 3");
            var account1 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password123", AccountRole.CAPTAIN, company.Id);
            var account2 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password456", AccountRole.MANAGER, company.Id);

            // Act
            var result = await _accountService.GetAccountsByCompanyIdAsync(company.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.Contains(result, a => a.Id == account1.Id);
            Assert.Contains(result, a => a.Id == account2.Id);
        }

        [Fact]
        public async Task GetAccountsByCompanyIdAsync_WithNoAccounts_ReturnsEmptyList()
        {
            // Arrange
            var (company, err) = await _companyService.CreateCompanyAsync("Test Company 4", "Test Street 765", "Test Province 4");

            // Act
            var result = await _accountService.GetAccountsByCompanyIdAsync(company.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAccountsByCompanyIdAsync_DoesNotReturnAccountsFromOtherCompanies()
        {
            // Arrange
            var (company1, err1) = await _companyService.CreateCompanyAsync("Test Company 5", "Test Street 654", "Test Province 5");
            var (company2, err2) = await _companyService.CreateCompanyAsync("Test Company 6", "Test Street 543", "Test Province 6");
            var account1 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password123", AccountRole.CAPTAIN, company1.Id);
            var account2 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password456", AccountRole.CAPTAIN, company2.Id);

            // Act
            var result = await _accountService.GetAccountsByCompanyIdAsync(company1.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, a => a.Id == account1.Id);
            Assert.DoesNotContain(result, a => a.Id == account2.Id);
        }

        #endregion

        #region GetCaptainsByCompanyIdAsync Tests

        [Fact]
        public async Task GetCaptainsByCompanyIdAsync_ReturnsOnlyCaptains()
        {
            // Arrange
            var (company, err) = await _companyService.CreateCompanyAsync("Test Company 7", "Test Street 432", "Test Province 7");
            var captain1 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password123", AccountRole.CAPTAIN, company.Id);
            var captain2 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password456", AccountRole.CAPTAIN, company.Id);
            var manager = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password789", AccountRole.MANAGER, company.Id);

            // Act
            var result = await _accountService.GetCaptainsByCompanyIdAsync(company.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.Contains(result, a => a.Id == captain1.Id);
            Assert.Contains(result, a => a.Id == captain2.Id);
            Assert.DoesNotContain(result, a => a.Id == manager.Id);
            Assert.All(result, account => Assert.Equal("CAPTAIN", account.Role));
        }

        [Fact]
        public async Task GetCaptainsByCompanyIdAsync_WithNoCaptains_ReturnsEmptyList()
        {
            // Arrange
            var (company, err) = await _companyService.CreateCompanyAsync("Test Company 8", "Test Street 111", "Test Province 8");
            await CreateTestAccountDirectly(GenerateUniqueEmail(), "password123", AccountRole.MANAGER, company.Id);

            // Act
            var result = await _accountService.GetCaptainsByCompanyIdAsync(company.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetAccountsWithoutCompanyAsync Tests

        [Fact]
        public async Task GetAccountsWithoutCompanyAsync_ReturnsAccountsWithNullCompanyId()
        {
            // Arrange
            var account1 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password123", AccountRole.CAPTAIN, null);
            var account2 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password456", AccountRole.MANAGER, null);

            // Act
            var result = await _accountService.GetAccountsWithoutCompanyAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
            Assert.Contains(result, a => a.Id == account1.Id);
            Assert.Contains(result, a => a.Id == account2.Id);
            Assert.All(result, account => Assert.Null(account.CompanyId));
        }

        [Fact]
        public async Task GetAccountsWithoutCompanyAsync_DoesNotReturnAccountsWithCompany()
        {
            // Arrange
            var (company, err) = await _companyService.CreateCompanyAsync("Test Company 9", "Test Street 222", "Test Province 9");
            var accountWithoutCompany = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password123", AccountRole.CAPTAIN, null);
            var accountWithCompany = await CreateTestAccountDirectly(GenerateUniqueEmail(), "password456", AccountRole.CAPTAIN, company.Id);

            // Act
            var result = await _accountService.GetAccountsWithoutCompanyAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, a => a.Id == accountWithoutCompany.Id);
            Assert.DoesNotContain(result, a => a.Id == accountWithCompany.Id);
        }

        #endregion

        #region Logout Tests

        [Fact]
        public async Task Logout_ClearsLoggedInAccount()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "password123";
            await CreateTestAccountDirectly(email, password, AccountRole.CAPTAIN);
            await _accountService.LoginAsync(email, password);

            Assert.NotNull(_accountService.LoggedInAccount);

            // Act
            _accountService.Logout();

            // Assert
            Assert.Null(_accountService.LoggedInAccount);
        }

        [Fact]
        public void Logout_WhenNoUserLoggedIn_DoesNotThrow()
        {
            // Arrange
            _accountService.Logout();

            // Act & Assert
            _accountService.Logout();
            Assert.Null(_accountService.LoggedInAccount);
        }

        #endregion

        #region End-to-End Workflow Tests

        [Fact]
        public async Task CompleteUserJourney_RegisterLoginUpdateLogout()
        {
            // Step 1: Register
            var name = "Journey User";
            var email = GenerateUniqueEmail();
            var password = "journey123";

            var registerResult = await _accountService.RegisterAsync(name, email, AccountRole.CAPTAIN, password);
            Assert.True(registerResult.success);
            Assert.NotNull(_accountService.LoggedInAccount);
            var accountId = _accountService.LoggedInAccount.Id;
            _testAccountIds.Add(accountId);

            // Step 2: Logout
            _accountService.Logout();
            Assert.Null(_accountService.LoggedInAccount);

            // Step 3: Login
            var loginResult = await _accountService.LoginAsync(email, password);
            Assert.True(loginResult.success);
            Assert.NotNull(_accountService.LoggedInAccount);
            Assert.Equal(accountId, _accountService.LoggedInAccount.Id);

            // Step 4: Update company
            var (company, err) = await _companyService.CreateCompanyAsync("Test Company 10", "Test Street 313", "Test Province 10");
            var updateResult = await _accountService.UpdateUserCompanyAsync(company.Id);
            Assert.True(updateResult);
            Assert.Equal(company.Id, _accountService.LoggedInAccount.CompanyId);

            // Step 5: Verify in database
            var dbAccount = await _supabaseService
                .Table<Account>()
                .Where(a => a.Id == accountId)
                .Single();
            Assert.Equal(company.Id, dbAccount.CompanyId);

            // Step 6: Final logout
            _accountService.Logout();
            Assert.Null(_accountService.LoggedInAccount);
        }

        [Fact]
        public async Task MultipleUsersInSameCompany_CanBeRetrieved()
        {
            // Arrange
            var (company, err) = await _companyService.CreateCompanyAsync("Test Company 11", "Test Street 333", "Test Province 11");

            var captain1 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "pass123", AccountRole.CAPTAIN, company.Id);
            var captain2 = await CreateTestAccountDirectly(GenerateUniqueEmail(), "pass456", AccountRole.CAPTAIN, company.Id);
            var manager = await CreateTestAccountDirectly(GenerateUniqueEmail(), "pass789", AccountRole.MANAGER, company.Id);

            // Act
            var allAccounts = await _accountService.GetAccountsByCompanyIdAsync(company.Id);
            var captainsOnly = await _accountService.GetCaptainsByCompanyIdAsync(company.Id);

            // Assert
            Assert.True(allAccounts.Count >= 3);
            Assert.Contains(allAccounts, a => a.Id == captain1.Id);
            Assert.Contains(allAccounts, a => a.Id == captain2.Id);
            Assert.Contains(allAccounts, a => a.Id == manager.Id);

            Assert.True(captainsOnly.Count >= 2);
            Assert.Contains(captainsOnly, a => a.Id == captain1.Id);
            Assert.Contains(captainsOnly, a => a.Id == captain2.Id);
            Assert.DoesNotContain(captainsOnly, a => a.Id == manager.Id);
        }

        #endregion
    }
}