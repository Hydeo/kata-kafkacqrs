using Microsoft.EntityFrameworkCore;
using Post.Query.Infrastructure.DataAccess;

var builder = WebApplication.CreateBuilder(args);

var co = builder.Configuration.GetConnectionString("SqlDb");
// Add services to the container.
Action<DbContextOptionsBuilder> configureDbContext =
    (o => o.UseLazyLoadingProxies().UseNpgsql(builder.Configuration.GetConnectionString("SqlDb")));
builder.Services.AddDbContext<CustomDatabaseContext>(configureDbContext);
builder.Services.AddSingleton<DatabaseContextFactory>(new DatabaseContextFactory(configureDbContext));

//Create DB & tables from code
var dataContext = builder.Services.BuildServiceProvider().GetRequiredService<CustomDatabaseContext>();
dataContext.Database.EnsureCreated();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();