using BluinoBlock.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
//signalR
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

var provider = new FileExtensionContentTypeProvider();
// Add new mappings
provider.Mappings[".fx"] = "application/fx";
provider.Mappings[".gltf"] = "model/vnd.gltf+json";
provider.Mappings[".glb"] = "application/octet-stream";
app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider
});

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//signalR
app.MapHub<DeviceHub>("/deviceHub");

app.Run();
