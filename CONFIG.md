# ══════════════════════════════════════════════
# appsettings.Development.json
# ══════════════════════════════════════════════
# Crea este archivo en src/RouteOps.API/ y NO lo subas a git (.gitignore)
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=routeops_dev;Username=postgres;Password=tu_password"
  },
  "Auth": {
    "Authority": "https://tu-proyecto.supabase.co/auth/v1",
    "Audience": "authenticated"
  },
  "WhatsApp": {
    "ApiUrl": "https://api.whatsapp.com/v1/messages",
    "Token": "TU_TOKEN_AQUI"
  },
  "Firebase": {
    "ProjectId": "tu-proyecto-firebase"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}

# ══════════════════════════════════════════════
# .gitignore recomendado (agrega esto al raíz)
# ══════════════════════════════════════════════
**/appsettings.Development.json
**/appsettings.Production.json
**/bin/
**/obj/
**/.vs/
*.user

# ══════════════════════════════════════════════
# RouteOps.API.csproj
# ══════════════════════════════════════════════
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Hangfire.AspNetCore"         Version="1.8.*" />
    <PackageReference Include="Hangfire.PostgreSql"         Version="1.20.*" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.*" />
    <PackageReference Include="Swashbuckle.AspNetCore"      Version="6.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\RouteOps.Application\RouteOps.Application.csproj" />
    <ProjectReference Include="..\RouteOps.Infrastructure\RouteOps.Infrastructure.csproj" />
  </ItemGroup>
</Project>

# ══════════════════════════════════════════════
# RouteOps.Application.csproj
# ══════════════════════════════════════════════
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MediatR"                     Version="12.*" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Abstractions"     Version="9.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\RouteOps.Domain\RouteOps.Domain.csproj" />
  </ItemGroup>
</Project>

# ══════════════════════════════════════════════
# RouteOps.Infrastructure.csproj
# ══════════════════════════════════════════════
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.*" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />
    <PackageReference Include="Hangfire.PostgreSql"                   Version="1.20.*" />
    <PackageReference Include="FirebaseAdmin"                         Version="3.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\RouteOps.Application\RouteOps.Application.csproj" />
  </ItemGroup>
</Project>
