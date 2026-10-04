# Studyroom

Studyroom is an ASP.NET Core Razor Pages study planner backed by Neon PostgreSQL. EF Core creates the `subjects` and `study_tasks` tables; the planner ensures a C# Programming course and a practical starter plan exist without replacing existing subjects or tasks.

Neon project configuration is in [neon.ts](neon.ts), with Neon Auth enabled. The Neon CLI project link is kept in `.neon`, and generated credentials are kept in `.env.local`; both are ignored by Git. During development, the ASP.NET app reads `DATABASE_URL` from `.env.local`. In deployment, it reads `ConnectionStrings__Neon` from the hosting platform's secret configuration.

## Connect Neon

1. If a connection string was exposed, reset the role password in the Neon Console under **Postgres database → Roles → Reset password**. Repeat on each branch that uses the role; the old password stops working on new connections.
2. From this project directory, refresh the ignored local environment file:

```powershell
neon env pull --file .env.local
```

1. Run `dotnet run`. The app loads `.env.local` in Development; stop and restart it after pulling new values. For deployment, set `ConnectionStrings__Neon` through the hosting platform's secret configuration. Never commit `.env.local` or a real connection string.

## Neon CLI setup already completed

From this directory, the project was linked and deployed with:

```powershell
neon config init
neon deploy
```

The resulting `neon.ts` contains `auth: true`. To repeat the setup on another machine, authenticate with `neon login`, run `neon skills -y`, then `neon mcp -y` and the commands above.

Neon Auth being enabled in `neon.ts` provisions the Neon Auth service. Application sign-in pages and session handling still need to be added separately if this planner should have individual user accounts.

## Application structure

`Subject` owns its study tasks and creates them through the domain model. The dashboard delegates planner operations to `IStudyPlannerService`; its EF Core implementation handles persistence, startup setup, and weekly summaries. Invalid domain operations use specific planner errors, while the page logs unexpected failures and shows user-safe messages.

Without that variable, the dashboard opens in setup mode and displays the required configuration name. Task and subject changes are stored in Neon after connecting.
