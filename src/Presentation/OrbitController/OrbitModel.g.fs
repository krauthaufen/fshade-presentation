//3b312e20-bb48-2d3f-3c4e-478f693e4249
//fc024482-c50a-2490-f381-95a037b41d09
#nowarn "49" // upper case patterns
#nowarn "66" // upcast is unncecessary
#nowarn "1337" // internal types
#nowarn "1182" // value is unused
namespace rec Aardvark.UI

open System
open FSharp.Data.Adaptive
open Adaptify
open Aardvark.UI
[<System.Diagnostics.CodeAnalysis.SuppressMessage("NameConventions", "*")>]
type AdaptiveOrbitModel(value : OrbitModel) =
    let _center_ = FSharp.Data.Adaptive.cval(value.center)
    let _phi_ = FSharp.Data.Adaptive.cval(value.phi)
    let _theta_ = FSharp.Data.Adaptive.cval(value.theta)
    let _radius_ = FSharp.Data.Adaptive.cval(value.radius)
    let _lastAction_ = FSharp.Data.Adaptive.cval(value.lastAction)
    let _startRot_ = FSharp.Data.Adaptive.cval(value.startRot)
    let _startZoom_ = FSharp.Data.Adaptive.cval(value.startZoom)
    let _rotating_ = FSharp.Data.Adaptive.cval(value.rotating)
    let _zooming_ = FSharp.Data.Adaptive.cval(value.zooming)
    let _moveSpeed_ = FSharp.Data.Adaptive.cval(value.moveSpeed)
    let _time_ = FSharp.Data.Adaptive.cval(value.time)
    let _camera_ = FSharp.Data.Adaptive.cval(value.camera)
    let mutable __value = value
    let __adaptive = FSharp.Data.Adaptive.AVal.custom((fun (token : FSharp.Data.Adaptive.AdaptiveToken) -> __value))
    static member Create(value : OrbitModel) = AdaptiveOrbitModel(value)
    static member Unpersist = Adaptify.Unpersist.create (fun (value : OrbitModel) -> AdaptiveOrbitModel(value)) (fun (adaptive : AdaptiveOrbitModel) (value : OrbitModel) -> adaptive.Update(value))
    member __.Update(value : OrbitModel) =
        if Microsoft.FSharp.Core.Operators.not((FSharp.Data.Adaptive.ShallowEqualityComparer<OrbitModel>.ShallowEquals(value, __value))) then
            __value <- value
            __adaptive.MarkOutdated()
            _center_.Value <- value.center
            _phi_.Value <- value.phi
            _theta_.Value <- value.theta
            _radius_.Value <- value.radius
            _lastAction_.Value <- value.lastAction
            _startRot_.Value <- value.startRot
            _startZoom_.Value <- value.startZoom
            _rotating_.Value <- value.rotating
            _zooming_.Value <- value.zooming
            _moveSpeed_.Value <- value.moveSpeed
            _time_.Value <- value.time
            _camera_.Value <- value.camera
    member __.Current = __adaptive
    member __.center = _center_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.V3d>
    member __.phi = _phi_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.float>
    member __.theta = _theta_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.float>
    member __.radius = _radius_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.float>
    member __.config = __value.config
    member __.lastAction = _lastAction_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.MicroTime>
    member __.startRot = _startRot_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.V2i>
    member __.startZoom = _startZoom_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.V2i>
    member __.rotating = _rotating_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.zooming = _zooming_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.moveSpeed = _moveSpeed_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.float>
    member __.time = _time_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.MicroTime>
    member __.camera = _camera_ :> FSharp.Data.Adaptive.aval<Aardvark.Rendering.Camera>

