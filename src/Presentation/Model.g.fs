namespace Presentation.Model

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Presentation.Model

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : Presentation.Model.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Presentation.Model.Model> = Aardvark.Base.Incremental.EqModRef<Presentation.Model.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<Presentation.Model.Model>
        let _currentModel = ResetMod.Create(__initial.currentModel)
        let _fill = ResetMod.Create(__initial.fill)
        let _cameraState = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.cameraState)
        
        member x.currentModel = _currentModel :> IMod<_>
        member x.fill = _fill :> IMod<_>
        member x.cameraState = _cameraState
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Presentation.Model.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_currentModel,v.currentModel)
                ResetMod.Update(_fill,v.fill)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_cameraState, v.cameraState)
                
        
        static member Create(__initial : Presentation.Model.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : Presentation.Model.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Presentation.Model.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let currentModel =
                { new Lens<Presentation.Model.Model, Presentation.Model.Primitive>() with
                    override x.Get(r) = r.currentModel
                    override x.Set(r,v) = { r with currentModel = v }
                    override x.Update(r,f) = { r with currentModel = f r.currentModel }
                }
            let fill =
                { new Lens<Presentation.Model.Model, System.Boolean>() with
                    override x.Get(r) = r.fill
                    override x.Set(r,v) = { r with fill = v }
                    override x.Update(r,f) = { r with fill = f r.fill }
                }
            let cameraState =
                { new Lens<Presentation.Model.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.cameraState
                    override x.Set(r,v) = { r with cameraState = v }
                    override x.Update(r,f) = { r with cameraState = f r.cameraState }
                }
    
    
    type MEigiModel(__initial : Presentation.Model.EigiModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Presentation.Model.EigiModel> = Aardvark.Base.Incremental.EqModRef<Presentation.Model.EigiModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Presentation.Model.EigiModel>
        let _time = ResetMod.Create(__initial.time)
        let _transform = ResetMod.Create(__initial.transform)
        let _skinning = ResetMod.Create(__initial.skinning)
        let _diffuseTexture = ResetMod.Create(__initial.diffuseTexture)
        let _alphaTest = ResetMod.Create(__initial.alphaTest)
        let _specularTexture = ResetMod.Create(__initial.specularTexture)
        let _normalMapping = ResetMod.Create(__initial.normalMapping)
        let _lighting = ResetMod.Create(__initial.lighting)
        let _animation = ResetMod.Create(__initial.animation)
        
        member x.time = _time :> IMod<_>
        member x.transform = _transform :> IMod<_>
        member x.skinning = _skinning :> IMod<_>
        member x.diffuseTexture = _diffuseTexture :> IMod<_>
        member x.alphaTest = _alphaTest :> IMod<_>
        member x.specularTexture = _specularTexture :> IMod<_>
        member x.normalMapping = _normalMapping :> IMod<_>
        member x.lighting = _lighting :> IMod<_>
        member x.animation = _animation :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Presentation.Model.EigiModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_time,v.time)
                ResetMod.Update(_transform,v.transform)
                ResetMod.Update(_skinning,v.skinning)
                ResetMod.Update(_diffuseTexture,v.diffuseTexture)
                ResetMod.Update(_alphaTest,v.alphaTest)
                ResetMod.Update(_specularTexture,v.specularTexture)
                ResetMod.Update(_normalMapping,v.normalMapping)
                ResetMod.Update(_lighting,v.lighting)
                ResetMod.Update(_animation,v.animation)
                
        
        static member Create(__initial : Presentation.Model.EigiModel) : MEigiModel = MEigiModel(__initial)
        static member Update(m : MEigiModel, v : Presentation.Model.EigiModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Presentation.Model.EigiModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module EigiModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let time =
                { new Lens<Presentation.Model.EigiModel, Aardvark.Base.MicroTime>() with
                    override x.Get(r) = r.time
                    override x.Set(r,v) = { r with time = v }
                    override x.Update(r,f) = { r with time = f r.time }
                }
            let transform =
                { new Lens<Presentation.Model.EigiModel, System.Boolean>() with
                    override x.Get(r) = r.transform
                    override x.Set(r,v) = { r with transform = v }
                    override x.Update(r,f) = { r with transform = f r.transform }
                }
            let skinning =
                { new Lens<Presentation.Model.EigiModel, System.Boolean>() with
                    override x.Get(r) = r.skinning
                    override x.Set(r,v) = { r with skinning = v }
                    override x.Update(r,f) = { r with skinning = f r.skinning }
                }
            let diffuseTexture =
                { new Lens<Presentation.Model.EigiModel, System.Boolean>() with
                    override x.Get(r) = r.diffuseTexture
                    override x.Set(r,v) = { r with diffuseTexture = v }
                    override x.Update(r,f) = { r with diffuseTexture = f r.diffuseTexture }
                }
            let alphaTest =
                { new Lens<Presentation.Model.EigiModel, System.Boolean>() with
                    override x.Get(r) = r.alphaTest
                    override x.Set(r,v) = { r with alphaTest = v }
                    override x.Update(r,f) = { r with alphaTest = f r.alphaTest }
                }
            let specularTexture =
                { new Lens<Presentation.Model.EigiModel, System.Boolean>() with
                    override x.Get(r) = r.specularTexture
                    override x.Set(r,v) = { r with specularTexture = v }
                    override x.Update(r,f) = { r with specularTexture = f r.specularTexture }
                }
            let normalMapping =
                { new Lens<Presentation.Model.EigiModel, System.Boolean>() with
                    override x.Get(r) = r.normalMapping
                    override x.Set(r,v) = { r with normalMapping = v }
                    override x.Update(r,f) = { r with normalMapping = f r.normalMapping }
                }
            let lighting =
                { new Lens<Presentation.Model.EigiModel, System.Boolean>() with
                    override x.Get(r) = r.lighting
                    override x.Set(r,v) = { r with lighting = v }
                    override x.Update(r,f) = { r with lighting = f r.lighting }
                }
            let animation =
                { new Lens<Presentation.Model.EigiModel, Aardvark.Base.Range1d>() with
                    override x.Get(r) = r.animation
                    override x.Set(r,v) = { r with animation = v }
                    override x.Update(r,f) = { r with animation = f r.animation }
                }
