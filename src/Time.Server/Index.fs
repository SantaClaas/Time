module Time.Server.Index

open Bolero
open Bolero.Html
open Bolero.Server.Html
open Time

let page = doctypeHtml {
    head {
        meta { attr.charset "UTF-8" }
        meta { attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0" }
        title { "Time" }
        ``base`` { attr.href "/" }
        link { attr.rel "stylesheet"; attr.href "css/index.css" }
    }
    body {
        div { attr.id "app"; rootComp<Client.Main.MyApp> }
        boleroScript
    }
}
