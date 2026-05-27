using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using pc2_7420_u20231f226.sale.application;
using pc2_7420_u20231f226.sale.domain.repositories;
using pc2_7420_u20231f226.sale.domain.service;
using pc2_7420_u20231f226.sale.infrastructure.persistence.EFC.context;
using pc2_7420_u20231f226.sale.infrastructure.persistence.EFC.repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Maquinarias API", 
        Version = "v1",
        Description = "RESTful API for Maquinarias bill management",
        Contact = new OpenApiContact
        {
            Name = "Alex Sanchez Ponce",
            Email = "alex.sanchez@example.com"
        }
    });
});

// Database Context
builder.Services.AddDbContext<BillContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))));

// Dependency Injection
builder.Services.AddScoped<IBillCommandService, BillCommandService>();
builder.Services.AddScoped<IbillRepository, BillRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Maquinarias API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BillContext>();
    context.Database.EnsureCreated();
}

app.Run();