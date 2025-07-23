using Microsoft.EntityFrameworkCore;
using Test_prod.Data;
using Test_prod.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PostgreDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("MyDataBase")));

builder.Services.AddControllers();
builder.Services.AddTransient<FileReader>();
builder.Services.AddTransient<PostgreRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PostgreDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
