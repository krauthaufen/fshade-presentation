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
module ``FShade Extensions`` =
    open FShade

    type LightDirAttribute() = inherit FShade.SemanticAttribute("LightDirection")
    type CamDirAttribute() = inherit FShade.SemanticAttribute("CameraDirection")
    type SpecularColorAttribute() = inherit FShade.SemanticAttribute("SpecularColor")

    type UniformScope with
        member x.AmbientColor : V4d = x?Material?AmbientColor
        member x.DiffuseColor : V4d = x?Material?DiffuseColor
        member x.EmissiveColor : V4d = x?Material?EmissiveColor
        member x.ReflectiveColor : V4d = x?Material?ReflectiveColor
        member x.SpecularColor : V4d = x?Material?SpecularColor
        member x.Shininess : float = x?Material?Shininess
        member x.BumpScale : float = x?Material?BumpScale



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
 

    module Eigi =
        open Aardvark.Base.Incremental.Operators
    // defines all shaders used
        module Shader =
            open FShade

            type Vertex =
                {
                    [<Position>]    pos : V4d
                    [<Normal>]      n : V3d
                    [<BiNormal>]    b : V3d
                    [<Tangent>]     t : V3d
                    [<TexCoord>]    tc : V2d

                    [<LightDir>]    l : V3d
                    [<CamDir>]      c : V3d
                    [<Color>]       color : V4d
                    [<SpecularColor>] spec : V4d
                    [<SamplePosition>] sp : V2d
                }

            // define some samplers
            let diffuseColor =
                sampler2d {
                    texture uniform?DiffuseColorTexture
                    filter Filter.Anisotropic
                    addressU WrapMode.Wrap
                    addressV WrapMode.Wrap
                }

            let specularColor =
                sampler2d {
                    texture uniform?SpecularColorTexture
                    filter Filter.Anisotropic
                    addressU WrapMode.Wrap
                    addressV WrapMode.Wrap
                }

            let normalMap =
                sampler2d {
                    texture uniform?NormalMapTexture
                    filter Filter.Anisotropic
                    addressU WrapMode.Wrap
                    addressV WrapMode.Wrap
                }


            // transform a vertex with all its attributes
            let transform (v : Vertex) =
                vertex {
                    let light = uniform.LightLocation
                    let wp = uniform.ModelTrafo.TransformPos(v.pos.XYZ)

                    return {
                        pos = uniform.ModelViewProjTrafo * v.pos
                        n = uniform.ModelViewTrafoInv.TransposedTransformDir v.n
                        b = uniform.ModelViewTrafo.TransformDir v.b
                        t = uniform.ModelViewTrafo.TransformDir v.t
                        tc = v.tc
                        sp = v.sp

                        l = uniform.ViewTrafo.TransformDir(light - wp)
                        c = -uniform.ViewTrafo.TransformPos(wp)
                        color = uniform.DiffuseColor
                        spec = uniform.SpecularColor
                    }

                }
        
            // change the per-fragment normal according to the NormalMap
            let normalMapping (v : Vertex) =
                fragment {
                    let vn = normalMap.Sample(v.tc).XYZ
                    let tn = vn * 2.0 - V3d.III |> Vec.normalize
                    
                    let n = Vec.normalize v.n
                    let b = Vec.normalize v.b
                    let t = Vec.normalize v.t

                    return { v with n = b * tn.X + t * tn.Y +  n * tn.Z }
                }

            // change the per-fragment color using the DiffuseTexture
            let diffuseTexture (v : Vertex) =
                fragment {
                    return diffuseColor.Sample(v.tc)
                }

            // change the per-fragment specularColor using the SpecularMap
            let specularTexture (v : Vertex) =
                fragment {
                    return { v with spec = specularColor.Sample(v.tc) }
                }

            // apply per-fragment lighting
            let lighting (v : Vertex) =
                fragment {
                    let n = Vec.normalize v.n
                    let l = Vec.normalize v.l
                    let c = Vec.normalize v.c

                    let diffuse     = Vec.dot n l |> clamp 0.0 1.0
                    let spec        = Vec.dot (Vec.reflect l n) (-c) |> clamp 0.0 1.0

                    let diff    = v.color
                    let specc   = v.spec.XYZ
                    let shine   = uniform.Shininess


                    let color = diff.XYZ * diffuse  +  specc * pow spec shine

                    return V4d(color, v.color.W)
                }
        
            // per-fragment alpha test using the current color
            let alphaTest (v : Vertex) =
                fragment {
                    let dummy = v.sp.X * 0.0000001
                    if v.color.W < 0.05 + dummy then
                        discard()

                    return v
                }

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
            let none    = Range1d(50.0, 51.0)
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

        let sg (anim : Range1d) (time : IMod<MicroTime>)= 
            scene 
                |> Sg.adapter
                |> Sg.uniform "Bones" ~~Animation.allBones
                |> Sg.uniform "NumFrames" ~~Animation.numFrames
                |> Sg.uniform "NumBones" ~~Animation.numBones
                |> Sg.uniform "Framerate" ~~Animation.fps
                |> Sg.uniform "FrameRange" ~~(V2d(anim.Min, anim.Max))
                |> Sg.uniform "Time" (time |> Mod.map (fun m -> m.TotalSeconds))
                |> Sg.uniform "TimeOffset" ~~0.0
                |> Sg.transform (Trafo3d.FromBasis(V3d.IOO, V3d.OOI, V3d.OIO, V3d.Zero) * Trafo3d.Scale 20.0)
                // apply all shaders we have
                

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
                    h3 [] [ text "a functional approach to shaders" ]
                    text "Georg Haaser"
                ]
            )

            Slide.slide [] (fun m ->
                [
                    div [clazz "simple"] [
                        h1 [ clazz "header" ] [ text "What are shaders?" ]
                        div [ clazz "content" ] [
                            ul [] [
                                li [] [ text "total functions" ]
                                li [] [ text "multiple in-/outputs" ]
                                li [] [ text "partial programs for hardware stages" ]
                                li [] [ 
                                    text "two "
                                    u [] [text "semantic"]
                                    text " domains"
                                    ul [] [
                                        li [] [ text "primitives / vertices" ]
                                        li [] [ text "fragments" ]
                                    ] 
                                ]
                            ]
                        ]
                    ]
                ]
            )

            Slide.slide [] (fun m ->
                [
                    div [clazz "simple"] [
                        h1 [ clazz "header" ] [ text "Why another shader language?" ]
                        div [ clazz "content" ] [
                            div [ clazz "horizontal" ] [                          
                                ul [] [
                                    li [] [ text "GLSL/HLSL lack proper abstraction" ]
                                    li [] [ text "first class shader representation" ]
                                    li [] [ text "multiple target languages" ]
                                    li [] [ text "dynamic specialization" ]
                                ]
                                img [ style "margin-left: 50pt"; attribute "src" "http://www.fshade.org/images/ShadowMapCaster.png"]

                                

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

            Slide.slide [] (fun m ->
                [
                    show {
                        att (style "width: 85%; height: 70%")
                        scene (
                            Eigi.sg Eigi.Animation.none m.time                                
                                |> Sg.shader {
                                    do! Eigi.Skinning.skinning
                                    do! DefaultSurfaces.trafo
                                    do! DefaultSurfaces.constantColor C4f.Gray80
                                    do! DefaultSurfaces.simpleLighting
                                }                       
                        )
                    }
                ]
            )


            Slide.slide [] (fun m ->
                [
                    show {
                        att (style "width: 85%; height: 70%; background: #FFFFFF")
                        scene (
                            Eigi.sg Eigi.Animation.none m.time
                            |> Sg.shader {
                                do! Eigi.Skinning.skinning
                                do! Eigi.Shader.transform
                                //do! Eigi.Shader.diffuseTexture
                                //do! Eigi.Shader.alphaTest

                                do! Eigi.Shader.specularTexture
                                do! Eigi.Shader.normalMapping
                                do! Eigi.Shader.lighting
                            }
                        )
                    }
                ]
            )

            Slide.slide [] (fun m ->
                [
                    show {
                        att (style "width: 85%; height: 70%; background: #FFFFFF")
                        scene (
                            Eigi.sg Eigi.Animation.idle m.time
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
                ]
            )

            Slide.slide [] (fun m ->
                [
                    show {
                        att (style "width: 85%; height: 70%; background: #FFFFFF")
                        scene (
                            Eigi.sg Eigi.Animation.walk m.time
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
                ]
            )

            Slide.nested
                (
                    Slide.slide [] (fun m ->
                        [
                            show {
                                att (style "width: 85%; height: 70%; background: #FFFFFF")
                                scene (
                                    Eigi.sg Eigi.Animation.none m.time
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