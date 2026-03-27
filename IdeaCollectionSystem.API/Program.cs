using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ?? DbContext (Business) ???????????????????????????????????????
builder.Services.AddDbContext<IdeaCollectionDbContext>(options =>
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("IdeaCollectionDbContext")));

// ?? DbContext (Identity) ??????????????????????????????????????
builder.Services.AddDbContext<IdeaCollectionIdentityDbContext>(options =>
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("IdeaCollectionIdentityDbContext")));

// ?? Identity ??????????????????????????????????????????????????
builder.Services.AddIdentity<IdeaUser, IdeaRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 8;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
	options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedAccount = false;
	options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<IdeaCollectionIdentityDbContext>()
.AddDefaultTokenProviders();

// ?? JWT Authentication ????????????????????????????????????????
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtIssuer,
		ValidAudience = jwtAudience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
	};
});

// ?? CORS cho React ????????????????????????????????????????????
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
	?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
	options.AddPolicy("ReactPolicy", policy =>
	{
		policy.WithOrigins(allowedOrigins)
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

// ?? Services DI ???????????????????????????????????????????????
builder.Services.AddScoped<IIdeaService, IdeaService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQAManagerService, QAManagerService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// ?? Controllers + Swagger ????????????????????????????????????
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Idea Collection API", Version = "v1" });

	// Swagger h? tr? JWT Bearer
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header. Nh?p: Bearer {token}",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
			},
			Array.Empty<string>()
		}
	});
});

var app = builder.Build();

// ?? Middleware ????????????????????????????????????????????????
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("ReactPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();