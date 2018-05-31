# Ordos
Ordos: Comtrade Manager para cumplimiento del Sistema de Lectura Remota de Protecciones del Coordinador Eléctrico Nacional de Chile.

## Instrucciones

### 1. Descargar e instalar [.Net Core](https://www.microsoft.com/net/download) (x86 o x64).

![Net Core Download Step 1](Assets/Images/NetCore1.PNG)

### 2. Instalar [SQL Server Express 2014 - LocalDb](https://www.microsoft.com/en-us/download/details.aspx?id=42299) (x86 o x64)

![SQL Download Step 1](Assets/Images/SQL1.PNG)

![SQL Download Step 2](Assets/Images//SQL2.PNG)

### 3. [Clonar](https://help.github.com/articles/cloning-a-repository/) o [descargar](https://stackoverflow.com/a/6466993) el repositorio.

![Repo Step 1](Assets/Images//Github1.PNG)

### 4. Entrar a la carpeta `Ordos` (en la consola: `cd Ordos`)

### 5. En la consola, ejecutar `build`

![Build progress](Assets/Images/Build1.PNG)

### 6. En la consola, ejecutar `ordos`
![Run messages](Assets/Images/Run1.PNG)

Las oscilografías se extraen a la carpeta `C:\Ordos\` según los requerimientos del SLRP.

## Contribuciones
La herramienta está en versión beta.

Cualquier duda o comentario, pueden [crear un Issue](https://help.github.com/articles/creating-an-issue/).

## Dependencias
Este proyecto utiliza las siguientes dependencias: 
- [lib61850](https://github.com/mz-automation/libiec61850) 
- [Fluent Scheduler](https://github.com/fluentscheduler/FluentScheduler)
