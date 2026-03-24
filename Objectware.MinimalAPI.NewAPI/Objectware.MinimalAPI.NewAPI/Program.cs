using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Objectware.MinimalAPI.NewAPI;
using Objectware.MinimalAPI.NewAPI.Endpoints;
using Objectware.MinimalAPI.NewAPI.Models;
using Objectware.MinimalAPI.NewAPI.Services;
using Objectware.MinimalAPI.NewAPI.Validation;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IValidator<ConfirmOrderRequest>, ConfirmOrderValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.MapPost("/auth/token", (IConfiguration config) =>
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: [new Claim(ClaimTypes.Name, "demo-user")],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    })
    .WithTags("Auth")
    .WithSummary("Generate a dev token")
    .AllowAnonymous();

app.MapGet("/api/orders", OrderEndpoints.GetOrders)
   .WithTags("Orders")
   .WithSummary("Get paginated orders")
   .RequireAuthorization();

app.MapPost("/api/orders/{id:guid}/confirm", OrderEndpoints.ConfirmOrder)
   .WithTags("Orders")
   .WithSummary("Confirm an order")
   .RequireAuthorization()
   .AddEndpointFilter<ValidationFilter<ConfirmOrderRequest>>();

app.Run();