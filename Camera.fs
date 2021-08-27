namespace RobotMovementSim

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input


[<AutoOpen>]
module MonoGameExtensions =
    type Viewport with
        member this.Center =
            Vector2(float32 this.Width * 0.5f, float32 this.Height * 0.5f)


type Camera(viewport: Viewport) =       
    let mutable scrollWheelValue = 0
    let maxZoom = 4f
    let minZoom = 0.5f
    member val WorldToScreen = Matrix.Identity with get, set
    member val ScreenToWorld = Matrix.Identity with get, set
    member val Zoom = 1.0f with get, set
    member val Position = Vector2.Zero with get, set
    member val Rotation = 0.0f with get, set

    member _.GetMovementDirection () =
        let mutable movementDirection = Vector2.Zero
        let state = Keyboard.GetState ()
        
        if state.IsKeyDown Keys.Down then
            movementDirection <- movementDirection + Vector2.UnitY
        
        if state.IsKeyDown Keys.Up then
            movementDirection <- movementDirection - Vector2.UnitY

        if state.IsKeyDown Keys.Left then
            movementDirection <- movementDirection - Vector2.UnitX

        if state.IsKeyDown Keys.Right then
            movementDirection <- movementDirection + Vector2.UnitX

        if movementDirection <> Vector2.Zero then
            movementDirection.Normalize ()

        movementDirection


    member this.AdjustZoom () =
        let mouseState = Mouse.GetState ()
        let newScrollWheelValue = mouseState.ScrollWheelValue

        if newScrollWheelValue < scrollWheelValue then
            scrollWheelValue <- newScrollWheelValue
            this.Zoom <- MathHelper.Clamp (this.Zoom * 0.8f, minZoom, maxZoom)

        if newScrollWheelValue > scrollWheelValue then
            scrollWheelValue <- newScrollWheelValue
            this.Zoom <- MathHelper.Clamp (this.Zoom * 1.2f, minZoom, maxZoom)
            

    member this.MoveCamera (gameTime: GameTime) =
        let speed = 800f
        let seconds = float32 gameTime.ElapsedGameTime.TotalSeconds
        let movementDirection = this.GetMovementDirection()
        this.Position <- this.Position + speed * movementDirection * seconds


    member this.Update (gameTime: GameTime) =
        this.MoveCamera gameTime
        this.AdjustZoom ()
        this.WorldToScreen <-
            Matrix.CreateTranslation(Vector3(-this.Position, 0.0f)) *
            Matrix.CreateRotationZ(this.Rotation ) *
            Matrix.CreateScale(Vector3(this.Zoom, this.Zoom, 1.f )) *
            Matrix.CreateTranslation(Vector3(viewport.Center, 0.f))
        this.ScreenToWorld <- Matrix.Invert(this.WorldToScreen)