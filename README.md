# Studyroom

Studyroom is an ASP.NET Core Razor Pages study planner backed by Neon PostgreSQL. EF Core creates the `subjects` and `study_tasks` tables; the planner ensures a C# Programming course and a practical starter plan exist without replacing existing subjects or tasks.

Neon project configuration is in [neon.ts](neon.ts), with Neon Auth enabled. The Neon CLI project link is kept in `.neon`, and generated credentials are kept in `.env.local`; both are ignored by Git.

## Connect Neon

1. Create a PostgreSQL project in Neon and copy its pooled or direct connection string from **Connect**.
2. Put the connection string in `appsettings.Development.json` under `ConnectionStrings:Neon`, or set it for the current PowerShell session:

   ```powershell
   $env:ConnectionStrings__Neon = "Host=YOUR_NEON_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
   dotnet run
   ```

3. Open the local URL printed by `dotnet run`. Keep the connection string out of source control; use an environment variable or a secret store.

## Neon CLI setup already completed

From this directory, the project was linked and deployed with:

```powershell
neon link --project-id orange-bird-14883508 --branch production -y
neon config init
neon deploy
```

The resulting `neon.ts` contains `auth: true`. To repeat the setup on another machine, authenticate with `neon login`, run `neon skills -y`, then `neon mcp -y` and the commands above.

Neon Auth being enabled in `neon.ts` provisions the Neon Auth service. Application sign-in pages and session handling still need to be added separately if this planner should have individual user accounts.

## Application structure

`Subject` owns its study tasks and creates them through the domain model. The dashboard delegates planner operations to `IStudyPlannerService`; its EF Core implementation handles persistence, startup setup, and weekly summaries. Invalid domain operations use specific planner errors, while the page logs unexpected failures and shows user-safe messages.

Without that variable, the dashboard opens in setup mode and displays the required configuration name. Task and subject changes are stored in Neon after connecting.