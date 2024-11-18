namespace Aardvark.UI

open Aardvark.Base
open Aardvark.Rendering
open FSharp.Data.Adaptive
open Adaptify

type OrbitConfig =
    {
        radiusRange         : Range1d
        thetaRange          : Range1d
        decay               : float
        autoRotateSpeed     : V2d
        autoRotateDelay     : MicroTime

        orbitSensitivity    : float
        zoomSensitivity     : float
        scrollSensitivity   : float
    }

[<ModelType>]
type OrbitModel =
    {
        center      : V3d
        phi         : float
        theta       : float
        radius      : float

        [<NonAdaptive>]
        config      : OrbitConfig

        lastAction  : MicroTime

        startRot    : V2i
        startZoom   : V2i
        rotating    : bool
        zooming     : bool
        moveSpeed   : float
        time        : MicroTime
        camera      : Camera
    }



