module State

open System
open Microsoft.Xna.Framework

let nil<'a> = Unchecked.defaultof<'a>
let maxAcc = 100f
let snapToDestinationDistance = 0.5f
let maxVelocity = 100.0f
let left = Vector2(-1f, 0f)
let right = Vector2(1f, 0f)
let up = Vector2(0f, -1f)
let down = Vector2(0f, 1f)

//[<RequireQualifiedAccess>]
type Step =
    | Right of float32
    | Down of float32
    | Left of float32
    | Up of float32

type Motion = Motion of destination: Vector2 * velocity: float32 * acceleration: float32

type BotState =
    | Idle
    | Stopped
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
    BotId : int
    Position : Vector2
    Rotation : float32
    State : BotState
    Steps : Step list
} with
    member _.Origin =
        Vector2 (32f / 2f, 32f / 2f)

module Bot =

    let composeStatusMessage (gameTime: GameTime) (bot: Bot) (status: string) =
        $"Bot={bot.BotId} | TimeStamp={gameTime.TotalGameTime} | Status={status}"

    let handleIdle (bot: Bot) =
        bot

    let handleStopped (gameTime: GameTime) (bot: Bot) =
        match bot.Steps with
        | [] -> 
            let msg = composeStatusMessage gameTime bot "idle"
            printfn "%s" msg
            {bot with State = Idle}
        | next::remaining ->
            match next with
            | Step.Right x -> {bot with State = RightTo (Motion (x * right + bot.Position, 0f, maxAcc)); Steps = remaining}
            | Step.Down  x -> {bot with State = DownTo  (Motion (x * down + bot.Position,  0f, maxAcc)); Steps = remaining}
            | Step.Left  x -> {bot with State = LeftTo  (Motion (x * left + bot.Position,  0f, maxAcc)); Steps = remaining}
            | Step.Up    x -> {bot with State = UpTo    (Motion (x * up + bot.Position,    0f, maxAcc)); Steps = remaining}


    let processDirection 
        (gameTime: GameTime)
        (bot: Bot)
        (direction: Vector2)
        (motion: Motion)
        stateBuilder : Bot =

        let (Motion (destination, velocity, acceleration)) = motion
        let elapsedSecs = float32 gameTime.ElapsedGameTime.TotalSeconds
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
            { bot with Position = destination; State = Stopped}

        elif distanceFromTarget <= slowdownDistance then
            { bot with Position = newLocation; State = stateBuilder (Motion (destination, newVelocity, -maxAcc))}
                
        else
            { bot with Position = newLocation; State = stateBuilder (Motion (destination, newVelocity, acc))}


    let move (gameTime: GameTime) (bot: Bot) : Bot =
        
        match bot.State with
        | Idle -> handleIdle bot
        | Stopped -> handleStopped gameTime bot
        | LeftTo m -> processDirection gameTime bot left m BotState.leftTo
        | RightTo m -> processDirection gameTime bot right m BotState.rightTo
        | UpTo m -> processDirection gameTime bot up m BotState.upTo
        | DownTo m -> processDirection gameTime bot down m BotState.downTo
