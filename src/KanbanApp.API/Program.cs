using KanbanApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// كل خدمات EF Core + Identity مسجّلة هنا عبر extension method واحد
builder.Services.AddInfrastructure(builder.Configuration);

// ملاحظة: JWT Authentication سيُضاف هنا في خطوة "نظام المصادقة" القادمة
// builder.Services.AddAuthentication(...)

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseAuthentication(); // سيُفعّل مع JWT
app.UseAuthorization();

app.MapControllers();

app.Run();