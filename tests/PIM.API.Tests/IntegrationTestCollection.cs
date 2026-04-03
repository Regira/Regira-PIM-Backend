using Xunit;

namespace PIM.API.Tests;

/// <summary>
/// Groups all integration test classes into one xUnit collection so they run sequentially.
/// This prevents race conditions in the Regira framework's interceptor caching
/// (EntityTypeExtensions.GetPropertyAttributes) when multiple test classes initialise
/// their DbContext concurrently.
/// Each class still uses IClassFixture&lt;TestFixture&gt; for its own isolated SQLite database.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection { }
