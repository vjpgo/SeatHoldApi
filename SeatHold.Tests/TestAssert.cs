namespace SeatHold.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

internal static class TestAssert
{
    public static async Task<TException> ThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TException ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected exception {typeof(TException).Name}, but got {ex.GetType().Name}: {ex.Message}");
            throw new AssertFailedException("Unexpected exception type.", ex);
        }

        Assert.Fail($"Expected exception {typeof(TException).Name}, but no exception was thrown.");
        throw new AssertFailedException($"Expected exception {typeof(TException).Name}, but no exception was thrown.");
    }
}
