# F14 — Endpoints MCP (Model Context Protocol)

## Metadata
- Estado: implementada
- Módulo: F14

## Descripción
Wardkitten expone un servidor **MCP** (Model Context Protocol) sobre HTTP para que, **en el futuro**, un
agente externo pueda operar Wardkitten (listar/crear watches, hacer check-ins, consultar incidentes…).

> **MCP no es IA.** Es un protocolo/interfaz estándar (JSON-RPC sobre HTTP), análogo a REST o gRPC.
> Wardkitten **no incorpora** ningún modelo ni dependencia de IA (ver `AGENTS.md` → "Sin IA"); simplemente
> ofrece esta interfaz para que una IA *externa* pueda conectarse si el usuario lo desea.

## Endpoint
- `/mcp` (Streamable HTTP). **Protegido por JWT** (`RequireAuthorization`): el cliente debe presentar un
  Bearer token de un usuario; cada herramienta opera sobre los datos de ese usuario. Ver SECURITY.md.

## Herramientas expuestas (F14.01)
`list_watches`, `get_watch`, `create_watch`, `check_in_watch`, `pause_watch`, `resume_watch`,
`list_incidents`, `acknowledge_incident`, `get_wallet_balance`. Definidas en
`src/Wardkitten.Api/Mcp/WardkittenMcpTools.cs` con el SDK oficial `ModelContextProtocol.AspNetCore`.

## Cómo conectarse
1. Obtén un access token (`POST /api/auth/login`).
2. Configura tu cliente MCP con la URL `https://api.wardkitten.com/mcp` y la cabecera
   `Authorization: Bearer <token>`.
3. El cliente descubre las herramientas y puede invocarlas.

## Reglas de negocio
- El usuario se resuelve del JWT (claim `sub`) vía `IHttpContextAccessor`; sin token válido → 401.
- Las herramientas reutilizan los servicios de aplicación (`WatchService`, `CheckInService`,
  `IncidentService`, `WalletService`), así que respetan límites de plan, validaciones y permisos.
