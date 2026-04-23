using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using QuanLyHeThongBanHang.Data;
using QuanLyHeThongBanHang.Models;
using System.Text;

namespace QuanLyHeThongBanHang
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Cấu hình CORS cho phép Frontend gọi API
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3001")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Đăng ký AppDbContext với chuỗi kết nối từ appsettings.json
            builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Thêm Identity Core với Password Policy đơn giản cho môi trường Dev
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Thêm Authentication và Cấu hình JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"] ?? "http://localhost:5000",
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"] ?? "http://localhost:5000",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? "SuperSecretKeyExample1234567890123456"))
                };
            });

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            // Configure Swagger to support JWT Bearer Authorization
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Nhập 'Bearer' [khoảng trắng] và theo sau là token của bạn.\r\n\r\nVí dụ: \"Bearer eyJhGci...\""
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Sử dụng CORS trước StaticFiles và Auth
            app.UseCors("AllowFrontend");

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Đảm bảo database và Roles được khởi tạo
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<AppDbContext>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = services.GetRequiredService<UserManager<AppUser>>();

                    dbContext.Database.EnsureCreated();

                    // Seed Roles
                    string[] roleNames = { "Admin", "Staff", "Editor", "Customer" };
                    foreach (var roleName in roleNames)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            await roleManager.CreateAsync(new IdentityRole(roleName));
                        }
                    }

                    // Seed System Functions
                    var functions = new List<(string Code, string Name, int Order)>
                    {
                        ("dashboard_view", "Tổng Quan", 1),
                        ("orders_manage", "Quản Lý Đơn Hàng", 2),
                        ("payments_manage", "Quản Lý Thanh Toán", 3),
                        ("debts_manage", "Quản Lý Công Nợ", 4),
                        ("categories_manage", "Quản Lý Danh Mục", 5),
                        ("products_manage", "Quản Lý Sản Phẩm", 6),
                        ("inventory_manage", "Quản Lý Tồn Kho", 7),
                        ("warehouses_manage", "Quản Lý Chi Nhánh Kho", 8),
                        ("banners_manage", "Quản Lý Banner", 9),
                        ("menus_manage", "Quản Lý Menu", 10),
                        ("topics_manage", "Quản Lý Chủ Đề", 11),
                        ("posts_manage", "Quản Lý Bài Viết", 12),
                        ("employees_manage", "Quản Lý Nhân Viên", 13),
                        ("customers_manage", "Quản Lý Khách Hàng", 14),
                        ("permissions_manage", "Quản Lý Phân Quyền", 15)
                    };

                    foreach (var fn in functions)
                    {
                        if (!await dbContext.AppFunctions.AnyAsync(f => f.Code == fn.Code))
                        {
                            dbContext.AppFunctions.Add(new AppFunction { Code = fn.Code, Name = fn.Name, SortOrder = fn.Order });
                        }
                    }
                    await dbContext.SaveChangesAsync();

                    // Optional: Seed a default Admin user if none exists
                    var adminEmail = "admin@salespro.com";
                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        var user = new AppUser { UserName = adminEmail, Email = adminEmail };
                        var result = await userManager.CreateAsync(user, "Admin123!");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Admin");

                            // Grant all permissions to the initial admin
                            var allFns = await dbContext.AppFunctions.ToListAsync();
                            foreach (var f in allFns)
                            {
                                dbContext.UserPermissions.Add(new UserPermission { UserId = user.Id, FunctionId = f.Id });
                            }
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            app.Run();
        }
    }
}