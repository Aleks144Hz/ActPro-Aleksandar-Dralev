# ActPro Development Guide

## Project Overview

ActPro is an ASP.NET Core sports facility booking platform with localization support (Bulgarian/English).

### Project Structure
```
ActPro.sln
├── ActPro/              # Main web application (MVC)
├── ActPro.Domain/       # Domain models and business logic
├── ActPro.DAL/          # Data access layer (EF Core entities)
└── ActPro.Services/     # Application services
```

---

## Build & Run Commands

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --project ActPro
# or
dotnet watch run --project ActPro  # Auto-reload on changes
```

### Clean & Rebuild
```bash
dotnet clean
dotnet build
```

### Test
```bash
dotnet test                           # Run all tests
dotnet test --filter "FullyQualifiedName~TestClassName"  # Run specific test class
dotnet test --filter "FullyQualifiedName~TestMethodName" # Run specific test
```

---

## Code Style Guidelines

### General
- **Language:** C# (.NET 9)
- **Framework:** ASP.NET Core MVC, Entity Framework Core
- **Target:** Windows (primary), deployment to Linux/Render

### Naming Conventions
- **Classes/Properties:** PascalCase (`PlaceService`, `UserId`)
- **Methods/Variables:** PascalCase for public, camelCase for private
- **Interfaces:** Start with `I` (`IPlaceService`)
- **ViewModels:** Suffix with `ViewModel` (`PlaceEntryViewModel`)
- **DbEntities:** No suffix or suffix with entity name (`Place`, `User`)

### Imports
```csharp
// Order: System → Microsoft → Third-party → Project
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActPro.DAL.Entities;
using ActPro.Domain.Models;
using ActPro.Services.Interfaces;
```

### Nullable Reference Types
- **USE nullable reference types** (`string?`, `int?`)
- Always handle nullable types with null checks (`??`, `?.`, `if (value == null)`)
- Use `GetValueOrDefault()` for nullable value types when appropriate

### Validation
- Use DataAnnotations on ViewModels: `[Required]`, `[StringLength]`, `[Range]`
- Validation messages use resource keys: `ErrorMessage = "ValidationNameRequired"`
- Resource files: `MessageResources.resx` (BG), `MessageResources.en.resx` (EN)
- Domain resources: `DomainResources.resx` (BG), `DomainResources.en.resx` (EN)

### Error Handling
- Use try-catch for operations that may fail (file I/O, database)
- Return meaningful error messages with `TempData["Error"]`
- Log errors with ILogger when appropriate

### Database (EF Core)
- Use nullable types for optional fields (`int?`, `decimal?`)
- Avoid navigation property loading issues with null checks
- Use `.GetValueOrDefault()` or null coalescing when mapping to non-nullable types

### Views (Razor)
- Use tag helpers (`asp-for`, `asp-action`, `asp-controller`)
- Include validation: `<span asp-validation-for="PropertyName">`
- Use DomainResources for UI text: `@DomainResources.ButtonSave`

### Localization
- **Two resource file sets:**
  - `MessageResources.resx` - Validation messages, UI strings in ActPro project
  - `DomainResources.resx` - Domain-specific strings in ActPro.Domain project
- Each has corresponding `.en.resx` files for English translations
- Default language is Bulgarian ("bg")

---

## Resource Files Explanation

**Why there are two sets of resource files:**

1. **ActPro/MessageResources.resx** - Contains:
   - Validation error messages (`ValidationNameRequired`, etc.)
   - General UI strings used across the application

2. **ActPro.Domain/DomainResources.resx** - Contains:
   - Domain-specific strings (place names, activity types)
   - Shared resources used by multiple projects

Each has Bulgarian (base .resx) and English (.en.resx) versions for localization.

---

## Common Patterns

### Controller with Service Injection
```csharp
public class PlaceController : Controller
{
    private readonly IPlaceService _placeService;
    
    public PlaceController(IPlaceService placeService)
    {
        _placeService = placeService;
    }
}
```

### ViewModel with Validation
```csharp
public class CreatePlaceViewModel
{
    [Required(ErrorMessage = "ValidationNameRequired")]
    [StringLength(200, ErrorMessage = "ValidationNameTooLong")]
    public string Name { get; set; } = string.Empty;
    
    public int? CityId { get; set; }  // Nullable for Required validation
}
```

### Entity Mapping with Null Safety
```csharp
var viewModel = new PlaceViewModel
{
    Id = place.Id,
    Name = place.Name ?? string.Empty,
    Price = place.Price ?? 0,
    CityName = place.City?.Name ?? "Unknown"
};
```

---

## Notes for Agents

1. **Always rebuild** after making changes to resource files (.resx)
2. **Clear browser cookies** when testing language switching
3. **Use nullable types** (`int?`, `string?`) for optional database fields
4. **Handle null** when mapping between entities and ViewModels
5. **Add validation** to ViewModels, not to Entity classes (when possible)
6. **Test both languages** when modifying UI strings
