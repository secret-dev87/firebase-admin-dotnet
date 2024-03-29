using AppCheckPoC.DAL;
using Microsoft.EntityFrameworkCore;
using AppCheckPoC.Middleware;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS
builder.Services.AddCors(c =>
{
    c.AddPolicy(name: MyAllowSpecificOrigins,
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var configuration = builder.Configuration;
builder.Services.AddDbContext<LocalContext>(options => options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

FirebaseApp firebaseApp = FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("Credentials.json"),
    ProjectId = "appcheckmvp",
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.UseAppCheckMiddleware(firebaseApp);

app.MapControllers();

app.Run();
