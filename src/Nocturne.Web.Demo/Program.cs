// The Nocturne demo for the web: one page with the pages of src/Nocturne.Wpf.Demo, each a partial
// view named after the WPF UserControl (Views/Demo/Pages/ControlsPage.cshtml ↔ Views/ControlsPage.xaml).
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();

// en-US, as the WPF demo forces for its snapshots
app.UseRequestLocalization("en-US");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Demo}/{action=Index}");

// The data grid page's server mode: one page of the cars at a time, searched, filtered and sorted here
app.MapPost("/api/cars", (Nocturne.Web.NocturneGridRequest request) => Nocturne.Web.NocturneGridServer.Query(Nocturne.Wpf.Demo.CarData.Cars, request));

app.Run();
