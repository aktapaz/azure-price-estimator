var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("AzureRetailPrices", client =>
{
    client.BaseAddress = new Uri("https://prices.azure.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AzureRetailPricesPolicy",
        builder =>
        {
            builder.WithOrigins("https://prices.azure.com")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();
// Enable CORS
app.UseCors("AzureRetailPricesPolicy");
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