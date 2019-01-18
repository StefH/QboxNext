# QboxNext
This project is based on https://bitbucket.org/qboxnext/dotnetcore-minimal all credits for the original source go to Sebastiaan.

## NuGet Packages

| NuGet | Info |
| --- | --- |
| [![NuGet Badge](https://buildstats.info/nuget/QboxNext)](https://www.nuget.org/packages/QboxNext) | Original source code from [qboxnext](https://bitbucket.org/qboxnext/dotnetcore-minimal) combined in one library.
| [![NuGet Badge](https://buildstats.info/nuget/QboxNext.Extensions)](https://www.nuget.org/packages/QboxNext.Extensions) | Additional code, see list below for the changes.

## QboxNext.Extensions
todo
todo
<hr>

## Example Project

### QBoxNext.WebApi
WebApi which can receive messages send by the Qbox.

### QBoxNext.Business
This project is a DI compatible business project which stores the received measurements data from Qbox.

### QboxNext.Domain
Domain model.

### QboxNext.Common
Common logic.

### QboxNext.Infrastructure.Azure
This project stores the measurements in Azure Tables.