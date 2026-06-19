using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the EF Core DbContext using the SQL Server LocalDB connection string.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// RabbitMQ messaging: shared connection + publisher (producer) + background consumer.
builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddSingleton<EventStore>();
builder.Services.AddHostedService<EmployeeEventConsumer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
    );

app.Run();
