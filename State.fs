module State

open System
open Microsoft.Xna.Framework

let maxAcc = 100f
let snapToDestinationDistance = 0.5f
let maxVelocity = 100.0f
let left = Vector2(-1f, 0f)
let right = Vector2(1f, 0f)
let up = Vector2(0f, -1f)
let down = Vector2(0f, 1f)

type Step =
    | Right of float32
    | Down of float32
    | Left of float32
    | Up of float32

type Motion = Motion of destination: Vector2 * velocity: float32 * acceleration: float32

type BotState =
    | Idle
    | LeftTo of Motion
    | RightTo of Motion
    | UpTo of Motion
    | DownTo of Motion
    with
        static member Zero = BotState.Idle

module BotState =

    let rightTo m = RightTo m
    let leftTo m = LeftTo m
    let upTo m = UpTo m
    let downTo m = DownTo m

type Bot = {
    Position : Vector2
    State : BotState
    Steps : Step list
}

module Bot =

    let handleIdle (b: Bot) =
        match b.Steps with
        | [] -> b
        | next::remaining ->
            match next with
            | Step.Right x -> {b with State = RightTo (Motion (x * right + b.Position, 0f, maxAcc)); Steps = remaining}
            | Step.Down  x -> {b with State = DownTo  (Motion (x * down + b.Position,  0f, maxAcc)); Steps = remaining}
            | Step.Left  x -> {b with State = LeftTo  (Motion (x * left + b.Position,  0f, maxAcc)); Steps = remaining}
            | Step.Up    x -> {b with State = UpTo    (Motion (x * up + b.Position,    0f, maxAcc)); Steps = remaining}


    let processDirection 
        (timeSpan: TimeSpan)
        (bot: Bot)
        (direction: Vector2)
        (motion: Motion)
        stateBuilder : Bot =

        let (Motion (destination, velocity, acceleration)) = motion
        let elapsedSecs = float32 timeSpan.TotalSeconds
        let newLocation = bot.Position + velocity * direction * elapsedSecs + 0.5f * acceleration * direction * elapsedSecs * elapsedSecs
        let newVelocity, acc = 
            let nextVelocity = velocity + acceleration * elapsedSecs
            if nextVelocity >= maxVelocity then
                maxVelocity, 0f
            else
                nextVelocity, acceleration

        let distanceFromTarget = (destination - newLocation).Length ()
        let slowdownDistance = (newVelocity * newVelocity) / (2f * maxAcc)

        if distanceFromTarget < snapToDestinationDistance then
            { bot with Position = destination; State = Idle}

        elif distanceFromTarget <= slowdownDistance then
            { bot with Position = newLocation; State = stateBuilder (Motion (destination, newVelocity, -maxAcc))}
                
        else
            { bot with Position = newLocation; State = stateBuilder (Motion (destination, newVelocity, acc))}


    let move (timeSpan: TimeSpan) (bot: Bot) : Bot =
        
        match bot.State with
        | Idle -> handleIdle bot
        | LeftTo m -> processDirection timeSpan bot left m BotState.leftTo
        | RightTo m -> processDirection timeSpan bot right m BotState.rightTo
        | UpTo m -> processDirection timeSpan bot up m BotState.upTo
        | DownTo m -> processDirection timeSpan bot down m BotState.downTo
