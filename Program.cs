using FluentValidation;
using ToolShare.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateLoanRequestValidator>();

builder.Services.AddSingleton<IRepository<Member>, InMemoryRepository<Member>>();
builder.Services.AddSingleton<IRepository<Tool>, InMemoryRepository<Tool>>();
builder.Services.AddSingleton<IRepository<Loan>, InMemoryRepository<Loan>>();

builder.Services.AddSingleton<IdempotencyStore>();
builder.Services.AddSingleton<ILoanService, LoanService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

var members = app.Services.GetRequiredService<IRepository<Member>>();
var tools = app.Services.GetRequiredService<IRepository<Tool>>();
var owner = new Member("Thabo Mokoena");
var borrower = new Member("Lerato Dlamini");
members.Add(owner);
members.Add(borrower);
tools.Add(new Tool("Cordless Drill", "Power Tools", owner.Id));
tools.Add(new Tool("Step Ladder", "Ladders", owner.Id));

app.Run();