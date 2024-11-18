//15fca7e7-7e75-5eca-b27c-70abebfbef6f
//84db7623-bae6-f474-b0d1-0baaed8cc777
#nowarn "49" // upper case patterns
#nowarn "66" // upcast is unncecessary
#nowarn "1337" // internal types
#nowarn "1182" // value is unused
namespace rec Aardvark.UI.Presentation

open System
open FSharp.Data.Adaptive
open Adaptify
open Aardvark.UI.Presentation
[<System.Diagnostics.CodeAnalysis.SuppressMessage("NameConventions", "*")>]
type AdaptiveSlideModel(value : SlideModel) =
    let _time_ = FSharp.Data.Adaptive.cval(value.time)
    let _isActive_ = FSharp.Data.Adaptive.cval(value.isActive)
    let _isOverview_ = FSharp.Data.Adaptive.cval(value.isOverview)
    let _activeSince_ = FSharp.Data.Adaptive.cval(value.activeSince)
    let mutable __value = value
    let __adaptive = FSharp.Data.Adaptive.AVal.custom((fun (token : FSharp.Data.Adaptive.AdaptiveToken) -> __value))
    static member Create(value : SlideModel) = AdaptiveSlideModel(value)
    static member Unpersist = Adaptify.Unpersist.create (fun (value : SlideModel) -> AdaptiveSlideModel(value)) (fun (adaptive : AdaptiveSlideModel) (value : SlideModel) -> adaptive.Update(value))
    member __.Update(value : SlideModel) =
        if Microsoft.FSharp.Core.Operators.not((FSharp.Data.Adaptive.ShallowEqualityComparer<SlideModel>.ShallowEquals(value, __value))) then
            __value <- value
            __adaptive.MarkOutdated()
            _time_.Value <- value.time
            _isActive_.Value <- value.isActive
            _isOverview_.Value <- value.isOverview
            _activeSince_.Value <- value.activeSince
    member __.Current = __adaptive
    member __.time = _time_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.MicroTime>
    member __.isActive = _isActive_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.isOverview = _isOverview_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>
    member __.activeSince = _activeSince_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.MicroTime>
[<System.Diagnostics.CodeAnalysis.SuppressMessage("NameConventions", "*")>]
type AdaptivePresentationModel(value : PresentationModel) =
    let _time_ = FSharp.Data.Adaptive.cval(value.time)
    let _active_ = FSharp.Data.Adaptive.cval(value.active)
    let _overview_ = FSharp.Data.Adaptive.cval(value.overview)
    let mutable __value = value
    let __adaptive = FSharp.Data.Adaptive.AVal.custom((fun (token : FSharp.Data.Adaptive.AdaptiveToken) -> __value))
    static member Create(value : PresentationModel) = AdaptivePresentationModel(value)
    static member Unpersist = Adaptify.Unpersist.create (fun (value : PresentationModel) -> AdaptivePresentationModel(value)) (fun (adaptive : AdaptivePresentationModel) (value : PresentationModel) -> adaptive.Update(value))
    member __.Update(value : PresentationModel) =
        if Microsoft.FSharp.Core.Operators.not((FSharp.Data.Adaptive.ShallowEqualityComparer<PresentationModel>.ShallowEquals(value, __value))) then
            __value <- value
            __adaptive.MarkOutdated()
            _time_.Value <- value.time
            _active_.Value <- value.active
            _overview_.Value <- value.overview
    member __.Current = __adaptive
    member __.time = _time_ :> FSharp.Data.Adaptive.aval<Aardvark.Base.MicroTime>
    member __.active = _active_ :> FSharp.Data.Adaptive.aval<SlideIndex>
    member __.overview = _overview_ :> FSharp.Data.Adaptive.aval<Microsoft.FSharp.Core.bool>

