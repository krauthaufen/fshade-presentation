namespace Aardvark.UI

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI

[<AutoOpen>]
module Mutable =

    
    
    type MOrbitModel(__initial : Aardvark.UI.OrbitModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Aardvark.UI.OrbitModel> = Aardvark.Base.Incremental.EqModRef<Aardvark.UI.OrbitModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Aardvark.UI.OrbitModel>
        let _center = ResetMod.Create(__initial.center)
        let _phi = ResetMod.Create(__initial.phi)
        let _theta = ResetMod.Create(__initial.theta)
        let _radius = ResetMod.Create(__initial.radius)
        let _lastAction = ResetMod.Create(__initial.lastAction)
        let _startRot = ResetMod.Create(__initial.startRot)
        let _startZoom = ResetMod.Create(__initial.startZoom)
        let _rotating = ResetMod.Create(__initial.rotating)
        let _zooming = ResetMod.Create(__initial.zooming)
        let _moveSpeed = ResetMod.Create(__initial.moveSpeed)
        let _time = ResetMod.Create(__initial.time)
        let _camera = ResetMod.Create(__initial.camera)
        
        member x.center = _center :> IMod<_>
        member x.phi = _phi :> IMod<_>
        member x.theta = _theta :> IMod<_>
        member x.radius = _radius :> IMod<_>
        member x.config = __current.Value.config
        member x.lastAction = _lastAction :> IMod<_>
        member x.startRot = _startRot :> IMod<_>
        member x.startZoom = _startZoom :> IMod<_>
        member x.rotating = _rotating :> IMod<_>
        member x.zooming = _zooming :> IMod<_>
        member x.moveSpeed = _moveSpeed :> IMod<_>
        member x.time = _time :> IMod<_>
        member x.camera = _camera :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Aardvark.UI.OrbitModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_center,v.center)
                ResetMod.Update(_phi,v.phi)
                ResetMod.Update(_theta,v.theta)
                ResetMod.Update(_radius,v.radius)
                ResetMod.Update(_lastAction,v.lastAction)
                ResetMod.Update(_startRot,v.startRot)
                ResetMod.Update(_startZoom,v.startZoom)
                ResetMod.Update(_rotating,v.rotating)
                ResetMod.Update(_zooming,v.zooming)
                ResetMod.Update(_moveSpeed,v.moveSpeed)
                ResetMod.Update(_time,v.time)
                ResetMod.Update(_camera,v.camera)
                
        
        static member Create(__initial : Aardvark.UI.OrbitModel) : MOrbitModel = MOrbitModel(__initial)
        static member Update(m : MOrbitModel, v : Aardvark.UI.OrbitModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Aardvark.UI.OrbitModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module OrbitModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let center =
                { new Lens<Aardvark.UI.OrbitModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.center
                    override x.Set(r,v) = { r with center = v }
                    override x.Update(r,f) = { r with center = f r.center }
                }
            let phi =
                { new Lens<Aardvark.UI.OrbitModel, System.Double>() with
                    override x.Get(r) = r.phi
                    override x.Set(r,v) = { r with phi = v }
                    override x.Update(r,f) = { r with phi = f r.phi }
                }
            let theta =
                { new Lens<Aardvark.UI.OrbitModel, System.Double>() with
                    override x.Get(r) = r.theta
                    override x.Set(r,v) = { r with theta = v }
                    override x.Update(r,f) = { r with theta = f r.theta }
                }
            let radius =
                { new Lens<Aardvark.UI.OrbitModel, System.Double>() with
                    override x.Get(r) = r.radius
                    override x.Set(r,v) = { r with radius = v }
                    override x.Update(r,f) = { r with radius = f r.radius }
                }
            let config =
                { new Lens<Aardvark.UI.OrbitModel, Aardvark.UI.OrbitConfig>() with
                    override x.Get(r) = r.config
                    override x.Set(r,v) = { r with config = v }
                    override x.Update(r,f) = { r with config = f r.config }
                }
            let lastAction =
                { new Lens<Aardvark.UI.OrbitModel, Aardvark.Base.MicroTime>() with
                    override x.Get(r) = r.lastAction
                    override x.Set(r,v) = { r with lastAction = v }
                    override x.Update(r,f) = { r with lastAction = f r.lastAction }
                }
            let startRot =
                { new Lens<Aardvark.UI.OrbitModel, Aardvark.Base.V2i>() with
                    override x.Get(r) = r.startRot
                    override x.Set(r,v) = { r with startRot = v }
                    override x.Update(r,f) = { r with startRot = f r.startRot }
                }
            let startZoom =
                { new Lens<Aardvark.UI.OrbitModel, Aardvark.Base.V2i>() with
                    override x.Get(r) = r.startZoom
                    override x.Set(r,v) = { r with startZoom = v }
                    override x.Update(r,f) = { r with startZoom = f r.startZoom }
                }
            let rotating =
                { new Lens<Aardvark.UI.OrbitModel, System.Boolean>() with
                    override x.Get(r) = r.rotating
                    override x.Set(r,v) = { r with rotating = v }
                    override x.Update(r,f) = { r with rotating = f r.rotating }
                }
            let zooming =
                { new Lens<Aardvark.UI.OrbitModel, System.Boolean>() with
                    override x.Get(r) = r.zooming
                    override x.Set(r,v) = { r with zooming = v }
                    override x.Update(r,f) = { r with zooming = f r.zooming }
                }
            let moveSpeed =
                { new Lens<Aardvark.UI.OrbitModel, System.Double>() with
                    override x.Get(r) = r.moveSpeed
                    override x.Set(r,v) = { r with moveSpeed = v }
                    override x.Update(r,f) = { r with moveSpeed = f r.moveSpeed }
                }
            let time =
                { new Lens<Aardvark.UI.OrbitModel, Aardvark.Base.MicroTime>() with
                    override x.Get(r) = r.time
                    override x.Set(r,v) = { r with time = v }
                    override x.Update(r,f) = { r with time = f r.time }
                }
            let camera =
                { new Lens<Aardvark.UI.OrbitModel, Aardvark.Base.Camera>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
