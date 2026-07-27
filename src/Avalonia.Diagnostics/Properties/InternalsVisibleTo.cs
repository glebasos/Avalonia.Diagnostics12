using System.Runtime.CompilerServices;

// Almost everything in this library is internal (the whole DevTools UI is an implementation
// detail), so the test assembly needs access. Both assemblies are strong-named with
// build/avalonia.snk, hence the PublicKey - it is the same value as $(AvaloniaPublicKey) in
// Directory.Build.props. Note this does NOT give the tests access to Avalonia's own internals:
// Avalonia.Base grants InternalsVisibleTo("Avalonia.Diagnostics", ...) and a handful of its own
// test assemblies, but not this one.
[assembly: InternalsVisibleTo("Avalonia.Diagnostics.UnitTests, PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100c1bba1142285fe0419326f" +
    "b25866ba62c47e6c2b5c1ab0c95b46413fad375471232cb81706932e1cef38781b9ebd39d5100401bacb65" +
    "1c6c5bbf59e571e81b3bc08d2a622004e08b1a6ece82a7e0b9857525c86d2b95fab4bc3dce148558d7f3ae" +
    "61aa3a234086902aeface87d9dfdd32b9d2fe3c6dd4055b5ab4b104998bd87")]
