namespace Aardvark.UI.Presentation

open Aardvark.Base
open Aardvark.Base.Incremental

type SlideIndex =
    {
        horizontal : int
        vertical    : int
    }

[<DomainType>]
type SlideModel =
    {
        time        : MicroTime
        isActive    : bool
        isOverview  : bool
        activeSince : MicroTime
    }

[<DomainType>] 
type PresentationModel =
    {
        time        : MicroTime
        active      : SlideIndex
        overview    : bool
    }
    
