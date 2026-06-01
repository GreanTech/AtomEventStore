using System;
using Xunit;

namespace Grean.AtomEventStore.AzureBlobAsync.Tests
{
    /// <summary>
    /// A <see cref="FactAttribute" /> that runs only when an Azure Blob
    /// connection string is configured via the
    /// <c>ATOMEVENTSTORE_AZURE_CONNECTION</c> environment variable; otherwise
    /// the test is reported as skipped (never silently passed).
    /// </summary>
    /// <remarks>
    /// Set the variable to a real (test) storage account connection string, or
    /// to <c>UseDevelopmentStorage=true</c> with Azurite running, to execute
    /// these integration tests.
    /// </remarks>
    public sealed class IntegrationFactAttribute : FactAttribute
    {
        public const string ConnectionVariable = "ATOMEVENTSTORE_AZURE_CONNECTION";

        public IntegrationFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(ConnectionVariable)))
            {
                this.Skip =
                    "Set " + ConnectionVariable +
                    " to an Azure Blob connection string (real test account or " +
                    "'UseDevelopmentStorage=true' with Azurite) to run.";
            }
        }
    }
}
