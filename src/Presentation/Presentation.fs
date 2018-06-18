﻿namespace Aardvark.UI.Presentation

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Presentation

type SlideMessage =
    | Activate
    | Deactivate
    | TimePassed of now : MicroTime * delta : MicroTime
    | OpenOverview
    | CloseOverview

type PresentationMessage =
    | OpenOverview
    | CloseOverview
    | Activate of index : SlideIndex
    | TimePassed of now : MicroTime * delta : MicroTime

type Slide =
    {
        content     : App<SlideModel, MSlideModel, SlideMessage>
        subSlides   : list<Slide>
    }


module Slide =
    
    let slide (att : list<string * AttributeValue<SlideMessage>>) (view : MSlideModel -> list<DomNode<SlideMessage>>) =
        { 
            content = 
                {
                    initial = { isActive = false; time = MicroTime.Zero; activeSince = MicroTime.Zero; isOverview = false }
                    view = fun m -> 
                        let att =
                            AttributeMap.ofListCond [
                                yield! att |> List.map always
                                yield always <| clazz "root"
                                yield onlyWhen m.isOverview <| style "pointer-events: none"

                            ]
                        Incremental.div att (AList.ofList (view m))
                        
                        //div (att @ [ clazz "root" ]) (view m)
                    update = fun m msg -> 
                        match msg with
                            | SlideMessage.Activate -> { m with isActive = true; activeSince = m.time }
                            | SlideMessage.Deactivate ->  { m with isActive = false; activeSince = MicroTime.Zero }
                            | SlideMessage.TimePassed(n,d) -> { m with time = n }
                            | SlideMessage.OpenOverview -> { m with isOverview = true }
                            | SlideMessage.CloseOverview -> { m with isOverview = false }

                    threads = fun _ -> ThreadPool.empty
                    unpersist = Unpersist.instance<SlideModel, MSlideModel>
                }
                
            subSlides =
                []
        }
        
    let nested (slide : Slide) (children : list<Slide>) =
        { slide with subSlides = children }

module Presentation =
    open System.Threading


    let private reveal =
        [
            { kind = Stylesheet; name = "reveal"; url = "https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.6.0/css/reveal.min.css" }
            { kind = Stylesheet; name = "revealdark"; url = "https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.6.0/css/theme/black.min.css" }
            { kind = Script; name = "reveal"; url = "./reveal.js" }
            { kind = Stylesheet; name = "revealfixes"; url = "fixes.css" }
        ]
        
    let private boot =
        String.concat " ; " [
            "Reveal.initialize({ width: 1920, height: 1080, margin: 0, minScale: 0.2, maxScale: 2.0 });"
            "Reveal.addEventListener( 'slidechanged', function( e ) { aardvark.processEvent('__ID__', 'slidechanged', e.indexh, e.indexv); });"
            "Reveal.addEventListener( 'overviewshown', function( event ) { aardvark.processEvent('__ID__', 'overview', 1) } );"
            "Reveal.addEventListener( 'overviewhidden', function( event ) { aardvark.processEvent('__ID__', 'overview', 0) } );"
        ]

    let ofSlides (slides : list<Slide>) =

        let rec all (slide : Slide) =
            slide :: (slide.subSlides |> List.collect all)



        let initial =
            {
                active      = { horizontal = 0; vertical = 0 }
                time        = MicroTime.Zero
                overview    = false
            }
        
        let view (m : MPresentationModel) =
            let rec wrap (considerSub : bool) (index : SlideIndex) (c : Slide) =
                match considerSub, c.subSlides with
                    | true, [] 
                    | false, _ ->
                        section [style "width: 100%; height: 100%"] [
                    
                            let mapIn (model : SlideModel) (msg : PresentationMessage) =
                        
                                match msg with
                                    | Activate idx when idx = index ->
                                        Seq.singleton (SlideMessage.Activate)

                                    | Activate _ when model.isActive ->
                                        Seq.singleton (SlideMessage.Deactivate)

                                    | TimePassed (n, delta) when model.isActive && not model.isOverview ->
                                        Seq.singleton (SlideMessage.TimePassed(n, delta))

                                    | OpenOverview ->
                                        Seq.singleton (SlideMessage.OpenOverview)

                                    | CloseOverview ->
                                        Seq.singleton (SlideMessage.CloseOverview)
                                
                                    | _ ->
                                        Seq.empty

                            let content =
                                if index.horizontal = 0 && index.vertical = 0 then
                                    { c.content with initial = { c.content.initial with isActive = true } }
                                else
                                    c.content

                            let app = 
                                subApp' 
                                    (fun _ _ -> Seq.empty) 
                                    mapIn
                                    [ clazz "root" ]
                                    content

                            yield app

                            //let mutable index = index
                            //for s in c.subSlides do
                            //    yield wrap index s
                            //    index <- { index with vertical = index.vertical + 1 }
                        ]
                    | _ ->
                        let all = all c
                        
                        section [style "width: 100%; height: 100%"] (
                            all |> List.mapi (fun i -> wrap false { index with vertical = i })
                        )


            let slideChanged (a : list<string>) =
                match a with
                    | h :: v :: _ ->
                        match System.Int32.TryParse h, System.Int32.TryParse v with
                            | (true, h), (true, v) ->
                                Seq.singleton (Activate { horizontal = h; vertical = v })
                            | _ ->
                                Seq.empty
                    | _ ->
                        Seq.empty
            let overview (a : list<string>) =
                match a with
                    | state :: _ ->
                        match System.Int32.TryParse state with
                            | (true, state) ->
                                if state = 0 then Seq.singleton CloseOverview
                                else Seq.singleton OpenOverview
                            | _ ->
                                Seq.empty
                    | _ ->
                        Seq.empty

            require Html.semui (
                require reveal (
                    onBoot boot (
                        div [clazz "reveal"; onEvent' "slidechanged" [] slideChanged; onEvent' "overview" [] overview ] [
                            div [ clazz "slides" ] (
                                slides |> List.mapi (fun i s -> wrap true { horizontal = i; vertical = 0 } s)
                            )
                            //|> UI.map (fun m -> failwith "slides may not raise messages"; Unchecked.defaultof<PresentationMessage>)
                        ]
                    )
                )
            )
            
        let update (m : PresentationModel) (msg : PresentationMessage) =
            match msg with
                | Activate index ->
                    { m with active = index  }

                | TimePassed(now,d) ->
                    { m with time = now }
                    
                | OpenOverview ->
                    { m with overview = true }

                | CloseOverview ->
                    { m with overview = false }
                
        
        let time =
            let sw = System.Diagnostics.Stopwatch.StartNew()

            let v = MVar.create sw.MicroTime
            let m = new MultimediaTimer.Trigger(16)
            let run() =
                while true do
                    m.Wait()
                    MVar.put v sw.MicroTime
            let thread = Thread(ThreadStart(run), IsBackground = true)
            thread.Start()

            let rec run(last : MicroTime) =
                proclist {
                    let! now = MVar.takeAsync v
                    yield TimePassed(now, now - last)
                    yield! run(now)
                }

            run(sw.MicroTime)

        let threads (m : PresentationModel) =
            ThreadPool.empty
                |> ThreadPool.add "time" time

        {   
            initial = initial
            update = update
            view = view
            threads = threads
            unpersist = Unpersist.instance
        }