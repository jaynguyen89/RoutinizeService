# RoutinizeService
## Backend API for Routinize app. Written in C# with .NET Core, and using T-SQL.

###### Introduction

RoutinizeService contains **RoutinizeCore**, which is the main API that communicates with client.
**RoutinizeCore** makes use of other *services* and *libraries* to separate the logics into smaller manageable sub-services.

The RoutinizeService is documented by **Swagger/OpenAPI** for development, testing and debugging. 

###### AssistantLibrary

This library provides the following services:

- Google Recaptcha Verification
- Email Sender
- Google Two Factor Authentication
- QRCode generator with QRCoder
- RSA Service: generates Public-Private Keys for digital signature
- Assistant Service: supports cryptography producer and verifier

###### HelperLibrary

This library provides the following services and assets:

- Common-used Constants
- Common-used Enums
- Helpers: static methods to process and produce arbitrary data on demand

###### NotifierLibrary

This library provides services that implement ***Google Firebase Cloud Message*** for the following activities:

- To notify a user upon new collaboration request, mentioning, task assignment, etc...
- To notify all users about app updates and announcements (i.e. promotions, events, scheduled activities...)

###### SignalLibrary

This library provides services that implement ***SignalR*** framework for group and collaborator discussions vis chat feature.

###### MongoLibrary

This library maintains a connection to ***MongoDB*** server for the following services:

- Add logs for the API to facilitate debugging activities
- Add audits and history logs on pending changes in user data that may be reverted
- Add logs and usage statistics for client app to assist debugging
- Add user feedbacks, suggestions, bug reports that help improve the API and mobile app

###### MediaLibrary

This library is developed using a combination of ***CakePHP*** and .NET frameworks, therefore, it will consist of 2 smaller projects.

- CakePHP project: runs in Apache server; provides services to process photos, videos and audios; saves them to local server storage and update data to MySQL server.
- .NET project: uses MySQL connector to access MySQL server; provides services to RoutinizeCore; communicates with the CakePHP project.


###### Swagger attributes and annotations to use

[Produces("application/json")]
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase


[HttpPost("{id}")]
[ProducesResponseType(typeof(Product), 200)]
[ProducesResponseType(typeof(IDictionary<string, string>), 400)]
[ProducesResponseType(500)]
public IActionResult GetById(int id)


public class PagingParams
{
    [Required]
    public int PageNo { get; set; }

    public int PageSize { get; set; }

    [DefaultValue(false)]
    public bool IsComplete { get; set; }
}


/// <summary>
/// Retrieves a specific product by unique id
/// </summary>
/// <remarks>Awesomeness!</remarks>
/// <param name="id" example="123">The product id</param>
/// <response code="200">Product created</response>
/// <response code="400">Product has missing/invalid values</response>
/// <response code="500">Oops! Can't create your product right now</response>
[HttpGet("{id}")]
[ProducesResponseType(typeof(Product), 200)]
[ProducesResponseType(typeof(IDictionary<string, string>), 400)]
[ProducesResponseType(500)]
public Product GetById(int id)


/// <summary>
/// Creates a TodoItem.
/// </summary>
/// <remarks>
/// Sample request:
///
///     POST /Todo
///     {
///        "id": 1,
///        "name": "Item1",
///        "isComplete": true
///     }
///
/// </remarks>
/// <param name="item"></param>
/// <returns>A newly created TodoItem</returns>
/// <response code="201">Returns the newly created item</response>
/// <response code="400">If the item is null</response>            
[HttpPost]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public ActionResult<TodoItem> Create(TodoItem item)
{
    _context.TodoItems.Add(item);
    _context.SaveChanges();

    return CreatedAtRoute("GetTodo", new { id = item.Id }, item);
}


public class Product
{
    /// <summary>
    /// The name of the product
    /// </summary>
    /// <example>Men's basketball shoes</example>
    public string Name { get; set; }

    /// <summary>
    /// Quantity left in stock
    /// </summary>
    /// <example>10</example>
    public int AvailableStock { get; set; }
}


[HttpGet]
public IActionResult GetProducts(
    [FromQuery, SwaggerParameter("Search keywords", Required = true)]string keywords)


[HttpPost]
public IActionResult CreateProduct(
    [FromBody, SwaggerRequestBody("The product payload", Required = true)]Product product)


[SwaggerSchema(Required = new[] { "Description" })]
public class Product
{
	[SwaggerSchema("The product identifier", ReadOnly = true)]
	public int Id { get; set; }

	[SwaggerSchema("The product description")]
	public string Description { get; set; }

	[SwaggerSchema("The date it was created", Format = "date")]
	public DateTime DateCreated { get; set; }
}


[SwaggerTag("Create, read, update and delete Products")]
public class ProductsController


[SwaggerSubType(typeof(Rectangle))]
[SwaggerSubType(typeof(Circle))]
public abstract class Shape


[SwaggerDiscriminator("shapeType")]
[SwaggerSubType(typeof(Rectangle), DiscriminatorValue = "rectangle")]
[SwaggerSubType(typeof(Circle), DiscriminatorValue = "circle")]
public abstract class Shape
{
    public ShapeType { get; set; }
}