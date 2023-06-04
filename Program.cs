using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Product;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapGet("/product/{code}",([FromRoute] int id, ApplicationDbContext context)=>{
    var product = context.Products
        .Include(p => p.Category)
        .Include(p => p.Tags)
        .Where(p => p.Id == id).First();
    if(product != null)
        return Results.Ok(product);
    return Results.NotFound();
});
app.MapPost("/product",(ProductRequest productRequest, ApplicationDbContext context)=>{
    var category = context.Categories.Where(c => c.Id == productRequest.CategoryId).First();
    var product = new Product{
        Code = productRequest.Code,
        Name = productRequest.Name,
        Description = productRequest.Description,
        Category = category
    };
    if(productRequest.Tags != null)
    {
        product.Tags = new List<Tag>();
        foreach(var item in productRequest.Tags)
        {
            product.Tags.Add(new Tag{ Name = item });
        }
    }
    context.Products.Add(product);
    context.SaveChanges();
    return Results.Created($"/products/{product.Id}",product.Id);
});

app.MapPut("/product/{id}",([FromRoute] int id,ProductRequest productRequest, ApplicationDbContext context)=>{
    var product = context.Products
        .Include(p => p.Tags)
        .Where(p => p.Id == id).First();
    var category = context.Categories.Where(c => c.Id == productRequest.CategoryId).First();
    product.Code = productRequest.Code;
    product.Name = productRequest.Name;
    product.Description = productRequest.Description;
    product.Category = category;
    if(productRequest.Tags != null)
    {
        product.Tags = new List<Tag>();
        foreach(var item in productRequest.Tags)
        {
            product.Tags.Add(new Tag{ Name = item });
        }
    }
    context.SaveChanges();
    return Results.Ok();
});
app.MapDelete("/product/{id}",([FromRoute] int id, ApplicationDbContext context)=>{
    var product = context.Products.Where(p => p.Id == id).First();
    context.Products.Remove(product);
    context.SaveChanges();
    return Results.Ok();
});
app.MapGet("/configuration/database",(IConfiguration configuration)=>{ //Iconfiguration return important data from appsettings.json
    return Results.Ok(configuration["database:connection"]);
});

app.MapGet("/configuration/port",(IConfiguration configuration)=>{ //Iconfiguration return important data from appsettings.json
    return Results.Ok($"{configuration["database:connection"]}/{configuration["database:port"]}");
});


app.Run();

public static class ProductRepository{
    public static List<Product> Products { get; set; }

    public static void Init(IConfiguration configuration){
        var products = configuration.GetRequiredSection("Products").Get<List<Product>>();
        Products = Products;
    }

    public static void Add(Product product){
        if(Products == null)
            Products=new List<Product>();

        Products.Add(product);
    }

    public static Product GetBy(string code){
        return Products.FirstOrDefault(p=>p.Code==code);
    }

    public static void Remove(Product product){
        Products.Remove(product);
    }
}

public class Category{ 
    public int Id { get; set; }
    public string Name { get; set; }
     
}

public class Tag{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ProductId { get; set; }
}
public class Product{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int CategoryId  { get; set; }
    public Category Category {get; set;}
    public List<Tag> Tags { get; set; }
}

public class ApplicationDbContext : DbContext {
    
    public DbSet<Product> Products { get; set; }

    public DbSet<Category> Categories { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>()
            .Property(p => p.Description).HasMaxLength(500).IsRequired(false);
        builder.Entity<Product>()
            .Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.Entity<Product>()
            .Property(p => p.Code).HasMaxLength(20).IsRequired();
        builder.Entity<Category>()
            .ToTable("Categories");
    } 
}

public record ProductRequest(string Code, string Name, string Description, int CategoryId, List<string> Tags);