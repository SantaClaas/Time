namespace Time.Server

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Bolero
open Bolero.Remoting.Server
open Bolero.Server
open Time
open Bolero.Templating.Server

module Program =

    [<EntryPoint>]
    let main arguments =
        let builder = WebApplication.CreateBuilder(arguments)
        builder.Services.AddRazorPages() |> ignore
        builder.Services.AddServerSideBlazor() |> ignore
        builder.Services.AddBoleroHost(server = true) |> ignore
#if DEBUG
        builder.Services.AddHotReload(fun configuration -> { configuration with Directory = __SOURCE_DIRECTORY__ + "/../Time.Client" }) |> ignore
#endif

        let app = builder.Build()

        app
            .UseHttpsRedirection()
            .UseStaticFiles()
            .UseRouting()
            .UseBlazorFrameworkFiles()
            .UseEndpoints(fun endpoints ->
#if DEBUG
                endpoints.UseHotReload() 
#endif
                endpoints.MapBlazorHub() |> ignore

                endpoints.MapFallbackToBolero(Index.page)
                |> ignore)
            |> ignore

        app.Run()
        0
