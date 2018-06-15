namespace Aardvark.UI

open Aardvark.Base
open Aardvark.Base.Rendering
open Aardvark.Base.Incremental
open Aardvark.Base.Incremental.Operators
open Aardvark.Application

module Orbit =
    
    type Message =
        | TimePassed
        | Down of b : MouseButtons * pos : V2i
        | Up of b : MouseButtons * pos : V2i
        | Move of newPos : V2i
        | Scroll of delta : float

    let private withCamera (m : OrbitModel) =
        let m = { m with theta = clamp m.thetaRange.Min m.thetaRange.Max m.theta }
        let m = { m with radius = clamp m.radiusRange.Min m.radiusRange.Max m.radius }

        let pos = V3d(cos m.theta * cos m.phi, cos m.theta * sin m.phi, sin m.theta) * m.radius
        { m with 
            camera =
                { m.camera with
                    cameraView = CameraView.lookAt (m.center + pos) m.center V3d.OOI
                }
        }

    let initial = 
        withCamera {
            radiusRange = Range1d(0.2, 100.0)
            thetaRange  = Range1d(-Constant.PiHalf + 5.0 * Constant.RadiansPerDegree, Constant.PiHalf - 5.0 * Constant.RadiansPerDegree)
            time        = 0.0
            startRot    = V2i.Zero
            startZoom   = V2i.Zero
            center      = V3d.Half
            phi         = 0.0
            theta       = 0.0
            radius      = 1.0

            zooming     = false
            rotating    = false
            moveSpeed   = 0.0

            camera = 
                {
                    cameraView = Unchecked.defaultof<CameraView>
                    frustum = Frustum.perspective 60.0 0.1 100.0 1.0
                }

        }

    let sw = System.Diagnostics.Stopwatch.StartNew()

    let threads (m : OrbitModel) =
        if abs m.moveSpeed > 1E-2 then
            let rec proc() = 
                proclist {
                    yield TimePassed
                    do! Async.Sleep 16
                    yield! proc()
                }

            ThreadPool.empty
                |> ThreadPool.add "time" (proc())
        else
            ThreadPool.empty

    let update (m : OrbitModel) (msg : Message) =
        match msg with
            | TimePassed ->
                let now = sw.Elapsed.TotalSeconds
                let dt = now - m.time |> clamp 0.0 (1.0 / 10.0)

                withCamera
                    { m with
                        radius = m.radius + m.moveSpeed * dt
                        moveSpeed = m.moveSpeed * pow 0.004 dt
                        time = now
                    }
            | Down (b,p) ->
                match b with
                    | MouseButtons.Left ->
                        { m with 
                            rotating = true
                            startRot = p 
                        }
                    | MouseButtons.Right ->
                        { m with 
                            zooming = true
                            startZoom = p 
                        }
                    | _ ->
                        m
                        

            | Up (b,p)  ->
                match b with
                    | MouseButtons.Left ->
                        { m with rotating = false }
                        
                    | MouseButtons.Right ->
                        { m with zooming = false }
                    | _ ->
                        m
            | Move np ->
                let m = 
                    if m.rotating then
                        let delta = np - m.startRot
                        let dPhi = float -delta.X / 100.0
                        let dTheta = float delta.Y / 100.0

                        withCamera
                            { m with 
                                phi = (m.phi + dPhi) % Constant.PiTimesTwo
                                theta = m.theta + dTheta
                                startRot = np
                            }
                    else
                        m

                if m.zooming then
                    let delta = np - m.startZoom
                    let dR = float delta.Y / 100.0
                    
                    withCamera
                        { m with 
                            radius = m.radius + dR
                            startZoom = np
                        }
                else
                    m

            | Scroll delta ->
                let delta = delta * -20.0
                { m with 
                    moveSpeed = m.moveSpeed + delta 
                    time = sw.Elapsed.TotalSeconds
                }
                    
                //Log.warn "delta: %A" delta
                //m
                
    let view (scene : ISg<Message>) (m : MOrbitModel) =
        let attributes =
            AttributeMap.ofListCond [
                always <| style "width: 100%; height: 100%"

                always <| onMouseDown (fun b p -> Down(b,p))
                always <| onMouseUp (fun b p -> Up(b,p))
                
                onlyWhen (m.rotating %|| m.zooming) <| onMouseMove (fun p -> Move p)

                always <| onWheel (fun delta -> Scroll delta.Y)

            ]

        Incremental.renderControl m.camera attributes (scene)

    let viewBox =
        view (
            Sg.box (Mod.constant C4b.Green) (Mod.constant Box3d.Unit)
                |> Sg.shader {
                    do! DefaultSurfaces.trafo
                    do! DefaultSurfaces.vertexColor
                    do! DefaultSurfaces.simpleLighting
                }
        )


    let app' (center : V3d) (phi : float) (theta : float) (radius : float) (scene : ISg<Message>) =
        {
            initial = withCamera { initial with phi = phi; theta = theta; radius = radius }
            update = update
            view = view scene
            threads = threads
            unpersist = Unpersist.instance
        }


    let app =
        {
            initial = initial
            update = update
            view = viewBox
            threads = threads
            unpersist = Unpersist.instance
        }


