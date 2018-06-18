namespace Presentation

open System
open Aardvark.Base
open Aardvark.Base.Ag
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.Base.Rendering
open Presentation.Model
open Aardvark.UI.Presentation
open Aardvark.SceneGraph.IO

[<AutoOpen>]
module Show =
    type ShowConfig =
        {
            orbitConfig  : OrbitConfig
            phi : float
            theta : float
            scene : ISg<Orbit.Message>
            attributes : list<string * string>
        }

    type ShowConfigBuilder() =
        member x.Yield(()) = 
            {
                orbitConfig = Orbit.initial.config
                phi = 45.0 * Constant.RadiansPerDegree
                theta = 30.0 * Constant.RadiansPerDegree
                scene = Sg.empty
                attributes = []
            }

        [<CustomOperation("phi")>]
        member x.Phi(s : ShowConfig, phi : float) =
            { s with phi = phi }

        [<CustomOperation("theta")>]
        member x.Theta(s : ShowConfig, theta : float) =
            { s with theta = theta }
            
        [<CustomOperation("scene")>]
        member x.Scene(s : ShowConfig, scene : ISg<Orbit.Message>) =
            { s with scene = scene }
            
        [<CustomOperation("att")>]
        member x.Att(s : ShowConfig, (k,v) : string * AttributeValue<'msg>) =
            match v with
                | AttributeValue.String str -> 
                    { s with attributes = s.attributes @ [k,str] }
                | _ ->
                    s

        member x.Run(c : ShowConfig) : DomNode<SlideMessage> =
            let box : IMod<Box3d> = c.scene?GlobalBoundingBox()
            let b = box.GetValue()

            let r = b.Size.Length
            let phi = 45.0 * Constant.RadiansPerDegree
            let theta = 30.0 * Constant.RadiansPerDegree
            
            let app = Orbit.app' b.Center phi theta r c.scene
            let att = c.attributes |> List.map (fun (k,v) -> k, AttributeValue.String v)

            let mapIn (_model : OrbitModel) (msg : SlideMessage) =
                match msg with
                    | SlideMessage.TimePassed(n,d) ->
                        Seq.singleton (Orbit.Message.TimePassed(n,d))
                    | _ ->
                        Seq.empty

            subApp' (fun _ _ -> Seq.empty<SlideMessage>) mapIn att (
                { app with
                    initial = 
                        { app.initial with 
                            config = c.orbitConfig 
                            phi = c.phi
                            theta = c.theta
                        }
                    threads = fun t -> ThreadPool.remove "time" (app.threads t)
                }
            )
            
    let show = ShowConfigBuilder()


type Message =
    | ToggleModel
    | ToggleFill
    | CameraMessage of CameraControllerMessage

module App =
    
    //let initial = { fill = true; currentModel = Box; cameraState = CameraController.initial }

    //let update (m : Model) (msg : Message) =
    //    match msg with
    //        | ToggleFill ->
    //            { m  with fill = not m.fill }

    //        | ToggleModel -> 
    //            match m.currentModel with
    //                | Box -> { m with currentModel = Sphere }
    //                | Sphere -> { m with currentModel = Box }

    //        | CameraMessage msg ->
    //            { m with cameraState = CameraController.update m.cameraState msg }

    //let view (m : MModel) =

    //    let frustum = 
    //        Frustum.perspective 60.0 0.1 100.0 1.0 
    //            |> Mod.constant

    //    let sg =
    //        m.currentModel |> Mod.map (fun v ->
    //            match v with
    //                | Box -> Sg.box (Mod.constant C4b.Red) (Mod.constant (Box3d(-V3d.III, V3d.III)))
    //                | Sphere -> Sg.sphere 5 (Mod.constant C4b.Green) (Mod.constant 1.0)
    //        )
    //        |> Sg.dynamic
    //        |> Sg.fillMode (m.fill |> Mod.map (function true -> FillMode.Fill | false -> FillMode.Line))
    //        |> Sg.shader {
    //            do! DefaultSurfaces.trafo
    //            do! DefaultSurfaces.simpleLighting
    //        }

    //    let att =
    //        [
    //            style "width: 100pt; height: 100pt"
    //        ]

    //    let reveal =
    //        [
    //            { kind = Stylesheet; name = "reveal"; url = "https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.6.0/css/reveal.min.css" }
    //            { kind = Stylesheet; name = "revealdark"; url = "https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.6.0/css/theme/black.min.css" }
    //            { kind = Script; name = "reveal"; url = "https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.6.0/js/reveal.js" }
    //            { kind = Stylesheet; name = "revealfixes"; url = "fixes.css" }

    //        ]



    //    let boot =
    //        String.concat " ; " [
    //            "Reveal.initialize({ width: '100%', height: '100%', margin: 0, minScale: 0.1, maxScale: 1.0});"
    //            "Reveal.addEventListener( 'slidechanged', function( e ) { aardvark.processEvent('__ID__', 'slidechanged', e.indexh, e.indexv); });"
    //        ]

    //    let slides (content : list<list<DomNode<'msg>>>) =
    //        require Html.semui (
    //            require reveal (
    //                onBoot boot (
    //                    div [clazz "reveal"; onEvent' "slidechanged" [] (fun a -> Log.warn "%A" a; Seq.empty) ] [
    //                        div [ clazz "slides" ] (
    //                            content |> List.map (fun c -> 
    //                                section [style "width: 100%; height: 100%"] [
    //                                    div [ clazz "root" ] c 
    //                                ]
    //                            )
    //                        )
    //                    ]
    //                )
    //            )
    //        )

    //    let slides (content : list<list<DomNode<'msg>>>) =
    //        require Html.semui (
    //            require reveal (
    //                onBoot boot (
    //                    div [clazz "reveal"; onEvent' "slidechanged" [] (fun a -> Log.warn "%A" a; Seq.empty) ] [
    //                        div [ clazz "slides" ] (
    //                            content |> List.map (fun c -> 
    //                                section [style "width: 100%; height: 100%"] [
    //                                    div [ clazz "root" ] c 
    //                                ]
    //                            )
    //                        )
    //                    ]
    //                )
    //            )
    //        )         
    //    //let show att (scene : ISg<Orbit.Message>) =
    //    //    let box : IMod<Box3d> = scene?GlobalBoundingBox()
    //    //    let b = box.GetValue()

    //    //    let r = b.Size.Length
    //    //    let phi = 45.0 * Constant.RadiansPerDegree
    //    //    let theta = 30.0 * Constant.RadiansPerDegree

    //    //    let app = Orbit.app' b.Center phi theta r scene
    //    //    subApp att (
    //    //        { app with
    //    //            initial = { app.initial with config = app.initial.config }
    //    //        }
    //    //    )
            
    //    slides [
    //        [
    //            //div [style "background: red; width: 50%; height: 50%"][]
                
    //            h2 [] [text "Show Builder"]
    //            text "The show builder provides an easy mechanism to show a scene using a simple orbit controller"
    //            show {
    //                att (style "width: 85%; height: 70%; background: #FFFFFF")
    //                scene (
    //                    Sg.box (Mod.constant C4b.Green) (Mod.constant Box3d.Unit)
    //                    |> Sg.shader {
    //                        do! DefaultSurfaces.trafo
    //                        do! DefaultSurfaces.vertexColor
    //                        do! DefaultSurfaces.simpleLighting
    //                    }
    //                )
    //            }

    //            //show [style "width: 85%; height: 85%; background: #FFFFFF" ] (
    //            //    Sg.box (Mod.constant C4b.Green) (Mod.constant Box3d.Unit)
    //            //        |> Sg.shader {
    //            //            do! DefaultSurfaces.trafo
    //            //            do! DefaultSurfaces.vertexColor
    //            //            do! DefaultSurfaces.simpleLighting
    //            //        }
    //            //)
                

    //        ]
    //        [
    //            h2 [] [text "I am cube"]

                
    //            DomNode.RenderControl(
    //                AttributeMap.ofList [ style "width: 50%; height: 50%; background: #222"],
    //                Mod.map (fun v -> { cameraView = v; frustum = Frustum.perspective 60.0 0.1 100.0 1.0 }) m.cameraState.view,
    //                AList.ofList [
    //                    Aardvark.UI.RenderCommand.Clear(Some (Mod.constant (C4b(34uy, 34uy, 34uy, 255uy).ToC4f())), Some (Mod.constant 1.0))
    //                    RenderCommand.SceneGraph sg
    //                ],
    //                None
    //            )
    //            |> CameraController.withControls m.cameraState CameraMessage frustum
                
    //            ul [] [
    //                li [] [text "Bullet 1"]
    //                li [] [text "Bullet 2"]
    //                li [] [ 
    //                    text "Wireframe: "
    //                    div [ clazz "ui mini toggle checkbox"] [
    //                        input [attribute "type" "checkbox"; onChange (fun _ -> ToggleFill) ]
    //                        label [] []
    //                    ]
    //                ]
    //            ]

    //        ]
    //        [
    //            h1 [] [text "Hi There"]
    //            ul [] [
    //                li [] [text "Bullet 1"]
    //                li [] [text "Bullet 2"]
    //            ]
    //        ]


    //        [
    //            div [ style "width: 100%; height: 100%"] [
    //                img [attribute "src" "https://upload.wikimedia.org/wikipedia/commons/e/e0/Clouds_over_the_Atlantic_Ocean.jpg"]
    //            ]
    //        ]

    //        //    //div [style "position: fixed; left: 20px; top: 20px"] [
    //        //    //    button [onClick (fun _ -> ToggleModel)] [text "Toggle Model"]
    //        //    //]
    //        //]
    //        //[
    //        //    DomNode.RenderControl(
    //        //        AttributeMap.ofList [ style "position: relative; left: 0; top: 0; width: 100%; height: calc(100% - 50px)"],
    //        //        Mod.map (fun v -> { cameraView = v; frustum = Frustum.perspective 60.0 0.1 100.0 1.0 }) m.cameraState.view,
    //        //        AList.ofList [
    //        //            Aardvark.UI.RenderCommand.Clear(Some (Mod.constant C4f.VRVisGreen), Some (Mod.constant 1.0))
    //        //            RenderCommand.SceneGraph sg
    //        //        ],
    //        //        None
    //        //    )
    //        //    |> CameraController.withControls m.cameraState CameraMessage frustum
     
    //        //]
    //    ]

    //let app =
    //    {
    //        initial = initial
    //        update = update
    //        view = view
    //        threads = Model.Lens.cameraState.Get >> CameraController.threads >> ThreadPool.map CameraMessage
    //        unpersist = Unpersist.instance
    //    }


    module Eigi =
        open Aardvark.Base.Incremental.Operators

        module Skinning = 
            open FShade

            type Vertex =
                {
                    [<Position>]    pos : V4d
                    [<Normal>]      n : V3d
                    [<BiNormal>]    b : V3d
                    [<Tangent>]     t : V3d

                    [<Semantic("VertexBoneIndices4")>] vbi : V4i
                    [<Semantic("VertexBoneWeights4")>] vbw : V4d

                }

            type UniformScope with
                member x.Bones : M44d[] = x?StorageBuffer?Bones
                member x.MeshTrafoBone : int = x?MeshTrafoBone
                member x.NumFrames : int = x?NumFrames
                member x.NumBones : int = x?NumBones
                member x.Framerate : float = x?Framerate
                member x.Time : float = x?Time
                member x.FrameRange : V2d = x?FrameRange
                member x.TimeOffset : float = x?TimeOffset
            [<ReflectedDefinition>]
            let getBoneTransform (i : V4i) (w : V4d) =
                let mutable res = M44d.Zero
                let mutable wSum = 0.0

                if i.X >= 0 then 
                    res <- res + w.X * uniform.Bones.[i.X]
                    wSum <- wSum + w.X

                if i.Y >= 0 then 
                    res <- res + w.Y * uniform.Bones.[i.Y]
                    wSum <- wSum + w.Y

                if i.Z >= 0 then 
                    res <- res + w.Z * uniform.Bones.[i.Z]
                    wSum <- wSum + w.Z

                if i.W >= 0 then 
                    res <- res + w.W * uniform.Bones.[i.W]
                    wSum <- wSum + w.W


                let id = M44d.Identity
                if wSum >= 1.0 then res
                elif wSum <= 0.0 then id
                else (1.0 - wSum) * id + wSum * res
            
            [<ReflectedDefinition>]
            let lerp (i : int) (f0 : int) (f1 : int) (t : float) =
                uniform.Bones.[f0 + i] * (1.0 - t) + uniform.Bones.[f1 + i] * t

            [<ReflectedDefinition>]
            let getBoneTransformFrame (i : V4i) (w : V4d) =
                let mutable res = M44d.Zero
                let mutable wSum = 0.0

                let iid = FShade.Imperative.ExpressionExtensions.ShaderIO.ReadInput<int>(Imperative.ParameterKind.Input, FShade.Intrinsics.InstanceId)

                let frame = (uniform.Time + uniform.TimeOffset) * uniform.Framerate
                let range = uniform.FrameRange
                let l = range.Y - range.X
                let frame = (l + (frame % l)) % l + range.X

                let f0 = int (floor frame)
                let f1 = ((f0 + 1) % uniform.NumFrames)
                let t = frame - float f0
                let b0 = f0 * uniform.NumBones
                let b1 = f1 * uniform.NumBones


                if i.X >= 0 then 
                    res <- res + w.X * lerp i.X b0 b1 t
                    wSum <- wSum + w.X

                if i.Y >= 0 then 
                    res <- res + w.Y * lerp i.Y b0 b1 t
                    wSum <- wSum + w.Y

                if i.Z >= 0 then 
                    res <- res + w.Z * lerp i.Z b0 b1 t
                    wSum <- wSum + w.Z

                if i.W >= 0 then 
                    res <- res + w.W * lerp i.W b0 b1 t
                    wSum <- wSum + w.W

                let meshTrafo =
                    uniform.Bones.[b0 + uniform.MeshTrafoBone] * (1.0 - t) + uniform.Bones.[b1 + uniform.MeshTrafoBone] * t

                if wSum <= 0.0 then meshTrafo
                else meshTrafo * res

            let skinning (v : Vertex) =
                vertex {
                    //let model = uniform.Bones.[uniform.MeshTrafoBone]
                    let mat = getBoneTransformFrame v.vbi v.vbw
                    //let mat = model * skin

                    return { 
                        pos = mat * v.pos
                        n = mat.TransformDir(v.n)
                        b = mat.TransformDir(v.b)
                        t = mat.TransformDir(v.t) 
                        vbi = V4i(-1,-1,-1,-1)
                        vbw = V4d.Zero
                    }
                }

        // load the model
        let scene = Loader.Assimp.loadFrom @"C:\Users\Schorsch\Desktop\raptor\raptor.dae" (Loader.Assimp.defaultFlags)// ||| Assimp.PostProcessSteps.FlipUVs)
        
        module Animation =
            let idle    = Range1d(50.0, 100.0)
            let walk    = Range1d(0.0, 36.0)
            let attack  = Range1d(150.0, 180.0)
            let die     = Range1d(200.0, 230.0)
        
            let arr = [| idle; walk; attack; die |] //; walk; attack; die |]

            let animation = scene.animantions |> Map.toSeq |> Seq.head |> snd

            let allBones =
                [|
                    let dt = 1.0 / animation.framesPerSecond
                    let mutable t = 0.0
                    for f in 0 .. animation.frames - 1 do
                        let trafos = animation.interpolate t
                        yield! trafos |> Array.map M44f.op_Explicit
                        t <- t + dt
                |]

            let numFrames = animation.frames // |> Mod.map (fun (a,_) -> a.frames)
            let numBones = animation.interpolate 0.0 |> Array.length // |> Mod.map (fun (a,_) -> a.interpolate 0.0 |> Array.length)
            let fps = animation.framesPerSecond // |> Mod.map (fun (a,_) -> a.framesPerSecond)

            let trafos =
                let s = V2i(5, 5)
                [|
                    for x in -s.X .. s.X do
                        for y in -s.Y .. s.Y do
                            yield Trafo3d.Translation(float x, float y, 0.0)
                |]

        let sg (time : IMod<MicroTime>)= 
            scene 
                |> Sg.adapter
                |> Sg.uniform "Bones" ~~Animation.allBones
                |> Sg.uniform "NumFrames" ~~Animation.numFrames
                |> Sg.uniform "NumBones" ~~Animation.numBones
                |> Sg.uniform "Framerate" ~~Animation.fps
                |> Sg.uniform "FrameRange" ~~(V2d(0.0, 36.0))
                |> Sg.uniform "Time" (time |> Mod.map (fun m -> m.TotalSeconds))
                |> Sg.uniform "TimeOffset" ~~0.0
                |> Sg.transform (Trafo3d.FromBasis(V3d.IOO, V3d.OOI, V3d.OIO, V3d.Zero) * Trafo3d.Scale 20.0)
                // apply all shaders we have
                |> Sg.shader {
                    do! Skinning.skinning
                    do! DefaultSurfaces.trafo
                    do! DefaultSurfaces.diffuseTexture
                    do! DefaultSurfaces.simpleLighting
                }

    let newApp =
        Presentation.ofSlides [

            Slide.slide [] (fun m ->
                [
                    h1 [] [ text "FShade" ]
                    h4 [] [ text "Functional Shaders" ]
                    text "Georg Haaser"
                ]
            )
            Slide.slide [] (fun m ->
                [
                    div [clazz "simple"] [
                        h2 [ clazz "header" ] [ text "Motivation" ]
                        div [ clazz "content" ] [
                            ul [] [
                                li [] [ text "asdasd" ]
                                li [] [ text "asasddasd" ]
                                li [] [ text "assadadasd" ]
                                li [] [ text "asasdasd" ]
                            ]
                        ]
                    ]
                ]
            )
            Slide.slide [] (fun m ->
                let c = m.isActive |> Mod.map (function true -> "Active" | false -> "Inactive")

                [
                    h1 [] [ Incremental.text c ]
                ]
            )
            
            //Slide.slide [] (fun m ->
            //    let c = m.isActive |> Mod.map (function true -> "Active 2" | false -> "Inactive 2")

            //    [
            //        h1 [] [ Incremental.text c ]
            //    ]
            //)

            //Slide.slide [] (fun m ->
            //    let c = m.isActive |> Mod.map (function true -> "Active 3" | false -> "Inactive 3")

            //    [
            //        h1 [] [ Incremental.text c ]
            //    ]
            //)

            Slide.nested
                (
                    Slide.slide [] (fun m ->
                        [
                            h2 [] [text "Show Builder"]
                            text "The show builder provides an easy mechanism to show a scene using a simple orbit controller"
                            show {
                                att (style "width: 85%; height: 70%; background: #FFFFFF")
                                scene (Eigi.sg m.time)
                            }
                        ]
                    )
                )
                [

                    Slide.slide [] (fun m ->
                        [
                            h2 [] [text "Textures"]
                            show {
                                att (style "width: 85%; height: 70%; background: #FFFFFF")
                                scene (
                                    Sg.box (Mod.constant C4b.Green) (Mod.constant Box3d.Unit)
                                    |> Sg.diffuseTexture DefaultTextures.checkerboard
                                    |> Sg.shader {
                                        do! DefaultSurfaces.trafo
                                        do! DefaultSurfaces.diffuseTexture
                                        do! DefaultSurfaces.simpleLighting
                                    }
                                )
                            }
                        ]
                    )
                ]

        ]