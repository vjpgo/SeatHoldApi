using SeatHold.Api.Extensions;
using SeatHold.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSeatHold();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();

// Needed for WebApplicationFactory integration tests
public partial class Program { }
