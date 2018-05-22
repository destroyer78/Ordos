# Ordos
Ordos: Comtrade Manager para cumplimiento del Sistema de Lectura Remota de Protecciones del Coordinador Eléctrico Nacional de Chile

## Instrucciones

1. Descargar e instalar [.Net Core](https://www.microsoft.com/net/download)

2. Instalar [SQL Server Express 2016 - LocalDb](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-2016-express-localdb) 

3. [Clonar](https://help.github.com/articles/cloning-a-repository/) o [descargar](https://stackoverflow.com/a/6466993) el repositorio.

4. Entrar a la carpeta `Ordos` (en la consola: `cd Ordos`)

5. En la consola, ejecutar `dotnet build`

6. Entrar a la carpeta `Ordos.Server` (`cd Ordos.Server`)

7. En la consola, ejecutar `dotnet run`

Las oscilografías se extraen a la carpeta `C:\Ordos\` según los requerimientos del SLRP.

## Contribuciones
La herramienta está en versión beta.

Cualquier duda o comentario, pueden [crear un Issue](https://help.github.com/articles/creating-an-issue/).
