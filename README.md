# bookpath

A small C# CLI for saving and jumping to frequently used directories.

The application is named `bookpath`. The intended shell alias is `bp`.

## Requirements

- .NET 10 SDK or newer
- Spectre.Console.Cli 0.55.0

`Spectre.Console.Cli` 0.55.0 is the stable package used by this project.

## Commands

Create a bookmark using the current directory:

```text
bp
```

Create a bookmark with a name:

```text
bp --name work
bp -n work
```

Create a bookmark with a specific path:

```text
bp --name work --path "C:\src\my-project"
```

If `--name` is omitted, the application chooses the next numeric name (`1`, `2`, `3`, ...). The next number is based on the highest existing numeric name, so deleting `2` and creating another bookmark produces `4` rather than reusing `2`.

If `--path` is omitted, the current working directory is stored.

List bookmarks:

```text
bp list
```

Update only the name:

```text
bp update work --name my-work
```

Update only the path:

```text
bp update work --path "C:\src\other-project"
```

Update both:

```text
bp update work --name my-work --path "C:\src\other-project"
```

Delete:

```text
bp delete work
```

Lookup a bookmark path:

```text
bp work
```

## Persistence

Bookmarks are stored in:

```text
%USERPROFILE%\.bookpath\bookmarks.json
```

On other platforms the equivalent user home directory is used:

```text
~/.bookpath/bookmarks.json
```

The JSON contains the GUID, unique name, and unique path for every bookmark.

## Important: changing the shell directory

A normal executable cannot change the working directory of the parent shell process. For that reason, `bookpath` prints only the resolved path when it is invoked as a direct bookmark lookup.

To make `bp <bookmark_name>` actually change the current directory, use the supplied shell function:

### PowerShell

Put `scripts/bp.ps1` in your PowerShell profile (or copy its function into the profile). Make sure `bookpath.exe` is available on `PATH`.

### Bash / Zsh

Add the contents of `scripts/bp.sh` to `~/.bashrc` or `~/.zshrc`. Make sure the `bookpath` executable is available on `PATH`.

Without the shell wrapper, this also works:

```powershell
Set-Location -LiteralPath (bookpath work)
```

## Build

```text
dotnet restore
dotnet build -c Release
```

For a self-contained Windows executable:

```text
dotnet publish src/BookPath/BookPath.csproj -c Release -r win-x64 --self-contained true
```

Then place `bookpath.exe` on `PATH` and load the PowerShell `bp` function.

## Design

The implementation is intentionally small:

- `Domain/Bookmark.cs` — bookmark entity.
- `Application/BookmarkService.cs` — uniqueness, normalization, CRUD and incremental naming rules.
- `Application/IBookmarkRepository.cs` — persistence abstraction.
- `Infrastructure/JsonBookmarkRepository.cs` — JSON file storage under the user's home directory.
- `Commands/*` — Spectre.Console.Cli command/settings classes.
- `Program.cs` — dependency wiring and command registration.

Names and paths are treated case-insensitively for uniqueness. Paths are normalized to full paths before they are stored.

The reserved command names are `list`, `update`, `delete`, and `help`, so those values cannot be used unambiguously with `bp <bookmark_name>` for direct navigation.
