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

//[<RequireQualifiedAccess>]
type Step =
    | Right of float32
    | Down of float32
    | Left of float32
    | Up of float32

type BotState =
    | Idle
    | LeftTo of destination: Vector2 * velocity: float32 * acceleration: float32
    | RightTo of destination: Vector2 * velocity: float32 * acceleration: float32
    | UpTo of destination: Vector2 * velocity: float32 * acceleration: float32
    | DownTo of destination: Vector2 * velocity: float32 * acceleration: float32
    with
        static member Zero = BotState.Idle

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
            | Step.Right x -> {b with State = RightTo (x * right + b.Position, 0f, maxAcc); Steps = remaining}
            | Step.Down  x -> {b with State = DownTo  (x * down + b.Position,  0f, maxAcc); Steps = remaining}
            | Step.Left  x -> {b with State = LeftTo  (x * left + b.Position,  0f, maxAcc); Steps = remaining}
            | Step.Up    x -> {b with State = UpTo    (x * up + b.Position,    0f, maxAcc); Steps = remaining}


    let move (ts: TimeSpan) (b: Bot) : Bot =
        let elapsedSecs = float32 ts.TotalSeconds
        
        match b.State with
        | Idle -> handleIdle b
        | LeftTo (d, vel, acc) ->
            let newLocation = b.Position + vel * left * elapsedSecs + 0.5f * acc * left * elapsedSecs * elapsedSecs
            let newVelocity, acc = 
                let nextVelocity = vel + acc * elapsedSecs
                if nextVelocity >= maxVelocity then
                    maxVelocity, 0f
                else
                    nextVelocity, acc

            let distanceFromTarget = (d - newLocation).Length ()
            let slowdownDistance = (newVelocity * newVelocity) / (2f * maxAcc)

            if distanceFromTarget < snapToDestinationDistance then
                { b with Position = d; State = Idle}

            elif distanceFromTarget <= slowdownDistance then
                { b with Position = newLocation; State = LeftTo (d, newVelocity, -maxAcc)}
                
            else
                { b with Position = newLocation; State = LeftTo (d, newVelocity, acc)}

        | RightTo (d, vel, acc) ->
            let newLocation = b.Position + vel * right * elapsedSecs + 0.5f * acc * right * elapsedSecs * elapsedSecs
            let newVelocity, acc = 
                let nextVelocity = vel + acc * elapsedSecs
                if nextVelocity >= maxVelocity then
                    maxVelocity, 0f
                else
                    nextVelocity, acc

            let distanceFromTarget = (d - newLocation).Length ()
            let slowdownDistance = (newVelocity * newVelocity) / (2f * maxAcc)

            if distanceFromTarget < snapToDestinationDistance then
                { b with Position = d; State = Idle}

            elif distanceFromTarget <= slowdownDistance then
                //let newAcc = -(newVelocity * newVelocity) / 2f / distanceFromTarget
                { b with Position = newLocation; State = RightTo (d, newVelocity, -maxAcc)}
                
            else
                { b with Position = newLocation; State = RightTo (d, newVelocity, acc)}

        | UpTo (d, vel, acc) ->
            let newLocation = b.Position + vel * up * elapsedSecs + 0.5f * acc * up * elapsedSecs * elapsedSecs
            let newVelocity, acc = 
                let nextVelocity = vel + acc * elapsedSecs
                if nextVelocity >= maxVelocity then
                    maxVelocity, 0f
                else
                    nextVelocity, acc

            let distanceFromTarget = (d - newLocation).Length ()
            let slowdownDistance = (newVelocity * newVelocity) / (2f * maxAcc)

            if distanceFromTarget < snapToDestinationDistance then
                { b with Position = d; State = Idle}

            elif distanceFromTarget <= slowdownDistance then
                { b with Position = newLocation; State = UpTo (d, newVelocity, -maxAcc)}
                
            else
                { b with Position = newLocation; State = UpTo (d, newVelocity, acc)}

        | DownTo (d, vel, acc) ->
            let newLocation = b.Position + vel * down * elapsedSecs + 0.5f * acc * down * elapsedSecs * elapsedSecs
            let newVelocity, acc = 
                let nextVelocity = vel + acc * elapsedSecs
                if nextVelocity >= maxVelocity then
                    maxVelocity, 0f
                else
                    nextVelocity, acc

            let distanceFromTarget = (d - newLocation).Length ()
            let slowdownDistance = (newVelocity * newVelocity) / (2f * maxAcc)

            if distanceFromTarget < snapToDestinationDistance then
                { b with Position = d; State = Idle}

            elif distanceFromTarget <= slowdownDistance then
                { b with Position = newLocation; State = DownTo (d, newVelocity, -maxAcc)}
                
            else
                { b with Position = newLocation; State = DownTo (d, newVelocity, acc)}
