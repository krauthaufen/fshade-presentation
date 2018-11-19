open Presentation

open Aardium
open Aardvark.Service
open Aardvark.UI
open Suave
open Suave.WebPart
open Aardvark.Rendering.Vulkan
open Aardvark.Base
open System
open Suave
open System.Reflection
open Aardvark.Application.Slim



[<EntryPoint>]
let main args =
    Ag.initialize()
    Aardvark.Init()
    Aardium.init()

    let port = 
        match args with
            | [| port |] ->
                match Int32.TryParse port with
                    | (true, p) -> p
                    | _ -> 4321
            | _ ->
                4321

    let app = new OpenGlApplication()

    WebPart.startServer port [
        MutableApp.toWebPart' app.Runtime false (App.start App.newApp)
        Reflection.assemblyWebPart (Assembly.GetEntryAssembly())
    ]

    Aardium.run {
        title "Aardvark rocks \\o/"
        width 1024
        height 768
        url "http://localhost:4321/"
    }

    0
