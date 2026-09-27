# Fotos de mascota (galería) — Cloudflare R2

Permite subir varias fotos por mascota (galería / "collage"), opcionalmente ligadas a
un registro médico (consulta). Los binarios se almacenan en **Cloudflare R2** (compatible
con S3); la base de datos guarda solo la referencia (clave del objeto + URL pública).

## Arquitectura

- **Dominio:** `FotoMascota` (Mascotas/FotoMascota.cs) — Id, VeterinariaId, MascotaId,
  RegistroMedicoId?, ClaveObjeto, Url, FechaSubida, SubidaPorUsuarioId.
- **Application:**
  - `IAlmacenamientoArchivos` — abstracción del almacenamiento (subir/eliminar).
  - `IFotoMascotaRepository` — persistencia de la galería.
  - `GestionFotoMascota` — casos de uso: `SubirAsync`, `ListarAsync`, `EliminarAsync`
    (aislamiento por veterinaria/tenant en cada uno).
- **Infrastructure:**
  - `AlmacenamientoR2` — sube a R2 vía **AWSSDK.S3**. Antes comprime con **ImageSharp**:
    `AutoOrient` (corrige rotación EXIF), `Resize` (lado máx 1000px), recodifica a **WebP q78**
    y quita EXIF. Una foto de celular de varios MB queda en ~50-120 KB.
  - `AlmacenamientoNulo` — fallback cuando R2 no está configurado (falla al subir).
  - `R2Opciones` — se llena desde variables de entorno.

## Endpoints

| Método | Ruta | Roles |
|---|---|---|
| POST | `/api/mascotas/{id}/fotos` (multipart, campo `archivo`; `?registroMedicoId=` opcional) | Admin, Veterinario, Recepcionista |
| GET | `/api/mascotas/{id}/fotos` | Admin, Veterinario, Recepcionista, DueñoMascota |
| DELETE | `/api/mascotas/{id}/fotos/{fotoId}` | Admin, Veterinario, Recepcionista |

- Límite de entrada: 5 MB (validado en el caso de uso).
- El `usuarioId` sale del claim `sub`/`NameIdentifier`; el `veterinariaId` del claim `veterinariaId`.

## Variables de entorno (Railway → servicio backend)

⚠️ Son secretas (Access Key / Secret). Nunca en el repo.

```
R2_ACCESS_KEY_ID       = <access key del token R2>
R2_SECRET_ACCESS_KEY   = <secret del token R2>
R2_ENDPOINT            = https://<account-id>.r2.cloudflarestorage.com   (sin el bucket)
R2_BUCKET              = chiron-fotos
R2_PUBLIC_URL          = https://pub-xxxxxxxx.r2.dev                     (sin barra final)
```

Si faltan, el backend arranca igual pero usa `AlmacenamientoNulo` (subir falla con mensaje claro).

## Migración EF (IMPORTANTE — generar y commitear)

Este feature agrega la tabla `FotosMascota`, por lo que requiere una migración EF.
El arranque ejecuta `db.Database.Migrate()` automáticamente en Railway, pero el archivo
de migración **debe estar commiteado**. Genérala localmente (requiere el SDK de .NET):

```bash
cd src/Chiron.Infrastructure
dotnet ef migrations add FotosMascota --startup-project ../Chiron.Api
```

Esto crea `Migrations/<timestamp>_FotosMascota.cs` (+ Designer + snapshot actualizado).
Revisa que la migración cree la tabla `FotosMascota` con sus columnas y **commitéala**.
Al desplegar, Railway la aplica sola.

> Nota: la migración no se incluyó en el PR porque se generó en un entorno sin el SDK
> de .NET. Es el único paso manual.
