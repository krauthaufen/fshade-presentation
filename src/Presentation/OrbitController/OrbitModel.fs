namespace Aardvark.UI

open Aardvark.Base
open Aardvark.Base.Incremental

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

[<DomainType>]
type OrbitModel =
    {
        center      : V3d
        phi         : float
        theta       : float
        radius      : float

        [<NonIncremental>]
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



