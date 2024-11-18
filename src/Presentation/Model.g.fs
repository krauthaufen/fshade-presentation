//d4c86b29-29e8-767c-f96c-fff510408de5
//edbe68b5-a96f-3b1e-b14c-19329fc01959
#nowarn "49" // upper case patterns
#nowarn "66" // upcast is unncecessary
#nowarn "1337" // internal types
#nowarn "1182" // value is unused
namespace rec Presentation.Model

open System
open FSharp.Data.Adaptive
open Adaptify
open Presentation.Model
[<System.Diagnostics.CodeAnalysis.SuppressMessage("NameConventions", "*")>]
type AdaptiveModel(value : Model) =
    let _currentModel_ = FSharp.Data.Adaptive.cval(value.currentModel)
    let _fill_ = FSharp.Data.Adaptive.cval(value.fill)
    let _cameraState_ = Aardvark.UI.Primitives.AdaptiveCameraControllerState(value.cameraState)
    let mutable __value = value
    let __adaptive = FSharp.Data.Adaptive.AVal.custom((fun (token : FSharp.Data.Adaptive.AdaptiveToken) -> __value))
    static member Create(value : Model) = AdaptiveModel(value)
    static member Unpersist = Adaptify.Unpersist.create (fun (value : Model) -> AdaptiveModel(value)) (fun (adaptive : AdaptiveModel) (value : Model) -> adaptive.Update(value))
    member __.Update(value : Model) =
        if Microsoft.FSharp.Core.Operators.not((FSharp.Data.Adaptive.ShallowEqualityComparer<Model>.ShallowEquals(value, __value))) then
            __value <- value
            __adaptive.MarkOutdated()
            _currentModel_.Value <- value.currentModel
            _fill_.Value <- value.fill
            _cameraState_.Update(value.cameraState)
    member __.Current = __adaptive
    member __.currentModel = _currentModel_ :> FSharp.Data.Adaptive.aval<Primitive>
    member __.fill = _fill_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.cameraState = _cameraState_
[<System.Diagnostics.CodeAnalysis.SuppressMessage("NameConventions", "*")>]
type AdaptiveEigiModel(value : EigiModel) =
    let _time_ = FSharp.Data.Adaptive.cval(value.time)
    let _transform_ = FSharp.Data.Adaptive.cval(value.transform)
    let _skinning_ = FSharp.Data.Adaptive.cval(value.skinning)
    let _diffuseTexture_ = FSharp.Data.Adaptive.cval(value.diffuseTexture)
    let _alphaTest_ = FSharp.Data.Adaptive.cval(value.alphaTest)
    let _specularTexture_ = FSharp.Data.Adaptive.cval(value.specularTexture)
    let _normalMapping_ = FSharp.Data.Adaptive.cval(value.normalMapping)
    let _shrink_ = FSharp.Data.Adaptive.cval(value.shrink)
    let _lighting_ = FSharp.Data.Adaptive.cval(value.lighting)
    let _animation_ = FSharp.Data.Adaptive.cval(value.animation)
    let mutable __value = value
    let __adaptive = FSharp.Data.Adaptive.AVal.custom((fun (token : FSharp.Data.Adaptive.AdaptiveToken) -> __value))
    static member Create(value : EigiModel) = AdaptiveEigiModel(value)
    static member Unpersist = Adaptify.Unpersist.create (fun (value : EigiModel) -> AdaptiveEigiModel(value)) (fun (adaptive : AdaptiveEigiModel) (value : EigiModel) -> adaptive.Update(value))
    member __.Update(value : EigiModel) =
        if Microsoft.FSharp.Core.Operators.not((FSharp.Data.Adaptive.ShallowEqualityComparer<EigiModel>.ShallowEquals(value, __value))) then
            __value <- value
            __adaptive.MarkOutdated()
            _time_.Value <- value.time
            _transform_.Value <- value.transform
            _skinning_.Value <- value.skinning
            _diffuseTexture_.Value <- value.diffuseTexture
            _alphaTest_.Value <- value.alphaTest
            _specularTexture_.Value <- value.specularTexture
            _normalMapping_.Value <- value.normalMapping
            _shrink_.Value <- value.shrink
            _lighting_.Value <- value.lighting
            _animation_.Value <- value.animation
    member __.Current = __adaptive
    member __.time = _time_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.MicroTime>
    member __.transform = _transform_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.skinning = _skinning_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.diffuseTexture = _diffuseTexture_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.alphaTest = _alphaTest_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.specularTexture = _specularTexture_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.normalMapping = _normalMapping_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.shrink = _shrink_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.lighting = _lighting_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.animation = _animation_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.Range1d>
[<System.Diagnostics.CodeAnalysis.SuppressMessage("NameConventions", "*")>]
type AdaptiveStableTrafoModel(value : StableTrafoModel) =
    let _offset_ = FSharp.Data.Adaptive.cval(value.offset)
    let mutable __value = value
    let __adaptive = FSharp.Data.Adaptive.AVal.custom((fun (token : FSharp.Data.Adaptive.AdaptiveToken) -> __value))
    static member Create(value : StableTrafoModel) = AdaptiveStableTrafoModel(value)
    static member Unpersist = Adaptify.Unpersist.create (fun (value : StableTrafoModel) -> AdaptiveStableTrafoModel(value)) (fun (adaptive : AdaptiveStableTrafoModel) (value : StableTrafoModel) -> adaptive.Update(value))
    member __.Update(value : StableTrafoModel) =
        if Microsoft.FSharp.Core.Operators.not((FSharp.Data.Adaptive.ShallowEqualityComparer<StableTrafoModel>.ShallowEquals(value, __value))) then
            __value <- value
            __adaptive.MarkOutdated()
            _offset_.Value <- value.offset
    member __.Current = __adaptive
    member __.offset = _offset_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.float>

