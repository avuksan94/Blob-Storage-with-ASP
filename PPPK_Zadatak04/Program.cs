using PPPK_Zadatak04.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("PPPKConnString");
var accountKey = builder.Configuration["AzureStorageConfig:StorageAccountKey"];
FileStorageRepository.Initialize(connectionString,accountKey);

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=FileStorage}/{action=Index}/{id?}");

app.Run();
