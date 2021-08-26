module RobotMovementSim.State

open System
open Microsoft.Xna.Framework

let nil<'a> = Unchecked.defaultof<'a>
let maxAcc = 200f
let snapToDestinationDistance = 0.5f
let snapToRotationDifference = 0.05f
let maxVelocity = 200.0f
let left = Vector2(-1f, 0f)
let right = Vector2(1f, 0f)
let up = Vector2(0f, -1f)
let down = Vector2(0f, 1f)
let rotationRate = 2.0f

[<RequireQualifiedAccess>]
type Direction =
    | Down
    | Left
    | Right
    | Up


[<RequireQualifiedAccess>]
module Direction =

    let rotation direction =
        match direction with
        | Direction.Down -> MathHelper.Pi
        | Direction.Left -> 3f * MathHelper.PiOver2
        | Direction.Right -> MathHelper.PiOver2
        | Direction.Up -> 0f

    let vector direction =
        match direction with
        | Direction.Down -> Vector2 (0f, 1f)
        | Direction.Left -> Vector2(-1f, 0f)
        | Direction.Right -> Vector2(1f, 0f)
        | Direction.Up -> Vector2(0f, -1f)


type Step = Step of direction: Direction * distance: float32

type Motion = Motion of destination: Vector2 * velocity: float32 * acceleration: float32

type BotState =
    | Idle
    | Stopped
    | Move of direction: Direction * motion: Motion
    with
        static member Zero = BotState.Idle

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
            let (Step (direction, distance)) = next
            let directionVector = Direction.vector direction
            match direction with
            | Direction.Down  -> {bot with State = Move (direction, Motion (bot.Position + distance * directionVector, 0f, maxAcc)); Steps = remaining}
            | Direction.Left  -> {bot with State = Move (direction, Motion (bot.Position + distance * directionVector, 0f, maxAcc)); Steps = remaining}
            | Direction.Right -> {bot with State = Move (direction, Motion (bot.Position + distance * directionVector, 0f, maxAcc)); Steps = remaining}
            | Direction.Up    -> {bot with State = Move (direction, Motion (bot.Position + distance * directionVector, 0f, maxAcc)); Steps = remaining}


    let processMove 
        (gameTime: GameTime)
        (bot: Bot)
        (direction: Direction)
        (motion: Motion) : Bot =

        let (Motion (destination, velocity, acceleration)) = motion
        let elapsedSecs = float32 gameTime.ElapsedGameTime.TotalSeconds

        let targetRotation = Direction.rotation direction
        let angleDifference = bot.Rotation - targetRotation
        if Math.Abs angleDifference > snapToRotationDifference then

            let newRotation = 
                if angleDifference < 0f then
                    bot.Rotation + rotationRate * elapsedSecs
                else
                    bot.Rotation - rotationRate * elapsedSecs

            if newRotation < 0f then
                { bot with Rotation = newRotation + MathHelper.TwoPi }
            elif newRotation > MathHelper.TwoPi then
                { bot with Rotation = newRotation - MathHelper.TwoPi }
            else
                { bot with Rotation = newRotation }
            
        else
            let directionVector = Direction.vector direction
            let newLocation = bot.Position + velocity * directionVector * elapsedSecs + 0.5f * acceleration * directionVector * elapsedSecs * elapsedSecs
            let newVelocity, acc = 
                let nextVelocity = velocity + acceleration * elapsedSecs
                if nextVelocity >= maxVelocity then
                    maxVelocity, 0f
                else
                    nextVelocity, acceleration

            let distanceFromTarget = (destination - newLocation).Length ()
            let slowdownDistance = (newVelocity * newVelocity) / (2f * maxAcc)

            if distanceFromTarget < snapToDestinationDistance then
                { bot with Position = destination; Rotation = targetRotation; State = Stopped}

            elif distanceFromTarget <= slowdownDistance then
                { bot with Position = newLocation; Rotation = targetRotation; State = Move (direction, (Motion (destination, newVelocity, -maxAcc)))}
                
            else
                { bot with Position = newLocation; Rotation = targetRotation; State = Move (direction, (Motion (destination, newVelocity, acc)))}


    let move (gameTime: GameTime) (bot: Bot) : Bot =
        
        match bot.State with
        | Idle -> handleIdle bot
        | Stopped -> handleStopped gameTime bot
        | Move (direction, motion) -> processMove gameTime bot direction motion 
