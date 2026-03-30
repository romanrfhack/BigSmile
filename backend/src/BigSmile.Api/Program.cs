using BigSmile.Application;
using BigSmile.Infrastructure;
using BigSmile.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application layer (MediatR handlers)
builder.Services.AddApplication();

// Register infrastructure (repositories, context)
builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Custom middleware for tenant resolution (must be before controllers)
app.UseMiddleware<TenantResolutionMiddleware>();

app.MapControllers();

app.Run();