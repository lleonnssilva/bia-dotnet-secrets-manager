using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Bia.Models;
using BIA.Data;
using BIA.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var awsOptions = new AwsOptions();
builder.Configuration.GetSection("AwsOptions").Bind(awsOptions);

var credentials = new BasicAWSCredentials(awsOptions.AccessKey, awsOptions.SecretKey);
var secretsManagerClient = new AmazonSecretsManagerClient(credentials, RegionEndpoint.USEast1);

var getSecretValueRequest = new GetSecretValueRequest { SecretId = awsOptions.SecretName };
var getSecretValueResponse = await secretsManagerClient.GetSecretValueAsync(getSecretValueRequest);

var secretJson = Newtonsoft.Json.Linq.JObject.Parse(getSecretValueResponse.SecretString);

var connectionString = $"Server={awsOptions.HostRds};Port=5432;User ID={secretJson["username"]};Password={secretJson["password"]};Database=bia; Pooling=true";
builder.Configuration.GetSection("ConnectionStrings")["DefaultConnection"] = connectionString;
builder.Services.AddDbContext<MeuDbContext>(options =>options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IServiceApi, ServiceApi>();


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();
app.Run();
