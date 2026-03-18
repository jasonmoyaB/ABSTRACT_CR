// Configurar límites de tamaño de archivo
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB
});

// Configurar Kestrel para limitar el tamaño del request
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 5 * 1024 * 1024; // 5MB
});