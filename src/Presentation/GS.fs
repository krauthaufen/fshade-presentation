﻿namespace Presentation

open System
open Aardvark.Base
open Aardvark.Base.Ag
open FSharp.Data.Adaptive
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.Rendering
open Presentation.Model
open Aardvark.UI.Presentation
open Aardvark.SceneGraph.Assimp
open Aardvark.Rendering.Text


module GS =

    module Shader =
        open FShade

        type Vertex = { [<Position>] p : V4d; [<WorldPosition>] wp : V4d; [<Color>] c : V4d; [<Normal>] n : V3d; [<Semantic("LightDir")>] ldir : V3d; [<SourceVertexIndex>] id : int }

        let divide (v : Triangle<Vertex>) =
            triangle {
                
                let ccDiv = v.P0.c
                let nDiv = v.P0.n
                let cpDiv = (v.P0.wp + v.P1.wp + v.P2.wp) / 3.0
                //let cc = (v.P0.c + v.P1.c + v.P2.c) / 3.0


                // 0 1 c   c 1 2   c 2 0
                yield { v.P0 with n = nDiv }
                yield { v.P1 with n = nDiv }
                yield { p = uniform.ViewProjTrafo * cpDiv; wp = cpDiv; c = V4d.IIII; n = nDiv; ldir = V3d.Zero; id = 0}
                restartStrip()

                yield { p = uniform.ViewProjTrafo * cpDiv; wp = cpDiv; c = V4d.IIII; n = nDiv; ldir = V3d.Zero; id = 1}
                yield { v.P1 with n = nDiv }
                yield { v.P2 with n = nDiv }
                restartStrip()
                
                yield { p = uniform.ViewProjTrafo * cpDiv; wp = cpDiv; c = V4d.IIII; n = nDiv; ldir = V3d.Zero; id = 2}
                yield { v.P2 with n = nDiv }
                yield { v.P0 with n = nDiv }
                restartStrip()

                //yield { v.P2 with n = nDiv }
                //yield { v.P0 with n = nDiv }
            }

        let shrink (v : Triangle<Vertex>) =
            triangle {
            
                let cp = (v.P0.wp + v.P1.wp + v.P2.wp) / 3.0
                
                let r = 0.9

                let a = V4d(cp.XYZ * (1.0 - r) + v.P0.wp.XYZ * r, v.P0.wp.W)
                let b = V4d(cp.XYZ * (1.0 - r) + v.P1.wp.XYZ * r, v.P1.wp.W)
                let c = V4d(cp.XYZ * (1.0 - r) + v.P2.wp.XYZ * r, v.P2.wp.W)

                yield { v.P0 with p = uniform.ViewProjTrafo * a; wp = a } 
                yield { v.P1 with p = uniform.ViewProjTrafo * b; wp = b } 
                yield { v.P2 with p = uniform.ViewProjTrafo * c; wp = c } 

            }

        let nop (v : Triangle<Vertex>) =
            triangle {
                yield v.P0
                yield v.P1
                yield v.P2
            }

        let invert (v : Triangle<Vertex>) =
            triangle {
                
                let p01 = (v.P0.wp + v.P1.wp) / 2.0
                let p12 = (v.P1.wp + v.P2.wp) / 2.0
                let p20 = (v.P2.wp + v.P0.wp) / 2.0
                
                yield { v.P0 with p = uniform.ViewProjTrafo * p01; wp = p01 } 
                yield { v.P1 with p = uniform.ViewProjTrafo * p12; wp = p12 } 
                yield { v.P2 with p = uniform.ViewProjTrafo * p20; wp = p20 } 

            }
            
        let tricolor (v : Triangle<Vertex>) =
            triangle {
                yield { v.P0 with c = V4d.OIII }
                yield { v.P1 with c = V4d.IOII }
                yield { v.P2 with c = V4d.IIOI }
            }

        let tritri (v : Triangle<Vertex>) =
            triangle {
                
                let p01 = (v.P0.wp + v.P1.wp) / 2.0
                let p12 = (v.P1.wp + v.P2.wp) / 2.0
                let p20 = (v.P2.wp + v.P0.wp) / 2.0

                yield v.P0
                yield { v.P0 with p = uniform.ViewProjTrafo * p01; wp = p01 } 
                yield { v.P2 with p = uniform.ViewProjTrafo * p20; wp = p20 } 
                restartStrip()

                yield { v.P0 with p = uniform.ViewProjTrafo * p01; wp = p01 } 
                yield { v.P1 with p = uniform.ViewProjTrafo * p12; wp = p12 } 
                yield { v.P2 with p = uniform.ViewProjTrafo * p20; wp = p20 } 
                restartStrip()

                yield { v.P2 with p = uniform.ViewProjTrafo * p20; wp = p20 } 
                yield { v.P1 with p = uniform.ViewProjTrafo * p12; wp = p12 }
                yield v.P2
                restartStrip()

                yield { v.P0 with p = uniform.ViewProjTrafo * p01; wp = p01 } 
                yield v.P1
                yield { v.P1 with p = uniform.ViewProjTrafo * p12; wp = p12 } 
                restartStrip()

                let p01 = p01 + V4d.OOIO
                let p12 = p12 + V4d.OOIO
                let p20 = p20 + V4d.OOIO
                yield { v.P0 with p = uniform.ViewProjTrafo * p01; wp = p01 } 
                yield { v.P1 with p = uniform.ViewProjTrafo * p12; wp = p12 } 
                yield { v.P2 with p = uniform.ViewProjTrafo * p20; wp = p20 } 
                restartStrip()
            }


        [<ReflectedDefinition>]
        let computeNormal (p0 : V4d) (p1 : V4d) (p2 : V4d) =
            Vec.cross (p1.XYZ - p0.XYZ) (p2.XYZ - p0.XYZ) |> Vec.normalize

        let extrude (v : Triangle<Vertex>) =
            triangle {
                let p0Ext = v.P0.wp.XYZ
                let p1Ext = v.P1.wp.XYZ
                let p2Ext = v.P2.wp.XYZ

                let cExt = (p0Ext + p1Ext + p2Ext) / 3.0
                
                let nExt = Vec.cross (p1Ext - p0Ext) (p2Ext - p1Ext)
                let lnExt = Vec.length nExt
                let areaExt = 0.5 * lnExt
                let nExt = nExt / lnExt

                let phExt = cExt + nExt * areaExt


                let w0Ext = V4d(p0Ext, 1.0)
                let w1Ext = V4d(p1Ext, 1.0)
                let w2Ext = V4d(p2Ext, 1.0)
                let whExt = V4d(phExt, 1.0)


                yield { v.P0 with wp = w0Ext; p = uniform.ViewProjTrafo * w0Ext; n = nExt; ldir = V3d.Zero }
                yield { v.P1 with wp = w1Ext; p = uniform.ViewProjTrafo * w1Ext; n = nExt; ldir = V3d.Zero }
                yield { v.P2 with wp = w2Ext; p = uniform.ViewProjTrafo * w2Ext; n = nExt; ldir = V3d.Zero }
                restartStrip()

                let nExt = computeNormal w0Ext w1Ext whExt
                yield { wp = w0Ext; p = uniform.ViewProjTrafo * w0Ext; n = nExt; c = v.P0.c; ldir = V3d.Zero; id = 0 }
                yield { wp = w1Ext; p = uniform.ViewProjTrafo * w1Ext; n = nExt; c = v.P1.c; ldir = V3d.Zero; id = 0}
                yield { wp = whExt; p = uniform.ViewProjTrafo * whExt; n = nExt; c = V4d.IOII; ldir = V3d.Zero; id = 0 }
                restartStrip()

                let nExt = computeNormal w1Ext w2Ext whExt
                yield { wp = w1Ext; p = uniform.ViewProjTrafo * w1Ext; n = nExt; c = v.P1.c; ldir = V3d.Zero; id = 0 }
                yield { wp = w2Ext; p = uniform.ViewProjTrafo * w2Ext; n = nExt; c = v.P2.c; ldir = V3d.Zero; id = 0 }
                yield { wp = whExt; p = uniform.ViewProjTrafo * whExt; n = nExt; c = V4d.IOII; ldir = V3d.Zero; id = 0 }
                restartStrip()

                let nExt = computeNormal w2Ext w0Ext whExt
                yield { wp = w2Ext; p = uniform.ViewProjTrafo * w2Ext; n = nExt; c = v.P2.c; ldir = V3d.Zero; id = 0 }
                yield { wp = w0Ext; p = uniform.ViewProjTrafo * w0Ext; n = nExt; c = v.P0.c; ldir = V3d.Zero; id = 0 }
                yield { wp = whExt; p = uniform.ViewProjTrafo * whExt; n = nExt; c = V4d.IOII; ldir = V3d.Zero; id = 0 }
                restartStrip()


            }

        let withLightDir (v : Vertex) =
            vertex {
                return { v with ldir = (uniform.ViewTrafo * (V4d(50.0, 60.0, 70.0, 1.0) - v.wp)).XYZ |> Vec.normalize; n = (uniform.ViewTrafo * V4d(v.n, 0.0)).XYZ |> Vec.normalize }
            }


    let available = 
        [ 
            "div", toEffect Shader.divide
            "shr", toEffect Shader.shrink 
            "ext", toEffect Shader.extrude
//            "invert", toEffect Shader.invert
//                "tricolor", toEffect Shader.tricolor
        ]

    let rec allInsertions (v : 'a) (l : list<'a>) =
        match l with
            | [] -> [[v]]
            | h :: t ->
                List.concat [
                    [v :: h :: t]
                    allInsertions v t |> List.map (fun r -> h :: r)
                ]

    let rec all (l : list<'a>) =
        match l with
            | [] -> [[]]
            | h :: t ->
                let t = all t
                List.append
                    (t |> List.collect (fun l -> allInsertions h l))
                    (t)

    let combinations = 
        // [available] // for less
        all available  // for more
            |> List.sortBy List.length
            |> List.map (fun l ->
                let name = 
                    match l |> List.map fst with
                        | [] -> "empty"
                        | list -> list |> String.concat ", " |> sprintf "[ %s ]"
                let effect = l |> List.map snd |> FShade.Effect.compose
                name, effect
            )
            |> List.toArray
  
    let w = 1 + (ceil (sqrt (float combinations.Length)) |> int)
    let h = ceil (float combinations.Length / float w) |> int

    let font = Font("Consolas")

    let sg<'a> : ISg<'a> =
        let sg = 
            Sg.ofList [
                for j in 0 .. h - 1 do
                    for i in 0 .. w - 1 do
                        let id = i + w * j
                        if id < combinations.Length then
                            let (name, effect) = combinations.[id]
     
                            let label = 
                                Sg.text font C4b.White (AVal.constant name)
                                    //|> Sg.billboard
                                    |> Sg.noEvents
                                    |> Sg.transform (Trafo3d.FromBasis(V3d.IOO, V3d.OOI, V3d.OIO, V3d.Zero) * Trafo3d.Scale(0.2))
                                    |> Sg.translate 0.0 0.0 1.0

                            let inner = 
                                Sg.ofList [
                                    Sg.sphere' 3 C4b.Red 0.5
                                        |> Sg.noEvents
                                        |> Sg.translate 0.5 0.5 0.5
                                        |> Sg.uniform "Color" (AVal.constant V4d.IOOI)

                                    Sg.box' C4b.Blue Box3d.Unit
                                    |> Sg.noEvents
                                        |> Sg.uniform "Color" (AVal.constant V4d.IIOI)
                                        |> Sg.translate 1.6 0.0 0.0
                                ]   
                                |> Sg.scale 0.5

                                |> Sg.shader {
                                    do! DefaultSurfaces.trafo
                                    do! effect
                                    do! Shader.withLightDir
                                    do! DefaultSurfaces.stableHeadlight
                                }
                            yield 
                                Sg.ofList [
                                    inner
                                    label
                                ]
                                |> Sg.translate (2.0 * float i) 0.0 (1.5 * float j)

            ]
            |> Sg.scale 0.5
            |> Sg.transform (Trafo3d.FromBasis(-V3d.IOO,V3d.OIO, V3d.OOI, V3d.OOO))
            
            // maxX = 0
            // minX = -(2 * (w - 1) + 1.3)*0.5
            
            // minY = 0
            // maxY = 0.25
            
            // minZ = 0
            // maxZ = (1.5 * (h - 1) + 0.5)* 0.5

        sg?GlobalBoundingBox <- AVal.constant (Box3d(V3d(-(2.0 * float (w - 1) + 1.3)*0.5, 0.0, 0.0), V3d(0.0, 0.25, (1.5 * float (h - 1) + 0.5)* 0.5)))
        sg