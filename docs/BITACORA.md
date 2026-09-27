# 🐾 Chiron — Bitácora del Proyecto

> Diario de sesiones e historias completadas. Lo más reciente arriba.

---

## Sesión 2 (cont.) — 2026-09-26

### 🎉 ÉPICA 4 (Citas) COMPLETADA — H4.1, H4.2
Rama: `feature/H4-citas`.

- **Domain**: `Citas/EstadoCita.cs` (Programada, Atendida, Cancelada, NoAsistio) y `Citas/Cita.cs` (VeterinariaId, MascotaId, FechaHora, Motivo, Estado). Valida fecha futura. Transiciones controladas: MarcarAtendida/Cancelar/MarcarNoAsistio solo desde Programada.
- **Application**: `ICitaRepository` (agenda del día, próximas), casos de uso `AgendarCita` (H4.1) y `VerAgenda` (H4.2), comando `AgendarCitaComando`.
- **Infrastructure**: `CitaRepositorioEnMemoria`.
- **DI**: casos de uso Transient; repo Singleton.
- Verificación (`dotnet run`): 2 citas agendadas; rechazo de cita en el pasado; próximas ordenadas; agenda del día filtra correcto.

### Estado global del backend
Épicas completadas: 1 (fundación), 1.5 (multi-tenant), 2 (clientes/mascotas), 3 (expediente), 4 (citas).
Dominio + Aplicación + Infra (en memoria) funcionando y probados en consola.

### Siguiente paso sugerido
- Épica 5 (Recordatorios — usa FechaProximaAplicacion del expediente + citas próximas).
- Épica 6 (Punto de venta).
- Épica 8 (API REST) para conectar frontend.

---

## Sesión 2 (cont.) — 2026-09-26

### 🎉 ÉPICA 3 (Expediente médico) COMPLETADA — H3.1, H3.2, H3.3
Rama: `feature/H3-expediente-medico`.

- **Domain**: `Expedientes/TipoRegistroMedico.cs` (Consulta, Vacuna, Desparasitacion, Cirugia, Otro) y `Expedientes/RegistroMedico.cs` (VeterinariaId, MascotaId, Tipo, Fecha, Descripcion, FechaProximaAplicacion). Método `TieneRecordatorioPendiente()`. Valida próxima aplicación > fecha atención.
- **Application**: `IRegistroMedicoRepository` (ObtenerPorMascota, ObtenerProximasAplicaciones para recordatorios), casos de uso `AgregarRegistroMedico` (H3.1/H3.2) y `VerExpedienteMascota` (H3.3), comando `AgregarRegistroMedicoComando`.
- **Infrastructure**: `RegistroMedicoRepositorioEnMemoria` (expediente ordenado desc por fecha; próximas aplicaciones filtradas por rango).
- **DI**: casos de uso Transient; repo Singleton.
- Verificación (`dotnet run`): consulta + vacuna (próx 2027-09-01) + desparasitación (próx 2026-12-01); rechazo de próxima anterior a la fecha; expediente con 3 registros.

### Siguiente paso
- Épica 4 (Citas).

---

## Sesión 2 (cont.) — 2026-09-26

### Casos de uso de Application (H2.2b + H2.3) — ✅ COMPLETADO
Rama: `feature/H2.2b-casos-uso` (parte de la rama del núcleo multi-tenant).

Primera lógica de la **capa de Aplicación** (patrón caso de uso + comando):
- **Contratos** (Application): `IClienteRepository`, `IMascotaRepository` (extienden IRepository<T> con consultas de negocio filtradas por tenant).
- **Casos de uso**:
  - `RegistrarClienteConMascota` (H2.2b, registro rápido): valida veterinaria activa → crea cliente → crea mascota asociada → persiste. Devuelve IDs.
  - `BuscarClientes` (H2.3): por nombre (case-insensitive) o todos si texto vacío.
  - `ListarMascotasDeCliente` (H2.3).
  - Objetos: `RegistrarClienteConMascotaComando` (record), `RegistroRapidoResultado` (record).
- **Implementaciones** (Infrastructure): `ClienteRepositorioEnMemoria`, `MascotaRepositorioEnMemoria` (heredan de RepositorioEnMemoria<T>).
- **DI**: casos de uso como Transient en Application. En Infrastructure, los repos específicos como Singleton y `IRepository<T>` redirige a la MISMA instancia (consistencia de datos).

Verificación (`dotnet run`): registro rápido de María+Firulais OK; búsqueda "maría" → 2 clientes; mascotas de María → Firulais (edad 5); rechazo de registro en veterinaria inexistente.

### Estado
- Épica 2 (clientes/mascotas) prácticamente completa a nivel dominio + aplicación.

### Siguiente paso sugerido
- Épica 4 (Citas) o Épica 3 (Expediente médico), o exponer API REST (Épica 8) para conectar frontend.

---

## Sesión 2 (cont.) — 2026-09-26

### Bloque de entidades del núcleo (Épica 1.5 + H2.2) — ✅ COMPLETADO
Rama: `feature/H1.5-nucleo-multitenant` (parte de H2.1).

**GIRO DEL PRODUCTO documentado**: Chiron pasa a ser SaaS **multi-tenant** (cada veterinaria = tenant, datos aislados) con usuarios y roles.

Entidades creadas en Domain:
- `Veterinarias/Veterinaria.cs` (Tenant): Nombre, Telefono, Activa, FechaAlta. Métodos Activar/Desactivar (base para suscripción).
- `Usuarios/RolUsuario.cs`: Administrador, Veterinario, Recepcionista.
- `Usuarios/Usuario.cs`: VeterinariaId, Nombre, Correo (validado), Rol, Activo. SIN contraseña/login (eso va en Épica 8).
- `Mascotas/EspecieMascota.cs`, `Mascotas/SexoMascota.cs`.
- `Mascotas/Mascota.cs`: VeterinariaId + ClienteId (relación 1 Cliente → N Mascotas), Nombre, Especie, Raza, Sexo, FechaNacimiento. Método `EdadEnAnios()`.
- `Clientes/Cliente.cs`: **modificado** — ahora requiere `VeterinariaId` (multi-tenant).

Verificación (`dotnet run`): flujo completo Veterinaria → Usuario → Cliente → Mascota persiste y relaciona bien; edad de Firulais = 5 años; validaciones rechazan cliente sin tenant y mascota sin dueño.

### Pendiente
- H2.2b (registro rápido cliente+mascota), H2.3 (listar/buscar): próximas.
- PRs a main los hace el usuario.

### Siguiente paso
- Continuar con casos de uso (Application) o más entidades según decida el usuario.

---

## Sesión 2 (cont.) — 2026-09-26

### [H2.1] Entidad Cliente con validaciones — ✅ COMPLETADA (inicia Épica 2)
- Modelo de Cliente acordado con el usuario:
  - `Nombre` (obligatorio), `Telefono` (obligatorio, base WhatsApp).
  - `FechaRegistro` (auto, UTC) → métricas de crecimiento/retención.
  - `Origen` (enum `OrigenCliente`) → métricas de marketing (cómo nos conoció).
  - Se descartó el correo (poco usado en LATAM).
- Archivos nuevos en **Domain**:
  - `Common/Result.cs` — tipo Result<T> para validaciones sin excepciones.
  - `Clientes/OrigenCliente.cs` — enum de canal de captación.
  - `Clientes/Cliente.cs` — entidad rica: constructor privado + fábrica `Crear` que valida (nombre no vacío, teléfono ≥ 10 dígitos) y normaliza el teléfono a solo dígitos.
- Eliminada `Common/EntidadPrueba.cs` (ya no se necesita).
- `Program.cs` demuestra: cliente válido (teléfono normalizado 777-123-4567 → 7771234567), y 2 casos inválidos rechazados. Total almacenado = 1.
- Verificación: `dotnet run` OK, comportamiento esperado en los 3 casos.

### Siguiente paso
- **H2.2** — entidad Mascota asociada a un Cliente.

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
