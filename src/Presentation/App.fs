namespace Presentation

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.Base.Rendering
open Presentation.Model

type Message =
    | ToggleModel
    | CameraMessage of CameraControllerMessage

module App =
    
    let initial = { currentModel = Box; cameraState = CameraController.initial }

    let update (m : Model) (msg : Message) =
        match msg with
            | ToggleModel -> 
                match m.currentModel with
                    | Box -> { m with currentModel = Sphere }
                    | Sphere -> { m with currentModel = Box }

            | CameraMessage msg ->
                { m with cameraState = CameraController.update m.cameraState msg }

    let view (m : MModel) =

        let frustum = 
            Frustum.perspective 60.0 0.1 100.0 1.0 
                |> Mod.constant

        let sg =
            m.currentModel |> Mod.map (fun v ->
                match v with
                    | Box -> Sg.box (Mod.constant C4b.Red) (Mod.constant (Box3d(-V3d.III, V3d.III)))
                    | Sphere -> Sg.sphere 5 (Mod.constant C4b.Green) (Mod.constant 1.0)
            )
            |> Sg.dynamic
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.simpleLighting
            }

        let att =
            [
                style "width: 100pt; height: 100pt"
            ]

        let reveal =
            [
                { kind = Stylesheet; name = "reveal"; url = "https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.6.0/css/reveal.min.css" }
                { kind = Stylesheet; name = "revealdark"; url = "https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.6.0/css/theme/black.min.css" }
                { kind = Script; name = "reveal"; url = "https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.6.0/js/reveal.js" }
                { kind = Stylesheet; name = "revealfixes"; url = "fixes.css" }

            ]

        let slides (content : list<list<DomNode<'msg>>>) =
            require reveal (
                onBoot "Reveal.initialize({ width: '100%', height: '100%', margin: 0, minScale: 1.0, maxScale: 1.0});" (
                    div [clazz "reveal"] [
                        div [ clazz "slides" ] (
                            content |> List.map (fun c -> 
                                section [] c
                            )
                        )
                    ]
                )
            )



            
        slides [
            [
                DomNode.RenderControl(
                    AttributeMap.ofList [ style "width: 100%; height: 100%; text-align: initial; transition: none !important"],
                    Mod.map (fun v -> { cameraView = v; frustum = Frustum.perspective 60.0 0.1 100.0 1.0 }) m.cameraState.view,
                    AList.ofList [
                        Aardvark.UI.RenderCommand.Clear(Some (Mod.constant C4f.White), Some (Mod.constant 1.0))
                        RenderCommand.SceneGraph sg
                    ],
                    None
                )
                |> CameraController.withControls m.cameraState CameraMessage frustum
     
                div [style "position: fixed; left: 20px; top: 20px"] [
                    button [onClick (fun _ -> ToggleModel)] [text "Toggle Model"]
                ]
            ]
            [
                text "slide 2"
            ]
        ]

    let app =
        {
            initial = initial
            update = update
            view = view
            threads = Model.Lens.cameraState.Get >> CameraController.threads >> ThreadPool.map CameraMessage
            unpersist = Unpersist.instance
        }