# EcoTrack — Tachos inteligentes

Sistema web ASP.NET Core 8 para Visual Studio 2022 (17.8 o posterior), con diseño adaptable a celular, autenticación de administrador mediante cookies, dashboard, estados de los tachos e historial persistente de limpieza.

## Abrir en Visual Studio

1. Instala la carga de trabajo **ASP.NET y desarrollo web** y el SDK .NET 8 desde Visual Studio Installer.
2. Abre `TachoInteligente.csproj` con **Archivo → Abrir → Proyecto o solución**.
3. Selecciona el perfil **TachoInteligente** y pulsa **F5**.
4. En desarrollo, ingresa con usuario **admin** y contraseña **Admin123!**.

También puedes ejecutar `dotnet run --project TachoInteligente.csproj` desde la carpeta del proyecto.

## Funcionamiento

- Dashboard con cantidad de tachos, atención requerida, ocupación y limpiezas.
- Tachos con nivel visual y confirmación para registrar una limpieza.
- Notificaciones: vacío (0–15%), disponible (16–69%), casi lleno (70–89%) y lleno (90–100%).
- Historial con fecha, usuario y nivel anterior. Se almacena en `App_Data/state.json`, fuera del control de versiones; conserva esa carpeta para no perder los registros.
- El registro de limpieza actualiza el nivel a 0% y persiste tanto el estado como el historial.

Los cuatro tachos iniciales son datos de demostración. No hay sensores conectados ni recepción automática de mediciones. La integración de hardware requiere definir dispositivos, protocolo y autenticación.

## Credenciales

La contraseña de demostración solo funciona con `ASPNETCORE_ENVIRONMENT=Development`. Para otro entorno configura `ADMIN_PASSWORD`; opcionalmente `ADMIN_USER` (predeterminado: `admin`). En Visual Studio puedes usar **Administrar secretos de usuario** tras inicializarlos con `dotnet user-secrets init`. No publiques con las credenciales de demostración. El acceso tiene un límite de 10 intentos por minuto para la aplicación.

Los datos se almacenan localmente para una instancia. Para un despliegue con múltiples instancias se requiere una base de datos compartida. Usa HTTPS al desplegar y realiza copias de seguridad de los datos.
