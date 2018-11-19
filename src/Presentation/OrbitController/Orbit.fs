namespace Aardvark.UI

open Aardvark.Base
open Aardvark.Base.Rendering
open Aardvark.Base.Incremental
open Aardvark.Base.Incremental.Operators
open Aardvark.Application

module Orbit =
    
    type Message =
        | TimePassed of now : MicroTime * delta : MicroTime
        | Down of b : MouseButtons * pos : V2i
        | Up of b : MouseButtons * pos : V2i
        | Move of newPos : V2i
        | Scroll of delta : float
        | SetCenter of pos : V3d


    let private positivePhi (phi : float) =
        let phi = phi % Constant.PiTimesTwo
        if phi < 0.0 then
            Constant.PiTimesTwo + phi
        else
            phi

    let private withCamera (m : OrbitModel) =
        let m = { m with theta = clamp m.config.thetaRange.Min m.config.thetaRange.Max m.theta }
        let m = { m with radius = clamp m.config.radiusRange.Min m.config.radiusRange.Max m.radius }
        let m = { m with phi = positivePhi m.phi }

        let pos = V3d(cos m.theta * cos m.phi, cos m.theta * sin m.phi, sin m.theta) * m.radius
        { m with 
            camera =
                { m.camera with
                    cameraView = CameraView.lookAt (m.center + pos) m.center V3d.OOI
                }
        }

    let initial = 
        withCamera {
            config =
                {
                    radiusRange = Range1d(0.2, 100.0)
                    thetaRange  = Range1d(-Constant.PiHalf + 5.0 * Constant.RadiansPerDegree, Constant.PiHalf - 5.0 * Constant.RadiansPerDegree)
                    decay = 0.004
                    
                    autoRotateSpeed     = V2d(0.2, 0.0)
                    autoRotateDelay     = MicroTime(System.TimeSpan.FromSeconds(2.0))

                    orbitSensitivity    = 1.0
                    zoomSensitivity     = 1.0
                    scrollSensitivity   = 1.0

                }
            time        = MicroTime.Zero
            lastAction  = MicroTime.Zero
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

    let private sw = lazy (System.Diagnostics.Stopwatch.StartNew())
    //let private time =
    //    let sw = System.Diagnostics.Stopwatch.StartNew()
    //    let rec proc(last : MicroTime) = 
    //        proclist {
    //            do! Async.Sleep 8
    //            let now = sw.MicroTime
    //            yield TimePassed(now, now - last)
    //            yield! proc(now)
    //        }
    //    proc(sw.MicroTime)

    //let threads (m : OrbitModel) =
    //    ThreadPool.empty |> ThreadPool.add "time" time

    let rec update (m : OrbitModel) (msg : Message) =
        match msg with
            | SetCenter c ->
                withCamera { m with center = c }

            | TimePassed(now, dt) ->
                let dt = dt.TotalSeconds |> clamp 0.0 (1.0 / 10.0)

                let m = 
                    if abs m.moveSpeed > 1E-2 then
                        withCamera
                            { m with
                                radius = m.radius + m.moveSpeed * dt
                                moveSpeed = m.moveSpeed * pow m.config.decay dt
                                time = now
                            }
                    else 
                        m
                if not m.rotating && (now - m.lastAction) > m.config.autoRotateDelay then
                    withCamera
                        { m with
                            phi = m.phi + m.config.autoRotateSpeed.X * dt
                            theta = m.theta + m.config.autoRotateSpeed.Y * dt
                            time = now
                        }
                else
                    m

            | Down (b,p) ->
                match b with
                    | MouseButtons.Left ->
                        { m with 
                            rotating = true
                            startRot = p 
                            lastAction = m.time
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
                        { m with rotating = false; lastAction = m.time }
                        
                    | MouseButtons.Right ->
                        { m with zooming = false }
                    | _ ->
                        m
            | Move np ->
                let m = 
                    if m.rotating then
                        let delta = np - m.startRot
                        let dPhi = m.config.orbitSensitivity * (float -delta.X / 100.0)
                        let dTheta = m.config.orbitSensitivity * (float delta.Y / 100.0)

                        if abs dPhi > 0.0 || abs dTheta > 0.0 then
                            withCamera
                                { m with 
                                    phi = (m.phi + dPhi) % Constant.PiTimesTwo
                                    theta = m.theta + dTheta
                                    startRot = np
                                    lastAction = m.time
                                }
                        else
                            m
                    else
                        m

                if m.zooming then
                    let delta = np - m.startZoom
                    let dR = m.config.zoomSensitivity * (float delta.Y / 100.0)
                    
                    withCamera
                        { m with 
                            radius = m.radius + dR
                            startZoom = np
                        }
                else
                    m

            | Scroll delta ->
                let delta = m.config.scrollSensitivity * (delta * -10.0)
                { m with 
                    moveSpeed = m.moveSpeed + delta 
                }

    let view (scene : ISg<Message>) (m : MOrbitModel) =
        let attributes =
            AttributeMap.ofListCond [
                always <| style "width: 100%; height: 100%; background: #222; min-width: 1px; min-height: 1px"
                
                always <| attribute "data-samples" "8"

                always <| onMouseDown (fun b p -> Down(b,p))
                always <| onMouseUp (fun b p -> Up(b,p))
                
                onlyWhen (m.rotating %|| m.zooming) <| onMouseMove (fun p -> Move p)

                always <| onWheel (fun delta -> Scroll delta.Y)
                
            ]
            
        DomNode.RenderControl(
            attributes,
            m.camera,
            scene,
            RenderControlConfig.standard,
            None
        )
        
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
            initial = withCamera { initial with phi = phi; theta = theta; radius = radius; center = center }
            update = update
            view = view scene
            threads = fun _ -> ThreadPool.empty
            unpersist = Unpersist.instance
        }


    let app =
        {
            initial = initial
            update = update
            view = viewBox
            threads = fun _ -> ThreadPool.empty
            unpersist = Unpersist.instance
        }


