# Meeting Summaries

A local web app for storing and browsing agile ceremony notes. Pick a date, see all meetings that day, manage bullet points per meeting.

Built with .NET 8, Blazor Server, PostgreSQL, and Docker Compose.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-7B3FE4?logo=blazor)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)

## Features

- Monthly calendar with color-coded dots showing which meeting types occurred on each day
- One page per day — all meeting types side by side
- Rich text points: multi-line, tab-indented, preserved formatting
- Drag-and-drop reordering of points with drop-zone indicators
- Inline editing, keyboard shortcuts (Ctrl+Enter to save, Escape to cancel)
- Selected date persisted in `localStorage`

## Meeting types

| Type | Color |
|---|---|
| Daily | blue |
| Refinement | orange |
| Retro | red |
| Sprint Review | green |
| Sprint Planning | purple |

## Running locally

Requirements: [Docker Desktop](https://www.docker.com/products/docker-desktop/)

```bash
git clone https://github.com/0Swat/meeting-summaries.git
cd meeting-summaries
docker compose up
```

Open **http://localhost:8080/meetings**

The database schema is applied automatically on first start.

## Stack

- **Backend** — ASP.NET Core 8 Web API (Controllers)
- **Frontend** — Blazor Server (interactive, no JS framework)
- **Database** — PostgreSQL 16 via EF Core (code-first migrations)
- **Docs** — Swagger at `/swagger`
