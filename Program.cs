using FluentValidation;
using ToolShare.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// WHY: registers the problem+json writer that our handler uses.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// WHY: finds every validator in this assembly automatically.
builder.Services.AddValidatorsFromAssemblyContaining<CreateLoanRequestValidator>();

// WHY Singleton: the in-memory "database" must live for the whole app lifetime,
// or the data would vanish after every request.
builder.Services.AddSingleton<IRepository<Member>, InMemoryRepository<Member>>();
builder.Services.AddSingleton<IRepository<Tool>, InMemoryRepository<Tool>>();
builder.Services.AddSingleton<IRepository<Loan>, InMemoryRepository<Loan>>();

// WHY Singleton: saved idempotency keys must survive between requests.
builder.Services.AddSingleton<IdempotencyStore>();
builder.Services.AddSingleton<ILoanService, LoanService>();

var app = builder.Build();

// WHY first: it must wrap everything after it to catch their exceptions.
app.UseExceptionHandler();
// WHY: makes plain 404s (unknown routes) come back as problem+json too.
app.UseStatusCodePages();

app.MapControllers();

// Seed data so the GET endpoints have something to return.
var members = app.Services.GetRequiredService<IRepository<Member>>();
var tools = app.Services.GetRequiredService<IRepository<Tool>>();
var owner = new Member("Thabo Mokoena");
var borrower = new Member("Lerato Dlamini");
members.Add(owner);
members.Add(borrower);
tools.Add(new Tool("Cordless Drill", "Power Tools", owner.Id));
tools.Add(new Tool("Step Ladder", "Ladders", owner.Id));

app.Run();