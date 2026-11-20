// Small pieces the C# compiler expects that .NET Framework 4.8 does not ship.
namespace System.Runtime.CompilerServices
{
    /// <summary>Lets <c>record</c> types and <c>init</c> accessors compile on .NET Framework.</summary>
    internal static class IsExternalInit { }
}
