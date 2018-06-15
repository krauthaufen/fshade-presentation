namespace Aardvark.UI

open Aardvark.Base
open Aardvark.Base.Incremental

[<DomainType>]
type OrbitModel =
    {
        center      : V3d
        phi         : float
        theta       : float
        radius      : float

        radiusRange : Range1d
        thetaRange  : Range1d

        startRot    : V2i
        startZoom   : V2i
        rotating    : bool
        zooming     : bool
        moveSpeed   : float
        time        : float
        camera      : Camera
    }



