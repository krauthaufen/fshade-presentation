namespace Aardvark.UI.Presentation

open Aardvark.Base
open Adaptify

type SlideIndex =
    {
        horizontal : int
        vertical    : int
    }

[<ModelType>]
type SlideModel =
    {
        time        : MicroTime
        isActive    : bool
        isOverview  : bool
        activeSince : MicroTime
    }

[<ModelType>] 
type PresentationModel =
    {
        time        : MicroTime
        active      : SlideIndex
        overview    : bool
    }
    
