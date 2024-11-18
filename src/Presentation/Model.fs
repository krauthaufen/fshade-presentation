namespace Presentation.Model

open System
open Aardvark.Base
open FSharp.Data.Adaptive
open Adaptify
open Aardvark.UI.Primitives
open Aardvark.UI

type Primitive =
    | Box
    | Sphere




[<ModelType>]
type Model =
    {
        currentModel    : Primitive
        fill            : bool
        cameraState     : CameraControllerState
    }
    
   
[<ModelType>]
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

    
[<ModelType>]
type StableTrafoModel =
    {
        offset : float
    }
