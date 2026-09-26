# 🐾 Chiron — Bitácora del Proyecto

> Diario de sesiones e historias completadas. Lo más reciente arriba.

---

## Sesión 2 — 2026-09-26

### Historias trabajadas
- **[H1.1] Inicializar solución .NET con Clean Architecture — ✅ COMPLETADA**

### Qué se hizo
- Instalado .NET 8 SDK (8.0.425) vía winget.
- Inicializado repositorio git (rama `main`) conectado a `origin`.
- Configurada identidad git local: JonYDM / jonyocampo05@gmail.com.
- Commit inicial de documentación (`dd15f31`) subido a GitHub.
- Creada rama `feature/H1.1-init-solucion`.
- Creada solución `Chiron.sln` con 4 proyectos en `src/`:
  - `Chiron.Domain` (classlib) — núcleo, sin dependencias.
  - `Chiron.Application` (classlib) — depende de Domain.
  - `Chiron.Infrastructure` (classlib) — depende de Application.
  - `Chiron.ConsoleApp` (console) — depende de Application + Infrastructure.
- Referencias configuradas respetando Clean Architecture.
- Eliminados los `Class1.cs` por defecto.
- `Program.cs` inicial con mensaje de arranque.

### Verificación
- `dotnet build`: **Compilación correcta, 0 advertencias, 0 errores.**
- `dotnet run`: la app arranca e imprime el mensaje inicial correctamente.

### Siguiente paso
- Commit de H1.1 y merge a `main`.
- Continuar con **H1.2** (configurar inyección de dependencias).

---

## Sesión 1 — 2026-09-26

### Definición del proyecto
- Se definió el producto: **Chiron**, SaaS de gestión para veterinarias en LATAM (inicio en Morelos).
- Nicho elegido: **veterinarias** (menos competencia, combina clínica + punto de venta).
- Diferenciador: **recordatorios automáticos por WhatsApp**.
- Validación inicial: un dueño de veterinaria ya mostró interés.

### Decisiones técnicas
- Lenguaje: **C# / .NET 8**.
- Arquitectura: **Clean Architecture**.
- Metodología: **Git Flow simplificado + Conventional Commits + commit por historia**.
- Documentación viva en `docs/*.md`.

### Documentación creada
- `docs/CONTEXTO.md`
- `docs/BACKLOG.md`
- `docs/METODOLOGIA.md`
- `docs/BITACORA.md`

### Estado
- Pendiente: arrancar **H1.1** (inicializar solución .NET).

---
<!-- Plantilla para nuevas entradas:

## Sesión N — AAAA-MM-DD

### Historias trabajadas
- [Hx.x] descripción — estado

### Decisiones / notas

### Siguiente paso
-->
