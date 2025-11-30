using Xunit;
using PortLog.Helpers;
using System.Text.RegularExpressions;

namespace PortLog.Testing.Helpers
{
    public class JoinCodeTests
    {
        private const string ValidCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        private const int ExpectedLength = 7;

        #region CreateFromGuid - Basic Functionality Tests

        [Fact]
        public void CreateFromGuid_ReturnsNonEmptyString()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            // Act
            var code = JoinCode.CreateFromGuid(companyId);

            // Assert
            Assert.False(string.IsNullOrEmpty(code));
        }

        [Fact]
        public void CreateFromGuid_ReturnsCodeWithCorrectLength()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            // Act
            var code = JoinCode.CreateFromGuid(companyId);

            // Assert
            Assert.Equal(ExpectedLength, code.Length);
        }

        [Fact]
        public void CreateFromGuid_ReturnsCodeWithOnlyValidCharacters()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            // Act
            var code = JoinCode.CreateFromGuid(companyId);

            // Assert
            foreach (char c in code)
            {
                Assert.Contains(c, ValidCharacters);
            }
        }

        [Fact]
        public void CreateFromGuid_DoesNotContainAmbiguousCharacters()
        {
            // Arrange
            var ambiguousChars = new[] { '0', 'O', 'o', 'I', 'l', '1' };
            var companyId = Guid.NewGuid();

            // Act
            var code = JoinCode.CreateFromGuid(companyId);

            // Assert
            foreach (char ambiguous in ambiguousChars)
            {
                Assert.DoesNotContain(ambiguous, code);
            }
        }

        #endregion

        #region CreateFromGuid - Deterministic Behavior Tests

        [Fact]
        public void CreateFromGuid_SameGuidProducesSameCode()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            // Act
            var code1 = JoinCode.CreateFromGuid(companyId);
            var code2 = JoinCode.CreateFromGuid(companyId);
            var code3 = JoinCode.CreateFromGuid(companyId);

            // Assert
            Assert.Equal(code1, code2);
            Assert.Equal(code2, code3);
        }

        [Fact]
        public void CreateFromGuid_DifferentGuidsProduceDifferentCodes()
        {
            // Arrange
            var companyId1 = Guid.NewGuid();
            var companyId2 = Guid.NewGuid();

            // Act
            var code1 = JoinCode.CreateFromGuid(companyId1);
            var code2 = JoinCode.CreateFromGuid(companyId2);

            // Assert
            Assert.NotEqual(code1, code2);
        }

        [Fact]
        public void CreateFromGuid_ProducesUniqueCodesFor100Companies()
        {
            // Arrange
            var codes = new HashSet<string>();
            const int iterations = 100;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var companyId = Guid.NewGuid();
                var code = JoinCode.CreateFromGuid(companyId);
                codes.Add(code);
            }

            // Assert - all codes should be unique
            Assert.Equal(iterations, codes.Count);
        }

        [Fact]
        public void CreateFromGuid_ProducesUniqueCodesFor1000Companies()
        {
            // Arrange
            var codes = new HashSet<string>();
            const int iterations = 1000;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var companyId = Guid.NewGuid();
                var code = JoinCode.CreateFromGuid(companyId);
                codes.Add(code);
            }

            // Assert
            Assert.Equal(iterations, codes.Count);
        }

        [Fact]
        public void CreateFromGuid_IsDeterministicAcrossMultipleCalls()
        {
            // Arrange
            var companyIds = Enumerable.Range(0, 10)
                .Select(_ => Guid.NewGuid())
                .ToList();

            // Act - Generate codes first time
            var firstRun = companyIds
                .Select(id => JoinCode.CreateFromGuid(id))
                .ToList();

            // Act - Generate codes second time
            var secondRun = companyIds
                .Select(id => JoinCode.CreateFromGuid(id))
                .ToList();

            // Assert - Both runs should produce identical results
            Assert.Equal(firstRun, secondRun);
        }

        #endregion

        #region CreateFromGuid - Known GUID Tests

        [Fact]
        public void CreateFromGuid_EmptyGuidProducesConsistentCode()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act
            var code1 = JoinCode.CreateFromGuid(emptyGuid);
            var code2 = JoinCode.CreateFromGuid(emptyGuid);

            // Assert
            Assert.Equal(code1, code2);
            Assert.Equal(ExpectedLength, code1.Length);
        }

        [Fact]
        public void CreateFromGuid_SpecificGuidProducesExpectedFormat()
        {
            // Arrange - Use a known GUID for reproducible testing
            var knownGuid = new Guid("12345678-1234-1234-1234-123456789abc");

            // Act
            var code = JoinCode.CreateFromGuid(knownGuid);

            // Assert
            Assert.Equal(ExpectedLength, code.Length);
            Assert.All(code, c => Assert.Contains(c, ValidCharacters));
        }

        [Fact]
        public void CreateFromGuid_SimilarGuidsProduceDifferentCodes()
        {
            // Arrange - GUIDs that differ by only one bit
            var guid1 = new Guid("12345678-1234-1234-1234-123456789abc");
            var guid2 = new Guid("12345678-1234-1234-1234-123456789abd");

            // Act
            var code1 = JoinCode.CreateFromGuid(guid1);
            var code2 = JoinCode.CreateFromGuid(guid2);

            // Assert - Even tiny GUID differences should produce different codes
            Assert.NotEqual(code1, code2);
        }

        #endregion

        #region CreateFromGuid - Format Validation Tests

        [Fact]
        public void CreateFromGuid_MatchesExpectedPattern()
        {
            // Arrange
            var pattern = $"^[{Regex.Escape(ValidCharacters)}]{{{ExpectedLength}}}$";
            var regex = new Regex(pattern);
            var companyId = Guid.NewGuid();

            // Act
            var code = JoinCode.CreateFromGuid(companyId);

            // Assert
            Assert.Matches(regex, code);
        }

        [Fact]
        public void CreateFromGuid_DoesNotContainWhitespace()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            // Act
            var code = JoinCode.CreateFromGuid(companyId);

            // Assert
            Assert.DoesNotContain(" ", code);
            Assert.DoesNotContain("\t", code);
            Assert.DoesNotContain("\n", code);
        }

        [Fact]
        public void CreateFromGuid_DoesNotContainSpecialCharacters()
        {
            // Arrange
            var specialChars = new[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+' };
            var companyId = Guid.NewGuid();

            // Act
            var code = JoinCode.CreateFromGuid(companyId);

            // Assert
            foreach (char special in specialChars)
            {
                Assert.DoesNotContain(special, code);
            }
        }

        #endregion

        #region CreateFromGuid - Character Distribution Tests

        [Fact]
        public void CreateFromGuid_UsesVarietyOfCharacters()
        {
            // Arrange
            var allCharsUsed = new HashSet<char>();
            const int iterations = 500;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var companyId = Guid.NewGuid();
                var code = JoinCode.CreateFromGuid(companyId);
                foreach (char c in code)
                {
                    allCharsUsed.Add(c);
                }
            }

            // Assert - should use at least 40 different characters out of 57 possible
            Assert.True(allCharsUsed.Count >= 40,
                $"Only {allCharsUsed.Count} unique characters used, expected at least 40");
        }

        [Fact]
        public void CreateFromGuid_ProducesVariedFirstCharacters()
        {
            // Arrange
            var firstChars = new HashSet<char>();
            const int iterations = 100;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var companyId = Guid.NewGuid();
                var code = JoinCode.CreateFromGuid(companyId);
                firstChars.Add(code[0]);
            }

            // Assert - should have varied first characters
            Assert.True(firstChars.Count >= 15,
                $"Only {firstChars.Count} unique first characters");
        }

        #endregion

        #region CreateFromGuid - Performance Tests

        [Fact]
        public void CreateFromGuid_CanGenerateManyCodesQuickly()
        {
            // Arrange
            const int iterations = 10000;
            var startTime = DateTime.UtcNow;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var companyId = Guid.NewGuid();
                JoinCode.CreateFromGuid(companyId);
            }

            var elapsed = DateTime.UtcNow - startTime;

            // Assert - should complete in less than 2 seconds
            Assert.True(elapsed.TotalSeconds < 2,
                $"Generating {iterations} codes took {elapsed.TotalSeconds:F2} seconds");
        }

        [Fact]
        public void CreateFromGuid_ThreadSafe_ProducesSameResultsInParallel()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            var results = new System.Collections.Concurrent.ConcurrentBag<string>();
            const int parallelCalls = 100;

            // Act
            Parallel.For(0, parallelCalls, _ =>
            {
                results.Add(JoinCode.CreateFromGuid(testGuid));
            });

            // Assert - all calls with same GUID should produce same code
            var uniqueResults = results.Distinct().ToList();
            Assert.Single(uniqueResults);
        }

        #endregion

        #region CreateFromGuid - Statistical Tests

        [Fact]
        public void CreateFromGuid_HasLowCollisionRate()
        {
            // Arrange
            const int iterations = 10000;
            var codes = new HashSet<string>();

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var companyId = Guid.NewGuid();
                var code = JoinCode.CreateFromGuid(companyId);
                codes.Add(code);
            }

            // Assert - collision rate should be very low
            var collisionRate = 1.0 - ((double)codes.Count / iterations);
            Assert.True(collisionRate < 0.01,
                $"Collision rate {collisionRate:P2} is too high (expected < 1%)");
        }

        #endregion

        #region CreateFromGuid - Edge Cases

        [Fact]
        public void CreateFromGuid_HandlesAllZeroGuid()
        {
            // Arrange
            var guid = new Guid("00000000-0000-0000-0000-000000000000");

            // Act
            var code = JoinCode.CreateFromGuid(guid);

            // Assert
            Assert.Equal(ExpectedLength, code.Length);
            Assert.All(code, c => Assert.Contains(c, ValidCharacters));
        }

        [Fact]
        public void CreateFromGuid_HandlesAllFsGuid()
        {
            // Arrange
            var guid = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");

            // Act
            var code = JoinCode.CreateFromGuid(guid);

            // Assert
            Assert.Equal(ExpectedLength, code.Length);
            Assert.All(code, c => Assert.Contains(c, ValidCharacters));
        }

        [Fact]
        public void CreateFromGuid_ConsistentlyReturns7Characters()
        {
            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var companyId = Guid.NewGuid();
                var code = JoinCode.CreateFromGuid(companyId);
                Assert.Equal(7, code.Length);
            }
        }

        [Fact]
        public void CreateFromGuid_NeverReturnsNull()
        {
            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                var companyId = Guid.NewGuid();
                var code = JoinCode.CreateFromGuid(companyId);
                Assert.NotNull(code);
            }
        }

        #endregion

        #region Real-World Scenario Tests

        [Fact]
        public void CreateFromGuid_MultipleCompaniesGetUniqueJoinCodes()
        {
            // Arrange - Simulate multiple companies registering
            var companies = new Dictionary<Guid, string>();

            // Act
            for (int i = 0; i < 50; i++)
            {
                var companyId = Guid.NewGuid();
                var joinCode = JoinCode.CreateFromGuid(companyId);
                companies[companyId] = joinCode;
            }

            // Assert - All join codes should be unique
            var uniqueCodes = companies.Values.Distinct().Count();
            Assert.Equal(companies.Count, uniqueCodes);
        }

        [Fact]
        public void CreateFromGuid_CompanyCanRetrieveSameCodeMultipleTimes()
        {
            // Arrange - Simulate a company checking their join code multiple times
            var companyId = Guid.NewGuid();

            // Act - Retrieve code at different times
            var code1 = JoinCode.CreateFromGuid(companyId);
            System.Threading.Thread.Sleep(10);
            var code2 = JoinCode.CreateFromGuid(companyId);
            System.Threading.Thread.Sleep(10);
            var code3 = JoinCode.CreateFromGuid(companyId);

            // Assert - Should always be the same
            Assert.Equal(code1, code2);
            Assert.Equal(code2, code3);
        }

        #endregion

        #region Comparison Tests

        [Fact]
        public void CreateFromGuid_ProducesDifferentCodesThanRandom()
        {
            // This test verifies the new method produces different results
            // than pure random generation would

            // Arrange
            var companyId = Guid.NewGuid();
            var guidBasedCode = JoinCode.CreateFromGuid(companyId);

            // Act - Generate multiple random codes
            var randomCodes = Enumerable.Range(0, 100)
                .Select(_ =>
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    return JoinCode.CreateRandom();
#pragma warning restore CS0618
                })
                .ToList();

            // Assert - GUID-based code should be deterministic, random ones vary
            var uniqueRandom = randomCodes.Distinct().Count();
            Assert.True(uniqueRandom > 90, "Random codes should be mostly unique");

            // Generate the same GUID code again - should be identical
            var guidBasedCode2 = JoinCode.CreateFromGuid(companyId);
            Assert.Equal(guidBasedCode, guidBasedCode2);
        }

        #endregion
    }
}