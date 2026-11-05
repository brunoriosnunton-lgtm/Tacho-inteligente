@echo off
cd /d "%~dp0"
echo EcoTrack - Usuario: ADMIN - Clave: 1234
echo Cuando aparezca Application started, abre http://localhost:5081
echo Mantiene esta ventana abierta mientras usas el sistema.
dotnet run --project TachoInteligente.csproj --urls "http://localhost:5081"
pause
