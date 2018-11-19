namespace Presentation.Model

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Primitives
open Aardvark.UI

type Primitive =
    | Box
    | Sphere




[<DomainType>]
type Model =
    {
        currentModel    : Primitive
        fill            : bool
        cameraState     : CameraControllerState
    }
    
   
[<DomainType>]
type EigiModel =
    {
        time : MicroTime
        
        transform : bool
        skinning : bool
        diffuseTexture : bool
        alphaTest : bool
        specularTexture : bool
        normalMapping : bool
        shrink : bool
        lighting : bool

        animation : Range1d

    }   

    
[<DomainType>]
type StableTrafoModel =
    {
        offset : float
    }
