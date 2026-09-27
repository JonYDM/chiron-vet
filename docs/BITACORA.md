# 🐾 Chiron — Bitácora del Proyecto

> Diario de sesiones e historias completadas. Lo más reciente arriba.

---

## Sesión 2 (cont.) — 2026-09-26

### [H1.3] Repositorio genérico + repositorio en memoria — ✅ COMPLETADA
- **Domain**: `Common/EntidadBase.cs` (Id Guid) y `Common/EntidadPrueba.cs` (temporal, se elimina en Épica 2).
- **Application**: `Common/IRepository<T>.cs` — contrato genérico asíncrono (Agregar, ObtenerPorId, ObtenerTodos, Actualizar, Eliminar).
- **Infrastructure**: `Persistencia/RepositorioEnMemoria<T>.cs` con `ConcurrentDictionary` (O(1), thread-safe). Registrado en DI como Singleton (open generic).
- Verificación: `dotnet run` resuelve `IRepository<EntidadPrueba>` desde el contenedor, agrega/recupera/lista correctamente (recupera "Firulais", total=1).

### 🎉 ÉPICA 1 (Fundación técnica) COMPLETADA
- H1.1 ✅ estructura Clean Architecture
- H1.2 ✅ inyección de dependencias
- H1.3 ✅ repositorio genérico intercambiable

### Pendiente de esta sesión
- Hacer PRs a main (los hace el usuario): mergear en orden H1.2 → H1.3 (H1.3 ya contiene a H1.2).
- Nota: rama H1.3 parte de H1.2 (historias dependientes acumuladas).

### Siguiente sesión
- **Épica 2 (H2.1)** — entidad Cliente con validaciones. Eliminar `EntidadPrueba`.

---

## Sesión 2 (cont.) — 2026-09-26

### [H1.2] Configurar inyección de dependencias — ✅ COMPLETADA
- Paquetes agregados (versiones fijadas a línea 8.x LTS):
  - `Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.2 en Application e Infrastructure.
  - `Microsoft.Extensions.Hosting` 8.0.1 en ConsoleApp.
- Patrón de registro por capa (métodos de extensión):
  - `Chiron.Application/DependencyInjection.cs` → `AddApplication()`.
  - `Chiron.Infrastructure/DependencyInjection.cs` → `AddInfrastructure()`.
- `Program.cs` reescrito usando **Generic Host** (`Host.CreateDefaultBuilder`), Composition Root único.
- Verificación: `dotnet build` correcto (0/0); `dotnet run` arranca, resuelve `ILogger` desde el contenedor e imprime mensajes.

### Nota de decisión — Frontend (pregunta del usuario)
- El backend expondrá una **API REST** (H8.1), por lo que el frontend es intercambiable.
- Opciones registradas en backlog (H8.2): **React/Vue** (aprovecha que el usuario sabe Node) o **Blazor** (todo en C#). App móvil futura con **.NET MAUI**.
- Recomendación: web primero (React o Blazor), móvil después.

### Siguiente paso
- **H1.3** — contrato de repositorio genérico + repositorio base en memoria.

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
