namespace Presentation

open System
open Aardvark.Base
open Aardvark.Base.Ag
open Aardvark.Base.Incremental
open Aardvark.Base.Incremental.Operators
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
            r : Option<float>
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
                r = None
            }
                
        [<CustomOperation("phi")>]
        member x.Phi(s : ShowConfig, phi : float) =
            { s with phi = phi }
            
        [<CustomOperation("distance")>]
        member x.Dist(s : ShowConfig, phi : float) =
            { s with r = Some phi }

        [<CustomOperation("theta")>]
        member x.Theta(s : ShowConfig, theta : float) =
            { s with theta = theta }
            
        [<CustomOperation("scene")>]
        member x.Scene(s : ShowConfig, scene : ISg<Orbit.Message>) =
            { s with scene = scene }
             
        [<CustomOperation("rotDelay")>]
        member x.RotDelay(s : ShowConfig, scene : float) =
            { s with orbitConfig = { s.orbitConfig with autoRotateDelay = MicroTime(TimeSpan.FromSeconds scene) } }
            
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

            let r = match c.r with | Some r -> r | _ -> b.Size.Length
            let phi = c.phi
            let theta = c.theta
            
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
    
    let unescape (str : string) =
        str.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n")

    let code (lang : string) (code : IMod<string>) =
        let initial = Mod.force code |> unescape
        let boot =
            String.concat "\r\n" [
                "document.getElementById('__ID__').innerHTML = hljs.highlight('" + lang + "', \"" + initial + "\", true).value"
                "code.onmessage = function(c) { document.getElementById('__ID__').innerHTML = hljs.highlight('" + lang + "', c, true).value };"
            ]
        onBoot' ["code", Mod.channel code ] boot (
            DomNode<_>("pre", None, AttributeMap.ofList [clazz ("hljs " + lang)] , DomContent.Empty)
        )

    let newApp =
        

        let many (sgs : list<ISg<_>>) =
            let rec manyAcc pass sgs =
                match sgs with
                    | [] -> Sg.empty
                    | [s] -> s |> Sg.pass pass
                    | h :: t ->
                        let name = Guid.NewGuid() |> string
                        Sg.ofList [
                            h |> Sg.pass pass                            
                            manyAcc (RenderPass.after name RenderPassOrder.Arbitrary pass) t
                        ]
            let bb = 
                match sgs with
                    | h :: _ -> h?GlobalBoundingBox()
                    | _ -> Mod.constant Box3d.Unit
            let res = manyAcc RenderPass.main sgs
            res?GlobalBoundingBox <- bb
            res


        Presentation.ofSlides [

            Slide.slide [] (fun m ->
                [
                    h1 [] [ text "FShade" ]
                    h3 [] [ text "why function composition matters" ]
                    text "Georg Haaser"
                ]
            )
            
           
            Slide.slide [] (fun m ->
                [
                    h2 [ ] [ text "Why FShade?" ]                      
                    ul [] [
                        li [] [ text "combinatorial explosion" ]
                        li [] [ text "programmatic manipulation" ]
                        li [] [ text "multiple target languages" ]
                    ]
                    img [ style "height: 30%"; attribute "src" "http://www.fshade.org/images/ShadowMapCaster.png"]
                ]
            )

            Slide.slide [] (fun m ->
                [
                    h2 [] [ text "What are shaders?" ]
                    img [ style "border: none; box-shadow: none"; attribute "src" "http://www.fshade.org/images/pipeline.png" ]
                    ul [] [
                        li [] [ text "pure multi-valued-functions" ]
                        li [] [ text "partial programs for hardware stages" ]
                        li [] [ 
                            text "inputs"
                            ul [] [
                                li [] [ text "frequency: attributes/uniforms" ]
                                li [] [ text "kind: vertex/primitive/fragment" ]
                            ]
                        ]
                    ]   
                ]
            )
            
            Slide.slide [] (fun m ->
                [
                    h2 [] [ text "Shader Modules" ]
                    img [ style "border: none; box-shadow: none; background: transparent"; attribute "src" "http://fshade.org/images/shader2.png" ]
                ]
            )

            Slide.slide [] (fun m ->
                [
                    h2 [] [ text "Composition" ]
                    img [ style "border: none; box-shadow: none; background: transparent; max-height: 60%"; attribute "src" "http://fshade.org/images/shader3.png" ]
                ]
            )

            Slide.slide [ attribute "data-transition" "slide-in none-out"; attribute "data-transition-speed" "fast" ] (fun m ->
                [
                    img [ style "border: none; box-shadow: none; background: transparent"; attribute "src" "http://fshade.org/images/comp1.svg" ]
                ]
            )
            Slide.slide [ attribute "data-transition" "none-in none-out"; attribute "data-transition-speed" "fast" ] (fun m ->
                [
                    img [ style "border: none; box-shadow: none; background: transparent"; attribute "src" "http://fshade.org/images/comp2.svg" ]
                ]
            )

            Slide.slide [ attribute "data-transition" "none-in slide-out"; attribute "data-transition-speed" "fast" ] (fun m ->
                [
                    img [ style "border: none; box-shadow: none; background: transparent"; attribute "src" "http://fshade.org/images/comp3.svg" ]
                ]
            )
            
            Slide.slide [] (fun m ->
                [
                    h2 [ ] [ text "Implementation" ]
                    ul [] [
                        li [] [ text "F# quotations provide typed syntax tree (TAST)" ]
                        li [] [ text "embedded in Aardvark" ]
                        li [] [ 
                            text "Extends to other languages" 
                            ul [] [
                                li [] [ text "Template Haskell" ] 
                                li [] [ text "Clojure" ]
                                li [] [ text "C++ via templates (if you're brave enough)" ]
                            ]
                        ]
                        li [] [ text "standalone parser" ]
                    ]
                ]
            )
            

            Slide.slide [] (fun m ->
                let light =
                    String.concat "\r\n" [
                        "type Vertex ="
                        "    {"
                        "        [<Position>]      pos : V4d"
                        "        [<Normal>]        n : V3d"
                        "        [<LightDir>]      l : V3d"
                        "        [<CamDir>]        c : V3d"
                        "        [<Color>]         color : V4d"
                        "        [<SpecularColor>] spec : V4d"
                        "    }"
                        ""
                        "let lighting (v : Vertex) ="
                        "    fragment {"
                        "        let n = Vec.normalize v.n"
                        "        let l = Vec.normalize v.l"
                        "        let c = Vec.normalize v.c"
                        "        let diffuse = Vec.dot n l |> clamp 0.0 1.0"
                        "        let spec = Vec.dot (Vec.reflect l n) (-c) |> clamp 0.0 1.0"
                        "        let specc = v.spec.XYZ"
                        "        return v.color.XYZ * diffuse + specc * pow spec uniform.Shininess"
                        "    }"
                    ]

                [
                    h2 [] [ text "Lighting" ]
                    code "fsharp" ~~light
                ]
            )

            Slide.slide [] (fun m ->
                let trafo =
                    String.concat "\r\n" [
                        "let transform (v : Vertex) ="
                        "    vertex {"
                        "        let light = uniform.LightLocation"
                        "        let wp = uniform.ModelMatrix * v.pos.XYZ"
                        ""
                        "        return {"
                        "            pos = uniform.ModelViewProjMatrix * v.pos"
                        "            n = uniform.ModelViewMatrixInv * v.n"
                        "            b = uniform.ModelViewMatrix * v.b"
                        "            t = uniform.ModelViewMatrix * v.t"
                        "            tc = v.tc"
                        "            l = uniform.ViewMatrix * (light - wp)"
                        "            c = -uniform.ViewMatrix * wp"
                        "            color = uniform.DiffuseColor"
                        "            spec = uniform.SpecularColor"
                        "        }"
                        ""
                        "    }"
                    ]

                [
                    h2 [] [ text "Vertex Transformation" ]
                    code "fsharp" ~~trafo
                ]
            )
            
            Slide.slide [] (fun m ->
                let app = EigiApp.app
                let mapIn (_model : EigiModel) (msg : SlideMessage) =
                    match msg with
                        | SlideMessage.TimePassed(n,d) -> Seq.singleton (EigiApp.Message.TimePassed(n,d))
                        | _ -> Seq.empty

                [
                    subApp' (fun _ _ -> Seq.empty<SlideMessage>) mapIn [style "width: 100%; height: 100%"] (
                        { app with
                            threads = fun t -> ThreadPool.remove "time" (app.threads t)
                        }
                    )
                ]
            )

            
            Slide.slide [] (fun m ->
                [
                    img [ style "border: none; box-shadow: none; background: transparent"; attribute "src" "http://fshade.org/images/geom.svg" ]
                ]
            )

            Slide.slide [] (fun m ->
                [
                    show {
                        att (style "width: 100%; height: 100%")
                        rotDelay 100000000.0
                        distance 3.0
                        phi Constant.PiHalf
                        theta 0.0
                        scene GS.sg
                    }
                ]
            )

            Slide.slide [] (fun m ->
                [
                    h2 [] [ text "Advanced Techniques" ] 
                    
                    ul [] [
                        li [] [ text "instancing" ]
                        li [] [ text "single pass stereo" ]
                        li [] [ text "platform adjustments (depth range, etc.)" ]
                        li [] [ text "specialization" ]
                        li [] [ text "unification" ]
                        li [] [ text "many more..." ]

                    ]

                ]
            )
            Slide.slide [] (fun m ->
                [
                    h2 [] [ text "Limitations" ] 
                    
                    ul [] [
                        li [] [ text "lambda functions" ]
                        li [] [ text "recursive functions and types" ]
                        li [] [ text "dynamic allocation" ]
                        li [] [ text "OOP constructs (references, subtyping, etc.)" ]
                    ]

                ]
            )
            Slide.slide [] (fun m ->
                [
                    h2 [] [ text "Questions?" ] 
                    show {
                        att (style "width: 100%; height: 60%")
                        scene (
                            Eigi.sg ~~Eigi.Animation.walk m.time
                                |> Sg.shader {
                                    do! Eigi.Skinning.skinning
                                    do! Eigi.Shader.transform

                                    do! Eigi.Shader.diffuseTexture
                                    do! Eigi.Shader.alphaTest
                                    do! Eigi.Shader.specularTexture
                                    do! Eigi.Shader.normalMapping
                                    do! Eigi.Shader.lighting
                                }
                        )
                    }
                    div [ style "font-size: 16pt" ] [
                        a [ attribute "target" "blank"; attribute "href"  "http://www.github.com/krauthaufen/FShade"] [ text "github.com/krauthaufen/FShade" ]
                        br []
                        a [ attribute "target" "blank"; attribute "href"  "http://www.fshade.org"] [ text "fshade.org" ]
                    ]

                    div [ style "font-size: 16pt" ] [
                        text "Thanks to Manuel Wieser for the Eigi Model,"
                        a [ attribute "target" "blank"; attribute "href" "http://www.manuelwieser.com/" ] [ text "www.manuelwieser.com" ]
                    ]

                ]
            )
        ]