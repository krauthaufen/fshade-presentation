namespace Aardvark.UI.Presentation

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Presentation

[<AutoOpen>]
module Mutable =

    
    
    type MSlideModel(__initial : Aardvark.UI.Presentation.SlideModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Aardvark.UI.Presentation.SlideModel> = Aardvark.Base.Incremental.EqModRef<Aardvark.UI.Presentation.SlideModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Aardvark.UI.Presentation.SlideModel>
        let _time = ResetMod.Create(__initial.time)
        let _isActive = ResetMod.Create(__initial.isActive)
        let _isOverview = ResetMod.Create(__initial.isOverview)
        let _activeSince = ResetMod.Create(__initial.activeSince)
        
        member x.time = _time :> IMod<_>
        member x.isActive = _isActive :> IMod<_>
        member x.isOverview = _isOverview :> IMod<_>
        member x.activeSince = _activeSince :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Aardvark.UI.Presentation.SlideModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_time,v.time)
                ResetMod.Update(_isActive,v.isActive)
                ResetMod.Update(_isOverview,v.isOverview)
                ResetMod.Update(_activeSince,v.activeSince)
                
        
        static member Create(__initial : Aardvark.UI.Presentation.SlideModel) : MSlideModel = MSlideModel(__initial)
        static member Update(m : MSlideModel, v : Aardvark.UI.Presentation.SlideModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Aardvark.UI.Presentation.SlideModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module SlideModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let time =
                { new Lens<Aardvark.UI.Presentation.SlideModel, Aardvark.Base.MicroTime>() with
                    override x.Get(r) = r.time
                    override x.Set(r,v) = { r with time = v }
                    override x.Update(r,f) = { r with time = f r.time }
                }
            let isActive =
                { new Lens<Aardvark.UI.Presentation.SlideModel, System.Boolean>() with
                    override x.Get(r) = r.isActive
                    override x.Set(r,v) = { r with isActive = v }
                    override x.Update(r,f) = { r with isActive = f r.isActive }
                }
            let isOverview =
                { new Lens<Aardvark.UI.Presentation.SlideModel, System.Boolean>() with
                    override x.Get(r) = r.isOverview
                    override x.Set(r,v) = { r with isOverview = v }
                    override x.Update(r,f) = { r with isOverview = f r.isOverview }
                }
            let activeSince =
                { new Lens<Aardvark.UI.Presentation.SlideModel, Aardvark.Base.MicroTime>() with
                    override x.Get(r) = r.activeSince
                    override x.Set(r,v) = { r with activeSince = v }
                    override x.Update(r,f) = { r with activeSince = f r.activeSince }
                }
    
    
    type MPresentationModel(__initial : Aardvark.UI.Presentation.PresentationModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Aardvark.UI.Presentation.PresentationModel> = Aardvark.Base.Incremental.EqModRef<Aardvark.UI.Presentation.PresentationModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Aardvark.UI.Presentation.PresentationModel>
        let _time = ResetMod.Create(__initial.time)
        let _active = ResetMod.Create(__initial.active)
        let _overview = ResetMod.Create(__initial.overview)
        
        member x.time = _time :> IMod<_>
        member x.active = _active :> IMod<_>
        member x.overview = _overview :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Aardvark.UI.Presentation.PresentationModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_time,v.time)
                ResetMod.Update(_active,v.active)
                ResetMod.Update(_overview,v.overview)
                
        
        static member Create(__initial : Aardvark.UI.Presentation.PresentationModel) : MPresentationModel = MPresentationModel(__initial)
        static member Update(m : MPresentationModel, v : Aardvark.UI.Presentation.PresentationModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Aardvark.UI.Presentation.PresentationModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module PresentationModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let time =
                { new Lens<Aardvark.UI.Presentation.PresentationModel, Aardvark.Base.MicroTime>() with
                    override x.Get(r) = r.time
                    override x.Set(r,v) = { r with time = v }
                    override x.Update(r,f) = { r with time = f r.time }
                }
            let active =
                { new Lens<Aardvark.UI.Presentation.PresentationModel, Aardvark.UI.Presentation.SlideIndex>() with
                    override x.Get(r) = r.active
                    override x.Set(r,v) = { r with active = v }
                    override x.Update(r,f) = { r with active = f r.active }
                }
            let overview =
                { new Lens<Aardvark.UI.Presentation.PresentationModel, System.Boolean>() with
                    override x.Get(r) = r.overview
                    override x.Set(r,v) = { r with overview = v }
                    override x.Update(r,f) = { r with overview = f r.overview }
                }
