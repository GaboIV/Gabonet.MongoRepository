# Gabonet.MongoRepository

Gabonet.MongoRepository es una biblioteca .NET que proporciona una implementación de repositorio genérico para trabajar con MongoDB de manera sencilla y eficiente.

## Características
- Repositorio genérico basado en MongoDB.
- Soporte para operaciones CRUD (
  - Crear (`Create`)
  - Leer (`Get`, `GetAll`)
  - Actualizar (`Update`)
  - Eliminar (`Delete`)
- Filtros avanzados para consultas.
- Integración con .NET y compatibilidad con DI (Dependency Injection).
- Manejo eficiente de colecciones en MongoDB.

## Instalación
Para instalar este paquete en tu proyecto, usa el siguiente comando:

```sh
 dotnet add package Gabonet.MongoRepository --version 0.2.0
```

O a través de NuGet Package Manager:

```powershell
Install-Package Gabonet.MongoRepository -Version 0.2.0
```

{{ edit_1 }}
- La versión 0.1.0 es para MongoDB.Driver 2.19 hasta 2.28.0
- La versión 0.2.0 es para la versión 2.29.0 en adelante hasta 3.2.1.
{{ edit_1 }}

## Uso Básico
### Configuración
Para comenzar, debes configurar la conexión a MongoDB en tu aplicación .NET:

```csharp
var settings = new MongoRepositorySettings
{
    ConnectionString = "mongodb://localhost:27017",
    DatabaseName = "MiBaseDeDatos"
};
var repository = new MongoRepository<MyEntity>(settings);
```

### Operaciones CRUD

#### Insertar un documento
```csharp
await repository.CreateAsync(new MyEntity { Id = "1", Name = "Ejemplo" });
```

#### Obtener un documento por ID
```csharp
var entity = await repository.GetByIdAsync("1");
```

#### Obtener todos los documentos
```csharp
var allEntities = await repository.GetAllAsync();
```

#### Actualizar un documento
```csharp
entity.Name = "Nuevo Nombre";
await repository.UpdateAsync(entity);
```

#### Eliminar un documento
```csharp
await repository.DeleteAsync("1");
```

## Contribuir
Si deseas contribuir a este proyecto, siéntete libre de enviar un **Pull Request** o reportar un **issue** en el repositorio oficial.

## Licencia
Este proyecto está bajo la licencia MIT.

---

> **Nota:** Si tienes algún problema o sugerencia, por favor abre un issue en GitHub.