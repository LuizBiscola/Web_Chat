using ChatAppApi.Hubs; // Precisaremos criar este namespace e classe depois
using ChatAppApi.Services;
using ChatAppApi.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL; // <<< Adicione este using

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Adicione o serviço SignalR
builder.Services.AddSignalR();

// 2. Opcional: Adicione CORS para permitir comunicação do frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed((host) => true) // Permite qualquer origem (para teste)
              .AllowCredentials(); // Essencial para SignalR com CORS
    });
});



builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// 3. Opcional: Use CORS antes de MapControllers e MapHub
app.UseCors();

app.MapControllers();

// 4. Mapeie o Hub do SignalR
app.MapHub<ChatHub>("/chat"); // /chat será o endpoint do WebSocket

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // NÃO CHAME context.Database.EnsureCreated(); aqui para DB persistente.
    // As migrações do EF Core cuidarão da criação/atualização do esquema.
    // context.Database.EnsureCreated(); // <--- REMOVA OU COMENTE ESTA LINHA

    // Opcional: Se você quer garantir que as migrações sejam aplicadas ao iniciar a app
    // Isso é comum em desenvolvimento, mas não recomendado em alguns cenários de produção
    // onde as migrações são aplicadas como parte do pipeline de CI/CD.
    context.Database.Migrate(); // Aplica migrações pendentes ao iniciar.

    // Adicione dados iniciais se o banco de dados estiver vazio (execute APÓS as migrações)
    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new ChatAppApi.Models.User { Username = "Alice" },
            new ChatAppApi.Models.User { Username = "Bob" },
            new ChatAppApi.Models.User { Username = "Charlie" }
        );
        context.SaveChanges();
    }
}

app.Run();