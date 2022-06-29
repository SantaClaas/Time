module Time.Client.Data

open System
open System.Collections.Generic
open System.Data
open System.IO
open FSharp.Data

type StartTime = StartTime of DateTimeOffset
type EndTime = EndTime of DateTimeOffset

// https://stackoverflow.com/questions/33075932/how-to-create-a-csv-file-and-write-data-into-in-f
// FSharp.Data does not support DateOnly and TimOnly
type TimeBookingsCsv =
    CsvProvider<Schema="Start (datetimeoffset), End (datetimeoffset), Text (string)", HasHeaders=false>

type TimeBooking = TimeBooking of startTime: StartTime * endTime: EndTime * text: string


let getOrCreateCsv (date: DateOnly) =
    let fileName = $"Time {date:yyyyMMdd}.csv"
    use stream = File.OpenWrite fileName
    TimeBookingsCsv.Load stream

let runWithCsv date (action: TimeBookingsCsv -> TimeBookingsCsv) =
    let fileName = $"Time {date:yyyyMMdd}.csv"
    use stream = File.OpenWrite fileName
    use csv = TimeBookingsCsv.Load stream
    use newCsv = action csv
    newCsv.Save stream


let openBookings () =
    File.Open("Bookings.csv", FileMode.OpenOrCreate)

let add (TimeBooking (StartTime start, EndTime ``end``, text)) =
    use stream = openBookings ()

    Console.WriteLine stream.Length

    use csv =
        if stream.Length = 0 then
            new TimeBookingsCsv([||])
        else
            TimeBookingsCsv.Load stream

    let newRow =
        TimeBookingsCsv.Row(start, ``end``, text)

    use newCsv = csv.Append [ newRow ]
    newCsv.Save stream

let getBookings () =
    use stream = openBookings ()

    if stream.Length = 0 then
        [||]
    else
        use csv = TimeBookingsCsv.Load stream

        csv.Rows
        |> Seq.map (fun row -> TimeBooking(row.Start |> StartTime, row.End |> EndTime, row.Text))
        |> Seq.toArray
