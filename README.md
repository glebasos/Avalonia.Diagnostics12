# AvaDiagnostics12

An unofficial community fork of the legacy `Avalonia.Diagnostics` in-process DevTools, updated to build
and run against **Avalonia 12**.

> Not affiliated with or endorsed by the AvaloniaUI project. The upstream package was
> [archived and deprecated](https://docs.avaloniaui.net/tools/developer-tools/installation) in favour of
> the standalone AvaloniaUI Developer Tools — see [Upstream alternative](#upstream-alternative) below.

## Install

```
dotnet add package AvaDiagnostics12
```

## Usage

The public API is unchanged from the original package — the namespaces are still `Avalonia.Diagnostics.*`,
so existing code needs no edits:

```csharp
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.MainWindow = new MainWindow();
#if DEBUG
        desktop.MainWindow.AttachDevTools();
#endif
    }

    base.OnFrameworkInitializationCompleted();
}
```

Press **F12** to open the DevTools window.

## What changed from upstream

- Ported to the Avalonia 12 input APIs (`RawInputEventArgs.Root` and `PointerOverRoot` are now
  `IInputRoot` rather than `Visual`), including hotkey and pointer-position handling over popup roots.
- The NuGet package id is `AvaDiagnostics12`, so this fork is never confused with the official
  `Avalonia.Diagnostics` package at install time.

## Known limitation: assembly identity

The shipped assembly is still named `Avalonia.Diagnostics` and is still strong-named with AvaloniaUI's
key. This is not a cosmetic leftover — Avalonia 12 exposes the internals this code needs
(`InputManager`, `IRenderer`, `IClassesChangedListener`) through
`[assembly: InternalsVisibleTo("Avalonia.Diagnostics", PublicKey=…)]`. Renaming the assembly or signing
with a different key fails the build with `CS0122`.

Consequence: **do not reference `AvaDiagnostics12` and the official `Avalonia.Diagnostics` package in
the same project** — both produce an `Avalonia.Diagnostics.dll` and the build will fail with a file
conflict. Use one or the other.

## Upstream alternative

If you don't need the classic in-process window, the maintained replacement is:

```
dotnet add package AvaloniaUI.DiagnosticsSupport
dotnet tool install --global AvaloniaUI.DeveloperTools
```

Then call `this.AttachDeveloperTools()` in `Application.Initialize()` and press **F12**. It includes a
free Community edition covering everything the legacy package did. See the
[Developer Tools documentation](https://docs.avaloniaui.net/tools/developer-tools/installation).

## License

MIT — see [LICENSE](LICENSE). Original code copyright © The AvaloniaUI Project. Everything in this
fork is released under the same MIT license; do whatever you like with it.
