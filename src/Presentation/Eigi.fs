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

        let boot (v : Vertex) =
            vertex {
                let light = uniform.LightLocation
                return {
                    v with
                        l = (light - v.pos.XYZ)
                        c = -v.pos.XYZ
                        color = uniform.DiffuseColor
                        spec = uniform.SpecularColor
                    }
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

    let sg (anim : IMod<Range1d>) (time : IMod<MicroTime>)= 
        scene 
            |> Sg.adapter
            |> Sg.uniform "Bones" ~~Animation.allBones
            |> Sg.uniform "NumFrames" ~~Animation.numFrames
            |> Sg.uniform "NumBones" ~~Animation.numBones
            |> Sg.uniform "Framerate" ~~Animation.fps
            |> Sg.uniform "FrameRange" (anim |> Mod.map (fun anim -> V2d(anim.Min, anim.Max)))
            |> Sg.uniform "Time" (time |> Mod.map (fun m -> m.TotalSeconds))
            |> Sg.uniform "TimeOffset" ~~0.0
            |> Sg.transform (Trafo3d.FromBasis(V3d.IOO, V3d.OOI, V3d.OIO, V3d.Zero) * Trafo3d.Scale 20.0)
            // apply all shaders we have
                
module EigiApp =  
    type Message =
        | ToggleTransform
        | ToggleSkinning
        | ToggleTexture
        | ToggleAlphaTest
        | ToggleSpecular
        | ToggleNormalMapping
        | ToggleLighting
        | TimePassed of now : MicroTime * delta : MicroTime
        | SetAnimation of range : Range1d

    let initial =
        {
            time            = MicroTime.Zero
            transform       = true
            skinning        = false
            diffuseTexture  = false
            alphaTest       = false
            specularTexture = false
            normalMapping   = false
            lighting        = true
            animation       = Eigi.Animation.none
        }

    let update (model : EigiModel) (msg : Message) =
        match msg with
            | TimePassed(n,d) ->
                { model with time = n }

            | ToggleTransform ->
                { model with transform = not model.transform }

            | ToggleSkinning ->
                { model with skinning = not model.skinning }
                
            | ToggleTexture ->
                { model with diffuseTexture = not model.diffuseTexture }

            | ToggleAlphaTest ->
                { model with alphaTest = not model.alphaTest }

            | ToggleSpecular ->
                { model with specularTexture = not model.specularTexture }

            | ToggleNormalMapping ->
                { model with normalMapping = not model.normalMapping }

            | ToggleLighting ->
                { model with lighting = not model.lighting }

            | SetAnimation a ->
                { model with animation = a }

    let dropDown<'a, 'msg> (att : list<string * AttributeValue<'msg>>) (current : IMod<'a>) (update : 'a -> 'msg) (names : hmap<'a, string>) : DomNode<'msg> =
        
        let mutable back = HMap.empty
        let forth = 
            names |> HMap.map (fun a s -> 
                let id = newId()
                back <- HMap.add id a back
                id
            )
        
        let selectedValue = current |> Mod.map (fun c -> HMap.find c forth)
        
        let boot = 
            String.concat "\r\n" [
                sprintf "$('#__ID__').dropdown().dropdown('set selected', %d);" (Mod.force selectedValue)
                "current.onmessage = function(v) { $('#__ID__').dropdown('set selected', v); };"
            ]
            
        select ((onChange (fun str -> HMap.find (str |> int) back |> update))::att) [
            for (value, name) in HMap.toSeq names do
                let v = HMap.find value forth
                yield option [attribute "value" (string v)] [ text name ]
        ]

    open FShade
    open Aardvark.Base.Ag

    let view (m : MEigiModel) =
        
        let mapOut _ m = Seq.empty
        let mapIn _ (m : Message) =
            match m with
                | Message.TimePassed(n,d) -> Seq.singleton (Orbit.Message.TimePassed(n,d))
                | _ -> Seq.empty
                
        let current, shader =
            let eBoot       = FShade.Effect.ofFunction Eigi.Shader.boot
            let eSkinning   = FShade.Effect.ofFunction Eigi.Skinning.skinning
            let eTransform  = FShade.Effect.ofFunction Eigi.Shader.transform
            let eTexture    = FShade.Effect.ofFunction Eigi.Shader.diffuseTexture
            let eAlphaTest  = FShade.Effect.ofFunction Eigi.Shader.alphaTest
            let eSpec       = FShade.Effect.ofFunction Eigi.Shader.specularTexture
            let eNormal     = FShade.Effect.ofFunction Eigi.Shader.normalMapping
            let eLight      = FShade.Effect.ofFunction Eigi.Shader.lighting

            let worstShaders =
                [
                    FShade.Effect.compose [
                        eBoot
                        eSkinning
                        eTransform
                        eTexture
                        eAlphaTest
                        eSpec
                        eNormal
                        eLight
                    ]

                    FShade.Effect.compose [
                        eBoot
                    ]
                    FShade.Effect.compose [
                        eBoot
                        eLight
                    ]
                ]
                    
            let cache = Dict<FShade.Effect,_>()
            let current =

                let cache = Dict<list<bool>, FShade.Effect>()
                
                let rec all (n : int) =
                    if n = 0 then [[]]
                    elif n < 0 then []
                    else
                        let rest = all (n - 1)

                        (rest |> List.map (fun l -> true :: l)) @
                        (rest |> List.map (fun l -> false :: l))
                     
                let getShader (key : list<bool>) =
                    cache.GetOrCreate(key, fun key -> 
                        match key with
                        | [ transform; skinning; diffuseTexture; alphaTest; specularTexture; normalMapping; lighting ] ->
                            FShade.Effect.compose [
                                yield eBoot
                                if skinning then yield eSkinning
                                if transform then yield eTransform
                                if diffuseTexture then yield eTexture
                                if alphaTest then yield eAlphaTest
                                if specularTexture then yield eSpec
                                if normalMapping then yield eNormal
                                if lighting then yield eLight
                            ]
                        | _ ->
                            FShade.Effect.empty
                    )
                    
                for v in all 7 do getShader v |> ignore


                Mod.custom (fun t ->
                    let transform = m.transform.GetValue t
                    let skinning = m.skinning.GetValue t
                    let diffuseTexture = m.diffuseTexture.GetValue t
                    let alphaTest = m.alphaTest.GetValue t
                    let specularTexture = m.specularTexture.GetValue t
                    let normalMapping = m.normalMapping.GetValue t
                    let lighting = m.lighting.GetValue t
                    let key = [ transform; skinning; diffuseTexture; alphaTest; specularTexture; normalMapping; lighting ]
                    getShader key
                )
               
            let surface = 
                Surface.FShade (fun cfg ->
                    let layout = FShade.EffectInputLayout.ofModules (worstShaders |> List.map (FShade.Effect.toModule cfg))

                    let activeModule = 
                        current |> Mod.map (fun effect ->
                            cache.GetOrCreate(effect, fun effect ->
                                let m = FShade.Effect.toModule cfg effect
                                FShade.EffectInputLayout.apply layout m
                            )
                        )


                    layout, activeModule
                )

            let wrap (s : ISg<_>) =
                Aardvark.SceneGraph.Sg.SurfaceApplicator(surface, s) :> Aardvark.SceneGraph.ISg
                    |> Sg.noEvents
            current, wrap
  

        let config =
            {
                depthRange = Range1d(-1.0, 1.0)
                lastStage = FShade.ShaderStage.Fragment
                flipHandedness = false
                outputs = Map.ofList [ string DefaultSemantic.Colors, (typeof<V4d>, 0) ]
            }

        let glslCode = 
            let rx = System.Text.RegularExpressions.Regex @"[\r\n]+"
            current |> Mod.map (fun e ->
                let glsl = 
                    e 
                    |> FShade.Effect.toModule config
                    |> ModuleCompiler.compileGLSL430
                rx.Replace(glsl.code, "\r\n")
            )


        let animations =
            HMap.ofList [
                Eigi.Animation.none,    "none"
                Eigi.Animation.idle,    "idle"
                Eigi.Animation.walk,    "walk"
                Eigi.Animation.attack,  "attack"
                Eigi.Animation.die,     "die"
            ]
         

        let time =
            m.animation |> Mod.bind (fun a ->
                if a = Eigi.Animation.none then ~~MicroTime.Zero
                else m.time
            )

        let scene =
            Eigi.sg m.animation time
                |> shader

        let box : IMod<Box3d> = scene?GlobalBoundingBox()
        let b = box.GetValue()

        let r = b.Size.Length
        let phi = -45.0 * Constant.RadiansPerDegree
        let theta = 30.0 * Constant.RadiansPerDegree
            
        let app = Orbit.app' b.Center phi theta r scene
        
        let att = [style "width: 100%; height: 100%"]
        let toggle (value : IMod<bool>) (toggle : Message) =
            div [] [
                div [ clazz "ui toggle checkbox" ] [
                    Incremental.input (
                        AttributeMap.ofListCond [ 
                            always <| attribute "type" "checkbox" 
                            onlyWhen value <| attribute "checked" "checked"
                            always <| onChange (fun _ -> toggle)
                        ]
                    )
                    label [] []
                ]
            ]

        let table (elements : list<list<DomNode<Message>>>) =
            elements |> List.map (fun row -> row |> List.map (List.singleton >> td []) |> tr []) |> table [style "font-size: 20pt"]
  
        div [style "width: 100%; height: 100%" ] [

            yield subApp' mapOut mapIn att { app with threads = fun m -> app.threads m |> ThreadPool.remove "time" }
            
            yield 
                div [ style "position: absolute; top: 40pt; left: 40pt" ] [
                    table [
                        [ text "transform";         toggle m.transform ToggleTransform              ]
                        [ text "skinning";          toggle m.skinning ToggleSkinning                ]
                        [ text "texture";           toggle m.diffuseTexture ToggleTexture           ] 
                        [ text "alpha-test";        toggle m.alphaTest ToggleAlphaTest              ]
                        [ text "specular";          toggle m.specularTexture ToggleSpecular         ]
                        [ text "normal";            toggle m.normalMapping ToggleNormalMapping      ]
                        [ text "lighting";          toggle m.lighting ToggleLighting                ]
                        [ text "animation";         dropDown [] m.animation SetAnimation animations ]
                    ]
                ]


            let boot =
                String.concat "\r\n" [
                    "code.onmessage = function(c) { document.getElementById('__ID__').innerHTML = hljs.highlight('glsl', c, true).value };"
                ]

            yield
                div [ style "position: absolute; top: 20pt; right: 10pt; bottom: 40pt; font-size: 13pt; text-align: left; width: 32%" ] [
                    onBoot' ["code", Mod.channel glslCode ] boot (
                        DomNode<_>("pre", None, AttributeMap.ofList [style "height: 95%; background: rgba(34,34,34,0.7)"; clazz "hljs glsl"] , DomContent.Empty)
                    )
                ]


        ]
   
    let app =
        {
            initial = initial
            update = update
            threads = fun _ -> ThreadPool.empty
            view = view
            unpersist = Unpersist.instance
        }

