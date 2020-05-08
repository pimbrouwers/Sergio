open System.IO
open Argu
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open System

type CliArguments =
    | [<MainCommand; ExactlyOnce; First>] Root of path:string
    | Listener of string * int
    | Log_Level of int
    | GZip of bool
with 
    interface IArgParserTemplate with 
        member s.Usage =
            match s with
            | Root _      -> "specify a working directory"
            | Listener _  -> "specify a listener (ex: --listener localhost 5001)"
            | Log_Level _ -> "set the log level (default = LogLevel.Error)"
            | GZip _      -> "enable gzip compress (default = False)"
    
[<EntryPoint>]
let main argv =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<CliArguments>(programName = "Sergio", errorHandler = errorHandler)
    
    match argv with
    | [||] -> parser.PrintUsage() |> printfn "%s"
    | _    ->
        let results = parser.ParseCommandLine argv
        
        let root = results.GetResult (Root, defaultValue = "/")        
        let webRoot = 
            match root |> Path.IsPathRooted with
            | true -> root
            | false -> Path.Combine [| Directory.GetCurrentDirectory(); root |]
        
        let listener = results.GetResult (Listener, defaultValue = ("localhost", 8080))
        let url = listener |> fun (l, p) -> sprintf "https://%s:%i" l p
          
        let logLevel = results.GetResult (Log_Level, defaultValue = 4)
        let configureLog =
            fun (log : ILoggingBuilder) ->
                log.AddFilter(fun l -> l >= (LogLevel.ToObject(typedefof<LogLevel>, logLevel) :?> LogLevel))
                   .AddConsole() |> ignore

        let gzip = results.GetResult (GZip, defaultValue = false)        

        WebHostBuilder()
            .ConfigureLogging(configureLog)
            .UseKestrel()  
            .UseContentRoot(webRoot)
            .UseWebRoot(webRoot)
            .UseUrls(url)
            .ConfigureServices(fun services ->
                if gzip then services.AddResponseCompression(fun compression -> compression.EnableForHttps <- true) |> ignore)
            .Configure(fun app ->
                if gzip then app.UseResponseCompression() |> ignore

                app.UseDefaultFiles()
                   .UseStaticFiles()
                |> ignore)
            .Build()
            .Run()

    0 // return an integer exit code
