using Application.Interfaces;
using Application.Mapping;
using Application.Services;
using DataAccess.Database;
using DataAccess.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("Front", p => p
    .WithOrigins("http://onlineshopuz.runasp.net", "http://localhost:4200")
    .AllowAnyHeader().AllowAnyMethod()));

// app.UseAuthorization(); dan oldin:


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapping).Assembly);

//builder.Services.AddScoped<Repository<Customer>>();
//builder.Services.AddScoped<Repository<Category>>();
//builder.Services.AddScoped<Repository<CompanyBranch>>();
builder.Services.AddScoped<IRepository<Company>,Repository<Company>>();
builder.Services.AddScoped<IRepository<User>,Repository<User>>();
builder.Services.AddScoped<IRepository<CartItem>, Repository<CartItem>>();
builder.Services.AddScoped<IRepository<Order>, Repository<Order>>();
builder.Services.AddScoped<IRepository<Category>, Repository<Category>>();
builder.Services.AddScoped<IRepository<Product>, Repository<Product>>();
builder.Services.AddScoped<IRepository<Cart>, Repository<Cart>>();
builder.Services.AddScoped<IRepository<CompanyBranch>, Repository<CompanyBranch>>();
builder.Services.AddScoped<IRepository<Comment>, Repository<Comment>>();


builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<IProductService,ProductService>();
builder.Services.AddScoped<ICategoryService,CategoryService>();
builder.Services.AddScoped<ICompanyService,CompanyService>();
builder.Services.AddScoped<ICompanyBranchService, CompanyBranchService>();
builder.Services.AddScoped<ICartService,CartService>();
builder.Services.AddScoped<IOrderService,OrderService>();
builder.Services.AddScoped<ICommentService,CommentService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

//builder.Services.AddScoped<CartService>();
//builder.Services.AddScoped<OrderService>();
//builder.Services.AddScoped<CommentService>();


builder.Services.AddDbContext<AppDbContext>(options=>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSnakeCaseNamingConvention();
});

var app = builder.Build();

//app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

app.UseCors("Front");

app.UseAuthorization();

app.MapControllers();

app.Run();
