# Temario de C# — módulo `sale`

**Lectura recomendada:** Las partes II a VI explican cada término técnico la primera vez que aparece, con lenguaje accesible.

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
| [II](#parte-ii--cómo-se-organizan-las-piezas-del-código-intermedio) | Intermedio | 9–14 |
| [III](#parte-iii--la-api-por-internet-intermedio) | Intermedio | 15–18 |
| [IV](#parte-iv--guardar-y-leer-en-base-de-datos-avanzado) | Avanzado | 19–23 |
| [V](#parte-v--cómo-está-organizado-el-módulo-completo-avanzado) | Avanzado | 24–28 |
| [VI](#parte-vi--cómo-se-enciende-el-módulo-al-arrancar-la-aplicación-avanzado) | Avanzado | 29 |
| [Anexo](#anexo--mapa-de-archivos-por-tema) | — | Referencia rápida |

---

## Parte I — Fundamentos del lenguaje C#

### Tema 1. Tipos de datos y propiedades

**Qué aparece en `sale`:** enteros, texto, fechas, decimales y enumeraciones en la entidad interna `bill` y en los objetos de transferencia para la API (DTO, temas 16 y 27).

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

**Dónde se usa este enum:**
- En el **JSON** del cuerpo de la petición (campo `ServiceId`): formato de texto que envía el cliente.
- En **validación** con `Enum.IsDefined`: comprobar que el número enviado sea uno de los permitidos.
- En la **base de datos (BD)** como número entero, gracias a la configuración `HasConversion<int>()` en el contexto de Entity Framework (ver tema 20).

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

## Parte II — Cómo se organizan las piezas del código (intermedio)

> **Cómo leer esta parte:** si aparece una palabra técnica, va con una explicación en lenguaje sencillo. El objetivo es que entiendas *qué hace el código en la vida real*, no memorizar nombres en inglés.

---

### Tema 9. Interfaces (el “contrato” de qué debe hacer una clase)

**Qué aparece en `sale`:** archivos que solo dicen *qué operaciones existen*, sin escribir cómo se hacen por dentro.

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

**Qué es una *interface* (interfaz):**  
Es como la lista de funciones que una clase **debe** ofrecer. No contiene la lógica; solo el “menú”. Por ejemplo: “tiene que existir un método `Handle` que recibe datos de creación y devuelve la factura creada”.

**Por qué se usa aquí:**  
El controlador puede decir “necesito algo que cumpla `IBillCommandService`” sin importarle si detrás está `BillCommandService` u otra clase. Eso hace el código más flexible y más fácil de probar.

**Qué es `Task<BillResource>`:**  
Significa “esta operación tarda un poco (por ejemplo, guardar en base de datos) y, cuando termine, devolverá un `BillResource`”. Se relaciona con el tema 12 (`async` / `await`).

---

### Tema 10. Clase que cumple el contrato (implementación)

**Qué aparece en `sale`:** las clases que sí tienen el código real detrás de cada interfaz.

| Contrato (interfaz) | Clase que lo implementa |
|---------------------|-------------------------|
| `IBillCommandService` | [`application/BillCommandService.cs`](application/BillCommandService.cs) |
| `IbillRepository` | [`infrastructure/persistence/EFC/repositories/BillRepository.cs`](infrastructure/persistence/EFC/repositories/BillRepository.cs) |

```9:10:pc2/sale/application/BillCommandService.cs
public class BillCommandService : IBillCommandService
{
```

**Qué significa `: IBillCommandService`:**  
La clase `BillCommandService` **promete** cumplir todo lo que define la interfaz (en este caso, el método `Handle` con sus validaciones y guardado).

**Analogía:** la interfaz es el contrato de trabajo; la clase es la persona que lo firma y hace el trabajo.

---

### Tema 11. Que el programa “te pase” las herramientas listas (inyección de dependencias)

**Qué aparece en `sale`:** en lugar de crear objetos a mano con `new`, el constructor **recibe** lo que necesita.

**Ejemplo en el controlador:** [`interfaces/REST/controllers/BillsController.cs`](interfaces/REST/controllers/BillsController.cs)

```18:27:pc2/sale/interfaces/REST/controllers/BillsController.cs
    private readonly IBillCommandService _commandService;

    public BillsController(IBillCommandService commandService)
    {
        _commandService = commandService;
    }
```

**Qué es *inyección de dependencias*:**  
“Dependencia” = algo que una clase necesita para funcionar (aquí, el servicio de facturas). “Inyección” = el framework de ASP.NET **la entrega automáticamente** al crear el controlador, en lugar de que tú escribas `new BillCommandService()` dentro del controlador.

**Para qué sirve en `sale`:**  
- El controlador no sabe *cómo* está hecho el servicio; solo usa la interfaz.  
- En [`../Program.cs`](../Program.cs) se registra la pareja: “cuando pidan `IBillCommandService`, usa `BillCommandService`”. Ese archivo es el “catálogo” de qué clase corresponde a cada contrato.

**Qué es `readonly`:**  
El campo `_commandService` solo se asigna en el constructor y no cambia después. Es una forma de decir “esta dependencia es fija para esta instancia del controlador”.

---

### Tema 12. Operaciones que esperan sin trabar el servidor (`async` / `await`)

**Qué aparece en `sale`:** al crear una factura, el programa **espera** la base de datos sin bloquear todo el servidor.

**Cadena de archivos:**

1. [`BillsController.cs`](interfaces/REST/controllers/BillsController.cs)  
2. [`BillCommandService.cs`](application/BillCommandService.cs)  
3. [`BillRepository.cs`](infrastructure/persistence/EFC/repositories/BillRepository.cs)

```45:50:pc2/sale/interfaces/REST/controllers/BillsController.cs
    public async Task<IActionResult> CreateBill([FromBody] CreateBillResource resource)
    {
        try
        {
            var result = await _commandService.Handle(resource);
            return CreatedAtAction(nameof(CreateBill), new { billNumber = result.BillNumber }, result);
```

| Palabra | Significado sencillo |
|---------|----------------------|
| `async` | “Este método puede hacer pausas mientras espera algo lento (red, base de datos).” |
| `await` | “Quédate aquí hasta que termine esa parte lenta; mientras tanto, el servidor puede atender otras peticiones.” |
| `Task` / `Task<T>` | La “promesa” de que la operación terminará y, si aplica, devolverá un valor (`T`). |
| `IActionResult` | Lo que el controlador devuelve al cliente: JSON, código 201, error 400, etc. |

**Ejemplo de la vida real:**  
Como pedir comida: no te quedas parado sin hacer nada en la cocina; puedes hacer otra cosa hasta que llegue el plato. `await` es esa espera ordenada.

Los métodos que terminan en `Async` (`AddAsync`, `SaveChangesAsync`) siguen la misma idea: hablan con MySQL sin trabar el hilo principal innecesariamente.

---

### Tema 13. Valores que pueden no existir (`bill?`)

**Archivo:** [`domain/repositories/IbillRepository.cs`](domain/repositories/IbillRepository.cs)

```27:27:pc2/sale/domain/repositories/IbillRepository.cs
    Task<bill?> GetByBillNumberAsync(int billNumber);
```

**Qué significa el `?` en `bill?`:**  
“Puede devolver una factura **o** no devolver nada (`null`) si no existe ese número.”

**Qué es `null`:**  
Ausencia de valor: “no hay objeto”. El proyecto tiene activada la opción de avisar cuando algo podría ser nulo (`<Nullable>enable</Nullable>` en el `.csproj`), para evitar errores del tipo “intentaste usar algo que no existe”.

---

### Tema 14. Errores controlados y reglas del negocio

**Qué aparece en `sale`:** si los datos no cumplen las reglas del taller, el código **lanza una excepción** (un error programado) con un mensaje claro.

**Archivo:** [`application/BillCommandService.cs`](application/BillCommandService.cs)

```29:31:pc2/sale/application/BillCommandService.cs
        if (string.IsNullOrWhiteSpace(resource.Customer) || resource.Customer.Length > 100)
            throw new ArgumentException("Customer is required and must be maximum 100 characters.");
```

```61:63:pc2/sale/application/BillCommandService.cs
        if (await _repository.ExistByInvoiceAsync(resource.InvoiceSerialNumber, resource.InvoiceSequentialNumber))
            throw new InvalidOperationException("A bill with the same invoice already exists.");
```

| Término | Qué significa aquí |
|---------|---------------------|
| **Regla de negocio** | Condición del mundo real: “el cliente es obligatorio”, “no puede haber dos facturas con la misma serie y correlativo”. |
| **`throw`** | Detener el flujo normal y saltar al manejo de errores (`catch` en el controlador). |
| **`ArgumentException`** | Datos incorrectos enviados por quien llama a la API → el controlador responde **400** (petición incorrecta). |
| **`InvalidOperationException`** | La operación no se puede hacer por estado del sistema (duplicado) → respuesta **409** (conflicto). |

El controlador **traduce** esas excepciones a códigos HTTP que entiende cualquier cliente web o móvil.

---

## Parte III — La API por internet (intermedio)

**Qué es una API en este proyecto:**  
Un conjunto de direcciones web a las que puedes enviar datos (por ejemplo JSON) y recibir respuestas. Aquí, para crear facturas.

**Qué es REST:**  
Un estilo de diseño donde cada acción usa un verbo HTTP conocido: `GET` (leer), `POST` (crear), `PUT`/`PATCH` (actualizar), `DELETE` (borrar). En `sale` solo se usa `POST` para crear.

**Qué es JSON:**  
Formato de texto para intercambiar datos: `{"customer": "Juan", "amount": 100}`. Las clases `CreateBillResource` y `BillResource` representan esa estructura en C#.

---

### Tema 15. Controlador: quien recibe la petición web

**Archivo:** [`interfaces/REST/controllers/BillsController.cs`](interfaces/REST/controllers/BillsController.cs)

```14:16:pc2/sale/interfaces/REST/controllers/BillsController.cs
[ApiController]
[Route("api/v1/bills")]
public class BillsController : ControllerBase
```

| Pieza | Explicación |
|-------|-------------|
| **Controlador** | Clase que atiende peticiones HTTP; es la “puerta de entrada” del módulo. |
| **`[Route("api/v1/bills")]`** | La URL base: todo lo de este controlador empieza por `/api/v1/bills`. |
| **`ControllerBase`** | Clase base de ASP.NET que aporta métodos útiles (`BadRequest`, `CreatedAtAction`, etc.) sin pantallas HTML. |
| **`POST` + `CreateBill`** | “Crear algo nuevo” enviando datos en el cuerpo de la petición. |
| **201 Created** | Código HTTP que significa: “se creó correctamente”. |
| **`CreatedAtAction`** | Respuesta 201 que además indica dónde quedó el recurso creado (cabecera `Location`). |

---

### Tema 16. Objetos solo para la API (DTO / Resource)

**Qué es un DTO (*Data Transfer Object*):**  
Una clase **solo para mover datos** entre el cliente y el servidor. No tiene lógica de negocio; es el “formulario” de entrada o el “recibo” de salida.

En este proyecto se llaman `*Resource`:

| Clase | Para qué sirve | Archivo |
|-------|----------------|---------|
| `CreateBillResource` | Datos que envía quien llama al `POST` (incluye placa y asesor) | [`CreateBillResource.cs`](interfaces/REST/resources/CreateBillResource.cs) |
| `BillResource` | Datos que devuelve el servidor tras crear (sin placa ni asesor) | [`BillResource.cs`](interfaces/REST/resources/BillResource.cs) |
| `InvoiceResource` | Formato alternativo de factura; definido pero casi no usado en el flujo actual | [`InvoiceResource.cs`](interfaces/REST/resources/InvoiceResource.cs) |

**Por qué no se usa directamente `bill` en la API:**  
La entidad `bill` tiene campos internos (auditoría, etc.) que no quieres mostrar fuera. Los DTO permiten **elegir qué se ve y qué no**.

---

### Tema 17. Respuestas cuando algo sale mal

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

**Qué es `try` / `catch`:**  
`try` = “intenta hacer esto”; `catch` = “si ocurre este tipo de error, responde así en lugar de caerse la aplicación”.

| Código HTTP | Nombre | Cuándo lo usa `sale` |
|-------------|--------|----------------------|
| **400** | Bad Request | Datos inválidos (cliente vacío, monto ≤ 0, etc.) |
| **409** | Conflict | Ya existe una factura con esa serie y correlativo |
| **500** | Internal Server Error | Error no previsto (fallo interno) |

**Qué es HTTP:**  
El protocolo con el que el navegador o Postman hablan con el servidor. Los números (400, 201…) son códigos estándar para que el cliente sepa qué pasó sin leer todo el mensaje.

---

### Tema 18. Documentación automática de la API (Swagger)

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

| Término | Significado |
|---------|-------------|
| **Swagger** | Herramienta que genera una página web para probar la API sin escribir código cliente. |
| **OpenAPI** | El estándar detrás de Swagger: describe rutas, parámetros y respuestas en un formato común. |
| **`[SwaggerOperation]`** | Título y descripción que verás en esa página de prueba. |
| **`[ProducesResponseType]`** | “Este endpoint puede devolver 201 con `BillResource`, o 400, o 409.” |

La configuración general (título “Maquinarias API”, etc.) está en [`../Program.cs`](../Program.cs).

---

## Parte IV — Guardar y leer en base de datos (avanzado)

**Qué es una base de datos aquí:**  
MySQL guarda las facturas en tablas (como hojas de Excel con muchas filas). El módulo `sale` no escribe SQL a mano en cada sitio; usa una librería que traduce objetos C# a consultas.

**Qué es Entity Framework Core (EF Core):**  
Herramienta de Microsoft que conecta tus clases C# (`bill`) con tablas SQL. Permite agregar, buscar y comprobar datos con código parecido a C# en lugar de escribir `INSERT`/`SELECT` en cada archivo.

---

### Tema 19. El “puente” con la base de datos (`DbContext` y `DbSet`)

**Archivo:** [`infrastructure/persistence/EFC/context/billContext.cs`](infrastructure/persistence/EFC/context/billContext.cs)

```7:11:pc2/sale/infrastructure/persistence/EFC/context/billContext.cs
public class BillContext : DbContext
{
    public DbSet<bill> Bills => Set<bill>();

    public BillContext(DbContextOptions<BillContext> options) : base(options) { }
```

| Término | Explicación |
|---------|-------------|
| **`DbContext`** | Objeto que representa una sesión de trabajo con la base de datos: sabe la conexión, las tablas y cuándo guardar cambios. |
| **`DbSet<bill>`** | La “colección” de todas las filas de facturas, como si fuera una lista en memoria pero respaldada por la tabla `bills`. |
| **`BillContextOptions`** | Configuración que llega desde `Program.cs` (servidor MySQL, usuario, contraseña en `appsettings`). |

Cuando el repositorio hace `_context.Bills.AddAsync(bill)`, está diciendo: “añade esta factura al contexto”; con `SaveChangesAsync()` se envía realmente a MySQL.

---

### Tema 20. Reglas de cómo se guarda cada campo (configuración en código)

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

**Qué es *Fluent API*:**  
Forma de describir reglas de la base de datos **escribiendo código** en `OnModelCreating`, en lugar de solo poner etiquetas `[...]` en las propiedades.

| Método en el código | Qué hace en palabras simples |
|--------------------|----------------------------|
| `HasKey` | Define la **clave primaria**: el identificador único de cada fila (`BillNumber`). |
| `ValueGeneratedOnAdd` | El número de factura lo genera la base de datos al insertar (autoincremento). |
| `IsRequired` | El campo no puede quedar vacío en la tabla. |
| `HasMaxLength` | Límite de caracteres (por ejemplo, cliente máximo 100). |
| `HasColumnType("decimal(18,2)")` | Guardar el monto como decimal con 2 decimales (más preciso que `double` para dinero). |
| `HasConversion<int>()` | Guardar el enum `EService` como número entero en la tabla. |
| `ToTable("bills")` | Nombre real de la tabla en MySQL. |

---

### Tema 21. Datos de factura (serie/correlativo) dentro de la misma fila (`OwnsOne`)

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

**Qué es `OwnsOne`:**  
Le dice a EF: “`Invoice` no tiene tabla propia; sus campos viven **dentro** de la fila de `bills`”. En la base verás columnas del estilo `invoice_serial_number`, no una tabla `invoices` separada.

**Por qué se hace así:**  
En el modelo de negocio, serie y correlativo son parte de la factura, no un registro independiente con vida propia. Es como guardar “dirección: calle y número” en la misma ficha del cliente, sin crear otra tabla solo para direcciones.

**Consulta de duplicados** en [`BillRepository.cs`](infrastructure/persistence/EFC/repositories/BillRepository.cs):

```26:28:pc2/sale/infrastructure/persistence/EFC/repositories/BillRepository.cs
        return await _context.Bills
            .AnyAsync(b => b.Invoice.SerialNumber == serialNumber && 
                           b.Invoice.SequentialNumber == sequentialNumber);
```

| Término | Significado |
|---------|-------------|
| **LINQ** | Sintaxis en C# para filtrar listas (`b => b.Invoice.SerialNumber == ...`). EF la convierte a SQL. |
| **`AnyAsync`** | Pregunta: “¿existe al menos una fila que cumpla esto?” — útil para comprobar duplicados sin traer todos los datos. |

---

### Tema 22. Repositorio: la clase que habla con la base por ti

**Archivo:** [`infrastructure/persistence/EFC/repositories/BillRepository.cs`](infrastructure/persistence/EFC/repositories/BillRepository.cs)

```17:21:pc2/sale/infrastructure/persistence/EFC/repositories/BillRepository.cs
    public async Task<bill> AddAsync(bill bill)
    {
        await _context.Bills.AddAsync(bill);
        await _context.SaveChangesAsync();
        return bill;
```

**Qué es un *repositorio*:**  
Capa que concentra **cómo** se guardan y buscan las facturas. El servicio de aplicación no llama a SQL directo; llama a `AddAsync`, `ExistByInvoiceAsync`, etc.

**Qué hace `SaveChangesAsync`:**  
Confirma los cambios pendientes en el contexto. Hasta ese momento, los datos pueden estar solo “en memoria” dentro de EF. Después del guardado, `BillNumber` suele venir ya rellenado por la base (autoincremento).

---

### Tema 23. Nombres de columnas con guión bajo (`snake_case`)

En C# es habitual `BillNumber` (varias palabras juntas). En MySQL del proyecto se prefieren nombres como `bill_number`.

**Uso en:** [`billContext.cs`](infrastructure/persistence/EFC/context/billContext.cs) — línea `modelBuilder.UserSnakeCaseNamingConventions();`

**Implementación:** [`../shared/Persistence/EFC/Extentions/NamingConventionsExtension.cs`](../shared/Persistence/EFC/Extentions/NamingConventionsExtension.cs)

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

| Término | Significado |
|---------|-------------|
| **snake_case** | Palabras en minúsculas separadas por `_` (`created_at`). |
| **Método de extensión** | Función extra que “se cuelga” de un tipo existente (`ModelBuilder`) y se escribe como si fuera propio de EF. El `this` en el primer parámetro es lo que lo hace extensión. |
| **Regex** | Búsqueda de patrones en texto; aquí inserta `_` entre minúscula y mayúscula para transformar nombres. |

Si hace falta un nombre exacto distinto al automático, se usa `[Column("CreatedAt")]` en [`bilordersaudit.cs`](domain/model/agreggates/bilordersaudit.cs).

---

## Parte V — Cómo está organizado el módulo completo (avanzado)

**Qué es “arquitectura” aquí:**  
No es un dibujo decorativo: es **repasar carpetas** para saber dónde va cada responsabilidad y en qué orden se ejecutan las cosas al crear una factura.

---

### Tema 24. Capas (carpetas) y el camino de una petición

```
sale/
├── interfaces/REST/     →  Recibe y responde por HTTP
├── application/           →  Reglas al crear una factura (BillCommandService)
├── domain/                →  Modelo de negocio y contratos (interfaces)
└── infrastructure/        →  Conexión real con MySQL (EF Core)
```

**Flujo al crear una factura (en orden):**

1. [`BillsController`](interfaces/REST/controllers/BillsController.cs) — recibe el JSON.  
2. [`BillCommandService`](application/BillCommandService.cs) — valida y arma el objeto `bill`.  
3. [`BillRepository`](infrastructure/persistence/EFC/repositories/BillRepository.cs) — guarda en base.  
4. [`BillContext`](infrastructure/persistence/EFC/context/billContext.cs) — traduce a tablas MySQL.  
5. Respuesta como [`BillResource`](interfaces/REST/resources/BillResource.cs) — JSON de vuelta al cliente.

Cada capa conoce principalmente la de abajo a través de **interfaces**, no de detalles internos.

---

### Tema 25. Un solo método para “crear factura” (`Handle`)

**Archivo:** [`application/BillCommandService.cs`](application/BillCommandService.cs)

**Qué es un *caso de uso*:**  
Una acción concreta del negocio: “registrar una nueva factura”. Aquí se concentra en el método `Handle`.

**Pasos que hace `Handle`:**

1. Comprobar reglas (cliente, monto, fecha, tipo de servicio, factura no duplicada).  
2. Crear el objeto `bill` con fechas de creación/actualización.  
3. Pedir al repositorio que lo guarde.  
4. Devolver `BillResource` (solo los campos que el cliente debe ver).

**Qué es el patrón *Command* (comando):**  
Nombre formal para “un método que representa una orden”: `Handle` = ejecutar la orden “crear factura”. El controlador no reparte validaciones; delega todo al servicio.

**Separación de responsabilidades:**  
El controlador no sabe SQL ni tablas. Solo llama al servicio. Eso facilita cambiar la base de datos o las reglas sin tocar la URL de la API.

---

### Tema 26. Comprobar que el tipo de servicio sea uno permitido

**Archivo:** [`application/BillCommandService.cs`](application/BillCommandService.cs)

```49:51:pc2/sale/application/BillCommandService.cs
        if (!Enum.IsDefined(typeof(EService), resource.ServiceId))
            throw new ArgumentException("Invalid service ID.");
```

**Problema que resuelve:**  
En JSON alguien podría enviar `"serviceId": 99`. Ese número no existe en el enum `EService` (solo 10, 20, 30, 40). Sin esta comprobación, podrías guardar un valor sin sentido.

**Qué es *deserializar*:**  
Convertir el texto JSON en objetos C#. Esa conversión no siempre valida reglas de negocio; por eso el servicio vuelve a comprobar.

---

### Tema 27. Convertir entre “modelo interno” y “lo que ve el cliente” (ensamblador)

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

**Qué es un *ensamblador* (assembler):**  
Clase o métodos que **copian datos** de un tipo a otro, campo por campo. Aquí: de `bill` (interno) a `BillResource` (respuesta API).

**Qué significa `static`:**  
Se usa sin crear instancia de la clase: `BillResourceAssembler.ToResource(bill)`.

**Qué significa `=> new() { ... }`:**  
Forma corta de crear un objeto y rellenar propiedades en una sola expresión.

**Por qué se omiten placa y asesor en la respuesta:**  
Requisito del negocio/API: al crear, el cliente recibe solo ciertos campos. El comentario en el código lo deja explícito para quien mantenga el proyecto.

Hoy parte del mapeo también está dentro de `BillCommandService`; el ensamblador sirve para no repetir la misma lógica en varios sitios.

---

### Tema 28. Ideas de diseño orientado al dominio (DDD) — en lenguaje llano

**DDD (*Domain-Driven Design*):**  
Forma de organizar el software **alrededor del lenguaje del negocio** (factura, servicio, asesor), no solo alrededor de tablas y pantallas.

| Idea DDD | Cómo se ve en `sale` | Archivo |
|----------|----------------------|---------|
| **Entidad** | Algo con identidad propia que persiste (`bill`, identificado por `BillNumber`) | [`bill.cs`](domain/model/agreggates/bill.cs) |
| **Objeto de valor** | Datos que se definen por su contenido, no por un ID (`Invoice` = serie + correlativo; `EService` = tipo de servicio) | [`Invoice.cs`](domain/model/valueobjects/Invoice.cs), [`EService.cs`](domain/model/valueobjects/EService.cs) |
| **Repositorio** | Puerta de entrada para guardar/buscar entidades sin mezclar SQL en todo el código | [`IbillRepository.cs`](domain/repositories/IbillRepository.cs) |
| **Servicio de aplicación** | Coordina un caso de uso usando entidades y repositorio | [`BillCommandService.cs`](application/BillCommandService.cs) |
| **Capa anti-corrupción** | DTOs (`CreateBillResource`, `BillResource`) que evitan que el formato JSON “contamine” el modelo interno | [`interfaces/REST/resources/`](interfaces/REST/resources/) |

**Nota honesta del template:**  
`IBillCommandService` está en `domain` pero devuelve `BillResource`, que es de la capa web. En un diseño muy estricto, el dominio no conocería ese DTO; el controlador haría la conversión final. Aquí se prioriza practicidad del template.

---

## Parte VI — Cómo se “enciende” el módulo al arrancar la aplicación (avanzado)

### Tema 29. Registro de servicios y base de datos

No está dentro de `sale/`, pero sin esto el módulo no funciona. Archivo: [`../Program.cs`](../Program.cs)

```29:35:pc2/Program.cs
builder.Services.AddDbContext<BillContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))));

builder.Services.AddScoped<IBillCommandService, BillCommandService>();
builder.Services.AddScoped<IbillRepository, BillRepository>();
```

| Línea | Qué significa |
|-------|----------------|
| `AddDbContext<BillContext>` | Registra el puente a MySQL y lee la cadena de conexión del archivo de configuración. |
| `UseMySql` + **Pomelo** | Librería que adapta EF Core para hablar con MySQL 8. |
| `AddScoped<Interfaz, Clase>` | **Scoped** = “crea una instancia nueva por cada petición web y reutilízala dentro de esa misma petición”. Así el controlador, servicio y repositorio comparten el mismo contexto de base de datos al guardar. |
| `EnsureCreated()` (más abajo en el mismo archivo) | Al iniciar, crea la base/tablas si no existen. Útil en desarrollo; en producción suele usarse otro mecanismo (**migraciones**: scripts versionados de cambios en el esquema). |

**Qué es el *host*:**  
El programa principal que levanta el servidor web. `Program.cs` es el punto de entrada que configura todo antes de `app.Run()`.

**Paquetes del proyecto:** [`../pc2-7420-u20231f226.csproj`](../pc2-7420-u20231f226.csproj) — .NET 9, EF Core, Swagger, etc.

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
