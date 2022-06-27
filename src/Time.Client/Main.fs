module Time.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home

/// The Elmish application's model.
type Model =
    {
        page: Page
        error: string option
    }

and Book =
    {
        title: string
        author: string
        publishDate: DateTime
        isbn: string
    }

let initModel =
    {
        page = Home
        error = None
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Error of exn

let update message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none
    | Error ``exception`` ->
        { model with error = Some ``exception``.Message }, Cmd.none
  
/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)


let view model dispatch =
     div { "hello world" }

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let update = update
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
