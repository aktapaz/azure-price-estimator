var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("AzureRetailPrices", client =>
{
    client.BaseAddress = new Uri("https://prices.azure.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();