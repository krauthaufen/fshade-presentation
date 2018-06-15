namespace Presentation

open System
open Aardvark.Base
open Aardvark.Base.Ag
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.Base.Rendering
open Presentation.Model

type Message =
    | ToggleModel
    | ToggleFill
    | CameraMessage of CameraControllerMessage

module App =
    
    let initial = { fill = true; currentModel = Box; cameraState = CameraController.initial }

    let update (m : Model) (msg : Message) =
        match msg with
            | ToggleFill ->
                { m  with fill = not m.fill }

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
            |> Sg.fillMode (m.fill |> Mod.map (function true -> FillMode.Fill | false -> FillMode.Line))
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
            require Html.semui (
                require reveal (
                    onBoot "Reveal.initialize({ width: '100%', height: '100%', margin: 0, minScale: 1.0, maxScale: 1.0});" (
                        div [clazz "reveal"] [
                            div [ clazz "slides" ] (
                                content |> List.map (fun c -> 
                                    section [style "width: 100%; height: 100%"] [
                                        div [ clazz "root" ] c 
                                    ]
                                )
                            )
                        ]
                    )
                )
            )
            
        let show att (scene : ISg<Orbit.Message>) =
            let box : IMod<Box3d> = scene?GlobalBoundingBox()
            let b = box.GetValue()

            let r = b.Size.Length
            let phi = 45.0 * Constant.RadiansPerDegree
            let theta = 30.0 * Constant.RadiansPerDegree

            subApp att (
                Orbit.app' b.Center phi theta r scene
            )
            
        slides [
            [
                //div [style "background: red; width: 50%; height: 50%"][]

                show [style "width: 85%; height: 85%; background: #FFFFFF" ] (
                    Sg.box (Mod.constant C4b.Green) (Mod.constant Box3d.Unit)
                        |> Sg.shader {
                            do! DefaultSurfaces.trafo
                            do! DefaultSurfaces.vertexColor
                            do! DefaultSurfaces.simpleLighting
                        }
                )
                

            ]
            [
                h2 [] [text "I am cube"]

                
                DomNode.RenderControl(
                    AttributeMap.ofList [ style "width: 50%; height: 50%; background: #222"],
                    Mod.map (fun v -> { cameraView = v; frustum = Frustum.perspective 60.0 0.1 100.0 1.0 }) m.cameraState.view,
                    AList.ofList [
                        Aardvark.UI.RenderCommand.Clear(Some (Mod.constant (C4b(34uy, 34uy, 34uy, 255uy).ToC4f())), Some (Mod.constant 1.0))
                        RenderCommand.SceneGraph sg
                    ],
                    None
                )
                |> CameraController.withControls m.cameraState CameraMessage frustum
                
                ul [] [
                    li [] [text "Bullet 1"]
                    li [] [text "Bullet 2"]
                    li [] [ 
                        text "Wireframe: "
                        div [ clazz "ui mini toggle checkbox"] [
                            input [attribute "type" "checkbox"; onChange (fun _ -> ToggleFill) ]
                            label [] []
                        ]
                    ]
                ]

            ]
            [
                h1 [] [text "Hi There"]
                ul [] [
                    li [] [text "Bullet 1"]
                    li [] [text "Bullet 2"]
                ]
            ]


            [
                div [ style "width: 100%; height: 100%"] [
                    img [attribute "src" "https://upload.wikimedia.org/wikipedia/commons/e/e0/Clouds_over_the_Atlantic_Ocean.jpg"]
                ]
            ]

            //    //div [style "position: fixed; left: 20px; top: 20px"] [
            //    //    button [onClick (fun _ -> ToggleModel)] [text "Toggle Model"]
            //    //]
            //]
            //[
            //    DomNode.RenderControl(
            //        AttributeMap.ofList [ style "position: relative; left: 0; top: 0; width: 100%; height: calc(100% - 50px)"],
            //        Mod.map (fun v -> { cameraView = v; frustum = Frustum.perspective 60.0 0.1 100.0 1.0 }) m.cameraState.view,
            //        AList.ofList [
            //            Aardvark.UI.RenderCommand.Clear(Some (Mod.constant C4f.VRVisGreen), Some (Mod.constant 1.0))
            //            RenderCommand.SceneGraph sg
            //        ],
            //        None
            //    )
            //    |> CameraController.withControls m.cameraState CameraMessage frustum
     
            //]
        ]

    let app =
        {
            initial = initial
            update = update
            view = view
            threads = Model.Lens.cameraState.Get >> CameraController.threads >> ThreadPool.map CameraMessage
            unpersist = Unpersist.instance
        }