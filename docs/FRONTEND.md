# 🐾 Chiron — Plan del Frontend

> Guía para desarrollar el frontend en una sesión nueva (repo separado `chiron-web`).
> Creado: 2026-09-27

---

## Decisiones tomadas

| Decisión | Elección | Razón |
|---|---|---|
| Tecnología | **React + Vite** | Rápido, ligero, aprovecha que el dev sabe Node; desarrollo por componentes |
| Tipo | **SPA + PWA** | Multiplataforma (web/celular sin instalar); PWA habilita instalación y push |
| Repo | **Separado** (`chiron-web`) | Más limpio; despliegue independiente del backend |
| Hosting | **Vercel** (o Netlify) | Gratis, deploy automático desde GitHub |
| Conexión | Consume la **API REST** en Railway | `https://chiron-vet-production.up.railway.app` |

> Se descartó Flutter (curva de Dart, orientado a móvil nativo, más fricción para demo web).
> Se descartó Blazor (el dev prefiere aprovechar Node/JS).

---

## Requisito previo en el BACKEND: CORS

El frontend estará en otro dominio (ej. `chiron.vercel.app`), así que la API debe permitir
CORS para ese origen. **Pendiente en el repo backend (chiron-vet):**

- Agregar en `Program.cs` de la API una política CORS que permita el dominio del frontend.
- En desarrollo: permitir `http://localhost:5173` (puerto por defecto de Vite).
- En producción: permitir el dominio de Vercel/Netlify.
- Colocar `app.UseCors(...)` antes de `UseAuthentication/UseAuthorization`.

---

## Backlog del Frontend (por épicas)

### ÉPICA F1 — Setup y autenticación
- F1.1: Crear proyecto React + Vite en repo `chiron-web`.
- F1.2: Estructura base (carpetas: pages, components, services, context).
- F1.3: Servicio de API (fetch/axios) con base URL configurable (variable de entorno).
- F1.4: **Pantalla de login** (identificador + PIN) → llama a `POST /api/auth/login`.
- F1.5: Guardar el token JWT (memoria/localStorage) y enviarlo en cada petición (header `Authorization: Bearer`).
- F1.6: Manejo de sesión: rutas protegidas, logout, redirección según rol.

### ÉPICA F2 — Layout y navegación por rol
- F2.1: Layout general (menú lateral/superior) responsivo (móvil + escritorio).
- F2.2: Menús distintos según rol (Admin, Veterinario, Recepcionista, Dueño, SuperAdmin).
- F2.3: Dashboard inicial por rol.

### ÉPICA F3 — Módulos de staff
- F3.1: Registro rápido (cliente + mascota).
- F3.2: Listado/búsqueda de clientes y mascotas.
- F3.3: Expediente de mascota (ver + agregar consultas/vacunas).
- F3.4: Agenda de citas.
- F3.5: Punto de venta (catálogo + registrar venta).

### ÉPICA F4 — Portal del dueño de mascota
- F4.1: Login del dueño (teléfono + PIN).
- F4.2: Ver mis mascotas + su expediente.
- F4.3: Ver mis recordatorios (in-app).

### ÉPICA F5 — Panel SuperAdmin
- F5.1: Crear/listar veterinarias.
- F5.2: Crear admin de veterinaria.
- F5.3: Activar/desactivar veterinarias (control de suscripción).

### ÉPICA F6 — PWA y notificaciones push
- F6.1: Configurar PWA (manifest + service worker) → instalable en celular.
- F6.2: Suscripción a notificaciones push (Web Push API).
- F6.3: Backend: guardar suscripciones push + enviar (implementa H10.3, requiere endpoint nuevo en la API).

---

## Endpoints de la API disponibles (referencia para el frontend)

Base: `https://chiron-vet-production.up.railway.app`

**Auth (público):**
- `POST /api/auth/login` → body `{ "identificador": "...", "pin": "......" }` → devuelve `{ token, expiraEn, nombre, rol }`

**SuperAdmin (rol 99):**
- `POST /api/admin/veterinarias` → `{ nombre, telefono }`
- `POST /api/admin/usuarios-admin` → `{ veterinariaId, nombreUsuario, nombre, pin, rol }`
- `POST /api/admin/veterinarias/{id}/activar`
- `POST /api/admin/veterinarias/{id}/desactivar`
- `GET  /api/admin/veterinarias`

**Gestión de usuarios (Administrador):**
- `POST /api/usuarios/staff` → `{ nombreUsuario, nombre, pin, rol }` (rol Veterinario=2 o Recepcionista=3)
- `POST /api/usuarios/dueno` → `{ clienteId, pin }`

**Staff (Admin/Veterinario/Recepcionista):**
- `POST /api/registro-rapido` → cliente + mascota
- `GET  /api/veterinarias/{veterinariaId}/clientes?texto=...`
- `GET  /api/clientes/{clienteId}/mascotas`
- `POST /api/expediente` (Admin/Veterinario) · `GET /api/mascotas/{mascotaId}/expediente`
- `POST /api/citas` · `GET /api/veterinarias/{veterinariaId}/citas/proximas`
- `POST /api/productos` (Admin) · `GET /api/veterinarias/{veterinariaId}/catalogo` · `POST /api/ventas` (Admin/Recepcionista)
- `POST /api/veterinarias/{veterinariaId}/recordatorios/enviar` (Admin)

**Portal del dueño (rol DuenoMascota=4):**
- `GET /api/portal/mis-mascotas`
- `GET /api/portal/mascotas/{mascotaId}/expediente`
- `GET /api/portal/mis-recordatorios`

> Todos (excepto login) requieren header `Authorization: Bearer <token>`.
> Valores de rol: Administrador=1, Veterinario=2, Recepcionista=3, DuenoMascota=4, SuperAdmin=99.

---

## Credenciales de prueba (solo modo EN MEMORIA / local del backend)
- SuperAdmin: `superadmin` / `123456`
- Admin: `admindemo` / `654321`
- Dueño: `7771234567` / `111222`

> En producción NO existen estos usuarios; ahí está el SuperAdmin real creado por variables de entorno.

---

## Primer paso de la próxima sesión
1. En el repo `chiron-web`: crear proyecto React + Vite (`npm create vite@latest`).
2. Configurar variable de entorno `VITE_API_URL` con la URL de Railway.
3. Construir la pantalla de **login** (F1.4) y probar contra la API real.
4. En paralelo (repo backend): agregar **CORS** para el dominio del frontend.
