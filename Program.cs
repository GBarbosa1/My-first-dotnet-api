using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Product;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>();

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapGet("/product/{code}",([FromRoute] string code)=>{
    var product = ProductRepository.GetBy(code);
    if(product != null)
        return Results.Ok(product);
    return Results.NotFound();
});
app.MapPost("/product",(Product product)=>{
    ProductRepository.Add(product);
    return Results.Created("/products/" + product.Code,product.Code);
});

app.MapPut("/product",(Product product)=>{
    var productSaved = ProductRepository.GetBy(product.Code);
    productSaved.Name = product.Name;
    return Results.Ok();
});
app.MapDelete("/product/{code}",([FromRoute] string code)=>{
    var productSaved = ProductRepository.GetBy(code);
    ProductRepository.Remove(productSaved);
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

public class Product{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }

}

public class ApplicationDbContext : DbContext {
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(
            "Server=localhost;Database=Products;User Id=SA;Password=Writemaster1580;MultipleActiveResultSets=true;Encrypt=YES;TrustServercertificate=YES"
        );
    
}
