# Temario de C# — módulo `sale`

Documento tipo **temario**: temas de C# y .NET que aparecen en [`pc2/sale`](.) ordenados de básico a avanzado. Cada tema enlaza al archivo del repositorio (puedes abrirlo en GitHub sin clonar).

**Ubicación de este temario:** [`pc2/sale/CONOCIMIENTOS_CSHARP.md`](CONOCIMIENTOS_CSHARP.md)

**Vista en GitHub (rama `main`):**  
`https://github.com/arturo-ns/lectura/blob/main/pc2/sale/CONOCIMIENTOS_CSHARP.md`

> Si el enlace anterior aún no existe en `main`, abre el mismo path en la rama donde esté el archivo (`cursor/conocimientos-csharp-sale-d7e1` o la rama del PR).

---

## Índice

| Parte | Nivel | Temas |
|-------|--------|--------|
| [I](#parte-i--fundamentos-del-lenguaje-c) | Básico | 1–8 |
| [II](#parte-ii--orientación-a-objetos-y-contratos) | Intermedio | 9–14 |
| [III](#parte-iii--api-web-y-capa-de-interfaces) | Intermedio | 15–18 |
| [IV](#parte-iv--persistencia-con-entity-framework-core) | Avanzado | 19–23 |
| [V](#parte-v--arquitectura-aplicación-y-dominio) | Avanzado | 24–28 |
| [VI](#parte-vi--ensamblado-del-módulo-en-el-host) | Avanzado | 29 |
| [Anexo](#anexo--mapa-de-archivos-por-tema) | — | Referencia rápida |

---

## Parte I — Fundamentos del lenguaje C#

### Tema 1. Tipos de datos y propiedades

**Qué aparece en `sale`:** enteros, texto, fechas, decimales y enumeraciones en entidades y DTOs.

**Archivos:** [`domain/model/agreggates/bill.cs`](domain/model/agreggates/bill.cs) · [`interfaces/REST/resources/BillResource.cs`](interfaces/REST/resources/BillResource.cs)

```7:14:pc2/sale/domain/model/agreggates/bill.cs
    public int BillNumber { get; set; }
    public string Customer { get; set; } = string.Empty;
    public EService ServiceId { get; set; }
    public string Plate { get; set; } = string.Empty;
    public DateTime Emission { get; set; }
    public Invoice Invoice { get; set; } = new Invoice();
    public double Amount { get; set; }
    public string Adviser { get; set; } = string.Empty;
```

- `int`, `string`, `DateTime`, `double`: tipos por valor o referencia según el caso.
- `= string.Empty` y `= new Invoice()`: inicialización para evitar `null` con nullable activado en el proyecto.

---

### Tema 2. Enumeraciones (`enum`)

**Qué aparece en `sale`:** catálogo cerrado de servicios con códigos numéricos fijos (10, 20, 30, 40).

**Archivo:** [`domain/model/valueobjects/EService.cs`](domain/model/valueobjects/EService.cs)

```3:9:pc2/sale/domain/model/valueobjects/EService.cs
public enum EService
{
    MaintenanceService = 10,
    BodyworkAndPaint = 20,
    Accessories = 30,
    SpareParts = 40
}
```

Se usa en el body JSON (`ServiceId`), en validación (`Enum.IsDefined`) y en BD como entero (`HasConversion<int>()` en el contexto EF).

---

### Tema 3. Namespaces y `using`

**Qué aparece en `sale`:** cada capa tiene su namespace bajo `pc2_7420_u20231f226.sale.*`.

**Ejemplo:** [`application/BillCommandService.cs`](application/BillCommandService.cs) (líneas 1–7)

```1:7:pc2/sale/application/BillCommandService.cs
using pc2_7420_u20231f226.sale.domain.model.agreggates;
using pc2_7420_u20231f226.sale.domain.model.valueobjects;
using pc2_7420_u20231f226.sale.domain.repositories;
using pc2_7420_u20231f226.sale.domain.service;
using pc2_7420_u20231f226.sale.interfaces.REST.resources;

namespace pc2_7420_u20231f226.sale.application;
```

Organiza dependencias entre capas: `application` importa `domain` e `interfaces`, no al revés de forma circular en el diseño ideal.

---

### Tema 4. Clases simples (objetos de valor en código)

**Qué aparece en `sale`:** `Invoice` agrupa serie y correlativo sin identidad propia en el modelo de dominio.

**Archivo:** [`domain/model/valueobjects/Invoice.cs`](domain/model/valueobjects/Invoice.cs)

```3:8:pc2/sale/domain/model/valueobjects/Invoice.cs
public class Invoice
{
    public string SerialNumber { get; set; } = string.Empty;
    
    public string SequentialNumber { get; set; } = string.Empty;
}
```

En DTOs de API esos campos van “aplanados” (`InvoiceSerialNumber` / `InvoiceSequentialNumber`); en dominio van anidados dentro de `bill`.

---

### Tema 5. Herencia entre clases

**Qué aparece en `sale`:** campos de auditoría reutilizados en la entidad factura.

**Archivos:** [`domain/model/agreggates/bilordersaudit.cs`](domain/model/agreggates/bilordersaudit.cs) · [`domain/model/agreggates/bill.cs`](domain/model/agreggates/bill.cs)

```5:12:pc2/sale/domain/model/agreggates/bilordersaudit.cs
public partial class bilordersaudit
{
    [Column("CreatedAt")] 
    public DateTime CreatedDate { get; set; }
    
    [Column("UpdatedAt")] 
    public DateTime UpdatedDate { get; set; }
```

```5:6:pc2/sale/domain/model/agreggates/bill.cs
public partial class bill : bilordersaudit
{
```

`bill` hereda `CreatedDate` y `UpdatedDate` sin duplicar propiedades.

---

### Tema 6. Clases `partial`

**Qué aparece en `sale`:** `bill` y `bilordersaudit` declaradas como `partial` para poder extender la misma clase en otro archivo (generadores, separación de concerns).

**Archivo:** [`domain/model/agreggates/bill.cs`](domain/model/agreggates/bill.cs) — palabra clave `partial` en la declaración de clase.

---

### Tema 7. Atributos (`[...]`)

**Qué aparece en `sale`:**

| Atributo | Dónde | Para qué |
|----------|--------|----------|
| `[Column("CreatedAt")]` | `bilordersaudit.cs` | Nombre de columna SQL distinto al de la propiedad C# |
| `[ApiController]`, `[Route]`, `[HttpPost]` | `BillsController.cs` | Convenciones ASP.NET Core |
| `[FromBody]` | `BillsController.cs` | Leer JSON del cuerpo de la petición |
| `[SwaggerOperation]`, `[ProducesResponseType]` | `BillsController.cs` | Documentación OpenAPI |

Fragmento REST: [`interfaces/REST/controllers/BillsController.cs`](interfaces/REST/controllers/BillsController.cs)

```14:16:pc2/sale/interfaces/REST/controllers/BillsController.cs
[ApiController]
[Route("api/v1/bills")]
public class BillsController : ControllerBase
```

---

### Tema 8. Comentarios XML (`///`)

**Qué aparece en `sale`:** documentación de API y contratos para Swagger/IntelliSense.

**Archivo:** [`interfaces/REST/controllers/BillsController.cs`](interfaces/REST/controllers/BillsController.cs) (bloque `/// <summary>` del método `CreateBill`).

---

## Parte II — Orientación a objetos y contratos

### Tema 9. Interfaces

**Qué aparece en `sale`:** contratos que ocultan implementación (servicio de aplicación y repositorio).

**Archivos:**  
- [`domain/service/IbillCommandService.cs`](domain/service/IbillCommandService.cs)  
- [`domain/repositories/IbillRepository.cs`](domain/repositories/IbillRepository.cs)

```11:18:pc2/sale/domain/service/IbillCommandService.cs
public interface IBillCommandService
{
    /// <summary>
    /// Handles the creation of a new bill
    /// </summary>
    Task<BillResource> Handle(CreateBillResource resource);
}
```

```5:12:pc2/sale/domain/repositories/IbillRepository.cs
public interface IbillRepository
{
    Task<bill> AddAsync(bill bill);
    // ...
    Task<bill?> GetByBillNumberAsync(int billNumber);
}
```

---

### Tema 10. Implementación de interfaces

**Qué aparece en `sale`:** clases concretas que cumplen el contrato.

| Interfaz | Implementación |
|----------|----------------|
| `IBillCommandService` | [`application/BillCommandService.cs`](application/BillCommandService.cs) |
| `IbillRepository` | [`infrastructure/persistence/EFC/repositories/BillRepository.cs`](infrastructure/persistence/EFC/repositories/BillRepository.cs) |

```9:10:pc2/sale/application/BillCommandService.cs
public class BillCommandService : IBillCommandService
{
```

---

### Tema 11. Inyección de dependencias por constructor

**Qué aparece en `sale`:** el framework inyecta dependencias al crear controlador, servicio y repositorio.

**Ejemplo controlador:** [`interfaces/REST/controllers/BillsController.cs`](interfaces/REST/controllers/BillsController.cs)

```18:27:pc2/sale/interfaces/REST/controllers/BillsController.cs
    private readonly IBillCommandService _commandService;

    public BillsController(IBillCommandService commandService)
    {
        _commandService = commandService;
    }
```

**Ejemplo servicio:** [`application/BillCommandService.cs`](application/BillCommandService.cs) — recibe `IbillRepository`.

El registro de parejas interfaz→clase está en [`../Program.cs`](../Program.cs) (fuera de `sale`, pero indispensable para el módulo).

---

### Tema 12. `async` / `await` y `Task<T>`

**Qué aparece en `sale`:** toda la cadena de creación de factura es asíncrona (no bloquea hilos mientras espera MySQL).

**Cadena de archivos:**

1. [`BillsController.cs`](interfaces/REST/controllers/BillsController.cs) — `async Task<IActionResult>`
2. [`BillCommandService.cs`](application/BillCommandService.cs) — `async Task<BillResource> Handle(...)`
3. [`BillRepository.cs`](infrastructure/persistence/EFC/repositories/BillRepository.cs) — `AddAsync`, `AnyAsync`, `SaveChangesAsync`

```45:50:pc2/sale/interfaces/REST/controllers/BillsController.cs
    public async Task<IActionResult> CreateBill([FromBody] CreateBillResource resource)
    {
        try
        {
            var result = await _commandService.Handle(resource);
            return CreatedAtAction(nameof(CreateBill), new { billNumber = result.BillNumber }, result);
```

---

### Tema 13. Tipos anulables (`bill?`)

**Qué aparece en `sale`:** el repositorio indica que puede no existir una factura.

**Archivo:** [`domain/repositories/IbillRepository.cs`](domain/repositories/IbillRepository.cs)

```27:27:pc2/sale/domain/repositories/IbillRepository.cs
    Task<bill?> GetByBillNumberAsync(int billNumber);
```

Con `<Nullable>enable</Nullable>` en el `.csproj`, `bill?` documenta “puede ser null” en el contrato.

---

### Tema 14. Excepciones y reglas de negocio

**Qué aparece en `sale`:** validaciones lanzan `ArgumentException`; duplicado de factura lanza `InvalidOperationException`.

**Archivo:** [`application/BillCommandService.cs`](application/BillCommandService.cs)

```29:31:pc2/sale/application/BillCommandService.cs
        if (string.IsNullOrWhiteSpace(resource.Customer) || resource.Customer.Length > 100)
            throw new ArgumentException("Customer is required and must be maximum 100 characters.");
```

```61:63:pc2/sale/application/BillCommandService.cs
        if (await _repository.ExistByInvoiceAsync(resource.InvoiceSerialNumber, resource.InvoiceSequentialNumber))
            throw new InvalidOperationException("A bill with the same invoice already exists.");
```

El controlador las traduce a HTTP 400 y 409.

---

## Parte III — API web y capa de interfaces

### Tema 15. Controladores REST (`ControllerBase`)

**Qué aparece en `sale`:** un endpoint POST para crear facturas.

**Archivo:** [`interfaces/REST/controllers/BillsController.cs`](interfaces/REST/controllers/BillsController.cs)

- Ruta base: `api/v1/bills`
- Acción: `CreateBill` con verbo HTTP POST
- Respuesta exitosa: **201 Created** con `CreatedAtAction`

---

### Tema 16. DTOs / Resources (contrato JSON)

**Qué aparece en `sale`:** tipos solo para entrada/salida HTTP, separados de la entidad `bill`.

| DTO | Rol | Archivo |
|-----|-----|---------|
| `CreateBillResource` | Body del POST (incluye placa y asesor) | [`CreateBillResource.cs`](interfaces/REST/resources/CreateBillResource.cs) |
| `BillResource` | Respuesta (sin placa ni asesor) | [`BillResource.cs`](interfaces/REST/resources/BillResource.cs) |
| `InvoiceResource` | Estructura alternativa de factura (definida, poco usada en el flujo actual) | [`InvoiceResource.cs`](interfaces/REST/resources/InvoiceResource.cs) |

---

### Tema 17. Manejo de errores HTTP en el controlador

**Qué aparece en `sale`:** `try/catch` por tipo de excepción → código de estado.

**Archivo:** [`interfaces/REST/controllers/BillsController.cs`](interfaces/REST/controllers/BillsController.cs)

```52:59:pc2/sale/interfaces/REST/controllers/BillsController.cs
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
```

| Excepción | HTTP |
|-----------|------|
| `ArgumentException` | 400 Bad Request |
| `InvalidOperationException` | 409 Conflict |
| Otras | 500 Internal Server Error |

---

### Tema 18. Documentación Swagger (OpenAPI)

**Qué aparece en `sale`:** metadatos en el endpoint para UI Swagger.

**Archivo:** [`interfaces/REST/controllers/BillsController.cs`](interfaces/REST/controllers/BillsController.cs)

```38:44:pc2/sale/interfaces/REST/controllers/BillsController.cs
    [SwaggerOperation(
        Summary = "Create a new bill",
        Description = "Creates a new bill with the provided data. Returns the created bill without plate and adviser information."
    )]
    [ProducesResponseType(typeof(BillResource), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
```

Configuración global de Swagger: [`../Program.cs`](../Program.cs).

---

## Parte IV — Persistencia con Entity Framework Core

### Tema 19. `DbContext` y `DbSet<T>`

**Qué aparece en `sale`:** `BillContext` es la sesión de EF contra MySQL para la tabla de facturas.

**Archivo:** [`infrastructure/persistence/EFC/context/billContext.cs`](infrastructure/persistence/EFC/context/billContext.cs)

```7:11:pc2/sale/infrastructure/persistence/EFC/context/billContext.cs
public class BillContext : DbContext
{
    public DbSet<bill> Bills => Set<bill>();

    public BillContext(DbContextOptions<BillContext> options) : base(options) { }
```

- `DbSet<bill>`: colección consultable (`AddAsync`, LINQ).
- Constructor con `DbContextOptions<BillContext>`: patrón estándar para inyectar cadena de conexión desde `Program.cs`.

---

### Tema 20. Configuración con Fluent API (`OnModelCreating`)

**Qué aparece en `sale`:** reglas de tabla, claves, longitudes y tipos SQL en código, no solo con atributos.

**Archivo:** [`infrastructure/persistence/EFC/context/billContext.cs`](infrastructure/persistence/EFC/context/billContext.cs)

```21:28:pc2/sale/infrastructure/persistence/EFC/context/billContext.cs
        modelBuilder.Entity<bill>(entity =>
        {
            // Primary key
            entity.HasKey(b => b.BillNumber);
            
            // Auto-generate bill number
            entity.Property(b => b.BillNumber)
                  .ValueGeneratedOnAdd();
```

| API | Efecto en `sale` |
|-----|------------------|
| `HasKey` | PK `BillNumber` |
| `ValueGeneratedOnAdd` | Autoincremento al insertar |
| `IsRequired` / `HasMaxLength` | Restricciones de columnas |
| `HasColumnType("decimal(18,2)")` | Monto con precisión en BD |
| `HasConversion<int>()` | `EService` guardado como entero |
| `ToTable("bills")` | Nombre de tabla |

---

### Tema 21. Tipos owned (`OwnsOne`) — **avanzado**

**Qué aparece en `sale`:** `Invoice` no es una tabla separada; sus columnas viven en la fila de `bills`.

**Archivo:** [`infrastructure/persistence/EFC/context/billContext.cs`](infrastructure/persistence/EFC/context/billContext.cs)

```46:56:pc2/sale/infrastructure/persistence/EFC/context/billContext.cs
            entity.OwnsOne(b => b.Invoice, invoice =>
            {
                invoice.Property(i => i.SerialNumber)
                       .IsRequired()
                       .HasMaxLength(10);
                       
                invoice.Property(i => i.SequentialNumber)
                       .IsRequired()
                       .HasMaxLength(10);
            });
```

**Por qué es avanzado:** modela un *value object* DDD en un esquema relacional sin FK a otra entidad. Las consultas LINQ usan `b.Invoice.SerialNumber` como si fuera objeto anidado; EF genera columnas embebidas (p. ej. `invoice_serial_number` con snake_case).

**Consulta relacionada:** duplicados en [`BillRepository.cs`](infrastructure/persistence/EFC/repositories/BillRepository.cs):

```26:28:pc2/sale/infrastructure/persistence/EFC/repositories/BillRepository.cs
        return await _context.Bills
            .AnyAsync(b => b.Invoice.SerialNumber == serialNumber && 
                           b.Invoice.SequentialNumber == sequentialNumber);
```

`AnyAsync` se traduce a SQL tipo `EXISTS` — eficiente para comprobar unicidad.

---

### Tema 22. Repositorio con LINQ asíncrono

**Qué aparece en `sale`:** implementación concreta del puerto de persistencia.

**Archivo:** [`infrastructure/persistence/EFC/repositories/BillRepository.cs`](infrastructure/persistence/EFC/repositories/BillRepository.cs)

```17:21:pc2/sale/infrastructure/persistence/EFC/repositories/BillRepository.cs
    public async Task<bill> AddAsync(bill bill)
    {
        await _context.Bills.AddAsync(bill);
        await _context.SaveChangesAsync();
        return bill;
```

`SaveChangesAsync` confirma la transacción; tras el insert, `BillNumber` queda rellenado por la BD (`ValueGeneratedOnAdd`).

---

### Tema 23. Convención snake_case (método de extensión) — **avanzado**

**Qué aparece en `sale`:** nombres C# en PascalCase/camelCase mapeados a columnas `snake_case` en MySQL.

**Uso en contexto:** [`billContext.cs`](infrastructure/persistence/EFC/context/billContext.cs) — `modelBuilder.UserSnakeCaseNamingConventions();`

**Implementación compartida:** [`../shared/Persistence/EFC/Extentions/NamingConventionsExtension.cs`](../shared/Persistence/EFC/Extentions/NamingConventionsExtension.cs)

```8:18:pc2/shared/Persistence/EFC/Extentions/NamingConventionsExtension.cs
    public static void UserSnakeCaseNamingConventions(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.DisplayName()));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }
    }
```

**Conceptos C# implicados:**

- **Método de extensión** (`this ModelBuilder builder`): se llama como si fuera método nativo de EF.
- **Regex** para insertar `_` entre minúsculas y mayúsculas (`BillNumber` → `bill_number`).

Se combina con `[Column("CreatedAt")]` en auditoría cuando el nombre final debe ser exacto (`created_at`).

---

## Parte V — Arquitectura, aplicación y dominio

### Tema 24. Capas del módulo (estructura de carpetas)

```
sale/
├── interfaces/REST/     → HTTP (controllers, resources, transform)
├── application/           → Casos de uso (BillCommandService)
├── domain/                → Modelo + contratos (IbillRepository, IBillCommandService)
└── infrastructure/        → EF Core (BillContext, BillRepository)
```

**Flujo del caso de uso “crear factura”:**

`BillsController` → `BillCommandService.Handle` → `BillRepository` → `BillContext` → MySQL

---

### Tema 25. Patrón Command / servicio de aplicación — **avanzado**

**Qué aparece en `sale`:** un método `Handle` concentra un caso de uso transaccional (validar + persistir + devolver DTO).

**Archivo:** [`application/BillCommandService.cs`](application/BillCommandService.cs)

Responsabilidades en un solo lugar:

1. Validar reglas (longitudes, monto, fecha, enum, unicidad).
2. Materializar entidad `bill` con auditoría UTC.
3. Persistir vía repositorio.
4. Proyectar a `BillResource` (respuesta acotada).

**Profundización:** el controlador no conoce EF ni SQL; solo conoce `IBillCommandService`. Eso es **separación de responsabilidades** y facilita pruebas unitarias del servicio con un repositorio falso.

---

### Tema 26. Validación de enum en tiempo de ejecución — **avanzado**

**Qué aparece en `sale`:** JSON puede deserializar un entero que no está en el enum; se rechaza explícitamente.

**Archivo:** [`application/BillCommandService.cs`](application/BillCommandService.cs)

```49:51:pc2/sale/application/BillCommandService.cs
        if (!Enum.IsDefined(typeof(EService), resource.ServiceId))
            throw new ArgumentException("Invalid service ID.");
```

Sin esto, un valor como `99` podría guardarse si EF no validara el dominio. Aquí la regla es de **negocio**, no solo de serialización.

---

### Tema 27. Ensamblador estático (mapeo entre capas) — **avanzado**

**Qué aparece en `sale`:** clase `static` sin estado que convierte entidad ↔ DTO.

**Archivo:** [`interfaces/REST/transform/billResourceAssembler.cs`](interfaces/REST/transform/billResourceAssembler.cs)

```20:30:pc2/sale/interfaces/REST/transform/billResourceAssembler.cs
    public static BillResource ToResource(bill bill) => new()
    {
        BillNumber = bill.BillNumber,
        Customer = bill.Customer,
        ServiceId = bill.ServiceId,
        Emission = bill.Emission,
        InvoiceSerialNumber = bill.Invoice.SerialNumber,
        InvoiceSequentialNumber = bill.Invoice.SequentialNumber,
        Amount = bill.Amount
        // Note: Plate, Adviser, and audit fields are intentionally excluded as per requirements
    };
```

**Sintaxis moderna C#:**

- `=> new() { ... }` — método con cuerpo de expresión y `target-typed new`.
- Comentario de intención: qué campos **no** se exponen al cliente.

Hoy `BillCommandService` también mapea inline; el assembler centraliza el mismo criterio para otros endpoints futuros.

**Método inverso:** `ToEntity(CreateBillResource)` en el mismo archivo — crea `bill` + `Invoice` + fechas UTC.

---

### Tema 28. Conceptos DDD usados en el código — **avanzado**

| Concepto DDD | En `sale` | Archivo |
|--------------|-----------|---------|
| **Entidad / agregado** | `bill` con identidad `BillNumber` | [`bill.cs`](domain/model/agreggates/bill.cs) |
| **Value object** | `Invoice`, `EService` | [`Invoice.cs`](domain/model/valueobjects/Invoice.cs), [`EService.cs`](domain/model/valueobjects/EService.cs) |
| **Repositorio** | `IbillRepository` | [`IbillRepository.cs`](domain/repositories/IbillRepository.cs) |
| **Servicio de dominio/aplicación** | `IBillCommandService` | [`IbillCommandService.cs`](domain/service/IbillCommandService.cs) |
| **Anti-corruption / DTO** | `CreateBillResource`, `BillResource` | carpeta [`interfaces/REST/resources/`](interfaces/REST/resources/) |

**Nota de diseño:** `IBillCommandService` vive en `domain` pero devuelve `BillResource` (capa interfaces). Es un acoplamiento pragmático del template; en un DDD estricto el servicio devolvería un tipo de dominio y el controlador mapearía al DTO.

---

## Parte VI — Ensamblado del módulo en el host

### Tema 29. Registro en DI y arranque (relacionado con `sale`)

No está dentro de `sale/`, pero sin esto el módulo no corre. Archivo: [`../Program.cs`](../Program.cs)

```29:35:pc2/Program.cs
builder.Services.AddDbContext<BillContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))));

builder.Services.AddScoped<IBillCommandService, BillCommandService>();
builder.Services.AddScoped<IbillRepository, BillRepository>();
```

| Registro | Concepto |
|----------|----------|
| `AddDbContext<BillContext>` | EF + proveedor **Pomelo** para MySQL 8 |
| `AddScoped<...>` | Misma instancia por petición HTTP (alineado con un `SaveChanges` por request) |
| `EnsureCreated()` | Crea BD/esquema al arrancar (desarrollo; en producción suelen usarse migraciones) |

**Stack del `.csproj`:** [`../pc2-7420-u20231f226.csproj`](../pc2-7420-u20231f226.csproj) — .NET 9, EF Core 8, Swashbuckle, nullable e implicit usings.

---

## Anexo — Mapa de archivos por tema

| Tema | Archivo en el repo |
|------|---------------------|
| Entidad factura | [`domain/model/agreggates/bill.cs`](domain/model/agreggates/bill.cs) |
| Auditoría | [`domain/model/agreggates/bilordersaudit.cs`](domain/model/agreggates/bilordersaudit.cs) |
| Value objects | [`domain/model/valueobjects/Invoice.cs`](domain/model/valueobjects/Invoice.cs), [`EService.cs`](domain/model/valueobjects/EService.cs) |
| Contrato repositorio | [`domain/repositories/IbillRepository.cs`](domain/repositories/IbillRepository.cs) |
| Contrato comando | [`domain/service/IbillCommandService.cs`](domain/service/IbillCommandService.cs) |
| Caso de uso | [`application/BillCommandService.cs`](application/BillCommandService.cs) |
| API | [`interfaces/REST/controllers/BillsController.cs`](interfaces/REST/controllers/BillsController.cs) |
| DTOs | [`interfaces/REST/resources/`](interfaces/REST/resources/) |
| Ensamblador | [`interfaces/REST/transform/billResourceAssembler.cs`](interfaces/REST/transform/billResourceAssembler.cs) |
| EF contexto | [`infrastructure/persistence/EFC/context/billContext.cs`](infrastructure/persistence/EFC/context/billContext.cs) |
| EF repositorio | [`infrastructure/persistence/EFC/repositories/BillRepository.cs`](infrastructure/persistence/EFC/repositories/BillRepository.cs) |
| Snake case | [`../shared/Persistence/EFC/Extentions/NamingConventionsExtension.cs`](../shared/Persistence/EFC/Extentions/NamingConventionsExtension.cs) |
| Host / DI | [`../Program.cs`](../Program.cs) |

---

## Enlaces directos GitHub (rama `main`)

Sustituye `main` por tu rama si el temario aún no está fusionado.

| Recurso | URL |
|---------|-----|
| Este temario | `https://github.com/arturo-ns/lectura/blob/main/pc2/sale/CONOCIMIENTOS_CSHARP.md` |
| Carpeta `sale` | `https://github.com/arturo-ns/lectura/tree/main/pc2/sale` |
| Controlador | `https://github.com/arturo-ns/lectura/blob/main/pc2/sale/interfaces/REST/controllers/BillsController.cs` |
| Servicio aplicación | `https://github.com/arturo-ns/lectura/blob/main/pc2/sale/application/BillCommandService.cs` |
| Contexto EF | `https://github.com/arturo-ns/lectura/blob/main/pc2/sale/infrastructure/persistence/EFC/context/billContext.cs` |

En la vista de GitHub, los enlaces **relativos** de este mismo archivo (p. ej. [`BillCommandService.cs`](application/BillCommandService.cs)) abren el código al instante sin `git pull`.
