module Time.Server.Index

open Bolero
open Bolero.Html
open Bolero.Server.Html
open Time

let page =
    doctypeHtml {
        head {
            meta { attr.charset "UTF-8" }

            meta {
                attr.name "viewport"
                attr.content "width=device-width, initial-scale=1.0"
            }

            title { "Time" }
            ``base`` { attr.href "/" }
            // Automatic dark mode
            meta {
                attr.name "color-scheme"
                attr.content "dark light"
            }

            meta {
                attr.name "theme-color"
                attr.content "#d800e9"
            }

            meta {
                attr.name "accent-color"
                attr.content "#d800e9"
            }
            // For dark/light mode specifically
            meta {
                attr.name "theme-color"
                attr.content "#d800e9"
            }

            meta {
                attr.name "accent-color"
                attr.content "#872e4e"
                attr.media "(prefers-color-scheme: dark)"
            }
#if DEBUG
            // Tailwind CSS play CDN only in development
            script { attr.src "https://cdn.tailwindcss.com" }
            style { attr.``type`` "text/tailwindcss" }
#endif
            link {
                attr.rel "stylesheet"
                attr.href "css/index.css"
            }
        }

        body {
            div {
                attr.id "app"
                rootComp<Client.App.MyApp>
            }

            boleroScript
        }
    }
