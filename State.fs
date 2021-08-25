module State

open System

let maxAcc = 0.0001f
let snapToDestinationDistance = 1.0f
let maxVelocity = 1.0f
let leftAcc = -maxAcc
let rightAcc = maxAcc
let downAcc = maxAcc
let upAcc = -maxAcc

type BotState =
    | Idle
    | LeftTo of float32
    | RightTo of float32
    | UpTo of float32
    | DownTo of float32
    with
        static member Zero = BotState.Idle

type Bots = {
    LocX : float32 []
    LocY : float32 []
    VelX : float32 []
    VelY : float32 []
    AccX : float32 []
    AccY : float32 []
    State : BotState[]
}

module Bots =

    let move (bots: Bots) (ts: TimeSpan) =
        let elapsedMs = float32 ts.TotalMilliseconds
        
        for i = 0 to bots.LocX.Length - 1 do
            bots.LocX.[i] <- bots.LocX.[i] + elapsedMs * bots.VelX.[i] + 0.5f * bots.AccX.[i] * elapsedMs * elapsedMs
            bots.LocY.[i] <- bots.LocY.[i] + elapsedMs * bots.VelY.[i] + 0.5f * bots.AccY.[i] * elapsedMs * elapsedMs

            bots.VelX.[i] <- Math.Clamp (bots.VelX.[i] + bots.AccX.[i] * elapsedMs, -maxVelocity, maxVelocity)
            bots.VelY.[i] <- Math.Clamp (bots.VelY.[i] + bots.AccY.[i] * elapsedMs, -maxVelocity, maxVelocity)

            match bots.State.[i] with
            | Idle -> ()
            | LeftTo x  -> 
                if Math.Abs (bots.LocX.[i] - x) <= snapToDestinationDistance then
                    bots.LocX.[i] <- x
                    bots.VelX.[i] <- 0.0f
                    bots.AccX.[i] <- 0f
                    bots.AccY.[i] <- upAcc
                    bots.State.[i] <- BotState.UpTo 0f
                else
                    let slowdownDistance = Math.Abs ((bots.VelX.[i] * bots.VelX.[i]) / (2f * bots.AccX.[i]))
                    let currentDistance = Math.Abs (bots.LocX.[i] - x)
                    if currentDistance <= slowdownDistance then bots.AccX.[i] <- rightAcc
            | RightTo x -> 
                if Math.Abs (bots.LocX.[i] - x) <= snapToDestinationDistance then
                    bots.LocX.[i] <- x
                    bots.VelX.[i] <- 0.0f
                    bots.AccX.[i] <- 0.0f
                    bots.AccY.[i] <- downAcc
                    bots.State.[i] <- BotState.DownTo 200f
                else
                    let slowdownDistance = Math.Abs ((bots.VelX.[i] * bots.VelX.[i]) / (2f * bots.AccX.[i]))
                    let currentDistance = Math.Abs (bots.LocX.[i] - x)
                    if currentDistance <= slowdownDistance then bots.AccX.[i] <- leftAcc
            | UpTo y ->
                if Math.Abs (bots.LocY.[i] - y) <= snapToDestinationDistance then
                    bots.LocY.[i] <- y
                    bots.VelY.[i] <- 0.0f
                    bots.AccY.[i] <- 0.0f
                    bots.AccX.[i] <- rightAcc
                    bots.State.[i] <- BotState.RightTo 200f
                else
                    let slowdownDistance = Math.Abs ((bots.VelY.[i] * bots.VelY.[i]) / (2f * bots.AccY.[i]))
                    let currentDistance = Math.Abs (bots.LocY.[i] - y)
                    if currentDistance <= slowdownDistance then bots.AccY.[i] <- downAcc
            | DownTo y  ->
                if Math.Abs (bots.LocY.[i] - y) <= snapToDestinationDistance then
                    bots.LocY.[i] <- y
                    bots.VelY.[i] <- 0.0f
                    bots.AccY.[i] <- 0.0f
                    bots.AccX.[i] <- leftAcc
                    bots.State.[i] <- BotState.LeftTo 0f
                else
                    let slowdownDistance = Math.Abs ((bots.VelY.[i] * bots.VelY.[i]) / (2f * bots.AccY.[i]))
                    let currentDistance = Math.Abs (bots.LocY.[i] - y)
                    if currentDistance <= slowdownDistance then bots.AccY.[i] <- upAcc


        ()

    let create (count: int) =
        {
            LocX = Array.zeroCreate count
            LocY = Array.zeroCreate count
            VelX = Array.zeroCreate count
            VelY = Array.zeroCreate count
            AccX = Array.zeroCreate count
            AccY = Array.zeroCreate count
            State = Array.create count BotState.Zero
        }