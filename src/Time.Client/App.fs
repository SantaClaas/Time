module Time.Client.App

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Time.Client.Data

/// Routing endpoints definition.
type Page = | [<EndPoint "/">] Home
type BookingCompletionError = BookingCompletionError of string

type IncompleteBooking =
    { start: StartTime
      ``end``: EndTime
      text: string option }

let completeBooking (incomplete: IncompleteBooking) : Result<TimeBooking, BookingCompletionError> =
    match incomplete with
    //    | { start = None } ->
//        BookingCompletionError "Start time is missing"
//        |> Error
//    | { ``end`` = None } ->
//        BookingCompletionError "End time is missing"
//        |> Error
    | { text = None } -> BookingCompletionError "Text is missing" |> Error
    | { start = start
        ``end`` = ``end``
        text = Some text } -> TimeBooking(start, ``end``, text) |> Ok



type BookingsState =
    | Loading
    | Loaded of TimeBooking array

/// The Elmish application's model.
type Model =
    { page: Page
      newBookingError: BookingCompletionError option
      newBooking: IncompleteBooking
      bookings: BookingsState
      error: string option }

let createBookingForHour (date: DateTimeOffset) =
    // Start log at start of hour and end it at end of hour
    let timeSinceHourStart =
        -date.Hour
        |> TimeSpan.FromHours
        |> date.TimeOfDay.Add

    let start = date - timeSinceHourStart

    let ``end`` = start + TimeSpan.FromHours 1

    { start = StartTime start
      ``end`` = EndTime ``end``
      text = None }

let initialModel =
    { page = Home
      error = None
      newBookingError = None
      bookings = Loading
      newBooking = createBookingForHour DateTimeOffset.Now }

type BookingBoundary =
    | Start of StartTime
    | End of EndTime

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | SetStart of StartTime
    | SetEnd of EndTime
    | SetText of string
    | SetBookingsState of BookingsState
    | Submit
    | LogError of Exception

let update message model =
    match message with
    | SetPage page -> { model with page = page }, Cmd.none
    | SetStart startTime -> { model with newBooking = { model.newBooking with start = startTime } }, Cmd.none
    | SetEnd endTime -> { model with newBooking = { model.newBooking with ``end`` = endTime } }, Cmd.none
    | SetText newText -> { model with newBooking = { model.newBooking with text = Some newText } }, Cmd.none
    | Submit ->
        let completionResult =
            completeBooking model.newBooking

        printfn "Submitted booking %O" model.newBooking

        match completionResult with
        | Ok booking ->
            // Add to file, bookings and reset new booking
            let newBookings =
                match model.bookings with
                | Loaded bookings -> bookings |> Array.append [| booking |] |> Loaded
                | _ -> model.bookings

            //TODO remove booking from model when adding and saving fails or only add after save
            //TODO set state of submit button to saving while we do IO
            //TODO determine date of next new booking
            { model with
                bookings = newBookings
                newBooking = createBookingForHour DateTimeOffset.Now },
            Cmd.OfFunc.attempt add booking LogError
        | Error error -> { model with newBookingError = Some error }, Cmd.none
    | SetBookingsState state -> { model with bookings = state }, Cmd.none
    | LogError ``exception`` ->
        Console.WriteLine ``exception``
        { model with error = Some ``exception``.Message }, Cmd.none

/// Connects the routing system to the Elmish application.
let router =
    Router.infer SetPage (fun model -> model.page)

let parseTime (timeString: string) = DateTimeOffset.Parse(timeString)

// 15 minute steps
[<Literal>]
let timeStepSize = 900

let bookingDialog model dispatch =
    dialog {
        attr.``open`` true
        attr.``class`` "fixed bottom-0 left-0 w-11/12 opacity-100 rounded-t-xl p-0 shadow"

        form {
            on.submit (fun _ -> dispatch Message.Submit)
            attr.``class`` "flex flex-col gap-2 bg-white/10 p-3 rounded-t-xl border-x border-t"

            label {
                attr.``for`` "start"
                attr.``class`` "text-slate-300"
                "Start"
            }

            input {
                attr.id "start"
                attr.``type`` "datetime-local"
                attr.required true
                attr.``class`` "text-slate-400"
                attr.step timeStepSize

                let value =
                    match model.newBooking.start with
                    | StartTime start -> start.ToLocalTime().ToString("s")

                bind.input.string value (parseTime >> StartTime >> SetStart >> dispatch)
            }

            label {
                attr.``for`` "end"
                attr.``class`` "text-slate-300"
                "End"
            }

            input {
                attr.id "end"
                attr.``type`` "datetime-local"
                attr.required true
                attr.``class`` "text-slate-400"
                attr.step timeStepSize

                let value =
                    match model.newBooking.``end`` with
                    | EndTime ``end`` -> ``end``.ToLocalTime().ToString("s")

                bind.input.string value (parseTime >> EndTime >> SetEnd >> dispatch)
            }

            label {
                attr.``for`` "text"
                attr.``class`` "text-slate-300"
                "Text"
            }

            input {
                attr.id "text"
                attr.autofocus true
                attr.``type`` "text"
                attr.required true
                attr.``class`` "text-slate-400"

                let value =
                    model.newBooking.text
                    |> Option.defaultValue String.Empty

                bind.input.string value (SetText >> dispatch)
            }

            div {
                attr.``class`` "flex justify-end"

                button {
                    attr.``type`` "submit"
                    attr.``class`` "bg-green-400 p-2 rounded-full text-slate-900 uppercase font-semibold"
                    "Submit"
                }
            }
        }
    }

[<Literal>]
let tableHeaderStyle =
    "sticky top-0 py-4 px-3 \
                        border-b border-green-800
                        bg-white/10 bg-opacity-75
                        text-start text-slate-300 font-semibold
                        backdrop-blur backdrop-filter"

[<Literal>]
let tableDataStyle =
    "border-b border-green-800 px-3 py-4 text-gray-400"

let bookingsTable model =
    div {
        attr.``class`` "bg-white/5 p-3 rounded-xl"

        cond model.bookings (fun bookings ->
            match bookings with
            | Loading -> p { "Loading bookings..." }
            | Loaded bookings ->
                table {
                    attr.``class`` "table-auto text-start w-full border-separate border-spacing-0"

                    thead {
                        tr {
                            th {
                                attr.``class`` tableHeaderStyle
                                "Start"
                            }

                            th {
                                attr.``class`` tableHeaderStyle
                                "End"
                            }

                            th {
                                attr.``class`` tableHeaderStyle
                                "Activity"
                            }
                        }
                    }

                    tbody {
                        forEach bookings (fun (TimeBooking (StartTime start, EndTime endTime, text)) ->
                            tr {

                                td {
                                    attr.``class`` tableDataStyle
                                    start.ToString("H:mm")
                                }

                                td {
                                    attr.``class`` tableDataStyle
                                    endTime.ToString("H:mm")
                                }

                                td {
                                    attr.``class`` tableDataStyle
                                    text
                                }
                            })
                    }
                })
    }

let view (model: Model) dispatch =
    main {
        attr.``class`` "p-4 flex flex-col gap-3"

        cond model.error (fun option ->
            match option with
            | Some ``exception`` ->
                div {
                    attr.``class`` "bg-red-600 rounded-xl p-3"
                    ``exception``.ToString()
                }
            | None -> empty ())

        bookingsTable model
        bookingDialog model dispatch
    }

let loadBookings =
    getBookings >> BookingsState.Loaded

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        Program.mkProgram
            (fun _ -> initialModel, Cmd.OfFunc.either loadBookings () SetBookingsState LogError)
            update
            view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
