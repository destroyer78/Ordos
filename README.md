# Ordos
Ordos: Comtrade Manager para cumplimiento del Sistema de Lectura Remota de Protecciones del Coordinador Eléctrico Nacional de Chile.

## Instrucciones

### 1. Descargar e instalar [.Net Core](https://www.microsoft.com/net/download) (x86 o x64).

En el sitio, hacer click sobre la versión que corresponda:<br />
<kbd>
  <img src="Assets/Images/NetCore1.PNG" alt="Net Core Download Step 1" />
</kbd>

### 2. Instalar [SQL Server Express 2014 - LocalDb](https://www.microsoft.com/en-us/download/details.aspx?id=42299) (x86 o x64)

Hacer click sobre el botón de descargar:<br />
<kbd>
  <img src="Assets/Images/SQL1.PNG" alt="SQL Download Step 1" />
</kbd>

Seleccionar la versión que corresponda:<br />
<kbd>
  <img src="Assets/Images/SQL2.PNG" alt="SQL Download Step 2" />
</kbd>

### 3. [Clonar](https://help.github.com/articles/cloning-a-repository/) o [descargar](https://github.com/gabrieldelaparra/Ordos/archive/master.zip) el repositorio.

Clonar o descargar:<br />
<kbd>
  <img src="Assets/Images/Github1.PNG" alt="Repo Step 1" />
</kbd>

### 4. Entrar a la carpeta `Ordos` (en la consola: `cd Ordos`)

### 5. En la consola, ejecutar `build`

Build:<br />
<kbd>
  <img src="Assets/Images/Build1.PNG" alt="Build progress" />
</kbd>

### 6. En la consola, ejecutar `ordos`

Run:<br />
<kbd>
  <img src="Assets/Images/Run1.PNG" alt="Run messages" />
</kbd>

Las oscilografías se extraen a la carpeta `C:\Ordos\` según los requerimientos del SLRP.

## Contribuciones
La herramienta está en versión beta.

Cualquier duda o comentario, pueden [crear un Issue](https://help.github.com/articles/creating-an-issue/).

## Dependencias
Este proyecto utiliza las siguientes dependencias: 
- [.Net Core](https://github.com/dotnet/core)
- [ASP.NET Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore)
- [libiec61850](https://github.com/mz-automation/libiec61850)
- [Fluent Scheduler](https://github.com/fluentscheduler/FluentScheduler)
- [PostgreSQL](https://github.com/postgres/postgres)
- [NLog](https://github.com/NLog/NLog)
- [xUnit](https://github.com/xunit/xunit)

## Licenciamiento
Este proyecto está bajo la licencia [GNU General Public License Version 3](https://github.com/gabrieldelaparra/Ordos/blob/master/LICENSE.MD)

Las dependencias ocupadas tienen las siguientes licencias:
- [.Net Core](https://github.com/dotnet/core/blob/master/LICENSE.TXT): MIT License
- [ASP.NET Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore/blob/master/LICENSE.txt): Apache License 2.0
- [libiec61850](https://github.com/mz-automation/libiec61850/blob/v1.3/COPYING): GNU General Public License Version 3
- [Fluent Scheduler](https://github.com/fluentscheduler/FluentScheduler/blob/master/LICENSE): BSD 3-Clause "New" or "Revised" License
- [PostgreSQL](https://github.com/postgres/postgres/blob/master/COPYRIGHT): PostgreSQL License
- [NLog](https://github.com/NLog/NLog/blob/dev/LICENSE.txt): BSD 3-Clause "New" or "Revised" License
- [xUnit](https://github.com/xunit/xunit/blob/master/license.txt): Apache License 2.0
