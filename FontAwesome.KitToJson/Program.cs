using ConsoleAppFramework;
using FontAwesome.KitToJson;

var app = ConsoleApp.Create();

app.Add<App>();

await app.RunAsync(args);