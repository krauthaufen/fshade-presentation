namespace Presentation

open System
open Aardvark.Base
open Aardvark.Base.Ag
open Aardvark.Base.Incremental
open Aardvark.Base.Incremental.Operators
open Aardvark.SceneGraph
open Aardvark.Base.Rendering
open Presentation.Model
open Aardvark.UI.Presentation
open Aardvark.SceneGraph.IO
open Aardvark.Rendering.Text
open Aardvark.UI
open Aardvark.UI.Generic
open Aardvark.UI.Primitives


module StableShaders =
    open FShade


    type Vertex = {
        [<Position>]                pos     : V4d
        [<Normal>]                  n       : V3d
        [<BiNormal>]                b       : V3d
        [<Tangent>]                 t       : V3d
        [<Color>]                   c       : V4d
        [<Semantic("LightDir")>]    ldir    : V3d
    }

    let stableTrafo (v : Vertex) =
        vertex {
            let vp = uniform.ModelViewTrafo * v.pos

            return {
                pos = uniform.ProjTrafo * vp
                n = uniform.ModelViewTrafoInv.TransposedTransformDir v.n |> Vec.normalize
                b = uniform.ModelViewTrafo.TransformDir v.b |> Vec.normalize
                t = uniform.ModelViewTrafo.TransformDir v.t |> Vec.normalize
                c = v.c
                ldir = -vp.XYZ |> Vec.normalize
            }
        }

    let stableLighting (v : Vertex) =
        fragment {
            let n = v.n |> Vec.normalize
            let c = v.ldir |> Vec.normalize

            let ambient = 0.2
            let diffuse = Vec.dot c n |> abs

            let l = ambient + (1.0 - ambient) * diffuse

            return V4d(v.c.XYZ * diffuse, v.c.W)
        }

module SimpleShaders =
    open FShade

    type Vertex = 
        {
            [<Position>]        pos     : V4d
            [<WorldPosition>]   wp      : V4d
            [<Normal>]          n       : V3d
            [<BiNormal>]        b       : V3d
            [<Tangent>]         t       : V3d
            [<Color>]           c       : V4d
            [<TexCoord>]        tc      : V2d
        }
    
    let trafo (v : Vertex) =
        vertex {
            let wp = uniform.ModelTrafo * v.pos
            return {
                pos = uniform.ViewProjTrafo * wp
                wp = wp
                n = uniform.ModelTrafoInv.TransposedTransformDir v.n
                b = uniform.ModelTrafo.TransformDir v.b
                t = uniform.ModelTrafo.TransformDir v.t
                c = v.c
                tc = v.tc
            }
        }
    

    let simpleLighting (v : Vertex) =
        fragment {
            let n = v.n |> Vec.normalize
            let c = uniform.LightLocation - v.wp.XYZ |> Vec.normalize

            let ambient = 0.2
            let diffuse = Vec.dot c n |> abs

            let l = ambient + (1.0 - ambient) * diffuse

            return V4d(v.c.XYZ * diffuse, v.c.W)
        }

module StableTrafo =
    
    
    type Message =
        | SetOffset of float
        | TimePassed of MicroTime * MicroTime

    let initial = 
        { 
            offset = 1000.0
        }
    
    let update (m : StableTrafoModel) (msg : Message) =
        match msg with
            | SetOffset o -> 
                { m with 
                    offset = o 
                }
            | TimePassed _ ->
                m

    let view (m : MStableTrafoModel) =
        let box = Box3d.FromCenterAndSize(V3d.Zero, V3d.III)
        let scene =
            Sg.box' C4b.Red box
                |> Sg.noEvents
                |> Sg.trafo (m.offset |> Mod.map (fun o -> Trafo3d.Translation(1000.0 * o, 0.0, 0.0)))

        let sg =
            Sg.ofList [
                scene
                |> Sg.translate -1.0 0.0 0.0
                |> Sg.shader {
                    do! SimpleShaders.trafo
                    do! SimpleShaders.simpleLighting
                }
                
                scene
                |> Sg.translate 1.0 0.0 0.0
                |> Sg.shader {
                    do! StableShaders.stableTrafo
                    do! StableShaders.stableLighting
                }
            ]

        let orbitApp = 
            let app = Orbit.app' (V3d(1000000.0, 0.0, 0.0)) Constant.PiQuarter Constant.PiQuarter 2.0 sg

            let mapIn _ msg =
                match msg with 
                | SetOffset o -> Seq.singleton (Orbit.SetCenter (V3d(1000.0 * o, 0.0, 0.0))) 
                | TimePassed(n,d) -> Seq.singleton (Orbit.TimePassed(n,d))
                | _ -> Seq.empty

            subApp'
                (fun _ _ -> Seq.empty)
                mapIn
                []
                app

        div [ style "width: 100%; height: 100%" ] [
            h2 [] "Stable Transformation"

            
            orbitApp

            div [ style "position: fixed; left: 25px; bottom: 25px" ] [
                labeledFloatInput' "offset (thousands)" 0.0 10000.0 1.0 SetOffset m.offset AttributeMap.empty AttributeMap.empty
            ]

        ]
    let app =
        {
            initial = initial
            update = update
            view = view
            threads = fun _ -> ThreadPool.empty
            unpersist = Unpersist.instance
        }
